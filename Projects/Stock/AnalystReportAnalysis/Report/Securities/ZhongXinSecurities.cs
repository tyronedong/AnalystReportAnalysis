using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.pdfbox.pdmodel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Report.Securities
{
    public class ZhongXinSecurities : ReportParser
    {
        //中信证券（2013年及之前的pfd解析乱码）
        public ZhongXinSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            //if (this.isValid)
            //{
            //    pdfText = loadPDFText();
            //    lines = pdfText.Split('\n');
            //    mergedParas = mergeToParagraph(lines);
            //}
            if (this.isValid)
            {
                try
                {
                    pdfText = loadPDFText();
                    lines = pdfText.Split('\n');
                    noABCLines = removeAnyButContentInLines(lines);
                    mergedParas = mergeToParagraph(noABCLines);
                    noABCParas = removeAnyButContentInParas(mergedParas);
                    finalParas = noABCParas;
                }
                catch (Exception e)
                {
                    this.isValid = false;
                    Trace.TraceError("ZhongXinSecurities.ZhongXinSecurities(string pdReportPath): " + e.Message);
                }
            }
        }

        public override bool extractStockBasicInfo()
        {

            return base.extractStockBasicInfo();
        }

        public override string[] removeAnyButContentInLines(string[] lines)
        {
            //Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(评级(标准|说明)[:：]?$)");//changed
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|((重要|特别)(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|研究员|作者)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (trimedLine.StartsWith("分析师声明"))//added
                {
                    break;
                }
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
                if (AnalystIntro.IsMatch(trimedLine))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }
    }
}
