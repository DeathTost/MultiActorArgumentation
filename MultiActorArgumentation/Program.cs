using Akka.Actor;
using MultiActorArgumentation.Argumentation;
using MultiActorArgumentation.Nlp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiActorArgumentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var argumentationSystem = ActorSystem.Create("ArgumentationSystem"))
            {
                var docProcessor = argumentationSystem.ActorOf(Props.Create(() => new DocumentProcessorActor()));
                var judge = argumentationSystem.ActorOf(Props.Create(() => new JudgeActor()));

                docProcessor.Tell("message");

                argumentationSystem.WhenTerminated.Wait();
            }
        }
    }
}
