using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //东方证券
    public class DongFangSecurities : ReportParser
    {
        public DongFangSecurities(string pdReportPath)
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
                    Trace.TraceError("DongFangSecurities.CommonSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            //Regex stockRRC = new Regex(@"(买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)[\(（][\u4e00-\u9fa5]{2,4}[\)）]");
            //Regex stockR = new Regex(@"买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避");
            Regex stockPricePattern = new Regex("^股价.*元$");
            Regex stockPrice = new Regex(@"\d+\.\d+");

            bool hasPriceMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockPricePattern.IsMatch(trimedLine))
                {
                    string spp = stockPricePattern.Match(trimedLine).Value;
                    if (stockPrice.IsMatch(spp))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("DongFangSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (hasPriceMatched)
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

            Regex extra = new Regex("^(HeaderTable_StatementCompany|HeaderTable_TypeTitle|东方证券股份有限公司及其关联机构在法律许可的范围内|有关分析师的申明，见本报告最后部分。其他重|东方证券股份有限公司经相关主管机关核准具备证券投资咨询业务)");
            Regex newReportTail = new Regex(@" \d{4}-\d{2}-\d{2}$");
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
                if (newReportTail.IsMatch(trimedPara))//added
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

            Regex regDate = new Regex(@"报告发布日期 *20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日");//报告发布日期 2013 年 10 月 22 日 

            string format = "报告发布日期yyyy年MM月dd日";

            bool hasTimeMatched = false;
            foreach (var line in lines)
            {
                if (regDate.IsMatch(line))
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
