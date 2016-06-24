using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Text.Handler;
using Text.Classify.Item;

namespace Text.Classify
{
    class TextPreProcess
    {
        /// <summary>
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        public static List<LabeledItem> GetLabeledItems()
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            //get all rough zhengli and fuli
            string[] zhengliCol = GetTrainDataOfZhengli();
            string[] fuliCol = GetTrainDataOfFuli();

            //normalize all zhengli and fuli
            string[] zhengli = NormalizeTrainData(zhengliCol);
            string[] fuli = NormalizeTrainData(fuliCol);

            //set two global variables
            LabeledItem.numberOfZhengli = zhengli.Length;
            LabeledItem.numberOfFuli = fuli.Length;

            WordSegHandler wsH = new WordSegHandler();
            if (!wsH.isValid) { Trace.TraceError("Text.Program.GenerateTrainDataFile() goes wrong"); return null; }
            else
            {
                foreach (var item in zhengli)
                {
                    wsH.ExecutePartition(item);
                    string[] segResult = wsH.GetNoStopWords();
                    labeledItems.Add(new LabeledItem(1, item, segResult));
                }
                foreach (var item in fuli)
                {
                    wsH.ExecutePartition(item);
                    string[] segResult = wsH.GetNoStopWords();
                    labeledItems.Add(new LabeledItem(-1, item, segResult));
                }
            }
            return labeledItems;
        }

        static string[] GetTrainDataOfZhengli()
        {
            string path = @"F:\事们\进行中\分析师报告\数据标注\FLI信息提取-样本.xlsx";
            ExcelHandler exlH = new ExcelHandler(path);
            string[] zhengliCol = exlH.GetColoum("sheet1", 2);
            //exlH.Destroy();
            return zhengliCol;
        }

        static string[] GetTrainDataOfFuli()
        {
            string path = @"F:\事们\进行中\分析师报告\数据标注\FLI信息提取-样本.xlsx";
            ExcelHandler exlH = new ExcelHandler(path);
            string[] fuliCol = exlH.GetColoum("sheet1", 4);
            //exlH.Destroy();
            return fuliCol;
        }

        /// <summary>
        /// Remove nonsense information from labeled data
        /// eg: '', ' ', 'FLI', 'NONFLI'
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        static string[] NormalizeTrainData(string[] samples)
        {
            List<string> nSamples = new List<string>();
            foreach (var sample in samples)
            {
                if (sample == null) { continue; }
                if (string.IsNullOrEmpty(sample.Trim())) { continue; }
                if (sample.Trim().Equals("FLI") || sample.Trim().Equals("NONFLI")) { continue; }
                nSamples.Add(sample);
            }
            return nSamples.ToArray();
        }
    }
}
