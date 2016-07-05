using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using org.apache.pdfbox.pdmodel;

namespace Report.Handler
{
    class FileHandler
    {
        public static string GetFilePathByName(string rootPath, string fileName)
        {
            try
            {
                string searchPattern = fileName + "*";
                string[] foundPaths = Directory.GetFiles(rootPath, searchPattern, SearchOption.AllDirectories);
                if (foundPaths.Length == 0)
                {
                    return null;
                }
                else
                {
                    return foundPaths[0];
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("FileHandler.GetFilePathByName(string rootPath, string fileName): " + e.ToString());
                return null;
            }
        }
    }
}
