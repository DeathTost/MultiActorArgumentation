using Akka.Actor;
using System;

namespace MultiActorArgumentation.Argumentation
{
    public class DefenderActor : ReceiveActor
    {
        public DefenderActor()
        {
            Console.WriteLine("Ready to Defend.");
        }
    }
}
