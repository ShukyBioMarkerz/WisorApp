using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using static WisorLib.Options;

namespace WisorLibrary.Logic
{
    class RateUtilities
    {

        // make the class singltone
        private static RateUtilities instance;

        public static RateUtilities SetFilename(string filename)
        {
            //get
            //{
            if (null == instance)
            {
                    instance = new RateUtilities(filename);
                }
                return instance;
            //}
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
        double[] rates;
        string filename;

        private RateUtilities(string filename = "")
        {
            bool rc = false;

            if (!String.IsNullOrEmpty(filename))
            {
                setFilename(filename);
                rc = LoadRates();
            }
            else
                WindowsUtilities.loggerMethod("NOTICE RateUtilities without setting the rates file name");
        }

        private void setFilename(string fn)
        {
            filename = fn;
        }

        public bool LoadRates()
        {
            if (null == rates || 0 >= rates.Length)
                rates = LoadRatesCSVFile(filename);
            WindowsUtilities.loggerMethod("NOTICE LoadRates succesfully load: " + rates.Length + " entries.");
            return (null != rates && 0 < rates.Length);
        }

        //public RateLine FindRatesForKey(RatesKey key)
        //{
        //    RateLine rate;

        //    if (! rates.TryGetValue(key, out rate))
        //        rate = null;
        //    return rate;
        //}

        //public double FindRateForKey(RatesKey key, int index)
        //{
        //    RateLine rate;
        //    double result = MiscConstants.UNDEFINED_DOUBLE;

        //    if (Share.shouldUseCombinations4marketAsNumbers)
        //        return FindRateForKeyAsNumber(key, index);

        //    if (0 < rates.Count)
        //    {
        //        if (!rates.TryGetValue(key, out rate))
        //            rate = null;
        //        if (null != rate)
        //            if (index <= rate.Length())
        //                result = rate[index];
        //    }
        //    else
        //    {
        //        Console.WriteLine("ERROR: FindRateForKey no rates where loaded");
        //        WindowsUtilities.loggerMethod("ERROR: FindRateForKey no rates where loaded");
        //    }

        //    if (0 > result)
        //    {
        //        Console.WriteLine("ERROR: FindRateForKey illegal rate for key: " + key.ToString() + " and index: " + index);
        //        //WindowsUtilities.loggerMethod("ERROR: FindRateForKey illegal rate for key: " + key.ToString() + " and index: " + index);
        //        // TBD
        //        result = 0.015;
        //    }

        //    return result;
        //}

        // optimization: use the combination and products as numbers
        // the forula for geting the rate:
        //      Product-index * MiscConstants.NumberOfProfiles * MiscConstants.NumberOfYearsFrProduct +
        //      profile * MiscConstants.NumberOfYearsFrProduct + year
        public double FindRateForKeyAsNumber(/*RatesKey key*/int productID, int profile, int index)
        {
           //Console.WriteLine("--- FindRateForKeyAsNumber key: " + key.ToString() + ",index: " + index);

           double result = MiscConstants.UNDEFINED_DOUBLE;
            int indexInRatesArray = /*key.*/productID * MiscConstants.NumberOfProfiles
                 * MiscConstants.NumberOfYearsFrProduct +
                 (/*key.*/profile - 1) * MiscConstants.NumberOfYearsFrProduct + index;
            if (Share.theProductsRates.Length > indexInRatesArray)
            {
                result = Share.theProductsRates[indexInRatesArray];
            }
            else
            {
                Console.WriteLine("ERROR: FindRateForKeyAsNumber illegal indexInRatesArray: " + indexInRatesArray + " while theProductsRates include: " + Share.theProductsRates.Length);
                WindowsUtilities.loggerMethod("ERROR: FindRateForKeyAsNumber illegal indexInRatesArray: " + indexInRatesArray + " while theProductsRates include: " + Share.theProductsRates.Length);
            }

            if (0 > result)
            {
                Console.WriteLine("ERROR: FindRateForKeyAsNumber illegal rate for key: " + /*key.*/productID.ToString() + " and index: " + index + ", indexInRatesArray: " + indexInRatesArray);
                //WindowsUtilities.loggerMethod("ERROR: FindRateForKeyAsNumber illegal rate for key: " + key.ToString() + " and index: " + index);
                // TBD
                result = 0.015;
            }

            return result;
        }

        private string[] GetProductsFromCombinations()
        {
            // load only the needed rates
            HashSet<string> products = new HashSet<string>();
            int numOfOptions2Check = Share.numberOfOption; //Enum.GetValues(typeof(options)).Length;
            string[,] combinations = CalculationConstants.GetCombination(Share.theMarket);
            int combinationsUpperBound = combinations.GetUpperBound(0);
            for (int i = 0; i <= combinationsUpperBound; i++)
            {
                for (int o = 0; o < numOfOptions2Check; o++)
                {
                    products.Add(combinations[i, o]);
                }
            }

            return products.ToArray();
        }
        
        private void ConvertProductsNaming()
        {
            // the index of the product is set now for the entire run, instaed of the name (string)
            //string[] productNames = products.ToArray();
            int[,] combinationsAsNumbers = new int[
                CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0) + 1,
                CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(1) + 1];
            string[,] combinationsAsString = new string[
                CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0) + 1,
                CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(1) + 1];

