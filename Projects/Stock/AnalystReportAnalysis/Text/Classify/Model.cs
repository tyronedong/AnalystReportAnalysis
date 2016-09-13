using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
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

        public Model(string type)
        {
            string modelPath = GetPath(type, "model.txt");
            string featurePath = GetPath(type, "chi_feature.txt");

            LoadModel(modelPath);
            this.wsH = new WordSegHandler();
            this.features = Feature.LoadChiFeature(featurePath);
        }

        public Model(string modelPath, string featureFilePath)
        {
            LoadModel(modelPath);
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
        /// 在libsvm的基础上加入了一些规则来做出更好的前瞻性预测
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public double AdvancedPredict(string type, string sentence)
        {
            if(type.Equals("FLI"))
            {
                if (isNotFLI(sentence))
                    return -1;
            }
            else if(type.Equals("FLIIND"))
            {
                //if (isNotFLIIND(sentence))
                //    return 0;
                //if (isFLIIND(sentence))
                //    return 1;
            }
            //else if(type.Equals("NONINNOV"))
            //{

            //}
            //else if (type.Equals("INNOVEMO"))
            //{
            //    if (isPosINNOVEMO(sentence))
            //        return 1;
            //}

            double predictResult = Predict(sentence);

            return predictResult;
        }

        private bool isFLIIND(string sentence)//是否是行业层面的信息
        {
            if (sentence.Contains("行业"))
                return true;
            return false;
        }

        private bool isNotFLIIND(string sentence)//是否是公司层面的信息
        {
            if (sentence.Contains("公司"))
                return true;
            return false;
        }

        //private bool isFLI(string sentence)
        //{
        //}

        private bool isPosINNOVEMO(string sentence)
        {
            Regex posWords = new Regex(@"看好|高效|推进|有望|坚挺|显著");
            Regex posWords2 = new Regex(@"如虎添翼|有条不紊|稳中有升");
            if (posWords.IsMatch(sentence) || posWords2.IsMatch(sentence))
                return true; 

            return false;
        }

        private bool isNotFLI(string sentence)
        {
            Regex likeButNotZhengli1 = new Regex(@"(每股收益|EPS|盈利预测|盈余预测)");
            Regex likeButNotZhengli2 = new Regex(@"((符合|未达|接近|超出|低于)(我们(的?))?预期|按预期完成|与预期一致)");

            if (likeButNotZhengli1.IsMatch(sentence)) //规则一
            {
                //string tempSen = sentence;
                //foreach (Match match in likeButNotZhengli1.Matches(sentence))
                //{ tempSen = tempSen.Replace(match.Value, ""); }
                return true;
            }
            if (likeButNotZhengli2.IsMatch(sentence))//规则二
            {
                string tempSen = sentence;
                foreach (Match match in likeButNotZhengli2.Matches(sentence))
                { tempSen = tempSen.Replace(match.Value, ""); }
                if (Predict(tempSen) == -1)
                    return true;
            }

            if (sentence.Contains("将"))//规则三
            {
                var segResult = this.wsH.GetSegmentation(sentence);
                bool hasJiangScanned = false;
                foreach (var partWordPair in segResult)
                {
                    if (hasJiangScanned)//上一个词是将
                    {
                        if (partWordPair.Key.StartsWith("n") || partWordPair.Key.StartsWith("m"))//“将”后面跟着名词或数词，则“将”不作为前瞻性的判定词
                        {
                            if (Predict(sentence.Replace("将", "")) == -1)//去除“将”后被判定为非前瞻性，则确实为非前瞻性
                                return true;
                        }
                        hasJiangScanned = false;
                    }

                    if(partWordPair.Value.Equals("将"))
                    { hasJiangScanned = true; }
                }
            }

            //三个规则都无法判定是负例，则返回false
            return false;
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

        private static string GetPath(string type, string fileName)
        {
            string rootForChi;

            if (type.Contains("INNOV"))
            {
                rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            }
            else if (type.Contains("FLI"))
            {
                rootForChi = ConfigurationManager.AppSettings["excel_foresight_root_dictionary"];
            }
            else
            {
                Trace.TraceError("Model.GetPath():type error");
                return null;
            }

            return Path.Combine(rootForChi, Path.Combine(type, fileName));
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
