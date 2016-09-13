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

            //ExcelHandler exH = new ExcelHandler(@"F:\things\running\分析师报告\数据标注\7-22\INNOV-文本标注输出结果示例20160721.xlsx");
            //string[] rows2 = exH.GetColoum("工作表1", 2);
            //exH.Destroy();


            //GenerateLibSVMVector("INNOV", "于11月15日成功并购了海南金大丰矿业开发有限公司（抱伦金矿）63％的股权");
            //Process0();
            //Process1("NONINNOV");
            CalNONINNOVPrecision();
            //CalNONINNOVPrecision();
            //CalINNOVTYPEPrecision();
            //CalINNOVPrecision();
            //CalFLIEMOPrecision();
            //CalINNOVEMOPrecision();
            //CalINNOVPrecision();
            //WordSegHandler wsH = new WordSegHandler();
            //var l = wsH.GetSegmentation("考虑到发电量预测调整以及其他非经常性项目，我们将2009-11年盈利预测分别上调6%、22%和16%。");
            //CalFLIPrecision();
            //CalPrecision();
            //CalFLIINDPrecision();
            //TestWordSeg("预计下半年养殖饲料高景气不变，公司产品量价齐升相信广告业务的毛利率回归正常水平是完全可期的，年底广告资源到期后重新的谈判签约值得关注。小米公司和超预期稳中有增爆炸性的极速折让不景气的负债率和资产负债率的微信公众平台和锂电池需求将于明后年进入高速增长期，进而拉动六氟磷酸锂的需求出现爆发性增长，如符合预期，未来几年需求复合增长率在35%左右。");

            Console.WriteLine("finished");
            Console.ReadLine();
        }

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

        static void GenerateLibSVMVector(string type, string sentence)
        {
            string appSetRoot = null, appSetFile = null;
            if (!SetAppConfigName(ref appSetRoot, ref appSetFile, type))
            { return; }

            string rootDic = ConfigurationManager.AppSettings[appSetRoot];

            string tempF = Path.Combine(type, "chi_feature.txt");
            string featureFile = Path.Combine(rootDic, tempF);

            List<FeatureItem> fItems = Feature.LoadChiFeature(featureFile);

            StringBuilder sb = new StringBuilder();
            //sb.Append((lItem.label == 0) ? 0 : 1);
            var lItem = new LabeledItem(1, sentence);
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
            var str = sb.ToString();
            Console.WriteLine("");
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

        static void Process0()
        {
            RandomSelect.ExecuteSelectFuli("FLI", 271, @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\foresight\FLI", "random_select_fuli.txt");
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

        /// <summary>
        /// 为创新性文本选择负例
        /// </summary>
        /// <param name="type"></param>
        static void Process4(string type)
        {
            //string rootDic = ConfigurationManager.AppSettings[""]
            RandomSelect.ExecuteSelectFuli("INNOV", 968, @"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\INNOV", "random_select_fuli.txt");
        }

        static bool SetAppConfigName(ref string appSetRoot, ref string appSetFile, string type)
        {
            if (type.Contains("INNOV"))
            {
                appSetRoot = "excel_innovation_root_dictionary";
                appSetFile = "excel_innovation_filename";
            }
            else if (type.Contains("FLI"))
            {
                appSetRoot = "excel_foresight_root_dictionary";
                appSetFile = "excel_foresight_filename";
            }
            else
            {
                Trace.TraceError("Program.SetAppConfigName():type error");
                return false;
            }
            return true;
        }

        static void CalINNOVPrecision()
        {
            Model model = new Model("INNOV");

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("INNOV", rootForChi, true, false, false, true);
            var data = tPP.GetTrainData("INNOV", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[2];
            double[] accCount = new double[2];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key <= 0 ? 0 : dataItem.Key] += dataItem.Value.Count();//key equals -1 or 1
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("INNOV", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[1]++;
                    else if (val == 0 && dataItem.Key == 0)
                        accCount[0]++;
                    else if (dataItem.Key == 0)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\INNOV\class0_wrong.txt", wrong.ToArray());

            Console.WriteLine("class 0 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 1 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1]) / (totalCount[0] + totalCount[1])));
        }

        static void CalINNOVTYPEPrecision()
        {
            Model model = new Model("INNOVTYPE");

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("INNOVTYPE", rootForChi, true, false, false, false);
            var data = tPP.GetTrainData("INNOVTYPE", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[3];
            double[] accCount = new double[3];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key - 1] += dataItem.Value.Count();//key equals 1, 2, 3
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("INNOVTYPE", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[0]++;
                    else if (val == 2 && dataItem.Key == 2)
                        accCount[1]++;
                    else if (val == 3 && dataItem.Key == 3)
                        accCount[2]++;
                    else if (dataItem.Key == 1)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\INNOVTYPE\class1_wrong.txt", wrong.ToArray());

            Console.WriteLine("class 1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 2 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("class 3 accuracy is " + (accCount[2] / totalCount[2]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1] + accCount[2]) / (totalCount[0] + totalCount[1] + totalCount[2])));
        }

        static void CalNONINNOVPrecision()
        {
            Model model = new Model("NONINNOV");

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("NONINNOV", rootForChi, true, false, false, true);
            var data = tPP.GetTrainData("NONINNOV", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[2];
            double[] accCount = new double[2];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key <= 0 ? 0 : dataItem.Key] += dataItem.Value.Count();//key equals -1,1
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("NONINNOV", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[1]++;
                    else if (val == -1 && dataItem.Key == -1)
                        accCount[0]++;
                    else if (dataItem.Key == 1)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\NONINNOV\class1_wrong.txt", wrong.ToArray());

            Console.WriteLine("class -1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 1 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1]) / (totalCount[0] + totalCount[1])));
        }

        static void CalNONINNOVTYPEPrecision()
        {
            Model model = new Model("NONINNOVTYPE");

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("NONINNOVTYPE", rootForChi, true, false, false, false);
            var data = tPP.GetTrainData("NONINNOVTYPE", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[5];
            double[] accCount = new double[5];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key - 1] += dataItem.Value.Count();//key equals 1, 2, 3
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("NONINNOVTYPE", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[0]++;
                    else if (val == 2 && dataItem.Key == 2)
                        accCount[1]++;
                    else if (val == 3 && dataItem.Key == 3)
                        accCount[2]++;
                    else if (val == 4 && dataItem.Key == 4)
                        accCount[3]++;
                    else if (val == 5 && dataItem.Key == 5)
                        accCount[4]++;
                    else if (dataItem.Key == 1)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\NONINNOVTYPE\class1_wrong.txt", wrong.ToArray());

            Console.WriteLine("class 1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 2 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("class 3 accuracy is " + (accCount[2] / totalCount[2]));
            Console.WriteLine("class 4 accuracy is " + (accCount[3] / totalCount[3]));
            Console.WriteLine("class 5 accuracy is " + (accCount[4] / totalCount[4]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1] + accCount[2] + accCount[3] + accCount[4]) / (totalCount[0] + totalCount[1] + totalCount[2] + totalCount[3] + totalCount[4])));
        }

        static void CalINNOVEMOPrecision()
        {
            Model model = new Model("INNOVEMO");

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("INNOVEMO", rootForChi, true, false, false, false);
            var data = tPP.GetTrainData("INNOVEMO", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[3];
            double[] accCount = new double[3];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key - 1] += dataItem.Value.Count();//key equals 1,2,3
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("INNOVEMO", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[0]++;
                    else if (val == 2 && dataItem.Key == 2)
                        accCount[1]++;
                    else if (val == 3 && dataItem.Key == 3)
                        accCount[2]++;
                    else if (dataItem.Key == 3)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\INNOVEMO\class3_wrong_adv.txt", wrong.ToArray());

            Console.WriteLine("class 1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 2 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("class 3 accuracy is " + (accCount[2] / totalCount[2]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1] + accCount[2]) / (totalCount[0] + totalCount[1] + totalCount[2])));
        }

        static void CalINNOVEMOPrecision_nosvm()
        {
            SentiAnalysis sa = new SentiAnalysis();
            if (!sa.isValid)
            {
                Console.WriteLine("Program.CalPrecision(): error");
                return;
            }

            string rootForChi = ConfigurationManager.AppSettings["excel_innovation_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_innovation_filename"];//

            TextPreProcess tPP = new TextPreProcess("INNOVEMO", rootForChi, true, false, false, false);
            var data = tPP.GetTrainData("INNOVEMO", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[3];
            double[] accCount = new double[3];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key - 1] += dataItem.Value.Count();//key equals 1,2,3
                foreach (var sentence in dataItem.Value)
                {
                    double val = sa.CalSentiValue(sentence);

                    if (val > 0 && dataItem.Key == 1)
                        accCount[dataItem.Key - 1]++;
                    else if (val == 0 && dataItem.Key == 3)
                        accCount[dataItem.Key - 1]++;
                    else if (val < 0 && dataItem.Key == 2)
                        accCount[dataItem.Key - 1]++;
                    else if (dataItem.Key == 1)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\innovation\INNOVEMO\class1_wrong_nosvm.txt", wrong.ToArray());

            Console.WriteLine("class 1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 2 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("class 3 accuracy is " + (accCount[2] / totalCount[2]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1] + accCount[2]) / (totalCount[0] + totalCount[1] + totalCount[2])));
        }
        static void CalFLIEMOPrecision()
        {
            SentiAnalysis sa = new SentiAnalysis();
            if (!sa.isValid)
            {
                Console.WriteLine("Program.CalPrecision(): error");
                return;
            }

            string rootForChi = ConfigurationManager.AppSettings["excel_foresight_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_foresight_filename"];//"FLI-信息提取-样本（20160720）.xlsx";

            TextPreProcess tPP = new TextPreProcess("FLIEMO", rootForChi, true, false, true, true);
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

        static void CalFLIPrecision()
        {
            Model model = new Model("FLI");

            string rootForChi = ConfigurationManager.AppSettings["excel_foresight_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_foresight_filename"];//"FLI-信息提取-样本（20160720）.xlsx";

            TextPreProcess tPP = new TextPreProcess("FLI", rootForChi, true, false, true, false);
            var data = tPP.GetTrainData("FLI", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[2];
            double[] accCount = new double[2];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key <= 0 ? 0 : dataItem.Key] += dataItem.Value.Count();//key equals -1 or 1
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("FLI", sentence);

                    if (val ==1 && dataItem.Key == 1)
                        accCount[1]++;
                    else if (val == -1 && dataItem.Key == -1)
                        accCount[0]++;
                    else if (dataItem.Key == -1)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\foresight\FLI\class-1_wrong.txt", wrong.ToArray());

            Console.WriteLine("class -1 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 1 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1]) / (totalCount[0] + totalCount[1])));
        }

        static void CalFLIINDPrecision()
        {
            Model model = new Model("FLIIND");

            string rootForChi = ConfigurationManager.AppSettings["excel_foresight_root_dictionary"];
            string dataFilePath = ConfigurationManager.AppSettings["excel_foresight_filename"];//"FLI-信息提取-样本（20160720）.xlsx";

            TextPreProcess tPP = new TextPreProcess("FLIIND", rootForChi, true, false, true, false);
            var data = tPP.GetTrainData("FLIIND", dataFilePath);

            List<string> wrong = new List<string>();
            double[] totalCount = new double[2];
            double[] accCount = new double[2];
            foreach (var dataItem in data)
            {
                totalCount[dataItem.Key <= 0 ? 0 : dataItem.Key] += dataItem.Value.Count();//key equals 0 or 1
                foreach (var sentence in dataItem.Value)
                {
                    double val = model.AdvancedPredict("FLIIND", sentence);

                    if (val == 1 && dataItem.Key == 1)
                        accCount[1]++;
                    else if (val == 0 && dataItem.Key == 0)
                        accCount[0]++;
                    else if (dataItem.Key == 0)
                        wrong.Add(sentence);
                }
            }

            FileHandler.SaveStringArray(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\foresight\FLIIND\class0_wrong.txt", wrong.ToArray());

            Console.WriteLine("class 0 accuracy is " + (accCount[0] / totalCount[0]));
            Console.WriteLine("class 1 accuracy is " + (accCount[1] / totalCount[1]));
            Console.WriteLine("total accuracy is " + ((accCount[0] + accCount[1]) / (totalCount[0] + totalCount[1])));
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
