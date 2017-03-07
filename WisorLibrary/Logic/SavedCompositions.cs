using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.Options;

namespace WisorLib
{
    class SavedCompositions
    {
        // General Parameters
        private Option fixedOptZ = null;
        private SavedMatchesBeforeCheck savedListForCheck = null;

        // For checking total payment
        private double ttlPayForCheck = -1;
        private enum ttlPayRange { SMALLER, EQUAL, LARGER };


        // List of saved compositions after calculation
        //private Composition compositionToSave = null;
 
        RunEnvironment env;



        public SavedCompositions(Option fixedOptionZ, SavedMatchesBeforeCheck savedMatchesListForOnePlane,
            RunEnvironment env)
        {
            this.env = env;
            fixedOptZ = fixedOptionZ;
            savedListForCheck = savedMatchesListForOnePlane;
            
            //Get Total payment for Fixed OptionZ (using borrower rates)
            fixedOptZ.GetTtlPay();

            CheckMatchList(env);
        }



        // **************************************************************************************************************************** //
        // ********************************** Check one point if Total Pay is lower than lowest so far ******************************** //

        private int CheckTotalPay(double pmtForCheck)
        {
            if (ResultsOutput.bestCompositionSoFar == null)
            {
                return (int)ttlPayRange.SMALLER;
            }
            else
            {
                if (pmtForCheck < ResultsOutput.bestCompositionSoFar.ttlPay)
                {
                    return (int)ttlPayRange.SMALLER;
                }
                else if (pmtForCheck > ResultsOutput.bestCompositionSoFar.ttlPay)
                {
                    return (int)ttlPayRange.LARGER;
                }
                else
                {
                    return (int)ttlPayRange.EQUAL;
                }
            }
        }


        
        // **************************************************************************************************************************** //
        // ********************************** Check one point if Total Pay is lower than lowest so far ******************************** //

        public void CheckMatch(Option[] matchingPoint, RunEnvironment env)
        {
            // count the composition number
            Share.counterOfCompositions++;

            // 1 - Calculate total pay for Option X and Option Y of matching point for check

            Option tempOptX = matchingPoint[(int)Options.options.OPTX];
            Option tempOptY = matchingPoint[(int)Options.options.OPTY];
            tempOptX.GetTtlPay();
            tempOptY.GetTtlPay();
            ttlPayForCheck = tempOptX.optTtlPay + tempOptY.optTtlPay + fixedOptZ.optTtlPay;

            CalcTheBankProfit(tempOptX, tempOptY, fixedOptZ, env);
            
            // Print to file
            // tempOptX.toString, tempOptY.tostring, fixedOptZ.ToString, 
            // tempOptX.optTtlPay.toString, tempOptY.optTtlPay.tostring, fixedOptZ.optTtlPay.ToString,
            // ttlPayForCheck
            
            // 2 - Check total pay if smaller than the lowest so far
            int ttlPayChecker = CheckTotalPay(ttlPayForCheck);
            
            if (ttlPayChecker == (int)ttlPayRange.SMALLER)
            {
                ResultsOutput.bestCompositionSoFar = new Composition(matchingPoint[(int)Options.options.OPTX],
                                                                        matchingPoint[(int)Options.options.OPTY], fixedOptZ);
            }
        }





        // **************************************************************************************************************************** //
        // ***************************** Check list of saved matching points and save lowest total pay ******************************** //

        private void CheckMatchList(RunEnvironment env)
        {
            for (int i = 0; i < savedListForCheck.numOfMatches; i++)
            {
                CheckMatch(savedListForCheck.savedMatchesList[i].savedMatch, env);
            }        
        }

