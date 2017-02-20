using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    public class RunLoanDetails
    {
        public string ID { get; set; }
        public int Status { get; set; }
        public long ElapsedMilliseconds { get; set; }

        string Outfilename;

        public RunLoanDetails(string id, int status, long elapseTime, string outfilename)
        {
            ID = id;
            Status = status;
            ElapsedMilliseconds = elapseTime;
            Outfilename = outfilename;
        }

        public override string ToString()
        {
            return "The load details: id: " + ID + ", Status: " + Status + ", epalse time: " + ElapsedMilliseconds +
                ", outfilename: " + Outfilename;
        }
    }
}
