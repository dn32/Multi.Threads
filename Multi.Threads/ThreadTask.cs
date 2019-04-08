using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Multi.Threads
{
    public class ThreadTask
    {
        #region PROPERTIES

        private int TimeOut { get; set; }
        private int SimultaneousThreads { get; set; }
        private int ThreadsComplete { get; set; }
        private int ThreadsCount { get; set; }
        private int ThreadsPerMinute { get; set; }
        private Func<object, object> Action { get; set; }
        private Exception Ex { get; set; }
        private int SimultaneousThreadsNow { get; set; }
        private int MinuteNow { get; set; }
        private int ThreadsPerMinuteNow { get; set; }
        private object LockSimultaneousThreadsNow { get; set; }
        private object LockThreadsPerMinuteNow { get; set; }

        #endregion

        #region PUBLIC

        public ThreadTask()
        {
            LockSimultaneousThreadsNow = new object();
            LockThreadsPerMinuteNow = new object();
        }

        public ThreadTask RunAsync(List<Task> parameters)
        {
            Execute(parameters);
            return this;
        }

        public ThreadTask Run(List<Task> parameters)
        {
            Execute(parameters);
            WhaitAllThread();
            return this;
        }

        public ThreadTask Wait()
        {
            WhaitAllThread();
            return this;
        }

        #endregion

        #region FLUENT

        public static ThreadTask Instance()
        {
            return new ThreadTask();
        }

        /// <summary>
        /// Time-out in seconds.
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public ThreadTask SetTimeOut(int timeOut)
        {
            this.TimeOut = timeOut;
            return this;
        }

        public ThreadTask SetAction(Func<object, object> action)
        {
            this.Action = action;
            return this;
        }

        public ThreadTask SetSimultaneousThreads(int simultaneousThreads)
        {
            this.SimultaneousThreads = simultaneousThreads;
            return this;
        }

        public ThreadTask SetThreadsPerMinute(int threadsPerMinute)
        {
            this.ThreadsPerMinute = threadsPerMinute;
            return this;
        }

        #endregion

        #region PRIVATE
        private void Execute(List<Task> parameters)
        {
            if (ThreadsCount != 0)
            {
                throw new Exception("This operation has already run!");
            }

            ThreadsCount = parameters.Count;
            foreach (var parameter in parameters)
            {
                WhaitThread();
                AddThread();

                new Thread((object param) =>
                {
                    ExecuteThread((Task)param);
                    RemoveThread();
                }
              ).Start(parameter);
            }
        }

        private void WhaitThread()
        {
            while (SimultaneousThreads > 0 && SimultaneousThreadsNow >= SimultaneousThreads)
            {
                Thread.Sleep(10);
            }

            while (this.ThreadsPerMinute > 0 && ThreadsPerMinuteNow >= this.ThreadsPerMinute && MinuteNow == DateTime.Now.Minute)
            {
                Thread.Sleep(10);
            }
        }

        private void WhaitAllThread()
        {
            while (ThreadsComplete != ThreadsCount)
            {
                Thread.Sleep(10);
            }

            ThreadsCount = 0;
        }

        private void AddThread()
        {
            if (SimultaneousThreads > 0)
            {
                lock (LockSimultaneousThreadsNow)
                {
                    SimultaneousThreadsNow++;
                }
            }

            if (this.ThreadsPerMinute > 0)
            {
                if (MinuteNow != DateTime.Now.Minute)
                {
                    MinuteNow = DateTime.Now.Minute;
                    ThreadsPerMinuteNow = 1;
                }
                else
                {
                    lock (this.LockThreadsPerMinuteNow)
                    {
                        ThreadsPerMinuteNow++;
                    }
                }
            }

        }

        private void RemoveThread()
        {
            if (SimultaneousThreads > 0)
            {
                lock (LockSimultaneousThreadsNow)
                {
                    SimultaneousThreadsNow--;
                    ThreadsComplete++;
                }
            }
        }

        private void ExecuteThread(Task param)
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    param.Return = Action(param.Parameter);
                }
                catch (Exception ex)
                {
                    param.Exception = ex;
                }
            });

            if (!task.Wait(TimeSpan.FromSeconds(this.TimeOut)))
            {
                param.Exception = new TimeoutException("Thread terminated after " + this.TimeOut + " second runtime");
            }
        }

        #endregion
    }
}
