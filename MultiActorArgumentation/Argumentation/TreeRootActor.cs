using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace MultiActorArgumentation.Argumentation
{
    public class TreeRootActor : ReceiveActor
    {
        private IReadOnlyList<string> caseArguments;
        private IDictionary<string, bool> resolvedCases = new Dictionary<string, bool>();
        private IDictionary<string, bool> finishedCases = new Dictionary<string, bool>();
        private IList<IActorRef> caseBranches = new List<IActorRef>();
        private int counter = 0;
        private bool isFinished = false;

        public TreeRootActor(IReadOnlyList<string> arguments)
        {
            caseArguments = arguments;
            this.SetReceiveTimeout(TimeSpan.FromSeconds(1000));
            StartArgumentation();
            ForwardArguments();
            EndArgumentation();
        }


        private void StartArgumentation()
        {
            Receive<StartArgumentationTreeMsg>((_) =>
            {
                int i = 0;
                foreach (var argument in caseArguments)
                {
                    caseBranches.Add(Context.ActorOf(Props.Create(() => new TreeNodeActor(argument, -1, 3)), "case"+i));
                    i++;
                    resolvedCases.Add(argument, false);
                    finishedCases.Add(argument, false);
                }
            });
        }

        private void ForwardArguments()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                Context.Parent.Tell(new RelatedArgumentsQueryMsg(new List<string>(x.BlacklistedArguments), x.QuerySender));
            });
        }

        private void EndArgumentation()
        {
            Receive<NodeResultMsg>((x) =>
            {
                if (!string.IsNullOrEmpty(x.Argument))
                {
                    resolvedCases[x.Argument] = x.Active;
                    finishedCases[x.Argument] = true;
                }
                counter++;

                if (!isFinished && counter >= resolvedCases.Count)
                {
                    Context.Parent.Tell(new EndArgumentationMsg(new Dictionary<string, bool>(resolvedCases), new Dictionary<string, bool>(finishedCases)));
                    isFinished = true;
                }
            });

            Receive<ReceiveTimeout>((x) =>
            {
                if (counter >= resolvedCases.Count)
                {
                    if (!isFinished)
                    {
                        Self.Tell(new NodeResultMsg("", true));
                    }
                    else
                    {
                        return;
                    }
                }
                counter++;
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromMinutes(1),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ActorKilledException e:
                            Console.WriteLine($"One of Root children was killed. Exception: {e.Message}");
                            return Directive.Stop;
                        default:
                            return Directive.Escalate;
                    }
                });
        }
    }
}
