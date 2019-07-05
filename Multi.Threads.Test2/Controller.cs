using System;
using System.Threading;

namespace Multi.Threads.Test2
{
    public class Controller
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private ThreadTask ThreadTask { get; set; }

        public MultiThreadsInfo Info => ThreadTask.Info;

        private static Random Random { get; set; } = new Random();

        private static int RandomNumber(int min, int max) => Random.Next(min, max);

        public Controller(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
        }

        public void StartQueues()
        {
            ThreadTask = ThreadTask
                        .Instance()
                        .SetAction(Operation)
                        .SetSimultaneousThreads(10)
                        // .SetLimitToDenyQueueRequest(300)
                        .SetTimeOut(30)
                        .RunForever(CancellationTokenSource.Token);
        }

        public void SetSimultaneousThreads(int time)
        {
            ThreadTask.SetSimultaneousThreads(time);
        }

        public Task AddToQueueAsync(object parameter)
        {
            return ThreadTask.AddToQueue(new Task(parameter));
        }

        public object AddToQueue(object parameter)
        {
            var task = ThreadTask.AddToQueue(new Task(parameter));
            return task.Result();
        }

        private static object Operation(object parameter)
        {
            Thread.Sleep(RandomNumber(100, 250));
            return DateTime.Now;
        }
    }
}
