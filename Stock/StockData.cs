using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.pdfbox.pdmodel;
using org.pdfbox.util;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Stock
{
   //基类
    public class StockData
    {
        public string ReportPath = null;

        public string StockName = null;
        public string StockCode = null;
        public double StockPrice = 0.0;
        public string StockRating = null;
        public string RatingChanges = null;

        public string Date = null;

        public string Stockjobber = null;

        public string Reporter = null;
        public string PhoneNumber = null;
        public string Email = null;

        public string Reporter2 = null;
        public string PhoneNumber2 = null;
        public string Email2 = null;

        public string Content = null;

        public static List<string> dic = null;

        public StockData() { }

        public StockData(string reportPath) 
        {
            ReportPath = reportPath;
        }

        public void setStockName(string stockName = null)
        { 
            StockName = stockName; 
        }

        public void setStockCode(string stockCode = null)
        {
            StockCode = stockCode;
        }

        public void setStockRating(string stockRating = null)
        {
            StockRating = stockRating;
        }

        public void setRatingChanges(string ratingChanges = null)
        {
            RatingChanges = ratingChanges;
        }

        public void setDate(string date = null)
        {
            Date = date;
        }

        public void setStockjobber(string stockjobber = null)
        {
            Stockjobber = stockjobber;
        }

        public virtual bool extrcactContent( )
        {
            return true;
        }

        public bool loadCompanyDic(string dicPath)
        {
            try
            {
                if (dic == null) dic = new List<string>();
                string[] lines = File.ReadAllLines(dicPath);
                foreach (string line in lines)
                {
                    dic.Add(line);
                }

                return true;
            }
            catch (Exception ex) { return false; }
        }

        public string getStockjobber( )
        {
            //如果存在，直接返回
            if (Stockjobber!=null)
                return Stockjobber;
            
            string dicPath = ConfigurationManager.ConnectionStrings["DIC_PATH"].ConnectionString.ToString();
            loadCompanyDic(dicPath);

            //如果文件名称包含证券公司信息，直接返回，如果名字按照一定的格式命名，可以按照固定格式写，可以节省时间
            foreach (string jobber in dic)
                if (ReportPath.Contains(jobber))
                {
                    Stockjobber = jobber;
                    return jobber;
                }
            
            string stockjobber = null;
            PDDocument doc = PDDocument.load(ReportPath);
            PDFTextStripper pdfStripper = new PDFTextStripper();
            string text = pdfStripper.getText(doc).Replace("\r\n", "\n");

            string[] lines2 = text.Split(new char[] { '\n' });
            bool isFind = false;
            foreach (string line in lines2)
            {
                if (isFind) break;
                foreach (string jobber in dic)
                {
                    if (line.Contains(jobber))
                    {
                        stockjobber = jobber;
                        isFind = true;
                        break;
                    }
                }
            }
            Stockjobber = stockjobber;
            return stockjobber;
        }

        public void saveResult(string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath);

            if (StockName != null) sw.WriteLine("股票名称："+StockName);
            if (StockCode != null) sw.WriteLine("股票代码："+StockCode);
            if (StockPrice != 0.0) sw.WriteLine("股票价格："+StockPrice);
            if (StockRating != null) sw.WriteLine("评级"+StockRating);
            if (RatingChanges != null) sw.WriteLine(RatingChanges);
            if (Date != null) sw.WriteLine("研报发表日期："+Date);
            if (Stockjobber != null) sw.WriteLine("证券公司："+Stockjobber);
            if (Reporter != null) sw.WriteLine("研报作者："+Reporter);
            if (PhoneNumber != null) sw.WriteLine("联系电话："+PhoneNumber);
            if (Email != null) sw.WriteLine("邮箱："+Email);
            if (!String.IsNullOrEmpty(Content)) sw.WriteLine("文本内容：\n"+Content);
            sw.Close();
        }

        public string[] loadPDFLines()
        {
            try
            {
                PDDocument doc = PDDocument.load(ReportPath);
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(doc).Replace("\r\n", "\n");
                PDFText2HTML p = new PDFText2HTML();
                string c = p.getText(doc);
                string[] lines = text.Split(new char[] { '\n' });

                string newStr = "";
                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    newStr += line + "\n";
                }

                string[] lines2 = newStr.Split(new char[] { '\n' });
                return lines2;
            }
            catch (Exception e) { return null; }
        }

        public string GetExtractionResult()
        {
            StringBuilder sb = new StringBuilder();
            if (StockName != null) sb.AppendLine("股票名称：" + StockName);
            if (StockCode != null) sb.AppendLine("股票代码：" + StockCode);
            if (StockPrice != 0.0) sb.AppendLine("股票价格：" + StockPrice);
            if (StockRating != null) sb.AppendLine("评级" + StockRating);
            if (RatingChanges != null) sb.AppendLine(RatingChanges);
            if (Date != null) sb.AppendLine("研报发表日期：" + Date);
            if (Stockjobber != null) sb.AppendLine("证券公司：" + Stockjobber);
            if (Reporter != null) sb.AppendLine("研报作者：" + Reporter);
            if (PhoneNumber != null) sb.AppendLine("联系电话：" + PhoneNumber);
            if (Email != null) sb.AppendLine("邮箱：" + Email);
            if (!String.IsNullOrEmpty(Content)) sb.AppendLine("文本内容：\n" + Content);
            return sb.ToString();
        }

        public void show(string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath);

            if (StockName != null) sw.WriteLine("股票名称：" + StockName);
            if (StockCode != null) sw.WriteLine("股票代码：" + StockCode);
            if (StockPrice != 0.0) sw.WriteLine("股票价格：" + StockPrice);
            if (StockRating != null) sw.WriteLine("评级" + StockRating);
            if (RatingChanges != null) sw.WriteLine(RatingChanges);
            if (Date != null) sw.WriteLine("研报发表日期：" + Date);
            if (Stockjobber != null) sw.WriteLine("证券公司：" + Stockjobber);
            if (Reporter != null) sw.WriteLine("研报作者：" + Reporter);
            if (PhoneNumber != null) sw.WriteLine("联系电话：" + PhoneNumber);
            if (Email != null) sw.WriteLine("邮箱：" + Email);
            if (!String.IsNullOrEmpty(Content)) sw.WriteLine("文本内容：\n" + Content);
            sw.Close();
        }

        public string loadPDFText()
        {
            try
            {
                PDDocument doc = PDDocument.load(ReportPath);
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(doc).Replace("\r\n", "\n");
                PDFText2HTML p = new PDFText2HTML();
                string c = p.getText(doc);
                return c;
            }
            catch (Exception e) { return null; }
        }

        public string extractContent(string[] lines)
        {
            if (lines == null) return null;
            string content = "";
            for (int i = 0; i < lines.Length; i++)
            {
                if (String.IsNullOrEmpty(lines[i]))
                    continue;
                Regex reg = new Regex(".*[0-9]+[%]?[E]?[ ]?$");
                if (reg.IsMatch(lines[i]) && reg.IsMatch(lines[i - 1]) || reg.IsMatch(lines[i + 1]) && reg.IsMatch(lines[i]))
                    continue;

                content += lines[i] + "\n";
            }

            string[] lines2 = Content.Split(new char[] { '\n' });

            content = "";

            for (int i = 0; i < lines.Length; i++)
            {
                if (String.IsNullOrEmpty(lines[i]))
                    continue;
                if (i >= 3 && i < lines.Length - 3)
                {
                    if (lines[i].EndsWith(" ") && lines[i + 1].EndsWith(" ") && lines[i + 2].EndsWith(" ") && lines[i + 3].EndsWith(" ") || lines[i].EndsWith(" ") && lines[i - 1].EndsWith(" ") && lines[i - 2].EndsWith(" ") && lines[i - 3].EndsWith(" ")
                        || lines[i].EndsWith(" ") && lines[i + 1].EndsWith(" ") && lines[i + 2].EndsWith(" ") && lines[i - 1].EndsWith(" ") || lines[i].EndsWith(" ") && lines[i + 1].EndsWith(" ") && lines[i - 1].EndsWith(" ") && lines[i - 2].EndsWith(" "))
                        continue;
                }
                else if (i < 3)
                {
                    if (lines[i].EndsWith(" ") && lines[i + 1].EndsWith(" ") && lines[i + 2].EndsWith(" ") && lines[i + 3].EndsWith(" "))
                        continue;
                }
                else if (i >= lines.Length - 3)
                {
                    if (lines[i].EndsWith(" ") && lines[i + 1].EndsWith(" ") && lines[i + 2].EndsWith(" ") && lines[i + 3].EndsWith(" "))
                        continue;
                }
                if (lines[i].EndsWith(" "))
                    content += lines[i] + "\n";
                else
                    content += lines[i];
            }
            return content;
        }

        //抽取pdf中的字段信息
        public void extractDetail(string[] lines)
        {
            if (lines == null) return;
            bool flag = false;

            string regexStr = "[a-z0-9]+([._\\-]*[a-zA-Z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$"; //邮箱正则表达式  Sxg5661@163.com这个例子比较少，A-Z可以看情况取舍
            Regex regexEmail = new Regex(regexStr);

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

            int t = 0;
            int count1 = 0;
            int index1 = 0;//reporter的匹配位置
            int index2 = 0;//reporter2的匹配位置
            int count = 0;

            foreach (string line in lines)
            {
                string str = line.Trim();
                t++;
                if (t == 87)
                    t = 87;
                if (String.IsNullOrEmpty(line))
                    continue;

                if (!flag)
                {
                    if (regexEmail.IsMatch(line.Trim()))
                    {
                        //Regex reg1 = new Regex("[a-zA-Z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+");
                        var mat = regexEmail.Match(line.Trim());
                        Email = mat.ToString();
                        flag = true;
                        count1 = count;
                        count++;
                        continue;
                    }
                }
                if (flag)
                {
                    if (regexEmail.IsMatch(line.Trim()))
                    {
                        //Regex reg1 = new Regex("[a-zA-Z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+");
                        var mat = regexEmail.Match(line.Trim());
                        Email2 = mat.ToString();
                        break;
                    }
                }
                count++;
            }

            if (Email2 != null)
                for (int j = 0; j <= 5; j++)
                {
                    Parser parser = new Parser();
                    string name = parser.getPerson(lines[count - j].Replace(" ", ""));
                    if (name != null)
                    {
                        Reporter2 = name;
                        index2 = count - j;
                        break;
                    }
                }
            if (Email != null)
                for (int j = 0; j <= 5; j++)
                {
                    Parser parser = new Parser();
                    string name = parser.getPerson(lines[count1 - j].Replace(" ", ""));
                    if (name != null)
                    {
                        Reporter = name;
                        index1 = count1 - j;
                        break;
                    }
                }
            if (Email2 != null)
                for (int j = 0; j <= 5; j++)
                {
                    if (regPhone1.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone1.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone2.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone2.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone3.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            // Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone3.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone4.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone4.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone5.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            // Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone5.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone6.IsMatch(lines[count + j].Trim()))
                    {
                        if (count + j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone6.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }
                    if (regPhone7.IsMatch(lines[count + j].Trim())) //string regexPhone7 = "[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+";
                    {
                        if (count + j >= index2)
                        {
                            //Regex reg1 = new Regex("[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+");
                            var mat = regPhone7.Match(lines[count + j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone1.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone1.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone2.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone2.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone3.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone3.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone4.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone4.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone5.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone5.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone6.IsMatch(lines[count - j].Trim()))
                    {
                        if (count - j >= index2)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone6.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }
                    if (regPhone7.IsMatch(lines[count - j].Trim())) //string regexPhone7 = "[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+";
                    {
                        if (count - j >= index2)
                        {
                            // Regex reg1 = new Regex("[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+");
                            var mat = regPhone7.Match(lines[count - j].Trim());
                            PhoneNumber2 = mat.ToString();
                            break;
                        }
                    }

                }
            if (Email != null)
                for (int j = 0; j <= 5; j++)
                {
                    if (regPhone1.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone1.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone2.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone2.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone3.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone3.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone4.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone4.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone5.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone5.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone6.IsMatch(lines[count1 + j].Trim()))
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone6.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }
                    if (regPhone7.IsMatch(lines[count1 + j].Trim())) //string regexPhone7 = "[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+";
                    {
                        if (count1 + j >= index1)
                        {
                            //Regex reg1 = new Regex("[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+");
                            var mat = regPhone7.Match(lines[count1 + j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }


                    if (regPhone1.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone1.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone2.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[-][ ]*[0-9]+");
                            var mat = regPhone2.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone3.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone3.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone4.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone4.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone5.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[—][ ]*[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone5.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                    if (regPhone6.IsMatch(lines[count1 - j].Trim()))
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[0-9]+[ ]*[－][ ]*[0-9]+");
                            var mat = regPhone6.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }
                    if (regPhone7.IsMatch(lines[count1 - j].Trim())) //string regexPhone7 = "[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+";
                    {
                        if (count1 - j >= index1)
                        {
                            //Regex reg1 = new Regex("[(][ ]*[0-9]+[ ]*[)][ ]*[0-9]+");
                            var mat = regPhone7.Match(lines[count1 - j].Trim());
                            PhoneNumber = mat.ToString();
                            break;
                        }
                    }

                }


            //东阿阿胶(000423)/11.7 元
            string regexDate = "^[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日$"; //日期正则表达式
            Regex regDate = new Regex(regexDate);
            string regexDate2 = "^[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日"; //日期正则表达式
            Regex regDate2 = new Regex(regexDate2);
            string regexDate3 = "[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日"; //日期正则表达式
            Regex regDate3 = new Regex(regexDate3);

            string regexStock = "[\u4E00-\u9FA5]+[ ]*[(][0-9]+[)]";
            Regex regStock = new Regex(regexStock);
            string regexStock2 = "[\u4E00-\u9FA5]+[ ]*[（]+[ ]*[0-9]+[ ]*[）]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock2 = new Regex(regexStock2);
            string regexStock3 = "[\u4E00-\u9FA5]+[ ]*[（][ ]*[0-9]+[.][A-Z]{2}[ ]*[）]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock3 = new Regex(regexStock3);
            string regexStock4 = "[\u4E00-\u9FA5]+[ ]*[(][ ]*[0-9]+[.][A-Z]{2}[ ]*[)]"; ; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock4 = new Regex(regexStock4);
            string regexStock5 = "[\u4E00-\u9FA5]+[ ]*[(][ ]*[0-9]+[/][0-9]+[.][0-9]+[ ]*元[ ]*[)]"; ; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock5 = new Regex(regexStock5);

            Regex regStockName = new Regex("[\u4E00-\u9FA5]+");
            Regex regStockCode = new Regex("[0-9]+");

            bool DateFlag = false;
            bool StockFlag = false;
            foreach (string line in lines)//2007年11月21日 杜冬松 010-59734988 duds@pasc.com.cn 
            {
                if (String.IsNullOrEmpty(line))
                    continue;

                if (regDate.IsMatch(line.Trim()))
                {
                    Date = line.Trim();
                    DateFlag = true;
                }
                if (!DateFlag && regDate2.IsMatch(line.Trim()))
                {
                    Regex reg1 = new Regex("[0-9]+年[ ]*[0-9]+月[ ]*[0-9]+日");
                    var mat = reg1.Match(line.Trim());
                    Date = mat.ToString();
                    DateFlag = true;
                }
                if (!DateFlag && regDate3.IsMatch(line.Trim()))
                {
                    Regex reg1 = new Regex("[0-9]+年[ ]*[0-9]+月[ ]*[0-9]+日");
                    var mat = reg1.Match(line.Trim());
                    Date = mat.ToString();
                    DateFlag = true;
                }
                if (!StockFlag && regStock.IsMatch(line.Trim()))
                {
                    string stockstr = line.Trim();
                    Regex reg1 = new Regex("^[\u4E00-\u9FA5]+");
                    var mat = reg1.Match(stockstr);
                    StockName = mat.ToString();

                    //Regex reg2 = new Regex("[0-9]+");
                    var mat2 = regStockCode.Match(stockstr);
                    StockCode = mat2.ToString();

                }
                if (!StockFlag && regStock2.IsMatch(line.Trim()))
                {
                    string stockstr = line.Trim();
                    //Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                    var mat = regStockName.Match(stockstr);
                    StockName = mat.ToString();

                    //Regex reg2 = new Regex("[0-9]+");
                    var mat2 = regStockCode.Match(stockstr);
                    StockCode = mat2.ToString();
                    StockFlag = true;
                }
                if (!StockFlag && regStock3.IsMatch(line.Trim()))
                {
                    string stockstr = line.Trim();
                    //Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                    var mat = regStockName.Match(stockstr);
                    StockName = mat.ToString();

                    //Regex reg2 = new Regex("[0-9]+");
                    var mat2 = regStockCode.Match(stockstr);
                    StockCode = mat2.ToString();
                    StockFlag = true;
                }
                if (!StockFlag && regStock4.IsMatch(line.Trim()))
                {
                    string stockstr = line.Trim();
                    //Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                    var mat = regStockName.Match(stockstr);
                    StockName = mat.ToString();

                    //Regex reg2 = new Regex("[0-9]+");
                    var mat2 = regStockCode.Match(stockstr);
                    StockCode = mat2.ToString();
                    StockFlag = true;
                }
                if (!StockFlag && regStock5.IsMatch(line.Trim()))
                {
                    string stockstr = line.Trim();
                    //Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                    var mat = regStockName.Match(stockstr);
                    StockName = mat.ToString();

                    Regex reg2 = new Regex("[0-9]{6}");
                    var mat2 = reg2.Match(stockstr);
                    StockCode = mat2.ToString();

                    Regex reg3 = new Regex("[0-9]+[.][0-9]+");
                    var mat3 = reg3.Match(stockstr);
                    StockPrice = Double.Parse(mat3.ToString());
                    StockFlag = true;
                }
            }
        }
    }
}
