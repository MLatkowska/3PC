using System;
using Akka.Actor;
using Akka.Configuration;

namespace _3PC.Shared
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
            var config = ConfigurationFactory.ParseString(@"
                akka {  
                    actor{
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                        deployment {
                            /cohort1 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                            /cohort2 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                            /cohort3 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                            /cohort4 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                            /cohort5 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                            /cohort6 {
                                remote = ""akka.tcp://CohortDeployTarget@localhost:8090""
                            }
                        }
                    }
                    remote {
                        helios.tcp {
		                    port = 0
		                    hostname = localhost
                        }
                    }
                }");

            using (var system = ActorSystem.Create("Deployer", config))
            {

                int agreeCount = GetAgreeCohortCount();
                int abortCount = GetAbortCohortCount();
                var supervisor = system.ActorOf(Supervisor.Props(agreeCount, abortCount), "supervisor");

                //var remoteAddress = Address.Parse("akka.tcp://CohortDeployTarget@localhost:8090");
                //var coordinator =
                 //   system.ActorOf(Props.Create(() => new CoordinatorActor(new List<IActorRef>())), "coordinator");
                //var remoteEcho2 =
                //    system.ActorOf(
                //        Props.Create(() => new EchoActor())
                //            .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))),
                //        "coderemoteecho"); //deploy remotely via code

                //system.ActorOf(Props.Create(() => new HelloActor(remoteEcho1)));
                //system.ActorOf(Props.Create(() => new HelloActor(remoteEcho2)));

                //system.ActorSelection("/user/remoteecho").Tell(new Hello("hi from selection!"));



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
                    }
                }
            }

            Console.ReadKey();
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
