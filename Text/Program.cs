using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Text.Handler;
using Text.Classify;
using Text.Classify.Item;

namespace Text
{
    class Program
    {
        static string preprocess_result_file = ConfigurationManager.AppSettings["preprocess_result_file"];

        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例


            //List<FeatureItem> fItems = Feature.LoadChiFeature(fileName);
            //Test2();

            //SelectAndSaveFulis();
            //ExtractAndSaveChiFeatures();
            //GenerateLibSVMInputFile();
            //ExecuteTrain();
            ExecutePredict();

            Console.ReadLine();
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

            //SVM.Train.ExecuteTrain();

            Console.ReadLine();
        }

        static void ExecutePredict()
        {
            //string trainSetFile = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\trainset.txt";
            string modelFile = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\model.txt";
            string text = "目前期间费用因上市不久故发生的管理较高，占收入比约在9% -10%10%左右，后续有望降低。";
            Model model = new Model();
            model.LoadModel(modelFile);

            //double[] vec = model.Predict(trainSetFile);
            double v = model.Predict(Feature.GetFeatureVec(text));
            //model.Predict()
            Console.WriteLine("Model predict finished");
        }

        static void ExecuteTrain()
        {
            string trainSetFile = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\trainset.txt";
            string modelFile = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\model.txt";
            Model model = new Model();
            model.Train(trainSetFile);

            model.SaveModel(modelFile);
            Console.WriteLine("Model train finished");
        }

        static void GenerateLibSVMInputFile()
        {
            string fileName = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\selected_chi_features_with_random_select_fulis.txt";
            string trainSetFile = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\trainset.txt";

            List<string> trainSet = new List<string>();

            List<FeatureItem> fItems = Feature.LoadChiFeature(fileName);
            List<LabeledItem> lItems = FileHandler.LoadLabeledItems(preprocess_result_file);

            foreach (var lItem in lItems)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(lItem.label);
                sb.Append(' ');
                int idx = 1;
                foreach (var fItem in fItems)
                {
                    if (lItem.wordCountDic.ContainsKey(fItem.featureWord))
                    {
                        sb.Append(idx);
                        sb.Append(':');
                        double tfidf = lItem.wordCountDic[fItem.featureWord] * fItem.globalWeight;
                        sb.Append(tfidf.ToString("0.000000"));
                        sb.Append(' ');
                    }
                    else { }//do nothing//sb.Append(0.0); }
                    idx++;
                }
                trainSet.Add(sb.ToString());
            }
            if (FileHandler.SaveStringArray(trainSetFile, trainSet.ToArray()))
                Console.WriteLine("GenerateLibSVMInputFile() execute success");
            else
                Console.WriteLine("GenerateLibSVMInputFile() execute failed");
        }

        static void ExtractAndSaveChiFeatures()
        {
            string fileName = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\selected_chi_features_with_random_select_fulis.txt";
            if (Feature.ChiFeatureExtract(fileName))
                Console.WriteLine("ExtractAndSaveChiFeatures() execute success");
            else
                Console.WriteLine("ExtractAndSaveChiFeatures() execute failed");
        }

        static void SelectAndSaveFulis()
        {
            if (RandomSelect.ExecuteSelect())
                Console.WriteLine("SelectAndSaveFulis() execute success");
            else
                Console.WriteLine("SelectAndSaveFulis() execute failed");
        }
    }
}
