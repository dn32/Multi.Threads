using Newtonsoft.Json;
using System;
using System.Threading;

namespace Multi.Threads.Test2
{
    class Program
    {
        private static CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        private static void ShowInfo(object parameter)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var controller = parameter as Controller;
                var info = controller.Info;
                var json = JsonConvert.SerializeObject(info, Formatting.Indented);
                Util.ShowMsg(json);
                Thread.Sleep(1000);
            }
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { CancellationTokenSource.Cancel(); };

            var controller = new Controller(CancellationTokenSource);
            controller.StartQueues();
            long i = 1;

            new Thread(ShowInfo).Start(controller);

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var ret = controller.AddToQueueAsync(i);
                    var ddd = 2;
                }
                catch (Exception ex)
                {
                    Util.ShowMsg(ex.Message);
                    Thread.Sleep(2000);
                }

                i++;
                Thread.Sleep(10);
            }
        }
    }
}
