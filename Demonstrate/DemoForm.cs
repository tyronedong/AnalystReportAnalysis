using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Report;
using Text.Classify;
using Report.Handler;
using Report.Securities;
using Report.Outsider;
using System.Diagnostics;

namespace Demonstrate
{
    public partial class DemoForm : Form
    {
        private string mode = ConfigurationManager.AppSettings["mode"];

        private SqlServerHandler sqlSH;
        private WordSegHandler wsH;
        private Model model;
        private List<string> selectedText;

        public DemoForm()
        {
            InitializeComponent();
            sqlSH = new SqlServerHandler();
            sqlSH.Init();
            wsH = new WordSegHandler();
            model = new Model();
            model.LoadModel(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\model.txt");
            selectedText = new List<string>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Failed to open file. You may fail to open document content.");
            }

            //string savePath = ConfigurationManager.ConnectionStrings["SAVE_DIR"].ConnectionString.ToString();

            string filePath = textBox1.Text;

            this.richTextBox1.Text = "Loading...";

            selectedText.Clear();

            if (mode.Equals("with_database"))
            {
                this.richTextBox1.Text = Extract_withdb(filePath);
            }
            else
            {
                this.richTextBox1.Text = Extract_nodb(filePath);
            }

            //set positive cases to red color
            foreach (var text in selectedText)
            {
                int start = this.richTextBox1.Text.IndexOf(text);

                this.richTextBox1.Select(start, text.Length + 1);
                this.richTextBox1.SelectionColor = Color.Red;
            }
        }

        private string Extract_nodb(string filePath)
        {
            ReportParser reportParser = null;
            //StockData stockData = null, stockParser = null;

            string securitiesName = ReportParser.getStockjobber(filePath);

            //if (string.IsNullOrEmpty(securitiesName))
            //{
            //    reportParser = new CommonSecurities(filePath);
            //}
            if (securitiesName == null) { return "File of security name list not found. Check the configuration item \"SecNameDic_Path\" and make sure it is right."; }
            else if (securitiesName.Equals(""))
            {
                reportParser = new CommonSecurities(filePath);
            }
            else if (securitiesName.Equals("长江证券"))
            {
                //flag = true;
                reportParser = new ChangJiangSecurities(filePath);
            }
            else if (securitiesName.Equals("申万宏源"))
            {
                //flag = true;
                reportParser = new ShenHongSecurities(filePath);
            }

            else if (securitiesName.Equals("海通证券"))
            {
                //flag = true;
                reportParser = new HaiTongSecurities(filePath);
            }
            else if (securitiesName.Equals("国泰君安"))
            {
                reportParser = new GuoJunSecurities(filePath);
            }
            else if (securitiesName.Equals("中信证券"))
            {
                //flag = true;
                reportParser = new ZhongXinSecurities(filePath);
            }
            else if (securitiesName.Equals("中金公司"))
            {
                reportParser = new ZhongJinSecurities(filePath);
            }
            else if (securitiesName.Equals("兴业证券"))
            {
                //flag = true;
                reportParser = new XingYeSecurities(filePath);
            }
            else if (securitiesName.Equals("招商证券"))
            {
                reportParser = new ZhaoShangSecurities(filePath);
            }
            else if (securitiesName.Equals("安信证券"))
            {
                //flag = true;
                reportParser = new AnXinSecurities(filePath);
            }
            else if (securitiesName.Equals("广发证券"))
            {
                //flag = true;
                reportParser = new GuangFaSecurities(filePath);
            }
            else if (securitiesName.Equals("天相投顾"))
            {
                //flag = true;
                reportParser = new TianTouSecurities(filePath);
            }
            else if (securitiesName.Equals("国金证券"))
            {
                //flag = true;
                reportParser = new GuoJinSecurities(filePath);
            }
            else if (securitiesName.Equals("华泰证券"))
            {
                //flag = true;
                reportParser = new HuaTaiSecurities(filePath);
            }
            else if (securitiesName.Equals("中银国际"))
            {
                //flag = true;
                reportParser = new ZhongGuoSecurities(filePath);
            }
            else if (securitiesName.Equals("东方证券"))
            {
                //flag = true;
                reportParser = new DongFangSecurities(filePath);
            }
            else if (securitiesName.Equals("国信证券"))
            {
                //flag = true;
                reportParser = new GuoXinSecurities(filePath);
            }
            else if (securitiesName.Equals("中信建投"))
            {
                //flag = true;
                reportParser = new ZhongJianSecurities(filePath);
            }
            else if (securitiesName.Equals("平安证券"))
            {
                //flag = true;
                reportParser = new PingAnSecurities(filePath);
            }
            else if (securitiesName.Equals("民生证券"))
            {
                //flag = true;
                reportParser = new MinShengSecurities(filePath);
            }
            else if (securitiesName.Equals("光大证券"))
            {
                //flag = true;
                reportParser = new GuangDaSecurities(filePath);
            }
            else if (securitiesName.Equals("东北证券"))
            {
                //flag = true;
                reportParser = new DongBeiSecurities(filePath);
            }
            else if (securitiesName.Equals("东兴证券"))
            {
                //flag = true;
                reportParser = new DongXingSecurities(filePath);
            }
            else if (securitiesName.Equals("方正证券"))
            {
                //flag = true;
                reportParser = new FangZhengSecurities(filePath);
            }
            else if (securitiesName.Equals("申银万国"))
            {
                //flag = true;
                reportParser = new ShenWanSecurities(filePath);
            }
            else
            {
                //if (securitiesName.Equals("民生证券")) { flag = true; }
                reportParser = new CommonSecurities(filePath);
            }

            AnalystReport curAnReport = new AnalystReport();
            //handle the data
            if (reportParser != null)
            {
                if (reportParser.isValid)
                {
                    curAnReport = reportParser.executeExtract_nodb(ref wsH);
                    curAnReport.Stockjobber = securitiesName;
                    //Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                    reportParser.CloseAll();
                }
                else { return "Extract failed!"; }
            }
            //else if (stockParser != null)
            //{
            //    stockParser.extrcactContent();
            //    //stockParser.extractDetail(stockParser.loadPDFLines());
            //    Report.Program.DataTransform(ref stockParser, ref curAnReport);
            //    curAnReport.Stockjobber = securitiesName;
            //    //Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
            //}
            else { return "Extract failed!"; }

            return GetExtractionResult(curAnReport, ref this.model);
        }

