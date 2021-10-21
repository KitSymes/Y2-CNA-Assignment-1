using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

    public class W02T03Thread
    {
        int min, depth;
        Program program;

        public W02T03Thread(int min, int depth, Program program)
        {
            this.min = min;
            this.depth = depth;
            this.program = program;
        }

        public void Run()
        {
            Console.WriteLine("New Thread Running");
            program.W02T03threadCount++;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine((i + min == 29 ? "# " : "") + (i + min));
                //Thread.Sleep(1);
            }

            if (depth == 2)
                return;

            for (int i = 0; i < 2; i++)
            {
                Thread thread = new Thread(new ThreadStart(new W02T03Thread(min + 10, depth + 1, program).Run));
                thread.Start();
                //thread.Join();
            }
        }
    }
