using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1.W04
{
    class Consumer
    {
        private Buffer buffer;
        private Random random = new Random();

        public Consumer(Buffer buffer)
        {
            this.buffer = buffer;
        }

        public void Consumption()
        {
            int data = -1;

            for (int i = 1; i <= 10; i++)
            {
                Thread.Sleep(random.Next(501));
                buffer.Read(ref data);
                data = -1;
            }
        }
    }
}
