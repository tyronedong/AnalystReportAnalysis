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
    }
}
