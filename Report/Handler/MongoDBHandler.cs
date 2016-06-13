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
        string ins_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
        string ins_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
        string ins_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];
        
        IMongoClient ins_mgclient;
        IMongoDatabase ins_mgdatabase;
        IMongoCollection<AnalystReport> ins_mgcollection;

        public MongoDBHandler() { }

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
