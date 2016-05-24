using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Report
{
    //国泰君安
    class GuoJunSecurities : ReportParser
    {
        public GuoJunSecurities(PDDocument pdreport)
            : base(pdreport)
        {
            pdfText = loadPDFText();
            lines = pdfText.Split('\n');
            mergedParas = mergeToParagraph(lines);
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
            return base.extractStockOtherInfo();
        }

        public override bool extractContent()
        {
            string s = "";
            foreach (var para in mergedParas)
            {

            }
            return base.extractContent();
        }

        public override string[] removeTable(string[] lines)
        {
            
            return base.removeTable(lines);
        }
    }
}
