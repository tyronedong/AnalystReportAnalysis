using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using Report.Outsider;

namespace Report
{
    public class ReportParser
    {
        public bool isValid = false;

        protected WordSegHandler wsH;

        protected string pdfPath;
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

        /// <summary>
        /// with database
        /// </summary>
        /// <param name="pdfPath"></param>
        public ReportParser(string pdfPath)
        {
            this.pdfPath = pdfPath;
            anaReport = new AnalystReport();
            try
            {
                pdfReport = PDDocument.load(pdfPath);
                //wsH = new WordSegHandler();//added to nodb handle
                isValid = true;
            }
            catch (Exception e)
            {
                Trace.TraceError("ReportParser.ReportParser(string pdfPath): " + e.Message);
                isValid = false;
            }
        }

        ///// <summary>
        ///// no database
        ///// </summary>
        ///// <param name="pdfPath"></param>
        ///// <param name="wsH"></param>
        //public ReportParser(string pdfPath, ref WordSegHandler wsH)
        //{
        //    this.pdfPath = pdfPath;
        //    anaReport = new AnalystReport();
        //    try
        //    {
        //        pdfReport = PDDocument.load(pdfPath);
        //        this.wsH = wsH;
        //        isValid = true;
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.TraceError("ReportParser.ReportParser(string pdfPath): " + e.Message);
        //        isValid = false;
        //    }
        //}
        //public ReportParser(string pdfPath, string stockjobber)
        //{
        //    anaReport = new AnalystReport();
        //    anaReport.Stockjobber = stockjobber;
        //    try
        //    {
        //        pdfReport = PDDocument.load(pdfPath);
        //        isValid = true;
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.TraceError("ReportParser.ReportParser(string pdfPath, string stockjobber): " + e.Message);
        //        isValid = false;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <returns>
        /// null if wrong
        /// empty if not found
        /// </returns>
        public static string findBrokerage(string pdfPath)
        {
            string SecurityNameDicPath = ConfigurationManager.AppSettings["SecNameDic_Path"];
            string[] securityNames = loadSecurityNames(SecurityNameDicPath);

            if (securityNames == null) { return null; }//null if wrong
            //judge by pdfPath
            foreach (var name in securityNames)
            {
                if (pdfPath.Contains(name))
                    return name;
            }

            //judge by pdf context
            string context = loadPDFText(pdfPath);
            if (string.IsNullOrEmpty(context)) { return null; }//null if text is null
            string[] lines = context.Split('\n').Reverse().ToArray();//从后往前搜索证券公司的名字
            foreach (var line in lines)
            {
                foreach (var name in securityNames)
                {
                    if (line.Contains(name))
                        return name;
                }
            }

            return "";//empty if not found
        }

        private static string[] loadSecurityNames(string dicPath)
        {
            
            try
            {
                List<string> dic = new List<string>();
                if (dic == null) dic = new List<string>();
                string[] lines = File.ReadAllLines(dicPath);
                foreach (string line in lines)
                {
                    dic.Add(line);
                }

                return dic.ToArray();
            }
            catch (Exception ex) { return null; }
        }

        public virtual AnalystReport executeExtract_withdb()
        {

            extractStockInfo();
            extractContent();

            extractCountInfo();//added
            extractReportInfo();//added;

            return anaReport;
        }

        public virtual AnalystReport executeExtract_nodb(ref WordSegHandler wsH)
        {
            this.wsH = wsH;

            extractStockInfo();
            extractContent();

            extractDate();
            extractAnalysts();
            
            return anaReport;
        }

        /// <summary>
        /// extract report title and type information
        /// must be executed after function extractStockInfo
        /// </summary>
        /// <returns></returns>
        public virtual bool extractReportInfo()
        {
            return extractReportTitle() && extractReportType();
        }

