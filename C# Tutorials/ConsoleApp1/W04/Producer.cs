using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1.W04
{
    class Producer
    {
        private Buffer buffer;
        private Random random = new Random();

        public Producer(Buffer buffer)
        {
            this.buffer = buffer;
        }

        public void Production()
        {
            for (int i = 1; i <= 10; i++)
            {
                Thread.Sleep(random.Next(501));
                buffer.Write(i);
            }
        }
    }
}
