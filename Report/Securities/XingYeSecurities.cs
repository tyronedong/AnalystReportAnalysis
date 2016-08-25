using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //兴业证券
    public class XingYeSecurities : ReportParser
    {
        public XingYeSecurities(string pdReportPath)
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
                    Trace.TraceError("XingYeSecurities.XingYeSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockInfo()
        {
            Regex stockPrice = new Regex(@"\d+\.\d+");

            bool hasCmpMatched = false, hasCodeMatched = false;
            bool hasRMatched = false, hasRCMatched = false, hasRRCMatched = false, hasPriceMatched = false;
            bool lastRC = false;
            bool lastCmp = false, lastCode = false;

            foreach (var line in lines)
            {
                string trimedLine = line.Trim();

                if (!hasCmpMatched && trimedLine.Contains("#dyCompany#"))
                { lastCmp = true; continue; }
                if (!hasCodeMatched && trimedLine.Contains("de#"))
                { lastCode = true; continue; }
                if (!hasRCMatched && trimedLine.Contains("nge#"))
                { lastRC = true; continue; }
                
                if (!hasRMatched && trimedLine.Contains("#investSuggestion#"))
                {
                    anaReport.StockRating = trimedLine.Replace("#investSuggestion#", "").Trim();
                    hasRMatched = true;
                }
                if (!hasRCMatched && lastRC)
                {
                    anaReport.RatingChanges = trimedLine;
                    hasRCMatched = true;
                }
                if (!hasCmpMatched && lastCmp)
                {
                    anaReport.StockName = trimedLine;
                    hasCmpMatched = true;
                }
                if (!hasCodeMatched && lastCode)
                {
                    anaReport.StockCode = trimedLine;
                    hasCodeMatched = true;
                }
                if (!hasPriceMatched && trimedLine.Contains("收盘价（元）"))
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
                            Trace.TraceError("XingYeSecurities.extractStockInfo(): " + e.Message);
                        }
                    }
                }
            }

            hasRRCMatched = hasRMatched && hasRCMatched;

            if (!hasRRCMatched)//如果没有匹配成功，则调用基类的方法
                hasRRCMatched = base.extractStockOtherInfo();

            return hasRRCMatched && hasPriceMatched && hasCodeMatched && hasCmpMatched;
        }
    }
}
