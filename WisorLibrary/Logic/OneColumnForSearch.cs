using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class OneColumnForSearch
    {
        // General Parameters
        public int columnNum = -1;
        private Option[] startingPoint = { null, null };
        private Option[] limitPoint = { null, null };

        private bool printOrNo = false;
        private double targetTwoOptionPmt = -1;

        public Option[] nextColumnStart = { null, null };
        public Option nextTimeY = null;

        // Parameters for searching column
        private enum saves { NONE, FIRST, MORE };

        // List of matching points
        public SavedMatchesBeforeCheck savedMatches = new SavedMatchesBeforeCheck();

        // Counters
        public int numOfMatches = 0;



        public OneColumnForSearch(double targetTwoOptPmt, int columnNumber, Option[] startPointForCheck, 
            Option[] limitPointForCheck, RunEnvironment env)
        {
            columnNum = columnNumber;
            startingPoint = startPointForCheck;
            printOrNo = env.PrintOptions.printFunctionsInConsole;
            targetTwoOptionPmt = targetTwoOptPmt;
            limitPoint = limitPointForCheck;


            if (printOrNo == true)
            {
                Console.WriteLine("\nNew Column number " + columnNum + "\nMaximum time for column = " + limitPoint[(int)Options.options.OPTY].optTime
                                    + "\nStarting Point X = " + startingPoint[(int)Options.options.OPTX].ToString()
                                    + "\nStarting Point Y = " + startingPoint[(int)Options.options.OPTY].ToString() + "\n");
            }

            nextColumnStart = SearchOneColumn(startingPoint, env);
            SavePointForNextColumn(env);

        }
        




        // **************************************************************************************************************************** //
        // *********************************************** Check if Pmt is in range  ************************************************** //

        private int ChecKPmt(Option optionX, Option optionY)
        {
            if ((optionX.optPmt + optionY.optPmt) < (targetTwoOptionPmt - CalculationConstants.largeDev))
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nPmt(X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") = "
                                        + (optionX.optPmt + optionY.optPmt).ToString() + "\nPmt(X,Y) Too small\n");
                }
                return (int)Options.pmtRange.TOOSMALL;
            }
            else if ((optionX.optPmt + optionY.optPmt) > (targetTwoOptionPmt + CalculationConstants.smallDev))
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nPmt(X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") = "
                                        + (optionX.optPmt + optionY.optPmt).ToString() + "\nPmt(X,Y) Too large\n");
                }
                return (int)Options.pmtRange.TOOLARGE;
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nPmt(X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") = "
                                        + (optionX.optPmt + optionY.optPmt).ToString() + "\nPmt(X,Y) In range !\n");
                }
                return (int)Options.pmtRange.INRANGE;
            }
        }



        // **************************************************************************************************************************** //
        // ***************************************** PRIVATE - Search Down One Single Column ****************************************** //

        private Option[] SearchOneColumn(Option[] startingPoint, RunEnvironment env)
        {
            int pmtChecker = ChecKPmt(startingPoint[(int)Options.options.OPTX], startingPoint[(int)Options.options.OPTY]);

            switch (pmtChecker)
            {
                case (int)Options.pmtRange.INRANGE:
                    {
                        if (printOrNo == true)
                        {
                            Console.WriteLine("Point (X,Y) (" + startingPoint[(int)Options.options.OPTX].optTime + ","
                                            + startingPoint[(int)Options.options.OPTY].optTime + ") is in range !\n");
                        }
                        // Add - Save Point as composition for future calculation of total payment
                        numOfMatches++;
                        savedMatches.InsertMatchToList(startingPoint[(int)Options.options.OPTX],
                                            startingPoint[(int)Options.options.OPTY]);
                        if (numOfMatches == 1)
                        {
                            nextTimeY = startingPoint[(int)Options.options.OPTY];
                            if (printOrNo == true)
                            {
                                Console.WriteLine("Point (X,Y) (" + startingPoint[(int)Options.options.OPTX].optTime + ","
                                                + startingPoint[(int)Options.options.OPTY].optTime + ") saved for next column\n");
                            }
                        }
                        if (startingPoint[(int)Options.options.OPTY].optTime < limitPoint[(int)Options.options.OPTY].optTime)
                        {
                            startingPoint[(int)Options.options.OPTY] = new Option(startingPoint[(int)Options.options.OPTY].optType,
                                                                startingPoint[(int)Options.options.OPTY].optAmt,
                                                                (startingPoint[(int)Options.options.OPTY].optTime
                                                                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.timeJump));
                            return SearchOneColumn(startingPoint, env);
                        }
                        else
                        {

                            return startingPoint;

                        }
                    }
                case (int)Options.pmtRange.TOOLARGE:
                    {
                        // Go down one cell
                        if (printOrNo == true)
                        {
                            Console.WriteLine("Point (X,Y) (" + startingPoint[(int)Options.options.OPTX].optTime + ","
                                            + startingPoint[(int)Options.options.OPTY].optTime + ") - Pmt too large"
                            + "\nLimit Time Y = " + limitPoint[(int)Options.options.OPTY].optTime + "\n");
                        }
                        if (startingPoint[(int)Options.options.OPTY].optTime < limitPoint[(int)Options.options.OPTY].optTime)
                        {
                            startingPoint[(int)Options.options.OPTY] = new Option(startingPoint[(int)Options.options.OPTY].optType,
                                                                startingPoint[(int)Options.options.OPTY].optAmt,
                                                                (startingPoint[(int)Options.options.OPTY].optTime
                                                                + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.timeJump));
                            return SearchOneColumn(startingPoint, env);
                        }
                        else
                        {
                            if (numOfMatches == 0)
                            {
                                nextTimeY = startingPoint[(int)Options.options.OPTY];
                            }
                            return startingPoint;

                        }
                    }
                case (int)Options.pmtRange.TOOSMALL:
                    {
                        if (printOrNo == true)
                        {
                            Console.WriteLine("Point (X,Y) (" + startingPoint[(int)Options.options.OPTX].optTime + ","
                                            + startingPoint[(int)Options.options.OPTY].optTime + ") - Pmt too small\n");
                        }
                        // End column search. 
                        if (numOfMatches == 0)
                        {
                            nextTimeY = startingPoint[(int)Options.options.OPTY];
                        }
                        return startingPoint;                        
                    }
                default:
                    {
                        return null;
                    }
            }            
        }





        // **************************************************************************************************************************** //
        // ************************************************* Save Point For Next Column *********************************************** //

        private void SavePointForNextColumn(RunEnvironment env)
        {
            if (nextColumnStart[(int)Options.options.OPTX].optTime > limitPoint[(int)Options.options.OPTX].optTime)
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Time X for next column = " + nextColumnStart[(int)Options.options.OPTX].optTime
                        + " - " + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump
                        + " = " + (nextColumnStart[(int)Options.options.OPTX].optTime -
                        env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump).ToString() + "\n");                      
                }
                nextColumnStart[(int)Options.options.OPTX] = new Option(nextColumnStart[(int)Options.options.OPTX].optType,
                                                                    nextColumnStart[(int)Options.options.OPTX].optAmt,
                                                                    (startingPoint[(int)Options.options.OPTX].optTime
                                                                    - env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump));
                nextColumnStart[(int)Options.options.OPTY] = nextTimeY;
                if (printOrNo == true)
                {
                    Console.WriteLine("Time for next column :\n" + nextColumnStart[(int)Options.options.OPTX].ToString()
                                        + nextColumnStart[(int)Options.options.OPTY].ToString() + "\n");
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Time X for next column = " + nextColumnStart[(int)Options.options.OPTX].optTime + " is at limit\n");                        
                }

            }
        }



        // **************************************************************************************************************************** //
        // *************************************************** Print Column Details *************************************************** //

        public override string ToString()
        {
            if (nextColumnStart != null)
            {
                return "Column number " + columnNum + "\n" + "\nStarting Point X = " + startingPoint[(int)Options.options.OPTX].ToString()
                        + "\nStarting Point Y = " + startingPoint[(int)Options.options.OPTY].ToString()
                        + "\nLimit Time Y = " + limitPoint[(int)Options.options.OPTY].optTime + "\nPoint for next column X = "
                        + nextColumnStart[(int)Options.options.OPTX].ToString() + "\nPoint for next column Y = "
                        + nextColumnStart[(int)Options.options.OPTY].ToString() + "\n";
            }
            else
            {
                return "Column is empty.\n";
            }
        }


    }
}
