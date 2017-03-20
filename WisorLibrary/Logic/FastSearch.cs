using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    public class FastSearch
    {

        public bool CanRunCalculation { get; set; }

        RunEnvironment env;

        // constructor
        public FastSearch(RunEnvironment Env)
        {
            CanRunCalculation = false;
            env = Env;

            if (MiscConstants.UNDEFINED_STRING == env.CheckInfo.orderID || 0 >= env.CalculationParameters.loanAmtWanted || 
                0 >= env.CalculationParameters.monthlyPmtWanted || 0 >= env.CalculationParameters.propertyValue || 
                0 >= env.CalculationParameters.income || 0 >= env.CalculationParameters.youngestLenderAge)
            {
                Console.WriteLine("NOTICE: Should load the user' parameters already");
            }
            else
            {
                CanRunCalculation = true;
           }
        }

        public RunLoanDetails runSearch()
        {
            long elapsedMs = 0;

            if (CanRunCalculation)
            {
                // Get start time for software
                env.CheckInfo.softwareOpenTime = DateTime.Now;
                    
                if (env.PrintOptions.printMainInConsole == true)
                {
                    Console.WriteLine("\n\tBegin Fast Three Option Check - Version 3.2.1\n\tSoftware Started at " + env.CheckInfo.softwareOpenTime
                                        + "\n\tAll Rights Reserved - Wisor Technologies Ltd. 2014-2015 \n");
                }
   
                // Set borrower risk profile for choosing interest rates
                BorrowerProfile bp = new BorrowerProfile(env);

                if (bp.ShowBorrowerProfile() == (int)CalculationConstants.borrowerProfiles.NOTOK)
                {
                    Console.WriteLine("\nBorrower profile not ok - closing software.");
                }
                else
                {
                    if (env.PrintOptions.printMainInConsole == true)
                    {
                        Console.WriteLine("\nLoan Amount = " + env.CalculationParameters.loanAmtWanted + "\nTarget monthly payment = "
                                            + env.CalculationParameters.monthlyPmtWanted + "\n\nThere are " + (CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0) + 1)
                                            + " combinations possible :");

                        CalculationConstants.PrintCombination(Share.theMarket);
                        
                    }
                    env.CheckInfo.searchStartTime = DateTime.Now;

                    // Run through each combination possible for three options
                    for (uint combinationCounter = 0; combinationCounter <= CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0); combinationCounter++)
                    {
                        // should each combination write a different file
                        if (Share.ShouldEachCombinationRunSeparetly)
                        {
                            string com0 = CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 0];
                            string com1 = CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 1];
                            string com2 = CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 2];
                            string additionalName = MiscConstants.NAME_SEP_CHAR + com0 + MiscConstants.NAME_SEP_CHAR + 
                                com1 + MiscConstants.NAME_SEP_CHAR + com2 + MiscConstants.NAME_SEP_CHAR;
                            // change the output file
                            env.CreateTheOutputFiles(env.theLoan, additionalName);
                       }

                        // Perform three option search for one combination of option types
                        env.CheckInfo.calculationStartTime = DateTime.Now;
                        DefineOptionTypes(combinationCounter, env);
                        Console.WriteLine();
                        ThreeOptionSearch search = new ThreeOptionSearch(env.CalculationParameters.minAmts[(int)Options.options.OPTX],
                                               env.CalculationParameters.maxAmts[(int)Options.options.OPTX], env);
                        env.CheckInfo.calculationEndTime = DateTime.Now;
                        // End of three option search for one combination of option types

                        // Print summary to console
                        if (env.PrintOptions.printMainInConsole == true)
                        {
                            Console.WriteLine("\nDone checking combination - " + (combinationCounter + 1).ToString() + " out of: " +
                                (CalculationConstants.GetCombination(Share.theMarket).GetUpperBound(0) + 1) + " : " +
                                CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 0] + " " +
                                CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 1] + " " +
                                CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 2] + " :");
                            if (env.resultsOutput.bestCompositionSoFar != null)
                            {
                                Console.WriteLine("\nBest composition so far :\n" + env.resultsOutput.bestCompositionSoFar.ToString());
                            }
                            else
                            {
                                // TBD: Shuky - what is the meaning?
                                Console.WriteLine("\nNo composition found");
                            }
                        }
                        // Print summary to file
                        if (env.PrintOptions.printToOutputFile == true)
                        {
                            //string dateCreated = env.CheckInfo.softwareOpenTime.Day.ToString() + "/" + env.CheckInfo.softwareOpenTime.Month.ToString()
                            //                        + "/" + env.CheckInfo.softwareOpenTime.Year.ToString() + " " + env.CheckInfo.softwareOpenTime.ToShortTimeString() + ":00";
                            //string summaryToFile = "" + env.CheckInfo.fastCheckID.ToString() + "," + env.CheckInfo.orderID + "," + dateCreated + ",";
                            string summaryToFile = null;
                            if (env.resultsOutput.bestCompositionSoFar != null)
                            {
                                summaryToFile += env.resultsOutput.bestCompositionSoFar.ToString();
                            }
                            else
                            {
                                summaryToFile += (CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 0])
                                                    + "," + "," + "," + "," + (CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 1])
                                                    + "," + "," + "," + "," + (CalculationConstants.GetCombination(Share.theMarket)[combinationCounter, 2]);
                            }
                            env.WriteToOutputFile(summaryToFile);
                        }
                        if (env.resultsOutput.bestCompositionSoFar != null)
                        {
                            if ((env.resultsOutput.bestComposition == null) ||
                                ((env.resultsOutput.bestComposition != null) && (env.resultsOutput.bestCompositionSoFar.ttlPay < env.resultsOutput.bestComposition.ttlPay)))
                            {
                                env.resultsOutput.bestComposition = env.resultsOutput.bestCompositionSoFar;
                            }
                            env.resultsOutput.bestCompositionSoFar = null;
                        }
                    }
                    // Get end time for software
                    env.CheckInfo.softwareCloseTime = DateTime.Now;

                    // Close output file before end.

                    env.WriteToOutputFile("\nCalculation ended at " + env.CheckInfo.softwareCloseTime);
                    env.WriteToOutputFile("Software runtime " + (env.CheckInfo.softwareCloseTime - env.CheckInfo.softwareOpenTime));
                    env.WriteToOutputFile("Search runtime " + (env.CheckInfo.softwareCloseTime - env.CheckInfo.searchStartTime));

                    env.CloseTheOutputFiles();
        
                    if (env.PrintOptions.printMainInConsole == true)
                    {
                        if (env.resultsOutput.bestComposition != null)
                        {
                            Console.WriteLine("\nBest composition in search is for combination "
                                            + env.resultsOutput.bestComposition.opts[(int)Options.options.OPTX].optType
                                            + env.resultsOutput.bestComposition.opts[(int)Options.options.OPTY].optType
                                            + env.resultsOutput.bestComposition.opts[(int)Options.options.OPTZ].optType + " :\n"
                                            + env.resultsOutput.bestComposition.ToString());
                        }
                        else
                        {
                            // TBD: Shuky - what is the meaning?
                            Console.WriteLine("\nNo composition found in search");

                        }
                    }


                }

            } // CanRunCalculation
            else
            {
                Console.WriteLine("NOTICE: can't run the calculation. CanRunCalculation falge is: " + CanRunCalculation);
            }
            if (Share.shouldPrintCounters)
            {
                Console.WriteLine("\nSavedCompositionsCounter: " + String.Format("{0:#,###,###}", Share.SavedCompositionsCounter) +
                    "\n, CalculateLuahSilukinCounter: " + String.Format("{0:#,###,###}", Share.CalculateLuahSilukinCounter) +
                    "\n, CalculatePmtCounter: " + String.Format("{0:#,###,###}", Share.CalculatePmtCounter) +
                    "\n, CalculatePmtFromCalculateLuahSilukinCounter: " + String.Format("{0:#,###,###}", Share.CalculatePmtFromCalculateLuahSilukinCounter) +
                    "\n, CalculateLuahSilukinCounterNOTInFirstTimePeriod: " + String.Format("{0:#,###,###}", Share.CalculateLuahSilukinCounterNOTInFirstTimePeriod) +
                    "\n, CalculateLuahSilukinCounterInFirstTimePeriod: " + String.Format("{0:#,###,###}", Share.CalculateLuahSilukinCounterInFirstTimePeriod) +
                    "\n, CalculateLuahSilukinCounterIndexUsedFirstTimePeriod: " + String.Format("{0:#,###,###}", Share.CalculateLuahSilukinCounterIndexUsedFirstTimePeriod) +
                    "\n, RateCounter: " + String.Format("{0:#,###,###}", Share.RateCounter) +
                    "\n, counterOfOneDivisionOfAmounts: " + String.Format("{0:#,###,###}", Share.counterOfOneDivisionOfAmounts) +
                    "\n, OptionObjectCounter: " + String.Format("{0:#,###,###}", Share.OptionObjectCounter));
            }

            return new RunLoanDetails(env.CheckInfo.orderID, Convert.ToInt32(CanRunCalculation), elapsedMs, env.GetOutputFileName());
        } 


        // ************************************** Define option types according to combination chosen ********************************* //

        private void DefineOptionTypes(uint combinationToDefine, RunEnvironment env)
        {
            env.CalculationParameters.optTypes = new OptionTypes(
                Share.combinations4market[combinationToDefine, 0],
                Share.combinations4market[combinationToDefine, 1],
                Share.combinations4market[combinationToDefine, 2], env);

            if (env.PrintOptions.printFunctionsInConsole == true)
            {
                Console.WriteLine("\nDefining combination for check - "  
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.productID.numberID + " "
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.productID.numberID + " "
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.productID.numberID + " :\n");
            }
        }







    }
}
