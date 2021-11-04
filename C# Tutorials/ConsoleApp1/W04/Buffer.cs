using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1.W04
{
    class Buffer
    {
        private int data;
        private bool empty = true;

        public void Read(ref int data)
        {
            lock(this)
            {
                if (empty)
                    Monitor.Wait(this);
                empty = true;
                data = this.data;
                Console.WriteLine("Data Read: " + data);
                Monitor.Pulse(this);
            }
        }

        public void Write(int data)
        {
            lock(this)
            {
                if (!empty)
                    Monitor.Wait(this);
                empty = false;
                this.data = data;
                Console.WriteLine("Data Written: " + data);
                Monitor.Pulse(this);
            }
        }
    }
}
