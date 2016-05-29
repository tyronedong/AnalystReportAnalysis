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
    //招商证券
    class ZhaoShangSecurities : ReportParser
    {
        public ZhaoShangSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                pdfText = loadPDFText();
                lines = pdfText.Split('\n');
                noTableLines = removeTable(lines);
                noTableAndOtherLines = removeOther(noTableLines);
                mergedParas = mergeToParagraph(noTableAndOtherLines);
            }
        }
        
            //base(pdreport);
        
        //public override

        public override bool extractContent()
        {      
            string content = "";
            //string pdftext = loadPDFText();
            //string[] lines = pdftext.Split('\n');
            //string[] noTableLines = removeTable(lines);
            //string[] noTableAndOtherLines = removeOther(noTableLines);
            //string[] mergedParas = mergeToParagraph(noTableAndOtherLines);

            //string s = "", ss = "";3
            //foreach (var line in noTableLines)
            //{
            //    s += line + '\n'; 
            //    Console.WriteLine(line);
            //}
            //foreach (var para in mergedParas)
            //{
            //    ss += para + '\n';
            //    Console.WriteLine(para);
            //}

            Regex paraHead = new Regex("^\\D ");
            Regex chinese = new Regex("[\u4e00-\u9fa5]");
            int index = 0; bool isFirst = true;
            foreach (var para in mergedParas)
            {
                if (paraHead.IsMatch(para) && chinese.IsMatch(para))
                {
                    if (isFirst)
                    {
                        content += mergedParas.ElementAt(index-1) + "\n";
                        isFirst = false;
                    }
                    content += para + "\n";
                }
                index++;
            }
            anaReport.Content = content;
            return true;
        }

        public override string[] removeTable(string[] lines)
        {
            Regex tableHead = new Regex("[表|图].{1,1}?\\d{1,2}|财务预测表");
            Regex tableTail = new Regex("资料来源");
            //string[] lines = text.Split('\n');
            List<string> newLines = new List<string>();
            bool isTable = false;
            foreach (var line in lines)
            {
                if (tableHead.IsMatch(line))
                {
                    isTable = true;
                }
                if (isTable && tableTail.IsMatch(line))
                {
                    isTable = false;
                    continue;
                }
                if (isTable)
                {
                    continue;
                }                
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        public override string[] removeOther(string[] lines)
        {
            //Regex spaces = new Regex(" ")
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    continue;
                }
                if (line.Trim().Equals("公司研究"))
                {
                    continue;
                }
                if (line.StartsWith("敬请阅读末页的重要说明"))
                {
                    continue;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        public override string[] mergeToParagraph(string[] lines)
        {
            string curPara = "";
            List<string> paragraphs = new List<string>();
            foreach (var line in lines)
            {
                if (line.EndsWith(" "))
                {
                    curPara += line;
                    paragraphs.Add(curPara);
                    curPara = "";
                    continue;
                }
                curPara += line;
            }
            return paragraphs.ToArray();
        }
    }
}