        public virtual bool extractReportTitle()
        {
            Regex chinese = new Regex("[\u4e00-\u9fa5]+");
            Regex nonsense = new Regex(@"^(\d* *)?(敬)?(各项声明|请阅读|请仔细阅读|请务必阅读|请通过合法途径|本(研究)?报告|本公司|市场有风险，投资需谨慎|证监会审核华创证券投资咨询业务资格批文号：证监|此份報告由群益證券)");//added
            Regex stockNameAndCode = new Regex(@"[\u4e00-\u9fa5]+ *[(（]? *\d{6}(\.[a-zA-Z]+)?[)）]?\D");//匹配“泸州老窖（000568）”六位数加一个非数字

            foreach (var line in lines)
            {
                string trimedLine = line.Trim();

                if (string.IsNullOrEmpty(trimedLine))
                { continue; }
                if (!chinese.IsMatch(trimedLine))
                { continue; }
                if (nonsense.IsMatch(trimedLine))//每篇报告开头都有的没有意义的话
                { continue; }
                if (trimedLine.Contains("报告") && trimedLine.Length <= 6)//非标题，但是含有报告，通常说明了本报告的类型
                { continue; }
                if (trimedLine.Contains("评级") && trimedLine.Length <= 4)
                { continue; }
                if (trimedLine.Equals(anaReport.StockName))
                { continue; }
                if (stockNameAndCode.IsMatch(trimedLine))
                { continue; }

                anaReport.ReportTitle = trimedLine;
                break;
            }

            return true;
        }

        public virtual bool extractReportType()
        {
            Regex isContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex type1 = new Regex(@"(年报|季报|业绩点评|公告|事件点评|事项点评|业绩回顾|点评报告|事件|事项)");//点评报告有问题
            Regex type2 = new Regex(@"深度报告|深度研究|深度跟踪|调研报告|调研简报|公司研究报告|公司动态|公司更新报告|评级调整|公司快报|动态跟踪|调整预测");
            Regex type3 = new Regex(@"新股");

            int breakCounter = 0;
            foreach(var line in lines)
            {
                if (isContent.IsMatch(line))
                { breakCounter++; }

                if (breakCounter > 5)
                { anaReport.ReportType = "常规报告"; return true; }

                if(type1.IsMatch(line))
                { anaReport.ReportType = "特殊事项点评"; return true; }
                //if(type2.IsMatch(line))
                //{ anaReport.ReportType = "常规报告"; return true; }
                if(type3.IsMatch(line))
                { anaReport.ReportType = "新股分析"; return true; }
            }

            return false;
        }

        /// <summary>
        /// This function is executed after report content was extracted.
        /// 计算一篇研报中图表（图和表）的数量
        /// </summary>
        /// <returns></returns>
        public virtual void extractCountInfo()
        {
            extractPicTableCount();
            extractValueCount();
        }

        public virtual void extractPicTableCount()
        {
            Regex picHead = new Regex(@"^图");
            Regex picTail = new Regex(@"(趋势|走势|表现|变化|对比)图?[:：]?$");

            Regex tableHead = new Regex(@"^表");
            Regex tableTail = new Regex(@"表$");

            //get picCount and tableCount 
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();

                if (trimedLine.StartsWith("图表"))//处理图表
                {
                    if (picTail.IsMatch(trimedLine))//只要不是图，就是表
                    { anaReport.picCount++; }
                    else { anaReport.tableCount++; }
                    continue;
                }

                if (picHead.IsMatch(trimedLine))
                { anaReport.picCount++; continue; }
                if (tableHead.IsMatch(trimedLine))
                { anaReport.tableCount++; continue; }
                if (picTail.IsMatch(trimedLine))
                { anaReport.picCount++; continue; }
                if (tableTail.IsMatch(trimedLine))
                { anaReport.tableCount++; }
            }

            return;
        }

