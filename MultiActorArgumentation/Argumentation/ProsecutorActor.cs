using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using Python.Runtime;

namespace MultiActorArgumentation.Argumentation
{
    public class ProsecutorActor : ReceiveActor
    {
        private double threshold;
        private IList<string> negativeParagraphs;

        public ProsecutorActor(IReadOnlyList<object> paragraphs)
        {
            MapParagraphs(paragraphs);
            Self.Tell(new RelatedArgumentsQueryMsg(new List<string> { "dummy" }, Self));
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
                using (Py.GIL())
                {
                    threshold = 0.05;
                    dynamic pythonDataProcessing = Py.Import("data_processing");
                    var converter = new PyConverter();
                    converter.AddListType();
                    converter.Add(new DoubleType());
                    var argument = x.BlacklistedArguments.First();
                    var paragraphsLeft = negativeParagraphs.Except(x.BlacklistedArguments).ToList();
                    var resultList = new List<string>();
					var valueMap = new Dictionary<int, Double>();
					foreach (var paragraph in paragraphsLeft)
					{
						var ngramEvaluation = pythonDataProcessing.evaluate_sentence(argument, paragraph, 2);
						List<object> ngramScores = converter.ToClr(ngramEvaluation);
						var value = 0.0;
						foreach (var score in ngramScores)
						{
							value += (double)score;
						}
						valueMap.Add(valueMap.Count, value);
					}
					Dictionary<int, Double> sortedValueMap = (valueMap.OrderByDescending(y => y.Value)).ToDictionary(y => y.Key, y => y.Value);

					foreach (var val in sortedValueMap)
					{
						if (val.Value > threshold)
							resultList.Add(paragraphsLeft.ElementAt(val.Key));
						else
							break;
						if (resultList.Count > 1)
						{
							break;
						}
					}

					x.QuerySender.Tell(new RelatedArgumentsProsecutorResponseMsg(resultList));
                }      
            });
        }
    }
}
