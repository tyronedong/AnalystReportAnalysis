using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report
{
    [Serializable]
    public class AnalystReport
    {
        public string _id { get; set; }
        public string ReportTitle { get; set; }//added
        public string ReportType { set; get; }//added

        //public string Stockjobber { get; set; }
        public string Brokerage { get; set; }
        public DateTime Date { get; set; }
        public string PDFileName { get; set; }
        
        //public string ReportTitle { get; set; }//added
        
        public string StockName { get; set; }
        public string StockCode { get; set; } 

        public string StockRating { get; set; }
        public string RatingChanges { get; set; }
        public double StockPrice { get; set; }

        public int picCount { get; set; }//added
        public int tableCount { get; set; }//added
        public int valCountInContent { get; set; }//added
        public int valCountOutContent { get; set; }//added

        public List<Analyst> Analysts { get; set; }

        public string Content { get; set; }

        //public static List<string> dic = null;

        public AnalystReport()
        {
            //StockCurPrice = 0.0;
            //StockTarPrice = 0.0;
            StockPrice = 0.0;
            picCount = 0;
            tableCount = 0;
            valCountInContent = 0;
            valCountOutContent = 0;
            Analysts = new List<Analyst>();
            //dic = new List<string>();
        }
    }

    [Serializable]
    public class Analyst
    {
        public Analyst() { }

        public Analyst(string Name)
        {
            this.Name = Name;
        }

        public Analyst(string _id, string Name, string CertificateNumber, string PhoneNumber, string Email)
        {
            this._id = _id;
            this.Name = Name;
            this.CertificateNumber = CertificateNumber;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
        }

        public string _id { get; set; }
        public string Name { get; set; }
        public string CertificateNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class FLIInfo
    {
        public string guid { get; set; }
        public string stockcd { get; set; }
        public DateTime rptdate { get; set; }
        public string typecd { get; set; }
        public int graph { get; set; }
        public bool flt { get; set; }
        public bool flt_tone { get; set; }
        public int tots { get; set; }
        public int poss { get; set; }
        public int negs { get; set; }
        public int totfls { get; set; }
        public int posfls { get; set; }
        public int negfls { get; set; }
        public int totfls_ind { get; set; }
        public int totfls_firm { get; set; }
        public int totnfls { get; set; }
        public int posnfls { get; set; }
        public int negnfls { get; set; }
        public bool isvalid { get; set; }
    }

    public class INNOVInfo
    {
        public string guid { get; set; }
        public string stock_code { get; set; }
        public DateTime rpt_date { get; set; }
        public string rpt_type { get; set; }
        public string title { get; set; }
        public string title_type { get; set; }
        public int text_sent_count { get; set; }//正文句子数
        public int text_char_count { get; set; }//正文字符数
        public int table_value_count { get; set; }
        public int text_value_count { get; set; }
        public string firstauthor { get; set; }
        public string firstauthor_id { get; set; }
        
        public bool has_innov { get; set; }
        
        public int innov_sent_count { get; set; }
        public int innov_char_count { get; set; }
        public int innov1_sent_count { get; set; }
        public int innov1_char_count { get; set; }
        public int innov2_sent_count { get; set; }
        public int innov2_char_count { get; set; }
        public int innov3_sent_count { get; set; }
        public int innov3_char_count { get; set; }
        
        public int innov_stage1_sent_count { get; set; }
        public int innov_stage1_char_count { get; set; }
        public int innov_stage2_sent_count { get; set; }
        public int innov_stage2_char_count { get; set; }
        public int innov_stage3_sent_count { get; set; }
        public int innov_stage3_char_count { get; set; }
        public int innov_stage4_sent_count { get; set; }
        public int innov_stage4_char_count { get; set; }

        public bool has_noninnov { get; set; }

        public int noninnov1_sent_count { get; set; }
        public int noninnov1_char_count { get; set; }
        public int noninnov2_sent_count { get; set; }
        public int noninnov2_char_count { get; set; }
        public int noninnov3_sent_count { get; set; }
        public int noninnov3_char_count { get; set; }
        public int noninnov4_sent_count { get; set; }
        public int noninnov4_char_count { get; set; }
        public int noninnov5_sent_count { get; set; }
        public int noninnov5_char_count { get; set; }

        public string rpt_tone { get; set; }

        public int rpt_pos_sent_count { get; set; }
        public int rpt_pos_char_count { get; set; }
        public int rpt_neg_sent_count { get; set; }
        public int rpt_neg_char_count { get; set; }
        public int rpt_innov_pos_sent_count { get; set; }
        public int rpt_innov_pos_char_count { get; set; }
        public int rpt_innov_neg_sent_count { get; set; }
        public int rpt_innov_neg_char_count { get; set; }

        public bool isvalid { get; set; }
    }
    //class Linkman
    //{
    //    public string Name { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public string Email { get; set; }
    //}
}


//public string Date = null;
//public string Reporter { get; set; }
//public string PhoneNumber { get; set; }
//public string Email { get; set; }

//public string Reporter2 = null;
//public string PhoneNumber2 = null;
//public string Email2 = null;