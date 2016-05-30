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

        public virtual string extractStockjobber()
        {
            return null;
        }

        public virtual string extractDate()
        {
            //just get date from database
            return null;
        }

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

        public virtual string[] removeTable(string[] lines)
        {
            string[] str = null;
            return str;
        }

        public virtual string[] removeOther(string[] lines)
        {
            string[] str = null;
            return str;
        }

        public virtual string[] removeOtherInParas(string[] paras)
        {
            return null;
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