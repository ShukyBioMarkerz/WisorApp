using System;
using WisorLibrary.DataObjects;

namespace Wisor
{
    class Reporter
    {
        public static int LenderReport(ResultReportData reportData, String HTMLfilename = null, String PDFfilename = null, CultureInfo cultureInfo = null)
        {
            LenderReport lr = new LenderReport();

            if (HTMLfilename != null)
            {
                lr.GenerateLenderHtmlReport(HTMLfilename, reportData);
            }

            if(PDFfilename != null)
            {
                lr.GenerateLenderPdfReport(PDFfilename, reportData);
            }

            return 0;
        }



        public static int BorrowerReport(ResultReportData reportData, String HTMLfilename = null, String PDFfilename = null, CultureInfo cultureInfo = null)
        {
            BorrowerReport br = new BorrowerReport();

            if (HTMLfilename != null)
            {
                br.GenerateBorrowerHtmlReport(HTMLfilename, reportData);
            }

            if (PDFfilename != null)
            {
                br.GenerateBorrowerPdfReport(PDFfilename, reportData);
            }

            return 0;
        }
    }
}
