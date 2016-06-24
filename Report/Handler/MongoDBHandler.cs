using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MongoDB.Driver;
using System.Diagnostics;

namespace Report.Handler
{
    class MongoDBHandler
    {
        //variables for insert into mongoDB
        string ins_mongoDBConnectionString;
        string ins_mongoDBName;
        string ins_mongoDBCollName;
        
        IMongoClient ins_mgclient;
        IMongoDatabase ins_mgdatabase;
        IMongoCollection<AnalystReport> ins_mgcollection;

        //variables for query from mongoDB
        string query_mongoDBConnectionString;
        string query_mongoDBName;
        string query_mongoDBCollName;

        IMongoClient query_mgclient;
        IMongoDatabase query_mgdatabase;
        IMongoCollection<AnalystReport> query_mgcollection;

        /// <summary>
        /// </summary>
        /// <param name="authority">Three optional values for param 'authority': "InsertOnly", "QueryOnly" or "InsertQuery"</param>
        public MongoDBHandler(string authority)
        {
            if (authority.Equals("InsertOnly"))
            {
                ins_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                ins_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                ins_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];
            }
            else if (authority.Equals("QueryOnly"))
            {
                query_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                query_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                query_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];
            }
            else
            {
                ins_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                ins_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                ins_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];

                query_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                query_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                query_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];
            }
        }

        public bool Init()
        {
            try
            {
                ins_mgclient = new MongoClient(ins_mongoDBConnectionString);
                ins_mgdatabase = ins_mgclient.GetDatabase(ins_mongoDBName);
                ins_mgcollection = ins_mgdatabase.GetCollection<AnalystReport>(ins_mongoDBCollName);
            }
            catch (Exception e)
            {
                Trace.TraceError("MongoDBHandler.Init(): " + e.Message);
                return false;
            }
            return true;
        }

        public bool InsertMany(List<AnalystReport> insertList)
        {
            try
            {
                var insertTask = ins_mgcollection.InsertManyAsync(insertList);
                insertTask.Wait();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("MongoDBHandler.InsertMany(): " + e.Message);
                return false;
            }
            return true;
        }

    }
}
