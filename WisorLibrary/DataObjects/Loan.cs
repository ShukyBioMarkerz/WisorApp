using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.FileUtils;

namespace WisorLib
{

    public class LoanList : List<loanDetails>
    {

    }


    public class loanDetails
    {
        public string ID { get; set; }

        public MortgageType MortgageType { get; set; }
        public uint PropertyValue { get; set; }
        public uint DesiredMonthlyPayment { get; set; }
        public uint LoanAmount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime DateTaken { get; set; }
        public MortgagProduct MortgagProduct { get; set; }
        public double InterestRate { get; set; }
        public uint TermInYears { get; set; }
        public uint BorrowerAge { get; set; }
        public uint YearlyIncome { get; set; }
        public CreditClass CreditClass { get; set; }

        public loanDetails(string id, uint loanAmount, uint desiredMonthlyPayment, uint propertyValue,
            uint yearlyIncome, uint borrowerAge)
        //,
        //// optional
        //MortgageType mortgageType = MortgageType.None, PaymentType paymentType = PaymentType.None, DateTime? dateTaken = null,
        //MortgagProduct mortgagProduct = MortgagProduct.None, double interestRate = MiscConstants.UNDEFINED_INT,
        //uint termInYears = MiscConstants.UNDEFINED_UINT, CreditClass creditClass = CreditClass.None)
        {
            ID = id;
            PropertyValue = propertyValue;
            DesiredMonthlyPayment = desiredMonthlyPayment;
            LoanAmount = loanAmount;
            BorrowerAge = borrowerAge;
            YearlyIncome = yearlyIncome;
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
                ", propertyValue: " + this.PropertyValue + ", income: " + this.YearlyIncome + ", age: " + this.BorrowerAge);
        }

    }

}
