using Akka.Actor;
using MultiActorArgumentation.Argumentation;
using MultiActorArgumentation.Nlp;
using Python.Runtime;

namespace MultiActorArgumentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var argumentationSystem = ActorSystem.Create("ArgumentationSystem"))
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                var judge = argumentationSystem.ActorOf(Props.Create(() => new JudgeActor()), "Judge");
                var docProcessor = argumentationSystem.ActorOf(Props.Create(() => new DocumentProcessorActor(judge)), "DocProcessor");

                System.Console.WriteLine("Choose prediction model <rf_model>|<bayes_model>|<svm_model>:");
                string modelName = System.Console.ReadLine();
                docProcessor.Tell(new LoadModelMsg(modelName));//"rf_model"
                argumentationSystem.WhenTerminated.Wait();
            }
        }
    }
}
