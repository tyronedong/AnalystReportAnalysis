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
    class Feature
    {
        private WordSegHandler wsH = null;
        private List<FeatureItem> featureItems = null;

        public Feature() { }
        
        public Feature(string featureFileName)
        {
            //string fileName = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\selected_chi_features_with_random_select_fulis.txt";
            wsH = new WordSegHandler();
            featureItems = LoadChiFeature(featureFileName);
        }

        /// <summary>
        /// Given a sentence, return its feature vector. Null if is error.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public double[] GetFeatureVec(string sentence)
        {
            if (this.wsH == null || !this.wsH.isValid) 
            { Trace.TraceError("Feature.GetFeatureVec(string sentence): WordSegHandler init failed"); return null; }

            if(this.featureItems == null)
            { Trace.TraceError("Feature.GetFeatureVec(string sentence): FeatureItem load failed"); return null; }

            List<double> featVec = new List<double>();

            Dictionary<string, int> wordCountDic = TextPreProcess.GetWordCountDic(sentence, ref this.wsH);

            foreach (var fItem in this.featureItems)
            {
                if (wordCountDic.ContainsKey(fItem.featureWord))
                {
                    double tfidf = wordCountDic[fItem.featureWord] * fItem.globalWeight;
                    featVec.Add(tfidf);
                }
                else { featVec.Add(0); }
            }

            return featVec.ToArray();
        }

        /// <summary>
        /// 返回和feature数组同样size的数组，对应位置表示该sentence在该feature下的特征值。当该特征不存在时为0
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="wsH"></param>
        /// <param name="featureItems"></param>
        /// <returns></returns>
        public static double[] GetFeatureVec(string sentence, ref WordSegHandler wsH, ref List<FeatureItem> featureItems)
        {
            List<double> featVec = new List<double>();

            Dictionary<string, int> wordCountDic = TextPreProcess.GetWordCountDic(sentence, ref wsH);

            if (wordCountDic == null)
            { Trace.TraceError("Feature.GetFeatureVec(string sentence, ref WordSegHandler wsH, ref List<FeatureItem> featureItems): failed get word count dic"); return null; }

            foreach (var fItem in featureItems)
            {
                if (wordCountDic.ContainsKey(fItem.featureWord))
                {
                    double tfidf = wordCountDic[fItem.featureWord] * fItem.globalWeight;
                    featVec.Add(tfidf);
                }
                else { featVec.Add(0); }
            }

            return featVec.ToArray();
        }

        public static bool ModifyChiFeature(string fileName)
        {
            string userFeaturePath = ConfigurationManager.AppSettings["user_feature_path"];
            //string featurePath = ConfigurationManager.AppSettings["chi_feature_path"];

            List<FeatureItem> newFeatures = new List<FeatureItem>();
            List<FeatureItem> oldFeatures = FileHandler.LoadFeatures(fileName);

            int id = 1;
            string[] userFeature = FileHandler.LoadStringArray(userFeaturePath);
            foreach (var userf in userFeature)
            {
                bool isContained = false;
                foreach (var f in oldFeatures)
                {
                    if (f.featureWord.Equals(userf))
                    {
                        isContained = true;
                        break;
                    }
                }

                if(!isContained)
                    newFeatures.Add(new FeatureItem(id++, userf, 1.0));
            }

            foreach (var of in oldFeatures)
            {
                newFeatures.Add(new FeatureItem(id++, of.featureWord, of.globalWeight));
            }

            return FileHandler.SaveFeatures(fileName, newFeatures);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">the file path to store chi-feature file</param>
        /// <param name="featRatio">define how much percent of words will be remained as chi-feature</param>
        /// <param name="minChiValue">define the min value of chi-value by which to decide weather is chi-feature</param>
        /// <param name="globalWeightType">define the type of 'global weight', default as 'tf-idf'</param>
        /// <returns></returns>
        public static bool ExtractAndStoreChiFeature(string type, string fileName, double featRatio = 0.20, double minChiValue = 10, string globalWeightType = "idf")
        {
            List<FeatureItem> featureItems = ChiFeatureExtract(type, featRatio, minChiValue, globalWeightType);

            if (FileHandler.SaveFeatures(fileName, featureItems)) return true;

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="featRatio">define how much percent of words will be remained as chi-feature</param>
        /// <param name="minChiValue">define the min value of chi-value by which to decide weather is chi-feature</param>
        /// <param name="globalWeightType">define the type of 'global weight', default as 'tf-idf'</param>
        /// <returns></returns>
        private static List<FeatureItem> ChiFeatureExtract(string type, double featRatio = 0.20, double minChiValue = 5, string globalWeightType = "idf")
        {
            //Dictionary<string, double> wordChiValueDic = GetWordChiValueDic("zhengli");
            string rootForChi = ConfigurationManager.AppSettings["feature_relate_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings[""];

            List<FeatureItem> featureItems = new List<FeatureItem>();

            TextPreProcess tPP = new TextPreProcess(type, rootForChi, true, true, true, true);//默认加入所有的数据源
            //this is temp
            //List<LabeledItem> labeledItems = tPP.GetLabeledItems("");
            var trainData = tPP.GetTrainData(type, dataFilePath);
            List<LabeledItem> labeledItems = tPP.GetLabeledItems(ref trainData);
            foreach(var curKey in trainData.Keys)
            {
                if (curKey == 0 || curKey == -1)//负例不再单独计算
                { continue; }

                Dictionary<string, WordItem> wordItemDic = GetWordItemDic(ref labeledItems, curKey);//获取单词正负例文档频数统计和正负例文档数统计
                Dictionary<string, double> wordChiValueDic = GetWordChiValueDic(ref wordItemDic);//计算卡方值
                var dicSort = from objDic in wordChiValueDic orderby objDic.Value descending select objDic;//按卡方值排序

                int countOfFeat = (int)(featRatio*)
            }
            
            //if (type.Equals("FLI"))
            //{
            //    Dictionary<string, WordItem> wordItemDic = GetWordItemDic(ref labeledItems, type, rootForChi);//获取辅助变量
            //    Dictionary<string, double> wordChiValueDic = GetWordChiValueDic(ref wordItemDic);//计算卡方值
            //    var dicSort = from objDic in wordChiValueDic orderby objDic.Value descending select objDic;//按卡方值排序

            //    int countOfFeat = (int)(featRatio * wordChiValueDic.Count);
            //    int N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;
            //    //选择卡方值较大的前k个值
            //    for (int i = 0; i < countOfFeat; i++)
            //    {
            //        if (dicSort.ElementAt(i).Value < minChiValue) { break; }
            //        FeatureItem featureItem = new FeatureItem();
            //        featureItem.id = i + 1;
            //        featureItem.featureWord = dicSort.ElementAt(i).Key;
            //        if (globalWeightType.Equals("idf"))
            //        {
            //            WordItem wordItem = wordItemDic[featureItem.featureWord];
            //            featureItem.globalWeight = Math.Log10(N * 1.0 / (wordItem.zhengliCount + wordItem.fuliCount + 1));
            //        }
            //        featureItems.Add(featureItem);
            //    }
            //}
            //else if (type.Equals("FLIEMO"))
            //{
            //    TextPreProcess tPP2 = new TextPreProcess(type, rootForChi, true, false, false, false);//默认加入所有的数据源
            //    //this is temp
            //    List<LabeledItem> labeledItems2 = tPP.GetLabeledItems("");
            //    foreach (var item in labeledItems)
            //    {
            //        if (item.label == 1)
            //            LabeledItem.numberOfZhengli++;
            //        else
            //            LabeledItem.numberOfFuli++;
            //    }
            //    Dictionary<string, WordItem> wordItemDic = GetWordItemDic( ref labeledItems2,type, rootForChi);//获取辅助变量
            //    Dictionary<string, double> wordChiValueDic = GetWordChiValueDic(ref wordItemDic);//计算卡方值
            //    var dicSort = from objDic in wordChiValueDic orderby objDic.Value descending select objDic;//按卡方值排序

            //    int countOfFeat = (int)(featRatio * wordChiValueDic.Count);
            //    int N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;
            //    //选择卡方值较大的前k个值
            //    for (int i = 0; i < countOfFeat; i++)
            //    {
            //        if (dicSort.ElementAt(i).Value < minChiValue) { break; }
            //        FeatureItem featureItem = new FeatureItem();
            //        featureItem.id = i + 1;
            //        featureItem.featureWord = dicSort.ElementAt(i).Key;
            //        if (globalWeightType.Equals("idf"))
            //        {
            //            WordItem wordItem = wordItemDic[featureItem.featureWord];
            //            featureItem.globalWeight = Math.Log10(N * 1.0 / (wordItem.zhengliCount + wordItem.fuliCount + 1));
            //        }
            //        featureItems.Add(featureItem);
            //    }
            //}
            //else if (type.Equals("INNOVTYPE"))
            //{

            //}
            //else if (type.Equals("INNOVSTAGE"))
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        Dictionary<string, WordItem> wordItemDic = GetWordItemDic(ref labeledItems, type, rootForChi, i);//获取辅助变量
            //        foreach (var item in labeledItems)
            //        {
            //            if (item.label == i)
            //                LabeledItem.numberOfZhengli++;
            //            else
            //                LabeledItem.numberOfFuli++;
            //        }
            //        Dictionary<string, double> wordChiValueDic = GetWordChiValueDic(ref wordItemDic);//计算卡方值
            //        var dicSort = from objDic in wordChiValueDic orderby objDic.Value descending select objDic;//按卡方值排序

            //        //int countOfFeat = (int)(featRatio * wordChiValueDic.Count);
            //        int countOfFeat = 30;
            //        int N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;
            //        //选择卡方值较大的前k个值
            //        for (int j = 0; j < countOfFeat; j++)
            //        {
            //            if (dicSort.ElementAt(i).Value < minChiValue) { break; }
            //            FeatureItem featureItem = new FeatureItem();
            //            featureItem.id = 30 * i + j + 1;
            //            featureItem.featureWord = dicSort.ElementAt(j).Key;
            //            if (globalWeightType.Equals("idf"))
            //            {
            //                WordItem wordItem = wordItemDic[featureItem.featureWord];
            //                featureItem.globalWeight = Math.Log10(N * 1.0 / (wordItem.zhengliCount + wordItem.fuliCount + 1));
            //            }
            //            featureItems.Add(featureItem);
            //        }
            //    }
            //}
            //else if (type.Equals("INNOVEMO"))
            //{

            //}
            //else if (type.Equals("NONINNOVTYPE"))
            //{

            //}

            //if (FileHandler.SaveFeatures(fileName, featureItems)) return true;
            return featureItems;
        }

        /// <summary>
        /// 1、get assist variable in the process of calculating chi values。
        /// 2、统计正负例文档数。
        /// 注释：labeledItems中出现过的每一个单词均对应WordItem中的一项。
        /// WordItem中的每一项均需要统计该单词在本类（由whichClass指定）中出现过几次（文档次数，而非词频），在其他类中出现过几次。
        /// </summary>
        /// <param name="labeledItems"></param>
        /// <param name="whichClass">多类问题中，指定当前的某一类为正例，其余类作为负例</param>
        /// <returns></returns>
        private static Dictionary<string, WordItem> GetWordItemDic(ref List<LabeledItem> labeledItems, int whichClass = 0)
        {
            Dictionary<string, WordItem> wordItemDic = new Dictionary<string, WordItem>();

            LabeledItem.numberOfZhengli = 0;//置零
            LabeledItem.numberOfFuli = 0;

            foreach (var labelItem in labeledItems)//统计单词正负例文档频数
            {
                if(labelItem.label == whichClass)
                { LabeledItem.numberOfZhengli++; }//统计正例的文档数
                else { LabeledItem.numberOfFuli++; }//统计负例的文档数

                foreach (var wordKvp in labelItem.wordCountDic)
                {
                    if (wordItemDic.ContainsKey(wordKvp.Key))
                    {
                        wordItemDic[wordKvp.Key].totalCount++;
                        if (labelItem.label == whichClass) { wordItemDic[wordKvp.Key].zhengliCount++; }
                        else { wordItemDic[wordKvp.Key].fuliCount++; }
                    }
                    else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (labelItem.label == whichClass))); }//label和whichClass相同则表示当前是正例反之则是负例
                }
            }

            return wordItemDic;
        }

        /// <summary>
        /// </summary>
        /// <param name="wordItemDic"></param>
        /// <returns></returns>
        private static Dictionary<string, double> GetWordChiValueDic(ref Dictionary<string, WordItem> wordItemDic)
        {
            Dictionary<string, double> wordChiValueDic = new Dictionary<string, double>();

            //Dictionary<string, WordItem> wordItemDic = GetWordItemDic();
            double N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;

            double A = 0, B = 0, C = 0, D = 0;
            foreach (var wordItemKvp in wordItemDic)
            {
                double chiValue = 0;
                string word = wordItemKvp.Key;
                WordItem wordItem = wordItemKvp.Value;

                A = wordItem.zhengliCount;
                B = wordItem.fuliCount;
                C = LabeledItem.numberOfZhengli - A;
                D = LabeledItem.numberOfFuli - B;

                if ((A + C) * (A + B) * (B + D) * (C + D) == 0)
                    chiValue = 0;
                else
                    chiValue = (N * Math.Pow((A * D - B * C), 2)) / ((A + C) * (A + B) * (B + D) * (C + D));

                wordChiValueDic.Add(word, chiValue);
            }

            return wordChiValueDic;
        }

        //private static Dictionary<string, WordItem> GetWordItemDic(ref List<LabeledItem> labeledItems, int whichClass = 0)
        //{
        //    Dictionary<string, WordItem> wordItemDic = new Dictionary<string, WordItem>();

        //    foreach (var labelItem in labeledItems)
        //    {
        //        foreach (var wordKvp in labelItem.wordCountDic)
        //        {
        //            if (wordItemDic.ContainsKey(wordKvp.Key))
        //            {
        //                wordItemDic[wordKvp.Key].totalCount++;
        //                if (labelItem.label == whichClass) { wordItemDic[wordKvp.Key].zhengliCount++; }
        //                else { wordItemDic[wordKvp.Key].fuliCount++; }
        //            }
        //            else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (labelItem.label == whichClass))); }//label和whichClass相同则表示当前是正例反之则是负例
        //        }
        //    }

        //    //if (type.Equals("FLIEMO"))
        //    //{
        //    //    foreach (var lItem in labeledItems)
        //    //    {
        //    //        foreach (var wordKvp in lItem.wordCountDic)
        //    //        {
        //    //            if (wordItemDic.ContainsKey(wordKvp.Key))
        //    //            {
        //    //                wordItemDic[wordKvp.Key].totalCount++;
        //    //                if (lItem.label == 1) { wordItemDic[wordKvp.Key].zhengliCount++; }
        //    //                else { wordItemDic[wordKvp.Key].fuliCount++; }
        //    //            }
        //    //            else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (lItem.label == 1))); }
        //    //        }
        //    //    }
        //    //}
        //    //else if (type.Equals("FLI"))
        //    //{
        //    //    foreach (var lItem in labeledItems)
        //    //    {
        //    //        foreach (var wordKvp in lItem.wordCountDic)
        //    //        {
        //    //            if (wordItemDic.ContainsKey(wordKvp.Key))
        //    //            {
        //    //                wordItemDic[wordKvp.Key].totalCount++;
        //    //                if (lItem.label == 1) { wordItemDic[wordKvp.Key].zhengliCount++; }
        //    //                else if (lItem.label == 0 || lItem.label == -1) { wordItemDic[wordKvp.Key].fuliCount++; }
        //    //            }
        //    //            else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (lItem.label == 1))); }
        //    //        }
        //    //    }
        //    //}
        //    //else if (type.Equals("INNOV"))
        //    //{
        //    //    foreach (var lItem in labeledItems)
        //    //    {
        //    //        foreach (var wordKvp in lItem.wordCountDic)
        //    //        {
        //    //            if (wordItemDic.ContainsKey(wordKvp.Key))
        //    //            {
        //    //                wordItemDic[wordKvp.Key].totalCount++;
        //    //                if (lItem.label == whichClass) { wordItemDic[wordKvp.Key].zhengliCount++; }
        //    //                else { wordItemDic[wordKvp.Key].fuliCount++; }
        //    //            }
        //    //            else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (lItem.label == whichClass))); }
        //    //        }
        //    //    }
        //    //}
        //    //else { Console.WriteLine("wrong type"); }

        //    return wordItemDic;
        //}
        ///// <summary>
        ///// In fact, the calculation result of zhengli and fuli equals
        ///// </summary>
        ///// <param name="whichClass">Two optional value for this para: "zhengli" or "fuli"</param>
        ///// <returns></returns>
        //static Dictionary<string, double> GetWordChiValueDic(string whichClass)
        //{
        //    Dictionary<string, double> wordChiValueDic = new Dictionary<string, double>();

        //    Dictionary<string, WordItem> wordItemDic = GetWordItemDic();
        //    double N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;

        //    double A = 0, B = 0, C = 0, D = 0;
        //    foreach (var wordItemKvp in wordItemDic)
        //    {
        //        double chiValue = 0;
        //        string word = wordItemKvp.Key;
        //        WordItem wordItem = wordItemKvp.Value;
        //        if (whichClass.Equals("zhengli"))
        //        {
        //            A = wordItem.zhengliCount;
        //            B = wordItem.fuliCount;
        //            C = LabeledItem.numberOfZhengli - A;
        //            D = LabeledItem.numberOfFuli - B;
        //        }
        //        else if (whichClass.Equals("fuli"))
        //        {
        //            A = wordItem.fuliCount;
        //            B = wordItem.zhengliCount;
        //            C = LabeledItem.numberOfFuli - A;
        //            D = LabeledItem.numberOfZhengli - B;
        //        }
        //        if ((A + C) * (A + B) * (B + D) * (C + D) == 0)
        //            chiValue = 0;
        //        else
        //            chiValue = (N * Math.Pow((A * D - B * C), 2)) / ((A + C) * (A + B) * (B + D) * (C + D));

        //        wordChiValueDic.Add(word, chiValue);
        //    }

        //    return wordChiValueDic;
        //}

        public static List<FeatureItem> LoadChiFeature(string fileName)
        {
            return FileHandler.LoadFeatures(fileName);
        }
    }
}
