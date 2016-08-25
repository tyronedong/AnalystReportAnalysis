using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class ChangJiangSecurities : ReportParser
    {
        public ChangJiangSecurities(string pdReportPath)
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
                    Trace.TraceError("ChangJiangSecurities.CommonSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"评级 {1,}(看好|中性|看淡|推荐|谨慎推荐|减持|无投资评级) {1,}[\u4e00-\u9fa5]{2,4} *$");//评级 推荐 维持  
            Regex stockR = new Regex(@"看好|中性|看淡|推荐|谨慎推荐|减持|无投资评级");

            bool hasRRCMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("评级", "").Trim();

                    hasRRCMatched = true;
                    break;
                }
            }

            if (!hasRRCMatched)//如果没有匹配成功，则调用基类的方法
                hasRRCMatched = base.extractStockOtherInfo();
            
            return hasRRCMatched;
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
            Regex extra = new Regex("^(本报告的信息均来自已公开信息，关于信息的准确性与完|本公司具备证券投资咨询业务资格，请务必阅读最后一页免责声明|证监会审核华创证券投资咨询业务资格批文号：证监)");//added

            Regex title = new Regex(@"评级 {1,}(看好|中性|看淡|推荐|谨慎推荐|减持|无投资评级)");//added
            Regex refReport = new Regex(@"^《.*?星期[一二三四五六日天]");//《二季度营收下滑，费用率下降带来净利率提升》2013/7/30 星期二

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
                if (picOrTabHead.IsMatch(trimedPara))
                {
                    continue;
                }
                if (title.IsMatch(trimedPara))
                {
                    continue;
                }
                if (refReport.IsMatch(trimedPara))
                {
                    continue;
                }
                if (trimedPara.Contains("公司报告(点评报告)"))//added
                {
                    newParas.Add(para.Replace("公司报告(点评报告)", "").Trim());
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

            Regex regDate1 = new Regex(@"[（(]\d{6}[)）] *20\d{2}-\d{1,2}-\d{1,2}$");//（300090） 2013-9-9 
            Regex pureRegDate1 = new Regex(@"20\d{2}-\d{1,2}-\d{1,2}");
            //Regex regDate2 = new Regex(@"报告日期： *20\d{2}-[01]\d-[0-3]\d");//报告日期： 2010-01-18

            //string format1 = "yyyy年MM月dd日";
            //string format2 = "报告日期：yyyy-MM-dd";

            bool hasTimeMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();

                if (regDate1.IsMatch(trimedLine))
                {
                    string dateStr1 = regDate1.Match(trimedLine).Value.Replace(" ", "");
                    string pDateStr1 = pureRegDate1.Match(dateStr1).Value;
                    string[] times = pDateStr1.Split('-');
                    anaReport.Date = new DateTime(Int32.Parse(times[0]), Int32.Parse(times[1]), Int32.Parse(times[2]));
                    //anaReport.Date = DateTime.ParseExact(dateStr1, format1, System.Globalization.CultureInfo.CurrentCulture);
                    hasTimeMatched = true;
                    break;
                }
                //if (regDate2.IsMatch(norLine))
                //{
                //    string dateStr2 = regDate2.Match(norLine).Value.Replace(" ", "");
                //    anaReport.Date = DateTime.ParseExact(dateStr2, format2, System.Globalization.CultureInfo.CurrentCulture);
                //    hasTimeMatched = true;
                //    break;
                //}
            }

            if (hasTimeMatched) { return true; }
            return false;
        }
    }
}
