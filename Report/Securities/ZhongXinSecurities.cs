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
                    Trace.TraceError("ZhongXinSecurities.ZhongXinSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockBasicInfo()
        {

            return base.extractStockBasicInfo();
        }

        public override string[] removeAnyButContentInLines(string[] lines)
        {
            //string s = "n增长回报*估值倍数波动性北京双鹭药业 (002038.SZ)亚太医药行业平均水平投资摘要低 高百分位 20th 40th 60th 80th 100th* 回报 - 资本回报率 投资摘要指标的全面描述请参见本报告的信息披露部分。主要数据 当前股价(Rmb) 34.1112个月目标价格(Rmb) 33.58市值(Rmb mn / US$ mn) 8,472.9 / 1,240.4外资持股比例(%) --12/08 12/09E 12/10E 12/11E每股盈利(Rmb) 新 0.88 1.14 1.34 1.62每股盈利调整幅度(%) 0.0 (1.7) (5.6) (6.4)每股盈利增长(%) (19.2) 29.5 18.2 20.5每股摊薄盈利(Rmb) 新 0.86 1.11 1.32 1.59市盈率(X) 38.9 30.0 25.4 21.1市净率(X) 11.1 8.8 7.1 5.7EV/EBITDA(X) 28.4 24.819.9 15.6股息收益率(%) 0.6 0.8 0.9 1.1净资产回报率(%) 33.2 32.5 30.7 29.8股价走势图242628303234363840Jul-08 Oct-08 Feb-09 May-094005006007008009001,0001,1001,200北京双鹭药业  (左轴) 深证A股指数  (右轴)股价表现(%) 3个月 6个月 12个月绝对 5.6 (4.8) 6.1相对于深证A股指数 (25.3) (48.3) (19.1)资&&来源：公司数据、高盛研究预测、FactSet（股价为7/27/2009收盘价）杜玮, Ph.D";
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

        public override string[] removeOtherInParas(string[] paras)
        {
            Regex mightBeContent = new Regex("[\u4e00-\u9fa5a][，。；]");

            Regex refReportHead = new Regex(@"^(\d{1,2} *[.、]? *)?《");
            Regex refReportTail = new Regex(@"\d{4}[-\./]\d{1,2}([-\./]\d{1,2})?$");
            Regex refReportHT = new Regex(@"^《.*》$");

            Regex noteShuju = new Regex("数据来源：.*$");
            Regex noteZiliao = new Regex("资料来源：.*$");
            Regex noteZhu = new Regex("注：.*$");

            Regex indexEntry = new Regex(@"\.{9,} *.*\d{1,3}$");

            Regex picOrTabHead = new Regex(@"^(图|表|图表) *\d{1,2}");
            Regex extra = new Regex("^(请通过合法途径获取本公司研究报告，如经由未经|本报告中?的信息均来(自|源)于?已公开的?(信息|资料)|本公司具备证券投资咨询业务资格，请务必阅读最后一页免责声明|证监会审核华创证券投资咨询业务资格批文号：证监|请务必阅读|每位主要负责编写本)");//added

            Regex extra2 = new Regex(@"^\d{1,3}\..*\(.*\d{2}\)$");

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
                if (trimedPara.Contains("有关分析师的申明，见本报告最后部分。"))//added
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
                        if (!mightBeContent.IsMatch(judgeStr)) { continue; }
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

        public override bool extractDate()
        {
            if (base.extractDate())
            { return true; }

            return false;
        }
    }
}
