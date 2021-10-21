﻿using System;
using System.Threading;

public class Program
{

    static void Main(string[] args)
    {
        //new Program();
        W03Main();
    }

    public Program()
    {
        //Console.WriteLine("");
        W02T04();
    }

    #region W03

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
                temp = SortNumbers[smallest];
                SortNumbers[smallest] = SortNumbers[i];
                SortNumbers[i] = temp;
            }
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
        Thread Thread1 = new Thread(new ThreadStart(() => W03Sort(Numbers1, 1)));
        Thread Thread2 = new Thread(new ThreadStart(() => W03Sort(Numbers2, 2)));
        Thread Thread3 = new Thread(new ThreadStart(() => W03Sort(Numbers3, 3)));
        Thread1.Start();
        Thread2.Start();
        Thread3.Start();
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