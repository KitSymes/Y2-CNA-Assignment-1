using System;
using System.Net;
using System.Security.Cryptography;

namespace Packets
{
    public enum PacketType
    {
        LOGIN, SERVER_PUBLIC_KEY,
        CLIENT_JOIN, CLIENT_LEAVE,
        CLIENT_NAME_UPDATE, CLIENT_NAME_UPDATE_RECEIVED,
        CHAT_MESSAGE, CHAT_MESSAGE_RECEIVED,
        ENCRYPTED_CHAT_MESSAGE, ENCRYPTED_CHAT_MESSAGE_RECEIVED,
        ENCRYPTED_PRIVATE_MESSAGE, ENCRYPTED_PRIVATE_MESSAGE_RECEIVED, ENCRYPTED_PRIVATE_MESSAGE_COMMAND_RECEIVED,
        CANVAS_SYNC, CANVAS_PAINT
    }

    [Serializable]
    public class Packet
    {
        private PacketType _packetType;
        public PacketType packetType { get { return _packetType; } protected set { _packetType = value; } }
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public IPEndPoint endPoint;
        public Guid guid;
        public string name;
        public RSAParameters publicKey;
        public LoginPacket(IPEndPoint endPoint, Guid guid, string name, RSAParameters publicKey)
        {
            packetType = PacketType.LOGIN;
            this.endPoint = endPoint;
            this.guid = guid;
            this.name = name;
            this.publicKey = publicKey;
        }
    }

    [Serializable]
    public class ServerPublicKeyPacket : Packet
    {
        public RSAParameters publicKey;
        public ServerPublicKeyPacket(RSAParameters publicKey)
        {
            packetType = PacketType.SERVER_PUBLIC_KEY;
            this.publicKey = publicKey;
        }
    }

    #region Join/Leave
    [Serializable]
    public class ClientJoinPacket : Packet
    {
        public Guid guid;
        public string nickname;
        public ClientJoinPacket(Guid guid, string name)
        {
            packetType = PacketType.CLIENT_JOIN;
            this.guid = guid;
            this.nickname = name;
        }
    }

    [Serializable]
    public class ClientLeftPacket : Packet
    {
        public Guid guid;
        public ClientLeftPacket(Guid guid)
        {
            packetType = PacketType.CLIENT_LEAVE;
            this.guid = guid;
        }
    }
    #endregion

    #region Client Name Change
    [Serializable]
    public class ClientNameChangePacket : Packet
    {
        public string name;
        public ClientNameChangePacket(string name)
        {
            packetType = PacketType.CLIENT_NAME_UPDATE;
            this.name = name;
        }
    }

    [Serializable]
    public class ClientNameChangeReceivedPacket : Packet
    {
        public Guid guid;
        public string nickname;
        public ClientNameChangeReceivedPacket(Guid guid, string name)
        {
            packetType = PacketType.CLIENT_NAME_UPDATE_RECEIVED;
            this.guid = guid;
            this.nickname = name;
        }
    }
    #endregion

    #region Chat Message
    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message;
        public ChatMessagePacket(string message)
        {
            packetType = PacketType.CHAT_MESSAGE;
            this.message = message;
        }
    }

    [Serializable]
    public class ChatMessageReceivedPacket : Packet
    {
        public string message;
        public Guid from;
        public ChatMessageReceivedPacket(string message, Guid from)
        {
            packetType = PacketType.CHAT_MESSAGE_RECEIVED;
            this.from = from;
            this.message = message;
        }
    }
    #endregion

    #region Encrypted Chat Message
    [Serializable]
    public class EncryptedChatMessagePacket : Packet
    {
        public byte[] message;
        public EncryptedChatMessagePacket(byte[] message)
        {
            packetType = PacketType.ENCRYPTED_CHAT_MESSAGE;
            this.message = message;
        }
    }

    [Serializable]
    public class EncryptedChatMessageReceivedPacket : Packet
    {
        public byte[] message;
        public byte[] from;
        public EncryptedChatMessageReceivedPacket(byte[] message, byte[] from)
        {
            packetType = PacketType.ENCRYPTED_CHAT_MESSAGE_RECEIVED;
            this.from = from;
            this.message = message;
        }
    }
    #endregion

    #region PrivateMessage
    [Serializable]
    public class EncryptedPrivateMessagePacket : Packet
    {
        public byte[] message;
        public byte[] to;
        public EncryptedPrivateMessagePacket(byte[] message, byte[] to)
        {
            packetType = PacketType.ENCRYPTED_PRIVATE_MESSAGE;
            this.message = message;
            this.to = to;
        }
    }

    [Serializable]
    public class EncryptedPrivateMessageReceivedPacket : Packet
    {
        public byte[] message;
        public byte[] from;
        public EncryptedPrivateMessageReceivedPacket(byte[] message, byte[] from)
        {
            packetType = PacketType.ENCRYPTED_PRIVATE_MESSAGE_RECEIVED;
            this.message = message;
            this.from = from;
        }
    }

    [Serializable]
    public class EncryptedPrivateMessageCommandReceivedPacket : Packet
    {
        public byte[] message;
        public byte[] channel;
        public EncryptedPrivateMessageCommandReceivedPacket(byte[] message, byte[] channel)
        {
            packetType = PacketType.ENCRYPTED_PRIVATE_MESSAGE_COMMAND_RECEIVED;
            this.message = message;
            this.channel = channel;
        }
    }
    #endregion

    #region Canvas
    [Serializable]
    public class CanvasSyncPacket : Packet
    {
        public int width;
        public int height;
        public byte[] r;
        public byte[] g;
        public byte[] b;
        public CanvasSyncPacket(int width, int height, byte[] r, byte[] g, byte[] b)
        {
            packetType = PacketType.CANVAS_SYNC;
            this.width = width;
            this.height = height;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    [Serializable]
    public class CanvasPaintPacket : Packet
    {
        public int x;
        public int y;
        public byte r;
        public byte g;
        public byte b;
        public CanvasPaintPacket(int x, int y, byte r, byte g, byte b)
        {
            packetType = PacketType.CANVAS_PAINT;
            this.x = x;
            this.y = y;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
    #endregion
}
