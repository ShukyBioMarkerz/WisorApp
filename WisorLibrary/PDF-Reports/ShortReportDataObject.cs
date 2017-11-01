using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;

namespace WisorLibrary.ReportApplication
{
    class ShortReportDataObject
    {

        public ShortReportDataObject(RunEnvironment Env, ResultReportData ReportData)
        {
            reportData = ReportData;
            env = Env;
        }

        private ResultReportData reportData { get; }
        private RunEnvironment env { get; }

        public int LenderTable1FuturePaymentTotal {
            get
            {
                if (null != reportData && null != reportData.compositions)
                    return reportData.compositions.Length;
                else
                    return 0;
            }
        }
        //public int LenderTable1BalanceToPayTotal { get { return 0; } }
        //public int LenderTable1PaySoFarTotal { get { return 0; } }
        //public int LenderTable1ReturnValueTotal { get { return 0; } }
        //public int LenderTable1AmountTotal { get { return 0; } }

        //public double LenderTable2ExpectedFuturePercentTotal {
        //    get
        //    {
        //        int sum = 0;
        //        for (int i = 0; i < NumberOfCompositions(); i++)
        //        {
        //            sum += LenderTable2ExpectedFutureTotal(i);
        //        }
        //        return sum;
        //    }
        //}

        public int LenderTable2TotalAmount(int index)
        {
            return GetCompositionTotalAmount(index);
        }

        public int LenderTable2TotalPMT(int index)
        {
            return GetCompositionTotalPMT(index);
        }

        public int LenderTable2TotalPMT2(int index)
        {
            return GetCompositionTotalPMT2(index);
        }

        public int LenderTable2TotalPayment(int index)
        {
            return GetCompositionTotalPayment(index);
        }

        public int LenderTable2ExpectedFutureTotal(int compositionIndex/*,out double EstimateFutureProfitPercantage*/)
        {
            double EstimateFutureProfitPercantage = MiscConstants.UNDEFINED_DOUBLE;
            return GetLenderExpectedFutureTotal(compositionIndex, out EstimateFutureProfitPercantage);
        }

        public int LenderTable2LenderProfitTotal(int index)
        {
            return GetCompositionLenderProfit(index);
        }

        public double LenderTable2LenderProfitPercantage(int index)
        {
            return GetCompositionLenderProfitPercantage(index);
        }

        //public double LenderTable2ProfitPercentTotal(int index)
        //{
        //    return 0;
        //} 
        public int LenderTable2ProfitTotal(int index)
        {
            return GetCompositionTotalAmount(index);
        }

        public int GetLenderProfit(int compositionIndex, int productIndex, out double LenderProfitPercantage)
        {
            int BankPay, BorrowerPay, Profit;
            Composition c = GetTheComposition(compositionIndex);
            MiscUtilities.CalcaulateProfitOfSpecificProduct(c, productIndex, out BankPay, out BorrowerPay, out Profit);
            LenderProfitPercantage = (double)Profit / reportData.RemaingLoanAmount * 100;
            return Profit;
        }
        public int GetLenderProfit(int compositionIndex, out double LenderProfitPercantage)
        {
            int BankPay, BorrowerPay, Profit;
            Composition c = GetTheComposition(compositionIndex);
            MiscUtilities.CalcaulateProfit(c, out BankPay, out BorrowerPay, out Profit);
            LenderProfitPercantage = (double)Profit / reportData.RemaingLoanAmount * 100;
            return Profit;
        }

        public string GetCompositionHeader(int compositionIndex)
        {
            string structureTypeString = MiscConstants.UNDEFINED_STRING;
            GenericProduct[] ProductsArray;
            int[] AmountArray;

            if (GetCompositionProducts(compositionIndex, out ProductsArray, out AmountArray))
            {
                MiscUtilities.CalculateTypeOfProducts3(ProductsArray, AmountArray, out structureTypeString);
            }

            return structureTypeString;
        }

        public string GetID()
        {
            if (null != reportData)
            {
                return reportData.ID + MiscConstants.NAME_SEP_CHAR + Share.CustomerName;
            }
            else
                return MiscConstants.UNDEFINED_STRING;

        }


