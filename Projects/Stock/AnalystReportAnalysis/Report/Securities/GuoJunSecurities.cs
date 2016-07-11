using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Report.Securities
{
    //国泰君安
    public class GuoJunSecurities : ReportParser
    {
        public GuoJunSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                try
                {
                    pdfText = loadPDFText();
                    lines = pdfText.Split('\n');
                    noABCLines = removeAnyButContentInLines(lines);
                    noOtherLines = removeOtherInLines(noABCLines);
                    mergedParas = mergeToParagraph(noOtherLines);
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

        public override AnalystReport executeExtract_withdb()
        {
            return base.executeExtract_withdb();
        }

        public override bool extractStockBasicInfo()
        {
            Regex stockNameAndCode = new Regex("[\u4e00-\u9fa5]+[(（]\\d+[)）]");//匹配“泸州老窖（000568）”
            Regex stockName = new Regex("[\u4e00-\u9fa5]+");
            Regex stockCode = new Regex("\\d+");
            bool isInMainInfo = false, isInTableStock = false, isInManySpaces = false;
            int index = 0;
            foreach (var line in lines)
            {
                if (line.Contains("Table_MainInfo"))
                {
                    if (stockNameAndCode.IsMatch(line))
                    {
                        string snc = stockNameAndCode.Match(line).Value;
                        string sn = stockName.Match(snc).Value;
                        string sc = stockCode.Match(snc).Value;
                        
                        anaReport.StockName = sn;
                        anaReport.StockCode = sc;
                        
                        isInMainInfo = true;
                        break;
                    }
                }
                if (line.Contains("Table_Stock"))
                {
                    string newLine = lines.ElementAt(index + 1);
                    if (stockNameAndCode.IsMatch(newLine))
                    {
                        string snc = stockNameAndCode.Match(newLine).Value;
                        string sn = stockName.Match(snc).Value;
                        string sc = stockCode.Match(snc).Value;

                        anaReport.StockName = sn;
                        anaReport.StockCode = sc;

                        isInTableStock = true;
                        break;
                    }
                }
                if (line.StartsWith("                                   "))
                {
                    if (stockNameAndCode.IsMatch(line))
                    {
                        string snc = stockNameAndCode.Match(line).Value;
                        string sn = stockName.Match(snc).Value;
                        string sc = stockCode.Match(snc).Value;

                        anaReport.StockName = sn;
                        anaReport.StockCode = sc;

                        isInManySpaces = true;
                        break;
                    }
                }
                index++;
            }
            if (isInMainInfo || isInTableStock || isInManySpaces)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool extractStockOtherInfo()
        {
            //information is in [Table_Invest] 
            //Regex stockPrice = new Regex("^当前价格：");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");
            Regex stockRating = new Regex("谨慎增持|增持|中性|减持");
            bool isPriceDone = false, isRatingDone = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (isPriceDone && isRatingDone)
                {
                    break;
                }
                if (trimedLine.StartsWith("评级：") || (!trimedLine.Contains("上次") && trimedLine.Contains("评级：")))
                {
                    if (stockRating.IsMatch(trimedLine))
                    {
                        anaReport.StockRating = stockRating.Match(trimedLine).Value;
                        isRatingDone = true;
                    }
                }
                if (trimedLine.StartsWith("当前价格："))
                {
                    if (stockPrice.IsMatch(trimedLine))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(trimedLine).Value);
                            //anaReport.StockPrice.ToString();
                            isPriceDone = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("GuoJunSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (isPriceDone && isRatingDone)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool extractContent()
        {
            //haven't handle the paragraph which contains only "：" 
            string content = "";
            Regex isContent = new Regex("[\u4e00-\u9fa5a][，。；]");
            Regex isNotContent = new Regex(@"^《.*\d+");
            foreach (var para in finalParas)
            {
                string normaledPara = para.Replace("[Table_Summary]", "").Replace(" ","").Replace(" ", "").Replace("", "").Trim();
                if (isNotContent.IsMatch(normaledPara))
                {
                    continue;
                }
                if (isContent.IsMatch(para))
                {
                    content += normaledPara;
                    content += "\n";
                }
            }
            anaReport.Content = content;
            return true;
        }
        //public override bool extractContent()
        //{
        //    string content = "";
        //    Regex paraHead_1 = new Regex(@"^(\[Table_Summary\])*( )* [\u4e00-\u9fa5a-zA-z\d]");//匹配"[Table_Summary]  投资建议："或者" 业绩略低于市场预期" （前面的空格是两个字符，其中一个是空格另一个比空格长一点）
        //    //Regex paraHead_2 = new Regex();
        //    string prePara = ""; bool isContentStart = false; 
        //    foreach (var para in mergedParas)
        //    {
        //        if (paraHead_1.IsMatch(para))
        //        {
        //            content += para.Replace("[Table_Summary]", "").Replace("", "").Trim();
        //            content += "\n";
        //            //content += paraHead_1.Match(para).Value + '\n';
        //        }
        //        if (prePara.Contains("本报告导读："))
        //        {
        //            content += para.Replace("", "").Trim();
        //            content += "\n";
        //        }
        //        prePara = para;
        //    }
        //    anaReport.Content = content;
        //    return true;
        //}

        public override string[] removeTableInLines(string[] lines)
        {

            return base.removeTableInLines(lines);
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
                if (trimedLine.StartsWith("请务必阅读正文之后的免责条款部分"))
                {
                    continue;
                }
                if (trimedLine.EndsWith("公司简介"))
                {
                    break;
                }
                if (trimedLine.Equals("分析师声明"))
                {
                    break;
                }
                if (trimedLine.Equals("免责声明"))
                {
                    break;
                }
                if (trimedLine.Equals("评级说明"))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        public override bool extractDate()
        {
            if (base.extractDate())
            { return true; }

            Regex regDate = new Regex(@"^20\d{2}\.[01]\d\.[0-3]\d$");

            string format = "yyyy.MM.dd";
            
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();//remove whitespace from head and tail

                if (regDate.IsMatch(trimedLine))
                {
                    string dateStr = regDate.Match(trimedLine).Value;
                    anaReport.Date = DateTime.ParseExact(dateStr, format, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
            }

            return false;
        }
    }
}
