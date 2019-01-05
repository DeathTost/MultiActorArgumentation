
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
}
