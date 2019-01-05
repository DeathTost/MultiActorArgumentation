using Akka.Actor;
using System;

namespace MultiActorArgumentation.Argumentation
{
    public class JudgeActor : ReceiveActor
    {
        public JudgeActor()
        {
            ReceiveUserInput();
            StartArgumentation();
            EndArgumentation();
        }

        protected override void PreStart()
        {
            //DO SOMETHING - MAYBE START ARGUMENTATION TREE 
            var prosecutor = Context.ActorOf<ProsecutorActor>("ProsecutorActor");
            var defender = Context.ActorOf<DefenderActor>("DefenderActor");
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            //DO SOMETHING - WHAT TO DO IN CASE OF EXCEPTION
            //sample code
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromMinutes(1),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case NullReferenceException e:
                            return Directive.Restart;
                        case ArgumentException e:
                            return Directive.Resume;
                        case InvalidOperationException e:
                            return Directive.Stop;
                        default:
                            return Directive.Escalate;
                    }
                });
        }

        private void ReceiveUserInput()
        {
            Receive<UserInputMsg>((_) =>
            {
                Console.WriteLine("Judge has come!");
                Console.WriteLine("Please provide a path to a file with your case.");
                var argumentationPath = Console.ReadLine();

                if (System.IO.File.Exists(argumentationPath))
                {
                    Self.Tell(new StartArgumentationTreeMsg());
                }
                else
                {
                    Console.WriteLine("File not found! Please provide an existing file.");
                    Console.WriteLine("Judge has left!");
                    Self.Tell(new UserInputMsg());
                }
            });
        }

        private void StartArgumentation()
        {
            Receive<StartArgumentationTreeMsg>((_) =>
            {
                Console.WriteLine($"Judge: {Self.Path}");
                var root = Context.ActorOf(Props.Create(() => new TreeNodeActor(3)), "TreeRoot");
                root.Tell(new CreateChildMsg(""));
            });
        }

        private void EndArgumentation()
        {
            Receive<EndArgumentationMsg>((x) =>
            {
                Console.WriteLine($"Sender: {Sender.Path}");
                Console.WriteLine(x.ArgumentationResult);
                Console.WriteLine("Judge has left!");
                Console.WriteLine("Case is closed!");
            });
        }
    }
}
