using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Report
{
    class ReportParser
    {
        protected string pdfText;
        protected string[] lines;
        protected string[] noTableLines;
        protected string[] noTableAndOtherLines;
        protected string[] mergedParas;

        protected PDDocument pdfReport;
        protected AnalystReport anaReport;

        public ReportParser()
        {
            pdfReport = null;
            anaReport = null;
        }

        public ReportParser(PDDocument doc)
        {
            pdfReport = doc;
            anaReport = new AnalystReport();
        }

        public virtual string extractStockjobber()
        {
            return "";
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

        public virtual string extractAnalysts()
        {
            return "";
        }

        public virtual string extractDate()
        {
            //just get date from database
            return "";
        }

        public virtual bool extractContent()
        {
            return false;
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

        public virtual string[] mergeToParagraph(string[] lines)
        {
            string[] str = null;
            return str;
        }
        //public virtual string getContent()
        //{
        //    return "";
        //}
    }
}