        private void CalcTheBankProfit(Option optX, Option optY, Option optZ, RunEnvironment env)
        {
            // Checking lender profit
            // get the Bank interset value
            double bankRate = Share.GetBankRate();
            optX.SetBankRate(bankRate);
            double optXBankTtlPay = optX.GetBankTtlPay();
            optY.SetBankRate(bankRate);
            double optYBankTtlPay = optY.GetBankTtlPay();
            optZ.SetBankRate(bankRate);
            double optZBankTtlPay = optZ.GetBankTtlPay();
            double ttlBankPayPayk = optX.optTtlPay + optY.optTtlPay + optZ.optTtlPay;
            double ttlPayForCheck = optX.optTtlPay + optY.optTtlPay + optZ.optTtlPay;

            // truncate the numbers
            string[] msg2write = {
                /*"X: " + */optX.ToString(),
                /*"Y: " + */optY.ToString(),
                /*"Z: " + */optZ.ToString(),
                /*"X_pmt: " +*/ Convert.ToInt32(optX.optPmt).ToString() + ":" +
                /*"Y_pmt: " +*/ Convert.ToInt32(optY.optPmt).ToString() + ":" +
                /*"Z_pmt: " +*/ Convert.ToInt32(optZ.optPmt).ToString() + "=" +
                /*"Ttl_pmt: " +*/ Convert.ToInt32(optX.optPmt + optY.optPmt + optZ.optPmt).ToString(),
                /*"X_Ttl: " +*/ Convert.ToInt32(optX.optTtlPay).ToString() + ":" +
                /*"Y_Ttl: " +*/ Convert.ToInt32(optY.optTtlPay).ToString() + ":" +
                /*"Z_Ttl: " +*/ Convert.ToInt32(optZ.optTtlPay).ToString(),
                /*"ttlPay: " +*/ Convert.ToInt32(ttlPayForCheck).ToString(),
                /*"X_TtlBank: " +*/ Convert.ToInt32(optXBankTtlPay).ToString() + ":" +
                /*"Y_TtlBank: " +*/ Convert.ToInt32(optYBankTtlPay).ToString() + ":" +
                /*"Z_TtlBank: " +*/ Convert.ToInt32(optZBankTtlPay).ToString(),
                /*"ttlPayBank: " +*/ Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay).ToString()
            };
            env.PrintLog2CSV(msg2write);

            //if (Share.shouldPrintResultsInList)
            //{
            //    (optType + 4).ToString() + "," + optAmt + "," + optTime + "," + optRateFirstPeriod;
            //    ttlPayForCheck: optXBankTtlPay + optYBankTtlPay + optZBankTtlPay
            //    string productNameX = GenericProduct.GetProductName(optX.optType);
            //    string resultX = productNameX + MiscConstants.DOTS_STR + optX.optAmt +
            //        MiscConstants.DOTS_STR + optX.optTime + MiscConstants.DOTS_STR + optX.optRateFirstPeriod;
            //    string productNameY = GenericProduct.GetProductName(optY.optType);
            //    string resultY = productNameY + MiscConstants.DOTS_STR + optY.optAmt +
            //        MiscConstants.DOTS_STR + optY.optTime + MiscConstants.DOTS_STR + optY.optRateFirstPeriod;
            //    string productNameZ = GenericProduct.GetProductName(optZ.optType);
            //    string resultZ = productNameZ + MiscConstants.DOTS_STR + optZ.optAmt +
            //        MiscConstants.DOTS_STR + optZ.optTime + MiscConstants.DOTS_STR + optZ.optRateFirstPeriod;
            //    string totalBorrower = Convert.ToInt32(ttlPayForCheck).ToString();
            //    string totalBank = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay).ToString();
            //    string diff = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay - ttlPayForCheck).ToString();

            //    add the result to the oredered list
            //   ChosenComposition comp = new ChosenComposition(optX, optY, optZ);
            //    comp.SetBorrowerPay(ttlPayForCheck);
            //    comp.SetBankPay(ttlBankPayPayk);

            //    env.listOfSelectedCompositions.Add(comp);
            //}
        }


    }

  
    public class ChosenComposition
    {
        Option[]        chosenCombination;
        public double   borrowerPay, bankPay;

        public ChosenComposition(Option combinationX, Option combinationY, Option combinationZ)
        {
            chosenCombination = new Option[3];
            chosenCombination[(int)options.OPTX] = combinationX;
            chosenCombination[(int)options.OPTY] = combinationY;
            chosenCombination[(int)options.OPTZ] = combinationZ;
        }

        public void SetBorrowerPay(double borrowerPay)
        {
            this.borrowerPay = borrowerPay;
        }

        public void SetBankPay(double bankPay)
        {
            this.bankPay = bankPay;
        }
    }
}
