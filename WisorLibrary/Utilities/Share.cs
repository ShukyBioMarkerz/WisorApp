﻿using System;
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
        public static bool ShouldCalcTheBankProfit { get; set; }

        public static bool shouldRunSync { get; set; }
        public static bool shouldRunLogicSync { get; set; }


        public static SelectionType theSelectionType { get; set; }
        public static int numberOfOption { get; set; }

        public static bool shouldRunFake { get; set; }
        public static int numberOfPrintResultsInList { get; set; }

        public static int[,] combinations4market;
        public static string[,] combinationsAsString;
        public static string[] theProductsNames { get; set; }
        public static double[] theProductsRates;
    
        /// <summary>
        /// Print the selected combination to excel file
        /// </summary>
        public static uint counterOfCompositions { get; set; }
        public static uint numOfMatchesGlobal { get; set; }


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

        public static double GetBankRate()
        {
            return MiscConstants.BANK_RATE;
        }
    }
}
