using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.Logic;

namespace WisorLib
{
    public class Composition
    {
        // General Parameters
        public Option[] opts = {null, null, null};
        private bool[] ttlPayCalculated = {false, false, false};
        public double ttlPmt = -1;
        public double ttlPay = 0;
        public double ttlRatePay = 0;
        public double ttlPrincipalPay = 0;

        double optXBankTtlPay, optYBankTtlPay, optZBankTtlPay;

        public bool calcTtlPayOrNo = false;

        public Composition(Option optionX, Option optionY, Option optionZ, RunEnvironment env)
        {
            opts[(int)Options.options.OPTX] = optionX;
            opts[(int)Options.options.OPTY] = optionY;
            opts[(int)Options.options.OPTZ] = optionZ;

            ttlPmt = (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt + opts[(int)Options.options.OPTZ].optPmt);
            CheckIfTtlCalculatedOrNo(env);

            CalculateBankProfit();
        }

        private void CalculateBankProfit()
        {
            // get the Bank interset value
            double bankRate = RateUtilities.GetBankRate(opts[(int)Options.options.OPTX].product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)opts[(int)Options.options.OPTX].optTime / 12 - 4);
            opts[(int)Options.options.OPTX].SetBankRate(bankRate);
            optXBankTtlPay = opts[(int)Options.options.OPTX].GetBankTtlPay();
            bankRate = RateUtilities.GetBankRate(opts[(int)Options.options.OPTY].product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)opts[(int)Options.options.OPTY].optTime / 12 - 4);
            opts[(int)Options.options.OPTY].SetBankRate(bankRate);
            optYBankTtlPay = opts[(int)Options.options.OPTY].GetBankTtlPay();
            bankRate = RateUtilities.GetBankRate(opts[(int)Options.options.OPTZ].product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)opts[(int)Options.options.OPTZ].optTime / 12 - 4);
            opts[(int)Options.options.OPTZ].SetBankRate(bankRate);
            optZBankTtlPay = opts[(int)Options.options.OPTZ].GetBankTtlPay();
         }

   

        // **************************************************************************************************************************** //
        // *********************************** Check if Options have Luah Silukins Calculated Or No *********************************** //

        private void CheckIfTtlCalculatedOrNo(RunEnvironment env)
        {
            if (ttlPayCalculated[(int)Options.options.OPTX] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTX, env);
            }
            if (ttlPayCalculated[(int)Options.options.OPTY] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTY, env);
            }
            if (ttlPayCalculated[(int)Options.options.OPTZ] == false)
            {
                CalculateTtlPayForOneOption((int)Options.options.OPTZ, env);
            }
        }



        // **************************************************************************************************************************** //
        // ************************************** PRIVATE - Calculating Luah Silukin for One Option *********************************** //

        private void CalculateTtlPayForOneOption(int optionInCompositionArray, RunEnvironment env)
        {
            if (ttlPayCalculated[optionInCompositionArray] == false)
            {
                opts[optionInCompositionArray].GetTtlPay(env);
                ttlPay += opts[optionInCompositionArray].optTtlPay;
                ttlRatePay += opts[optionInCompositionArray].optTtlRatePay;
                ttlPrincipalPay += opts[optionInCompositionArray].optTtlPrincipalPay;
                ttlPayCalculated[optionInCompositionArray] = true;
            }
        }





        // **************************************************************************************************************************** //
        // ***************************************** PUBLIC - Show Luah Silukin for One Option **************************************** //

        public double GetTtlPayOneOption(int optInCompositionForCalculation, RunEnvironment env)
        {
            if (ttlPayCalculated[optInCompositionForCalculation] == false)
            {
                CalculateTtlPayForOneOption(optInCompositionForCalculation, env);
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
            int ttlBankPayPayk = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay);

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
                        + "," + (int)ttlPay
                        // the bank profit data
                        + "," + Convert.ToInt32(optXBankTtlPay).ToString()
                        + "," + Convert.ToInt32(optYBankTtlPay).ToString()
                        + "," + Convert.ToInt32(optZBankTtlPay).ToString()
                        + "," + Convert.ToInt32(ttlBankPayPayk).ToString()
                        + "," + Convert.ToInt32(ttlPay - ttlBankPayPayk).ToString();
        }

    }
}
