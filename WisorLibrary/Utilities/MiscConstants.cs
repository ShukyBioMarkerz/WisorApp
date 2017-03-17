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
        public static string    CRITERIA_FILENAME = "criteria.txt";
        public static string    OUTPUT_DIR = "Output";
        public static string    DATA_DIR = @"..\..\..\Data";
        public static char      NAME_SEP_CHAR = '-';
        public static string    CSV_EXT = ".csv";
        public static string    DOTS_STR = ":";
        public static string    SEQ_STR = "#";
        public static string    RATES_FILE = "RateFileGeneric.csv";
        public static string    BANK_RATES_FILE = "CitiRateMarginGeneric.csv";
        public static string    LOAN_FILE = "POC Data - Test Run.csv";
        public static string    PRODUCTS_FILE = "MortgageProducts - Updated.xml";
        public static string    CRETIRIA_FILE = "Gui.xml";
        public static string    LOGGER_FILE = "LOGGER";


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

        public static uint DEFAULT_PERCANTAGE_OF_MONTHLY_PAYMENT = 30;

        public const int NumberOfProfiles = 6;
        public const int NumberOfYearsFrProduct = 27;

        public static double BANK_RATE = 0.005;

        // type of the selection window
        public enum SelectionType { ReadCretiria = 0, ReadProducts , ReadRates , ReadLoansFile};
        
  
        // TBD - shuky
        public enum indices { MADAD, PRIME, CPI, FED, LIBOR, EUROBOR, BBBR, MAKAM, OTHER, NONE }; // Are the options in the code or pulled from outside DB?

        public static uint CalculateMonthlyPayment(uint loanAmount, uint propertyValue, uint yearlyIncome, uint borrowerAge)
        {
            uint desiredMonthlyPayment = 0;

            desiredMonthlyPayment = (uint)yearlyIncome / 3;
            return desiredMonthlyPayment;
        }

        public static double GetIndexRateForOption(indices indic)
        {
            double index = 0;

            return index;
        }

        public static bool LoadIndexFile(string filename)
        {
            bool rc = false;

            return rc;
        }

        // function to get the product by the unique id
        public static GenericProduct GetProduct(int id)
        {
            GenericProduct product = null;
            if (null != Share.theLoadedProducts)
                product = Share.theLoadedProducts.GetProduct(id);
            else
                WindowsUtilities.loggerMethod("GetProduct: Share.theLoadedProducts in null!!! ");

            if (null == product)
            {
                WindowsUtilities.loggerMethod("GetProduct: failed to find product id: " + id);
            }
            else
            {
                // TBD: check product correctess
            }

            return product;
        }

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

        // spend some time .....
        public static void FakeFunctionality()
        {
            Thread.Sleep(12345);
        }


    }

    public class MiscUtilities
    {
        // TBD: either read it from file
        public static int GetLoanID()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000);
            return randomNumber;
        }

        public static uint GetSequenceID()
        {
            // TBD
            return 1;
        }

        public static string CreateOutputFilename(string orderid, double loanAmtWanted, double monthlyPmtWanted, uint sequenceID, string additionalName)
        {
            string customer = (String.IsNullOrEmpty(Share.CustomerName)) ? MiscConstants.UNDEFINED_STRING : (Share.CustomerName + MiscConstants.NAME_SEP_CHAR);
            string seq = (MiscConstants.UNDEFINED_UINT == sequenceID) ? MiscConstants.UNDEFINED_STRING : MiscConstants.SEQ_STR + sequenceID + MiscConstants.SEQ_STR + MiscConstants.NAME_SEP_CHAR;
            string add = (String.IsNullOrEmpty(additionalName)) ? MiscConstants.UNDEFINED_STRING : MiscConstants.NAME_SEP_CHAR + additionalName + MiscConstants.NAME_SEP_CHAR;
            string fn = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar +
                customer + seq + orderid +
                MiscConstants.NAME_SEP_CHAR + loanAmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR +
                monthlyPmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR + add +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;

            return fn;
        }

    }

}