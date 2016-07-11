using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //安信证券
    public class AnXinSecurities : ReportParser
    {
        public AnXinSecurities(string pdReportPath)
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
                    base.extractStockBasicInfo();//added
                    noABCParas = removeAnyButContentInParas(mergedParas);
                    finalParas = noABCParas;
                }
                catch (Exception e)
                {
                    this.isValid = false;
                    Trace.TraceError("AnXinSecurities.AnXinSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            //Regex stockRRC = new Regex(@"(买入|增持|中性|减持|卖出)[\(（][\u4e00-\u9fa5]{2,3}[\)）]");
            Regex stockR = new Regex(@"买入|增持|中性|减持|卖出");
            Regex stockPrice = new Regex(@"\d+\.\d+");

            int index = 0;
            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasRRCMatched && hasPriceMatched) { break; }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && trimedLine.StartsWith("投资评级"))
                {
                    if (stockR.IsMatch(trimedLine))
                    {
                        anaReport.StockRating = stockR.Match(trimedLine).Value;
                        anaReport.RatingChanges = lines[index + 1].Trim().Replace("评级", "");
                        hasRRCMatched = true;
                    }
                }
                if (!hasPriceMatched && trimedLine.StartsWith("股价") && trimedLine.EndsWith("元"))
                {
                    if (stockPrice.IsMatch(trimedLine))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(trimedLine).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("AnXinSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
                index++;
            }
            if (hasRRCMatched && hasPriceMatched)
            {
                return true;
            }
            return false;
        } 

        public override string[] removeAnyButContentInLines(string[] lines)
        {
            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|((重要|特别)(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|研究员|作者)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (trimedLine.StartsWith("收益评级："))
                {
                    break;
                }
                if (InvestRatingStatement.IsMatch(trimedLine))
                {
                    break;
                }
                if (Statements.IsMatch(trimedLine))
                {
                    break;
                }
                if (FirmIntro.IsMatch(trimedLine))
                {
                    break;
                }
                if (AnalystIntro.IsMatch(trimedLine))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");
            //Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{15,} *\d{1,3}$");

            Regex extra = new Regex("(本报告版权属于安信证券股份有限公司。|各项声明请参见报告尾页。) *$");
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
                if (trimedPara.StartsWith(anaReport.StockName + "："))//added
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

            Regex regDate1 = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日 *$");//2013 年 06 月 17 日 
            Regex regDate2 = new Regex(@"报告日期： *20\d{2}-[01]\d-[0-3]\d");//报告日期： 2010-01-18

            string format1 = "yyyy年MM月dd日";
            string format2 = "报告日期：yyyy-MM-dd";

            bool hasTimeMatched = false;
            foreach (var line in lines)
            {
                string norLine = line.Replace("Table_Title", "").Trim();

                if (regDate1.IsMatch(norLine))
                {
                    string dateStr1 = regDate1.Match(norLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr1, format1, System.Globalization.CultureInfo.CurrentCulture);
                    hasTimeMatched = true;
                    break;
                }
                if (regDate2.IsMatch(norLine))
                {
                    string dateStr2 = regDate2.Match(norLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr2, format2, System.Globalization.CultureInfo.CurrentCulture);
                    hasTimeMatched = true;
                    break;
                }
            }

            if (hasTimeMatched) { return true; }
            return false;
        }
    }
}
