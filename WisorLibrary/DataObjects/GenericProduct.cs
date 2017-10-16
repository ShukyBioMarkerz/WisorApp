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
        public markets localMarket;

        // Identify the Index used to calculate rate
        // Once Index is identified -> DB has the historical rates -> the final value should be a percentage
        public double indexUsedFirstTimePeriod { get; set; }
        public double indexUsedSecondTimePeriod { get; set; }
        public indices originalIndexUsedFirstTimePeriod { get; set; }
        public indices originalIndexUsedSecondTimePeriod { get; set; }
        
        public /*Benefit*/ double benefit { get; set; }
        public FixOrAdjustable fixOrAdjustable { get; set; }
        public /*Risk*/ double risk { get; set; }
        public /*Liquidity*/ double liquidity { get; set; }
        public bool mustBeUsed { get; set; }
        public bool shouldConsider { get; set; }


        // Identify the time between updates of chosen index
        // Once Index is identified -> DB has the historical rates -> the final value should be an number
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
        public string /*typeId, */name, hebrewName;

        public uint firstTimePeriod;    // If product is completely fixed rate then firstTimePeriod is not relevant
        public uint secondTimePeriod;   // If product is completely fixed rate then secondTimePeriod is not relevant

        //public double rateFormulaFirstTimePeriod;   // If product is completely fixed rate then rateFormulaFirstTimePeriod is not relevant
        //public double rateFormulaSecondTimePeriod;  // If product is completely fixed rate then rateFormulaSecondTimePeriod is not relevant

        public double maxPercentageOfLoan;

        public uint Score { get; set; }

        public GenericProduct(ProductID productID, markets localMarket = markets.NONE, // USA, UK, ISRAEL, OTHER
            string name = MiscConstants.UNDEFINED_STRING, string hebrewName = MiscConstants.UNDEFINED_STRING,
            indices indexUsedFirstTimePeriod = indices.NONE, indices indexUsedSecondTimePeriod = indices.NONE, // PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE
            indexJumps indexJumpFirstTimePeriod = indexJumps.NONE, indexJumps indexJumpSecondTimePeriod = indexJumps.NONE, // AY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60, MONTHS84, MONTHS120, OTHER
            uint minTime = MiscConstants.UNDEFINED_UINT, uint maxTime = MiscConstants.UNDEFINED_UINT,
            uint timeJump = MiscConstants.UNDEFINED_UINT, uint firstTimePeriod = MiscConstants.UNDEFINED_UINT,
            double maxPercentageOfLoan = MiscConstants.UNDEFINED_DOUBLE,
            /*Risk*/ double risk = MiscConstants.UNDEFINED_DOUBLE, /*Liquidity*/ double liquidity = MiscConstants.UNDEFINED_DOUBLE,
            FixOrAdjustable fixOrAdjustable = FixOrAdjustable.ADJUSTABLE, /*Benefit*/ double benefit = MiscConstants.UNDEFINED_DOUBLE,
            bool mustBeUsed = MiscConstants.UNDEFINED_BOOL, bool shouldBeUsed = MiscConstants.UNDEFINED_BOOL) 
        {
            this.productID = productID;
            this.localMarket = localMarket;
            this.name = name;
            this.hebrewName = hebrewName;
            this.originalIndexUsedFirstTimePeriod = indexUsedFirstTimePeriod;
            this.originalIndexUsedSecondTimePeriod = indexUsedSecondTimePeriod;
            this.indexUsedFirstTimePeriod = MiscUtilities.GetIndexRateForOption(indexUsedFirstTimePeriod);
            this.indexUsedSecondTimePeriod = MiscUtilities.GetIndexRateForOption(indexUsedSecondTimePeriod);
            this.indexJumpFirstTimePeriod = indexJumpFirstTimePeriod;
            this.indexJumpSecondTimePeriod = indexJumpSecondTimePeriod;
            this.minTime = minTime;
            this.maxTime = maxTime;
            this.timeJump = timeJump;
            this.firstTimePeriod = firstTimePeriod;
            this.maxPercentageOfLoan = maxPercentageOfLoan;
            this.benefit = benefit;
            this.fixOrAdjustable = fixOrAdjustable;
            this.risk = risk;
            this.liquidity = liquidity;
            this.mustBeUsed = mustBeUsed;
            this.shouldConsider = shouldBeUsed;

            CheckCorrectness();

            Score = CalculateScore();

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
            }
        }

        // each product have a beneficial score dependent on the risk, liquidity and benefit parameters
        uint CalculateScore()
        {
            return MiscUtilities.CalculateProductScore(risk, liquidity, benefit);
        }

    
        public static ProductsList LoadXMLProductsFile(string filename)
        {
            ProductsList products = new ProductsList();
            ProductsList productsAll = new ProductsList();
            XElement currProduct = null;
            markets market;
            string name, typeId, hebrewName = MiscConstants.UNDEFINED_STRING;
            indices iftp, istp;
            indexJumps ijftp, ijstp;
            uint minTime, maxTime, timeJump, firstTimePeriod;
            double maxPercentageLoan = MiscConstants.UNDEFINED_DOUBLE;
            int index = 0, productsAllIndex = 0;
            /*Benefit*/ double benefit;
            FixOrAdjustable fixOrAdjustable;
            /*Risk*/ double risk;
            /*Liquidity*/ double liquidity;
            HashSet<string> productsHash = new HashSet<string>();
            bool mustBeUsed, shouldConsider;
            uint score;
            bool beneficial;

            // Load only th nneded products according to the combination definition
            try
            {
                if (File.Exists(filename))
                {
                    WindowsUtilities.loggerMethod("LoadXMLProductsFile file: " + filename);

                    XDocument doc = XDocument.Load(filename);

                    // loop all the items in the document
                    foreach (XElement product in from p in doc.Descendants(MiscConstants.Product) select p)
                    {
                        currProduct = product;
                        typeId = product.Element("typeId").Value;

                        // check if the product should be loaded
                        shouldConsider = MiscConstants.UNDEFINED_BOOL;
                        if (null != product.Element(MiscConstants.shouldConsider) && MiscConstants.UNDEFINED_STRING != product.Element(MiscConstants.shouldConsider).Value)
                            shouldConsider = MiscUtilities.IsTrue(product.Element(MiscConstants.shouldConsider).Value);
                                // Convert.ToBoolean(product.Element(MiscConstants.shouldConsider).Value);
                        if (!shouldConsider)
                        {
                            // Since there are products which the bank use and should be display in the report,
                            // we should load all the products, even those which should not be used.
                            // If we build the combination dynamickly, then we will use the 'shouldConsider' flag CreateCombination function.
                            // If we use a static combination list from the file, then the other will be ignored anyway
                            //WindowsUtilities.loggerMethod("LoadXMLProductsFile producs: " + typeId +
                            //" shouldConsider not be used. Skip it.");
                            //continue;
                        }

                        market = (markets)Enum.Parse(typeof(markets), product.Element(MiscConstants.market).Value, true);
                        // ensure its the needed market
                        if (market != Share.theMarket)
                            continue;
                        name = product.Element(MiscConstants.name).Value;
                        if (null != product.Element(MiscConstants.hebrewName))
                            hebrewName = product.Element(MiscConstants.hebrewName).Value;
                        iftp = (indices)Enum.Parse(typeof(indices), product.Element(MiscConstants.indexUsedFirstTimePeriod).Value, true);
                        istp = (indices)Enum.Parse(typeof(indices), product.Element(MiscConstants.indexUsedSecondTimePeriod).Value, true);
                        ijftp = (indexJumps)Enum.Parse(typeof(indexJumps), product.Element(MiscConstants.indexJumpFirstTimePeriod).Value, true);
                        ijstp = (indexJumps)Enum.Parse(typeof(indexJumps), product.Element(MiscConstants.indexJumpSecondTimePeriod).Value, true);
                        minTime = Convert.ToUInt32(product.Element(MiscConstants.minTime).Value);
                        maxTime = Convert.ToUInt32(product.Element(MiscConstants.maxTime).Value);
                        timeJump = Convert.ToUInt32(product.Element(MiscConstants.timeJump).Value);
                        //string optName = product.Element("optName").Value;
                        firstTimePeriod = Convert.ToUInt32(product.Element(MiscConstants.firstTimePeriod).Value);
                        maxPercentageLoan = MiscConstants.UNDEFINED_DOUBLE; 
                        if (null != product.Element(MiscConstants.maxPercentageOfLoan))
                            maxPercentageLoan = Convert.ToDouble(product.Element(MiscConstants.maxPercentageOfLoan).Value);
                        benefit = MiscConstants.UNDEFINED_DOUBLE; /*Benefit.NONEBenefit;*/
                        if (null != product.Element(MiscConstants.benefit))
                            benefit = Convert.ToDouble(product.Element(MiscConstants.benefit).Value); //  (Benefit)Enum.Parse(typeof(Benefit), product.Element("benefit").Value, true);
                        fixOrAdjustable = FixOrAdjustable.ADJUSTABLE;
                        if (null != product.Element(MiscConstants.fixOrAdjustable) && MiscConstants.UNDEFINED_STRING != product.Element(MiscConstants.fixOrAdjustable).Value)
                            fixOrAdjustable = (FixOrAdjustable)Enum.Parse(typeof(FixOrAdjustable), product.Element(MiscConstants.fixOrAdjustable).Value, true);
                        risk = MiscConstants.UNDEFINED_DOUBLE;  /*Risk.NONERisk;*/
                        if (null != product.Element(MiscConstants.risk))
                            risk = Convert.ToDouble(product.Element(MiscConstants.risk).Value); // (Risk)Enum.Parse(typeof(Risk), product.Element("risk").Value, true);
                        liquidity = MiscConstants.UNDEFINED_DOUBLE;  /*Liquidity.NONELiquidity;*/
                        if (null != product.Element(MiscConstants.liquidity))
                            liquidity = Convert.ToDouble(product.Element(MiscConstants.liquidity).Value); // (Liquidity)Enum.Parse(typeof(Liquidity), product.Element("liquidity").Value, true);
                        mustBeUsed = MiscConstants.UNDEFINED_BOOL;
                        if (null != product.Element(MiscConstants.mustBeUsed) && MiscConstants.UNDEFINED_STRING != product.Element(MiscConstants.mustBeUsed).Value)
                            mustBeUsed = MiscUtilities.IsTrue(product.Element(MiscConstants.mustBeUsed).Value);
                        //mustBeUsed = Convert.ToBoolean(product.Element(MiscConstants.mustBeUsed).Value);

                        // ensure the product is needed
                        // set the index of the product later
                        //index = GenericProduct.GetProductIndex(typeId);
                        // ensure the beneficial score is above the threshold
                        score = MiscUtilities.CalculateProductScore(risk, liquidity, benefit);
                        beneficial = MiscUtilities.CheckBeneficialProducts(score);
                        
                        //// debug the score calculation
                        //WindowsUtilities.loggerMethod("LoadXMLProductsFile producs: " + typeId +
                        //    ", risk: " + risk + ", liquidity: " + liquidity + ", benefit: " + benefit + 
                        //    " , score: " + score + ", is beneficial: " + beneficial);

                        if (/*MiscConstants.UNDEFINED_INT < index &&*/ beneficial)
                        {

                            // anyway add the product to the all-list in order tot enable using all 
                            // the products that the bank uses in the report
                            ProductID productIDAll = new ProductID(productsAllIndex, typeId);
                            productsAll.Add(productsAllIndex++, new GenericProduct(productIDAll, market, name, hebrewName,
                                   iftp /*indices*/, istp /*indices*/,
                                   ijftp /*indexJumps*/, ijstp /*indexJumps*/,
                                   minTime, maxTime, timeJump, firstTimePeriod, maxPercentageLoan,
                                   risk, liquidity, fixOrAdjustable, benefit, mustBeUsed, shouldConsider));

                            if (shouldConsider)
                            {
                                ProductID productID = new ProductID(index, typeId);
                                products.Add(index++, new GenericProduct(productID, market, name, hebrewName,
                                    iftp /*indices*/, istp /*indices*/,
                                    ijftp /*indexJumps*/, ijstp /*indexJumps*/,
                                    minTime, maxTime, timeJump, firstTimePeriod, maxPercentageLoan,
                                    risk, liquidity, fixOrAdjustable, benefit, mustBeUsed, shouldConsider));

                                // add the product name to the indexed list
                                productsHash.Add(typeId);
                            }
   
                        }
                        // the product in the products file does not have rate so ignore it
                        // or beneficial
                        else
                        {
                            //if (MiscConstants.UNDEFINED_INT >= index)
                            //    WindowsUtilities.loggerMethod("LoadXMLProductsFile producs: " + typeId + " does not have rate. Ignore it.");
                            if (!beneficial)
                                WindowsUtilities.loggerMethod("LoadXMLProductsFile producs: " + typeId + " does not defined as beneficial. Ignore it.");
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
            }
            else
            {
                WindowsUtilities.loggerMethod("LoadXMLProductsFile succeffuly load: " + products.Count + " products from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }

            string[] productNames = productsHash.ToArray();
            WindowsUtilities.loggerMethod("NOTICE: #of productNames: " + productNames.Length + " from file: " +
                filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1), true);
            Share.theProductsNames = productNames;
            Share.theLoadedProducts = products;
            Share.theAllLoadedProducts = productsAll;
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

       // get the product by the name. Look at the entire loaded products, including those which shoiuld not be used in the combinations
        public static GenericProduct GetProductFromAllListByName(string productName)
        {
            GenericProduct product = null;
            if (null != Share.theAllLoadedProducts)
            {
                foreach (KeyValuePair<int, GenericProduct> p in Share.theAllLoadedProducts)
                {
                    if (p.Value.productID.stringTypeId.ToLower() == productName.ToLower())
                    {
                        product = p.Value;
                        break;
                    }
                }
            }
            else
                WindowsUtilities.loggerMethod("GetProductFromAllListByName: Share.theAllLoadedProducts in null!!! ");

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
                    if (p.Value.productID.stringTypeId.ToLower() == productName.ToLower())
                    {
                        product = p.Value;
                        break;
                    }
                }
            }
            else
                WindowsUtilities.loggerMethod("GetProductByName: Share.theLoadedProducts in null!!! ");

            //if (null == product)
            //{
            //    WindowsUtilities.loggerMethod("GetProductByName: failed to find productName: " + productName);
            //}
            //else
            //{
            //    // TBD: check product correctess
            //}

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
