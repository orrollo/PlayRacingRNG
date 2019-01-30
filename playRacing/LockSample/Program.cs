using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;

namespace LockSample
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> data = new List<int>();
            List<Thread> threads = new List<Thread>();

            int currentFlag = 0;

            var mtxName = Guid.NewGuid().ToString();
            using (Mutex mtx = new Mutex(true, mtxName))
            {
                //lock (data)
                //{
                for (int i = 0; i < 16; i++)
                {
                    var thread = new Thread((prm) =>
                    {
                        var mutex = mtx;
                        int number = (int)prm, flag = number & 1;
                        var stop = false;
                        while (!stop)
                        {
                            try
                            {
                                //lock (data)
                                //{
                                if (mutex.WaitOne(0))
                                {
                                    try
                                    {
                                        if (data.Count < 8000)
                                        {
                                            data.Add(currentFlag == flag ? 1 : 0);
                                            currentFlag = flag;
                                        }
                                        else
                                            stop = true;
                                    }
                                    finally
                                    {
                                        mutex.ReleaseMutex();
                                    }
                                }
                                //else Thread.Sleep(0);
                                //}
                                //if (!stop) Thread.Sleep(0);
                            }
                            catch (Exception ex)
                            {
                                stop = ex is ThreadAbortException;
                            }
                        }

                        lock (threads)
                        {
                            threads.Remove(Thread.CurrentThread);
                        }
                    });
                    //thread.IsBackground = true;
                    threads.Add(thread);
                    thread.Start(i);
                }

                mtx.ReleaseMutex();
                var dostop = false;
                while (!dostop)
                {
                    lock (threads)
                    {
                        dostop = threads.Count == 0;
                    }
                    if (!dostop) Thread.Sleep(50);
                }
            }

            var bitString = Nist.ToBitString(data, 1);
            Nist.Test(bitString);

            Console.WriteLine("press enter to exit...");
            Console.ReadLine();
        }
    }
}
