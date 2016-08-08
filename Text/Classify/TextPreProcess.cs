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
        string type, rootSourcePath;

        bool isZhengliXls = false, isZhengliTxt = false;
        bool isFuliXls = false, isFuliTxt = false;

        /// <summary>
        /// 初始化的时候指定源文件的类型：“FLI”、“INNOV”
        /// 初始化的时候指定源文件的存放目录，同时通过四个布尔值指定用到目录下的那些文件
        /// </summary>
        /// <param name="rootSourcePath"></param>
        /// <param name="isZhengliXls"></param>
        /// <param name="isZhengliTxt"></param>
        /// <param name="isFuliXls"></param>
        /// <param name="isFuliTxt"></param>
        public TextPreProcess(string type, string rootSourcePath, bool isZhengliXls, bool isZhengliTxt, bool isFuliXls, bool isFuliTxt)
        {
            this.type = type;
            this.rootSourcePath = rootSourcePath;
            this.isZhengliXls = isZhengliXls;
            this.isZhengliTxt = isZhengliTxt;
            this.isFuliXls = isFuliXls;
            this.isFuliTxt = isFuliTxt;
        }
        /// <summary>
        /// 根据不同的type，采用不同的数据获取方式
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        public List<LabeledItem> GetLabeledItems()
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            WordSegHandler wsH = new WordSegHandler();
            if (!wsH.isValid) { Trace.TraceError("Text.Program.GenerateTrainDataFile() goes wrong"); return null; }
            if (type.Equals("FLIEMO"))
            {
                //read excel
                ExcelHandler exlH = new ExcelHandler(Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengfuli_excel_emo_filename"]));

                string[] textExl = null, toneExl = null;

                textExl = exlH.GetColoum("sheet1", 3);
                toneExl = exlH.GetColoum("sheet1", 4);

                int len = textExl.Length;
                for (int i = 1; i < len; i++)
                {
                    if (string.IsNullOrEmpty(toneExl[i]))
                        continue;
                    wsH.ExecutePartition(textExl[i]);
                    string[] segResult = wsH.GetNoStopWords();
                    labeledItems.Add(new LabeledItem(Int32.Parse(toneExl[i]), textExl[i], segResult));
                }
            }
            if (type.Equals("FLI"))
            {
                //get all rough zhengli and fuli
                string[] zhengli = GetTrainDataOfZhengli();
                string[] fuli = GetTrainDataOfFuli();

                ////normalize all zhengli and fuli
                //string[] zhengli = NormalizeTrainData(zhengliCol);
                //string[] fuli = NormalizeTrainData(fuliCol);

                //set two global variables
                LabeledItem.numberOfZhengli = zhengli.Length;
                LabeledItem.numberOfFuli = fuli.Length;

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
                    labeledItems.Add(new LabeledItem(0, item, segResult));
                }
            }
            else if (type.Equals("INNOV"))
            {
                var tdic = GetTrainDataOfInnov();
                foreach (var kvp in tdic)
                {
                    foreach (var curStr in kvp.Value)
                    {
                        wsH.ExecutePartition(curStr);
                        string[] segResult = wsH.GetNoStopWords();
                        labeledItems.Add(new LabeledItem(kvp.Key, curStr, segResult));
                    }
                }
            }
            //if (saveIntoFile) { FileHandler.SaveLabeledItems(preprocess_result_file, labeledItems); }
            return labeledItems;
        }

        /// <summary>
        /// 返回一个dic，键值是类别，value是相应类别下的字符串数组
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<string>> GetTrainDataOfInnov()
        {
            //tiqu param
            //string[] zhengliExl = null, zhengliTxt = null, zhengliCol, zhengli;
            //string[] fuliExl = null, fuliTxt = null, fuliCol, fuli;
            //txt param
            //string txtPath;

            //define data structure
            Dictionary<int, List<string>> trainData = new Dictionary<int, List<string>>();
            List<string> nonInnov = new List<string>();
            List<string> class1 = new List<string>();
            List<string> class2 = new List<string>();
            List<string> class3 = new List<string>();
            List<string> class4 = new List<string>();

            //excel param
            string xlsPath, sheetName;
            int textColumn, nonInnovColumn, innovClassColumn;

            xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengfuli_excel_filename"]);
            sheetName = ConfigurationManager.AppSettings["zhengfuli_excel_sheet"];
            textColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_text"]);
            nonInnovColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_noninnov"]);
            innovClassColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_innovclass"]);

            //read excel
            ExcelHandler exlH = new ExcelHandler(xlsPath);

            //zhengli and fuli
            //from excel
            if (isFuliXls)
            {
                string[] textExl = null, nonInnovExl = null, innovTypeExl = null;

                textExl = exlH.GetColoum(sheetName, textColumn);
                nonInnovExl = exlH.GetColoum(sheetName, nonInnovColumn);
                innovTypeExl = exlH.GetColoum(sheetName, innovClassColumn);

                int length = textExl.Length;
                for (int i = 0; i < length; i++)
                {
                    if (string.IsNullOrEmpty(nonInnovExl[i]) && string.IsNullOrEmpty(innovTypeExl[i]))
                    { continue; }
                    else if (nonInnovExl[i] != "0")
                    { nonInnov.Add(textExl[i]); }
                    else if (innovTypeExl[i] == "1")
                    { class1.Add(textExl[i]); }
                    else if (innovTypeExl[i] == "2")
                    { class2.Add(textExl[i]); }
                    else if (innovTypeExl[i] == "3")
                    { class3.Add(textExl[i]); }
                    else if (innovTypeExl[i] == "4")
                    { class4.Add(textExl[i]); }
                }
            }
            //from txt
            string txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_txt_filename"]);
            string[] fuliTxt = FileHandler.LoadStringArray(txtPath);

            foreach(var fuli in fuliTxt)
            { nonInnov.Add(fuli); }

            //end work
            trainData.Add(0, nonInnov);
            trainData.Add(1, class1);
            trainData.Add(2, class2);
            trainData.Add(3, class3);
            trainData.Add(4, class4);

            return trainData;
        }

        /// <summary>
        /// Return all zhenglis in the form of a string array
        /// </summary>
        /// <returns></returns>
        public string[] GetTrainDataOfZhengli()
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
                xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengli_excel_filename"]);
                sheetName = ConfigurationManager.AppSettings["zhengli_excel_sheet"];
                whichColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengli_excel_column"]);

                ExcelHandler exlH = new ExcelHandler(xlsPath);
                zhengliExl = exlH.GetColoum(sheetName, whichColumn);
            }
            if (isZhengliTxt)
            {
                txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengli_txt_filename"]);

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
        public string[] GetTrainDataOfFuli()
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
                xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_excel_filename"]);
                sheetName = ConfigurationManager.AppSettings["fuli_excel_sheet"];
                whichColumn = Int32.Parse(ConfigurationManager.AppSettings["fuli_excel_column"]);

                ExcelHandler exlH = new ExcelHandler(xlsPath);
                fuliExl = exlH.GetColoum(sheetName, whichColumn);
            }
            if (isFuliTxt)
            {
                txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_txt_filename"]);

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
