using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Text.Outsider
{
    public class AnalystReport
    {
        public string _id { get; set; }
        public string ReportTitle { get; set; }

        public string Stockjobber { get; set; }
        public DateTime Date { get; set; }
        public string PDFileName { get; set; }

        public string StockName { get; set; }
        public string StockCode { get; set; } 

        public string StockRating { get; set; }
        public string RatingChanges { get; set; }
        public double StockPrice { get; set; }

        public List<Analyst> Analysts { get; set; }

        public string Content { get; set; }

        //public static List<string> dic = null;

        public AnalystReport()
        {
            //StockCurPrice = 0.0;
            //StockTarPrice = 0.0;
            StockPrice = 0.0;
            Analysts = new List<Analyst>();
            //dic = new List<string>();
        }
    }

    public class Analyst
    {
        public Analyst(string Name, string CertificateNumber, string PhoneNumber, string Email)
        {
            this.Name = Name;
            this.CertificateNumber = CertificateNumber;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
        }

        public string Name { get; set; }
        public string CertificateNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
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