using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Text.Classify.Item;

namespace Text.Handler
{
    class FileHandler
    {
        public static bool SaveFeatures(string fileName, List<FeatureItem> featureItems)
        {
            List<string> featureLines = new List<string>();
            foreach (var feature in featureItems)
            {
                string line = feature.id + " " + feature.featureWord + " " + feature.globalWeight;
                featureLines.Add(line);
            }

            try { File.WriteAllLines(fileName, featureLines.ToArray()); }
            catch (Exception e) { Trace.TraceError("Text.Handler.FileHandler.SaveFeatures(string fileName, List<FeatureItem> featureItems): " + e.ToString()); return false; }
            return true;
        }

        public static List<FeatureItem> LoadFeatures(string fileName)
        {
            List<FeatureItem> featureItems = new List<FeatureItem>();

            string[] featureLines = File.ReadAllLines(fileName);
            foreach (var line in featureLines)
            {
                string[] attrs = line.Split(' ');
                if (attrs.Length == 3)
                {
                    featureItems.Add(new FeatureItem(Int32.Parse(attrs[0]), attrs[1], double.Parse(attrs[2])));
                }
            }

            return featureItems;
        }
    }
}
