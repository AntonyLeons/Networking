﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace location
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [STAThread]
        public static int Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    List<string> clean = new List<string>(args);
                    TcpClient client = new TcpClient();
                    string host = "whois.net.dcs.hull.ac.uk";
                    int port = 43;
                    string protocol = "whois";
                    string username = null;
                    string location = null;
                    string input = "";
                    for (int i = 0; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "-h": host = args[i++]; break;
                            case "-p": port = int.Parse(args[i++]); break;
                            case "-h9":
                            case "-h0":
                            case "-h1": protocol = args[i]; break;

                            default:
                                if (username == null)
                                {
                                    username = args[i];
                                }
                                else if (location == null)
                                {
                                    location = args[i];
                                }
                                else
                                {
                                    Console.WriteLine("Too many arguments");
                                }
                                break;
                        }
                        if (username == null)
                        {
                            Console.WriteLine("Too few arguments");
                        }
                    }
                    client.Connect(host, port);
                    client.ReceiveTimeout = 1000;
                    client.SendTimeout = 1000;
                    StreamWriter sw = new StreamWriter(client.GetStream());
                    StreamReader sr = new StreamReader(client.GetStream());
                    StringBuilder appendLine = new StringBuilder();
                    string response = "";
                    sw.AutoFlush = true;
                    switch (protocol)
                    {
                        case "whois":
                            if (location == null)
                            {
                                sw.WriteLine(username);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("ERROR: no entries found"))
                                {
                                    Console.WriteLine(response);
                                }
                                else
                                {
                                    Console.WriteLine(username + " is " + response);
                                }
                            }
                            else
                            {
                                sw.WriteLine(username + " " + location);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location);
                                }
                            }
                            break;
                        case "-h9":
                            if (location == null)
                            {
                                sw.Write("GET /" + username + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    input = sr.ReadLine();
                                    appendLine.Append(input);
                                }
                                response = appendLine.ToString();
                                location = input;
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                }
                                else
                                {
                                    Console.WriteLine(username + " is " + location);
                                }
                            }
                            else 
                            {
                                sw.Write("PUT /" + username + "\r\n" + "\r\n" + location + "\r \n");
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location);
                                }
                            }
                            break;
                        case "h0":
                            if (location==null)
                            {
                                sw.Write("GET /?" + username + " HTTP/1.0" + "\r\n" + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    input = sr.ReadLine();
                                    appendLine.Append(input);
                                }
                                response = appendLine.ToString();
                                location = input;
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine(username + " is " + location);
                                    break;
                                }
                            }
                            else
                            {
                                sw.Write("POST /" + username + " HTTP/1.0" + "\r\n" + "Content-Length: " + location.Length + "\r\n" + "\r\n" + location);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location);
                                    break;
                                }
                            }
                            break;
                        case "h1":
                            if (location==null)
                            {
                                sw.Write("GET /?name=" + username + " HTTP/1.1" + "\r\n" + "Host: " + host + "\r\n" + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    input = sr.ReadLine();
                                    appendLine.Append(input);
                                }
                                response = appendLine.ToString();
                                location = input;
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine(username + " is " + location);
                                    break;
                                }
                            }
                            else
                            {
                                int length = username.Length + location.Length + 15;
                                sw.Write("POST / " + "HTTP/1.1" + "\r\n" + "Host: " + host + "\r\n" + "Content-Length: " + length + "\r\n" + "\r\n" + "name=" + username + "&location=" + location);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(username + " location changed to be " + location);
                                    break;
                                }
                            }
                            break;
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong");
                    Console.WriteLine(e);
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
    }
}
