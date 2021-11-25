using System;

namespace Packets
{
    public enum PacketType
    {
        CLIENT_JOIN, CLIENT_LEAVE, CLIENT_NAME_UPDATE,
        CHAT_MESSAGE, CHAT_MESSAGE_RECEIVED,
        PRIVATE_MESSAGE, PRIVATE_MESSAGE_RECEIVED
    }

    [Serializable]
    public class Packet
    {
        private PacketType _packetType;
        public PacketType PacketType { get { return _packetType; } protected set { _packetType = value; } }
    }


    [Serializable]
    public class ClientJoinPacket : Packet
    {
        public Guid _guid;
        public string _name;
        public ClientJoinPacket(Guid guid, string name)
        {
            PacketType = PacketType.CLIENT_JOIN;
            _guid = guid;
            _name = name;
        }
    }

    #region Chat Message
    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message;
        public ChatMessagePacket(string message)
        {
            PacketType = PacketType.CHAT_MESSAGE;
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
            PacketType = PacketType.CHAT_MESSAGE_RECEIVED;
            this.message = message;
            this.from = from;
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
            PacketType = PacketType.PRIVATE_MESSAGE;
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
            PacketType = PacketType.PRIVATE_MESSAGE_RECEIVED;
            this.message = message;
            this.from = from;
        }
    }
    #endregion
}
