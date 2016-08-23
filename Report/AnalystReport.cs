using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report
{
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

    public class Analyst
    {
        public Analyst() { }

        public Analyst(string Name)
        {
            this.Name = Name;
        }

        public Analyst(string Name, string CertificateNumber, string PhoneNumber, string Email)
        {
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