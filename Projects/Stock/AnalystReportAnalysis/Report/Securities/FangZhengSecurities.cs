using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class FangZhengSecurities : ReportParser
    {
        public FangZhengSecurities(string pdReportPath)
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
                    Trace.TraceError("FangZhengSecurities.FangZhengSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"20\d{2}\.\d{1,2}\.\d{1,2} *(看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)");//2013.04.18 买入 
            Regex stockR = new Regex(@"看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|谨慎推荐|推荐|回避");

            bool hasRRCMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    //anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "").Replace("/","");

                    hasRRCMatched = true;
                    break;
                }
            }
            if (hasRRCMatched)
            {
                return true;
            }
            return false;
        }
    }
}
