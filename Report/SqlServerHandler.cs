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
            //sqlReader = sqlCmd.ExecuteReader();
            foreach (DataRow curRow in dataTable.Rows)
            {
                personTable.Add((string)curRow[0], new PersonalInfo((string)curRow[1], (string)curRow[2], (string)curRow[4]));
            }
            return true ;
        }
    }

    class PersonalInfo
    {
        public PersonalInfo(string CertificateNumber, string PhoneNumber, string Email)
        {
            this.CertificateNumber = CertificateNumber;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
        }
        public string CertificateNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
