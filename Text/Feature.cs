﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Text.Handler;

namespace Text
{
    class Feature
    {
        public static List<FeatureItem> LoadChiFeature(string fileName)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="featRatio"></param>
        /// <param name="globalWeightType"></param>
        /// <returns></returns>
        public static bool ChiFeatureExtract(string fileName, double featRatio = 0.4, string globalWeightType = "idf")
        {
            //Dictionary<string, double> wordChiValueDic = GetWordChiValueDic("zhengli");
            Dictionary<string, WordItem> wordItemDic = GetWordItemDic();
            Dictionary<string, double> wordChiValueDic = GetWordChiValueDic(ref wordItemDic);
            var dicSort = from objDic in wordChiValueDic orderby objDic.Value descending select objDic;

            List<FeatureItem> featureItems = new List<FeatureItem>();
            int numberOfFeat = (int)(featRatio * wordChiValueDic.Count);
            int N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;
            for (int i = 0; i < numberOfFeat; i++)
            {
                FeatureItem featureItem = new FeatureItem();
                featureItem.id = i + 1;
                featureItem.featureWord = dicSort.ElementAt(i).Key;
                if (globalWeightType.Equals("idf"))
                {
                    WordItem wordItem = wordItemDic[featureItem.featureWord];
                    featureItem.globalWeight = Math.Log10(N * 1.0 / (wordItem.zhengliCount + wordItem.fuliCount + 1));
                }
                featureItems.Add(featureItem);
            }

            if (FileHandler.SaveFeatures(fileName, featureItems)) return true;
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordItemDic"></param>
        /// <returns></returns>
        static Dictionary<string, double> GetWordChiValueDic(ref Dictionary<string, WordItem> wordItemDic)
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

        /// <summary>
        /// In fact, the calculation result of zhengli and fuli equals
        /// </summary>
        /// <param name="whichClass">Two optional value for this para: "zhengli" or "fuli"</param>
        /// <returns></returns>
        static Dictionary<string, double> GetWordChiValueDic(string whichClass)
        {
            Dictionary<string, double> wordChiValueDic = new Dictionary<string, double>();

            Dictionary<string, WordItem> wordItemDic = GetWordItemDic();
            double N = LabeledItem.numberOfZhengli + LabeledItem.numberOfFuli;

            double A = 0, B = 0, C = 0, D = 0;
            foreach (var wordItemKvp in wordItemDic)
            {
                double chiValue = 0;
                string word = wordItemKvp.Key;
                WordItem wordItem = wordItemKvp.Value;
                if (whichClass.Equals("zhengli"))
                {
                    A = wordItem.zhengliCount;
                    B = wordItem.fuliCount;
                    C = LabeledItem.numberOfZhengli - A;
                    D = LabeledItem.numberOfFuli - B;
                }
                else if (whichClass.Equals("fuli"))
                {
                    A = wordItem.fuliCount;
                    B = wordItem.zhengliCount;
                    C = LabeledItem.numberOfFuli - A;
                    D = LabeledItem.numberOfZhengli - B;
                }
                if ((A + C) * (A + B) * (B + D) * (C + D) == 0)
                    chiValue = 0;
                else
                    chiValue = (N * Math.Pow((A * D - B * C), 2)) / ((A + C) * (A + B) * (B + D) * (C + D));

                wordChiValueDic.Add(word, chiValue);
            }

            return wordChiValueDic;
        }

        /// <summary>
        /// Each item in the dictionary 
        /// </summary>
        /// <returns>return the dicitonary which contains all the words exist in the training data.</returns>
        static Dictionary<string, WordItem> GetWordItemDic()
        {
            Dictionary<string, WordItem> wordItemDic = new Dictionary<string, WordItem>();
            List<LabeledItem> labeledItems = GetLabeledItems();

            foreach (var lItem in labeledItems)
            {
                foreach (var wordKvp in lItem.wordCountDic)
                {
                    if (wordItemDic.ContainsKey(wordKvp.Key))
                    {
                        wordItemDic[wordKvp.Key].totalCount++;
                        if (lItem.label == 1) { wordItemDic[wordKvp.Key].zhengliCount++; }
                        else if (lItem.label == -1) { wordItemDic[wordKvp.Key].fuliCount++; }
                    }
                    else { wordItemDic.Add(wordKvp.Key, new WordItem(wordKvp.Key, (lItem.label == 1))); }
                }
            }

            return wordItemDic;
        }

        /// <summary>
        /// Get labeled items from sorce training file in the form of class LabeledItem
        /// </summary>
        /// <returns></returns>
        static List<LabeledItem> GetLabeledItems()
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
