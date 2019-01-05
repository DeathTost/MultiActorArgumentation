using Akka.Actor;
using MultiActorArgumentation.Argumentation;
using MultiActorArgumentation.Nlp;

namespace MultiActorArgumentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var argumentationSystem = ActorSystem.Create("ArgumentationSystem"))
            {
                var docProcessor = argumentationSystem.ActorOf(Props.Create(() => new DocumentProcessorActor()), "DocProcessor");
                var judge = argumentationSystem.ActorOf(Props.Create(() => new JudgeActor()), "Judge");

                //docProcessor.Tell("message");
                //judge.Tell(new CreateChildMsg("SomeMessage"));
                judge.Tell(new UserInputMsg());
                argumentationSystem.WhenTerminated.Wait();
            }
        }
    }
}
