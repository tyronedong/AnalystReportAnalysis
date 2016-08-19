using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //中银国际
    public class ZhongGuoSecurities:ReportParser
    {
        public ZhongGuoSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                try
                {
                    pdfText = loadPDFText();
                    lines = pdfText.Split('\n');
                    noABCLines = removeAnyButContentInLines(lines);
                    mergedParas = mergeToParagraph(noABCLines);
                    noABCParas = removeAnyButContentInParas(mergedParas);
                    finalParas = noABCParas;
                }
                catch (Exception e)
                {
                    this.isValid = false;
                    Trace.TraceError("ZhongGuoSecurities.CommonSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockInfo()
        {
            Regex stockCode = new Regex(@"\d{6}\.[a-zA-Z]{1,4}");
            Regex stockPrice = new Regex(@"\d+\.\d+");
            bool hasCodeMatched = false, hasPriceMatched = false;

            base.extractStockBasicInfo();
            if (string.IsNullOrEmpty(anaReport.StockName) || anaReport.StockName.Equals("中国银行大厦四楼"))
            {
                anaReport.StockName = null;
                foreach (var line in lines)
                {
                    string trimedLine = line.Trim();
                    if (stockCode.IsMatch(trimedLine))
                    {
                        anaReport.StockCode = trimedLine.Split('.')[0];
                        hasCodeMatched = true;
                        break;
                    }
                }
            }
            else if (anaReport.StockName.Equals("股票代码"))
            {
                anaReport.StockName = null;
            }

            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (trimedLine.StartsWith("价格：") || trimedLine.StartsWith("收盘价 "))
                {
                    if ( stockPrice.IsMatch(trimedLine))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("ZhongGuoSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }

            return hasCodeMatched && hasPriceMatched;
        } 

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{15,} *\d{1,3}$");

            Regex extra = new Regex(@"^(\(人民币，百万\)|\(元/台，套\))");//added
            //Regex picOrTabHead = new Regex(@"^(图|表) *\d{1,2}");

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                {
                    continue;
                }
                if (indexEntry.IsMatch(trimedPara))
                {
                    continue;
                }
                if (extra.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (trimedPara.Contains("数据来源："))
                {
                    if (trimedPara.StartsWith("数据来源：")) { continue; }
                    else
                    {
                        string shuju = noteShuju.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(shuju, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (trimedPara.Contains("资料来源："))
                {
                    if (trimedPara.StartsWith("资料来源：")) { continue; }
                    else
                    {
                        string ziliao = noteZiliao.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(ziliao, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (trimedPara.Contains("注："))
                {
                    if (trimedPara.StartsWith("注：")) { continue; }
                    else
                    {
                        string zhu = noteZhu.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(zhu, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (isTableDigits(trimedPara))
                {
                    continue;
                }
                newParas.Add(para);
            }
            return newParas.ToArray();
        }

        public override bool extractDate()
        {
            if (base.extractDate())
            { return true; }

            Regex regDate1 = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日");//2009年 8月 20日 东阿阿胶    
            Regex regDate2 = new Regex(@"20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日$");//消息快报 | 证券研究报告  2012 年 4 月 24 日 

            string format1 = "yyyy年MM月dd日";
            string format2 = "yyyy年MM月dd日";

            foreach (var line in lines)
            {
                string trimedLine = line.Trim();//remove whitespace from head and tail

                if (regDate1.IsMatch(trimedLine))
                {
                    string dateStr1 = regDate1.Match(trimedLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr1, format1, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
                if (regDate2.IsMatch(trimedLine))
                {
                    string dateStr2 = regDate2.Match(trimedLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr2, format2, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
            }

            return false;
        }
    }
}
