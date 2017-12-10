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

        CombinationsPerAmount[] allCombinationsPerAmount;

        // make the class singltone
        private static Combinations instance;

  
        public static bool SetFilename(string filename)
        {
            bool rc = false;

            if (null == instance)
            {
                instance = new Combinations(filename);
            }
            if (null != instance && (null != instance.allCombination || null != instance.allCombinationsPerAmount))
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
            set
            {
                // clear the previous array and ask to reload from different file
                instance = null;
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

                int fromLineNumber = 0;
                string[] entities;
                int fromAmount, toAmount, numberOfEntires, startLine;
                double fromLTV, toLTV;
                bool genericCombination;

                if (!String.IsNullOrEmpty(filename))
                {
                    string[] comb = LoadCombinationsFileData(filename, false /*skipTheHeader*/);
                    if (null == comb || 0 >= comb.Length)
                    {
                        WindowsUtilities.loggerMethod("ERROR Combinations failed to load from file: " + filename);
                        return;
                        //Console.WriteLine("ERROR Combinations failed to load from file: " + filename);
                    }
                    else
                    {
                        // TBD - now we have also combination per amount...
                        // so check if there are generic combination or only per amount

                        fromLineNumber = GetAmountSeperationBlockIndex(comb, fromLineNumber, out fromAmount, out toAmount,
                            out fromLTV, out toLTV, out numberOfEntires, out genericCombination, out startLine);

                        if (genericCombination)
                        {
                            allCombination = new string[/*comb.Length*/ numberOfEntires, MiscUtilities.GetNumberOfProductsInCombination()];
                            string line = null;
                            int counter = 0;

                            // for (int i = startLine; i < numberOfEntires; i++)
                            while (counter < numberOfEntires)
                            {
                                line = comb[startLine++];
                                if (String.IsNullOrEmpty(line))
                                    continue;

                                entities = line.Split(MiscConstants.COMMA);
                                if (String.IsNullOrEmpty(entities[0]))
                                    continue;

                                // skip the header
                                if (MiscConstants.COMBINATION_PER_AMOUNT == entities[0] || MiscConstants.GENERIC_COMBINATION == entities[0])
                                    continue;

                                for (int j = 0; j < MiscUtilities.GetNumberOfProductsInCombination(); j++)
                                {
                                    allCombination[counter, j] = entities[j].Trim();
                                }
                                counter++;
                            }
                        }
                        else
                        {
                            // read the specific combination from the start
                            fromLineNumber = 0;
                        }
                    }

                    int amountSeperationBlockIndex = 0;
                    HashSet<string> combinationsAsUniqueStringList = new HashSet<string>(); 
                    List<CombinationsPerAmount> listOfCombinationsPerAmount = new List<CombinationsPerAmount>();
                    // load the per amount combination
                    while (comb.Length > amountSeperationBlockIndex) {
                        
                        amountSeperationBlockIndex = GetAmountSeperationBlockIndex(comb, fromLineNumber, out fromAmount, out toAmount,
                            out fromLTV, out toLTV, out numberOfEntires, out genericCombination, out startLine);
                        if (0 >= numberOfEntires)
                            break;

                        CombinationsPerAmount combinationsPerAmount = new CombinationsPerAmount(fromAmount, toAmount, fromLTV, toLTV);
                        string[,] currentCombinations = new string[numberOfEntires , MiscUtilities.GetNumberOfProductsInCombination()];
                        int counter = 0;

                        // for (int i = 0; i < numberOfEntires; i++)
                        while (counter < numberOfEntires)
                        {
                            if (fromLineNumber >= comb.Length)
                                break;

                            string line = comb[startLine++];
                            if (String.IsNullOrEmpty(line))
                                continue;

                            entities = line.Split(MiscConstants.COMMA);
                            if (String.IsNullOrEmpty(entities[0]))
                                continue;
                          
                            // skip the header
                            if (MiscConstants.COMBINATION_PER_AMOUNT == entities[0] || MiscConstants.GENERIC_COMBINATION == entities[0])
                                continue;

                            if (entities.Length != MiscUtilities.GetNumberOfProductsInCombination())
                            {
                                WindowsUtilities.loggerMethod("ERROR Combinations file with: " + entities.Length + " entries while NumberOfProductsInCombination: " + MiscUtilities.GetNumberOfProductsInCombination());
                                continue;
                            }

                            for (int j = 0; j < MiscUtilities.GetNumberOfProductsInCombination(); j++)
                            {
                                currentCombinations[counter, j] = entities[j].Trim();
                                combinationsAsUniqueStringList.Add(currentCombinations[counter, j]);
                            }
                            counter++;
                        }

                        combinationsPerAmount.SetCombinationsPerAmount(currentCombinations);

                        // check that this combination not already exists
                        listOfCombinationsPerAmount.Add(combinationsPerAmount);

                        fromLineNumber = amountSeperationBlockIndex; // get the next block
                    }

                    if (null != listOfCombinationsPerAmount && 0 < listOfCombinationsPerAmount.Capacity)
                    {
                        allCombinationsPerAmount = listOfCombinationsPerAmount.ToArray();
                        Share.combinationsAsUniqueString = combinationsAsUniqueStringList.ToArray();
                    }
                    else
                        allCombinationsPerAmount = null;

                }
                else
                {
                    WindowsUtilities.loggerMethod("ERROR Combinations no file selected to load from.");
                }
            }
        }

        private string[] LoadCombinationsFileData(string filename, bool skipTheHeader = true)
        {
            int lineElemntsNum = MiscUtilities.GetNumberOfProductsInCombination();
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
                        lines = CSVUtilities.GetLinesFromFile(filename, skipTheHeader);
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
            string filename = GetCombinationsFilename();

            bool rc = Combinations.SetFilename(filename);

            if (rc)
            {
                // number all the combination 
                rc = ConvertProductsNaming();
                WindowsUtilities.loggerMethod("NOTICE SetCombinationsFilename filename: " + filename); //  + ", combinations4market.Length: " + Share.combinations4market.Length);
            }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE ERROR SetCombinationsFilename filename: " + filename + ", no composition was found");
            }
            return rc;
        }

        public static string GetCombinationsFilename()
        {
            //string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            //string filename = dir + MiscConstants.COMBINATIONS_FILE;
            string filename;
            if (MiscUtilities.Use3ProductsInComposition())
                filename = MiscUtilities.GetFilename(Share.CombinationFileName, MiscConstants.UNDEFINED_STRING /*MiscConstants.COMBINATION_FILENAME*/);
            else
                filename = MiscUtilities.GetFilename(Share.Products2InCombinationFileName, MiscConstants.UNDEFINED_STRING /*MiscConstants.COMBINATION_FILENAME_2_PRODUCTS_IN_COMBINATION*/);

            return filename;
        }


        public static string[,] GetCombination(markets market)
        {
            return Instance.allCombination;
        }

        // TBD - should make it optimized for performance
        public static string[,] GetCombinationByAmount(markets market, uint amount, double LTV)
        {
            int i;
            string[,] ret = null;

            if (null != Instance && null != Instance.allCombinationsPerAmount)
            {
                for (i = 0; i < Instance.allCombinationsPerAmount.Length; i++)
                {
                    if (amount >= Instance.allCombinationsPerAmount[i].fromAmount && amount <= Instance.allCombinationsPerAmount[i].toAmount &&
                        LTV >= Instance.allCombinationsPerAmount[i].fromLTV && LTV <= Instance.allCombinationsPerAmount[i].toLTV)
                        break;
                }
                if (i < Instance.allCombinationsPerAmount.Length)
                    ret = Instance.allCombinationsPerAmount[i].combinationPerAmount;
            }
            
            // to be on the safe side...
            if (null == ret)
            {
                WindowsUtilities.loggerMethod("NOTICE GetCombinationByAmount (UK) : didn't find entry for amount: " + amount + ". Set the generic combination instead.");
                if (null != Instance && null != Instance.allCombination)
                {
                    ret = Instance.allCombination;
                }
            }
            return ret;
        }

        public static int[,] GetCombinationByAmountI(markets market, uint amount, double LTV)
        {
            int i;
            int[,] ret = null;

            if (null != Instance && null != Instance.allCombinationsPerAmount)
            {
                for (i = 0; i < Instance.allCombinationsPerAmount.Length; i++)
                {
                    if (amount >= Instance.allCombinationsPerAmount[i].fromAmount && amount <= Instance.allCombinationsPerAmount[i].toAmount &&
                        LTV >= Instance.allCombinationsPerAmount[i].fromLTV && LTV <= Instance.allCombinationsPerAmount[i].toLTV)
                        break;
                }
                if (i < Instance.allCombinationsPerAmount.Length)
                    ret = Instance.allCombinationsPerAmount[i].combinationPerAmountI;
            }


            // to be on the safe side...
            if (null == ret)
            {
                WindowsUtilities.loggerMethod("NOTICE GetCombinationByAmount (UK) : didn't find entry for amount: " + amount + ". Set the generic combination instead.");
                if (null != Instance && null != Instance.allCombination)
                {
                    ret = Share.combinations4market;
                }
            }

            return ret;
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
                    
                    string[] oneCombination = new string[MiscUtilities.GetNumberOfProductsInCombination()];
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
                    comb.GetUpperBound(0) + 1,
                    comb.GetUpperBound(1) + 1];
                string[,] combinationsAsString = new string[
                    comb.GetUpperBound(0) + 1,
                    comb.GetUpperBound(1) + 1];
                List<string> combinationsAsUniqueStringList = new List<string>();

                for (int i = 0; i <= comb.GetUpperBound(0); i++)
                {
                    for (int o = 0; o <= comb.GetUpperBound(1); o++)
                    {
                        //string item = GetCombination(Share.theMarket)[i, o];
                        //int ind = Array.IndexOf(Share.theProductsNames, item);
                        index = Array.IndexOf(Share.theProductsNames, comb[i, o]);
                        if (0 > index)
                        {
                            WindowsUtilities.loggerMethod("ERROR Combinations::ConvertProductsNaming un-defined product: " + comb[i, o]);
                        }
                        combinationsAsNumbers[i, o] = index;
                        combinationsAsString[i, o] = comb[i, o];
                        combinationsAsUniqueStringList.Add(comb[i, o]);
                    }
                }
                Share.combinations4market = combinationsAsNumbers;
                Share.combinationsAsString = combinationsAsString;
                Share.combinationsAsUniqueString = combinationsAsUniqueStringList.ToArray();
                rc = true;
            }
            // maybe there are combination by amount only
            else if (null != Instance.allCombinationsPerAmount)
            {
                rc = true;
            }
            else
            {
                WindowsUtilities.loggerMethod("ERROR Combinations::ConvertProductsNaming no combination were loaded.");
            }
            //Share.theProductsNames = productNames;
            return rc;
        }

        int GetAmountSeperationBlockIndex(string[] combLines, int fromLineNumber, out int fromAmount, out int toAmount,
            out double fromLTV, out double toLTV, out int numberOfEntires, out bool genericCombination, out int startLine)
        {
            int i;
            string[] entities;
            fromAmount = toAmount = numberOfEntires = (int) MiscConstants.UNDEFINED_UINT;
            fromLTV = toLTV = MiscConstants.UNDEFINED_DOUBLE;
            genericCombination = false;
            bool findStart = false, findStartGeneric = false;
            startLine = 0;

            for (i = fromLineNumber; i < combLines.Length; i++)
            {
                if (!String.IsNullOrEmpty(combLines[i]))
                {
                    entities = combLines[i].Split(MiscConstants.COMMA);
                    if (!String.IsNullOrEmpty(entities[0]))
                    {
                        if (MiscConstants.COMBINATION_PER_AMOUNT == entities[0] || MiscConstants.GENERIC_COMBINATION == entities[0])
                        {
                            if (!findStart)
                            {
                                genericCombination = MiscConstants.GENERIC_COMBINATION == entities[0];
                                findStart = true;
                                startLine = i+1;
                                if (MiscConstants.GENERIC_COMBINATION == entities[0])
                                    findStartGeneric = true;
                                else // found COMBINATION_PER_AMOUNT
                                {
                                    if (5 == entities.Length)
                                    {
                                        if (!String.IsNullOrEmpty(entities[1]))
                                            fromAmount = Convert.ToInt32(entities[1]);
                                        if (!String.IsNullOrEmpty(entities[2]))
                                            toAmount = Convert.ToInt32(entities[2]);
                                        if (!String.IsNullOrEmpty(entities[3]))
                                            fromLTV = Convert.ToDouble(entities[3]);
                                        if (!String.IsNullOrEmpty(entities[4]))
                                            toLTV = Convert.ToDouble(entities[4]);
                                    }
                                    else
                                    {
                                        WindowsUtilities.loggerMethod("ERROR Combinations::GetAmountSeperationBlockIndex illegal fields number on line: " + combLines[i]);
                                    }
                                }
                            }
                            else // recognize the EOF or the stop
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (findStart)
                                numberOfEntires++;
                        }
                    }
                 
                }
            }

            if (findStartGeneric)
            {
                fromAmount = toAmount = -1;
            }

            // EOF
            if (i != combLines.Length)
                i -= 1; // next time start in this line

             return  i;
        }


    }

    public class CombinationsPerAmount
    {
        public int fromAmount { get; }
        public int toAmount { get; }
        public double fromLTV { get; }
        public double toLTV { get; }
        public string[,] combinationPerAmount { get; set; }
        public int[,] combinationPerAmountI { get; set; }

        public CombinationsPerAmount(int fromAmount, int toAmount, double fromLTV, double toLTV, string[,] combination = null)
        {
            this.fromAmount = fromAmount;
            this.toAmount = toAmount;
            this.fromLTV = fromLTV;
            this.toLTV = toLTV;

            SetCombinationsPerAmount(combination);
        }

        public void SetCombinationsPerAmount(string[,] combination)
        {
            if (null != combination)
            {
                combinationPerAmount = combination;

                combinationPerAmountI = new int[
                    combination.GetUpperBound(0) + 1,
                    combination.GetUpperBound(1) + 1];

                for (int i = 0; i <= combination.GetUpperBound(0); i++)
                {
                    for (int o = 0; o <= combination.GetUpperBound(1); o++)
                    {
                        int index = Array.IndexOf(Share.theProductsNames, combination[i, o]);
                        if (0 > index)
                        {
                            WindowsUtilities.loggerMethod("ERROR Combinations::CombinationsPerAmount un-defined product: " + combination[i, o]);
                        }
                        combinationPerAmountI[i, o] = index;
                    }
                }

            }
       
        }

     
    }
}
