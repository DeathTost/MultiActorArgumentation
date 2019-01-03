using Akka.Actor;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Nlp
{
    public class DocumentProcessorActor : ReceiveActor
    {
        public DocumentProcessorActor()
        {
            Receive<string>((x) => 
            {
                //requires installing pythonnet in Python site-packages (pip install pythonnet)
                //Debugger needs to be set to x64
                //Py.Import works weird, temporary solution - copy your python files to bin/debug folder because file needs to be in the same place as .exe and .dlls
                //converter python to .Net is working so current idea:
                // 1. get path to file in C#
                // 2. run python script with NLP and training model
                // 3. return model to C# (only for storage, we will pass it to python when it is needed)
                // 4. get user input in c# and run python again to classify our arguments
                // 5. get the results from python and convert them to C# for better interpretation
                // that means that we are working on data only in python, but input and output is processed in C#
                using (Py.GIL())
                {
                    try
                    {
                        dynamic a = Py.Import("PythonExample");
                        PyObject result = a.methodA();

                        var converter = new PyConverter();
                        converter.AddListType();
                        converter.Add(new Int32Type());
                        converter.Add(new Int64Type());
                        converter.Add(new StringType());
                        converter.AddDictType<string, object>();

                        List<object> answer = (List<object>)converter.ToClr(result);
                        Console.WriteLine((answer.First() as Dictionary<string, object>).First().Key);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
        }

        /*
        private readonly string jarRoot = "...\\...\\...\\stanford-corenlp-full-2017-06-09";
        public DocumentProcessorActor()
        {
            Receive<string>((x) =>
            {
                // Trzeba pobrać z ich strony plik stanford-corenlp-full-2017-06-09.zip
                // po rozpakowaniu trzeba rozpakować folder z models.jar bo inaczej nie znajduje ścieżki
                var modelsDirectory = jarRoot + "\\edu\\stanford\\nlp\\models";
                var lp = LexicalizedParser.loadModel(modelsDirectory + "\\lexparser\\englishPCFG.ser.gz");

                var text = "John has a white dog and a blue baloon.";
                var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
                var textReader = new StringReader(text);
                var rawWords = tokenizerFactory.getTokenizer(textReader).tokenize();

                var ngrams = GetNGramsWithPositions(rawWords, 1, 2);
                foreach (var word in rawWords.toArray())
                {
                    System.Console.WriteLine(word);
                }
                textReader.close();
                var tree = lp.apply(rawWords);

                var tlp = new PennTreebankLanguagePack();
                var gsf = tlp.grammaticalStructureFactory();
                var gs = gsf.newGrammaticalStructure(tree);
                var tdl = gs.typedDependenciesCCprocessed();
                System.Console.WriteLine($"{tdl}");

                var tp = new TreePrint("penn,typedDependenciesCollapsed");
                tp.printTree(tree);
            });
        }

        public List<java.util.List> GetNGramsWithPositions(java.util.List items, int minSize, int maxSize)
        {
            List<java.util.List> ngrams = new List<java.util.List>();
            int listSize = items.size();
            for (int i = 0; i < listSize; ++i)
            {
                for (int ngramSize = minSize; ngramSize <= maxSize; ++ngramSize)
                {
                    if (i + ngramSize <= listSize)
                    {
                        java.util.List ngram = new java.util.ArrayList();
                        for (int j = i; j < i + ngramSize; ++j)
                        {
                            ngram.add(items.get(j).ToString());
                        }
                        ngram.add(i.ToString());
                        ngrams.Add(ngram);
                    }
                }
            }
            return ngrams;
        }

        public void Tokenize()
        {
            var modelsDirectory = jarRoot + "\\edu\\stanford\\nlp\\models";
            var curDir = System.Environment.CurrentDirectory;
            System.IO.Directory.SetCurrentDirectory(jarRoot);

            Properties properties = new Properties();
            properties.setProperty("annotators", "tokenize,ssplit,pos,lemma");
            StanfordCoreNLP pipeline = new StanfordCoreNLP(properties);
            System.IO.Directory.SetCurrentDirectory(curDir);
            Annotation tokenAnnotation = new Annotation("Disability is to be able to achieve the greatest in life");
            pipeline.annotate(tokenAnnotation);

            using (var stream = new ByteArrayOutputStream())
            {
                pipeline.prettyPrint(tokenAnnotation, new PrintWriter(stream));
                System.Console.WriteLine(stream.toString());
                stream.close();
            }
        }
        */
    }
}
