using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1.W04
{
    class Bowl
    {
        private int food = 10;
        Random rand = new Random();
        Semaphore forks;

        public Bowl(Semaphore sem)
        {
            forks = sem;
        }

        public void Think()
        {
            Thread.Sleep(rand.Next(501));
        }

        public void Eat(object count)
        {
            while (food > 0)
            {
                if (food > 0)
                {
                    forks.WaitOne();
                    Console.WriteLine("Philospher {0} takes forks.", count);
                    food--;
                    Console.WriteLine("Philospher {0} eats.", count);
                    Console.WriteLine("Philospher {0} returns forks.", count);
                    forks.Release();
                }
                Think();
            }
        }
    }
}
