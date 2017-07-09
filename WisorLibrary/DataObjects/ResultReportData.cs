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
        loanDetails theLoan;

        RunEnvironment env;

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

   
        static public string GetFileName(string id, FileType fileType)
        {
            string ext = MiscUtilities.GetFileTypeExtension(fileType);
            string fn = AppDomain.CurrentDomain.BaseDirectory
                + MiscConstants.REPORTS_DIR + Path.DirectorySeparatorChar +
                MiscConstants.LENDER_REPORT_PREFIX + MiscConstants.NAME_SEP_CHAR + id + MiscConstants.NAME_SEP_CHAR +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + ext;
            return fn;
        }

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

            try
            {
                string HTMLfilename = null, PDFfilename = null;
                
                // create the report
                if (shouldCreateHTMLReport)
                {
                    HTMLfilename = GetFileName(ID, FileType.HTML);
                }
                if (shouldCreatePDFReport)
                {
                     PDFfilename = GetFileName(ID, FileType.PDF);
                }

                if (! String.IsNullOrEmpty(HTMLfilename) || ! String.IsNullOrEmpty(PDFfilename))
                    Reporter.LenderReport(this, HTMLfilename, PDFfilename /*, CultureInfo cultureInfo*/);

                //// store in XML file
                //if (shouldStoreInDB)
                //{
                //    StoreResultsInDB();
                //}

                UpdateGeneralResults();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("ResultReportData::Activate ex: " + ex.ToString());
            }
        
        }

        // manage the summery file updating the bulk of loans results
        // store for each loan: id, amount, savings, calculation time
        // TBD - omri.
        void UpdateGeneralResults()
        {
            bool shouldThisLoanReFinance = false, totalBenefitPerLoan = false;
            bool noCompositionFounded = true;

            foreach (Composition comp in compositions)
            {
                if (null != comp)
                {
                    noCompositionFounded = false;
                    int ttlBankPay, ttlBorrowerPay, ttlProfit;
                    MiscUtilities.CalcaulateProfit(comp, out ttlBankPay, out ttlBorrowerPay, out ttlProfit);
                    //// borrower benefit = diff between wisor composition and the original bank
                    //int borrowerProfitCalc = (int)theLoan.resultReportData.PayFuture - (int)ttlBorrowerPay;
                    //// original bank profit: diff between orig borrower pay and orig bank pay
                    //int bankOptionProfit = (int)theLoan.resultReportData.PayFuture - (int)theLoan.resultReportData.BankPayFuture;
                    //// total bank profit: diff between wisor option profit and original bank profit
                    //int bankProfitCalc = ttlProfit - bankOptionProfit;
                    //// total benefit: sum of the borrower profit and the bank profit
                    //int totalBenefit = borrowerProfitCalc + bankProfitCalc;
                    int borrowerProfitCalc, bankProfitCalc, totalBenefit;
                    MiscUtilities.CalcaulateProfitAll(comp, theLoan, 
                        out borrowerProfitCalc, out bankProfitCalc, out totalBenefit);
                    
                    //uint origProfit = theLoan.resultReportData.PayFuture - theLoan.resultReportData.BankPayFuture;
                    //string borPayMsg = "ttlBorrowerPay difference. Wisor: " + comp.ttlPay + " while original: " + theLoan.resultReportData.PayFuture;
                    //string bankPayMsg = "TtlBankPay difference. Wisor: " + ttlBankPay + " while original: " + theLoan.resultReportData.BankPayFuture;
                    //string bankProfit = "TtlBankProfit difference. Wisor: " + ttlProfit + " while original: " + origProfit;

                    bool shouldReFinance, canBorrowerSave, canLenderProfit, canIncreaseTotalProfit;
                    double totalProfitPercantage;
                    canBorrowerSave = (0 < borrowerProfitCalc); // (theLoan.resultReportData.PayFuture >= ttlBorrowerPay);
                    canLenderProfit = (0 < bankProfitCalc); // (bankOptionProfit <= ttlProfit);
                    shouldReFinance = canBorrowerSave && canLenderProfit;
                    shouldThisLoanReFinance = shouldThisLoanReFinance || shouldReFinance;
                    totalBenefitPerLoan = totalBenefitPerLoan || (0 < totalBenefit);
                    totalProfitPercantage = ((double) totalBenefit / theLoan.LoanAmount * 100);
                    canIncreaseTotalProfit = (0 < totalBenefit);

                    if (null != Share.summaryLogFile)
                    {
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
                            ((double) bankProfitCalc / theLoan.LoanAmount * 100).ToString(),// Bank beneficial%
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
                            MiscUtilities.PrintSummaryFileS(Share.theWinWinSummaryFile, msg);
                        }
                        else
                        {
                            if (canBorrowerSave)
                            {
                                MiscUtilities.PrintSummaryFileS(Share.theBorrowerWinSummaryFile, msg);
                            }
                            if (canLenderProfit)
                            {
                                MiscUtilities.PrintSummaryFileS(Share.theBankWinSummaryFile, msg);
                            }
                            if (canIncreaseTotalProfit)
                            {
                                MiscUtilities.PrintSummaryFileS(Share.theTotalWinSummaryFile, msg);
                            }
                        }
                    }

                    //MiscUtilities.PrintMiscLogger("\n\nSummery file updating the bulk of loans results");
                    //MiscUtilities.PrintMiscLogger(comp.name);
                    //MiscUtilities.PrintMiscLogger(borPayMsg);
                    //MiscUtilities.PrintMiscLogger(bankPayMsg);
                    //MiscUtilities.PrintMiscLogger(bankProfit);
                    //MiscUtilities.PrintMiscLogger(comp.ToString());
          
                    //if (null != env)
                    //{
                    //    env.WriteToOutputFile("\n\nSummery file updating the bulk of loans results");
                    //    env.WriteToOutputFile(comp.name);
                    //    env.WriteToOutputFile(borPayMsg);
                    //    env.WriteToOutputFile(bankPayMsg);
                    //    env.WriteToOutputFile(bankProfit);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("\n\nSummery file updating the bulk of loans results");
                    //    Console.WriteLine(comp.name);
                    //    Console.WriteLine(borPayMsg);
                    //    Console.WriteLine(bankPayMsg);
                    //    Console.WriteLine(bankProfit);
                    //}
                }

            }

           if (noCompositionFounded) // no composition was found
           {
                string[] msg = { theLoan.ID, "No composition founded for load id: " };
                MiscUtilities.PrintSummaryFile(msg);
           }

            // count the refinince 
            if (shouldThisLoanReFinance)
                Share.NumberOfCanRefininceLoans++;
            if (totalBenefitPerLoan)
                Share.NumberOfPositiveBeneficialLoans++;
        }

        void StoreResultsInDB()
        {
            string fn = GetFileName(ID, FileType.XML);

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
