using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Text.Handler
{
    class SqlServerHandler
    {
        public bool isValid = true;

        private SqlConnection sqlCnn;

        private SqlDataAdapter sqlAdapter;
        private SqlCommand sqlReportCmd;//for batch process
        private DataTable dataTable;
        private SqlParameter param_guid_rank = new SqlParameter("@rank", SqlDbType.Int);

        private static string sqlConnectionString = ConfigurationManager.AppSettings["SqlConnectionString"];
        private static string storedProcName_GUIDRank = ConfigurationManager.AppSettings["StoredProcName_GUIDRank"];

        public SqlServerHandler()
        {
            //sqlCnn = new SqlConnection(sqlConnectionString);
        }

        public bool Init()
        {
            try
            {
                //set connection
                sqlCnn = new SqlConnection(sqlConnectionString);

                //set cmd
                sqlReportCmd = new SqlCommand(storedProcName_GUIDRank, sqlCnn);
                sqlReportCmd.CommandType = CommandType.StoredProcedure;
                sqlReportCmd.Parameters.Add(param_guid_rank);
                sqlReportCmd.CommandTimeout = 60;
                dataTable = new DataTable();

                sqlCnn.Open();
                isValid = true;
            }
            catch (Exception e)
            {
                Trace.TraceError("Text.Handler.SqlServerHandler.Init(): " + e.Message);
                isValid = false;
                return false;
            }
            return true;
        }


        private bool ModifyCMDByRank(int rank)
        {
            try
            {
                param_guid_rank.Value = rank;
            }
            catch (Exception e)
            {
                Trace.TraceError("Text.Handler.SqlServerHandler.ModifyCMD2ById(int rank): " + e.Message);
                return false;
            }
            return true;
        }

        public DataTable GetRecordByRank(int rank)
        {
            dataTable.Clear();

            if (ModifyCMDByRank(rank))
            {
                try
                {
                    sqlAdapter.Fill(dataTable);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Text.Handler.SqlServerHandler.GetRecordById(int rank): " + e.Message);
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
}
