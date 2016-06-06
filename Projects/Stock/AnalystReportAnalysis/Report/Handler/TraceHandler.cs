using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using System.IO;

namespace Report.Handler
{
    class TraceHandler : TraceListener
    {
        private string fileLoc = ConfigurationManager.AppSettings["log_file_location"];

        public override void Write(string message)
        {

            try
            {
                File.AppendAllText(fileLoc, message);
            }
            catch (Exception e)
            { }

        }



        public override void WriteLine(string message)
        {
            try
            {
                File.AppendAllText(fileLoc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss    ") + message + Environment.NewLine);
            }
            catch (Exception e)
            { }
        }

    }
}
