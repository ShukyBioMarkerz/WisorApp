﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    //public class CheckInfo
    //{
    //    // Time Software Opened and closed
    //    public static DateTime calculationStartTime;
    //    public static DateTime calculationEndTime;
    //    public static DateTime softwareOpenTime;
    //    public static DateTime searchStartTime;
    //    public static DateTime softwareCloseTime;

    //    // Execution Information
    //    public static double startTimeToMeasure = new DateTime(2014, 1, 1).Ticks;
    //    public static string fastCheckID = "";
    //    public static string orderID = "";
    //    public static double resultsID = 0;
    //}




    class Options
    {
        // List of Options as inserted in website admin
        // PRIME, MAKAM, FIXTSAMUD, FIXNOTSAMUD, ALT12 , ALT24, ALT30, ALT60, ALT60NOTSAMUD, ALT84, ALT120, ZAKAUT, NORATETSAMUD, NORATENOTSAMUD
        // 5        6       7           8           9       10    11     12         13         14     15        16      17              18
        // List of Options as inserted in original code
        // PRIME, FIXNOTSAMUD, ALT60NOTSAMUD, FIXTSAMUD, ALT12, ALT24, ALT30, ALT60, ALT84, ALT120
        //  1           2           3           4           5       6   7       8       9       10
        // List of Options as inserted in code to correlate with website
        // PRIME, MAKAM, FIXTSAMUD, FIXNOTSAMUD, ALT12 , ALT24, ALT30, ALT60, ALT60NOTSAMUD, ALT84, ALT120, ZAKAUT, NORATETSAMUD, NORATENOTSAMUD
        // 1        2       3           4           5       6    7       8         9           10     11        12      13              14

        // Type list as inserted in website admin

        // Shuky TBD
        // The typesList shoyld be defined in thre xml typeId 
        //public enum typesList {  EMPTY, PRIME, MAKAM, FIXTSAMUD, FIXNOTSAMUD, ALT12, ALT24, ALT30, ALT60, 
        //                            ALT60NOTSAMUD, ALT84, ALT120, ZAKAUT, NORATETSAMUD, NORATENOTSAMUD };
        //public static string[] optionTypes = { "EMPTY", "PRIME", "MAKAM", "FIXTSAMUD", "FIXNOTSAMUD", "ALT12", "ALT24", "ALT30", "ALT60", 
        //                                            "ALT60NOTSAMUD", "ALT84", "ALT120", "ZAKAUT", "NORATETSAMUD", "NORATENOTSAMUD" };


        // Option Names For Calculation
        public enum options { OPTX, OPTY, OPTZ };

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

        // Time restriction from age of youngest lender
        public uint maximumTimeForLoan = 360;

        // Option Types Chosen for calculation
        public OptionTypes optTypes = null;

        // Minimum Amounts possible for every Option (Option X, Option Y, Option Z)
        public double[] minAmts;

        // Maximum Amounts possible for every Option (Option X, Option Y, Option Z)
        public double[] maxAmts;

        public CalculationParameters(double loanAmtWante, double monthlyPmtWante,
                uint propertyValu, uint incom, uint youngestLenderAg)
        {
            loanAmtWanted = loanAmtWante;
            monthlyPmtWanted = monthlyPmtWante;
            propertyValue = propertyValu;
            income = incom;
            youngestLenderAge = youngestLenderAg;
            ltv = (loanAmtWanted / propertyValue);
            pti = (monthlyPmtWanted / income);

            minAmts = new double[] {CalculationConstants.optionMinimumAmount,
                              CalculationConstants.optionMinimumAmount,
                              CalculationConstants.optionMinimumAmount };

            maxAmts = new double[] { -1, -1, -1 };
        }

    }



    //public class CalculationParameters
    //{
    //    // Loan Amount Wanted - Inserted by Admin Once
    //    public static double loanAmtWanted = -1;

    //    // Monthly Payment Wanted - Inserted by Admin Once
    //    public static double monthlyPmtWanted = -1;

    //    // Income
    //    public static uint income = 0;

    //    // Property value
    //    public static uint propertyValue = 0;

    //    // Youngest lender age
    //    public static uint youngestLenderAge = 0;

    //    // Loan to Value (LTV)
    //    public static double ltv = -1;

    //    // Payment to Income (PTI)
    //    public static double pti = -1;

    //    // Time restriction from age of youngest lender
    //    public static uint maximumTimeForLoan = 360;

    //    // Option Types Chosen for calculation
    //    public static OptionTypes optTypes = null;

    //    // Minimum Amounts possible for every Option (Option X, Option Y, Option Z)
    //    public static double[] minAmts = { CalculationConstants.optionMinimumAmount, 
    //                                        CalculationConstants.optionMinimumAmount, 
    //                                        CalculationConstants.optionMinimumAmount };

    //    // Maximum Amounts possible for every Option (Option X, Option Y, Option Z)
    //    public static double[] maxAmts = { -1, -1, -1 };

    //}



    class CalculationConstants
    {
        // Maximum loan amount possible
        public const double maximumLoanAmount = 5000000;

        // Minimum Amount possible in a single Option
        public const double optionMinimumAmount = 40000;

        // Jump between amounts for check
        public const double jumpBetweenAmounts = 200;

        // Minimum Time to take a loan
        public const uint minimumTimeForLoan = 48;

        // Yearly inflation for calculation
        public const double inflation = 0.018;

        // Large deviation from target monthly payment
        public const double largeDev = 20.0;

        // Small deviation from target monthly payment
        public const double smallDev = 1.0;

        // Size of Round for fractions - number of digits after decimal
        public const int fractionRound = 100000000;

        // Size of Round for Amounts - number of digits after decimal
        public const int doubleRound = 100;

        // Borrower profile - Low =1 / Medium = 2 / High = 3
        public enum borrowerProfiles { NOTSET, BEST, VERY_GOOD, GOOD, AVERAGE, NOT_SO_GOOD, BAD, NOTOK };
        public static string[] profiles = { "NOTSET", "6/6 - BEST", "5/6 - VERY GOOD", "4/6 - GOOD", "3/6 - AVERAGE", "2/6 - NOT SO GOOD", "1/6 - BAD", "NOTOK" };

        // Possible combinations of options for fast search
        //public static readonly uint[,] combinations = { { 4, 4, 4 }, { 3, 3, 3 } };

        // Omri: define the other markets
        // Should be outside the code
        //public static readonly uint[,] combinations = { { 1, 4, 9 }, { 1, 4, 8 }, { 1, 3, 9 }, { 1, 3, 8 }, { 1, 4, 4 }, { 1, 3, 3 }, { 1, 4, 3 } };
        public static readonly string[,] combinations = { { "PrimeIsrael", "FixedNoTsamudIsrael", "Alt60NoTsamudIsrael" },
                                                        { "PrimeIsrael", "FixedNoTsamudIsrael", "Alt60TsamudIsrael" }, 
                                                        { "PrimeIsrael", "FixedTsamudIsrael", "Alt60NoTsamudIsrael" }, 
                                                        { "PrimeIsrael", "FixedTsamudIsrael", "Alt60TsamudIsrael" }, 
                                                        { "PrimeIsrael", "FixedNoTsamudIsrael", "FixedNoTsamudIsrael" },
                                                        { "PrimeIsrael", "FixedTsamudIsrael", "FixedTsamudIsrael" },
                                                        { "PrimeIsrael", "FixedNoTsamudIsrael", "FixedTsamudIsrael" } };


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
            printMainInConsole = true;
            printFunctionsInConsole = true;
            printSubFunctionsInConsole = true;
            printPercentageDone = true;
            printToOutputFile = true;
        }
    }


    //class PrintOptions
    //{
    //    // Print main function text - without internal functions
    //    public const bool printMainInConsole = true;

    //    // Print as program runs on console or not
    //    public const bool printFunctionsInConsole = false;

    //    // Print sub functions on console
    //    public const bool printSubFunctionsInConsole = false;

    //    // Print percentage done
    //    public const bool printPercentageDone = true;

    //    // Print saved matches to output file or not
    //    public const bool printToOutputFile = true;
    //}



    class ResultsOutput
    {
        // Composition saved with mimimum payment
        public static Composition bestCompositionSoFar = null;
        public static Composition bestComposition = null;
    }




    //class InterestRates
    //{
    //    double[] fixedTsamudRates = null;
    //    double[] fixedNoTsamudRates = null;
    //    double[] alternatingRates = null;
    //}




    //class OutputConstants
    //{

    //    public static OutputFile outputFile = null;
        
    //    // Lenovo
    //    // public const string filePath = "C:\\Business\\Software\\Test Runs\\FastThreeOptionSearchV3_2_1\\";
    //    public const string filePath = ".\\output\\";

    //    // Desktop       
    //    //public const string filePath = "C:\\Optimmizer Software\\Three Options\\Test Runs\\ExperimentThree\\";

    //    // Amit Marzel Laptop
    //    //public const string filePath = "C:\\Users\\Amitma\\Desktop\\Wisor\\Orders\\Optimization Results\\";

    //}

  

}