        public OriginalLoanUKTableShort OriginalLoanUKTableShortValue { 
            get
            {
                // LoanList ll = env.theLoan.OriginalLoanDetaild;
                if (null != reportData)
                {
                    DateTime dateOfRateChange = reportData.OriginalDateTaken.AddMonths(reportData.firstTimePeriod);
                    OriginalLoanUKTableShort olus = new OriginalLoanUKTableShort(reportData.OriginalDateTaken, (int) reportData.OriginalLoanAmount,
                        reportData.ProductName, (int)reportData.OriginalTime, reportData.OriginalRate * 100, reportData.OriginalRate2 * 100,
                        dateOfRateChange, (int)reportData.FirstMonthlyPMT, (int)reportData.FirstMonthlyPMT2, (int)reportData.PayUntilToday,
                        (int)reportData.RemaingLoanAmount, (int)reportData.PayFuture, (int)reportData.YearlyIncome, reportData.PTI * 100,
                        reportData.PrintedOriginalMargin * 100, reportData.PrintedOriginalMargin2 * 100, reportData.EstimateProfitPercantageSoFar * 100,
                        (int)reportData.EstimateProfitSoFar, reportData.EstimateTotalProfitPercantage * 100,
                        (int)reportData.EstimateTotalProfit, (int)reportData.EstimateFutureProfit, reportData.EstimateFutureProfitPercantage * 100);
                    return olus;
                }
                else
                {
                    return new OriginalLoanUKTableShort(DateTime.Today, 0, "Fixed 5 years", 240, 3.875, 5.390, DateTime.Today, 2907, 3403, 43606, 464408, 644756, 11100, 26.19, 1.25, 1.85, 1.541, 7474, 15.216, 73796, 66322, 13.675);
                }
            }
        }

        public void GetTheProfitData()
        {

        }

        public OriginalLoanTableShort[] ShortOriginalLoanTableValues
        {
            get {
                // shuky: should return the original loan details...
                if (null != env && null != reportData) {
                    List<OriginalLoanTableShort> lolt4s = new List<OriginalLoanTableShort>();
                    LoanList ll = env.theLoan.OriginalLoanDetaild;
                    for (int i = 0; i < ll.Count; i++)
                    {
                        OriginalLoanTableShort olt4s = new OriginalLoanTableShort(
                                (int)ll[i].OriginalLoanAmount,
                                Share.cultureInfo.Name.Equals("he-IL") ? MiscUtilities.GetProductHebrewName(ll[i].ProductID.stringTypeId) : ll[i].ProductID.stringTypeId,
                                ll[i].OriginalRate,
                                (int)ll[i].OriginalTime, MiscUtilities.IsProductTsamud(ll[i].indicesFirstTimePeriod),
                                (int)ll[i].DesiredMonthlyPayment,
                                (int)ll[i].resultReportData.PayUntilToday, (int)ll[i].resultReportData.RemaingLoanAmount, (int)ll[i].resultReportData.PayFuture,
                                (int) ll[i].resultReportData.EstimateProfitSoFar, ll[i].resultReportData.EstimateProfitPercantageSoFar,
                                (int) ll[i].resultReportData.EstimateFutureProfit, ll[i].resultReportData.EstimateFutureProfitPercantage);
                        lolt4s.Add(olt4s);
                    }
                    return lolt4s.ToArray();

                    //reportData.compositions[i].opts[j].product.name,
                    //        reportData.compositions[i].opts[j].optRateFirstPeriod, (int)reportData.compositions[i].opts[j].optTime,
                    //        MiscUtilities.IsProductTsamud(reportData.compositions[i].opts[j].product.originalIndexUsedFirstTimePeriod),
                    //        (int)reportData.compositions[i].opts[j].optPmt,
                    //        (int)reportData.compositions[i].opts[j].optTtlPay, balance, futurePayment);
                    //lolt4s.Add(olt4s);

                }
                else
                    return null;
            }

            //get
            //{
            //    return GetTheCompositionData();
            //}
        }

        public OriginalLoanTableShort[] GetTheCompositionData()
        {
            if (null != reportData && null != reportData.compositions)
            {
                List<OriginalLoanTableShort> lolt4s = new List<OriginalLoanTableShort>();
                int futurePayment = 0, balance = 0; // TBD - where are those taken

                for (int i = 0; i < reportData.compositions.Length; i++)
                {
                    for (int j = 0; j < reportData.compositions[i].opts.Length; j++)
                    {
                        if (null == reportData.compositions[i].opts[j])
                            continue;
                        OriginalLoanTableShort olt4s = new OriginalLoanTableShort(
                            (int)reportData.compositions[i].opts[j].optAmt, reportData.compositions[i].opts[j].product.name,
                            reportData.compositions[i].opts[j].optRateFirstPeriod, (int)reportData.compositions[i].opts[j].optTime,
                            MiscUtilities.IsProductTsamud(reportData.compositions[i].opts[j].product.originalIndexUsedFirstTimePeriod),
                            (int)reportData.compositions[i].opts[j].optPmt,
                            (int)reportData.compositions[i].opts[j].optTtlPay, balance, futurePayment);
                        lolt4s.Add(olt4s);
                    }
                }
                return lolt4s.ToArray();
            }
            else
                return null;
        }

