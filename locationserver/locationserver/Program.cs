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
        static Dictionary<DateTime, string> log = new Dictionary<DateTime, string>();
        static Dictionary<string, string> data = new Dictionary<string, string>();
        static string logstatement = "";
        [STAThread]
         static int Main(string[] args)
        {
            if (args != null)
            {
                try
                {
                    RunServer();
                }

                catch
                {
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

                    DateTime localDate = DateTime.Now;
                    string datastring;
                    string locationstring;
                    string userstring;
                    string input = "";
                    string check = "";
                    string whoisdata;

                    while (sr.Peek() > -1)
                    {
                        input += (char)sr.Read();
                    }
                    whoisdata = input.TrimEnd('\r', '\n');
                    string[] Whois = whoisdata.Split(new char[] { ' ' }, 2);
                    input = input.Trim();
                    datastring = input.Replace("\r\n", ",");
                    List<string> lines = new List<string>(datastring.Split(','));
                    datastring = input.Replace("\r\n", " ");
                    List<string> sections = new List<string>(datastring.Split(' '));
                    locationstring = lines[lines.Count - 1];
                    if (sections.Count >= 2)
                    {
                        check = sections[1];
                    }
                    if (sections[0] == ("GET") && check.StartsWith("/"))
                    {
                        if (!sections.Contains("HTTP/1.0") && !sections.Contains("HTTP/1.1")) //h9
                        {
                            sections.RemoveAt(0);
                            userstring = sections[0];
                            userstring = userstring.Remove(0, 1);

                            if (data.ContainsKey(userstring))
                            {
                                sw.WriteLine("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + data[userstring] + "\r\n"); ///location OK 3
                               logstatement += "GET " + datastring + " - OK";
                            }
                            else
                            {
                                sw.WriteLine("HTTP/0.9 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); /// location 404 responce 4
                                logstatement += "GET " + datastring + " ERROR: no entries found";
                            }
                        }
                        else if (sections.Contains("HTTP/1.0")) //-h0
                        {
                            sections.RemoveAt(0);
                            userstring = sections[0];
                            userstring = userstring.Remove(0, 2);

                            if (data.ContainsKey(userstring))
                            {
                                sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + data[userstring] + "\r\n");
                                logstatement += "GET " + datastring + " - OK";
                            }
                            else
                            {
                                sw.WriteLine("HTTP/1.0 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); /// location 404 responce 4
                                logstatement += "GET " + datastring + " ERROR: no entries found";
                            }
                        }
                        else if (sections.Contains("HTTP/1.1")) //-h1
                        {
                            sections.RemoveAt(0);
                            userstring = sections[0];
                            userstring = userstring.Remove(0, 7);

                            if (data.ContainsKey(userstring))
                            {
                                sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + data[userstring] + "\r\n"); //location ok responce 3
                                logstatement += "GET " + datastring + " - OK";
                            }
                            else
                            {
                                sw.WriteLine("HTTP/1.1 404 Not Found" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); /// location 404 responce 4
                                logstatement += "GET " + datastring + " ERROR: no entries found";
                            }
                        }
                    }
                    else if (sections[0] == ("PUT") && check.StartsWith("/") && lines.Count >= 3)
                    {
                        if (!sections.Contains("HTTP/1.0") && !sections.Contains("HTTP/1.1") && sections.Count >= 2) //-h9
                        {
                            sections.RemoveAt(0);
                            userstring = sections[0];
                            sections.RemoveAt(0);
                            userstring = userstring.Remove(0, 1);

                            if (data.ContainsKey(userstring))
                            {
                                data[userstring] = locationstring;
                                sw.WriteLine("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                logstatement += "Put " + datastring + " - OK";
                            }
                            else
                            {
                                data.Add(userstring, locationstring);
                                sw.WriteLine("HTTP/0.9 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                logstatement += "Put " + datastring + " - OK";
                            }
                        }
                    }
                    else if (sections[0] == ("POST") && check.StartsWith("/") && lines.Count >= 3)
                    {
                        if (sections.Contains("HTTP/1.0"))
                        {
                            sections.RemoveAt(0);
                            userstring = sections[0];
                            userstring = userstring.Remove(0, 1);

                            if (data.ContainsKey(userstring))
                            {
                                data[userstring] = locationstring;
                                sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                logstatement += "Put " + datastring + " - OK";
                            }
                            else
                            {
                                data.Add(userstring, locationstring);
                                sw.WriteLine("HTTP/1.0 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                logstatement += "Put " + datastring + " - OK";
                            }
                        }
                        else if (sections.Contains("HTTP/1.1"))
                        {
                            {
                                userstring = locationstring.Remove(0, 5);
                                userstring = userstring.Replace("&location=", ",");
                                string[] tmp = userstring.Split(',');
                                userstring = tmp[0];
                                locationstring = tmp[1];

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                    logstatement += "Put " + datastring + " - OK";
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.1 200 OK" + "\r\n" + "Content-Type: text/plain" + "\r\n" + "\r\n"); ///location added (put) responce 5
                                   logstatement += "Put " + datastring + " - OK";
                                }
                            }
                        }
                    }
                    else if (Whois.Length == 2)
                    {
                        if (data.ContainsKey(Whois[0]))
                        {
                            data[Whois[0]] = Whois[1];
                            sw.WriteLine("OK");
                         logstatement += "Put " + datastring + " - OK";
                        }
                        else
                        {
                            data.Add(Whois[0], Whois[1]);
                            sw.WriteLine("OK");
                            logstatement += "Put " + datastring + " - OK";
                        }
                    }
                    else if (Whois.Length == 1)
                    {

                        if (data.ContainsKey(Whois[0]))
                        {
                            sw.WriteLine(data[Whois[0]]);
                            logstatement += "GET " + datastring + " - OK";
                        }
                        else
                        {
                            sw.WriteLine("ERROR: no entries found");
                            logstatement += "GET " + datastring + " ERROR: no entries found";
                        }
                    }


                    log.Add(localDate, logstatement);
                    Console.WriteLine(log.Keys.Last() + " " + log.Values.Last());
                    logstatement = "";
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
