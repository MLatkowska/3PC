using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using _3PC.Shared.Messages;

namespace _3PC.Shared.Actors
{
    public class CoordinatorActor : UntypedActor
    {
        private const int DELAY_IN_MS = 500;

        private readonly int _id = 0;
        private readonly IActorRef _timerActorRef = Context.ActorOf<TimerActor>("timer0");
        private readonly List<IActorRef> _cohorts;

        private readonly int _cohortsCount;
        private int _agreeCount;
        private int _abortCount;
        private int _prepareAckCount;

        private bool AllCohortsReplied => _agreeCount + _abortCount == _cohortsCount;
        private bool AllCohortsAgreed => _agreeCount == _cohortsCount;
        private bool AnyCohortAborted => _abortCount > 0;
        private bool AllCohortsPrepared => _prepareAckCount == _cohorts.Count;


        public CoordinatorActor(List<IActorRef> cohorts)
        {
            _cohorts = cohorts;
            _cohortsCount = _cohorts.Count;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case object _:
                    SendToCohorts(AgreeRequest.Instance);
                    StartTimer();
                    Become(W0);
                    break;
            }
        }

        private void W0(object message)
        {
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
                case Timeout _:
                    Print("Timeout, W0 -> A0");
                    Become(A0);
                    break;
                case Fail _:
                    Print("Fail, W0 -> A0");
                    Become(A0);
                    break;
            }

            if (AllCohortsReplied || AnyCohortAborted)
            {
                if (AllCohortsAgreed)
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
            Delay();
        }

        private void P0(object message)
        {
            switch (message)
            {
                case PrepareAck _:
                    _prepareAckCount++;
                    Print($"Received prepare ({_prepareAckCount} of {_cohortsCount})");
                    if (AllCohortsPrepared)
                    {
                        Print("All prepared, P0 -> C0 (sending commit)");
                        SendToCohorts(Commit.Instance);
                        Become(C0);
                    }
                    break;
                case Timeout _:
                    Print("Timeout, P0 -> A0 (sending rollback)");
                    SendToCohorts(Rollback.Instance);
                    Become(A0);
                    break;
                case Fail _:
                    Print("Fail, P0 -> C0");
                    Become(C0);
                    break;
            }
            Delay();
        }

        private void C0(object message)
        {
            Terminate();
        }

        private void A0(object message)
        {
            Terminate();
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
            _timerActorRef.Tell(new StartTimer(10));
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

        public static Props Props(List<IActorRef> cohorts) =>
            Akka.Actor.Props.Create(() => new CoordinatorActor(cohorts));
    }
}