using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //申万宏源
    public class ShenHongSecurities : ReportParser
    {
        public ShenHongSecurities(string pdReportPath)
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
                    Trace.TraceError("ShenHongSecurities.ShenHongSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            //Regex stockRRC = new Regex(@"(买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)[\(（][\u4e00-\u9fa5]{2,3}[\)）]");
            Regex stockR = new Regex(@"^(买入|增持|中性|减持|看好|看淡)");
            //Regex stockPrice = new Regex(@"^收盘价（元） *\d+(\.\d+)?");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");

            int index = 0;
            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasRRCMatched && hasPriceMatched) { break; }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && stockR.IsMatch(trimedLine))
                {
                    anaReport.StockRating = stockR.Match(trimedLine).Value;
                    if (lines[index + 1].Contains(' '))
                    {
                        anaReport.RatingChanges = lines[index + 1].Trim().Split(' ')[0];
                    }
                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && trimedLine.StartsWith("收盘价（元）"))
                {
                    if (stockPrice.IsMatch(line))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("ShenHongSecurities.extractStockOtherInfo(): " + e.Message);
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
                if (trimedLine.StartsWith("报告原因："))//added(提高报告原因的优先级使其不被break掉)
                {
                    newLines.Add(line);
                    continue;
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
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?(证券分析师)?$");//modified
            Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{9,} *\d{1,3}$");

            Regex shouyeNote = new Regex("^本公司不持有或交易股票及其衍生品.*?客户应全面理解本报告结尾.*");//added
            Regex meiyeNote = new Regex(@"本研究报告仅通过邮件提供给.*?使用。\d{1,3}");//added
            Regex picOrTabHead = new Regex(@"^(附)?(图|表) *\d{1,3}");//added

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string curPara = para;
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                {
                    continue;
                }
                if (indexEntry.IsMatch(trimedPara))
                {
                    continue;
                }
                if (trimedPara.StartsWith("百万元，百万股"))//added
                {
                    continue;
                }
                if (trimedPara.StartsWith("单季度，百万"))//added
                {
                    continue;
                }
                if (trimedPara.StartsWith("单位：元，百万元，"))//added
                {
                    continue;
                }
                if (trimedPara.StartsWith("本公司不持有或交易股票及其衍生品，"))//added
                {
                    continue;
                }
                if (picOrTabHead.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (meiyeNote.IsMatch(trimedPara))//added
                {
                    string matchedStr = meiyeNote.Match(trimedPara).Value;
                    trimedPara = trimedPara.Replace(matchedStr, "").Trim();
                    if (string.IsNullOrEmpty(trimedPara)) { continue; }
                    curPara = para.Replace(matchedStr, "");
                }
                if (shouyeNote.IsMatch(trimedPara))//added
                {
                    string matchedStr = shouyeNote.Match(trimedPara).Value;
                    trimedPara = trimedPara.Replace(matchedStr, "").Trim();
                    if (string.IsNullOrEmpty(trimedPara)) { continue; }
                    curPara = para.Replace(matchedStr, "");
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
                newParas.Add(curPara);
            }
            return newParas.ToArray();
        }

        public override bool extractDate()
        {
            if (base.extractDate())
            { return true; }

            return false;
        }
    }
}
