using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Text.Handler;

namespace Text.Sentiment
{
    class SentiAnalysis
    {
        string[] posWords = null;
        string[] negWords = null;
        string[] notWords = null;

        Dictionary<string, double> posWordsDic = null;
        Dictionary<string, double> negWordsDic = null;
        Dictionary<string, double> notWordsDic = null;

        private WordSegHandler wsH = null;

        public SentiAnalysis()
        {
            wsH = new WordSegHandler();
        }

        public bool LoadSentiDic(string posWordPath, string negWordPath, string notWordPath)
        {
            posWords = FileHandler.LoadStringArray(posWordPath);
            negWords = FileHandler.LoadStringArray(negWordPath);
            notWords = FileHandler.LoadStringArray(notWordPath);

            posWordsDic = new Dictionary<string, double>();
            negWordsDic = new Dictionary<string, double>();
            notWordsDic = new Dictionary<string, double>();

            if (posWords == null || negWords == null || notWords == null)
            { return false; }

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
            if (wsH == null || !wsH.isValid)
            { Trace.TraceError("SentiAnalysis.JudgeSenti:  WordSegHandler not valid"); return double.NaN; }

            string[] groups = sentence.Split('，');

            List<double> groupVals = new List<double>();
            List<double> baseVals = new List<double>();
            foreach (var group in groups)
            {
                //处理每个情感群
                if (!wsH.ExecutePartition(sentence))
                { Trace.TraceError("SentiAnalysis.JudgeSenti:  WordSegHandler partition failed"); return double.NaN; }
                
                string[] words = wsH.GetNoStopWords();

                baseVals.Clear();
                int curNot = 1;
                foreach (var word in words)
                {
                    if (posWordsDic.ContainsKey(word)) 
                    {
                        baseVals.Add(posWordsDic[word] * curNot);
                        curNot = 1;
                        continue;
                    }
                    if (negWordsDic.ContainsKey(word))
                    {
                        baseVals.Add(negWordsDic[word] * curNot);
                        curNot = 1;
                        continue;
                    }
                    if (notWordsDic.ContainsKey(word))
                    {
                        curNot = -curNot;
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
