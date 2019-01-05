using Akka.Actor;
using System;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeNodeActor : ReceiveActor
    {
        public TreeNodeActor(int layerLeft)
        {
            System.Console.WriteLine(Context.Self.Path);
            Receive<CreateChildMsg>((x) =>
            {
                layerLeft--;
                if (layerLeft < 0)
                {
                    System.Console.WriteLine($"{layerLeft} is lower than 0. No new childs.");
                    Sender.Tell(new EndArgumentationMsg("Guilty"));
                    return;
                }
                var leftChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Left{layerLeft}");
                var rightChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Right{layerLeft}");
                leftChild.Tell(new CreateChildMsg(""));
                rightChild.Tell(new CreateChildMsg(""));
            });
            Receive<EndArgumentationMsg>((x) =>
            {
                Console.WriteLine("Sending EndingMessage");
                Console.WriteLine($"Sender: {Sender.Path}");
                Console.WriteLine($"Receiver: {Self.Path}");
                Console.WriteLine($"Parent: {Context.Parent.Path}");
                Context.Parent.Tell(new EndArgumentationMsg(x.ArgumentationResult));
            });
        }
    }
}
