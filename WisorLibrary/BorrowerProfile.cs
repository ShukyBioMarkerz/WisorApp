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

        // Omri: define the other markets

        private readonly int[,] profileMatrix = new int[5,5] {
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





        public BorrowerProfile()
        {
            GetBorrowerProfile();
            InsertRatesToProfile(borrowerProfile);
        }





        // **************************************************************************************************************************** //
        // ************************************ Getting Borrower Profile According to LTV and PTI ************************************* //

        private void GetBorrowerProfile()
        {
            int ltvCounter = 0;
            int ptiCounter = 0;
            while (borrowerProfile == (int)CalculationConstants.borrowerProfiles.NOTSET)
            {
                if (CalculationParameters.pti <= ptiRatios[ptiCounter])
                {
                    if (CalculationParameters.ltv <= ltvRatios[ltvCounter])
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
                                                + "\nLTV = " + Math.Round(CalculationParameters.ltv * 100, 2) + "%\nPTI = "
                                                + Math.Round(CalculationParameters.pti * 100, 2)
                                                + "%\nProfile = " + CalculationConstants.profiles[borrowerProfile]);
        }




        // **************************************************************************************************************************** //
        // *********************************** Inserting Interest Rates According to Borrower Profile ********************************* //
        
        private void InsertRatesToProfile(int profileToApply)
        {
            if ((Rates.fixedNoTsamudRates == null) || (Rates.fixedTsamudRates == null) || (Rates.alternateRates == null))
            {
                Rates.ReadInterestRateFileAndUpdateRatesInSoftware(profileToApply);
                //Rates.InsertRatesAccordingToProfile(profileToApply);
            }
        }





        // **************************************************************************************************************************** //
        // ********************************* One Time Addition to Interest Rates - Fixed/Alternating ********************************** //
        /*
        public void AddToInterestRatesOnce(string ratesToChange, double howMuchToAdd)
        {
            if (ratesToChange == "TSAMUD")
            {
                Console.WriteLine("Input string - TSAMUD\n");
                if (Rates.fixedTsamudRates != null)
                {
                    for (int i = 0; i < Rates.fixedTsamudRates.Length; i++)
                    {
                        Rates.fixedTsamudRates[i] += (howMuchToAdd / 100);
                    }
                }
                else
                {
                    Console.WriteLine("Interest rates list is empty - rates not updated");
                }
            }
            else if (ratesToChange == "NOTSAMUD")
            {
                Console.WriteLine("Input string - NO TSAMUD\n");
                if (Rates.fixedNoTsamudRates != null)
                {
                    for (int i = 0; i < Rates.fixedNoTsamudRates.Length; i++)
                    {
                        Rates.fixedNoTsamudRates[i] += (howMuchToAdd / 100);
                    }
                }
                else
                {
                    Console.WriteLine("Interest rates list is empty - rates not updated");
                }
            }
            else
            {
                Console.WriteLine("Bad input string - interest rates not changed\n");
            }
        }

        */


        // **************************************************************************************************************************** //
        // *********************************************** Print Borrower Profile in Console ****************************************** //
        
        public int ShowBorrowerProfile()
        {
            return borrowerProfile;
        }






    }
}
