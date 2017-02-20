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

            if ("" == env.CheckInfo.orderID || 0 >= env.CalculationParameters.loanAmtWanted || 
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

                // TBD: Shuky - whay to pause??
                // Console.ReadKey();

                // Shuky - measure the elapse time. Start
                var watch = System.Diagnostics.Stopwatch.StartNew();

                if (bp.ShowBorrowerProfile() == (int)CalculationConstants.borrowerProfiles.NOTOK)
                {
                    Console.WriteLine("\nBorrower profile not ok - closing software.");
                }
                else
                {
                    //Rates.ReadInterestRateFileAndUpdateRatesInSoftware(3);

                    //Rates.ExportRatesToCSVFile();
                    //Rates.AddToInterestRatesOnce("NOTSAMUD", -0.46);
                    //Rates.AddToInterestRatesOnce("TSAMUD", +0.14);

                    //if (env.PrintOptions.printToOutputFile == true)
                    //{
                    //    OutputConstants.outputFile = new OutputFile();
                    //}
                    // Print input and parameters
                    if (env.PrintOptions.printMainInConsole == true)
                    {
                        Console.WriteLine("\nLoan Amount = " + env.CalculationParameters.loanAmtWanted + "\nTarget monthly payment = "
                                            + env.CalculationParameters.monthlyPmtWanted + "\n\nThere are " + (CalculationConstants.combinations.GetUpperBound(0) + 1)
                                            + " combinations possible :");
                        for (int i = 0; i <= CalculationConstants.combinations.GetUpperBound(0); i++)
                        {
                            Console.WriteLine((i + 1) + " : " + CalculationConstants.combinations[i, 0] + CalculationConstants.combinations[i, 1]
                                            + CalculationConstants.combinations[i, 2]);
                        }
                    }
                    env.CheckInfo.searchStartTime = DateTime.Now;

                    // Run through each combination possible for three options
                    for (uint combinationCounter = 0; combinationCounter <= CalculationConstants.combinations.GetUpperBound(0); combinationCounter++)
                    {
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
                            Console.WriteLine("\nDone checking combination - " + CalculationConstants.combinations[combinationCounter, 0]
                                                    + CalculationConstants.combinations[combinationCounter, 1]
                                                    + CalculationConstants.combinations[combinationCounter, 2] + " :");
                            if (ResultsOutput.bestCompositionSoFar != null)
                            {
                                Console.WriteLine("\nBest composition so far :\n" + ResultsOutput.bestCompositionSoFar.ToString());
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

                            string dateCreated = env.CheckInfo.softwareOpenTime.Day.ToString() + "/" + env.CheckInfo.softwareOpenTime.Month.ToString()
                                                    + "/" + env.CheckInfo.softwareOpenTime.Year.ToString() + " " + env.CheckInfo.softwareOpenTime.ToShortTimeString() + ":00";




                            string summaryToFile = "" + env.CheckInfo.fastCheckID.ToString() + "," + env.CheckInfo.orderID + "," + dateCreated + ",";





                            if (ResultsOutput.bestCompositionSoFar != null)
                            {
                                summaryToFile += ResultsOutput.bestCompositionSoFar.ToString();
                            }
                            else
                            {
                                summaryToFile += (CalculationConstants.combinations[combinationCounter, 0] + 4)
                                                    + "," + "," + "," + "," + "," + (CalculationConstants.combinations[combinationCounter, 1] + 4)
                                                    + "," + "," + "," + "," + "," + (CalculationConstants.combinations[combinationCounter, 2] + 4);
                            }
                            env.OutputFile.WriteNewLineInSummaryFile(summaryToFile);
                        }
                        if (ResultsOutput.bestCompositionSoFar != null)
                        {
                            if ((ResultsOutput.bestComposition == null) ||
                                ((ResultsOutput.bestComposition != null) && (ResultsOutput.bestCompositionSoFar.ttlPay < ResultsOutput.bestComposition.ttlPay)))
                            {
                                ResultsOutput.bestComposition = ResultsOutput.bestCompositionSoFar;
                            }
                            ResultsOutput.bestCompositionSoFar = null;
                        }
                    }
                    // Get end time for software
                    env.CheckInfo.softwareCloseTime = DateTime.Now;

                    // Close output file before end.
                    if (env.PrintOptions.printToOutputFile == true)
                    {
                        env.OutputFile.CloseOutputFile(env.CheckInfo);
                    }
                    if (env.PrintOptions.printMainInConsole == true)
                    {
                        if (ResultsOutput.bestComposition != null)
                        {
                            Console.WriteLine("\nBest composition in search is for combination "
                                            + ResultsOutput.bestComposition.opts[(int)Options.options.OPTX].optType
                                            + ResultsOutput.bestComposition.opts[(int)Options.options.OPTY].optType
                                            + ResultsOutput.bestComposition.opts[(int)Options.options.OPTZ].optType + " :\n"
                                            + ResultsOutput.bestComposition.ToString());

                        }
                        else
                        {
                            // TBD: Shuky - what is the meaning?
                            Console.WriteLine("\nNo composition found in search");

                        }
                    }


                }

                // Shuky - measure the elapse time. Stop
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("\n*** Calculation time in milliseconds*** {0}", elapsedMs);

            } // CanRunCalculation
            else
            {
                Console.WriteLine("NOTICE: can't run the calculation. CanRunCalculation falge is: " + CanRunCalculation);
            }

            return new RunLoanDetails(env.CheckInfo.orderID, Convert.ToInt32(CanRunCalculation), elapsedMs, env.OutputFile.OutputFilename);
        } 


        // ************************************** Define option types according to combination chosen ********************************* //

        private void DefineOptionTypes(uint combinationToDefine, RunEnvironment env)
        {
            env.CalculationParameters.optTypes = new OptionTypes(CalculationConstants.combinations[combinationToDefine, 0],
                                                                CalculationConstants.combinations[combinationToDefine, 1],
                                                                    CalculationConstants.combinations[combinationToDefine, 2], env);


            //CalculationParameters.optTypes = new OptionTypes((combinationToDefine / 100), ((combinationToDefine - 100) / 10),
            //                       (combinationToDefine - 100 - (10 * ((combinationToDefine - 100) / 10))));
            if (env.PrintOptions.printFunctionsInConsole == true)
            {
                Console.WriteLine("\nDefining combination for check - "  
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].typeId.ToString()
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].typeId.ToString()
                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].typeId.ToString() + " :\n");
            }
        }







    }
}
