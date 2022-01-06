using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using Packets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace ServerProject
{
    class Server
    {
        private TcpListener tcpListener;
        private UdpClient udpClient;
        private Bitmap bitmap = new Bitmap(166, 75);

        private ConcurrentDictionary<int, ConnectedClient> connectedClients;
        private ConcurrentDictionary<Guid, ConnectedClient> clientsByID;
        private List<RPSGame> rpsGames = new List<RPSGame>();
        private object rpsGamesLock;

        public Server(string ipAddress, int port)
        {
            rpsGamesLock = new object();

            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    bitmap.SetPixel(x, y, System.Drawing.Color.White);

            IPAddress ip = IPAddress.Parse(ipAddress);
            tcpListener = new TcpListener(ip, port);

            udpClient = new UdpClient(port);
        }

        public void Start()
        {
            tcpListener.Start();
            Thread udp = new Thread(() =>
            {
                UdpListen();
            });
            udp.Start();

            Console.WriteLine("Server is Listening");

            connectedClients = new ConcurrentDictionary<int, ConnectedClient>();
            clientsByID = new ConcurrentDictionary<Guid, ConnectedClient>();
            int clientIndex = 0;

            while (true)
            {
                Socket socket = tcpListener.AcceptSocket();

                Console.WriteLine("Connection Made");

                int index = clientIndex;
                connectedClients.TryAdd(index, new ConnectedClient(socket));
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

            try
            {
                while ((receivedMessage = client.TCPRead()) != null)
                {
                    ProcessPacket(receivedMessage, client);
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Client Left: " + client.nickname);
            }
            catch (IOException)
            {
                Console.WriteLine("IOException for Client: " + client.nickname);
            }

            client.Close();
            ConnectedClient clientOut;
            connectedClients.TryRemove(index, out clientOut);
            clientsByID.TryRemove(client.guid, out clientOut);
            foreach (ConnectedClient remainingClient in clientsByID.Values)
                remainingClient.TCPSend(new ClientLeftPacket(client.guid));
        }

        private void UdpListen()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    byte[] buffer = udpClient.Receive(ref endPoint);
                    MemoryStream ms = new MemoryStream(buffer);
                    Packet packet = formatter.Deserialize(ms) as Packet;
                    foreach (ConnectedClient client in connectedClients.Values)
                    {
                        if (endPoint.ToString() != client.endPoint.ToString())
                            continue;
                        ProcessPacket(packet, client);
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
            }
        }

        private void ProcessPacket(Packet packet, ConnectedClient sender)
        {
            if (!sender.ready)
            {
                LoginPacket loginPacket = (LoginPacket)packet;
                if (clientsByID.ContainsKey(loginPacket.guid))
                    return;

                foreach (ConnectedClient connectedClient in clientsByID.Values)
                    sender.TCPSend(new ClientJoinPacket(connectedClient.guid, connectedClient.nickname));

                clientsByID.TryAdd(loginPacket.guid, sender);
                sender.endPoint = loginPacket.endPoint;
                sender.guid = loginPacket.guid;
                sender.nickname = loginPacket.name;
                sender.SetClientKey(loginPacket.publicKey);
                sender.ready = true;

                int size = bitmap.Width * bitmap.Height;
                byte[] r, g, b;
                r = new byte[size];
                g = new byte[size];
                b = new byte[size];
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        r[x + y * bitmap.Width] = bitmap.GetPixel(x, y).R;
                        g[x + y * bitmap.Width] = bitmap.GetPixel(x, y).G;
                        b[x + y * bitmap.Width] = bitmap.GetPixel(x, y).B;
                    }

                sender.TCPSend(new CanvasSyncPacket(bitmap.Width, bitmap.Height, r, g, b));

                sender.TCPSend(new ServerPublicKeyPacket(sender.GetPublicKey()));

                foreach (ConnectedClient connectedClient in clientsByID.Values)
                    connectedClient.TCPSend(new ClientJoinPacket(sender.guid, sender.nickname));
            }
            else
                switch (packet.packetType)
                {
                    case PacketType.CHAT_MESSAGE:
                        ChatMessagePacket chatPacket = (ChatMessagePacket)packet;
                        foreach (ConnectedClient c in clientsByID.Values)
                            if (c.ready)
                                c.TCPSend(new ChatMessageReceivedPacket(sender.nickname + ": " + chatPacket.message, sender.guid));
                        break;
                    case PacketType.ENCRYPTED_CHAT_MESSAGE:
                        EncryptedChatMessagePacket encryptedChatPacket = (EncryptedChatMessagePacket)packet;
                        string decryptedMessage = sender.DecryptString(encryptedChatPacket.message);
                        foreach (ConnectedClient c in clientsByID.Values)
                            if (c.ready)
                                c.TCPSend(new EncryptedChatMessageReceivedPacket(c.EncryptString(decryptedMessage), c.EncryptString(sender.nickname + ": " + sender.guid.ToString())));
                        break;
                    case PacketType.ENCRYPTED_PRIVATE_MESSAGE:
                        EncryptedPrivateMessagePacket encryptedPrivateMessagePacket = (EncryptedPrivateMessagePacket)packet;
                        string decryptedPrivateMessage = sender.DecryptString(encryptedPrivateMessagePacket.message);
                        string decryptedPrivateID = sender.DecryptString(encryptedPrivateMessagePacket.to);
                        ConnectedClient pmReciever = clientsByID[Guid.Parse(decryptedPrivateID)];
                        if (pmReciever.ready)
                        {
                            if (decryptedPrivateMessage.StartsWith("/"))
                            {
                                String[] command = decryptedPrivateMessage.Split(' ');
                                switch (command[0])
                                {
                                    case "/help":
                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("/help                      - Bring up this message"),
                                            sender.EncryptString(pmReciever.guid.ToString())));
                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("/rps <rock/paper/scissors> - Start/finish a game of Rock Papaer Scissors"),
                                            sender.EncryptString(pmReciever.guid.ToString())));
                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("/shrug                     - Shrug at them"),
                                            sender.EncryptString(pmReciever.guid.ToString())));
                                        break;
                                    case "/rps":
                                        if (command.Length != 2)
                                        {
                                            sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("Invalid command length " + command.Length + ", must be 2."),
                                                sender.EncryptString(pmReciever.guid.ToString())));
                                            break;
                                        }
                                        else
                                            lock (rpsGamesLock)
                                            {
                                                RPSGame game = null;
                                                foreach (RPSGame rps in rpsGames)
                                                    if (rps.Matches(sender.guid, pmReciever.guid))
                                                        game = rps;
                                                if (game == null)
                                                {
                                                    game = new RPSGame(sender.guid, pmReciever.guid);
                                                    game.SetChoice(sender.guid, command[1]);
                                                    rpsGames.Add(game);
                                                    sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You challenged " + pmReciever.nickname + " to a game of Rock, Paper, Scissors!"),
                                                        sender.EncryptString(pmReciever.guid.ToString())));
                                                    pmReciever.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(pmReciever.EncryptString(sender.nickname + " challenged you to a game of Rock, Paper, Scissors!"),
                                                        pmReciever.EncryptString(sender.guid.ToString())));
                                                    if (game.GetChoice(sender.guid) == RPSGame.RPS.NONE)
                                                    {
                                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You made an invalid choice! You are not ready."),
                                                            sender.EncryptString(pmReciever.guid.ToString())));
                                                    } else
                                                    {
                                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You picked " + game.GetChoice(sender.guid).ToString() + "! You are ready."),
                                                            sender.EncryptString(pmReciever.guid.ToString())));
                                                        pmReciever.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(pmReciever.EncryptString(sender.nickname + " is ready."),
                                                            pmReciever.EncryptString(sender.guid.ToString())));
                                                    }
                                                }
                                                else
                                                {
                                                    if (game.GetChoice(sender.guid) == RPSGame.RPS.NONE)
                                                    {
                                                        game.SetChoice(sender.guid, command[1]);
                                                        if (game.GetChoice(sender.guid) == RPSGame.RPS.NONE)
                                                        {
                                                            sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You made an invalid choice! You are not ready."),
                                                                sender.EncryptString(pmReciever.guid.ToString())));
                                                        }
                                                        else
                                                        {
                                                            sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You picked " + game.GetChoice(sender.guid).ToString() + "! You are ready."),
                                                                sender.EncryptString(pmReciever.guid.ToString())));
                                                            pmReciever.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(pmReciever.EncryptString(sender.nickname + " is ready."),
                                                                pmReciever.EncryptString(sender.guid.ToString())));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("You already picked " + game.GetChoice(sender.guid).ToString() + "!"),
                                                            sender.EncryptString(pmReciever.guid.ToString())));
                                                    }

                                                    if (game.IsReady())
                                                    {
                                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("The winner is..."),
                                                            sender.EncryptString(pmReciever.guid.ToString())));
                                                        pmReciever.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(pmReciever.EncryptString("The winner is..."),
                                                            pmReciever.EncryptString(sender.guid.ToString())));
                                                        Guid winner = game.GetWinner();
                                                        string winMsg = "";
                                                        if (winner == Guid.Empty)
                                                            winMsg = "Nobody! It was a tie.";
                                                        else
                                                            winMsg = clientsByID[game.GetWinner()].nickname + "!";
                                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString(winMsg),
                                                            sender.EncryptString(pmReciever.guid.ToString())));
                                                        pmReciever.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(pmReciever.EncryptString(winMsg),
                                                            pmReciever.EncryptString(sender.guid.ToString())));
                                                        rpsGames.Remove(game);
                                                    }
                                                }
                                            }
                                        break;
                                    case "/shrug":
                                        pmReciever.TCPSend(new EncryptedPrivateMessageReceivedPacket(pmReciever.EncryptString(sender.nickname + ": " + "¯\\_(ツ)_/¯"), pmReciever.EncryptString(sender.guid.ToString())));
                                        sender.TCPSend(new EncryptedPrivateMessageReceivedPacket(sender.EncryptString(sender.nickname + ": " + "¯\\_(ツ)_/¯"), sender.EncryptString(pmReciever.guid.ToString())));
                                        break;
                                    default:
                                        sender.TCPSend(new EncryptedPrivateMessageCommandReceivedPacket(sender.EncryptString("Command not found"),
                                            sender.EncryptString(pmReciever.guid.ToString())));
                                        break;
                                }
                            }
                            else
                            {
                                pmReciever.TCPSend(new EncryptedPrivateMessageReceivedPacket(pmReciever.EncryptString(sender.nickname + ": " + decryptedPrivateMessage), pmReciever.EncryptString(sender.guid.ToString())));
                                sender.TCPSend(new EncryptedPrivateMessageReceivedPacket(sender.EncryptString(sender.nickname + ": " + decryptedPrivateMessage), sender.EncryptString(pmReciever.guid.ToString())));
                            }
                        }
                        break;
                    case PacketType.CLIENT_NAME_UPDATE:
                        ClientNameChangePacket nameChangePacket = (ClientNameChangePacket)packet;
                        sender.nickname = nameChangePacket.name;
                        foreach (ConnectedClient cl in clientsByID.Values)
                            cl.TCPSend(new ClientNameChangeReceivedPacket(sender.guid, nameChangePacket.name));
                        break;
                    case PacketType.CANVAS_PAINT:
                        CanvasPaintPacket canvasPaintPacket = (CanvasPaintPacket)packet;
                        Color c1 = bitmap.GetPixel(canvasPaintPacket.x, canvasPaintPacket.y);
                        bitmap.SetPixel(canvasPaintPacket.x, canvasPaintPacket.y, System.Drawing.Color.FromArgb(canvasPaintPacket.r, canvasPaintPacket.g, canvasPaintPacket.b));
                        Color c2 = bitmap.GetPixel(canvasPaintPacket.x, canvasPaintPacket.y);
                        foreach (ConnectedClient c in clientsByID.Values)
                            c.TCPSend(canvasPaintPacket);
                        break;
                    default:
                        break;
                }
        }
    }
}
