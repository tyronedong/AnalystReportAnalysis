using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Report
{
    class ReportParser
    {
        public bool isValid = false;

        protected string pdfText;
        protected string[] lines;
        protected string[] noABCLines;
        protected string[] noTableLines;
        protected string[] noOtherLines;
        protected string[] noTableAndOtherLines;
        protected string[] mergedParas;
        protected string[] advancedMergedParas;
        protected string[] finalParas;

        protected PDDocument pdfReport;
        protected AnalystReport anaReport;

        public ReportParser()
        {
            pdfReport = null;
            anaReport = null;
        }

        public ReportParser(string pdfPath)
        {
            anaReport = new AnalystReport();
            try
            {
                pdfReport = PDDocument.load(pdfPath);
                isValid = true;
            }
            catch (Exception e)
            {
                Trace.TraceError("ReportParser.ReportParser(string pdfPath): " + e.Message);
                isValid = false;
            }
        }

        public virtual AnalystReport executeExtract()
        {
            extractStockInfo();
            extractContent();
            return anaReport;
        }
        
        public virtual bool extractStockInfo()
        {
            bool f1 = extractStockBasicInfo();
            bool f2 = extractStockOtherInfo();
            if (f1 && f2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool extractStockBasicInfo()
        {
            //extract stock name and stock code
            return false;
        }

        public virtual bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            return false;
        }         

        public virtual bool extractContent()
        {
            //problem: haven't remove something like "分析员，SAC 执业证书编号" and "公司简介"
            string content = "";
            Regex isContent = new Regex("[\u4e00-\u9fa5a][，。；]");
            foreach (var para in finalParas)
            {
                string normaledPara = para.Trim();
                if (isContent.IsMatch(para))
                {
                    content += normaledPara + '\n';
                }
            }
            anaReport.Content = content;
            return true;
        }

        /// <summary>
        /// This method has been abandoned cause Jobber info already exists in database.
        /// </summary>
        /// <returns></returns>
        public virtual string extractStockjobber()
        {
            return null;
        }

        /// <summary>
        /// This method has been abandoned cause Date info already exists in database.
        /// </summary>
        /// <returns></returns>
        public virtual string extractDate()
        {
            //just get date from database
            return null;
        }

        /// <summary>
        /// This method has been temporarily abandoned cause Analysts info already exists in database though not integral enough.
        /// </summary>
        /// <returns></returns>
        public virtual string extractAnalysts()
        {
            return null;
        }

        public string loadPDFText()
        {
            try
            {
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(pdfReport).Replace("\r\n", "\n");
                return text;
            }
            catch (Exception e) { return null; }  
        }

        public virtual string[] removeTableInLines(string[] lines)
        {
            return null;
        }

        public virtual string[] removeOtherInLines(string[] lines)
        {
            string[] str = null;
            return str;
        }

        public virtual string[] removeOtherInParas(string[] paras)
        {
            return null;
        }

        public virtual string[] removeAnyButContentInLines(string[] lines)
        {
            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex Statements = new Regex("^((分析师(声明|承诺))|(重要声明)|(免责(条款|声明))|(法律声明)|(披露声明))[:：]?$");
            Regex FirmIntro = new Regex("公司(简介|研究)[:：]?$");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (InvestRatingStatement.IsMatch(trimedLine))
                {
                    break;
                }
                if (Statements.IsMatch(trimedLine))
                {
                    break;
                }
                if (FirmIntro.IsMatch(trimedLine))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        public virtual string[] removeAnyButContentInParas(string[] lines)
        {
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (string.IsNullOrEmpty(trimedLine))
                {
                    continue;
                }
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

        public virtual string[] mergeToParagraph(string[] lines)
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

        public void CloseAll()
        {
            pdfReport.close();
        }

    }
}


//public ReportParser(PDDocument doc)
//{
//    pdfReport = doc;
//    anaReport = new AnalystReport();
//}

//public virtual string getContent()
//{
//    return "";
//}