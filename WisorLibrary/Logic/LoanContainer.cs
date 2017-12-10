using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Utilities;

/*
 * Collect all the loans with the same ID and calculate the sum of remaining loan, the monthly payment
 * and create a new loan with those parameters
 */
namespace WisorLibrary.Logic
{
 
    public class LoanContainer
    {
        LoanList _loans;

        public LoanContainer()
        {
            _loans = new LoanList();
        }

        public void Add(loanDetails loan)
        {
            _loans.Add(loan);
        }

        public LoanList GroupLoansByID()
        {
            LoanList originalLoans = new LoanList(), returnLoans = new LoanList();
            string id = MiscConstants.UNDEFINED_STRING;
            loanDetails calcLoan = null;
            bool failedInCalculation = false;
            // TBD - print log . Should be removed
            //PrintOriginalLoanLuahSilukin(loans);

            WindowsUtilities.loggerMethod("NOTICE: GroupLoansByID collectedLoans size is: " + _loans.Count);
 
            for (int i = 0; i < _loans.Count; i++, failedInCalculation = false)
            {
                WindowsUtilities.loggerMethod("NOTICE: calculating the: " + (i + 1).ToString() + " loan out of: " + _loans.Count + ", amount: " + _loans[i].LoanAmount + " , date: " + _loans[i].DateTaken);

                // is it the same ID
                if (MiscConstants.UNDEFINED_STRING == id || id == _loans[i].ID)
                {
                    originalLoans.Add(_loans[i]);
                }
                else
                {
                    try
                    {
                        calcLoan = AccumulaLoanData(originalLoans);
                    }
                    catch (ArgumentOutOfRangeException /*aoore*/)
                    {
                        WindowsUtilities.loggerMethod("NOTICE: GroupLoansByID ArgumentOutOfRangeException occured: " /*+ aoore.ToString()*/ +
                            ". Skiping Loan id: " + id);
                        failedInCalculation = true;
                    }
                    if (! failedInCalculation)
                        returnLoans.Add(calcLoan);
                    originalLoans.Clear();
                    originalLoans.Add(_loans[i]);
                }
                id = _loans[i].ID;
            }


            if (0 < originalLoans.Count)
            {
                try
                {
                    calcLoan = AccumulaLoanData(originalLoans);
                    returnLoans.Add(calcLoan);
                }
                catch (ArgumentOutOfRangeException aoore)
                {
                    WindowsUtilities.loggerMethod("NOTICE: GroupLoansByID ArgumentOutOfRangeException occured: " + aoore.Message +
                        " in Loans id: " + originalLoans[0].ID);
                    string[] msg = { "NOTICE: GroupLoansByID ArgumentOutOfRangeException occured: " + aoore.Message +
                        " in Loans id: " + originalLoans[0].ID };
                    MiscUtilities.PrintSummaryFile(msg);
                }
            }

            WindowsUtilities.loggerMethod("NOTICE: LoanContainer got: " + _loans.Count + " loans, convert to: " + returnLoans.Count);
            Console.WriteLine("The Original loans:");
            for (int i = 0; i < _loans.Count; i++)
            {
                Console.WriteLine("Loan: " + (i+1).ToString() + " : " + _loans[i].ToString());
            }

            Console.WriteLine("\nThe grouped loans:\n");
            for (int i = 0; i < returnLoans.Count; i++)
            {
                Console.WriteLine("Loan: " + (i + 1).ToString() + " : " + returnLoans[i].ToString());
                // debug print
                //if (Share.shouldDebugLoans)
                //{
                //    MiscUtilities.PrintMiscLogger("The original Loan: " + (i + 1).ToString() + " : " + returnLoans[i].ToString());
                //    MiscUtilities.PrintMiscLogger("The luch silkin: " + returnLoans[i].resultReportData.ToString());
                //}
            }

            // calculate the luaah silukin 
            //GetTheLuahSilukin(ref returnLoans);

            return returnLoans;
        }


