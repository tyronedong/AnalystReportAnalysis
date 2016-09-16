using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using Report;
using Report.Handler;
using Report.Outsider;
using Report.Securities;
using Text.Classify;
using Text.Sentiment;

namespace Output
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            //var L = new INNOVInfo();
            INNOVProcess();
            //FLIProcess();
            Console.WriteLine("Process finished");
            Console.ReadLine();
        }

        static void INNOVProcess()
        {
            string idFileName = ConfigurationManager.AppSettings["IdFileName"];
            string dataRootPath = ConfigurationManager.AppSettings["DataRootPath"];

            while (true)
            {
                CurIdHandler curIH = new CurIdHandler(idFileName);
                SqlServerHandler sqlSH = new SqlServerHandler();
                MongoDBHandler mgDBH = new MongoDBHandler("QueryOnly");
                //WordSegHandler wsH = new WordSegHandler();
                Model INNOVModel = new Model("INNOV");
                Model INNOVTYPEModel = new Model("INNOVTYPE");
                Model NONINNOVModel = new Model("NONINNOV");
                Model NONINNOVTYPEModel = new Model("NONINNOVTYPE");

                SentiAnalysis senti = new SentiAnalysis();
                if (!senti.LoadSentiDic())
                {
                    System.Console.WriteLine("sentiment dictionary load failed");
                    Trace.TraceError("sentiment dictionary load failed");
                    Thread.Sleep(10000);
                    continue;
                }
                if (!sqlSH.Init_INNOV())
                {
                    System.Console.WriteLine("sqlSH.Init failed");
                    Trace.TraceError("sqlSH.Init failed");
                    Thread.Sleep(10000);
                    continue;
                }
                if (!sqlSH.InitInsertTable_INNOV())
                {
                    System.Console.WriteLine("sqlSH.InitInsertTable_INNOV failed");
                    Trace.TraceError("sqlSH.InitInsertTable_INNOV failed");
                    Thread.Sleep(10000);
                    continue;
                }
                if (!mgDBH.Init())
                {
                    System.Console.WriteLine("mgDBH.Init() failed");
                    Trace.TraceError("mgDBH.Init() failed");
                    Thread.Sleep(10000);
                    continue;
                }

                bool isError; string curId, nextCurId;
                //string reportRelativeRootPath = @"{0}\{1}-{2}-{3}";
                List<AnalystReport> reports = new List<AnalystReport>();
                //HashSet<string> idSet = new HashSet<string>();

                try
                {
                    while (true)
                    {
                        isError = false;
                        //get current id in the log file
                        curId = curIH.GetCurIdFromFile();
                        if (curId == null)
                        {
                            isError = true;
                            break;
                        }
                        nextCurId = curId;
                        Console.WriteLine("Start handle reports whose id are greater than " + curId);
                        Trace.TraceInformation("Start handle reports whose id are greater than " + curId);

                        //idSet.Clear();
                        List<AnalystReport> curMReports = mgDBH.FindMany(curId);
                        if(curMReports==null)
                        {
                            isError = true;
                            break;
                        }
                        if(curMReports.Count==0)
                        {
                            isError = false;
                            Console.WriteLine("process finished");
                            break;
                        }

                        reports.Clear();
                        foreach (AnalystReport curReports in curMReports)
                        {
                            INNOVInfo innovInfo = INNOVConvert(ref INNOVModel, ref INNOVTYPEModel, ref NONINNOVModel, ref NONINNOVTYPEModel, ref senti, curReports);
                            sqlSH.AddRowToInsertTable_INNOV(innovInfo);

                            //update nextCurId
                            nextCurId = curReports._id;
                        }//for
                        if (isError)
                        {
                            Console.WriteLine("Something wrong within the inner foreach loop!");
                            Trace.TraceInformation("Something wrong within the inner foreach loop!");
                            //Thread.Sleep(10000);
                            isError = true;
                            break;
                        }
                        ////insert reports list to mongoDB
                        ////debug
                        //if (!mgDBH.InsertMany(reports))
                        //{
                        //    isError = true;
                        //    break;
                        //}
                        if (!sqlSH.ExecuteInsertTable_INNOV())
                        {
                            isError = true;
                            break;
                        }

                        //set curid to id file
                        sqlSH.ClearInsertTable_INNOV();
                        curIH.SetCurIdToFile(nextCurId);
                    }//while(true)
                    if (isError)
                    {
                        Console.WriteLine("Something wrong within the inner while loop!");
                        Trace.TraceInformation("Something wrong within the inner while loop!");
                        Thread.Sleep(10000);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Process finished!");
                        Trace.TraceInformation("Program.Execute() process finished!");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Program.Execute(): " + e.ToString());
                    Thread.Sleep(10000);
                    continue;
                }
            }//while(true)
        }

        static INNOVInfo INNOVConvert(ref Model innovModel, ref Model innovtypeModel, ref Model noninnovModel, ref Model noninnovtypeModel, ref SentiAnalysis sensor, AnalystReport anaReport)
        {
            INNOVInfo innovInfo = new INNOVInfo();

            innovInfo.guid = anaReport._id;
            innovInfo.title = anaReport.ReportTitle;
            innovInfo.rpt_date = anaReport.Date;
            if (string.IsNullOrEmpty(anaReport.Content))
                return innovInfo;

            innovInfo.stock_code = anaReport.StockCode;
            if (!string.IsNullOrEmpty(anaReport.ReportType))
            {
                if (anaReport.ReportType.Equals("特殊事项点评"))
                    innovInfo.rpt_type = "1";
                else if (anaReport.ReportType.Equals("常规报告"))
                    innovInfo.rpt_type = "2";
                else
                    innovInfo.rpt_type = "3";
            }


            if (!string.IsNullOrEmpty(anaReport.ReportTitle))
            {
                if (innovModel.AdvancedPredict("INNOV", anaReport.ReportTitle) == 0)
                    innovInfo.title_type = "2";//不包含前瞻性信息
                else
                    innovInfo.title_type = "1";//包含前瞻性信息

                double title_se = sensor.CalSentiValue(anaReport.ReportTitle);
                if (title_se > 0)
                    innovInfo.rpt_tone = "1";
                else if (title_se == 0)
                    innovInfo.rpt_tone = "3";
                else
                    innovInfo.rpt_tone = "2";
            }

            if (!string.IsNullOrEmpty(anaReport.Content))
            {
                innovInfo.isvalid = true;

                string[] sentences = GetSegSentences(anaReport.Content);
                innovInfo.text_sent_count = sentences.Count();
                innovInfo.text_char_count = anaReport.Content.Length;
                innovInfo.table_value_count = anaReport.valCountOutContent;
                innovInfo.text_value_count = anaReport.valCountInContent;

                if (anaReport.Analysts.Count() >= 1)
                {
                    innovInfo.firstauthor = anaReport.Analysts[0].Name;
                    innovInfo.firstauthor_id = anaReport.Analysts[0]._id;
                }

                foreach (string sent in sentences)
                {
                    bool isPos = false, isNeg = false;
                    double sent_val = sensor.CalSentiValue(sent);
                    if (sent_val > 0)
                    {
                        isPos = true;
                        innovInfo.rpt_pos_sent_count++;
                        innovInfo.rpt_pos_char_count += sent.Length;
                    }
                    else if (sent_val < 0)
                    {
                        isNeg = true;
                        innovInfo.rpt_neg_sent_count++;
                        innovInfo.rpt_neg_char_count += sent.Length;
                    }

                    if (innovModel.AdvancedPredict("INNOV", sent) == 1)//是创新性信息
                    {
                        if (isPos)
                        {
                            innovInfo.rpt_innov_pos_sent_count++;
                            innovInfo.rpt_innov_pos_char_count += sent.Length;
                        }
                        if (isNeg)
                        {
                            innovInfo.rpt_innov_neg_sent_count++;
                            innovInfo.rpt_innov_neg_char_count += sent.Length;
                        }

                        innovInfo.innov_sent_count++;
                        innovInfo.innov_char_count += sent.Length;

                        double innov_type = innovtypeModel.AdvancedPredict("INNOVTYPE", sent);
                        if (innov_type == 1)
                        {
                            innovInfo.innov1_sent_count++;
                            innovInfo.innov1_char_count += sent.Length;
                        }
                        else if (innov_type == 2)
                        {
                            innovInfo.innov2_sent_count++;
                            innovInfo.innov2_char_count += sent.Length;
                        }
                        else
                        {
                            innovInfo.innov3_sent_count++;
                            innovInfo.innov3_char_count += sent.Length;
                        }
                    }
                    else//不是创新性信息
                    {
                        if (noninnovModel.AdvancedPredict("NONINNOV", sent) == 1)
                        {
                            double non_innov_type = noninnovtypeModel.AdvancedPredict("NONINNOVTYPE", sent);
                            if (non_innov_type == 1)
                            {
                                innovInfo.noninnov1_sent_count++;
                                innovInfo.noninnov1_char_count += sent.Length;
                            }
                            else if (non_innov_type == 2)
                            {
                                innovInfo.noninnov2_sent_count++;
                                innovInfo.noninnov2_char_count += sent.Length;
                            }
                            else if (non_innov_type == 3)
                            {
                                innovInfo.noninnov3_sent_count++;
                                innovInfo.noninnov3_char_count += sent.Length;
                            }
                            else if (non_innov_type == 4)
                            {
                                innovInfo.noninnov4_sent_count++;
                                innovInfo.noninnov4_char_count += sent.Length;
                            }
                            else
                            {
                                innovInfo.noninnov5_sent_count++;
                                innovInfo.noninnov5_char_count += sent.Length;
                            }
                        }
                    }
                }//for loop
                if (innovInfo.innov_sent_count > 0)
                    innovInfo.has_innov = true;
                int count = innovInfo.noninnov1_sent_count + innovInfo.noninnov2_sent_count + innovInfo.noninnov3_sent_count + innovInfo.noninnov4_sent_count + innovInfo.noninnov5_sent_count;
                if (count > 0)
                    innovInfo.has_noninnov = true;
            }//if content is valid
            else innovInfo.isvalid = false;

            return innovInfo;
        }

        static void FLIProcess()
        {
            string idFileName = ConfigurationManager.AppSettings["IdFileName"];
            string dataRootPath = ConfigurationManager.AppSettings["DataRootPath"];

            while (true)
            {
                CurIdHandler curIH = new CurIdHandler(idFileName);
                SqlServerHandler sqlSH = new SqlServerHandler();
                //MongoDBHandler mgDBH = new MongoDBHandler("InsertOnly");
                //WordSegHandler wsH = new WordSegHandler();
                Model FLIModel = new Model("FLI");
                Model FLIINDModel = new Model("FLIIND");
                SentiAnalysis senti = new SentiAnalysis();
                if(!senti.LoadSentiDic())
                {
                    System.Console.WriteLine("sentiment dictionary load failed");
                    Trace.TraceError("sentiment dictionary load failed");
                    Thread.Sleep(10000);
                    continue;
                }
                if (!sqlSH.Init())
                {
                    System.Console.WriteLine("sqlSH.Init failed");
                    Trace.TraceError("sqlSH.Init failed");
                    Thread.Sleep(10000);
                    continue;
                }
                if (!sqlSH.InitInsertTable())
                {
                    System.Console.WriteLine("sqlSH.InitInsertTable failed");
                    Trace.TraceError("sqlSH.InitInsertTable failed");
                    Thread.Sleep(10000);
                    continue;
                }
                //if (!mgDBH.Init())
                //{
                //    System.Console.WriteLine("mgDBH.Init() failed");
                //    Trace.TraceError("mgDBH.Init() failed");
                //    Thread.Sleep(10000);
                //    continue;
                //}

                bool isError; string curId, nextCurId;
                string reportRelativeRootPath = @"{0}\{1}-{2}-{3}";
                List<AnalystReport> reports = new List<AnalystReport>();
                HashSet<string> idSet = new HashSet<string>();

                try
                {
                    while (true)
                    {
                        isError = false;
                        //get current id in the log file
                        curId = curIH.GetCurIdFromFile();
                        if (curId == null)
                        {
                            isError = true;
                            break;
                        }
                        nextCurId = curId;
                        Console.WriteLine("Start handle reports whose id are greater than " + curId);
                        Trace.TraceInformation("Start handle reports whose id are greater than " + curId);

                        //get data by id
                        DataTable curReportsTable = sqlSH.GetAssistTableById(curId);
                        if (curReportsTable == null)
                        {
                            isError = true;
                            break;
                        }

                        //judge if data has all been handled
                        if (curReportsTable.Rows.Count == 0)
                        {
                            isError = false;
                            break;
                        }

                        idSet.Clear();
                        reports.Clear();
                        foreach (DataRow curRow in curReportsTable.Rows)
                        {
                            string curSecur = "";
                            try
                            {
                                bool flag = false;
                                //get values in row 
                                //var time = curRow[0].ToString();
                                var id = curRow[0].ToString();
                                var time = (DateTime)curRow[1];
                                var securitiesName = curRow[3].ToString();
                                var reportName = curRow[4].ToString();
                                var language = curRow[5].ToString();
                                var stockCode = curRow[6].ToString();
                                var person1 = curRow[9].ToString();
                                var person2 = curRow[10].ToString();
                                var person3 = curRow[11].ToString();
                                curSecur = securitiesName;
                                //update nextCurId
                                nextCurId = id;

                                //judge if duplicate
                                if (idSet.Contains(id))
                                {
                                    Trace.TraceWarning("Skip duplicate analyst report whose id is: " + id);
                                    continue;
                                }
                                else
                                    idSet.Add(id);
                                //judge if current document is handlable
                                if (language.Equals("EN"))
                                {
                                    Trace.TraceWarning("Skip English analyst report whose id is: " + id);
                                    continue;
                                }
                                //find report file from directory by time and id
                                string curRootPath = Path.Combine(dataRootPath, string.Format(reportRelativeRootPath, time.Year, time.Year, time.Month, time.Day));
                                string filePath = FileHandler.GetFilePathByName(curRootPath, id);
                                if (filePath == null)
                                {
                                    //isError = true;
                                    //break;
                                    Trace.TraceWarning("file not found");
                                    continue;
                                }

                                Console.WriteLine("start handle " + securitiesName + " : " + id);
                                //get pdf file parser by securities
                                ReportParser reportParser = null;
                                //StockData stockData = null, stockParser = null;
                                if (securitiesName.Equals("长江证券"))
                                {
                                    //flag = true;
                                    reportParser = new ChangJiangSecurities(filePath);
                                }
                                else if (securitiesName.Equals("申万宏源"))
                                {
                                    //flag = true;
                                    reportParser = new ShenHongSecurities(filePath);
                                }

                                else if (securitiesName.Equals("海通证券"))
                                {
                                    //flag = true;
                                    reportParser = new HaiTongSecurities(filePath);
                                }
                                else if (securitiesName.Equals("国泰君安"))
                                {
                                    reportParser = new GuoJunSecurities(filePath);
                                }
                                else if (securitiesName.Equals("中信证券"))
                                {
                                    //flag = true;
                                    reportParser = new ZhongXinSecurities(filePath);
                                }
                                else if (securitiesName.Equals("中金公司"))
                                {
                                    reportParser = new ZhongJinSecurities(filePath);
                                }
                                else if (securitiesName.Equals("兴业证券"))
                                {
                                    //flag = true;
                                    reportParser = new XingYeSecurities(filePath);
                                }
                                else if (securitiesName.Equals("招商证券"))
                                {
                                    reportParser = new ZhaoShangSecurities(filePath);
                                }
                                else if (securitiesName.Equals("安信证券"))
                                {
                                    //flag = true;
                                    reportParser = new AnXinSecurities(filePath);
                                }
                                else if (securitiesName.Equals("广发证券"))
                                {
                                    //flag = true;
                                    reportParser = new GuangFaSecurities(filePath);
                                }
                                else if (securitiesName.Equals("天相投顾"))
                                {
                                    //flag = true;
                                    reportParser = new TianTouSecurities(filePath);
                                }
                                else if (securitiesName.Equals("国金证券"))
                                {
                                    //flag = true;
                                    reportParser = new GuoJinSecurities(filePath);
                                }
                                else if (securitiesName.Equals("华泰证券"))
                                {
                                    //flag = true;
                                    reportParser = new HuaTaiSecurities(filePath);
                                }
                                else if (securitiesName.Equals("中银国际"))
                                {
                                    //flag = true;
                                    reportParser = new ZhongGuoSecurities(filePath);
                                }
                                else if (securitiesName.Equals("东方证券"))
                                {
                                    //flag = true;
                                    reportParser = new DongFangSecurities(filePath);
                                }
                                else if (securitiesName.Equals("国信证券"))
                                {
                                    //flag = true;
                                    reportParser = new GuoXinSecurities(filePath);
                                }
                                else if (securitiesName.Equals("中信建投"))
                                {
                                    //flag = true;
                                    reportParser = new ZhongJianSecurities(filePath);
                                }
                                else if (securitiesName.Equals("平安证券"))
                                {
                                    //flag = true;
                                    reportParser = new PingAnSecurities(filePath);
                                }
                                else if (securitiesName.Equals("民生证券"))
                                {
                                    //flag = true;
                                    reportParser = new MinShengSecurities(filePath);
                                }
                                else if (securitiesName.Equals("光大证券"))
                                {
                                    //flag = true;
                                    reportParser = new GuangDaSecurities(filePath);
                                }
                                else if (securitiesName.Equals("东北证券"))
                                {
                                    //flag = true;
                                    reportParser = new DongBeiSecurities(filePath);
                                }
                                else if (securitiesName.Equals("东兴证券"))
                                {
                                    //flag = true;
                                    reportParser = new DongXingSecurities(filePath);
                                }
                                else if (securitiesName.Equals("方正证券"))
                                {
                                    //flag = true;
                                    reportParser = new FangZhengSecurities(filePath);
                                }
                                else if (securitiesName.Equals("申银万国"))
                                {
                                    //flag = true;
                                    reportParser = new ShenWanSecurities(filePath);
                                }
                                else
                                {
                                    //if (securitiesName.Equals("民生证券")) { flag = true; }
                                    reportParser = new CommonSecurities(filePath);
                                }


                                AnalystReport curAnReport = new AnalystReport();
                                //handle the data
                                if (reportParser != null)
                                {
                                    if (reportParser.isValid)
                                    {
                                        curAnReport = reportParser.executeExtract_withdb();
                                        Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, stockCode, person1, person2, person3);

                                        reportParser.CloseAll();
                                    }
                                    else
                                    {
                                        //isError = true;
                                        //break;
                                        Trace.TraceError(securitiesName + " : " + id + " is unvalid (init instance failed)");
                                        nextCurId = id;
                                        continue;
                                    }
                                }
                                else
                                {
                                    nextCurId = id;
                                    continue;
                                }

                                //reports.Add(curAnReport);
                                FLIInfo fliInfo = FLIConvert(ref FLIModel, ref FLIINDModel, ref senti, curAnReport);
                                sqlSH.AddRowToInsertTable(fliInfo);
                                //if (flag)
                                //{
                                //    System.Console.WriteLine("Hello");
                                //}
                                //update nextCurId
                                nextCurId = id;
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError("Program.Execute() for loop when handle " + curSecur + " :" + e.ToString());
                                continue;
                            }
                        }//for
                        if (isError)
                        {
                            Console.WriteLine("Something wrong within the inner foreach loop!");
                            Trace.TraceInformation("Something wrong within the inner foreach loop!");
                            //Thread.Sleep(10000);
                            isError = true;
                            break;
                        }
                        ////insert reports list to mongoDB
                        ////debug
                        //if (!mgDBH.InsertMany(reports))
                        //{
                        //    isError = true;
                        //    break;
                        //}
                        if(!sqlSH.ExecuteInsertTable())
                        {
                            isError = true;
                            break;
                        }
                        
                        //set curid to id file
                        sqlSH.ClearInsertTable();
                        curIH.SetCurIdToFile(nextCurId);
                    }//while(true)
                    if (isError)
                    {
                        Console.WriteLine("Something wrong within the inner while loop!");
                        Trace.TraceInformation("Something wrong within the inner while loop!");
                        Thread.Sleep(10000);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Process finished!");
                        Trace.TraceInformation("Program.Execute() process finished!");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Program.Execute(): " + e.Message);
                    Thread.Sleep(10000);
                    continue;
                }
            }//while(true)
        }

        static FLIInfo FLIConvert(ref Model FLIModel, ref Model INDModel, ref SentiAnalysis sensor, AnalystReport anaReport)
        {
            FLIInfo fliInfo = new FLIInfo();

            fliInfo.guid = anaReport._id;
            fliInfo.stockcd = anaReport.StockCode;
            fliInfo.rptdate = anaReport.Date;

            if (!string.IsNullOrEmpty(anaReport.Content))//正文不空，表示该报告有效
            {
                fliInfo.isvalid = true;
                if (anaReport.ReportType.Equals("特殊事项点评"))
                    fliInfo.typecd = "1";
                else if (anaReport.ReportType.Equals("常规报告"))
                    fliInfo.typecd = "2";
                else
                    fliInfo.typecd = "3";
                //fliInfo.typecd = anaReport.ReportType;
                fliInfo.graph = anaReport.tableCount + anaReport.picCount;

                //判断标题是否是前瞻性语句
                if (FLIModel.AdvancedPredict("FLI", anaReport.ReportTitle) == 1)
                    fliInfo.flt = true;
                else
                    fliInfo.flt = false;

                //判断标题是否积极语气前瞻性
                if (fliInfo.flt)
                {
                    if (sensor.CalSentiValue(anaReport.ReportTitle) > 0)
                        fliInfo.flt_tone = true;
                    else
                        fliInfo.flt_tone = false;
                }
                //else
                //    fliInfo.flt_tone = false;

                string[] sentences = anaReport.Content.Replace("\n", "").Split('。');
                //计算研报正文句子总数
                fliInfo.tots = sentences.Length;
                //计算正文中积极消极语气总数以及各类数
                int poss = 0, negs = 0, totfls = 0, posfls = 0, negfls = 0;
                int totfls_ind = 0, totfls_firm = 0, totnfls = 0, posnfls = 0, negnfls = 0;
                foreach (var sentence in sentences)
                {
                    double sentiVal = sensor.CalSentiValue(sentence);
                    bool isPos = sentiVal > 0;//判断语气是否积极
                    bool isNeg = sentiVal < 0;//判断语气是否消极
                    if (isPos)
                        poss++;//积极语气
                    else if (isNeg)
                        negs++;//消极语气

                    if (FLIModel.AdvancedPredict("FLI", sentence) == 1)
                    {
                        totfls++;//广义前瞻性语句总数

                        if (isPos)
                            posfls++;//积极前瞻性
                        else if (isNeg)
                            negfls++;//消极前瞻性

                        if (INDModel.AdvancedPredict("FLIIND", sentence) == 1)
                            totfls_ind++;//行业前瞻性
                        else
                            totfls_firm++;//企业前瞻性

                        if (isNarrowSense(sentence))
                        {
                            totnfls++;//狭义前瞻性

                            if (isPos)
                                posnfls++;//积极狭义
                            else if (isNeg)
                                negnfls++;//消极狭义
                        }
                    }
                }
                fliInfo.poss = poss;
                fliInfo.negs = negs;
                fliInfo.totfls = totfls;
                fliInfo.posfls = posfls;
                fliInfo.negfls = negfls;
                fliInfo.totfls_ind = totfls_ind;
                fliInfo.totfls_firm = totfls_firm;
                fliInfo.totnfls = totnfls;
                fliInfo.posnfls = posnfls;
                fliInfo.negnfls = negnfls;
            }
            else
            {
                fliInfo.isvalid = false;
            }

            return fliInfo;
        }

        static bool isNarrowSense(string sentence)
        {
            Regex narrowSense = new Regex(@"我们预计|我们预期|我们预测|我们推测|我们测算|我们看好|我们认为|我们相信|预计|预期|推测|看好|相信");

            return narrowSense.IsMatch(sentence);
        }

        /// <summary>
        /// 获取正规化的句子列表
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        static string[] GetSegSentences(string document)
        {
            char[] separator = { '。', '？', '！' };
            document = document.Replace("\n", "。");//替换成‘。'有问题吧
            //替换成‘。’没有问题
            string[] sents = document.Split(separator);
            List<string> normalized_sent = new List<string>();
            foreach(string sent in sents)
            {
                if (string.IsNullOrEmpty(sent) || string.IsNullOrWhiteSpace(sent))
                    continue;
                normalized_sent.Add(sent);
            }
            return normalized_sent.ToArray();
        }

    }
}
