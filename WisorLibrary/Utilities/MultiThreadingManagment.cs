using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Logic;

namespace WisorLibrary.Utilities
{
    public class MultiThreadingManagment
    {

        public static async void RunTheLoansWraperASync(LoanList loans/*, FieldList fields*/)
        {
            await Task.Run(() =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                LoanCalculationASyncWrapper(loans);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                //RunTheLoansASync(loans/*, fields*/);
            });
        }

        private static async Task LoanCalculationASyncWrapper(LoanList loans)
        {
            List<Task> tasks = new List<Task>();

            // start the time elapse counter
            StartPerformanceCalculation();
            Share.TotalNumberOfLoans = loans.Count;

            foreach (loanDetails loan in loans)
            {
                try
                {
                    tasks.Add(LoanCalculationASync(loan));
                }
                catch (Exception ex)
                {
                    WindowsUtilities.loggerMethod("NOTICE: LoanCalculationASyncWrapper Exception occured: " + ex.ToString());
                }
            }

            WindowsUtilities.loggerMethod("--- BEFORE ASYNC 333 Task.WhenAll");
            await Task.WhenAll(tasks.ToArray());
            WindowsUtilities.loggerMethod("--- AFTER ASYNC 333 Task.WhenAll");
        }

        private static async Task LoanCalculationASync(loanDetails loan)
        {
            RunLoanDetails result = await Task.Run(() => LoanCalculation(loan));
        }

        //////////////////////////////////////////////////////////////////


        public static void RunTheLoansWraperSync(LoanList loans/*, FieldList fields*/)
        {
            Task.Run(() =>
            {
                //RunTheLoansSync(loans/*, fields*/);
                LoanCalculationSimpleSyncWrapper(loans);
            });

        }

        private static void LoanCalculationSimpleSyncWrapper(LoanList loans)
        {
            //List<Task> tasks = new List<Task>();

            // start the time elapse counter
            MultiThreadingManagment.StartPerformanceCalculation();
            Share.TotalNumberOfLoans = loans.Count;

            foreach (loanDetails loan in loans)
            {
                try
                {
                    RunLoanDetails rld = MultiThreadingManagment.LoanCalculation(loan, false /* shouldStopPerformance */);
                    //tasks.Add(LoanCalculationASync(loan));
                }
                catch (Exception ex)
                {
                    WindowsUtilities.loggerMethod("NOTICE: LoanCalculationSimpleASyncWrapper Exception occured: " + ex.ToString());
                }
            }

            //WindowsUtilities.loggerMethod("--- BEFORE ASYNC 333 Task.WhenAll");
            //await Task.WhenAll(tasks.ToArray());
            MultiThreadingManagment.StopPerformanceCalculation();
            WindowsUtilities.loggerMethod("--- AFTER LoanCalculationSimpleASyncWrapper");
        }



        private async void RunTheLoansASync(LoanList loans)
        {
            WindowsUtilities.loggerMethod("Running the engine for the: " + loans.Count + " loans.");
            List<Task> tasks = new List<Task>();

            // start the time elapse counter
            MultiThreadingManagment.StartPerformanceCalculation();

            foreach (loanDetails loan in loans)
            {
                try
                {
                    tasks.Add(Task.Factory.StartNew(async () =>
                    {
                        RunLoanDetails result = await Task.Run(() => MultiThreadingManagment.LoanCalculation(loan));
                        //WindowsUtilities.loggerMethod("--- Complete ASYNC running the engine with: " + loan.ToString() + ", result: " + result.ToString());
                    }));
                }
                catch (Exception ex)
                {
                    WindowsUtilities.loggerMethod("NOTICE: Exception occured: " + ex.ToString());
                }

                // fake completion
                //if (3 <= count)
                //    break;
            }

            WindowsUtilities.loggerMethod("--- BEFORE ASYNC 222 Task.WaitAll");
            await Task.WhenAll(tasks.ToArray());
            //Task.WaitAll(tasks.ToArray());
            WindowsUtilities.loggerMethod("--- AFTER ASYNC 2222 Task.WaitAll");
        }

