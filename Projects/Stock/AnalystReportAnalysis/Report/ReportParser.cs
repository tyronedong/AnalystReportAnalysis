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
        protected string[] mergedParas;
        protected string[] noABCParas;
        protected string[] finalParas;

        protected string[] noTableLines;
        protected string[] noOtherLines;
        protected string[] noTableAndOtherLines;
        protected string[] advancedMergedParas;
        
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
            Regex stockNameAndCode = new Regex(@"[\u4e00-\u9fa5]+ *[(（]? *\d{6}(\.[a-zA-Z]+)?[)）]?\D");//匹配“泸州老窖（000568）”六位数加一个非数字
            Regex stockName = new Regex("[\u4e00-\u9fa5]+");
            Regex stockCode = new Regex(@"\d+");

            bool hasNCMatched = false;
            foreach (var line in lines)
            {
                if (stockNameAndCode.IsMatch(line))
                {
                    string snc = stockNameAndCode.Match(line).Value;
                    if (snc.Contains("邮编")) { continue; }
                    string sn = stockName.Match(snc).Value;
                    string sc = stockCode.Match(snc).Value;

                    anaReport.StockName = sn;
                    anaReport.StockCode = sc;

                    hasNCMatched = true;
                    break;
                }
            }
            if (hasNCMatched) { return true; }
            else { return false; }
        }

        public virtual bool extractStockOtherInfo()
        {
            //extract stock price, stock rating and stock rating change
            Regex stockRRC = new Regex(@"(买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避)[\(（][\u4e00-\u9fa5]{2,3}[\)）]");
            Regex stockR = new Regex(@"买入|增持|持有|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避");

            bool hasRRCMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "");

                    hasRRCMatched = true;
                    break;
                }
            }
            if (hasRRCMatched)
            {
                return true;
            }
            return false;
        }         

        /// <summary>
        /// Use a simple strategy that if one line contains [\u4e00-\u9fa5a][，。；] then regard it as main content
        /// Use a simple regulization strategy 
        /// </summary>
        /// <returns></returns>
        public virtual bool extractContent()
        {
            string content = "", normaledPara; 
            MatchCollection matchCol;
            Regex isContent = new Regex("[\u4e00-\u9fa5a][，。；]");
            Regex normalizedText = new Regex("[，,。.；;：:“”'\"《<》>？?{}\\[\\]【】()（）*&^$￥#…@！!~·`|+＋\\-－×_—=/、% 0-9a-zA-Z\u4e00-\u9fa5a]+");
            foreach (var para in finalParas)
            {
                if (isContent.IsMatch(para))
                {
                    normaledPara = "";
                    matchCol = normalizedText.Matches(para);
                    foreach (Match match in matchCol)
                    {
                        normaledPara += match.Value + "&&";
                    }
                    normaledPara = normaledPara.Remove(normaledPara.Length - 2).Trim();
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

        /// <summary>
        /// In this function we do not remove a single line in case the paragraph structure information(used to merge into paras) might lost
        /// We only remove those information which appears in the tail of a report
        /// Most of these information are something like:
        /// "投资评级说明"
        /// "法律声明"
        /// "公司简介"
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public virtual string[] removeAnyButContentInLines(string[] lines)
        {
            Regex InvestRatingStatement = new Regex("(^投资评级(的)?说明)|(投资评级(的)?(说明)?[:：]?$)|(评级(标准|说明)[:：]?$)");
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|((重要|特别)(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|研究员|作者)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");
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
                if (AnalystIntro.IsMatch(trimedLine))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        /// <summary>
        /// In this function we remove single lines(paras) which are wrongly recognized as main content
        /// Most of these wrongly recognized lines are something like:
        /// "《三季度延续高增长趋势，增长提速》2012-10-22"
        /// "数据来源：铁道部，第一创业证券研究所"
        /// "注：每股收益按增发后 69140.13 万股摊薄计算。"
        /// "图 9  管理费用上升，财务费用下降"
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public virtual string[] removeAnyButContentInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{15,} *\d{1,3}$");

            //Regex picOrTabHead = new Regex(@"^(图|表) *\d{1,2}");

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                {
                    continue;
                }
                if (indexEntry.IsMatch(trimedPara))
                {
                    continue;
                }
                if (trimedPara.Contains("数据来源："))
                {
                    if (trimedPara.StartsWith("数据来源：")) { continue; }
                    else
                    {
                        string shuju = noteShuju.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(shuju, "");
                        if (!mightBeContent.IsMatch(judgeStr)){ continue; }
                    }
                }
                if (trimedPara.Contains("资料来源："))
                {
                    if (trimedPara.StartsWith("资料来源：")) { continue; }
                    else
                    {
                        string ziliao = noteZiliao.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(ziliao, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                if (trimedPara.Contains("注："))
                {
                    if (trimedPara.StartsWith("注：")) { continue; }
                    else
                    {
                        string zhu = noteZhu.Match(trimedPara).Value;
                        string judgeStr = trimedPara.Replace(zhu, "");
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
                    }
                }
                newParas.Add(para);
            }
            return newParas.ToArray();
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