namespace JPhysics
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //Some troubles with multithreading

    public class ThreadManager
    {
        public static int ThreadsPerProcessor = 1;

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

        static ThreadManager instance;

        public int ThreadCount { get; internal set; }

        ManualResetEvent waitHandleA, waitHandleB, currentWaitHandle;
        AutoResetEvent[] waitHandles;

        volatile List<Action<object>> tasks = new List<Action<object>>();
        volatile List<object> parameters = new List<object>();

        Thread[] threads;
        int currentTaskIndex;

        void Initialize()
        {
            ThreadCount = Environment.ProcessorCount * ThreadsPerProcessor;

            threads = new Thread[ThreadCount];
            waitHandles = new AutoResetEvent[ThreadCount];
            waitHandleA = new ManualResetEvent(false);
            waitHandleB = new ManualResetEvent(false);

            for (var i = 0; i < waitHandles.Length; i++)
            {
                waitHandles[i] = new AutoResetEvent(false);
            }

            currentWaitHandle = waitHandleA;

            var initWaitHandle = new AutoResetEvent(false);

            for (var i = 0; i < threads.Length; i++)
            {
                int i1 = i;
                threads[i] = new Thread(() =>
                {
                    initWaitHandle.Set();
                    ThreadProc(waitHandles[i1]);
                }) {IsBackground = true};

                threads[i].Start();
                initWaitHandle.WaitOne();
            }
        }


        public void Execute()
        {
            currentTaskIndex = 0;

            currentWaitHandle.Set();

            WaitHandle.WaitAll(waitHandles);

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

        void ThreadProc(EventWaitHandle wait)
        {
            while (true)
            {
                waitHandleA.WaitOne();
                PumpTasks();
                wait.Set();

                waitHandleB.WaitOne();
                PumpTasks();
                wait.Set();
            }
        }

        void PumpTasks()
        {
            var count = tasks.Count;

            while (currentTaskIndex < count)
            {
                var taskIndex = currentTaskIndex;

                if (taskIndex == Interlocked.CompareExchange(ref currentTaskIndex, taskIndex + 1, taskIndex)
                    && taskIndex < count) tasks[taskIndex](parameters[taskIndex]);

            }

        }

    }

}