        private string Extract_withdb(string filePath)
        {
            if (!sqlSH.isValid) { return "Sql server init failed!"; }
            //sqlSH.GetRecordById()
            string[] paths = filePath.Split('\\');
            string fileId = paths[paths.Length - 1].Replace(".pdf", "").Replace(".PDF", "");

            DataTable record = sqlSH.GetRecordById(fileId);
            if (record == null) { return "Extract failed!"; }
            if (record.Rows.Count == 1)
            {
                DataRow curRow = record.Rows[0];

                var time = (DateTime)curRow[0];
                var id = curRow[1].ToString();
                var securitiesName = curRow[2].ToString();
                var reportName = curRow[3].ToString();
                var language = curRow[4].ToString();
                var person1 = curRow[5].ToString();
                var person2 = curRow[6].ToString();
                var person3 = curRow[7].ToString();

                //judge if current document is handlable
                if (language.Equals("EN"))
                {
                    Trace.TraceWarning("Skip English analyst report whose id is: " + id);
                    return "Extract failed!";
                }

                //get pdf file parser by securities
                ReportParser reportParser = null;
                //StockData stockData = null, stockParser = null;
                if (securitiesName.Equals("长江证券"))
                {
                    //flag = true;
                    reportParser = new ChangJiangSecurities(filePath);
                }
                else if (securitiesName.Equals("申万宏源"))
                {
                    //flag = true;
                    reportParser = new ShenHongSecurities(filePath);
                }

                else if (securitiesName.Equals("海通证券"))
                {
                    //flag = true;
                    reportParser = new HaiTongSecurities(filePath);
                }
                else if (securitiesName.Equals("国泰君安"))
                {
                    reportParser = new GuoJunSecurities(filePath);
                }
                else if (securitiesName.Equals("中信证券"))
                {
                    //flag = true;
                    reportParser = new ZhongXinSecurities(filePath);
                }
                else if (securitiesName.Equals("中金公司"))
                {
                    reportParser = new ZhongJinSecurities(filePath);
                }
                else if (securitiesName.Equals("兴业证券"))
                {
                    //flag = true;
                    reportParser = new XingYeSecurities(filePath);
                }
                else if (securitiesName.Equals("招商证券"))
                {
                    reportParser = new ZhaoShangSecurities(filePath);
                }
                else if (securitiesName.Equals("安信证券"))
                {
                    //flag = true;
                    reportParser = new AnXinSecurities(filePath);
                }
                else if (securitiesName.Equals("广发证券"))
                {
                    //flag = true;
                    reportParser = new GuangFaSecurities(filePath);
                }
                else if (securitiesName.Equals("天相投顾"))
                {
                    //flag = true;
                    reportParser = new TianTouSecurities(filePath);
                }
                else if (securitiesName.Equals("国金证券"))
                {
                    //flag = true;
                    reportParser = new GuoJinSecurities(filePath);
                }
                else if (securitiesName.Equals("华泰证券"))
                {
                    //flag = true;
                    reportParser = new HuaTaiSecurities(filePath);
                }
                else if (securitiesName.Equals("中银国际"))
                {
                    //flag = true;
                    reportParser = new ZhongGuoSecurities(filePath);
                }
                else if (securitiesName.Equals("东方证券"))
                {
                    //flag = true;
                    reportParser = new DongFangSecurities(filePath);
                }
                else if (securitiesName.Equals("国信证券"))
                {
                    //flag = true;
                    reportParser = new GuoXinSecurities(filePath);
                }
                else if (securitiesName.Equals("中信建投"))
                {
                    //flag = true;
                    reportParser = new ZhongJianSecurities(filePath);
                }
                else if (securitiesName.Equals("平安证券"))
                {
                    //flag = true;
                    reportParser = new PingAnSecurities(filePath);
                }
                else if (securitiesName.Equals("民生证券"))
                {
                    //flag = true;
                    reportParser = new MinShengSecurities(filePath);
                }
                else if (securitiesName.Equals("光大证券"))
                {
                    //flag = true;
                    reportParser = new GuangDaSecurities(filePath);
                }
                else if (securitiesName.Equals("东北证券"))
                {
                    //flag = true;
                    reportParser = new DongBeiSecurities(filePath);
                }
                else if (securitiesName.Equals("东兴证券"))
                {
                    //flag = true;
                    reportParser = new DongXingSecurities(filePath);
                }
                else if (securitiesName.Equals("方正证券"))
                {
                    //flag = true;
                    reportParser = new FangZhengSecurities(filePath);
                }
                else if (securitiesName.Equals("申银万国"))
                {
                    //flag = true;
                    reportParser = new ShenWanSecurities(filePath);
                }
                else
                {
                    //if (securitiesName.Equals("民生证券")) { flag = true; }
                    reportParser = new CommonSecurities(filePath);
                }

                AnalystReport curAnReport = new AnalystReport();
                //handle the data
                if (reportParser != null)
                {
                    if (reportParser.isValid)
                    {
                        curAnReport = reportParser.executeExtract_withdb();
                        Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                        reportParser.CloseAll();
                    }
                    else { return "Extract failed!"; }
                }
                //else if (stockParser != null)
                //{
                //    stockParser.extrcactContent();
                //    //stockParser.extractDetail(stockParser.loadPDFLines());
                //    Report.Program.DataTransform(ref stockParser, ref curAnReport);
                //    Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                //}
                else { return "Extract failed!"; }

                return GetExtractionResult(curAnReport, ref this.model);
            }
            else { return "PDF not found in database!"; }
        }

