using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using Text.Classify.Item;

namespace Text.Handler
{
    class FileHandler
    {
        /// <summary>
        /// Write all strs into file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool SaveStringArray(string fileName, string[] strs)
        {
            try { File.WriteAllLines(fileName, strs); }
            catch (Exception e) { Trace.TraceError("Text.Handler.FileHandler.SaveStringArray(string fileName, string[] strs): " + e.ToString()); return false; }
            return true;
        }

        public static string[] LoadStringArray(string fileName)
        {
            try { return File.ReadAllLines(fileName); ;}
            catch (Exception e) { Trace.TraceError("Text.Handler.FileHandler.LoadStringArray(string fileName): " + e.ToString()); return null; }
        }

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

        public static bool SaveLabeledItems(string fileName, List<LabeledItem> labeledItems)
        {
            try
            {
                //序列化
                FileStream fs = new FileStream(fileName, FileMode.Create);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, labeledItems);
                fs.Close();
            }
            catch (Exception e) { Trace.TraceError("Text.Handler.FileHandler.SaveLabeledItems(string fileName, List<LabeledItem> labeledItems): " + e.ToString()); return false; }
            return true;
        }

        public static List<LabeledItem> LoadLabeledItems(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                List<LabeledItem> labeledItems = bf.Deserialize(fs) as List<LabeledItem>;
                return labeledItems;
            }
            catch (Exception e) { Trace.TraceError("Text.Handler.FileHandler.LoadLabeledItems(string fileName): " + e.ToString()); return null; }
        }
    }
}
