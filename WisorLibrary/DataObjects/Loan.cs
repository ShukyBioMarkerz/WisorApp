using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.Logic;
using static WisorLib.FileUtils;

namespace WisorLib
{

    public class LoanList : List<loanDetails>
    {

    }


    public class loanDetails
    {
        public string ID { get; set; }

        public uint PropertyValue { get; set; }
        public uint DesiredMonthlyPayment { get; set; }
        public uint LoanAmount { get; set; }
        public uint OriginalLoanAmount { get; set; }
        public uint fico { get; set; }
        public DateTime DateTaken { get; set; }
        public uint DesireTerminationMonth { get; set; }
        public uint BorrowerAge { get; set; }
        public uint YearlyIncome { get; set; }
        public uint SequentialNumber { get; set; }
        //string OriginalProduct { get; set; }

        double OriginalRate { get; set; }
        uint OriginalTime { get; set; }

        public double RemaingLoanAmount { get; set; }
        public uint RemaingLoanTime { get; set; }

        //public CreditClass CreditClass { get; set; }
        //public MortgageType MortgageType { get; set; }
        //public MortgagProduct MortgagProduct { get; set; }
        //public double InterestRate { get; set; }
        //public PaymentType PaymentType { get; set; }


        public loanDetails(string id, uint loanAmount, uint desiredMonthlyPayment, uint propertyValue,
            uint yearlyIncome, uint borrowerAge, uint fic,
            // optional parameters
            DateTime dateTaken,
            //string originalProduct = MiscConstants.UNDEFINED_STRING, 
            double originalRate = MiscConstants.UNDEFINED_DOUBLE, uint originalTime = MiscConstants.UNDEFINED_UINT,
            uint desireTerminationMonth = MiscConstants.UNDEFINED_UINT,
            uint sequentialNumber = MiscConstants.UNDEFINED_UINT)
         {
            ID = id;
            PropertyValue = propertyValue;
            DesiredMonthlyPayment = desiredMonthlyPayment;
            OriginalLoanAmount = LoanAmount = loanAmount;
            BorrowerAge = borrowerAge;
            YearlyIncome = yearlyIncome;
            fico = fic;
            DateTaken = dateTaken;
            DesireTerminationMonth = desireTerminationMonth;
            SequentialNumber = (MiscConstants.UNDEFINED_UINT == sequentialNumber) ? MiscUtilities.GetSequenceID() : sequentialNumber;

            //OriginalProduct = originalProduct;
            OriginalRate = originalRate;
            OriginalTime = originalTime;

            int monthOfDateLoanTaken = DateTaken.Month;
            int yearOfDateLoanTaken = DateTaken.Year;
            //int optionType = GenericProduct.GetProductIndex(OriginalProduct);

            uint remaingLoanTime;
            RemaingLoanAmount = Calculations.CalculateRemainingAmount((double) LoanAmount, OriginalTime, /*optionType,*/
                 (uint) monthOfDateLoanTaken, (uint) yearOfDateLoanTaken, OriginalRate,
                 (double) 0 /*originalInflation*/, (double) 0 /*interestPaidSoFar*/, (double) 0 /*totalPaidSoFar*/,
                 (double) 0 /*principalPaidSoFar*/, out remaingLoanTime);
            RemaingLoanTime = remaingLoanTime;

            // update the values for the new loan
            LoanAmount = (uint) RemaingLoanAmount;
            BorrowerAge = 35; //  (uint) 75 - (remaingLoanTime / 12);

            // DesiredMonthlyPayment can be undefined, calculate it
            if (MiscConstants.UNDEFINED_UINT == DesiredMonthlyPayment)
            {
                DesiredMonthlyPayment = MiscConstants.CalculateMonthlyPayment(loanAmount, propertyValue, yearlyIncome, borrowerAge);
            }
            //    MortgageType = mortgageType;
            //    PaymentType = paymentType;
            //    DateTaken = (DateTime) dateTaken;
            //    MortgagProduct = mortgagProduct;
            //    InterestRate = interestRate;
            //    TermInYears = termInYears;
            //    CreditClass = creditClass;
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
    }

}
