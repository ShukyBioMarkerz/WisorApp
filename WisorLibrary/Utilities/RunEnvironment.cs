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
        public List<ChosenComposition> listOfSelectedCompositions { get; set; }

        public static string CreateOutputFilename(string orderid, double loanAmtWanted, double monthlyPmtWanted)
        {
            string fn = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar + orderid +
                MiscConstants.NAME_SEP_CHAR + loanAmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR +
                monthlyPmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR + DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;

            return fn;
        }

        // Hold the entire running environment data
        public RunEnvironment(string orderid, double loanAmtWanted, double monthlyPmtWanted,
                    uint propertyValue, uint income, uint youngestLenderAge, uint fico)
        {
            OrderID = orderid;
            //OutputFilename = CreateOutputFilename(orderid, loanAmtWanted, monthlyPmtWanted);
            CheckInfo = new CheckInfo(OrderID);
            CalculationParameters = new CalculationParameters(loanAmtWanted, monthlyPmtWanted,
                    propertyValue, income, youngestLenderAge, fico);
            PrintOptions = new PrintOptions();
            if (PrintOptions.printToOutputFile == true)
            {
                OutputFile = new OutputFile(orderid, loanAmtWanted, monthlyPmtWanted, CheckInfo);
            }
            listOfSelectedCompositions = new List<ChosenComposition>();
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
            WindowsUtilities.loggerMethod("NOTICE: running for market: " + market.ToString() + ", #of combination: " + combination.GetUpperBound(0));
            Console.WriteLine("NOTICE: running for market: " + market.ToString() + ", #of combination: " + combination.Length);
            return rc;
        }

        public List<ChosenComposition> OrderCompositionListByBorrower()
        {
            List<ChosenComposition> SortedSelectedCompositions =
                listOfSelectedCompositions.OrderBy(o => o.borrowerPay).ToList();
            return SortedSelectedCompositions;
        }

        public List<ChosenComposition> OrderCompositionListByBank()
        {
            List<ChosenComposition> SortedSelectedCompositions =
                listOfSelectedCompositions.OrderBy(o => o.bankPay).ToList();
            return SortedSelectedCompositions;
        }


        /// <summary>
        /// Enable to write logs
        /// </summary>
        /// 
        private StreamWriter fileStream;

        public void PrepareLog2CSV(string ID)
        {
            fileStream = null;
            string filename = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar +
                /*orderid*/ ID +
                MiscConstants.NAME_SEP_CHAR + "Logger" + MiscConstants.NAME_SEP_CHAR +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;


            // TBD: Shuky - ensure the directory realy exists
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            fileStream = new StreamWriter(filename);
        }

        public void PrintLog2CSV(string[] msg)
        {
            try
            {
                string msg2write = null;
                for (int i = 0; i < msg.Length; i++)
                {
                    msg2write += msg[i] + MiscConstants.COMMA_SEERATOR_STR;
                }

                fileStream.WriteLine(msg2write);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: PrintLog2CSV got Exception: " + e.ToString());
            }
        }

        public void CloseLog2CSV()
        {
            fileStream.Close();
        }





    }


}
