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

    public class ResultReportData
    {
        public loanDetails theLoan { get; set; }

        public RunEnvironment env { get; set; }

        // Loan data
        public string BankName;
        public string ID { get; set; }
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
        public double OriginalMargin { get; set; }
        public uint OriginalTime { get; set; }
        //public indices indices { set; get; }
        public double OriginalInflation { set; get; }
        // the selected compositions
        public Composition[] compositions { get; set; }
        //CompositionReportData[] compositionReportData;

        // calculation data
        public uint RemaingLoanTime { get; set; }
        public uint FirstMonthlyPMT { get; set; }
        public uint PayUntilToday { get; set; }
        public uint PayFuture { get; set; }

        public uint BankPayUntilToday { get; set; }
        public uint BankPayFuture { get; set; }

        //public uint Left2Pay { get; set; }
        public uint EstimateFuturePay { get; set; }
        //public double EstimateMargin { get; set; }
        public uint EstimateProfitSoFar { get; set; }
        public double EstimateProfitPercantageSoFar { get; set; }
        public uint EstimateTotalProfit { get; set; }
        public double EstimateTotalProfitPercantage { get; set; }
        public uint EstimateFutureProfit { get; set; }
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

   
        public string[] GetProducts()
        {
            return Share.theProductsNames;
        }

        //public CompositionReportData[] GetCompositionData()
        //{
        //    return compositionReportData;
        //}

        public void SetLoanData(loanDetails loan)
        {
            theLoan = loan;
        }

        public void SetEnvData(RunEnvironment  env)
        {
            this.env = env;
        }

        
        public void SetCompositionData(Composition[] compositions)
        {
            if (null != compositions)
            {
                // for the sake of storing as XML
                this.compositions = compositions;
            }
            
        }

        public void Activate(bool shouldStoreInDB, bool shouldCreateHTMLReport, bool shouldCreatePDFReport)
        {
            bool shouldThisLoanReFinance;

            UpdateGeneralResults(out shouldThisLoanReFinance);

            WindowsUtilities.loggerMethod("\nCreating report for loan: " + this.ID + " shouldThisLoanReFinance: " + shouldThisLoanReFinance +
                ", shouldCreatePDFReport: " + shouldCreatePDFReport);
            if (shouldThisLoanReFinance && (shouldCreateHTMLReport || shouldCreatePDFReport))
            {

                try
                {
                    // TBD - calculate the time of creating the reposts
                    string HTMLfilename = null, PDFfilename = null;

                    // create the report
                    if (shouldCreateHTMLReport)
                    {
                        HTMLfilename = MiscUtilities.GetReportFileName(ID, FileType.HTML);
                    }
                    if (shouldCreatePDFReport)
                    {
                        PDFfilename = MiscUtilities.GetReportFileName(ID, FileType.PDF);
                    }

                    if (!String.IsNullOrEmpty(HTMLfilename) || !String.IsNullOrEmpty(PDFfilename))
                        Reporter.LenderReport(this, HTMLfilename, PDFfilename, Share.cultureInfo);

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
                 //WindowsUtilities.loggerMethod("Activate loan: " + this.ID + " should not be refininced");
            }
        
        }

        // manage the summery file updating the bulk of loans results
        // store for each loan: id, amount, savings, calculation time
        // TBD - omri.
        void UpdateGeneralResults(out bool shouldThisLoanReFinance)
        {
            bool totalBenefitPerLoan = false;
            bool noCompositionFounded = true;
            shouldThisLoanReFinance = false;
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

            foreach (Composition comp in compositions)
            {
                if (null != comp)
                {
                    noCompositionFounded = false;
                    int ttlBankPay, ttlBorrowerPay, ttlProfit;
                    MiscUtilities.CalcaulateProfit(comp, out ttlBankPay, out ttlBorrowerPay, out ttlProfit);
                    int borrowerProfitCalc, bankProfitCalc, totalBenefit, bankOriginalProfit;
                    MiscUtilities.CalcaulateProfitAll(comp, theLoan, 
                        out borrowerProfitCalc, out bankProfitCalc, out totalBenefit, out bankOriginalProfit);
                    comp.BorrowerProfitCalc = borrowerProfitCalc;
                    comp.BankProfitCalc = bankProfitCalc;
                    comp.TotalBenefit = totalBenefit;
                    comp.BankOriginalProfit = bankOriginalProfit;

                    bool shouldReFinance, canBorrowerSave, canLenderProfit, canIncreaseTotalProfit;
                    double totalProfitPercantage;
                    canBorrowerSave = (0 < borrowerProfitCalc); // (theLoan.resultReportData.PayFuture >= ttlBorrowerPay);
                    canLenderProfit = (0 < bankProfitCalc); // (bankOptionProfit <= ttlProfit);
                    shouldReFinance = canBorrowerSave && canLenderProfit;
                    shouldThisLoanReFinance = shouldThisLoanReFinance || shouldReFinance;
                    totalBenefitPerLoan = totalBenefitPerLoan || (0 < totalBenefit);
                    totalProfitPercantage = ((double) totalBenefit / theLoan.LoanAmount * 100);
                    canIncreaseTotalProfit = (0 < totalBenefit);
                    comp.IsWinWin = shouldReFinance;

                    string[] msg = {
                        theLoan.ID, // "Loan ID",
                        theLoan.OriginalLoanAmount.ToString(), // "Original Loan Amount",
                        theLoan.OriginalDateTaken.ToString(), // "Date Taken",
                        theLoan.LoanAmount.ToString(), // "Remaining Amount",
                        theLoan.resultReportData.PayUntilToday.ToString(), // "Borrower Paid So Far",
                        theLoan.resultReportData.BankPayUntilToday.ToString(), // "Bank Profit So Far",
                        theLoan.resultReportData.PayFuture.ToString(), // "Borrower Future Payment",
                        theLoan.resultReportData.BankPayFuture.ToString(), // "Lender Future Payment",
                        (theLoan.resultReportData.PayFuture - theLoan.resultReportData.BankPayFuture).ToString(), // "Orig Bank Future Profit",
                        (shouldReFinance ? "Yes" : "No" ), // "Refinance Or No",
                        (canBorrowerSave ? "Yes" : "No" ), // "Can Save Borrower Money",
                        (canLenderProfit ? "Yes" : "No" ), // "Can Increase Lender Profit",
                        (canIncreaseTotalProfit ? "Yes" : "No" ), // "Can Increase total Profit",
                        ttlBorrowerPay.ToString(), // "Minimum Borrower Total Payment",
                        ttlBankPay.ToString(), // "Lender pay"
                        ttlProfit.ToString(), // "Maximim Lender Profit"
                        borrowerProfitCalc.ToString(), // diff of Borrower Future between the 2 options
                        ((double) borrowerProfitCalc / theLoan.LoanAmount * 100).ToString(),// Borrower beneficial%
                        bankProfitCalc.ToString(), // diff of Bank Future between the 2 options
                        // bank benefit % = difference in the bank benefit / original benefit
                        ((double) (bankProfitCalc - bankOriginalProfit) / bankOriginalProfit * 100).ToString(),
                        // ((double) bankProfitCalc / theLoan.LoanAmount * 100).ToString(),// Bank beneficial%
                        totalBenefit.ToString(), // total benefit
                        totalProfitPercantage.ToString(), // total benefit percantage
                        comp.name, // for debug - print the composition name
                        theLoan.BorrowerAge.ToString(), // "Age",
                        theLoan.resultReportData.LTV.ToString(), //"LTV",
                        theLoan.resultReportData.PTI.ToString(), // "PTI",
                        theLoan.YearlyIncome.ToString(), //"Income"
                        theLoan.fico.ToString() // fico
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
                string[] ms = { theLoan.ID, "No composition founded for load id: " };
                MiscUtilities.PrintSummaryFile(ms);
           }

            // count the refinince 
            if (shouldThisLoanReFinance)
                Share.NumberOfCanRefininceLoans++;
            if (totalBenefitPerLoan)
                Share.NumberOfPositiveBeneficialLoans++;

            // update all the accumulative data
            if (shouldThisLoanReFinance)
            {
                string[] msgn = {
                        theLoan.ID, // "Loan ID",
                        theLoan.OriginalLoanAmount.ToString(), // "Original Loan Amount",
                        theLoan.OriginalDateTaken.ToString(), // "Date Taken",
                        theLoan.LoanAmount.ToString(), // "Remaining Amount",
                        theLoan.resultReportData.PayUntilToday.ToString(), // "Borrower Paid So Far",
                        theLoan.resultReportData.BankPayUntilToday.ToString(), // "Bank Profit So Far",
                        theLoan.resultReportData.PayFuture.ToString(), // "Borrower Future Payment",
                        theLoan.resultReportData.BankPayFuture.ToString(), // "Lender Future Payment",
                        (theLoan.resultReportData.PayFuture - theLoan.resultReportData.BankPayFuture).ToString(), // "Orig Bank Future Profit",

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


    }
}
