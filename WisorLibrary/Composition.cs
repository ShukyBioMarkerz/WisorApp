using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class Composition
    {
        // General Parameters
        public Option[] opts = {null, null, null};
        private bool[] ttlPayCalculated = {false, false, false};
        public double ttlPmt = -1;
        public double ttlPay = 0;
        public double ttlRatePay = 0;
        public double ttlPrincipalPay = 0;

        public bool calcTtlPayOrNo = false;

        public Composition(Option optionX, Option optionY, Option optionZ)
        {
            opts[(int)Options.options.OPTX] = optionX;
            opts[(int)Options.options.OPTY] = optionY;
            opts[(int)Options.options.OPTZ] = optionZ;

            ttlPmt = (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt + opts[(int)Options.options.OPTZ].optPmt);
            CheckIfTtlCalculatedOrNo();

        }








        // **************************************************************************************************************************** //
        // *********************************** Check if Options have Luah Silukins Calculated Or No *********************************** //

        private void CheckIfTtlCalculatedOrNo()
        {
            if (ttlPayCalculated[(int)Options.options.OPTX] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTX);
            }
            if (ttlPayCalculated[(int)Options.options.OPTY] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTY);
            }
            if (ttlPayCalculated[(int)Options.options.OPTZ] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTZ);
            }
        }



        // **************************************************************************************************************************** //
        // ************************************** PRIVATE - Calculating Luah Silukin for One Option *********************************** //

        private void CalculateTtlPayForOneOption(int optionInCompositionArray)
        {
            if (ttlPayCalculated[optionInCompositionArray] == false)
            {
                opts[optionInCompositionArray].GetTtlPay();
                ttlPay += opts[optionInCompositionArray].optTtlPay;
                ttlRatePay += opts[optionInCompositionArray].optTtlRatePay;
                ttlPrincipalPay += opts[optionInCompositionArray].optTtlPrincipalPay;
                ttlPayCalculated[optionInCompositionArray] = true;
            }
        }





        // **************************************************************************************************************************** //
        // ***************************************** PUBLIC - Show Luah Silukin for One Option **************************************** //

        public double GetTtlPayOneOption(int optInCompositionForCalculation)
        {
            if (ttlPayCalculated[optInCompositionForCalculation] == false)
            {
                CalculateTtlPayForOneOption(optInCompositionForCalculation);
                return opts[optInCompositionForCalculation].optTtlPay;
            }
            else
            {
                return opts[optInCompositionForCalculation].optTtlPay;
            }
        }








        // **************************************************************************************************************************** //
        // *************************************************** Print Composition To String ******************************************** //

        public override string ToString()
        {
            return "" + opts[(int)Options.options.OPTX]
                    + "," + opts[(int)Options.options.OPTY]
                        + "," + opts[(int)Options.options.OPTZ]
                        + "," + (int)opts[(int)Options.options.OPTX].optPmt
                        + "," + (int)opts[(int)Options.options.OPTY].optPmt
                        + "," + (int)opts[(int)Options.options.OPTZ].optPmt
                        + "," + (int)ttlPmt
                        + "," + (int)opts[(int)Options.options.OPTX].optTtlPay
                        + "," + (int)opts[(int)Options.options.OPTY].optTtlPay
                        + "," + (int)opts[(int)Options.options.OPTZ].optTtlPay                        
                        + "," + (int)ttlPay;
            /*
            return "" + opts[(int)Options.options.OPTX] + "," + opts[(int)Options.options.OPTX].optTtlPay
            + "," + opts[(int)Options.options.OPTY] + "," + opts[(int)Options.options.OPTY].optTtlPay
            + "," + opts[(int)Options.options.OPTZ] + "," + opts[(int)Options.options.OPTZ].optTtlPay
            + "," + ttlPmt + "," + ttlPay;
            */
        }



    }
}
