﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Report.Securities
{
    //招商证券
    public class ZhaoShangSecurities : ReportParser
    {
        public ZhaoShangSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                try
                {
                    pdfText = loadPDFText();
                    lines = pdfText.Split('\n');
                    noABCLines = removeAnyButContentInLines(lines);
                    //noOtherLines = removeOtherInLines(lines);
                    mergedParas = mergeToParagraph(noABCLines);
                    advancedMergedParas = removeOtherInParas(mergedParas);
                    noABCParas = removeAnyButContentInParas(advancedMergedParas);
                    finalParas = noABCParas;
                    //noTableLines = removeTable(lines);
                    //noTableAndOtherLines = removeOther(noTableLines);
                    //mergedParas = mergeToParagraph(noTableAndOtherLines);
                }
                catch (Exception e)
                {
                    this.isValid = false;
                    Trace.TraceError("ZhaoShangSecurities.ZhaoShangSecurities(string pdReportPath): " + e.ToString());
                }
            }
        }

        public override bool extractStockInfo()
        {
            Regex stockTest = new Regex(@"(强烈推荐|审慎推荐|推荐|中性|回避)-[a-zA-Z]+");
            Regex stockTest2 = new Regex(@" *[\u4e00-\u9fa5a-zA-ZＡ]+");
            //Regex stockNCRRP = new Regex(@"(强烈推荐|审慎推荐|推荐|中性|回避)-[a-zA-Z]+（[\u4e00-\u9fa5]+） *[\u4e00-\u9fa5a-zA-Z]+ *[\(（]?\d+\.[a-zA-Z]+[\)）]?");//匹配强烈推荐-A（维持） 鄂武商Ａ 000501.SZ 
            Regex stockNCRRP = new Regex(@"(强烈推荐|审慎推荐|推荐|中性|回避)-[a-zA-Z]+（[\u4e00-\u9fa5]+） *\D+ *[(（]?\d+\.[a-zA-Z]+[)）]?");//匹配强烈推荐-A（维持） 鄂武商Ａ 000501.SZ 
            Regex stockNameAndCode = new Regex(@"\D+ *[(（]?\d+\.[a-zA-Z]+[)）]?");//匹配“云南白药  (000538.CH)”或 “云南白药  000538.CH”
            Regex stockName = new Regex(@"\D+");
            Regex stockCode = new Regex(@"\d+");
            Regex stockRRP = new Regex("(强烈推荐|审慎推荐|推荐|中性|回避).+（[\u4e00-\u9fa5]+）");
            Regex stockRating = new Regex("强烈推荐|审慎推荐|推荐|中性|回避");
            Regex stockRatingChange = new Regex(@"（[\u4e00-\u9fa5]+）");
            Regex stockPrice = new Regex(@"\d+(\.\d+)?");
            bool isNameCodeDone = false, isRandRCDone = false, isPriceDone = false;
            foreach (var line in lines)
            {
                if (isNameCodeDone && isRandRCDone && isPriceDone)
                {
                    break;
                }
                //if (stockTest2.IsMatch(line))
                //{
                //    Console.WriteLine("");
                //}
                if (!isNameCodeDone && stockNCRRP.IsMatch(line))
                {
                    string snc = stockNameAndCode.Match(line).Value.Replace("）", "").Trim();
                    string sn = stockName.Match(snc).Value;
                    string sc = stockCode.Match(snc).Value;

                    anaReport.StockName = sn;
                    anaReport.StockCode = sc;

                    isNameCodeDone = true;

                    string srrc = stockRRP.Match(line).Value;
                    anaReport.StockRating = stockRating.Match(srrc).Value;
                    anaReport.RatingChanges = stockRatingChange.Match(srrc).Value.Replace("（", "").Replace("）", "");

                    isRandRCDone = true;
                    //if (stockRating.IsMatch(line) && stockRatingChange.IsMatch(line))
                    //{
                    //    anaReport.StockRating = stockRating.Match(line).Value;
                    //    anaReport.RatingChanges = stockRatingChange.Match(line).Value.Replace("（", "").Replace("）", "");

                    //    isRandRCDone = true;
                    //}
                }
                if (!isPriceDone && line.Trim().StartsWith("当前股价："))
                {
                    if (stockPrice.IsMatch(line))
                    {
                        try
                        {
                            anaReport.StockPrice = float.Parse(stockPrice.Match(line).Value);
                            //anaReport.StockPrice.ToString();
                            isPriceDone = true;
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("GuoJunSecurities.extractStockOtherInfo(): " + e.Message);
                        }
                    }
                }
            }
            if (!isRandRCDone)//如果没有匹配成功，则调用基类的方法
                isRandRCDone = base.extractStockOtherInfo();

            return isNameCodeDone && isRandRCDone && isPriceDone;
        }

        //public override bool extractcontent()
        //{
        //    string content = "";
        //    regex iscontent = new regex("[\u4e00-\u9fa5a][，。；]");
        //    foreach (var para in finalparas)
        //    {
        //        string normaledpara = para.replace(" ", "").trim();
        //        if (iscontent.ismatch(para))
        //        {
        //            content += normaledpara + '\n';
        //        }
        //    }
        //    anareport.content = content;
        //    return true;
        //}

        //public override bool extractContent()
        //{      
        //    string content = "";

        //    Regex paraHead = new Regex("^\\D ");
        //    Regex chinese = new Regex("[\u4e00-\u9fa5]");
        //    int index = 0; bool isFirst = true;
        //    foreach (var para in mergedParas)
        //    {
        //        if (paraHead.IsMatch(para) && chinese.IsMatch(para))
        //        {
        //            if (isFirst)
        //            {
        //                content += mergedParas.ElementAt(index-1) + "\n";
        //                isFirst = false;
        //            }
        //            content += para.Replace(" ","").Trim() + "\n";
        //        }
        //        index++;
        //    }
        //    anaReport.Content = content;
        //    return true;
        //}

        public override string[] removeTableInLines(string[] lines)
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

        public override string[] removeOtherInLines(string[] lines)
        {
            //Regex spaces = new Regex(" ")
            //Regex referencesReport = new Regex(@"^\d+、《.+\d");
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                string trimedLine = line.Trim();
                //if (string.IsNullOrEmpty(trimedLine))
                //{
                //    continue;
                //}
                if (trimedLine.StartsWith("资料来源"))
                {
                    continue;
                }
                if (trimedLine.StartsWith("数据来源："))
                {
                    continue;
                }
                if (trimedLine.Equals("分析师承诺"))
                {
                    break;
                }
                if (trimedLine.Equals("投资评级定义"))
                {
                    break;
                }
                if (trimedLine.Equals("重要声明"))
                {
                    break;
                }
                newLines.Add(line);
            }
            return newLines.ToArray();
        }

        //public override string[] mergeToParagraph(string[] lines)
        //{
        //    string curPara = "";
        //    List<string> paragraphs = new List<string>();
        //    foreach (var line in lines)
        //    {
        //        if (line.EndsWith(" "))
        //        {
        //            curPara += line;
        //            paragraphs.Add(curPara);
        //            curPara = "";
        //            continue;
        //        }
        //        curPara += line;
        //    }
        //    return paragraphs.ToArray();
        //}

        public override string[] removeOtherInParas(string[] paras)
        {
            Regex referencesReport = new Regex(@"^\d+、 *《.+\d");
            Regex referencesReport2 = new Regex(@"^\d+、.*?\d{4}[/\.]\d{1,2}[/\.]\d{1,2}");
            List<string> newLines = new List<string>();
            foreach (var para in paras)
            {
                string trimedLine = para.Trim();
                if (referencesReport.IsMatch(trimedLine))
                {
                    continue;
                }
                if (referencesReport2.IsMatch(trimedLine))
                {
                    continue;
                }
                newLines.Add(para);
            }
            return newLines.ToArray();
        }

        public override bool extractDate()
        {
            //2009年10月14日

            if (base.extractDate())
            { return true; }

            return false;
        }
    }
}
