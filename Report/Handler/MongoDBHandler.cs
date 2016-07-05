using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Diagnostics;

namespace Report.Handler
{
    class MongoDBHandler
    {
        private string authority;
        //variables for insert into mongoDB
        string ins_mongoDBConnectionString;
        string ins_mongoDBName;
        string ins_mongoDBCollName;

        //IMongoClient ins_mgclient;
        //IMongoDatabase ins_mgdatabase;
        //IMongoCollection<AnalystReport> ins_mgcollection;
        MongoClient ins_mgclient;
        IMongoDatabase ins_mgdatabase;
        IMongoCollection<AnalystReport> ins_mgcollection;


        //variables for query from mongoDB
        string query_mongoDBHost;
        string query_mongoDBPort;
        string query_mongoDBName;
        string query_mongoDBCollName;

        MongoServerSettings query_serverSetting;
        MongoServer query_mgserver;
        MongoDatabase query_mgdatabase;
        MongoCollection query_mgcollection;

        /// <summary>
        /// </summary>
        /// <param name="authority">Three optional values for param 'authority': "InsertOnly", "QueryOnly" or "InsertQuery"</param>
        public MongoDBHandler(string authority)
        {
            this.authority = authority;
            if (authority.Equals("InsertOnly"))
            {
                ins_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                ins_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                ins_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];
            }
            else if (authority.Equals("QueryOnly"))
            {
                query_mongoDBHost = ConfigurationManager.AppSettings["query_mongodbhost"];
                query_mongoDBPort = ConfigurationManager.AppSettings["query_mongodbport"];
                query_mongoDBName = ConfigurationManager.AppSettings["query_mongodbname"];
                query_mongoDBCollName = ConfigurationManager.AppSettings["query_mongodbcollectionname"];
            }
            else
            {
                ins_mongoDBConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
                ins_mongoDBName = ConfigurationManager.AppSettings["insert_mongodbname"];
                ins_mongoDBCollName = ConfigurationManager.AppSettings["insert_mongodbcollectionname"];

                query_mongoDBHost = ConfigurationManager.AppSettings["query_mongodbhost"];
                query_mongoDBPort = ConfigurationManager.AppSettings["query_mongodbport"];
                query_mongoDBName = ConfigurationManager.AppSettings["query_mongodbname"];
                query_mongoDBCollName = ConfigurationManager.AppSettings["query_mongodbcollectionname"];
            }
        }

        public bool Init()
        {
            try
            {
                if (this.authority.Equals("InsertOnly"))
                {
                    ins_mgclient = new MongoClient(ins_mongoDBConnectionString);
                    ins_mgdatabase = ins_mgclient.GetDatabase(ins_mongoDBName);
                    ins_mgcollection = ins_mgdatabase.GetCollection<AnalystReport>(ins_mongoDBCollName);
                }
                else if (this.authority.Equals("QueryOnly"))
                {
                    query_serverSetting = new MongoServerSettings();
                    query_serverSetting.Server = new MongoServerAddress(query_mongoDBHost, Int32.Parse(query_mongoDBPort));
                    query_mgserver = new MongoServer(query_serverSetting);
                    query_mgdatabase = query_mgserver.GetDatabase(query_mongoDBName);
                    query_mgcollection = query_mgdatabase.GetCollection<AnalystReport>(query_mongoDBCollName);
                }
                else
                {
                    ins_mgclient = new MongoClient(ins_mongoDBConnectionString);
                    ins_mgdatabase = ins_mgclient.GetDatabase(ins_mongoDBName);
                    ins_mgcollection = ins_mgdatabase.GetCollection<AnalystReport>(ins_mongoDBCollName);

                    query_serverSetting = new MongoServerSettings();
                    query_serverSetting.Server = new MongoServerAddress(query_mongoDBHost, Int32.Parse(query_mongoDBPort));
                    query_mgserver = new MongoServer(query_serverSetting);
                    query_mgdatabase = query_mgserver.GetDatabase(query_mongoDBName);
                    query_mgcollection = query_mgdatabase.GetCollection<AnalystReport>(query_mongoDBCollName);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("MongoDBHandler.Init(): " + e.ToString());
                return false;
            }
            return true;
        }

        public bool InsertMany(List<AnalystReport> insertList)
        {
            try
            {
                var insertTask =  ins_mgcollection.InsertManyAsync(insertList);
                insertTask.Wait();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("MongoDBHandler.InsertMany(): " + e.ToString());
                return false;
            }
            return true;
        }

        public MongoCursor<AnalystReport> FormulateCursor(int quidRank)
        {
            IMongoQuery query = Query.Empty;
            MongoCursor<AnalystReport> cursor = query_mgcollection
                .FindAs<AnalystReport>(query)
                .SetSortOrder(SortBy.Ascending("_id"))
                .SetLimit(1)
                .SetSkip(quidRank);
           
            return cursor;
        }

        public List<AnalystReport> FindMany(ref MongoCursor<AnalystReport> cursor)
        {
            List<AnalystReport> tempList = new List<AnalystReport>();

            if (cursor == null)
            {
                Trace.TraceWarning("Report.Handler.MongoDBHandler.FindMany goes wrong");
                return null;
            }

            try
            {
                if (cursor.Size() == 0)
                {
                    Trace.TraceWarning("Report.Handler.MongoDBHandler.FindMany goes wrong");
                    return null;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Report.Handler.MongoDBHandler.FindMany: " + e.ToString());
                return null;
            }

            try
            {
                foreach (var report in cursor)
                { tempList.Add(report); }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Report.Handler.MongoDBHandler.FindMany: " + e.ToString());
                return null;
            }
            return tempList;
        }
    }
}
