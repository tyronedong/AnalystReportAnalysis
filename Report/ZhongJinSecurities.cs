using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                noOtherLines = removeOther(lines);
                mergedParas = mergeToParagraph(noOtherLines);
            }
        }

        public override bool extractStockBasicInfo()
        {
            Regex stockNameAndCode = new Regex("[\u4e00-\u9fa5]+ *[(（]\\d+.?[a-zA-Z]*[)）]");
            Regex stockName = new Regex("[\u4e00-\u9fa5]+");
            Regex stockCode = new Regex("\\d+");
            Regex stockNameLocater = new Regex("^\\d{4}年 *\\d{1,2}月 *\\d{1,2}日"); //用来定位stock name的前一行
            bool hasMatched = false;
            foreach (var line in lines)
            {
                if (stockNameAndCode.IsMatch(line))
                {
                    string snc = stockNameAndCode.Match(line).Value;
                    string sn = stockName.Match(snc).Value;
                    string sc = stockCode.Match(snc).Value;

                    anaReport.StockName = sn;
                    anaReport.StockCode = sc;

                    hasMatched = true;
                    break;
                }
            }
            if (hasMatched) 
            {
                return true;
            }
            else
            {
                bool hasNameMatched = false, hasCodeMatched = false;
                int index = 0;
                foreach (var line in lines)
                {
                    if (hasNameMatched && hasCodeMatched)
                    {
                        break;
                    }
                    string trimedLine = line.Trim();
                    if (stockNameLocater.IsMatch(trimedLine))
                    {
                        anaReport.StockName = lines.ElementAt(index + 1).Trim();
                        hasNameMatched = true;
                    }
                    if (trimedLine.StartsWith("股票代码"))
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

        public override string[] removeOther(string[] lines)
        {
            //remove nonsence information including "法律声明" ,table head, table tail and other things
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (string.IsNullOrEmpty(trimedLine))
                {
                    continue;
                }
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
