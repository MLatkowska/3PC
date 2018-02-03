using System;
using Akka.Actor;
using Akka.Event;
using _3PC.Shared.Messages;

namespace _3PC.Shared.Actors
{
    public class CohortActor : UntypedActor
    {
        private const int DELAY_IN_MS = 500;

        private readonly int _id;
        private readonly bool _shouldAgree;
        private readonly IActorRef _timerActorRef;

        public CohortActor(int id, bool shouldAgree)
        {
            _id = id;
            _shouldAgree = shouldAgree;
            _timerActorRef = Context.ActorOf<TimerActor>("timer"+_id);
        }

        protected override void OnReceive(object message)
        {
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
            Delay();
        }

        private void W(object message)
        {
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
                case Timeout _:
                    Print("Timeout, W -> A");
                    Become(A);
                    break;
                case Fail _:
                    Print("Fail, W -> A");
                    Become(A);
                    break;
            }
            Delay();
        }

        private void P(object message)
        {
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
                case Timeout _:
                    Print("Timeout, P -> C");
                    Become(C);
                    break;
                case Fail _:
                    Print("Fail, P -> C");
                    Become(C);
                    break;
            }
            Delay();
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
            _timerActorRef.Tell(new StartTimer(20));
        }

        private void Delay()
        {
            System.Threading.Thread.Sleep(DELAY_IN_MS);
        }

        private void Terminate()
        {
            Context.Stop(_timerActorRef);
            Context.Stop(Self);
        }

        private void Print(string messageToPrint)
        {
            ColorfulConsole.WriteLine(_id, messageToPrint);
        }

        public static Props Props(int id, bool shouldAgree) =>
            Akka.Actor.Props.Create(() => new CohortActor(id, shouldAgree));
    }
}