        private string GetExtractionResult(AnalystReport curAnReport)
        {
            StringBuilder sb = new StringBuilder();
            if (curAnReport.StockName != null) sb.AppendLine("股票名称：" + curAnReport.StockName);
            if (curAnReport.StockCode != null) sb.AppendLine("股票代码：" + curAnReport.StockCode);
            if (curAnReport.StockPrice != 0.0) sb.AppendLine("股票价格：" + curAnReport.StockPrice);
            if (curAnReport.StockRating != null)
            {
                sb.Append("评级: " + curAnReport.StockRating);
                if (curAnReport.RatingChanges != null)
                    sb.Append(" " + curAnReport.RatingChanges);
                sb.Append("\n");
            }
            if (curAnReport.Date != null) sb.AppendLine("研报发表日期：" + curAnReport.Date);
            if (curAnReport.Stockjobber != null) sb.AppendLine("证券公司：" + curAnReport.Stockjobber);
            if (curAnReport.Analysts.Count != 0)
            {
                Analyst author = curAnReport.Analysts[0];
                if (!String.IsNullOrEmpty(author.Name)) { sb.AppendLine("研报作者：" + author.Name); }
                if (!String.IsNullOrEmpty(author.PhoneNumber)) { sb.AppendLine("联系电话：" + author.PhoneNumber); }
                if (!String.IsNullOrEmpty(author.Email)) { sb.AppendLine("邮箱：" + author.Email); }
                if (!String.IsNullOrEmpty(author.CertificateNumber)) { sb.AppendLine("执业证书：" + author.CertificateNumber); }
            }
            if (!String.IsNullOrEmpty(curAnReport.Content)) sb.AppendLine("文本内容：\n" + curAnReport.Content);
            return sb.ToString();
        }

