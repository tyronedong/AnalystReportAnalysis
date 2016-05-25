using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.pdfbox.pdmodel;

namespace Report
{
    class ZhongXinSecurities : ReportParser
    {
        //中信证券（2013年及之前的pfd解析乱码）
        public ZhongXinSecurities(PDDocument pdreport)
            : base(pdreport)
        {
            pdfText = loadPDFText();
            lines = pdfText.Split('\n');
            mergedParas = mergeToParagraph(lines);
        }

        public override bool extractStockBasicInfo()
        {

            return base.extractStockBasicInfo();
        }
    }
}
