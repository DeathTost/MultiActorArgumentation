using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiActorArgumentation.Argumentation
{
    public class JudgeActor : ReceiveActor
    {
        public JudgeActor()
        {
            Receive<CreateChildMsg>((x) =>
            {
                var prosecutor = Context.ActorOf<ProsecutorActor>("ProsecutorActor");
                var defender = Context.ActorOf<DefenderActor>("DefenderActor");
                var root = Context.ActorOf(Props.Create(() => new TreeNodeActor(3)), "TreeRoot");
                root.Tell("Create tree");
            });
        }

        protected override void PreStart()
        {
            //DO SOMETHING - MAYBE START ARGUMENTATION TREE 
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            //DO SOMETHING - WHAT TO DO IN CASE OF EXCEPTION
            return base.SupervisorStrategy();
        }
    }
}
