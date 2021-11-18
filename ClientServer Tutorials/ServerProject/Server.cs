using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ServerProject
{
    class Server
    {
        private TcpListener tcpListener;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tcpListener = new(ip, port);
        }

        public void Start()
        {
            tcpListener.Start();

            Console.WriteLine("Server is Listening");

            Socket socket = tcpListener.AcceptSocket();
            Console.WriteLine("Connection Made");
            ClientMethod(socket);
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        private void ClientMethod(Socket socket)
        {
            string receivedMessage;
            NetworkStream stream = new(socket);
            StreamReader reader = new(stream, Encoding.UTF8);
            StreamWriter writer = new(stream, Encoding.UTF8);

            writer.WriteLine("Hello!! Enter 0 for options!");
            writer.Flush();

            while ((receivedMessage = reader.ReadLine()) != null)
            {
                writer.WriteLine(GetReturnMessage(receivedMessage));
                writer.Flush();
                if (receivedMessage.ToLower() == "exit")
                    break;
            }

            reader.Close();
            writer.Close();
            socket.Close();
        }

        private string GetReturnMessage(string code)
        {
            switch(code.ToLower())
            {
                case "hi":
                    return "Hello";
                case "0":
                    return "Type Hi for a greeting, 1 for a joke, 2 for a Haiku or Exit to exit";
                case "1":
                    return "I would tell you a UDP joke, but you might not get it";
                case "2":
                    return "Networking is fun; Parsing messages is not; Refrigerator";
                default:
                    return "Invalid";
            }
        }
    }
}
