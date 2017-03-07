using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class BorrowerProfile
    {
        // Borrower Index
        public static int borrowerProfile = (int)CalculationConstants.borrowerProfiles.NOTSET;

        // General Parameters
        // Local market
        // Israel only
        private readonly uint[] giniIndexList = new uint[3] { 9500, 6718, 1 }; // income depemdency
        private readonly double[] ltvRatios = new double[5] { 0.50, 0.60, 0.70, 0.75, 0.80 }; // loan amount / property value (loan to value)
        private readonly double[] ptiRatios = new double[5] { 0.20, 0.25, 0.30, 0.35, 0.40 }; // monthy patment / income (payment to income)
        // End of Israel only

 
        public BorrowerProfile(RunEnvironment env)
        {
            // get the profile from the fico store
            uint ficoScore = env.CalculationParameters.fico;
            if ((0 < ficoScore) && (550 >= ficoScore))
            {
                borrowerProfile = (int)CalculationConstants.borrowerProfiles.AVERAGE;
            }
            else if ((550 < ficoScore) && (650 >= ficoScore))
            {
                borrowerProfile = (int)CalculationConstants.borrowerProfiles.GOOD;
            }
            else if ((650 < ficoScore) && (750 >= ficoScore))
            {
                borrowerProfile = (int)CalculationConstants.borrowerProfiles.VERY_GOOD;
            }
            else if ((750 < ficoScore) && (850 >= ficoScore))
            {
                borrowerProfile = (int)CalculationConstants.borrowerProfiles.BEST;
            }
            else
            {
                // Should happen...
                borrowerProfile = (int)CalculationConstants.borrowerProfiles.NOTOK;
            }
         }

        
        // **************************************************************************************************************************** //
        // *********************************************** Print Borrower Profile in Console ****************************************** //
        
        public int ShowBorrowerProfile()
        {
            return borrowerProfile;
        }

    }
}
