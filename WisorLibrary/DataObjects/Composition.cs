using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WisorLibrary.Logic;


namespace WisorLib
{
    [Serializable]
    public class Composition
    {
        // General Parameters
        public string name { get; set; }
        public Option[] opts = {null, null, null};
        private bool[] ttlPayCalculated = {false, false, false};
        public double ttlPmt = 0;
        public double ttlPay = 0;
        [XmlIgnoreAttribute]
        public double ttlRatePay = 0;
        [XmlIgnoreAttribute]
        public double ttlPrincipalPay = 0;
        [XmlIgnoreAttribute]
        public double score = 0; // enable to sort the composition by scoring

        public uint optXBankTtlPay, optYBankTtlPay, optZBankTtlPay;
        [XmlIgnoreAttribute]
        public bool calcTtlPayOrNo = false;

        // for the sake of serialization.....
        public Composition()
        {
        }

        public Composition(Option optionX, Option optionY, Option optionZ, RunEnvironment env, string name)
        {
            this.name = name;
            opts[(int)Options.options.OPTX] = optionX;
            opts[(int)Options.options.OPTY] = optionY;
            opts[(int)Options.options.OPTZ] = optionZ;

            ttlPmt = (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt + opts[(int)Options.options.OPTZ].optPmt);
            CheckIfTtlCalculatedOrNo(env);

            CalculateBankProfit(env);
        }

        private void CalculateBankProfit(RunEnvironment env)
        {
            Calculations.CalculateTheBankProfit(opts[(int)Options.options.OPTX], opts[(int)Options.options.OPTY],
                opts[(int)Options.options.OPTZ], env.BorrowerProfile.profile);

            optXBankTtlPay = opts[(int)Options.options.OPTX].CalculateLuahSilukinBank();
            optYBankTtlPay = opts[(int)Options.options.OPTY].CalculateLuahSilukinBank();
            optZBankTtlPay = opts[(int)Options.options.OPTZ].CalculateLuahSilukinBank();
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

        public static string PrintHeader()
        {
            string s = "\nComposition list:";

            s += /*"Ticks" + "," + "OrderID" + "," + "Time" + "," +*/
                "X:optType" + "," + "X:optAmt" + "," + "X:optTime" + "," + "X:RateFirstPeriod" + "," +
                "Y:optType" + "," + "Y:optAmt" + "," + "Y:optTime" + "," + "Y:RateFirstPeriod" + "," +
                "Z:optType" + "," + "Z:optAmt" + "," + "Z:optTime" + "," + "Z:RateFirstPeriod" + "," +
                "OPTXPmt" + "," + "OPTYPmt" + "," + "OPTZPmt" + "," + "ttlPmt" + "," +
                "OPTXTtlPay" + "," + "OPTYTtlPay" + "," + "OPTZTtlPay" + "," + 
                // bank profit data
                "X:BankTtlPay" + "," + "Y:BankTtlPay" + "," + "Z:BankTtlPay" + "," +
                "ttlBorrowerPay" + "," + "TtlBankPay" + "," +  "TtlBankProfit";

            return s;
        }

        public override string ToString()
        {
            int ttlBankPayPayk = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay);
            string s = MiscConstants.UNDEFINED_STRING;

            if (null != opts && null != opts[(int)Options.options.OPTX] && null != opts[(int)Options.options.OPTY] &&
                null != opts[(int)Options.options.OPTZ])
            {
                s = "" + opts[(int)Options.options.OPTX].ToString()
                        + "," + opts[(int)Options.options.OPTY].ToString()
                            + "," + opts[(int)Options.options.OPTZ].ToString()
                            + "," + (int)opts[(int)Options.options.OPTX].optPmt
                            + "," + (int)opts[(int)Options.options.OPTY].optPmt
                            + "," + (int)opts[(int)Options.options.OPTZ].optPmt
                            + "," + (int)ttlPmt
                            + "," + (int)opts[(int)Options.options.OPTX].optTtlPay
                            + "," + (int)opts[(int)Options.options.OPTY].optTtlPay
                            + "," + (int)opts[(int)Options.options.OPTZ].optTtlPay
                            // the bank profit data
                            + "," + Convert.ToInt32(optXBankTtlPay).ToString()
                            + "," + Convert.ToInt32(optYBankTtlPay).ToString()
                            + "," + Convert.ToInt32(optZBankTtlPay).ToString()
                            + "," + (int)ttlPay
                            + "," + Convert.ToInt32(ttlBankPayPayk).ToString()
                            + "," + Convert.ToInt32(ttlPay - ttlBankPayPayk).ToString()
                            /*+ "," + score*/;
            }
            return s;
        }

        public static Predicate<Composition> CompositionPredicate(Composition c)
        {
            return delegate (Composition comp)
            {
                return
                    comp.opts[(int)Options.options.OPTX].optAmt == c.opts[(int)Options.options.OPTX].optAmt &&
                    comp.opts[(int)Options.options.OPTX].optTime == c.opts[(int)Options.options.OPTX].optTime &&
                    comp.opts[(int)Options.options.OPTY].optAmt == c.opts[(int)Options.options.OPTY].optAmt &&
                    comp.opts[(int)Options.options.OPTY].optTime == c.opts[(int)Options.options.OPTY].optTime &&
                    comp.opts[(int)Options.options.OPTZ].optAmt == c.opts[(int)Options.options.OPTZ].optAmt &&
                    comp.opts[(int)Options.options.OPTZ].optTime == c.opts[(int)Options.options.OPTZ].optTime;
            };
        }

    }
}
