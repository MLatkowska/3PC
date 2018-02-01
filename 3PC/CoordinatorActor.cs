using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using _3PC.Messages;
using _3PCMessages;

namespace _3PC
{
    public class CoordinatorActor : UntypedActor
    {
        private const int DELAY_IN_MS = 1500;

        private readonly int _id = 0;
        private int _timerCount = 0;
        private readonly IActorRef _timerActorRef = Context.ActorOf<TimerActor>();
        private static readonly Object ConsoleLock = new Object();
        private readonly List<IActorRef> _cohorts;
        private readonly int _cohortsCount;
        private int _agreeCount;
        private int _abortCount;
        private int _prepareAckCount;


        public CoordinatorActor(List<IActorRef> cohorts)
        {
            _cohorts = cohorts;
            _cohortsCount = _cohorts.Count;
        }

        public ILoggingAdapter Log { get; } = Context.GetLogger();

        protected override void PreStart() => Log.Info($"Coordinator {_id} starting.");
        protected override void PostStop() => Log.Info($"Coordinator {_id} stopping.");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Start _:
                    SendToCohorts(AgreeRequest.Instance);
                    StartTimer();
                    Become(W0);
                    break;
            }
        }

        private void W0(object message)
        {
            Delay();
            switch (message)
            {
                case Agree _:
                    _agreeCount++;
                    Print($"Received agree ({_agreeCount} of {_cohortsCount})");
                    break;
                case Abort _:
                    _abortCount++;
                    Print("Received abort");
                    break;
                case TimerActor.Timeout _:
                    _timerCount--;
                    if (IsTimeout())
                    {
                        Print("Timeout, W0 -> A0");
                        Become(A0);
                    }
                    break;
                case Fail _:
                    Print("Fail, W0 -> A0");
                    Become(A0);
                    break;
            }

            if (AllCohortsReplied())
            {
                if (AllCohortsAgreed())
                {
                    Print("All agreed, W0 -> P0 (sending prepare)");
                    SendToCohorts(Prepare.Instance);
                    StartTimer();
                    Become(P0);
                }
                else
                {
                    Print("Not all agreed, W0 -> A0 (sending rollback)");
                    SendToCohorts(Rollback.Instance);
                    Become(A0);
                }
            }
        }

        private void P0(object message)
        {
            Delay();
            switch (message)
            {
                case PrepareAck _:
                    Print("Received prepare");
                    _prepareAckCount++;
                    if (AllCohortsPrepared())
                    {
                        Print("All prepared, P0 -> C0 (sending commit)");
                        SendToCohorts(Commit.Instance);
                        Become(C0);
                    }
                    break;
                case TimerActor.Timeout _:
                    _timerCount--;
                    if (IsTimeout())
                    {
                        Print("Timeout, P0 -> A0 (sending rollback)");
                        SendToCohorts(Rollback.Instance);
                        Become(A0);
                    }
                    break;
                case Fail _:
                    Print("Fail, P0 -> C0");
                    Become(C0);
                    break;
            }
        }

        private void C0(object message)
        {
            Terminate();
        }

        private void A0(object message)
        {
            Terminate();
        }

        private bool AllCohortsReplied()
        {
            return _agreeCount + _abortCount == _cohortsCount;
        }

        private bool AllCohortsAgreed()
        {
            return _agreeCount == _cohortsCount;
        }

        private bool AllCohortsPrepared()
        {
            return _prepareAckCount == _cohorts.Count;
        }

        private void SendToCohorts(Object message)
        {
            foreach (IActorRef cohort in _cohorts)
            {
                cohort.Tell(message);
            }
        }

        private void StartTimer()
        {
            _timerActorRef.Tell(TimerActor.Start.Instance);
            _timerCount++;
        }

        private bool IsTimeout()
        {
            _timerCount--;
            return _timerCount <= 0; //TODO: Only == 0
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

        public static Props Props(List<IActorRef> cohorts) =>
            Akka.Actor.Props.Create(() => new CoordinatorActor(cohorts));

        public class Start
        {
            public static Start Instance = new Start();
            private Start() { }
        }
    }
}