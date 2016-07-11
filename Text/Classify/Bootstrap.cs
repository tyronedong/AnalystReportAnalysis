using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Text.Classify.Item;

namespace Text.Classify
{
    class Bootstrap
    {
        static string modelPath = ConfigurationManager.AppSettings["model_path"];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="howManySampleEachClass"></param>
        /// <returns></returns>
        static List<LabeledItem> GenerateNewChiFeatureSource(int howManySampleEachClass)
        {
            //获取人工标注的excel中的前瞻性语句
            string[] zhengli = TextPreProcess.GetTrainDataOfZhengli();
            if (howManySampleEachClass < zhengli.Length) { return null; }

            //通过训练好的分类器选择新的前瞻性语句
            if (!RandomSelect.ExecuteSelectZhengli(howManySampleEachClass - zhengli.Length, modelPath)) return null;
            

            return null;
        }
    }
}
