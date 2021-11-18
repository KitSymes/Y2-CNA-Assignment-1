using System;

namespace ClientProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new();

            if (client.Connect("127.0.0.1", 4444))
                client.RPS();
            else
                Console.WriteLine("Failed to connect to the server");

            Console.WriteLine("Client Closed");
            Console.ReadLine();
        }
    }
}
