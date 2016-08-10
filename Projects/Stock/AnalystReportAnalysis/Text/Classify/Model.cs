using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using LibSVMsharp;
using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using Text.Handler;
using Text.Classify.Item;

namespace Text.Classify
{
    public class Model
    {
        public SVMModel model;
        private WordSegHandler wsH;
        private List<FeatureItem> features;

        public Model()
        {
            this.model = null;
            this.wsH = new WordSegHandler();
            this.features = null;
        }

        public Model(string modelFile, string featureFilePath)
        {
            LoadModel(modelFile);
            this.wsH = new WordSegHandler();
            this.features = Feature.LoadChiFeature(featureFilePath);

        }

        //public Model(string fileName) { this.model = SVM.LoadModel(fileName); }

        public void LoadModel(string fileName)
        {
            this.model = SVM.LoadModel(fileName);
        }

        public void LoadModel(string modelFile, string featureFile)
        {
            this.model = SVM.LoadModel(modelFile);
            this.features = Feature.LoadChiFeature(featureFile);
        }

        public void SaveModel(string fileName)
        {
            SVM.SaveModel(this.model, fileName);
        }

        public void Train(string fileName)
        {
            // Load the datasets: In this example I use the same datasets for training and testing which is not suggested
            SVMProblem trainingSet = SVMProblemHelper.Load(fileName);
            // Normalize the datasets if you want: L2 Norm => x / ||x||
            trainingSet = trainingSet.Normalize(SVMNormType.L2);
            // Select the parameter set
            SVMParameter parameter = new SVMParameter();
            parameter.Type = SVMType.C_SVC;
            parameter.Kernel = SVMKernelType.LINEAR;
            parameter.C = 1;
            parameter.Gamma = 1;

            // Do cross validation to check this parameter set is correct for the dataset or not
            double[] crossValidationResults; // output labels
            int nFold = 5;
            trainingSet.CrossValidation(parameter, nFold, out crossValidationResults);

            // Evaluate the cross validation result
            // If it is not good enough, select the parameter set again
            double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);

            // Train the model, If your parameter set gives good result on cross validation
            this.model = trainingSet.Train(parameter);

            //return true;
        }

        public double[] Predicts(string fileName)
        {
            SVMProblem testSet = SVMProblemHelper.Load(fileName);
            double[] predictResults = testSet.Predict(model);
            return predictResults;
        }

        public double Predict(double[] featVector)
        {
            SVMNode[] vector = ConvertFeatVector(featVector);
            double predictResult = model.Predict(vector);
            return predictResult;
        }

        /// <summary>
        /// Get the input of a single sentence and return the feature vector extracted from the sentence
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public double Predict(string sentence)
        {
            double[] featVector = Feature.GetFeatureVec(sentence, ref wsH, ref features);
            double predictResult = Predict(featVector);
            return predictResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">content is the extracted result stored in MongoDB with field name 'content'</param>
        /// <returns></returns>
        public string[] GetPositiveCases(string content)
        {
            List<string> positiveCases = new List<string>();
            //content = content.Replace("\n", "。");//ignore all paragraph information
            //string[] sentences = content.Split('。');
            string[] sentences = TextPreProcess.SeparateParagraph(content);

            foreach (var sentence in sentences)
            {
                double predictResult = Predict(sentence);
                if (predictResult == 1)
                    positiveCases.Add(sentence);
            }
            return positiveCases.ToArray();
        }

        /// <summary>
        /// The index of SVMNode start from 0
        /// </summary>
        /// <param name="featVector"></param>
        /// <returns></returns>
        private SVMNode[] ConvertFeatVector(double[] featVector)
        {
            List<SVMNode> vector = new List<SVMNode>();
            
            int idx = 0;
            foreach (var featValue in featVector)
            {
                idx++;
                if (featValue == 0) { continue; }
                else { vector.Add(new SVMNode(idx, featValue)); }
            }
            return vector.ToArray();
        }

        //public static bool GenerateTrainSet(string rootPath)
        //{
        //    string rootSourcePath = ConfigurationManager.AppSettings["model_relate_root_dictionary"];

        //    TextPreProcess tPP = new TextPreProcess("FLI", rootSourcePath, true, false, true, false);
        //    string[] zhenglis = tPP.GetTrainDataOfZhengli();
        //    string[] fulis = tPP.GetTrainDataOfFuli();

        //    return RandomSelect.ExecuteSelectFuli("FLI", rootPath, zhenglis.Length - fulis.Length);
        //}
    }
}
