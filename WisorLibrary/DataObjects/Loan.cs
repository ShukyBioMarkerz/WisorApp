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
        ProductID ProductID;
        public uint PropertyValue { get; set; }
        public uint DesiredMonthlyPayment { get; set; }
        public uint LoanAmount { get; set; }
        public uint OriginalLoanAmount { get; set; }
        public int fico { get; set; }
        public DateTime DateTaken { get; set; }
        public uint DesireTerminationMonth { get; set; }
        public uint BorrowerAge { get; set; }
        public uint YearlyIncome { get; set; }
        [XmlIgnoreAttribute]
        public uint SequentialNumber { get; set; }
        //string OriginalProduct { get; set; }

        double OriginalRate { get; set; }
        uint OriginalTime { get; set; }

        //public uint RemaingLoanAmount { get; set; }
        //public uint RemaingLoanTime { get; set; }
        //public uint FirstMonthlyPMT { get; set; }
        //public uint PayUntilToday { get; set; }

        [XmlIgnoreAttribute]
        public Risk risk { set; get; }
        [XmlIgnoreAttribute]
        public Liquidity liquidity { set; get; }

        public indices indices { set; get; }

        public double OriginalInflation { set; get; }

        public ResultReportData resultReportData { set; get; }


        [XmlIgnoreAttribute]
        public bool Status { set; get; }

        // for the sake of serialization.....
        public loanDetails()
        {
        }

        public loanDetails(string id, uint loanAmount, uint desiredMonthlyPayment, uint propertyValue,
            uint yearlyIncome, uint borrowerAge, int fic,
            // optional parameters
            DateTime dateTaken, ProductID product, bool shouldCalculate = true,
            double originalRate = MiscConstants.UNDEFINED_DOUBLE, uint originalTime = MiscConstants.UNDEFINED_UINT,
            //uint desireTerminationMonth = MiscConstants.UNDEFINED_UINT,
            uint sequentialNumber = MiscConstants.UNDEFINED_UINT,
            Risk risk = Risk.NONERisk, Liquidity liquidity = Liquidity.NONELiquidity)
        {
            ID = id;
            PropertyValue = propertyValue;
            DesiredMonthlyPayment = desiredMonthlyPayment;
            OriginalLoanAmount = LoanAmount = loanAmount;
            BorrowerAge = borrowerAge;
            YearlyIncome = yearlyIncome;
            fico = fic;
            DateTaken = dateTaken;
            ProductID = product;
            //DesireTerminationMonth = desireTerminationMonth;
            SequentialNumber = (MiscConstants.UNDEFINED_UINT == sequentialNumber) ? MiscUtilities.GetSequenceID() : sequentialNumber;

            OriginalInflation = MiscConstants.UNDEFINED_DOUBLE;

            // get the indics from the product to calculate the exact rate
            indices = indices.NONE;
            if (null == product)
            {
                Console.WriteLine("ERROR: loanDetails got NULL product. Ignore calculate the historical rate.");
            }
            else
            {
                GenericProduct gp = GenericProduct.GetProductByName(product.stringTypeId);
                if (null != gp)
                {
                    indices = gp.originalIndexUsedFirstTimePeriod;
                    OriginalInflation = gp.indexUsedFirstTimePeriod;
                }
                else
                {
                    Console.WriteLine("ERROR: loanDetails can't recognize the product: " + product.stringTypeId);
                }
            }

            OriginalRate = originalRate;
            OriginalTime = originalTime;
 
            if (! shouldCalculate || DateTime.Now == dateTaken
                /*|| MiscConstants.UNDEFINED_DOUBLE == originalRate ||  MiscConstants.UNDEFINED_UINT == originalTime*/)
            {
                // nothing to calculate
            }
            else
            {
                ResultReportData resultData = new ResultReportData();
                Calculations.CalculateRemainingAmount(
                     indices, (double)LoanAmount, OriginalTime, DateTaken, OriginalRate, 
                     OriginalInflation, ref resultData);
                resultReportData = resultData;
                // update the values for the new loan
                //RemaingLoanAmount = calculationData.RemaingLoanAmount;
                //LoanAmount = calculationData.RemaingLoanAmount;
                BorrowerAge = (uint) 75 - (resultReportData.RemaingLoanTime / 12);
            }
            //  TBD: should we limit this in each market 
            //BorrowerAge = 65; //  (uint) 75 - (remaingLoanTime / 12);

            // DesiredMonthlyPayment can be undefined, calculate it
            if (MiscConstants.UNDEFINED_UINT == DesiredMonthlyPayment)
            {
                DesiredMonthlyPayment = MiscUtilities.CalculateMonthlyPayment(
                    resultReportData.RemaingLoanAmount, propertyValue, yearlyIncome, borrowerAge);
            }
          
            this.risk = risk;
            this.liquidity = liquidity;

            Status = CheckConsistancy();

            // ensure the resultReport object is updated
            UpdateResultReportData();
        }

        void UpdateResultReportData()
        {
            resultReportData.BankName = Share.CustomerName;
            resultReportData.ProductName = ProductID.stringTypeId;
            resultReportData.ID = ID;
            resultReportData.PropertyValue = PropertyValue;
            resultReportData.DesiredMonthlyPayment = DesiredMonthlyPayment;
            resultReportData.LoanAmount = LoanAmount;
            resultReportData.OriginalLoanAmount = OriginalLoanAmount;
            resultReportData.fico = fico;
            resultReportData.DateTaken = DateTaken;
            resultReportData.DesireTerminationMonth = DesireTerminationMonth;
            resultReportData.BorrowerAge = BorrowerAge;
            resultReportData.YearlyIncome = YearlyIncome;
            resultReportData.OriginalRate = OriginalRate;
            resultReportData.OriginalTime = OriginalTime;
            //resultReportData.indices = indices;
            resultReportData.OriginalInflation = OriginalInflation;
            // start the timer
            resultReportData.StartTime = DateTime.Now;
        }

        private bool CheckConsistancy()
        {
            bool rc = false;

            if (MiscConstants.UNDEFINED_UINT < LoanAmount && MiscConstants.UNDEFINED_UINT < DesiredMonthlyPayment &&
                MiscConstants.UNDEFINED_UINT < PropertyValue && MiscConstants.UNDEFINED_UINT < YearlyIncome &&
                MiscConstants.UNDEFINED_DOUBLE < OriginalRate && MiscConstants.UNDEFINED_UINT < OriginalTime &&
                MiscConstants.UNDEFINED_DOUBLE < OriginalInflation && indices.NONE != indices &&
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
                if (indices.NONE == indices)
                    err += " illegal indices value: " + indices;
                WindowsUtilities.loggerMethod("ERROR: illegal loanDetails: " + err);
            }

            return rc;
        }
        public override string ToString()
        {
            return ("Loan data ID: " + this.ID + ", amount: " + this.LoanAmount + ", monthly payment: " + this.DesiredMonthlyPayment +
                ", propertyValue: " + this.PropertyValue + ", income: " + this.YearlyIncome + ", age: " + this.BorrowerAge + ", fico: " + this.fico);
        }

        public static uint CalculateDesireMonthlyPayment(uint loanAmount, uint propertyValue, uint yearlyIncome, uint borrowerAge)
        {
            uint monthlyPayment = MiscConstants.UNDEFINED_UINT;

            // TBD - rule of thumb: 30% of the income
            monthlyPayment = (MiscConstants.DEFAULT_PERCANTAGE_OF_MONTHLY_PAYMENT / 100 * yearlyIncome);
            return monthlyPayment;

        }

        public void CompleteCalculation(Composition[]composition, bool shouldStoreInDB, bool shouldCreateReport)
        {
            resultReportData.CalculationTime = DateTime.Now - resultReportData.StartTime;
            resultReportData.SetCompositionData(composition);
            resultReportData.Activate(shouldStoreInDB, shouldCreateReport);
         }
    }

}
