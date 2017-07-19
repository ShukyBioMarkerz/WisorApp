using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.Reporting
{
    public class Reporter
    {
        public static int LenderReport(ResultReportData reportData, String HTMLfilename, String PDFfilename, CultureInfo cultureInfo)
        {
            // debug the entire data correctness
            if (false) {
                LenderReportDebug(reportData);
            }

            LenderReport lr = new LenderReport(cultureInfo);

            if (HTMLfilename != null)
            {
                lr.GenerateLenderHtmlReport(HTMLfilename, reportData);
            }

            if (PDFfilename != null)
            {
                lr.GenerateLenderPdfReport(PDFfilename, reportData);
            }

            return 0;
        }

       
        public static int BorrowerReport(ResultReportData reportData, String HTMLfilename = null, String PDFfilename = null, CultureInfo cultureInfo = null)
        {
            BorrowerReport br = new BorrowerReport();

            if (HTMLfilename != null)
            {
                br.GenerateBorrowerHtmlReport(HTMLfilename, reportData);
            }

            if (PDFfilename != null)
            {
                br.GenerateBorrowerPdfReport(PDFfilename, reportData);
            }

            return 0;
        }

        public static void LenderReportDebug(ResultReportData reportData /* ,string filename,  
            bool shouldCreateHTML, bool shouldCreatePDF, CultureInfo cultureInfo = null */)
        {
            // get the report data
            Console.WriteLine(
                "\n\nBankName: " + reportData.BankName +
                " ID: " + reportData.ID +
                "\n Date: " + reportData.OriginalDateTaken +
                " Amount: " + reportData.OriginalLoanAmount +
                "\n Product: " + reportData.ProductName +
                " Time: " + reportData.OriginalTime +
                "\n Rate: " + reportData.OriginalRate +
                " PMT: " + reportData.FirstMonthlyPMT +
                "\n Income: " + reportData.YearlyIncome +
                " Paid: " + reportData.PayUntilToday +
                "\n Debt (PTI) : " + reportData.PTI +
                " Left: " + reportData.RemaingLoanAmount +
                "\n Future pay: " + reportData.EstimateFuturePay +
                " Original Margin: " + reportData.OriginalMargin +
                "\n Profit %: " + reportData.EstimateProfitPercantageSoFar + " , Profit: " + reportData.EstimateProfitSoFar +
                "\n Total Profit %: " + reportData.EstimateTotalProfitPercantage + " ,Total Profit: " + reportData.EstimateTotalProfit +
                "\n Future Profit %: " + reportData.EstimateFutureProfitPercantage + " ,Future Profit : " + reportData.EstimateFutureProfit + 
                " Proprty value: " + reportData.PropertyValue
                
                );

            // get the entire original loan' detail
            LoanList ll = reportData.theLoan.OriginalLoanDetaild;
            foreach (loanDetails ld in ll)
            {
                Console.WriteLine(
                    " Amount: " + ld.OriginalLoanAmount +
                    " Product: " + ld.ProductID.stringTypeId +
                    "\n Time: " + ld.OriginalTime +
                    " Rate: " + ld.OriginalRate +
                    "\n Tsamud: " + MiscUtilities.IsProductTsamud(ld.indices) +
                    " Monthly pay: " + ld.DesiredMonthlyPayment +
                    "\n Paid: " + ld.resultReportData.PayUntilToday +
                    " Future pay: " + ld.resultReportData.PayFuture +
                    " Estimate Future pay: " + ld.resultReportData.EstimateFuturePay +
                    "\n Profit %: " + ld.resultReportData.EstimateProfitPercantageSoFar + " , Profit: " + ld.resultReportData.EstimateProfitSoFar +
                    "\n Future Profit %: " + ld.resultReportData.EstimateFutureProfitPercantage + " ,Future Profit : " + ld.resultReportData.EstimateFutureProfit);
            }

            // get the products
            string[] products = reportData.GetProducts();
            GenericProduct gp;
            int profile = 1, minRateIndex = 0, maxRateIndex = MiscConstants.NumberOfYearsFrProduct;
            foreach (string p in products)
            {
                // TBD - Omri. which rate and margin should be get
                gp = GenericProduct.GetProductByName(p);
                double bankRateFrom = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, minRateIndex);
                double bankRateTo = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, maxRateIndex);
                double borrowerRateFrom = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, minRateIndex);
                double borrowerRateTo = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, maxRateIndex);
                Console.WriteLine(
                    " Product: " + p + " Rate: " + borrowerRateFrom + "-" + borrowerRateTo + 
                    " Margin: " + bankRateFrom + "-" + bankRateTo
                    );
            }

            // get the 3 compositions
            Composition[] compData = reportData.compositions;

            //// check how many real compositions we got
            //uint realCompCounter = MiscConstants.UNDEFINED_UINT;
            //bool isBestBorrower = false, isBestAllBorrower = false, isBestBank = false, isBestAllBank = false;
   
            //// prioritiez the compositions if there are enought...
            //for (int i = 0; i < compData.Length; i++)
            //{
            //    if (null != compData[i])
            //    {
            //        if (MiscConstants.BEST_BORROWER_COMPOSITION == compData[i].name)
            //            isBestBorrower = true;
            //        if (MiscConstants.BEST_BANK_COMPOSITION == compData[i].name)
            //            isBestBank = true;
            //        if (MiscConstants.BEST_ALL_PROFIT_COMPOSITION_BORROWER == compData[i].name)
            //            isBestAllBorrower = true;
            //        if (MiscConstants.BEST_ALL_PROFIT_COMPOSITION_BANK == compData[i].name)
            //            isBestAllBank = true;
            //        realCompCounter++;
            //    }
            //}

            for (int i = 0; i < compData.Length /*&& i < MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION*/; i++)
            {
                if (null == compData[i])
                    continue;

                //// TBD - check which compositions to choose, 
                //// depending on the results of win-win and the most beneficial to the both sides
                //if (MiscConstants.NUM_OF_PRODUCTS_IN_COMBINATION < realCompCounter)
                //{

                //}

                // ensure its a win-win composition
                if (! compData[i].IsWinWin)
                    continue;

                // calculate the relations between tzamud and not in all the products which consist the composition
                uint fix = MiscConstants.UNDEFINED_UINT, adjustable = MiscConstants.UNDEFINED_UINT;
 
                int ttlBankPay, ttlBorrowerPay, ttlProfit;
                MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                for (int j = 0; j < compData[i].opts.Length; j++)
                {
                    if (MiscUtilities.IsProductFix(compData[i].opts[j].product.fixOrAdjustable))
                        fix += (uint) compData[i].opts[j].optAmt;
                    else
                        adjustable += (uint) compData[i].opts[j].optAmt;

                    Console.WriteLine(
                        " Option: " + compData[i].opts[j].product.name +
                        " Amt: " + compData[i].opts[j].optAmt +
                        " Rate: " + compData[i].opts[j].optRateFirstPeriod +
                        "\n Tsamud: " + MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod).ToString() +
                        " Time: " + compData[i].opts[j].optTime +
                        "\n PMT: " + compData[i].opts[j].optPmt +
                        " TTLPay: " + compData[i].opts[j].optTtlPay +
                        "\n Lender profit: " + ttlProfit +
                        " Lender % profit: " + ((double)ttlProfit / reportData.RemaingLoanAmount * 100).ToString()
                    );
                    
                }

                // calculate the fix vs. adjustable numbers
                uint entireSum = fix + adjustable;
                uint fixNum = MiscConstants.UNDEFINED_UINT, adjustableNum = MiscConstants.UNDEFINED_UINT;
                if (0 >= entireSum)
                {
                    fixNum = (uint)(fix / entireSum) * 100;
                    adjustableNum = 100 - fixNum;
                }

                Console.WriteLine(
                    " Composition header fix : " + fixNum + " vs. adjustable: " + adjustableNum +
                    " Composition: " + compData[i].name +
                    "\n XBankTtlPay: " + compData[i].optXBankTtlPay +
                    " YBankTtlPay: " + compData[i].optYBankTtlPay +
                    " ZBankTtlPay: " + compData[i].optZBankTtlPay +
                    "\n ttlPay: " + compData[i].ttlPay +
                    " ttlPmt: " + compData[i].ttlPmt +
                    "\n borrower saving: " + (compData[i].ttlPay - reportData.PayFuture).ToString() +
                    " lender profit: " + ttlProfit
                );

            }

        }
    }
}
