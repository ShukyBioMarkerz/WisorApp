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

namespace WisorLibrary.Reporting
{
    public class Reporter
    {
        public static int LenderReport(ResultReportData reportData, String HTMLfilename = null, String PDFfilename = null, CultureInfo cultureInfo = null)
        {
            LenderReport lr = new LenderReport();

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

        public static void LenderReportDebug(string filename, ResultReportData reportData, 
            bool shouldCreateHTML, bool shouldCreatePDF, CultureInfo cultureInfo = null)
        {
            // get the report data
            Console.WriteLine(
                "BankName: " + reportData.BankName +
                " ID: " + reportData.ID +
                " Date: " + reportData.DateTaken +
                " Amount: " + reportData.OriginalLoanAmount +
                " Product: " + reportData.ProductName +
                " Time: " + reportData.OriginalTime +
                " rate: " + reportData.OriginalRate +
                " PMT: " + reportData.FirstMonthlyPMT +
                " Income: " + reportData.YearlyIncome +
                " Paid: " + reportData.PayUntilToday +
                " Debt: " + reportData.PTI +
                " Left: " + reportData.RemaingLoanAmount +
                " Future: " + reportData.EstimateFuturePay +
                " Margin: " + reportData.OriginalMargin +
                " Profit: " + reportData.EstimateProfitPercantageSoFar + " , " + reportData.EstimateProfitSoFar + 
                " Total Profit: " + reportData.EstimateTotalProfitPercantage + " , " + reportData.EstimateTotalProfit +
                " Future Profit: " + reportData.EstimateFutureProfitPercantage + " , " + reportData.EstimateFutureProfit
                );

            // get the products
            string[] products = reportData.GetProducts();
            GenericProduct gp;
            int profile = 1, index = 0;
            foreach (string p in products)
            {
                // TBD - Omri. which rate and margin should be get
                gp = GenericProduct.GetProductByName(p);
                double bankRate = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, index);
                double borrowerRate = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, index);
                Console.WriteLine(
                    " Product: " + p + " Rate: " +  borrowerRate + " Margin: " + bankRate
                    );
            }

            // get the 3 compositions
            Composition[] compData = reportData.compositions;
            //CompositionReportData[] compData = reportData.GetCompositionData();
            for (int i = 0; i < compData.Length; i++)
            {
                if (null == compData[i])
                    continue;

                uint ttlBankPay, ttlBorrowerPay, ttlProfit;
                MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                Console.WriteLine(
                    " Composition: " + compData[i].name +
                    " XBankTtlPay: " + compData[i].optXBankTtlPay +
                    " YBankTtlPay: " + compData[i].optYBankTtlPay +
                    " ZBankTtlPay: " + compData[i].optZBankTtlPay +
                    " ttlPay: " + compData[i].ttlPay +
                    " ttlPmt: " + compData[i].ttlPmt +
                    " borrower saving: " + (compData[i].ttlPay - reportData.PayFuture).ToString() +
                    " lender profit: " + ttlProfit
                );

                //for (int j = 0; j < compData[i].optionReportData.Length; j++)
                for (int j = 0; j < compData[i].opts.Length; j++)
                {
                    Console.WriteLine(
                        " Option: " + compData[i].opts[j].product.name +
                        " Amt: " + compData[i].opts[j].optAmt +
                        " Rate: " + compData[i].opts[j].optRateFirstPeriod +
                        " Time: " + compData[i].opts[j].optTime +
                        " PMT: " + compData[i].opts[j].optPmt +
                        " TTLPay: " + compData[i].opts[j].optTtlPay +
                        " Lender profit: " + ttlProfit +
                        " Lender % profit: " + (ttlProfit / reportData.RemaingLoanAmount).ToString() 
                    );
                }
            }

        }
    }
}
