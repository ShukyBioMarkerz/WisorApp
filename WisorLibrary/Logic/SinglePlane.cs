using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.Utilities;

namespace WisorLib
{
    class SinglePlane
    {
        // General Parameters
        public Option fixedOptZ = null;
        private PmtLimits pmtLimits = null;
        private bool printOrNo;
        private double targetTwoOptionPmt = -1;
        private InitialLimitPoint[] initialLimitPoints = { null, null };
        private FinalLimitPoint[] finalLimitPoints = { null, null };
        private SearchAreaInPlane searchArea = null;


        // Lists
        public SavedMatchesBeforeCheck savedMatches = new SavedMatchesBeforeCheck();


        // Counters
        public int totalLimitPointChecks = 0;
        public int totalLimitPointMatches = 0;
        public int totalColumnSearchChecks = 0;
        public int totalColumnSearchMatches = 0;
        public int totalLimitPointExists = 0;
        public int totalLimitPointNotExists = 0;


        public SinglePlane(PmtLimits planePmtLimits, Option optZ0, RunEnvironment env)
        {
            pmtLimits = planePmtLimits;
            fixedOptZ = optZ0;
            printOrNo = env.PrintOptions.printFunctionsInConsole;

            // 1 - Calculate target Pmt for two Option X and Option Y for limit point search
            targetTwoOptionPmt = env.CalculationParameters.monthlyPmtWanted - fixedOptZ.optPmt;

            // 2 - Get Initial Limit Points
            FindInitialLimitPointsForSearchArea(env);
            //Console.ReadKey();

            if (initialLimitPoints[(int)Options.options.OPTX].opts[(int)Options.options.OPTX] != null)
            {

                if (printOrNo == true)
                {
                    Console.WriteLine("\nInitial limit A = " + initialLimitPoints[(int)Options.options.OPTX].ToString()
                        + "\nInitial limit B = " + initialLimitPoints[(int)Options.options.OPTY].ToString());
                }
                totalLimitPointExists++;

                // 3 - Get Final Limit Points
                FindFinalExtendedLimitPointsForSearchArea(env);
                //Console.ReadKey();

                if (! MiscUtilities.CheckConsistency(finalLimitPoints))
                {
                    Console.WriteLine("ERROR: SinglePlane failed in CheckConsistency.");
                    return;
                }

                // 4 - Create final search Area
                FindFinalSearchArea(env);
                //Console.ReadKey();

                // 5 - Perform column search in final search area 
                SearchAllColumnsInFinalSearchArea(env);
                //Console.ReadKey();
            }
            else
            {
                totalLimitPointNotExists++;
                if (printOrNo == true)
                {
                    Console.WriteLine("\nInitial limit points not found\n");
                }
            }
        }

    



        // **************************************************************************************************************************** //
        // ****************************************** Find Initial Limit Points A and B *********************************************** //

        private void FindInitialLimitPointsForSearchArea(RunEnvironment env)
        {
            // 1 - Getting Initial Points
            initialLimitPoints[(int)Options.limitPointsLetters.A] = new InitialLimitPoint((int)Options.limitPointsLetters.A,
                                                                                        pmtLimits, targetTwoOptionPmt, env);
            initialLimitPoints[(int)Options.limitPointsLetters.B] = new InitialLimitPoint((int)Options.limitPointsLetters.B,
                                                                                        pmtLimits, targetTwoOptionPmt, env);

            if (printOrNo == true)
            {
                Console.WriteLine("\n\n" + initialLimitPoints[(int)Options.limitPointsLetters.A] +
                                    "\n" + initialLimitPoints[(int)Options.limitPointsLetters.B] + "\n");
            }
        }



        // **************************************************************************************************************************** //
        // *************************************** Find Final Extended Limit Points A and B ******************************************* //

        private void FindFinalExtendedLimitPointsForSearchArea(RunEnvironment env)
        {
            finalLimitPoints[(int)Options.limitPointsLetters.A] = new FinalLimitPoint(initialLimitPoints[(int)Options.limitPointsLetters.A], env);
            finalLimitPoints[(int)Options.limitPointsLetters.B] = new FinalLimitPoint(initialLimitPoints[(int)Options.limitPointsLetters.B], env);

            if (null == finalLimitPoints ||
                null == finalLimitPoints[(int)Options.limitPointsLetters.A] ||
                null == finalLimitPoints[(int)Options.limitPointsLetters.B])
            {
                Console.WriteLine("ERROR: FindFinalExtendedLimitPointsForSearchArea got null finalLimitPoints");
                return;
            }

            if ((finalLimitPoints[(int)Options.limitPointsLetters.A].savedMatches.numOfMatches == 1) &&
                 (finalLimitPoints[(int)Options.limitPointsLetters.B].savedMatches.numOfMatches == 1))
            {
                savedMatches.InsertListOfMatches(finalLimitPoints[(int)Options.limitPointsLetters.A].savedMatches);
            }
            else
            {
                savedMatches.InsertListOfMatches(finalLimitPoints[(int)Options.limitPointsLetters.A].savedMatches);
                savedMatches.InsertListOfMatches(finalLimitPoints[(int)Options.limitPointsLetters.B].savedMatches);
            }

            if (printOrNo == true)
            {
                Console.WriteLine("\n\n" + finalLimitPoints[(int)Options.limitPointsLetters.A] +
                                    "\n" + finalLimitPoints[(int)Options.limitPointsLetters.B] + "\n");
            }
        }


    // **************************************************************************************************************************** //
    // ************************************************ Create Final Search Area ************************************************** //

    private void FindFinalSearchArea(RunEnvironment env)
        {
            searchArea = new SearchAreaInPlane(finalLimitPoints, env);

            if (printOrNo == true)
            {
                Console.WriteLine(searchArea.ToString());
            }
        }






        // **************************************************************************************************************************** //
        // **************************************** Search All Colomns In Final Search Area ******************************************* //

        private void SearchAllColumnsInFinalSearchArea(RunEnvironment env)
        {
            // 1 - Perform search in final search area
            if (searchArea.columns != null)     // There are columns to check - search area is at least 1X1
            {
                for (int i = 0; i < searchArea.columns.Count(); i++)
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine("Searching column " + i);
                    }
                    searchArea.SearchOneColumnAndUpdateCounters(i, env);
                }
                savedMatches.InsertListOfMatches(searchArea.savedMatches);
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nSearch area is empty\n");
                }
            }
        }



    }
}