        public static /*async*/ RunLoanDetails runSingleLoanASyncMethod(loanDetails loan)
        {
            WindowsUtilities.loggerMethod("Running the engine for the loan: " + loan.ToString());
            RunLoanDetails result = null;

            // calculate the luch silumkn for comparison
            LoanContainer loanContainer = new LoanContainer();
            loanContainer.Add(loan);
            LoanList ll = loanContainer.GroupLoansByID();
            if (0 >= ll.Count)
            {
                WindowsUtilities.loggerMethod("ERROR: RunTheLoanASync failed in loanContainer.GroupLoansByID");
                return result;
            }

            // start the time elapse counter
            MultiThreadingManagment.StartPerformanceCalculation();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    result = await Task.Run(() => MultiThreadingManagment.LoanCalculation(/*loan*/ ll[0]));
                    //WindowsUtilities.loggerMethod("--- Complete ASYNC running the engine with: " + loan.ToString() + ", result: " + result.ToString());
                }
                catch (Exception ex)
                {
                    WindowsUtilities.loggerMethod("NOTICE: Exception occured: " + ex.ToString());
                }

            });

            return result;
        }

        public static RunLoanDetails runSingleLoanSyncMethod(loanDetails loan)
        {
            WindowsUtilities.loggerMethod("Running the engine for the loan: " + loan.ToString());
            RunLoanDetails result = null;

            // calculate the luch silumkn for comparison
            LoanContainer loanContainer = new LoanContainer();
            loanContainer.Add(loan);
            LoanList ll = loanContainer.GroupLoansByID();
            if (0 >= ll.Count)
            {
                WindowsUtilities.loggerMethod("ERROR: runSingleLoanSyncMethod failed in loanContainer.GroupLoansByID");
                return result;
            }

            // start the time elapse counter
            MultiThreadingManagment.StartPerformanceCalculation();
            Share.TotalNumberOfLoans = ll.Count;

            Task t = null;
            try
            {
                t = Task.Factory.StartNew(() => result = MultiThreadingManagment.LoanCalculation(ll[0]));
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("NOTICE: runSingleLoanSyncMethod Exception occured: " + ex.ToString());
            }
            
            t.Wait();

            return result;
        }

        public static void RunTheLoansSync(LoanList loans/*, FieldList fields*/)
        {
            WindowsUtilities.loggerMethod("Running the engine for the: " + loans.Count + " loans.");
            List<Task> tasks = new List<Task>();

            // start the time elapse counter
            MultiThreadingManagment.StartPerformanceCalculation();
            Share.TotalNumberOfLoans = loans.Count;

            int count = 1;
            foreach (loanDetails loan in loans)
            {

                tasks.Add(Task.Factory.StartNew(/*async*/ () =>
                {
                    try
                    {
                        RunLoanDetails result = MultiThreadingManagment.LoanCalculation(loan);
                        //WindowsUtilities.loggerMethod("--- Complete SYNC running the engine with: " + loan.ToString() + ", result: " + result.ToString());
                    }
                    catch (ArgumentOutOfRangeException aoore)
                    {
                        WindowsUtilities.loggerMethod("NOTICE: RunTheLoansSync ArgumentOutOfRangeException occured: " + aoore.ToString());
                    }
                    catch (Exception ex)
                    {
                        WindowsUtilities.loggerMethod("NOTICE: RunTheLoansSync Exception occured: " + ex.ToString());
                    }

                }
                ));

                count++;

                // fake completion
                //if (3 <= count)
                //    break;
            }

            Console.WriteLine("--- BEFORE SYNC Task.WaitAll");
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("--- AFTER SYNC Task.WaitAll");
            MultiThreadingManagment.StopPerformanceCalculation();
            //SetButtonEnable(true);

            // WindowsUtilities.loggerMethod("Complete calculate the entire " + loans.Count + " loans");
        }

        ////////////////////////////////////////////////

        public static RunLoanDetails LoanCalculation(loanDetails loan, bool shouldManagePerformance = true)
        {
            RunLoanDetails result = null;

            // is the loan ready to be calculated?
            if (loan.Status)
            {
                if (1 == Interlocked.Add(ref GlobalConcurrentLoanCounter, 1))
                {
                    // start the time elapse counter
                    if (shouldManagePerformance)
                        StartPerformanceCalculation();
                }

                WindowsUtilities.loggerMethod("+++ LoanCalculation Running a new task with: " + loan.ToString() + ", Task.CurrentId: " + Task.CurrentId + ", GlobalCurrentLoanCounter: " + GlobalConcurrentLoanCounter);
                RunEnvironment env = new RunEnvironment(loan);
                //env.risk = 1;
                //env.liquidity = 2;

                FastSearch fs = new FastSearch(env);

                try
                {
                    result = fs.runSearch();
                }
                catch (ArgumentOutOfRangeException aoore)
                {
                    Console.WriteLine("ERROR: LoanCalculation ArgumentOutOfRangeException occured: "  + aoore.ToString()  +
                        " for loan id: " + loan.ID);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: LoanCalculation Exception occured: "  + ex.ToString()  +
                        " for loan id: " + loan.ID);
                }

                // for counting the amount of completed loans
                Interlocked.Add(ref GlobalCompletedLoansCounter, 1);
                Interlocked.Decrement(ref GlobalConcurrentLoanCounter);
                long c = Interlocked.Read(ref GlobalConcurrentLoanCounter);
                if (shouldManagePerformance && 0 >= c)
                    StopPerformanceCalculation();

                string infoStrs = " .Can re-finince: " + Share.NumberOfCanRefininceLoans + " which means: " +
                    ((double)Share.NumberOfCanRefininceLoans / GlobalCompletedLoansCounter * 100).ToString() +
                    " and NumberOfPositiveBeneficialLoans: " + Share.NumberOfPositiveBeneficialLoans.ToString() +
                    " (" + ((double)Share.NumberOfPositiveBeneficialLoans / GlobalCompletedLoansCounter * 100).ToString() + "%)";

                WindowsUtilities.loggerMethod("--- Complete running the engine with loan ID: " + loan.ID +
                    //+ loan.ToString() + ", result: " + result.ToString() + ", Task.CurrentId: " + Task.CurrentId + 
                    ", Completed Counter: " + GlobalCompletedLoansCounter +
                    " out of total: " + Share.TotalNumberOfLoans + infoStrs,
                    true /*write to console*/, true /*color*/);

                // TBD - do we need it? if so then define pointer to the function
                //Utilities.PrintResultsInList(env);
            }
            else
            {
                WindowsUtilities.loggerMethod("--- LoanCalculation illegal loan details: " + loan.ToString());
            }

            return result;
        }

        //////////////////////////////////////////////////////////


        /////////////////////////////////////////////////
        /// <summary>
        /// Manage the diagnistic utility
        /// </summary>
        /// 


        static long GlobalConcurrentLoanCounter = 0;
        static long GlobalCompletedLoansCounter = 0;

        static System.Diagnostics.Stopwatch watch = null;

        public static void StartPerformanceCalculation()
        {
            // Shuky - measure the elapse time. Start
            if (null == watch)
                watch = System.Diagnostics.Stopwatch.StartNew();
        }

        public static void StopPerformanceCalculation()
        {
            // Shuky - measure the elapse time. Stop
            if (null != watch)
                watch.Stop();
            WindowsUtilities.loggerMethod("\n\n*** Calculation time in milliseconds*** " + String.Format("{0:#,###,###}", watch.ElapsedMilliseconds));

            MiscUtilities.CleanUp();
        }

        // /////////////////////////////////////////////////////



    }
}
