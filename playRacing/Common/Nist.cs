//
// code from article:
//
// Test Run - Implementing the National Institute of Standards and Technology Tests of Randomness Using C#
//
// By James McCaffrey | Government 2013
//
// https://msdn.microsoft.com/en-us/magazine/dn520240.aspx
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class Nist
    {
        public static void Test(string bitString)
        {
            try
            {
                BitArray bitArray = MakeBitArray(bitString);

                Console.WriteLine("\nBegin NIST tests of randomness using C# demo\n");
                Console.WriteLine("Input sequence to test for randomness: \n");
                ShowBitArray(bitArray, 4, 52);

                Console.WriteLine("\n\n1. Testing input frequencies");
                double pFreq = FrequencyTest(bitArray);
                Console.WriteLine("pValue for Frequency test = " + pFreq.ToString("F4"));
                if (pFreq < 0.01)
                    Console.WriteLine("There is evidence that sequence is NOT random");
                else
                    Console.WriteLine("Sequence passes NIST frequency test for randomness");

                int blockLength = 8;
                Console.WriteLine("\n\n2. Testing input blocks (block length = " + blockLength + ")");
                double pBlock = BlockTest(bitArray, blockLength);
                Console.WriteLine("pValue for Block test = " + pBlock.ToString("F4"));
                if (pBlock < 0.01)
                    Console.WriteLine("There is evidence that sequence is NOT random");
                else
                    Console.WriteLine("Sequence passes NIST block test for randomness");

                Console.WriteLine("\n\n3. Testing input runs");
                double pRuns = RunsTest(bitArray);
                Console.WriteLine("pValue for Runs test = " + pRuns.ToString("F4"));
                if (pRuns < 0.01)
                    Console.WriteLine("There is evidence that sequence is NOT random");
                else
                    Console.WriteLine("Sequence passes NIST runs test for randomness");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
// Main

        // ------------------------------------------------------------------

        static BitArray MakeBitArray(string bitString)
        {
            // ex: string "010 101" -> a BitArray of [false,true,false,true,false,true]
            int size = 0;
            for (int i = 0; i < bitString.Length; ++i)
                if (bitString[i] != ' ') ++size;

            BitArray result = new BitArray(size); // default is false
            int k = 0; // ptr into result
            for (int i = 0; i < bitString.Length; ++i)
            {
                if (bitString[i] == ' ') continue;
                if (bitString[i] == '1')
                    result[k] = true;
                else
                    result[k] = false; // not necessary in C#
                ++k;
            }
            return result;
        }

        static void ShowBitArray(BitArray bitArray, int blockSize, int lineSize)
        {
            for (int i = 0; i < bitArray.Length; ++i)
            {
                if (i > 0 && i % blockSize == 0)
                    Console.Write(" ");

                if (i > 0 && i % lineSize == 0)
                    Console.WriteLine("");

                if (bitArray[i] == false) Console.Write("0");
                else Console.Write("1");
            }
            Console.WriteLine("");
        }

        // ------------------------------------------------------------------


        static double FrequencyTest(BitArray bitArray)
        {
            // perform a NIST frequency test on bitArray
            double sum = 0;
            for (int i = 0; i < bitArray.Length; ++i)
            {
                if (bitArray[i] == false)
                    sum = sum - 1;
                else
                    sum = sum + 1;
            }
            double testStat = Math.Abs(sum) / Math.Sqrt(bitArray.Length);
            double rootTwo = 1.414213562373095;
            double pValue = ErrorFunctionComplement(testStat / rootTwo);
            return pValue;
        }

        static double ErrorFunction(double x)
        {
            // assume x > 0.0
            // Abramowitz and Stegun eq. 7.1.26
            double p = 0.3275911;
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double t = 1.0 / (1.0 + p * x);
            double err = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
            return err;
        }

        static double ErrorFunctionComplement(double x)
        {
            return 1 - ErrorFunction(x);
        }

        // ------------------------------------------------------------------

        static double BlockTest(BitArray bitArray, int blockLength)
        {
            // NIST intra-block frequency test
            int numBlocks = bitArray.Length / blockLength; // 'N'

            double[] proportions = new double[numBlocks];
            int k = 0; // ptr into bitArray
            for (int block = 0; block < numBlocks; ++block)
            {
                int countOnes = 0;
                for (int i = 0; i < blockLength; ++i)
                {
                    if (bitArray[k++] == true)
                        ++countOnes;
                }

                proportions[block] = (countOnes * 1.0) / blockLength;
            }

            double summ = 0.0;
            for (int block = 0; block < numBlocks; ++block)
                summ = summ + (proportions[block] - 0.5) * (proportions[block] - 0.5);
            double chiSquared = 4 * blockLength * summ;

            double a = numBlocks / 2.0;
            double x = chiSquared / 2.0;
            double pValue = GammaFunctions.GammaUpper(a, x);
            return pValue;
        }

        static double RunsTest(BitArray bitArray)
        {
            // NIST Runs test
            double numOnes = 0.0;
            for (int i = 0; i < bitArray.Length; ++i)
                if (bitArray[i] == true)
                    ++numOnes;

            double prop = (numOnes * 1.0) / bitArray.Length;

            //double tau = 2.0 / Math.Sqrt(bitArray.Length * 1.0);
            //if (Math.Abs(prop - 0.5) >= tau)
            //  return 0.0; // not-random short-circuit

            int runs = 1;
            for (int i = 0; i < bitArray.Length - 1; ++i)
                if (bitArray[i] != bitArray[i + 1])
                    ++runs;

            double num = Math.Abs(runs - (2 * bitArray.Length * prop * (1 - prop)));
            double denom = 2 * Math.Sqrt(2.0 * bitArray.Length) * prop * (1 - prop);
            double pValue = ErrorFunctionComplement(num / denom);
            return pValue;
        }


        // ------------------------------------------------------------------
        public static string ToBitString(List<int> data, int bitSize)
        {
            var sb = new StringBuilder();
            var mod = (1 << bitSize) - 1;
            foreach (var current in data)
            {
                int value = current & mod;
                sb.Append(Convert.ToString(value, 2).PadLeft(bitSize, '0'));
            }
            var bitString = sb.ToString();
            return bitString;
        }
    } // Program
}
