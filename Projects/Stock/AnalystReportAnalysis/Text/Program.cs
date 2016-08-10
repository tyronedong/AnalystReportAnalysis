using System;
using System.IO;
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
        //static string preprocess_result_file = ConfigurationManager.AppSettings["preprocess_result_file"];
        static string featureRootPath = ConfigurationManager.AppSettings["feature_relate_root_dictionary"];
        
        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            //Bootstrap.ExecuteBootstrap(3000);
            //Feature.ExtractAndStoreChiFeature(featurePath);
            //List<FeatureItem> fItems = Feature.LoadChiFeature(fileName);
            //Test2();
            //Feature.ModifyChiFeature(featurePath);
            //SelectAndSaveFulis();
            //ExtractAndSaveChiFeatures();
            //GenerateLibSVMInputFile();
            //ExecuteTrain();
            //ExecutePredict();
            //string rootDicForModelRelate = ConfigurationManager.AppSettings["model_relate_root_dictionary"];
            //Model.GenerateTrainSet(rootDicForModelRelate);

            //GenerateLibSVMInputFile();
            //ExecuteTrain();

            //RandomSelect.ExecuteSelectFuli("INNOV", featureRootPath, 300);
            //GenerateChiFeatureWord("FLIEMO");
            //GenerateLibSVMInputFile("FLIEMO");
            //ExecuteTrain();
            //Feature.ExtractAndStoreChiFeature("FLIEMO", featureRootPath);

            //Feature.ExtractAndStoreChiFeature("FLIEMO");
            //GenerateLibSVMInputFile("FLIEMO");
            ExecuteTrain("FLIEMO");

            Console.WriteLine("finished");
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
            string modelFile = ConfigurationManager.AppSettings["model_path"];
            string featureFile = ConfigurationManager.AppSettings["chi_feature_path"];
            string text = "目前期间费用因上市不久故发生的管理较高，占收入比约在9% -10%左右，后续有望降低。";
            Model model = new Model();
            model.LoadModel(modelFile);

            //double[] vec = model.Predict(trainSetFile);
            Feature feat = new Feature(featureFile);
            double v = model.Predict(feat.GetFeatureVec(text));
            //model.Predict()
            Console.WriteLine("Model predict finished");
        }

        static void ExecuteTrain(string type)
        {
            string appSetRoot, appSetFile;
            if (type.Equals("FLI") || type.Equals("FLIEMO"))
            {
                appSetRoot = "excel_foresight_root_dictionary";
                appSetFile = "excel_foresight_filename";
            }
            else if (type.Equals("INNOVTYPE") || type.Equals("INNOVSTAGE") || type.Equals("INNOVEMO") || type.Equals("NONINNOVTYPE"))
            {
                appSetRoot = "excel_innovation_root_dictionary";
                appSetFile = "excel_innovation_filename";
            }
            else
            {
                Trace.TraceError("error");
                return;
            }

            string rootDic = ConfigurationManager.AppSettings[appSetRoot];
            string dataFilePath = ConfigurationManager.AppSettings[appSetFile];

            string tempT = Path.Combine(type, "train_set.txt");
            string trainSetFile = Path.Combine(rootDic, tempT);
            string tempM = Path.Combine(type, "model.txt");
            string modelFile = Path.Combine(rootDic, tempM);

            Model model = new Model();
            model.Train(trainSetFile);

            model.SaveModel(modelFile);
            Console.WriteLine("Model train finished");
        }

        static void GenerateChiFeatureWord(string type)
        {
            bool tOrf = false;

            if (type.Equals("FLIEMO"))
            {
                tOrf = Feature.ExtractAndStoreChiFeature(type);
            }
            else if (type.Equals("FLI"))
            {

            }
            else if(type.Equals("INNOV"))
            {
                tOrf = Feature.ExtractAndStoreChiFeature(type);
            }
        }

        static void GenerateLibSVMInputFile(string type)
        {
            string appSetRoot, appSetFile;
            if(type.Equals("FLI")||type.Equals("FLIEMO"))
            {
                appSetRoot = "excel_foresight_root_dictionary";
                appSetFile = "excel_foresight_filename";
            }
            else if(type.Equals("INNOVTYPE")||type.Equals("INNOVSTAGE")||type.Equals("INNOVEMO")||type.Equals("NONINNOVTYPE"))
            {
                appSetRoot = "excel_innovation_root_dictionary";
                appSetFile = "excel_innovation_filename";
            }
            else
            {
                Trace.TraceError("error");
                return;
            }

            string rootDic = ConfigurationManager.AppSettings[appSetRoot];
            string dataFilePath = ConfigurationManager.AppSettings[appSetFile];

            string tempF = Path.Combine(type, "chi_feature.txt");
            string featureFile = Path.Combine(rootDic, tempF);
            string tempT = Path.Combine(type, "train_set.txt");
            string trainSetFile = Path.Combine(rootDic, tempT);
            

            List<string> trainSet = new List<string>();

            List<FeatureItem> fItems = Feature.LoadChiFeature(featureFile);
            TextPreProcess tPP = new TextPreProcess(type, rootDic, true, false, true, true);
            List<LabeledItem> lItems = tPP.GetLabeledItems(dataFilePath);//FileHandler.LoadLabeledItems(preprocess_result_file);

            foreach (var lItem in lItems)
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append((lItem.label == 0) ? 0 : 1);
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

        //static void ExtractAndSaveChiFeatures()
        //{
        //    string fileName = @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\selected_chi_features_with_random_select_fulis.txt";
        //    if (Feature.ChiFeatureExtract(fileName))
        //        Console.WriteLine("ExtractAndSaveChiFeatures() execute success");
        //    else
        //        Console.WriteLine("ExtractAndSaveChiFeatures() execute failed");
        //}

        //static void SelectAndSaveFulis()
        //{
        //    if (RandomSelect.ExecuteSelectFuli(300))
        //        Console.WriteLine("SelectAndSaveFulis() execute success");
        //    else
        //        Console.WriteLine("SelectAndSaveFulis() execute failed");
        //}
    }
}
