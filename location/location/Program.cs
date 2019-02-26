using System;
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
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i]=="-h")
                        {
                            host = args[i + 1];
                            clean.Remove(args[i]);
                            clean.Remove(args[i + 1]);
                        }
                       else if (args[i]=="-p")
                        {
                            port = Int32.Parse(args[i + 1]);
                            clean.Remove(args[i]);
                            clean.Remove(args[i + 1]);
                        }
                    }
                    client.Connect(host, port);
                    client.ReceiveTimeout = 1000;
                    client.SendTimeout = 1000;
                    StreamWriter sw = new StreamWriter(client.GetStream());
                    StreamReader sr = new StreamReader(client.GetStream());
                    StringBuilder appendLine = new StringBuilder();
                    StringBuilder appendData = new StringBuilder();
                    string LocationData = "";
                    int c = 0;
                    bool flag = false;

                    string response = "";
                    sw.AutoFlush = true;

                    for (int i = 0; i < clean.Count; i++)
                    {
                        if (clean[i] == "-h9")
                        {
                            clean.Remove(clean[i]);
                            flag = true;
                            if (clean.Count == 1)
                            {
                                sw.Write("GET /" + clean[0] + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    c++;
                                    if (c <= 3)
                                    {
                                        appendLine.Append(sr.ReadLine());
                                    }
                                    else
                                    {
                                        appendData.Append(sr.ReadLine());
                                    }
                                }
                                response = appendLine.ToString();
                                LocationData = appendData.ToString();
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine(clean[0] + " is " + LocationData);
                                    break;
                                }
                            }
                            else if (clean.Count == 2)
                            {
                                sw.Write("PUT /" + clean[0] + "\r\n" + "\r\n" + clean[1] + "\r \n");
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(clean[0] + " location changed to be " + clean[1]);
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments provided");
                                break;
                            }
                        }
                        else if (clean[i] == "-h0") 
                        {
                            clean.Remove(clean[i]);
                            flag = true;
                            if (clean.Count == 1)
                            {
                                sw.Write("GET /?" + clean[0] + " HTTP/1.0" + "\r\n" + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    c++;
                                    if (c <= 3)
                                    {
                                        appendLine.Append(sr.ReadLine());
                                    }
                                    else
                                    {
                                        appendData.Append(sr.ReadLine());
                                    }
                                }
                                response = appendLine.ToString();
                                LocationData = appendData.ToString();
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine(clean[0] + " is " + LocationData);
                                    break;
                                }
                            }
                            else if (clean.Count == 2)
                            {
                                sw.Write("POST /" + clean[0] + " HTTP/1.0" + "\r\n" + "Content-Length: " + clean[1].Length + "\r\n" + "\r\n" + clean[1]);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(clean[0] + " location changed to be " + clean[1]);
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments provided");
                                break;
                            }
                        }
                        else if (clean[i] == "-h1")
                        {
                            clean.Remove(clean[i]);
                            flag = true;
                            if (clean.Count == 1)
                            {
                                sw.Write("GET /?name=" + clean[0] + " HTTP/1.1" + "\r\n" + "Host: " + host + "\r\n" + "\r\n");
                                while (sr.Peek() >= 0)
                                {
                                    c++;
                                    if (c <= 3)
                                    {
                                        appendLine.Append(sr.ReadLine());
                                    }
                                    else
                                    {
                                        appendData.Append(sr.ReadLine());
                                    }
                                }
                                response = appendLine.ToString();
                                LocationData = appendData.ToString();
                                if (response.Contains("404 Not Found"))
                                {
                                    Console.WriteLine(response);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine(clean[0] + " is " + LocationData);
                                    break;
                                }
                            }
                            else if (clean.Count == 2)
                            {
                                int length = clean[0].Length + clean[1].Length + 15;
                                sw.Write("POST / " + "HTTP/1.1" + "\r\n" + "Host: " + host + "\r\n" + "Content-Length: " + length + "\r\n" + "\r\n" + "name=" + clean[0] + "&location=" + clean[1]);
                                while (sr.Peek() >= 0)
                                {
                                    appendLine.Append(sr.ReadLine());
                                }
                                response = appendLine.ToString();
                                if (response.Contains("OK"))
                                {
                                    Console.WriteLine(clean[0] + " location changed to be " + clean[1]);
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid arguments provided");
                                break;
                            }
                        }
                    }
                    if (flag == false)
                    {
                        if (clean.Count == 1)
                        {
                            sw.WriteLine(clean[0]);
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
                                Console.WriteLine(clean[0] + " is " + response);
                            }
                        }
                        else if (clean.Count == 2)
                        {
                            sw.WriteLine(clean[0] + " " + clean[1]);
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("OK"))
                            {
                                Console.WriteLine(clean[0] + " location changed to be " + clean[1]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid arguments provided");
                        }
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

