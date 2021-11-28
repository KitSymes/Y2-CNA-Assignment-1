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

                        foreach (ConnectedClient cl in clientsByID.Values)
                            client.Send(new ClientJoinPacket(cl.guid, cl.nickname));

                        clientsByID.TryAdd(packet._guid, client);
                        client.guid = packet._guid;
                        client.nickname = packet._name;
                        client.ready = true;

                        foreach (ConnectedClient cl in clientsByID.Values)
                            cl.Send(packet);
                    }
                    continue;
                }

                switch (receivedMessage.PacketType)
                {
                    case PacketType.CHAT_MESSAGE:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)receivedMessage;
                        foreach (ConnectedClient c in clientsByID.Values)
                            if (c.ready)
                                c.Send(new ChatMessageReceivedPacket(chatPacket.message, client.guid));
                        break;
                    case PacketType.CLIENT_NAME_UPDATE:
                        ClientNameChangePacket nameChangePacket = (ClientNameChangePacket)receivedMessage;
                        client.nickname = nameChangePacket._name;
                        foreach (ConnectedClient cl in clientsByID.Values)
                            cl.Send(new ClientNameChangeReceivedPacket(client.guid, nameChangePacket._name));
                        break;
                    default:
                        break;
                }
            }

            client.Close();
            ConnectedClient clientOut;
            connectedClients.TryRemove(index, out clientOut);
            clientsByID.TryRemove(client.guid, out clientOut);
            foreach (ConnectedClient c in clientsByID.Values)
                c.Send(new ClientLeftPacket(client.guid));
        }
    }
}
