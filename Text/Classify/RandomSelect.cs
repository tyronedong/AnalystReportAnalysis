using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using Text.Handler;
using Text.Outsider;

namespace Text.Classify
{
    class RandomSelect
    {
        static Random ran = new Random();
        static int selectNumber = 300;

        /// <summary>
        /// Select and store strings to file
        /// </summary>
        /// <returns></returns>
        public static bool ExecuteSelect()
        {
            string path = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\random_select_fulis.txt";

            string[] selectedStrs = SelectStrs();
            if (selectedStrs == null) { Trace.TraceError("Text.Classify.RandomSelect.ExecuteSelect() goes wrong"); return false; }

            if (FileHandler.SaveStringArray(path, selectedStrs))
                return true;
            else
                return false;
        }

        static string[] SelectStrs()
        {
            List<string> selectedStrs = new List<string>();

            MongoDBHandler mgH = new MongoDBHandler("QueryOnly");
            if (!mgH.Init()) { return null; }

            int counter = 0;
            while (counter<selectNumber)
            {
                int rank = RandomNumber(1, 213894);

                AnalystReport anaReport = mgH.FindXthOne(rank - 1);
                if (anaReport == null)
                { Trace.TraceError("Text.Classify.RandomSelect.SelectStrs() goes wrong in " + counter + "th with rank " + rank); continue; }

                string str = SelectOneFromContent(anaReport.Content);
                if (string.IsNullOrWhiteSpace(str))
                { continue; }
                else
                { selectedStrs.Add(str); counter++; Console.WriteLine(counter + "th select string: " + str); }
            }

            return selectedStrs.ToArray();
        }

        static int RandomNumber(int min, int max)
        {
            return ran.Next(min, max);
        }

        static string SelectOneFromContent(string content)
        {
            content = content.Replace("\n", "");
            string[] sentences = content.Split('。');

            int counter = 0; string sent = "";
            while (true)
            {
                counter++;
                if (counter >= 10) { break; }

                int whichOne = RandomNumber(0, sentences.Length - 1);
                sent = sentences[whichOne];

                if (string.IsNullOrEmpty(sent)) { continue; }
                if (sent.Contains("预测")) { continue; }
                if (sent.Contains("预期")) { continue; }
                if (sent.Contains("预告")) { continue; }
                if (sent.Contains("预示")) { continue; }
                if (sent.Contains("预计")) { continue; }
                if (sent.Contains("估计")) { continue; }
                if (sent.Contains("将来")) { continue; }
                if (sent.Contains("未来")) { continue; }
                if (sent.Contains("有望")) { continue; }
                
                break;
            }
            return sent;
        }
    }
}
