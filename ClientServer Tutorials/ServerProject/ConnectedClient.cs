using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace ServerProject
{
    class ConnectedClient
    {
        private Socket socket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Object readLock;
        private Object writeLock;

        public ConnectedClient(Socket socket)
        {
            readLock = new object();
            writeLock = new object();
            
            this.socket = socket;

            stream = new(socket);
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

        public string Read()
        {
            lock (readLock)
            {
                return reader.ReadLine();
            }
        }

        public void Send(string message)
        {
            lock (writeLock)
            {
                writer.WriteLine(message);
                writer.Flush();
            }
        }
    }
}
