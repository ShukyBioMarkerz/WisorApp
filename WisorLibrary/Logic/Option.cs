using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class Option
    {

        // General Parameters
        public uint optType = 0;
        public double optAmt = 0;
        public uint optTime = 0;
        public double inflation = -1;
        public double optPmt = -1;

        private double[] optRateList = new double[360];
        public double optTtlPay = 0;
        public double optTtlRatePay = 0;
        public double optTtlPrincipalPay = 0;
        private bool printOrNo = false;

        private string inflationStr = "";

        private double optRate = -1;
        private double optRateFirstPeriod = -1;
        private double optRateSecondPeriod = -1;


        public Option(uint optionType, double optionAmount, uint optionTime)
        {
            optType = optionType;
            optAmt = optionAmount;
            optTime = optionTime;
            inflation = GetInflationForOption();
            optRate = FindInterestRate();
            optPmt = CalculatePmt(optAmt, optTime, optRate);
        }







        private void InsertRatesInListForCalculation()
        {
            if ((optType > (int)Options.typesList.EMPTY) &&
                    (optAmt >= CalculationConstants.optionMinimumAmount) &&
                        (optTime >= CalculationConstants.minimumTimeForLoan))                                                      
            {
                for (int i = 0; i < optRateList.Length; i++ )
                {
                    optRateList[i] = optRate;
                }
            }
            else
            {
                if (optType == (int)Options.typesList.EMPTY)
                {
                    throw new System.ArgumentOutOfRangeException("Option Type Out of Range");
                }
                else if (optAmt <= CalculationConstants.optionMinimumAmount)
                {
                    throw new System.ArgumentOutOfRangeException("Option Amount Out of Range");
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
                }
            }

        }





        // **************************************************************************************************************************** //
        // **************************************** Getting Inflation According to Option Type **************************************** //

        private double GetInflationForOption()
        {
            if (optType > (int)Options.typesList.EMPTY)
            {
                switch (optType)
                {
                    case ((int)Options.typesList.PRIME):
                        {
                            inflationStr = "No";
                            return 0;
                        }
                    case ((int)Options.typesList.FIXNOTSAMUD):
                        {
                            inflationStr = "No";
                            return 0;
                        }
                    case ((int)Options.typesList.ALT60NOTSAMUD):
                        {
                            inflationStr = "No";
                            return 0;
                        }
                    case ((int)Options.typesList.FIXTSAMUD):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT12):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT24):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT30):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT60):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT84):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    case ((int)Options.typesList.ALT120):
                        {
                            inflationStr = "Yes";
                            return CalculationConstants.inflation;
                        }
                    default: return -1;
                }
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Option Type Out of Range");
            }
        }


        // **************************************************************************************************************************** //
        // ***************************** Calculating Interest Rate According to Option Type and Option Time *************************** //

        private double FindInterestRate()
        {
            if ((optType > (int)Options.typesList.EMPTY) && (optTime >= CalculationConstants.minimumTimeForLoan))
            {
                switch (optType)
                {
                    case ((int)Options.typesList.PRIME):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.PRIME];
                    case ((int)Options.typesList.FIXNOTSAMUD):
                                                    return Rates.fixedNoTsamudRates[(optTime / 12) - 4];
                    case ((int)Options.typesList.ALT60NOTSAMUD):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH60NOINF];
                    case ((int)Options.typesList.FIXTSAMUD):
                                                    return Rates.fixedTsamudRates[(optTime / 12) - 4];
                    case ((int)Options.typesList.ALT12):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH12];
                    case ((int)Options.typesList.ALT24):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH24];
                    case ((int)Options.typesList.ALT30):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH30];
                    case ((int)Options.typesList.ALT60):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH60YESINF];
                    case ((int)Options.typesList.ALT84):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH84];
                    case ((int)Options.typesList.ALT120):
                                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH120];
                    default: return -1;
                }
            }
            else
            {
                if (optType == (int)Options.typesList.EMPTY)
                {
                    throw new System.ArgumentOutOfRangeException("Option Type Out of Range");

                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
                }
            }
        }





        // **************************************************************************************************************************** //
        // *************************************************** Show Interest Rate ***************************************************** //

        public double ShowRate()
        {
            if ((optTime >= CalculationConstants.minimumTimeForLoan) && (optType > (int)Options.typesList.EMPTY))
            {
                if (optRate == -1)
                {
                    optRate = FindInterestRate();
                    return optRate;
                }
                else if (optRate >= 0)
                {
                    return optRate;
                }              
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Rate Out of Range");
                }
            }
            else
            {
                if (optType == (int)Options.typesList.EMPTY)
                {
                    throw new System.ArgumentOutOfRangeException("Option Type Out of Range");
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
                }
            }
        }





        // **************************************************************************************************************************** //
        // ******************************* Calculating PMT According to Option Type and Time and Rate ********************************* //

        public double CalculatePmt(double amtForCalc, uint timeForCalc, double interestRateForCalc)
        {
            if ((optType > (int)Options.typesList.EMPTY) && (amtForCalc > 0) && (timeForCalc > 0))                                          
            {
                if ((amtForCalc == optAmt) && (timeForCalc == optTime))
                {
                    if (optRate == -1)
                    {
                        optRate = FindInterestRate();
                    }
                }
                if (printOrNo == true)
                {
                    WindowsUtilities.GetLogger()("Option:\nType = " + Options.optionTypes[optType] + "\nAmount = " + amtForCalc
                                            + "\nTime in months = " + timeForCalc + "\nYearly Rate = " + interestRateForCalc
                                            + "\nYearly Inflation = " + inflation + " \n");
                }
                double i = ((inflation / 12 * 100000000) - ((inflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                if (optRate == 0)
                {

                    double monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round
                    if (printOrNo == true)
                    {
                        double r = 0;
                        double calcPow = 0;
                        WindowsUtilities.GetLogger()("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
                    }
                    return monthlyPmt;
                }
                else if (optRate > 0)
                {
                    double r = ((optRate / 12 * 100000000) - ((optRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double calcPow = Math.Pow((1 + r), timeForCalc);
                    double monthlyPmt = ((amtForCalc * (1 + i)) * (r * calcPow) / (calcPow - 1));
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round

                    if (printOrNo == true)
                    {
                        WindowsUtilities.GetLogger()("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
                    }
                    return monthlyPmt;
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Rate Out of Range");
                }                

            }
            else
            {
                if (optType == (int)Options.typesList.EMPTY)
                {
                    throw new System.ArgumentOutOfRangeException("Option Type Out of Range");
                }
                else if (optAmt <= CalculationConstants.optionMinimumAmount)
                {
                    throw new System.ArgumentOutOfRangeException("Option Amount Out of Range");
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
                }
            }
        }





        // **************************************************************************************************************************** //
        // ********************************** PRIVATE - Calculating full luah silukin for option ************************************** //

        private void CalculateLuahSilukin(int x)
        {
            if (inflation == 0)
            {
                optTtlPrincipalPay = optAmt;
                optTtlPay = optTime * Math.Round(optPmt, 2);
                optTtlRatePay = optTtlPay - optTtlPrincipalPay;
            }
            else
            {
                // Shuky: add the new plans definitions
                double r = ((optRate / 12 * 100000000) - ((optRate / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double i = ((inflation / 12 * 100000000) - ((inflation / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                double monthlyPmt = Math.Round(optPmt, 2);
                double startingAmount = optAmt;

                for (uint months = 1; months <= optTime; months++)
                {
                    // Shuky: add the new plans definitions

                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    optTtlPay += monthlyPmt;
                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(CalculatePmt(startingAmount, (optTime - months), optRate), 2);
                    }
                }
            }
        }






        // **************************************************************************************************************************** //
        // ************************************* PUBLIC - Calculating full luah silukin for option ************************************ //

        public double GetTtlPay()
        {
            if (optTtlPay > 0)
            {
                return optTtlPay;
            }
            else
            {
                CalculateLuahSilukin(7);
                return optTtlPay;
            }
        }








        // **************************************************************************************************************************** //
        // *************************************************** Print Option To String ************************************************* //

        public override string ToString()
        {
            //return "(" + optType + "," + optAmt + "," + optTime + "," + optRate + "," + optPmt + ")";
            
            //return (optType + 4).ToString() + "," + optAmt + "," + optTime + "," + inflationStr + "," + optRate
            //            + "," + (int)optPmt + "," + (int)optTtlPay;
            return (optType + 4).ToString() + "," + optAmt + "," + optTime + "," + inflationStr + "," + optRate;
        }



    }
}
