using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ClientProject
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        private MainWindow form;

        public Client()
        {
            tcpClient = new TcpClient();
            form = new(this);
            form.ShowDialog();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipAddress);
                tcpClient.Connect(ip, port);
                stream = tcpClient.GetStream();
                writer = new(stream);
                reader = new(stream);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void Run()
        {

            Thread thread = new(() =>
            {
                ProcessServerResponse();
            });
            thread.Start();

        }

        public void Close()
        {
            if (!tcpClient.Connected)
                return;
            reader.Close();
            writer.Close();
            stream.Close();
            tcpClient.Close();
        }

        private void ProcessServerResponse()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    string input = reader.ReadLine();
                    form.UpdateChatBox(input);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void SendMessage(string message)
        {
            if (!tcpClient.Connected)
                return;
            writer.WriteLine(message);
            writer.Flush();
        }
    }
}
