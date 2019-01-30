using System;
using System.Collections.Generic;
using System.Threading;

namespace ColesaBanescu
{
    class ColesaBanescuRng : IDisposable
    {
        private byte _nextByte;

        public byte NextByte => _nextByte;

        protected void ThreadProc(object prm)
        {
            int shift = ((int) prm) & 15;
            var stop = false;
            while (!stop)
            {
                try
                {
                    //unchecked
                    //{
                        //int val = _nextByte + shift;
                        int val = _nextByte;
                        int current = val + 3;
                        while (current >= 0)
                        {
                            val = (val + shift) * 141 + 77;
                            current--;
                        }
                        Thread.Sleep(0);
                        _nextByte = (byte)(val >> 3);
                        //_nextByte = (byte) val;
                        //if ((val & 15) == 1) Thread.Sleep(0);
                    //}
                }
                catch (ThreadAbortException)
                {
                    stop = true;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        protected bool isStarted = false;
        protected List<Thread> threads = new List<Thread>();

        public void Start()
        {
            Start(16);
        }

        public void Start(int threadCount)
        {
            Start(threadCount, 200);
        }

        public void Start(int threadCount, int startDelay)
        {
            if (threadCount < 8) throw new ArgumentException(nameof(threadCount));
            if (startDelay < 100) throw new ArgumentException(nameof(startDelay));
            lock (this)
            {
                if (isStarted) throw new Exception("already started");
                _nextByte = 0;
                threads.Clear();
                for (int i = 0; i < threadCount; i++)
                {
                    var thread = new Thread(ThreadProc) { IsBackground = true };
                    threads.Add(thread);
                    thread.Start(i + 1);
                }
                Thread.Sleep(startDelay);
                isStarted = true;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (!isStarted) return;
                foreach (var thread in threads) thread.Abort();
                threads.Clear();
                isStarted = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
