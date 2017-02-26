using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
        public static char      DOT_STR = '.';
        public static string    CRITERIA_FILENAME = "criteria.txt";
        public static string    OUTPUT_DIR = "Output";
        public static char      NAME_SEP_CHAR = '-';
        public static string    CSV_EXT = ".csv";

        // Loan parameter
        public const string LOAN_AMOUNT = "Loan amount";
        public const string MONTHLY_PAYMENT = "Desired monthly payment";
        public const string PROPERTY_VALUE = "Property value";
        public const string YEARLY_INCOME = "Yearly income";
        public const string AGE = "Borrower age";
        public const string MORTGAGE_TYPE = "Mortgage type"; // First time buyer, 
        public const string PAYMENT_TYPE = "Payment type";
        public const string MORTGAGE_PRODUCT = "Mortgage product";

        public static uint DEFAULT_PERCANTAGE_OF_MONTHLY_PAYMENT = 30;

        // type of the selection window
        public enum SelectionType { ReadLoans = 0, ReadProducts , ReadRates };

        // TBD: either read it from file
        public static int GetLoanID()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000);
            return randomNumber;
        }

        // TBD - shuky
        public enum indices { MADAD, PRIME, CPI, FED, LIBOR, EUROBOR, BBBR, MAKAM, OTHER, NONE }; // Are the options in the code or pulled from outside DB?

        public static uint CalculateMonthlyPayment(uint loanAmount, uint propertyValue,
            uint yearlyIncome, uint borrowerAge)
        {
            uint desiredMonthlyPayment = 0;

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
        public static GenericProduct GetProduct(string id)
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

        

     
    }

}