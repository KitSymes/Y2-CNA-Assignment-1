using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Packets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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

        private RSACryptoServiceProvider rsaProvider;
        private RSAParameters publicKey;
        private RSAParameters privateKey;
        private RSAParameters serverKey;

        private ConcurrentDictionary<Guid, OtherClient> clients;
        public ChatChannel mainChannel;

        private MainWindow form;

        public Guid guid;
        public String nickname;

        public Client()
        {
            tcpClient = new TcpClient();

            rsaProvider = new RSACryptoServiceProvider(2048);
            publicKey = rsaProvider.ExportParameters(false);
            privateKey = rsaProvider.ExportParameters(true);

            clients = new ConcurrentDictionary<Guid, OtherClient>();
            mainChannel = new ChatChannel();
            guid = Guid.NewGuid();

            form = new MainWindow(this);
            form.ShowDialog();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipAddress);
                tcpClient.Connect(ip, port);
                stream = tcpClient.GetStream();
                writer = new BinaryWriter(stream);
                reader = new BinaryReader(stream);
                formatter = new BinaryFormatter();

                udpClient = new UdpClient();
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
            Thread tcp = new Thread(() =>
            {
                TCPProcessServerResponse();
            });
            tcp.Start();
            Thread udp = new Thread(() =>
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
            TCPSend(new LoginPacket((IPEndPoint)udpClient.Client.LocalEndPoint, guid, nickname, publicKey));
        }

        private void ProcessPacket(Packet packet)
        {
            switch (packet.packetType)
            {
                case PacketType.CLIENT_JOIN:
                    ClientJoinPacket joinPacket = (ClientJoinPacket)packet;
                    OtherClient otherClient = new OtherClient(joinPacket.guid, joinPacket.name, joinPacket.guid == guid);
                    clients.TryAdd(joinPacket.guid, otherClient);
                    form.AddClient(otherClient);
                    mainChannel.Add(joinPacket.name + " has joined.");
                    if (mainChannel.watched)
                        form.UpdateChatBox(mainChannel.Format());
                    break;
                case PacketType.SERVER_PUBLIC_KEY:
                    ServerPublicKeyPacket serverPublicKeyPacket = (ServerPublicKeyPacket)packet;
                    serverKey = serverPublicKeyPacket.publicKey;
                    break;
                case PacketType.CLIENT_NAME_UPDATE_RECEIVED:
                    ClientNameChangeReceivedPacket nameChangePacket = (ClientNameChangeReceivedPacket)packet;
                    if (clients.ContainsKey(nameChangePacket.guid))
                    {
                        mainChannel.Add(clients[nameChangePacket.guid].name + " has changed their name to " + nameChangePacket.name + ".");
                        clients[nameChangePacket.guid].ChangeName(nameChangePacket.name, form);
                        if (mainChannel.watched)
                            form.UpdateChatBox(mainChannel.Format());
                    }
                    break;
                case PacketType.CHAT_MESSAGE_RECEIVED:
                    ChatMessageReceivedPacket messagePacket = (ChatMessageReceivedPacket)packet;
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
                case PacketType.ENCRYPTED_CHAT_MESSAGE_RECEIVED:
                    EncryptedChatMessageReceivedPacket encryptedMessagePacket = (EncryptedChatMessageReceivedPacket)packet;
                    string decryptedMessage = DecryptString(encryptedMessagePacket.message);
                    Guid decryptedGuid = Guid.Parse(DecryptString(encryptedMessagePacket.from));
                    if (clients.ContainsKey(decryptedGuid))
                    {
                        mainChannel.Add(clients[decryptedGuid].name + ": " + decryptedMessage);
                        if (mainChannel.watched)
                            form.UpdateChatBox(mainChannel.Format());
                    }
                    else
                    {
                        mainChannel.Add("???: " + decryptedMessage);
                        if (mainChannel.watched)
                            form.UpdateChatBox(mainChannel.Format());
                    }
                    break;
                default:
                    break;
            }
        }

        #region UDP
        public void UDPSend(Packet packet)
        {
            if (udpClient == null)
                return;
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, packet);
            byte[] buffer = ms.GetBuffer();
            udpClient.Send(buffer, buffer.Length);
        }

        private void UDPProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    byte[] buffer = udpClient.Receive(ref endPoint);
                    MemoryStream ms = new MemoryStream(buffer);
                    Packet packet = formatter.Deserialize(ms) as Packet;
                    ProcessPacket(packet);
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
                MemoryStream ms = new MemoryStream(buffer);
                return formatter.Deserialize(ms) as Packet;
            }
            return null;
        }

        private void TCPProcessServerResponse()
        {
            while (tcpClient.Connected)
            {
                try
                {
                    Packet receivedMessage = TCPRead();
                    ProcessPacket(receivedMessage);
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
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, packet);
            byte[] buffer = ms.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }
        #endregion

        #region Encryption
        private byte[] Encrypt(byte[] data)
        {
            lock (rsaProvider)
            {
                rsaProvider.ImportParameters(serverKey);
                return rsaProvider.Encrypt(data, true);
            }
        }

        private byte[] Decrypt(byte[] data)
        {
            lock (rsaProvider)
            {
                rsaProvider.ImportParameters(privateKey);
                return rsaProvider.Decrypt(data, true);
            }
        }

        public byte[] EncryptString(string message)
        {
            return Encrypt(Encoding.UTF8.GetBytes(message));
        }

        private string DecryptString(byte[] message)
        {
            return Encoding.UTF8.GetString(Decrypt(message));
        }
        #endregion
    }
}