        loanDetails AccumulaLoanData(LoanList collectedLoans)
        {
            ResultReportData resultData = new ResultReportData();
            loanDetails ld = new loanDetails();
            ld.resultReportData = new ResultReportData();

            if (0 < collectedLoans.Count) {
                // ld = collectedLoans[0];

                uint originalAmount = MiscConstants.UNDEFINED_UINT,
                    desiredMonthlyPayment = MiscConstants.UNDEFINED_UINT,
                    yearlyIncome = MiscConstants.UNDEFINED_UINT;
                uint PayUntilToday = MiscConstants.UNDEFINED_UINT,
                    PayFuture = MiscConstants.UNDEFINED_UINT,
                    RemaingLoanAmount = MiscConstants.UNDEFINED_UINT,
                    MonthlyPaymentCalc = MiscConstants.UNDEFINED_UINT,
                    BankPayUntilToday = MiscConstants.UNDEFINED_UINT,
                    BankPayFuture = MiscConstants.UNDEFINED_UINT,
                    FirstMonthlyPMT = MiscConstants.UNDEFINED_UINT,
                    FirstMonthlyPMT2 = MiscConstants.UNDEFINED_UINT;

                for (int i = 0; i < collectedLoans.Count; i++)
                {
                    yearlyIncome = collectedLoans[i].YearlyIncome;
                    originalAmount += collectedLoans[i].OriginalLoanAmount;
                    desiredMonthlyPayment += collectedLoans[i].DesiredMonthlyPayment;
                    // calculate luch silukim
                    Calculations.CalculateLuahSilukinFullAll(
                        collectedLoans[i].indicesFirstTimePeriod, collectedLoans[i].indicesSecondTimePeriod,
                        collectedLoans[i].OriginalRate, collectedLoans[i].OriginalRate2,
                        collectedLoans[i].OriginalMargin, collectedLoans[i].OriginalMargin2,
                        collectedLoans[i].OriginalInflation, collectedLoans[i].firstTimePeriod,
                        collectedLoans[i].OriginalLoanAmount, collectedLoans[i].OriginalTime,
                        collectedLoans[i].DateTaken, collectedLoans[i].ID, ref resultData);
                 

                    // borrower data
                    PayUntilToday += resultData.PayUntilToday;
                    PayFuture += resultData.PayFuture;
                    RemaingLoanAmount += resultData.RemaingLoanAmount;
                    MonthlyPaymentCalc += resultData.MonthlyPaymentCalc;
                    // bank data
                    BankPayUntilToday += resultData.BankPayUntilToday;
                    BankPayFuture += resultData.BankPayFuture;
                    FirstMonthlyPMT += resultData.FirstMonthlyPMT;
                    FirstMonthlyPMT2 += resultData.FirstMonthlyPMT2;

                    // update the data in order to display in the report
                    collectedLoans[i].resultReportData.PayUntilToday = resultData.PayUntilToday;
                    collectedLoans[i].resultReportData.PayFuture = resultData.PayFuture;
                    collectedLoans[i].resultReportData.RemaingLoanAmount = resultData.RemaingLoanAmount;
                    collectedLoans[i].resultReportData.RemaingLoanTime = resultData.RemaingLoanTime;
                    collectedLoans[i].resultReportData.MonthlyPaymentCalc = resultData.MonthlyPaymentCalc;
                    collectedLoans[i].resultReportData.BankPayUntilToday = resultData.BankPayUntilToday;
                    collectedLoans[i].resultReportData.BankPayFuture = resultData.BankPayFuture;
                    collectedLoans[i].resultReportData.FirstMonthlyPMT = resultData.FirstMonthlyPMT;
                    collectedLoans[i].resultReportData.FirstMonthlyPMT2 = resultData.FirstMonthlyPMT2;
                    collectedLoans[i].resultReportData.EstimateFuturePay = resultData.PayFuture;
                    collectedLoans[i].resultReportData.EstimateProfitSoFar =
                        (int) (collectedLoans[i].resultReportData.PayUntilToday - collectedLoans[i].resultReportData.BankPayUntilToday);
                    collectedLoans[i].resultReportData.EstimateFutureProfit =
                        (int) (collectedLoans[i].resultReportData.PayFuture - collectedLoans[i].resultReportData.BankPayFuture);

                    if (0 < collectedLoans[i].OriginalLoanAmount) {
                        collectedLoans[i].resultReportData.EstimateProfitPercantageSoFar =
                             (double)collectedLoans[i].resultReportData.EstimateProfitSoFar / collectedLoans[i].OriginalLoanAmount;
                        collectedLoans[i].resultReportData.EstimateTotalProfitPercantage =
                            (double)collectedLoans[i].resultReportData.EstimateTotalProfit / collectedLoans[i].OriginalLoanAmount;
                        collectedLoans[i].resultReportData.EstimateFutureProfitPercantage =
                            (double)collectedLoans[i].resultReportData.EstimateFutureProfit / collectedLoans[i].OriginalLoanAmount;
                    }
                
                }

                UpdateLoanData(ref ld, collectedLoans[0], PayUntilToday, PayFuture, RemaingLoanAmount,
                    MonthlyPaymentCalc, BankPayUntilToday, BankPayFuture, yearlyIncome, originalAmount, FirstMonthlyPMT,
                    FirstMonthlyPMT2, desiredMonthlyPayment);


                // add the original loan' details to the calculated new loan in order to show it in the report
                ld.OriginalLoanDetaild = new LoanList();
                foreach (loanDetails loand in collectedLoans)
                {
                    ld.OriginalLoanDetaild.Add(loand);
                }
            }

            //if (Share.shouldDebugLoans)
            //{
            //    MiscUtilities.PrintMiscLogger("\n\nThe Accumulated loan:\n");
            //    MiscUtilities.PrintMiscLogger(ld.ToString());
            //}
        
            return ld;
        }



