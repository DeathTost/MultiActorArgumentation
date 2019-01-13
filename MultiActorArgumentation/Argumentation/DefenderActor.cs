using Akka.Actor;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Argumentation
{
    public class DefenderActor : ReceiveActor
    {
        private IList<string> positiveParagraphs;
        private double threshold = 0.05;

        public DefenderActor(IReadOnlyList<object> paragraphs)
        {
            MapParagraphs(paragraphs);
            TellRelatedParagraphs();
        }

        private void MapParagraphs(IReadOnlyList<object> paragraphs)
        {
            positiveParagraphs = new List<string>();
            foreach (var paragraph in paragraphs)
            {
                positiveParagraphs.Add((string)paragraph);
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
                    var argument = x.BlacklistedArguments.First();
                    var paragraphsLeft = positiveParagraphs.Except(x.BlacklistedArguments).ToList();
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
                            threshold = value;
                            resultList.Add(paragraph);
                        }
                        if (resultList.Count > 5)
                        {
                            break;
                        }
                    }
                    x.QuerySender.Tell(new RelatedArgumentsDefenderResponseMsg(resultList));
                }
            });
        }

    }
}
