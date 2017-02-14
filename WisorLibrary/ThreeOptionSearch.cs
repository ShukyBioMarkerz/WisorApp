using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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





        public ThreeOptionSearch(double minAmountOptX, double maxAmountOptX)
        {
            //minAmtOptX = CalculationParameters.minAmts[(int)Options.options.OPTX];
            //maxAmtOptX = CalculationParameters.maxAmts[(int)Options.options.OPTX];
            minAmtOptX = maxAmountOptX;
            maxAmtOptX = maxAmountOptX;
            minAmtOptY = CalculationParameters.minAmts[(int)Options.options.OPTY];
            maxAmtOptY = CalculationParameters.maxAmts[(int)Options.options.OPTY];
            minAmtOptZ = CalculationParameters.minAmts[(int)Options.options.OPTZ];
            maxAmtOptZ = CalculationParameters.maxAmts[(int)Options.options.OPTZ];
            if (PrintOptions.printFunctionsInConsole == true)
            {
                Console.WriteLine("\nPerforming three option search ...\n");
            }
            PerformFullThreeOptionSearch();


        }






        // **************************************************************************************************************************** //
        // ***************** PRIVATE - Performs Full Search for Three Options According to Limit Amounts for Option X ***************** //

        private void PerformFullThreeOptionSearch()
        {
            DateTime tStart = DateTime.Now;

            double percentDone = 0;
            double loopCount = ((maxAmtOptY - minAmtOptY) / CalculationConstants.jumpBetweenAmounts) + 1;
            double percentForOneLoop = ((((1 / loopCount) * 100) * 100) - ((((1 / loopCount) * 100) * 100) % 1)) / 100;

            // Begin loop for Option 1 amount
            for (double opt1Amt = minAmtOptX; opt1Amt <= maxAmtOptX; opt1Amt += CalculationConstants.jumpBetweenAmounts)
            {
                // Begin loop for Option 2 amount
                for (double opt2Amt = minAmtOptY; opt2Amt <= maxAmtOptY; opt2Amt += CalculationConstants.jumpBetweenAmounts)
                {
                    // Show progress and  calculate remaining time
                    if (PrintOptions.printPercentageDone == true)
                    {
                        amtYCounter++;
                        TimeSpan tLoop = DateTime.Now - tStart;
                        tStart = DateTime.Now;
                        percentDone = (amtYCounter / loopCount) * 100;
                        percentDone = ((percentDone * 100) - ((percentDone * 100) % 1)) / 100;
                        Console.Write("\r" + amtYCounter + " / " + loopCount + " - AmountX checked = "
                                            + opt1Amt + " ( " + percentDone + " % done ) - took " + tLoop);
                    }

                    if (((CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt) >= minAmtOptZ) &&
                            ((CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt) <= maxAmtOptZ))
                    {
                        double opt3Amt = CalculationParameters.loanAmtWanted - opt1Amt - opt2Amt;
                        // Perform main thing here ...
                        searchOneDivisionOfAmounts = new OneDivisionOfAmounts(opt1Amt, opt2Amt, opt3Amt);
                    }
                }
            }
        }
                

    }
}
