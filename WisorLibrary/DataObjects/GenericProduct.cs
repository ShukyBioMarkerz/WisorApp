using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class GenericProduct
    {
        public string ID { get; set; } // unique identifier

        // Identify which local market is used
        public enum markets { USA, UK, ISRAEL, OTHER , NONE}; // Are the options in the code or pulled from outside DB?
        public markets localMarket;

        // Identify the Index used to calculate rate
        // Once Index is identified -> DB has the historical rates -> the final value should be a percentage
        public indices indexUsedFirstTimePeriod { get; set; }
        public indices indexUsedSecondTimePeriod { get; set; }

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

        public GenericProduct(string ID, markets localMarket = markets.NONE, // USA, UK, ISRAEL, OTHER
            string name = MiscConstants.UNDEFINED_STRING, 
            indices indexUsedFirstTimePeriod = indices.NONE, indices indexUsedSecondTimePeriod = indices.NONE, // PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE
            indexJumps indexJumpFirstTimePeriod = indexJumps.NONE, indexJumps indexJumpSecondTimePeriod = indexJumps.NONE, // AY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60, MONTHS84, MONTHS120, OTHER
            uint minTime = MiscConstants.UNDEFINED_UINT, uint maxTime = MiscConstants.UNDEFINED_UINT,
            uint timeJump = MiscConstants.UNDEFINED_UINT, uint firstTimePeriod = MiscConstants.UNDEFINED_UINT,
            double maxPercentageOfLoan = MiscConstants.UNDEFINED_DOUBLE) 
        {
            this.ID = ID;
            this.localMarket = localMarket;
            this.name = name;
            this.indexUsedFirstTimePeriod = indexUsedFirstTimePeriod;
            this.indexUsedSecondTimePeriod = indexUsedSecondTimePeriod;
            this.indexJumpFirstTimePeriod = indexJumpFirstTimePeriod;
            this.indexJumpSecondTimePeriod = indexJumpSecondTimePeriod;
            this.minTime = minTime;
            this.maxTime = maxTime;
            this.timeJump = timeJump;
            this.firstTimePeriod = firstTimePeriod;
            this.maxPercentageOfLoan = maxPercentageOfLoan;
         }

     
        public static ProductsList LoadXMLProductsFile(string filename)
        {
            ProductsList products = new ProductsList();

            try {
                if (File.Exists(filename))
                {
                    XDocument doc = XDocument.Load(filename);

                    // loop all the items in the document
                    foreach (XElement product in from p in doc.Descendants("Product") select p)
                    {
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
                        
                        // add to the memory
                        products.Add(new GenericProduct(typeId /*ID*/, market /*localMarket*/, name, 
                            iftp /*indices*/, istp /*indices*/,
                            ijftp /*indexJumps*/, ijstp /*indexJumps*/,
                            minTime, maxTime, timeJump, firstTimePeriod, maxPercentageLoan));
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadXMLProductsFile file: " + filename + " does not exists!!!");
                }
            }
            catch(Exception ex)
            {
                WindowsUtilities.loggerMethod("NOTICE: LoadXMLProductsFile Exception occured: " + ex.ToString());
            }

            WindowsUtilities.loggerMethod("LoadXMLProductsFile succeffuly load: " + products.Count + " products.");
            Share.theLoadedProducts = products;
            return products;
        }

    }


    public class ProductsList : List<GenericProduct>
    {
        public static Predicate<GenericProduct> ProductPredicate(GenericProduct gp)
        {
            return delegate (GenericProduct p)
            {
                return p.ID.ToLower() == gp.ID.ToLower();
            };
        }

        public GenericProduct GetProduct(string id)
        {
            return this.Find(ProductPredicate(new GenericProduct(id)));
        }

        //public int GetIndexOf(string id)
        //{
        //    int index = -1;
        //    GenericProduct gp = this.Find(ProductPredicate(new GenericProduct(id)));
        //    if (null != gp)
        //        index = gp.index;
        //    return index;
        //}
    }
}
