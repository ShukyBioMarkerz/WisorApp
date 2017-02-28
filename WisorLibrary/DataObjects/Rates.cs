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

        public static void SetRatesFile(string filename)
        {
            ratesFilename = filename;
            RateUtilities.SetFilename(filename);
        }

        public static double FindRateForKey(string productID, int profile, int index)
        {
            double rate = RateUtilities.Instance.FindRateForKey(new WisorLibrary.DataObjects.RatesKey(productID, profile), index);
            return rate;
        }

        public static RateLine FindRatesForKey(string productID, int profile)
        {
            // RateLine : double[] value
            RateLine rate = RateUtilities.Instance.FindRatesForKey(new RatesKey(productID, profile));
            return rate;
        }

        public static void ReadInterestRateFileAndUpdateRatesInSoftware_OLD_DONT_USE_IT_ANYMORE(int profileOfBorrower)
        {
            int tsamudNum = profileOfBorrower;
            int noTsamudNum = tsamudNum + 6;
            int alternateNum = noTsamudNum + 6;

            double[] finalTsamudRates = new double[27];
            double[] finalNoTsamudRates = new double[27];
            double[] finalAlternateRates = new double[8];
            string lineInput = "";
            
            if (! File.Exists(ratesFilename))
            {
                Console.WriteLine("NOTICE: ReadInterestRateFileAndUpdateRatesInSoftware file: {0} does not exists!!!", ratesFilename);
                throw new System.InvalidOperationException("NOTICE: ReadInterestRateFileAndUpdateRatesInSoftware file: " + ratesFilename + " does not exists!!!");
                //return;
            }


            System.IO.StreamReader fileReader = new System.IO.StreamReader(ratesFilename);
            Console.WriteLine("Borrower Profile : " + profileOfBorrower);
            string[] lineValues = null;
            try
            {
                for (int i = 0; i <= alternateNum; i++)
                {
                    lineInput = fileReader.ReadLine();
                    if (i == tsamudNum)
                    {
                        //string[] lineValues = lineInput.Split(',');
                        lineValues = lineInput.Split(MiscConstants.COMMA_SEERATOR_STR);
                        for (int j = 2; j < lineValues.Length; j++)
                        {
                            finalTsamudRates[j - 2] = double.Parse(lineValues[j]);
                        }
                    }
                    
                    if (i == noTsamudNum)
                    {
                        //string[] lineValues = lineInput.Split(',');
                        lineValues = lineInput.Split(',');
                        for (int j = 2; j < lineValues.Length; j++)
                        {
                            finalNoTsamudRates[j - 2] = double.Parse(lineValues[j]);
                        }
                    }
                    if (i == alternateNum)
                    {
                        //string[] lineValues = lineInput.Split(',');
                        lineValues = lineInput.Split(MiscConstants.COMMA_SEERATOR_STR);
                        for (int j = 2; j < 10; j++)
                        {
                            finalAlternateRates[j - 2] = double.Parse(lineValues[j]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ReadInterestRateFileAndUpdateRatesInSoftware. lineValues[j]: " + lineValues.ToString());
            }

            fixedTsamudRates = finalTsamudRates;
            fixedNoTsamudRates = finalNoTsamudRates;
            alternateRates = finalAlternateRates;

            for (int i = 0; i < fixedTsamudRates.Length; i++)
            {
                Console.Write(fixedTsamudRates[i] + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < fixedNoTsamudRates.Length; i++)
            {
                Console.Write(fixedNoTsamudRates[i] + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < alternateRates.Length; i++)
            {
                Console.Write(alternateRates[i] + " ");
            }
        }







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
