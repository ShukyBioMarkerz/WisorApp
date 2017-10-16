
using CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.FileUtils;
using static WisorLib.MiscConstants;

namespace WisorLib
{

    public class LoanList : List<loanDetails>
    {

    }

   
    [Serializable]
    public class loanDetails
    {
        public string ID { get; set; }
        public ProductID ProductID { get; set; }
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
        [XmlIgnoreAttribute]
        public uint SequentialNumber { get; set; }
        //string OriginalProduct { get; set; }
        
        public double OriginalRate { get; set; }
        public double OriginalRate2 { get; set; }
        public double OriginalMargin { get; set; }
        public double OriginalMargin2 { get; set; }
        public uint OriginalTime { get; set; }

        [XmlIgnoreAttribute]
        public Risk risk { set; get; }
        [XmlIgnoreAttribute]
        public Liquidity liquidity { set; get; }

        public indices indicesFirstTimePeriod { set; get; }
        public indices indicesSecondTimePeriod { set; get; }
        
        public int firstTimePeriod { set; get; }
        public double originalRateFirstTimePeriod { get; set; }
        // public double originalRateSecondTimePeriod { get; set; }

        public double OriginalInflation { set; get; }

        public ResultReportData resultReportData { set; get; }

        // the input data from the user
        public OrderDataContainer2 orderDataContainer2 { set; get; }

        [XmlIgnoreAttribute]
        public bool Status { set; get; }
        public LoanList OriginalLoanDetaild { get; internal set; }

        // for the sake of serialization.....
        public loanDetails()
        {
            Status = true;
        }

        public loanDetails(string id, uint loanAmount, uint desiredMonthlyPayment, uint propertyValue,
            uint yearlyIncome, uint borrowerAge, int fic,
            // optional parameters
            DateTime dateTaken, ProductID product, bool shouldCalculate = true,
            double originalRate = MiscConstants.UNDEFINED_DOUBLE, double originalRate2 = MiscConstants.UNDEFINED_DOUBLE,
            uint originalTime = MiscConstants.UNDEFINED_UINT,
            double originalMargin = MiscConstants.UNDEFINED_DOUBLE, double originalMargin2 = MiscConstants.UNDEFINED_DOUBLE,
            uint sequentialNumber = MiscConstants.UNDEFINED_UINT,
            Risk ris = Risk.NONERisk, Liquidity liquidit = Liquidity.NONELiquidity)
        {
            try
            {
                ID = id;
                PropertyValue = propertyValue;
                DesiredMonthlyPayment = desiredMonthlyPayment;
                OriginalLoanAmount = LoanAmount = loanAmount;
                BorrowerAge = borrowerAge;
                YearlyIncome = yearlyIncome;
                fico = fic;
                OriginalDateTaken = DateTaken = dateTaken;
                ProductID = product;
                //DesireTerminationMonth = desireTerminationMonth;
                SequentialNumber = (MiscConstants.UNDEFINED_UINT == sequentialNumber) ? MiscUtilities.GetSequenceID() : sequentialNumber;
                OriginalInflation = MiscConstants.UNDEFINED_DOUBLE;
                OriginalRate = originalRate;
                OriginalRate2 = originalRate2;
                OriginalTime = originalTime;
                if (0 >= originalMargin)
                {
                    Console.WriteLine("NOTICE: loanDetails got zero originalMargin ");
                }
                OriginalMargin = originalRate + originalMargin;
                OriginalMargin2 = originalRate2 + originalMargin2;

                risk = ris;
                liquidity = liquidit;
                resultReportData = new ResultReportData();

                // get the indics from the product to calculate the exact rate
                indicesFirstTimePeriod = indices.NONE;
                if (null == product)
                {
                    Console.WriteLine("NOTICE: loanDetails got NULL product. Ignore calculate the historical rate.");
                }
                else
                {
                    GenericProduct gp = GenericProduct.GetProductByName(product.stringTypeId);
                    if (null != gp)
                    {
                        indicesFirstTimePeriod = gp.originalIndexUsedFirstTimePeriod;
                        indicesSecondTimePeriod = gp.originalIndexUsedSecondTimePeriod;
                        OriginalInflation = gp.indexUsedFirstTimePeriod;
                        product.numberID = gp.productID.numberID;
                        firstTimePeriod = (int) gp.firstTimePeriod;
                    }
                    else
                    {
                        // try to look at the entire products, even those which should not be selected
                        gp = GenericProduct.GetProductFromAllListByName(product.stringTypeId);
                        if (null != gp)
                        {
                            indicesFirstTimePeriod = gp.originalIndexUsedFirstTimePeriod;
                            indicesSecondTimePeriod = gp.originalIndexUsedSecondTimePeriod;
                            OriginalInflation = gp.indexUsedFirstTimePeriod;
                            product.numberID = gp.productID.numberID;
                        }
                        else
                        {
                            Console.WriteLine("ERROR: loanDetails can't recognize the product: " + product.stringTypeId);
                        }
                    }
                }

                if (!shouldCalculate || DateTime.Now == dateTaken
                    /*|| MiscConstants.UNDEFINED_DOUBLE == originalRate ||  MiscConstants.UNDEFINED_UINT == originalTime*/)
                {
                    // nothing to calculate
                }
                else
                {
                    //ResultReportData resultData = new ResultReportData();
                    //Calculations.CalculateRemainingAmount(
                    //     indices, (double)LoanAmount, OriginalTime, DateTaken, OriginalRate, 
                    //     OriginalInflation, ref resultData);
                    //resultReportData = resultData;
                }

                // DesiredMonthlyPayment can be undefined, calculate it
                if (MiscConstants.UNDEFINED_UINT == DesiredMonthlyPayment)
                {
                    DesiredMonthlyPayment = MiscUtilities.CalculateMonthlyPayment(
                        resultReportData.RemaingLoanAmount, propertyValue, yearlyIncome, borrowerAge);
                }


                // since there are several loans with the same id, not all lines holds legal values
                // Status = CheckConsistancy();
                Status = true;

                // ensure the resultReport object is updated
                UpdateResultReportData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: loanDetails Exception: " + ex.ToString());
            }
        }

