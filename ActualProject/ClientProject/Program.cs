using System;

namespace ClientProject
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Client client = new Client();

            client.Close();

            Console.WriteLine("Client Closed");
            Console.ReadLine();
        }
    }
}
