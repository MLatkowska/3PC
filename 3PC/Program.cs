using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using _3PC.Messages;

namespace _3PC
{
    class Program
    {
        static void Main(string[] args)
        {
        
            while (true)
            {
                Run3PC();

                if (ShouldExit())
                    return;
            }
        }

        private static void Run3PC()
        {
            using (var system = ActorSystem.Create("3pc-system"))
            {
                int agreeCount = GetAgreeCohortCount();
                int abortCount = GetAbortCohortCount();
                var supervisor = system.ActorOf(Supervisor.Props(agreeCount, abortCount), "supervisor");

                bool run = true;
                while (run)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Enter:
                            supervisor.Tell("start");
                            break;
                        case ConsoleKey.Escape:
                            run = false;
                            break;
                        case ConsoleKey.K:
                            supervisor.Tell("Coordinator");
                            break;
                        case ConsoleKey.U:
                            supervisor.Tell("Cohort");
                            break;
                            //case ConsoleKey.O:
                            //    supervisor.Tell(LooseNextPong.Instance);
                            //    break;
                    }
                }
            }
        }


        private static int GetAgreeCohortCount()
        {
            int count = -1;
            while (count < 0)
            {
                Console.WriteLine("Specify number of cohorts that should vote agree:");
                Int32.TryParse(Console.ReadLine(), out count);
            }

            return count;
        }

        private static int GetAbortCohortCount()
        {
            int count = -1;
            while (count < 0)
            {
                Console.WriteLine("Specify number of cohorts that should vote abort:");
                Int32.TryParse(Console.ReadLine(), out count);
            }

            return count;
        }

        private static bool ShouldExit()
        {
            while (true)
            {
                Console.WriteLine("Press Enter to restart, or Esc to quit");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        return true;
                    case ConsoleKey.Enter:
                        return false;
                }

            }
        }
    }
}
