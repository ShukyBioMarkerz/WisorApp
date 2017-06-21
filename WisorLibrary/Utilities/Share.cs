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
        public static bool shouldShowRatesSelectionWindow { get; set; }
        //public static bool ShouldCalcTheBankProfit { get; set; }
        public static bool ShouldStoreAllCombinations { get; set; }

        public static bool shouldRunSync { get; set; }
        public static bool shouldRunLogicSync { get; set; }

        public static SelectionType theSelectionType { get; set; }
       
        public static bool shouldRunFake { get; set; }
        public static int numberOfPrintResultsInList { get; set; }
        public static int numberOfOption { get; set; }

        public static bool ShouldEachCombinationRunSeparetly { get; set; }
        

        public static int[,] combinations4market;
        public static string[,] combinationsAsString;
        public static string[] theProductsNames { get; set; }
        public static double[] theProductsRates;
        // bank rates
        public static double[] theBankRates;

        public static double[] riskAndLiquidity { get; set; }

        public static double RiskFactor;
        public static double LiquidityFactor;
        public static double BenefitFactor;

        public static uint ProductBeneficialScoreCriteria { get; set; }
        public static uint MaxCombinationNumber { get; set; }

        // Reports
        public static bool ShouldStoreInDB { get; set; }
        public static bool ShouldCreateReport { get; set; }
        public static bool ShouldStoreHTMLReport { get; set; }


        /// <summary>
        /// Print some counters for performance benchmark
        /// </summary>
        public static bool shouldPrintCounters { get; set; }

        public static long counterOfOneDivisionOfAmounts;
        public static long SavedCompositionsCounter;
        public static long CalculatePmtCounter;
        public static long CalculateLuahSilukinCounter;
        public static long RateCounter;
        public static long OptionObjectCounter;
        public static long CalculatePmtFromCalculateLuahSilukinCounter;
        public static long CalculateLuahSilukinCounterNOTInFirstTimePeriod;
        public static long CalculateLuahSilukinCounterInFirstTimePeriod;
        public static long CalculateLuahSilukinCounterIndexUsedFirstTimePeriod;

        public static string CustomerName { get; set; }

        public static markets theMarket { get; set; }

        private static string criteriaFileName;
        public static string CriteriaFileName
        {
            get {
                if (string.IsNullOrEmpty(criteriaFileName))
                    return MiscConstants.CRETIRIA_FILE;
                return criteriaFileName;
            }
            set
            {
                //if (File.Exists(value))
                    criteriaFileName = value;
            }
        }

        private static string productsFileName;
        public static string ProductsFileName
        {
            get
            {
                if (string.IsNullOrEmpty(productsFileName))
                    return MiscConstants.PRODUCTS_FILE;
                return productsFileName;
            }
            set
            {
                //if (File.Exists(value))
                    productsFileName = value;
            }
        }

        private static string loansFileName;
        public static string LoansFileName
        {
            get
            {
                if (string.IsNullOrEmpty(loansFileName))
                    return MiscConstants.LOAN_FILE;
                return loansFileName;
            }
            set
            {
                //if (File.Exists(value))
                    loansFileName = value;
            }
        }

        private static string ratesFileName;
        public static string RatesFileName
        {
            get
            {
                if (string.IsNullOrEmpty(ratesFileName))
                    return MiscConstants.RATES_FILE;
                return ratesFileName;
            }
            set
            {
                //if (File.Exists(value))
                    ratesFileName = value;
            }
        }

        private static string bankRatesFileName;
        public static string BankRatesFileName
        {
            get
            {
                if (string.IsNullOrEmpty(bankRatesFileName))
                    return MiscConstants.BANK_RATES_FILE;
                return bankRatesFileName;
            }
            set
            {
                //if (File.Exists(value))
                bankRatesFileName = value;
            }
        }

        private static string historicFileName;
        public static string HistoricFileName
        {
            get
            {
                if (string.IsNullOrEmpty(historicFileName))
                    return MiscConstants.HISTORIC_FILE;
                return historicFileName;
            }
            set
            {
                //if (File.Exists(value))
                    historicFileName = value;
            }
        }

        private static string combinationFileName;
        public static string CombinationFileName
        {
            get
            {
                if (string.IsNullOrEmpty(combinationFileName))
                    return MiscConstants.COMBINATIONS_FILE;
                return combinationFileName;
            }
            set
            {
                //if (File.Exists(value))
                    combinationFileName = value;
            }
        }

        private static string riskAndLiquidityFileName;
        public static string RiskAndLiquidityFileName
        {
            get
            {
                if (string.IsNullOrEmpty(riskAndLiquidityFileName))
                    return MiscConstants.RISK_LIQUIDITY_FILE;
                return riskAndLiquidityFileName;
            }
            set
            {
                //if (File.Exists(value))
                    riskAndLiquidityFileName = value;
            }
        }
        

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
