using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;

namespace Report
{
    class SqlServerHandler
    {

        private SqlConnection sqlCnn;

        private SqlDataAdapter sqlAdapter;
        private SqlCommand sqlReportCmd;
        private DataTable dataTable;
        private SqlParameter param_num_once_select = new SqlParameter("@num_once_select", SqlDbType.Int);
        private SqlParameter param_id_min = new SqlParameter("@id_min", SqlDbType.Char);

        private Dictionary<string, PersonalInfo> personTable;

        private static string sqlConnectionString = ConfigurationManager.AppSettings["SqlConnectionString"];
        private static string storedProcName_Person = ConfigurationManager.AppSettings["StoredProcName_Person"];
        private static string storedProcName_Report = ConfigurationManager.AppSettings["StoredProcName_Report"];
        private static string numOnceSelect = ConfigurationManager.AppSettings["num_once_select"];

        //private static 

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

                sqlReportCmd = new SqlCommand(storedProcName_Report, sqlCnn);
                sqlReportCmd.CommandType = CommandType.StoredProcedure;
                sqlReportCmd.Parameters.Add(param_num_once_select);
                sqlReportCmd.Parameters.Add(param_id_min);
                sqlReportCmd.CommandTimeout = 60;
                sqlAdapter = new SqlDataAdapter(sqlReportCmd);
                dataTable = new DataTable();

                sqlCnn.Open();
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlServerHandler.Init(): " + e.Message);
                return false;
            }
            return true;
        }

        public bool LoadPersonTable()
        {
            //select all persons in person_d_fact table and store it into personTable
            SqlCommand sqlCmd = new SqlCommand(storedProcName_Person, sqlCnn);
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

        public bool ModifyCMDById(string curId)
        {
            try
            {
                param_num_once_select.Value = Int32.Parse(numOnceSelect);
                param_id_min.Value = curId;
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlServerHandler.ModifyCMDById(string curId): " + e.Message);
                return false;
            }
            return true;
        }

        public DataTable GetTableById(string curId)
        {
            dataTable.Clear();

            if (ModifyCMDById(curId))
            {
                try
                {
                    sqlAdapter.Fill(dataTable);
                }
                catch (Exception e)
                {
                    Trace.TraceError("SqlServerHandler.GetTableById(string curId): " + e.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
            
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
