using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class SearchAreaInPlane
    {
        // General Parameters
        public Option[] cornerAMaxTimeXMinTimeY = { null, null };
        public Option[] cornerBMinTimeXMaxTimeY = { null, null };
        public uint numOfCells = 0;


        // Finding new corners and creating search area
        private FinalLimitPoint[] limitPoints = { null, null };
        private Option[] optsA = { null, null };
        private Option[] optsB = { null, null };
        

        // Searching in search area
        public double targetTwoOptionPmt = -1;
        private bool printOrNo = false;


        // Columns and column search
        public OneColumnForSearch[] columns = null;
        private bool[] columnsChecked = null;

        // Lists of matching points
        public SavedMatchesBeforeCheck savedMatches = new SavedMatchesBeforeCheck();

        // Counters
        private uint numOfLines = 0, numOfColumns = 0;





        public SearchAreaInPlane(FinalLimitPoint[] finalPoints, RunEnvironment env)
        {
            limitPoints[(int)Options.limitPointsLetters.A] = finalPoints[(int)Options.limitPointsLetters.A];
            limitPoints[(int)Options.limitPointsLetters.B] = finalPoints[(int)Options.limitPointsLetters.B];
            targetTwoOptionPmt = finalPoints[(int)Options.options.OPTX].targetTwoOptionPmt;

            printOrNo = env.PrintOptions.printFunctionsInConsole;
          

            cornerAMaxTimeXMinTimeY = FindCornerAPointMaxTimeXMinTimeY(env);
            cornerBMinTimeXMaxTimeY = FindCornerBPointMinTimeXMaxTimeY(env);

            
            if ((cornerBMinTimeXMaxTimeY[(int)Options.options.OPTY].optTime - 
                cornerAMaxTimeXMinTimeY[(int)Options.options.OPTY].optTime > 0)                            
                && (cornerAMaxTimeXMinTimeY[(int)Options.options.OPTX].optTime - 
                    cornerBMinTimeXMaxTimeY[(int)Options.options.OPTX].optTime > 0))                            
            {
                numOfLines = ((cornerBMinTimeXMaxTimeY[(int)Options.options.OPTY].optTime -
                                cornerAMaxTimeXMinTimeY[(int)Options.options.OPTY].optTime) /
                                env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.timeJump) + 1;
                numOfColumns = ((cornerAMaxTimeXMinTimeY[(int)Options.options.OPTX].optTime -
                                cornerBMinTimeXMaxTimeY[(int)Options.options.OPTX].optTime) /
                                env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump) + 1;
                numOfCells = numOfLines * numOfColumns;
                columns = new OneColumnForSearch[numOfColumns];
                columnsChecked = new bool[numOfColumns];
            }





        }



        

        // **************************************************************************************************************************** //
        // ***************************************** Find Limit time for Corner Point Check ******************************************* //

        private bool FindLimitTimeForCornerPoint(uint pointLetter, uint pointNumber)
        {
            switch (pointLetter)
            {
                case (int)Options.limitPointsLetters.A:
                {
                    switch (pointNumber)
                    {
                        case (int)Options.limitPointsNumbers.ONE:
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Limit for timeX = "
                                                    + limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optTime);
                                }
                                if (limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optTime >
                                        limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optTime)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        case (int)Options.limitPointsNumbers.TWO:
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Limit for timeY = "
                                                    + limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optTime);
                                }
                                if (limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optTime <
                                        limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optTime)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        default:
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Shouldnt happen");
                                }
                                return false;
                            }
                    }                    
                }
                case (int)Options.limitPointsLetters.B:
                    {
                        switch (pointNumber)
                        {
                            case (int)Options.limitPointsNumbers.ONE:
                                {
                                    if (printOrNo == true)
                                    {
                                        Console.WriteLine("Limit for timeY = "
                                                        + limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optTime);
                                    }
                                    if (limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optTime >
                                            limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optTime)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            case (int)Options.limitPointsNumbers.TWO:
                                {
                                    if (printOrNo == true)
                                    {
                                        Console.WriteLine("Limit for timeX = "
                                                        + limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optTime);
                                    }
                                    if (limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optTime <
                                            limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optTime)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            default:
                                {
                                    if (printOrNo == true)
                                    {
                                        Console.WriteLine("Shouldnt happen");
                                    }
                                    return false;
                                }
                        }
                    }
                default:
                    {
                        if (printOrNo == true)
                        {
                            Console.WriteLine("Shouldnt happen");
                        }
                        return false;
                    }
            }
        }




        // **************************************************************************************************************************** //
        // ***************************************** Find Corner Point A(maxTimeX, minTimeY) ****************************************** //

        private Option[] FindCornerAPointMaxTimeXMinTimeY(RunEnvironment env)
        {
            optsA[(int)Options.options.OPTX] = limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX];
            optsA[(int)Options.options.OPTY] = limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY];

            if (printOrNo == true)
            {
                Console.WriteLine("\nFinding corner for final limit point "
                                    + Options.letters[limitPoints[(int)Options.limitPointsLetters.A].letter]
                                    + Options.numbers[limitPoints[(int)Options.limitPointsLetters.A].number]
                                    + "\n(X,Y) = (" + limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optTime
                                    + "," + limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optTime + ")\n");
                                    
            }
            bool limitTimeChecker = FindLimitTimeForCornerPoint(limitPoints[(int)Options.limitPointsLetters.A].letter,
                                                limitPoints[(int)Options.limitPointsLetters.A].number);
            if (printOrNo == true)
            {
                Console.WriteLine("Limit Time Checker = " + limitTimeChecker);
            }
            if (limitPoints[(int)Options.limitPointsLetters.A].exactMatchFound == true)
            {
                switch (limitPoints[(int)Options.limitPointsLetters.A].number)
                {
                    case (int)Options.limitPointsNumbers.ONE:
                        {
                            if (limitTimeChecker == true)
                            {                                
                                optsA[(int)Options.options.OPTX] = 
                                    new Option(limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optType,
                                        limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optAmt,
                                        (limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTX].optTime -
                                        env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump), env);
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Moving corner to cell with smaller timeX\nNew timeX = "
                                                        + optsA[(int)Options.options.OPTX].optTime);
                                }
                                return optsA;
                            }
                            else
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine(Options.letters[limitPoints[(int)Options.limitPointsLetters.A].letter]
                                                        + Options.numbers[limitPoints[(int)Options.limitPointsLetters.A].number]
                                                        + " is at limit for moving to smaller timeX\n");
                                }
                                return optsA;
                            }
                        }
                    case (int)Options.limitPointsNumbers.TWO:
                        {
                            if (limitTimeChecker == true)
                            {
                                optsA[(int)Options.options.OPTY] =
                                    new Option(limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optType,
                                        limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optAmt,
                                        (limitPoints[(int)Options.limitPointsLetters.A].opts[(int)Options.options.OPTY].optTime +
                                        env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.timeJump), env);
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Moving corner to cell with larger timeY\nNew timeY = "
                                                        + optsA[(int)Options.options.OPTY].optTime);
                                }
                                return optsA;
                            }
                            else
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine(Options.letters[limitPoints[(int)Options.limitPointsLetters.A].letter]
                                                        + Options.numbers[limitPoints[(int)Options.limitPointsLetters.A].number]
                                                        + " is at limit for moving to larger timeY\n");
                                }
                                return optsA;
                            }
                        }
                    default:
                        {
                            // Add Throw - Exeption ...
                            return null;
                        }
                }
            }
            else if (limitPoints[(int)Options.limitPointsLetters.A].exactMatchFound == false)
            {
                return optsA;
            }
            else
            {
                // Add Throw - Exeption ...
                return null;
            }
        }





        // **************************************************************************************************************************** //
        // ***************************************** Find Corner Point B(minTimeX, maxTimeY) ****************************************** //

        private Option[] FindCornerBPointMinTimeXMaxTimeY(RunEnvironment env)
        {
            optsB[(int)Options.options.OPTX] = limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX];
            optsB[(int)Options.options.OPTY] = limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY];
            if (printOrNo == true)
            {
                Console.WriteLine("\nFinding corner for final limit point "
                                    + Options.letters[limitPoints[(int)Options.limitPointsLetters.B].letter]
                                    + Options.numbers[limitPoints[(int)Options.limitPointsLetters.B].number]
                                    + "\n(X,Y) = (" + limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optTime
                                    + "," + limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optTime + ")\n");
            }
            bool limitTimeChecker = FindLimitTimeForCornerPoint(limitPoints[(int)Options.limitPointsLetters.B].letter,
                                                    limitPoints[(int)Options.limitPointsLetters.B].number);
            if (printOrNo == true)
            {
                Console.WriteLine("Limit Time Checker = " + limitTimeChecker);
            }
            if (limitPoints[(int)Options.limitPointsLetters.B].exactMatchFound == true)
            {
                switch (limitPoints[(int)Options.limitPointsLetters.B].number)
                {
                    case (int)Options.limitPointsNumbers.ONE:
                        {
                            if (limitTimeChecker == true)
                            {
                                optsB[(int)Options.options.OPTY] =
                                    new Option(limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optType,
                                        limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optAmt,
                                        (limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTY].optTime -
                                        env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY].product.timeJump), env);
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Moving corner to cell with smaller timeY\nNew timeY = "
                                                        + optsB[(int)Options.options.OPTY].optTime);                                  
                                }
                                return optsB;
                            }
                            else
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine(Options.letters[limitPoints[(int)Options.limitPointsLetters.B].letter]
                                                        + Options.numbers[limitPoints[(int)Options.limitPointsLetters.B].number]
                                                        + " is at limit for moving to smaller timeY\n");
                                }
                                return optsB;
                            }
                        }
                    case (int)Options.limitPointsNumbers.TWO:
                        {
                            if (limitTimeChecker == true)
                            {
                                optsB[(int)Options.options.OPTX] =
                                    new Option(limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optType,
                                        limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optAmt,
                                        (limitPoints[(int)Options.limitPointsLetters.B].opts[(int)Options.options.OPTX].optTime +
                                        env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX].product.timeJump), env);
                                if (printOrNo == true)
                                {
                                    Console.WriteLine("Moving corner to cell with larger timeX\nNew timeX = "
                                                        + optsB[(int)Options.options.OPTX].optTime);
                                }
                                return optsB;
                            }
                            else
                            {
                                if (printOrNo == true)
                                {
                                    Console.WriteLine(Options.letters[limitPoints[(int)Options.limitPointsLetters.B].letter]
                                                        + Options.numbers[limitPoints[(int)Options.limitPointsLetters.B].number]
                                                        + " is at limit for moving to larger timeX\n");
                                }
                                return optsB;
                            }
                        }
                    default:
                        {
                            // Add Throw - Exeption ...
                            return null;
                        }
                }
            }
            else if (limitPoints[(int)Options.limitPointsLetters.B].exactMatchFound == false)
            {
                return optsB;
            }
            else
            {
                // Add Throw - Exeption ...
                return null;
            }           
        }




        // **************************************************************************************************************************** //
        // ********************************************* Search One Column and Update Counters **************************************** //

        public void SearchOneColumnAndUpdateCounters(int columnIndex, RunEnvironment env)
        {
            if (columnsChecked[columnIndex] == false)
            {
                if (columnIndex == 0)
                {
                    columns[columnIndex] = new OneColumnForSearch(targetTwoOptionPmt, columnIndex,
                                                    cornerAMaxTimeXMinTimeY, cornerBMinTimeXMaxTimeY, env);

                }
                else
                {
                    
                    columns[columnIndex] = new OneColumnForSearch(targetTwoOptionPmt, columnIndex,
                                                    columns[columnIndex - 1].nextColumnStart, cornerBMinTimeXMaxTimeY, env);
                }
                savedMatches.InsertListOfMatches(columns[columnIndex].savedMatches);
                columnsChecked[columnIndex] = true;
            }
        }




        // **************************************************************************************************************************** //
        // ****************************************** Print Final Search Area Borders To String *************************************** //

        public override string ToString()
        {
            if (cornerAMaxTimeXMinTimeY != null)
            {
                return "\nSearch Area Borders:\nTop right corner A(X,Y) = (" + cornerAMaxTimeXMinTimeY[(int)Options.options.OPTX].optTime
                            + "," + cornerAMaxTimeXMinTimeY[(int)Options.options.OPTY].optTime + ")\nBottom left corner B(X,Y) = ("
                            + cornerBMinTimeXMaxTimeY[(int)Options.options.OPTX].optTime + ","
                            + cornerBMinTimeXMaxTimeY[(int)Options.options.OPTY].optTime +
                            ")\nNumber of Columns = " + numOfColumns + "\nNumber of Lines = " + numOfLines + "\n";
            }
            else
            {
                return "\nSearch Area still not defined.\n";
            }
        }
    }
}
