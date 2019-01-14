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
        private bool isBored = false;

        // mock for deciding what to do with node
        private int EvaluationValue = 0;

        public TreeNodeActor(string argument,int turn, int layersLeft)
        {
            Argument = argument;
            Turn = turn;
            this.SetReceiveTimeout(TimeSpan.FromSeconds(10 * (layersLeft+1)));
            if (layersLeft != 0)
            {
                Context.Parent.Tell(new RelatedArgumentsQueryMsg(argument, Context.Self));
            }
            else
            {
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
                Context.Parent.Tell(x.AppendArgument(Argument));
            });
        }

        private void WaitingTooLongForAnswer()
        {
            Receive<ReceiveTimeout>((x) =>
            {
                if (!Answered)
                {
                    isBored = true;
                    Eval();
                }
            });
        }

        private void CreateDefenderChildren(int layersLeft)
        {
            Receive<RelatedArgumentsDefenderResponseMsg>((x) =>
            {
                Console.WriteLine($"{Self.Path} creates {x.RelatedArguments.Count} defender children");
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
                    Answer(Argument);
                }
            });
        }

        private void CreateProsecutorChildren(int layersLeft)
        {
            Receive<RelatedArgumentsProsecutorResponseMsg>((x) =>
            {
                Console.WriteLine($"{Self.Path} creates {x.RelatedArguments.Count} prosecutor children");
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
                    Answer(Argument);
                }
            });
        }

        private void EvaluateNode()
        {
            Receive<NodeResultMsg>((x) =>
            {
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
            if (isBored)
            {
                Console.WriteLine($"{Self.Path} timed out");
            }
            else if (DefenderChildren.Count == 0 && ProsecutorChildren.Count == 0)
            {
                Console.WriteLine($"{Self.Path} got no arguments from Prosecutor and Defender");
            }
            else
            {
                Console.WriteLine($"{Self.Path} was killed!");
            }
            
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
                Console.WriteLine($"{Self.Path} active");
                Answer(Argument);
            }
            else
            {
                Console.WriteLine($"{Self.Path} inactive");
                Answer(Argument, false);
            }
        }
    }
}
