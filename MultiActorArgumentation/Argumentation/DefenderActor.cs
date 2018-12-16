using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
