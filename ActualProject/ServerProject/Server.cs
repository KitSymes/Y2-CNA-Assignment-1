using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using Packets;

namespace ServerProject
{
    class Server
    {
        private TcpListener tcpListener;
        private ConcurrentDictionary<int, ConnectedClient> connectedClients;
        private ConcurrentDictionary<Guid, ConnectedClient> clientsByID;

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
            clientsByID = new();
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
            Packet receivedMessage;
            ConnectedClient client = connectedClients[index];

            // TODO send user list

            while ((receivedMessage = client.Read()) != null)
            {
                if (!client.ready)
                {
                    if (receivedMessage.PacketType == PacketType.CLIENT_JOIN)
                    {
                        ClientJoinPacket packet = (ClientJoinPacket)receivedMessage;
                        if (clientsByID.ContainsKey(packet._guid))
                            continue;
                        clientsByID.TryAdd(packet._guid, client);
                        client.nickname = packet._name;
                        client.ready = true;
                    }
                    continue;
                }

                switch (receivedMessage.PacketType)
                {
                    case PacketType.CHAT_MESSAGE:
                        ChatMessagePacket packet = (ChatMessagePacket)receivedMessage;
                        foreach (ConnectedClient c in connectedClients.Values)
                            if (c.ready)
                                c.Send(new ChatMessageReceivedPacket(packet.message, client.guid));
                        break;
                    default:
                        break;
                }
            }

            client.Close();
            connectedClients.TryRemove(index, out client);
        }
    }
}
