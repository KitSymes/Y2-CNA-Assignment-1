using System;
using System.Collections.Generic;
using System.Threading;

class W02T04Thread
{
    public bool end = false;
    private Program program;

    public W02T04Thread(Program program)
    {
        this.program = program;
    }

    public void Run()
    {
        while (!end)
        {
            int check = program.W02T04nextToCheck;
            //Console.WriteLine("Checking " + check);
            program.W02T04nextToCheck++;
            double root = Math.Sqrt(check);
            if (root % 1 == 0) // int, so whole root
                continue;
            else
            {
                bool prime = true;
                for (int i = 2; i < Math.Floor(root); i++)
                {
                    if (check % i == 0) // i is factor, so not prime
                    {
                        prime = false;
                        break;
                    }
                }

                if (prime && program.W02T04lastPrime < check)
                    //Console.WriteLine("Prime found: " + check);
                    program.W02T04lastPrime = check;
            }
        }
    }
}
