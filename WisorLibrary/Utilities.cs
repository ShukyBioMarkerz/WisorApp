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
        public const uint       UNDEFINED_UINT = 0;
        public static string    LOAN_AMOUNT = "Loan amount";
        public static string    MONTHLY_PAYMENT = "Desired monthly payment";
        public static string    PROPERTY_VALUE = "Property value";
        public static string    YEARLY_INCOME = "Yearly income";
        public static string    AGE = "Borrower age";
        public static string    MIN_STR = "min";
        public static string    MAX_STR = "max";
        public static string    EQUAL_STR = "=";
        public static char      SEERATOR_STR = ';';
        public static char      DOT_STR = '.';
        public static string    CRITERIA_FILENAME = "criteria.txt";
        public static string    OUTPUT_DIR = "Output";
        public static char      NAME_SEP_CHAR = '-';
        public static string    CSV_EXT = ".csv";

        // TBD: either read it from file
        public static int GetLoanID()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000);
            return randomNumber;
        }
    }

}