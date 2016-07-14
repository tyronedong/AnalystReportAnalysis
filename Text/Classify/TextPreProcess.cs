using System;
using System.IO;
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
        //static string model_relate_root_dictionary = ConfigurationManager.AppSettings["model_relate_root_dictionary"];
        //static string 

        bool isZhengliXls = false, isZhengliTxt = false;
        bool isFuliXls = false, isFuliTxt = false;

        public TextPreProcess(bool isZhengliXls, bool isZhengliTxt, bool isFuliXls, bool isFuliTxt)
        {
            this.isZhengliXls = isZhengliXls;
            this.isZhengliTxt = isZhengliTxt;
            this.isFuliXls = isFuliXls;
            this.isFuliTxt = isFuliTxt;
        }
        /// <summary>
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        public List<LabeledItem> GetLabeledItems(string rootTxtPath)
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            //get all rough zhengli and fuli
            string[] zhengli = GetTrainDataOfZhengli(rootTxtPath);
            string[] fuli = GetTrainDataOfFuli(rootTxtPath);

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
        /// Return all zhenglis in the form of a string array
        /// </summary>
        /// <returns></returns>
        public string[] GetTrainDataOfZhengli(string rootTxtPath)
        {
            //excel param
            string xlsPath;
            string sheetName; 
            int whichColumn;
            //txt param
            string txtPath;
            //tiqu param
            string[] zhengliExl = null, zhengliTxt = null, zhengliCol, zhengli;

            if (isZhengliXls)
            {
                xlsPath = ConfigurationManager.AppSettings["zhengli_excel_path"];
                sheetName = ConfigurationManager.AppSettings["zhengli_excel_sheet"];
                whichColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengli_excel_column"]);

                ExcelHandler exlH = new ExcelHandler(xlsPath);
                zhengliExl = exlH.GetColoum(sheetName, whichColumn);
            }
            if (isZhengliTxt)
            {
                txtPath = Path.Combine(rootTxtPath, ConfigurationManager.AppSettings["zhengli_txt_file"]);

                zhengliTxt = FileHandler.LoadStringArray(txtPath);
            }

            //merge excel and txt into one array
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
        /// Return all fulis in the form of a string array
        /// </summary>
        /// <returns></returns>
        public string[] GetTrainDataOfFuli(string rootTxtPath)
        {
            //excel param
            string xlsPath;
            string sheetName;
            int whichColumn;
            //txt param
            string txtPath;
            //tiqu param
            string[] fuliExl = null, fuliTxt = null, fuliCol, fuli;

            if (isFuliXls)
            {
                xlsPath = ConfigurationManager.AppSettings["fuli_excel_path"];
                sheetName = ConfigurationManager.AppSettings["fuli_excel_sheet"];
                whichColumn = Int32.Parse(ConfigurationManager.AppSettings["fuli_excel_column"]);

                ExcelHandler exlH = new ExcelHandler(xlsPath);
                fuliExl = exlH.GetColoum(sheetName, whichColumn);
            }
            if (isFuliTxt)
            {
                txtPath = Path.Combine(rootTxtPath, ConfigurationManager.AppSettings["fuli_txt_file"]);

                fuliTxt = FileHandler.LoadStringArray(txtPath);
            }

            //merge excel and txt into one array
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
        private string[] NormalizeTrainData(string[] samples)
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

        ///// <summary>
        ///// Given a sentence, calculate its word count dictionary
        ///// </summary>
        ///// <param name="sentence"></param>
        ///// <returns></returns>
        //public static Dictionary<string, int> GetWordCountDic(string sentence)
        //{
        //    WordSegHandler wsH = new WordSegHandler();
        //    wsH.ExecutePartition(sentence);
        //    if (!wsH.isValid) { Trace.TraceError("Text.Classify.TextPreProcess.GetWordCountDic() goes wrong"); return null; }
        //    string[] words = wsH.GetNoStopWords();

        //    Dictionary<string, int> wordCountDic = new Dictionary<string, int>();
        //    foreach (var word in words)
        //    {
        //        //could add a word normalize function in this placce so that numbers could be regarded as one word
        //        if (wordCountDic.ContainsKey(word)) { wordCountDic[word]++; }
        //        else
        //        {
        //            wordCountDic.Add(word, 1);
        //        }
        //    }
        //    return wordCountDic;
        //}

        /// <summary>
        /// Given a sentence, calculate its word count dictionary. Return null if wsH is unvalid
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="wsH"></param>
        /// <returns>
        /// 
        /// </returns>
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


        public static string[] SeparateParagraph(string paragraph)
        {
            char[] separator = { '。', '；', '？', '！' };
            paragraph = paragraph.Replace("\n", "。");

            return paragraph.Split(separator);
        }
    }
}
