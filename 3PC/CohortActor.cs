using System;
using System.Net.NetworkInformation;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using _3PC.Messages;

namespace _3PC
{
    public class CohortActor : UntypedActor
    {
        private const int DELAY_IN_MS = 1500;

        private readonly int _id;
        private readonly bool _shouldAgree;
        private int _timerCount = 0;
        private readonly IActorRef _timerActorRef;
        private static readonly Object ConsoleLock = new Object();

        public CohortActor(int id, bool shouldAgree)
        {
            _id = id;
            _shouldAgree = shouldAgree;
            _timerActorRef = Context.ActorOf<TimerActor>();
        }

        public ILoggingAdapter Log { get; } = Context.GetLogger();

        protected override void PreStart() => Log.Info($"Cohort {_id} starting.");
        protected override void PostStop() => Log.Info($"Cohort {_id} stopping.");

        protected override void OnReceive(object message)
        {
            Delay();
            switch (message)
            {
                case AgreeRequest _:
                    if (_shouldAgree)
                    {
                        Print("AgreeRequest, Q -> W, (replying agree)");
                        Sender.Tell(Agree.Instance);
                        StartTimer();
                        Become(W);
                    }
                    else
                    {
                        Print("AgreeRequest, Q -> A, (replying abort)");
                        Sender.Tell(Abort.Instance);
                        Become(A);
                    }
                    break;
                case Fail _:
                    Print("Fail, Q -> A");
                    Become(A);
                    break;
            }
        }

        private void W(object message)
        {
            Delay();
            switch (message)
            {
                case Prepare _:
                    Print("Prepare, W -> P (sending ack)");
                    StartTimer();
                    Sender.Tell(PrepareAck.Instance);
                    Become(P);
                    break;
                case Rollback _:
                    Print("Rollback, W -> A");
                    Become(A);
                    break;
                case TimerActor.Timeout _:
                    if (IsTimeout())
                    {
                        Print("Timeout, W -> A");
                        Become(A);
                    }
                    break;
                case Fail _:
                    Print("Fail, W -> A");
                    Become(A);
                    break;
            }
        }

        private void P(object message)
        {
            Delay();
            switch (message)
            {
                case Commit _:
                    Print("Commit, P -> C");
                    Become(C);
                    break;
                case Rollback _:
                    Print("Rollback, P -> A");
                    Become(A);
                    break;
                case TimerActor.Timeout _:
                    if (IsTimeout())
                    {
                        Print("Timeout, P -> C");
                        Become(C);
                    }
                    break;
                case Fail _:
                    Print("Fail, P -> C");
                    Become(C);
                    break;
            }
        }

        private void C(object message)
        {
            Terminate();
        }

        private void A(object message)
        {
            Terminate();
        }

        private void StartTimer()
        {
            _timerActorRef.Tell(TimerActor.Start.Instance);
            _timerCount++;
        }

        private bool IsTimeout()
        {
            _timerCount--;
            return _timerCount <= 0;
        }
        private void Delay()
        {
            Thread.Sleep(DELAY_IN_MS);
        }

        private void Terminate()
        {
            Context.Stop(_timerActorRef);
            Context.Stop(Self);
        }

        private void Print(string messageToPrint)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColorFactory.FromId(_id);
                Console.WriteLine($"{_id}:\t{messageToPrint}");
                Console.ResetColor();
            }
        }

        public static Props Props(int id, bool shouldAgree) =>
            Akka.Actor.Props.Create(() => new CohortActor(id, shouldAgree));
    }
}