        public OriginalLoanTable4Short[] ShortOriginalLoanTable4Values
        {
            get
            {
                if (null != reportData && null != reportData.compositions)
                {
                    List<OriginalLoanTable4Short> lolt4s = new List<OriginalLoanTable4Short>();

                    for (int i = 0; i < reportData.compositions.Length; i++)
                    {
                        for (int j = 0; j < reportData.compositions[i].opts.Length; j++)
                        {
                            if (null == reportData.compositions[i].opts[j])
                                continue;
                            OriginalLoanTable4Short olt4s = new OriginalLoanTable4Short(
                                (int) reportData.compositions[i].opts[j].optAmt, reportData.compositions[i].opts[j].product.name,
                                (int) reportData.compositions[i].opts[j].optTime, reportData.compositions[i].opts[j].optRateFirstPeriod,
                                MiscUtilities.IsProductTsamud(reportData.compositions[i].opts[j].product.originalIndexUsedFirstTimePeriod), 
                                (int) reportData.compositions[i].opts[j].optPmt,
                                (int)reportData.compositions[i].opts[j].optTtlPay);
                            lolt4s.Add(olt4s);
                        }
                    }
                    return lolt4s.ToArray();
                }
                else
                {

                    return new OriginalLoanTable4Short[] {
                        new OriginalLoanTable4Short(0, "Prime", 300, 1.37, false, 2214, 797122),
                        new OriginalLoanTable4Short(0, "Fixed Tsamud", 120, 3.37, true, 8205, 1009505),
                        new OriginalLoanTable4Short(0, "Alternate 60 months ", 360, 3.37, true, 2772, 1076421)
                    };
                }
            }
        }


        // ***
        // new code
        // ***
        public OriginalLoanTable4ShortUK[] ShortOriginalLoanTable4ValuesUK_OLDCODE
        {
            get
            {
                if (null != reportData && null != reportData.compositions)
                {
                    List<OriginalLoanTable4ShortUK> lolt4s = new List<OriginalLoanTable4ShortUK>();

                    for (int i = 0; i < reportData.compositions.Length; i++)
                    {
                        for (int j = 0; j < reportData.compositions[i].opts.Length; j++)
                        {
                            if (null == reportData.compositions[i].opts[j])
                                continue;
 
                            OriginalLoanTable4ShortUK olt4s = new OriginalLoanTable4ShortUK(
                                (int)reportData.compositions[i].opts[j].optAmt, reportData.compositions[i].opts[j].product.name,
                                (int)reportData.compositions[i].opts[j].optTime, reportData.compositions[i].opts[j].optRateFirstPeriod,
                                reportData.compositions[i].opts[j].optRateSecondPeriod, (int)reportData.compositions[i].opts[j].optPmt,
                                (int) reportData.compositions[i].opts[j].optTtlPay,
                                (int)reportData.compositions[i].opts[j].optTtlPay);
                            lolt4s.Add(olt4s);
                        }
                    }
                    return lolt4s.ToArray();
                }
                else
                {
                        return new OriginalLoanTable4ShortUK[] {
                        new OriginalLoanTable4ShortUK(0, "Prime1", 300, 1.37, 1.21, 7100, 2214, 797122),
                        new OriginalLoanTable4ShortUK(0, "Fixed1 Tsamud", 120, 3.37, 3.05, 4500, 8205, 1009505),
                        new OriginalLoanTable4ShortUK(0, "Alternate1 60 months ", 360, 3.37, 2.45, 2112, 2772, 1076421)
                    };
                }
            }
        }

        public int NumberOfCompositions()
        {
            int count = 0;
            if (null != reportData && null != reportData.compositions)
            {
                for (int i = 0; i < reportData.compositions.Length; i++)
                {
                    if (null != reportData.compositions[i] && null != reportData.compositions[i].opts)
                        count++;
                }
            }

            return count;
        }

        public int NumberOfWinWinCompositions()
        {
            int count = 0;
         
            for (int i = 0; i < NumberOfCompositions(); i++)
            {
                if (null != reportData.compositions[i] && null != reportData.compositions[i].opts && reportData.compositions[i].IsWinWin)
                    count++;
            }
 
            return count;
        }

