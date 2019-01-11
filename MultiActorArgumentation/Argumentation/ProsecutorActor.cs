using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Argumentation
{
    public class ProsecutorActor : ReceiveActor
    {
        private IList<int> ProsArgs = new List<int>(new int[] { 1, 4, 5, 6 });
        public ProsecutorActor()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                x.QuerySender.Tell(new RelatedArgumentsProsecutorResponseMsg(ProsArgs.Where((e) => !x.BlacklistedArguments.Contains(e)).ToList()));
            });
        }
    }
}
