using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;
using static WisorLib.Options;

namespace WisorLibrary.DataObjects
{
    public class Combinations
    {
        string[,] allCombination; // { { } };

        // make the class singltone
        private static Combinations instance;

  
        public static bool SetFilename(string filename)
        {
            bool rc = false;

            if (null == instance)
            {
                instance = new Combinations(filename);
                if (null != instance)
                    rc = true;
            }
            if (null != instance)
                rc = true;
            return rc;
         }

        public static Combinations Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new Combinations();
                }
                return instance;
            }
        }

        Combinations(string filename = "")
        {
            //allCombination = null;

            if (Share.ShouldCreateCombinationDynamickly)
            {
                // no more static combination file but rather building the combination dynamickly from the beneficial products
                allCombination = CreateCombination();
            }
            else
            {

                if (String.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    filename = Combinations.GetCombinationsFilename();
                }

                if (!String.IsNullOrEmpty(filename))
                {
                    string[] comb = LoadCombinationsFileData(filename);
                    if (null == comb || 0 >= comb.Length)
                    {
                        WindowsUtilities.loggerMethod("ERROR Combinations failed to load from file: " + filename);
                        //Console.WriteLine("ERROR Combinations failed to load from file: " + filename);
                    }
                    else
                    {
                        //string[] oneCombination = new string[MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION];

                        allCombination = new string[comb.Length, MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION]; // { { } };
                        string[] entities;
                        string line = null;

                        for (int i = 0; i < comb.Length; i++)
                        {
                            line = comb[i];
                            if (String.IsNullOrEmpty(line))
                                continue;

                            entities = line.Split(MiscConstants.COMMA);
                            for (int j = 0; j < MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION; j++)
                            {
                                allCombination[i, j] = entities[j].Trim();
                            }
                        }
                        //Share.combinations = allCombination;
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("ERROR Combinations no file selected to load from.");
                }
            }
        }

        private string[] LoadCombinationsFileData(string filename)
        {
            uint lineElemntsNum = MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION;
            string[] lines = null;
 
            try
            {
                if (File.Exists(filename))
                {
                    string ext = Path.GetExtension(filename);

                    if (".xls" == ext || ".xlsx" == ext)
                    {
                        lines = ExcelUtilities.GetLinesFromFile(filename, false /*shouldRemoveFractions*/);
                    }
                    else if (".csv" == ext)
                    {
                        lines = CSVUtilities.GetLinesFromFile(filename);
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadCombinationsFileData file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadCombinationsFileData got Exception: " + e.ToString()/* + ". line: " + line*/);
            }

            if (null == lines || 0 >= lines.Length)
            {
                WindowsUtilities.loggerMethod("ERROR Combinations failed to load from file: " + 
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE successfully load: " + lines.Length + " Combinations from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }

            return lines;
        }

        public static bool SetCombinationsFilename()
        {
            string filename = MiscUtilities.GetFilename(Share.CombinationFileName, MiscConstants.COMBINATIONS_FILE);
            bool rc = Combinations.SetFilename(filename);

            if (rc)
            // number all the combination 
                rc = ConvertProductsNaming();
            WindowsUtilities.loggerMethod("NOTICE SetCombinationsFilename filename: " + filename + ", combinations4market.Length: " + Share.combinations4market.Length);

            return rc;
        }

        public static string GetCombinationsFilename()
        {
            //string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            //string filename = dir + MiscConstants.COMBINATIONS_FILE;
            string filename = MiscUtilities.GetFilename(Share.CombinationFileName, MiscConstants.COMBINATIONS_FILE);

            return filename;
        }

   
        public static string[,] GetCombination(markets market)
        {
            return Instance.allCombination;
        }


        /*
        * choosing k objects from a set of n objects is given by: (n above k) = n! / k! * (n−k)!
        * Share.theLoadedProducts contain only the valid beneficial products
        */
        public static string[,] CreateCombination(/*markets market*/)
        {
            string[,] combination = null;       // { { } };
            List<string> products = new List<string>();
            List<string> mustProducts = new List<string>();
            int numOfCombination = Enum.GetNames(typeof(options)).Length;
            if (null != Share.theLoadedProducts && 0 < Share.theLoadedProducts.Count)
            {
                foreach (KeyValuePair<int, GenericProduct> p in Share.theLoadedProducts)
                {
                    // Check if some products must be in the combination
                    if (p.Value.mustBeUsed)
                    {
                        mustProducts.Add(p.Value.productID.stringTypeId);
                    }
                    else if (p.Value.shouldConsider)
                    {
                        products.Add(p.Value.productID.stringTypeId);
                    }
                }

                // create the all combinations
                combination = CalculateCombination(mustProducts, products, numOfCombination);
            }
            else
                WindowsUtilities.loggerMethod("CreateCombination: Share.theLoadedProducts in null!!! ");

            //// print the combinations
            WindowsUtilities.loggerMethod("CreateCombination: out of: " + Share.theLoadedProducts.Count +
                " loaded products, #of combinations is: " + (1+combination.GetUpperBound(0)).ToString() + ", combinations: ");
            for (int i = 0; i <= combination.GetUpperBound(0); i++)
            {
                string s = "comp " + (i + 1).ToString() + ": ";

                for (int o = 0; o <= combination.GetUpperBound(1); o++)
                {
                    s += ", " + combination[i, o];
                }
                WindowsUtilities.loggerMethod(s);
            }

            return combination;
        }

        // calculate the valid combination of 3 products from the entire valid product's list
        private static string[,] CalculateCombination(List<string> mustProducts, List<string> products, int numberOfCombinations)
        {
            // Generate the selections.
            List<List<string>> results = GenerateSelections<string>(products, numberOfCombinations - mustProducts.Count);
            WindowsUtilities.loggerMethod("CalculateCombination found: " + results.Count + " combinmations out of #must: " +
                mustProducts.Count + " and #products: " + products.Count);

            string[] mustProductsArray = mustProducts.ToArray();

            // Display the results.
            //string comstr;
            //foreach (List<string> combination in results)
            //{
            //    comstr = string.Join(" ", combination.ToArray());
            //    Console.WriteLine("Found comstr: " + comstr + "\n");
            //}

            // Calculate the number of items.
            //decimal num_combinations = NChooseK(items.Length, n);
            //Console.WriteLine("Found for N: " + items.Length + " and K: " + n + " num_combinations: " + num_combinations + "\n");

            string[,] combinations = null;  // { { } };
            if (null != results)
            {
                int numberOfCom = Math.Min((int) Share.MaxCombinationNumber, results.Count);
                combinations = new string[numberOfCom, Enum.GetNames(typeof(options)).Length];

                WindowsUtilities.loggerMethod("NOTICE CalculateCombination found: " + results.Count + " combinations while the configuration limit is: "
                    + Share.MaxCombinationNumber);
                int i = 0;
                foreach (List<string> c in results)
                {
                    // should the number of combination be limited
                    if (Share.MaxCombinationNumber <= i)
                    {
                        WindowsUtilities.loggerMethod("NOTICE CalculateCombination reach the maximum limitation of: " + Share.MaxCombinationNumber);
                        break;
                    }
                    
                    string[] oneCombination = new string[MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION];
                    oneCombination = c.ToArray();
                    // TBD - blockcopy or array copy
                    // copy first the must products
                    int m;
                    for (m = 0; m < mustProducts.Count; m++)
                    {
                        combinations[i, m] = mustProductsArray[m];
                    }
                    // copy the rest yield combinations
                    for (int r = 0; m < numberOfCombinations; m++, r++)
                    {
                        combinations[i, m] = oneCombination[r]; 
                    }
                    //combinations[i, 0] = oneCombination[0];
                    //combinations[i, 1] = oneCombination[1];
                    //combinations[i, 2] = oneCombination[2];
                    i++;

                   //Buffer.BlockCopy(oneCombination, 0, combinations, oneCombination.Length * i++, oneCombination.Length);
                    //combinations.CopyTo(oneCombination, i++);
                    //comstr = string.Join(" ", combination.ToArray());
                    //Console.WriteLine("Found comstr: " + comstr + "\n");
                }
                
            }
            return combinations;
        }

        // Generate selections of n items.
        private static List<List<T>> GenerateSelections<T>(List<T> items, int n)
        {
            // Make an array to tell whether
            // an item is in the current selection.
            bool[] in_selection = new bool[items.Count];

            // Make a result list.
            List<List<T>> results = new List<List<T>>();

            // Build the combinations recursively.
            SelectItems<T>(items, in_selection, results, n, 0);

            // Return the results.
            return results;
        }

        // Recursively select n additional items with indexes >= first_item.
        // If n == 0, add the current combination to the results.
        private static void SelectItems<T>(List<T> items, bool[] in_selection,
            List<List<T>> results, int n, int first_item)
        {
            if (n == 0)
            {
                // Add the current selection to the results.
                List<T> selection = new List<T>();
                for (int i = 0; i < items.Count; i++)
                {
                    // If this item is selected, add it to the selection.
                    if (in_selection[i]) selection.Add(items[i]);
                }
                results.Add(selection);
                //Console.WriteLine("Found n == 0, items: " + PrintListChar(items) + ", bools: " + PrintArrayChar(in_selection) +
                //    ", first: " + first_item + ", n: " + n + ", selection: " + PrintListChar(selection) + "\n");
            }
            else
            {
                //Console.WriteLine("Found n != 0, items: " + PrintListChar(items) + ", bools: " + PrintArrayChar(in_selection) + ", first: " + first_item + ", n: " + n);

                // Try adding each of the remaining items.
                for (int i = first_item; i < items.Count; i++)
                {
                    // Try adding this item.
                    in_selection[i] = true;

                    // Recursively add the rest of the required items.
                    SelectItems(items, in_selection, results, n - 1, i + 1);

                    // Remove this item from the selection.
                    in_selection[i] = false;
                }
            }
        }

        // Return N choose K calculated directly.
        // For a description of the algorithm, see:
        //      http://csharphelper.com/blog/2014/08/calculate-the-binomial-coefficient-n-choose-k-efficiently-in-c/
        public static decimal NChooseK(decimal N, decimal K)
        {
            decimal result = 1;
            for (int i = 1; i <= K; i++)
            {
                result *= N - (K - i);
                result /= i;
            }
            return result;
        }

        private static bool ConvertProductsNaming()
        {
            // the index of the product is set now for the entire run, instead of the name (string)
            //string[] productNames = products.ToArray();
            int index = MiscConstants.UNDEFINED_INT;
            string[,] comb = GetCombination(Share.theMarket);
            bool rc = false;
            if (null != comb)
            {
                int[,] combinationsAsNumbers = new int[
                    GetCombination(Share.theMarket).GetUpperBound(0) + 1,
                    GetCombination(Share.theMarket).GetUpperBound(1) + 1];
                string[,] combinationsAsString = new string[
                    GetCombination(Share.theMarket).GetUpperBound(0) + 1,
                    GetCombination(Share.theMarket).GetUpperBound(1) + 1];

                for (int i = 0; i <= GetCombination(Share.theMarket).GetUpperBound(0); i++)
                {
                    for (int o = 0; o <= GetCombination(Share.theMarket).GetUpperBound(1); o++)
                    {
                        //string item = GetCombination(Share.theMarket)[i, o];
                        //int ind = Array.IndexOf(Share.theProductsNames, item);
                        index = Array.IndexOf(Share.theProductsNames, GetCombination(Share.theMarket)[i, o]);
                        if (0 > index)
                        {
                            WindowsUtilities.loggerMethod("ERROR Combinations::ConvertProductsNaming un-defined product: " + GetCombination(Share.theMarket)[i, o]);
                        }
                        combinationsAsNumbers[i, o] = index;
                        combinationsAsString[i, o] = GetCombination(Share.theMarket)[i, o];
                    }
                }
                Share.combinations4market = combinationsAsNumbers;
                Share.combinationsAsString = combinationsAsString;
                rc = true;
            }
            else
            {
                WindowsUtilities.loggerMethod("ERROR Combinations::ConvertProductsNaming no combination were loaded.");
            }
            //Share.theProductsNames = productNames;
            return rc;
        }

    }
}
