using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class DongBeiSecurities : ReportParser
    {
        public DongBeiSecurities(string pdReportPath)
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
                    Trace.TraceError("DongBeiSecurities.DongBeiSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"上次评级： *(优于大势|同步大势|落后大势|推荐|谨慎推荐|中性|回避)");
            Regex stockR = new Regex(@"优于大势|同步大势|落后大势|推荐|谨慎推荐|中性|回避");

            Regex stockPrice = new Regex(@"\d+\.\d+");

            bool hasRRCMatched = false, hasPriceMatched = false;
            string lastLine = "";
            foreach (var line in lines)
            {
                if (hasRRCMatched && hasPriceMatched) { break; }

                string trimedLine = line.Trim();
                if (stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    string lastRating = stockR.Match(srrc).Value;
                    string curRating = stockR.Match(lastLine).Value;

                    anaReport.StockRating = curRating;
                    anaReport.RatingChanges = RatingChange(curRating, lastRating);

                    hasRRCMatched = true;
                }
                if (trimedLine.StartsWith("收盘价（元）"))
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
                            Trace.TraceError("DongBeiSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }

                lastLine = line;
            }

            if (!hasRRCMatched)//如果没有匹配成功，则调用基类的方法
                hasRRCMatched = base.extractStockOtherInfo();

            return hasRRCMatched && hasPriceMatched;
        }

        public string RatingChange(string curRating, string lastRating)
        {
            int curInt = 0, lastInt = 0;

            if (curRating.Equals("优于大势") || curRating.Equals("推荐") || curRating.Equals("谨慎推荐")) { curInt = 1; }
            if (lastRating.Equals("优于大势") || lastRating.Equals("推荐") || lastRating.Equals("谨慎推荐")) { lastInt = 1; }

            if (curRating.Equals("同步大势") || curRating.Equals("中性") ) { curInt = 0; }
            if (lastRating.Equals("同步大势") || lastRating.Equals("中性") ) { lastInt = 0; }

            if (curRating.Equals("落后大势") || curRating.Equals("回避")) { curInt = -1; }
            if (lastRating.Equals("落后大势") || lastRating.Equals("回避")) { lastInt = -1; }

            if (curInt > lastInt) { return "上调"; }
            else if (curInt == lastInt) { return "维持"; }
            else { return "下调"; }
        }

        public override string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *\.? *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");
            Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{9,} *\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(图|表|图表) *\d{1,2}");
            Regex extra = new Regex(@"^(本报告的信息均来自已公开信息，关于信息的准确性与完|本公司具备证券投资咨询业务资格，请务必阅读最后一页免责声明|证监会审核华创证券投资咨询业务资格批文号：证监|(\d{1,3} *)?郑重声明)");//added

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                {
                    continue;
                }
                if (refReportHT.IsMatch(trimedPara))
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
                if (picOrTabHead.IsMatch(trimedPara))
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

                newParas.Add(para.Replace("[Table_Summary]", "").Trim());
            }
            return newParas.ToArray();
        }

    }
}
