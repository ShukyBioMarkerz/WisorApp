using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.DataObjects
{
    public class RiskLiquidityObject
    {
        string[] productsArray;
        int  productsIndex;
        int lineElemntsNum;


        // make the class singltone
        private static RiskLiquidityObject instance;

        public static bool SetFilename(string filename)
        {
            bool rc = false;

            if (null == instance)
            {
                instance = new RiskLiquidityObject(filename);
                if (null != instance)
                    rc = true;
            }
            if (null != instance)
                rc = true;

            return rc;
       }

        public static RiskLiquidityObject Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new RiskLiquidityObject();
                }
                return instance;
            }
        }

        RiskLiquidityObject(string filename = "")
        {
            lineElemntsNum = (Enum.GetNames(typeof(Risk)).Length - 1) * 2;

            if (!String.IsNullOrEmpty(filename) && null == Share.riskAndLiquidity)
            {
                double[] riskAndLiquidity = LoadRiskLiquidityFile(filename);
                if (null == riskAndLiquidity || 0 >= riskAndLiquidity.Length)
                {
                    WindowsUtilities.loggerMethod("ERROR RiskLiquidityObject failed to load from file: " + filename);
                 }
                else
                {
                    Share.riskAndLiquidity = riskAndLiquidity;
                }
            }
        }

        private string[] LoadRiskLiquidityFileData(string filename)
        {
            string[] lines = null;
            productsArray = new string[Share.theLoadedProducts.Count];

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
                    WindowsUtilities.loggerMethod("LoadRiskLiquidityFileData file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadRiskLiquidityFileData got Exception: " + e.ToString()/* + ". line: " + line*/);
            }

            return lines;
        }

        int GetProductIndex(string productName)
        {
            return Array.IndexOf(productsArray, productName);
        }

        double[] LoadRiskLiquidityFile(string filename)
        {
             // allocate thre entire rates array
            double[] riskLiquidityArray = new double[Share.theLoadedProducts.Count *
                (Enum.GetNames(typeof(Risk)).Length - 1) *          // NONE
                (Enum.GetNames(typeof(Liquidity)).Length - 1) *     // NONE
                2 /*min, max*/];

            string line = MiscConstants.UNDEFINED_STRING;
            string[] entities;
            double[] currentValues = new double[lineElemntsNum];

            try
            {
                string[] lines = LoadRiskLiquidityFileData(filename);
                int currentIndex = 0;
                int productLocation;

                for (int l = MiscConstants.RISK_LIQUIDITY_HEADER; l < lines.Length; l++)
                {
                    line = lines[l];
                    if (String.IsNullOrEmpty(line))
                        continue;

                    entities = line.Split(MiscConstants.COMMA);
                    // some lines have the Liquidity value, some don't. So treat it properly
                    productLocation = entities.Length - lineElemntsNum - 1;
                    string productName = entities[productLocation].Trim();
                    if (String.IsNullOrEmpty(productName))
                    {
                        Console.WriteLine("ERROR: LoadRiskLiquidityFile empty productName: " + productName);
                        continue;
                    }

                    int index = GenericProduct.GetProductIndex(productName);
                    // ensure the line correctness: check the product name 
                    if (MiscConstants.UNDEFINED_INT == index)
                    {
                        //WindowsUtilities.loggerMethod("LoadRiskLiquidityFile producs: " + productName + " does not have rate. Ignore it.");
                        continue;
                    }

                    //// ensre the same index is used
                    //if (index != productsIndex)
                    //{
                    //    Console.WriteLine("ERROR: LoadRiskLiquidityFile illegal index for productName: " + 
                    //        productName + ", external index: " + index + " while internal index: " + productsIndex);
                    //    continue;
                    //}

                    // set the location in the entire table
                    if (0 > GetProductIndex(productName))
                    {
                        productsArray[productsIndex++] = productName;
                    }

                    // what is the weight for each of them


                    // clean all redundant chars e.g. %
                    for (int i = productLocation+1, j = 0; i < entities.Length; i++, j++)
                    {
                        int ind = entities[i].IndexOf(MiscConstants.PERCANTAGE_STR);
                        string trimed = (0 < ind) ? entities[i].Remove(ind).Trim() : entities[i].Trim();
                        entities[i] = trimed;
                        currentValues[j] = Double.Parse(trimed);
                    }

                    currentValues.CopyTo(riskLiquidityArray, currentIndex);
                    currentIndex += lineElemntsNum;
               }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: LoadRiskLiquidityFile got Exception: " + e.ToString() + ". line: " + line);
                riskLiquidityArray = null;
            }

            return riskLiquidityArray;
        }


        public bool FindRiskLiquidity(RiskLiquidityValue riskLiquidityValue)
        {
            bool rc = false;
            int indexInArray = MiscConstants.UNDEFINED_INT;
            try
            {
                // what is the calculation:
                // the product index * # of loaded products * Liquidity + Risk 
                int productIndex = GetProductIndex(riskLiquidityValue.productID.stringTypeId);
                if (0 > productIndex)
                {
                    WindowsUtilities.loggerMethod("ERROR: FindRiskLiquidity illegal productID: " + riskLiquidityValue.productID.stringTypeId);
                }
                else
                {
                    int liquidityVal = Array.IndexOf(Enum.GetValues(riskLiquidityValue.liquidity.GetType()), riskLiquidityValue.liquidity);
                    int riskVal = Array.IndexOf(Enum.GetValues(riskLiquidityValue.risk.GetType()), riskLiquidityValue.risk);
                    indexInArray = liquidityVal * Share.theLoadedProducts.Count *
                        (Enum.GetNames(typeof(Risk)).Length - 1) *          // NONE
                        2 /*min, max*/ +
                        productIndex * (Enum.GetNames(typeof(Risk)).Length - 1) * 2 +
                        riskVal * 2;

                    if (0 <= indexInArray && Share.riskAndLiquidity.Length > indexInArray + 1)
                    {
                        riskLiquidityValue.min = Share.riskAndLiquidity[indexInArray];
                        riskLiquidityValue.max = Share.riskAndLiquidity[indexInArray + 1];
                        rc = true;
                    }
                    else
                    {
                        WindowsUtilities.loggerMethod("ERROR: FindRiskLiquidity illegal indexInArray: " + indexInArray + " while riskAndLiquidity include: " + Share.riskAndLiquidity.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: FindRiskLiquidity got Exception: " + e.ToString() + ", indexInArray: " + indexInArray + ", riskLiquidityValue: " + riskLiquidityValue.ToString());
            }
            return rc;
        }


    }

    public class RiskLiquidityValue
    {
        public ProductID productID { set; get; }
        public Risk risk { set; get; }
        public Liquidity liquidity { set; get; }
        public double min { set; get; }
        public double max { set; get; }

        public override string ToString()
        {
            return "product: " + productID.stringTypeId + ", risk: " + risk.ToString() +
                ", liquidity: " + liquidity.ToString() + ", result min: " + min + ", max: " + max;

        }
    }
}