        void  UpdateLoanData(ref /*not needed but for the sake of god will...*/ 
                        loanDetails ld, loanDetails loan, uint PayUntilToday, uint PayFuture,
                        uint RemaingLoanAmount, uint MonthlyPaymentCalc, uint BankPayUntilToday, uint BankPayFuture,
                        uint yearlyIncome, uint originalAmount, uint FirstMonthlyPMT, uint FirstMonthlyPMT2, uint desiredMonthlyPayment)
        {
            // ld.LoanAmount = loan.LoanAmount;
            ld.ProductID = loan.ProductID;
            ld.resultReportData.ProductName = (null != loan.ProductID) ? loan.ProductID.stringTypeId : MiscConstants.UNDEFINED_STRING;
            ld.resultReportData.BankName = Share.CustomerName;
            ld.resultReportData.ID = ld.ID = loan.ID;
            // the age is passing...
            int years = MiscUtilities.CalculateYearsBetweenDates(loan.DateTaken, DateTime.Now);
            ld.resultReportData.BorrowerAge = ld.BorrowerAge = loan.BorrowerAge + (uint) years;
            ld.resultReportData.OriginalRate = ld.OriginalRate = loan.OriginalRate;
            ld.resultReportData.OriginalRate2 = ld.OriginalRate2 = loan.OriginalRate2;
            ld.resultReportData.OriginalTime = ld.OriginalTime = loan.OriginalTime;
            ld.resultReportData.fico = ld.fico = loan.fico;
            ld.resultReportData.FirstMonthlyPMT = FirstMonthlyPMT;
            ld.resultReportData.FirstMonthlyPMT2 = FirstMonthlyPMT2;
            ld.resultReportData.OriginalDateTaken = loan.OriginalDateTaken;
            ld.resultReportData.DateTaken = ld.DateTaken = DateTime.Now; // loan.DateTaken;
            ld.resultReportData.PropertyValue = ld.PropertyValue = loan.PropertyValue;
            ld.indicesFirstTimePeriod = loan.indicesFirstTimePeriod;
            ld.indicesSecondTimePeriod = loan.indicesSecondTimePeriod;
            ld.firstTimePeriod = loan.firstTimePeriod;
            ld.liquidity = loan.liquidity;
            ld.risk = loan.risk;
            ld.orderDataContainer2 = loan.orderDataContainer2;
            ld.OriginalDateTaken = loan.OriginalDateTaken;
            ld.resultReportData.OriginalInflation = ld.OriginalInflation = loan.OriginalInflation;
            ld.resultReportData.OriginalMargin = ld.OriginalMargin = loan.OriginalMargin;
            ld.resultReportData.OriginalMargin2 = ld.OriginalMargin2 = loan.OriginalMargin2;
            ld.resultReportData.PrintedOriginalMargin = ld.PrintedOriginalMargin = loan.PrintedOriginalMargin;
            ld.resultReportData.PrintedOriginalMargin2 = ld.PrintedOriginalMargin2 = loan.PrintedOriginalMargin2;
            ld.resultReportData.firstTimePeriod = ld.firstTimePeriod = loan.firstTimePeriod;
            ld.resultReportData.StartTime = loan.resultReportData.StartTime;
            ld.resultReportData.RemaingLoanAmount = ld.resultReportData.LoanAmount = ld.LoanAmount = RemaingLoanAmount;
            ld.resultReportData.RemaingLoanTime = loan.resultReportData.RemaingLoanTime;
            ld.resultReportData.YearlyIncome = ld.YearlyIncome = yearlyIncome;
            ld.resultReportData.OriginalLoanAmount = ld.OriginalLoanAmount = originalAmount;
            ld.DesiredMonthlyPayment = desiredMonthlyPayment;
            // the new monthly amount should be the sum of the all monthly payments today
            if (0 >= MonthlyPaymentCalc)
                ld.resultReportData.DesiredMonthlyPayment = /*ld.DesiredMonthlyPayment =*/ desiredMonthlyPayment;
            else 
                ld.resultReportData.DesiredMonthlyPayment = /*ld.DesiredMonthlyPayment =*/ MonthlyPaymentCalc;
            
            if (0 < ld.YearlyIncome)
                ld.resultReportData.PTI = (double)ld.DesiredMonthlyPayment / ld.YearlyIncome;
            ld.resultReportData.LTV = (double)ld.LoanAmount / ld.PropertyValue;
            ld.resultReportData.PayUntilToday = PayUntilToday;
            ld.resultReportData.PayFuture = PayFuture;
            ld.resultReportData.BankPayUntilToday = BankPayUntilToday;
            ld.resultReportData.BankPayFuture = BankPayFuture;
            ld.resultReportData.EstimateFuturePay = ld.resultReportData.PayFuture;
            // make consistancy checks
            if (ld.resultReportData.BankPayUntilToday > ld.resultReportData.PayUntilToday)
            {
                WindowsUtilities.loggerMethod("NOTICE: loan.ID: " + loan.ID + " the bank pay until today is larger than the borrower: " +
                    ld.resultReportData.BankPayUntilToday + " , " + ld.resultReportData.PayUntilToday);
                ld.resultReportData.EstimateProfitSoFar = 0;
            }
            else
            {
                ld.resultReportData.EstimateProfitSoFar =
                    (int) (ld.resultReportData.PayUntilToday - ld.resultReportData.BankPayUntilToday);
            }
            
            int sum = (int) ld.resultReportData.PayUntilToday + (int) ld.resultReportData.PayFuture -
                (int) ld.resultReportData.BankPayUntilToday - (int) ld.resultReportData.BankPayFuture;
            if (0 >= sum)
            {
                WindowsUtilities.loggerMethod("NOTICE: loan.ID: " + loan.ID + " illega EstimateTotalProfit which sum to: " +
                    sum);
                ld.resultReportData.EstimateTotalProfit = 0;
            }
            else
            {
                ld.resultReportData.EstimateTotalProfit = sum;
            }

            if (ld.resultReportData.BankPayFuture > ld.resultReportData.PayFuture)
            {
                WindowsUtilities.loggerMethod("NOTICE: loan.ID: " + loan.ID + " the bank pay future is larger than the borrower: " +
                    ld.resultReportData.BankPayFuture + " , " + ld.resultReportData.PayFuture);
                ld.resultReportData.EstimateProfitSoFar = 0;
            }
            else
            {
                ld.resultReportData.EstimateFutureProfit =
                    (int)(ld.resultReportData.PayFuture - ld.resultReportData.BankPayFuture);
            }

            if (0 >= ld.OriginalLoanAmount)
            {
                WindowsUtilities.loggerMethod("NOTICE: loan.ID: " + loan.ID + " illega OriginalLoanAmount: " +
                    ld.OriginalLoanAmount);
                ld.resultReportData.EstimateProfitPercantageSoFar = 0;
                ld.resultReportData.EstimateTotalProfitPercantage = 0;
                ld.resultReportData.EstimateFutureProfitPercantage = 0;
            }
            else
            {
                ld.resultReportData.EstimateProfitPercantageSoFar =
                     (double)ld.resultReportData.EstimateProfitSoFar / ld.OriginalLoanAmount;
                ld.resultReportData.EstimateTotalProfitPercantage =
                    (double)ld.resultReportData.EstimateTotalProfit / ld.OriginalLoanAmount;
                ld.resultReportData.EstimateFutureProfitPercantage =
                    (double)ld.resultReportData.EstimateFutureProfit / ld.OriginalLoanAmount;
            }
        }

