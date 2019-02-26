using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace locationserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Dictionary<DateTime, string> log = new Dictionary<DateTime, string>();
        static Dictionary<string, string> data = new Dictionary<string, string>();
        static string logstatement = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

                TcpListener listener;
                Socket connection;
                NetworkStream socketStream;
                try
                {
                    listener = new TcpListener(IPAddress.Parse(Address.Text), Int32.Parse(Port.Text));
                    listener.Start();
                    Status.AppendText("server started listening");

                    Start.IsEnabled = false;
                    Stop.IsEnabled = true;
                    while (Start.IsEnabled == false)
                    {
                        connection = listener.AcceptSocket();
                        socketStream = new NetworkStream(connection);
                        logstatement += "- " + IPAddress.Parse(((IPEndPoint)listener.LocalEndpoint).Address.ToString()) + " - ";
                        Status.AppendText("Connection Recieved");
                        doRequest(socketStream);
                        socketStream.Close();
                        connection.Close();
                    }
                }
                catch (Exception x)
                {
                    Status.AppendText("Exception:" + x.ToString());
                }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
        }
        
        private void doRequest(NetworkStream socketStream)
        {
            try
            {
                socketStream.ReadTimeout = 1000;
                socketStream.WriteTimeout = 1000;
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);

                string line = sr.ReadLine();
                string[] sections = line.Split(new char[] { ' ' }, 2);
                DateTime localDate = DateTime.Now;
                if (sections.Length == 2)
                {
                    if (data.ContainsKey(sections[0]))
                    {
                        data[sections[0]] = sections[1];
                        sw.WriteLine("OK");
                        logstatement += "Put " + line + " - OK";
                    }
                    else
                    {
                        data.Add(sections[0], sections[1]);
                        sw.WriteLine("OK");
                        logstatement += "Put " + line + " - OK";
                    }
                }
                if (sections.Length == 1)
                {

                    if (data.ContainsKey(sections[0]))
                    {
                        sw.WriteLine(data[sections[0]]);
                        logstatement += "Get " + line + " - OK";
                    }
                    else
                    {
                        sw.WriteLine("ERROR: no entries found");
                        logstatement += "Get " + line + " ERROR: no entries found";
                    }
                }
                else
                {

                }
                log.Add(localDate, logstatement);
                Status.AppendText(log.Keys.Last() + " " + log.Values.Last());
                logstatement = "";
                sw.Flush();

            }

            catch (Exception x)
            {
                Status.AppendText(x.ToString());
            }
        }

        private void Status_TextChanged(object sender, TextChangedEventArgs e)
        {
            Status.ScrollToEnd();
        }
    }
}
