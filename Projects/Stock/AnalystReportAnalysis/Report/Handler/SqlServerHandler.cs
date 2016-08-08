﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;

namespace Report.Handler
{
    public class SqlServerHandler
    {
        public bool isValid = true;

        private SqlConnection sqlCnn;

        private SqlDataAdapter sqlAdapter;
        private SqlDataAdapter sqlAdapter2;
        private SqlCommand sqlReportCmd;//for batch process
        private SqlCommand sqlReportCmd2;//for select one process
        private DataTable dataTable;
        private SqlParameter param_num_once_select_cmd = new SqlParameter("@num_once_select", SqlDbType.Int);
        private SqlParameter param_id_min_cmd = new SqlParameter("@id_min", SqlDbType.Char);
        private SqlParameter param_id_cmd2 = new SqlParameter("@GUID", SqlDbType.Char);

        private Dictionary<string, Analyst> personTable;

        private static string sqlConnectionString = ConfigurationManager.AppSettings["SqlConnectionString"];
        //private static string storedProcName_Person = ConfigurationManager.AppSettings["StoredProcName_Person"];
        //private static string storedProcName_Report = ConfigurationManager.AppSettings["StoredProcName_Report"];
        //private static string storedProcName_Report2 = ConfigurationManager.AppSettings["StoredProcName_Report2"];
        //private static string numOnceSelect = ConfigurationManager.AppSettings["num_once_select"];
        private static string storedProcName_Person = "[dbo].[selectAllPerson]";
        private static string storedProcName_Report = "[dbo].[selectTopN]";
        private static string storedProcName_Report2 = "[dbo].[selectByGUID]";
        private static string numOnceSelect = "100";

        public SqlServerHandler()
        {
            //sqlCnn = new SqlConnection(sqlConnectionString);
            personTable = new Dictionary<string, Analyst>();
        }

        public bool Init()
        {
            try
            {
                //set connection
                sqlCnn = new SqlConnection(sqlConnectionString);

                //set cmd
                sqlReportCmd = new SqlCommand(storedProcName_Report, sqlCnn);
                sqlReportCmd.CommandType = CommandType.StoredProcedure;
                sqlReportCmd.Parameters.Add(param_num_once_select_cmd);
                sqlReportCmd.Parameters.Add(param_id_min_cmd);
                sqlReportCmd.CommandTimeout = 60;
                sqlReportCmd2 = new SqlCommand(storedProcName_Report2, sqlCnn);
                sqlReportCmd2.CommandType = CommandType.StoredProcedure;
                sqlReportCmd2.Parameters.Add(param_id_cmd2);
                sqlReportCmd2.CommandTimeout = 60;
                sqlAdapter = new SqlDataAdapter(sqlReportCmd);
                sqlAdapter2 = new SqlDataAdapter(sqlReportCmd2);
                dataTable = new DataTable();       

                sqlCnn.Open();
                LoadPersonTable();
                isValid = true;
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlServerHandler.Init(): " + e.Message);
                isValid = false;
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
                personTable.Add(s0, new Analyst(s1, s2, string.IsNullOrEmpty(s3) ? s3 : s4, s5));
            }
            return true ;
        }

        public bool ModifyCMDById(string curId)
        {
            try
            {
                param_num_once_select_cmd.Value = Int32.Parse(numOnceSelect);
                param_id_min_cmd.Value = curId;
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

        public bool ModifyCMD2ById(string curId)
        {
            try
            {
                param_id_cmd2.Value = curId;
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlServerHandler.ModifyCMD2ById(string curId): " + e.Message);
                return false;
            }
            return true;
        }

        public DataTable GetRecordById(string curId)
        {
            dataTable.Clear();

            if (ModifyCMD2ById(curId))
            {
                try
                {
                    sqlAdapter2.Fill(dataTable);
                }
                catch (Exception e)
                {
                    Trace.TraceError("SqlServerHandler.GetRecordById(string curId): " + e.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }

            return dataTable;
        }

        public List<Analyst> GetAnalysts(string pid1, string pid2, string pid3)
        {
            List<Analyst> analysts = new List<Analyst>();
            if (!string.IsNullOrEmpty(pid1))
            {
                if (personTable.ContainsKey(pid1))
                {
                    analysts.Add(personTable[pid1]);
                }
            }
            if (!string.IsNullOrEmpty(pid2))
            {
                if (personTable.ContainsKey(pid2))
                {
                    analysts.Add(personTable[pid2]);
                }
            }
            if (!string.IsNullOrEmpty(pid3))
            {
                if (personTable.ContainsKey(pid3))
                {
                    analysts.Add(personTable[pid3]);
                }
            }
            return analysts;
        }
    }

    //class PersonalInfo
    //{
    //    public PersonalInfo(string Name, string CertificateNumber, string PhoneNumber, string Email)
    //    {
    //        this.Name = Name;
    //        this.CertificateNumber = CertificateNumber;
    //        this.PhoneNumber = PhoneNumber;
    //        this.Email = Email;
    //    }
    //    public string Name { get; set; }
    //    public string CertificateNumber { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public string Email { get; set; }
    //}
}