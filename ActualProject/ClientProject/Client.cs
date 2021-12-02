using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Packets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ClientProject
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;

        private UdpClient udpClient;

        private ConcurrentDictionary<Guid, OtherClient> clients;
        public ChatChannel mainChannel;

        private MainWindow form;

        public Guid guid;
        public String nickname;

        public Client()
        {
            tcpClient = new();

            clients = new();
            mainChannel = new();
            guid = Guid.NewGuid();

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
                formatter = new();

                udpClient = new();
                udpClient.Connect(ip, port);
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
            Thread tcp = new(() =>
            {
                TDPProcessServerResponse();
            });
            tcp.Start();
            Thread udp = new(() =>
            {
                UDPProcessServerResponse();
            });
            udp.Start();
            Login();
        }

        public void Close()
        {
            if (!tcpClient.Connected)
                return;
            reader.Close();
            writer.Close();
            stream.Close();
            tcpClient.Close();
            udpClient.Close();
        }

        public void Login()
        {
            TCPSend(new LoginPacket((IPEndPoint)tcpClient.Client.LocalEndPoint, guid, nickname));
        }

        #region
        public void UDPSend(Packet packet)
        {
            if (udpClient == null)
                return;
            MemoryStream ms = new();
            formatter.Serialize(ms, packet);
            byte[] buffer = ms.GetBuffer();
            udpClient.Send(buffer, buffer.Length);
        }

        private void UDPProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new(IPAddress.Any, 0);
                while(true)
                {
                    byte[] buffer = udpClient.Receive(ref endPoint);
                    MemoryStream ms = new(buffer);
                    Packet packet = formatter.Deserialize(ms) as Packet;
                    switch (packet.packetType)
                    {
                        default:
                            break;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }
        #endregion

        #region TCP
        public Packet TCPRead()
        {
            int numberOfBytes;
            if ((numberOfBytes = reader.ReadInt32()) != -1)
            {
                byte[] buffer = reader.ReadBytes(numberOfBytes);
                MemoryStream ms = new(buffer);
                return formatter.Deserialize(ms) as Packet;
            }
            return null;
        }

        private void TDPProcessServerResponse()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    Packet receivedMessage = TCPRead();
                    switch (receivedMessage.packetType)
                    {
                        case PacketType.CLIENT_JOIN:
                            ClientJoinPacket joinPacket = (ClientJoinPacket)receivedMessage;
                            OtherClient otherClient = new(joinPacket.guid, joinPacket.name, joinPacket.guid == guid);
                            clients.TryAdd(joinPacket.guid, otherClient);
                            form.AddClient(otherClient);
                            mainChannel.Add(joinPacket.name + " has joined.");
                            if (mainChannel.watched)
                                form.UpdateChatBox(mainChannel.Format());
                            break;
                        case PacketType.CLIENT_NAME_UPDATE_RECEIVED:
                            ClientNameChangeReceivedPacket nameChangePacket = (ClientNameChangeReceivedPacket)receivedMessage;
                            if (clients.ContainsKey(nameChangePacket.guid))
                            {
                                mainChannel.Add(clients[nameChangePacket.guid].name + " has changed their name to " + nameChangePacket.name + ".");
                                clients[nameChangePacket.guid].ChangeName(nameChangePacket.name, form);
                                if (mainChannel.watched)
                                    form.UpdateChatBox(mainChannel.Format());
                            }
                            break;
                        case PacketType.CHAT_MESSAGE_RECEIVED:
                            ChatMessageReceivedPacket messagePacket = (ChatMessageReceivedPacket)receivedMessage;
                            if (clients.ContainsKey(messagePacket.from))
                            {
                                mainChannel.Add(clients[messagePacket.from].name + ": " + messagePacket.message);
                                if (mainChannel.watched)
                                    form.UpdateChatBox(mainChannel.Format());
                            }
                            else
                            {
                                mainChannel.Add("???: " + messagePacket.message);
                                if (mainChannel.watched)
                                    form.UpdateChatBox(mainChannel.Format());
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void TCPSend(Packet packet)
        {
            if (!tcpClient.Connected)
                return;
            MemoryStream ms = new();
            formatter.Serialize(ms, packet);
            byte[] buffer = ms.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }
        #endregion
    }
}
