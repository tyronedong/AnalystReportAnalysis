using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using org.apache.pdfbox.pdmodel;

namespace Report
{
    class FileHandler
    {
        public static string GetFilePathByName(string rootPath, string fileName)
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
    }
}
