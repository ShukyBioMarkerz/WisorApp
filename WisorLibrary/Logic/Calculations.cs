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



    }
}
