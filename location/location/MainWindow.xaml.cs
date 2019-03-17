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
                string username = null;
                string location = null;

                client.Connect(Address.Text, short.Parse(Port.Text));
                if (short.Parse(TimeBox.Text) > 0)
                {
                    client.ReceiveTimeout = short.Parse(TimeBox.Text);
                    client.SendTimeout = short.Parse(TimeBox.Text);
                }

                StreamWriter sw = new StreamWriter(client.GetStream());
                StreamReader sr = new StreamReader(client.GetStream());
                string response = "";
                sw.AutoFlush = true;
                if (Http0_9.IsChecked == true)
                {
                    if (Location.Text == "" && User.Text != "")
                    {
                        sw.Write("GET /" + User.Text + "\r\n");
                        response = sr.ReadLine();
                        sr.ReadLine();
                        string OH = sr.ReadLine();
                        location = sr.ReadLine();
                        if (response.Contains("404 Not Found"))
                        {
                            Status.AppendText(response +"\n");
                        }
                        else
                        {
                            Status.AppendText(User.Text + " is " + location + "\n");
                        }
                    }
                    else if (Location.Text != "" && User.Text != "")
                    {
                        sw.Write("PUT /" + User.Text + "\r\n" + "\r\n" + Location.Text + "\r\n");
                        response = sr.ReadLine();
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
                        response = sr.ReadLine();
                        sr.ReadLine();
                        string OH = sr.ReadLine();
                        location = sr.ReadLine();

                        if (response.Contains("404 Not Found"))
                        {
                            Status.AppendText(response+ "\n");
                        }
                        else
                        {
                            Status.AppendText(User.Text + " is " + location + "\n");
                        }
                    }
                    else if (Location.Text != "" && User.Text != "")
                    {
                        sw.Write("POST /" + User.Text + " HTTP/1.0" + "\r\n" + "Content-Length: " + Location.Text.Length + "\r\n" + "\r\n" + Location.Text);
                        response = sr.ReadLine();
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
                        response = sr.ReadLine();
                        sr.ReadLine();
                        string OH = sr.ReadLine();
                        while (OH != "")
                        {
                            OH = sr.ReadLine();
                        }
                        location = sr.ReadLine();
                        while (sr.Peek() >= 0)
                        {
                            location += sr.ReadLine() + "\n";
                        }

                        if (response.Contains("404 Not Found"))
                        {
                            Status.AppendText(response + "\n");
                        }
                        else
                        {
                            Status.AppendText(User.Text + " is " + location);
                        }
                    }
                    else if (Location.Text != "" && User.Text != "")
                    {
                        int length = User.Text.Length + Location.Text.Length + 15;
                        sw.Write("POST / " + "HTTP/1.1" + "\r\n" + "Host: " + Address.Text + "\r\n" + "Content-Length: " + length + "\r\n" + "\r\n" + "name=" + User.Text + "&location=" + Location.Text);
                        response = sr.ReadLine();
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
                else if (Whois.IsChecked == true)
                {
                    if (Location.Text == "" && User.Text != "")
                    {
                        sw.WriteLine(User.Text);
                        response = sr.ReadLine();
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
                        response = sr.ReadLine();
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

                if (Debug.IsChecked == true)
                {
                    Status.AppendText(x.ToString());
                }
                else
                {
                    Status.AppendText("Something went wrong + \n");
                }
            }
        }

        private void Status_TextChanged(object sender, TextChangedEventArgs e)
        {
            Status.ScrollToEnd();
        }
    }
}
