using System;

namespace Multi.Threads
{
    public class Task
    {
        public Task(object parameter)
        {
            this.Parameter = parameter;
        }

        public object Parameter { get; private set; }
        public Exception Exception { get; internal set; }
        public object Return { get; internal set; }
        public TimeSpan ElapsedTime { get; set; }
    }
}
