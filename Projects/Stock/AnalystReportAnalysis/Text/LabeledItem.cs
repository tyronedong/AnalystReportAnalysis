using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Text
{
    class LabeledItem
    {
        public static int numberOfZhengli { get; set; }
        public static int numberOfFuli { get; set; }

        public int label { get; set; }
        public string sentence { get; set; }
        public List<string> words { get; set; }
        public Dictionary<string, int> wordCountDic { get; set; }

        public LabeledItem() { words = new List<string>(); wordCountDic = new Dictionary<string, int>(); }
        public LabeledItem(int label, string sentence, string[] words) 
        { 
            this.label = label;
            this.sentence = sentence;
            this.words = new List<string>(words);
            this.wordCountDic = GetWordCountDic();
        }
        public LabeledItem(int label, string sentence, List<string> words)
        {
            this.label = label;
            this.sentence = sentence;
            this.words = words;
            this.wordCountDic = GetWordCountDic();
        }

        private Dictionary<string, int> GetWordCountDic()
        {
            Dictionary<string, int> wordCountDic = new Dictionary<string, int>();
            foreach (var word in this.words)
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

    class WordItem
    {
        public string word { get; set; }
        public int totalCount { get; set; }
        public int zhengliCount { get; set; }
        public int fuliCount { get; set; }

        public WordItem(string word, bool isZhengli)
        {
            this.word = word;
            this.totalCount = 1;
            if (isZhengli) { this.zhengliCount = 1; this.fuliCount = 0; }
            else { this.zhengliCount = 0; this.fuliCount = 1; }
            
        }
    }
}
