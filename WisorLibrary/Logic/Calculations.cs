using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using static WisorLib.MiscConstants;

namespace WisorLibrary.Logic
{
    class Calculations
    {
        public static double CalculateLuahSilukin2(double rateFirstPeriod, double rateSecondPeriod, 
            int productFirstTimePeriod, indices productIndexUsedFirstTimePeriod,
            double optAmt, int optTime, double optPmt, 
            double indexFirstPeriod, double indexSecondPeriod, int optType, bool printOrNo
            //,out double ttlPay
            )
        {
            double optTtlPrincipalPay = MiscConstants.UNDEFINED_DOUBLE,
                    optTtlRatePay = MiscConstants.UNDEFINED_DOUBLE;

            //Console.WriteLine("CalculateLuahSilukin product.firstTimePeriod: " + product.firstTimePeriod);
            double ttlPay = MiscConstants.UNDEFINED_DOUBLE;

            if (productFirstTimePeriod == 0)
            {
                if (productIndexUsedFirstTimePeriod == 0)
                {
                    optTtlPrincipalPay = optAmt;
                    ttlPay = optTime * Math.Round(optPmt, 2);
                    optTtlRatePay = ttlPay - optTtlPrincipalPay;
                }
                else
                {
                    double r = ((rateFirstPeriod / 12 * 100000000) - ((rateFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                    double monthlyPmt = Math.Round(optPmt, 2);
                    double startingAmount = optAmt;

                    for (int months = 1; months <= optTime; months++)
                    {
                        double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                        double principalPmt = monthlyPmt - ratePmt;
                        optTtlPrincipalPay += principalPmt;
                        optTtlRatePay += ratePmt;
                        ttlPay += monthlyPmt;
                        startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                        if (months < optTime)
                        {
                            monthlyPmt = Math.Round(
                                CalculatePmt2(startingAmount, (optTime - months), rateFirstPeriod,
                                optType, optAmt, productFirstTimePeriod,
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

                double monthlyPmt = Math.Round(optPmt, 2);
                double startingAmount = optAmt;
                rateSecondPeriod = rateFirstPeriod;
                // TBD Omri
                //optRateSecondPeriod = CalculateInterestRate4SecondPeriod()

                for (int months = 1; months <= productFirstTimePeriod; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    ttlPay += monthlyPmt;
                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(
                            CalculatePmt2(startingAmount, (optTime - months), rateFirstPeriod,
                            optType, optAmt, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo),
                            2);
                    }
                }
                r = ((rateSecondPeriod / 12 * 100000000) - ((rateSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                i = ((indexSecondPeriod / 12 * 100000000) - ((indexSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                for (int months = productFirstTimePeriod + 1; months <= optTime; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    ttlPay += monthlyPmt;
                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(
                            CalculatePmt2(startingAmount, (optTime - months), rateSecondPeriod,
                            optType, optAmt, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo), 
                            2);
                    }
                }
            }

            return ttlPay;
        }
        

        public static double CalculatePmt2(double amtForCalc, int timeForCalc, double interestRateForCalc,
            int optType, double optAmt, int productFirstTimePeriod, 
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
                else if (currInterest > 0)
                {
                    double r = ((currInterest / 12 * 100000000) - ((currInterest / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double calcPow = Math.Pow((1 + r), timeForCalc);
                    double monthlyPmt = ((amtForCalc * (1 + i)) * (r * calcPow) / (calcPow - 1));
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                    if (printOrNo == true)
                    {
                        Console.WriteLine("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
                    }
                    return monthlyPmt;
                }
                else
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Rate Out of Range");
                }

            }
            else
            {
                if (MiscConstants.UNDEFINED_INT == optType)
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Option Type Out of Range");
                }
                else if (optAmt <= CalculationConstants.optionMinimumAmount)
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
    
        public static double CalculateRemainingAmount(double originalLoanAmount, uint originalLoanTime, /*int optionType,*/
             uint monthOfDateLoanTaken, uint yearOfDateLoanTaken, double originalRate,
             double originalInflation, double interestPaidSoFar, double totalPaidSoFar,
             double principalPaidSoFar, out uint remaingLoanTime)
         {

            //Option optionForCheck = new Option(optionType, originalLoanAmount, originalLoanTime);
            //optionForCheck.optRateForRemainingAmount = originalRate;

            double firstMonthlyPMT = CalculateMonthlyPmt(originalLoanAmount, originalLoanTime,
                /*optionForCheck.optRateForRemainingAmount,*/ originalRate, originalInflation);
 
            double remeaingAmount = CalculateLuahSilukinSoFar(
                originalRate, originalInflation,
                originalLoanAmount, originalLoanTime,
                monthOfDateLoanTaken, yearOfDateLoanTaken,
                interestPaidSoFar, totalPaidSoFar,
                firstMonthlyPMT, principalPaidSoFar, out remaingLoanTime);
            return remeaingAmount;
        }
        
        // **************************************************************************************************************************** //
        // **************************************** Getting Inflation According to Option Type **************************************** //

        private static string PresentPastOrFuture(uint mm, uint yy)
        {
            string resultToReturn = "";

            if (yy < DateTime.Today.Year)
            {
                resultToReturn = "P";
            }
            else if (yy == DateTime.Today.Year)
            {
                if (mm < DateTime.Today.Month)
                {
                    resultToReturn = "P";
                }
                else if (mm == DateTime.Today.Month)
                {
                    resultToReturn = "N";
                }
                else
                {
                    resultToReturn = "F";
                }
            }
            else
            {
                resultToReturn = "F";
            }
            return resultToReturn;
        }
        
        // **************************************************************************************************************************** //
        // ******************************* Calculating PMT According to Option Type and Time and Rate ********************************* //

        public static double CalculateMonthlyPmt(double amtForCalc, uint timeForCalc, /*double interestRateForCalc,*/
            double originalRate, double originalInflation)
        {
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

            if (originalRate == 0)
            {
                double monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                return monthlyPmt;
            }
            else if (originalRate > 0)
            {
                double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double calcPow = Math.Pow((1 + r), timeForCalc);
                double monthlyPmt = ((amtForCalc * (1 + i)) * (r * calcPow) / (calcPow - 1));
                monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                return monthlyPmt;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Rate Out of Range");
            }
        }
        

        // **************************************************************************************************************************** //
        // ********************************** PRIVATE - Calculating full luah silukin for option ************************************** //

        private static double CalculateLuahSilukinSoFar(
            double originalRate, double originalInflation,
            double originalLoanAmount, uint originalLoanTime,
            uint monthOfDateLoanTaken, uint yearOfDateLoanTaken,
            double interestPaidSoFar, double totalPaidSoFar,
            double firstMonthlyPMT, double principalPaidSoFar, out uint remaingLoanTime)
        {

            double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

            double monthlyPmt = Math.Round(firstMonthlyPMT, 2);
            double startingAmount = originalLoanAmount;

            string timeString = PresentPastOrFuture(monthOfDateLoanTaken, yearOfDateLoanTaken);
            uint timeCounter = 1;
            uint monthCounter = monthOfDateLoanTaken;
            uint yearCounter = yearOfDateLoanTaken;
            
            remaingLoanTime = originalLoanTime;

            while (timeString != "F")
            {
                remaingLoanTime--;
                double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                double principalPmt = monthlyPmt - ratePmt;
                principalPaidSoFar += principalPmt;
                interestPaidSoFar += ratePmt;
                totalPaidSoFar += monthlyPmt;
                startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                
                if (timeCounter < originalLoanTime)
                {
                    monthlyPmt = Math.Round(CalculateMonthlyPmt(startingAmount, (originalLoanTime - timeCounter),
                        /*originalRate,*/ originalRate, originalInflation), 2);
                    timeCounter++;
                }

                if (monthCounter == 12)
                {
                    monthCounter = 1;
                    yearCounter++;
                }
                else
                {
                    monthCounter++;
                }
                timeString = PresentPastOrFuture(monthCounter, yearCounter);
             }

            return startingAmount;

        }

    }
}
