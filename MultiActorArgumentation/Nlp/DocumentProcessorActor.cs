using Akka.Actor;
using edu.stanford.nlp.ling;
using java.io;
using java.util;
using edu.stanford.nlp.tagger;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.process;
using edu.stanford.nlp.trees;
using System.Collections.Generic;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;

namespace MultiActorArgumentation.Nlp
{
    public class DocumentProcessorActor : ReceiveActor
    {
        public DocumentProcessorActor()
        {
            Receive<string>((x) =>
            {
                // Trzeba pobrać z ich strony plik stanford-corenlp-full-2017-06-09.zip
                // po rozpakowaniu trzeba rozpakować folder z models.jar bo inaczej nie znajduje ścieżki
                var jarRoot = "...\\...\\...\\stanford-corenlp-full-2017-06-09";
                var modelsDirectory = jarRoot + "\\edu\\stanford\\nlp\\models";
                var lp = LexicalizedParser.loadModel(modelsDirectory + "\\lexparser\\englishPCFG.ser.gz");

                var text = "John has a white dog and a blue baloon.";
                var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
                var textReader = new StringReader(text);
                var rawWords = tokenizerFactory.getTokenizer(textReader).tokenize();

                var ngrams = GetNGramsPositions(rawWords, 1, 2);
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

        public List<java.util.List> GetNGramsPositions(java.util.List items, int minSize, int maxSize)
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
            var jarRoot = "...\\...\\...\\stanford-corenlp-full-2017-06-09";
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

    }
}
