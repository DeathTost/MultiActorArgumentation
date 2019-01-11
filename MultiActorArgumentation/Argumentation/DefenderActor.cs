using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Argumentation
{
    public class DefenderActor : ReceiveActor
    {
        private IList<int> DefArgs = new List<int>(new int[] { -1, -2, -3, -4 });
        public DefenderActor()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                x.QuerySender.Tell(new RelatedArgumentsDefenderResponseMsg(DefArgs.Where((e) => !x.BlacklistedArguments.Contains(e)).ToList()));
            });
        }
    }
}
