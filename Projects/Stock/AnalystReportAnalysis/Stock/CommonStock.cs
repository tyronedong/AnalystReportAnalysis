using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using org.pdfbox.pdmodel;
using org.pdfbox.util;

namespace Stock
{
    //通用的抽取
    public class CommonStock:StockData
    {
        public CommonStock() { }
        public CommonStock(StockData stockData)
        {
            ReportPath = stockData.ReportPath;

            StockName = stockData.StockName;
            StockCode = stockData.StockCode;
            StockPrice = stockData.StockPrice;
            StockRating = stockData.StockRating;
            RatingChanges = stockData.RatingChanges;

            Date = stockData.Date;

            Stockjobber = stockData.Stockjobber;

            Reporter = stockData.Reporter;
            PhoneNumber = stockData.PhoneNumber;
            Email = stockData.Email;

            Reporter2 = stockData.Reporter2;
            PhoneNumber2 = stockData.PhoneNumber2;
            Email2 = stockData.Email2;

            Content = stockData.Content;
        }
        //抽取pdf中的内容
        public string getContent(string[] lines)
        {

            List<List<string>> Paragraphs = new List<List<string>>();
            List<string> Paragraph = new List<string>();
            int index = 0;
            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line))
                {
                    if (Paragraph.Count != 0)
                    {
                        List<string> Paragraph2 = new List<string>(Paragraph);
                        Paragraphs.Add(Paragraph2);

                        Paragraph.Clear();
                    }
                }
                else
                    Paragraph.Add(line);
            }

            List<List<string>> Paragraphs2 = new List<List<string>>();
            foreach (List<string> Para in Paragraphs)
            {
                int num = 0;
                bool flag = true;
                for (int t = 0; t < Para.Count; t++)
                {
                    if (Para[t].EndsWith("表") && Para[t].Length < 21)
                        flag = false;
                    if (Para[t].StartsWith("表") && Para[t].Length < 21)
                        flag = false;
                    if (Para[t].EndsWith("图") && Para[t].Length < 21)
                        flag = false;
                    if (Para[t].StartsWith("图") && Para[t].Length < 21)
                        flag = false;

                    Regex reg2 = new Regex("表[ ]?[0-9][:]");
                    Regex reg3 = new Regex("表[ ]?[0-9][：]");
                    Regex reg4 = new Regex("表[ ]?[0-9][、]");
                    if(reg2.IsMatch(Para[t]))
                        flag = false;
                    if (reg3.IsMatch(Para[t]))
                        flag = false;
                    if (reg4.IsMatch(Para[t]))
                        flag = false;
                    Regex reg5 = new Regex("图[ ]?[0-9][:]");
                    Regex reg6 = new Regex("图[ ]?[0-9][：]");
                    Regex reg7 = new Regex("图[ ]?[0-9][、]");
                    if (reg5.IsMatch(Para[t]))
                        flag = false;
                    if (reg6.IsMatch(Para[t]))
                        flag = false;
                    if (reg7.IsMatch(Para[t]))
                        flag = false;

                    Regex reg8 = new Regex(".*[0-9]+[ ]?[%]?[E]?[ ]?$");
                    Regex reg9 = new Regex(".*[a-z]+[ ]?$");
                    Regex reg10 = new Regex(".*[A-Z]+[ ]?$");
                    if (reg8.IsMatch(Para[t]) || reg9.IsMatch(Para[t]) || reg10.IsMatch(Para[t]) || Para[t].Length < 21)
                        num++;

                    if (!flag)
                        break;
                }
                double rate = (num*1.0) / Para.Count;
                if(rate>=0.5)
                    flag = false;
                if (flag == true && Para.Count>1)
                    Paragraphs2.Add(Para);
            }

            string result = "";
            foreach (List<string> Para in Paragraphs2)
            {
                foreach (string str in Para)
                {
                    if (str.EndsWith(" "))
                        result += str+"\n";
                    else
                        result += str;
                }
            }
            return result;

            //for (int i = 0; i < lines2.Length; i++)
            //{
            //    if (String.IsNullOrEmpty(lines2[i]))
            //        continue;
            //    Regex reg = new Regex(".*[0-9]+[%]?[E]?[ ]?$");
            //    if (reg.IsMatch(lines2[i]) && reg.IsMatch(lines2[i - 1]) || reg.IsMatch(lines2[i + 1]) && reg.IsMatch(lines2[i]))
            //        continue;

            //    Content += lines2[i] + "\n";
            //}

            //string[] lines3 = Content.Split(new char[] { '\n' });
            //Content = "";
            //for (int i = 0; i < lines3.Length; i++)
            //{
            //    if (String.IsNullOrEmpty(lines3[i]))
            //        continue;
            //    if (i >= 3 && i < lines.Length - 3)
            //    {
            //        if (lines3[i].EndsWith(" ") && lines3[i + 1].EndsWith(" ") && lines3[i + 2].EndsWith(" ") && lines3[i + 3].EndsWith(" ") || lines3[i].EndsWith(" ") && lines3[i - 1].EndsWith(" ") && lines3[i - 2].EndsWith(" ") && lines3[i - 3].EndsWith(" ")
            //            || lines3[i].EndsWith(" ") && lines3[i + 1].EndsWith(" ") && lines3[i + 2].EndsWith(" ") && lines3[i - 1].EndsWith(" ") || lines3[i].EndsWith(" ") && lines3[i + 1].EndsWith(" ") && lines3[i - 1].EndsWith(" ") && lines3[i - 2].EndsWith(" "))
            //            continue;
            //    }
            //    else if (i < 3)
            //    {
            //        if (lines3[i].EndsWith(" ") && lines3[i + 1].EndsWith(" ") && lines3[i + 2].EndsWith(" ") && lines3[i + 3].EndsWith(" "))
            //            continue;
            //    }
            //    else if (i >= lines.Length - 3)
            //    {
            //        if (lines3[i].EndsWith(" ") && lines3[i + 1].EndsWith(" ") && lines3[i + 2].EndsWith(" ") && lines3[i + 3].EndsWith(" "))
            //            continue;
            //    }
            //    if (lines3[i].EndsWith(" "))
            //        Content += lines3[i] + "\n";
            //    else
            //        Content += lines3[i];
            //}
        }
        //抽取pdf中的字段信息
        public override bool extrcactContent()
        {
            try
            {
                PDDocument doc = PDDocument.load(ReportPath);
                PDFTextStripper pdfStripper = new PDFTextStripper();
                string text = pdfStripper.getText(doc).Replace("\r\n", "\n");
                string[] lines = text.Split(new char[] { '\n' });

                Content = getContent(lines);

                int count = 0;

                string newStr = "";
                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    newStr += line + "\n";
                }

                string[] lines2 = newStr.Split(new char[] { '\n' });

                bool flag = false;

                string regexStr = "[a-z0-9]+([._\\-]*[a-zA-Z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+"; //邮箱正则表达式  Sxg5661@163.com这个例子比较少，A-Z可以看情况取舍
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
                string regexStock4 = "[\u4E00-\u9FA5]+[ ]*[(][ ]*[0-9]+[.][A-Z]{2}[ ]*[)]" ; //—— 金陵药业（ 000919） 半年 度 财报 点评
                Regex regStock4 = new Regex(regexStock4);
                string regexStock5 = "[\u4E00-\u9FA5]+[ ]*[(][ ]*[0-9]+[/][0-9]+[.][0-9]+[ ]*元[ ]*[)]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
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
                        //Regex reg1 = new Regex("^[\u4E00-\u9FA5]+");
                        var mat = regStockName.Match(stockstr);
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
                return true;
            }
            catch (Exception ex) {
                return false; 
            }
        }
    }
}
