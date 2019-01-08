using Akka.Actor;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Nlp
{
    public class DocumentProcessorActor : ReceiveActor
    {
        private PyObject trainedModel;

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
                        dynamic a = Py.Import("data_loader");
                        var path = System.IO.Path.GetFullPath("...\\...\\...\\Nlp\\Poland_Penal_Code.pdf");
                        PyObject result = a.read_pdf_to_text(path);

                        var converter = new PyConverter();
                        converter.AddListType();
                        converter.Add(new Int32Type());
                        converter.Add(new Int64Type());
                        converter.Add(new StringType());
                        converter.AddDictType<string, object>();

                        string answer = (string)converter.ToClr(result);
                        Console.WriteLine(answer);
                        //Console.WriteLine((answer.First() as Dictionary<string, object>).First().Key);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Self.Tell(new LoadModelMsg("rf_model", "Poland_Penal_Code.pdf"));
            });
            LoadModel();
        }

        private void LoadModel()
        {
            Receive<LoadModelMsg>((x) =>
            {
                if (string.IsNullOrEmpty(x.FileName))
                {
                    Console.WriteLine("Empty file name.");
                    return;
                }
                using (Py.GIL())
                {
                    var path = System.IO.Path.GetFullPath("...\\...\\...\\Nlp\\");
                    dynamic pythonDataLoader = Py.Import("data_loader");
                    dynamic pythonDataProcessing = Py.Import("data_processing");
                    dynamic pythonDataClassification = Py.Import("data_classifiers");
                    if (System.IO.File.Exists(path + x.ModelName))
                    {
                        trainedModel = pythonDataLoader.load_model(path + x.ModelName);
                    }
                    else
                    {
                        if (System.IO.File.Exists(path + x.FileName))
                        {
                            PyObject data = pythonDataLoader.read_pdf_to_text(path + x.FileName);
                            trainedModel = pythonDataClassification.build_RandomForest(pythonDataProcessing.sentence_tokenization(data), pythonDataClassification.encode_labels(pythonDataProcessing.sentence_tokenization(data)));
                            pythonDataLoader.save_model(trainedModel, path + x.ModelName);
                        }
                        else
                        {
                            Console.WriteLine("No file found.");
                        }
                    }
                }
            });
        }
    }

    public class LoadModelMsg
    {
        public LoadModelMsg(string modelName, string fileName)
        {
            ModelName = modelName;
            FileName = fileName;
        }

        public string ModelName { get; private set; }
        public string FileName { get; private set; }
    }
}
