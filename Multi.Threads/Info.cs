namespace Multi.Threads
{
    public class MultiThreadsInfo
    {
        public int RunningQuantity { get; set; }
        public int CurrentQueueSize { get; set; }
        public double AverageRunningTime { get; set; }
        public double ExpectedWaitingTimeInQueue { get; set; }
    }
}
