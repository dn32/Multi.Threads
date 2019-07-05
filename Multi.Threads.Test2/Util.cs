using System;

namespace Multi.Threads.Test2
{
    public class Util
    {
        private static object LockMessage = new object();

        public static void ShowMsg(string mensagem, ConsoleColor cor = ConsoleColor.Black, int tamanhoFonte = 10)
        {
            lock (LockMessage)
            {
                //Console.BackgroundColor = ConsoleColor.White;
                //Console.ForegroundColor = cor;
                //Console.WriteLine(DateTime.Now + ":" + mensagem);
                Console.WriteLine(mensagem);
               // Console.ForegroundColor = ConsoleColor.Black;
            }
        }
    }
}