        void UpdateResultReportData()
        {
            if (markets.ISRAEL == Share.theMarket)
            // those values are not uniqie since the original loan is constructed from several options
            {
                resultReportData.ProductName = MiscConstants.UNDEFINED_STRING;
                resultReportData.OriginalTime = MiscConstants.UNDEFINED_UINT;
                resultReportData.OriginalRate = MiscConstants.UNDEFINED_DOUBLE;
            }
            else
            {
                if (null != ProductID)
                    resultReportData.ProductName = ProductID.stringTypeId;
                resultReportData.OriginalRate = OriginalRate;
                resultReportData.OriginalTime = OriginalTime;
            }
            resultReportData.BankName = Share.CustomerName;
            resultReportData.ID = ID;
            resultReportData.PropertyValue = PropertyValue;
            resultReportData.DesiredMonthlyPayment = DesiredMonthlyPayment;
            resultReportData.LoanAmount = LoanAmount;
            resultReportData.OriginalLoanAmount = OriginalLoanAmount;
            resultReportData.fico = fico;
            resultReportData.DateTaken = DateTaken;
            resultReportData.OriginalDateTaken = OriginalDateTaken;
            resultReportData.DesireTerminationMonth = DesireTerminationMonth;
            resultReportData.BorrowerAge = BorrowerAge;
            if (MiscConstants.UNDEFINED_UINT == BorrowerAge)
                resultReportData.BorrowerAge = (uint)75 - (resultReportData.RemaingLoanTime / 12);
            resultReportData.YearlyIncome = YearlyIncome;
            //resultReportData.indices = indices;
            resultReportData.OriginalInflation = OriginalInflation;
            //if (0 < resultReportData.YearlyIncome)
            //    resultReportData.PTI = resultReportData.FirstMonthlyPMT / resultReportData.YearlyIncome;
            //resultReportData.LTV = LoanAmount / PropertyValue;
            resultReportData.OriginalMargin = OriginalMargin;

            // start the timer
            resultReportData.StartTime = DateTime.Now;
        }

