using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;

namespace ColesaBanescu
{
    // GammaFunctions

    class Program
    {
        static void Main(string[] args)
        {
            List<int> data = new List<int>();

            var rnd = new ColesaBanescuRng();
            rnd.Start(32);

            for (int i = 0; i < 1000; i++)
            {
                data.Add(rnd.NextByte);
                Thread.Sleep(0);
            }

            for (int i = 0; i < data.Count; i++)
            {
                if (i > 0 && ((i & 15) == 0)) Console.WriteLine();
                Console.Write("{0:D3} ", data[i]);
            }

            Console.WriteLine();

            var bitString = Nist.ToBitString(data, 8);
            Nist.Test(bitString);

            rnd.Stop();

            Console.WriteLine("press enter to exit...");
            Console.ReadLine();
        }
    }
}