        //void PrintOriginalLoanLuahSilukin(LoanList loans/*, LoanList groupedLoans*/)
        //{
        //    ResultReportData resultData = new ResultReportData();

        //    if (null != Share.theMiscLogger)
        //    {
        //        for (int i = 0; i < loans.Count; i++)
        //        {
        //            Share.theMiscLogger.PrintLog("\nLoan: " + (i + 1).ToString() + " : " + loans[i].ToString() + "\n");
        //            Share.theMiscLogger.PrintLog("\nCalculateLuahSilukinFullAll results:\n");
        //            Calculations.CalculateLuahSilukinFullAll(loans[i].indices,
        //                loans[i].OriginalRate, loans[i].OriginalMargin, loans[i].OriginalInflation,
        //                loans[i].OriginalLoanAmount, loans[i].OriginalTime,
        //                loans[i].DateTaken, ref resultData);
        //            Share.theMiscLogger.PrintLog("\n\n");
        //        }

        //        //Share.theMiscLogger.PrintLog("\nThe grouped loans:\n");
        //        //for (int i = 0; i < groupedLoans.Count; i++)
        //        //{
        //        //    Share.theMiscLogger.PrintLog("\nLoan: " + (i + 1).ToString() + " : " + groupedLoans[i].ToString() + "\n");
        //        //    Share.theMiscLogger.PrintLog("\n\n");
        //        //}
        //    }
        //}


