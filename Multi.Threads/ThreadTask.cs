using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Multi.Threads
{
    public class ThreadTask
    {
        #region PROPERTIES

        /// <summary>
        /// Time in second.
        /// </summary>
        private int TimeOut { get; set; }
        private int SimultaneousThreads { get; set; }
        private int ThreadsComplete { get; set; }
        private int ThreadsCount { get; set; }
        private int ThreadsPerMinute { get; set; }
        private Func<object, object> Action { get; set; }
        private int SimultaneousThreadsNow { get; set; }
        private int MinuteNow { get; set; }
        private int ThreadsPerMinuteNow { get; set; }
        private object LockSimultaneousThreadsNow { get; set; }
        private object LockThreadsPerMinuteNow { get; set; }
        public bool Running { get; set; }
        private List<Task> Queue { get; set; }
        private List<Task> ItemsPerformed { get; set; }
        private int TimetInSecondsToDenyQueueRequest { get; set; }
        public MultiThreadsInfo Info => GetInfo();

        #endregion

        #region PUBLIC

        public ThreadTask()
        {
            LockSimultaneousThreadsNow = new object();
            LockThreadsPerMinuteNow = new object();
            Queue = new List<Task>();
            ItemsPerformed = new List<Task>();
        }

        public ThreadTask RunAsync(List<Task> parameters)
        {
            if (ThreadsCount != 0)
            {
                throw new Exception("This operation has already run!");
            }

            Execute(parameters);
            return this;
        }

        public ThreadTask RunForever(CancellationToken token)
        {
            if (Running) { return this; }
            Running = true;
            RunForeverInternal(token);
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
        /// <param name="timeOut">In seconds</param>
        /// <returns></returns>
        public ThreadTask SetTimeOut(int timeOutInSeconds)
        {
            this.TimeOut = timeOutInSeconds;
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

        public ThreadTask SetLimitToDenyQueueRequest(int limitInSeconds)
        {
            this.TimetInSecondsToDenyQueueRequest = limitInSeconds;
            return this;
        }

        public Task AddToQueue(Task task)
        {
            if (TimetInSecondsToDenyQueueRequest != 0)
            {
                double averange = 0;
                lock (ExecutionTimes)
                {
                    averange = ExecutionTimes.Count == 0 ? 0 : ExecutionTimes.Average(x => x);
                }

                var expectedWaitingTimeInQueue = averange * Queue.Count;

                if (expectedWaitingTimeInQueue >= TimetInSecondsToDenyQueueRequest)
                {
                    throw new BadRequestException($"The queue time is longer than the {TimetInSecondsToDenyQueueRequest} seconds set timeout");
                }
            }

            lock (Queue)
            {
                task.Add();
                Queue.Add(task);
            }

            return task;
        }

        #endregion

        #region PRIVATE

        private void RunForeverInternal(CancellationToken token)
        {
            new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    WhaitThread();
                    Task next;
                    lock (Queue)
                    {
                        next = Queue.Next();
                    }

                    if (next == null)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        AddThread();

                        new Thread((object param) =>
                        {
                            var task = param as Task;
                            ExecuteOperation(task);
                            ItemsPerformed.Add(task);
                            RemoveThread();

                            lock (ExecutionTimes)
                            {
                                if (ExecutionTimes.Count > 500)
                                {
                                    ExecutionTimes.RemoveRange(0, 400);
                                }

                                ExecutionTimes.Add(task.TimeExecuted.TotalSeconds);
                            }

                        }).Start(next);
                    }

                }
            }).Start();
        }

        public List<double> ExecutionTimes { get; set; } = new List<double>();

        private MultiThreadsInfo GetInfo()
        {
            double averange = 0;
            lock (ExecutionTimes)
            {
                averange = ExecutionTimes.Count == 0 ? 0 : ExecutionTimes.Average(x => x);
            }

            var currentQueueSize = Queue.Count;

            return new MultiThreadsInfo
            {
                AverageRunningTime = averange,
                CurrentQueueSize = currentQueueSize,
                ExpectedWaitingTimeInQueue = (averange * currentQueueSize) / SimultaneousThreads,
                RunningQuantity = SimultaneousThreadsNow
            };
        }

        private void Execute(List<Task> parameters)
        {
            if (ThreadsCount != 0)
            {
                throw new Exception("This operation has already run!");
            }

            ThreadsCount = parameters.Count;

            parameters.ForEach(x => x.Add());

            new Thread(() =>
            {
                foreach (var parameter in parameters)
                {
                    WhaitThread();
                    AddThread();

                    new Thread((object param) =>
                    {
                        ExecuteOperation((Task)param);
                        RemoveThread();
                    }
                  ).Start(parameter);
                }
            }).Start();
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

        private void ExecuteOperation(Task param)
        {
            param.Start();

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

            if (TimeOut == 0)
            {
                task.Wait();
            }

            if (!task.Wait(TimeSpan.FromSeconds(this.TimeOut)))
            {
                param.Exception = new TimeoutException("Thread terminated after " + this.TimeOut + " second runtime");
            }

            param.End();
        }

        #endregion
    }
}
