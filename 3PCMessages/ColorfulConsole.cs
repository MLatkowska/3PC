using System;
using System.Collections.Generic;
using System.Text;

namespace _3PC.Shared
{
    public static class ColorfulConsole
    {
        private static readonly Object ConsoleLock = new Object();

        public static void WriteLine(int id, string messageToPrint)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColorFactory.FromId(id);
                Console.WriteLine($"{id}:\t{messageToPrint}");
                Console.ResetColor();
            }
        }
    }
}
