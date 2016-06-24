using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Text.Handler;

namespace Text
{
    class Program
    {
        static void Main(string[] args)
        {
            Feature.ChiFeatureExtract();

            Console.WriteLine();
        }

        static void Test()
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            string path = @"F:\事们\进行中\分析师报告\数据标注\FLI信息提取-样本.xlsx";
            ExcelHandler exlH = new ExcelHandler(path);
            string[] zhenglis = exlH.GetColoum("sheet1", 2);

            Dictionary<string, int> dic = new Dictionary<string, int>();
            WordSegHandler wsH = new WordSegHandler();
            if (!wsH.isValid) { Console.WriteLine("init failed"); }
            else
            {
                foreach (var zhengli in zhenglis)
                {
                    if (zhengli == null || string.IsNullOrEmpty(zhengli.Trim())) { continue; }
                    wsH.ExecutePartition(zhengli);
                    string[] result = wsH.GetAll();
                    //string[] noStopWords = wsH.GetNoStopWords();
                    //List<string> words = new List<string>(noStopWords);
                    foreach (var word in result)
                    {
                        if (dic.ContainsKey(word))
                        {
                            dic[word]++;
                        }
                        else
                        {
                            dic.Add(word, 1);
                        }
                    }
                }
            }
            var dicSort = from objDic in dic orderby objDic.Value descending select objDic;

            foreach (var line in dicSort)
            {
                Trace.TraceInformation(line.Key + ":" + line.Value);
            }

            SVM.Train.ExecuteTrain();

            Console.ReadLine();
        }

       
    }
}
