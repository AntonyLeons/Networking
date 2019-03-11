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
                //        case "-l": savepath = args[++i]; 
                //        case "-d":
                //        case "-f": loadpath = args[++i]; 
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
                    string locationstring = "";
                    string userstring;
                    string input = "";
                    bool slash = false;
                    bool queslash = false;
                    bool get = false;
                    bool post = false;
                    int lines = 1;

                    input = sr.ReadLine();

                    string[] Whois = input.Split(new char[] { ' ' }, 2);
                    input = input.Trim();
                    while (sr.Peek() >= 0)
                    {
                        sr.ReadLine();
                        lines++;
                        break;
                    }
                    while (sr.Peek() >= 0)
                    {
                        locationstring = sr.ReadLine();
                        lines++;
                        break;
                    }
                    List<string> sections = new List<string>(input.Split(' '));
                    if(sections[0]==("GET"))
                    {
                        get = true;
                    }
                    else if(sections[0]==("POST"))
                    {
                        post = true;
                    }
                    if (sections.Count >= 2) //check
                    {
                        if (sections[1].StartsWith("/"))
                        {
                            slash = true;
                        }
                        if(sections[1].StartsWith("/?"))
                        {
                            queslash = true;
                        }
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        if (sections.Count >= 3 && lines >= 2)
                        {
                            if (get==true && sections[2] == ("HTTP/1.0") && queslash == true)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 2);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n");
                                    break;

                                }
                                else
                                {
                                    sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    break;
                                }
                            }
                            else if (post==true && sections[2] == ("HTTP/1.0") && slash==true)
                            {
                                while (sr.Peek() >= 0)
                                {
                                    locationstring += (char)sr.Read();
                                }
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                    break;
                                }
                            } // -h0
                            else if (get==true && sections[2] == ("HTTP/1.1") && queslash==true && lines >= 3)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 7);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); //location ok responce 3
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    break;
                                }
                            }
                            else if (post==true && sections[2] == ("HTTP/1.1") && slash==true && lines >= 3)
                            {
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
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    break;
                                }
                            } //h1
                        }
                        if (sections.Count >= 2)
                        {
                            if (get==true && slash==true)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location OK 3
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    break;
                                }
                            }
                            else if (sections[0] == ("PUT") && slash==true && lines == 3)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    break;
                                }
                            }
                        }
                         if (Whois.Length == 2)
                        {
                            if (data.ContainsKey(Whois[0]))
                            {
                                data[Whois[0]] = Whois[1];
                                sw.WriteLine("OK");
                                break;
                            }
                            else
                            {
                                data.Add(Whois[0], Whois[1]);
                                sw.WriteLine("OK");
                                break;
                            }
                        }
                         if (Whois.Length == 1)
                        {

                            if (data.ContainsKey(Whois[0]))
                            {
                                sw.WriteLine(data[Whois[0]]);
                                break;
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                break;
                            }
                        }
                    }
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
public class Logging
{
    public static String LogFile = null;
    public Logging(String filename)
    {
        LogFile = filename;
    }

    private static readonly object locker = new object();

    public void WriteToLog(String message,String host, String Status)
    {
        String line =host+" - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'")+ " \"" +message ///35 mins
        lock (locker)
        {
            Console.WriteLine(message);
            if(LogFile==null)
            {
                return;
            }
            StreamWriter SW;
            SW = File.AppendText(LogFile);
            SW.WriteLine(message);
            SW.Close();
        }
    }
}