        public int NumberOfProductsInCompositions(int compositionIndex)
        {
            if (null != reportData && null != reportData.compositions)
            {
                OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
                if (null != olt4s)
                {
                    return olt4s.Length;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public OriginalLoanTable4ShortUK GetProductInComposition(int compositionIndex, int productIndex)
        {
            OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
            if (null != olt4s && olt4s.Length >= productIndex)
            {
                return olt4s[productIndex];
            }
            else
                return null;
        }

        public OriginalLoanTable4Short GetProductInCompositionUSandIL(int compositionIndex, int productIndex)
        {
            OriginalLoanTable4Short[] olt4s = GetCompositionUSandIL(compositionIndex);
            if (null != olt4s && olt4s.Length >= productIndex)
            {
                return olt4s[productIndex];
            }
            else
                return null;
        }

        public OriginalLoanTable4Short GetProductSummaryInCompositionUSandIL(int compositionIndex)
        {
            OriginalLoanTable4Short olt4s = GetCompositionSummaryUSandIL(compositionIndex);
            return olt4s;
        }

        public int GetCompositionTotalAmount(int compositionIndex)
        {
            OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
            int sum = 0;
            if (null != olt4s)
            {
                for (int i = 0; i < olt4s.Length; i++)
                {
                    sum += olt4s[i].Amount;
                }
            }
            return sum;
        }

        public int GetCompositionTotalPMT(int compositionIndex)
        {
            OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
            int sum = 0;
            if (null != olt4s)
            {
                for (int i = 0; i < olt4s.Length; i++)
                {
                    sum += olt4s[i].MonthlyPMT;
                }
            }
            return sum;
        }

        public int GetCompositionTotalPMT2(int compositionIndex)
        {
            OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
            int sum = 0;
            if (null != olt4s)
            {
                for (int i = 0; i < olt4s.Length; i++)
                {
                    sum += olt4s[i].MonthlyPMT;
                }
            }
            return sum;
        }

        public int GetCompositionTotalPayment(int compositionIndex)
        {
            OriginalLoanTable4ShortUK[] olt4s = GetComposition(compositionIndex);
            int sum = 0;
            if (null != olt4s)
            {
                for (int i = 0; i < olt4s.Length; i++)
                {
                    sum += olt4s[i].TotalPayment;
                }
            }
            return sum;
        }

        public int BorrwerCanSave(int compositionIndex, out double borrowerCanSavePercantage)
        {
            int borrowerCanSave = 0;
            borrowerCanSavePercantage = 0;
            Composition c = GetTheComposition(compositionIndex);
            if (null != c)
            {
                borrowerCanSave = c.BorrowerProfitCalc;
                borrowerCanSavePercantage = Math.Round((double)c.BorrowerProfitCalc / reportData.RemaingLoanAmount * 100, 2);
            }

            return borrowerCanSave;
        }

        public int LenderCanIncrease(int compositionIndex, out double LenderCanIncreasePercantage)
        {
            int LenderCanIncrease = 0;
            LenderCanIncreasePercantage = 0;
            Composition c = GetTheComposition(compositionIndex);
            if (null != c)
            {
                LenderCanIncrease = c.BankProfitCalc;
                LenderCanIncreasePercantage = Math.Round((double)c.BankProfitCalc / reportData.EstimateFutureProfit * 100, 2);
            }

            return LenderCanIncrease;
        }

        public int GetLenderExpectedFutureTotal(int compositionIndex, out double EstimateFutureProfitPercantage)
        {
            int EstimateFutureProfit = MiscConstants.UNDEFINED_INT;
            EstimateFutureProfitPercantage = MiscConstants.UNDEFINED_DOUBLE;

            if (null != reportData)
            {
                EstimateFutureProfit = (int) reportData.EstimateFutureProfit;
                EstimateFutureProfitPercantage = reportData.EstimateFutureProfitPercantage;
            }

            return EstimateFutureProfit;
        }

        public int GetCompositionLenderProfit(int compositionIndex)
        {
            double lenderProfitPercantage;
            int profit = GetLenderProfit(compositionIndex, out lenderProfitPercantage);
            //int profit = 0;
            //if (null != reportData && null != reportData.compositions)
            //{
            //    profit = reportData.compositions[compositionIndex].BorrowerProfitCalc;
            //}
            return profit;
        }

        public int GetCompositionLenderProfit(int compositionIndex, int productIndex)
        {
            double lenderProfitPercantage;
            int profit = GetLenderProfit(compositionIndex, productIndex, out lenderProfitPercantage);
            //int profit = 0;
            //if (null != reportData && null != reportData.compositions)
            //{
            //    profit = reportData.compositions[compositionIndex].BorrowerProfitCalc;
            //}
            return profit;
        }

        public double GetCompositionLenderProfitPercantage(int compositionIndex)
        {
            double lenderProfitPercantage;
            int profit = GetLenderProfit(compositionIndex, out lenderProfitPercantage);
            return lenderProfitPercantage;
        }

        public double GetCompositionLenderProfitPercantage(int compositionIndex, int productIndex)
        {
            double lenderProfitPercantage;
            int profit = GetLenderProfit(compositionIndex, productIndex, out lenderProfitPercantage);
            return lenderProfitPercantage;

            //double sum = 0;
            //int TotalBenefit = 0;
            //if (null != reportData && null != reportData.compositions)
            //{
            //    sum = reportData.compositions[compositionIndex].BorrowerProfitCalc;
            //    TotalBenefit = reportData.compositions[compositionIndex].TotalBenefit;
            //}
            //return sum / TotalBenefit * 100;
        }

        public Composition GetTheComposition(int index)
        {
            Composition c = null;

            if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                  null != reportData.compositions[index].opts)
            {
                c = reportData.compositions[index];
            }

            return c;
        }

        public bool GetCompositionProducts(int index, out GenericProduct[] ProductsArray, out int[] AmountArray)
        {
            List<GenericProduct> lgp = new List<GenericProduct>();
            List<int> lamount = new List<int>();
            bool rc = false;
            ProductsArray =  null;
            AmountArray = null;

            if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                  null != reportData.compositions[index].opts)
            {
                Composition c = reportData.compositions[index];

                for (int j = 0; j < c.opts.Length; j++)
                {
                    if (null == c.opts[j])
                        continue;
                    lgp.Add(c.opts[j].product);
                    lamount.Add((int) c.opts[j].optAmt);
                }

                ProductsArray = lgp.ToArray();
                AmountArray = lamount.ToArray();
                rc = true;
            }

            return rc;
        }

        public OriginalLoanTable4ShortUK[] GetComposition(int index)
        {
            if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                    null != reportData.compositions[index].opts)
            {
                List<OriginalLoanTable4ShortUK> lolt4s = new List<OriginalLoanTable4ShortUK>();
                Composition c = reportData.compositions[index];

                for (int j = 0; j < c.opts.Length; j++)
                {
                    if (null == c.opts[j])
                        continue;

                     OriginalLoanTable4ShortUK olt4s = new OriginalLoanTable4ShortUK(
                        (int)c.opts[j].optAmt, c.opts[j].product.name,
                        (int)c.opts[j].optTime, c.opts[j].optRateFirstPeriod * 100,  c.opts[j].optRateSecondPeriod * 100, 
                        (int)c.opts[j].optPmt, (int)c.opts[j].optPmt2, 
                        (int)c.opts[j].optTtlPay);
                    lolt4s.Add(olt4s);
                }

                return lolt4s.ToArray();
            }
            else
                return null;
        }

