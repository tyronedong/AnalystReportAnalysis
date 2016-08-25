using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class GuoJinSecurities : ReportParser
    {
        //国金证券
        public GuoJinSecurities(string pdReportPath)
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
                    Trace.TraceError("GuoJinSecurities.GuoJinSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            Regex stockR = new Regex(@"买入|增持|中性|减持");
            Regex stockRC = new Regex(@"[\u4e00-\u9fa5a]+评级");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");

            bool hasRMatched = false, hasRCMatched = false, hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasRMatched && hasRCMatched && hasPriceMatched) { break; }
                string trimedLine = line.Trim();
                if (trimedLine.StartsWith("评级："))
                {
                    if (stockR.IsMatch(trimedLine))
                    {
                        anaReport.StockRating = stockR.Match(trimedLine).Value;
                        hasRMatched = true;
                    }
                    if (stockRC.IsMatch(trimedLine))
                    {
                        anaReport.RatingChanges = stockRC.Match(trimedLine).Value.Replace("评级", "");
                        hasRCMatched = true;
                    }
                }
                if (trimedLine.StartsWith("市价"))
                {
                    if (stockPrice.IsMatch(trimedLine))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("GuoJinSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }

            hasRRCMatched = hasRMatched && hasRCMatched;

            if (!hasRRCMatched)//如果没有匹配成功，则调用基类的方法
                hasRRCMatched = base.extractStockOtherInfo();

            return hasRRCMatched && hasPriceMatched;
        }

        public override string[] removeAnyButContentInLines(string[] lines)
        {
            double perCounter = 0;
            double perPerLine = 1.0 / lines.Length;

            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|((重要|特别)(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|研究员|作者)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                //add this regulation to aviod the loss of main content
                if (perCounter <= 0.30)
                {
                    newLines.Add(line);
                    perCounter += perPerLine;
                    continue;
                }

                string trimedLine = line.Trim();
                if (trimedLine.StartsWith("长期竞争力评级的说明："))//added
                {
                    break;
                }
                if (trimedLine.StartsWith("优化市盈率计算的说明："))//added
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
                perCounter += perPerLine;
            }
            return newLines.ToArray();
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

            //Regex picOrTabHead = new Regex(@"^(图|表) *\d{1,2}");

            Regex noteLaiyuan = new Regex("来源：.*$");//added

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
                if (trimedPara.Contains("来源："))//added
                {
                    if (trimedPara.StartsWith("来源：")) { continue; }
                    else
                    {
                        string laiyuan = noteLaiyuan.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(laiyuan, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
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

            Regex regDate = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日 *");//报告发布日期 2013 年 10 月 22 日 

            string format = "报告发布日期yyyy年MM月dd日";

            bool hasTimeMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (regDate.IsMatch(trimedLine))
                {
                    string dateStr = regDate.Match(line).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr, format, System.Globalization.CultureInfo.CurrentCulture);
                    hasTimeMatched = true;
                    break;
                }
            }

            if (hasTimeMatched) { return true; }
            return false;
        }
    }
}
