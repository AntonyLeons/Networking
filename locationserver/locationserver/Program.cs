using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace locationserver
{
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
      //  static Dictionary<DateTime, string> log = new Dictionary<DateTime, string>();
        static Dictionary<string, string> data = new Dictionary<string, string>();
       // static string logstatement = "";
        [STAThread]
         static int Main(string[] args)
        {
            if (!args.Contains("-w"))
            {
                //string savepath;
                //string loadpath;
                //for (int i = 0; i < args.Length; i++)
                //{
                //    switch (args[i])
                //    {
                //        case "-l": savepath = args[++i]; break;
                //        case "-d":
                //        case "-f": loadpath = args[++i]; break;
                //    }
                //}
                {
                    RunServer();
                }

                return 0;
            }
            else
            {
                FreeConsole();
                var app = new App();
                return app.Run();
            }
        }
        static void RunServer()
        {
            TcpListener listener;
            Socket connection;

            Handler RequestHandler;
            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                Console.WriteLine("server started listening");
                while (true)
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler();
                    
                    Thread t = new Thread(() => RequestHandler.doRequest(connection));
                    t.Start();
                    //Console.WriteLine("Connection Recieved");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.ToString());
            }
        }
        class Handler
        {


            public void doRequest(Socket connection)
            {
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                try
                {

                    socketStream.ReadTimeout = 1000;
                    socketStream.WriteTimeout = 1000;
                    StreamWriter sw = new StreamWriter(socketStream);
                    StreamReader sr = new StreamReader(socketStream);
                    sw.AutoFlush = true;
                   // DateTime localDate = DateTime.Now;
                   // string datastring;
                    string locationstring="";
                    string userstring;
                    string input = "";
                    string check = "";

                    input = sr.ReadLine();

                    string[] Whois = input.Split(new char[] { ' ' }, 2);
                    input = input.Trim();
                    List<string> sections = new List<string>(input.Split(' '));
                    //   datastring = input.Replace("\r\n", ",");
                    //  List<string> lines = new List<string>(datastring.Split(','));
                    //   datastring = input.Replace("\r\n", " ");
                    //   List<string> sections = new List<string>(datastring.Split(' '));
                    //   locationstring = lines[lines.Count - 1];
                    if(sections.Count>=2)
                    {
                        check = sections[1];
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        if (sections.Count >= 3)
                        {
                            

                            if (sections[2] == ("HTTP/1.0") && check.StartsWith("/")) //-h0
                            {
                                if (sections[0] == ("GET"))
                                {
                                    sections.RemoveAt(0);
                                    userstring = sections[0];
                                    userstring = userstring.Remove(0, 2);

                                    if (data.TryGetValue(userstring, out locationstring))
                                    {
                                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n");
                                        //    logstatement += "GET " + datastring + " - OK";
                                        break;
                                    }
                                    else
                                    {
                                        sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                        //   logstatement += "GET " + datastring + " ERROR: no entries found";
                                        break;
                                    }
                                }
                                if (sections[0]==("POST"))
                                {
                                    locationstring = sr.ReadLine();
                                    locationstring = sr.ReadLine();
                                    while (sr.Peek() >= 0)
                                    {
                                        locationstring += (char)sr.Read();
                                    }
                                    sections.RemoveAt(0);
                                    userstring = sections[0];
                                    userstring = userstring.Remove(0, 1);

                                    if (data.ContainsKey(userstring))
                                    {
                                        data[userstring] = locationstring;
                                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                        //  logstatement += "Put " + datastring + " - OK";
                                        break;
                                    }
                                    else
                                    {
                                        data.Add(userstring, locationstring);
                                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                        // logstatement += "Put " + datastring + " - OK";
                                        break;
                                    }
                                }
                            } // -h0
                            else if (sections[2] == ("HTTP/1.1") && check.StartsWith("/"))
                            {
                                if (sections[0] == ("GET"))
                                {
                                    sections.RemoveAt(0);
                                    userstring = sections[0];
                                    userstring = userstring.Remove(0, 7);

                                    if (data.TryGetValue(userstring, out locationstring))
                                    {
                                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); //location ok responce 3
                                                                                                                                       //  logstatement += "GET " + datastring + " - OK";
                                        break;
                                    }
                                    else
                                    {
                                        sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                        // logstatement += "GET " + datastring + " ERROR: no entries found";
                                        break;
                                    }
                                }
                                if (sections[0] == ("POST"))
                                {
                                    locationstring = sr.ReadLine();
                                    locationstring = sr.ReadLine();
                                    locationstring = sr.ReadLine();
                                    while (sr.Peek() >= 0)
                                    {
                                        locationstring += (char)sr.Read();
                                    }
                                    userstring = locationstring.Remove(0, 5);
                                    userstring = userstring.Replace("&location=", "ÿ");
                                    string[] tmp = userstring.Split('ÿ');
                                    userstring = tmp[0];
                                    locationstring = tmp[1];

                                    if (data.ContainsKey(userstring))
                                    {
                                        data[userstring] = locationstring;
                                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                        //  logstatement += "Put " + datastring + " - OK";
                                        break;
                                    }
                                    else
                                    {
                                        data.Add(userstring, locationstring);
                                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                        // logstatement += "Put " + datastring + " - OK";
                                        break;
                                    }
                                }

                            } //-h1
                        }
                        if ((sections[0] == "GET" || sections[0] == "PUT") && check.StartsWith("/")) //-h9
                        {
                            if (sections[0] == ("GET"))
                            {
                                sections.RemoveAt(0);
                                userstring = sections[0];
                                userstring = userstring.Remove(0, 1);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location OK 3
                                    //  logstatement += "GET " + datastring + " - OK";
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    //      logstatement += "GET " + datastring + " ERROR: no entries found";
                                    break;
                                }
                            }
                            if (sections[0] == ("PUT"))
                            {
                                locationstring = sr.ReadLine();
                                locationstring = sr.ReadLine();
                                sections.RemoveAt(0);
                                userstring = sections[0];
                                sections.RemoveAt(0);
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    //    logstatement += "Put " + datastring + " - OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    //    logstatement += "Put " + datastring + " - OK";
                                    break;
                                }
                            }
                        } //-h9


                        else if (Whois.Length == 2)
                        {
                            if (data.ContainsKey(Whois[0]))
                            {
                                data[Whois[0]] = Whois[1];
                                sw.WriteLine("OK");
                                // logstatement += "Put " + datastring + " - OK";
                                break;
                            }
                            else
                            {
                                data.Add(Whois[0], Whois[1]);
                                sw.WriteLine("OK");
                                // logstatement += "Put " + datastring + " - OK";
                                break;
                            }
                        }
                        else if (Whois.Length == 1)
                        {

                            if (data.ContainsKey(Whois[0]))
                            {
                                sw.WriteLine(data[Whois[0]]);
                                //  logstatement += "GET " + datastring + " - OK";
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                // logstatement += "GET " + datastring + " ERROR: no entries found";
                            }
                        }
                    }
                  //  log.Add(localDate, logstatement);
               //     Console.WriteLine(log.Keys.Last() + " " + log.Values.Last());
               //     logstatement = "";
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.ToString());
                }
                finally
                {
                    socketStream.Close();
                    connection.Close();
                }
            }
        }
    }
}