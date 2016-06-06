using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace Stock
{
    //东兴证券的抽取方法
    public class DongXingStock : StockData
    {
         List<string> subjects;
         List<string> blackString;
         public DongXingStock() { }
         public DongXingStock(StockData stockData)
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
            subjects = new List<string>();
            subjects.Add("摘要");
            subjects.Add("报告摘要");
          //  subjects.Add("目录");
            subjects.Add("事件");
            subjects.Add("评论");
            subjects.Add("观点");
            subjects.Add("结论");
            subjects.Add("联系人简介");
            subjects.Add("分析师简介");
            subjects.Add("分析师承诺");
            subjects.Add("免责声明");
            subjects.Add("行业评级体系");
            subjects.Add("积极因素");
            subjects.Add("消极因素");
            subjects.Add("盈利预测与投资建议");
            subjects.Add("风险提示");
            blackString = new List<string>();
            blackString.Add("资料来源:");
        }

         public void addSubjectsFromMulu(string[] lines)
         {
             int i=0;
             for (i=0; i<lines.Length; i++)
             {
                 if(lines[i].Replace(" ","").Equals("目录"))
                 {
                     i++;
                     break;
                 }
             }
             string regexNum = "[0-9]";
             Regex regNum = new Regex(regexNum);
             for (; i < lines.Length; i++)
             {
                 if (regNum.IsMatch(lines[i]))
                 {
                     Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                     var mat = reg1.Match(lines[i]);
                     subjects.Add(mat.ToString());

                 }
                 else
                     break;
             }
 
         }

         public bool isSubject(string line)
         {
             for (int j = 0; j < subjects.Count; j++)
             {
                 string temp = line.Replace(" ", "");

                 if (temp.EndsWith(subjects[j]) || temp.StartsWith(subjects[j]))
                 {
                     Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
                     var mat = reg1.Match(temp);
                     if (mat.ToString().Contains(subjects[j]))
                     {
                         return true;
                     }
                 }
             }
             return false;
         }
         //抽取pdf中的内容
         public string extractContent(string[] lines)
         {
             string str = "";
             string newstr ="";
             for (int i = 0; i < lines.Length; i++)
             {
                 string temp = lines[i].Replace(" ", "");

                 if (temp.Contains("东兴证券深度报告"))
                     continue;
                 if (temp.Equals("科伦药业（002422）：三发驱动，驶向新的未来"))
                     continue;
                 if (temp.Equals("敬请参阅报告结尾处的免责声明东方财智兴盛之源"))
                     continue;
                 if (temp.Contains("DONGXINGSECURITIES"))
                     continue;
                 if (temp.Equals("东兴证券财报点评"))
                 {
                     i++; continue;
                 }
                 newstr = newstr + lines[i];
                 if (lines[i].EndsWith(" ")) 
                 {
                    newstr = newstr +"\n";
                 }
             }

             string[] lines2 = newstr.Split(new char[] { '\n' });

             addSubjectsFromMulu(lines);
             for (int i = 0; i < lines2.Length; i++)
             {
                 if (isSubject(lines2[i]))
                 {
                     str += lines2[i] + "\n";
                     if (lines2[i].Replace(" ", "").Contains("联系人简介") || lines2[i].Replace(" ", "").Contains("分析师简介") || lines2[i].Replace(" ", "").Contains("行业评级体系"))
                     {
                         i++;
                          for (; i < lines2.Length; i++)
                         {
                                bool flag = isSubject(lines2[i]);
                                if (flag)
                                {
                                    i--;
                                    break;
                                }
                                else
                                {
                                        str += lines2[i] + "\n";
                                }
                         }
                     }
                     i++;
                     
                     for (; i < lines2.Length; i++)
                     {
                         string temp= lines2[i].Replace(" ", "");
                         if (temp.EndsWith("。") || temp.EndsWith("：") || temp.EndsWith("，"))
                         {
                             str += lines2[i] + "\n";
                         }
                         else
                         {
                             bool flag = isSubject(lines2[i]);
                             if (flag)
                             {
                                 i--;
                                 break;
                             }
                             else
                             {
                                  continue;
                             }
                         }
                     }
                 }
             }
             return str;

         }
         //抽取pdf中的字段信息
         public override bool extrcactContent()
        {
            
            PDDocument doc = PDDocument.load(ReportPath);
            PDFTextStripper pdfStripper = new PDFTextStripper();
            string text = pdfStripper.getText(doc).Replace("\r\n", "\n");

            string[] lines = text.Split(new char[]{'\n'});

            ///********************/
            //extractDetail(lines);
            ///********************/

            int count = 0;

            string newStr = "";
            foreach (string line in lines)
            {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                newStr += line+"\n";
            }

            string[] lines2 = newStr.Split(new char[] { '\n' });

            Content = extractContent(lines2);

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

            string regexDate = "^[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日$"; //日期正则表达式
            Regex regDate = new Regex(regexDate);
            string regexDate2 = "^[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日"; //日期正则表达式
            Regex regDate2 = new Regex(regexDate2);
            string regexDate3 = "[0-9]+[ ]*年[ ]*[0-9]+[ ]*月[ ]*[0-9]+[ ]*日"; //日期正则表达式
            Regex regDate3 = new Regex(regexDate3);

            //string regexStock3 = "[\u4E00-\u9FA5]+[ ]*[（]+[ ]*[0-9]+[ ]*[）]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
            //Regex regStock3 = new Regex(regexStock3);
            string regexStock = "[\u4E00-\u9FA5]+[ ]*[(][0-9]+[)]";
            Regex regStock = new Regex(regexStock);
            string regexStock2 = "[\u4E00-\u9FA5]+[ ]*[（]+[ ]*[0-9]+[ ]*[）]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock2 = new Regex(regexStock2);
            string regexStock3 = "[\u4E00-\u9FA5]+[ ]*[（][ ]*[0-9]+[.][A-Z]{2}[ ]*[）]"; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock3 = new Regex(regexStock3);
            string regexStock4 = "[\u4E00-\u9FA5]+[ ]*[(][ ]*[0-9]+[.][A-Z]{2}[ ]*[)]"; ; //—— 金陵药业（ 000919） 半年 度 财报 点评
            Regex regStock4 = new Regex(regexStock4);

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
                    //Regex reg1 = new Regex("[\u4E00-\u9FA5]+");
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
            }
            return true;
        }
    }
}
