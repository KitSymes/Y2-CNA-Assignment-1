using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ClientProject
{
    class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        public Client()
        {
            tcpClient = new TcpClient();
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
            string input;
            ProcessServerResponse();

            while ((input = Console.ReadLine()) != null)
            {
                writer.WriteLine(input);
                writer.Flush();
                if (input.ToLower() == "exit")
                    break;
                ProcessServerResponse();
            }

            reader.Close();
            writer.Close();
            stream.Close();
            tcpClient.Close();
        }

        public void RPS()
        {
            string input;


            while ((input = reader.ReadLine()) != null)
            {
                if (input == "input")
                    break;

                Console.WriteLine("Server: " + input);
                Console.WriteLine();
            }

            writer.WriteLine(Console.ReadLine());
            writer.Flush();

            while ((input = reader.ReadLine()) != null)
            {
                if (input == "ok")
                    break;

                Console.WriteLine("Server: " + input);
                Console.WriteLine();

                writer.WriteLine(Console.ReadLine());
                writer.Flush();
            }

            while ((input = reader.ReadLine()) != null)
            {
                if (input == "exit")
                    break;

                Console.WriteLine("Server: " + input);
                Console.WriteLine();
            }

            reader.Close();
            writer.Close();
            stream.Close();
            tcpClient.Close();
        }

        private void ProcessServerResponse()
        {
            Console.WriteLine("Server: " + reader.ReadLine());
            Console.WriteLine();
        }
    }
}
