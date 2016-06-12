using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //海通证券
    public class HaiTongSecurities:ReportParser
    {
        public HaiTongSecurities(string pdReportPath)
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
                    Trace.TraceError("HaiTongSecurities.HaiTongSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"^(买 入|增 持|中 性|减 持|买入|增持|中性|减持) *[\u4e00-\u9fa5]{2,3}");
            Regex stockR = new Regex(@"(买 入|增 持|中 性|减 持|买入|增持|中性|减持)");
            Regex stockPricePattern = new Regex(@"收盘(价|于)[ ：:]\d+\.\d+ ?元");
            Regex stockPrice = new Regex(@"\d+\.\d+");

            int index = 0;
            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasRRCMatched && hasPriceMatched) { break; }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && stockRRC.IsMatch(trimedLine))
                {

                    string srrc = stockRRC.Match(trimedLine).Value;
                    string sr = stockR.Match(srrc).Value;
                    anaReport.StockRating = sr.Replace(" ", "");
                    anaReport.RatingChanges = srrc.Replace(sr, "").Trim();

                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && stockPricePattern.IsMatch(trimedLine))
                {
                    try
                    {
                        anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                        //anaReport.StockPrice.ToString();
                        hasPriceMatched = true;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("HaiTongSecurities.extractStockOtherInfo(): " + e.Message);
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

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{15,} *\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(附)?(图|表) *\d{1,3}");//added
            Regex newRefReportTail = new Regex(@".*?(点评|报告|简报|行业)[：-]?.*?20[01][0-9]\.?[01][0-9]\.?[0123][0-9]$");//added
            Regex certificateNum = new Regex("执业证书编号[:：]");
            Regex extra = new Regex("^((分产品收入和毛利率数据。)|(利润表主要数据。))$");

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
                if (certificateNum.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (newRefReportTail.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (picOrTabHead.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (trimedPara.Contains("数据来源："))
                {
                    if (trimedPara.StartsWith("数据来源：")) { continue; }
                    else
                    {
                        string shuju = noteShuju.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(shuju, "").Replace("，左轴）", "").Replace("，右轴）", "");//modified
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (trimedPara.Contains("资料来源："))
                {
                    if (trimedPara.StartsWith("资料来源：")) { continue; }
                    else
                    {
                        string ziliao = noteZiliao.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(ziliao, "").Replace("，左轴）", "").Replace("，右轴）", "");//modified
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (trimedPara.Contains("注："))
                {
                    if (trimedPara.StartsWith("注：")) { continue; }
                    else
                    {
                        string zhu = noteZhu.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(zhu, "").Replace("，左轴）", "").Replace("，右轴）", "");//modified
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                newParas.Add(para);
            }
            return newParas.ToArray();
        }
    }
}
