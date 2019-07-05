using System;
using System.Collections.Generic;
using System.Threading;
using static Multi.Threads.Test.Util;

namespace Multi.Threads.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            var parameters = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                parameters.Add(new Task(i));
            }

            var t = ThreadTask
                    .Instance()
                    .SetAction(Operation)
                    .SetSimultaneousThreads(10)
                    .SetThreadsPerMinute(50)
                    .SetTimeOut(1)
                    .RunAsync(parameters);

            /* Execute another operation */
            Thread.Sleep(2000);

            t.Wait();

            parameters.ForEach((task) =>
            {
                if (task.Exception == null)
                {
                    ShowMsg(task.Parameter + " - " + task.Return, ConsoleColor.Blue);
                }
                else
                {
                    ShowMsg(task.Parameter + " - " + task.Exception.Message, ConsoleColor.Red);
                }
            });

            Console.ReadKey();
        }

        private static object Operation(object parameter)
        {
            ShowMsg(parameter.ToString());
            Thread.Sleep(2000);
           return DateTime.Now;
        }
    }
}
