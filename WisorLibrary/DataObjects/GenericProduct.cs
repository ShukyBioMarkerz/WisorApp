using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WisorLib;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class ProductID {
        public int numberID { get; set; } 
        public string stringTypeId { get; set; }

        public ProductID(int numberID, string stringTypeId)
        {
            this.numberID = numberID;
            this.stringTypeId = stringTypeId;
        }
        public override string ToString()
        {
            return ("ProductID name: " + stringTypeId + ", number: " + numberID);

        }

    }


    public class GenericProduct
    {
        public ProductID productID { get; set; } // unique identifier
  
        // Identify which local market is used
        public enum markets { USA, UK, ISRAEL, OTHER , NONE}; // Are the options in the code or pulled from outside DB?
        public markets localMarket;

        // Identify the Index used to calculate rate
        // Once Index is identified -> DB has the historical rates -> the final value should be a percentage
        public double indexUsedFirstTimePeriod { get; set; }
        public double indexUsedSecondTimePeriod { get; set; }
        public indices originalIndexUsedFirstTimePeriod { get; set; }

        // Identify the time between updates of chosen index
        // Once Index is identified -> DB has the historical rates -> the final value should be an number
        public enum indexJumps { DAY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60,
                MONTHS84, MONTHS120, OTHER , NONE};
        public indexJumps indexJumpFirstTimePeriod;
        public indexJumps indexJumpSecondTimePeriod;

        //// Identify the operator used to calculate rate
        //public enum operators { PLUS, MINUS, OTHER, NONE }; // Are the options in the code or pulled from outside DB?
        //public operators operatorUsedFirstTimePeriod;
        //public operators operatorUsedSecondTimePeriod;

        // Need to add a field that contains the actual operator -> Bool? Maybe not needed?

        // General Parameters
        //public uint typeId;
        public uint minTime;    // If product is only available for a single time then minTime = maxTime
        public uint maxTime;    // If product is only available for a single time then minTime = maxTime
        public uint timeJump;       // If product is only available for a single time then jump is not relevant
        public string /*typeId, */name;

        public uint firstTimePeriod;    // If product is completely fixed rate then firstTimePeriod is not relevant
        public uint secondTimePeriod;   // If product is completely fixed rate then secondTimePeriod is not relevant

        //public double rateFormulaFirstTimePeriod;   // If product is completely fixed rate then rateFormulaFirstTimePeriod is not relevant
        //public double rateFormulaSecondTimePeriod;  // If product is completely fixed rate then rateFormulaSecondTimePeriod is not relevant

        public double maxPercentageOfLoan;

        public GenericProduct(ProductID productID, markets localMarket = markets.NONE, // USA, UK, ISRAEL, OTHER
            string name = MiscConstants.UNDEFINED_STRING, 
            indices indexUsedFirstTimePeriod = indices.NONE, indices indexUsedSecondTimePeriod = indices.NONE, // PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE
            indexJumps indexJumpFirstTimePeriod = indexJumps.NONE, indexJumps indexJumpSecondTimePeriod = indexJumps.NONE, // AY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60, MONTHS84, MONTHS120, OTHER
            uint minTime = MiscConstants.UNDEFINED_UINT, uint maxTime = MiscConstants.UNDEFINED_UINT,
            uint timeJump = MiscConstants.UNDEFINED_UINT, uint firstTimePeriod = MiscConstants.UNDEFINED_UINT,
            double maxPercentageOfLoan = MiscConstants.UNDEFINED_DOUBLE) 
        {
            this.productID = productID;
            this.localMarket = localMarket;
            this.name = name;
            this.originalIndexUsedFirstTimePeriod = indexUsedFirstTimePeriod;
            this.indexUsedFirstTimePeriod = MiscUtilities.GetIndexRateForOption(indexUsedFirstTimePeriod);
            this.indexUsedSecondTimePeriod = MiscUtilities.GetIndexRateForOption(indexUsedSecondTimePeriod);
            this.indexJumpFirstTimePeriod = indexJumpFirstTimePeriod;
            this.indexJumpSecondTimePeriod = indexJumpSecondTimePeriod;
            this.minTime = minTime;
            this.maxTime = maxTime;
            this.timeJump = timeJump;
            this.firstTimePeriod = firstTimePeriod;
            this.maxPercentageOfLoan = maxPercentageOfLoan;

            CheckCorrectness();
         }

        // Alert for illegal values
        void CheckCorrectness()
        {
            string msg = null;
            if (MiscConstants.UNDEFINED_UINT >= minTime)
                msg += " Illegal minTime: " + minTime.ToString() + " .";
            if (MiscConstants.UNDEFINED_UINT >= maxTime)
                msg += " Illegal maxTime: " + maxTime.ToString() + " .";
            if (MiscConstants.UNDEFINED_UINT > firstTimePeriod)
                msg += " Illegal firstTimePeriod: " + firstTimePeriod.ToString() + " .";
            if (MiscConstants.UNDEFINED_UINT >= timeJump)
                msg += " Illegal timeJump: " + timeJump.ToString() + " .";
            if (MiscConstants.UNDEFINED_DOUBLE >= maxPercentageOfLoan)
                msg += " Illegal maxPercentageOfLoan: " + maxPercentageOfLoan.ToString() + " .";
            //if (indices.NONE  == indexUsedFirstTimePeriod)
            //    msg += " Illegal indexUsedFirstTimePeriod: " + indexUsedFirstTimePeriod.ToString() + " .";
            //if (indices.NONE == indexUsedSecondTimePeriod)
            //    msg += " Illegal indexUsedSecondTimePeriod: " + indexUsedSecondTimePeriod.ToString() + " .";
            if (indexJumps.NONE == indexJumpFirstTimePeriod)
                msg += " Illegal indexJumpFirstTimePeriod: " + indexJumpFirstTimePeriod.ToString() + " .";
            if (indexJumps.NONE  == indexJumpSecondTimePeriod)
                msg += " Illegal indexJumpSecondTimePeriod: " + indexJumpSecondTimePeriod.ToString() + " .";
            if (markets.NONE == localMarket)
                msg += " Illegal localMarket: " + localMarket.ToString() + " .";
            
            if (null != msg)
            {
                WindowsUtilities.loggerMethod("ERROR: CheckCorrectness for productID: " + productID.stringTypeId + " found: " + msg);
                Console.WriteLine("ERROR: CheckCorrectness for productID: " + productID.stringTypeId + " found: " + msg);
            }
        }


        public static ProductsList LoadXMLProductsFile(string filename)
        {
            ProductsList products = new ProductsList();
            XElement currProduct = null;

            // Load only th nneded products according to the combination definition
            try {
                if (File.Exists(filename))
                {
                    XDocument doc = XDocument.Load(filename);

                    // loop all the items in the document
                    foreach (XElement product in from p in doc.Descendants("Product") select p)
                    {
                        currProduct = product;
                        markets market = (markets)Enum.Parse(typeof(markets), product.Element("market").Value, true);
                        string name = product.Element("name").Value;
                        string typeId = product.Element("typeId").Value;
                        indices iftp = (indices)Enum.Parse(typeof(indices), product.Element("indexUsedFirstTimePeriod").Value, true);
                        indices istp = (indices)Enum.Parse(typeof(indices), product.Element("indexUsedSecondTimePeriod").Value, true);
                        indexJumps ijftp = (indexJumps)Enum.Parse(typeof(indexJumps), product.Element("indexJumpFirstTimePeriod").Value, true);
                        indexJumps ijstp = (indexJumps)Enum.Parse(typeof(indexJumps), product.Element("indexJumpSecondTimePeriod").Value, true);
                        uint minTime = Convert.ToUInt32(product.Element("minTime").Value);
                        uint maxTime = Convert.ToUInt32(product.Element("maxTime").Value);
                        uint timeJump = Convert.ToUInt32(product.Element("timeJump").Value);
                        //string optName = product.Element("optName").Value;
                        uint firstTimePeriod = Convert.ToUInt32(product.Element("firstTimePeriod").Value);
                        double maxPercentageLoan = MiscConstants.UNDEFINED_DOUBLE; 
                        if (null != product.Element("maxPercentageOfLoan"))
                            maxPercentageLoan = Convert.ToDouble(product.Element("maxPercentageOfLoan").Value);

                        // ensure the product is needed
                        int index = GenericProduct.GetProductIndex(typeId);
                        if (MiscConstants.UNDEFINED_INT < index)
                        {
                            ProductID productID = new ProductID(index, typeId);
                            products.Add(index, new GenericProduct(productID /*ID*/, market /*localMarket*/, name,
                                iftp /*indices*/, istp /*indices*/,
                                ijftp /*indexJumps*/, ijstp /*indexJumps*/,
                                minTime, maxTime, timeJump, firstTimePeriod, maxPercentageLoan));
                        }
                        // the product in the products file does not have rate so ignore it
                        else
                        {
                            //WindowsUtilities.loggerMethod("LoadXMLProductsFile producs: " + typeId + " does not have rate. Ignore it.");
                        }

                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadXMLProductsFile file: " + filename + " does not exists!!!");
                }
            }
            catch(Exception ex)
            {
                WindowsUtilities.loggerMethod("NOTICE: LoadXMLProductsFile Exception occured: " + ex.ToString() + ", product: " + currProduct.ToString());
            }

            if (null == products || 0 >= products.Count)
            {
                WindowsUtilities.loggerMethod("ERROR: failed to load products from file: " + filename);
                Console.WriteLine("ERROR: failed to load products from file: " + filename);
            }
            else
            {
                WindowsUtilities.loggerMethod("LoadXMLProductsFile succeffuly load: " + products.Count + " products from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                Console.WriteLine("LoadXMLProductsFile succeffuly load: " + products.Count + " products from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }
            Share.theLoadedProducts = products;
            return products;
        }

        public static int GetProductIndex(string productName)
        {
            if (null != Share.theProductsNames && 0 < Share.theProductsNames.Length)
            {
                int ind = Array.IndexOf(Share.theProductsNames, productName);
                if (MiscConstants.UNDEFINED_INT > ind)
                {
                    Console.WriteLine("ERROR: GetProductIndex failed to find index for product: " + productName);
                    return MiscConstants.UNDEFINED_INT;
                }
                else
                {
                    return ind;
                }
            }
            else
            {
                Console.WriteLine("ERROR: GetProductIndex theProductsNames is empty");
                return MiscConstants.UNDEFINED_INT;
            }
        }

        public static string GetProductName(int productIndex)
        {
            string productName = MiscConstants.UNDEFINED_STRING;

            if (null != Share.theProductsNames && 0 < Share.theProductsNames.Length)
            {
                if (MiscConstants.UNDEFINED_INT < productIndex && productIndex < Share.theProductsNames.Length)
                {
                    productName = Share.theProductsNames[productIndex];
               }
                else
                {
                    Console.WriteLine("ERROR: GetProductIndex failed to find index for product: " + productName);
                }
            }
            else
            {
                Console.WriteLine("ERROR: GetProductIndex theProductsNames is empty");
            }

            return productName;
        }

        // get the product by the unique id
        public static GenericProduct GetProduct(int id)
        {
            GenericProduct product = null;
            if (null != Share.theLoadedProducts)
                product = Share.theLoadedProducts.GetProduct(id);
            else
                WindowsUtilities.loggerMethod("GetProduct: Share.theLoadedProducts in null!!! ");

            if (null == product)
            {
                WindowsUtilities.loggerMethod("GetProduct: failed to find product id: " + id);
            }
            else
            {
                // TBD: check product correctess
            }

            return product;
        }

        // get the product by the name
        public static GenericProduct GetProductByName(string productName)
        {
            GenericProduct product = null;
            if (null != Share.theLoadedProducts)
            {
                foreach (KeyValuePair<int, GenericProduct> p in Share.theLoadedProducts)
                {
                    if (p.Value.productID.stringTypeId == productName)
                    {
                        product = p.Value;
                        break;
                    }
                }
            }
            else
                WindowsUtilities.loggerMethod("GetProductByName: Share.theLoadedProducts in null!!! ");

            if (null == product)
            {
                WindowsUtilities.loggerMethod("GetProductByName: failed to find productName: " + productName);
            }
            else
            {
                // TBD: check product correctess
            }

            return product;
        }

    }


    //public class ProductsList : List<GenericProduct>
    //{
    //    public static Predicate<GenericProduct> ProductPredicate(GenericProduct gp)
    //    {
    //        return delegate (GenericProduct p)
    //        {
    //            //return p.ID.ToLower() == gp.ID.ToLower();
    //            // performance ease
    //            return p.ID == gp.ID;
    //        };
    //    }

    //    public GenericProduct GetProduct(string id)
    //    {
    //        return this.Find(ProductPredicate(new GenericProduct(id)));
    //    }
    //}

    public class ProductsList : Dictionary<int, GenericProduct>
    {

        public GenericProduct GetProduct(int id)
        {
            GenericProduct gp;

            if (!this.TryGetValue(id, out gp))
                gp = null;
            return gp;
            //return this.Find(ProductPredicate(new GenericProduct(id)));
        }
    }


}
