﻿using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Packets;

namespace ServerProject
{
    class ConnectedClient
    {
        private Socket socket;
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private BinaryFormatter formatter;
        private Object readLock;
        private Object writeLock;

        public bool ready = false;
        public Guid guid;
        public string nickname;

        public ConnectedClient(Socket socket)
        {
            readLock = new object();
            writeLock = new object();
            
            this.socket = socket;

            stream = new(socket);
            formatter = new();
            reader = new(stream, Encoding.UTF8);
            writer = new(stream, Encoding.UTF8);
        }

        public void Close()
        {
            writer.Close();
            reader.Close();
            stream.Close();
            socket.Close();
        }

        public Packet Read()
        {
            lock (readLock)
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
        }

        public void Send(Packet message)
        {
            lock (writeLock)
            {
                MemoryStream ms = new();
                formatter.Serialize(ms, message);
                byte[] buffer = ms.GetBuffer();
                writer.Write(buffer.Length);
                writer.Write(buffer);
                writer.Flush();
            }
        }
    }
}