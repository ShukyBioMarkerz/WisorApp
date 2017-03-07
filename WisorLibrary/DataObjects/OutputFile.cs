using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    public class OutputFile
    {
        // General Parameters
        private StreamWriter summaryFile;

        public string OutputFilename { get; }

        public OutputFile(string orderid, double loanAmtWanted, double monthlyPmtWanted, CheckInfo CheckInfo)
        {
            // get the exact output filename
            OutputFilename = RunEnvironment.CreateOutputFilename(orderid, loanAmtWanted, monthlyPmtWanted);

            // TBD: Shuky - ensure the directory realy exists
            if (!Directory.Exists(Path.GetDirectoryName(OutputFilename)))
                Directory.CreateDirectory(Path.GetDirectoryName(OutputFilename));

            try
            {
                summaryFile = new StreamWriter(OutputFilename);
             }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: OutputFile got Exception: " + e.ToString());
            }

            WriteToOutputFile("Fast Three Option Check V3.2");
            WriteToOutputFile("Order ID : " + CheckInfo.orderID);
            WriteToOutputFile("Execution ID : " + CheckInfo.fastCheckID);
            WriteToOutputFile("Borrower profile : " + CalculationConstants.profiles[BorrowerProfile.borrowerProfile]);
            WriteToOutputFile("Start time : " + CheckInfo.softwareOpenTime);

            // Shuky - add the header line 
            /*
             * "" + opts[(int)Options.options.OPTX]
                    + "," + opts[(int)Options.options.OPTY]
                        + "," + opts[(int)Options.options.OPTZ]
                        + "," + (int)opts[(int)Options.options.OPTX].optPmt
                        + "," + (int)opts[(int)Options.options.OPTY].optPmt
                        + "," + (int)opts[(int)Options.options.OPTZ].optPmt
                        + "," + (int)ttlPmt
                        + "," + (int)opts[(int)Options.options.OPTX].optTtlPay
                        + "," + (int)opts[(int)Options.options.OPTY].optTtlPay
                        + "," + (int)opts[(int)Options.options.OPTZ].optTtlPay                        
                        + "," + (int)ttlPay;
             */
            WriteToOutputFile("OPTX" + "," + "OPTY" + "," + "OPTZ" + "," + "OPTX-optPmt" + "," + "OPTY-optPmt" + "," + "OPTZ-optPmt"
                + "," + "ttlPmt" + "," + "OPTX-optTtlPay" + "," + "OPTY-optTtlPay" + "," + "OPTZ-optTtlPay" + "," + "ttlPay");

        }

        public void WriteNewLineInSummaryFile(string lineToWrite)
        {
            WriteToOutputFile(lineToWrite);
        }

        public void CloseOutputFile(CheckInfo ci)
        {
            WriteToOutputFile("\nCalculation ended at " + ci.softwareCloseTime);
            WriteToOutputFile("Software runtime " + (ci.softwareCloseTime - ci.softwareOpenTime));
            WriteToOutputFile("Search runtime " + (ci.softwareCloseTime - ci.searchStartTime));
            summaryFile.Close();

        }

        // The only output function 
        public void WriteToOutputFile(string message)
        { 
            summaryFile.WriteLine(message);
        }

     }
}

    