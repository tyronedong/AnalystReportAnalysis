using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class GuoXinSecurities : ReportParser
    {
        //国信证券
        public GuoXinSecurities(string pdReportPath)
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
                    Trace.TraceError("GuoXinSecurities.GuoXinSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        /// <summary>
        /// stockNameAndCode is used for locate the rating and rating changes
        /// eg:
        /// "餐饮旅游 [Table_StockInfo] 黄山旅游（600054） 谨慎推荐 
        /// 2013 年三季报点评 （维持评级） "
        /// No price infomation is extracted.
        /// </summary>
        /// <returns></returns>
        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockNameAndCode = new Regex(@"[\u4e00-\u9fa5]+ *[(（]?\d{6}(\.[a-zA-Z]+)?[)）]?\D");//匹配“泸州老窖（000568）”
            Regex stockR = new Regex(@"谨慎推荐|推荐|中性|回避");
            Regex stockRC = new Regex("[（(][\u4e00-\u9fa5]+评级[）)]");

            int index = 0;
            bool hasRMatched = false, hasRCMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockNameAndCode.IsMatch(trimedLine))
                {
                    if (stockR.IsMatch(trimedLine))
                    {
                        anaReport.StockRating = stockR.Match(trimedLine).Value;
                        hasRMatched = true;

                        if (stockRC.IsMatch(lines[index + 1]))
                        {
                            anaReport.RatingChanges = stockRC.Match(lines[index + 1]).Value.Replace("(", "").Replace("（", "").Replace(")", "").Replace("）", "").Replace("评级", "");
                            hasRCMatched = true;
                        }
                    }
                }
                index++;
            }
            if (hasRMatched && hasRCMatched)
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
                if (trimedPara.StartsWith("作者保证报告所采用的数据均来自合规渠道，"))//added
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
                newParas.Add(para);
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
