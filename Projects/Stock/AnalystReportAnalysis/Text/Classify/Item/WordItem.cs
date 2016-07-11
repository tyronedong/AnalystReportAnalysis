using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Text.Classify.Item
{
    /// <summary>
    /// Each instance of WordItem contains a word and informaiton about how many zhengli and fuli docs contain the word.
    /// totalCount is the sum of zhengliCount and fuliCount
    /// </summary>
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
