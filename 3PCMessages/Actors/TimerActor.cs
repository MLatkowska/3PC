using System;
using Akka.Actor;
using _3PC.Shared.Messages;

namespace _3PC.Shared.Actors
{
    public class TimerActor : UntypedActor
    {
        private ICancelable _scheduledTimeout;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case StartTimer startTimer:
                    _scheduledTimeout?.Cancel();
                    _scheduledTimeout = Context.System.Scheduler.ScheduleTellOnceCancelable(new TimeSpan(0, 0, startTimer.Seconds), Sender, Timeout.Instance, null);
                    break;
            }
        }

        protected override void PostStop()
        {
            _scheduledTimeout?.Cancel();
        }

        public static Props Props() => Akka.Actor.Props.Create<TimerActor>();
    }
}