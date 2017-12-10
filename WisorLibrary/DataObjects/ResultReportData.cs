using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using WisorLibrary.Reporting;
using static WisorLib.MiscConstants;
using System.Xml.Serialization;
using System.Globalization;
using WisorLibrary.ReportApplication;
using WisorLibrary.Logic;

namespace WisorLibrary.DataObjects
{
    public class CompositionReportData
    {
        public string name;
        public uint ttlPmt, ttlPay;
        public uint optXBankTtlPay, optYBankTtlPay, optZBankTtlPay;
        public uint ttlBankPay, ttlBorrowerPay, ttlProfit;
        public OptionReportData[] optionReportData;

    }

    public class OptionReportData
    {
        public int optType;
        public string optTypeName;
        public uint optAmt, optTime, optPmt ,optTtlPay;
        public double optRateFirstPeriod;
    }

    //[Serializable]
    // remove the Serializable for the sake of avoiding adding special cases for the setter functions
    // consider use "Data Contracts" instead
    public class ResultReportData
    {
        //public loanDetails theLoan { get; set; }

        [XmlIgnore]
        //public RunEnvironment env { get; set; }

        // Loan data
        public string BankName;
        public string ID; // { get; set; }
        public string ProductName { get; set; }
        public uint PropertyValue { get; set; }
        public uint DesiredMonthlyPayment { get; set; }
        public uint LoanAmount { get; set; }
        public uint OriginalLoanAmount { get; set; }
        public int fico { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime OriginalDateTaken { get; set; }
        public uint DesireTerminationMonth { get; set; }
        public uint BorrowerAge { get; set; }
        public uint YearlyIncome { get; set; }
        public double OriginalRate { get; set; }
        public double OriginalRate2 { get; set; }
        public double OriginalMargin { get; set; }
        public double OriginalMargin2 { get; set; }
        public double PrintedOriginalMargin { get; set; }
        public double PrintedOriginalMargin2 { get; set; }
      
        public uint OriginalTime { get; set; }
        public int firstTimePeriod { set; get; }
        //public indices indices { set; get; }
        public double OriginalInflation { set; get; }
        // the selected compositions
        public Composition[] compositions { get; set; }
        //CompositionReportData[] compositionReportData;

        // calculation data
        public uint RemaingLoanTime { get; set; }
        public uint FirstMonthlyPMT { get; set; }
        public uint FirstMonthlyPMT2 { get; set; }
        public uint PayUntilToday { get; set; }
        public uint PayFuture { get; set; }

        public uint BankPayUntilToday { get; set; }
        public uint BankPayFuture { get; set; }

        //public uint Left2Pay { get; set; }
        public uint EstimateFuturePay { get; set; }
        //public double EstimateMargin { get; set; }
        public int EstimateProfitSoFar { get; set; }
        public double EstimateProfitPercantageSoFar { get; set; }
        public int EstimateTotalProfit { get; set; }
        public double EstimateTotalProfitPercantage { get; set; }
        public int EstimateFutureProfit { get; set; }
        public double EstimateFutureProfitPercantage { get; set; }
        public uint RemaingLoanAmount { get; set; }
        public uint MonthlyPaymentCalc { get; set; }
        public double PTI { get; set; }
        public double LTV { set; get; }

     
        // performance data
        public DateTime StartTime { get; set; }
        // since TimeSpan does not serialized...
        TimeSpan m_CalculationTime;
        [XmlIgnore]
        public TimeSpan CalculationTime {
            get { return m_CalculationTime; } 
            set {
                m_CalculationTime = value;
                CalculationTimeElapsed = value.ToString();
            }
        }
        public string CalculationTimeElapsed { get; set; }
        TimeSpan m_TotalTime;
        [XmlIgnore]
        public TimeSpan TotalTime
        {
            get { return m_TotalTime; }
            set
            {
                m_TotalTime = value;
                TotalTimeElapsed = value.ToString();
            }
        }
        public string TotalTimeElapsed { get; set; }

        public string ReportFilename { get; set; }


        public string[] GetProducts()
        {
            // TBD - should use only the products which defined in the combination file
            // return Share.theProductsNames;
            string[] response = MiscUtilities.GetCombinationsUniqueNames(LoanAmount, LTV);
            return response;
        }

        //public CompositionReportData[] GetCompositionData()
        //{
        //    return compositionReportData;
        //}

        //public void SetLoanData(loanDetails loan)
        //{
        //    theLoan = loan;
        //}

        //public void SetEnvData(RunEnvironment  env)
        //{
        //    this.env = env;
        //}

        
        public void SetCompositionData(Composition[] compositions)
        {
            if (null != compositions)
            {
                // for the sake of storing as XML
                this.compositions = compositions;
            }

        }

        public void Activate(RunEnvironment env, bool shouldStoreInDB, bool shouldCreateShortPDFReport, 
            bool shouldCreateLongPDFReport, CommonObjects.OrderDataContainer2 orderDataContainer2)
        {
            bool shouldThisLoanReFinance;

            UpdateGeneralResults(env, out shouldThisLoanReFinance);
 
            if ((shouldThisLoanReFinance && shouldCreateLongPDFReport) || shouldCreateShortPDFReport)
            {
                // create the report
                // is it the Lender or the Borrower side? Should be decided by the orderDataContainer2 value since it set by the UI or not
                //bool isLender = null == orderDataContainer2;
                bool isLender = shouldCreateShortPDFReport;
                ReportFilename = MiscUtilities.GetReportFileName(ID, FileType.PDF, isLender);
                WindowsUtilities.loggerMethod("\nCreating report for loan: " + this.ID + " ReportFilename: " + ReportFilename);

                try
                {
                    Console.WriteLine("Notice: Set PDF report file: " + ReportFilename);

                    // does it the short or long report
                    if (!String.IsNullOrEmpty(ReportFilename))
                    {
                        if (shouldCreateShortPDFReport)
                        {
                            // get the entire composition luch silukin
                            for (int i = 0; i < compositions.Length; i++)
                            {
                                // now is the time to calculate the fee from the loanAmount and the chosen composition

                                AmortisationData amortisationData = new AmortisationData();
                                Calculations.CalculateLuahSilukinAllResultsForComposition(compositions[i], ref amortisationData);
                                compositions[i].AmortisationData = amortisationData;
                                
                                // debug - print the values
                                //bool shouldDebugAmortisationData = true;
                                //if (shouldDebugAmortisationData)
                                //    MiscUtilities.PrintAmortisationData(amortisationData, true /*print2file*/);

                                uint adjustableNum, tsamudNum;
                                string AdjustablePercantageString, NoAdjustablePercantageString,
                                    TsamudPercantageString, NoTsamudPercantageString;
                                // set the composition AdjustablePercantage and TsamudPercantage
                                MiscUtilities.CalculateTypeOfProductsInComposition(compositions[i], out adjustableNum, out tsamudNum,
                                    out AdjustablePercantageString, out NoAdjustablePercantageString,
                                    out TsamudPercantageString, out NoTsamudPercantageString);
                                compositions[i].AdjustablePercantage = adjustableNum;
                                compositions[i].NoAdjustablePercantage = 100 - adjustableNum;
                                compositions[i].AdjustablePercantageString = AdjustablePercantageString;
                                compositions[i].NoAdjustablePercantageString = NoAdjustablePercantageString;
                                compositions[i].TsamudPercantage = tsamudNum;
                                compositions[i].NoTsamudPercantage = 100 - tsamudNum;
                                compositions[i].TsamudPercantageString = TsamudPercantageString;
                                compositions[i].NoTsamudPercantageString = NoTsamudPercantageString;
                                string structureTypeString;
                                MiscUtilities.CalculateTypeOfProductsHeader(adjustableNum, tsamudNum, out structureTypeString);
                                compositions[i].HeaderSummary = structureTypeString;
                            }

                            // Reporter.LenderReport(env, ReportFilename, Share.cultureInfo, false /*isPrintCovers*/);
                            bool rc = MiscUtilities.RunShortPDFreport(ReportFilename, orderDataContainer2, env.theLoan.resultReportData,
                                Share.cultureInfo, env);
                                // TBD - once debbuging the PDF English version should use the real culture 
                                // CultureInfo.CreateSpecificCulture(/*"he-IL"*/"en-GB") /*Share.cultureInfo*//*, env*/);
                        }
                        else if (shouldCreateLongPDFReport)
                        {
                            bool shouldUseTheDirectPDFlib = false;
                            string shouldUseTheDirectPDFlibStr = System.Configuration.ConfigurationManager.AppSettings["ShouldUseTheDirectPDFlib"];
                            if (!String.IsNullOrEmpty(shouldUseTheDirectPDFlibStr) && MiscConstants._YES_KEY == shouldUseTheDirectPDFlibStr.ToLower())
                                shouldUseTheDirectPDFlib = true;

                            if (shouldUseTheDirectPDFlib)
                            {
                                bool rc = MiscUtilities.RunLongPDFreport(ReportFilename, orderDataContainer2, env.theLoan.resultReportData,
                                    // TBD - once debbuging the PDF English version should use the real culture 
                                    CultureInfo.CreateSpecificCulture("he-IL") /*Share.cultureInfo*/);
                            }
                            else
                            {
                                CreateTheFullReport(env, ReportFilename, Share.cultureInfo);
                            }
                            
                        }
                    }

                    //// store in XML file
                    //if (shouldStoreInDB)
                    //{
                    //    StoreResultsInDB();
                    //}

                }
                catch (Exception ex)
                {
                    Console.WriteLine("ResultReportData::Activate ex: " + ex.ToString());
                }
            }
            else {
                WindowsUtilities.loggerMethod("\nNOTICE: Don't create report for loan: " + this.ID +
                    ", shouldThisLoanReFinance: " + shouldThisLoanReFinance+
                    ", shouldCreateShortPDFReport: " + shouldCreateShortPDFReport +
                    ", shouldCreateLongPDFReport: " + shouldCreateLongPDFReport);

                //WindowsUtilities.loggerMethod("Activate loan: " + this.ID + " should not be refininced");
            }

        }

        // manage the summery file updating the bulk of loans results
        // store for each loan: id, amount, savings, calculation time
        // TBD - omri.
        void UpdateGeneralResults(RunEnvironment env, out bool shouldThisLoanReFinance)
        {
            bool IsItNewLoan = false;
            bool totalBenefitPerLoan = false;
            bool noCompositionFounded = true;
            shouldThisLoanReFinance = false;
            bool shouldThisLoanReFinanceWithFee = false;
            // if the loan is new, always mark it as should refinance
            int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(env.theLoan.OriginalDateTaken, DateTime.Now);
            if (numOfMonths <= 0)
                shouldThisLoanReFinanceWithFee = shouldThisLoanReFinance = IsItNewLoan = true;
            int maxBorrowerPayment, maxBorrowerPayment_corrspondLenderPayment, maxBorrowerPayment_corrspondBorrowerSaving,
                maxBorrowerPayment_corrspondLenderProfit;
            string maxBorrowerPayment_corrspondName;
            int minBorrowerPayment, minBorrowerPayment_corrspondLenderPayment, minBorrowerPayment_corrspondBorrowerSaving,
                minBorrowerPayment_corrspondLenderProfit;
            string minBorrowerPayment_corrspondName;

            int maxLenderProfit, maxLenderProfit_corrspondBorrowerPayment, maxLenderProfit_corrspondBorrowerSaving,
                maxLenderProfit_corrspondLenderPayment;
            string maxLenderProfit_corrspondName;
            int minLenderProfit, minLenderProfit_corrspondBorrowerPayment, minLenderProfit_corrspondBorrowerSaving,
                minLenderProfit_corrspondLenderPayment;
            string minLenderProfit_corrspondName;
            maxBorrowerPayment = maxBorrowerPayment_corrspondLenderPayment = maxBorrowerPayment_corrspondBorrowerSaving =
                maxBorrowerPayment_corrspondLenderProfit = minBorrowerPayment = minBorrowerPayment_corrspondLenderPayment =
                minBorrowerPayment_corrspondBorrowerSaving = minBorrowerPayment_corrspondLenderProfit =
                maxLenderProfit = maxLenderProfit_corrspondBorrowerPayment = maxLenderProfit_corrspondBorrowerSaving =
                maxLenderProfit_corrspondLenderPayment = minLenderProfit = minLenderProfit_corrspondBorrowerPayment =
                minLenderProfit_corrspondBorrowerSaving = minLenderProfit_corrspondLenderPayment = MiscConstants.UNDEFINED_INT;
            maxBorrowerPayment_corrspondName = minBorrowerPayment_corrspondName = maxLenderProfit_corrspondName =
                minLenderProfit_corrspondName = MiscConstants.UNDEFINED_STRING;
            double compFeePayment = MiscConstants.UNDEFINED_DOUBLE;
            bool shouldReFinanceWithFee, canBorrowerSaveWithFee, canLenderProfitWithFee;
            shouldReFinanceWithFee = canBorrowerSaveWithFee = canLenderProfitWithFee = MiscConstants.UNDEFINED_BOOL;
            int borrowerProfitCalcWithFee = MiscConstants.UNDEFINED_INT;

            foreach (Composition comp in compositions)
            {
                if (null != comp)
                {
                    // calculate the fee 
                    uint remainingLoanAmount = env.theLoan.LoanAmount;
                    compFeePayment = comp.FeePayment = MiscUtilities.CalculateFees(remainingLoanAmount, IsItNewLoan,
                         comp.opts[(int)Options.options.OPTX].product,
                         comp.opts[(int)Options.options.OPTY].product,
                         (null == comp.opts[(int)Options.options.OPTZ] ? null : comp.opts[(int)Options.options.OPTZ].product),
                         env.theLoan.ProductID.numberID);

                    noCompositionFounded = false;
                    int ttlBankPay, ttlBorrowerPay, ttlProfit;
                    int borrowerProfitCalc, bankProfitCalc, totalBenefit, bankOriginalProfit;

                    // MiscUtilities.CalcaulateProfit(comp, out ttlBankPay, out ttlBorrowerPay, out ttlProfit);
                    MiscUtilities.CalcaulateProfitAll(comp, env.theLoan, (uint) comp.FeePayment, 
                        out borrowerProfitCalc, out bankProfitCalc, out totalBenefit, out bankOriginalProfit,
                        out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                    comp.BorrowerProfitCalc = borrowerProfitCalc;
                    borrowerProfitCalcWithFee = borrowerProfitCalc - (int)comp.FeePayment;
                    comp.BankProfitCalc = bankProfitCalc;
                    comp.TotalBenefit = totalBenefit;
                    comp.BankOriginalProfit = bankOriginalProfit;

                    bool shouldReFinance, canBorrowerSave, canLenderProfit, canIncreaseTotalProfit;
                    double totalProfitPercantage;
                    canBorrowerSave = (0 < borrowerProfitCalc);
                    canBorrowerSaveWithFee = (0 < borrowerProfitCalcWithFee);
                    canLenderProfit = (0 < bankProfitCalc);
                    // is it a new loan? so should refinince
                    if (IsItNewLoan)
                    {
                        shouldReFinance = shouldReFinanceWithFee = true;
                    }
                    else
                    {
                        shouldReFinance = canBorrowerSave && canLenderProfit;
                        shouldReFinanceWithFee = canBorrowerSaveWithFee && canLenderProfit;
                    }
                    shouldThisLoanReFinance = shouldThisLoanReFinance || shouldReFinance;
                    shouldThisLoanReFinanceWithFee = shouldThisLoanReFinanceWithFee || shouldReFinanceWithFee;
                    totalBenefitPerLoan = totalBenefitPerLoan || (0 < totalBenefit);
                    totalProfitPercantage = ((double) totalBenefit / env.theLoan.LoanAmount * 100);
                    canIncreaseTotalProfit = (0 < totalBenefit);
                    comp.IsWinWin = shouldReFinance;

                    string[] msg = {
                        env.theLoan.ID, // "Loan ID",
                        env.theLoan.OriginalLoanAmount.ToString(), // "Original Loan Amount",
                        env.theLoan.DesiredMonthlyPayment.ToString(), // "Monthly payment",
                        env.theLoan.OriginalDateTaken.ToString(DATE_FORMAT), // "Date Taken",
                        env.theLoan.LoanAmount.ToString(), // "Remaining Amount",
                        env.theLoan.resultReportData.PayUntilToday.ToString(), // "Borrower Paid So Far",
                        env.theLoan.resultReportData.BankPayUntilToday.ToString(), // "Bank Profit So Far",
                        env.theLoan.resultReportData.PayFuture.ToString(), // "Borrower Future Payment",
                        env.theLoan.resultReportData.BankPayFuture.ToString(), // "Lender Future Payment",
                        (env.theLoan.resultReportData.PayFuture - env.theLoan.resultReportData.BankPayFuture).ToString(), // "Orig Bank Future Profit",
                        (shouldReFinance ? "Yes" : "No" ), // "Refinance Or No",
                        (canBorrowerSave ? "Yes" : "No" ), // "Can Save Borrower Money",
                        (canLenderProfit ? "Yes" : "No" ), // "Can Increase Lender Profit",
                        (canIncreaseTotalProfit ? "Yes" : "No" ), // "Can Increase total Profit",
                        ttlBorrowerPay.ToString(), // "Minimum Borrower Total Payment",
                        ttlBankPay.ToString(), // "Lender pay"
                        ttlProfit.ToString(), // "Maximim Lender Profit"
                        borrowerProfitCalc.ToString(), // diff of Borrower Future between the 2 options
                        ((double) borrowerProfitCalc / env.theLoan.LoanAmount * 100).ToString(),// Borrower beneficial%
                        bankProfitCalc.ToString(), // diff of Bank Future between the 2 options
                        // bank benefit % = difference in the bank benefit / original benefit
                        (0 != bankOriginalProfit) ? ((double) (bankProfitCalc /*- bankOriginalProfit*/) / bankOriginalProfit * 100).ToString() : 0.ToString(),
                        // ((double) bankProfitCalc / theLoan.LoanAmount * 100).ToString(),// Bank beneficial%
                        totalBenefit.ToString(), // total benefit
                        totalProfitPercantage.ToString(), // total benefit percantage
                        comp.name, // for debug - print the composition name
                        env.theLoan.BorrowerAge.ToString(), // "Age",
                        env.theLoan.resultReportData.LTV.ToString(), //"LTV",
                        env.theLoan.resultReportData.PTI.ToString(), // "PTI",
                        env.theLoan.YearlyIncome.ToString(), //"Income"
                        env.theLoan.fico.ToString(), // fico
                        comp.FeePayment.ToString(),
                        (shouldReFinanceWithFee ? "Yes" : "No" ), // "With Fee: Refinance Or No"
                        (canBorrowerSaveWithFee ? "Yes" : "No" ), // "With Fee: Can Save Borrower Money"
                        borrowerProfitCalcWithFee.ToString() // "With Fee: borrower saving"
                    };

                    MiscUtilities.PrintSummaryFile(msg);

                    // print on the various summary files
                    if (shouldReFinance)
                    {
                        // ensure only one composition 
                        // TBD Shuky. Choose the "right" one, what is the cretiria?
                        // Accumulate the all cases to one entry in the file
                      
                        if (ttlBorrowerPay > maxBorrowerPayment) // found new max...
                        {
                            maxBorrowerPayment = ttlBorrowerPay;
                            maxBorrowerPayment_corrspondLenderPayment = ttlBankPay;
                            maxBorrowerPayment_corrspondBorrowerSaving = borrowerProfitCalc;
                            maxBorrowerPayment_corrspondLenderProfit = ttlProfit;
                            maxBorrowerPayment_corrspondName = comp.name;
                        }
                        if (MiscConstants.UNDEFINED_INT == minBorrowerPayment || ttlBorrowerPay < minBorrowerPayment) // found new min...
                        {
                            minBorrowerPayment = ttlBorrowerPay;
                            minBorrowerPayment_corrspondLenderPayment = ttlBankPay;
                            minBorrowerPayment_corrspondBorrowerSaving = borrowerProfitCalc;
                            minBorrowerPayment_corrspondLenderProfit = ttlProfit;
                            minBorrowerPayment_corrspondName = comp.name;
                        }
                        // and for the lender
                        if (ttlProfit > maxLenderProfit) // found new max...
                        {
                            maxLenderProfit = ttlProfit;
                            maxLenderProfit_corrspondBorrowerPayment = ttlBorrowerPay;
                            maxLenderProfit_corrspondBorrowerSaving = borrowerProfitCalc;
                            maxLenderProfit_corrspondLenderPayment = ttlBankPay;
                            maxLenderProfit_corrspondName = comp.name;
                        }
                        if (MiscConstants.UNDEFINED_INT == minLenderProfit || ttlProfit < minLenderProfit) // found new min...
                        {
                            minLenderProfit = ttlProfit;
                            minLenderProfit_corrspondBorrowerPayment = ttlBorrowerPay;
                            minLenderProfit_corrspondBorrowerSaving = borrowerProfitCalc;
                            minLenderProfit_corrspondLenderPayment = ttlBankPay;
                            minLenderProfit_corrspondName = comp.name;
                        }

                        MiscUtilities.PrintSummaryFileS(Share.theTotalWinSummaryFile, msg);

                        //// now we want only the win-win but in different files according to the names
                        //switch(comp.name)
                        //{
                        //    case MiscConstants.BEST_BANK_COMPOSITION:
                        //    case MiscConstants.BEST_ALL_PROFIT_COMPOSITION_BANK:
                        //        MiscUtilities.PrintSummaryFileS(Share.theBankWinSummaryFile, msg);
                        //        break;
                        //    case MiscConstants.BEST_BORROWER_COMPOSITION:
                        //    case MiscConstants.BEST_ALL_PROFIT_COMPOSITION_BORROWER:
                        //        MiscUtilities.PrintSummaryFileS(Share.theBorrowerWinSummaryFile, msg);
                        //        break;
                        //    default:
                        //        MiscUtilities.PrintSummaryFileS(Share.theTotalWinSummaryFile, msg);
                        //        break;
                        //}
                    }
                  
                }

            }

           if (noCompositionFounded) // no composition was found
           {
                string[] ms = { env.theLoan.ID, "No composition founded for load id: " + env.theLoan.ID };
                MiscUtilities.PrintSummaryFile(ms);
                Share.NumberOfNoCompositionFound++;
           }

            // count the refinince 
            if (shouldThisLoanReFinance)
                Share.NumberOfCanRefininceLoans++;
            if (shouldThisLoanReFinanceWithFee)
                Share.NumberOfCanRefininceLoansWithFee++;
            if (totalBenefitPerLoan)
                Share.NumberOfPositiveBeneficialLoans++;

            // update all the accumulative data
            if (shouldThisLoanReFinance)
            {
                string[] msgn = {
                        env.theLoan.ID, // "Loan ID",
                        env.theLoan.OriginalLoanAmount.ToString(), // "Original Loan Amount",
                        env.theLoan.DesiredMonthlyPayment.ToString(), // "Monthly payment",
                        env.theLoan.OriginalDateTaken.ToString(DATE_FORMAT), // "Date Taken",
                        env.theLoan.LoanAmount.ToString(), // "Remaining Amount",
                        env.theLoan.resultReportData.PayUntilToday.ToString(), // "Borrower Paid So Far",
                        env.theLoan.resultReportData.BankPayUntilToday.ToString(), // "Bank Profit So Far",
                        env.theLoan.resultReportData.PayFuture.ToString(), // "Borrower Future Payment",
                        env.theLoan.resultReportData.BankPayFuture.ToString(), // "Lender Future Payment",
                        (env.theLoan.resultReportData.PayFuture - env.theLoan.resultReportData.BankPayFuture).ToString(), // "Orig Bank Future Profit",

                        // add the accululate data
                        maxBorrowerPayment.ToString() ,
                        maxBorrowerPayment_corrspondLenderPayment.ToString() ,
                        maxBorrowerPayment_corrspondBorrowerSaving.ToString() ,
                        maxBorrowerPayment_corrspondLenderProfit.ToString(),
                        maxBorrowerPayment_corrspondName.ToString() ,
                        minBorrowerPayment.ToString(),
                        minBorrowerPayment_corrspondLenderPayment.ToString(),
                        minBorrowerPayment_corrspondBorrowerSaving.ToString(),
                        minBorrowerPayment_corrspondLenderProfit.ToString(),
                        minBorrowerPayment_corrspondName,
                        maxLenderProfit.ToString(),
                        maxLenderProfit_corrspondBorrowerPayment.ToString(),
                        maxLenderProfit_corrspondBorrowerSaving.ToString(),
                        maxLenderProfit_corrspondLenderPayment.ToString(),
                        maxLenderProfit_corrspondName,
                        minLenderProfit.ToString(),
                        minLenderProfit_corrspondBorrowerPayment.ToString(),
                        minLenderProfit_corrspondBorrowerSaving.ToString(),
                        minLenderProfit_corrspondLenderPayment.ToString(),
                        minLenderProfit_corrspondName
                        // add the fee consideration
                        ,compFeePayment.ToString(),
                        (shouldReFinanceWithFee ? "Yes" : "No" ), // "With Fee: Refinance Or No"
                        (canBorrowerSaveWithFee ? "Yes" : "No" ), // "With Fee: Can Save Borrower Money"
                        borrowerProfitCalcWithFee.ToString() // "With Fee: borrower saving"
                    };
                MiscUtilities.PrintSummaryFileS(Share.theWinWinSummaryFile, msgn);
            }

        }

        void StoreResultsInDB()
        {
            string fn = MiscUtilities.GetReportFileName(ID, FileType.XML);

            // ensure the directory realy exists
            if (!Directory.Exists(Path.GetDirectoryName(fn)))
                Directory.CreateDirectory(Path.GetDirectoryName(fn));

            // update the time elapse
            TotalTime = DateTime.Now - StartTime;
            StoreResultsAsXML(fn);
            //// cleanup the redundant xml statments
            //string string2removeStart = @"<\?xml";
            //string string2removeEnd = @"\?>";
            //FileUtils.DoRemoveStringFromFile(fn, string2removeStart, string2removeEnd);

            //// test - read from XML file
            //ResultReportData resultReportData;
            //LoadResultsFromXML(fn, out resultReportData);
        }

        void StoreResultsAsXML(string filename)
        {
            XMLUtilities.WriteToXmlFile<ResultReportData>(filename, this);
        }

        public static void LoadResultsFromXML(string filename, out ResultReportData resultReportData)
        {
            resultReportData = null;

             // ensure the file exists
            if (File.Exists(filename))
            {
                resultReportData = XMLUtilities.ReadFromXmlFile<ResultReportData>(filename);
            }
            else
            {
                WindowsUtilities.loggerMethod("LoadResultsFromXML file: " + filename + " does not exists!!!");
            }
        }

        public override string ToString()
        {
            string s = MiscConstants.UNDEFINED_STRING;

            s = " ID: " + ID + ", PropertyValue: " + PropertyValue + ", LoanAmount: " + LoanAmount +
                ", OriginalLoanAmount: " + OriginalLoanAmount + ", PayUntilToday: " + PayUntilToday +
                ", PayFuture: " + PayFuture + ", BankPayUntilToday: " + BankPayUntilToday +
                ", BankPayFuture: " + BankPayFuture + ", RemaingLoanAmount: " + RemaingLoanAmount +
                ", MonthlyPaymentCalc: " + MonthlyPaymentCalc;
            return s;
        }

        // the long report
        public static void CreateTheFullReport(RunEnvironment env, string pdfFileName, CultureInfo cultureInfo)
        {
            ResultReportData reportData = env.theLoan.resultReportData;
            LongReportDataObject lrdo = new LongReportDataObject(reportData, null /*OrderDataContainer*/, cultureInfo);

            // Report Type
            lrdo.CurrentReportType = LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers;

            // Test Another Types

            //lrdo.CurrentReportType = LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.PurchaseNew_PriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.PurchaseUsed_PriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers;
            //lrdo.CurrentReportType = LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers;

            // Changes Preferences 
            lrdo.CurrentСhangesPreferences = LongReportDataObject.СhangesPreferencesScenario.ChangeInMonthlyPayment;

            // Another settings
            //lrdo.NumberPriceOffers = 2; // PART2 PAGE 2
            //lrdo.NumberRecommendedStructures = 2;  // PART3 
            //lrdo.NumberStressTestRecommendedStructures = 2; // PART4
            //lrdo.NumberStressTestPriceOffers = 2; // PART4B

            LongReportDataObject.RunTheReport(lrdo, pdfFileName, cultureInfo);
        }


    }
}
