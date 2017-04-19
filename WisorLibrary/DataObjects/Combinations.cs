using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;

namespace WisorLibrary.DataObjects
{
    public class Combinations
    {
        string[,] allCombination; // { { } };

        // make the class singltone
        private static Combinations instance;

        public string[,] GetCombination(markets market)
        {
            return Instance.allCombination;
        }

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
            allCombination = null;

            if (String.IsNullOrEmpty(filename))
            {
                filename = MiscUtilities.GetCombinationsFilename();
            }

            if (!String.IsNullOrEmpty(filename))
            {
                string[] comb = LoadCombinationsFileData(filename);
                if (null == comb || 0 >= comb.Length)
                {
                    WindowsUtilities.loggerMethod("ERROR Combinations failed to load from file: " + filename);
                    Console.WriteLine("ERROR Combinations failed to load from file: " + filename);
                }
                else
                {
                    string[] oneCombination = new string[MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION];

                    allCombination = new string[comb.Length, MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION]; // { { } };
                    string[] entities;
                    string line = null;

                    for (int i = 0; i < comb.Length; i++)
                    {
                        line = comb[i];
                        if (String.IsNullOrEmpty(line))
                            continue;

                        entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);
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
                Console.WriteLine("ERROR Combinations no file selected to load from.");
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

            return lines;
        }



    }
}
