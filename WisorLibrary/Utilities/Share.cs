using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class Share
    {
        public static FieldList theLoadedCriteriaFields { get; set; }
        public static FieldList theSelectedCriteriaFields { get; set; }

        public static string theCriteriaFilename { get; set; }
        public static bool shouldShowCriteriaSelectionWindow { get; set; }
        public static bool shouldShowCriteriaSelectionContinue { get; set; }


        public static ProductsList theLoadedProducts { get; set; }
        public static ProductsList theSelectedProducts { get; set; }

        public static string theProductsFilename { get; set; }
        public static bool shouldShowProductSelectionWindow { get; set; }
        public static bool shouldShowProductSelectionContinue { get; set; }

        public static bool shouldShowLoansSelectionWindow { get; set; }

        public static SelectionType theSelectionType { get; set; }

        //public static uint counterOfCompositions { get; set; }
        //public static uint numOfMatchesGlobal { get; set; }

        //static StreamWriter fileStream;

        //public static void PrepareLog2CSV()
        //{
        //    fileStream = null;
        //    string filename = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
        //        + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar +
        //        /*orderid*/ MiscConstants.GetLoanID() +
        //        MiscConstants.NAME_SEP_CHAR + "Logger" + MiscConstants.NAME_SEP_CHAR +
        //        DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;


        //    // TBD: Shuky - ensure the directory realy exists
        //    if (!Directory.Exists(Path.GetDirectoryName(filename)))
        //        Directory.CreateDirectory(Path.GetDirectoryName(filename));

        //    fileStream = new StreamWriter(filename);

        //}

        //public static void PrintLog2CSV(string[] msg)
        //    //str1, string str2, string str3, string str4, string str5, string str6, string str7)
        //{
        //    try
        //    {
                
        //        string msg2write = null;
        //        for (int i = 0; i < msg.Length; i++)
        //        {
        //            msg2write += msg[i] + COMMA_SEERATOR_STR;
        //        }
           
        //        fileStream.WriteLine(msg2write);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("ERROR: PrintLog2CSV got Exception: " + e.ToString());
        //    }
        //}

        public static markets theMarket { get; set; }

        // Once the user select the order, update the loaded list accordingly
        public static void OrderTheCriteriaFields()
        {
            FieldList theNewList = new FieldList();

            if (null == theSelectedCriteriaFields)
                theSelectedCriteriaFields = theLoadedCriteriaFields;

            foreach (CriteriaField cf in theSelectedCriteriaFields)
            {
                CriteriaField f = theLoadedCriteriaFields.GetField(cf.ID);
                f.index = cf.index;
                theNewList.Add(f);
            }
            theSelectedCriteriaFields = theNewList;
        }

        public static void OrderTheProductsFields()
        {
            ProductsList theNewList = new ProductsList();

            theNewList = theSelectedProducts;
            //foreach (GenericProduct p in theSelectedProducts)
            //{
            //    GenericProduct gp = theLoadedProducts.GetProduct(p.ID);
            //    theNewList.Add(gp);
            //}
            //theSelectedProducts = theNewList;
        }
    }
}
