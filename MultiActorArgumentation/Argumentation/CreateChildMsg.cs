
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
            BlacklistedArguments = new List<int>();
            BlacklistedArguments.Add(argument);
            QuerySender = querySender;
        }

        public void AppendArgument(int argument)
        {
            BlacklistedArguments.Add(argument);
        }

        public List<int> BlacklistedArguments { get; private set; }
        public IActorRef QuerySender { get; private set; }
    }

    public class RelatedArgumentsDefenderResponseMsg
    {
        public RelatedArgumentsDefenderResponseMsg(List<int> argsList)
        {
            RelatedArguments = argsList;
        }
        public List<int> RelatedArguments { get; private set; }
    }

    public class RelatedArgumentsProsecutorResponseMsg
    {
        public RelatedArgumentsProsecutorResponseMsg(List<int> argsList)
        {
            RelatedArguments = argsList;
        }
        public List<int> RelatedArguments { get; private set; }
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
}
