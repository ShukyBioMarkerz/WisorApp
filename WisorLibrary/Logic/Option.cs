using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    class Option
    {

        // General Parameters
        public string optType;
        public double optAmt = 0;
        public uint optTime = 0;
        public double optPmt = -1;

        //private double[] optRateList = new double[360];
        public double optTtlPay = 0;
        public double optTtlRatePay = 0;
        public double optTtlPrincipalPay = 0;
        private bool printOrNo = false;

        private string inflationStr = "";

        //private double optRate = -1;
        private double optRateFirstPeriod = -1;
        private double optRateSecondPeriod = -1;

        // Shuky changes:
        //public double inflation = -1;
        public double indexFirstPeriod { get; set; }
        public double indexSecondPeriod { get; set; }

        private GenericProduct product;

        public Option(/*uint optionType*/ string optionType, double optionAmount, uint optionTime)
        {
            optType = optionType;
            optAmt = optionAmount;
            optTime = optionTime;

            // TBD: what is the unique ID of the product???
            product = MiscConstants.GetProduct(optionType);
            //inflation = GetInflationForOption();
            // Shuky TBD
            //inflation = 0;
            bool rc = GetInflationsForOption(optType);

            optRateFirstPeriod = FindInterestRate();
            if (-1 == optRateFirstPeriod)
            {
                // TBD: Uston we have a problem
            }
            optPmt = CalculatePmt(optAmt, optTime, optRateFirstPeriod);
        }

        //private void InsertRatesInListForCalculation()
        //{
        //    if ((optType > (int)Options.typesList.EMPTY) &&
        //            (optAmt >= CalculationConstants.optionMinimumAmount) &&
        //                (optTime >= CalculationConstants.minimumTimeForLoan))                                                      
        //    {
        //        for (int i = 0; i < optRateList.Length; i++ )
        //        {
        //            optRateList[i] = optRate;
        //        }
        //    }
        //    else
        //    {
        //        if (optType == (int)Options.typesList.EMPTY)
        //        {
        //            throw new System.ArgumentOutOfRangeException("Option Type Out of Range");
        //        }
        //        else if (optAmt <= CalculationConstants.optionMinimumAmount)
        //        {
        //            throw new System.ArgumentOutOfRangeException("Option Amount Out of Range");
        //        }
        //        else
        //        {
        //            throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
        //        }
        //    }

        //}





        // **************************************************************************************************************************** //
        // **************************************** Getting Inflation According to Option Type **************************************** //

        //should be called GetIndex4Option
        // and return 2 values for: indexUsedFirstTimePeriod and indexUsedSecondTimePeriod
        // according to the Product -> typeId
        //indices { MADAD, PRIME, CPI, FED, LIBOR, EUROBOR, BBBR, OTHER, NONE

        private bool GetInflationsForOption(string id)
        {
            bool rc = false;
            GenericProduct product = GetProduct(id);
            if (null != product)
            {
                indices indexUsedFirstTimePeriod = product.indexUsedFirstTimePeriod;
                indices indexUsedSecondTimePeriod = product.indexUsedSecondTimePeriod;
                indexFirstPeriod = MiscConstants.GetIndexRateForOption(indexUsedFirstTimePeriod);
                indexSecondPeriod = MiscConstants.GetIndexRateForOption(indexUsedSecondTimePeriod);
                rc = true;
            }
            else
            {
                WindowsUtilities.loggerMethod("GetInflationsForOption: failed to find product id: " + id);
                indexFirstPeriod = indexSecondPeriod = ILLEGAL_RATE_VALUE;
            }
            return rc;
        }


  

        // **************************************************************************************************************************** //
        // ***************************** Calculating Interest Rate According to Option Type and Option Time *************************** //

        // get the rate by the product, the borrower profile and time
        private double FindInterestRate()
        {
            double rate = MiscConstants.UNDEFINED_DOUBLE;

            if (/*(optType > (int)Options.typesList.EMPTY)*/ 
                ! String.IsNullOrEmpty(optType ) && (optTime >= CalculationConstants.minimumTimeForLoan))
            {
                // Instead of looking for product in local class -> lookup via xml
                // Go to local Rates class, lookup value and plug in
                // TBD: define the all types in the xml like Options.typesList.PRIME
                // Omri 
                // TBD - fix the -4 constant 
                rate = Rates.FindRateForKey(product.ID, BorrowerProfile.borrowerProfile, (int)optTime/12-4);
              
                //switch (optType)
                //{
                //    // TBD Omri - we should solve this
                //    case ((int)Options.typesList.PRIME):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.PRIME];
                //    case ((int)Options.typesList.FIXNOTSAMUD):
                //                                    return Rates.fixedNoTsamudRates[(optTime / 12) - 4];
                //    case ((int)Options.typesList.ALT60NOTSAMUD):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH60NOINF];
                //    case ((int)Options.typesList.FIXTSAMUD):
                //                                    return Rates.fixedTsamudRates[(optTime / 12) - 4];
                //    case ((int)Options.typesList.ALT12):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH12];
                //    case ((int)Options.typesList.ALT24):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH24];
                //    case ((int)Options.typesList.ALT30):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH30];
                //    case ((int)Options.typesList.ALT60):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH60YESINF];
                //    case ((int)Options.typesList.ALT84):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH84];
                //    case ((int)Options.typesList.ALT120):
                //                                    return Rates.alternateRates[(int)Rates.alternatingRates.MTH120];
                //    default: return -1;
                //}
            }
            else
            {
                if (String.IsNullOrEmpty(optType) /*optType == (int)Options.typesList.EMPTY*/)
                {
                    throw new System.ArgumentOutOfRangeException("Option Type Out of Range");

                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Time Out of Range");
                }
            }

            return rate;
        }





        // **************************************************************************************************************************** //
        // *************************************************** Show Interest Rate ***************************************************** //

        public double ShowRate()
        {
            if ((optTime >= CalculationConstants.minimumTimeForLoan) && (! String.IsNullOrEmpty(optType) /*> (int)Options.typesList.EMPTY*/))
            {
                if (optRateFirstPeriod == -1)
                {
                    optRateFirstPeriod = FindInterestRate();
                    return optRateFirstPeriod;
                }
                else if (optRateFirstPeriod >= 0)
                {
                    return optRateFirstPeriod;
                }              
                else
                {
                    throw new System.ArgumentOutOfRangeException("Option Rate Out of Range");
                }
            }
            else
            {
                if (String.IsNullOrEmpty(optType))
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
            double currInterest = MiscConstants.UNDEFINED_DOUBLE; 

            if ((! String.IsNullOrEmpty(optType) /*optType > Options.typesList.EMPTY*/) && (amtForCalc > 0) && (timeForCalc > 0))                                          
            {
                if ((amtForCalc > 0) && (timeForCalc > 0))
                {
                    if (product.firstTimePeriod <= timeForCalc)
                    {
                        currInterest = optRateFirstPeriod = FindInterestRate();
                    }
                    else
                    {
                        currInterest = optRateSecondPeriod = FindInterestRate();
                    }
                }
                if (printOrNo == true)
                {
                    WindowsUtilities.loggerMethod("Option:\nType = " + product.name /*Options.optionTypes[optType]*/ + "\nAmount = " + amtForCalc
                                            + "\nTime in months = " + timeForCalc + "\nYearly Rate = " + interestRateForCalc
                                            + "\nYearly Inflation = " + indexFirstPeriod /*inflation*/ + " \n");
                }
                double i = ((indexFirstPeriod /*inflation*/ / 12 * 100000000) - ((indexFirstPeriod /*inflation*/ / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                if (0 == currInterest)
                {

                    double monthlyPmt = ((amtForCalc * (1 + i)) / timeForCalc);
                    monthlyPmt = ((monthlyPmt * 100000) - ((monthlyPmt * 100000) % 1)) / 100000; // Instead of Math.Round
                    if (printOrNo == true)
                    {
                        double r = 0;
                        double calcPow = 0;
                        WindowsUtilities.loggerMethod("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
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
                        WindowsUtilities.loggerMethod("Monthly Rate = " + r + "\nPower = " + calcPow + "\nMonthly Inflation = " + i + "\nMonthly Payment = " + monthlyPmt + " \n");
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
                if (String.IsNullOrEmpty(optType) /*optType == (int)Options.typesList.EMPTY*/)
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

        private void CalculateLuahSilukin()
        {
            if (product.firstTimePeriod == 0)
            {
                if (product.indexUsedFirstTimePeriod == 0)
                {
                    optTtlPrincipalPay = optAmt;
                    optTtlPay = optTime * Math.Round(optPmt, 2);
                    optTtlRatePay = optTtlPay - optTtlPrincipalPay;
                }
                else
                {
                    double r = ((optRateFirstPeriod / 12 * 100000000) - ((optRateFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                    double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                    double monthlyPmt = Math.Round(optPmt, 2);
                    double startingAmount = optAmt;

                    for (uint months = 1; months <= optTime; months++)
                    {
                        double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                        double principalPmt = monthlyPmt - ratePmt;
                        optTtlPrincipalPay += principalPmt;
                        optTtlRatePay += ratePmt;
                        optTtlPay += monthlyPmt;
                        startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                        if (months < optTime)
                        {
                            monthlyPmt = Math.Round(CalculatePmt(startingAmount, (optTime - months), optRateFirstPeriod), 2);
                        }
                    }
                }
            }
            else
            {
                double r = ((optRateFirstPeriod / 12 * 100000000) - ((optRateFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                double i = ((indexFirstPeriod / 12 * 100000000) - ((indexFirstPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round

                double monthlyPmt = Math.Round(optPmt, 2);
                double startingAmount = optAmt;
                optRateSecondPeriod = optRateFirstPeriod;
                // TBD Omri
                //optRateSecondPeriod = CalculateInterestRate4SecondPeriod()

                for (uint months = 1; months <= product.firstTimePeriod; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    optTtlPay += monthlyPmt;
                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(CalculatePmt(startingAmount, (optTime - months), optRateFirstPeriod), 2);
                    }
                }
                r = ((optRateSecondPeriod / 12 * 100000000) - ((optRateSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                i = ((indexSecondPeriod / 12 * 100000000) - ((indexSecondPeriod / 12 * 100000000) % 1)) / 100000000; // Instead of Math.Round
                for (uint months = product.firstTimePeriod + 1; months <= optTime; months++)
                {
                    double ratePmt = Math.Round((startingAmount * (1 + i) * r), 2);
                    double principalPmt = monthlyPmt - ratePmt;
                    optTtlPrincipalPay += principalPmt;
                    optTtlRatePay += ratePmt;
                    optTtlPay += monthlyPmt;
                    startingAmount = Math.Round((((startingAmount) * (1 + i)) - principalPmt), 2);
                    if (months < optTime)
                    {
                        monthlyPmt = Math.Round(CalculatePmt(startingAmount, (optTime - months), optRateSecondPeriod), 2);
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
                CalculateLuahSilukin();
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
            return (optType + 4).ToString() + "," + optAmt + "," + optTime + "," + inflationStr + "," + optRateFirstPeriod;
        }



    }
}
