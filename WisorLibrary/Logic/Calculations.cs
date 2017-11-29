using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.ReportApplication;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.Logic
{
    class Calculations
    {
        public static uint CalculateLuahSilukinBank(double rateFirstPeriod, double rateSecondPeriod, 
            int productFirstTimePeriod, /*indices*/ double productIndexUsedFirstTimePeriod,
            double optAmt, int optTime, double optPmt, 
            double indexFirstPeriod, double indexSecondPeriod, int optType, bool printOrNo
            //,out double ttlPay
            )
        {
            Interlocked.Add(ref Share.Calculation_CalculateLuahSilukinBankCounter, 1);
            // printOrNo = true;

            double optTtlPrincipalPay = MiscConstants.UNDEFINED_DOUBLE, optTtlRatePay = MiscConstants.UNDEFINED_DOUBLE;

            if (Share.ShouldPrintLog && printOrNo)
                MiscUtilities.PrintMiscLogger("Bank. amt= " + optAmt + ", m= " + optTime + ", r1= " + rateFirstPeriod + ", r2= "
                    + rateSecondPeriod);

            //Console.WriteLine("CalculateLuahSilukin product.firstTimePeriod: " + product.firstTimePeriod);
            double ttlPay = MiscConstants.UNDEFINED_DOUBLE;

            if (productFirstTimePeriod == 0)
            {
                if (productIndexUsedFirstTimePeriod == 0)
                {
                    double bankPmt = CalculatePmt2(optAmt, optTime, rateFirstPeriod, optType, productFirstTimePeriod,
                         rateFirstPeriod, rateSecondPeriod, indexFirstPeriod, indexSecondPeriod, printOrNo);
                    optTtlPrincipalPay = optAmt;
                    ttlPay = optTime * Math.Round(bankPmt, 2);
                    optTtlRatePay = ttlPay - optTtlPrincipalPay;
                    // debug - print to file
                    if (Share.ShouldPrintLog && printOrNo)
                        MiscUtilities.PrintMiscLogger("bank case1: indexUsedFirstTimePeriod == 0. ttlPayTmp: " + ttlPay +
                            ", optTtlRatePay: " + optTtlRatePay);
                }
                else
                {
                    double r = ((rateFirstPeriod / 12 * 100000000) - ((rateFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double bankPmt = CalculatePmt2(optAmt, optTime, rateFirstPeriod, optType, productFirstTimePeriod,
                                    rateFirstPeriod, rateSecondPeriod, indexFirstPeriod, indexSecondPeriod, printOrNo);
                    double monthlyPmt = Math.Round(bankPmt, 2);
                    double startingAmount = optAmt;

                    for (int months = 1; months <= optTime; months++)
                    {
                        double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                        double principalPmt = monthlyPmt - ratePmt;
                        optTtlPrincipalPay += principalPmt;
                        optTtlRatePay += ratePmt;
                        ttlPay += monthlyPmt;

                        // debug - print to file
                        if (Share.ShouldPrintLog && printOrNo)
                            MiscUtilities.PrintMiscLogger("Bank case2: " + months + " - " + startingAmount.ToString() + " - r= " + r.ToString() + " - i= " +
                                            i.ToString() + " - intPaid= " + ratePmt.ToString() + " - prinPaid= " +
                                            principalPmt.ToString() + " - pmt= " + monthlyPmt + " - ttlPay= " +
                                            ttlPay.ToString());

                        startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                        if (months < optTime)
                        {
                            monthlyPmt = Math.Round(
                                CalculatePmt2(startingAmount, (optTime - months), rateFirstPeriod,
                                optType, productFirstTimePeriod,
                                rateFirstPeriod, rateSecondPeriod,
                                indexFirstPeriod, indexSecondPeriod, printOrNo), 
                                2);
                        }
                    }
                }
            }
            else
            {
                double r = ((rateFirstPeriod / 12 * 100000000) - ((rateFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double bankPmt = CalculatePmt2(optAmt, optTime, rateFirstPeriod, optType, productFirstTimePeriod,
                                rateFirstPeriod, rateSecondPeriod, indexFirstPeriod, indexSecondPeriod, printOrNo);
                double monthlyPmt = Math.Round(bankPmt, 2);
                double startingAmount = optAmt;
                // need to update second period rate
                // rateSecondPeriod = rateFirstPeriod;
                // TBD Omri
                //optRateSecondPeriod = CalculateInterestRate4SecondPeriod()

                for (int months = 1; months <= productFirstTimePeriod; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    ttlPay += monthlyPmt;

                    // debug - print to file
                    if (Share.ShouldPrintLog && printOrNo)
                        MiscUtilities.PrintMiscLogger("Bank case3: " + months + " - " + startingAmount.ToString() + " - r= " + r.ToString() + " - i= " +
                                        i.ToString() + " - intPaid= " + ratePmt.ToString() + " - prinPaid= " +
                                        principalPmt.ToString() + " - pmt= " + monthlyPmt + " - ttlPay= " +
                                        ttlPay.ToString());

                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(
                            CalculatePmt2(startingAmount, (optTime - months), rateFirstPeriod,
                            optType, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo),
                            2);
                    }
                }

                // calculate again...
                monthlyPmt = // CalculatePmt2(startingAmount, (optTime - productFirstTimePeriod + 1), rateSecondPeriod, env);
                    CalculatePmt2(startingAmount, (optTime - productFirstTimePeriod + 1), rateSecondPeriod,
                            optType, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo);

                r = ((rateSecondPeriod / 12 * 100000000) - ((rateSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                i = ((indexSecondPeriod / 12 * 100000000) - ((indexSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                for (int months = productFirstTimePeriod + 1; months <= optTime; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    ttlPay += monthlyPmt;

                    // debug - print to file
                    if (Share.ShouldPrintLog)
                        MiscUtilities.PrintMiscLogger("Bank case4: " + months + " - " + startingAmount.ToString() + " - r= " + r.ToString() + " - i= " +
                                        i.ToString() + " - intPaid= " + ratePmt.ToString() + " - prinPaid= " +
                                        principalPmt.ToString() + " - pmt= " + monthlyPmt + " - ttlPay= " +
                                        ttlPay.ToString());

                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(
                            CalculatePmt2(startingAmount, (optTime - months), rateSecondPeriod,
                            optType, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo), 
                            2);
                    }
                }
            }

            return (uint)Math.Round(ttlPay);
        }
        

        public static double CalculatePmt2(double amtForCalc, int timeForCalc, double interestRateForCalc,
            int optType, /*double optAmt,*/ int productFirstTimePeriod, 
            double rateFirstPeriod, double rateSecondPeriod,
            double indexFirstPeriod, double indexSecondPeriod,  bool printOrNo)
        {
             double currInterest = MiscConstants.UNDEFINED_DOUBLE;

            //Console.WriteLine("CalculatePmt amtForCalc: " + amtForCalc + ", timeForCalc: " + timeForCalc + ", interestRateForCalc: " + interestRateForCalc);
            if ((MiscConstants.UNDEFINED_INT != optType) && (amtForCalc > 0) && (timeForCalc > 0))
            {
                if ((amtForCalc > 0) && (timeForCalc > 0))
                {
                    if (productFirstTimePeriod <= timeForCalc)
                    {
                        currInterest = rateFirstPeriod = interestRateForCalc; 
                    }
                    else
                    {
                        currInterest = rateSecondPeriod = interestRateForCalc; 
                    }
                }
                if (printOrNo == true)
                {
                    Console.WriteLine("Option:\nType = " + optType + "\nAmount = " + amtForCalc
                                + "\nTime in months = " + timeForCalc + "\nYearly Rate = " + interestRateForCalc
                                + "\nYearly Inflation = " + indexFirstPeriod + " \n");
                }
                double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                if (0 == currInterest)
                {
                    double monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round
                    if (printOrNo == true)
                    {
                        double r = 0;
                        double calcPow = 0;
                        Console.WriteLine("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
                    }
                    return monthlyPmt;
                }
                else /*if (currInterest > 0)*/
                // bank rate can be negative
                {
                    double r = ((currInterest / 12 * 100000000) - ((currInterest / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    // TBD - suspected as performance bottleneck
                    double calcPow = Math.Pow((1 + r), timeForCalc);
                    double monthlyPmt = ((amtForCalc * (1 + i)) * (r * calcPow) / (calcPow - 1));
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                    if (printOrNo == true)
                    {
                        Console.WriteLine("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
                    }
                    return monthlyPmt;
                }
                //else
                //{
                //    Console.WriteLine("ERROR: CalculatePmt2 Rate Out of Range. currInterest: " + currInterest);
                //}

            }
            else
            {
                if (MiscConstants.UNDEFINED_INT == optType)
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Option Type Out of Range");
                }
                else if (amtForCalc <= CalculationConstants.optionMinimumAmount)
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Option Amount Out of Range");
                }
                else
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Option Time Out of Range");
                }
            }

            return currInterest;
        }

        /// <summary>
        /// Calculate the remaing amount once the load have been started already
        /// </summary>
        /// <param name="dateTaken"></param>
        /// <param name="originalProduct"></param>
        /// <param name="originalRate"></param>
        /// <param name="originalTime"></param>
        /// <returns></returns>
    
        //public static void CalculateRemainingAmount(indices indices, double originalLoanAmount, uint originalLoanTime, 
        //     DateTime dateLoanTaken, double originalRate, double originalInflation, 
        //     ref ResultReportData resultReportData,
        //     RunEnvironment env = null)
        // {
 
        //    resultReportData.FirstMonthlyPMT = (uint) Math.Round(CalculateMonthlyPmt(originalLoanAmount, originalLoanTime,
        //        originalRate, originalInflation));
 
        //    CalculateLuahSilukinSoFar2(
        //        indices,
        //        originalRate, originalInflation,
        //        originalLoanAmount, originalLoanTime,
        //        dateLoanTaken, ref resultReportData, env);
        //}
        
        // **************************************************************************************************************************** //
        // **************************************** Getting Inflation According to Option Type **************************************** //

        //private static string PresentPastOrFuture(uint mm, uint yy)
        //{
        //    string resultToReturn = "";

        //    if (yy < DateTime.Today.Year)
        //    {
        //        resultToReturn = "P";
        //    }
        //    else if (yy == DateTime.Today.Year)
        //    {
        //        if (mm < DateTime.Today.Month)
        //        {
        //            resultToReturn = "P";
        //        }
        //        else if (mm == DateTime.Today.Month)
        //        {
        //            resultToReturn = "N";
        //        }
        //        else
        //        {
        //            resultToReturn = "F";
        //        }
        //    }
        //    else
        //    {
        //        resultToReturn = "F";
        //    }
        //    return resultToReturn;
        //}
        
        // **************************************************************************************************************************** //
        // ******************************* Calculating PMT According to Option Type and Time and Rate ********************************* //

        public static double CalculateMonthlyPmt(double amtForCalc, uint timeForCalc, /*double interestRateForCalc,*/
            double originalRate, double originalInflation)
        {
            if (0 >= timeForCalc)
            {
                //Console.WriteLine("ERROR: CalculateMonthlyPmt got illegal timeForCalc: " + timeForCalc);
                return 0;
            }

            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

            if (originalRate == 0)
            {
                double monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                return monthlyPmt;
            }
            // the rate now can be negative as well
            else /*if (originalRate > 0)*/
            {
                double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double calcPow = Math.Pow((1 + r), timeForCalc);
                // be carfull here...
                double upperSide = (amtForCalc * (1 + i)) * (r * calcPow);
                double monthlyPmt = MiscConstants.UNDEFINED_DOUBLE;
                if (0 == upperSide)
                {
                    monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round
                }
                else
                {
                    monthlyPmt = (upperSide / (calcPow - 1));
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round
                }

                return monthlyPmt;
            }
            //else
            //{
            //    Console.WriteLine("ERROR: CalculateMonthlyPmt got illegal originalRate: " + originalRate);
            //    // return 0;
            //    throw new System.ArgumentOutOfRangeException("(CalculateMonthlyPmt) Rate Out of Range. originalRate: " + originalRate.ToString());
            //}
        }
        

        // **************************************************************************************************************************** //
        // ********************************** PRIVATE - Calculating full luah silukin for option ************************************** //

        //private static double CalculateLuahSilukinSoFar(
        //    double originalRate, double originalInflation,
        //    double originalLoanAmount, uint originalLoanTime,
        //    uint monthOfDateLoanTaken, uint yearOfDateLoanTaken,
        //    double interestPaidSoFar, double totalPaidSoFar,
        //    double firstMonthlyPMT, double principalPaidSoFar, out uint remaingLoanTime)
        //{

        //    double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
        //    double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

        //    double monthlyPmt = Math.Round(firstMonthlyPMT, 2);
        //    double startingAmount = originalLoanAmount;

        //    string timeString = PresentPastOrFuture(monthOfDateLoanTaken, yearOfDateLoanTaken);
        //    uint timeCounter = 1;
        //    uint monthCounter = monthOfDateLoanTaken;
        //    uint yearCounter = yearOfDateLoanTaken;
            
        //    remaingLoanTime = originalLoanTime;

        //    while (timeString != "F")
        //    {
        //        remaingLoanTime--;
        //        double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
        //        double principalPmt = monthlyPmt - ratePmt;
        //        principalPaidSoFar += principalPmt;
        //        interestPaidSoFar += ratePmt;
        //        totalPaidSoFar += monthlyPmt;
        //        startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                
        //        if (timeCounter < originalLoanTime)
        //        {
        //            monthlyPmt = Math.Round(CalculateMonthlyPmt(startingAmount, (originalLoanTime - timeCounter),
        //                /*originalRate,*/ originalRate, originalInflation), 2);
        //            timeCounter++;
        //        }

        //        if (monthCounter == 12)
        //        {
        //            monthCounter = 1;
        //            yearCounter++;
        //        }
        //        else
        //        {
        //            monthCounter++;
        //        }
        //        timeString = PresentPastOrFuture(monthCounter, yearCounter);
        //     }

        //    return startingAmount;

        //}

        public static void Log(string msg)
        {
            MiscUtilities.Log2File(msg);
        }

        /*
         * OriginalLoanTable4ShortUK olt4s = new OriginalLoanTable4ShortUK(
                        (int)c.opts[j].optAmt, c.opts[j].product.name,
                        (int)c.opts[j].optTime, c.opts[j].optRateFirstPeriod * 100,  c.opts[j].optRateSecondPeriod * 100, 
                        (int)c.opts[j].optPmt, (int)c.opts[j].optPmt2, 
                        (int)c.opts[j].optTtlPay);

          public OriginalLoanTable4ShortUK(int amount, string product, int time, double rate, double followOnRate, 
                    int monthlyPMT, int monthly, int totalPayment)

        */

#if NEED_2_COMPLETE

#else
        static void  AddAmortisationData(AmortisationData amortisationData, ref AmortisationData acculumaleAmortisationData)
        {
            if (null == acculumaleAmortisationData || null == acculumaleAmortisationData.AmortisationTable || 0 >= acculumaleAmortisationData.AmortisationTable.Length)
            {
                acculumaleAmortisationData = new AmortisationData();
                acculumaleAmortisationData.AmortisationTable = amortisationData.AmortisationTable;
            }
            else
            {
                int length = Math.Min(amortisationData.AmortisationTable.Length, acculumaleAmortisationData.AmortisationTable.Length);
         
                for (int i = 0; i < length; i++)
                {
                    int currentYear = amortisationData.AmortisationTable[i].Year;
                    // find the same year in the acculumaleAmortisationData
                    // add the fields
                    acculumaleAmortisationData.AmortisationTable[i].MonthlyPmt += amortisationData.AmortisationTable[i].MonthlyPmt;
                    acculumaleAmortisationData.AmortisationTable[i].PaidSoFar += amortisationData.AmortisationTable[i].PaidSoFar;
                    acculumaleAmortisationData.AmortisationTable[i].RemainingAmount += amortisationData.AmortisationTable[i].RemainingAmount;
                    acculumaleAmortisationData.AmortisationTable[i].Year = amortisationData.AmortisationTable[i].Year;
                }
               
            }
        }

        // Calculate the luach silukin and get the entire monthly data as well for the entire composition
        public static void CalculateLuahSilukinAllResultsForComposition(Composition composition, ref AmortisationData amortisationData)
        {
            AmortisationData currentAmortisationData = new AmortisationData();

            MiscUtilities.PrintMiscLogger("\nPrint accumulate AmortisationData for new composition");
            if (null != composition && null != composition.opts)
            {
                for (int i = 0; i < composition.opts.Length; i++)
                {
                    if (null != composition.opts[i])
                    {
                        GenericProduct gp = composition.opts[i].product;
                        CalculateLuahSilukinAllResults(gp.originalIndexUsedFirstTimePeriod, // indices 
                           composition.opts[i].optRateFirstPeriod /* originalRate*/, gp.indexUsedFirstTimePeriod /*originalInflation*/,
                           composition.opts[i].optAmt /*originalLoanAmount*/, composition.opts[i].optTime /*originalLoanTime*/,
                           DateTime.Now /*dateLoanTaken*/, ref currentAmortisationData,
                           null /*RunEnvironment env*/, false /*IsBank*/);

                        AddAmortisationData(currentAmortisationData, ref amortisationData);

                        // debug
                        MiscUtilities.PrintMiscLogger("\nPrint accumulate AmortisationData while adding product: " + gp.name);
                        MiscUtilities.PrintAmortisationData(amortisationData, true /*2file*/);
                    }
                }
            }
        }
#endif

        // Calculate the luach silukin and get the entire monthly data as well
        public static void CalculateLuahSilukinAllResults(indices indices,
           double originalRate, double originalInflation,
           double originalLoanAmount, uint originalLoanTime,
           DateTime dateLoanTaken, ref AmortisationData amortisationData,
           RunEnvironment env, bool IsBank)
        {
            List<AmortisationTable> amortisationTable = new List<AmortisationTable>();
            int NumOfYears = 0;
            double interestPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
                totalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
                principalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE;
            double startingAmount = originalLoanAmount;
            //calculationData.RemaingLoanTime = originalLoanTime;
            double primeMargin = MiscConstants.UNDEFINED_DOUBLE;
            double historicRate = MiscUtilities.GetHistoricIndexRateForDate(indices, dateLoanTaken, originalRate,
                out primeMargin, IsBank);
            historicRate += primeMargin;
            // the difference between the original rate and the historic rate define the base for the product
            double productPlanRate = historicRate - originalRate;
            double rate2calculateR = (IsBank ? historicRate : originalRate);
            double r = ((rate2calculateR / 12 * 100000000) - ((rate2calculateR / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double monthlyPmt = CalculateMonthlyPmt(originalLoanAmount, originalLoanTime, historicRate, originalInflation); ;
            //calculationData.FirstMonthlyPMT = (uint)Math.Round(monthlyPmt);
            int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(dateLoanTaken, DateTime.Now);
            double currentRate = originalRate, ratePmt = MiscConstants.UNDEFINED_DOUBLE, principalPmt = MiscConstants.UNDEFINED_DOUBLE;
            DateTime currentDate = dateLoanTaken;
            int m;
            double ratePmtFuture = MiscConstants.UNDEFINED_DOUBLE, principalPmtFuture = MiscConstants.UNDEFINED_DOUBLE, principalPayFuture = MiscConstants.UNDEFINED_DOUBLE,
                 optTtlRatePayFuture = MiscConstants.UNDEFINED_DOUBLE,
                 totalPaidFuture = MiscConstants.UNDEFINED_DOUBLE,
                 startingAmountFuture, monthlyPmtFuture;
            double redundantValue;
 
            double monthlyPmtCalc;
            for (m = 1; m <= numOfMonths; m++)
            {
                ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                principalPmt = monthlyPmt - ratePmt;
                principalPaidSoFar += principalPmt;
                interestPaidSoFar += ratePmt;
                totalPaidSoFar += monthlyPmt;
                //calculationData.RemaingLoanTime--;

                // calculate the exact historical rate 
                currentDate = dateLoanTaken.AddMonths(m);
                //historicRate = MiscUtilities.GetHistoricIndexRateForPeriod(indices, currentDate);
                historicRate = MiscUtilities.GetHistoricIndexRateForDate(indices, currentDate, originalRate,
                    out redundantValue, IsBank);
                historicRate += primeMargin;
                currentRate = (IsBank ? historicRate : historicRate + productPlanRate);
                r = ((currentRate / 12 * 100000000) - ((currentRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                monthlyPmtCalc = Math.Round(CalculateMonthlyPmt(startingAmount, (uint)(originalLoanTime - m),
                        currentRate, originalInflation), 2);
                if (0 < (uint)Math.Round(monthlyPmtCalc))
                {
                    monthlyPmt = monthlyPmtCalc;
                }

                if (0 == m % 12)
                {
                    amortisationTable.Add(new AmortisationTable(NumOfYears, startingAmount /*remaining amount*/,
                        totalPaidSoFar /*accululate payment*/, monthlyPmt));
                    NumOfYears++;
                }
            }

            startingAmountFuture = startingAmount;
            monthlyPmtFuture = monthlyPmt;

            // calculate from now till the end loan' time
            for (/*m = numOfMonths*/; m <= originalLoanTime; m++)
            {
                ratePmtFuture = Math.Round((startingAmountFuture * (1 + i) * r), 2);
                principalPmtFuture = monthlyPmtFuture - ratePmtFuture;
                principalPayFuture += principalPmtFuture;
                optTtlRatePayFuture += ratePmtFuture;
                totalPaidFuture += monthlyPmtFuture;
                startingAmountFuture = Math.Round((((startingAmountFuture) * (1 + i)) - principalPmtFuture), 2);
                monthlyPmtCalc = Math.Round(
                    CalculateMonthlyPmt(startingAmountFuture, (uint)(originalLoanTime - m),
                    currentRate, originalInflation), 2);
                if (0 < (uint)Math.Round(monthlyPmtCalc))
                {
                    monthlyPmtFuture = monthlyPmtCalc;
                }

                if (0 == m % 12)
                {
                    amortisationTable.Add(new AmortisationTable(NumOfYears, startingAmountFuture /*remaining amount*/,
                        totalPaidSoFar + totalPaidFuture /*accululate payment*/, monthlyPmtFuture));
                    NumOfYears++;
                }

            }
            amortisationData.AmortisationTable = amortisationTable.ToArray();
        }


 

        /*
         * This calculation is manily for the products which have a rate dependency 
         * actually the Israel PRIME and the ARM products
         */
        
        public static void CalculateLuahSilukinFull(
            indices indicesFirstTimePeriod, 
            double originalRate, double originalInflation,
            double originalLoanAmount, uint originalLoanTime, uint currentPeriodTime,
            DateTime dateLoanTaken, ref ResultReportData calculationData,
            RunEnvironment env, bool IsBank)
        {
            double interestPaidSoFar = MiscConstants.UNDEFINED_DOUBLE, 
                totalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE, 
                principalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE;
            double startingAmount = originalLoanAmount;
            calculationData.RemaingLoanTime = originalLoanTime;
            double primeMargin = MiscConstants.UNDEFINED_DOUBLE;
            double historicRate = MiscUtilities.GetHistoricIndexRateForDate(indicesFirstTimePeriod, dateLoanTaken, originalRate,
                out primeMargin, IsBank);
            historicRate += primeMargin;
            // the difference between the original rate and the historic rate define the base for the product
            double productPlanRate = historicRate - originalRate;
            double rate2calculateR = (IsBank ? historicRate : originalRate);
            double r = ((rate2calculateR / 12 * 100000000) - ((rate2calculateR / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double monthlyPmt = CalculateMonthlyPmt(originalLoanAmount, originalLoanTime, historicRate, originalInflation); ;
            calculationData.FirstMonthlyPMT = (uint)Math.Round(monthlyPmt);
            int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(dateLoanTaken, DateTime.Now);
            int loopCounter = Math.Min(numOfMonths, (int)currentPeriodTime /*originalLoanTime*/);
            double currentRate = originalRate, ratePmt = MiscConstants.UNDEFINED_DOUBLE, principalPmt = MiscConstants.UNDEFINED_DOUBLE;
            DateTime currentDate = dateLoanTaken;
            double ratePmtFuture = MiscConstants.UNDEFINED_DOUBLE, principalPmtFuture = MiscConstants.UNDEFINED_DOUBLE, principalPayFuture = MiscConstants.UNDEFINED_DOUBLE,
                 optTtlRatePayFuture = MiscConstants.UNDEFINED_DOUBLE,
                 totalPaidFuture = MiscConstants.UNDEFINED_DOUBLE,
                 startingAmountFuture, monthlyPmtFuture;
            double redundantValue;
            string msg;
            int m;

            Interlocked.Add(ref Share.Calculation_CalculateLuahSilukinCounter, 1);

            //try
            //{
            // calculate till now
            bool shouldLog = false; //  true;
            double monthlyPmtCalc;
            for (m = 1; m <= loopCounter /*numOfMonths*/; m++)
            {
                ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                principalPmt = monthlyPmt - ratePmt;
                principalPaidSoFar += principalPmt;
                interestPaidSoFar += ratePmt;
                totalPaidSoFar += monthlyPmt;
                calculationData.RemaingLoanTime--;

                //// for debug:
                if (shouldLog)
                {
                    msg = m + "," + startingAmount + "," + ratePmt + "," + principalPmt + "," + r + "," +
                      historicRate + "," + currentRate + "," + monthlyPmt + "," + i + "," + totalPaidSoFar + "," + numOfMonths;
                    //shouldLog = false;
                    //Log("First run of CalculateLuahSilukinFull past");
                    Log(msg);
                }
                    
                // calculate the exact historical rate 
                currentDate = dateLoanTaken.AddMonths(m);
                //historicRate = MiscUtilities.GetHistoricIndexRateForPeriod(indices, currentDate);
                historicRate = MiscUtilities.GetHistoricIndexRateForDate(indicesFirstTimePeriod, currentDate, originalRate, 
                    out redundantValue, IsBank);
                historicRate += primeMargin;
                currentRate = (IsBank ? historicRate : historicRate + productPlanRate);
                r = ((currentRate / 12 * 100000000) - ((currentRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                monthlyPmtCalc = Math.Round(CalculateMonthlyPmt(startingAmount, (uint)(originalLoanTime - m),
                        currentRate, originalInflation), 2);
                if (0 < (uint)Math.Round(monthlyPmtCalc))
                {
                    monthlyPmt = monthlyPmtCalc;
                }
            }

            //msg = m + "," + startingAmount + "," + ratePmt + "," + principalPmt + "," + r + "," +
            //        historicRate + "," + currentRate + "," + monthlyPmt + "," + i + "," + totalPaidSoFar + "," + numOfMonths;
            //Log("Last run of CalculateLuahSilukinFull past");
            //Log(msg);
  
            startingAmountFuture = startingAmount;
            monthlyPmtFuture = monthlyPmt;

            Log("\nAnd now for the futur\n");
            shouldLog = false; //  true;

            // calculate from now till the end loan' time
            for (/*m = numOfMonths*/; m <= originalLoanTime; m++)
            {
                ratePmtFuture = Math.Round((startingAmountFuture * (1 + i) * r), 2);
                principalPmtFuture = monthlyPmtFuture - ratePmtFuture;
                principalPayFuture += principalPmtFuture;
                optTtlRatePayFuture += ratePmtFuture;
                totalPaidFuture += monthlyPmtFuture;
                startingAmountFuture = Math.Round((((startingAmountFuture) * (1 + i)) - principalPmtFuture), 2);
                monthlyPmtCalc = Math.Round(
                    CalculateMonthlyPmt(startingAmountFuture, (uint)(originalLoanTime - m),
                    currentRate, originalInflation), 2);
                if (0 < (uint)Math.Round(monthlyPmtCalc))
                {
                    monthlyPmtFuture = monthlyPmtCalc;
                }

                // for debug:
                if (shouldLog)
                {
                    msg = m + "," + startingAmountFuture + "," + ratePmtFuture + "," + principalPmtFuture + "," + r + ","
                          + historicRate + "," + currentRate + "," + monthlyPmtFuture + "," + i + "," + totalPaidFuture
                          + "," + m + "," + originalLoanTime;
                    //shouldLog = false;
                    //Log("First run of CalculateLuahSilukinFull future");
                    Log(msg);
                }
            }

            //msg = m + "," + startingAmountFuture + "," + ratePmtFuture + "," + principalPmtFuture + "," + r + ","
            //            + historicRate + "," + currentRate + "," + monthlyPmtFuture + "," + i + "," + totalPaidFuture
            //            + "," + m + "," + originalLoanTime;
            //Log("Last run of CalculateLuahSilukinFull future");
            //Log(msg);

            calculationData.PayUntilToday = (uint)Math.Round(totalPaidSoFar);
            calculationData.PayFuture = (uint)Math.Round(totalPaidFuture);
            calculationData.RemaingLoanAmount = (uint)Math.Round(startingAmount);
            calculationData.MonthlyPaymentCalc = (uint)Math.Round(monthlyPmt);
            //}
            //catch (Exception ex)
            //{
            //    WindowsUtilities.loggerMethod("ERROR: CalculateLuahSilukinSoFar2 got Exception: " + ex.ToString());
            //    throw;
            //}
        }

        public static void CalculateLuahSilukinFullAll(indices indicesFirstTimePeriod, indices indicesSecondTimePeriod,
            double originalBorrowerRate, double originalBorrowerRate2,
            double originalbankRate, double originalbankRate2, 
            double originalInflation, int firstTimePeriod,
            double originalLoanAmount, uint originalLoanTime,
            DateTime dateLoanTaken, string loanID, ref ResultReportData calculationData,
            RunEnvironment env = null)
        {
            ResultReportData calculationBorrowerData = new ResultReportData();
            ResultReportData calculationBankData = new ResultReportData();

            // set the proper rate by the time period
            // if First Time Period == 0 , use the indexUsedFirstTimePeriod value
            // if First Time Period > 0 then: 
            //  calculate the first time with the rates 
            //  calculate the remaining time with the second time rates
            //  accululate the number, for the borrower and the bank

            if (markets.UK == Share.theMarket)
            {
                DriveCalculateLuahSilukinFullUK(indicesFirstTimePeriod, indicesSecondTimePeriod,
                    originalBorrowerRate, originalBorrowerRate2,
                    originalbankRate, originalbankRate2,
                    originalInflation, firstTimePeriod,
                    originalLoanAmount, originalLoanTime,
                    dateLoanTaken, loanID, ref calculationData,
                    env);
            }
            else
            {
                // write to log file
                Log("\nCalculate Luah Silukin for Borrower:\n");
                // the borrower side
                CalculateLuahSilukinFull(indicesFirstTimePeriod, originalBorrowerRate, originalInflation, originalLoanAmount, 
                    originalLoanTime, originalLoanTime,
                    dateLoanTaken, ref calculationBorrowerData, env, false /*IsBank*/);
               
                // the bank side
                Log("\nCalculate Luah Silukin for Bank:\n");
                CalculateLuahSilukinFull(indicesFirstTimePeriod, originalbankRate, originalInflation, originalLoanAmount, 
                    originalLoanTime, originalLoanTime,
                    dateLoanTaken, ref calculationBankData, env, true /*IsBank*/);

                CopyResultRepportData(originalLoanAmount, calculationData, calculationBorrowerData, calculationBankData);

            }

            string msg =
                "\nLoan details: " + loanID +
                "\nDate taken: " + dateLoanTaken +
                "\nAmount: " + originalLoanAmount +
                "\nTerm: " + originalLoanTime +
                "\nRate: " + originalBorrowerRate +
                "\nMargin: " + originalbankRate +
                "\nInflation: " + originalInflation +
                "\nIdices: " + indicesFirstTimePeriod +
                "\nFirst PMT: " + calculationData.FirstMonthlyPMT +
                "\nPaid until today: " + calculationData.PayUntilToday +
                "\nLeft to pay: " + calculationData.RemaingLoanAmount +
                "\nBank Paid until today: " + calculationData.BankPayUntilToday +
                "\nBank Left to pay: " + calculationData.BankPayFuture +
                "\nEstimate future payment: " + calculationData.EstimateFuturePay +
                "\nEstimate % profit so far: " + calculationData.EstimateProfitPercantageSoFar +
                "\nEstimate profit so far: " + calculationData.EstimateProfitSoFar +
                "\nEstimate total % profit: " + calculationData.EstimateTotalProfitPercantage +
                "\nEstimate total profit: " + calculationData.EstimateTotalProfit +
                "\nEstimate future profit: " + calculationData.EstimateFutureProfit +
                "\nEstimate future % profit : " + calculationData.EstimateFutureProfitPercantage;
  
                Log(msg);
            }


        //private static void CalculateLuahSilukinSoFar2(indices indices,
        //    double originalRate, double originalInflation,
        //    double originalLoanAmount, uint originalLoanTime,
        //    DateTime dateLoanTaken, ref ResultReportData calculationData,
        //    RunEnvironment env)
        //{
        //    double interestPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
        //        totalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
        //        principalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE;
        //    double startingAmount = originalLoanAmount;
        //    calculationData.RemaingLoanTime = originalLoanTime;
        //    double historicRate = MiscUtilities.GetHistoricIndexRateForDate(indices, dateLoanTaken);
        //    // the difference between the original rate and the historic rate define the base for the product
        //    double productPlanRate = originalRate - historicRate;
        //    double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
        //    double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
        //    double monthlyPmt = calculationData.FirstMonthlyPMT;
        //    int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(dateLoanTaken, DateTime.Now);
        //    double currentRate = originalRate, ratePmt, principalPmt;
        //    DateTime currentDate = dateLoanTaken;
        //    int m;

        //    try
        //    {
        //        if (null != env)
        //            env.WriteToOutputFile("historicRate: " + historicRate + ", productPlanRate: " + productPlanRate);

        //        for (m = 1; m <= numOfMonths; m++)
        //        {
        //            ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
        //            principalPmt = monthlyPmt - ratePmt;
        //            principalPaidSoFar += principalPmt;
        //            interestPaidSoFar += ratePmt;
        //            totalPaidSoFar += monthlyPmt;

        //            calculationData.RemaingLoanTime--;
        //            // for debug:
        //            if (null != env)
        //                env.WriteToOutputFile(m + "," + startingAmount + "," + principalPmt +
        //                    "," + ratePmt + "," + r + "," + historicRate + "," + currentRate + "," + monthlyPmt);

        //            // calculate the exact historical rate 
        //            currentDate = dateLoanTaken.AddMonths(m);
        //            historicRate = MiscUtilities.GetHistoricIndexRateForPeriod(indices, currentDate);
        //            currentRate = historicRate + productPlanRate;
        //            r = ((currentRate / 12 * 100000000) - ((currentRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
        //            startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
        //            monthlyPmt = Math.Round(CalculateMonthlyPmt(startingAmount, (uint)(originalLoanTime - m),
        //                    currentRate, originalInflation), 2);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WindowsUtilities.loggerMethod("ERROR: CalculateLuahSilukinSoFar2 got Exception: " + ex.ToString());
        //    }
        //    //calculationData.PayUntilToday = (uint)Math.Round(totalPaidSoFar);
        //    //calculationData.RemaingLoanAmount = (uint)Math.Round(startingAmount);

        //}




        // The maximun and minimum amounts is calculated now by the Risk and Liquidity scores of the user
        // for primeIsrael (indids == PRIME) then the min = max amount
        public static MinMax FindMinMaxAmount(double loanAmtWanted, OneOptType optType, Risk risk, 
            Liquidity liquidity, double prevMin)
        {
            MinMax minmax = new MinMax();

            if (null == optType.product)
            {
                WindowsUtilities.loggerMethod("ERROR FindMinMaxAmount null product.");
            }
            else
            {
                if (MiscUtilities.Use3ProductsInComposition())
                {
                    // TBD: should check if this is the case when the interest is low 
                    if (indices.PRIME == optType.product.originalIndexUsedFirstTimePeriod)
                    {
                        minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                        minmax.min = minmax.max;
                    }
                    else
                    {
                        if (Risk.NONERisk == risk || Liquidity.NONELiquidity == liquidity)
                        {
                            // keep the old good stuff...
                            minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                            minmax.min = (int)prevMin; // ... don't touch
                        }
                        else
                        {
                            RiskLiquidityValue riskLiquidityValue = new RiskLiquidityValue();
                            riskLiquidityValue.liquidity = liquidity;
                            riskLiquidityValue.risk = risk;
                            riskLiquidityValue.productID = optType.product.productID;
                            bool rc = MiscUtilities.FindRiskLiquidity(riskLiquidityValue);
                            if (rc)
                            {
                                // should calculate the loan values according to the percantage
                                minmax.min = (int)(loanAmtWanted * riskLiquidityValue.min / 100 * 100);
                                minmax.max = (int)(loanAmtWanted * riskLiquidityValue.max / 100 * 100);
                            }
                            else
                            {
                                Console.WriteLine("FindMinMaxAmount failed for: " + riskLiquidityValue.ToString());
                                // keep the old good stuff...
                                minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                                minmax.min = (int)prevMin; // ... don't touch
                            }
                        }
                    }
                }
                else // 2 products in a combination
                {
                    if (indices.PRIME == optType.product.originalIndexUsedFirstTimePeriod)
                    {
                        minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                        minmax.min = (int)prevMin;
                    }
                    else
                    {
                        if (Risk.NONERisk == risk || Liquidity.NONELiquidity == liquidity)
                        {
                            // keep the old good stuff...
                            minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                            minmax.min = (int)prevMin; // ... don't touch
                        }
                        else
                        {
                            RiskLiquidityValue riskLiquidityValue = new RiskLiquidityValue();
                            riskLiquidityValue.liquidity = liquidity;
                            riskLiquidityValue.risk = risk;
                            riskLiquidityValue.productID = optType.product.productID;
                            bool rc = MiscUtilities.FindRiskLiquidity(riskLiquidityValue);
                            if (rc)
                            {
                                // should calculate the loan values according to the percantage
                                minmax.min = (int)(loanAmtWanted * riskLiquidityValue.min / 100 * 100);
                                minmax.max = (int)(loanAmtWanted * riskLiquidityValue.max / 100 * 100);
                            }
                            else
                            {
                                Console.WriteLine("FindMinMaxAmount failed for: " + riskLiquidityValue.ToString());
                                // keep the old good stuff...
                                minmax.max = (int)(Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                                minmax.min = (int)prevMin; // ... don't touch
                            }
                        }
                    }
                }
                Console.WriteLine("FindMinMaxAmount for product: " + optType.product.name + " ,loanAmtWanted: " + loanAmtWanted
                    + ", return min: " + minmax.min + ", max: " + minmax.max);

            }
            return minmax;
        }

        private static double FindMaxAmount(double loanAmountTemp, OneOptType optType)
        {
            double loanAmount = loanAmountTemp;
            OneOptType optTypeForTest = optType;
            int numOfFixedOptions = MiscUtilities.GetNumberOfProductsInCombination() - 1;
            double result = MiscConstants.UNDEFINED_DOUBLE;

            // Omri - need the maxPercentageOfLoan value
            if (optTypeForTest.product.maxPercentageOfLoan < 100)
            {
                if ((((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100) / 100) - (((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100) / 100) % 1)) % 2 == 1)
                    result = ((((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100 / 100) - ((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100 / 100) % 1)) - 1) * 100);
                else
                    result = (((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100 / 100) - ((loanAmount * optTypeForTest.product.maxPercentageOfLoan / 100 / 100) % 1)) * 100);

                if (((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount))) < result)
                    result = ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
            }
            else
            {
                result = ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
            }

            return result;
        }


        //////////////////////////////////////

        public static void CalculateTheBankProfit(Option optionX, Option optionY, Option optionZ, int profile)
        {
            // get the Bank interset value
            double bankRate = RateUtilities.Instance.GetBankRate(optionX.product.productID.numberID,
                profile, (int)optionX.optTime / 12 - 4);
            optionX.SetBankRate(bankRate);
            // second period
            bankRate = RateUtilities.Instance.GetBankRateSecondPeriod(optionX.product.productID.numberID,
                profile, (int)optionX.optTime / 12 - 4);
            optionX.SetBankRate2(bankRate);
            
            bankRate = RateUtilities.Instance.GetBankRate(optionY.product.productID.numberID,
                profile, (int)optionY.optTime / 12 - 4);
            optionY.SetBankRate(bankRate);
            // second period
            bankRate = RateUtilities.Instance.GetBankRateSecondPeriod(optionY.product.productID.numberID,
                profile, (int)optionY.optTime / 12 - 4);
            optionY.SetBankRate2(bankRate);
            if (MiscUtilities.Use3ProductsInComposition())
            {
                bankRate = RateUtilities.Instance.GetBankRate(optionZ.product.productID.numberID,
                    profile, (int)optionZ.optTime / 12 - 4);
                optionZ.SetBankRate(bankRate);
                // second period
                bankRate = RateUtilities.Instance.GetBankRateSecondPeriod(optionZ.product.productID.numberID,
                   profile, (int)optionZ.optTime / 12 - 4);
                optionZ.SetBankRate2(bankRate);
            }
        }

        private static void CopyResultRepportData(double originalLoanAmount, ResultReportData calculationData,
            ResultReportData calculationBorrowerData, ResultReportData calculationBankData)
        {
            // return the borrower data
            calculationData.PayUntilToday = calculationBorrowerData.PayUntilToday;
            calculationData.PayFuture = calculationBorrowerData.PayFuture;
            calculationData.RemaingLoanAmount = calculationBorrowerData.RemaingLoanAmount;
            calculationData.RemaingLoanTime = calculationBorrowerData.RemaingLoanTime;
            calculationData.MonthlyPaymentCalc = calculationBorrowerData.MonthlyPaymentCalc;
            calculationData.FirstMonthlyPMT = calculationBorrowerData.FirstMonthlyPMT;
            calculationData.FirstMonthlyPMT2 = calculationBorrowerData.FirstMonthlyPMT2;

            // return the bank data
            calculationData.BankPayUntilToday = calculationBankData.PayUntilToday;
            calculationData.BankPayFuture = calculationBankData.PayFuture;

            // TBD - omri please fill the calculation here
            calculationData.EstimateFuturePay = calculationBorrowerData.PayFuture;

            // the bank pay - the borrower pay is the profit
            calculationData.EstimateProfitSoFar = (int) (calculationBorrowerData.PayUntilToday - calculationBankData.PayUntilToday);
            // EstimateProfitSoFar / loan amount
            if (0 < originalLoanAmount)
                calculationData.EstimateProfitPercantageSoFar = calculationData.EstimateProfitSoFar / originalLoanAmount;

            // EstimateTotalProfit = total borrower - total bank
            // check the amounts correctness
            uint totalBorowerPay = calculationBorrowerData.PayUntilToday + calculationBorrowerData.PayFuture;
            uint totalBankPay = calculationBankData.PayUntilToday + calculationBankData.PayFuture;
            if (totalBorowerPay >= totalBankPay)
            {
                calculationData.EstimateTotalProfit =
                    (int) (calculationBorrowerData.PayUntilToday + calculationBorrowerData.PayFuture -
                    calculationBankData.PayUntilToday - calculationBankData.PayFuture);
            }
            else
            {
                WindowsUtilities.loggerMethod("ERROR: CalculateLuahSilukinFullAll the borrower pay: " + totalBorowerPay +
                    " less than the bank: " + totalBankPay 
                    //+ " (originalBorrowerRate: " + originalBorrowerRate + ", originalbankRate: " + originalbankRate
                    );
            }
            // EstimateTotalProfitPercantage = EstimateTotalProfit / loan amount
            if (0 < originalLoanAmount)
                calculationData.EstimateTotalProfitPercantage = calculationData.EstimateTotalProfit / originalLoanAmount;

            // 
            calculationData.EstimateFutureProfit = (int) (calculationBorrowerData.PayFuture - calculationBankData.PayFuture);
            if (0 < originalLoanAmount)
                calculationData.EstimateFutureProfitPercantage = calculationData.EstimateFutureProfit / originalLoanAmount;
        }

        private static void CopyResultRepportData(double originalLoanAmount, ResultReportData calculationData,
            ResultReportData calculationData1, ResultReportData calculationData2,
            double finalRemaingLoanAmount, double finalFirstMonthlyPMT1, double finalFirstMonthlyPMT2, double finalRemaingLoanTime, bool isBank)
        {
            // return the borrower data
            calculationData.PayUntilToday = 
                calculationData1.PayUntilToday + calculationData2.PayUntilToday;
            calculationData.PayFuture = 
                calculationData1.PayFuture + calculationData2.PayFuture;
            calculationData.RemaingLoanAmount = (uint) finalRemaingLoanAmount;
            // calculationBorrowerData1.RemaingLoanAmount + calculationBorrowerData2.RemaingLoanAmount;
            calculationData.RemaingLoanTime = (uint) finalRemaingLoanTime;
                // calculationBorrowerData1.RemaingLoanTime + calculationBorrowerData2.RemaingLoanTime;
            calculationData.MonthlyPaymentCalc = 
                calculationData1.MonthlyPaymentCalc + calculationData2.MonthlyPaymentCalc;
            calculationData.FirstMonthlyPMT = (uint) finalFirstMonthlyPMT1;
            calculationData.FirstMonthlyPMT2 = (uint)finalFirstMonthlyPMT2;
            //    calculationBorrowerData1.FirstMonthlyPMT + calculationBorrowerData2.FirstMonthlyPMT;
            //calculationData.EstimateFuturePay = 
            //    calculationBorrowerData1.PayFuture + calculationBorrowerData2.PayFuture;
            if (isBank)
            {
                // return the bank data
                calculationData.BankPayUntilToday = 
                    calculationData1.PayUntilToday + calculationData2.PayUntilToday;
                calculationData.BankPayFuture = 
                    calculationData1.PayFuture + calculationData2.PayFuture;
            }
        }

        //private static void CopyResultRepportBankData(double originalLoanAmount, ResultReportData calculationData,
        // ResultReportData calculationBankData1, ResultReportData calculationBankData2,
        // double finalRemaingLoanAmount, double finalFirstMonthlyPMT, double finalRemaingLoanTime)
        //{
        //    // return the bank data
        //    calculationData.BankPayUntilToday = calculationData.PayUntilToday =
        //        calculationBankData1.PayUntilToday + calculationBankData2.PayUntilToday;
        //    calculationData.BankPayFuture = calculationData.PayFuture =
        //        calculationBankData1.PayFuture + calculationBankData2.PayFuture;
        //}


        /*
         * This calculation is manily for the products which have a rate dependency 
         * actually the Israel PRIME and the ARM products
         */
         // duplicate the original version for UK in order to avoid checking all the USA and Israel side
         // ugly but that's life... someone should be brave enought

            
       

    public static void CalculateLuahSilukinFullUK(
            indices indicesForPeriod,
            double originalRate, double originalInflation,
            double originalLoanAmount, uint originalLoanTime, uint currentPeriodTime,
            DateTime dateLoanTaken, ref ResultReportData calculationData,
            RunEnvironment env, bool IsBank, bool useConstantRate, out double lastRate, uint accululateTime,
            ref double finalRemaingLoanAmount, ref double finalFirstMonthlyPMT, ref double finalRemaingLoanTime)
        {
            if (MiscConstants.UNDEFINED_DOUBLE >= originalLoanAmount)
            {
                WindowsUtilities.loggerMethod("NOTICE: CalculateLuahSilukinFullUK illegal originalLoanAmount: " + originalLoanAmount);
                lastRate = MiscConstants.UNDEFINED_DOUBLE;
                return;
            }

            Interlocked.Add(ref Share.Calculation_CalculateLuahSilukinUKCounter, 1);

            double interestPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
                totalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE,
                principalPaidSoFar = MiscConstants.UNDEFINED_DOUBLE;
            double startingAmount = originalLoanAmount;
            calculationData.RemaingLoanTime = originalLoanTime - accululateTime;
            double primeMargin = MiscConstants.UNDEFINED_DOUBLE;
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            //int loopCounter = Math.Min(numOfMonths, (int)currentPeriodTime /*originalLoanTime*/);
            uint loopCounter = currentPeriodTime;
            double currentRate = originalRate, ratePmt = MiscConstants.UNDEFINED_DOUBLE, principalPmt = MiscConstants.UNDEFINED_DOUBLE;
            DateTime currentDate = dateLoanTaken;
            double redundantValue;
            string msg;
            int m;

            // is it the future period
            int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(dateLoanTaken, DateTime.Now);
            bool futurePeriod = (0 >= numOfMonths);

            // should we calculate the rate or is it constant
            double rate2calculateR, historicRate, productPlanRate = MiscConstants.UNDEFINED_DOUBLE;
 
            if (useConstantRate)
            {
                historicRate = rate2calculateR = originalRate; // (IsBank ? historicRate : originalRate);
            }
            else
            {
                historicRate = MiscUtilities.GetHistoricIndexRateForDate(indicesForPeriod, dateLoanTaken, originalRate,
                    out primeMargin, IsBank);
                historicRate += primeMargin;
                // the difference between the original rate and the historic rate define the base for the product
                // productPlanRate = historicRate - originalRate;
                rate2calculateR = (IsBank ? historicRate : originalRate);

            }
            double r = ((rate2calculateR / 12 * 100000000) - ((rate2calculateR / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double monthlyPmt = CalculateMonthlyPmt(originalLoanAmount, originalLoanTime - accululateTime, historicRate, originalInflation); ;
            calculationData.FirstMonthlyPMT = (uint)Math.Round(monthlyPmt);

            // ugly, but that's life...
            if (0 == numOfMonths)
            {
                finalRemaingLoanAmount = startingAmount;
                finalFirstMonthlyPMT = monthlyPmt;
                finalRemaingLoanTime = calculationData.RemaingLoanTime;
            }

            // calculate till now
            bool shouldLog = false; //  true;
            double monthlyPmtCalc;
            uint counter;
            for (m = 1; m <= loopCounter /*numOfMonths*/; m++)
            {
                ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                principalPmt = monthlyPmt - ratePmt;
                principalPaidSoFar += principalPmt;
                interestPaidSoFar += ratePmt;
                totalPaidSoFar += monthlyPmt;
                calculationData.RemaingLoanTime--;

                //// for debug:
                if (shouldLog)
                {
                    counter = accululateTime + (uint) m;
                    msg = counter + "," + startingAmount + "," + ratePmt + "," + principalPmt + "," + r + "," +
                      historicRate + "," + currentRate + "," + monthlyPmt + "," + totalPaidSoFar;
                    Log(msg);
                }

                if (useConstantRate)
                {
                    currentRate = rate2calculateR;
                    r = ((currentRate / 12 * 100000000) - ((currentRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                }
                else
                {
                    // calculate the exact historical rate 
                    currentDate = dateLoanTaken.AddMonths(m);
                    //historicRate = MiscUtilities.GetHistoricIndexRateForPeriod(indices, currentDate);
                    historicRate = MiscUtilities.GetHistoricIndexRateForDate(indicesForPeriod, currentDate, originalRate,
                        out redundantValue, IsBank);
                    historicRate += primeMargin;
                    currentRate = (IsBank ? historicRate : historicRate + productPlanRate);
                    r = ((currentRate / 12 * 100000000) - ((currentRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                }

                startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                monthlyPmtCalc = Math.Round(CalculateMonthlyPmt(startingAmount, (uint)(originalLoanTime - m - accululateTime),
                        currentRate, originalInflation), 2);
                if (0 < (uint)Math.Round(monthlyPmtCalc))
                {
                    monthlyPmt = monthlyPmtCalc;
                }
            }

            // no more future....

            //startingAmountFuture = startingAmount;
            //monthlyPmtFuture = monthlyPmt;

            //Log("\nAnd now for the futur\n");
            //shouldLog = true;

            //// calculate from now till the end loan' time
            //for (/*m = numOfMonths*/; m <= originalLoanTime; m++)
            //{
            //    ratePmtFuture = Math.Round((startingAmountFuture * (1 + i) * r), 2);
            //    principalPmtFuture = monthlyPmtFuture - ratePmtFuture;
            //    principalPayFuture += principalPmtFuture;
            //    optTtlRatePayFuture += ratePmtFuture;
            //    totalPaidFuture += monthlyPmtFuture;
            //    startingAmountFuture = Math.Round((((startingAmountFuture) * (1 + i)) - principalPmtFuture), 2);
            //    monthlyPmtCalc = Math.Round(
            //        CalculateMonthlyPmt(startingAmountFuture, (uint)(originalLoanTime - m),
            //        currentRate, originalInflation), 2);
            //    if (0 < (uint)Math.Round(monthlyPmtCalc))
            //    {
            //        monthlyPmtFuture = monthlyPmtCalc;
            //    }

            //    // for debug:
            //    if (shouldLog)
            //    {
            //        msg = m + "," + startingAmountFuture + "," + ratePmtFuture + "," + principalPmtFuture + "," + r + ","
            //              + historicRate + "," + currentRate + "," + monthlyPmtFuture + "," + i + "," + totalPaidFuture
            //              + "," + m + "," + originalLoanTime;
            //         Log(msg);
            //    }
            //}

            if (futurePeriod)
                calculationData.PayFuture = (uint)Math.Round(totalPaidSoFar);
            else
            {
                calculationData.PayUntilToday = (uint)Math.Round(totalPaidSoFar);
                
            }

            calculationData.MonthlyPaymentCalc = (uint)Math.Round(monthlyPmt);
            calculationData.RemaingLoanAmount = (uint)Math.Round(startingAmount);
            lastRate = currentRate;
        }

        public static void DriveCalculateLuahSilukinFullUK(indices indicesFirstTimePeriod, indices indicesSecondTimePeriod,
            double originalBorrowerRate, double originalBorrowerRate2,
            double originalbankRate, double originalbankRate2,
            double originalInflation, int firstTimePeriod,
            double originalLoanAmount, uint originalLoanTime,
            DateTime dateLoanTaken, string loanID, ref ResultReportData calculationData,
            RunEnvironment env = null)
        {
            ResultReportData calculationBorrowerData = new ResultReportData();
            ResultReportData calculationBankData = new ResultReportData();
            ResultReportData calculationBorrowerData1 = new ResultReportData(),
                calculationBorrowerData2 = new ResultReportData(),
                calculationBorrowerData3 = new ResultReportData();
            ResultReportData calculationBankData1 = new ResultReportData(),
                calculationBankData2 = new ResultReportData(),
                calculationBankData3 = new ResultReportData();
            bool needToAccumulateResults = true;
            double finalRemaingLoanAmount, finalRemaingLoanTime, finalFirstMonthlyPMT;
            double finalFirstMonthlyPMT1 = MiscConstants.UNDEFINED_DOUBLE, finalFirstMonthlyPMT2 = MiscConstants.UNDEFINED_DOUBLE;
            finalRemaingLoanAmount = finalFirstMonthlyPMT = finalRemaingLoanTime = MiscConstants.UNDEFINED_DOUBLE;
            double finalBankRemaingLoanAmount, finalBankFirstMonthlyPMT, finalBankRemaingLoanTime;
            finalBankRemaingLoanAmount = finalBankFirstMonthlyPMT = finalBankRemaingLoanTime = MiscConstants.UNDEFINED_DOUBLE;

            // how much time is the first time and the second time
            int numOfMonthsUntilToday = MiscUtilities.CalculateMonthBetweenDates(dateLoanTaken, DateTime.Now);
            uint newLoanTime, accululateTime = 0;

 
            /*
            Case 1:
               if numOfMonthsUntilToday > firstTimePeriod
               * Luah silukin is divided into 3 parts:
                   1. From date taken -> first time period -> interest rate is constant -> taken from input file -> original rate first period

                   2. From first time period -> today -> borrower interest rate is changing each month -> taken from historic DB file
                       * Calculating the final interest rate to be used
                           * First month -> Use original rate second period to determine the margin from the historic rate and the actual rate.
                               Example: historic rate is 5.00% and original rate second period is 5.39% -> margin is +0.39% -> this is for the borrower
                                   The actual borrower rate for the first iteration of the loop = original rate second period
                                   To calculate bank rate -> use the actual borrower rate minus the margin from the margin file
                           * Second month and on -> Use historic DB rate and add to that the margin calculated in the first month
                               Example: historic rate is 5.50% and margin is +0.39% -> actual borrower rate = 5.89% -> this is for the borrower
                                   To calculate bank rate -> use the actual borrower rate minus the margin from the margin file

                   3. From today until the end -> borrower interest rate is constant -> uses the last value of the last loop and doesnt change
            */
            double lastRate = MiscConstants.UNDEFINED_DOUBLE;
            double newLoanAmount = MiscConstants.UNDEFINED_DOUBLE;
            DateTime newLoanDate2;

            if (numOfMonthsUntilToday > firstTimePeriod)
            {
                newLoanAmount = originalLoanAmount;
                newLoanTime = (uint)firstTimePeriod;
                // 1.From date taken -> first time period->interest rate is constant -> taken from input file -> original rate first period
                Log("\nCalculate Luah Silukin for Borrower first step for numOfMonthsUntilToday > firstTimePeriod:\n");
                Log("\nindicesForPeriod: " + indicesFirstTimePeriod + ", originalRate: " + originalBorrowerRate +
                    ", originalInflation: " + originalInflation +
                    ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                    ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + dateLoanTaken +
                    ", accululateTime: " + accululateTime + ", IsBank: " + false + ",  useConstantRate: " + true +  "\n");

                // the borrower side
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalBorrowerRate, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    dateLoanTaken, ref calculationBorrowerData1, env, false /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);

                finalFirstMonthlyPMT1 = calculationBorrowerData1.FirstMonthlyPMT;
                accululateTime += newLoanTime;

                // 2. From first time period -> today -> borrower interest rate is changing each month -> taken from historic DB file
                Log("\nCalculate Luah Silukin for Borrower second step for numOfMonthsUntilToday > firstTimePeriod:\n");
                newLoanAmount = calculationBorrowerData1.RemaingLoanAmount;
                if (firstTimePeriod > numOfMonthsUntilToday)
                {
                    WindowsUtilities.loggerMethod("NOTICE: DriveCalculateLuahSilukinFullUK firstTimePeriod : " + firstTimePeriod + " is greater than numOfMonthsUntilToday: " + numOfMonthsUntilToday);
                }
                else
                {
                    newLoanTime = (uint)(numOfMonthsUntilToday - firstTimePeriod);
                    newLoanDate2 = dateLoanTaken.AddMonths(firstTimePeriod);
                    Log("\nindicesForPeriod: " + indicesSecondTimePeriod + ", originalRate: " + originalBorrowerRate2 +
                         ", originalInflation: " + originalInflation +
                         ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                         ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + newLoanDate2 +
                         ", accululateTime: " + accululateTime + ", IsBank: " + false + ",  useConstantRate: " + false + "\n");
                    CalculateLuahSilukinFullUK(indicesSecondTimePeriod, originalBorrowerRate2, originalInflation, newLoanAmount,
                        originalLoanTime, newLoanTime,
                        newLoanDate2, ref calculationBorrowerData2, env, false /*IsBank*/, false /*useConstantRate*/,
                        out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);
                    finalFirstMonthlyPMT2 = calculationBorrowerData2.FirstMonthlyPMT;
                    accululateTime += newLoanTime;
                }

                // 3. From today until the end -> borrower interest rate is constant -> uses the last value of the last loop and doesnt change
                Log("\nCalculate Luah Silukin for Borrower third step for numOfMonthsUntilToday > firstTimePeriod:\n");
                newLoanAmount = calculationBorrowerData2.RemaingLoanAmount;
                if (numOfMonthsUntilToday > originalLoanTime)
                {
                    WindowsUtilities.loggerMethod("NOTICE: DriveCalculateLuahSilukinFullUK numOfMonthsUntilToday : " + numOfMonthsUntilToday + " is greater than originalLoanTime: " + originalLoanTime);
                }
                else
                {
                    newLoanTime = (uint)(originalLoanTime - numOfMonthsUntilToday);
                    newLoanDate2 = DateTime.Now;
                    Log("\nindicesForPeriod: " + indicesSecondTimePeriod + ", originalRate: " + lastRate +
                        ", originalInflation: " + originalInflation +
                        ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                        ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + newLoanDate2 +
                        ", accululateTime: " + accululateTime + ", IsBank: " + false + ",  useConstantRate: " + true + "\n");
                    CalculateLuahSilukinFullUK(indicesSecondTimePeriod, lastRate, originalInflation, newLoanAmount,
                        originalLoanTime, newLoanTime,
                        newLoanDate2, ref calculationBorrowerData3, env, false /*IsBank*/, true /*useConstantRate*/,
                        out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);
                }

                // the bank side
                newLoanAmount = originalLoanAmount;
                newLoanTime = (uint)firstTimePeriod;
                accululateTime = 0;
                Log("\nCalculate Luah Silukin for Bank first step for numOfMonthsUntilToday > firstTimePeriod:\n");
                Log("\nindicesForPeriod: " + indicesFirstTimePeriod + ", originalRate: " + originalbankRate +
                   ", originalInflation: " + originalInflation +
                   ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                   ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + dateLoanTaken +
                   ", accululateTime: " + accululateTime + ", IsBank: " + true + ",  useConstantRate: " + true + "\n");
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalbankRate, originalInflation, newLoanAmount,
                     originalLoanTime, newLoanTime,
                     dateLoanTaken, ref calculationBankData1, env, true /*IsBank*/, true /*useConstantRate*/, 
                     out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);
                accululateTime += newLoanTime;

                Log("\nCalculate Luah Silukin for Bank second step for numOfMonthsUntilToday > firstTimePeriod:\n");
                newLoanAmount = calculationBankData1.RemaingLoanAmount;
                if (firstTimePeriod > numOfMonthsUntilToday)
                {
                    WindowsUtilities.loggerMethod("NOTICE: DriveCalculateLuahSilukinFullUK Bank firstTimePeriod : " + firstTimePeriod + " is greater than numOfMonthsUntilToday: " + numOfMonthsUntilToday);
                }
                else
                {
                    newLoanTime = (uint)(numOfMonthsUntilToday - firstTimePeriod);
                    newLoanDate2 = dateLoanTaken.AddMonths(firstTimePeriod);
                    Log("\nindicesForPeriod: " + indicesSecondTimePeriod + ", originalRate: " + originalbankRate2 +
                        ", originalInflation: " + originalInflation +
                        ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                        ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + newLoanDate2 +
                        ", accululateTime: " + accululateTime + ", IsBank: " + true + ",  useConstantRate: " + false + "\n");
                    CalculateLuahSilukinFullUK(indicesSecondTimePeriod, originalbankRate2, originalInflation, newLoanAmount,
                        originalLoanTime, newLoanTime,
                        newLoanDate2, ref calculationBankData2, env, true /*IsBank*/, false /*useConstantRate*/,
                        out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);
                    accululateTime += newLoanTime;
                }

                // 3. From today until the end -> borrower interest rate is constant -> uses the last value of the last loop and doesnt change
                Log("\nCalculate Luah Silukin for Bank third step for numOfMonthsUntilToday > firstTimePeriod:\n");
                newLoanAmount = calculationBankData2.RemaingLoanAmount;
                if (numOfMonthsUntilToday > originalLoanTime)
                {
                    WindowsUtilities.loggerMethod("NOTICE: DriveCalculateLuahSilukinFullUK numOfMonthsUntilToday : " + numOfMonthsUntilToday + " is greater than originalLoanTime: " + originalLoanTime);
                }
                else
                {
                    newLoanTime = (uint)(originalLoanTime - numOfMonthsUntilToday);
                    newLoanDate2 = DateTime.Now;
                    Log("\nindicesForPeriod: " + indicesSecondTimePeriod + ", originalRate: " + lastRate +
                         ", originalInflation: " + originalInflation +
                         ", originalLoanAmount: " + newLoanAmount + ", originalLoanTime: " + originalLoanTime +
                         ", currentPeriodTime: " + newLoanTime + ", dateLoanTaken: " + newLoanDate2 +
                         ", accululateTime: " + accululateTime + ", IsBank: " + true + ",  useConstantRate: " + true + "\n");
                    CalculateLuahSilukinFullUK(indicesSecondTimePeriod, lastRate, originalInflation, newLoanAmount,
                        originalLoanTime, newLoanTime,
                        newLoanDate2, ref calculationBankData3, env, true /*IsBank*/, true /*useConstantRate*/,
                        out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);
                }

            } // (numOfMonthsUntilToday > firstTimePeriod)
            else if (firstTimePeriod > numOfMonthsUntilToday)
            {
                /*
                Case 2:
                if firstTimePeriod > numOfMonthsUntilToday
	            * Luah silukin is divided into 3 parts
		            1. From date taken -> today -> borrower interest rate is constant -> taken from input file -> original rate first period
		            2. From today -> first time period-> borrower interest rate is constant -> taken from input file -> original rate first period
			            * Add -> calculating the final interest rate to be used
		            3. From first time period until the end -> borrower interest rate is constant -> taken from input file -> original rate second period
                */
                newLoanAmount = originalLoanAmount;
                accululateTime = 0;
                newLoanTime = (uint)numOfMonthsUntilToday;
                // 	1. From date taken -> today -> borrower interest rate is constant -> taken from input file -> original rate first period
                Log("\nCalculate Luah Silukin for Borrower first step for firstTimePeriod > numOfMonthsUntilToday:\n");
                // the borrower side
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalBorrowerRate, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    dateLoanTaken, ref calculationBorrowerData1, env, false /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);
                accululateTime += newLoanTime;

                // 2. From today -> first time period-> borrower interest rate is constant -> taken from input file -> original rate first period
                Log("\nCalculate Luah Silukin for Borrower second step for firstTimePeriod > numOfMonthsUntilToday:\n");
                newLoanAmount = calculationBorrowerData1.RemaingLoanAmount;
                newLoanTime = (uint)(firstTimePeriod - numOfMonthsUntilToday);
                newLoanDate2 = DateTime.Now;
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalBorrowerRate, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    newLoanDate2, ref calculationBorrowerData2, env, false /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);
                finalFirstMonthlyPMT1 = calculationBorrowerData1.FirstMonthlyPMT; // TBD - omri. is it the first or second???
                accululateTime += newLoanTime;

                // 3. From first time period until the end -> borrower interest rate is constant -> taken from input file -> original rate second period
                Log("\nCalculate Luah Silukin for Borrower third step for firstTimePeriod > numOfMonthsUntilToday:\n");
                newLoanAmount = calculationBorrowerData2.RemaingLoanAmount;
                newLoanTime = (uint)(originalLoanTime - firstTimePeriod);
                newLoanDate2 = dateLoanTaken.AddMonths(firstTimePeriod);
                CalculateLuahSilukinFullUK(indicesSecondTimePeriod, originalBorrowerRate2 /*lastRate*/, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    newLoanDate2, ref calculationBorrowerData3, env, false /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);
                finalFirstMonthlyPMT2 = calculationBorrowerData3.FirstMonthlyPMT;

                // the bank side
                newLoanAmount = originalLoanAmount;
                accululateTime = 0;
                newLoanTime = (uint)numOfMonthsUntilToday;
                // 	1. From date taken -> today -> borrower interest rate is constant -> taken from input file -> original rate first period
                Log("\nCalculate Luah Silukin for Bank first step for firstTimePeriod > numOfMonthsUntilToday:\n");
                // the bank side
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalbankRate, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    dateLoanTaken, ref calculationBankData1, env, true /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);
                accululateTime += newLoanTime;

                // 2. From today -> first time period-> borrower interest rate is constant -> taken from input file -> original rate first period
                Log("\nCalculate Luah Silukin for Bank second step for firstTimePeriod > numOfMonthsUntilToday:\n");
                newLoanAmount = calculationBankData1.RemaingLoanAmount;
                newLoanTime = (uint)(firstTimePeriod - numOfMonthsUntilToday);
                newLoanDate2 = DateTime.Now;
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalbankRate, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    newLoanDate2, ref calculationBankData2, env, true /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);
                accululateTime += newLoanTime;

                // 3. From first time period until the end -> borrower interest rate is constant -> taken from input file -> original rate second period
                Log("\nCalculate Luah Silukin for Bank third step for firstTimePeriod > numOfMonthsUntilToday:\n");
                newLoanAmount = calculationBankData2.RemaingLoanAmount;
                newLoanTime = (uint)(originalLoanTime - firstTimePeriod);
                newLoanDate2 = dateLoanTaken.AddMonths(firstTimePeriod);
                CalculateLuahSilukinFullUK(indicesSecondTimePeriod, originalbankRate2 /*lastRate*/, originalInflation, newLoanAmount,
                    originalLoanTime, newLoanTime,
                    newLoanDate2, ref calculationBankData3, env, true /*IsBank*/, true /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);

            }
            else if (0 == numOfMonthsUntilToday)
            /*
             Case 3:
                if numOfMonthsUntilToday = 0
	                * luah silukin is only new loans using our algorithms -> finding structured loans only
	                * Borrower rates are taken from the rate excel file
	                * Bank rates are calculated using the borrower rate minus the margin from the margin excel file
             */
            {
                newLoanTime = (uint)originalLoanTime;
                accululateTime = 0;
                Log("\nCalculate Luah Silukin for Borrower final step for 0 == numOfMonthsUntilToday:\n");
                // the borrower side
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalBorrowerRate, originalInflation, originalLoanAmount,
                    originalLoanTime, newLoanTime,
                    dateLoanTaken, ref calculationBorrowerData, env, false /*IsBank*/, false /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalRemaingLoanAmount, ref finalFirstMonthlyPMT, ref finalRemaingLoanTime);

                Log("\nCalculate Luah Silukin for Bank final step for 0 == numOfMonthsUntilToday:\n");
                // the bank side
                CalculateLuahSilukinFullUK(indicesFirstTimePeriod, originalbankRate, originalInflation, originalLoanAmount,
                    originalLoanTime, newLoanTime,
                    dateLoanTaken, ref calculationBankData, env, true /*IsBank*/, false /*useConstantRate*/, 
                    out lastRate, accululateTime, ref finalBankRemaingLoanAmount, ref finalBankFirstMonthlyPMT, ref finalBankRemaingLoanTime);

                needToAccumulateResults = false;
            }
            else
            {
                // what is this case?
                Console.WriteLine("ERROR DriveCalculateLuahSilukinFullUK unknown if statment. Checj yourself.");
                needToAccumulateResults = false;
            }

            if (needToAccumulateResults) {

                // print all the returned data
                MiscUtilities.PrintResultReportData("calculationBorrowerData1", calculationBorrowerData1);
                MiscUtilities.PrintResultReportData("calculationBorrowerData2", calculationBorrowerData2);
                MiscUtilities.PrintResultReportData("calculationBorrowerData3", calculationBorrowerData3);

                MiscUtilities.PrintResultReportData("calculationBankData1", calculationBankData1);
                MiscUtilities.PrintResultReportData("calculationBankData2", calculationBankData2);
                MiscUtilities.PrintResultReportData("calculationBankData3", calculationBankData3);
  
                // accululate the borrower results
                // now we have 3 result records
                CopyResultRepportData(originalLoanAmount, calculationBorrowerData, calculationBorrowerData1, 
                    calculationBorrowerData2, finalRemaingLoanAmount, finalFirstMonthlyPMT1, finalFirstMonthlyPMT2, finalRemaingLoanTime, false /*isBanke*/);
                CopyResultRepportData(originalLoanAmount, calculationBorrowerData, calculationBorrowerData, 
                    calculationBorrowerData3, finalRemaingLoanAmount, finalFirstMonthlyPMT1, finalFirstMonthlyPMT2, finalRemaingLoanTime, false /*isBanke*/);

                CopyResultRepportData(originalLoanAmount, calculationBankData, calculationBankData1, calculationBankData2, 
                    finalBankRemaingLoanAmount, finalBankFirstMonthlyPMT, finalBankFirstMonthlyPMT, finalBankRemaingLoanTime, true /*isBanke*/);
                CopyResultRepportData(originalLoanAmount, calculationBankData, calculationBankData, calculationBankData3, 
                    finalBankRemaingLoanAmount, finalBankFirstMonthlyPMT, finalBankFirstMonthlyPMT, finalBankRemaingLoanTime, true /*isBanke*/);

                MiscUtilities.PrintResultReportData("calculationBorrowerData total", calculationBorrowerData);
                MiscUtilities.PrintResultReportData("calculationBankData total", calculationBankData);

                CopyResultRepportData(originalLoanAmount, calculationData, calculationBorrowerData, calculationBankData);
                MiscUtilities.PrintResultReportData("calculationData total", calculationData);
            }
        }


    }

    /// <summary>
    /// 
    /// </summary>
    public class MinMax
    {
        public int min { get; set; }
        public int max { get; set; }

        public MinMax()
        {
            min = MiscConstants.UNDEFINED_INT;
            max = MiscConstants.UNDEFINED_INT;
        }
    }
}
