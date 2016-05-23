using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stock;
using System.Configuration;

namespace StockForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
            StockData stocdData = new StockData(filePath);
            stocdData.getStockjobber();
            CommonStock zhaoshang = new CommonStock(stocdData);
            zhaoshang.extrcactContent();
            //zhaoshang.show(savePath+"1.txt");
            string contentExtracted = zhaoshang.GetExtractionResult();
            //MessageBox.Show("Done");
            this.richTextBox1.Text = contentExtracted;
        }

    }
}
