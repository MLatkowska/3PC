using System;

namespace _3PC.Shared
{
    public class ConsoleColorFactory
    {
        public static ConsoleColor FromId(int id)
        {
            switch (id)
            {
                case 0: return ConsoleColor.DarkCyan;
                case 1: return ConsoleColor.Blue;
                case 2: return ConsoleColor.Green;
                case 3: return ConsoleColor.Yellow;
                case 4: return ConsoleColor.Red;
                case 5: return ConsoleColor.Magenta;
                case 6: return ConsoleColor.Cyan;
                default: return ConsoleColor.White;
            }
        }
    }
}