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
        protected PDDocument report;

        public ReportParser()
        {
            report = null;
        }

        public ReportParser(PDDocument doc)
        {
            report = doc;
        }

        public virtual string extractStockjobber()
        {
            return "";
        }

        public virtual string extractStockCode()
        {
            return "";
        }

        public virtual string extractStockName()
        {
            return "";
        }

        public virtual string extractStockPrice()
        {
            return "";
        }

        public virtual string extractStockRating()
        {
            return "";
        }

        public virtual string extractRatingChanges()
        {
            return "";
        }

        public virtual string extractAnalysts()
        {
            return "";
        }

        public virtual string extractDate()
        {
            return "";
        }

        public virtual string extractContent()
        {
            return "";
        }

        public string loadPDFText()
        {
            try
            {
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(report).Replace("\r\n", "\n");
                return text;
            }
            catch (Exception e) { return null; }  
        }

        public virtual string[] removeTable(string[] lines)
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
