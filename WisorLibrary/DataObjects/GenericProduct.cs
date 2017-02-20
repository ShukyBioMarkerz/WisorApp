using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
        public enum indices { PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE }; // Are the options in the code or pulled from outside DB?
        public indices indexUsedFirstTimePeriod;
        public indices indexUsedSecondTimePeriod;

        // Identify the time between updates of chosen index
        // Once Index is identified -> DB has the historical rates -> the final value should be an number
        public enum indexJumps { DAY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60,
                MONTHS84, MONTHS120, OTHER , NONE};
        public indexJumps indexJumpFirstTimePeriod;
        public indexJumps indexJumpSecondTimePeriod;

        // Identify the operator used to calculate rate
        public enum operators { PLUS, MINUS, OTHER, NONE }; // Are the options in the code or pulled from outside DB?
        public operators operatorUsedFirstTimePeriod;
        public operators operatorUsedSecondTimePeriod;

        // Need to add a field that contains the actual operator -> Bool? Maybe not needed?

        // General Parameters
        public uint typeId;
        public uint minTime;    // If product is only available for a single time then minTime = maxTime
        public uint maxTime;    // If product is only available for a single time then minTime = maxTime
        public uint timeJump;       // If product is only available for a single time then jump is not relevant
        public string optName;

        public uint firstTimePeriod;    // If product is completely fixed rate then firstTimePeriod is not relevant
        public uint secondTimePeriod;   // If product is completely fixed rate then secondTimePeriod is not relevant

        public double rateFormulaFirstTimePeriod;   // If product is completely fixed rate then rateFormulaFirstTimePeriod is not relevant
        public double rateFormulaSecondTimePeriod;  // If product is completely fixed rate then rateFormulaSecondTimePeriod is not relevant

        public GenericProduct(string ID, markets localMarket = markets.NONE, // USA, UK, ISRAEL, OTHER
            indices indexUsedFirstTimePeriod = indices.NONE, indices indexUsedSecondTimePeriod = indices.NONE, // PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE
            indexJumps indexJumpFirstTimePeriod = indexJumps.NONE, indexJumps indexJumpSecondTimePeriod = indexJumps.NONE, // AY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60, MONTHS84, MONTHS120, OTHER
            operators operatorUsedFirstTimePeriod = operators.NONE, operators operatorUsedSecondTimePeriod = operators.NONE) // PLUS, MINUS, OTHER, NONE
        {
            this.ID = ID;
            this.localMarket = localMarket;
            this.indexUsedFirstTimePeriod = indexUsedFirstTimePeriod;
            this.indexUsedSecondTimePeriod = indexUsedSecondTimePeriod;
            this.indexJumpFirstTimePeriod = indexJumpFirstTimePeriod;
            this.indexJumpSecondTimePeriod = indexJumpSecondTimePeriod;
            this.operatorUsedFirstTimePeriod = operatorUsedFirstTimePeriod;
            this.operatorUsedSecondTimePeriod = operatorUsedSecondTimePeriod;
        }

        private bool ActivateIndices(indices indices)
        {
            bool rc = false;
            double factor = 0;

            switch (indices)
            {
                case indices.PRIME:
                    factor = 1.1;
                    break;
                case indices.BBBR:
                    factor = 1.1;
                    break;
                case indices.EUROBOR:
                    factor = 1.1;
                    break;
                case indices.FED:
                    factor = 1.1;
                    break;
                case indices.LIBOR:
                    factor = 1.1;
                    break;
                case indices.OTHER:
                    factor = 1.1;
                    break;
                default:
                    factor = 1.3;
                    break;
            }
            return rc;
        }


        private bool ActivateIndexJumps(indexJumps indexJumps)
        {
            bool rc = false;
            double factor = 0;

            switch (indexJumps)
            {
                case indexJumps.DAY:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS1:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS12:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS120:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS24:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS3:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS30:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS36:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS6:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS60:
                    factor = 1.1;
                    break;
                case indexJumps.MONTHS84:
                    factor = 1.1;
                    break;
                case indexJumps.WEEK:
                    factor = 1.1;
                    break;
                default:
                    factor = 1.3;
                    break;
            }
            return rc;
        }

        

        private bool ActivateOperators(operators operators)
        {
            bool rc = false;
            double factor = 0;

            switch (operators)
            {
                case operators.MINUS:
                    factor = 1.1;
                    break;
                case operators.PLUS:
                    factor = 1.1;
                    break;
                case operators.OTHER:
                    factor = 1.1;
                    break;
                default:
                    factor = 1.3;
                    break;
            }
            return rc;
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
