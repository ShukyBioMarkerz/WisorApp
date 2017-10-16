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
        public int profile = (int)CalculationConstants.borrowerProfiles.NOTSET;

        // General Parameters
        // Local market
        // Israel only
        //private readonly uint[] giniIndexList = new uint[3] { 9500, 6718, 1 }; // income depemdency
        // TBD: read it from file for all markets
        private readonly double[] ltvRatios = new double[5] { 0.50, 0.60, 0.70, 0.75, 0.80 }; // loan amount / property value (loan to value)
        private readonly double[] ptiRatios = new double[5] { 0.20, 0.25, 0.30, 0.35, 0.40 }; // monthy patment / income (payment to income)
        private readonly int[,] profileMatrix = new int[5, 5] {
                                {
                                    (int)CalculationConstants.borrowerProfiles.BEST,
                                    (int)CalculationConstants.borrowerProfiles.VERY_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.VERY_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.GOOD,
                                    (int)CalculationConstants.borrowerProfiles.AVERAGE
                                },
                                {
                                    (int)CalculationConstants.borrowerProfiles.VERY_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.VERY_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.GOOD,
                                    (int)CalculationConstants.borrowerProfiles.AVERAGE,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD
                                },
                                {
                                    (int)CalculationConstants.borrowerProfiles.VERY_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.GOOD,
                                    (int)CalculationConstants.borrowerProfiles.AVERAGE,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD
                                },
                                {
                                    (int)CalculationConstants.borrowerProfiles.GOOD,
                                    (int)CalculationConstants.borrowerProfiles.AVERAGE,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.BAD,
                                },
                                {
                                    (int)CalculationConstants.borrowerProfiles.AVERAGE,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.NOT_SO_GOOD,
                                    (int)CalculationConstants.borrowerProfiles.BAD,
                                    (int)CalculationConstants.borrowerProfiles.NOTOK
                                }
        };
        // End of Israel only


        // handle borrower class with FICO or class 

        public BorrowerProfile(CalculationParameters CalculationParameters, int ficoScore = MiscConstants.UNDEFINED_INT)
        {
            // TBD - Omri. Define for each market the profile calculation
            if (MiscConstants.UNDEFINED_INT == ficoScore)
            {
                profile = CalculateProfile(CalculationParameters.pti, CalculationParameters.ltv);
            }
            else
            {
                profile = CalculateProfile(ficoScore);
            }


            if ((int)CalculationConstants.borrowerProfiles.BAD < profile || (int)CalculationConstants.borrowerProfiles.BEST > profile)
            {
                Console.WriteLine("NOTICE: set to defult profile. ficoScore: " + ficoScore +
                    ", CalculationParameters.pti: " + CalculationParameters.pti +
                    ", CalculationParameters.ltv: " + CalculationParameters.ltv);
                profile = (int)CalculationConstants.borrowerProfiles.AVERAGE;
            }

        }

        // **************************************************************************************************************************** //
        // ************************************ Getting Borrower Profile According to LTV and PTI ************************************* //


        private int CalculateProfile(double pti, double ltv)
        {
            int borrowerProfile = (int)CalculationConstants.borrowerProfiles.NOTSET;

            int ltvCounter = 0;
            int ptiCounter = 0;
            while (borrowerProfile == (int)CalculationConstants.borrowerProfiles.NOTSET)
            {
                if (pti <= ptiRatios[ptiCounter])
                {
                    if (ltv <= ltvRatios[ltvCounter])
                    {
                        borrowerProfile = profileMatrix[ptiCounter, ltvCounter];
                    }
                    else
                    {
                        if (ltvCounter == (ltvRatios.Length - 1))
                        {
                            borrowerProfile = (int)CalculationConstants.borrowerProfiles.NOTOK;
                        }
                        else
                        {
                            ltvCounter++;
                        }
                    }
                }
                else
                {
                    if (ptiCounter == (ptiRatios.Length - 1))
                    {
                        borrowerProfile = (int)CalculationConstants.borrowerProfiles.NOTOK;
                    }
                    else
                    {
                        ptiCounter++;
                    }
                }
            }
            Console.WriteLine("Matrix Index [" + (ptiCounter + 1) + "," + (ltvCounter + 1) + "] = " + borrowerProfile
                                                + "\nLTV = " + Math.Round(ltv * 100, 2) + "%\nPTI = "
                                                + Math.Round(pti * 100, 2)
                                                + "%\nProfile = " + CalculationConstants.profiles[borrowerProfile]);
            return borrowerProfile;
        }


        private int CalculateProfile(int ficoScore)
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

            return profile;
        }



        // **************************************************************************************************************************** //
        // *********************************************** Print Borrower Profile in Console ****************************************** //

        public int ShowBorrowerProfile()
        {
            return profile;
        }

    }
}
