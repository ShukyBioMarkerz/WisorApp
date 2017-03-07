using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class FinalLimitPoint
    {
        // General Parameters
        private InitialLimitPoint initialPoint = null;
        public uint letter = 0;
        public uint number = 0;
        public Option[] opts = { null, null };
        private OneOptType[] optTypes = { null, null };
        private bool printOrNo = false;
        public double targetTwoOptionPmt = -1;
        public bool exactMatchFound = false;

        // Parameters for Expanding Point
        private Option[] optsForSave = { null, null };
        private Option[] expandingOpts = { null, null };
        private bool a2b2NoMatchBool = true;    // For A2B2 Expansion with no match in binary search

        // List for matching points
        public SavedMatchesBeforeCheck savedMatches = new SavedMatchesBeforeCheck();



        public FinalLimitPoint(InitialLimitPoint initialLimitPoint, RunEnvironment env)
        {
            initialPoint = initialLimitPoint;
            printOrNo = env.PrintOptions.printFunctionsInConsole;
            letter = initialPoint.letter;
            number = initialPoint.number;
            targetTwoOptionPmt = initialLimitPoint.targetTwoOptionPmt;
            exactMatchFound = initialLimitPoint.matchOrNo;
            if (exactMatchFound == true)
            {
                savedMatches.InsertMatchToList(initialPoint.opts[(int)Options.options.OPTX],
                                                            initialPoint.opts[(int)Options.options.OPTY]);
            }
            InsertOptionsAccordingToLetter(env);

            if (printOrNo == true)
            {
                Console.WriteLine("Looking for expanded point " + Options.letters[(int)letter] + Options.numbers[(int)number]
                                    + " (Match = " + initialPoint.matchOrNo + ")\nOptX Type = "
                                    + optTypes[(int)Options.options.OPTX].product.productID.numberID + "\nOptY Type = "
                                    + optTypes[(int)Options.options.OPTY].product.productID.numberID + "\nOptX = " + expandingOpts[(int)Options.options.OPTX].ToString()
                                    + "\nOptY = " + expandingOpts[(int)Options.options.OPTY].ToString() + "\n");
            }
            InsertOptionsForSavedPoint(ExpandPoint(optTypes[(int)Options.options.OPTX], optTypes[(int)Options.options.OPTY],
                                        expandingOpts[(int)Options.options.OPTX], expandingOpts[(int)Options.options.OPTY]));

        }






        // **************************************************************************************************************************** //
        // *************************************** Insert Options for search according to Letter ************************************** //

        private void InsertOptionsAccordingToLetter(RunEnvironment env)
        {
            if (letter == (int)Options.limitPointsLetters.A)
            {
                optTypes[(int)Options.options.OPTX] = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX];
                optTypes[(int)Options.options.OPTY] = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY];
                expandingOpts[(int)Options.options.OPTX] = initialPoint.opts[(int)Options.options.OPTX];
                expandingOpts[(int)Options.options.OPTY] = initialPoint.opts[(int)Options.options.OPTY];
            }
            else if (letter == (int)Options.limitPointsLetters.B)
            {
                optTypes[(int)Options.options.OPTX] = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY];
                optTypes[(int)Options.options.OPTY] = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX];
                expandingOpts[(int)Options.options.OPTX] = initialPoint.opts[(int)Options.options.OPTY];
                expandingOpts[(int)Options.options.OPTY] = initialPoint.opts[(int)Options.options.OPTX];
            }
            else
            {
                // Add Throw ....
            }
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
        // ***************************** Expand Initial Limit Point - A1 or B1 - Exact Match in Binary Search ************************* //

        private Option[] ExpandPointExactMatchA1B1(OneOptType optionXType, OneOptType optionYType, Option optionX, Option optionY)
        {

            expandingOpts[(int)Options.options.OPTX] = optionX;
            expandingOpts[(int)Options.options.OPTY] = optionY;

            if (printOrNo == true)
            {
                Console.WriteLine("Starting point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
            }

            if (optionY.optTime == optTypes[(int)Options.options.OPTY].product.minTime)
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Time is at minimum limit\n");
                }
                return expandingOpts;
            }
            else if (optionY.optTime > optTypes[(int)Options.options.OPTY].product.minTime)
            {
                optionY = new Option(optionY.optType, optionY.optAmt, (optionY.optTime - optTypes[(int)Options.options.OPTY].product.timeJump));
                if (printOrNo == true)
                {
                    Console.WriteLine("New Point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
                }
                int pmtChecker = ChecKPmt(optionX, optionY);
                if (pmtChecker == (int)Options.pmtRange.INRANGE)
                {
                    // Add - Save Point as composition for future calculation of total payment
                    savedMatches.InsertMatchToList(optionX, optionY);                                                                            
                    return ExpandPointExactMatchA1B1(optTypes[(int)Options.options.OPTX], optTypes[(int)Options.options.OPTY], optionX, optionY);
                }
                else
                {
                    // End.
                    return expandingOpts;
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Point : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") ---> EXEPTION");
                }
                // Add exeption - throw ...
                return null;
            }
        }




        // **************************************************************************************************************************** //
        // ***************************** Expand Initial Limit Point - A2 or B2 - Exact Match in Binary Search ************************* //

        private Option[] ExpandPointExactMatchA2B2(OneOptType optionXType, OneOptType optionYType, Option optionX, Option optionY)
        {
            expandingOpts[(int)Options.options.OPTX] = optionX;
            expandingOpts[(int)Options.options.OPTY] = optionY;

            if (printOrNo == true)
            {
                Console.WriteLine("Starting point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
            }
            if (optionX.optTime == optTypes[(int)Options.options.OPTX].product.maxTime)
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Time is at maximum limit\n");
                }
                return expandingOpts;
            }
            else if (optionX.optTime < optTypes[(int)Options.options.OPTX].product.maxTime)
            {
                optionX = new Option(optionX.optType, optionX.optAmt, (optionX.optTime + optTypes[(int)Options.options.OPTX].product.timeJump));
                if (printOrNo == true)
                {
                    Console.WriteLine("New Point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
                }
                int pmtChecker = ChecKPmt(optionX, optionY);
                if (pmtChecker == (int)Options.pmtRange.INRANGE)
                {
                    // Add - Save Point as composition for future calculation of total payment
                    savedMatches.InsertMatchToList(optionX, optionY);      
                    return ExpandPointExactMatchA2B2(optTypes[(int)Options.options.OPTX], optTypes[(int)Options.options.OPTY], optionX, optionY);
                }
                else
                {
                    // End.
                    return expandingOpts;
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Point : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") ---> EXEPTION");
                }                
                // Add exeption - throw ...
                return null;
            }
        }








        // **************************************************************************************************************************** //
        // ************************** Expand Initial Limit Point - A1 or B1 - No Exact Match in Binary Search ************************* //

        private Option[] ExpandPointNoMatchA1B1(OneOptType optionXType, OneOptType optionYType, Option optionX, Option optionY)
        {
            expandingOpts[(int)Options.options.OPTX] = optionX;
            expandingOpts[(int)Options.options.OPTY] = optionY;

            if (printOrNo == true)
            {
                Console.WriteLine("Starting point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
            }
            if (optionX.optTime == optTypes[(int)Options.options.OPTX].product.minTime)
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Time is at minimum limit\n");
                }
                return expandingOpts;
            }
            else if (optionX.optTime > optTypes[(int)Options.options.OPTX].product.minTime)
            {
                optionX = new Option(optionX.optType, optionX.optAmt, (optionX.optTime - optTypes[(int)Options.options.OPTX].product.timeJump));
                if (printOrNo == true)
                {
                    Console.WriteLine("New Point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
                }
                int pmtChecker = ChecKPmt(optionX, optionY);
                if (pmtChecker == (int)Options.pmtRange.INRANGE)
                {
                    // Add - Save Point as composition for future calculation of total payment
                    savedMatches.InsertMatchToList(optionX, optionY);      
                    expandingOpts[(int)Options.options.OPTX] = optionX;
                    expandingOpts[(int)Options.options.OPTY] = optionY;
                    return expandingOpts;
                }
                else if (pmtChecker == (int)Options.pmtRange.TOOSMALL)
                {
                    return ExpandPointNoMatchA1B1(optTypes[(int)Options.options.OPTX], optTypes[(int)Options.options.OPTY], optionX, optionY);
                }
                else
                {
                    // End.
                    return expandingOpts;
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Point : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") ---> EXEPTION");
                }                
                // Add exeption - throw ...
                return null;
            }
        }





        // **************************************************************************************************************************** //
        // ************************** Expand Initial Limit Point - A2 or B2 - No Exact Match in Binary Search ************************* //

        private Option[] ExpandPointNoMatchA2B2(OneOptType optionXType, OneOptType optionYType, Option optionX, Option optionY)                                                
        {
            expandingOpts[(int)Options.options.OPTX] = optionX;
            expandingOpts[(int)Options.options.OPTY] = optionY;

            //  Step 1 - Move 1 - X Axis
            if (optionX.optTime == optTypes[(int)Options.options.OPTX].product.minTime)
            {
                a2b2NoMatchBool = false;
                if (printOrNo == true)
                {
                    Console.WriteLine("Time is at minimum limit\n");
                }
                return expandingOpts;
            }
            else if (optionX.optTime > optTypes[(int)Options.options.OPTX].product.minTime)
            {
                if (a2b2NoMatchBool == true)
                {
                    optionX = new Option(optionX.optType, optionX.optAmt, (optionX.optTime - optTypes[(int)Options.options.OPTX].product.timeJump));
                    a2b2NoMatchBool = false;
                }

                //  Step 2 - Move 2 - Y Axis
                if (optionY.optTime == optTypes[(int)Options.options.OPTY].product.maxTime)
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine("Time is at maximum limit\n");
                    }
                    return expandingOpts;
                }
                else if (optionY.optTime < optTypes[(int)Options.options.OPTY].product.maxTime)
                {
                    optionX = expandingOpts[(int)Options.options.OPTX];
                    optionY = new Option(optionY.optType, optionY.optAmt, (optionY.optTime + optTypes[(int)Options.options.OPTY].product.timeJump));
                    if (printOrNo == true)
                    {
                        Console.WriteLine("New Point for check : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ")");
                    }
                    int pmtChecker = ChecKPmt(optionX, optionY);
                    if (pmtChecker == (int)Options.pmtRange.INRANGE)
                    {
                        // Add - Save Point as composition for future calculation of total payment
                        savedMatches.InsertMatchToList(optionX, optionY);      
                        expandingOpts[(int)Options.options.OPTX] = optionX;
                        expandingOpts[(int)Options.options.OPTY] = optionY;
                        return expandingOpts;
                    }
                    else if (pmtChecker == (int)Options.pmtRange.TOOLARGE)
                    {
                        return ExpandPointNoMatchA2B2(optTypes[(int)Options.options.OPTX], optTypes[(int)Options.options.OPTY], optionX, optionY);                                                            
                    }
                    else
                    {
                        // End.
                        return expandingOpts;
                    }
                }
                else
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine("Point : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") ---> EXEPTION");
                    }
                    // Add exeption - throw ...
                    a2b2NoMatchBool = false;
                    return null;
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("Point : (X,Y) = (" + optionX.optTime + "," + optionY.optTime + ") ---> SHOULDNT HAPPEN");
                }
                // Shouldnt happen ...
                a2b2NoMatchBool = false;
                return null;
            }
        }





        // **************************************************************************************************************************** //
        // *************************************** Expand Initial Limit Point - General Function ************************************** //

        private Option[] ExpandPoint(OneOptType optionXType, OneOptType optionYType, Option optionX, Option optionY)

        {
            if (initialPoint.matchOrNo == true)
            {
                if (number == (int)Options.limitPointsNumbers.ONE)
                {
                    // Expand Point - Exact Match - A1/B1
                    return ExpandPointExactMatchA1B1(optionXType, optionYType, optionX, optionY);
                }
                else if (number == (int)Options.limitPointsNumbers.TWO)
                {
                    // Expand Point - Exact Match - A2/B2
                    return ExpandPointExactMatchA2B2(optionXType, optionYType, optionX, optionY);
                }
                else
                {
                    // Add throw - exeption ....
                    return null;
                }

            }
            else
            {
                if (number == (int)Options.limitPointsNumbers.ONE)
                {
                    // Expand Point - No Match - A1/B1
                    return ExpandPointNoMatchA1B1(optionXType, optionYType, optionX, optionY);
                }
                else if (number == (int)Options.limitPointsNumbers.TWO)
                {
                    // Expand Point - No Match - A2/B2                   
                    return ExpandPointNoMatchA2B2(optionXType, optionYType, optionX, optionY);
                }
                else
                {
                    // Add throw - exeption ....
                    return null;
                }
            }
        }





        // **************************************************************************************************************************** //
        // ************************************************** Save Final Limit Point (X,Y) ******************************************** //

        private void InsertOptionsForSavedPoint(Option[] optionsForSave)
        {
            if (letter == (int)Options.limitPointsLetters.A)
            {
                opts[(int)Options.options.OPTX] = optionsForSave[(int)Options.options.OPTX];
                opts[(int)Options.options.OPTY] = optionsForSave[(int)Options.options.OPTY];
            }
            else if (letter == (int)Options.limitPointsLetters.B)
            {
                opts[(int)Options.options.OPTX] = optionsForSave[(int)Options.options.OPTY];
                opts[(int)Options.options.OPTY] = optionsForSave[(int)Options.options.OPTX];
            }
            else
            {
                // Add Throw Exeption ...
            }
        }




        // **************************************************************************************************************************** //
        // ****************************************** Print Final Limit Point To String ********************************************* //

        public override string ToString()
        {
            if (opts[(int)Options.options.OPTX] != null)
            {
                return "Final Limit Point " + Options.letters[(int)letter] + Options.numbers[(int)number] + "(X,Y) = ("
                        + opts[(int)Options.options.OPTX].optTime + "," + opts[(int)Options.options.OPTY].optTime + ")\nOption X = "
                        + opts[(int)Options.options.OPTX].ToString() + "\nOption Y = " + opts[(int)Options.options.OPTY].ToString()
                        + "\nPMT(X = " + opts[(int)Options.options.OPTX].optTime + ") = " + opts[(int)Options.options.OPTX].optPmt
                        + "\nPmt(Y = " + opts[(int)Options.options.OPTY].optTime + ") = " + opts[(int)Options.options.OPTY].optPmt
                        + "\nTotal PMT(" + opts[(int)Options.options.OPTX].optTime + "," + opts[(int)Options.options.OPTY].optTime
                        + ") = " + (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt)
                        + "\nBinary Search Result = " + initialPoint.matchOrNo + "\n";
                        
            }
            else
            {
                return "Final Limit Point is empty.\n";
            }
        }


    }
}
