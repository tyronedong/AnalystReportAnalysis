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
using Text.Sentiment;

namespace Text
{
    class Program
    {   
        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            //Process3("FLIEMO");
            CalPrecision();
            //TestWordSeg("预计下半年养殖饲料高景气不变，公司产品量价齐升相信广告业务的毛利率回归正常水平是完全可期的，年底广告资源到期后重新的谈判签约值得关注。小米公司和超预期稳中有增爆炸性的极速折让不景气的负债率和资产负债率的微信公众平台和锂电池需求将于明后年进入高速增长期，进而拉动六氟磷酸锂的需求出现爆发性增长，如符合预期，未来几年需求复合增长率在35%左右。");

            Console.WriteLine("finished");
            Console.ReadLine();
        }

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
        //ExecuteTrain("FLIEMO");
        //static void Test()
        //{
        //    Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
        //    Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

        //    string path = @"F:\事们\进行中\分析师报告\数据标注\FLI信息提取-样本.xlsx";
        //    ExcelHandler exlH = new ExcelHandler(path);
        //    string[] zhenglis = exlH.GetColoum("sheet1", 2);

        //    Dictionary<string, int> dic = new Dictionary<string, int>();
        //    WordSegHandler wsH = new WordSegHandler();
        //    if (!wsH.isValid) { Console.WriteLine("init failed"); }
        //    else
        //    {
        //        foreach (var zhengli in zhenglis)
        //        {
        //            if (zhengli == null || string.IsNullOrEmpty(zhengli.Trim())) { continue; }
        //            wsH.ExecutePartition(zhengli);
        //            string[] result = wsH.GetAll();
        //            //string[] noStopWords = wsH.GetNoStopWords();
        //            //List<string> words = new List<string>(noStopWords);
        //            foreach (var word in result)
        //            {
        //                if (dic.ContainsKey(word))
        //                {
        //                    dic[word]++;
        //                }
        //                else
        //                {
        //                    dic.Add(word, 1);
        //                }
        //            }
        //        }
        //    }
        //    var dicSort = from objDic in dic orderby objDic.Value descending select objDic;

        //    foreach (var line in dicSort)
        //    {
        //        Trace.TraceInformation(line.Key + ":" + line.Value);
        //    }

        //    //SVM.Train.ExecuteTrain();

        //    Console.ReadLine();
        //}

        static void ExecutePredict(string type)
        {
            string appSetRoot = null, appSetFile = null;
            if (!SetAppConfigName(ref appSetRoot, ref appSetFile, type))
            { return; }

            string rootDic = ConfigurationManager.AppSettings[appSetRoot];
            string dataFilePath = ConfigurationManager.AppSettings[appSetFile];

            string tempM = Path.Combine(type, "model.txt");
            string modelFile = Path.Combine(rootDic, tempM);
            string tempF = Path.Combine(type, "chi_feature.txt");
            string featureFile = Path.Combine(rootDic, tempF);

            Model model = new Model();
            model.LoadModel(modelFile);

            //double[] vec = model.Predict(trainSetFile);
            Feature feat = new Feature(featureFile);
            string text = "start";
            Console.WriteLine("Enter the text you want to predict. Enter 'q' for quit.");
            while (text != "q")
            {
                text = Console.ReadLine();
                double v = model.Predict(feat.GetFeatureVec(text));
                Console.WriteLine("Model predict result is: " + v);
            }
            Console.WriteLine("Process finished");
            Console.ReadLine();
        }

        static void ExecuteTrain(string type)
        {
            string appSetRoot = null, appSetFile = null;
            if (!SetAppConfigName(ref appSetRoot, ref appSetFile, type))
            { return; }

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

        static void GenerateLibSVMInputFile(string type)
        {
            string appSetRoot = null, appSetFile = null;
            if(!SetAppConfigName(ref appSetRoot, ref appSetFile, type))
            { return; }

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

        static void GenerateChiFeatureFile(string type)
        {
            if (Feature.ExtractAndStoreChiFeature(type))
                Console.WriteLine("GenerateChiFeatureFile() execute success");
            else
                Console.WriteLine("GenerateChiFeatureFile() execute failed");
        }

        static void Process1(string type)
        {
            GenerateChiFeatureFile(type);
            GenerateLibSVMInputFile(type);
            ExecuteTrain(type);
            Console.WriteLine("Process1 finished");
        }

        static void Process2(string type)
        {
            Feature.ModifyAndSaveChiFeature(type);
            GenerateLibSVMInputFile(type);
            ExecuteTrain(type);
            Console.WriteLine("Process2 finished");
        }

        static void Process3(string type)
        {
            ExecuteTrain(type);
            Console.WriteLine("Process3 finished");
        }

        static bool SetAppConfigName(ref string appSetRoot, ref string appSetFile, string type)
        {
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
                Trace.TraceError("Program.SetAppConfigName():type error");
                return false;
            }
            return true;
        }

        static void CalPrecision()
        {
            SentiAnalysis sa = new SentiAnalysis();
            if (!sa.isValid)
            {
                Console.WriteLine("Program.CalPrecision(): error");
                return;
            }

            string rootForChi = ConfigurationManager.AppSettings["excel_foresight_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_foresight_filename"];//"FLI-信息提取-样本（20160720）.xlsx";

            TextPreProcess tPP = new TextPreProcess("FLIEMO", rootForChi, true, true, true, true);
            var data = tPP.GetTrainData("FLIEMO", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[3];
            double[] accCount = new double[3];
            foreach(var dataItem in data)
            {
                totalCount[dataItem.Key - 1] += dataItem.Value.Count();
                foreach(var sentence in dataItem.Value)
                {
                    double val = sa.CalSentiValue(sentence);
                    if (val > 0 && dataItem.Key == 3)
                        accCount[dataItem.Key - 1]++;
                    else if (val == 0 && dataItem.Key == 2)
                        accCount[dataItem.Key - 1]++;
                    else if (val < 0 && dataItem.Key == 1)
                        accCount[dataItem.Key - 1]++;
                    else if (dataItem.Key == 3)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\Sentiment\class2_wrong.txt", wrong.ToArray());

            Console.WriteLine("class 1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 2 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("class 3 accuracy is " + (accCount[2] / totalCount[2]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1] + accCount[2]) / (totalCount[0] + totalCount[1] + totalCount[2])));
        }

        static void TestWordSeg(string sentence)
        {
            WordSegHandler wsH = new WordSegHandler();
            if (!wsH.isValid)
                Console.WriteLine("wrong!");

            wsH.ExecutePartition(sentence);
            string[] noSW = wsH.GetNoStopWords();
            foreach (var word in noSW)
                Console.Write(word + " ");
            Console.WriteLine();
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
