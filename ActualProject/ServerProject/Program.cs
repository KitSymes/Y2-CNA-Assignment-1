using System;

namespace ServerProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("127.0.0.1", 4444);
            server.Start();
            server.Stop();

            Console.WriteLine("Server Closed");
            Console.ReadLine();
        }
    }
}
