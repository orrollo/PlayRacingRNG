using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace StopwatchSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var qu = new ConcurrentQueue<long>();

            long sum = 0;
            for (int n = 0; n < 100; n++)
            {
                var started = Stopwatch.GetTimestamp();
                var period = Stopwatch.GetTimestamp() - started;
                sum += period;
            }
            sum /= 100;
            Console.WriteLine(sum);
            Console.WriteLine();

            ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount*4 };
            Parallel.For(0, opt.MaxDegreeOfParallelism*5, opt, n =>
            {
                var lst = new Queue<long>();
                for (int i = 0; i < 10; i++)
                {
                    var started = Stopwatch.GetTimestamp();
                    Thread.Sleep(10);
                    var period = Stopwatch.GetTimestamp() - started - sum;
                    lst.Enqueue(period & 15);
                }
                foreach (var v in lst) qu.Enqueue(v);
            });

            var data = qu.Select(v=>(int)v).ToList();
            var bitString = Nist.ToBitString(data, 4);
            Nist.Test(bitString);

            Console.ReadLine();
        }
    }
}
