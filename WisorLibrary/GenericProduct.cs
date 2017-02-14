using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class GenericProduct
    {
        // Identify which local market is used
        public enum markets { USA, UK, ISRAEL, OTHER }; // Are the options in the code or pulled from outside DB?
        public markets localMarket;

        // Identify the Index used to calculate rate
        // Once Index is identified -> DB has the historical rates -> the final value should be a percentage
        public enum indices { PRIME, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE }; // Are the options in the code or pulled from outside DB?
        public indices indexUsedFirstTimePeriod;
        public indices indexUsedSecondTimePeriod;

        // Identify the time between updates of chosen index
        // Once Index is identified -> DB has the historical rates -> the final value should be an number
        public enum indexJumps { DAY, WEEK, MONTHS1, MONTHS3, MONTHS6, MONTHS12, MONTHS24, MONTHS30, MONTHS36, MONTHS60, MONTHS84, MONTHS120, OTHER };
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


    }
}
