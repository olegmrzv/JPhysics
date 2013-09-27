namespace JPhysics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public class ThreadManager
    {
        public const int ThreadsPerProcessor = 1;

        private ManualResetEvent waitHandleA, waitHandleB;
        private ManualResetEvent currentWaitHandle;

        internal volatile List<Action<object>> tasks = new List<Action<object>>();
        internal volatile List<object> parameters = new List<object>();

        private Thread[] threads;
        private int currentTaskIndex, waitingThreadCount;

        public int ThreadCount { get; internal set; }

        static ThreadManager instance;

        public static ThreadManager Instance 
        { 
            get 
            {
                if (instance == null)
                {
                    instance = new ThreadManager();
                    instance.Initialize();
                }

                return instance;
            }
        }

        private void Initialize()
        {
            ThreadCount = Environment.ProcessorCount * ThreadsPerProcessor;

            threads = new Thread[ThreadCount];
            waitHandleA = new ManualResetEvent(false);
            waitHandleB = new ManualResetEvent(false);

            currentWaitHandle = waitHandleA;

            var initWaitHandle = new AutoResetEvent(false);

            for (var i = 1; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {

                    initWaitHandle.Set();
                    ThreadProc();
                }) {IsBackground = true};

                threads[i].Start();
                initWaitHandle.WaitOne();
            }



        }


        public void Execute()
        {
            currentTaskIndex = 0;
            waitingThreadCount = 0;

            currentWaitHandle.Set();

            while (waitingThreadCount < threads.Length - 1) Thread.Sleep(0);

            currentWaitHandle.Reset();
            currentWaitHandle = (currentWaitHandle == waitHandleA) ? waitHandleB : waitHandleA;

            tasks.Clear();
            parameters.Clear();
        }

        public void AddTask(Action<object> task, object param)
        {
            tasks.Add(task);
            parameters.Add(param);
        }

        private void ThreadProc()
        {
            while (true)
            {

                Interlocked.Increment(ref waitingThreadCount);
                waitHandleA.WaitOne();
                PumpTasks();


                Interlocked.Increment(ref waitingThreadCount);
                waitHandleB.WaitOne();
                PumpTasks();
            }
        }

        private void PumpTasks()
        {
            var count = tasks.Count;

            while (currentTaskIndex < count)
            {

                int taskIndex = currentTaskIndex;

                if (taskIndex == Interlocked.CompareExchange(ref currentTaskIndex, taskIndex + 1, taskIndex)
                    && taskIndex < count)
                {
                    tasks[taskIndex](parameters[taskIndex]);
                }

            }


        }

    }

}
