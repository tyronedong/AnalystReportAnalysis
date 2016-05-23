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
    class ZhaoShangSecurities : ReportParser
    {
        public ZhaoShangSecurities(PDDocument pdreport)
            : base(pdreport)
        {

        }
        
            //base(pdreport);
        
        //public override

        public override string extractContent()
        {
            string content = "";
            string pdftext = loadPDFText();
            string[] lines = pdftext.Split('\n');
            string[] noTableLines = removeTable(lines);
            string[] mergedParas = mergeToParagraph(noTableLines);
            //string s = "", ss = "";
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
            foreach (var para in mergedParas)
            {
                if (paraHead.IsMatch(para))
                {
                    content += para + "\n";
                }
            }
            return "";
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
