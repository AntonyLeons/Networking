﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace locationserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Dictionary<string, string> data = new Dictionary<string, string>();
        static List<string> log = new List<string>();
        public static Logging Log;
       public static TcpListener listener;
        public short timeout { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
        }
        public string line;
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            SaveLog.IsEnabled = true;
            Timebox.IsEnabled = false;
            Path.IsEnabled = false;
            DebugBox.IsEnabled = false;
            string logpath = "";
            string savepath = Path.Text;
            if (savepath != "")
            {
                try
                {
                    string[] lines = File.ReadAllLines(savepath); ///read save file and import to dictionary
                    foreach (string entry in lines)
                    {
                        string[] entrysplit = entry.Split();
                        data.Add(entrysplit[0], entrysplit[1]);
                    }
                    Status.AppendText("file found and data loaded\n");
                }
                catch
                {
                    Status.AppendText("No file found, creating new file\n");
                }
            }
            timeout = short.Parse(Timebox.Text);
            Thread r = new Thread(() => RunServer(timeout, Log));
            Log = new Logging(logpath, savepath);
            r.Start();
            Status.AppendText("Server Started... \n");
        }
        static void RunServer(short timeout, Logging Log) /// From Brians Youtube
        {
            Socket connection;

            Handler RequestHandler;
            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                while (true)
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler();

                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Log, timeout));
                    t.Start();
                    //Console.WriteLine("Connection Recieved");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.ToString());
            }
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            SaveLog.IsEnabled = false;
            Timebox.IsEnabled = true;
            Path.IsEnabled = true;
            DebugBox.IsEnabled = true;
            listener.Stop();
            Status.AppendText("Server Stopped \n");

        }
        public void Status_TextChanged(object sender, TextChangedEventArgs e)
        {
            Status.ScrollToEnd(); ///auto scroll
        }

        class Handler
        {

            public void doRequest(Socket connection, Logging Log, short timeout)
            {
                String Host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                string input = "";
                string State = "";
                try
                {
                    if (timeout > 0) ///timeout
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
                    while (sr.Peek() >= 0) /// Check lines exist and count them
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
                    if (sections.Count >= 2) //check if more than 2 arguments
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
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location added (GET) responce 3
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
                                userstring = sections[1];
                                userstring = userstring.Remove(0, 1);

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (POST) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n\r\n"); ///location added (POST) responce 5
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
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location OK responce 3
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

                                if (data.ContainsKey(userstring))
                                {
                                    data[userstring] = locationstring;
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (POST) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (POST) responce 5
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
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + locationstring + "\r\n"); ///location OK responce 3
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
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (POST) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                                else
                                {
                                    data.Add(userstring, locationstring);
                                    sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n"); ///location added (POST) responce 5
                                    input = "POST " + userstring + " " + locationstring;
                                    State = "OK";
                                    break;
                                }
                            }
                        }
                        if (Whois.Length == 2) //whois protocol
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

                            if (data.ContainsKey(Whois[0]))
                            {
                                sw.WriteLine(data[Whois[0]]);
                                input = "GET " + Whois[0];
                                State = "OK";
                                break;
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                input = "GET " + Whois[0];
                                State = "OK";
                                break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.ToString());
                    State = "EXCEPTION";
                }
                finally
                {
                    socketStream.Close();
                    connection.Close();
                    Log.WriteToLog(Host, input, State);
                }
            }
        }
        public class Logging /// based on example from https://stackoverflow.com/questions/2954900/simple-multithread-safe-log-class
        {
            public static string LogFile = null;
            public static string SaveFile = null;

            public Logging(string Logpath, string savepath)
            {
                LogFile = Logpath;
                SaveFile = savepath;
            }

            private static readonly object locker = new object();



            public void WriteToLog(string Host, string input, string State)
            {
                string line = Host + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + input + "\" " + State;
                log.Add(line);
                lock (locker)
                {
                    if (SaveFile == "")
                    {
                    }
                    else
                    {
                        try
                        {
                            StreamWriter SW;
                            SW = new StreamWriter(SaveFile, false);
                            foreach (var entry in data)
                            {
                                SW.WriteLine(entry.Key + " " + entry.Value);
                            }
                            SW.Close();
                        }
                        catch (Exception s)
                        {
                            {
                                Console.WriteLine("Unable to Write Save File");
                            }
                        }
                    }
                }
            }
        }
        private void SaveLog_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog ///based on code from documentation https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.savefiledialog?view=netframework-4.7.2
            {
                FileName = "Log", // Default file name
                DefaultExt = ".txt", // Default file extension
                Title = "Save Log",
                Filter = "Text documents (.txt)|*.txt" // Filter files by extension
            };

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                using (StreamWriter file = new StreamWriter(filename))
                    foreach (var entry in log)
                        file.WriteLine(entry);
            }
        }
    }
}
