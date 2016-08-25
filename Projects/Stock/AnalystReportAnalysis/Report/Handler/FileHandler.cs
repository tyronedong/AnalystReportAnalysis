using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using org.apache.pdfbox.pdmodel;

namespace Report.Handler
{
    public class FileHandler
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

        /// <summary>
        /// return null if error
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string[] LoadStringArray(string fileName)
        {
            try { return File.ReadAllLines(fileName); ;}
            catch (Exception e) { Trace.TraceError("Report.Handler.FileHandler.LoadStringArray(string fileName): " + e.ToString()); return null; }
        }

        /// <summary>
        /// 向一个文件中追加文本行
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool AppendStringArray(string fileName, string[] strs)
        {
            try
            {
                if (File.Exists(fileName))
                    File.AppendAllLines(fileName, strs);
                else
                    File.WriteAllLines(fileName, strs);
            }
            catch (Exception e) { Trace.TraceError("Report.Handler.FileHandler.AppendStringArray(string fileName, string[] strs): " + e.ToString()); return false; }
            return true;
        }

        /// <summary>
        /// 保存类实例数组（List）到指定的文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="instances"></param>
        /// <returns></returns>
        public static bool SaveClassInstances<T>(string filePath, List<T> instances)
        {
            try
            {
                FileStream fs;
                if (File.Exists(filePath))
                    fs = new FileStream(filePath, FileMode.Append);
                else
                    fs = new FileStream(filePath, FileMode.Create);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, instances);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError("Text.Handler.FileHandler.SaveClassInstances<T>(): " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 从指定的文件中加载类实例数组（List）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<T> LoadClassInstances<T>(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                List<T> instances = new List<T>();
                while (true)
                {
                    if (fs.Position == fs.Length)
                        break;
                    instances.AddRange(bf.Deserialize(fs) as List<T>);
                    //bf.Deserialize(fs) as List<T>;
                    Console.WriteLine("a");
                }
                fs.Close();
                return instances;
            }
            catch (Exception e)
            {
                Trace.TraceError("Text.Handler.FileHandler.LoadClassInstances<T>(): " + e.ToString());
                return null;
            }
        }
    }
}
