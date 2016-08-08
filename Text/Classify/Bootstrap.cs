using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using Text.Classify.Item;

namespace Text.Classify
{
    class Bootstrap
    {
        static string rootFeaturePath = ConfigurationManager.AppSettings["feature_relate_root_dictionary"];
        static string rootModelPath = ConfigurationManager.AppSettings["model_relate_root_dictionary"];

        static string modelPath = ConfigurationManager.AppSettings["model_path"];
        static string featurePath = ConfigurationManager.AppSettings["chi_feature_filename"];
        //static string resultRootDic = ConfigurationManager.AppSettings["result_file_root_dictionary"];




        /// <summary>
        /// </summary>
        /// <param name="howManySampleEachClass"></param>
        /// <returns></returns>
        static bool GenerateNewChiFeatureSource(int howManySampleEachClass)
        {
            TextPreProcess tPP = new TextPreProcess("FLI", rootModelPath, true, false, true, false);

            //获取人工标注的excel中的前瞻性语句
            string[] zhengli = tPP.GetTrainDataOfZhengli();
            if (howManySampleEachClass < zhengli.Length) 
            { return false; }
            //通过训练好的分类器选择新的前瞻性语句
            if (!RandomSelect.ExecuteSelectZhengli(rootFeaturePath, howManySampleEachClass - zhengli.Length, modelPath, featurePath))
            { return false; }

            //获取人工标注的excel中的非前瞻性语句
            string[] fuli = tPP.GetTrainDataOfFuli();
            if (!RandomSelect.ExecuteSelectFuli("FLI", rootFeaturePath, howManySampleEachClass - fuli.Length))
            { return false; }
            
            return true;
        }

        public static bool ExecuteBootstrap(int bootScale)
        {
            string featurePath = ConfigurationManager.AppSettings["chi_feature_path"];

            bool b1 = false, b2 = false;
            if(!GenerateNewChiFeatureSource(bootScale))
            { Trace.TraceError("Bootstrap.ExecuteBootstrap(int bootScale): GenerateNewChiFeatureSource failed"); return false; }
            b2 = Feature.ExtractAndStoreChiFeature("FLI", featurePath);

            return b2;
        }
    }
}
