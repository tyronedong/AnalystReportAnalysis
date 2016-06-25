using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;

namespace Text.Classify
{
    class Model
    {
        public SVMModel model;

        public Model() { this.model = null; }

        //public Model(string fileName) { this.model = SVM.LoadModel(fileName); }

        public void LoadModel(string fileName)
        {
            this.model = SVM.LoadModel(fileName);
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

        public double[] Predict(string fileName)
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
    }
}
