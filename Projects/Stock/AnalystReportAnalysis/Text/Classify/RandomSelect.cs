using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Configuration;
using Text.Handler;
using Text.Outsider;

namespace Text.Classify
{
    class RandomSelect
    {
        static Random ran = new Random();
        //static int selectNumber = 300;
        static string resultRootDic = ConfigurationManager.AppSettings["result_file_root_dictionary"];

        /// <summary>
        /// Select and store zhengli into file
        /// </summary>
        /// <param name="selectHowMany"></param>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public static bool ExecuteSelectZhengli(int selectHowMany, string modelPath)
        {
            //string path = Path.Combine(resultRootDic, "random_select_" + selectHowMany + "_zhengli");
            string path = Path.Combine(resultRootDic, "random_select_zhengli");

            Model model = new Model();
            model.LoadModel(modelPath);
            
            string[] selectedStrs = SelectStrsZhengli(selectHowMany, ref model);
            if (selectedStrs == null) { Trace.TraceError("Text.Classify.RandomSelect.ExecuteSelectZhengli() goes wrong"); return false; }

            if (FileHandler.SaveStringArray(path, selectedStrs))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 执行逻辑：每次从mongodb中随机选取一个record，提取该record下的所有由model预测的前瞻性语句加入zhengli数组
        /// 重复直到zhengli达到了足够的数量
        /// </summary>
        /// <param name="selectCount"></param>
        /// <returns></returns>
        static string[] SelectStrsZhengli(int selectCount, ref Model model)
        {
            List<string> selectedStrs = new List<string>();

            MongoDBHandler mgH = new MongoDBHandler("QueryOnly");
            if (!mgH.Init()) { return null; }

            int counter = 0;
            while (counter < selectCount)
            {
                int maxNum = mgH.GetCollectionCount();
                int rank = RandomNumber(1, maxNum);

                AnalystReport anaReport = mgH.FindXthOne(rank - 1);
                if (anaReport == null)
                { Trace.TraceError("Text.Classify.RandomSelect.SelectStrsZhengli() goes wrong in " + counter + "th with rank " + rank); continue; }

                string[] zhenglis = SelectAllInContentZhengli(anaReport.Content, ref model);
                foreach (var zhengli in zhenglis)
                { selectedStrs.Add(zhengli); counter++; Console.WriteLine(counter + "th select zhengli: " + zhengli); }
            }
            return selectedStrs.ToArray();
        }

        static string[] SelectAllInContentZhengli(string content, ref Model model)
        {
            List<string> zhenglis = new List<string>();
            content = content.Replace("\n", "。");
            string[] sentences = content.Split('。');

            foreach (var sentence in sentences)
            {
                if (model.Predict(sentence) == 1.0)
                { zhenglis.Add(sentence); }
            }
            //model.Predict()
            return zhenglis.ToArray();
        }

        /// <summary>
        /// Select and store fuli strings to file
        /// 执行逻辑：每次从mongodb中随机选取一个record，再从该record中依启发式规则随机选择一句话加入fuli数组
        /// 重复直到fuli达到了足够的数量
        /// </summary>
        /// <returns></returns>
        public static bool ExecuteSelectFuli(int selectHowMany)
        {
            //string path = Path.Combine(resultRootDic, "random_select_" + selectHowMany + "_fuli");
            string path = Path.Combine(resultRootDic, "random_select_fuli");

            string[] selectedStrs = SelectStrsFuli(selectHowMany);
            if (selectedStrs == null) { Trace.TraceError("Text.Classify.RandomSelect.ExecuteSelectFuli() goes wrong"); return false; }

            if (FileHandler.SaveStringArray(path, selectedStrs))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Select selectNumber strings, each string is selected from one record of mongodb
        /// </summary>
        /// <returns></returns>
        static string[] SelectStrsFuli(int selectCount)
        {
            List<string> selectedStrs = new List<string>();

            MongoDBHandler mgH = new MongoDBHandler("QueryOnly");
            if (!mgH.Init()) { return null; }

            int counter = 0;
            while (counter < selectCount)
            {
                int maxNum = mgH.GetCollectionCount();
                int rank = RandomNumber(1, maxNum);

                AnalystReport anaReport = mgH.FindXthOne(rank - 1);
                if (anaReport == null)
                { Trace.TraceError("Text.Classify.RandomSelect.SelectStrsFuli() goes wrong in " + counter + "th with rank " + rank); continue; }

                string str = SelectOneFromContentFuli(anaReport.Content);
                if (string.IsNullOrWhiteSpace(str))
                { continue; }
                else
                { selectedStrs.Add(str); counter++; Console.WriteLine(counter + "th select fuli: " + str); }
            }
            return selectedStrs.ToArray();
        }

        /// <summary>
        /// Given a content, select one sentence from it.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        static string SelectOneFromContentFuli(string content)
        {
            content = content.Replace("\n", "。");
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

        static int RandomNumber(int min, int max)
        {
            return ran.Next(min, max);
        }
    }
}
