using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    public class OneOptType
    {

        public enum marketType { UK, USA, Israel, Other };
        public enum indexType { Prime, Libor, EUROor, Other };
        public enum indexOperator { Plus, Minux, Other };


        // General Parameters
        public uint         typeId;
        public uint         minTime;
        public uint         maxTime;
        public uint         jump;
        public string       optName;

        // Shuky: add the new plans definitions
        public uint         firstPeriod;
        public uint         secondPeriod;
        public marketType   market;

        public indexType    index1;
        public indexOperator        index1FormulaOperatorFirstPeriod;
        public double               index1FormulaDeltaFirstPeriod;
        public indexOperator        index1FormulaOperatorSecondPeriod;
        public double               index1FormulaDeltaSecondPeriod;
        public indexType    index2;
        public indexOperator        index2FormulaOperatorFirstPeriod;
        public double               index2FormulaDeltaFirstPeriod;
        public indexOperator        index2FormulaOperatorSecondPeriod;
        public double               index2FormulaDeltaSecondPeriod;


        // Product types
        // Omri: define the other markets
        public OneOptType(uint optionType)
        {
            if (optionType > (int)Options.typesList.EMPTY)
            {
                typeId = optionType;
                switch (optionType)
                {
                    case (int)Options.typesList.PRIME:
                        minTime = 48;
                        maxTime = 360;
                        jump = 12;
                        optName = "Prime";
                        break;

                    case (int)Options.typesList.FIXNOTSAMUD:
                        minTime = 48;
                        maxTime = 360;
                        jump = 12;
                        optName = "Kalatz";
                        break;

                    case (int)Options.typesList.ALT60NOTSAMUD:
                        minTime = 120;
                        maxTime = 360;
                        jump = 60;
                        optName = "Alt-5Yr No Tsamud";
                        break;

                    case (int)Options.typesList.FIXTSAMUD:
                        minTime = 48;
                        maxTime = 360;
                        jump = 12;
                        optName = "Fix Tsamud";
                        break;

                    case (int)Options.typesList.ALT12:
                        minTime = 48;
                        maxTime = 360;
                        jump = 12;
                        optName = "Alt 1-Yr";
                        break;

                    case (int)Options.typesList.ALT24:
                        minTime = 48;
                        maxTime = 360;
                        jump = 24;
                        optName = "Alt 2-Yr";
                        break;

                    case (int)Options.typesList.ALT30:
                        minTime = 60;
                        maxTime = 360;
                        jump = 30;
                        optName = "Alt 2.5-Yr";
                        break;

                    case (int)Options.typesList.ALT60:
                        minTime = 120;
                        maxTime = 360;
                        jump = 60;
                        optName = "Alt 5-Yr";
                        break;

                    case (int)Options.typesList.ALT84:
                        minTime = 168;
                        maxTime = 336;
                        jump = 84;
                        optName = "Alt 7-Yr";
                        break;

                    case (int)Options.typesList.ALT120:
                        minTime = 240;
                        maxTime = 360;
                        jump = 120;
                        optName = "Alt 10-Yr";
                        break;
                }
            }
            else
            {
                typeId = (int)Options.typesList.EMPTY;
                minTime = 0;
                maxTime = 0;
                jump = 0;
                optName = "Empty";
            }
        }
    }


    public class OptionTypes
    {
        public OneOptType[] optionTypes = { null, null, null };
        
        

        public OptionTypes(uint optXType, uint optYType, uint optZType)
        {
            optionTypes[(int)Options.options.OPTX] = new OneOptType(optXType);
            optionTypes[(int)Options.options.OPTY] = new OneOptType(optYType);
            optionTypes[(int)Options.options.OPTZ] = new OneOptType(optZType);
            GetAgeRestriction();
            GetMaxAmountsForThreeOptions();
        }





        // ***************************************************** Age Restriction ****************************************************** //

        public void GetAgeRestriction()
        {
            if (CalculationParameters.youngestLenderAge >= 18)
            {
                if (CalculationParameters.youngestLenderAge <= 45)
                {
                    CalculationParameters.maximumTimeForLoan = 360;
                }
                else
                {
                    CalculationParameters.maximumTimeForLoan = ((75 - CalculationParameters.youngestLenderAge) * 12);
                }
            }
            else
            {
                // Shouldnt happen - input should be 18-71
            }

            if (PrintOptions.printSubFunctionsInConsole == true)
            {
                Console.WriteLine("\nMaximum time possible = " + (CalculationParameters.maximumTimeForLoan / 12));
            }
            GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTX]);
            GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTY]);
            GetAgeRestrictionOneOption(optionTypes[(int)Options.options.OPTZ]);
        }





        // ******************************************** Get Maximum Time For One Option *********************************************** //

        private static void GetAgeRestrictionOneOption(OneOptType optTypeForCheck)
        {
            //Console.WriteLine("Option type = " + optTypeForCheck.typeId);
            //Console.ReadKey();
            uint remainingTimePossible = CalculationParameters.maximumTimeForLoan - (CalculationParameters.maximumTimeForLoan % optTypeForCheck.jump);

            if (PrintOptions.printSubFunctionsInConsole == true)
            {
                Console.WriteLine("Option Type = " + optTypeForCheck.optName + "\nMaximum time for option type = " + optTypeForCheck.maxTime
                                    + "\nMaximum time possible for loaner = " + CalculationParameters.maximumTimeForLoan
                                    + "\nNew maximum time for option type = " + remainingTimePossible);
            }

            optTypeForCheck.maxTime = remainingTimePossible;

        }





        // **************************************************************************************************************************** //
        // ***************************************** Finding Maximum Amounts For All Options ****************************************** //

        private void GetMaxAmountsForThreeOptions()
        {
            CalculationParameters.maxAmts[(int)Options.options.OPTX] = FindMaxAmount(CalculationParameters.loanAmtWanted,
                                                                optionTypes[(int)Options.options.OPTX].typeId);
            CalculationParameters.maxAmts[(int)Options.options.OPTY] = FindMaxAmount(CalculationParameters.loanAmtWanted,
                                                                            optionTypes[(int)Options.options.OPTY].typeId);
            CalculationParameters.maxAmts[(int)Options.options.OPTZ] = FindMaxAmount(CalculationParameters.loanAmtWanted,
                                                                            optionTypes[(int)Options.options.OPTZ].typeId);          
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
        
        private static double FindMaxAmount(double loanAmountTemp, uint optionType)
        {
            double loanAmount = loanAmountTemp;
            int numOfFixedOptions = 2;

            switch (optionType)
            {
                case ((int)Options.typesList.PRIME):
                    /*
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 /100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);                        
                    }                                                           
                    */
                    /*
                    {
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }
                    */
                    
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                    

                case ((int)Options.typesList.FIXNOTSAMUD): return ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
                case ((int)Options.typesList.ALT60NOTSAMUD):
                    /*{
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }*/
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                case ((int)Options.typesList.FIXTSAMUD): return ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
                case ((int)Options.typesList.ALT12):
                    /*{
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }*/
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                case ((int)Options.typesList.ALT24):
                    /*{
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }*/
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                case ((int)Options.typesList.ALT30):
                    /*{
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }*/
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                case ((int)Options.typesList.ALT60):
                    /*{
                        if ((((loanAmount / 3) / 100) - (((loanAmount / 3) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount / 300) - ((loanAmount / 300) % 1)) - 1) * 100);
                        else
                            return (((loanAmount / 300) - ((loanAmount / 300) % 1)) * 100);
                    }*/
                    {
                        if ((((loanAmount * 3 / 10) / 100) - (((loanAmount * 3 / 10) / 100) % 1)) % 2 == 1)
                            return ((((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) - 1) * 100);
                        else
                            return (((loanAmount * 3 / 10 / 100) - ((loanAmount * 3 / 10 / 100) % 1)) * 100);
                    }
                case ((int)Options.typesList.ALT84): return ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
                case ((int)Options.typesList.ALT120): return ((loanAmount - (numOfFixedOptions * CalculationConstants.optionMinimumAmount)));
                default: return -1;
            }
        }
        
        

    }

}
