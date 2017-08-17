using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.DataObjects;

namespace WisorLib
{
    public class RunLoanDetails
    {
        public ResultReportData ReportData { get; set; }

        public RunLoanDetails(ResultReportData reportData)
        {
            ReportData = reportData;
        }

        //public string ID { get; set; }
        //public int Status { get; set; }
        //public long ElapsedMilliseconds { get; set; }

        //public string Outfilename { get; }

        //public RunLoanDetails(string id, int status, long elapseTime, string outfilename)
        //{
        //    ID = id;
        //    Status = status;
        //    ElapsedMilliseconds = elapseTime;
        //    Outfilename = outfilename;
        //}

        //public override string ToString()
        //{
        //    return "The load details: id: " + ID + ", Status: " + Status + ", epalse time: " + ElapsedMilliseconds +
        //        ", outfilename: " + Outfilename;
        //}

    }
}
