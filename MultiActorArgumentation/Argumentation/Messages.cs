using Akka.Actor;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MultiActorArgumentation.Argumentation
{
    public class CreateChildMsg
    {
        public CreateChildMsg(string name)
        {
            ChildName = name;
        }

        public string ChildName { get; private set; }
    }

    public class UserInputMsg
    {
        public UserInputMsg()
        {

        }
    }

    public class StartArgumentationTreeMsg
    {
        public StartArgumentationTreeMsg()
        {
        }
    }

    public class EndArgumentationMsg
    {
        public EndArgumentationMsg(IReadOnlyDictionary<string, bool> resolved, IReadOnlyDictionary<string, bool> finished)
        {
            ResolvedCases = resolved;
            FinishedCases = finished;
        }

        public IReadOnlyDictionary<string, bool> ResolvedCases { get; private set; }
        public IReadOnlyDictionary<string, bool> FinishedCases { get; private set; }
    }

    public class RelatedArgumentsQueryMsg
    {
        public RelatedArgumentsQueryMsg(string argument, IActorRef querySender)
        {
            var list = new List<string>();
            list.Add(argument);
            BlacklistedArguments = list;
            QuerySender = querySender;
        }

        public RelatedArgumentsQueryMsg(IList<string> args, IActorRef querySender)
        {
            BlacklistedArguments = new List<string>(args);
            QuerySender = querySender;
        }

        public RelatedArgumentsQueryMsg AppendArgument(string argument)
        {
            var list = new List<string>(BlacklistedArguments);
            list.Add(argument);
            return new RelatedArgumentsQueryMsg(list, QuerySender);
        }

        public IReadOnlyList<string> BlacklistedArguments { get; private set; }
        public IActorRef QuerySender { get; private set; }
    }

    public class RelatedArgumentsDefenderResponseMsg
    {
        public RelatedArgumentsDefenderResponseMsg(List<string> argsList)
        {
            RelatedArguments = argsList;
        }
        public IReadOnlyList<string> RelatedArguments { get; private set; }
    }

    public class RelatedArgumentsProsecutorResponseMsg
    {
        public RelatedArgumentsProsecutorResponseMsg(List<string> argsList)
        {
            RelatedArguments = argsList;
        }
        public IReadOnlyList<string> RelatedArguments { get; private set; }
    }

    public class NodeResultMsg
    {
        public NodeResultMsg(string argument, bool active = true)
        {
            Argument = argument;
            Active = active;
        }
        public string Argument { get; private set; }
        public bool Active { get; private set; }
    }

    public class LoadModelMsg
    {
        public LoadModelMsg(string modelName, string fileName = "Poland_Penal_Code.pdf", string trainingDataName = "paragraphs_labeled.csv")
        {
            ModelName = modelName;
            FileName = fileName;
            TrainingDataName = trainingDataName;
        }

        public string ModelName { get; private set; }
        public string FileName { get; private set; }
        public string TrainingDataName { get; private set; }
    }

    public class PredictParagraphsMsg
    {
        public PredictParagraphsMsg(string fileName, string trainingDataName)
        {
            FileName = fileName;
            TrainingDataName = trainingDataName;
        }

        public string FileName { get; private set; }
        public string TrainingDataName { get; private set; }
    }

    public class ReturnParagraphsMsg
    {
        public ReturnParagraphsMsg(IReadOnlyList<object> positives, IReadOnlyList<object> negatives)
        {
            PositiveParagraphs = positives;
            NegativeParagraphs = negatives;
        }

        public IReadOnlyList<object> PositiveParagraphs { get; private set; }
        public IReadOnlyList<object> NegativeParagraphs { get; private set; }
    }
}
