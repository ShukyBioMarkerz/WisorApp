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

        public OutputFile(string orderid, double loanAmtWanted, double monthlyPmtWanted, CheckInfo CheckInfo)
        {
            // get the exact output filename
            string ofn = RunEnvironment.CreateOutputFilename(orderid, loanAmtWanted, monthlyPmtWanted);

            // TBD: Shuky - ensure the directory realy exists
            if (!Directory.Exists(Path.GetDirectoryName(ofn)))
                Directory.CreateDirectory(Path.GetDirectoryName(ofn));

            try
            {
                summaryFile = new StreamWriter(ofn);
                    //OutputConstants.filePath + "Fast Check V3_2_1 - " + CheckInfo.orderID
                    //                                + " - " + GetFileAmtName() + "_" + GetFilePmtName() + ".csv");
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

        

        //private string GetFileAmtName()
        //{
        //    if ((CalculationParameters.loanAmtWanted >= 1000000) && (CalculationParameters.loanAmtWanted <= CalculationConstants.maximumLoanAmount))
        //    {
        //        return CalculationParameters.loanAmtWanted.ToString().Insert(1, ",").Insert(5, ",");
        //    }
        //    else if ((CalculationParameters.loanAmtWanted >= 100000) && (CalculationParameters.loanAmtWanted < 1000000))
        //    {
        //        return CalculationParameters.loanAmtWanted.ToString().Insert(3, ",");
        //    }
        //    else if ((CalculationParameters.loanAmtWanted >= CalculationConstants.optionMinimumAmount) && (CalculationParameters.loanAmtWanted < 100000))
        //    {
        //        return CalculationParameters.loanAmtWanted.ToString().Insert(2, ",");
        //    }
        //    else
        //    {
        //        throw new System.ArgumentOutOfRangeException("Loan Amount Out of Range");
        //    }
        //}



        //private string GetFilePmtName()
        //{
        //    if (CalculationParameters.monthlyPmtWanted > 0)
        //    {
        //        if (CalculationParameters.monthlyPmtWanted >= 10000)
        //        {
        //            return CalculationParameters.monthlyPmtWanted.ToString().Insert(2, ",");
        //        }
        //        else if ((CalculationParameters.monthlyPmtWanted >= 1000) && (CalculationParameters.monthlyPmtWanted < 10000))
        //        {
        //            return CalculationParameters.monthlyPmtWanted.ToString().Insert(1, ",");
        //        }
        //        else
        //        {
        //            return CalculationParameters.monthlyPmtWanted.ToString();
        //        }
        //    }
        //    else
        //    {
        //        throw new System.ArgumentOutOfRangeException("Monthly PMT Wanted Out of Range");
        //    }
        //}






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

        // The only output function 
        public void ReadFormatInputFromFile(string filename)
        {
            // ensure the directory file exists

            // read the format
        }

    }
}

    