            for (int i = 0; i <= CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0); i++)
            {
                for (int o = 0; o <= CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(1); o++)
                {
                    string item = CalculationConstants.GetCombination(Share.theMarket)[i, o];
                    int ind = Array.IndexOf(Share.theProductsNames, item);
                    combinationsAsNumbers[i, o] = Array.IndexOf(Share.theProductsNames, CalculationConstants.GetCombination(Share.theMarket)[i, o]);
                    combinationsAsString[i, o] = CalculationConstants.GetCombination(Share.theMarket)[i, o];
                }
            }
            Share.combinations4market = combinationsAsNumbers;
            Share.combinationsAsString = combinationsAsString;
            //Share.theProductsNames = productNames;
        }
        

        private double[] /*Dictionary<RatesKey, RateLine>*/ LoadRatesCSVFile(string filename)
        {
            string[] uniqueProducts4market = GetProductsFromCombinations();

            // allocate thre entire rates array
            double[] ratesArray = new double[uniqueProducts4market.Length * 
                MiscConstants.NumberOfProfiles * MiscConstants.NumberOfYearsFrProduct];
       
            //Dictionary < RatesKey, RateLine> dic = new Dictionary<RatesKey, RateLine>();
            string curr = MiscConstants.UNDEFINED_STRING;
            string[] entities;
            double[] currentRates = new double[MiscConstants.NumberOfYearsFrProduct];
            HashSet<string> products = new HashSet<string>();

            try
            {
                if (File.Exists(filename))
                {
                    var file = new FileInfo(filename);
                    StreamReader fileReader = new System.IO.StreamReader(filename);
                    string line;
                    int lineNumber = 1;
                    int currentIndex = 0;
       
                    do
                    {
                        line = fileReader.ReadLine();
                        curr = line;
                        if (String.IsNullOrEmpty(line))
                            continue;

                        // skip the first line
                        // TBD: should read the headers and relate to it
                        if (1 == lineNumber++)
                            continue;

                        entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);
                        if (String.IsNullOrEmpty(entities[0]))
                        {
                            continue;
                        }

                        if (-1 == Array.IndexOf(uniqueProducts4market, entities[0]))
                        {
                            continue;
                        }
                        
                        // ensure the line correctness. 2 are the product name and the profile
                        if (MiscConstants.NumberOfYearsFrProduct + 2 != entities.Length)
                        {
                            continue;
                        }

                        // add the product to the indexed list
                        
                        products.Add(entities[0]);
                        
                        // clean all redundant chars e.g. %
                        for (int i = 2, j = 0; i < entities.Length; i++, j++)
                        {
                            int index = entities[i].IndexOf(MiscConstants.PERCANTAGE_STR);
                            string trimed = (0 < index) ? entities[i].Remove(index) : entities[i];
                            entities[i] = trimed;
                            currentRates[j] = Double.Parse(trimed);
                        }

                        // TBD: should define properly the index with no hard coding...
                        // the first column is the ProductID
                        // the second column is the user profile
                        // and than 27 entries according to the years from 4 to 30
                        currentRates.CopyTo(ratesArray, currentIndex);
                        currentIndex += MiscConstants.NumberOfYearsFrProduct;

                        //RatesKey ratesKey = new RatesKey(entities[0], Convert.ToInt32(entities[1]));
                        //dic.Add(ratesKey, new RateLine(entities, 2 /*startingIndex*/));
                    }
                    while (! System.String.IsNullOrEmpty(line));

                }
                else
                {
                    Console.WriteLine("ERROR: LoadRatesFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: LoadRatesFile got Exception: " + e.ToString() + ". Curr: " + curr);
            }

            //return dic;
            string[] productNames = products.ToArray();
            Share.theProductsNames = productNames;
            ConvertProductsNaming();
            Share.theProductsRates = ratesArray;
            return ratesArray;
        }

    }
}
