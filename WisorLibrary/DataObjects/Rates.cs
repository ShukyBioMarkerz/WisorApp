using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;

namespace WisorLib
{
    public class Rates
    {
        // Fixed rates - times list
        public enum fixRates
        {
            YR4, YR5, YR6, YR7, YR8, YR9, YR10, YR11, YR12,
            YR13, YR14, YR15, YR16, YR17, YR18, YR19, YR20, YR21,
            YR22, YR23, YR24, YR25, YR26, YR27, YR28, YR29, YR30
        };
        // Alternating rates - types list
        public enum alternatingRates { PRIME, MTH60NOINF, MTH12, MTH24, MTH30, MTH60YESINF, MTH84, MTH120 };

        // Interest rates list for software import
        //private static double[,] ratesForInput = new double[18, 27];
        public static double[] fixedNoTsamudRates;
        public static double[] fixedTsamudRates;
        public static double[] alternateRates;
        public static double[][] ratesMatrix;






        public Rates()
        {

        }





        // **************************************************************************************************************************** //
        // ************************* Read Interest Rate File and Update Rates According to Borrower Profile *************************** //

        static private string ratesFilename;

        public static bool SetRatesFile(string filename, string bankFilename, string secondPeriodFilename,
            string secondPeriodBankRatesFileName)
        {
            ratesFilename = filename;
            RateUtilities.SetFilename(filename, bankFilename, secondPeriodFilename, secondPeriodBankRatesFileName);
            return RateUtilities.Instance.Status;
        }

        public static double FindRateForKey(int productID, int profile, int index)
        {
            //double rate = RateUtilities.Instance.FindRateForKey(new WisorLibrary.DataObjects.RatesKey(productID, profile), index);
            double rate = RateUtilities.Instance.GetBorrowerRate(/*new WisorLibrary.DataObjects.RatesKey(productID, profile),*/
                productID, profile, index);
            return rate;
        }

        public static double FindRateForKeySecondPeriod(int productID, int profile, int index)
        {
             double rate = RateUtilities.Instance.GetBorrowerRateSecondPeriod(
                productID, profile, index);
            return rate;
        }

        public static double FindBankMarginForKey(int productID, int profile, int index)
        {
            //double rate = RateUtilities.Instance.FindRateForKey(new WisorLibrary.DataObjects.RatesKey(productID, profile), index);
            double rate = RateUtilities.Instance.GetBankRate(
                productID, profile, index);
            return rate;
        }

        public static double FindBankMarginForKeySecondPeriod(int productID, int profile, int index)
        {
            double rate = RateUtilities.Instance.GetBankRateSecondPeriod(
               productID, profile, index);
            return rate;
        }

        //public static RateLine FindRatesForKey(string productID, int profile)
        //{
        //    // RateLine : double[] value
        //    RateLine rate = RateUtilities.Instance.FindRatesForKey(new RatesKey(productID, profile));
        //    return rate;
        //}








        // **************************************************************************************************************************** //
        // ********************************* One Time Addition to Interest Rates - Fixed/Alternating ********************************** //

        public static void AddToInterestRatesOnce(string ratesToChange, double howMuchToAdd)
        {
            if (ratesToChange == "TSAMUD")
            {
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




        // **************************************************************************************************************************** //
        // ************************************* Print Interest Rates for Borrower Profile in Console ********************************* //

        public static void ShowAllRatesForProfile(int profileToUse)
        {
            if ((profileToUse == (int)CalculationConstants.borrowerProfiles.NOTSET) || (profileToUse == (int)CalculationConstants.borrowerProfiles.NOTOK))
            {
                Console.WriteLine("Borrower profile not inserted");
            }
            else
            {
                Console.WriteLine("Fixed Tsamud");
                string stringToWrite = "";
                for (int i = 0; i < fixedTsamudRates.Length; i++)
                {
                    stringToWrite += fixedTsamudRates[i].ToString() + ",";
                }
                Console.WriteLine(stringToWrite);

                Console.WriteLine("Fixed No Tsamud");
                stringToWrite = "";
                for (int i = 0; i < fixedNoTsamudRates.Length; i++)
                {
                    stringToWrite += fixedNoTsamudRates[i].ToString() + ",";
                }
                Console.WriteLine(stringToWrite);

                Console.WriteLine("Alternating");
                stringToWrite = "";
                for (int i = 0; i < alternateRates.Length; i++)
                {
                    stringToWrite += alternateRates[i].ToString() + ",";
                }
                Console.WriteLine(stringToWrite);
            }
        }
    }
}