        public virtual void extractValueCount()
        {
            Regex value = new Regex(@"\d+(\.\d+)?");

            //if(noABCLines == null)
            //{ Trace.TraceError("noABCLine is null"); }
            //get total value count
            foreach (var nLine in noABCLines)
            {
                if (value.IsMatch(nLine))
                { anaReport.valueCountOutContent += value.Matches(nLine).Count; }
            }

            //get value count in content
            if (string.IsNullOrEmpty(anaReport.Content))
            {
                return;
            }
            else
            {
                if (value.IsMatch(anaReport.Content))
                { anaReport.valueCountInContent += value.Matches(anaReport.Content).Count; }
            }

            //get value count out content
            anaReport.valueCountOutContent -= anaReport.valueCountInContent;
            
            return;
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
            Regex stockRRC = new Regex(@"(看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|推荐|回避) *(([\(（][\u4e00-\u9fa5]{2,4}[\)）])|(/ *[\u4e00-\u9fa5]{2,4}))");//推荐(维持)||推荐/维持
            Regex stockR = new Regex(@"看好|看淡|买入|增持|持有|减持|卖出|强于大市|中性|弱于大市|强烈推荐|审慎推荐|谨慎推荐|推荐|回避");

            bool hasRRCMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                if (stockRRC.IsMatch(line))
                {
                    string srrc = stockRRC.Match(line).Value;
                    anaReport.StockRating = stockR.Match(srrc).Value;
                    anaReport.RatingChanges = srrc.Replace(anaReport.StockRating, "").Replace("(", "").Replace("（", "").Replace("）", "").Replace(")", "").Replace("/","").Trim();

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
            Regex normalizedText = new Regex("[，,。.．；;：:“”'\"《<》>？?{}\\[\\]【】()（）*&^$￥#…@！!~～·`|+＋\\-－×_—=/、%％ 0-9a-zA-Z\u4e00-\u9fa5a]+");
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
        /// This is the basic time extractor. Only when the returned value is true can the result be credible.
        /// </summary>
        /// <returns></returns>
        public virtual bool extractDate()
        {
            //just get date from database
            Regex dateInPath = new Regex(@"20\d{2}[01]\d[0-3]\d");

            Regex regDate1 = new Regex(@"(报告|分析|发布)日期[:：]？ *20\d{2} ?[-.年] ?\d{1,2} ?[-.月] ?\d{1,2} ?日?");
            Regex regDate2 = new Regex(@"^20\d{2} ?年 ?\d{1,2} ?月 ?\d{1,2} ?日$");
            //Regex regDate2 = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日$");
            //Regex regDate3 = new Regex(@"^20\d{2} ?年 ?\d ?月 ?[0-3]\d ?日$");
            //Regex regDate4 = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?\d ?日$");
            //Regex regDate5 = new Regex(@"^20\d{2} ?年 ?\d ?月 ?\d ?日$");

            Regex regDate1f = new Regex(@"^20\d{2} ?年 ?\d{1,2} ?月 ?\d{1,2} ?日");
            Regex regDate2f = new Regex(@"20\d{2} ?年 ?\d{1,2} ?月 ?\d{1,2} ?日$");
            Regex regDate3f = new Regex(@"20\d{2} ?\. ?\d{1,2} ?\. ?\d{1,2}");
            Regex regDate4f = new Regex(@"20\d{2} ?- ?\d{1,2} ?- ?\d{1,2}");
            //Regex regDate1f = new Regex(@"^20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日");
            //Regex regDate2f = new Regex(@"20\d{2} ?年 ?[01]\d ?月 ?[0-3]\d ?日$");
            //Regex regDate3f = new Regex(@"20\d{2}\.[01]\d\.[0-3]\d");
            //Regex regDate4f = new Regex(@"20\d{2}-[01]\d-[0-3]\d");

            string format = "yyyyMMdd";

            //string format1 = "报告日期yyyy-MM-dd";
            //string format2 = "yyyy年MM月dd日";
            //string format3 = "yyyy年M月dd日";
            //string format4 = "yyyy年MM月d日";
            //string format5 = "yyyy年M月d日";
            
            //string format1f = "yyyy年MM月dd日";
            //string format2f = "yyyy年MM月dd日";
            //string format3f = "yyyy.MM.dd";
            //string format4f = "yyyy-MM-dd";

            if (dateInPath.IsMatch(pdfPath))
            {
                string dateString = dateInPath.Match(pdfPath).Value;
                anaReport.Date = DateTime.ParseExact(dateString, format, System.Globalization.CultureInfo.CurrentCulture);
                return true;
            }

            bool hasFalseDateMatched = false;
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();//remove whitespace from head and tail

                if (regDate1.IsMatch(trimedLine))
                {
                    string dateStr1 = regDate1.Match(trimedLine).Value.Replace(":", "").Replace("：", "").Replace(" ", "").Replace("报告", "").Replace("分析", "").Replace("发布", "").Replace("日期", "").Trim();
                    string curFormat = GetDateStrFormat(dateStr1);
                    //if (curFormat == null) { continue; }
                    anaReport.Date = DateTime.ParseExact(dateStr1, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }
                if (regDate2.IsMatch(trimedLine))
                {
                    string dateStr2 = regDate2.Match(trimedLine).Value.Replace(" ", "");
                    string curFormat = GetDateStrFormat(dateStr2);
                    anaReport.Date = DateTime.ParseExact(dateStr2, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    return true;
                }

                if (!hasFalseDateMatched && regDate1f.IsMatch(trimedLine))
                {
                    string dateStr1f = regDate1f.Match(trimedLine).Value.Replace(" ", "");
                    string curFormat = GetDateStrFormat(dateStr1f);
                    anaReport.Date = DateTime.ParseExact(dateStr1f, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    hasFalseDateMatched = true;
                }
                if (!hasFalseDateMatched && regDate2f.IsMatch(trimedLine))
                {
                    string dateStr2f = regDate2f.Match(trimedLine).Value.Replace(" ", "");
                    string curFormat = GetDateStrFormat(dateStr2f);
                    anaReport.Date = DateTime.ParseExact(dateStr2f, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    hasFalseDateMatched = true;
                }
                if (!hasFalseDateMatched && regDate3f.IsMatch(trimedLine))
                {
                    string dateStr3f = regDate3f.Match(trimedLine).Value.Replace(" ", "");
                    string curFormat = GetDateStrFormat(dateStr3f);
                    anaReport.Date = DateTime.ParseExact(dateStr3f, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    hasFalseDateMatched = true;
                }
                if (!hasFalseDateMatched && regDate4f.IsMatch(trimedLine))
                {
                    string dateStr4f = regDate4f.Match(trimedLine).Value.Replace(" ", "");
                    string curFormat = GetDateStrFormat(dateStr4f);
                    anaReport.Date = DateTime.ParseExact(dateStr4f, curFormat, System.Globalization.CultureInfo.CurrentCulture);
                    hasFalseDateMatched = true;
                }
            }

            return false;
        }

        /// <summary>
        /// Input string must have the format of yyyy年MM月dd日, yyyy.MM.dd or yyyy-MM-dd
        /// </summary>
        /// <param name="dateStr"></param>
        /// <returns></returns>
        public string GetDateStrFormat(string dateStr)
        {
            StringBuilder sb = new StringBuilder();

            if (dateStr.Contains("年") || dateStr.Contains("月") || dateStr.Contains("日"))
            {
                sb.Append("yyyy年");
                int mlen = dateStr.IndexOf("月")-dateStr.IndexOf("年");
                for (int i = 1; i < mlen; i++) { sb.Append("M"); }

                sb.Append("月");
                
                int dlen = dateStr.IndexOf("日") - dateStr.IndexOf("月");
                for (int i = 1; i < mlen; i++) { sb.Append("d"); }

                sb.Append("日");

                return sb.ToString();
            }

            if (dateStr.Contains("."))
            {
                string[] tList = dateStr.Split('.');
                if (tList.Length != 3) { return null; }
                
                for (int i = 0; i < tList[0].Length; i++) { sb.Append("y"); }
                sb.Append(".");

                for (int i = 0; i < tList[1].Length; i++) { sb.Append("M"); }
                sb.Append(".");

                for (int i = 0; i < tList[2].Length; i++) { sb.Append("d"); }

                return sb.ToString();
            }

            if (dateStr.Contains("-"))
            {
                string[] tList = dateStr.Split('-');
                if (tList.Length != 3) { return null; }

                for (int i = 0; i < tList[0].Length; i++) { sb.Append("y"); }
                sb.Append("-");

                for (int i = 0; i < tList[1].Length; i++) { sb.Append("M"); }
                sb.Append("-");

                for (int i = 0; i < tList[2].Length; i++) { sb.Append("d"); }

                return sb.ToString();
            }
            return null;
        }

        /// <summary>
        /// We only extract the first name meet in the context and asume it was the analyst
        /// </summary>
        /// <returns></returns>
        public virtual bool extractAnalysts()
        {
            List<Analyst> analysts = new List<Analyst>();
            
            if (!wsH.isValid) 
            {
                Trace.TraceError("ReportParser.extractAnalysts(): NLPIR init failed");
                return false; 
            }

            int index = 0; bool hasAnalystMatched = false;
            foreach (var line in lines)
            {
                if (!wsH.ExecutePartition(line))
                { index++; continue; }

                string[] analystsNames = wsH.GetPersonNames();
                
                if (analystsNames.Length == 0)
                { index++; continue; }

                Analyst analyst = getAnalyst(analystsNames[0], index);
                
                if (analyst == null)
                { index++; continue; }
                else
                {
                    anaReport.Analysts.Add(analyst);
                    hasAnalystMatched = true;
                    break;
                }
            }
            return hasAnalystMatched;
        }

        private Analyst getAnalyst(string name, int index)
        {
            List<string> infoLines = new List<string>();
            Analyst analyst = new Analyst(name);

            string regexStr = "[a-z0-9]+([._\\-]*[a-zA-Z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+"; //邮箱正则表达式  Sxg5661@163.com这个例子比较少，A-Z可以看情况取舍
            Regex regEmail = new Regex(regexStr);

            string regexPhone1 = "[0-9]+[ ]*[-][ ]*[0-9]+[ ]*[-][ ]*[0-9]+";
            Regex regPhone1 = new Regex(regexPhone1);
            string regexPhone2 = "[0-9]+[ ]*[-][ ]*[0-9]+";
            Regex regPhone2 = new Regex(regexPhone2);
            string regexPhone3 = "[0-9]+[ ]*[－][ ]*[0-9]+[ ]*[－][ ]*[0-9]+";
            Regex regPhone3 = new Regex(regexPhone3);
            string regexPhone4 = "[0-9]+[ ]*[－][ ]*[0-9]+";
            Regex regPhone4 = new Regex(regexPhone4);
            string regexPhone5 = "[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[－][ ]*[0-9]+";
            Regex regPhone5 = new Regex(regexPhone5);
            string regexPhone6 = "[0-9]+[ ]*[－][ ]*[0-9]+";
            Regex regPhone6 = new Regex(regexPhone6);
            string regexPhone7 = "[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+";
            Regex regPhone7 = new Regex(regexPhone7);

            string regexSAC = @"S\d{13}";
            Regex regSAC = new Regex(regexSAC);

            for (int i = 0; i < 6; i++)
            {
                if (index + i >= lines.Length - 1) { break; }
                infoLines.Add(lines[index + i]);
            }

            bool hasEmailMatched = false, hasPhoneMatched = false, hasSACMatched = false;
            foreach (var line in infoLines)
            {
                if (hasEmailMatched && hasPhoneMatched && hasSACMatched) { break; }

                if (!hasEmailMatched && regEmail.IsMatch(line))
                {
                    analyst.Email = regEmail.Match(line).Value;
                    hasEmailMatched = true;
                }

                if (!hasPhoneMatched && regPhone1.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone1.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone2.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone2.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone3.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone3.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone4.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone4.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone5.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone5.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone6.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone6.Match(line).Value;
                    hasPhoneMatched = true;
                }
                else if (!hasPhoneMatched && regPhone7.IsMatch(line))
                {
                    analyst.PhoneNumber = regPhone7.Match(line).Value;
                    hasPhoneMatched = true;
                }

                if (!hasSACMatched && regSAC.IsMatch(line))
                {
                    analyst.CertificateNumber = regSAC.Match(line).Value;
                    hasSACMatched = true;
                }
            }

            if(!(hasEmailMatched||hasPhoneMatched||hasSACMatched))
            {
                return null;
            } 

            return analyst;
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
            double perCounter = 0;
            double perPerLine = 1.0 / lines.Length;

            Regex InvestRatingStatement = new Regex("(^投资评级(的)?(说明|定义))|(投资评级(的)?(说明|定义)?[:：]?$)|(评级(标准|说明|定义)[:：]?$)");
            Regex Statements = new Regex("^(((证券)?分析师(申明|声明|承诺))|((重要|特别)(声|申)明)|(免责(条款|声明|申明))|(法律(声|申)明)|(独立性(声|申)明)|(披露(声|申)明)|(信息披露)|(要求披露))[:：]?$");
            Regex FirmIntro = new Regex("公司简介[:：]?$");
            Regex AnalystIntro = new Regex("^(分析师|分析师与联系人|分析师与行业专家|研究员|作者|研究团队)(简介|介绍)[\u4e00-\u9fa5a]*?[:：]?$");

            Regex extra = new Regex(@"^银河证券行业评级体系");//added

            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                //add this regulation to aviod the loss of main content
                if (perCounter <= 0.30)
                {
                    newLines.Add(line);
                    perCounter += perPerLine;
                    continue;
                }

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
                if(extra.IsMatch(trimedLine))//added
                {
                    break;
                }
                newLines.Add(line);
                perCounter += perPerLine;
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

            Regex refReportHead = new Regex(@"^(\d{1,2} *[.、]? *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");
            Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{9,}.*\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(图|表|图表) *\d{1,2}");
            Regex extra = new Regex("^(独立性申明：|请通过合法途径获取本公司研究报告，如经由未经|本报告中?的?信息均来(自|源)于?已?公开的?(信息|资料)|本公司具备证券投资咨询业务资格，请务必阅读最后一页免责声明|证监会审核华创证券投资咨询业务资格批文号：证监|请务必阅读|每位主要负责编写本|市场有风险，投资需谨慎|此份報告由群益證券)");//added

            List<string> newParas = new List<string>();
            foreach (var para in paras)
            {
                string trimedPara = para.Trim();
                if (refReportHead.IsMatch(trimedPara) && refReportTail.IsMatch(trimedPara))
                {
                    continue;
                }
                if (refReportHT.IsMatch(trimedPara))
                {
                    continue;
                }
                if (indexEntry.IsMatch(trimedPara))
                {
                    continue;
                }
                if (extra.IsMatch(trimedPara))//added
                {
                    continue;
                }
                if (picOrTabHead.IsMatch(trimedPara))
                {
                    continue;
                }
                if (trimedPara.StartsWith("本人") && trimedPara.Contains("在此申明"))
                { continue; }
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

                if (isTableDigits(trimedPara))
                {
                    continue;
                }

                newParas.Add(para);
            }
            return newParas.ToArray();
        }

        public bool isTableDigits(string para)
        {
            //judge if most is digit
            Regex digiPat = new Regex(@"[0-9a-zA-Z% .,]{15,}");

            Regex digit = new Regex(@"[0-9a-zA-Z% .,]");
            double dPer = digit.Matches(para).Count * 1.0 / (para.Length + 1);

            return digiPat.IsMatch(para) && (dPer > 0.6);
            //int digiCount = 0;
            //foreach (var c in para)
            //{
            //    if (char.IsDigit(c))
            //    { digiCount++; }
            //    else if (char.IsWhiteSpace(c))
            //    { digiCount++; }
            //    else if (c.Equals('.'))
            //    { digiCount++; }
            //    else if (c.Equals('%'))
            //    { digiCount++; }
            //    else if(c.Equals('·'))
            //    { digiCount++; }
            //    else if(c.Equals('、'))
            //    { digiCount--; }
            //}
            //double dPer = digiCount * 1.0 / para.Length;
            //if (para.Length < 140)
            //{
            //    if (dPer > 0.80)
            //        return true;
            //}
            //else if (dPer < 250)
            //{
            //    if (dPer > 0.75)
            //        return true;
            //}
            //else
            //{
            //    if (dPer > 0.65)
            //        return true;
            //}
            //return false;
        }

        public static string loadPDFText(string pdfPath)
        {
            try
            {
                PDDocument pdfReport = PDDocument.load(pdfPath);
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(pdfReport).Replace("\r\n", "\n");
                pdfReport.close();
                return text;
            }
            catch (Exception e) { return null; }
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
            //some report not end paragraph with ' ', 
            //so use '。' to handle these cases
            if(paragraphs.Count==0)
            {
                curPara = "";
                foreach (var line in lines)
                {
                    if (line.EndsWith("。"))
                    {
                        curPara += line;
                        paragraphs.Add(curPara);
                        curPara = "";
                        continue;
                    }
                    curPara += line;
                }
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