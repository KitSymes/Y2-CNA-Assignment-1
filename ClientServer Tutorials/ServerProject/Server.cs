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
        bool ready = false;
        bool p1 = false, p2 = false;
        bool p1done = false, p2done = false;
        string p1choice, p2choice;

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

            while (clientIndex < 2)
            {
                Socket socket = tcpListener.AcceptSocket();

                Console.WriteLine("Connection Made");

                int index = clientIndex;
                connectedClients.TryAdd(index, new(socket));
                clientIndex++;

                Thread thread = new Thread(() => { RPS(index); });
                thread.Start();
            }
            ready = true;
            while (!p1done || !p2done)
            {
                Thread.Sleep(1000);
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

            client.Send("Hello!! Enter 0 for options!");

            while ((receivedMessage = client.Read()) != null)
            {
                client.Send(GetReturnMessage(receivedMessage));
                if (receivedMessage.ToLower() == "exit")
                    break;
            }

            client.Close();
            connectedClients.TryRemove(index, out client);
        }

        private void RPS(int index)
        {
            string receivedMessage;
            ConnectedClient client = connectedClients[index];

            bool isP1 = index == 0;

            client.Send("Hello!! Waiting for another player...!");

            while (!ready)
            {
                Thread.Sleep(100);
            }

            client.Send("Get ready!");
            Thread.Sleep(1000);
            client.Send("3!");
            Thread.Sleep(1000);
            client.Send("2!");
            Thread.Sleep(1000);
            client.Send("1!");
            Thread.Sleep(1000);
            client.Send("Rock/Paper/Scissors!");
            client.Send("input");

            while ((receivedMessage = client.Read()) != null)
            {
                if (receivedMessage.ToLower() == "rock" ||
                    receivedMessage.ToLower() == "paper" ||
                    receivedMessage.ToLower() == "scissors")
                {
                    if (isP1)
                        p1choice = receivedMessage.ToLower();
                    else
                        p2choice = receivedMessage.ToLower();

                    break;
                }
                client.Send("Invalid!");
            }

            client.Send("ok");
            client.Send("Waiting for opponent...");

            if (isP1)
                p1 = true;
            else
                p2 = true;

            while (!p1 || !p2)
            {
                Thread.Sleep(100);
            }


            if (p1choice == "rock")
            {
                if (p2choice == "rock")
                {
                    client.Send("You drew!");
                } else if (p2choice == "paper")
                {
                    client.Send("You " + (!isP1 ? "won" : "lost") + "!");
                }
                else if (p2choice == "scissors")
                {
                    client.Send("You " + (isP1 ? "won" : "lost") + "!");
                }
            } else if (p1choice == "paper")
            {
                if (p2choice == "rock")
                {
                    client.Send("You " + (isP1 ? "won" : "lost") + "!");
                }
                else if (p2choice == "paper")
                {
                    client.Send("You drew!");
                }
                else if (p2choice == "scissors")
                {
                    client.Send("You " + (!isP1 ? "won" : "lost") + "!");
                }
            }
            else if (p1choice == "scissors")
            {
                if (p2choice == "rock")
                {
                    client.Send("You " + (!isP1 ? "won" : "lost") + "!");
                }
                else if (p2choice == "paper")
                {
                    client.Send("You " + (isP1 ? "won" : "lost") + "!");
                }
                else if (p2choice == "scissors")
                {
                    client.Send("You drew!");
                }
            }

            client.Send("exit");

            if (isP1)
                p1done = true;
            else
                p2done = true;

            client.Close();
            connectedClients.TryRemove(index, out _);
        }

        private string GetReturnMessage(string code)
        {
            switch (code.ToLower())
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
