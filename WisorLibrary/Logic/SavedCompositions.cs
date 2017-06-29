﻿using System;
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

        private int CheckTotalPay(double pmtForCheck, RunEnvironment env)
        {
            if (env.resultsOutput.bestCompositionSoFar == null)
            {
                return (int)ttlPayRange.SMALLER;
            }
            else
            {
                if (pmtForCheck < env.resultsOutput.bestCompositionSoFar.ttlPay)
                {
                    return (int)ttlPayRange.SMALLER;
                }
                else if (pmtForCheck > env.resultsOutput.bestCompositionSoFar.ttlPay)
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

            CalculateTheCompositionProfit(tempOptX, tempOptY, fixedOptZ, env);
            
            // 2 - Check total pay if smaller than the lowest so far
            int ttlPayChecker = CheckTotalPay(ttlPayForCheck, env);
            
            if (ttlPayChecker == (int)ttlPayRange.SMALLER)
            {
                env.resultsOutput.bestCompositionSoFar = new Composition(matchingPoint[(int)Options.options.OPTX],
                                   matchingPoint[(int)Options.options.OPTY], fixedOptZ, env,
                                   "bestCompositionSoFar");
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

        private void CalculateTheCompositionProfit(Option optX, Option optY, Option optZ, RunEnvironment env)
        {
            //if (0 < Share.numberOfPrintResultsInList || Share.ShouldStoreAllCombinations)
            //{
                // Checking lender profit
                // get the Bank interset value
                Calculations.CalculateTheBankProfit(optX, optY, optZ, env.BorrowerProfile.profile);

                double optXBankTtlPay = optX.CalculateLuahSilukinBank();
                double optYBankTtlPay = optY.CalculateLuahSilukinBank();
                double optZBankTtlPay = optZ.CalculateLuahSilukinBank();
                int ttlBankPayPayk = Convert.ToInt32(optX.CalculateLuahSilukinBank() + optY.CalculateLuahSilukinBank() + optZ.CalculateLuahSilukinBank());
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
                string totalBorrower = ttlPayForCheck.ToString();
                string totalBank = ttlBankPayPayk.ToString();
                string diff = (ttlPayForCheck - ttlBankPayPayk).ToString();

                // save it anyway in order to store the best from borrower and from the bank sides
                //if (0 < Share.numberOfPrintResultsInList)
                //{
                //add the result to the oredered list
                    if (1 < Share.numberOfPrintResultsInList)
                    {
                        ChosenComposition comp = new ChosenComposition()
                            { resultX, resultY, resultZ, totalBorrower, totalBank, diff };
                        comp.SetBorrowerPay(ttlPayForCheck);
                        comp.SetBankPay(ttlBankPayPayk);
                        comp.SetBankProfit(Convert.ToInt32(ttlPayForCheck - ttlBankPayPayk));
                
                        env.listOfSelectedCompositions.Add(comp);
                    }
            
                    if (env.MaxProfit < Convert.ToInt32(diff))
                    {
                        env.MaxProfit = Convert.ToInt32(diff);
                        env.bestDiffComposition = new Composition(optX, optY, optZ, env, MiscConstants.BEST_DIFF_COMPOSITION);
                    }
                    if (env.MaxBankPay < Convert.ToInt32(totalBank))
                    {
                        env.MaxBankPay = Convert.ToInt32(totalBank);
                        env.bestBankComposition = new Composition(optX, optY, optZ, env, MiscConstants.BEST_BANK_COMPOSITION);
                    }
                    if (0 >= env.MinBorrowerPay)
                    {
                        env.MinBorrowerPay = Convert.ToInt32(totalBorrower);
                        env.bestBorrowerComposition = new Composition(optX, optY, optZ, env, MiscConstants.BEST_BORROWER_COMPOSITION);
                    }
                    if (env.MinBorrowerPay > Convert.ToInt32(totalBorrower))
                    {
                        env.MinBorrowerPay = Convert.ToInt32(totalBorrower);
                        env.bestBorrowerComposition = new Composition(optX, optY, optZ, env, MiscConstants.BEST_BORROWER_COMPOSITION);
                    }
                //}
               
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

                    env.Logger.PrintLog2CSV(msg2write);
                }


            //}

        }


    }

  
    public class ChosenComposition : List<string>
    {
        //public Composition      chosenCombination;
        public double           borrowerPay, bankPay, profit;

        //public ChosenComposition(Option combinationX, Option combinationY, Option combinationZ)
        //{
        //    chosenCombination = new Option[3];
        //    chosenCombination[(int)options.OPTX] = combinationX;
        //    chosenCombination[(int)options.OPTY] = combinationY;
        //    chosenCombination[(int)options.OPTZ] = combinationZ;
        //}

        //public void AddOption(Option optX, Option optY, Option optZ, RunEnvironment env)
        //{
        //    chosenCombination = new Composition(optX, optY, optZ, env);
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

        //public override string ToString()
        //{
        //    return chosenCombination.ToString();
        //}
    }
}
