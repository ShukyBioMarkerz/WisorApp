using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.GenericProduct;

namespace WisorLib
{
    public class RunEnvironment
    {
        public string OrderID { get; }
        //public string OutputFilename { get; }

        public CheckInfo CheckInfo { get; }

        public CalculationParameters CalculationParameters { get; }

        public PrintOptions PrintOptions { get; }

        public OutputFile OutputFile { get; }

        public static string CreateOutputFilename(string orderid, double loanAmtWanted, double monthlyPmtWanted)
        {
            string fn = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar + orderid +
                MiscConstants.NAME_SEP_CHAR + loanAmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR +
                monthlyPmtWanted.ToString() + DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;

            return fn;
        }

        // Hold the entire running environment data
        public RunEnvironment(string orderid, double loanAmtWanted, double monthlyPmtWanted,
                    uint propertyValue, uint income, uint youngestLenderAge)
        {
            OrderID = orderid;
            //OutputFilename = CreateOutputFilename(orderid, loanAmtWanted, monthlyPmtWanted);
            CheckInfo = new CheckInfo(OrderID);
            CalculationParameters = new CalculationParameters(loanAmtWanted, monthlyPmtWanted,
                    propertyValue, income, youngestLenderAge);
            PrintOptions = new PrintOptions();
            if (PrintOptions.printToOutputFile == true)
            {
                OutputFile = new OutputFile(orderid, loanAmtWanted, monthlyPmtWanted, CheckInfo);
            }
        }

        public static bool SetMarket(markets market)
        {
            bool rc = false;
            Share.theMarket = market;
            // ensure there are combination for this market
            string[,] combination = CalculationConstants.GetCombination(market);

            if (null == combination || 0 == combination.Length)
            {
                WindowsUtilities.loggerMethod("ERROR: no combination founded for market: " + market.ToString());
            }
            else
                rc = true;
            return rc;
        }

        


    }


}
