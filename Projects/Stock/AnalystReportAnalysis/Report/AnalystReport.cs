using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report
{
    class AnalystReport
    {
        public string StockCode { get; set; }
        public string StockName { get; set; }

        public double StockPrice { get; set; }
        public string StockRating { get; set; }
        public string RatingChanges { get; set; }

        public List<Analyst> Analysts { get; set; }
        public string Stockjobber { get; set; }
        public DateTime Date { get; set; }

        public string Content { get; set; }

        public static List<string> dic = null;

        public AnalystReport()
        {
            StockPrice = 0.0;
            Analysts = new List<Analyst>();
            dic = new List<string>();
        }
    }

    class Analyst
    {
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