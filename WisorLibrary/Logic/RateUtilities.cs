using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;
using static WisorLib.Options;

namespace WisorLibrary.Logic
{
    class RateUtilities
    {

        // make the class singltone
        private static RateUtilities instance;

        public static RateUtilities SetFilename(string filename, string bankFilename, string secondPeriodFN,
            string secondPeriodBankRatesFN)
        {
           if (null == instance)
           {
                instance = new RateUtilities(filename, bankFilename, secondPeriodFN, secondPeriodBankRatesFN);
           }
           return instance;
         }

        public static RateUtilities Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new RateUtilities();
                }
                return instance;
            }
        }

        //Dictionary<RatesKey, RateLine> rates;
        string filename, bankFilename, secondPeriodFilename, secondPeriodBankFilename;
        public bool Status { get; internal set; }

        private RateUtilities(string filename = "", string bankFilename = "", string secondPeriodFN = "", string secondPeriodBankRatesFN = "")
        {
            bool rc = false;
            Status = true;

            if (!String.IsNullOrEmpty(filename) && !String.IsNullOrEmpty(bankFilename))
            {
                setFilename(filename);
                setBankFilename(bankFilename);
                secondPeriodFilename = secondPeriodFN;
                secondPeriodBankFilename = secondPeriodBankRatesFN;
                rc = LoadRates();
                if (!rc)
                {
                    Status = rc;
                    WindowsUtilities.loggerMethod("ERROR RateUtilities failed to load borrower rates from file: " + filename);
                }
                rc = LoadBankRates();
                if (!rc)
                {
                    Status = rc;
                    WindowsUtilities.loggerMethod("ERROR RateUtilities failed to load bank  rates from file: " + bankFilename);
                }
            }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE RateUtilities without setting the rates file name");
            }
        }

        private void setFilename(string fn)
        {
            filename = fn;
        }

        private void setBankFilename(string fn)
        {
            bankFilename = fn;
        }

        public bool LoadRates()
        {
            if (null == Share.theProductsRates || 0 >= Share.theProductsRates.Length)
                Share.theProductsRates = LoadRatesCSVFile(filename);
            if (null == Share.theProductsRates || 0 >= Share.theProductsRates.Length)
            {
                WindowsUtilities.loggerMethod("ERROR LoadRates failed to load.");
            }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE LoadRates succesfully load: " + Share.theProductsRates.Length + " entries from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }

            if (markets.UK == Share.theMarket)
            {
                if (null == Share.theProductsRatesSecondPeriod || 0 >= Share.theProductsRatesSecondPeriod.Length)
                    Share.theProductsRatesSecondPeriod = LoadRatesCSVFile(secondPeriodFilename);
                if (null == Share.theProductsRatesSecondPeriod || 0 >= Share.theProductsRatesSecondPeriod.Length)
                {
                    WindowsUtilities.loggerMethod("ERROR theProductsRatesSecondPeriod failed to load.");
                }
                else
                {
                    WindowsUtilities.loggerMethod("NOTICE theProductsRatesSecondPeriod succesfully load: " + Share.theProductsRatesSecondPeriod.Length + " entries from file: " +
                        filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                }
            }
            
            return (null != Share.theProductsRates && 0 < Share.theProductsRates.Length);
        }

 
        public bool LoadBankRates()
        {
            if (null == Share.theBankRates || 0 >= Share.theBankRates.Length)
                Share.theBankRates = LoadRatesCSVFile(bankFilename);
            int numOfRates = (null == Share.theBankRates) ? 0 : Share.theBankRates.Length;
            WindowsUtilities.loggerMethod("NOTICE LoadBankRates succesfully load: " + numOfRates + " entries from file: " +
                bankFilename.Substring(bankFilename.LastIndexOf(Path.DirectorySeparatorChar) + 1));

            if (markets.UK == Share.theMarket)
            {
                if (null == Share.theProductsBankRatesSecondPeriod || 0 >= Share.theProductsBankRatesSecondPeriod.Length)
                    Share.theProductsBankRatesSecondPeriod = LoadRatesCSVFile(secondPeriodBankFilename);
                if (null == Share.theProductsBankRatesSecondPeriod || 0 >= Share.theProductsBankRatesSecondPeriod.Length)
                {
                    WindowsUtilities.loggerMethod("ERROR theProductsBankRatesSecondPeriod failed to load.");
                }
                else
                {
                    WindowsUtilities.loggerMethod("NOTICE theProductsBankRatesSecondPeriod succesfully load: " + Share.theProductsBankRatesSecondPeriod.Length + " entries from file: " +
                        filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                }
            }

            return (null != Share.theBankRates && 0 < Share.theBankRates.Length);
        }

   
        // optimization: use the combination and products as numbers
        // the forula for geting the rate:
        //      Product-index * MiscConstants.NumberOfProfiles * MiscConstants.NumberOfYearsFrProduct +
        //      profile * MiscConstants.NumberOfYearsFrProduct + year
        public double GetBorrowerRate(/*RatesKey key*/int productID, int profile, int index)
        {
            //Console.WriteLine("--- FindRateForKeyAsNumber key: " + key.ToString() + ",index: " + index);
            Interlocked.Add(ref Share.RateCounter, 1);
            //env.RateCounter++;

            double result = MiscConstants.UNDEFINED_DOUBLE;
            if (null != Share.theProductsRates)
            {
                int indexInRatesArray = CalculateIndexInTable(productID, profile) + index;
                if (Share.theProductsRates.Length > indexInRatesArray)
                {
                    result = Share.theProductsRates[indexInRatesArray];
                }
                else
                {
                    Console.WriteLine("ERROR: FindRateForKeyAsNumber illegal indexInRatesArray: " + indexInRatesArray
                        + " while theProductsRates include: " + Share.theProductsRates.Length + ", productID: " + GenericProduct.GetProductName(productID) + ", profile: " + profile);
                }

                if (0 > result)
                {
                    WindowsUtilities.loggerMethod("ERROR: FindRateForKeyAsNumber illegal rate for productID: " + productID + ", " + GenericProduct.GetProductName(productID) + " and index: " + index + ", indexInRatesArray: " + indexInRatesArray);
                    // TBD
                    result = 0.015;
                }
            }
            else
            {
                Console.WriteLine("ERROR: FindRateForKeyAsNumber Share.theProductsRates is NULL.");
            }
            //Console.WriteLine("--- FindRateForKeyAsNumber key: " + productID.ToString() + ", profile: " 
            //    + profile + ", index: " + index + ", result: " + result);
            return result;
        }

        


        // optimization: use the combination and products as numbers
        // the forula for geting the rate:
        //      Product-index * MiscConstants.NumberOfProfiles * MiscConstants.NumberOfYearsFrProduct +
        //      profile * MiscConstants.NumberOfYearsFrProduct + year
        public double GetBorrowerRateSecondPeriod(int productID, int profile, int index)
        {
            double result = MiscConstants.UNDEFINED_DOUBLE;
            if (markets.UK == Share.theMarket)
            {
                if (null == Share.theProductsRatesSecondPeriod || 0 >= Share.theProductsRatesSecondPeriod.Length)
                    return result;

                int indexInRatesArray = CalculateIndexInTable(productID, profile) + index;
                if (Share.theProductsRatesSecondPeriod.Length > indexInRatesArray)
                {
                    result = Share.theProductsRatesSecondPeriod[indexInRatesArray];
                }
                else
                {
                    Console.WriteLine("ERROR: GetBorrowerRateSecondPeriod illegal indexInRatesArray: " + indexInRatesArray
                        + " while theProductsRates include: " + Share.theProductsRates.Length + ", productID: " + GenericProduct.GetProductName(productID) + ", profile: " + profile);
                }

                if (0 > result)
                {
                    WindowsUtilities.loggerMethod("ERROR: GetBorrowerRateSecondPeriod illegal rate for productID: " + productID + ", " + GenericProduct.GetProductName(productID) + " and index: " + index + ", indexInRatesArray: " + indexInRatesArray);
                    // TBD
                    result = 0.015;
                }
            }
            return result;
        }

        private string[] GetProductsFromCombinations()
        {
            // load only the needed rates
            HashSet<string> products = new HashSet<string>();
            int numOfOptions2Check = Share.numberOfOption; //Enum.GetValues(typeof(options)).Length;
            string[,] combinations = Combinations.GetCombination(Share.theMarket);
            int combinationsUpperBound = combinations.GetUpperBound(0);
            for (int i = 0; i <= combinationsUpperBound; i++)
            {
                for (int o = 0; o < numOfOptions2Check; o++)
                {
                    products.Add(combinations[i, o].Trim());
                }
            }

            return products.ToArray();
        }
        

        

        private double[] LoadRatesCSVFile(string filename)
        {
            string ext = Path.GetExtension(filename);
            string[] lines = null;

            if (".xls" == ext || ".xlsx" == ext)
            {
                lines = ExcelUtilities.GetLinesFromFile(filename, false /*shouldRemoveFractions*/);
            }
            else if (".csv" == ext)
            {
                lines = CSVUtilities.GetLinesFromFile(filename);
            }
            if (null == lines || 0 >= lines.Length)
            {
                Console.WriteLine("ERROR: LoadRatesFile failed to load from file: " + filename);
                return null;
            }

            //string[] uniqueProducts4market = GetProductsFromCombinations();
            string[] uniqueProducts4market = Share.theProductsNames;

            // allocate thre entire rates array
            double[] ratesArray = new double[uniqueProducts4market.Length * 
                MiscConstants.NumberOfProfiles * MiscConstants.NumberOfYearsFrProduct];
       
            //Dictionary < RatesKey, RateLine> dic = new Dictionary<RatesKey, RateLine>();
            string curr = MiscConstants.UNDEFINED_STRING;
            string[] entities;
            double[] currentRates = new double[MiscConstants.NumberOfYearsFrProduct];

            string line;
            int lineNumber = 1;
            int currentIndex = 0;

            for (int li = 0; li < lines.Length; li++)
            {
                curr = line = lines[li];
                if (String.IsNullOrEmpty(line))
                    continue;

                // skip the first line
                // TBD: should read the headers and relate to it
                if (1 == lineNumber++)
                    continue;

                entities = line.Split(MiscConstants.COMMA);
                if (String.IsNullOrEmpty(entities[0]))
                {
                    continue;
                }

                int productIndex = Array.IndexOf(uniqueProducts4market, entities[0].Trim());
                if (0 > productIndex)
                {
                    continue;
                }
                        
                // ensure the line correctness. 2 are the product name and the profile
                if (MiscConstants.NumberOfYearsFrProduct + 2 != entities.Length)
                {
                    continue;
                }

                int userProfile = Int32.Parse(entities[1]);
                int indexInRatesArray = CalculateIndexInTable(productIndex, userProfile);

                // clean all redundant chars e.g. %
                for (int i = 2, j = 0; i < entities.Length; i++, j++)
                {
                    int index = entities[i].IndexOf(MiscConstants.PERCANTAGE_STR);
                    string trimed = (0 < index) ? entities[i].Remove(index).Trim() : entities[i].Trim();
                    entities[i] = trimed;
                    currentRates[j] = Double.Parse(trimed);
                }

                // TBD: should define properly the index with no hard coding...
                // the first column is the ProductID
                // the second column is the user profile
                // and than 27 entries according to the years from 4 to 30
                //currentRates.CopyTo(ratesArray, currentIndex);
                // should copy to the right place according to the product index
                currentRates.CopyTo(ratesArray, indexInRatesArray);
                currentIndex += MiscConstants.NumberOfYearsFrProduct;
                
                //if ("FixedNoTsamudIsrael" == entities[0].Trim() && 5 == userProfile)
                //    Console.WriteLine("LoadRates from file: " + filename + " for productID: " + entities[0].Trim() + ", productIndex: " + productIndex 
                //        + ", userProfile: " + userProfile + ", indexInRatesArray: " + indexInRatesArray + " store rates: " + MiscUtilities.GetArray(currentRates));
            }
            
            return ratesArray;
        }

        private static int CalculateIndexInTable(int productID, int profile)
        {
            return productID * MiscConstants.NumberOfProfiles
                 * MiscConstants.NumberOfYearsFrProduct +
                 (profile - 1) * MiscConstants.NumberOfYearsFrProduct;
        }

        /// <summary>
        /// ///////////
        /// Bank rates
        /// <returns></returns>
 
        public double GetBankRate(int productID, int profile, int index)
        {
            //return MiscConstants.BANK_RATE;

            double result = MiscConstants.UNDEFINED_DOUBLE;
            int indexInRatesArray = CalculateIndexInTable(productID, profile) + index;
            if (null != Share.theBankRates)
            {
                if (Share.theBankRates.Length > indexInRatesArray)
                {
                    result = Share.theBankRates[indexInRatesArray];
                }
                else
                {
                    Console.WriteLine("ERROR: GetBankRate illegal indexInRatesArray: " + indexInRatesArray + " while theProductsRates include: " + Share.theProductsRates.Length);
                }

                // allow negative bank rate
                //if (0 > result)
                //{
                //    Console.WriteLine("ERROR: GetBankRate illegal rate for key: " + productID + ", " + GenericProduct.GetProductName(productID) + " and index: " + index + ", indexInRatesArray: " + indexInRatesArray);
                //    result = MiscConstants.BANK_RATE;
                //}

                //Console.WriteLine("--- GetBankRate productID: " + productID.ToString() + ", profile: "
                //    + profile.ToString() + ", index: " + index + ", result: " + result);
            }
            else
            {
                Console.WriteLine("ERROR: GetBankRate Share.theBankRates is NULL.");
            }

            return result;
        }

        public double GetBankRateSecondPeriod(int productID, int profile, int index)
        {
            //return MiscConstants.BANK_RATE;

            double result = MiscConstants.UNDEFINED_DOUBLE;
            if (markets.UK == Share.theMarket)
            {
                int indexInRatesArray = CalculateIndexInTable(productID, profile) + index;
                if (Share.theProductsBankRatesSecondPeriod.Length > indexInRatesArray)
                {
                    result = Share.theProductsBankRatesSecondPeriod[indexInRatesArray];
                }
                else
                {
                    Console.WriteLine("ERROR: GetBankRateSecondPeriod illegal indexInRatesArray: " + indexInRatesArray + " while theProductsRates include: " + Share.theProductsRates.Length);
                }
            }
            
            return result;
        }

        

    }
}
