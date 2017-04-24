using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    public class InitialLimitPoint
    {
        // General Parameters
        public uint letter = 0;
        public uint number = 0;
        private bool printOrNo = false;
        private PmtLimits pmtLimits = null;
        public Option[] opts = {null, null};
        public double targetTwoOptionPmt = -1;


        // Parameters for Binary Search
        private OneOption fixedOpt = null;
        private OneOption optForCheck = null;
        private double optForCheckAmt = -1;
        private OneOptType optForCheckType = null;
        private double targetOneOptPmt = -1;

        private int matchMarker = -1;
        public bool matchOrNo = false;
        private uint savedTime = 0;



        public InitialLimitPoint(uint pointLetter, PmtLimits planePmtLimits, double targetPmtTwoOptions, RunEnvironment env)
        {
            letter = pointLetter;
            pmtLimits = planePmtLimits;
            targetTwoOptionPmt = targetPmtTwoOptions;
            printOrNo = env.PrintOptions.printFunctionsInConsole;


            GetInitialPoint((int)Options.limitPointsNumbers.ONE, env);
            if (matchMarker == (int)Options.binarySearchResults.TOOSMALL)
            {
                GetInitialPoint((int)Options.limitPointsNumbers.TWO, env);
                if (matchMarker == (int)Options.binarySearchResults.TOOSMALL)
                {
                    opts[(int)Options.options.OPTX] = null;
                    opts[(int)Options.options.OPTY] = null;
                }
                else
                {
                    SaveInitialLimitPoint((int)Options.limitPointsNumbers.TWO, env);
                }
            }
            else
            {
                SaveInitialLimitPoint((int)Options.limitPointsNumbers.ONE, env);
            }



        }




        // **************************************************************************************************************************** //
        // ************************************************ Get Initial Limit Point *************************************************** //

        private void GetInitialPoint(uint numberForCheck, RunEnvironment env)
        {
            if (printOrNo == true)
            {
                Console.WriteLine("\nSearching for limit point " + Options.letters[(int)letter] + Options.numbers[(int)numberForCheck]
                                    + " for target Pmt = " + targetTwoOptionPmt + "\n");
            }

            InsertOptionsAccordingToLetterAndNumber(numberForCheck, env);
            FindTargetPmtForCheckedOption(numberForCheck);
            savedTime = PerformBinarySearch(optForCheck.times[(int)Options.pmtLimits.MINTIME].optTime,
                                                optForCheck.times[(int)Options.pmtLimits.MAXTIME].optTime, env);
        }







        // **************************************************************************************************************************** //
        // *************************************** Insert Options for search according to Letter ************************************** //

        private void InsertOptionsAccordingToLetterAndNumber(uint numberForCheck, RunEnvironment env)
        {
            if (((letter == (int)Options.limitPointsLetters.A) && (numberForCheck == (int)Options.limitPointsNumbers.ONE)) ||
                    ((letter == (int)Options.limitPointsLetters.B) && (numberForCheck == (int)Options.limitPointsNumbers.TWO)))
            {
                fixedOpt = pmtLimits.opts[(int)Options.options.OPTX];
                optForCheckType = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY];
                optForCheck = pmtLimits.opts[(int)Options.options.OPTY];
                optForCheckAmt = pmtLimits.amts[(int)Options.options.OPTY];
            }
            else if (((letter == (int)Options.limitPointsLetters.B) && (numberForCheck == (int)Options.limitPointsNumbers.ONE)) ||
                    ((letter == (int)Options.limitPointsLetters.A) && (numberForCheck == (int)Options.limitPointsNumbers.TWO)))
            {
                fixedOpt = pmtLimits.opts[(int)Options.options.OPTY];
                optForCheckType = env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX];
                optForCheck = pmtLimits.opts[(int)Options.options.OPTX];
                optForCheckAmt = pmtLimits.amts[(int)Options.options.OPTX];
            }
            else
            {
                // Add Throw ....
            }
        }



        // **************************************************************************************************************************** //
        // ************************************ Find Target PMT For Checked Option for Binary Search ********************************** //

        private void FindTargetPmtForCheckedOption(uint numberForCheck)
        {
            if (numberForCheck == (int)Options.limitPointsNumbers.ONE)
            {
                targetOneOptPmt = targetTwoOptionPmt - fixedOpt.times[(int)Options.pmtLimits.MAXTIME].optPmt;
            }
            else if (numberForCheck == (int)Options.limitPointsNumbers.TWO)
            {
                targetOneOptPmt = targetTwoOptionPmt - fixedOpt.times[(int)Options.pmtLimits.MINTIME].optPmt;
            }
            else
            {
                // Add throw ....
            }
        }





        // **************************************************************************************************************************** //
        // ***************************************************** Perform Binary Search ************************************************ //

        private uint PerformBinarySearch(uint minTimeValue, uint maxTimeValue, RunEnvironment env)
        {
            uint timeForSave = 0;
            uint midValue = 0, midCell = 0, numOfCells = 0;
            if (printOrNo == true)
            {
                Console.WriteLine("\nSearching for time that gives PMT = " + targetOneOptPmt);
            }
            if (minTimeValue <= maxTimeValue)
            {
                matchMarker = (int)Options.binarySearchResults.INRANGE;
                // TBD
                if (0 == optForCheckType.product.timeJump)
                {
                    numOfCells = 1;
                }
                else
                {
                    numOfCells = ((maxTimeValue - minTimeValue) / optForCheckType.product.timeJump) + 1;
                }
                if (printOrNo == true)
                {
                    Console.WriteLine("\nMinValue = " + minTimeValue + " | MaxValue = " + maxTimeValue
                                        + "\nThere are " + numOfCells + " cells in the list");
                }
                if ((numOfCells % 2) == 0)
                {
                    midCell = (numOfCells / 2) - 1;
                }
                else
                {
                    midCell = (numOfCells - 1) / 2;
                }
                midValue = minTimeValue + (midCell * optForCheckType.product.timeJump);
                Option opt = new Option(optForCheckType.product.productID.numberID, optForCheckAmt, midValue, env);
                double midPmt = opt.optPmt;
                if (printOrNo == true)
                {
                    Console.WriteLine("Option Type: " + opt.optType + " , Amount= " + opt.optAmt + " , Time = "
                                        + opt.optTime + " , Rate = " + (opt.ShowRate(env.BorrowerProfile.profile) * 100) + " - PMT = " + opt.optPmt
                                        + "\nMidCell = " + midCell + " | MidValue = " + midValue);
                }
                if (midPmt < (targetOneOptPmt - CalculationConstants.largeDev))  // No exact match, time saved is middle cell
                {
                    matchOrNo = false;
                    matchMarker = (int)Options.binarySearchResults.TOOSMALL;
                    timeForSave = midValue;
                    if (printOrNo == true)
                    {
                        Console.WriteLine("\nPMT(" + midValue + ") = " + midPmt + " -> Too Small - Cutting array\n"
                                            + "\nnew MinValue = " + minTimeValue + " | new MaxValue = " + (midValue - optForCheckType.product.timeJump));
                    }
                    return PerformBinarySearch(minTimeValue, (midValue - optForCheckType.product.timeJump), env);                    
                }
                else if (midPmt > (targetOneOptPmt + CalculationConstants.smallDev)) // No exact match, time saved is middle cell
                {
                    matchOrNo = false;
                    timeForSave = midValue;
                    if (printOrNo == true)
                    {
                        Console.WriteLine("\nPMT(" + midValue + ") = " + midPmt + " -> Too Large - Cutting array\n"
                                            + "\nnew MinValue = " + (midValue + optForCheckType.product.timeJump) + " | new MaxValue = " + maxTimeValue);
                    }
                    return PerformBinarySearch((midValue + optForCheckType.product.timeJump), maxTimeValue, env);
                }
                else
                {
                    matchOrNo = true;
                    timeForSave = midValue;
                    matchMarker = (int)Options.binarySearchResults.MATCH;
                    if (printOrNo == true)
                    {
                        Console.WriteLine("\nPMT(" + midValue + ") = " + midPmt + " -> Is in range - Match !\n"
                                            + "\nBinary search ended - Result Marker = " + matchOrNo + " , Time = " + timeForSave + "\n");
                    }
                    return timeForSave;
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nBinary search ended. No match found\nMinTime = " + minTimeValue + " MaxTime = " + maxTimeValue + "\n");
                }
                if (minTimeValue > optForCheckType.product.maxTime)
                {
                    //matchMarker = (int)Options.binarySearchResults.OUTOFRANGE;
                    matchMarker = (int)Options.binarySearchResults.TOOSMALL;
                    matchOrNo = false;
                    timeForSave = maxTimeValue;
                }
                else if (maxTimeValue < optForCheckType.product.minTime)
                {
                    //matchMarker = (int)Options.binarySearchResults.TOOSMALL;
                    matchMarker = (int)Options.binarySearchResults.TOOLARGE;
                    matchOrNo = false;
                    timeForSave = minTimeValue;
                }
                else
                {
                    matchMarker = (int)Options.binarySearchResults.INRANGE;
                    matchOrNo = false;
                    timeForSave = minTimeValue;
                }

                if (printOrNo == true)
                {
                    Console.WriteLine("Time saved = " + timeForSave);
                }
                return timeForSave;
            }
        }




        // **************************************************************************************************************************** //
        // ************************************************ Save Initial Limit Point (X,Y) ******************************************** //

        private void SaveInitialLimitPoint(uint numberForSave, RunEnvironment env)
        {
            if ((letter == (int)Options.limitPointsLetters.A) && (numberForSave == (int)Options.limitPointsNumbers.ONE))
            {
                opts[(int)Options.options.OPTX] = fixedOpt.times[(int)Options.pmtLimits.MAXTIME];
                opts[(int)Options.options.OPTY] = new Option(optForCheckType.product.productID.numberID, optForCheckAmt, savedTime, env);
            }
            else if ((letter == (int)Options.limitPointsLetters.A) && (numberForSave == (int)Options.limitPointsNumbers.TWO))
            {
                opts[(int)Options.options.OPTX] = new Option(optForCheckType.product.productID.numberID, optForCheckAmt, savedTime, env);
                opts[(int)Options.options.OPTY] = fixedOpt.times[(int)Options.pmtLimits.MINTIME];

            }
            else if ((letter == (int)Options.limitPointsLetters.B) && (numberForSave == (int)Options.limitPointsNumbers.ONE))
            {
                opts[(int)Options.options.OPTX] = new Option(optForCheckType.product.productID.numberID, optForCheckAmt, savedTime, env);
                opts[(int)Options.options.OPTY] = fixedOpt.times[(int)Options.pmtLimits.MAXTIME];

            }
            else if ((letter == (int)Options.limitPointsLetters.B) && (numberForSave == (int)Options.limitPointsNumbers.TWO))
            {
                opts[(int)Options.options.OPTX] = fixedOpt.times[(int)Options.pmtLimits.MINTIME];
                opts[(int)Options.options.OPTY] = new Option(optForCheckType.product.productID.numberID, optForCheckAmt, savedTime, env);
            }
            number = numberForSave;
        }



        // **************************************************************************************************************************** //
        // ****************************************** Print Initial Limit Point To String ********************************************* //

        public override string ToString()
        {
            if (opts[(int)Options.options.OPTX] != null)
            {
                return "Initial Limit Point " + Options.letters[(int)letter] + Options.numbers[(int)number] + "(X,Y) = ("
                        + opts[(int)Options.options.OPTX].optTime + "," + opts[(int)Options.options.OPTY].optTime + ")\nOption X = "
                        + opts[(int)Options.options.OPTX].ToString() + "\nOption Y = " + opts[(int)Options.options.OPTY].ToString()
                        + "\nPMT(X = " + opts[(int)Options.options.OPTX].optTime + ") = " + opts[(int)Options.options.OPTX].optPmt
                        + "\nPmt(Y = " + opts[(int)Options.options.OPTY].optTime + ") = " + opts[(int)Options.options.OPTY].optPmt
                        + "\nTotal PMT(" + opts[(int)Options.options.OPTX].optTime + "," + opts[(int)Options.options.OPTY].optTime
                        + ") = " + (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt)
                        + "\nBinary Search Result = " + Options.ranges[matchMarker] + "\n";                        
            }
            else
            {
                return "Initial Limit Point is empty.\n";
            }
        }



    }
}
