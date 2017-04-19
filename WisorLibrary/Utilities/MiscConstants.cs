using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using WisorLib;
//using System.Windows.Forms;

namespace WisorLib
{

    public class MiscConstants
    {
        public const int        UNDEFINED_INT = -1;
        public const int        ILLEGAL_RATE_VALUE = -1;
        public const uint       UNDEFINED_UINT = 0;
        public const double     UNDEFINED_DOUBLE = -1;
        public const string     UNDEFINED_STRING = "";

        public static string    MIN_STR = "min";
        public static string    MAX_STR = "max";
        public static string    EQUAL_STR = "=";
        public static char      SEERATOR_STR = ';';
        public static char      COMMA_SEERATOR_STR = ',';
        public static char      DOT_STR = '.';
        public static char      PERCANTAGE_STR = '%';
        public static char      DOLLAR_STR = '$';
        public static string    CRITERIA_FILENAME = "criteria.txt";
        public static string    OUTPUT_DIR = "Output";
        public static string    DATA_DIR = @"..\..\..\Data";
        public static char      NAME_SEP_CHAR = '-';
        public static string    CSV_EXT = ".csv";
        public static string    DOTS_STR = ":";
        public static string    SEQ_STR = "#";

        // files to load from

        // citi:
        //public static string CRETIRIA_FILE = "CitiGui.xml"; // "Gui.xml"; //  "ClalGui.xml";
        //public static string RATES_FILE = "RateFileGeneric.csv"; // "RateFileClalOnlyApril2017.xlsx"; // ;
        //public static string BANK_RATES_FILE = "CitiRateMarginGeneric.csv"; // "MarginFileClalOnlyApril2017.xlsx";
        //public static string LOAN_FILE = "TestCases.xlsx"; // "ClalPOCDataForFinalCalculation.xlsx"; // "Citi Test cases (2).csv"; // "POC Data - Test Run.csv";
        //public static string COMBINATIONS_FILE = "Combinations.csv"; // "CombinationsIsrael.csv";

        public static string PRODUCTS_FILE = "MortgageProducts - Updated.xml";
        public static string LOGGER_FILE = "LOGGER";

        public static string CRETIRIA_FILE = "ClalGui.xml"; // "Gui.xml"; 
        public static string RATES_FILE = "RateFileClalOnlyApril2017.xlsx";
        public static string BANK_RATES_FILE = "MarginFileClalOnlyApril2017.xlsx";
        public static string LOAN_FILE = "ClalShort.xlsx"; //"ClalPOCDataForFinalCalculation.xlsx"; // "Citi Test cases (2).csv"; // "POC Data - Test Run.csv";
        public static string COMBINATIONS_FILE = "CombinationsIsrael.csv";


        // Loan parameter
        public const string LOAN_AMOUNT = "Loan amount";
        public const string MONTHLY_PAYMENT = "Desired monthly payment";
        public const string PROPERTY_VALUE = "Property value";
        public const string YEARLY_INCOME = "Yearly income";
        public const string AGE = "Borrower age";
        public const string MORTGAGE_TYPE = "Mortgage type"; // First time buyer, 
        public const string PAYMENT_TYPE = "Payment type";
        public const string MORTGAGE_PRODUCT = "Mortgage product";
        public const string LOAN_FICO = "FICO";
        public const string DATE_TAKEN = "Date Taken";
        public const string DESIRE_TERMINATION_MONTH = "DesireTerminationMonth";
        public const string SEQ_NUMBER = "SequentialNumber";
        public const string ORIGINAL_PRODUCT = "Original Product";
        public const string ORIGINAL_RATE = "Original Rate";
        public const string ORIGINAL_TIME = "Original Time";
        public const string CUSTOMER_NAME = "Customer name";
        public const string RISK_VALUE = "Risk";
        public const string LIQUIDITY_VALUE = "Liquidity";

        public static uint DEFAULT_PERCANTAGE_OF_MONTHLY_PAYMENT = 30;

        public static uint NUM_OF_PRODUCTS_IN_COMBINATION = 3;

        public const int NumberOfProfiles = 6;
        public const int NumberOfYearsFrProduct = 27;

        public static double BANK_RATE = 0.005;

        // type of the selection window
        public enum SelectionType { ReadCretiria = 0, ReadProducts , ReadRates , ReadLoansFile};
        
  
        // TBD - shuky
        public enum indices { MADAD, PRIME, CPI, FED, LIBOR, EUROBOR, BBBR, MAKAM, OTHER, NONE }; // Are the options in the code or pulled from outside DB?

        // Risk and Liquidity
        public const int RISK_LIQUIDITY_HEADER = 3;
        public enum Risk { MinimumRisk1, LessRisk2, MediumRisk3, MoreRisk4, MaximumRisk5, NONERisk}; 
        public enum Liquidity { MinimumLiquidity1, LessLiquidity2, MediumLiquidity3, MoreLiquidity4, MaximumLiquidity5 , NONELiquidity};
        public static string RISK_LIQUIDITY_FILE = "RiskLiquidityCiti.xlsx"; 

  
        //public static GenericProduct GetProduct(uint id)
        //{
        //    (markets)Enum.Parse(typeof(markets), product.Element("market").Value, true);

        //    GenericProduct product = Share.theLoadedProducts.GetProduct(id);
        //    if (null == product)
        //    {
        //        WindowsUtilities.loggerMethod("GetProduct: failed to find product id: " + id);
        //    }
        //    else
        //    {
        //        // TBD: check product correctess
        //        product = null;
        //    }

        //    return product;
        //}

        //// spend some time .....
        //public static void FakeFunctionality()
        //{
        //    Thread.Sleep(12345);
        //}


    }

}

