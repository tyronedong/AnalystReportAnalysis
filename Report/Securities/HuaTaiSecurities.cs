using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    //华泰证券
    public class HuaTaiSecurities : ReportParser
    {
        public HuaTaiSecurities(string pdReportPath)
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
                    Trace.TraceError("HuaTaiSecurities.HuaTaiSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"(买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)[\(（][\u4e00-\u9fa5]{2,4}[\)）]");
            Regex stockR = new Regex(@"买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");

            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasRRCMatched && hasPriceMatched) { break; }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "");

                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && trimedLine.StartsWith("当前价格(元)："))
                {
                    if (stockPrice.IsMatch(line))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            hasPriceMatched = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("HuaTaiSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (hasRRCMatched && hasPriceMatched)
            {
                return true;
            }
            return false;
        }

        public override bool extractDate()
        {
            if (base.extractDate())
            { return true; }

            Regex regDate1 = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日");//2013年04月19日  纺织服装 / 服装Ⅱ 
            Regex regDate2 = new Regex(@"^20\d{2}-[01]\d-[0-3]\d$");

            string format1 = "yyyy年MM月dd日";
            string format2 = "yyyy-MM-dd";
            
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();//remove whitespace from head and tail

                if (regDate1.IsMatch(trimedLine))
                {
                    string dateStr1 = regDate1.Match(trimedLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr1, format1, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
                if (regDate2.IsMatch(trimedLine))
                {
                    string dateStr2 = regDate2.Match(trimedLine).Value.Replace(" ", "");
                    anaReport.Date = DateTime.ParseExact(dateStr2, format2, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
            }

            return false;
        }
    }
}
