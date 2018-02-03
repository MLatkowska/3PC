using System;
using Akka.Actor;

namespace _3PC.Shared.Actors
{
    public class TimerActor : UntypedActor
    {
        private static readonly int TimeoutInSeconds = 12;

        public class Start
        {
            public static Start Instance = new Start();
            private Start() { }
        }

        public class Timeout
        {
            public static Timeout Instance = new Timeout();
            private Timeout() { }
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Start _:
                    Context.System.Scheduler.ScheduleTellOnce(new TimeSpan(0, 0, TimeoutInSeconds), Sender, Timeout.Instance, null);
                    break;
            }
        }

        public static Props Props() => Akka.Actor.Props.Create<TimerActor>();
    }
}