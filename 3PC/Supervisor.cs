﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Event;
using _3PC.Shared.Actors;
using _3PC.Shared.Messages;

namespace _3PC
{
    public class Supervisor : UntypedActor
    {
        public ILoggingAdapter Log { get; } = Context.GetLogger();

        private int _cohortCount;
        private readonly List<IActorRef> _cohorts = new List<IActorRef>();
        private IActorRef _coordinator;
        private readonly int _agreeCount;
        private readonly int _abortCount;

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
                    Start();
                    break;
                case "stop":
                    Stop();
                    break;
                case int id:
                    SimulateFailure(id);
                    break;
                default:
                    Log.Warning($"Received unsupported message {message}");
                    break;
            }
        }
        private void Start()
        {
            _coordinator.Tell(AgreeRequest.Instance);
        }

        private void SimulateFailure(int id)
        {
            if (id == 0)
                _coordinator.Tell(Fail.Instance);
            else if (id > 0 && id <= _cohortCount)
                _cohorts.ElementAt(id - 1).Tell(Fail.Instance);
        }

        private void Stop()
        {
            foreach (IActorRef cohort in _cohorts)
            {
                Context.Stop(cohort);
            }
            Context.Stop(_coordinator);
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
                    _cohorts.Add(Context.System.ActorOf(CohortActor.Props(i, true), "cohort" + i));
                }
                for (int i = _agreeCount + 1; i <= _cohortCount; i++)
                {
                    _cohorts.Add(Context.System.ActorOf(CohortActor.Props(i, false), "cohort" + i));
                }

                Console.WriteLine($"Creating coordinator...");
                _coordinator = Context.System.ActorOf(CoordinatorActor.Props(_cohorts), "coordinator");

                Console.WriteLine($"Cohorts and coordinator created.");
            }
        }

        public static Props Props(int agreeCount, int abortCount) =>
            Akka.Actor.Props.Create(() => new Supervisor(agreeCount, abortCount));
    }
}