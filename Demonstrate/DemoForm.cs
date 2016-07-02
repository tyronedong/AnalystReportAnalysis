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
using Report.Stocks;
using Text.Classify;
using Report.Handler;
using Report.Securities;
using System.Diagnostics;

namespace Demonstrate
{
    public partial class DemoForm : Form
    {
        private SqlServerHandler sqlSH;
        private Model model;

        public DemoForm()
        {
            InitializeComponent();
            sqlSH = new SqlServerHandler();
            sqlSH.Init();
            model = new Model();
            model.LoadModel(@"D:\workingwc\Stock\AnalystReportAnalysis\Text\result\model.txt");
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

            this.richTextBox1.Text = Extract(filePath);
            //StockData stocdData = new StockData(filePath);
            //stocdData.getStockjobber();
            //CommonStock zhaoshang = new CommonStock(stocdData);
            //zhaoshang.extrcactContent();
            ////zhaoshang.show(savePath+"1.txt");
            //string contentExtracted = zhaoshang.GetExtractionResult();
            ////MessageBox.Show("Done");
            //this.richTextBox1.Text = contentExtracted;
        }

        private string Extract(string filePath)
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
                StockData stockData = null, stockParser = null;
                if (securitiesName.Equals("国泰君安"))
                {
                    reportParser = new GuoJunSecurities(filePath);
                }
                if (securitiesName.Equals("中信证券"))
                {
                    //flag = true;
                    reportParser = new ZhongXinSecurities(filePath);
                }
                else if (securitiesName.Equals("中信建投"))
                {
                    //flag = true;
                    reportParser = new ZhongJianSecurities(filePath);
                }
                else if (securitiesName.Equals("国信证券"))
                {
                    //flag = true;
                    reportParser = new GuoXinSecurities(filePath);
                }
                else if (securitiesName.Equals("国金证券"))
                {
                    //flag = true;
                    reportParser = new GuoJinSecurities(filePath);
                }
                else if (securitiesName.Equals("中金公司"))
                {
                    reportParser = new ZhongJinSecurities(filePath);
                }
                else if (securitiesName.Equals("招商证券"))
                {
                    reportParser = new ZhaoShangSecurities(filePath);
                }
                else if (securitiesName.Equals("东北证券"))
                {
                    stockData = new StockData(filePath);
                    //stockData.setStockjobber("东北证券");
                    stockParser = new DongBeiStock(stockData);
                    //stockParser.extrcactContent();
                }
                else if (securitiesName.Equals("东兴证券"))
                {
                    stockData = new StockData(filePath);
                    stockParser = new DongXingStock(stockData);
                }
                else if (securitiesName.Equals("方正证券"))
                {
                    stockData = new StockData(filePath);
                    stockParser = new FangZhengStock(stockData);
                }
                else if (securitiesName.Equals("平安证券"))
                {
                    stockData = new StockData(filePath);
                    stockParser = new PingAnStock(stockData);
                }
                else if (securitiesName.Equals("兴业证券"))
                {
                    stockData = new StockData(filePath);
                    stockParser = new XingYeStock(stockData);
                }
                else if (securitiesName.Equals("长江证券"))
                {
                    stockData = new StockData(filePath);
                    stockParser = new ChangJiangStock(stockData);
                }
                else
                {
                    //flag = true;
                    reportParser = new CommonSecurities(filePath);
                }

                AnalystReport curAnReport = new AnalystReport();
                //handle the data
                if (reportParser != null)
                {
                    if (reportParser.isValid)
                    {
                        curAnReport = reportParser.executeExtract();
                        Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                        reportParser.CloseAll();
                    }
                    else { return "Extract failed!"; }
                }
                else if (stockParser != null)
                {
                    stockParser.extrcactContent();
                    //stockParser.extractDetail(stockParser.loadPDFLines());
                    Report.Program.DataTransform(ref stockParser, ref curAnReport);
                    Report.Program.SetExistedInfo(ref curAnReport, ref sqlSH, id, reportName, securitiesName, time, person1, person2, person3);
                }
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
                sb.AppendLine("正例：");
                if (positiveCases.Length == 0) { sb.AppendLine("未找到正例"); }
                else
                {
                    int i = 0;
                    foreach (var posCase in positiveCases)
                    {
                        i++;
                        sb.AppendLine(i + "：" + posCase);
                    }
                }
            }
            return sb.ToString();
        }

    }
}
