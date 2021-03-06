﻿using Akka.Actor;
using MultiActorArgumentation.Argumentation;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiActorArgumentation.Nlp
{
    public class DocumentProcessorActor : ReceiveActor
    {
        private PyObject trainedModel;
        private IActorRef judge;

        public DocumentProcessorActor(IActorRef actor)
        {
            judge = actor;
            LoadOrCreateModel();
            PredictParagraphs();
        }

        private void PredictParagraphs()
        {
            Receive<PredictParagraphsMsg>((x) =>
            {
                using (Py.GIL())
                {
                    var path = System.IO.Path.GetFullPath("...\\...\\...\\Nlp\\");
                    dynamic pythonDataLoader = Py.Import("data_loader");
                    var paragraphs = pythonDataLoader.split_into_paragraphs(pythonDataLoader.read_pdf_to_text(path + x.FileName));
                    dynamic pythonDataClassification = Py.Import("data_classifiers");

                    var converter = new PyConverter();
                    converter.AddListType();
                    converter.Add(new Int32Type());
                    converter.Add(new Int64Type());
                    converter.Add(new DoubleType());
                    converter.Add(new FloatType());
                    converter.Add(new StringType());
                    var predicted_paragraphs = pythonDataClassification.predict_arguments(trainedModel, paragraphs);
                    var testLabels = pythonDataLoader.read_labels_of_paragraphs(path + x.TrainingDataName);
                    var predictedLabels = pythonDataLoader.get_labels(predicted_paragraphs);
                    pythonDataClassification.display_metrics(testLabels, predictedLabels);
                    List<object> positive_paragraphs = converter.ToClr(pythonDataClassification.get_positive_paragraphs(predicted_paragraphs));
                    List<object> negative_paragraphs = converter.ToClr(pythonDataClassification.get_negative_paragraphs(predicted_paragraphs));
                   
                    judge.Tell(new ReturnParagraphsMsg(positive_paragraphs, negative_paragraphs));
                }
            });
        }

        private void LoadOrCreateModel()
        {
            Receive<LoadModelMsg>((x) =>
            {
                if (string.IsNullOrEmpty(x.FileName))
                {
                    Console.WriteLine("No legislation file name given.");
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
                        Console.WriteLine($"{x.ModelName} loaded");
                        trainedModel = pythonDataLoader.load_model(path + x.ModelName);
                        Self.Tell(new PredictParagraphsMsg(x.FileName, x.TrainingDataName));
                    }
                    else
                    {
                        if (System.IO.File.Exists(path + x.TrainingDataName))
                        {
                            var trainingData = pythonDataLoader.randomize_training_paragraphs(path + x.TrainingDataName, 60, 20, 60);
                            var trainingText = pythonDataLoader.get_training_data(trainingData);
                            var labels = pythonDataLoader.get_labels(trainingData);

                            if (x.ModelName == "svm_model")
                            {
                                Console.WriteLine("SVM model chosen");
                                trainedModel = pythonDataClassification.build_SVM(trainingText, labels);            
                            }
                            else if (x.ModelName == "bayes_model")
                            {
                                Console.WriteLine("NaiveBayes model chosen");
                                trainedModel = pythonDataClassification.build_MultinomialNB(trainingText, labels);
                            }
                            else if (x.ModelName == "rf_model")
                            {
                                Console.WriteLine("RandomForest model chosen");
                                trainedModel = pythonDataClassification.build_RandomForest(trainingText, labels);
                            }
                            else
                            {
                                Console.WriteLine("Default model (RandomForest) built");
                                trainedModel = pythonDataClassification.build_RandomForest(trainingText, labels);
                            }
                            pythonDataLoader.save_model(trainedModel, path + x.ModelName);
                            Self.Tell(new PredictParagraphsMsg(x.FileName, x.TrainingDataName));
                        }
                        else
                        {
                            Console.WriteLine("No training data found.");
                        }
                    }
                }
            });
        }
    }
}
