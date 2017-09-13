﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Multi.Threads.Test.Util;

namespace Multi.Threads.Test
{
    public class Util
    {
        private static object LockMessage = new object();

        public static void ShowMsg(string mensagem, ConsoleColor cor = ConsoleColor.Black, int tamanhoFonte = 10)
        {
            lock (LockMessage)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = cor;
                Console.WriteLine(DateTime.Now + ":" + mensagem);
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }
    }
}
