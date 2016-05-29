﻿using System;
using System.Linq;
using System.Text;
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

        protected PDDocument pdfReport;
        protected AnalystReport anaReport;

        public ReportParser()
        {
            pdfReport = null;
            anaReport = null;
        }

        //public ReportParser(PDDocument doc)
        //{
        //    pdfReport = doc;
        //    anaReport = new AnalystReport();
        //}

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
            extractStockBasicInfo();
            extractStockOtherInfo();
            extractContent();
            return anaReport;
        }

        public virtual string extractStockjobber()
        {
            return null;
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
            return null;
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
        //public virtual string getContent()
        //{
        //    return "";
        //}
    }
}
