using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.CalculationConstants;

namespace WisorLib
{
    public class BorrowerProfile
    {
        // Borrower Index
        public /* static */ int profile = (int)CalculationConstants.borrowerProfiles.NOTSET;

        // General Parameters
        // Local market
        // Israel only
        private readonly uint[] giniIndexList = new uint[3] { 9500, 6718, 1 }; // income depemdency
        private readonly double[] ltvRatios = new double[5] { 0.50, 0.60, 0.70, 0.75, 0.80 }; // loan amount / property value (loan to value)
        private readonly double[] ptiRatios = new double[5] { 0.20, 0.25, 0.30, 0.35, 0.40 }; // monthy patment / income (payment to income)
        // End of Israel only

 
           // handle borrower class with FICO or class 

        public BorrowerProfile(uint ficoScore)
        {
            // get the profile from the fico store
            //uint ficoScore = env.CalculationParameters.fico;
            // TBD: this is hugly but short - in order to enable using FICO and class (1-6), once recognize as 1-6 it means the exact class and not fico...
            if (0 <= ficoScore && ficoScore < Enum.GetValues(typeof(borrowerProfiles)).Length)
            {
                profile = (int)Enum.Parse(typeof(borrowerProfiles), ficoScore.ToString(), true);
            }
            else
            {
                if ((0 < ficoScore) && (550 >= ficoScore))
                {
                    profile = (int)CalculationConstants.borrowerProfiles.AVERAGE;
                }
                else if ((550 < ficoScore) && (650 >= ficoScore))
                {
                    profile = (int)CalculationConstants.borrowerProfiles.GOOD;
                }
                else if ((650 < ficoScore) && (750 >= ficoScore))
                {
                    profile = (int)CalculationConstants.borrowerProfiles.VERY_GOOD;
                }
                else if ((750 < ficoScore) && (850 >= ficoScore))
                {
                    profile = (int)CalculationConstants.borrowerProfiles.BEST;
                }
                else
                {
                    // Should happen...
                    profile = (int)CalculationConstants.borrowerProfiles.NOTOK;

                    // TBD. to be on the safe side
                    profile = (int)CalculationConstants.borrowerProfiles.AVERAGE;
                }
            }
         }

        
        // **************************************************************************************************************************** //
        // *********************************************** Print Borrower Profile in Console ****************************************** //
        
        public int ShowBorrowerProfile()
        {
            return profile;
        }

    }
}
