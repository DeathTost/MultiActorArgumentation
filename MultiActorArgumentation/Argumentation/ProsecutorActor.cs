using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using Python.Runtime;

namespace MultiActorArgumentation.Argumentation
{
    public class ProsecutorActor : ReceiveActor
    {
        private IList<int> ProsArgs = new List<int>(new int[] { 1, 4, 5, 6 });
        private double threshold = 0.5;
        private IList<string> negativeParagraphs;

        public ProsecutorActor(IReadOnlyList<object> paragraphs)
        {
            MapParagraphs(paragraphs);
            Self.Tell(new RelatedArgumentsQueryMsg(1, Context.Parent));
            TellRelatedParagraphs();
        }

        private void MapParagraphs(IReadOnlyList<object> paragraphs)
        {
            negativeParagraphs = new List<string>();
            foreach (var paragraph in paragraphs)
            {
                negativeParagraphs.Add((string)paragraph);
            }
        }

        private void TellRelatedParagraphs()
        {
            Receive<RelatedArgumentsQueryMsg>((x) =>
            {
                //JUST MOCKUP, BLACKLIST ARGUMENTS NEED TO CHANGE TYPE FROM INT TO STRING
                using (Py.GIL())
                {
                    dynamic pythonDataProcessing = Py.Import("data_processing");
                    var converter = new PyConverter();
                    converter.AddListType();
                    converter.Add(new DoubleType());
                    var argument = negativeParagraphs.First();//x.BlacklistedArguments.First()
                    var paragraphsLeft = negativeParagraphs.Except(negativeParagraphs.Where(y => y.Contains("month")).ToList()/*x.BlacklistedArguments*/).ToList();
                    var resultList = new List<string>();
                    foreach (var paragraph in paragraphsLeft)
                    {
                        
                        var ngramEvaluation = pythonDataProcessing.evaluate_sentence(argument, paragraph, 2);
                        List<object> ngramScores = converter.ToClr(ngramEvaluation);
                        var value = 0.0;
                        foreach (var score in ngramScores)
                        {
                            value += (double)score;
                        }
                        if (value > threshold)
                        {
                            resultList.Add(paragraph);
                        }
                    }
                    
                    x.QuerySender.Tell(new RelatedArgumentsProsecutorResponseMsg(ProsArgs.Where((e) => !x.BlacklistedArguments.Contains(e)).ToList()));
                }      
            });
        }
    }
}
