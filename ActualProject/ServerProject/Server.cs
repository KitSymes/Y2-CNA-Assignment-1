using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using Packets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerProject
{
    class Server
    {
        private TcpListener tcpListener;
        private UdpClient udpClient;

        private ConcurrentDictionary<int, ConnectedClient> connectedClients;
        private ConcurrentDictionary<Guid, ConnectedClient> clientsByID;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            tcpListener = new(ip, port);

            udpClient = new(port);
        }

        public void Start()
        {
            tcpListener.Start();
            Thread udp = new(() =>
            {
                UdpListen();
            });
            udp.Start();

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

                Thread thread = new Thread(() => { TCPClientMethod(index); });
                thread.Start();
            }
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        private void TCPClientMethod(int index)
        {
            Packet receivedMessage;
            ConnectedClient client = connectedClients[index];

            while ((receivedMessage = client.TCPRead()) != null)
            {
                if (!client.ready)
                {
                    if (receivedMessage.packetType == PacketType.LOGIN)
                    {
                        LoginPacket packet = (LoginPacket)receivedMessage;
                        if (clientsByID.ContainsKey(packet.guid))
                            continue;

                        foreach (ConnectedClient cl in clientsByID.Values)
                            client.TCPSend(new ClientJoinPacket(cl.guid, cl.nickname));

                        clientsByID.TryAdd(packet.guid, client);
                        client.endPoint = (IPEndPoint)packet.endPoint;
                        client.guid = packet.guid;
                        client.nickname = packet.name;
                        client.ready = true;

                        foreach (ConnectedClient cl in clientsByID.Values)
                            cl.TCPSend(new ClientJoinPacket(client.guid, client.nickname));
                    }
                    continue;
                }

                switch (receivedMessage.packetType)
                {
                    case PacketType.CHAT_MESSAGE:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)receivedMessage;
                        foreach (ConnectedClient c in clientsByID.Values)
                            if (c.ready)
                                c.TCPSend(new ChatMessageReceivedPacket(chatPacket.message, client.guid));
                        break;
                    case PacketType.CLIENT_NAME_UPDATE:
                        ClientNameChangePacket nameChangePacket = (ClientNameChangePacket)receivedMessage;
                        client.nickname = nameChangePacket.name;
                        foreach (ConnectedClient cl in clientsByID.Values)
                            cl.TCPSend(new ClientNameChangeReceivedPacket(client.guid, nameChangePacket.name));
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
                c.TCPSend(new ClientLeftPacket(client.guid));
        }
    
        private void UdpListen()
        {
            BinaryFormatter formatter = new();
            try
            {
                IPEndPoint endPoint = new(IPAddress.Any, 0);
                while (true)
                {
                    byte[] buffer = udpClient.Receive(ref endPoint);
                    MemoryStream ms = new(buffer);
                    Packet packet = formatter.Deserialize(ms) as Packet;
                    foreach (ConnectedClient c in connectedClients.Values)
                    {
                        if (endPoint.ToString() != c.endPoint.ToString())
                            continue;
                        switch (packet.packetType)
                        {
                            default:
                                break;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }
    }
}
