using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeNodeActor : ReceiveActor
    {
        public TreeNodeActor(int layerLeft)
        {
            System.Console.WriteLine(Context.Self.Path);
            Receive<string>((x) =>
            {
                layerLeft--;
                if (layerLeft < 0)
                {
                    System.Console.WriteLine($"{layerLeft} is lower than 0. No new childs.");
                    return;
                }
                var leftChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Left{layerLeft}");
                var rightChild = Context.ActorOf(Props.Create(() => new TreeNodeActor(layerLeft)), $"Right{layerLeft}");
                leftChild.Tell("Rise my child.");
                rightChild.Tell("Rise my child.");
            });
        }
    }
}
