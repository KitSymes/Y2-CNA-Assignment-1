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

        private ConcurrentDictionary<Guid, OtherClient> clients;
        public ChatChannel mainChannel;

        private MainWindow form;

        public Guid guid;
        public String name;

        public Client()
        {
            tcpClient = new TcpClient();

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

        public Packet Read()
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

        private void ProcessServerResponse()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    Packet receivedMessage = Read();
                    switch (receivedMessage.PacketType)
                    {
                        case PacketType.CLIENT_JOIN:
                            ClientJoinPacket joinPacket = (ClientJoinPacket)receivedMessage;
                            OtherClient otherClient = new OtherClient(joinPacket._guid, joinPacket._name, joinPacket._guid == guid);
                            clients.TryAdd(joinPacket._guid, otherClient);
                            form.AddClient(otherClient);
                            mainChannel.Add(joinPacket._name + " has joined.");
                            form.UpdateChatBox(mainChannel.Format());
                            break;
                        case PacketType.CLIENT_NAME_UPDATE_RECEIVED:
                            ClientNameChangeReceivedPacket nameChangePacket = (ClientNameChangeReceivedPacket)receivedMessage;
                            if (clients.ContainsKey(nameChangePacket._guid))
                            {
                                mainChannel.Add(clients[nameChangePacket._guid].name + " has changed their name to " + nameChangePacket._name + ".");
                                clients[nameChangePacket._guid].ChangeName(nameChangePacket._name, form);
                            }
                            break;
                        case PacketType.CHAT_MESSAGE_RECEIVED:
                            ChatMessageReceivedPacket messagePacket = (ChatMessageReceivedPacket)receivedMessage;
                            if (clients.ContainsKey(messagePacket.from))
                            {
                                mainChannel.Add(clients[messagePacket.from].name + ": " + messagePacket.message);
                                if (mainChannel.watched)
                                {
                                    form.UpdateChatBox(mainChannel.Format());
                                }
                            }
                            else
                            {
                                mainChannel.Add("???: " + messagePacket.message);
                                if (mainChannel.watched)
                                {
                                    form.UpdateChatBox(mainChannel.Format());
                                }
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

        public void Send(Packet message)
        {
            if (!tcpClient.Connected)
                return;
            MemoryStream ms = new();
            formatter.Serialize(ms, message);
            byte[] buffer = ms.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }
    }
}
