﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Configuration;
using System.Text.RegularExpressions;
using Text.Handler;
using Text.Outsider;

namespace Text.Classify
{
    class RandomSelect
    {
        static Random ran = new Random();

        /// <summary>
        /// 选择指定
        /// </summary>
        /// <param name="selectHowMany"></param>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public static bool ExecuteSelectZhengli(string resultSaveRootDic, int selectHowMany, string modelPath, string featurePath)
        {
            string path = Path.Combine(resultSaveRootDic, ConfigurationManager.AppSettings["zhengli_excel_filename"]);

            Model model = new Model();
            model.LoadModel(modelPath, featurePath);
            
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
            //content = content.Replace("\n", "。");
            //string[] sentences = content.Split('。');
            string[] sentences = TextPreProcess.SeparateParagraph(content);

            foreach (var sentence in sentences)
            {
                if (model.Predict(sentence) == 1.0)
                { zhenglis.Add(sentence); }
            }
            //model.Predict()
            return zhenglis.ToArray();
        }

        /// <summary>
        /// 还需指定工作模式
        /// Select and store fuli strings to file
        /// 执行逻辑：每次从mongodb中随机选取一个record，再从该record中依启发式规则随机选择一句话加入fuli数组
        /// 重复直到fuli达到了足够的数量
        /// </summary>
        /// <returns></returns>
        public static bool ExecuteSelectFuli(string type, string resultSaveRootDic, int selectHowMany)
        {
            //string path = Path.Combine(resultRootDic, "random_select_" + selectHowMany + "_fuli");
            string path = Path.Combine(resultSaveRootDic, ConfigurationManager.AppSettings["fuli_txt_filename"]);

            string[] selectedStrs = SelectStrsFuli(type, selectHowMany);
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
        static string[] SelectStrsFuli(string type, int selectCount)
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

                string str = SelectOneFromContentFuli(type, anaReport.Content);
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
        static string SelectOneFromContentFuli(string type, string content)
        {
            string sent = "";
            //content = content.Replace("\n", "。");
            //string[] sentences = content.Split('。');
            if (type.Equals("FLI"))
            {
                Regex likeButNotZhengli1 = new Regex(@"(每股收益|EPS|盈利预测|盈余预测)");
                Regex likeButNotZhengli2 = new Regex(@"((符合|未达|接近|超出|低于)(我们(的?))?预期|按预期完成|与预期一致)");

                string[] sentences = TextPreProcess.SeparateParagraph(content);

                int counter = 0;
                while (true)
                {
                    counter++;
                    if (counter >= 10) { break; }

                    int whichOne = RandomNumber(0, sentences.Length - 1);
                    sent = sentences[whichOne];

                    if (string.IsNullOrEmpty(sent)) { continue; }
                    if (likeButNotZhengli1.IsMatch(sent)) { return sent; }//像正例的负例
                    if (likeButNotZhengli2.IsMatch(sent)) { return sent; }//像正例的负例
                    if (sent.Contains("预测")) { continue; }
                    if (sent.Contains("预期")) { continue; }
                    if (sent.Contains("预告")) { continue; }
                    if (sent.Contains("预示")) { continue; }
                    if (sent.Contains("预计")) { continue; }
                    if (sent.Contains("估计")) { continue; }
                    if (sent.Contains("将来")) { continue; }
                    if (sent.Contains("未来")) { continue; }
                    if (sent.Contains("有望")) { continue; }
                    if (sent.Contains("可能")) { continue; }

                    break;
                }
            }
            else
            {
                string[] sentences = TextPreProcess.SeparateParagraph(content);

                int num = ran.Next(sentences.Length - 1);
                sent = sentences[num];
            }
            return sent;
        }

        static int RandomNumber(int min, int max)
        {
            return ran.Next(min, max);
        }
    }
}
