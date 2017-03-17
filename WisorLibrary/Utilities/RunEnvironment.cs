using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.DataObjects;
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

        public OutputFile OutputFile { get; internal set; }
        public List<ChosenComposition> listOfSelectedCompositions { get; set; }
        public List<string> headerOfListOfSelectedCompositions  {  get; }

        public int MaxProfit { get; set; }
        public int MaxBankPay { get; set; }
        public int MinBorrowerPay { get; set; }

        public LogCombinationResults Logger  { get; internal set; }

        public loanDetails theLoan { get; }


        // Hold the entire running environment data
        public RunEnvironment(loanDetails loan)
            //string orderid, double loanAmtWanted, double monthlyPmtWanted,
            //        uint propertyValue, uint income, uint youngestLenderAge, uint fico, uint sequenceID)
        {
            theLoan = loan;
            OrderID = loan.ID;
            //OutputFilename = CreateOutputFilename(orderid, loanAmtWanted, monthlyPmtWanted);
            CheckInfo = new CheckInfo(OrderID);
            CalculationParameters = new CalculationParameters(loan.LoanAmount, loan.DesiredMonthlyPayment,
                    loan.PropertyValue, loan.YearlyIncome, loan.BorrowerAge, loan.fico);
            PrintOptions = new PrintOptions();

            CreateTheOutputFiles(loan);
        
            listOfSelectedCompositions = new List<ChosenComposition>();
            headerOfListOfSelectedCompositions = new List<string>()
            {
                "ProductX", "ProductY", "ProductZ", "Borrower pay", "Bank amount", "Profit"
            };
            MaxProfit = MaxBankPay = MinBorrowerPay = 0;
   
        }

        public void CreateTheOutputFiles(loanDetails loan, string additionalName = MiscConstants.UNDEFINED_STRING) {
            CloseTheOutputFiles();

            OutputFile = new OutputFile(loan, additionalName);
            // create the combination logger file
            if (Share.ShouldStoreAllCombinations)
            {
                Logger = new LogCombinationResults(OutputFile.OutputFilename);
            }
        }

        public void CloseTheOutputFiles()
        {
            if (null != OutputFile)
            {
                OutputFile.Remove();
                OutputFile.CloseOutputFile();
            }
            //OutputFile = null;
            if (null != Logger)
                Logger.CloseLog2CSV();
            //Logger = null;
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

     


       
    }


}
