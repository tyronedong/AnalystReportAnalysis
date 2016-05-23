using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using org.pdfbox.pdmodel;
using org.pdfbox.util;

namespace Stock
{
    class Program
    {
        static void Main(string[] args)
        {
            ////PDDocumentInformation ingo = new PDDocumentInformation()
            ////PDDocument doc = PDDocument.load("F:\\桌面文件备份\\mission\\分析师报告\\分析师研报\\分析师报告\\002566\\20120828-东兴证券-益盛药业-002566-调研报告：首批非林地参明年将进入收获期.pdf");
            //PDDocument doc = PDDocument.load("D:\\fangzheng_1.pdf");
            
            ////PDFText2HTML p = new PDFText2HTML();
            ////string c = p.getText(doc);
            //PDFTextStripper pdfStripper = new PDFTextStripper();
            //////pdfStripper.setFonts()
            
            //string text = pdfStripper.getText(doc);//.Replace("\r\n", "\n");

            //Console.WriteLine("Hello");
            //string filePath = @"E:\数据\分析师研报\按照证券公司分类\招商证券\20150826-招商证券-华润三九-000999-中报增长平稳，未来提升空间较大.pdf";
            //StockData stocdData = new StockData(filePath);
            //stocdData.getStockjobber();

            string filePath2 = "F:/桌面文件备份/mission/分析师报告/分析师研报/分析师报告/000538/20150819-兴业证券-云南白药-000538-公司点评报告：业绩增长平稳，健康产品发展态势良好.pdf";
            StockData stocdData2 = new StockData(filePath2);

            //string filePath3 = "E:/数据/分析师研报/按照证券公司分类/平安证券/20150917-平安证券-安科生物-300009-公司调研报告：内生增长动力强劲，进军精准医疗，围绕生物大健康布局.pdf";
            //StockData stocdData3 = new StockData(filePath3);

            //string filePath4 = "E:/数据/分析师研报/按照证券公司分类/长江证券/20141111-长江证券-丰原药业-000153-深度报告：如虎添翼，腾飞在即.pdf";
            //StockData stocdData4 = new StockData(filePath4);

            //string filePath5 = "E:/数据/分析师研报/按照证券公司分类/安信证券/20150414-安信证券-探路者-300005-发起设立旅游基金，分享万亿旅游服务市场.pdf";
            //StockData stocdData5 = new StockData(filePath5);

            //string filePath6 = "E:/数据/分析师研报/按照证券公司分类/中金公司/20081024-中金公司-丽珠集团-000513-投资损失吞噬经营利润.pdf";
            //StockData stocdData6 = new StockData(filePath6);
            //stocdData6.setStockjobber("中金公司");

            //string filePath7 = "E:/数据/分析师研报/按照证券公司分类/海通证券/20070116-海通证券--雅戈尔房地产投资步伐加快，调高目标价格(买入,_维持)_.pdf";
            //StockData stocdData7 = new StockData(filePath7);

            //string filePath8 = "E:/数据/分析师研报/按照证券公司分类/申银万国/20150120-申银万国-嘉欣丝绸-002404-打造“网上丝绸之路”，长期看好公司供应链金融闭环交易平台.pdf";
            //StockData stocdData8 = new StockData(filePath8);
            //stocdData8.setStockjobber("申银万国");

            //string filePath9 = "E:/数据/分析师研报/按照证券公司分类/东北证券/20080505-东北证券-云南白药-000538-平稳增长，蓄势待发.pdf";
            //StockData stocdData9 = new StockData(filePath9);
            //stocdData9.setStockjobber("东北证券");

            //string filePath10 = "E:/数据/分析师研报/按照证券公司分类/招商证券/20060710-招商证券-东阿阿胶-000423-经营拐点确立，消费升级鸿篇巨制徐徐开启.pdf";
            //StockData stocdData10 = new StockData(filePath10);
            //stocdData10.setStockjobber("招商证券");

            //string filePath12 = @"E:/数据/分析师研报/按照证券公司分类/东兴证券/20110808-东兴证券-永安药业-002365-中报点评 量价齐升,业绩小幅反弹.pdf";
            //StockData stocdData12 = new StockData(filePath12);

            //string filePath13 = @"E:/数据/分析师研报/按照证券公司分类/方正证券/20150615-方正证券-安科生物-300009-公司事件点评：携手博生吉，逐步布局细胞治疗.pdf";
            //StockData stocdData13 = new StockData(filePath13);

            //string filePath14 = @"E:\数据\分析师研报\按照证券公司分类\民生证券\20111024-民生证券-太安堂-002433-三季度业绩再超预期,建议关注.pdf";
            //StockData stocdData14 = new StockData(filePath14);
            //stocdData14.getStockjobber();

            //CommonStock zhaoshang = new CommonStock(stocdData); zhaoshang.extrcactContent(); zhaoshang.saveResult("D:\\1.txt");
            XingYeStock xingye = new XingYeStock(stocdData2); xingye.extractContent(xingye.loadPDFLines());//xingye.extrcactContent(); xingye.saveResult("D:\\2.txt");
            Console.WriteLine("");
            //PingAnStock pingan = new PingAnStock(stocdData3); pingan.extrcactContent(); pingan.saveResult("D:\\3.txt");
            //ChangJiangStock changjiang = new ChangJiangStock(stocdData4); changjiang.extrcactContent(); changjiang.saveResult("D:\\4.txt");
            //AnXinStock anxin = new AnXinStock(stocdData5); anxin.extrcactContent(); anxin.saveResult("D:\\5.txt");
            //CommonStock zhongjin = new CommonStock(stocdData6); zhongjin.extrcactContent(); zhongjin.saveResult("D:\\6.txt");
            //CommonStock haitong = new CommonStock(stocdData7); haitong.extrcactContent(); haitong.saveResult("D:\\7.txt");
            //ShenYinStock shenyin = new ShenYinStock(stocdData8); shenyin.extrcactContent(); shenyin.saveResult("D:\\8.txt");
            //DongBeiStock dongbei = new DongBeiStock(stocdData9); dongbei.extrcactContent(); dongbei.saveResult("D:\\9.txt");
            //DongXingStock dongxing = new DongXingStock(stocdData12); dongxing.extrcactContent(); dongxing.saveResult("D:\\10.txt");
            //FangZhengStock fangzheng = new FangZhengStock(stocdData13); fangzheng.extrcactContent(); fangzheng.saveResult("D:\\11.txt");
            //CommonStock datong = new CommonStock(stocdData14); datong.extrcactContent(); datong.saveResult("D:\\12.txt");
        }
    }
}
