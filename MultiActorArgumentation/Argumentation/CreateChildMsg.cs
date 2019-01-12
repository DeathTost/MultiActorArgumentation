using Akka.Actor;
using System.Collections.Generic;

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
        public EndArgumentationMsg(string result)
        {
            ArgumentationResult = result;
        }

        public string ArgumentationResult { get; private set; }
    }

    public class RelatedArgumentsQueryMsg
    {
        public RelatedArgumentsQueryMsg(int argument, IActorRef querySender)
        {
            var list = new List<int>();
            list.Add(argument);
            BlacklistedArguments = list;
            QuerySender = querySender;
        }

        public RelatedArgumentsQueryMsg(IList<int> args, IActorRef querySender)
        {
            BlacklistedArguments = new List<int>(args);
            QuerySender = querySender;
        }

        public RelatedArgumentsQueryMsg AppendArgument(int argument)
        {
            var list = new List<int>(BlacklistedArguments);
            list.Add(argument);
            return new RelatedArgumentsQueryMsg(list, QuerySender);
        }

        public IReadOnlyList<int> BlacklistedArguments { get; private set; }
        public IActorRef QuerySender { get; private set; }
    }

    public class RelatedArgumentsDefenderResponseMsg
    {
        public RelatedArgumentsDefenderResponseMsg(List<int> argsList)
        {
            RelatedArguments = argsList;
        }
        public IReadOnlyList<int> RelatedArguments { get; private set; }
    }

    public class RelatedArgumentsProsecutorResponseMsg
    {
        public RelatedArgumentsProsecutorResponseMsg(List<int> argsList)
        {
            RelatedArguments = argsList;
        }
        public IReadOnlyList<int> RelatedArguments { get; private set; }
    }

    public class NodeResultMsg
    {
        public NodeResultMsg(int argument, bool active = true)
        {
            Argument = argument;
            Active = active;
        }
        public int Argument { get; private set; }
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
        public PredictParagraphsMsg(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
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
