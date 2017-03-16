using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WisorLibrary.Logic;
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
            Interlocked.Add(ref Share.SavedCompositionsCounter, 1);

            this.env = env;
            fixedOptZ = fixedOptionZ;
            savedListForCheck = savedMatchesListForOnePlane;
            
            //Get Total payment for Fixed OptionZ (using borrower rates)
            fixedOptZ.GetTtlPay(env);

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
             // 1 - Calculate total pay for Option X and Option Y of matching point for check

            Option tempOptX = matchingPoint[(int)Options.options.OPTX];
            Option tempOptY = matchingPoint[(int)Options.options.OPTY];
            tempOptX.GetTtlPay(env);
            tempOptY.GetTtlPay(env);
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
                                                                        matchingPoint[(int)Options.options.OPTY], fixedOptZ, env);
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
            if (0 < Share.numberOfPrintResultsInList)
            {
                // Checking lender profit
                // get the Bank interset value
                double bankRate = RateUtilities.GetBankRate(optX.product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)optX.optTime / 12 - 4);
                optX.SetBankRate(bankRate);
                double optXBankTtlPay = optX.GetBankTtlPay();
                bankRate = RateUtilities.GetBankRate(optY.product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)optY.optTime / 12 - 4);
                optY.SetBankRate(bankRate);
                double optYBankTtlPay = optY.GetBankTtlPay();
                bankRate = RateUtilities.GetBankRate(optZ.product.productID.numberID, BorrowerProfile.borrowerProfile,
                    (int)optZ.optTime / 12 - 4);
                optZ.SetBankRate(bankRate);
                double optZBankTtlPay = optZ.GetBankTtlPay();
                int ttlBankPayPayk = Convert.ToInt32(optXBankTtlPay + optYBankTtlPay + optZBankTtlPay);
                int ttlPayForCheck = Convert.ToInt32(optX.optTtlPay + optY.optTtlPay + optZ.optTtlPay);
                          
                string productNameX = GenericProduct.GetProductName(optX.optType);
                string resultX = productNameX + MiscConstants.DOTS_STR + optX.optAmt +
                    MiscConstants.DOTS_STR + optX.optTime + MiscConstants.DOTS_STR + optX.optRateFirstPeriod;
                string productNameY = GenericProduct.GetProductName(optY.optType);
                string resultY = productNameY + MiscConstants.DOTS_STR + optY.optAmt +
                    MiscConstants.DOTS_STR + optY.optTime + MiscConstants.DOTS_STR + optY.optRateFirstPeriod;
                string productNameZ = GenericProduct.GetProductName(optZ.optType);
                string resultZ = productNameZ + MiscConstants.DOTS_STR + optZ.optAmt +
                    MiscConstants.DOTS_STR + optZ.optTime + MiscConstants.DOTS_STR + optZ.optRateFirstPeriod;
                string totalBorrower = Convert.ToInt32(ttlPayForCheck).ToString();
                string totalBank = Convert.ToInt32(ttlBankPayPayk).ToString();
                string diff = Convert.ToInt32(ttlPayForCheck - ttlBankPayPayk).ToString();

                //add the result to the oredered list
                ChosenComposition comp = new ChosenComposition()
                { resultX, resultY, resultZ, totalBorrower, totalBank, diff };
                comp.SetBorrowerPay(ttlPayForCheck);
                comp.SetBankPay(ttlBankPayPayk);
                comp.SetBankProfit(Convert.ToInt32(ttlPayForCheck - ttlBankPayPayk));

                env.listOfSelectedCompositions.Add(comp);
                if (env.MaxProfit < Convert.ToInt32(diff))
                    env.MaxProfit = Convert.ToInt32(diff);
                if (env.MaxBankPay < Convert.ToInt32(totalBank))
                    env.MaxBankPay = Convert.ToInt32(totalBank);
                if (0 >= env.MinBorrowerPay)
                    env.MinBorrowerPay = Convert.ToInt32(totalBorrower);
                else if (env.MinBorrowerPay > Convert.ToInt32(totalBorrower))
                    env.MinBorrowerPay = Convert.ToInt32(totalBorrower);
        
                if (Share.ShouldStoreAllCombinations)
                {
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
                    /*"ttlPayBank: " +*/ Convert.ToInt32(ttlBankPayPayk).ToString()
                    };

                    env.PrintLog2CSV(msg2write);
                }

            }
        }


    }

  
    public class ChosenComposition : List<string>
    {
        //Option[]        chosenCombination;
        public double borrowerPay, bankPay, profit;

        //public ChosenComposition(Option combinationX, Option combinationY, Option combinationZ)
        //{
        //    chosenCombination = new Option[3];
        //    chosenCombination[(int)options.OPTX] = combinationX;
        //    chosenCombination[(int)options.OPTY] = combinationY;
        //    chosenCombination[(int)options.OPTZ] = combinationZ;
        //}

        public void SetBorrowerPay(double borrowerPay)
        {
            this.borrowerPay = borrowerPay;
        }

        public void SetBankPay(double bankPay)
        {
            this.bankPay = bankPay;
        }
        public void SetBankProfit(double profit)
        {
            this.profit = profit;
        }
    }
}
