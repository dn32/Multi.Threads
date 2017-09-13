﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Multi.Threads
{
    public class ThreadTask
    {
        private int TimeOut { get; set; }
        private int SimultaneousThreads { get; set; }
        private int ThreadsComplete { get; set; }
        private int ThreadsCount { get; set; }
        private int ThreadsPerMinute { get; set; }
        private Func<Task, object> Action { get; set; }
        private Exception Ex { get; set; }
        private int SimultaneousThreadsNow { get; set; }
        private int MinuteNow { get; set; }
        private int ThreadsPerMinuteNow { get; set; }
        private object LockSimultaneousThreadsNow { get; set; }
        private object LockThreadsPerMinuteNow { get; set; }

        public void Run(List<Task> parameters)
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

            WhaitAllThread();
        }

        public ThreadTask()
        {
            LockSimultaneousThreadsNow = new object();
            LockThreadsPerMinuteNow = new object();
        }

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

        public ThreadTask SetAction(Func<Task, object> action)
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
            var t1 = new Thread((object paramt1) =>
            {
                try
                {
                    ((Task)paramt1).Return = Action((Task)paramt1);
                }
                catch (ThreadAbortException)
                {
                    //Ignored
                }
                catch (Exception ex)
                {
                    param.Exception = ex;
                }
            });

            t1.Start(param);

            if (this.TimeOut == 0)
            {
                t1.Join();
            }
            else
            {
                if (!t1.Join(this.TimeOut * 1000))
                {
                    try { t1.Abort(); } catch (Exception ex) { /* ignored */ }
                    param.Exception = new TimeoutException("Thread terminated after " + this.TimeOut + " second runtime");
                }
            }
        }

        #endregion
    }
}