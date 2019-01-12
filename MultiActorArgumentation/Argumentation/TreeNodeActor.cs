using Akka.Actor;
using System;
using System.Collections.Generic;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeNodeActor : ReceiveActor
    {
        private int Argument;

        private IDictionary<int, IActorRef> ProsecutorChildren = new Dictionary<int, IActorRef>();
        private IDictionary<int, IActorRef> DefenderChildren = new Dictionary<int, IActorRef>();
        private int ProsecutorResponseCounter = 0;
        private int DefenderResponseCounter = 0;

        private bool ProsecutorAnswered = false;
        private bool DefenderAnswered = false;
        private bool Answered = false;

        // mock for deciding what to do with node
        private int EvaluationValue = 0;

        public TreeNodeActor(int argument, int layersLeft)
        {
            Argument = argument;
            this.SetReceiveTimeout(TimeSpan.FromSeconds(3));
            System.Console.WriteLine(Context.Self.Path);
            if (layersLeft != 0)
            {
                Context.Parent.Tell(new RelatedArgumentsQueryMsg(argument, Context.Self));
            }
            else
            {
                System.Console.WriteLine($"{layersLeft} is 0. No new children.");
                Answer(argument);
            }

            CreateDefenderChildren(layersLeft);
            CreateProsecutorChildren(layersLeft);
            ForwardMessage();
            EvaluateNode();
        }

        private void ForwardMessage()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                Console.WriteLine("Passing message further");
                Context.Parent.Tell(x.AppendArgument(Argument));
            });
        }

        private void WaitingTooLongForAnswer()
        {
            Receive<ReceiveTimeout>((x) =>
            {
                Console.WriteLine("Nothing is happening with me...");
                if (!Answered)
                {
                    Console.WriteLine("I'm bored, goodbye");
                    Eval();
                }
            });
        }

        private void CreateDefenderChildren(int layersLeft)
        {
            Receive<RelatedArgumentsDefenderResponseMsg>((x) =>
            {
                Console.WriteLine("Creating Defender children");
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, layersLeft - 1)), "argument" + id);
                    DefenderChildren.Add(id, child);
                }
                DefenderAnswered = true;
                if (DefenderAnswered && ProsecutorAnswered && DefenderChildren.Count == 0 && ProsecutorChildren.Count == 0)
                {
                    System.Console.WriteLine("Got no arguments from Prosecutor and Defender");
                Answer(Argument);
                }
            });
        }

        private void CreateProsecutorChildren(int layersLeft)
        {
            Receive<RelatedArgumentsProsecutorResponseMsg>((x) =>
            {
                Console.WriteLine("Creating Defender children");
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, layersLeft - 1)), "argument" + id);
                    ProsecutorChildren.Add(id, child);
                }
                ProsecutorAnswered = true;
                if (DefenderAnswered && ProsecutorAnswered && DefenderChildren.Count == 0 && ProsecutorChildren.Count == 0)
                {
                    System.Console.WriteLine("Got no arguments from Prosecutor and Defender");
                Answer(Argument);
                }
            });
        }

        private void EvaluateNode()
        {
            Receive<NodeResultMsg>((x) =>
            {
                Console.WriteLine("Got response from child");
                if (ProsecutorChildren.ContainsKey(x.Argument))
                {
                    ProsecutorResponseCounter++;
                }
                if (DefenderChildren.ContainsKey(x.Argument))
                {
                    DefenderResponseCounter++;
                }

                if (x.Active) EvaluationValue += x.Argument;

                if (ProsecutorResponseCounter == ProsecutorChildren.Count
                    && DefenderResponseCounter == DefenderChildren.Count)
                {
                    Eval();
                }
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(1),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ActorKilledException e:
                            return Directive.Stop;
                        default:
                            return Directive.Stop;
                    }
                });
        }

        protected override void PostStop()
        {
            Console.WriteLine($"{Self.Path} was killed!");
        }

        private void Answer(int argument, bool active = true)
        {
            Context.Parent.Tell(new NodeResultMsg(Argument, active));
            Answered = true;
        }

        private void Eval()
        {
            if (Argument * EvaluationValue >= 0)
            {
                System.Console.WriteLine("Evaluated to be active");
                Answer(Argument);
            }
            else
            {
                System.Console.WriteLine("Evaluated to be disactivated");
                Answer(Argument, false);
            }
        }
    }
}
