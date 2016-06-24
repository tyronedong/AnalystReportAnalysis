using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Text.Classify.Item
{
    class FeatureItem
    {
        public int id { get; set; }
        public string featureWord { get; set; }
        public double globalWeight { get; set; }

        public FeatureItem() { }
        public FeatureItem(int id, string featureWord, double globalWeight)
        {
            this.id = id;
            this.featureWord = featureWord;
            this.globalWeight = globalWeight;
        }
    }
}
