using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WisorLibrary.Logic;
using WisorLibrary.ReportApplication;
using WisorLibrary.Utilities;

namespace WisorLib
{
    //[Serializable]
    // remove the Serializable for the sake of avoiding adding special cases for the setter functions
    // consider use "Data Contracts" instead
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

        // store the benefits
        public int BorrowerProfitCalc { get; set; }
        public int BankProfitCalc { get; set; }
        public int TotalBenefit { get; set; }
        public int BankOriginalProfit { get; set; }
        public bool IsWinWin { get; internal set; }

        // luch silukin
        public AmortisationData AmortisationData { get; set; }

        // for the sake of serialization.....
        public Composition()
        {
        }

        public Composition(Option optionX, Option optionY, Option optionZ, RunEnvironment env, string name)
        {
            this.name = name;
            opts[(int)Options.options.OPTX] = optionX;
            opts[(int)Options.options.OPTY] = optionY;

            if (MiscUtilities.Use3ProductsInComposition())
            {
                opts[(int)Options.options.OPTZ] = optionZ;
                ttlPmt = (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt + opts[(int)Options.options.OPTZ].optPmt);
            }
            else
            {
                opts[(int)Options.options.OPTZ] = null;
                ttlPmt = (opts[(int)Options.options.OPTX].optPmt + opts[(int)Options.options.OPTY].optPmt);
            }

            CheckIfTtlCalculatedOrNo(env);

            CalculateBankProfit(env);
        }

        private void CalculateBankProfit(RunEnvironment env)
        {
            if (MiscUtilities.Use3ProductsInComposition())
                Calculations.CalculateTheBankProfit(opts[(int)Options.options.OPTX], opts[(int)Options.options.OPTY],
                    opts[(int)Options.options.OPTZ], env.BorrowerProfile.profile);
            else
                Calculations.CalculateTheBankProfit(opts[(int)Options.options.OPTX], opts[(int)Options.options.OPTY],
                    null, env.BorrowerProfile.profile);

            optXBankTtlPay = opts[(int)Options.options.OPTX].CalculateLuahSilukinBank();
            optYBankTtlPay = opts[(int)Options.options.OPTY].CalculateLuahSilukinBank();
            if (MiscUtilities.Use3ProductsInComposition())
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
            if (MiscUtilities.Use3ProductsInComposition())
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

            if (MiscUtilities.Use3ProductsInComposition())
            {
                s += 
                "X:optType" + "," + "X:optAmt" + "," + "X:optTime" + "," + "X:RateFirstPeriod" + "," +
                "Y:optType" + "," + "Y:optAmt" + "," + "Y:optTime" + "," + "Y:RateFirstPeriod" + "," +
                "Z:optType" + "," + "Z:optAmt" + "," + "Z:optTime" + "," + "Z:RateFirstPeriod" + "," +
                "OPTXPmt" + "," + "OPTYPmt" + "," + "OPTZPmt" + "," + "ttlPmt" + "," +
                "OPTXTtlPay" + "," + "OPTYTtlPay" + "," + "OPTZTtlPay" + "," +
                // bank profit data
                "X:BankTtlPay" + "," + "Y:BankTtlPay" + "," + "Z:BankTtlPay" + "," +
                "ttlBorrowerPay" + "," + "TtlBankPay" + "," + "TtlBankProfit";
            }
            else
            {
               s +=
              "X:optType" + "," + "X:optAmt" + "," + "X:optTime" + "," + "X:RateFirstPeriod" + "," +
              "Y:optType" + "," + "Y:optAmt" + "," + "Y:optTime" + "," + "Y:RateFirstPeriod" + "," +
              "OPTXPmt" + "," + "OPTYPmt"  + "," + "ttlPmt" + "," +
              "OPTXTtlPay" + "," + "OPTYTtlPay" +  "," +
              // bank profit data
              "X:BankTtlPay" + "," + "Y:BankTtlPay"  + "," +
              "ttlBorrowerPay" + "," + "TtlBankPay" + "," + "TtlBankProfit";
            }

            return s;
        }

        public override string ToString()
        {
            int ttlBankPayPayk = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay);
            string s = MiscConstants.UNDEFINED_STRING;

            if (MiscUtilities.Use3ProductsInComposition())
            {
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
                                /*+ "," + score*/
                                /* is win-win */ + "," + IsWinWin;
                }
            }
            else
            {
                if (null != opts && null != opts[(int)Options.options.OPTX] && null != opts[(int)Options.options.OPTY])
                {
                    s = "" + opts[(int)Options.options.OPTX].ToString()
                            + "," + opts[(int)Options.options.OPTY].ToString()
                                + "," + (int)opts[(int)Options.options.OPTX].optPmt
                                + "," + (int)opts[(int)Options.options.OPTY].optPmt
                                + "," + (int)ttlPmt
                                + "," + (int)opts[(int)Options.options.OPTX].optTtlPay
                                + "," + (int)opts[(int)Options.options.OPTY].optTtlPay
                                // the bank profit data
                                + "," + Convert.ToInt32(optXBankTtlPay).ToString()
                                + "," + Convert.ToInt32(optYBankTtlPay).ToString()
                                + "," + (int)ttlPay
                                + "," + Convert.ToInt32(ttlBankPayPayk).ToString()
                                + "," + Convert.ToInt32(ttlPay - ttlBankPayPayk).ToString()
                                /*+ "," + score*/
                                /* is win-win */ + "," + IsWinWin;
                }

            }
            return s;
        }

        public static Predicate<Composition> CompositionPredicate(Composition c)
        {

            return delegate (Composition comp)
            {
                if (null == c)
                    return true; // avoid to add a null value

                if (MiscUtilities.Use3ProductsInComposition())
                {
                    return 
                    comp.opts[(int)Options.options.OPTX].optAmt == c.opts[(int)Options.options.OPTX].optAmt &&
                    comp.opts[(int)Options.options.OPTX].optTime == c.opts[(int)Options.options.OPTX].optTime &&
                    comp.opts[(int)Options.options.OPTY].optAmt == c.opts[(int)Options.options.OPTY].optAmt &&
                    comp.opts[(int)Options.options.OPTY].optTime == c.opts[(int)Options.options.OPTY].optTime &&
                    comp.opts[(int)Options.options.OPTZ].optAmt == c.opts[(int)Options.options.OPTZ].optAmt &&
                    comp.opts[(int)Options.options.OPTZ].optTime == c.opts[(int)Options.options.OPTZ].optTime;
                }
                else
                {
                    return
                    comp.opts[(int)Options.options.OPTX].optAmt == c.opts[(int)Options.options.OPTX].optAmt &&
                    comp.opts[(int)Options.options.OPTX].optTime == c.opts[(int)Options.options.OPTX].optTime &&
                    comp.opts[(int)Options.options.OPTY].optAmt == c.opts[(int)Options.options.OPTY].optAmt &&
                    comp.opts[(int)Options.options.OPTY].optTime == c.opts[(int)Options.options.OPTY].optTime;
                }
              };
        }

    }
}
