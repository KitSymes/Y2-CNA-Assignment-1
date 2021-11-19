using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace ServerProject
{
    class Server
    {
        private TcpListener tcpListener;
        private ConcurrentDictionary<int, ConnectedClient> connectedClients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tcpListener = new(ip, port);
        }

        public void Start()
        {
            tcpListener.Start();

            Console.WriteLine("Server is Listening");

            connectedClients = new();
            int clientIndex = 0;

            while (true)
            {
                Socket socket = tcpListener.AcceptSocket();

                Console.WriteLine("Connection Made");

                int index = clientIndex;
                connectedClients.TryAdd(index, new(socket));
                clientIndex++;

                Thread thread = new Thread(() => { ClientMethod(index); });
                thread.Start();
            }
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        private void ClientMethod(int index)
        {
            string receivedMessage;
            ConnectedClient client = connectedClients[index];

            client.Send("Connected!");

            while ((receivedMessage = client.Read()) != null)
            {
                foreach (ConnectedClient c in connectedClients.Values)
                    c.Send(receivedMessage);
            }

            client.Close();
            connectedClients.TryRemove(index, out client);
        }
    }
}
