using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace locationserver
{
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)] /// hybrid application from https://stackoverflow.com/questions/5339193/wpf-console-hybrid-application
        static extern bool FreeConsole();
        static Dictionary<string, string> data = new Dictionary<string, string>();
        public static Logging Log;
        [STAThread]
        static int Main(string[] args)
        {

            if (!args.Contains("-w"))
            {
                string savepath = "";
                string logpath = "";
                short timeout = 1000;
                bool debug = false;

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-l": logpath = args[++i]; break;
                        case "-d": debug = true;  break;
                        case "-f": savepath = args[++i]; break;
                        case "-t": timeout = short.Parse(args[++i]); break;
                        default:
                            Console.WriteLine("Unknown Operation");
                            break;
                    }
                }
                if (savepath != "")
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(savepath);
                        foreach (string entry in lines)
                        {
                            string[] entrysplit = entry.Split();
                            data.Add(entrysplit[0], entrysplit[1]);
                        }
                        Console.WriteLine("file found and data loaded");
                    }
                    catch
                    {
                        Console.WriteLine("No file found, creating new file");
                    }
                }
                Log = new Logging(logpath,savepath);
                {
                    RunServer(timeout,debug);
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
        static void RunServer(short timeout,bool debug)
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

                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Log, timeout, debug));
                    t.Start();
                }
            }
            catch (Exception e)
            {
                if (debug == true)
                {
                    Console.WriteLine(e.ToString());
                }
                else
                {
                    Console.WriteLine("Server failed");
                }
            }
        }
        class Handler
        {


            public void doRequest(Socket connection, Logging Log, short timeout,bool debug)
            {
                String Host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                string input = "";
                string State = "";
                try
                {
                    if (timeout > 0)
                    {
                        socketStream.ReadTimeout = timeout;
                        socketStream.WriteTimeout = timeout;
                    }
                    StreamWriter sw = new StreamWriter(socketStream);
                    StreamReader sr = new StreamReader(socketStream);
                    sw.AutoFlush = true;
                    string locationstring = "";
                    string userstring;
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
                    if (sections[0] == ("GET"))
                    {
                        get = true;
                    }
                    else if (sections[0] == ("POST"))
                    {
                        post = true;
                    }
                    if (sections.Count >= 2) //check number of arguments
                    {
                        if (sections[1].StartsWith("/"))
                        {
                            slash = true;
                        }
                        if (sections[1].StartsWith("/?"))
                        {
                            queslash = true;
                        }
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        if (sections.Count >= 3 && lines >= 2)
                        {
                            if (get == true && sections[2] == ("HTTP/1.0") && queslash == true)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 2);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n");
                                    input = "GET " + userstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    input = "GET " + userstring;
                                    State = "UNKNOWN";
                                    break;
                                }
                            }
                            else if (post == true && sections[2] == ("HTTP/1.0") && slash == true)
                            {
                                while (sr.Peek() >= 0)
                                {
                                    locationstring += (char)sr.Read();
                                }
                                locationstring = locationstring.Replace("\r\n", "");
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                            } // -h0
                            else if (get == true && sections[2] == ("HTTP/1.1") && queslash == true && lines >= 3)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 7);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); //location ok responce 3
                                    input = "GET " + userstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    input = "GET " + userstring;
                                    State = "UNKNOWN";
                                    break;
                                }
                            }
                            else if (post == true && sections[2] == ("HTTP/1.1") && slash == true && lines >= 3)
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
                                locationstring = locationstring.Replace("\r\n", "");

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                            } //h1
                        }
                        if (sections.Count >= 2)
                        {
                            if (get == true && slash == true)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.TryGetValue(userstring, out locationstring))
                                {
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location OK 3
                                    input = "GET " + userstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n"); /// location 404 responce 4
                                    input = "GET " + userstring;
                                    State = "UNKNOWN";
                                    break;
                                }
                            }
                            else if (sections[0] == ("PUT") && slash == true && lines == 3)
                            {
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (put) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
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
                                input = "POST " + Whois[0] + " " + Whois[1];
                                State = "OK";
                                break;
                            }
                            else
                            {
                                data.Add(Whois[0], Whois[1]);
                                sw.WriteLine("OK");
                                input = "POST " + Whois[0] + " " + Whois[1];
                                State = "OK";
                                break;
                            }
                        }
                        if (Whois.Length == 1)
                        {

                            if (data.TryGetValue(Whois[0], out locationstring))
                            {
                                sw.WriteLine(locationstring);
                                input = "GET " + Whois[0];
                                State = "OK";
                                break;
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                input = "GET " + Whois[0];
                                State = "UNKNOWN";
                                break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    if (debug == true)
                    {
                        Console.WriteLine(x.ToString());
                        input = "Something went wrong";
                    }
                    else
                    {
                        input = "Something went wrong";
                    }
                    State = "EXCEPTION";
                }
                finally
                {
                    socketStream.Close();
                    connection.Close();
                    Log.WriteToLog(Host, input, State, debug);
                }
            }
        }


        public class Logging /// based on example from https://stackoverflow.com/questions/2954900/simple-multithread-safe-log-class
        {
            public static string LogFile = null;
            public static string savefile = null;
            public Logging(string Logpath, string savepath)
            {
                LogFile = Logpath;
                savefile = savepath;
            }

            private static readonly object locker = new object();


            public void WriteToLog(string Host, string input, string State, bool debug)
            {
                string line = Host + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + input + "\" " + State; 
                lock (locker)
                {
                    Console.WriteLine(line);
                    if (savefile == "")
                    {
                    }
                    else
                    {
                        try
                        {
                            StreamWriter SW;
                            SW=new StreamWriter (savefile, false);
                            foreach (var entry in data)
                                {
                                    SW.WriteLine(entry.Key + " "+ entry.Value);
                                }
                            SW.Close();
                        }
                        catch (Exception s)
                        {
                            if (debug == true)
                            {
                                Console.WriteLine(s.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Unable to Write Save File");
                            }
                        }
                    }
                    if (LogFile == "")
                    {
                        return;
                    }
                    try
                    {
                        StreamWriter SW;
                        SW = File.AppendText(LogFile);
                        SW.WriteLine(line);
                        SW.Close();
                    }
                    catch (Exception l)
                    {
                        if (debug == true)
                        {
                            Console.WriteLine(l.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Unable to Write Log File");
                        }
                    }
                }
            }
        }
    }
}