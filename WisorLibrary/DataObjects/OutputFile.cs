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

 
        public OutputFile(loanDetails loan, string additionalName)
            //string orderid, double loanAmtWanted, double monthlyPmtWanted, CheckInfo CheckInfo, uint sequenceID)
        {
            // get the exact output filename
            OutputFilename = MiscUtilities.CreateOutputFilename(
                loan.ID, loan.LoanAmount, loan.DesiredMonthlyPayment, loan.SequentialNumber, additionalName);
    
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
            WriteToOutputFile("Order ID : " + loan.ID);
            WriteToOutputFile("Original loan date : " + loan.DateTaken.ToString());
            WriteToOutputFile("Original loan amount : " + loan.OriginalLoanAmount);
            WriteToOutputFile("Remainig loan amount : " + loan.LoanAmount);
            
            //WriteToOutputFile("Execution ID : " + CheckInfo.fastCheckID);
            //WriteToOutputFile("Borrower profile : " + CalculationConstants.profiles[BorrowerProfile.borrowerProfile]);
            WriteToOutputFile("Start time : " + DateTime.Now);

            // Shuky - add the header line 
            WriteToOutputFile(/*"Ticks" + "," + "OrderID" + "," + "Time" + "," +*/
                "X:optType" + "," + "X:optAmt" + "," + "X:optTime" + "," + "X:RateFirstPeriod" + "," +
                "Y:optType" + "," + "Y:optAmt" + "," + "Y:optTime" + "," + "Y:RateFirstPeriod" + "," +
                "Z:optType" + "," + "Z:optAmt" + "," + "Z:optTime" + "," + "Z:RateFirstPeriod" + "," +
                "OPTX-optPmt" + "," + "OPTY-optPmt" + "," + "OPTZ-optPmt"
                + "," + "ttlPmt" + "," + "OPTX-optTtlPay" + "," + "OPTY-optTtlPay" +
                "," + "OPTZ-optTtlPay" + "," + "ttlPay" +
                // bank profit data
                "," + "X:BankTtlPay" + "," + "Y:BankTtlPay" + "," + "Z:BankTtlPay" +
                "," + "TtlBankPayPay" + "," + "TtlBankProfit");

        }

        public void WriteNewLineInSummaryFile(string lineToWrite)
        {
            WriteToOutputFile(lineToWrite);
        }

        public void CloseOutputFile()
        {
            summaryFile.Close();
        }

        // The only output function 
        public void WriteToOutputFile(string message)
        { 
            summaryFile.WriteLine(message);
        }

        public void Remove()
        {
            CloseOutputFile();

            try
            {
                if (File.Exists(OutputFilename))
                {
                    string filenm = System.IO.Path.GetFileNameWithoutExtension(OutputFilename);
                    string ext = System.IO.Path.GetExtension(OutputFilename);
                    string dir = System.IO.Path.GetDirectoryName(OutputFilename);
                    string newfn = dir + System.IO.Path.DirectorySeparatorChar + filenm + "-OLD" + ext;

                    File.Move(OutputFilename, newfn);
                }
            }
            catch (Exception ex)
            {
                // TBD use the WindowsUtilities.loggerMethod
                Console.WriteLine("ERROR: OutputFile File.Delete got Exception from file: " + OutputFilename + ". Exception: " + ex.ToString());
            }
        }


    }
}

    