using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using Report.Handler;
using Report.Outsider;
using Report.Securities;

namespace Report.Output
{
    class Executor
    {
        static void FLIProcess()
        {
            string idFileName = ConfigurationManager.AppSettings["IdFileName"];
            string dataRootPath = ConfigurationManager.AppSettings["DataRootPath"];

            while (true)
            {
                CurIdHandler curIH = new CurIdHandler(idFileName);
                SqlServerHandler sqlSH = new SqlServerHandler();
                //MongoDBHandler mgDBH = new MongoDBHandler("InsertOnly");
                WordSegHandler wsH = new WordSegHandler();
                if (!sqlSH.Init())
                {
                    System.Console.WriteLine("sqlSH.Init() failed");
                    Trace.TraceError("sqlSH.Init() failed");
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
                                        if (true)
                                        {
                                            curAnReport = reportParser.executeExtract_withdb();
                                            Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, stockCode,person1, person2, person3);
                                        }

                                        //if (false)
                                        //{
                                        //    curAnReport = reportParser.executeExtract_nodb(ref wsH);
                                        //    curAnReport.Brokerage = securitiesName;
                                        //}

                                        reportParser.CloseAll();
                                    }
                                    else
                                    {
                                        //isError = true;
                                        //break;
                                        Trace.TraceError(securitiesName + " is unvalid");
                                        nextCurId = id;
                                        continue;
                                    }
                                }
                                else
                                {
                                    nextCurId = id;
                                    continue;
                                }

                                reports.Add(curAnReport);
                                //if (flag)
                                //{
                                //    System.Console.WriteLine("Hello");
                                //}
                                //update nextCurId
                                nextCurId = id;
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError("Program.Execute() for loop when handle " + curSecur + " :" + e.Message);
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

                        //set curid to id file
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

        //static FLIInfo FLIConvert(ref  fliModel, AnalystReport anaReport)
        //{
        //    FLIInfo fliInfo = new FLIInfo();

        //    fliInfo.guid = anaReport._id;
        //    fliInfo.stockcd = anaReport.StockCode;
        //    fliInfo.rptdate = anaReport.Date;
        //    fliInfo.typecd = anaReport.ReportType;
        //    fliInfo.graph = anaReport.tableCount + anaReport.picCount;
            
        //    //判断标题是否是前瞻性语句

        //    return null;
        //}
    }
}
