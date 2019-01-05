using Akka.Actor;
using System;

namespace MultiActorArgumentation.Argumentation
{
    public class ProsecutorActor : ReceiveActor
    {
        public ProsecutorActor()
        {
            Console.WriteLine("Ready to Attack.");
        }
    }
}
