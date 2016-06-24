using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;

namespace Text.Handler
{
    class ExcelHandler
    {
        Excel.Application exlApp;
        Excel.Workbook exlWorkBook;
        

        public ExcelHandler(string exlPath)
        {
            exlApp = new Excel.Application();
            exlWorkBook = exlApp.Workbooks.Open(exlPath, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sheetName">Sheet name which is usually displayed on the left bottom corner of excel</param>
        /// <param name="colNum"></param>
        /// <returns></returns>
        public string[] GetColoum(string sheetName, int colNum)
        {
            List<string> colStrs = new List<string>();

            Excel.Worksheet exlWorkSheet;
            Excel.Range exlRange;

            exlWorkSheet = (Excel.Worksheet)exlWorkBook.Sheets[sheetName];
            exlRange = exlWorkSheet.UsedRange;

            for (int rowCnt = 1; rowCnt <= exlRange.Rows.Count; rowCnt++)
            {
                var val2 = (exlRange.Cells[rowCnt, colNum] as Excel.Range).Value2;
                string str = val2 == null ? null : val2.ToString();
                //if (str == null) { continue; }
                colStrs.Add(str);
            }

            releaseObject(exlWorkSheet);

            return colStrs.ToArray();
        }

        public void Destroy()
        {
            exlWorkBook.Close(true, null, null);
            //exlWorkBook.Close();
            exlApp.Quit();

            releaseObject(exlWorkBook);
            releaseObject(exlApp);
        }

        public bool test()
        {
            string path = @"F:\事们\进行中\分析师报告\数据标注\FLI信息提取-样本.xlsx";   
            Excel.Worksheet exlWorkSheet;
            Excel.Range exlRange;

            string str;
            int rowCnt = 0;
            int colCnt = 0;

            exlApp = new Excel.Application();
            exlWorkBook = exlApp.Workbooks.Open(path, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            exlWorkSheet = (Excel.Worksheet)exlWorkBook.Sheets["Sheet1"];

            //exlRange = exlWorkSheet.get_Range("A1");
            //Console.WriteLine(exlRange.Value2);

            //exlRange = exlWorkSheet.UsedRange;
            //for (rCnt = 1; rCnt <= exlRange.Rows.Count; rCnt++)
            //{
            //    for (cCnt = 1; cCnt <= exlRange.Columns.Count; cCnt++)
            //    {
            //        var val2 = (exlRange.Cells[rCnt, cCnt] as Excel.Range).Value2;
            //        str = val2 == null ? null : val2.ToString();
            //        Console.WriteLine(str);
            //    }
            //}

            exlRange = exlWorkSheet.UsedRange;
            colCnt = 2;
            for (rowCnt = 1; rowCnt <= exlRange.Rows.Count; rowCnt++)
            {
                var val2 = (exlRange.Cells[rowCnt, colCnt] as Excel.Range).Value2;
                str = val2 == null ? null : val2.ToString();
                Console.WriteLine(str);
            }

            exlWorkBook.Close(true, null, null);
            exlApp.Quit();

            releaseObject(exlWorkSheet);
            releaseObject(exlWorkBook);
            releaseObject(exlApp);

            return false;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        } 
    }
}
