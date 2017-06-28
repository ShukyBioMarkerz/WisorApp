using System;
using System.Reflection;

namespace Wisor
{
    class Program
    {
        static void Main(string[] args)
        {
            String Path = new System.IO.FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            ResultReportData data = new ResultReportData();

            //Path += "\\report.html";
            Path += "\\report.pdf";

            Reporter.LenderReport(Path, data, true, false);
            //Reporter.LenderReport(Path, data, false, true);
          //  Reporter.BorrowerReport(Path, data, true, false);
        }
    }
}