        private string GetExtractionResult(AnalystReport curAnReport, ref Model model)
        {
            StringBuilder sb = new StringBuilder();
            if (curAnReport.StockName != null) sb.AppendLine("股票名称：" + curAnReport.StockName);
            if (curAnReport.StockCode != null) sb.AppendLine("股票代码：" + curAnReport.StockCode);
            if (curAnReport.StockPrice != 0.0) sb.AppendLine("股票价格：" + curAnReport.StockPrice);
            if (curAnReport.StockRating != null)
            {
                sb.Append("评级: " + curAnReport.StockRating);
                if (curAnReport.RatingChanges != null)
                    sb.Append(" " + curAnReport.RatingChanges);
                sb.Append("\n");
            }
            if (curAnReport.Date != null) sb.AppendLine("研报发表日期：" + curAnReport.Date);
            if (curAnReport.Stockjobber != null) sb.AppendLine("证券公司：" + curAnReport.Stockjobber);
            if (curAnReport.Analysts.Count != 0)
            {
                Analyst author = curAnReport.Analysts[0];
                if (!String.IsNullOrEmpty(author.Name)) { sb.AppendLine("研报作者：" + author.Name); }
                if (!String.IsNullOrEmpty(author.PhoneNumber)) { sb.AppendLine("联系电话：" + author.PhoneNumber); }
                if (!String.IsNullOrEmpty(author.Email)) { sb.AppendLine("邮箱：" + author.Email); }
                if (!String.IsNullOrEmpty(author.CertificateNumber)) { sb.AppendLine("执业证书：" + author.CertificateNumber); }
            }
            if (!String.IsNullOrEmpty(curAnReport.Content))
            {
                sb.AppendLine("文本内容：\n" + curAnReport.Content);
                string[] positiveCases = model.GetPositiveCases(curAnReport.Content);
                sb.AppendLine("前瞻性语句：");
                if (positiveCases.Length == 0) { sb.AppendLine("未找到正例"); }
                else
                {
                    int i = 0;
                    foreach (var posCase in positiveCases)
                    {
                        i++;
                        sb.AppendLine(i + "：" + posCase);
                        selectedText.Add(posCase);
                    }
                }
            }
            return sb.ToString();
        }

    }
}
