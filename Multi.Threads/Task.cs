using System;
using System.Diagnostics;
using System.Threading;

namespace Multi.Threads
{
    public class Task
    {
        public EnumStatus Status { get; set; }
        private Stopwatch StopwatchStandby { get; set; }
        private Stopwatch StopwatchExecution { get; set; }

        public TimeSpan StandbyTime { get; set; }
        public TimeSpan TimeExecuted { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public object Parameter { get; internal set; }
        public Exception Exception { get; internal set; }
        public object Return { get; internal set; }

        public Task(object parameter)
        {
            this.Parameter = parameter;
        }

        public Task()
        {
        }

        public Task SetCancelationToken(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return this;
        }

        public void Wait(int clock = 50)
        {
            while (CancellationToken.IsCancellationRequested == false && Status != EnumStatus.FINISHED)
            {
                Thread.Sleep(50);
            }
        }

        public object Result(int clock = 50)
        {
            while (CancellationToken.IsCancellationRequested == false && Status != EnumStatus.FINISHED)
            {
                Thread.Sleep(50);
            }

            return Return;
        }

        internal void Add()
        {
            Status = EnumStatus.WAITING;
            StopwatchStandby = new Stopwatch();
            StopwatchStandby.Start();
        }

        internal void Start()
        {
            Status = EnumStatus.RUNNING;
            StopwatchStandby.Stop();
            StandbyTime = StopwatchStandby.Elapsed;
            StopwatchExecution = new Stopwatch();
            StopwatchExecution.Start();
        }

        internal void End()
        {
            Status = EnumStatus.FINISHED;
            StopwatchExecution.Stop();
            TimeExecuted = StopwatchExecution.Elapsed;
        }
    }
}
