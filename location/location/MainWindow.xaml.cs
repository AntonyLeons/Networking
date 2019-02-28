using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace location
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(Address.Text, Int32.Parse(Port.Text));
                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                StreamWriter sw = new StreamWriter(client.GetStream());
                StreamReader sr = new StreamReader(client.GetStream());
                StringBuilder appendLine = new StringBuilder();
                StringBuilder appendData = new StringBuilder();
                string location = "";
                string username;
                string input="";
                int c = 0;
                string response = "";
                sw.AutoFlush = true;

                    if (Http0_9.IsChecked == true)
                    {
                        if (Location.Text == "" && User.Text != "")
                        {
                            sw.Write("GET /" + User.Text + "\r\n");
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
                            location = appendData.ToString();
                        if (response.Contains("404 Not Found"))
                            {
                                Status.AppendText(response + "\n");
                            }
                            else
                            {
                                Status.AppendText(User.Text + " is " + location + "\n");
                            }
                        }
                        else if (Location.Text != "" && User.Text != "")
                        {
                            sw.Write("PUT /" + User.Text + "\r\n" + "\r\n" + Location.Text + "\r\n");
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("OK"))
                            {
                                Status.AppendText(User.Text + " location changed to be " + Location.Text + "\n");
                            }
                        }
                        else
                        {
                            Status.AppendText("Invalid arguments provided \n");
                        }
                    }
                    else if (Http1_0.IsChecked == true) 
                    {
                        if (Location.Text == "" && User.Text != "")
                        {
                            sw.Write("GET /?" + User.Text + " HTTP/1.0" + "\r\n" + "\r\n");
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
                        location = appendData.ToString();
                        if (response.Contains("404 Not Found"))
                        {
                            Status.AppendText(response + "\n");
                        }
                        else
                        {
                            Status.AppendText(User.Text + " is " + location + "\n");
                        }
                    }
                        else if (Location.Text != "" && User.Text != "")
                        {
                            sw.Write("POST /" + User.Text + " HTTP/1.0" + "\r\n" + "Content-Length: " + Location.Text.Length + "\r\n" + "\r\n" + Location.Text);
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("OK"))
                            {
                                Status.AppendText(User.Text + " location changed to be " + Location.Text + "\n");
                            }
                        }
                        else
                        {
                            Status.AppendText("Invalid arguments provided \n");
                        }
                    }
                    else if (Http1_1.IsChecked == true)
                    {
                        
                        if (Location.Text == "" && User.Text != "")
                        {
                            sw.Write("GET /?name=" + User.Text + " HTTP/1.1" + "\r\n" + "Host: " + Address.Text + "\r\n" + "\r\n");
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
                        location = appendData.ToString();
                        if (response.Contains("404 Not Found"))
                        {
                            Status.AppendText(response + "\n");
                        }
                        else
                        {
                            Status.AppendText(User.Text + " is " + location + "\n");
                        }
                    }
                        else if (Location.Text != "" && User.Text != "")
                        {
                            int length = User.Text.Length + Location.Text.Length + 15;
                            sw.Write("POST / " + "HTTP/1.1" + "\r\n" + "Host: " + Address.Text + "\r\n" + "Content-Length: " + length + "\r\n" + "\r\n" + "name=" + User.Text + "&location=" + Location.Text);
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("OK"))
                            {
                                Status.AppendText(User.Text + " location changed to be " + Location.Text + "\n");
                            }
                        }
                        else
                        {
                            Status.AppendText("Invalid arguments provided \n");
                            
                        }
                    }
                    else if (Whois.IsChecked ==true)
                    {
                        if (Location.Text == "" && User.Text != "")
                        {
                            sw.WriteLine(User.Text);
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("ERROR: no entries found"))
                            {
                                Status.AppendText(response + "\n");
                            }
                            else
                            {
                                Status.AppendText(User.Text + " is " + response + "\n");
                            }
                        }
                        else if (Location.Text != "" && User.Text != "")
                        {
                            sw.WriteLine(User.Text + " " + Location.Text);
                            while (sr.Peek() >= 0)
                            {
                                appendLine.Append(sr.ReadLine());
                            }
                            response = appendLine.ToString();
                            if (response.Contains("OK"))
                            {
                                Status.AppendText(User.Text + " location changed to be " + Location.Text + "\n");
                            }
                        }
                        else
                        {
                            Status.AppendText("Invalid arguments provided \n");
                        }
                    }
                }
            

            catch (Exception x)
            {
                Status.AppendText("Something went wrong + \n");
            }
        }

        private void Status_TextChanged(object sender, TextChangedEventArgs e)
        {
            Status.ScrollToEnd();
        }
    }
}
