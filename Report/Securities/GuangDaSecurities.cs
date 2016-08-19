using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class GuangDaSecurities : ReportParser
    {
        public GuangDaSecurities(string pdReportPath)
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
                    Trace.TraceError("GuangDaSecurities.GuangDaSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"(看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避) *(([\(（][\u4e00-\u9fa5]{2,4}[\)）])|(/[\u4e00-\u9fa5]{2,4}))");//推荐(维持)||推荐/维持
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
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "").Replace("/", "").Trim();

                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && trimedLine.StartsWith("当前价/目标价："))
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
                            Trace.TraceError("GuangDaSecurities.extractStockInfo(): " + e.Message);
                        }
                    }
                }
            }
            return hasPriceMatched && hasRRCMatched;
        }
    }
}
