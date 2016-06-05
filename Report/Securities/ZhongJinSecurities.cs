using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using org.apache.pdfbox.pdmodel;

namespace Report
{
    class ZhongJinSecurities : ReportParser
    {
        //中金公司
        public ZhongJinSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                pdfText = loadPDFText();
                lines = pdfText.Split('\n');
                noOtherLines = removeOtherInLines(lines);
                mergedParas = mergeToParagraph(noOtherLines);
                finalParas = mergedParas;
            }
        }

        public override bool extractStockBasicInfo()
        {
            Regex stockNameAndCode = new Regex("[\u4e00-\u9fa5]+ *[(（]\\d+.?[a-zA-Z]*[)）]");//匹配“云南白药  (000538.CH)”
            Regex stockName = new Regex("[\u4e00-\u9fa5]+");
            Regex stockCode = new Regex("\\d+");
            Regex stockNameLocater = new Regex(@"^\d{4}年\d{1,2}月\d{1,2}日"); //用来定位stock name的前一行       

            bool hasNCMatched = false; string preLine = "";
            foreach (var line in lines)
            {
                if (stockNameAndCode.IsMatch(line))
                {
                    string snc = stockNameAndCode.Match(line).Value;
                    string sn = stockName.Match(snc).Value;
                    string sc = stockCode.Match(snc).Value;

                    anaReport.StockName = sn;
                    anaReport.StockCode = sc;

                    anaReport.StockRating = preLine.Trim();

                    hasNCMatched = true;
                    break;
                }
                preLine = line;
            }
            if (hasNCMatched) 
            {
                return true;
            }
            else
            {
                bool hasNameMatched = false, hasCodeMatched = false;
                int index = 0;
                foreach (var nLine in noOtherLines)
                {
                    if (hasNameMatched && hasCodeMatched)
                    {
                        break;
                    }
                    string trimedLine = nLine.Trim();
                    if (!hasNameMatched && stockNameLocater.IsMatch(trimedLine.Replace(" ", "").Replace("证券研究报告", "")))
                    {
                        anaReport.StockName = noOtherLines.ElementAt(index + 1).Trim();
                        hasNameMatched = true;
                    }
                    if (!hasCodeMatched && trimedLine.StartsWith("股票代码"))
                    {
                        if (stockCode.IsMatch(trimedLine))
                        {
                            anaReport.StockCode = stockCode.Match(trimedLine).Value;
                        }
                        hasCodeMatched = true;
                    }
                    index++;
                }
                if (hasCodeMatched && hasNameMatched)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool extractStockOtherInfo()
        {
            Regex stockRRC = new Regex(@"(买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)[\(（][\u4e00-\u9fa5]{2,3}[\)）]");
            Regex stockRRC2 = new Regex(@"(首次|维持|上调|下调)[\u4e00-\u9fa5]?(买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)");
            Regex stockR = new Regex(@"买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避");
            //Regex stockPrice = new Regex(@"^最新收盘价|当前价");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");

            bool hasRRCMatched = false, hasPriceMatched = false;
            foreach (var line in lines)
            {
                if (hasPriceMatched && hasRRCMatched)
                {
                    break;
                }
                string trimedLine = line.Trim();
                if (!hasRRCMatched && stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "");

                    hasRRCMatched = true;
                }
                if (!hasRRCMatched && stockRRC2.IsMatch(line))
                {
                    string srrc = stockRRC2.Match(line).Value;
                    string sr = stockR.Match(srrc).Value;
                    string src = srrc.Replace(sr, "");

                    anaReport.StockRating = sr;
                    anaReport.RatingChanges = src;

                    hasRRCMatched = true;
                }
                if (!hasPriceMatched && (trimedLine.StartsWith("最新收盘价") || trimedLine.StartsWith("当前价")))
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
                            Trace.TraceError("GuoJunSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (hasPriceMatched && hasRRCMatched)
            {
                return true;
            }
            return false;
        }

        public override bool extractContent()
        {
            //problem: haven't remove something like "分析员，SAC 执业证书编号" and "公司简介"
            string content = "";
            Regex isContent = new Regex("[\u4e00-\u9fa5a][，。；]");
            foreach (var para in mergedParas)
            {
                if (isContent.IsMatch(para))
                {
                    content += para + '\n';
                }
            }
            anaReport.Content = content;
            return true;
        }

        public override string[] removeOtherInLines(string[] lines)
        {
            //remove nonsence information including "法律声明" ,table head, table tail and other things
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                //if (string.IsNullOrEmpty(trimedLine))
                //{
                //    continue;
                //}
                if (trimedLine.Equals("公司研究"))
                {
                    continue;
                }
                if (trimedLine.StartsWith("请仔细阅读在本报告尾部的重要法律声明"))
                {
                    continue;
                }
                if (trimedLine.StartsWith("图表 "))
                {
                    continue;
                }
                if (trimedLine.StartsWith("资料来源："))
                {
                    continue;
                }
                if (trimedLine.Equals("法律声明"))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }
    }
}
