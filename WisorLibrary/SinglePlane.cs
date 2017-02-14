using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class SinglePlane
    {
        // General Parameters
        public Option fixedOptZ = null;
        private PmtLimits pmtLimits = null;
        private bool printOrNo = PrintOptions.printFunctionsInConsole;
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


        public SinglePlane(PmtLimits planePmtLimits, Option optZ0)
        {
            pmtLimits = planePmtLimits;
            fixedOptZ = optZ0;

            // 1 - Calculate target Pmt for two Option X and Option Y for limit point search
            targetTwoOptionPmt = CalculationParameters.monthlyPmtWanted - fixedOptZ.optPmt;

            // 2 - Get Initial Limit Points
            FindInitialLimitPointsForSearchArea();
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
                FindFinalExtendedLimitPointsForSearchArea();
                //Console.ReadKey();

                // 4 - Create final search Area
                FindFinalSearchArea();
                //Console.ReadKey();

                // 5 - Perform column search in final search area 
                SearchAllColumnsInFinalSearchArea();
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

        private void FindInitialLimitPointsForSearchArea()
        {
            // 1 - Getting Initial Points
            initialLimitPoints[(int)Options.limitPointsLetters.A] = new InitialLimitPoint((int)Options.limitPointsLetters.A,
                                                                                        pmtLimits, targetTwoOptionPmt);
            initialLimitPoints[(int)Options.limitPointsLetters.B] = new InitialLimitPoint((int)Options.limitPointsLetters.B,
                                                                                        pmtLimits, targetTwoOptionPmt);

            if (printOrNo == true)
            {
                Console.WriteLine("\n\n" + initialLimitPoints[(int)Options.limitPointsLetters.A] +
                                    "\n" + initialLimitPoints[(int)Options.limitPointsLetters.B] + "\n");
            }
        }



        // **************************************************************************************************************************** //
        // *************************************** Find Final Extended Limit Points A and B ******************************************* //

        private void FindFinalExtendedLimitPointsForSearchArea()
        {
            finalLimitPoints[(int)Options.limitPointsLetters.A] = new FinalLimitPoint(initialLimitPoints[(int)Options.limitPointsLetters.A]);
            finalLimitPoints[(int)Options.limitPointsLetters.B] = new FinalLimitPoint(initialLimitPoints[(int)Options.limitPointsLetters.B]);

            savedMatches.InsertListOfMatches(finalLimitPoints[(int)Options.limitPointsLetters.A].savedMatches);
            savedMatches.InsertListOfMatches(finalLimitPoints[(int)Options.limitPointsLetters.B].savedMatches);


            if (printOrNo == true)
            {
                Console.WriteLine("\n\n" + finalLimitPoints[(int)Options.limitPointsLetters.A] +
                                    "\n" + finalLimitPoints[(int)Options.limitPointsLetters.B] + "\n");
            }
        }





        // **************************************************************************************************************************** //
        // ************************************************ Create Final Search Area ************************************************** //

        private void FindFinalSearchArea()
        {
            searchArea = new SearchAreaInPlane(finalLimitPoints);

            if (printOrNo == true)
            {
                Console.WriteLine(searchArea.ToString());
            }
        }






        // **************************************************************************************************************************** //
        // **************************************** Search All Colomns In Final Search Area ******************************************* //

        private void SearchAllColumnsInFinalSearchArea()
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
                    searchArea.SearchOneColumnAndUpdateCounters(i);
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
