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
using Spire.Pdf;
using org.apache.pdfbox;
using org.apache.pdfbox.cos;
using org.apache.pdfbox.util;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdfparser;
using java.io;
using java.util;
using Stock;

namespace Report
{
    class Program
    {
        private static string idFileName = ConfigurationManager.AppSettings["IdFileName"];
        private static string dataRootPath = ConfigurationManager.AppSettings["DataRootPath"];

        static void Main(string[] args)
        {
            Trace.Listeners.Clear();  //清除系统监听器 (就是输出到Console的那个)
            Trace.Listeners.Add(new TraceHandler()); //添加MyTraceListener实例

            Execute();

            System.Console.WriteLine("hello");
        }

        static bool Execute()
        {
            CurIdHandler curIH = new CurIdHandler(idFileName);
            SqlServerHandler sqlSH = new SqlServerHandler();
            if (!sqlSH.Init())
            {
                throw new Exception("sqlSH.Init() failed");
            }

            bool isError; string curId, nextCurId;
            string reportRelativeRootPath = @"{0}\{1}-{2}-{3}";
            List<AnalystReport> reports = new List<AnalystReport>();

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
                    break;
                }

                reports.Clear();
                foreach (DataRow curRow in curReportsTable.Rows)
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
                    //update nextCurId
                    nextCurId = id;
                    if (time.Year != 2013)
                    {
                        continue;
                    }
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
                        continue;
                    }

                    //get pdf file parser by securities
                    ReportParser reportParser = null;
                    StockData stockData = null, stockParser = null; 
                    if (securitiesName.Equals("国泰君安"))
                    {
                        reportParser = new GuoJunSecurities(filePath);
                    }
                    else if (securitiesName.Equals("中金公司"))
                    {
                        reportParser = new ZhongJinSecurities(filePath);
                    }
                    else if (securitiesName.Equals("招商证券"))
                    {
                        reportParser = new ZhaoShangSecurities(filePath);
                    }
                    //else if (securitiesName.Equals("东北证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    //stockData.setStockjobber("东北证券");
                    //    stockParser = new DongBeiStock(stockData);
                    //    //stockParser.extrcactContent();
                    //}
                    //else if (securitiesName.Equals("东兴证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    stockParser = new DongXingStock(stockData);
                    //}
                    //else if (securitiesName.Equals("方正证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    stockParser = new FangZhengStock(stockData);
                    //}
                    //else if (securitiesName.Equals("平安证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    stockParser = new PingAnStock(stockData);
                    //}
                    //else if (securitiesName.Equals("兴业证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    stockParser = new XingYeStock(stockData);
                    //}
                    //else if (securitiesName.Equals("长江证券"))
                    //{
                    //    stockData = new StockData(filePath);
                    //    stockParser = new ChangJiangStock(stockData);
                    //}
                    //else
                    //{
                    //    flag = true;
                    //    reportParser = new CommonSecurities(filePath);
                    //}
                    
                    AnalystReport curAnReport = new AnalystReport();
                    //handle the data
                    if (reportParser != null)
                    {
                        if (reportParser.isValid)
                        {
                            curAnReport = reportParser.executeExtract();
                            SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                            reportParser.CloseAll();
                        }
                        else
                        {
                            isError = true;
                            break;
                        }
                    }
                    else if (stockParser != null)
                    {
                        stockParser.extrcactContent();
                        //stockParser.extractDetail(stockParser.loadPDFLines());
                        DataTransform(ref stockParser, ref curAnReport);
                        SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
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
                    
                }//for
                //insert reports list to mongoDB
                
                
                //set curid to id file
                curIH.SetCurIdToFile(nextCurId);
            }//while(true)

            if (isError)
            {
                return false;
            }
            else
            {
                return true;
            }
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
        static void SetExistedInfo(ref AnalystReport anaReport, ref SqlServerHandler sqlSH, string pdFileName, string reportTitle, string jobber, DateTime time, string person1, string person2, string person3)
        {
            anaReport.ReportTitle = reportTitle;
            anaReport.PDFileName = pdFileName;
            anaReport.Stockjobber = jobber;
            anaReport.Date = time;
            anaReport.Analysts = sqlSH.GetAnalysts(person1, person2, person3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockData"></param>
        /// <param name="anaReport"></param>
        /// <returns></returns>
        static bool DataTransform(ref StockData stockData, ref AnalystReport anaReport)
        {
            anaReport.Content = stockData.Content;
            anaReport.RatingChanges = stockData.RatingChanges;
            anaReport.StockCode = stockData.StockCode;
            anaReport.StockName = stockData.StockName;
            anaReport.StockPrice = stockData.StockPrice;
            anaReport.StockRating = stockData.StockRating;
            return true;
        }
    }
}


//string text = buffer.ToString();
//string path_2 = "F:\\桌面文件备份\\mission\\分析师报告\\sample\\02338FE9-0DD6-4299-A2CE-50D4B8DBDE74.PDF";
//string path_1 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告\\300039\\20120330-招商证券-上海凯宝-300039-产能瓶颈得到解决,业绩释放值得期待.pdf";
//string path_guojun_1 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告\\300039\\20150528-国泰君安-上海凯宝-300039-主业平稳增长，外延动力十足.pdf";
//string path_guojun_2 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\国泰君安\\015CF7D4-3E6F-4673-9B70-DB21994E71DE.PDF";
//string path_guojun_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\国泰君安\\2015\\01EF95BC-B1CF-46F8-A7A1-985D8AFE4EFC.PDF";
//string path_zhongxin_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中信证券\\2014\\CF3AB7B1-2A50-4506-A82C-8BAA9D78804D.PDF";
//string path_zhongjin_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2015\\20150319-中金公司-华润三九-000999-经营触底，趋势向上.pdf";
//string path_zhongjin_15_2 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2015\\20150826-中金公司-华兰生物-002007-业绩稳定增长，浆站拓展顺利.pdf";
//string path_zhongjin_13 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2013\\20130228-中金公司-上海凯宝-300039-业绩回顾：稳定增长现金牛公司.pdf";
//string path_changjiang_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\长江证券\\2011\\20110304-长江证券-益盛药业-002566-产品线丰富,拥有人参系列特色产品的中药企业.pdf";
//try
//{
//    PDDocument doc = PDDocument.load("D:\\aa\\b.pdf");
//}
//catch (Exception e)
//{
//    System.Console.WriteLine("hello");
//}
//ZhongJinSecurities zx = new ZhongJinSecurities(doc);
//string s = zx.loadPDFText();
//zx.extractStockBasicInfo();
//string s = zx.loadPDFText();
//PDFTextStripper pdfStripper = new PDFTextStripper("unicode");
//string text = pdfStripper.getText(doc).Replace("\r\n", "\n");
//zx.extractContent();