        private bool CheckConsistancy()
        {
            bool rc = false;

            if (MiscConstants.UNDEFINED_UINT < LoanAmount && MiscConstants.UNDEFINED_UINT < DesiredMonthlyPayment &&
                MiscConstants.UNDEFINED_UINT < PropertyValue && MiscConstants.UNDEFINED_UINT < YearlyIncome &&
                MiscConstants.UNDEFINED_DOUBLE < OriginalRate && MiscConstants.UNDEFINED_UINT < OriginalTime &&
                MiscConstants.UNDEFINED_DOUBLE < OriginalInflation && indices.NONE != indicesFirstTimePeriod &&
                MiscConstants.UNDEFINED_UINT < resultReportData.RemaingLoanTime)
            {
                rc = true;
            }
            else
            {
                string err = null;
                if (MiscConstants.UNDEFINED_UINT >= LoanAmount)
                    err += " illegal LoanAmount value: " + LoanAmount;
                if (MiscConstants.UNDEFINED_UINT >= DesiredMonthlyPayment)
                    err += " illegal DesiredMonthlyPayment value: " + DesiredMonthlyPayment;
                if (MiscConstants.UNDEFINED_UINT >= PropertyValue)
                    err += " illegal PropertyValue value: " + PropertyValue;
                if (MiscConstants.UNDEFINED_UINT >= YearlyIncome)
                    err += " illegal YearlyIncome value: " + YearlyIncome;
                if (MiscConstants.UNDEFINED_DOUBLE >= OriginalRate)
                    err += " illegal OriginalRate value: " + OriginalRate;
                if (MiscConstants.UNDEFINED_UINT >= OriginalTime)
                    err += " illegal OriginalTime value: " + OriginalTime;
                if (MiscConstants.UNDEFINED_UINT >= resultReportData.RemaingLoanTime)
                    err += " illegal RemaingLoanTime value: " + resultReportData.RemaingLoanTime;
                if (MiscConstants.UNDEFINED_DOUBLE >= OriginalInflation)
                    err += " illegal OriginalInflation value: " + OriginalInflation;
                if (indices.NONE == indicesFirstTimePeriod)
                    err += " illegal indices value: " + indicesFirstTimePeriod;
                WindowsUtilities.loggerMethod("ERROR: illegal loanDetails: " + err);
            }

            return rc;
        }
        public override string ToString()
        {
            return ("Loan data ID: " + this.ID + ", Original Date: " + OriginalDateTaken + ", Date: " + DateTaken + ", amount: " + this.LoanAmount + ", monthly payment: " + this.DesiredMonthlyPayment +
                ", propertyValue: " + this.PropertyValue + ", income: " + this.YearlyIncome + ", age: " + this.BorrowerAge + ", fico: " + this.fico);
        }

        public static uint CalculateDesireMonthlyPayment(uint loanAmount, uint propertyValue, uint yearlyIncome, uint borrowerAge)
        {
            uint monthlyPayment = MiscConstants.UNDEFINED_UINT;

            // TBD - rule of thumb: 30% of the income
            monthlyPayment = (MiscConstants.DEFAULT_PERCANTAGE_OF_MONTHLY_PAYMENT / 100 * yearlyIncome);
            return monthlyPayment;

        }

 
        public void CompleteCalculation(Composition[]composition, bool shouldStoreInDB, 
            bool shouldCreateShortPDFReport, bool shouldCreateLongPDFReport, RunEnvironment env)
        {
            Composition[] cleanComp = MiscUtilities.CleanComposition(composition);

            resultReportData.CalculationTime = DateTime.Now - resultReportData.StartTime;
            resultReportData.SetCompositionData(cleanComp);
            resultReportData.Activate(env, shouldStoreInDB, shouldCreateShortPDFReport, shouldCreateLongPDFReport,
                orderDataContainer2);
            //resultReportData.CalculationTime = DateTime.Now - resultReportData.CalculationTime;
         }
    }

}
