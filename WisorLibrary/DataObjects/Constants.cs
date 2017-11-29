using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.DataObjects;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;

namespace WisorLib
{

    public class CheckInfo
    {
        // Time Software Opened and closed
        public DateTime calculationStartTime;
        public DateTime calculationEndTime;
        public DateTime softwareOpenTime { get; set; }
        public DateTime searchStartTime;
        public DateTime softwareCloseTime;

        // Execution Information
        public double startTimeToMeasure;
        public string fastCheckID;
        public string orderID { get; }
        public double resultsID;

        public CheckInfo(string orderid)
        {
            orderID = orderid;
            startTimeToMeasure = new DateTime(2014, 1, 1).Ticks;
            // Define Execution ID
            fastCheckID = (softwareOpenTime.Ticks - startTimeToMeasure).ToString();
        }
    }

 
    class Options
    {
        
        // Option Names For Calculation
        public enum options { OPTX, OPTY, OPTZ };
        // 2 products per composition
        public enum options2Products { OPTX, OPTY };

        // Limit Times for Options
        public enum pmtLimits { MAXTIME, MINTIME };

        // Limit Points Letters for Search
        public enum limitPointsLetters { A, B };
        public static char[] letters = { 'A', 'B' };

        // Limit Points Numbers for Search
        public enum limitPointsNumbers { ONE, TWO };
        public static int[] numbers = { 1, 2 };

        // Binary Search Results Options
        public enum binarySearchResults { MATCH, INRANGE, TOOLARGE, TOOSMALL };
        public static string[] ranges = { "MATCH", "INRANGE", "TOOLARGE", "TOOSMALL" };

        public enum pmtRange { TOOSMALL, INRANGE, TOOLARGE };
        public static string[] pmtRanges = { "TOOSMALL", "INRANGE", "TOOLARGE" };

    }



    public class CalculationParameters
    {
        // Loan Amount Wanted - Inserted by Admin Once
        public double loanAmtWanted;

        // Monthly Payment Wanted - Inserted by Admin Once
        public double monthlyPmtWanted;

        // Income
        public uint income;

        // Property value
        public uint propertyValue;

        // Youngest lender age
        public uint youngestLenderAge;

        // Loan to Value (LTV)
        public double ltv;

        // Payment to Income (PTI)
        public double pti;

        // Credit score (FICO)
        public int fico { get; set; }

        // Time restriction from age of youngest lender
        public uint maximumTimeForLoan = Share.maximumTimeForLoan; // 360;

        // Option Types Chosen for calculation
        public OptionTypes optTypes = null;

        // Minimum Amounts possible for every Option (Option X, Option Y, Option Z)
        public double[] minAmts;

        // Maximum Amounts possible for every Option (Option X, Option Y, Option Z)
        public double[] maxAmts;

        public CalculationParameters(double loanAmtWante, double monthlyPmtWante,
                uint propertyValu, uint incom, uint youngestLenderAg, int fic)
        {
            loanAmtWanted = loanAmtWante;
            monthlyPmtWanted = monthlyPmtWante;
            propertyValue = propertyValu;
            income = incom;
            youngestLenderAge = youngestLenderAg;
            ltv = (loanAmtWanted / propertyValue);
            pti = (monthlyPmtWanted / income);
            fico = fic;

            minAmts = new double[] {CalculationConstants.optionMinimumAmount,
                              CalculationConstants.optionMinimumAmount,
                              CalculationConstants.optionMinimumAmount };
            maxAmts = new double[] { -1, -1, -1 };

        }

    }



    
    class CalculationConstants
    {
        // Maximum loan amount possible
        //public const double maximumLoanAmount = 5000000;

        // Minimum Amount possible in a single Option
        public const double optionMinimumAmount = Share.optionMinimumAmount; // 30000; // 40000;

        // Jump between amounts for check
        public static double jumpBetweenAmounts = Share.jumpBetweenAmounts; // 1000;

        // Minimum Time to take a loan
        public const uint minimumTimeForLoan = Share.minimumTimeForLoan; // 48;

        // Large deviation from target monthly payment
        public const double largeDev = Share.largeDev; // 20.0;

        // Small deviation from target monthly payment
        public const double smallDev = Share.smallDev; // 1.0;

        // Size of Round for fractions - number of digits after decimal
        //public const int fractionRound = 100000000;

        // Size of Round for Amounts - number of digits after decimal
        //public const int doubleRound = 100;

        // Borrower profile - Low =1 / Medium = 2 / High = 3
        public enum borrowerProfiles { NOTSET, BEST, VERY_GOOD, GOOD, AVERAGE, NOT_SO_GOOD, BAD, NOTOK };
        public static string[] profiles = { "NOTSET", "6/6 - BEST", "5/6 - VERY GOOD", "4/6 - GOOD", "3/6 - AVERAGE", "2/6 - NOT SO GOOD", "1/6 - BAD", "NOTOK" };

        // enum markets { USA, UK, ISRAEL, OTHER , NONE}
        //public static string[,] GetCombination(markets market)
        //{
        //    string[,] combination = Combinations.GetCombination(market);
        //    return combination;
        //}

        public static void PrintCombination(string[,] com)
        {
            string msg = MiscConstants.UNDEFINED_STRING;
            // string[,] com = Combinations.GetCombination(theMarket);

            for (int i = 0; i <= com.GetUpperBound(0); i++)
            {
                msg += (i + 1) + " : ";
                for (int j = 0; j < Share.NumberOfProductsInCombination; j++)
                {
                    msg += com[i, j] + ", ";
                }
                msg += "\n";
             }
            Console.WriteLine(msg);
            //return msg;
        }
    }


    public class PrintOptions
    {
        // Print main function text - without internal functions
        public bool printMainInConsole;

        // Print as program runs on console or not
        public bool printFunctionsInConsole;

        // Print sub functions on console
        public bool printSubFunctionsInConsole;

        // Print percentage done
        public bool printPercentageDone;

        // Print saved matches to output file or not
        public bool printToOutputFile;

        public PrintOptions()
        {
            printMainInConsole = Share.printMainInConsole;
            printToOutputFile = Share.printToOutputFile;
            printFunctionsInConsole = Share.printFunctionsInConsole;
            printSubFunctionsInConsole = Share.printSubFunctionsInConsole;
            printPercentageDone = Share.printPercentageDone;
        }
    }


    public class ResultsOutput
    {
        // Composition saved with mimimum payment
        public Composition bestCompositionSoFar { get; set; }
        public Composition bestComposition { get; set; }

        public ResultsOutput()
        {
            bestCompositionSoFar = null;
            bestComposition = null;
        }
    }
}
