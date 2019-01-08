using Akka.Actor;
using System;
using System.Collections.Generic;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeNodeActor : ReceiveActor
    {
        private int Argument;

        private Dictionary<int, IActorRef> ProsecutorChildren = new Dictionary<int, IActorRef>();
        private Dictionary<int, IActorRef> DefenderChildren = new Dictionary<int, IActorRef>();
        private int ProsecutorResponseCounter = 0;
        private int DefenderResponseCounter = 0;

        private bool ProsecutorAnswered = false;
        private bool DefenderAnswered = false;

        // mock for deciding what to do with node
        private int EvaluationValue = 0;

        public TreeNodeActor(int argument, int layersLeft)
        {
            Argument = argument;
            if (layersLeft != 0)
            {
                Context.Parent.Tell(new RelatedArgumentsQueryMsg(argument, Context.Self));
            }
            else
            {
                System.Console.WriteLine($"{layersLeft} is 0. No new children.");
                Context.Parent.Tell(new NodeResultMsg(argument));
            }
            System.Console.WriteLine(Context.Self.Path);

            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                Console.WriteLine("Passing message further");
                x.AppendArgument(Argument);
                Context.Parent.Tell(x);
            });

            Receive<RelatedArgumentsProsecutorResponseMsg>((x) =>
            {
                Console.WriteLine("Creating Defender children");
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, layersLeft - 1)), "argument" + id);
                    ProsecutorChildren.Add(id,child);
                }
                ProsecutorAnswered = true;
                if (DefenderAnswered && ProsecutorAnswered && DefenderChildren.Count == 0 && ProsecutorChildren.Count == 0)
                {
                    System.Console.WriteLine("Got no arguments from Prosecutor and Defender");
                    Context.Parent.Tell(new NodeResultMsg(argument));
                }
            });

            Receive<RelatedArgumentsDefenderResponseMsg>((x) =>
            {
                Console.WriteLine("Creating Defender children");
                foreach (var id in x.RelatedArguments)
                {
                    var child = Context.ActorOf(Props.Create(() => new TreeNodeActor(id, layersLeft - 1)), "argument"+id);
                    DefenderChildren.Add(id,child);
                }
                DefenderAnswered = true;
                if (DefenderAnswered && ProsecutorAnswered && DefenderChildren.Count == 0 && ProsecutorChildren.Count == 0)
                {
                    System.Console.WriteLine("Got no arguments from Prosecutor and Defender");
                    Context.Parent.Tell(new NodeResultMsg(argument));
                }
            });

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
                    if (Argument * EvaluationValue >= 0)
                    {
                        System.Console.WriteLine("Evaluated to be active");
                        Context.Parent.Tell(new NodeResultMsg(argument));
                    }
                    else
                    {
                        System.Console.WriteLine("Evaluated to be disactivated");
                        Context.Parent.Tell(new NodeResultMsg(argument, false));
                    }
                }
            });

            //legacy
            //Receive<CreateChildMsg>((x) =>
            //{
            //    layerLeft--;
            //    if (layerLeft < 0)
            //    {
            //        System.Console.WriteLine($"{layerLeft} is lower than 0. No new childs.");
            //        Sender.Tell(new EndArgumentationMsg("Guilty"));
            //        return;
            //    }
            //    var leftChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Left{layerLeft}");
            //    var rightChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Right{layerLeft}");
            //    leftChild.Tell(new CreateChildMsg(""));
            //    rightChild.Tell(new CreateChildMsg(""));
            //});
            //Receive<EndArgumentationMsg>((x) =>
            //{
            //    Console.WriteLine("Sending EndingMessage");
            //    Console.WriteLine($"Sender: {Sender.Path}");
            //    Console.WriteLine($"Receiver: {Self.Path}");
            //    Console.WriteLine($"Parent: {Context.Parent.Path}");
            //    Context.Parent.Tell(new EndArgumentationMsg(x.ArgumentationResult));
            //});
            //Receive<EndArgumentationMsg>((x) =>
            //{
            //    Console.WriteLine("Sending EndingMessage");
            //    Console.WriteLine($"Sender: {Sender.Path}");
            //    Console.WriteLine($"Receiver: {Self.Path}");
            //    Console.WriteLine($"Parent: {Context.Parent.Path}");
            //    Context.Parent.Tell(new EndArgumentationMsg(x.ArgumentationResult));
            //});
        }
    }
}
