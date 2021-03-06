﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// 初始化的时候指定源文件的类型：“FLI”、“FLIEMO”“INNOV”、“INNOVEMO”
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
        public List<LabeledItem> GetLabeledItems(string dataFilePath)
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            //read excel
            Dictionary<int, List<string>> trainData = GetTrainData(this.type, dataFilePath);

            //convert to LabeledItem
            foreach (var labelStrsPair in trainData)
            {
                foreach (var curStr in labelStrsPair.Value)
                { labeledItems.Add(new LabeledItem(labelStrsPair.Key, curStr)); }
            }

            return labeledItems;
        }

        /// <summary>
        /// 根据不同的type，采用不同的数据获取方式
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        public List<LabeledItem> GetLabeledItems(ref Dictionary<int, List<string>> trainData)
        {
            List<LabeledItem> labeledItems = new List<LabeledItem>();

            //read excel
            //Dictionary<int, List<string>> trainData = GetTrainData(this.type, dataFilePath);

            //convert to LabeledItem
            foreach (var labelStrsPair in trainData)
            {
                foreach (var curStr in labelStrsPair.Value)
                { labeledItems.Add(new LabeledItem(labelStrsPair.Key, curStr)); }
            }

            return labeledItems;
        }

        public Dictionary<int, List<string>> GetTrainData(string type, string dataFilePath)
        {
            ExcelHandler exlH = null;
            try
            {
                Dictionary<int, List<string>> trainData = new Dictionary<int, List<string>>();

                exlH = new ExcelHandler(Path.Combine(rootSourcePath, dataFilePath));
                if(!exlH.isValid)
                { return null; }

                if (type.Equals("FLI"))
                {
                    string[] zhengliCol, fuliCol;

                    string[] zhengliExl = null, zhengliTxt = null;
                    string[] fuliExl = null, fuliTxt = null;

                    //read
                    if (isZhengliXls)
                    { zhengliExl = exlH.GetColoum("sheet1", 3); }
                    if (isZhengliTxt)
                    {
                        string txtPath = Path.Combine(rootSourcePath, "./FLI/random_select_zhengli.txt");
                        zhengliTxt = FileHandler.LoadStringArray(txtPath);
                    }

                    if (isFuliXls)
                    { fuliExl = exlH.GetColoum("sheet1", 6); }
                    if (isFuliTxt)
                    {
                        string txtPath = Path.Combine(rootSourcePath, "./FLI/random_select_fuli.txt");
                        fuliTxt = FileHandler.LoadStringArray(txtPath);
                    }

                    //merge
                    if (zhengliExl == null && zhengliTxt == null)
                    { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
                    else if (zhengliExl != null && zhengliTxt == null)
                    { zhengliCol = zhengliExl; }
                    else if (zhengliTxt != null && zhengliExl == null)
                    { zhengliCol = zhengliTxt; }
                    else
                    { zhengliCol = zhengliExl.Concat(zhengliTxt).ToArray(); }

                    if (fuliExl == null && fuliTxt == null)
                    { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
                    else if (fuliExl != null && fuliTxt == null)
                    { fuliCol = fuliExl; }
                    else if (fuliTxt != null && fuliExl == null)
                    { fuliCol = fuliTxt; }
                    else
                    { fuliCol = fuliExl.Concat(fuliTxt).ToArray(); }

                    //normalize
                    zhengliCol = NormalizeTrainData(zhengliCol);
                    fuliCol = NormalizeTrainData(fuliCol);
                    //exlH.Destroy();
                    trainData.Add(1, new List<string>(zhengliCol));
                    trainData.Add(-1, new List<string>(fuliCol));
                }
                else if (type.Equals("FLIEMO"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 3);//文本
                    string[] toneExl = exlH.GetColoum("sheet1", 4);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref toneExl))//转化数据成trainData
                    { return null; }
                }
                else if (type.Equals("FLIIND"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 3);//文本
                    string[] indExl = exlH.GetColoum("sheet1", 5);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref indExl))//转化数据成trainData
                    { return null; }

                    //trainData[0].RemoveRange(trainData[1].Count, (trainData[0].Count - trainData[1].Count));
                }
                else if (type.Equals("INNOV"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 2);
                    string[] noninnovExl = exlH.GetColoum("sheet1", 6);

                    if (!NormalizeLabels(ref noninnovExl, type))//标注label中有误标的2-4的label
                        return null;

                    if (!ConvertToDic(ref trainData, ref textExl, ref noninnovExl))
                        return null;

                    if (!ReverseDic(ref trainData))
                        return null;

                    if (isFuliTxt)
                    {
                        string txtPath = Path.Combine(rootSourcePath, "./INNOV/random_select_fuli.txt");
                        string[] fuliTxt = FileHandler.LoadStringArray(txtPath);
                        trainData[0].AddRange(fuliTxt);
                    }
                }
                else if (type.Equals("INNOVTYPE"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 2);//文本
                    string[] typeExl = exlH.GetColoum("sheet1", 3);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref typeExl))//转化数据成trainData
                    { return null; }
                }
                else if (type.Equals("INNOVSTAGE"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 2);//文本
                    string[] stageExl = exlH.GetColoum("sheet1", 4);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref stageExl))//转化数据成trainData
                    { return null; }
                }
                else if (type.Equals("INNOVEMO"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 2);//文本
                    string[] typeExl = exlH.GetColoum("sheet1", 5);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref typeExl))//转化数据成trainData
                    { return null; }
                }
                else if(type.Equals("NONINNOV"))
                {
                    //read excel file for zhengli
                    string[] textExl = exlH.GetColoum("sheet1", 2);
                    string[] labelExl = exlH.GetColoum("sheet1", 7);

                    if (textExl.Length != labelExl.Length)
                        return null;
                    int len = textExl.Length;
                    List<string> zhengli = new List<string>();
                    for (int i = 1; i < len; i++)
                    {
                        if (string.IsNullOrEmpty(textExl[i]) || string.IsNullOrEmpty(labelExl[i]))//skip invalid row, null 行会被pass掉
                            continue;
                        if (string.IsNullOrWhiteSpace(textExl[i]) || string.IsNullOrWhiteSpace(labelExl[i]))//skip invalid row
                            continue;

                        zhengli.Add(textExl[i]);
                    }
                    trainData.Add(1, zhengli);

                    //read excel file for fuli
                    string txtPath = Path.Combine(rootSourcePath, "./NONINNOV/random_select_fuli.txt");
                    string[] fuliTxt = FileHandler.LoadStringArray(txtPath);
                    trainData.Add(-1, new List<string>(fuliTxt));
                }
                else if (type.Equals("NONINNOVTYPE"))
                {
                    string[] textExl = exlH.GetColoum("sheet1", 2);//文本
                    string[] typeExl = exlH.GetColoum("sheet1", 7);//对应文本的情感标注

                    if (!ConvertToDic(ref trainData, ref textExl, ref typeExl))//转化数据成trainData
                        return null;
                }
                else
                {
                    return null;
                }
                return trainData;
            }
            catch(Exception e)
            {
                Trace.TraceError("TextPreProcess.GetTrainData(): " + e.ToString());
                return null;
            }
            finally
            {
                if (exlH != null && exlH.isValid)
                    exlH.Destroy();
            }
        }

        /// <summary>
        /// 正对label的不一致性，作一个正规化
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool NormalizeLabels(ref string[] labels, string type)
        {
            try
            {
                Regex isLabel = new Regex(@"\d+");

                if (type.Equals("INNOV"))
                {
                    int index = -1;
                    foreach (var label in labels)
                    {
                        index++;
                        if (label == null)
                            continue;
                        if (label.Equals("0") || label.Equals("1"))
                            continue;
                        if(isLabel.IsMatch(label))
                            labels[index] = "1"; 
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                Trace.TraceError("TextPreProcess.NormalizeLabels(): " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 将两个pair对应的键值交换
        /// </summary>
        /// <param name="trainDataDic"></param>
        /// <returns></returns>
        private bool ReverseDic(ref Dictionary<int, List<string>> trainDataDic)
        {
            try
            {
                if (trainDataDic.Keys.Count != 2)
                    return false;

                int key0 = trainDataDic.Keys.ElementAt(0);
                int key1 = trainDataDic.Keys.ElementAt(1);

                var value0 = trainDataDic[key0];
                var value1 = trainDataDic[key1];

                trainDataDic.Clear();
                trainDataDic.Add(key0, value1);
                trainDataDic.Add(key1, value0);

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError("TextPreProcess.ReverseDic(): " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 输入一列数据和一列lable，输出相应的dictionary
        /// </summary>
        /// <param name="trainDataDic"></param>
        /// <param name="textColumn"></param>
        /// <param name="labelColumn"></param>
        /// <returns></returns>
        private bool ConvertToDic(ref Dictionary<int, List<string>> trainDataDic, ref string[] textColumn, ref string[] labelColumn)
        {
            if (textColumn.Length != labelColumn.Length)
            { Trace.TraceError("TextPreProcess.ConvertToDic():text and label is unmatch"); return false; }

            int len = textColumn.Length;
            for (int i = 1; i < len; i++)
            {
                if (string.IsNullOrEmpty(textColumn[i]) || string.IsNullOrEmpty(labelColumn[i]))//skip invalid row, null 行会被pass掉
                    continue;
                if (string.IsNullOrWhiteSpace(textColumn[i]) || string.IsNullOrWhiteSpace(labelColumn[i]))//skip invalid row
                    continue;

                int resultLabel;//将label从string转化为int
                if (!Int32.TryParse(labelColumn[i], out resultLabel))//如果label不是int型数，说明改行有误，跳过
                { Trace.TraceWarning("TextPreProcess.ConvertToDic():label is not a int value"); continue; }

                if(trainDataDic.ContainsKey(resultLabel))//按照label是否出现做出不同的操作
                { trainDataDic[resultLabel].Add(textColumn[i]); }
                else
                { 
                    List<string> strList= new List<string>();
                    strList.Add(textColumn[i]);
                    trainDataDic.Add(resultLabel, strList);
                }
            }
            return true;
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
        ///// 根据不同的type，采用不同的数据获取方式
        ///// Get labeled items from sorce training file in the form of class LabeledItem
        ///// </summary>
        ///// <returns></returns>
        //public List<LabeledItem> GetLabeledItems(string dataFilePath)
        //{
        //    List<LabeledItem> labeledItems = new List<LabeledItem>();

        //    WordSegHandler wsH = new WordSegHandler();
        //    if (!wsH.isValid) { Trace.TraceError("Text.Program.GenerateTrainDataFile() goes wrong"); return null; }

        //    //read excel
        //    Dictionary<int, List<string>> trainData = GetTrainData(this.type, dataFilePath);

        //    //convert to LabeledItem
        //    foreach (var labelStrsPair in trainData)
        //    {
        //        foreach (var curStr in labelStrsPair.Value)
        //        { labeledItems.Add(new LabeledItem(labelStrsPair.Key, curStr)); }
        //    }

        //    //if (type.Equals("FLI"))
        //    //{
        //    //    //get all rough zhengli and fuli
        //    //    string[] zhengli = GetTrainDataOfZhengli();
        //    //    string[] fuli = GetTrainDataOfFuli();

        //    //    ////normalize all zhengli and fuli
        //    //    //string[] zhengli = NormalizeTrainData(zhengliCol);
        //    //    //string[] fuli = NormalizeTrainData(fuliCol);

        //    //    //set two global variables
        //    //    LabeledItem.numberOfZhengli = zhengli.Length;
        //    //    LabeledItem.numberOfFuli = fuli.Length;

        //    //    foreach (var item in zhengli)
        //    //    {
        //    //        wsH.ExecutePartition(item);
        //    //        string[] segResult = wsH.GetNoStopWords();
        //    //        labeledItems.Add(new LabeledItem(1, item, segResult));
        //    //    }
        //    //    foreach (var item in fuli)
        //    //    {
        //    //        wsH.ExecutePartition(item);
        //    //        string[] segResult = wsH.GetNoStopWords();
        //    //        labeledItems.Add(new LabeledItem(0, item, segResult));
        //    //    }
        //    //}
        //    //else if (type.Equals("FLIEMO"))
        //    //{
        //    //    //read excel
        //    //    Dictionary<int, List<string>> trainData = GetTrainData("FLIEMO", dataFilePath);

        //    //    //convert to LabeledItem
        //    //    foreach (var labelStrsPair in trainData)
        //    //    {
        //    //        foreach (var curStr in labelStrsPair.Value)
        //    //        { labeledItems.Add(new LabeledItem(labelStrsPair.Key, curStr)); }
        //    //    }
        //    //}
        //    //else if (type.Equals("INNOV"))
        //    //{
        //    //    //read
        //    //    var tdic = GetTrainDataOfInnov();

        //    //    //convert
        //    //    foreach (var labelStrsPair in tdic)
        //    //    {
        //    //        foreach (var curStr in labelStrsPair.Value)
        //    //        { labeledItems.Add(new LabeledItem(labelStrsPair.Key, curStr)); }
        //    //    }
        //    //}
        //    //if (saveIntoFile) { FileHandler.SaveLabeledItems(preprocess_result_file, labeledItems); }
        //    return labeledItems;
        //}
        ///// <summary>
        ///// 返回一个dic，键值是类别，value是相应类别下的字符串数组
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<int, List<string>> GetTrainDataOfInnov()
        //{
        //    //tiqu param
        //    //string[] zhengliExl = null, zhengliTxt = null, zhengliCol, zhengli;
        //    //string[] fuliExl = null, fuliTxt = null, fuliCol, fuli;
        //    //txt param
        //    //string txtPath;

        //    //define data structure
        //    Dictionary<int, List<string>> trainData = new Dictionary<int, List<string>>();
        //    List<string> nonInnov = new List<string>();
        //    List<string> class1 = new List<string>();
        //    List<string> class2 = new List<string>();
        //    List<string> class3 = new List<string>();
        //    List<string> class4 = new List<string>();

        //    //excel param
        //    string xlsPath, sheetName;
        //    int textColumn, nonInnovColumn, innovClassColumn;

        //    xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengfuli_excel_filename"]);
        //    sheetName = ConfigurationManager.AppSettings["zhengfuli_excel_sheet"];
        //    textColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_text"]);
        //    nonInnovColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_noninnov"]);
        //    innovClassColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengfuli_excel_innovclass"]);

        //    //read excel
        //    ExcelHandler exlH = new ExcelHandler(xlsPath);

        //    //zhengli and fuli
        //    //from excel
        //    if (isFuliXls)
        //    {
        //        string[] textExl = null, nonInnovExl = null, innovTypeExl = null;

        //        textExl = exlH.GetColoum(sheetName, textColumn);
        //        nonInnovExl = exlH.GetColoum(sheetName, nonInnovColumn);
        //        innovTypeExl = exlH.GetColoum(sheetName, innovClassColumn);

        //        int length = textExl.Length;
        //        for (int i = 0; i < length; i++)
        //        {
        //            if (string.IsNullOrEmpty(nonInnovExl[i]) && string.IsNullOrEmpty(innovTypeExl[i]))
        //            { continue; }
        //            else if (nonInnovExl[i] != "0")
        //            { nonInnov.Add(textExl[i]); }
        //            else if (innovTypeExl[i] == "1")
        //            { class1.Add(textExl[i]); }
        //            else if (innovTypeExl[i] == "2")
        //            { class2.Add(textExl[i]); }
        //            else if (innovTypeExl[i] == "3")
        //            { class3.Add(textExl[i]); }
        //            else if (innovTypeExl[i] == "4")
        //            { class4.Add(textExl[i]); }
        //        }
        //    }
        //    //from txt
        //    string txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_txt_filename"]);
        //    string[] fuliTxt = FileHandler.LoadStringArray(txtPath);

        //    foreach(var fuli in fuliTxt)
        //    { nonInnov.Add(fuli); }
        //    //end work
        //    trainData.Add(0, nonInnov);
        //    trainData.Add(1, class1);
        //    trainData.Add(2, class2);
        //    trainData.Add(3, class3);
        //    trainData.Add(4, class4);
        //    return trainData;
        //}
        ///// <summary>
        ///// Return all zhenglis in the form of a string array
        ///// </summary>
        ///// <returns></returns>
        //public string[] GetTrainDataOfZhengli()
        //{
        //    //excel param
        //    string xlsPath;
        //    string sheetName; 
        //    int whichColumn;
        //    //txt param
        //    string txtPath;
        //    //tiqu param
        //    string[] zhengliExl = null, zhengliTxt = null, zhengliCol, zhengli;

        //    if (isZhengliXls)
        //    {
        //        xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengli_excel_filename"]);
        //        sheetName = ConfigurationManager.AppSettings["zhengli_excel_sheet"];
        //        whichColumn = Int32.Parse(ConfigurationManager.AppSettings["zhengli_excel_column"]);

        //        ExcelHandler exlH = new ExcelHandler(xlsPath);
        //        zhengliExl = exlH.GetColoum(sheetName, whichColumn);
        //    }
        //    if (isZhengliTxt)
        //    {
        //        txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["zhengli_txt_filename"]);

        //        zhengliTxt = FileHandler.LoadStringArray(txtPath);
        //    }

        //    //merge excel and txt into one array
        //    if (zhengliExl == null && zhengliTxt == null) { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
        //    else if (zhengliExl != null && zhengliTxt == null)
        //    {
        //        zhengliCol = zhengliExl;
        //    }
        //    else if (zhengliTxt != null && zhengliExl == null)
        //    {
        //        zhengliCol = zhengliTxt;
        //    }
        //    else
        //    {
        //        zhengliCol = zhengliExl.Concat(zhengliTxt).ToArray();
        //    }

        //    zhengli = NormalizeTrainData(zhengliCol);
        //    //exlH.Destroy();
        //    return zhengli;
        //}
        ///// <summary>
        ///// Return all fulis in the form of a string array
        ///// </summary>
        ///// <returns></returns>
        //public string[] GetTrainDataOfFuli()
        //{
        //    //excel param
        //    string xlsPath;
        //    string sheetName;
        //    int whichColumn;
        //    //txt param
        //    string txtPath;
        //    //tiqu param
        //    string[] fuliExl = null, fuliTxt = null, fuliCol, fuli;

        //    if (isFuliXls)
        //    {
        //        xlsPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_excel_filename"]);
        //        sheetName = ConfigurationManager.AppSettings["fuli_excel_sheet"];
        //        whichColumn = Int32.Parse(ConfigurationManager.AppSettings["fuli_excel_column"]);

        //        ExcelHandler exlH = new ExcelHandler(xlsPath);
        //        fuliExl = exlH.GetColoum(sheetName, whichColumn);
        //    }
        //    if (isFuliTxt)
        //    {
        //        txtPath = Path.Combine(rootSourcePath, ConfigurationManager.AppSettings["fuli_txt_filename"]);

        //        fuliTxt = FileHandler.LoadStringArray(txtPath);
        //    }

        //    //merge excel and txt into one array
        //    if (fuliExl == null && fuliTxt == null) { Trace.TraceError("Text.Classify.TextPreProcess.GetTrainDataOfZhengli(): no zhengli found"); return null; }
        //    else if (fuliExl != null && fuliTxt == null)
        //    {
        //        fuliCol = fuliExl;
        //    }
        //    else if (fuliTxt != null && fuliExl == null)
        //    {
        //        fuliCol = fuliTxt;
        //    }
        //    else
        //    {
        //        fuliCol = fuliExl.Concat(fuliTxt).ToArray();
        //    }

        //    fuli = NormalizeTrainData(fuliCol);
        //    //exlH.Destroy();
        //    return fuli;
        //}
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
            paragraph = paragraph.Replace("\n", "。");//替换成‘。'有问题吧
                                                      //替换成‘。’没有问题
            return paragraph.Split(separator);
        }
    }
}
