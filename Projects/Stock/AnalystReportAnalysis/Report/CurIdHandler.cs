using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace Report
{
    class CurIdHandler
    {
        //need a setting like <add key="CurIdRootPath" value="D:\\IdDict\\{0}.txt"/>
        // and a initial string
        private string rootPathPattern = ConfigurationManager.AppSettings["CurIdRootPath"];
        private string firstId = ConfigurationManager.AppSettings["FirstId"];
        private string curidPath;

        public CurIdHandler(string FileName)
        {
            curidPath = string.Format(rootPathPattern, FileName);
            SetCurIdToFile(firstId);
        }

        public string GetCurIdFromFile()
        {
            StreamReader curReader; string id;
            if (File.Exists(curidPath))
            {
                try
                {
                    curReader = File.OpenText(curidPath);
                    id = curReader.ReadToEnd();
                    curReader.Dispose();
                    curReader.Close();

                    return id;
                }
                catch (Exception e)
                {
                    Trace.TraceError("something is wrong when get current id and need attention " + e.Message);
                    Console.WriteLine("something is wrong when get current id and need attention " + e.Message);
                    return "null";
                }
            }
            else
            {
                //SetCurIdToFile(firstId);
                Trace.TraceError("file doesn't exist when get current id ");
                Console.WriteLine("file doesn't exist when get current id ");
                return null;
            }
        }//getcuridfromfile

        public bool SetCurIdToFile(string id)
        {
            StreamWriter curWriter;
            try
            {
                curWriter = File.CreateText(curidPath);
                curWriter.Write(id);
                curWriter.Flush();
                curWriter.Dispose();
                curWriter.Close();

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError("something is wrong when set current id and need attention " + e.Message);
                Console.WriteLine("something is wrong when set current id and need attention " + e.Message);
                return false;
            }
        }//setcuridtofile
    }
}