        public OriginalLoanTable4Short[] GetCompositionUSandIL(int index)
        {
            if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                    null != reportData.compositions[index].opts)
            {
                List<OriginalLoanTable4Short> lolt4s = new List<OriginalLoanTable4Short>();
                Composition c = reportData.compositions[index];

                for (int j = 0; j < c.opts.Length; j++)
                {
                    if (null == c.opts[j])
                        continue;
                     OriginalLoanTable4Short olt4s = new OriginalLoanTable4Short(
                       (int)c.opts[j].optAmt, 
                       MiscUtilities.GetProductHebrewName(c.opts[j].product),
                       (int)c.opts[j].optTime, 
                       c.opts[j].optRateFirstPeriod,
                       MiscUtilities.IsProductTsamud(c.opts[j].product.originalIndexUsedFirstTimePeriod),
                       (int)c.opts[j].optPmt,
                       (int)c.opts[j].optTtlPay);
                    lolt4s.Add(olt4s);
                }

                return lolt4s.ToArray();
            }
            else
                return null;
        }

        public OriginalLoanTable4Short GetCompositionSummaryUSandIL(int index)
        {
            if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                    null != reportData.compositions[index].opts)
            {
                Composition c = reportData.compositions[index];
                OriginalLoanTable4Short olt4s = null;
                int optAmt, optPmt, optTtlPay;
                optAmt = optPmt = optTtlPay = 0;
                for (int j = 0; j < c.opts.Length; j++)
                {
                    if (null == c.opts[j])
                        continue;
                    optAmt += (int)c.opts[j].optAmt;
                    optPmt += (int)c.opts[j].optPmt;
                    optTtlPay += (int)c.opts[j].optTtlPay;
                }
                olt4s = new OriginalLoanTable4Short(
                      optAmt,
                      MiscConstants.UNDEFINED_STRING,
                      MiscConstants.UNDEFINED_INT,
                      MiscConstants.UNDEFINED_DOUBLE,
                      MiscUtilities.IsProductTsamud(c.opts[0].product.originalIndexUsedFirstTimePeriod),
                      optPmt,
                      optTtlPay);
                return olt4s;
            }
            else
                return null;
        }

        //
        //

        //public OriginalLoanTable2Short[] ShortOriginalLoanTable2Values
        //{
        //    get
        //    {
        //        if (null != reportData)
        //        {
        //            List<OriginalLoanTable2Short> lolts = new List<OriginalLoanTable2Short>();
        //            lolts.Add(new OriginalLoanTable2Short((int) reportData.EstimateProfitSoFar, reportData.EstimateProfitPercantageSoFar,
        //                (int) reportData.EstimateFutureProfit, reportData.EstimateFutureProfitPercantage));
        //            return lolts.ToArray();
        //        }
        //        else
        //        {
        //            return new OriginalLoanTable2Short[] {
        //                new OriginalLoanTable2Short(0, 2.383, 0, 11.694),
        //                new OriginalLoanTable2Short(0, 0.838, 0, 4.111)
        //            };
        //        }
        //    }
        //}

        // get the "products used in analysis"
        public ProductsUsedInAnalysisTableShort[] ShortProductsUsedInAnalysisTableValues
        {
            get
            {
                List<ProductsUsedInAnalysisTableShort> lpuiats = new List<ProductsUsedInAnalysisTableShort>();
                ProductsUsedInAnalysisTableShortUK[] puiats = ShortProductsUsedInAnalysisTableValuesUK;
                for (int i = 0; i < puiats.Length; i++)
                {
                    lpuiats.Add(new ProductsUsedInAnalysisTableShort(puiats[i].Margin, puiats[i].Rate, puiats[i].Product));
                }

                return lpuiats.ToArray();

                //return new ProductsUsedInAnalysisTableShort[] {
                //    new ProductsUsedInAnalysisTableShort(0, 11.694, "Fixed Tsamud"),
                //    new ProductsUsedInAnalysisTableShort(0, 21.694, "Fixed No Tsamud"),
                //    new ProductsUsedInAnalysisTableShort(0, 15.694, "Alternate 60 months"),
                //    new ProductsUsedInAnalysisTableShort(0, 8.694, "Alternate 60 months"),
                //    new ProductsUsedInAnalysisTableShort(0, 3.694, "Prime"),
                //};
            }
        }

        // ***
        // new code
        public ProductsUsedInAnalysisTableShortUK[] ShortProductsUsedInAnalysisTableValuesUK
        {
            get
            {
                if (null != reportData) {
                    string[] products = reportData.GetProducts();
                    GenericProduct gp;
                    int profile = 1, minRateIndex = 0, maxRateIndex = MiscConstants.NumberOfYearsFrProduct - 1;
                    List<ProductsUsedInAnalysisTableShortUK> lopuiats = new List<ProductsUsedInAnalysisTableShortUK>();
                    foreach (string p in products)
                    {
                        gp = GenericProduct.GetProductByName(p);
                        if (null == gp)
                        {
                            gp = GenericProduct.GetProductFromAllListByName(p);
                        }
                        if (null == gp)
                        {
                            WindowsUtilities.loggerMethod("ERROR: ShortProductsUsedInAnalysisTableValuesUK unrecognized product: " + p);
                            continue;
                        }

                        string productName = gp.name;
                        if (Share.cultureInfo.Name.Equals("he-IL"))
                        {
                            productName = (null != gp.hebrewName) ? gp.hebrewName : productName;
                        }
                    
                        double bankRate1 = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, maxRateIndex);
                        double borrowerRate1 = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, maxRateIndex);
                        
                        bankRate1 = borrowerRate1 + bankRate1;
                        // TBD: Omri, where should we get the lower and the higher rate && margin values
                        double bankRate2 = bankRate1;
                        double borrowerRate2 = borrowerRate1;
                        if (MiscConstants.markets.UK == Share.theMarket)
                        {
                            bankRate2 = RateUtilities.Instance.GetBankRateSecondPeriod(gp.productID.numberID, profile, maxRateIndex);
                            borrowerRate2 = RateUtilities.Instance.GetBorrowerRateSecondPeriod(gp.productID.numberID, profile, minRateIndex);
                            bankRate2 = borrowerRate2 + bankRate2;
                        }

                        ProductsUsedInAnalysisTableShortUK puiats = new ProductsUsedInAnalysisTableShortUK(
                            bankRate1 * 100, borrowerRate1 * 100, bankRate2 * 100, borrowerRate2 * 100, productName);
                        lopuiats.Add(puiats);
                    }
                    return lopuiats.ToArray();

                }
                else {
                    return new ProductsUsedInAnalysisTableShortUK[] {
                        new ProductsUsedInAnalysisTableShortUK(0, 11.694, 6.393, 10.694, "Fixed Tsamud"),
                        new ProductsUsedInAnalysisTableShortUK(0, 21.694, 9.783, 2.694, "Fixed No Tsamud"),
                        new ProductsUsedInAnalysisTableShortUK(0, 15.694, 8.873, 5.694, "Alternate 60 months"),
                        new ProductsUsedInAnalysisTableShortUK(0, 8.694, 1.283, 28.694, "Alternate 60 months"),
                        new ProductsUsedInAnalysisTableShortUK(0, 3.694, 7.483, 31.694, "Prime"),
                    };
                }
            }
        }


        public int GetPropertyValue()
        {
            int PropertyValue = 0;

            if (null != reportData)
            {
                PropertyValue = (int)reportData.PropertyValue;
            }

            return PropertyValue;
        }

        public DateTime GetDate()
        {
            DateTime Date = DateTime.Now;

            if (null != reportData)
            {
                Date = reportData.OriginalDateTaken;
            }

            return Date;
        }

        public int GetIncome()
        {
            int income = 0;

            if (null != reportData)
            {
                income = (int)reportData.YearlyIncome;
            }

            return income;
        }

        public AmortisationData GetAmortisationData(int index)
        {
             if (null != reportData && null != reportData.compositions && null != reportData.compositions[index] &&
                null != reportData.compositions[index].opts)
            {
                Composition c = reportData.compositions[index];
                return c.AmortisationData;
            }
            else
            {
                return null;
            }
        }
      

    }

    class OriginalLoanTableShort
    {
        public int Amount { get; set; }
        public string Product { get; set; }
        public double Rate { get; set; }
        public int Time { get; set; }
        public bool Indexed { get; set; }
        public int ReturnValue { get; set; }
        public int PaySoFar { get; set; }
        public int Balance { get; set; }
        public int FuturePayment { get; set; }
        public int EstimateProfitSoFar { get; set; }
        public int EstimateFutureProfit { get; set; }
        public double EstimateProfitPercantageSoFar { get; set; }
        public double EstimateFutureProfitPercantage { get; set; }

        public OriginalLoanTableShort(int amount, string product, double rate, int time, bool indexed, int returnValue, 
            int paySoFar, int balance, int futurePayment,
            int estimateProfitSoFar = 0, double estimateProfitPercantageSoFar = 0, 
            int estimateFutureProfit = 0, double estimateFutureProfitPercantage = 0)
        {
            Amount = amount;
            Product = product;
            Rate = rate;
            Time = time;
            Indexed = indexed;
            ReturnValue = returnValue;
            PaySoFar = paySoFar;
            Balance = balance;
            FuturePayment = futurePayment;
            EstimateProfitSoFar = estimateProfitSoFar;
            EstimateProfitPercantageSoFar = estimateProfitPercantageSoFar;
            EstimateFutureProfit = estimateFutureProfit;
            EstimateFutureProfitPercantage = estimateFutureProfitPercantage;
        }
    }


    class OriginalLoanUKTableShort
    {
        public DateTime DateTaken { get; set; }
        public int Amount { get; set; }
        public string Product { get; set; }
        public int Term { get; set; }
        public double InitialRate { get; set; }
        public double FollowOnRate { get; set; }
        public DateTime DateOfRateChange { get; set; }
        public int FirstPMT { get; set; }
        public int FollowOnPMT { get; set; }
        public int PaidUntilToday { get; set; }
        public int LeftToPay { get; set; }
        public int FuturePayment { get; set; }
        public int TotalIncome { get; set; }
        public double PTI { get; set; }
        public double InitialMargin { get; set; }
        public double FollowUpMargin { get; set; }
        public double EstimatePercentProfit { get; set; }
        public int EstimateProfit { get; set; }
        public double EstimateTotalPercentProfit { get; set; }
        public int EstimateTotalProfit { get; set; }
        public int EstimateFutureProfit { get; set; }
        public double EstimateFuturePercentProfit { get; set; }

        public OriginalLoanUKTableShort(DateTime dateTaken, int amount, string product, int term, double initialRate, double followOnRate,
            DateTime dateOfRateChange, int firstPMT, int followOnPmt, int paidUntilToday, int leftToPay, int futurePayment, int totalIncome, 
            double pti, double initialMargin, double followUpMargin, double estimatePercentProfit, int estimateProfit, double estimateTotalPercentProfit,
            int estimateTotalProfit, int estimateFutureProfit , double estimateFuturePercentProfit)
        {
            DateTaken = dateTaken;
            Amount = amount;
            Product = product;
            InitialRate = initialRate;
            Term = term;
            FollowOnRate = followOnRate;
            DateOfRateChange = dateOfRateChange;
            FirstPMT = firstPMT;
            FollowOnPMT = followOnPmt;
            PaidUntilToday = paidUntilToday;
            LeftToPay = leftToPay;
            FuturePayment = futurePayment;
            TotalIncome = totalIncome;
            PTI = pti;
            InitialMargin = initialMargin;
            FollowUpMargin = followUpMargin;
            EstimatePercentProfit = estimatePercentProfit;
            EstimateProfit = estimateProfit;
            EstimateTotalPercentProfit = estimateTotalPercentProfit;
            EstimateTotalProfit = estimateTotalProfit;
            EstimateFutureProfit = estimateFutureProfit;
            EstimateFuturePercentProfit = estimateFuturePercentProfit;
        }
    }

    class OriginalLoanTable2Short
    {
        public int Profit { get; set; }
        public double ProfitPercent { get; set; }
        public int ExpectedProfit { get; set; }
        public double ExpectedProfitPercent { get; set; }

        public OriginalLoanTable2Short(int profit, double profitPercent, int expectedProfit, double expectedProfitPercent)
        {
            Profit = profit;
            ProfitPercent = profitPercent;
            ExpectedProfit = expectedProfit;
            ExpectedProfitPercent = expectedProfitPercent;
        }
    }

    class ProductsUsedInAnalysisTableShort
    {
        public double Margin { get; set; }
        public double Rate { get; set; }
        public string Product { get; set; }

        public ProductsUsedInAnalysisTableShort(double margin, double rate, string product)
        {
            Margin = margin;
            Rate = rate;
            Product = product;
        }
    }

    class ProductsUsedInAnalysisTableShortUK
    {
        public double Margin { get; set; }
        public double Rate { get; set; }
        public double Margin2 { get; set; }
        public double Rate2 { get; set; }
        public string Product { get; set; }

        public ProductsUsedInAnalysisTableShortUK(double margin, double rate, double margin2, double rate2,  string product)
        {
            Margin = margin;
            Rate = rate;
            Margin2 = margin2;
            Rate2 = rate2;
            Product = product;
        }
    }

    class OriginalLoanTable4Short
    {
        public int Amount { get; set; }
        public string Product { get; set; }
        public double Rate { get; set; }
        public int Time { get; set; }
        public bool Indexed { get; set; }
        public int Monthly { get; set; }
        public int TotalPayment { get; set; }

        public OriginalLoanTable4Short(int amount, string product, int time, double rate, bool indexed, int monthly, int totalPayment)
        {
            Amount = amount;
            Product = product;
            Time = time;
            Rate = rate;
            Indexed = indexed;
            Monthly = monthly;
            TotalPayment = totalPayment;
        }
    }

    class OriginalLoanTable4ShortUK
    {
        public int Amount { get; set; }
        public string Product { get; set; }
        public double Rate { get; set; }
        public double FollowOnRate { get; set; }
        public int Time { get; set; }
        public int MonthlyPMT { get; set; }
        public int Monthly { get; set; }
        public int TotalPayment { get; set; }

        public OriginalLoanTable4ShortUK(int amount, string product, int time, double rate, double followOnRate, int monthlyPMT, int monthly, int totalPayment)
        {
            Amount = amount;
            Product = product;
            Time = time;
            Rate = rate;
            FollowOnRate = followOnRate;
            MonthlyPMT = monthlyPMT;
            Monthly = monthly;
            TotalPayment = totalPayment;
        }
    }
}
