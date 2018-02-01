using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using _3PC.Messages;

namespace _3PC
{
    public class Supervisor : UntypedActor
    {
        public ILoggingAdapter Log { get; } = Context.GetLogger();

        private int _cohortCount;
        private readonly List<IActorRef> _cohorts = new List<IActorRef>();
        private IActorRef _coordinator;
        private int _agreeCount;
        private int _abortCount;

        public Supervisor(int agreeCount, int abortCount)
        {
            this._agreeCount = agreeCount;
            this._abortCount = abortCount;
            CreateCohortsAndCoordinator();
        }

        protected override void PreStart() => Log.Info("3PC started");
        protected override void PostStop() => Log.Info("3PC stopped");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "start":
                    StartCoordinator();
                    break;
                case "Coordinator":
                    _coordinator.Tell(Fail.Instance);
                    break;
                case "Cohort":
                    _cohorts.First().Tell(Fail.Instance);
                    break;
                default:
                    Log.Warning($"Received unsupported message {message}");
                    break;
            }
        }

        private void StartCoordinator()
        {
            _coordinator.Tell(CoordinatorActor.Start.Instance);
        }

        private void CreateCohortsAndCoordinator()
        {
            _cohortCount = _agreeCount + _abortCount;
            if (_cohortCount < 1)
            {
                Console.WriteLine("There should be at least 1 cohort.");
            }
            else
            {
                Console.WriteLine($"Creating {_cohortCount} cohorts...");
                for (int i = 1; i <= _agreeCount; i++)
                {
                    _cohorts.Add(Context.ActorOf(CohortActor.Props(i, true)));
                }
                for (int i = _agreeCount+1; i <= _cohortCount; i++)
                {
                    _cohorts.Add(Context.ActorOf(CohortActor.Props(i, false)));
                }

                Console.WriteLine($"Creating coordinator...");
                _coordinator = Context.ActorOf(CoordinatorActor.Props(_cohorts));

                Console.WriteLine($"Cohorts and coordinator created.");
            }
        }

        public static Props Props(int agreeCount, int abortCount) =>
            Akka.Actor.Props.Create(() => new Supervisor(agreeCount, abortCount));
    }
}