using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class MinShengSecurities : ReportParser
    {
        public MinShengSecurities(string pdReportPath)
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
                    Trace.TraceError("MinShengSecurities.MinShengSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"(看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避) *[\u4e00-\u9fa5]{1,4}评级");//谨慎推荐    首次评级 
            Regex stockR = new Regex(@"看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|谨慎推荐|推荐|回避");

            Regex stockPrice = new Regex(@"\d+\.\d+");

            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (hasPriceMatched && hasRRCMatched) { break; }
                if (!hasRRCMatched && stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "").Replace("/", "").Replace("评级","").Trim();

                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && trimedLine.StartsWith("收盘价（元）"))
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
                            Trace.TraceError("MinShengSecurities.extractStockInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (!hasRRCMatched)//如果没有匹配成功，则调用基类的方法
                hasRRCMatched = base.extractStockOtherInfo();

            return hasRRCMatched && hasPriceMatched;
        }

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *[.、]? *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");
            Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{9,} *\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(图|表|图表) *\d{1,2}");
            Regex extra = new Regex("^(独立性申明：|请通过合法途径获取本公司研究报告，如经由未经|本报告中?的?信息均来(自|源)于?已?公开的?(信息|资料)|本公司具备证券投资咨询业务资格，请务必阅读最后一页免责声明|证监会审核华创证券投资咨询业务资格批文号：证监|请务必阅读|每位主要负责编写本|市场有风险，投资需谨慎|此份報告由群益證券)");//added

            Regex refReport1 = new Regex(@"^\d{1,2} *[.、]?.*\.\d{1,2}$");//added
            Regex refReport2 = new Regex(@"^\d{1,2}[.、].*《.*\d{6}$");

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                    continue;
                if (refReportHT.IsMatch(trimedPara))
                    continue;
                if (indexEntry.IsMatch(trimedPara))
                    continue;
                if (refReport1.IsMatch(trimedPara))//added
                    continue;
                if (refReport2.IsMatch(trimedPara))
                    continue;
                if (extra.IsMatch(trimedPara))//added
                    continue;
                if (picOrTabHead.IsMatch(trimedPara))
                    continue;
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
    }
}
