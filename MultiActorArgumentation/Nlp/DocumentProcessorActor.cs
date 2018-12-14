using Akka.Actor;
using edu.stanford.nlp.ling;
using java.io;
using java.util;
using edu.stanford.nlp.tagger;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.process;
using edu.stanford.nlp.trees;

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
    }
}
