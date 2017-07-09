using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace WisorLib
{
    class ThreeOptionSearch
    {
        // General Parameters
        private double minAmtOptX = -1;
        private double maxAmtOptX = -1;
        private double minAmtOptY = -1;
        public double maxAmtOptY = -1;
        private double minAmtOptZ = -1;
        public double maxAmtOptZ = -1;
        private OneDivisionOfAmounts searchOneDivisionOfAmounts = null;
        
        // Counters
        private double amtYCounter = 0;
        public uint numOfCalculations;




        public ThreeOptionSearch(/*double minAmountOptX, double maxAmountOptX,*/ RunEnvironment env)
        {
            minAmtOptX = env.CalculationParameters.minAmts[(int)Options.options.OPTX];
            maxAmtOptX = env.CalculationParameters.maxAmts[(int)Options.options.OPTX]; 
            minAmtOptY = env.CalculationParameters.minAmts[(int)Options.options.OPTY];
            maxAmtOptY = env.CalculationParameters.maxAmts[(int)Options.options.OPTY];
            minAmtOptZ = env.CalculationParameters.minAmts[(int)Options.options.OPTZ];
            maxAmtOptZ = env.CalculationParameters.maxAmts[(int)Options.options.OPTZ];
            if (env.PrintOptions.printFunctionsInConsole == true)
            {
                Console.WriteLine("\nPerforming three option search ...\n");
            }
            PerformFullThreeOptionSearch(env);


        }






        // **************************************************************************************************************************** //
        // ***************** PRIVATE - Performs Full Search for Three Options According to Limit Amounts for Option X ***************** //

        private void PerformFullThreeOptionSearch(RunEnvironment env)
        {
            DateTime tStart = DateTime.Now;

            double percentDone = 0;
            double loopCount = ((maxAmtOptY - minAmtOptY) / CalculationConstants.jumpBetweenAmounts) + 1;
            double percentForOneLoop = ((((1 / loopCount) * 100) * 100) - ((((1 / loopCount) * 100) * 100) % 1)) / 100;
            numOfCalculations = 0;

            // Begin loop for Option 1 amount
            for (double opt1Amt = minAmtOptX; opt1Amt <= maxAmtOptX; opt1Amt += CalculationConstants.jumpBetweenAmounts)
            {
                // Begin loop for Option 2 amount
                for (double opt2Amt = minAmtOptY; opt2Amt <= maxAmtOptY; opt2Amt += CalculationConstants.jumpBetweenAmounts)
                {
                    numOfCalculations++;
                    // Show progress and  calculate remaining time
                    if (env.PrintOptions.printPercentageDone == true)
                    {
                        amtYCounter++;
                        TimeSpan tLoop = DateTime.Now - tStart;
                        tStart = DateTime.Now;
                        percentDone = (amtYCounter / loopCount) * 100;
                        percentDone = ((percentDone * 100) - ((percentDone * 100) % 1)) / 100;
                        Console.Write("\r" + amtYCounter + " / " + loopCount + " - AmountX checked = "
                                            + opt1Amt + " ( " + percentDone + " % done ) - took " + tLoop);
                    }

                    if (((env.CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt) >= minAmtOptZ) &&
                            ((env.CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt) <= maxAmtOptZ))
                    {
                        double opt3Amt = env.CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt;
                        // Perform main thing here ...
                        // Probably the right place to run in different task here
                        if (Share.shouldRunLogicSync)
                        {
                            // Omri - what do we do with the searchOneDivisionOfAmounts object?
                            try
                            {
                                searchOneDivisionOfAmounts = new OneDivisionOfAmounts(opt1Amt, opt2Amt, opt3Amt, env);
                            }
                            catch (ArgumentOutOfRangeException aoore)
                            {
                                Console.WriteLine("NOTICE: PerformFullThreeOptionSearch ArgumentOutOfRangeException occured: " /* + aoore.ToString() */ +
                                    " for opt1Amt: " + opt1Amt + ", opt2Amt: " + opt2Amt + ", opt3Amt: " + opt3Amt);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("NOTICE: PerformFullThreeOptionSearch Exception occured: "  + ex.Message  +
                                    " for opt1Amt: " + opt1Amt + ", opt2Amt: " + opt2Amt + ", opt3Amt: " + opt3Amt);
                                continue;
                            }
                        }
                        else
                        {
                            ManageASync(opt1Amt, opt2Amt, opt3Amt, env);
                            //Console.WriteLine("RETURN DoComputeASync searchOneDivisionOfAmounts: " + searchOneDivisionOfAmounts.plane.totalColumnSearchChecks);
                        }
             
                    }
                }
            }

            Task.WaitAll(TaskList.ToArray());
        }

        /// <summary>
        /// Manage the logic calculation async
        /// </summary>
        /// <returns></returns>


        List<Task> TaskList = new List<Task>();

        private async Task ManageASync(double opt1Amt, double opt2Amt, double opt3Amt, RunEnvironment env)
        {
            TaskList.Add(DoComputeASync2(opt1Amt, opt2Amt, opt3Amt, env));
            //Console.WriteLine("BEFORRRR TaskList contain: " + TaskList.Count);

            //Task.WaitAll(TaskList.ToArray());
            ////Console.WriteLine("AFTERRRRR TaskList.ToArray ");
        }
       

        private async Task DoComputeASync2(double opt1Amt, double opt2Amt, double opt3Amt, RunEnvironment env)
        {
            //Console.WriteLine("BEFORE DoComputeASync2 ");
            var result =  await Task.Run(() => /*searchOneDivisionOfAmounts = */new OneDivisionOfAmounts(opt1Amt, opt2Amt, opt3Amt, env));
            //return searchOneDivisionOfAmounts;
            //Console.WriteLine("AFTER DoComputeASync2 ");
        }

   

    }
}
