using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Report
{
    class SqlServerHandler
    {

        private SqlConnection sqlCnn;
        private SqlDataReader sqlReader;
        private SqlDataAdapter sqlAdapter;
        //private Hashtable 
        private Dictionary<string, PersonalInfo> personTable;

        private static string sqlConnectionString = ConfigurationManager.AppSettings["SqlConnectionString"];
        private static string storedProcName = ConfigurationManager.AppSettings["StoredProcName"];

        public SqlServerHandler()
        {
            //sqlCnn = new SqlConnection(sqlConnectionString);
            personTable = new Dictionary<string, PersonalInfo>();
        }

        //public DataTable 

        public bool Init()
        {
            try
            {
                sqlCnn = new SqlConnection(sqlConnectionString);
                sqlCnn.Open();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public bool LoadPersonTable()
        {
            //select all persons in person_d_fact table and store it into personTable
            SqlCommand sqlCmd = new SqlCommand(storedProcName, sqlCnn);
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
            DataTable dataTable = new DataTable();
            sqlAdapter.Fill(dataTable);

            foreach (DataRow curRow in dataTable.Rows)
            {
                var s0 = curRow[0].ToString(); //GUID
                var s1 = curRow[1].ToString(); //NM
                var s2 = curRow[2].ToString(); //CER_ID
                var s3 = curRow[3].ToString(); //TELEPHONE
                var s4 = curRow[4].ToString(); //MOBILE
                var s5 = curRow[5].ToString(); //EMAIL
                if (string.IsNullOrEmpty(s3))
                    personTable.Add(s0, new PersonalInfo(s1, s2, string.IsNullOrEmpty(s3) ? s3 : s4, s5));
            }
            return true ;
        }

        public DataTable GetTableByCMD(SqlCommand sqlCmd)
        {
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
            DataTable dataTable = new DataTable();
            sqlAdapter.Fill(dataTable);


            return dataTable;
        }
    }

    class PersonalInfo
    {
        public PersonalInfo(string Name, string CertificateNumber, string PhoneNumber, string Email)
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
}
