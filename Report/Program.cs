﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Spire.Pdf;
using org.apache.pdfbox;
using org.apache.pdfbox.cos;
using org.apache.pdfbox.util;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdfparser;
using java.io;
using java.util;

namespace Report
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlServerHandler slh = new SqlServerHandler();
            slh.Init();
            slh.LoadPersonTable();
            //PdfDocument doc = new PdfDocument();
            //doc.LoadFromFile("D:\\fangzheng_1.pdf");
            //StringBuilder buffer = new StringBuilder();
            //foreach (PdfPageBase page in doc.Pages)
            //{
            //    buffer.Append(page.ExtractText());
            //}
            //doc.Close();

            //string text = buffer.ToString();
            //string path_2 = "F:\\桌面文件备份\\mission\\分析师报告\\sample\\02338FE9-0DD6-4299-A2CE-50D4B8DBDE74.PDF";
            //string path_1 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告\\300039\\20120330-招商证券-上海凯宝-300039-产能瓶颈得到解决,业绩释放值得期待.pdf";
            //string path_guojun_1 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告\\300039\\20150528-国泰君安-上海凯宝-300039-主业平稳增长，外延动力十足.pdf";
            //string path_guojun_2 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\国泰君安\\015CF7D4-3E6F-4673-9B70-DB21994E71DE.PDF";
            //string path_guojun_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\国泰君安\\2015\\01EF95BC-B1CF-46F8-A7A1-985D8AFE4EFC.PDF";
            //string path_zhongxin_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中信证券\\2014\\CF3AB7B1-2A50-4506-A82C-8BAA9D78804D.PDF";
            //string path_zhongjin_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2015\\20150319-中金公司-华润三九-000999-经营触底，趋势向上.pdf";
            //string path_zhongjin_15_2 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2015\\20150826-中金公司-华兰生物-002007-业绩稳定增长，浆站拓展顺利.pdf";
            //string path_zhongjin_13 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\中金\\2013\\20130228-中金公司-上海凯宝-300039-业绩回顾：稳定增长现金牛公司.pdf";
            //string path_changjiang_15 = "F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告按证券分\\长江证券\\2011\\20110304-长江证券-益盛药业-002566-产品线丰富,拥有人参系列特色产品的中药企业.pdf";

            //PDDocument doc = PDDocument.load(path_zhongjin_15_2);
            //ZhongJinSecurities zx = new ZhongJinSecurities(doc);
            //string s = zx.loadPDFText();
            //zx.extractStockBasicInfo();
            //string s = zx.loadPDFText();
            //PDFTextStripper pdfStripper = new PDFTextStripper("unicode");
            //string text = pdfStripper.getText(doc).Replace("\r\n", "\n");
            //zx.extractContent();

            //GuoJunSecurities gj = new GuoJunSecurities(doc);
            //gj.extractContent();
            //string text = gj.loadPDFText();
            //PDDocument doc = PDDocument.load(path_1);
            //ZhaoShangSecurities s = new ZhaoShangSecurities(doc);
            //s.extractContent();
            //PDFTextStripper strip = new PDFTextStripper();
            //string pdftext = strip.getText(doc).Replace("\r\n", "\n");
            ////string pdftext = loadPDFText();
            //string[] lines = pdftext.Split('\n');
            //foreach (var line in lines)
            //{
                
            //    System.Console.WriteLine(line);
            //}
            //strip.setStartPage(1);
            //strip.setEndPage(2);
            //string text1_2 = strip.getText(doc);
            //string text_mat = strip.getParagraphEnd();
            //string s3 = strip.getPageSeparator();
            //string s4 = strip.getPageSeparator
            //PDDocument doc = PDDocument.load(path_1);
            //File pdfile = new File(path_1);
            //COSDocument cdoc = new COSDocument();
            //PDFParser parser = new PDFParser(new FileInputStream(pdfile));
            //parser.parse();
            //cdoc = parser.getDocument();

            //string text1_2_2 = strip.getText(cdoc);

            //java.util.List objectlist = cdoc.getObjects();
            //COSDictionary cosdic = cdoc.getTrailer();
            
            

            //COSDictionary page = PdfDocumentPageCollection
            //PDFText2HTML t2h = new PDFText2HTML("utf8");
            //Splitter sp = new Splitter();
            //var li = sp.split(doc);
            //PDDocument l0, l1, l2, l3, l4;
            //l0 = (PDDocument)li.get(0);
            //l1 = (PDDocument)li.get(1);
            //l2 = (PDDocument)li.get(2);
            //l3 = (PDDocument)li.get(3);
            //l4 = (PDDocument)li.get(4);
            //PDFTextStripper pdfStripper = new PDFTextStripper();
            //string s0 = pdfStripper.getText(l0);
            //string s1 = pdfStripper.getText(l1);
            //string s2 = pdfStripper.getText(l2);
            //string s3 = pdfStripper.getText(l3);
            //string s4 = pdfStripper.getText(l4);
            
            
            //COSArray coss = new COSArray();
            //COSDocument cosd = new COSDocument();
            //PDFSplit sp = PDFSplit.main(doc);
            //ReportParser report = new ReportParser(doc);
            //ZhaoShangSecurities zs_report = new ZhaoShangSecurities(doc);
//zs_report.extractContent();
            //PDFText2HTML p = new PDFText2HTML();
            //string c = p.getText(doc);
            //PDFTextStripper pdfStripper = new PDFTextStripper();
            ////pdfStripper.setFonts()

            //string text = pdfStripper.getText(doc);//.Replace("\r\n", "\n");



            System.Console.WriteLine("Hello");
            System.Console.WriteLine("hello");
        }
    }
}