using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Text.Handler;
using Text.Classify.Item;

namespace Text.Classify
{
    class TextPreProcess
    {
        //static string preprocess_result_file = ConfigurationManager.AppSettings["preprocess_result_file"];



        /// <summary>
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        public static List<LabeledItem> GetLabeledItems()
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            //get all rough zhengli and fuli
            string[] zhengli = GetTrainDataOfZhengli();
            string[] fuli = GetTrainDataOfFuli();

            ////normalize all zhengli and fuli
            //string[] zhengli = NormalizeTrainData(zhengliCol);
            //string[] fuli = NormalizeTrainData(fuliCol);

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
            //if (saveIntoFile) { FileHandler.SaveLabeledItems(preprocess_result_file, labeledItems); }
            return labeledItems;
        }

        /// <summary>
        /// </summary>
        /// <returns>Return all zhenglis in the form of a string array</returns>
        public static string[] GetTrainDataOfZhengli()
        {
            string xlsPath = ConfigurationManager.AppSettings["zhengli_excel_path"];
            string sheetName = ConfigurationManager.AppSettings["zhengli_excel_sheet"];
            int whichColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengli_excel_column"]);

            string txtPath = ConfigurationManager.AppSettings["zhengli_txt_path"];

            string[] zhengliExl = null, zhengliTxt = null, zhengliCol, zhengli;

            if (!xlsPath.Equals("not_valid"))
            {
                ExcelHandler exlH = new ExcelHandler(xlsPath);
                zhengliExl = exlH.GetColoum(sheetName, whichColumn);
            }

            if (!txtPath.Equals("not_valid"))
            {
                zhengliTxt = FileHandler.LoadStringArray(txtPath);
            }

            if (zhengliExl == null && zhengliTxt == null) { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
            else if (zhengliExl != null && zhengliTxt == null)
            {
                zhengliCol = zhengliExl;
            }
            else if (zhengliTxt != null && zhengliExl == null)
            {
                zhengliCol = zhengliTxt;
            }
            else
            {
                zhengliCol = zhengliExl.Concat(zhengliTxt).ToArray();
            }

            zhengli = NormalizeTrainData(zhengliCol);
            //exlH.Destroy();
            return zhengli;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Return all fulis in the form of a string array</returns>
        static string[] GetTrainDataOfFuli()
        {
            string xlsPath = ConfigurationManager.AppSettings["fuli_excel_path"];
            string sheetName = ConfigurationManager.AppSettings["fuli_excel_sheet"];
            int whichColumn = Int32.Parse(ConfigurationManager.AppSettings["fuli_excel_column"]);

            string txtPath = ConfigurationManager.AppSettings["fuli_txt_path"];

            string[] fuliExl = null, fuliTxt = null, fuliCol, fuli;

            if (!xlsPath.Equals("not_valid"))
            {
                ExcelHandler exlH = new ExcelHandler(xlsPath);
                fuliExl = exlH.GetColoum(sheetName, whichColumn);
            }

            if (!txtPath.Equals("not_valid"))
            {
                fuliTxt = FileHandler.LoadStringArray(txtPath);
            }

            if (fuliExl == null && fuliTxt == null) { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
            else if (fuliExl != null && fuliTxt == null)
            {
                fuliCol = fuliExl;
            }
            else if (fuliTxt != null && fuliExl == null)
            {
                fuliCol = fuliTxt;
            }
            else
            {
                fuliCol = fuliExl.Concat(fuliTxt).ToArray();
            }

            fuli = NormalizeTrainData(fuliCol);
            //exlH.Destroy();
            return fuli;
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

        /// <summary>
        /// Given a sentence, calculate its word count dictionary
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetWordCountDic(string sentence)
        {
            WordSegHandler wsH = new WordSegHandler();
            wsH.ExecutePartition(sentence);
            if (!wsH.isValid) { Trace.TraceError("Text.Classify.TextPreProcess.GetWordCountDic() goes wrong"); return null; }
            string[] words = wsH.GetNoStopWords();

            Dictionary<string, int> wordCountDic = new Dictionary<string, int>();
            foreach (var word in words)
            {
                //could add a word normalize function in this placce so that numbers could be regarded as one word
                if (wordCountDic.ContainsKey(word)) { wordCountDic[word]++; }
                else
                {
                    wordCountDic.Add(word, 1);
                }
            }
            return wordCountDic;
        }

        public static Dictionary<string, int> GetWordCountDic(string sentence, ref WordSegHandler wsH)
        {
            if (!wsH.isValid) { Trace.TraceError("Text.Classify.TextPreProcess.GetWordCountDic() goes wrong"); return null; }
            wsH.ExecutePartition(sentence);
            string[] words = wsH.GetNoStopWords();

            Dictionary<string, int> wordCountDic = new Dictionary<string, int>();
            foreach (var word in words)
            {
                //could add a word normalize function in this placce so that numbers could be regarded as one word
                if (wordCountDic.ContainsKey(word)) { wordCountDic[word]++; }
                else
                {
                    wordCountDic.Add(word, 1);
                }
            }
            return wordCountDic;
        }
    }
}
