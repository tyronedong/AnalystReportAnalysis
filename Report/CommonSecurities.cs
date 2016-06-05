using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Report
{
    class CommonSecurities : ReportParser
    {
        public CommonSecurities(string pdReportPath)
            : base(pdReportPath)
        {
            if (this.isValid)
            {
                try
                {
                    pdfText = loadPDFText();
                    lines = pdfText.Split('\n');
                    noABCLines = removeAnyButContentInLines(lines);
                    mergedParas = mergeToParagraph(noABCLines);
                    finalParas = mergedParas;
                }
                catch (Exception e)
                {
                    Trace.TraceError("GuoJunSecurities.GuoJunSecurities(string pdReportPath): " + e.Message);
                }
            }
        }
    }
}
