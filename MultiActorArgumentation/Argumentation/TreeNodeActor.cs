using Akka.Actor;
using System;
using System.Collections.Generic;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeNodeActor : ReceiveActor
    {
        private string Argument;
        private int Turn;

        private IDictionary<string, IActorRef> ProsecutorChildren = new Dictionary<string, IActorRef>();
        private IDictionary<string, IActorRef> DefenderChildren = new Dictionary<string, IActorRef>();
        private int ProsecutorResponseCounter = 0;
        private int DefenderResponseCounter = 0;

        private bool ProsecutorAnswered = false;
        private bool DefenderAnswered = false;
        private bool Answered = false;

        // mock for deciding what to do with node
        private int EvaluationValue = 0;

        public TreeNodeActor(string argument,int turn, int layersLeft)
        {
            Argument = argument;
            Turn = turn;
            this.SetReceiveTimeout(TimeSpan.FromSeconds(10 * (layersLeft+1)));
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
            WaitingTooLongForAnswer();
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
                if (!Answered)
                {
                    Console.WriteLine("Nothing is happening with me... I'm bored, goodbye");
                    Eval();
                }
            });
        }

        private void CreateDefenderChildren(int layersLeft)
        {
            Receive<RelatedArgumentsDefenderResponseMsg>((x) =>
            {
                Console.WriteLine($"Creating {x.RelatedArguments.Count} Defender children");
                var i = 0;
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, -1, layersLeft - 1)), "argument"+layersLeft+"DEF" + i);
                    DefenderChildren.Add(id, child);
                    i++;
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
                Console.WriteLine("Creating Prosecutor children");
                var i = 0;
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, 1, layersLeft - 1)), "argument" + layersLeft + "PRO" + i);
                    ProsecutorChildren.Add(id, child);
                    i++;
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

                if (x.Active)
                {
                    if (ProsecutorChildren.Keys.Contains(x.Argument)) EvaluationValue++;
                    if (DefenderChildren.Keys.Contains(x.Argument)) EvaluationValue--;
                }

                if (ProsecutorResponseCounter >= ProsecutorChildren.Count
                    && DefenderResponseCounter >= DefenderChildren.Count
                    && DefenderAnswered && ProsecutorAnswered)
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

        private void Answer(string argument, bool active = true)
        {
            Context.Parent.Tell(new NodeResultMsg(Argument, active));
            Answered = true;
            Context.Stop(Self);
        }

        private void Eval()
        {
            if (Turn * EvaluationValue >= 0)
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