        //void GetTheLuahSilukin(ref LoanList loans)
        //{
        //    ResultReportData resultData = new ResultReportData();

        //    try
        //    {
        //        for (int i = 0; i < loans.Count; i++)
        //        {
        //            Calculations.CalculateLuahSilukinFullAll(loans[i].indices,
        //                loans[i].OriginalRate, loans[i].OriginalMargin, loans[i].OriginalInflation,
        //                loans[i].OriginalLoanAmount, loans[i].OriginalTime,
        //                loans[i].DateTaken, ref resultData);

        //            loans[i].resultReportData.EstimateFuturePay = resultData.EstimateFuturePay;
        //            loans[i].resultReportData.EstimateProfitSoFar = resultData.EstimateProfitSoFar;
        //            loans[i].resultReportData.EstimateProfitPercantageSoFar = resultData.EstimateProfitPercantageSoFar;
        //            loans[i].resultReportData.EstimateTotalProfit = resultData.EstimateTotalProfit;
        //            loans[i].resultReportData.EstimateTotalProfitPercantage = resultData.EstimateTotalProfitPercantage;
        //            loans[i].resultReportData.EstimateFutureProfit = resultData.EstimateFutureProfit;
        //            loans[i].resultReportData.EstimateFutureProfitPercantage = resultData.EstimateFutureProfitPercantage;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        WindowsUtilities.loggerMethod("ERROR: GetTheLuahSilukin got Exception: " + e.ToString());
        //    }
        //}

    }
}
