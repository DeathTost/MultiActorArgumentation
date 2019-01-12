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

        public JudgeActor()
        {
            CreateProsecutorAndDefender();
            ReceiveUserInput();
            StartArgumentation();
            ArgumentRedirection();
            EndArgumentation();
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
                Console.WriteLine("Please provide a path to a file with your case.");
                var argumentationPath = Console.ReadLine();

                Self.Tell(new StartArgumentationTreeMsg());
                Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(100), Prosecutor, Kill.Instance);
            });
        }

        private void StartArgumentation()
        {
            Receive<StartArgumentationTreeMsg>((_) =>
            {
                Console.WriteLine($"Judge: {Self.Path}");
                //for now starting with argument 1 
                Root = Context.ActorOf(Props.Create(() => new TreeNodeActor(1,3)), "TreeRoot");
            });
        }

        private void EndArgumentation()
        {
            Receive<EndArgumentationMsg>((x) =>
            {
                Console.WriteLine($"Sender: {Sender.Path}");
                Console.WriteLine(x.ArgumentationResult);
                Console.WriteLine("Judge has left!");
                Console.WriteLine("Case is closed!");
            });

            Receive<NodeResultMsg>((x) =>
            {
                Console.WriteLine("Case is closed!");
                if (x.Active)
                {
                    Console.WriteLine("Thesis has been proved!");
                }
                else
                {
                    Console.WriteLine("Thesis has been disproved!");
                }
                Console.WriteLine("Judge has left!");
            });
        }

        private void ArgumentRedirection()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                Prosecutor.Tell(x);
                Defender.Tell(x);
            });
        }

    }
}
