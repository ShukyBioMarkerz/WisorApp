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

      
        // constructor
        public FastSearch(string orderID = "", double loanAmtWanted = 0, double monthlyPmtWanted = 0, 
                uint propertyValue = 0, uint income = 0, uint youngestLenderAge = 0)
        {
            CanRunCalculation = false;

            if ("" == orderID || 0 >= loanAmtWanted || 0 >= monthlyPmtWanted || 0 >= propertyValue || 0 >= income || 0 >= youngestLenderAge)
            {
                // TBD: Shuky - get all params easily
                Boolean _shouldReadLoanParameters = false;
                if (_shouldReadLoanParameters)
                {
                    Console.Write("Enter Order ID: ");
                    CheckInfo.orderID = Console.ReadLine().ToString();
                    Console.Write("Enter loan amount: ");
                    CalculationParameters.loanAmtWanted = double.Parse(Console.ReadLine());
                    Console.Write("Enter desired monthly payment: ");
                    CalculationParameters.monthlyPmtWanted = double.Parse(Console.ReadLine());

                    Console.Write("Enter property value: ");
                    CalculationParameters.propertyValue = uint.Parse(Console.ReadLine());
                    Console.Write("Enter monthly income: ");
                    CalculationParameters.income = uint.Parse(Console.ReadLine());
                    Console.Write("Enter age of youngest borrower: ");
                    CalculationParameters.youngestLenderAge = uint.Parse(Console.ReadLine());
                }
                else
                {
                    Console.WriteLine("NOTICE: Should load the user' parameters already");

                    //CheckInfo.orderID = "123"; // Order ID
                    //CalculationParameters.loanAmtWanted = Convert.ToUInt32(InMemCntrls.MemoryControlsGetValue(MiscConstants.LOAN_AMOUNT)); // (double)1000000; // loan amount
                    //CalculationParameters.monthlyPmtWanted = Convert.ToUInt32(InMemCntrls.MemoryControlsGetValue(MiscConstants.MONTHLY_PAYMENT)); // (double)6000; // desired monthly payment
                    //CalculationParameters.propertyValue = Convert.ToUInt32(InMemCntrls.MemoryControlsGetValue(MiscConstants.PROPERTY_VALUE)); // (uint)2000000; // property value
                    //CalculationParameters.income = Convert.ToUInt32(InMemCntrls.MemoryControlsGetValue(MiscConstants.YEARLY_INCOME)); // (uint)30000; // monthly income:
                    //CalculationParameters.youngestLenderAge = Convert.ToUInt32(InMemCntrls.MemoryControlsGetValue(MiscConstants.AGE)); // (uint)38; // age of youngest borrower
                }
            }

            if ("" != orderID && 0 < loanAmtWanted && 0 < monthlyPmtWanted && 0 < propertyValue && 0 < income && 0 < youngestLenderAge)
            {
                CanRunCalculation = true;
            }

            CalculationParameters.ltv = (CalculationParameters.loanAmtWanted / CalculationParameters.propertyValue);
            CalculationParameters.pti = (CalculationParameters.monthlyPmtWanted / CalculationParameters.income);

            // runSearch();
        }

        public bool runSearch()
        {
            if (CanRunCalculation)
            {
                // Get start time for software
                CheckInfo.softwareOpenTime = DateTime.Now;
                // Define Execution ID
                CheckInfo.fastCheckID = (CheckInfo.softwareOpenTime.Ticks - CheckInfo.startTimeToMeasure).ToString();
                //CheckInfo.fastCheckID = CheckInfo.fastCheckID.Substring(CheckInfo.fastCheckID.Length - 7, 7);

                if (PrintOptions.printMainInConsole == true)
                {
                    Console.WriteLine("\n\tBegin Fast Three Option Check - Version 3.2.1\n\tSoftware Started at " + CheckInfo.softwareOpenTime
                                        + "\n\tAll Rights Reserved - Wisor Technologies Ltd. 2014-2015 \n");
                }
                /*
                Console.Write("Enter loan to value ratio (LTV): ");
                CalculationParameters.ltv = double.Parse(Console.ReadLine());
                Console.Write("Enter payment to income ratio (PTI): ");
                CalculationParameters.pti = double.Parse(Console.ReadLine());
                */

                // Set borrower risk profile for choosing interest rates
                BorrowerProfile bp = new BorrowerProfile();

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

                    if (PrintOptions.printToOutputFile == true)
                    {
                        OutputConstants.outputFile = new OutputFile();
                    }
                    // Print input and parameters
                    if (PrintOptions.printMainInConsole == true)
                    {
                        Console.WriteLine("\nLoan Amount = " + CalculationParameters.loanAmtWanted + "\nTarget monthly payment = "
                                            + CalculationParameters.monthlyPmtWanted + "\n\nThere are " + (CalculationConstants.combinations.GetUpperBound(0) + 1)
                                            + " combinations possible :");
                        for (int i = 0; i <= CalculationConstants.combinations.GetUpperBound(0); i++)
                        {
                            Console.WriteLine((i + 1) + " : " + CalculationConstants.combinations[i, 0] + CalculationConstants.combinations[i, 1]
                                            + CalculationConstants.combinations[i, 2]);
                        }
                    }
                    CheckInfo.searchStartTime = DateTime.Now;

                    // Run through each combination possible for three options
                    for (uint combinationCounter = 0; combinationCounter <= CalculationConstants.combinations.GetUpperBound(0); combinationCounter++)
                    {
                        // Perform three option search for one combination of option types
                        CheckInfo.calculationStartTime = DateTime.Now;
                        DefineOptionTypes(combinationCounter);
                        Console.WriteLine();
                        ThreeOptionSearch search = new ThreeOptionSearch(CalculationParameters.minAmts[(int)Options.options.OPTX],
                                                                                CalculationParameters.maxAmts[(int)Options.options.OPTX]);
                        CheckInfo.calculationEndTime = DateTime.Now;
                        // End of three option search for one combination of option types

                        // Print summary to console
                        if (PrintOptions.printMainInConsole == true)
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
                        if (PrintOptions.printToOutputFile == true)
                        {

                            string dateCreated = CheckInfo.softwareOpenTime.Day.ToString() + "/" + CheckInfo.softwareOpenTime.Month.ToString()
                                                    + "/" + CheckInfo.softwareOpenTime.Year.ToString() + " " + CheckInfo.softwareOpenTime.ToShortTimeString() + ":00";




                            string summaryToFile = "" + CheckInfo.fastCheckID.ToString() + "," + CheckInfo.orderID + "," + dateCreated + ",";





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
                            OutputConstants.outputFile.WriteNewLineInSummaryFile(summaryToFile);
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
                    CheckInfo.softwareCloseTime = DateTime.Now;

                    // Close output file before end.
                    if (PrintOptions.printToOutputFile == true)
                    {
                        OutputConstants.outputFile.CloseOutputFile();
                    }
                    if (PrintOptions.printMainInConsole == true)
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
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("\n*** Calculation time in milliseconds*** {0}", elapsedMs);

            } // CanRunCalculation
            else
            {
                Console.WriteLine("NOTICE: can't run the calculation. CanRunCalculation falge is: " + CanRunCalculation);
            }

            return CanRunCalculation;
        } 


        // ************************************** Define option types according to combination chosen ********************************* //

        public static void DefineOptionTypes(uint combinationToDefine)
        {
            CalculationParameters.optTypes = new OptionTypes(CalculationConstants.combinations[combinationToDefine, 0],
                                                                CalculationConstants.combinations[combinationToDefine, 1],
                                                                    CalculationConstants.combinations[combinationToDefine, 2]);


            //CalculationParameters.optTypes = new OptionTypes((combinationToDefine / 100), ((combinationToDefine - 100) / 10),
            //                       (combinationToDefine - 100 - (10 * ((combinationToDefine - 100) / 10))));
            if (PrintOptions.printFunctionsInConsole == true)
            {
                Console.WriteLine("\nDefining combination for check - "  
                + CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].typeId.ToString()
                + CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].typeId.ToString()
                + CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].typeId.ToString() + " :\n");
            }
        }







    }
}
