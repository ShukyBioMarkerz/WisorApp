using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class OneOptType
    {

        public GenericProduct product { get; }

        //public enum marketType { UK, USA, Israel, Other };
        //public enum indexType { Prime, Libor, EUROor, Other };
        //public enum indexOperator { Plus, Minux, Other };


        //// General Parameters
        //public uint         typeId;
        //public uint         minTime;
        //public uint         maxTime;
        //public uint         jump;
        //public string       optName;

        //// Shuky: add the new plans definitions
        //public uint         firstPeriod;
        //public uint         secondPeriod;
        //public marketType   market;

        //public indexType    index1;
        //public indexOperator        index1FormulaOperatorFirstPeriod;
        //public double               index1FormulaDeltaFirstPeriod;
        //public indexOperator        index1FormulaOperatorSecondPeriod;
        //public double               index1FormulaDeltaSecondPeriod;
        //public indexType    index2;
        //public indexOperator        index2FormulaOperatorFirstPeriod;
        //public double               index2FormulaDeltaFirstPeriod;
        //public indexOperator        index2FormulaOperatorSecondPeriod;
        //public double               index2FormulaDeltaSecondPeriod;


        // Product types
        // Omri: define the other markets
        // TBD - all arrive from the xml product file

        public OneOptType(int optionType)
        {
            product = GenericProduct.GetProduct(optionType);
            if (null == product)
                WindowsUtilities.loggerMethod("ERROR OneOptType can't find product id: " + optionType);
        }

        //public OneOptType(uint optionType)
        //{
        //    if (optionType > (int)Options.typesList.EMPTY)
        //    {
        //        typeId = optionType;
        //        switch (optionType)
        //        {
        //            case (int)Options.typesList.PRIME:
        //                minTime = 48;
        //                maxTime = 360;
        //                jump = 12;
        //                optName = "Prime";
        //                break;

        //            case (int)Options.typesList.FIXNOTSAMUD:
        //                minTime = 48;
        //                maxTime = 360;
        //                jump = 12;
        //                optName = "Kalatz";
        //                break;

        //            case (int)Options.typesList.ALT60NOTSAMUD:
        //                minTime = 120;
        //                maxTime = 360;
        //                jump = 60;
        //                optName = "Alt-5Yr No Tsamud";
        //                break;

        //            case (int)Options.typesList.FIXTSAMUD:
        //                minTime = 48;
        //                maxTime = 360;
        //                jump = 12;
        //                optName = "Fix Tsamud";
        //                break;

        //            case (int)Options.typesList.ALT12:
        //                minTime = 48;
        //                maxTime = 360;
        //                jump = 12;
        //                optName = "Alt 1-Yr";
        //                break;

        //            case (int)Options.typesList.ALT24:
        //                minTime = 48;
        //                maxTime = 360;
        //                jump = 24;
        //                optName = "Alt 2-Yr";
        //                break;

        //            case (int)Options.typesList.ALT30:
        //                minTime = 60;
        //                maxTime = 360;
        //                jump = 30;
        //                optName = "Alt 2.5-Yr";
        //                break;

        //            case (int)Options.typesList.ALT60:
        //                minTime = 120;
        //                maxTime = 360;
        //                jump = 60;
        //                optName = "Alt 5-Yr";
        //                break;

        //            case (int)Options.typesList.ALT84:
        //                minTime = 168;
        //                maxTime = 336;
        //                jump = 84;
        //                optName = "Alt 7-Yr";
        //                break;

        //            case (int)Options.typesList.ALT120:
        //                minTime = 240;
        //                maxTime = 360;
        //                jump = 120;
        //                optName = "Alt 10-Yr";
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        typeId = (int)Options.typesList.EMPTY;
        //        minTime = 0;
        //        maxTime = 0;
        //        jump = 0;
        //        optName = "Empty";
        //    }
        //}
    }


    public class OptionTypes
    {
        public OneOptType[] optionTypes = { null, null, null };
  

        public OptionTypes(int optXType, int optYType, int optZType, RunEnvironment env)
        {
            optionTypes[(int)Options.options.OPTX] = new OneOptType(optXType);
            optionTypes[(int)Options.options.OPTY] = new OneOptType(optYType);
            if (MiscUtilities.Use3ProductsInComposition())
                optionTypes[(int)Options.options.OPTZ] = new OneOptType(optZType);
            GetAgeRestriction(env);
            GetMinMaxAmountsForThreeOptions(env);
         }





        // ***************************************************** Age Restriction ****************************************************** //

        public void GetAgeRestriction(RunEnvironment env)
        {
            if (env.CalculationParameters.youngestLenderAge >= 18)
            {
                if (env.CalculationParameters.youngestLenderAge <= 45)
                {
                    env.CalculationParameters.maximumTimeForLoan = 360;
                }
                else
                {
                    env.CalculationParameters.maximumTimeForLoan = ((75 - env.CalculationParameters.youngestLenderAge) * 12);
                }
            }
            else
            {
                // Shouldnt happen - input should be 18-71
            }

            if (env.PrintOptions.printSubFunctionsInConsole == true)
            {
                Console.WriteLine("\nMaximum time possible = " + (env.CalculationParameters.maximumTimeForLoan / 12));
            }
            if (markets.ISRAEL == Share.theMarket)
            {
                GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTX], env);
                GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTY], env);
                if (MiscUtilities.Use3ProductsInComposition())
                    GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTZ], env);
            }
        }





        // ******************************************** Get Maximum Time For One Option *********************************************** //

        private static void GetAgeRestrictionOneOption(OneOptType optTypeForCheck, RunEnvironment env)
        {
            //Console.WriteLine("Option type = " + optTypeForCheck.typeId);
            //Console.ReadKey();

            uint remainingTimePossible;
            if (null != optTypeForCheck.product && 0 < optTypeForCheck.product.timeJump)
            {
                remainingTimePossible = env.CalculationParameters.maximumTimeForLoan - (env.CalculationParameters.maximumTimeForLoan % optTypeForCheck.product.timeJump);
            }
            else
            {
                remainingTimePossible = env.CalculationParameters.maximumTimeForLoan;
            }

            if (null != optTypeForCheck.product && optTypeForCheck.product.maxTime > optTypeForCheck.product.minTime)
            {
                if (remainingTimePossible >= optTypeForCheck.product.maxTime)
                    optTypeForCheck.product.maxTime = remainingTimePossible;
            }



            if (env.PrintOptions.printSubFunctionsInConsole == true)
            {
                Console.WriteLine("Option Type = " + optTypeForCheck.product.name + "\nMaximum time for option type = " + optTypeForCheck.product.maxTime
                                    + "\nMaximum time possible for loaner = " + env.CalculationParameters.maximumTimeForLoan
                                    + "\nNew maximum time for option type = " + remainingTimePossible);
            }

            if (null != optTypeForCheck.product)
            {
                optTypeForCheck.product.maxTime = remainingTimePossible;
                if (optTypeForCheck.product.maxTime <= CalculationConstants.minimumTimeForLoan)
                {
                    Console.WriteLine("NOTICE: Option Type = " + optTypeForCheck.product.name + "\nMaximum time for option type = " + optTypeForCheck.product.maxTime
                                        + "\nMaximum time possible for loaner = " + env.CalculationParameters.maximumTimeForLoan
                                        + "\nNew maximum time for option type = " + remainingTimePossible);

                }
            }

        }





        // **************************************************************************************************************************** //
        // ***************************************** Finding Maximum Amounts For All Options ****************************************** //

        private void GetMinMaxAmountsForThreeOptions(RunEnvironment env)
        {
            MinMax minmax = Calculations.FindMinMaxAmount(
                env.CalculationParameters.loanAmtWanted, optionTypes[(int)Options.options.OPTX], 
                env.theLoan.risk, env.theLoan.liquidity,
                env.CalculationParameters.minAmts[(int)Options.options.OPTX]);
            if (null != minmax)
            {
                env.CalculationParameters.minAmts[(int)Options.options.OPTX] = minmax.min;
                env.CalculationParameters.maxAmts[(int)Options.options.OPTX] = minmax.max;
            }

            minmax = Calculations.FindMinMaxAmount(
                env.CalculationParameters.loanAmtWanted, optionTypes[(int)Options.options.OPTY],
                env.theLoan.risk, env.theLoan.liquidity,
                env.CalculationParameters.minAmts[(int)Options.options.OPTY]);
            if (null != minmax)
            {
                env.CalculationParameters.minAmts[(int)Options.options.OPTY] = minmax.min;
                env.CalculationParameters.maxAmts[(int)Options.options.OPTY] = minmax.max;
            }

            // number of products in composition
            if (MiscUtilities.Use3ProductsInComposition())
            {
                minmax = Calculations.FindMinMaxAmount(
                        env.CalculationParameters.loanAmtWanted, optionTypes[(int)Options.options.OPTZ],
                        env.theLoan.risk, env.theLoan.liquidity,
                        env.CalculationParameters.minAmts[(int)Options.options.OPTZ]);
                if (null != minmax)
                {
                    env.CalculationParameters.minAmts[(int)Options.options.OPTZ] = minmax.min;
                    env.CalculationParameters.maxAmts[(int)Options.options.OPTZ] = minmax.max;
                }
            }

            // TBD: ensure the sum produce the loan ...
       }


        // **************************************************************************************************************************** //
        // *********************************** Finding Maximum Amount According to Option Type **************************************** //

        /*private static double FindMaxAmount(double loanAmountTemp, uint optionType)
        {
            double loanAmount = loanAmountTemp;
            if ((optionType == (int)Options.typesList.ALT60) || (optionType == (int)Options.typesList.ALT60NOTSAMUD))
            {
                return (loanAmount / 2);
            }
            else
                return (loanAmount - (2 * CalculationConstants.optionMinimumAmount));    

        }
        */

        // **************************************************************************************************************************** //
        // *********************************** Finding Maximum Amount According to Option Type **************************************** //

        // Omri TBD


     
    }

}
