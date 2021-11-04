using System;
using System.Threading;

public class Program
{

    static void Main(string[] args)
    {
        new Program();
    }

    public Program()
    {
        //Console.WriteLine("");
        W04T03();
    }

    #region W04

    public void W04T03()
    {
        Semaphore sem = new Semaphore(0, 2);
        ConsoleApp1.W04.Bowl spag = new ConsoleApp1.W04.Bowl(sem);
        for (int i = 0; i < 5; i++)
        {
            Thread philosopher = new Thread(new ParameterizedThreadStart(spag.Eat));
            philosopher.Start(i);
        }
        Thread.Sleep(500);
        sem.Release(2);
        Console.ReadLine();
    }

    public void W04T02()
    {
        int num = 0, count = 10;
        Thread[] threads = new Thread[count];
        for (int i = 0; i < count; i++)
            threads[i] = new Thread(new ParameterizedThreadStart((Object pr) =>
            {
                lock (pr)
                {
                    int newNum = num;
                    newNum++;
                    Console.WriteLine(newNum);
                    num = newNum;
                    Monitor.Pulse(pr);
                }
            }));
        for (int i = 0; i < count; i++)
            threads[i].Start(this);
        Console.ReadLine();
    }

    public void W04T01()
    {
        ConsoleApp1.W04.Buffer buff = new ConsoleApp1.W04.Buffer();
        ConsoleApp1.W04.Producer prod = new ConsoleApp1.W04.Producer(buff);
        ConsoleApp1.W04.Consumer con = new ConsoleApp1.W04.Consumer(buff);

        Thread producerThread = new Thread(new ThreadStart(prod.Production));
        Thread consumerThread = new Thread(new ThreadStart(con.Consumption));
        producerThread.Start();
        consumerThread.Start();
    }

    #endregion

    #region W03

    public void W03T03()
    {
        int[] thread1Result = new int[10];
        int[] thread2Result = new int[10];
        int[] thread3Result = new int[10];
        Thread thread1 = new Thread(new ThreadStart(() => W03T03Fibonacci(1, 1, 0, thread1Result)));
        Thread thread2 = new Thread(new ThreadStart(() => W03T03Fibonacci(89, 144, 0, thread2Result)));
        Thread thread3 = new Thread(new ThreadStart(() => W03T03Fibonacci(10946, 17711, 0, thread3Result)));
        thread1.Start();
        thread2.Start();
        thread3.Start();
        if (thread1.IsAlive)
            thread1.Join();
        if (thread2.IsAlive)
            thread2.Join();
        if (thread3.IsAlive)
            thread3.Join();
        for (int i = 0; i < 10; i++)
            Console.Write(thread1Result[i] + ", ");
        for (int i = 0; i < 10; i++)
            Console.Write(thread2Result[i] + ", ");
        for (int i = 0; i < 10; i++)
            Console.Write(thread3Result[i] + ", ");
        Console.WriteLine();
        Console.ReadLine();
    }

    public void W03T03Fibonacci(int first, int second, int pos, int[] result)
    {
        if (pos == 10)
            return;
        result[pos] = first;
        W03T03Fibonacci(second, first + second, pos + 1, result);
    }

    public static void W03Sort(int[] SortNumbers, int count)
    {
        int temp, smallest;
        for (int i = 0; i < 10; i++)
        {
            smallest = i;
            for (int j = i + 1; j < 10; j++)
            {
                if (SortNumbers[j] < SortNumbers[smallest])
                    smallest = j;
            }
            temp = SortNumbers[smallest];
            SortNumbers[smallest] = SortNumbers[i];
            SortNumbers[i] = temp;
        }
        Console.WriteLine("Sorted " + count);
    }

    public static void W03Generate(int[] NewNumbers)
    {
        System.Random random = new System.Random();

        for (int i = 0; i < 10; i++)
            NewNumbers[i] = random.Next(50);
    }

    public static void W03ToScreen(int[] NumbersToPrint)
    {
        for (int i = 0; i < 10; i++)
            Console.Write(NumbersToPrint[i] + " ");
        Console.WriteLine();
    }

    public static void W03Main()
    {
        int[] Numbers1 = new int[10];
        int[] Numbers2 = new int[10];
        int[] Numbers3 = new int[10];
        W03Generate(Numbers1);
        W03Generate(Numbers2);
        W03Generate(Numbers3);
        Console.WriteLine("Initial Lists Are:");
        Console.WriteLine();
        W03ToScreen(Numbers1);
        W03ToScreen(Numbers2);
        W03ToScreen(Numbers3);
        W03Sort(Numbers1, 1);
        W03Sort(Numbers2, 2);
        W03Sort(Numbers3, 3);
        Console.WriteLine("Sorted Lists Are:");
        Console.WriteLine();
        W03ToScreen(Numbers1);
        W03ToScreen(Numbers2);
        W03ToScreen(Numbers3);
    }

    #endregion

    #region W02

    public int W02T04nextToCheck = 2;
    public int W02T04lastPrime = 0;
    public void W02T04()
    {
        W02T04Thread primeThread = new W02T04Thread(this);
        Thread thread1 = new Thread(new ThreadStart(primeThread.Run));
        thread1.Start();
        Thread.Sleep(1000 * 60 * 3);
        primeThread.end = true;
        Console.WriteLine("Largest found after 3 minutes is: " + W02T04lastPrime);
        Console.WriteLine("Trying with 3 threads:");
        W02T04nextToCheck = 2;
        W02T04lastPrime = 0;
        W02T04Thread primeThread2 = new W02T04Thread(this);
        W02T04Thread primeThread3 = new W02T04Thread(this);
        W02T04Thread primeThread4 = new W02T04Thread(this);
        Thread thread2 = new Thread(new ThreadStart(primeThread2.Run));
        Thread thread3 = new Thread(new ThreadStart(primeThread3.Run));
        Thread thread4 = new Thread(new ThreadStart(primeThread4.Run));
        thread2.Start();
        thread3.Start();
        thread4.Start();
        Thread.Sleep(1000 * 60 * 3);
        primeThread2.end = true;
        primeThread3.end = true;
        primeThread4.end = true;
        Console.WriteLine("Largest found after 3 minutes is: " + W02T04lastPrime);
    }

    public int W02T03threadCount = 0;
    public void W02T03()
    {
        Thread thread1 = new Thread(new ThreadStart(new W02T03Thread(0, 0, this).Run));
        thread1.Start();
        Console.ReadLine();
        Console.WriteLine("Total Threads: " + W02T03threadCount);
    }

    #endregion

}
