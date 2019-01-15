using Akka.Actor;
using System;
using System.Collections.Generic;

namespace MultiActorArgumentation.Argumentation
{
    public class JudgeActor : ReceiveActor
    {
        private IActorRef Prosecutor;
        private IActorRef Defender;
        private IActorRef Root;
        private bool KilledAChild;

        public JudgeActor()
        {
            CreateProsecutorAndDefender();
            ReceiveUserInput();
            ArgumentRedirection();
            ArgumentationResult();
        }

        private void CreateProsecutorAndDefender()
        {
            Receive<ReturnParagraphsMsg>((x) =>
            {
                Prosecutor = Context.ActorOf(Props.Create(() => new ProsecutorActor(x.NegativeParagraphs)), "ProsecutorActor");
                Defender = Context.ActorOf(Props.Create(() => new DefenderActor(x.PositiveParagraphs)), "DefenderActor");
                Self.Tell(new UserInputMsg());
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromMinutes(1),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ActorKilledException e:
                            Console.WriteLine($"One of Judge children was killed. Exception: {e.Message}");
                            return Directive.Restart;
                        default:
                            return Directive.Escalate;
                    }
                });
        }

        private void ReceiveUserInput()
        {
            Receive<UserInputMsg>((_) =>
            {
                Console.WriteLine("Judge has come!");
                Console.WriteLine("Please provide a path to a file with your case. <case_file.txt>");
                var path = System.IO.Path.GetFullPath("...\\...\\...\\Argumentation\\");
                var argumentationFile = Console.ReadLine();//case_file.txt
                if (!System.IO.File.Exists(path + argumentationFile))
                {
                    Self.Tell(new UserInputMsg());
                    return;
                }
                var caseArguments = new List<string>();
                var lines = System.IO.File.ReadLines(path + argumentationFile);
                foreach (var line in lines)
                {
                    caseArguments.Add(line);
                }
                Root = Context.ActorOf(Props.Create(() => new TreeRootActor(caseArguments)), "TreeRoot");
                Root.Tell(new StartArgumentationTreeMsg());
                //Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(100), Prosecutor, Kill.Instance);
            });
        }

        private void ArgumentationResult()
        {
            Receive<EndArgumentationMsg>((x) =>
            {
                foreach (var resolvedCase in x.ResolvedCases)
                {
                    var result = resolvedCase.Value ? " case proved" : " case defeated";
                    var isFinished = x.FinishedCases[resolvedCase.Key];
                    if (isFinished)
                    {
                        Console.WriteLine($"{resolvedCase.Key} | {result}");
                    }
                    else
                    {
                        Console.WriteLine("Case wasn't finished on time.");
                    }
                }
                Console.WriteLine("Case is closed.");
                Console.WriteLine("Judge has left!");
            });
        }

        private void ArgumentRedirection()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                if (!KilledAChild && x.BlacklistedArguments.Count > 3)
                {
                    Console.WriteLine("I killed a child and I liked it");
                    //x.QuerySender.Tell(Kill.Instance);
                    Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(100), x.QuerySender, Kill.Instance);
                    KilledAChild = true;
                    return;
                }
                Prosecutor.Tell(x);
                Defender.Tell(x);
            });
        }

    }
}
