using System;
using System.Net;

namespace Packets
{
    public enum PacketType
    {
        LOGIN, CLIENT_JOIN, CLIENT_LEAVE,
        CLIENT_NAME_UPDATE, CLIENT_NAME_UPDATE_RECEIVED,
        CHAT_MESSAGE, CHAT_MESSAGE_RECEIVED,
        PRIVATE_MESSAGE, PRIVATE_MESSAGE_RECEIVED
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
        public EndPoint endPoint;
        public Guid guid;
        public string name;
        public LoginPacket(EndPoint endPoint, Guid guid, string name)
        {
            this.endPoint = endPoint;
            packetType = PacketType.LOGIN;
            this.guid = guid;
            this.name = name;
        }
    }

    [Serializable]
    public class ClientJoinPacket : Packet
    {
        public Guid guid;
        public string name;
        public ClientJoinPacket(Guid guid, string name)
        {
            packetType = PacketType.CLIENT_JOIN;
            this.guid = guid;
            this.name = name;
        }
    }

    [Serializable]
    public class ClientLeftPacket : Packet
    {
        public Guid guid;
        public ClientLeftPacket(Guid guid)
        {
            packetType = PacketType.CLIENT_JOIN;
            this.guid = guid;
        }
    }

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
        public string name;
        public ClientNameChangeReceivedPacket(Guid guid, string name)
        {
            packetType = PacketType.CLIENT_NAME_UPDATE_RECEIVED;
            this.guid = guid;
            this.name = name;
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

    #region PrivateMessage
    [Serializable]
    public class PrivateMessagePacket : Packet
    {
        public string message;
        public Guid to;
        public PrivateMessagePacket(string message, Guid to)
        {
            packetType = PacketType.PRIVATE_MESSAGE;
            this.message = message;
            this.to = to;
        }
    }

    [Serializable]
    public class PrivateMessageReceivedPacket : Packet
    {
        public string message;
        public Guid from;
        public PrivateMessageReceivedPacket(string message, Guid from)
        {
            packetType = PacketType.PRIVATE_MESSAGE_RECEIVED;
            this.message = message;
            this.from = from;
        }
    }
    #endregion
}
