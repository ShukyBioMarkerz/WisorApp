using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.Logic
{
    class Calculations
    {
        public static double CalculateLuahSilukin2(double rateFirstPeriod, double rateSecondPeriod, 
            int productFirstTimePeriod, /*indices*/ double productIndexUsedFirstTimePeriod,
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
                    double bankPmt = CalculatePmt2(optAmt, optTime, rateFirstPeriod, optType, productFirstTimePeriod,
                         rateFirstPeriod, rateSecondPeriod, indexFirstPeriod, indexSecondPeriod, printOrNo);
                    optTtlPrincipalPay = optAmt;
                    ttlPay = optTime * Math.Round(bankPmt, 2);
                    optTtlRatePay = ttlPay - optTtlPrincipalPay;
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
                            optType, productFirstTimePeriod,
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
                            optType, productFirstTimePeriod,
                            rateFirstPeriod, rateSecondPeriod,
                            indexFirstPeriod, indexSecondPeriod, printOrNo), 
                            2);
                    }
                }
            }

            return ttlPay;
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
                else if (currInterest > 0)
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
                else
                {
                    Console.WriteLine("ERROR: CalculatePmt2 Rate Out of Range. currInterest: " + currInterest);
                }

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
                Console.WriteLine("NOTICE: CalculateMonthlyPmt got illegal originalRate: " + originalRate);
                return 0;
                //throw new System.ArgumentOutOfRangeException("Rate Out of Range");
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

        private static double CalculateLuahSilukinSoFar2(indices indices,
            double originalRate, double originalInflation,
            double originalLoanAmount, uint originalLoanTime,
            uint monthOfDateLoanTaken, uint yearOfDateLoanTaken,
            double interestPaidSoFar, double totalPaidSoFar,
            double firstMonthlyPMT, double principalPaidSoFar, out uint remaingLoanTime)
        {
            double historicRate = MiscUtilities.GetHistoricIndexRateForOption(indices, yearOfDateLoanTaken, monthOfDateLoanTaken);

            double r = ((originalRate / 12 * 100000000) - ((originalRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
            double i = ((originalInflation / 12 * 100000000) - ((originalInflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

            double monthlyPmt = Math.Round(firstMonthlyPMT, 2);
            double startingAmount = originalLoanAmount;

            int numOfMonths = CalculateTimeLoops(monthOfDateLoanTaken, yearOfDateLoanTaken);
  
            remaingLoanTime = originalLoanTime;

            for (int m = 0; m < numOfMonths; m++)
            {
                remaingLoanTime--;
                double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                double principalPmt = monthlyPmt - ratePmt;
                principalPaidSoFar += principalPmt;
                interestPaidSoFar += ratePmt;
                totalPaidSoFar += monthlyPmt;
                startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);

                monthlyPmt = Math.Round(CalculateMonthlyPmt(startingAmount, (uint)(originalLoanTime - m+1),
                /*originalRate,*/ originalRate, originalInflation), 2);
            }

            return startingAmount;
        }


        private static int CalculateTimeLoops(uint Month, uint Year)
        {
            int months = ((DateTime.Now.Year * 12) + DateTime.Now.Month) - (((int)Year * 12) + (int)Month);
            return months;
        }




        // The maximun and minimum amounts is calculated now by the Risk and Liquidity scores of the user
        public static MinMax FindMinMaxAmount(double loanAmtWanted, OneOptType optType, Risk risk, 
            Liquidity liquidity, double prevMin)
        {
            RiskLiquidityValue riskLiquidityValue = new RiskLiquidityValue();
            riskLiquidityValue.liquidity = liquidity; 
            riskLiquidityValue.risk = risk; 
            riskLiquidityValue.productID = optType.product.productID;
            bool rc = false;
            MinMax minmax = new MinMax();

            if (Risk.NONERisk == risk || Liquidity.NONELiquidity == liquidity)
            {
                // keep the old good stuff...
                minmax.max = (int) (Calculations.FindMaxAmount(loanAmtWanted, optType) / 100 * 100);
                minmax.min = (int) prevMin; // ... don't touch
                //Console.WriteLine("FindMinMaxAmount no risk or liquidity for product: " + optType.product.productID.ToString());
            }
            else
            {
                rc = MiscUtilities.FindRiskLiquidity(riskLiquidityValue);
                if (rc)
                {
                    // should calculate the loan values according to the percantage
                    minmax.min = (int)(loanAmtWanted * riskLiquidityValue.min / 100 * 100);
                    minmax.max = (int)(loanAmtWanted * riskLiquidityValue.max / 100 * 100);
                }
                else
                    Console.WriteLine("TestRiskLiquidity failed for: " + riskLiquidityValue.ToString());
            }
 
            return minmax;
        }

        private static double FindMaxAmount(double loanAmountTemp, OneOptType optType)
        {
            double loanAmount = loanAmountTemp;
            OneOptType optTypeForTest = optType;
            int numOfFixedOptions = 2;
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
            double bankRate = RateUtilities.GetBankRate(optionX.product.productID.numberID,
                profile, (int)optionX.optTime / 12 - 4);
            optionX.SetBankRate(bankRate);
            bankRate = RateUtilities.GetBankRate(optionY.product.productID.numberID,
                profile, (int)optionY.optTime / 12 - 4);
            optionY.SetBankRate(bankRate);
            bankRate = RateUtilities.GetBankRate(optionZ.product.productID.numberID,
                profile, (int)optionZ.optTime / 12 - 4);
            optionZ.SetBankRate(bankRate);
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
