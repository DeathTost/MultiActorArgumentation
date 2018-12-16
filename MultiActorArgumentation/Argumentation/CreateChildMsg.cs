using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
