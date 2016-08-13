using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class ZhongJianSecurities : ReportParser
    {
        //中信建投
        public ZhongJianSecurities(string pdReportPath)
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
                    Trace.TraceError("GuoJunSecurities.GuoJunSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            Regex stockRRC1 = new Regex("[\u4e00-\u9fa5a]+ *(买入|增持|中性|减持|卖出) *$");
            Regex stockRRC2 = new Regex("^(买入|增持|中性|减持|卖出)");
            Regex stockR = new Regex(@"买入|增持|中性|减持|卖出");
            //Regex stockRC = new Regex(@"[\u4e00-\u9fa5a]+评级");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");

            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasPriceMatched && hasRRCMatched) { break; }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && stockRRC1.IsMatch(trimedLine))
                {
                    string srrc = stockRRC1.Match(trimedLine).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Trim();

                    hasRRCMatched = true;
                }
                if (!hasRRCMatched && stockRRC2.IsMatch(trimedLine))
                {
                    anaReport.StockRating = stockR.Match(trimedLine).Value;
                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && trimedLine.StartsWith("当前股价："))
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
            if (hasPriceMatched && hasRRCMatched) { return true; }
            return false;
        }

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *\.? *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{15,} *\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(图|表|图表) *\d{1,2}");

            Regex newRefReportHead = new Regex(@"^\d{4}[-\./]?\d{1,2}([-\./]?\d{1,2})?$");//added
            Regex newRefReportTail = new Regex("》 *$");//added

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
                if (newRefReportHead.IsMatch(trimedPara) && newRefReportTail.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (trimedPara.Contains("数据来源："))
                {
                    if (trimedPara.StartsWith("数据来源：")) 
                    { continue; }
                    else
                    {
                        string shuju = noteShuju.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(shuju, "");
                        if (!mightBeContent.IsMatch(judgeStr)) 
                        { continue; }
                    }
                }
                if (trimedPara.Contains("资料来源："))
                {
                    if (trimedPara.StartsWith("资料来源：")) 
                    { continue; }
                    else
                    {
                        string ziliao = noteZiliao.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(ziliao, "");
                        if (!mightBeContent.IsMatch(judgeStr)) 
                        { continue; }
                    }
                }
                if (trimedPara.Contains("注："))
                {
                    if (trimedPara.StartsWith("注：")) 
                    { continue; }
                    else
                    {
                        string zhu = noteZhu.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(zhu, "");
                        if (!mightBeContent.IsMatch(judgeStr)) 
                        { continue; }
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
            //发布日期： 2013 年6 月24 日
            //分析日期： 2008 年03 月25 日

            if (base.extractDate())
            { return true; }

            return false;
        }
    }
}
