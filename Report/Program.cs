using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Report.Handler;
using Report.Securities;
using Report.Outsider;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.graphics.xobject;

namespace Report
{
    public class Program
    {
        private static string idFileName = ConfigurationManager.AppSettings["IdFileName"];
        private static string dataRootPath = ConfigurationManager.AppSettings["DataRootPath"];

        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            //Execute();

            //System.Console.ReadLine();

            Some();

            System.Console.ReadLine();
        }

        static bool Some()
        {
            string path = @"D:\人民大学\分析师报告\2010\2010-6-8\01E0192C-12D7-4035-A0E4-85B1F73A3AB7.PDF";
            string path2 = @"F:\things\running\分析师报告\分析师研报\分析师报告\000513\20130826-银河证券-丽珠集团-000513-业绩基本符合预期，新品贡献度有望提升.pdf";
            string path3 = @"F:\things\running\分析师报告\分析师研报\分析师报告\002317\20091211-大通证券-众生药业-002317-上市定价报告.pdf";
            string path4 = @"D:\人民大学\分析师报告\2011\2011-8-16\28406CC8-29EB-4C62-B1E5-B3CD7DBC1622.PDF";
            string imgSavePath = @"F:\things\running\分析师报告\";

            //PDDocument document = PDDocument.load(path4);
            
            //PDDocumentInformation info = document.getDocumentInformation();

            //var title = info.getTitle();
            //var sub = info.getSubject();
            //var au = info.getAuthor();
            //var key = info.getKeywords();
            //var t = info.getTrapped();

            ///** 文档页面信息 **/
            //PDDocumentCatalog cata = document.getDocumentCatalog();
            //java.util.List pages = cata.getAllPages();
            //int count = 1;
            //for (int i = 0; i < pages.size(); i++)
            //{
            //    PDPage page = (PDPage)pages.get(i);
            //    if (page != null)
            //    {
            //        //PDResources res = page.findResources();
            //        PDResources res = page.getResources();
            //        //获取页面图片信息  
            //        java.util.Map imgs = res.getImages();
            //        if (imgs != null)
            //        {
            //            var keySet = imgs.keySet();
            //            var it = keySet.iterator();
            //            while (it.hasNext())
            //            {
            //                string key = it.next().ToString();
            //                PDXObjectImage img = (PDXObjectImage)imgs.get(key);
            //                try
            //                {
            //                    img.write2file(imgSavePath + count);
            //                }
            //                catch (Exception e)
            //                {
            //                    count++;
            //                    continue;
            //                }
            //                count++;
            //            }
            //        }
            //    }
            //}  

            //Console.WriteLine(info.toString());

            string t = ReportParser.loadPDFText(path4);
            //ReportParser rp = new ZhongJinSecurities(path);
            //WordSegHandler wsH = new WordSegHandler();
            //var r = rp.executeExtract_nodb(ref wsH);


            //string format1 = "yyyy年MM月dd日";
            //string format2 = "报告日期：yyyy-MM-dd";
            //string format3 = "yyyy-M-d";

            //string dateStr1 = "2013-9-9";
            //DateTime d = DateTime.ParseExact(dateStr1, format3, System.Globalization.CultureInfo.CurrentCulture);
            
           
            return false;
        }

        static bool Execute()
        {
            while (true)
            {
                CurIdHandler curIH = new CurIdHandler(idFileName);
                SqlServerHandler sqlSH = new SqlServerHandler();
                MongoDBHandler mgDBH = new MongoDBHandler("InsertOnly");
                WordSegHandler wsH = new WordSegHandler();
                if (!sqlSH.Init())
                {
                    System.Console.WriteLine("sqlSH.Init() failed");
                    Trace.TraceError("sqlSH.Init() failed");
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
                        DataTable curReportsTable = sqlSH.GetTableById(curId);
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
                                var time = (DateTime)curRow[0];
                                var id = curRow[1].ToString();
                                var securitiesName = curRow[2].ToString();
                                var reportName = curRow[3].ToString();
                                var language = curRow[4].ToString();
                                var person1 = curRow[5].ToString();
                                var person2 = curRow[6].ToString();
                                var person3 = curRow[7].ToString();
                                curSecur = securitiesName;
                                //update nextCurId
                                nextCurId = id;
                                //if (time.Year != 2013)
                                //{
                                //    continue;
                                //}
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
                                            SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, null, person1, person2, person3);
                                        }

                                        if (false)
                                        {
                                            curAnReport = reportParser.executeExtract_nodb(ref wsH);
                                            curAnReport.Brokerage = securitiesName;
                                        }
                                        
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
                                //else if (stockParser != null)
                                //{
                                //    stockParser.extrcactContent();
                                //    //stockParser.extractDetail(stockParser.loadPDFLines());
                                //    DataTransform(ref stockParser, ref curAnReport);
                                //    SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                                //}
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
                        //insert reports list to mongoDB
                        //debug
                        if (!mgDBH.InsertMany(reports))
                        {
                            isError = true;
                            break;
                        }

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
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anaReport"></param>
        /// <param name="sqlSH"></param>
        /// <param name="pdFileName"></param>
        /// <param name="reportTitle"></param>
        /// <param name="jobber"></param>
        /// <param name="time"></param>
        /// <param name="person1"></param>
        /// <param name="person2"></param>
        /// <param name="person3"></param>
        public static void SetExistedInfo(ref AnalystReport anaReport, ref SqlServerHandler sqlSH, string pdFileName, string reportTitle, string jobber, DateTime time, string stockCode, string person1, string person2, string person3)
        {
            anaReport._id = pdFileName;
            anaReport.ReportTitle = reportTitle;
            //if (string.IsNullOrEmpty(anaReport.ReportTitle))
            //{ anaReport.ReportTitle = reportTitle; }
            anaReport.PDFileName = pdFileName;
            anaReport.Brokerage = jobber;
            anaReport.Date = time;
            if (stockCode != null)
                anaReport.StockCode = stockCode;
            anaReport.Analysts = sqlSH.GetAnalysts(person1, person2, person3);
            if (anaReport.StockName == null && reportTitle.Contains("："))
                anaReport.StockName = reportTitle.Split('：')[0];
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="stockData"></param>
        ///// <param name="anaReport"></param>
        ///// <returns></returns>
        //public static bool DataTransform(ref StockData stockData, ref AnalystReport anaReport)
        //{
        //    anaReport.Content = ContentTransform(stockData.Content);
        //    //anaReport.Content = stockData.Content;
        //    anaReport.RatingChanges = stockData.RatingChanges;
        //    anaReport.StockCode = stockData.StockCode;
        //    anaReport.StockName = stockData.StockName;
        //    anaReport.StockPrice = stockData.StockPrice;
        //    anaReport.StockRating = stockData.StockRating;
        //    return true;
        //}

        public static string ContentTransform(string content)
        {
            string[] paras = content.Split('\n');

            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|(重要(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|研究员|作者)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");
            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (InvestRatingStatement.IsMatch(trimedPara))
                {
                    break;
                }
                if (Statements.IsMatch(trimedPara))
                {
                    break;
                }
                if (FirmIntro.IsMatch(trimedPara))
                {
                    break;
                }
                if (AnalystIntro.IsMatch(trimedPara))
                {
                    break;
                }
                newParas.Add(para);
            }

            string transformedContent = "";
            foreach (var newPara in newParas)
            {
                transformedContent += newPara.Trim() + "\n";
            }
            return transformedContent;
        }
    }
}