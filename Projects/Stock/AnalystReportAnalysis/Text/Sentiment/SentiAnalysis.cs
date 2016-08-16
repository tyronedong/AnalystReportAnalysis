using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Text.Handler;

namespace Text.Sentiment
{
    public class SentiAnalysis
    {
        public bool isValid = false;

        string[] posWords = null;
        string[] negWords = null;
        string[] notWords = null;
        string[] degWords = null;

        Dictionary<string, double> posWordsDic = null;
        Dictionary<string, double> negWordsDic = null;
        Dictionary<string, double> notWordsDic = null;
        Dictionary<string, double> degWordsDic = null;

        private WordSegHandler wsH = null;

        public SentiAnalysis()
        {
            wsH = new WordSegHandler();
            isValid = wsH.isValid && LoadSentiDic();
        }

        public bool LoadSentiDic(string posWordFile = "pdic.txt", string negWordFile = "ndic.txt", string notWordFile = "notdic.txt", string degWordFile = "degreedic.txt")
        {
            string sentRootDic = ConfigurationManager.AppSettings["sentiment_dic_root_dictionary"];

            posWords = FileHandler.LoadStringArray(Path.Combine(sentRootDic, posWordFile));
            negWords = FileHandler.LoadStringArray(Path.Combine(sentRootDic, negWordFile));
            notWords = FileHandler.LoadStringArray(Path.Combine(sentRootDic, notWordFile));
            degWords = FileHandler.LoadStringArray(Path.Combine(sentRootDic, degWordFile));

            posWordsDic = new Dictionary<string, double>();
            negWordsDic = new Dictionary<string, double>();
            notWordsDic = new Dictionary<string, double>();
            degWordsDic = new Dictionary<string, double>();

            if (posWords == null || negWords == null || notWords == null || degWords == null)
            { return false; }

            Regex wordValuePair = new Regex(@"[\u4e00-\u9fa5]+ \d+(\.\d+)?");

            foreach(var p in posWords)
            {
                if (posWordsDic.ContainsKey(p)) { continue; }
                posWordsDic.Add(p, 1);
            }
            foreach (var n in negWords)
            {
                if (negWordsDic.ContainsKey(n)) { continue; }
                negWordsDic.Add(n, -1);
            }
            foreach (var not in notWords)
            {
                if (notWordsDic.ContainsKey(not)) { continue; }
                notWordsDic.Add(not, 0);
            }
            foreach(var degPair in degWords)
            {
                if (!wordValuePair.IsMatch(degPair)) { continue; }

                string[] degKV = degPair.Split(' ');
                if (degWordsDic.ContainsKey(degKV[0])) { continue; }
                degWordsDic.Add(degKV[0], double.Parse(degKV[1]));
            }

            return true;
        }

        /// <summary>
        /// if returned value greater than 0, positive;
        /// if returned value less than 0, negtive;
        /// if returned value equals 0, neutral;
        /// return double.NaN if error
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public double CalSentiValue(string sentence)
        {
            Regex cost = new Regex(@"成本");
            Regex profit = new Regex(@"毛利率|净利率|利润|增速");

            Regex goodSpace = new Regex(@"(提升|上升|拓展|成长|市值|业务|盈利|发展|市场|行业|对接)空间");
            Regex spaceBig = new Regex(@"无限|巨大|较大|大|广阔|打开");
            Regex spaceSmall = new Regex(@"有限|较小|小");

            if (wsH == null || !wsH.isValid)
            { Trace.TraceError("SentiAnalysis.JudgeSenti:  WordSegHandler not valid"); return double.NaN; }

            string[] groups = sentence.Split('，');

            List<double> groupVals = new List<double>();
            List<double> baseVals = new List<double>();//一个baseVal值对应一个情感词
            foreach (var group in groups)
            {
                //处理每个情感群
                if (!wsH.ExecutePartition(group))
                { Trace.TraceError("SentiAnalysis.JudgeSenti: WordSegHandler partition failed"); return double.NaN; }
                
                string[] words = wsH.GetNoStopWords();

                baseVals.Clear();
                double curW = 1, curDeg = 1;
                int notIndex = 0, degIndex = 0, indexCounter = 0;
                foreach (var word in words)
                {
                    indexCounter++;
                    //处理每个意群

                    //扫描到正面词或者否定词，意味着一个意群的结束
                    if (posWordsDic.ContainsKey(word)) 
                    {
                        baseVals.Add(curW * curDeg * posWordsDic[word]);
                        curW = 1; curDeg = 1;
                        notIndex = 0; degIndex = 0;
                        continue;
                    }
                    if (negWordsDic.ContainsKey(word))
                    {
                        baseVals.Add(curW * curDeg * negWordsDic[word]);
                        curW = 1; curDeg = 1;
                        notIndex = 0; degIndex = 0;
                        continue;
                    }

                    //处理出现的否定词或程度副词
                    if (notWordsDic.ContainsKey(word))
                    {
                        curW = -curW;
                        notIndex = indexCounter;
                    }
                    if(degWordsDic.ContainsKey(word))
                    {
                        curDeg *= degWordsDic[word];
                        degIndex = indexCounter;
                    }

                    //处理程度副词在否定词之后的情况
                    if (notIndex != 0 && degIndex != 0)
                    {
                        if (notIndex < degIndex)
                        {
                            curW = -curW;//之前的curW反转是不需要的,因此再做一次反转返回到原值
                            curW = 0.5 * curW; //表示程度的减弱
                        }
                        notIndex = 0; degIndex = 0;
                    }

                    //扫描到“降低”、“下降”，需要具体区分是什么主体
                    if (word.Equals("降低") || word.Equals("下降"))
                    {
                        if (cost.IsMatch(group))
                        { baseVals.Add(1); break; }
                        if (profit.IsMatch(group))
                        { baseVals.Add(-1); break; }
                    }
                    if(word.Contains("空间"))
                    {
                        if(goodSpace.IsMatch(group))
                        {
                            if(spaceBig.IsMatch(group))
                            { baseVals.Add(1); break; }
                            if(spaceSmall.IsMatch(group))
                            { baseVals.Add(-1); break; }
                        }
                    }
                }

                //根据得到的该情感群出现的词综合出该情感群的得分(目前采用求和)
                double groupVal = 0;
                foreach (var baseV in baseVals)
                { groupVal += baseV; }

                groupVals.Add(groupVal);
            }

            double sentenceVal = 0;
            foreach(var groupV in groupVals)
            { sentenceVal += groupV; }

            return sentenceVal;
        }
    }
}
