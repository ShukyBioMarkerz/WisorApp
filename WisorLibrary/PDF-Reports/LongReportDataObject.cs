using CommonObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;
using static WisorLibrary.ReportApplication.Utils;

namespace WisorLibrary.ReportApplication
{
    public class LongReportDataObject
    {
        public LongReportDataObject(ResultReportData ReportData, OrderDataContainer2 OrderData, CultureInfo CultureInfo)
        {
            reportData = ReportData;
            orderData = OrderData;
            cultureInfo = CultureInfo;

            reportType = GetReportType();

            if (null != OrderData && null != OrderData.Offers && null != OrderData.Offers.OffersList)
                ReadAllBanksData(orderData.Offers.OffersList);

            if (null != OrderData && null != OrderData.RecycleCheck && null != OrderData.RecycleCheck.RecycleLoans)
                ReadAllRecycleCheckData(orderData.RecycleCheck);
        }

        // Types of reports
        public enum ReportType
        {
            PurchaseNew_PriceOffers,
            PurchaseNew_NoPriceOffers,
            PurchaseUsed_PriceOffers,
            PurchaseUsed_NoPriceOffers,
            Refinance_LoanInserted_NoPriceOffers,
            Refinance_LoanInserted_PriceOffers,
            Refinance_NoLoanInserted_NoPriceOffers,
            Refinance_NoLoanInserted_PriceOffers
        }

        public enum СhangesPreferencesScenario
        {
            NoChanges,
            ChangeInMonthlyPayment,
            EarlyRepayment
        }

        // the actual results from the loan-engine-run
        private ResultReportData reportData { get; }

        // the current order data 
        private OrderDataContainer2 orderData { get; set; }

        private ReportType reportType;

        private CultureInfo cultureInfo;


        ReportType GetReportType()
        {
            ReportType rt = ReportType.PurchaseNew_PriceOffers;

            return rt;
        }

        public void SetOrderDataContainer(OrderDataContainer2 OrderData)
        {
            orderData = OrderData;
        }

        //
        // Config section
        //
        public ReportType CurrentReportType = ReportType.Refinance_LoanInserted_PriceOffers;
        //public ReportType CurrentReportType = ReportType.PurchaseNew_NoPriceOffers;
        public СhangesPreferencesScenario CurrentСhangesPreferences = СhangesPreferencesScenario.ChangeInMonthlyPayment;

        public int NumberPriceOffers {
            get
            {
                if (null != orderData && null != orderData.Offers && null != orderData.Offers.OffersList)
                    return orderData.Offers.OffersList.Count;
                else
                    return 0;
            }
        } // PART2 PAGE 2

        public int NumberRecommendedStructures {
            get
            {
                if (null != reportData && null != reportData.compositions)
                    return reportData.compositions.Length;
                else
                    return 0;
            }
        } // PART3 

        // for every recommandad structure ther is a strress test 
        public int NumberStressTestRecommendedStructures {
            get
            {
                return NumberRecommendedStructures;
            }
        } // PART4

        // number of offers inserted by the user
        public int NumberStressTestPriceOffers {
            get
            {
                return NumberPriceOffers;
            }
        } // PART4B
        //

        //page1
        public DateTime dateOfReport { get { /*if (null != reportData) return reportData.DateTaken; else*/ return DateTime.Now; } }

        // section 2.
        private string orderNumberValue;
        public string OrderNumberValue {
            get
            {
                if (null != reportData)
                    return reportData.ID;
                else if (!String.IsNullOrEmpty(orderNumberValue))
                    return orderNumberValue;
                else 
                    return "1234";
            }
            set
            {
                orderNumberValue = value;
            }
        }

        private string emailValue;
        public string EmailValue {
            get {
                if (!String.IsNullOrEmpty(emailValue))
                    return emailValue;
                else
                    return MiscConstants.UNDEFINED_STRING;
                // if (null != orderData && null != orderData.Userid) return orderData.Userid.ToString(); else return "ShukyIsMe@abcd.com";
            }
            set
            {
                emailValue = value;
            }
        } // דוא"ל שם משתמש

        // section 2.1.
        public string DealTypeValue { get { if (null != orderData) return orderData.DealType.DealTypeName; return "fake orderData.DealType.Type"; } }

        public int PropertyValueValue { get { if (null != orderData) return (int)orderData.LoanDetails.PropertyValue; else return 123; } }

        public int LoanAmountValue { get { if (null != orderData) return (int)orderData.LoanDetails.LoanDesire; else return 1234567; } }

        // calculation: property value - loan amount
        public int DownPaymentValue { get { return PropertyValueValue - LoanAmountValue; } }

        public int MonthlyPaymentValue { get { if (null != orderData) return (int)orderData.LoanDetails.WantedReturn; else return 12334; } }

        public DateTime DateOfEntranceValue { get { if (null != orderData) return new DateTime((int)orderData.DealType.DealProperty.EnterYear, (int)orderData.DealType.DealProperty.EnterMonth, 1); else return DateTime.Now; } }

        public int RentTodayValue { get { if (null != orderData) return (int)orderData.DealType.DealProperty.CurrentRent; else return 5643; } }

        public int NumOfBorrowersValue { get { if (null != orderData) return (int)orderData.Loaners.NumBorrowers; else return 7890; } }

        public int YoungestAgeValue { get { if (null != orderData) return (int)orderData.Loaners.YoungBorrower; else return 999; } }

        public int TotalIncomeValue { get { if (null != orderData) return (int)orderData.Loaners.NetoSum; else return 123456; } }
        public int ObligationsValue { get { if (null != orderData) return Utils.CalculateSum(orderData.Loaners.Debts); else return 1; } }

        public string FutureMoneyValue { get { if (null != orderData) return Translator.TranslateBoolToYesOrNo2(orderData.Loaners.IsRelease); else return "יש"; } }

        public string SavingsValue { get { if (null != orderData) return Translator.TranslateBoolToYesOrNo2(orderData.Loaners.IsSavings); else return "יש"; } }

        // section 2.1. table
        // The PriorLiabilitiesTable description
        // The values
        public PriorLiabilitiesData[] PriorLiabilitiesTableValues
        {
            get
            {
                if (null != orderData)
                {
                    return Utils.GetLiabilities(orderData.Loaners.Debts);
                }
                else {
                    return new PriorLiabilitiesData[] {
                    new PriorLiabilitiesData(400, new DateTime(2002, 11, 1)) ,
                    new PriorLiabilitiesData(440, new DateTime(2024, 04, 1)),
                    new PriorLiabilitiesData(500, new DateTime(2022, 03, 1))
                    };
                };
            }
        }

        // The FutureReleasesTable description
        // The values
        public FutureReleases[] FutureReleasesTableValues
        {
            get
            {
                if (null != orderData)
                {
                    return Utils.GetFutureReleases(orderData.Loaners.Releases);
                }
                else
                {
                    return new FutureReleases[] {
                        new FutureReleases(50000, new DateTime(2022, 08, 1)) ,
                        new FutureReleases(20000, new DateTime(2021, 12, 1)),
                        new FutureReleases(30000, new DateTime(2023, 05, 1))
                    };
                }
            }
        }

        // The values. Luch Silukin
        public AmortisationTable[] AmortisationTableValues
        {
            get
            {
                if (null != orderData && null != orderData.LoanDetails)
                {
                    return Utils.GetAmortisation(orderData.LoanDetails.LoanDesire).AmortisationTable;
                }
                else
                {
                    return null;
                //    return new AmortisationTable[] {
                //    new AmortisationTable(1, 50000000, 1500),
                //    new AmortisationTable(2, 23000, 2500),
                //    new AmortisationTable(3, 41000, 3500),
                //    new AmortisationTable(4, 4000, 4500),
                //    new AmortisationTable(5, 5000, 5500),
                //    new AmortisationTable(6, 10000, 1001),
                //    new AmortisationTable(7, 20000, 1013),
                //    new AmortisationTable(8, 30000, 6500),
                //    new AmortisationTable(9, 40000, 7500),
                //    new AmortisationTable(10, 50000, 8500),
                //    new AmortisationTable(11, 60000, 1300),
                //    new AmortisationTable(12, 70000, 1400),
                //    new AmortisationTable(13, 20000, 1500),
                //    new AmortisationTable(14, 20000, 1500),
                //    new AmortisationTable(15, 20000, 1200),
                //    new AmortisationTable(16, 20000, 4200),
                //    new AmortisationTable(17, 40400, 5600),
                //    new AmortisationTable(18, 50500, 1500),
                //    new AmortisationTable(19, 50600, 1500),
                //    new AmortisationTable(20, 50700, 1500),
                //    new AmortisationTable(21, 52200, 2400),
                //    new AmortisationTable(22, 50200, 1200),
                //    new AmortisationTable(23, 51300, 5200),
                //    new AmortisationTable(24, 55300, 5300),
                //    new AmortisationTable(25, 13400, 6600),
                //    new AmortisationTable(26, 34100, 1300),
                //    new AmortisationTable(27, 50000, 1300),
                //    new AmortisationTable(28, 134000000, 1200),
                //    new AmortisationTable(29, 2410000, 1100),
                //    new AmortisationTable(30, 131000, 1100)
                //};
                }
            }
        }

        public Point[] SingleStructureGraphs
        {
            get
            {
                return new Point[]
                {
                    new Point { X = 1, Y = 100 },
                    new Point { X = 2, Y = 150 },
                    new Point { X = 3, Y = 200 },
                    new Point { X = 4, Y = 250 },
                    new Point { X = 5, Y = 300 },
                    new Point { X = 6, Y = 350 },
                    new Point { X = 7, Y = 400 },
                    new Point { X = 8, Y = 450 },
                    new Point { X = 9, Y = 500 },
                    new Point { X = 10, Y = 550 },
                    new Point { X = 11, Y = 600 },
                    new Point { X = 12, Y = 650 },
                    new Point { X = 13, Y = 700 },
                    new Point { X = 14, Y = 780 },
                    new Point { X = 15, Y = 900 },
                    new Point { X = 16, Y = 1000 },
                    new Point { X = 17, Y = 950 },
                    new Point { X = 18, Y = 1050 },
                    new Point { X = 19, Y = 1100 },
                    new Point { X = 20, Y = 1200 },
                    new Point { X = 21, Y = 1250 },
                    new Point { X = 22, Y = 900 },
                    new Point { X = 23, Y = 1400 },
                    new Point { X = 24, Y = 1300 },
                    new Point { X = 25, Y = 1200 },
                    new Point { X = 26, Y = 1350 },
                    new Point { X = 27, Y = 1400 },
                    new Point { X = 28, Y = 1500 },
                    new Point { X = 29, Y = 1550 },
                    new Point { X = 30, Y = 1450 },
                    new Point { X = 31, Y = 1250 }
                };
            }
        }

        // The values of Graphs
        public Point[] SingleStructureGraphs2
        {
            get
            {
                return new Point[] {
                    new Point(1, 2000),
                    new Point(2, 1900),
                    new Point(3, 1800),
                    new Point(4, 1700),
                    new Point(5, 1600),
                    new Point(6, 1550),
                    new Point(7, 1500),
                    new Point(8, 1450),
                    new Point(9, 1400),
                    new Point(10, 1350),
                    new Point(11, 1300),
                    new Point(12, 1250),
                    new Point(13, 1200),
                    new Point(14, 1100),
                    new Point(15, 1050),
                    new Point(16, 1000),
                    new Point(17, 950),
                    new Point(18, 900),
                    new Point(19, 850),
                    new Point(20, 800),
                    new Point(21, 750),
                    new Point(22, 700),
                    new Point(23, 600),
                    new Point(24, 550),
                    new Point(25, 450),
                    new Point(26, 400),
                    new Point(27, 350),
                    new Point(28, 300),
                    new Point(29, 200),
                    new Point(30, 100),
                    new Point(31, 50)
                };
            }
        }

        // The FixedSavingsTable description
        // The values
        public FixedSavings[] FixedSavingsTableValues
        {
            get
            {
                if (null != orderData && null != orderData.Loaners && null != orderData.Loaners.Savings)
                {
                    return Utils.GetFixedSavings(orderData.Loaners.Savings);
                }
                else
                {
                    return new FixedSavings[] {
                        new FixedSavings(400, /*"פקמ"*/4, 2, /*Translator.TranslateBoolToYesOrNo*/(true)) ,
                        new FixedSavings(550, /*"מניות"*/2, 3, /*Translator.TranslateBoolToYesOrNo*/(false)),
                        new FixedSavings(450, /*"אחר"*/2, 4, /*Translator.TranslateBoolToYesOrNo*/(true))
                    };
                }
            }
        }

        public double LtvRatioValue { get { return (double)LoanAmountValue / PropertyValueValue; } }

        public double PtiRatioValue { get { return (double)MonthlyPaymentValue / TotalIncomeValue; } }

        // section 2.2.
        /*
        NewPredictedPayment = Data.Personal.changesType.ToLower() == "monthlyreturn" ? (int?)Data.Personal.monthlyReturnSum : null,
        TimeOfChange = Data.Personal.changesType.ToLower() == "monthlyreturn" ? (int?)Data.Personal.monthlyReturnTime : null,
        TimeOfRepay = Data.Personal.changesType.ToLower() != "monthlyreturn" ? (int?)Data.Personal.fullOutTime : null
        
        public class Personal2
	    {
		    public string planningTime { get; set; }
		    public string stability { get; set; }
		    public bool changes { get; set; }
		    public string changesType { get; set; }
		    public int monthlyReturnSum { get; set; }
		    public int monthlyReturnTime { get; set; }
		    public int fullOutTime { get; set; }
	    }
             
        */
        public string PlanningPreferencesValue {
            get
            {
                if (null != orderData && null != orderData.Personal.changesType)
                {
                    return orderData.Personal.changesType;
                }
                else
                    return "זמן ארוך";
            }
        }

        public string LiquidityPreferencesValue {
            get
            {
                if (null != orderData && null != orderData.Personal.stability)
                {
                    return orderData.Personal.stability;
                }
                else
                    return "שקט נפשי, החזר יציב";
            }
        }

        public int NewPmtYearsValue {
            get
            {
                if (null != orderData)
                {
                    return orderData.Personal.monthlyReturnSum;
                }
                else
                    return 10;
            }
        }
        public int RepaymentYearsValue {
            get
            {
                if (null != orderData)
                {
                    return orderData.Personal.fullOutTime;
                }
                else
                    return 30;
            }
        }

        public string ChangesPreferencesValue
        {
            get
            {
                if (CurrentСhangesPreferences == СhangesPreferencesScenario.NoChanges)
                {
                    return Properties.Resources.СhangesPreferencesNoChanges; // "אין צפי לשינויים";
                }
                if (CurrentСhangesPreferences == СhangesPreferencesScenario.ChangeInMonthlyPayment)
                {
                    return Properties.Resources.СhangesPreferencesChangeInMonthlyPayment; //  "צפי לסילוק מלא";
                }
                if (CurrentСhangesPreferences == СhangesPreferencesScenario.EarlyRepayment)
                {
                    return Properties.Resources.СhangesPreferencesEarlyRepayment;  //"צפי לסילוק מלא";
                }
                return "";
            }
        }

        //section 2.3
        // bank offers

        public List<string> ApprovingBankNameValue
        {
            get
            {
                if (null != _BankOffers /*null != orderData && null != orderData.Offers*/)
                {
                    List<string> bankNames = new List<string>();
                    for (int i = 0; i < _BankOffers.Length; i++)
                    {
                        bankNames.Add(_BankOffers[i].BankId.ToString());
                    }
                    return bankNames;
                }
                else return new List<string>()
                {
                    "מזרחי-טפחות",
                    "דיסקונט"
                };
            }
        }

        public string mainAccountNameValue {
            get {
                if (null != orderData && null != orderData.Banks && null != orderData.Banks.Accounts
                    && null != orderData.Banks.Accounts.Main)
                {
                    return orderData.Banks.Accounts.Main.ToString();
                }
                else
                    return "אין";
            }
        }
        public string secondaryAccountNameValue
        {
            get
            {
                if (null != orderData && null != orderData.Banks && null != orderData.Banks.Accounts
                    && null != orderData.Banks.Accounts.Secondary)
                {
                    return orderData.Banks.Accounts.Secondary.ToString();
                }
                else
                    return "אין";
            }
        }

        public string GetTitleOfferBankValue(int index)
        {
            if (null != _BankOffers && index < _BankOffers.Length  /*null != orderData && null != orderData.Offers*/)
            {
                return _BankOffers[index].BankId.ToString();
            }
            else 
                return "בנק לא ידוע";
        }

        /* 
         * Use GetTitleOfferBankValue(int index) instead */
        public string TitleOfferBankValue { get { return GetTitleOfferBankValue(0); } }
        public string TitleOfferBankValue2 { get { return GetTitleOfferBankValue(1); } }
        public string TitleOfferBankValue3 { get { return GetTitleOfferBankValue(2); } }
        

        public int GetOfferBankAmountTotalValue(int index)
        {
            if (null != _BankOffers && index < _BankOffers.Length  /*null != orderData && null != orderData.Offers*/)
            {
                return _BankOffers[index].Sum;
            }
            else
                return 500;
        }
        /* Use  GetOfferBankAmountTotalValue(int index) instead */
        public int OfferBankAmountTotalValue { get { return GetOfferBankAmountTotalValue(0); } }
        public int OfferBankAmountTotalValue2 { get { return GetOfferBankAmountTotalValue(1); } }
        public int OfferBankAmountTotalValue3 { get { return GetOfferBankAmountTotalValue(2); } }
        

        public int GetTitleOfferPmtTotalValue(int index)
        {
            if (null != _BankOffers && index < _BankOffers.Length  /*null != orderData && null != orderData.Offers*/)
            {
                return _BankOffers[index].ReturnSum;
            }
            else
                return 500;
        }
        /* Use GetTitleOfferPmtTotalValue(int index) instead */
        public int TitleOfferPmtTotalValue { get { return GetTitleOfferPmtTotalValue(0); } }
        public int TitleOfferPmtTotalValue2 { get { return GetTitleOfferPmtTotalValue(1); } }
        public int TitleOfferPmtTotalValue3 { get { return GetTitleOfferPmtTotalValue(2); } }

        public int GetOfferTotalFuturePaymentValue(int index)
        {
            if (null != _BankOffers && index < _BankOffers.Length  /*null != orderData && null != orderData.Offers*/)
            {
                return _BankOffers[index].ReturnSum; // TBD - what should be return here???
            }
            else
                return 500;
        }
        /* use GetOfferTotalFuturePaymentValue(int index) instaed */
        public int OfferTotalFuturePaymentValue { get { return GetOfferTotalFuturePaymentValue(0); } }
        public int OfferTotalFuturePaymentValue2 { get { return GetOfferTotalFuturePaymentValue(1); } }
        public int OfferTotalFuturePaymentValue3 { get { return GetOfferTotalFuturePaymentValue(2); } }
        

        public int StructureTotalPayment1 {
            get
            {
                int currentIndex = 0; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optTtlPay + c.opts[1].optTtlPay +
                        (null == c.opts[2] ? 0 : c.opts[2].optTtlPay));
                    return sum;
                }
                else
                {
                    return 2160767;
                }
            }
        }
        public int StructureTotalPayment2
        {
            get
            {
                int currentIndex = 1; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optTtlPay + c.opts[1].optTtlPay +
                        (null == c.opts[2] ? 0 : c.opts[2].optTtlPay));
                    return sum;
                }
                else
                {
                    return 1160767;
                }
            }
        }
        public int StructureTotalPayment3
        {
            get
            {
                int currentIndex = 2; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optTtlPay + c.opts[1].optTtlPay +
                        (null == c.opts[2] ? 0 : c.opts[2].optTtlPay));
                    return sum;
                }
                else
                {
                    return 5160767;
                }
            }
        }
        public int TotalStructureAmount1 {
            get {
                int currentIndex = 0; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optAmt + c.opts[1].optAmt + 
                        (null == c.opts[2] ? 0 : c.opts[2].optAmt));
                    return sum;
                }
                else
                {
                    return 2100000;
                }
            }
        }
        public int StressTotalStructureAmount1 { get { return 2150000; } }
        public int TotalStructurePmt1
        {
            get
            {
                int currentIndex = 0; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optPmt + c.opts[1].optPmt +
                        (null == c.opts[2] ? 0 : c.opts[2].optPmt));
                    return sum;
                }
                else
                {
                    return 1200;
                }
            }
        }
        public int StressTotalStructurePmt1 { get { return 12500; } }

        public int TotalStructureAmount2
        {
            get
            {
                int currentIndex = 1; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optAmt + c.opts[1].optAmt +
                        (null == c.opts[2] ? 0 : c.opts[2].optAmt));
                    return sum;
                }
                else
                {
                    return 2100000;
                }
            }
        }
        public int StressTotalStructureAmount2 { get { return 2100000; } }
        public int TotalStructurePmt2 {
            get
            {
                int currentIndex = 1; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optPmt + c.opts[1].optPmt +
                        (null == c.opts[2] ? 0 : c.opts[2].optPmt));
                    return sum;
                }
                else
                {
                    return 7000;
                }
            }
        }
       
        public int StressTotalStructurePmt2 { get { return 5000; } }

        public int TotalPaymentTotalAmount { get { return 2100000; } }
        public int StressTotalPaymentTotalAmount { get { return 110000; } }
        public int StressTotalPaymentTotalAmount2 { get { return 1190000; } }
        public int StressTotalPaymentTotalAmount3 { get { return 2200000; } }
        public int TotalPaymentTotalPrincipalPaid { get { return 10500; } }
        public int StressTotalPaymentTotalPrincipalPaid { get { return 10500; } }
        public int StressTotalPaymentTotalPrincipalPaid2 { get { return 20500; } }
        public int StressTotalPaymentTotalPrincipalPaid3 { get { return 30500; } }
        public int TotalPaymentTotalInterestPaid { get { return 55000; } }
        public int StressTotalPaymentTotalInterestPaid { get { return 55000; } }
        public int StressTotalPaymentTotalInterestPaid2 { get { return 55000; } }
        public int StressTotalPaymentTotalInterestPaid3 { get { return 55000; } }
        public int TotalPaymentTotalPaidAmount { get { return 2000; } }
        public int StressTotalPaymentTotalPaidAmount { get { return 2000; } }
        public int StressTotalPaymentTotalPaidAmount2 { get { return 1000; } }
        public int StressTotalPaymentTotalPaidAmount3 { get { return 3000; } }

        public int TotalPaymentTotalAmount2 { get { return 2100000; } }
        public int TotalPaymentTotalPrincipalPaid2 { get { return 10500; } }
        public int TotalPaymentTotalInterestPaid2 { get { return 55000; } }
        public int TotalPaymentTotalPaidAmount2 { get { return 2000; } }

        public int TotalPaymentTotalAmount3 { get { return 2100000; } }
        public int TotalPaymentTotalPrincipalPaid3 { get { return 10500; } }
        public int TotalPaymentTotalInterestPaid3 { get { return 55000; } }
        public int TotalPaymentTotalPaidAmount3 { get { return 2000; } }

        public int FirstFiveYearsTotalAmount1 { get { return 2100000; } }
        public int StressFirstFiveYearsTotalAmount { get { return 2100000; } }
        public int StressFirstFiveYearsTotalAmount2 { get { return 3100000; } }
        public int StressFirstFiveYearsTotalAmount3 { get { return 8100000; } }
        public int FirstFiveYearsTotalRemainingAmount1 { get { return 1679559; } }
        public int FirstFiveYearsTotalPrincipalPaid1 { get { return 420441; } }
        public int StressFirstFiveYearsTotalRemainingAmount1 { get { return 679559; } }
        public int StressFirstFiveYearsTotalRemainingAmount2 { get { return 2679559; } }
        public int StressFirstFiveYearsTotalRemainingAmount3 { get { return 3179559; } }
        public int StressFiveYearsTotalPrincipalPaid { get { return 42441; } }
        public int StressFiveYearsTotalPrincipalPaid2 { get { return 320441; } }
        public int StressFiveYearsTotalPrincipalPaid3 { get { return 820441; } }
        public int FirstFiveYearsTotalPaidSoFar1 { get { return 720000; } }
        public int StressFirstFiveYearsTotalPaidSoFar { get { return 920000; } }
        public int StressFirstFiveYearsTotalPaidSoFar2 { get { return 72000; } }
        public int StressFirstFiveYearsTotalPaidSoFar3 { get { return 420000; } }
        public int firstFiveYearsTotalNextPmt1 { get { return 12000; } }
        public int StressFirstFiveYearsTotalNextPmt1 { get { return 12000; } }
        public int StressFirstFiveYearsTotalNextPmt2 { get { return 1000; } }
        public int StressFirstFiveYearsTotalNextPmt3 { get { return 2000; } }
        public int firstFiveYearsTotalInterestPaid1 { get { return 299579; } }
        public int StressFirstFiveYearsTotalInterestPaid1 { get { return 199579; } }
        public int StressFirstFiveYearsTotalInterestPaid2 { get { return 99579; } }
        public int StressFirstFiveYearsTotalInterestPaid3 { get { return 111579; } }

        public int PaidUntilTodayTotalAmount { get { return 800000; } }
        public int PaidUntilTodayTotalPrincipalPaid { get { return 311501; }}
        public int PaidUntilTodayTotalInterestPaid { get { return 348374; } }
        public int PaidUntilTodayTotalTotalPaid { get { return 659875; } }
        public int PaidUntilTodayTotalNextPmt { get { return 3920; } }

        public int FuturePaymentTotalRemainingAmount { get { return 522044; } }
        public int FuturePaymentTotalPrincipalPayment { get { return 534151; } }
        public int FuturePaymentTotalInterestPayment { get { return 140277; } }
        public int FuturePaymentTotalTotalPayment { get { return 674428; } }

        public int AppendixPaymentTotalAmount { get { return 800000; } }
        public int AppendixPaymentTotalPrincipalPaid { get { return 845653; } }
        public int AppendixPaymentTotalInterestPaid { get { return 488650; } }
        public int AppendixPaymentTotalTotalPaid { get { return 1334303; } }

        public int FirstFiveYearsTotalAmount2 { get { return 2100000; } }
        public int FirstFiveYearsTotalRemainingAmount2 { get { return 1679559; } }
        public int FirstFiveYearsTotalPrincipalPaid2 { get { return 420441; } }
        public int FirstFiveYearsTotalPaidSoFar2 { get { return 720000; } }
        public int firstFiveYearsTotalNextPmt2 { get { return 12000; } }
        public int firstFiveYearsTotalInterestPaid2 { get { return 299579; } }

        public int FirstFiveYearsTotalAmount3 { get { return 2100000; } }
        public int FirstFiveYearsTotalRemainingAmount3 { get { return 1679559; } }
        public int FirstFiveYearsTotalPrincipalPaid3 { get { return 420441; } }
        public int FirstFiveYearsTotalPaidSoFar3 { get { return 720000; } }
        public int firstFiveYearsTotalNextPmt3 { get { return 12000; } }
        public int firstFiveYearsTotalInterestPaid3 { get { return 299579; } }

        public int TotalStructureAmount3
        {
            get
            {
                int currentIndex = 2; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optAmt + c.opts[1].optAmt +
                        (null == c.opts[2] ? 0 : c.opts[2].optAmt));
                    return sum;
                }
                else
                {
                    return 2100000;
                }
            }
        }
        public int StressTotalStructureAmount3 { get { return 1000000; } }
        public int TotalStructurePmt3 {
            get
            {
                int currentIndex = 2; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    // TBD - should be function...
                    int sum = Convert.ToInt32(c.opts[0].optPmt + c.opts[1].optPmt +
                        (null == c.opts[2] ? 0 : c.opts[2].optPmt));
                    return sum;
                }
                else
                {
                    return 8000;
                }
            }
        }
        public int StressTotalStructurePmt3 { get { return 42000; } }

        public double SingleStructureFixed { get { return 43; } }
        public double SingleStructureFixed2 { get { return 22; } }
        public double SingleStructureFixed3 { get { return 55; } }
        public double StressSingleStructureFixed { get { return 43; } }
        public double StressSingleStructureFixed2 { get { return 63; } }
        public double StressSingleStructureFixed3 { get { return 83; } }
        public double SingleStructureAdjustable { get { return 57; } }
        public double SingleStructureAdjustable2 { get { return 23; } }
        public double SingleStructureAdjustable3 { get { return 80; } }
        public double StressSingleStructureAdjustable { get { return 57; } }
        public double StressSingleStructureAdjustable2 { get { return 57; } }
        public double StressSingleStructureAdjustable3 { get { return 57; } }

        public double AppendixSectionAverageLoanTime { get { return 27.1; } }
        public double AppendixSectionAverageLoanRate { get { return 4.325 ; } }
        public double AppendixSectionTotalRepaymentCost { get { return 1.67; } }

        public double SingleStructureTotalNoTsamud { get { return 100; } }
        public double SingleStructureTotalNoTsamud2 { get { return 53; } }
        public double SingleStructureTotalNoTsamud3 { get { return 14; } }
        public double SingleStructureTotalTsamud { get { return 0; } }
        public double SingleStructureTotalTsamud2 { get { return 1; } }
        public double SingleStructureTotalTsamud3 { get { return 3; } }

        public string SingleStructureGraphsTitleLeft { get { return "החזר חודשי לאורך זמן"; } }
        public string SingleStructureGraphsTitleRight { get { return "תשלום מצטבר ויתרה לסילוק לאורך זמן"; } }

        public string stressTestSummaryPriceOfferValue1 { get { return "דיסקונט"; } }
        public string stressTestSummaryPriceOfferValue2 { get { return "דיסקונט"; } }
        public string stressTestSummaryPriceOfferValue3 { get { return "דיסקונט"; } }
        public int StressTestDifferenceInTotalPaymentValue { get { return 167532; } }
        public int StressTestDifferenceInTotalPaymentValue2 { get { return 167532; } }
        public int StressTestDifferenceInTotalPaymentValue3 { get { return 167532; } }

        public int OfferOriginalTotalPaymentValue { get { return 1100; } }
        public int OfferOriginalTotalPaymentValue2 { get { return 1100; } }
        public int OfferOriginalTotalPaymentValue3 { get { return 1100; } }
        public int OfferTotalNewPayment { get { return 1268; } }
        public int OfferTotalNewPayment2 { get { return 1268; } }
        public int OfferTotalNewPayment3 { get { return 1268; } }

        // Refinance Section 2.3
        public int TotalPmtToday { get { return MonthlyPaymentValue; } }
        public int TotalOriginalAmount { get { return LoanAmountValue; } }
        public string OfferingBankNameValue { get { return _RecycleLoanBankName; } }

        // Part2B
        public int TotalOriginalPmt2B
        {
            get
            {
                if (null != _RecycleLoan2)
                {
                    int total = 0;

                    for (int i = 0; i < _RecycleLoan2.Length; i++)
                    {
                        total += _RecycleLoan2[i].ReturnSum;
                    }
                    return total;
                }
                else
                {
                    return 4700;
                }
            }
        }
        public int TotalOriginalAmount2B {
            get {
                if (null != _RecycleLoan2)
                {
                    int total = 0;

                    for (int i = 0; i < _RecycleLoan2.Length; i++)
                    {
                        total += _RecycleLoan2[i].Sum;
                    }
                    return total;
                }
                else
                {
                    return 4700;
                }
            }
        }


        OfferBank[] GetOfferBankTableValues(int index)
        {
            if (null != _BankOffers && index <= _BankOffers.Length)
            {
                List<OfferBank> listOfferBank = new List<OfferBank>();
                listOfferBank.Add(new OfferBank(_BankOffers[index].Sum, _BankOffers[index].Type.ToString(),
                        (double)_BankOffers[index].Interest, _BankOffers[index].Time, _BankOffers[index].ReturnSum));
                return listOfferBank.ToArray();
            }
            else
            {
                return new OfferBank[] {
                            new OfferBank(50000, "קבועה צמודה", 3.200, 180, 2244) ,
                            new OfferBank(20000, "פריים", 0.800, 240, 1353),
                            new OfferBank(20000, "פריים", 0.800, 240, 1353),
                            new OfferBank(20000, "פריים", 0.800, 240, 1353),
                            new OfferBank(20000, "פריים", 0.800, 240, 1353),
                            new OfferBank(20000, "פריים", 0.800, 240, 1353),
                            new OfferBank(50000, "קבועה צמודה", 3.200, 180, 2244) ,
                        new OfferBank(50000, "קבועה צמודה", 3.200, 180, 2244)
                        };
            }
        }

        // The values
        public OfferBank[] OfferBankTableValues
        {
            get
            {
                int index = 0;
                return GetOfferBankTableValues(index);
            }
        }

        // The values for table #2
        public OfferBank[] OfferBankTableValues2
        {
            get
            {
                int index = 1;
                return GetOfferBankTableValues(index);
            }
        }

        // The values for table #3
        public OfferBank[] OfferBankTableValues3
        {
            get
            {
                int index = 2;
                return GetOfferBankTableValues(index);
            }
        }

        // Table Section 2.3
        public SectionOriginalLoanKnownTable[] SectionOriginalLoanKnownTableValues
        // TBD - why should we have another original data???
        {  
            get
            {
                if (null != _RecycleLoan2)
                {
                    List<SectionOriginalLoanKnownTable> listOriginalLoanDetailsTable = new List<SectionOriginalLoanKnownTable>();

                    for (int i = 0; i < _RecycleLoan2.Length; i++)
                    {
                        OriginalLoanDetailsTable OriginalLoanDetailsTable = GetOriginalLoanDetails(i);
                        SectionOriginalLoanKnownTable SectionOriginalLoanKnownTable = 
                            new SectionOriginalLoanKnownTable(OriginalLoanDetailsTable.OriginalAmount,
                                OriginalLoanDetailsTable.OriginalProductType, OriginalLoanDetailsTable.OriginalTime,
                                OriginalLoanDetailsTable.TimeLeft, OriginalLoanDetailsTable.RateToday,
                                OriginalLoanDetailsTable.FirstPmt);
                        listOriginalLoanDetailsTable.Add(SectionOriginalLoanKnownTable);
                    }
                    return listOriginalLoanDetailsTable.ToArray();
                }
                else
                {
                    return new SectionOriginalLoanKnownTable[] {
                        new SectionOriginalLoanKnownTable(200000, "פריים", 360, 200, 1.000, 1000),
                        new SectionOriginalLoanKnownTable(200000, "פריים", 300, 140, 3.000, 1000),
                        new SectionOriginalLoanKnownTable(200000, "פריים", 360, 200, 4.000, 1000),
                        new SectionOriginalLoanKnownTable(100000, "פריים", 300, 140, 2.000, 500),
                        new SectionOriginalLoanKnownTable(100000, "פריים", 264, 104, 5.000, 500),
                        new SectionOriginalLoanKnownTable(150000, "פריים", 300, 140, 3.500, 700)
                    };
                }
            }
        }

        //  public OriginalLoanDetailsTable(int originalAmount, string originalProductType, 
        // DateTime dateTaken, int originalTime, 
        // double originalRate, int firstPmt, int timeLeft, double rateToday)

        public OriginalLoanDetailsTable GetOriginalLoanDetails(int index)
        {
            OriginalLoanDetailsTable originalLoanDetailsTable; 

            if (null != _RecycleLoan2 && index < _RecycleLoan2.Length)
            {
                DateTime dt = new DateTime(_RecycleLoan2[index].StartYear, _RecycleLoan2[index].StartMonth, 1);
                int alreadyPassedMonyths = MiscUtilities.CalculateMonthBetweenDates(dt, DateTime.Now);
                int timeLeft = _RecycleLoan2[index].Time - alreadyPassedMonyths;
                originalLoanDetailsTable = new OriginalLoanDetailsTable(_RecycleLoan2[index].Sum,
                    _RecycleLoan2[index].Type.ToString() /*convert from database*/, dt, 
                    _RecycleLoan2[index].Time,  (double)_RecycleLoan2[index].Interest,
                    _RecycleLoan2[index].ReturnSum, timeLeft, (double) _RecycleLoan2[index].Interest /*???*/);
            }
            else
                originalLoanDetailsTable = new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000) ;
            return originalLoanDetailsTable;
        }

        // Table Part 2B
        public OriginalLoanDetailsTable[] OriginalLoanDetailsTableValues
        {
            get
            {
                if (null != _RecycleLoan2)
                {
                    List<OriginalLoanDetailsTable> listOriginalLoanDetailsTable = new List<OriginalLoanDetailsTable>();

                    for (int i = 0; i < _RecycleLoan2.Length; i++)
                    {
                        listOriginalLoanDetailsTable.Add(GetOriginalLoanDetails(i));
                    }
                    return listOriginalLoanDetailsTable.ToArray();
                }
                else
                {
                    return new OriginalLoanDetailsTable[] {
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000),
                        new OriginalLoanDetailsTable(200000, "פריים", DateTime.Now, 360, 1.000, 1000, 200, 2.000)
                    };
                }
            }
        }

        // Need to keep track of how many pages have already been created
        public int RecommendedAndStressTestStructuresCounter { get; set; }

        // The values
        public SingleStructureDataTable[] SingleStructureDataTableValues
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(630000, "פריים", 1.300, 360, 906) ,
                    new SingleStructureDataTable(386000, "קבועה לא צמודה", 4.180, 204, 2647),
                    new SingleStructureDataTable(244000, "משתנה כל חמש שנים לא צמודה", 3.760, 240, 1448)
                };
            }
        }

        // The values
        public SingleStructureDataTable[] SingleStructureDataTableValues2
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(130000, "פריים", 1.300, 360, 906) ,
                    new SingleStructureDataTable(286000, "קבועה לא צמודה", 1.180, 204, 2647),
                    new SingleStructureDataTable(244000, "משתנה כל חמש שנים לא צמודה", 3.760, 240, 1448)
                };
            }
        }

        // The values
        public SingleStructureDataTable[] SingleStructureDataTableValues3
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(630000, "פריים", 1.300, 360, 305) ,
                    new SingleStructureDataTable(50000, "קבועה לא צמודה", 8.180, 204, 2647),
                    new SingleStructureDataTable(244000, "משתנה כל חמש שנים לא צמודה", 3.760, 120, 1448)
                };
            }
        }

        // The values
        public SingleStructureDataTable[] StressTestStructureDataTableValues
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(530000, "פריים", 2.300, 300, 906) ,
                    new SingleStructureDataTable(386500, "קבועה לא צמודה", 4.580, 105, 2647),
                    new SingleStructureDataTable(144000, "משתנה כל חמש שנים לא צמודה", 3.160, 220, 1448)
                };
            }
        }

        // The values
        public SingleStructureDataTable[] StressTestStructureDataTableValues2
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(530040, "פריים", 3.300, 350, 906) ,
                    new SingleStructureDataTable(286500, "קבועה לא צמודה", 4.180, 115, 2647),
                    new SingleStructureDataTable(144000, "משתנה כל חמש שנים לא צמודה", 7.160, 320, 2448)
                };
            }
        }

        // The values
        public SingleStructureDataTable[] StressTestStructureDataTableValues3
        {
            get
            {
                return new SingleStructureDataTable[] {
                    new SingleStructureDataTable(430000, "פריים", 2.300, 100, 706) ,
                    new SingleStructureDataTable(381500, "קבועה לא צמודה", 4.580, 105, 2647),
                    new SingleStructureDataTable(149000, "משתנה כל חמש שנים לא צמודה", 8.160, 443, 1498)
                };
            }
        }
        // The values
        public SingleStructureFirstFiveYearsTable[] SingleStructureFirstFiveYearsTableValues
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(906, 231980, 54368, 16300, 38020, 270000),
                    new SingleStructureFirstFiveYearsTable(2647, 299285, 158893, 72180, 86715, 386000),
                    new SingleStructureFirstFiveYearsTable(1448, 198667, 86875, 41760, 45033, 244000),
                    new SingleStructureFirstFiveYearsTable(5001, 730232, 300000, 130760, 169768, 900000)
                };
            }
        }

        // The values
        public SingleStructureFirstFiveYearsTable[] StressStructureFirstFiveYearsTableValues
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(806, 231180, 14368, 16340, 38010, 270900),
                    new SingleStructureFirstFiveYearsTable(1647, 291285, 18893, 72180, 86115, 389000),
                    new SingleStructureFirstFiveYearsTable(2448, 193667, 83875, 41260, 45043, 249000),
                    new SingleStructureFirstFiveYearsTable(3001, 720232, 310000, 150760, 162768, 909000)
                };
            }
        }

        // The values
        public SingleStructureFirstFiveYearsTable[] StressStructureFirstFiveYearsTableValues2
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(106, 231180, 14368, 16340, 31010, 270910),
                    new SingleStructureFirstFiveYearsTable(2647, 291285, 18893, 72110, 36115, 389000),
                    new SingleStructureFirstFiveYearsTable(2443, 193667, 83875, 41210, 25043, 249000),
                    new SingleStructureFirstFiveYearsTable(4001, 720232, 310000, 152760, 262768, 809000)
                };
            }
        }

        // The values
        public SingleStructureFirstFiveYearsTable[] StressStructureFirstFiveYearsTableValues3
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(1806, 531180, 14368, 16340, 38010, 270900),
                    new SingleStructureFirstFiveYearsTable(1647, 291285, 18893, 72380, 86115, 389000),
                    new SingleStructureFirstFiveYearsTable(448, 193667, 83875, 41260, 45043, 249000),
                    new SingleStructureFirstFiveYearsTable(3001, 120232, 310000, 150760, 162768, 909000)
                };
            }
        }

        // The values
        public AppendixPaidUntilTodayTable[] AppendixPaidUntilTodayTableValues
        {
            get
            {
                return new AppendixPaidUntilTodayTable[] {
                    new AppendixPaidUntilTodayTable(770, 172091, 113936, 58155, 200000),
                    new AppendixPaidUntilTodayTable(1085, 162392, 67711, 94681, 200000),
                    new AppendixPaidUntilTodayTable(955, 152773, 91991, 60782, 200000),
                    new AppendixPaidUntilTodayTable(485, 72574, 22006, 50568, 100000),
                    new AppendixPaidUntilTodayTable(625, 100045, 52729, 47316, 100000)
                };
            }
        }

        // The values
        public AppendixExpectedFuturePaymentTable[] AppendixExpectedFuturePaymentTableValues
        {
            get
            {
                return new AppendixExpectedFuturePaymentTable[] {
                    new AppendixExpectedFuturePaymentTable(185516, 43671, 141845, 141845),
                    new AppendixExpectedFuturePaymentTable(160976, 24834, 136142, 127871),
                    new AppendixExpectedFuturePaymentTable(190966, 51748, 139218, 139218),
                    new AppendixExpectedFuturePaymentTable(71941, 7679, 64262, 60426),
                    new AppendixExpectedFuturePaymentTable(65029, 12345, 52684, 52684)
                };
            }
        }

        // The values
        public AppendixTotalPaymentTable[] AppendixTotalPaymentTableValues
        {
            get
            {
                return new AppendixTotalPaymentTable[] {
                    new AppendixTotalPaymentTable(357607, 157607, 200000, 200000),
                    new AppendixTotalPaymentTable(323368, 92546, 230823, 200000),
                    new AppendixTotalPaymentTable(343739, 143739, 200000, 200000),
                    new AppendixTotalPaymentTable(144515, 29685, 114830, 100000),
                    new AppendixTotalPaymentTable(165074, 65074, 100000, 100000)
                };
            }
        }

        // The values
        public SingleStructureFirstFiveYearsTable[] SingleStructureFirstFiveYearsTableValues2
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(906, 231980, 54368, 16300, 38020, 270000),
                    new SingleStructureFirstFiveYearsTable(2647, 299285, 158893, 72180, 86715, 386000),
                    new SingleStructureFirstFiveYearsTable(1448, 198667, 86875, 41760, 45033, 244000),
                    new SingleStructureFirstFiveYearsTable(5001, 730232, 300000, 130760, 169768, 900000)
                };
            }
        }

        // The values
        public SingleStructureFirstFiveYearsTable[] SingleStructureFirstFiveYearsTableValues3
        {
            get
            {
                return new SingleStructureFirstFiveYearsTable[] {
                    new SingleStructureFirstFiveYearsTable(906, 231980, 54368, 16300, 38020, 270000),
                    new SingleStructureFirstFiveYearsTable(2647, 299285, 158893, 72180, 86715, 386000),
                    new SingleStructureFirstFiveYearsTable(1448, 198667, 86875, 41760, 45033, 244000),
                    new SingleStructureFirstFiveYearsTable(5001, 730232, 300000, 130760, 169768, 900000)
                };
            }
        }

        // The values
        public SingleStructureTotalPaymentTable[] SingleStructureTotalPaymentTableValues
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(630000, 630000, 121938, 751938),
                    new SingleStructureTotalPaymentTable(872000, 872000, 321057, 1193057),
                    new SingleStructureTotalPaymentTable(598000, 598000, 253660, 851660),
                    new SingleStructureTotalPaymentTable(2100000, 2100000, 696656, 2796656)
                };
            }
        }

        // The values
        public SingleStructureTotalPaymentTable[] StressStructureTotalPaymentTableValues
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(630050, 530000, 321938, 51938),
                    new SingleStructureTotalPaymentTable(772000, 272000, 121057, 13057),
                    new SingleStructureTotalPaymentTable(198000, 498000, 353660, 85160),
                    new SingleStructureTotalPaymentTable(4100000, 1100000, 796656, 276656)
                };
            }
        }

        // The values
        public SingleStructureTotalPaymentTable[] StressStructureTotalPaymentTableValues2
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(330000, 63000, 12138, 751938),
                    new SingleStructureTotalPaymentTable(172000, 87200, 32057, 1193057),
                    new SingleStructureTotalPaymentTable(298000, 59800, 23660, 851660),
                    new SingleStructureTotalPaymentTable(3100000, 210000, 66656, 2796656)
                };
            }
        }

        // The values
        public SingleStructureTotalPaymentTable[] StressStructureTotalPaymentTableValues3
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(630000, 130000, 131938, 751938),
                    new SingleStructureTotalPaymentTable(872000, 172000, 321057, 1193057),
                    new SingleStructureTotalPaymentTable(598000, 298000, 253111, 851660),
                    new SingleStructureTotalPaymentTable(2100000, 2100000, 696111, 2796656)
                };
            }
        }

        public SingleStructureTotalPaymentTable[] SingleStructureTotalPaymentTableValues2
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(630000, 630000, 121938, 751938),
                    new SingleStructureTotalPaymentTable(872000, 872000, 321057, 1193057),
                    new SingleStructureTotalPaymentTable(598000, 598000, 253660, 851660),
                    new SingleStructureTotalPaymentTable(2100000, 2100000, 696656, 2796656)
                };
            }
        }

        public SingleStructureTotalPaymentTable[] SingleStructureTotalPaymentTableValues3
        {
            get
            {
                return new SingleStructureTotalPaymentTable[] {
                    new SingleStructureTotalPaymentTable(630000, 630000, 121938, 751938),
                    new SingleStructureTotalPaymentTable(872000, 872000, 321057, 1193057),
                    new SingleStructureTotalPaymentTable(598000, 598000, 253660, 851660),
                    new SingleStructureTotalPaymentTable(2100000, 2100000, 696656, 2796656)
                };
            }
        }

        // The values
        public RecommendedStructure[] RecommendedStructureTableValues1
        {
            get
            {
                int currentIndex = 0; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    RecommendedStructure[] reconendations = MiscUtilities.CalculateRecommendedStructure(c, reportData);
                    return reconendations;

                    //bool isHebrew = cultureInfo.Name.Equals(MiscConstants.HEBREW_STR);
                    //// RecommendedStructure(int amount, string productType, double rate, int time, int pmt)
                    //return new RecommendedStructure[] {
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[0].optAmt),
                    //        (isHebrew) ? c.opts[0].product.hebrewName : c.opts[0].product.name ,
                    //        c.opts[0].optRateFirstPeriod, (int) c.opts[0].optTime, Convert.ToInt32(c.opts[0].optPmt), c.opts[0].product) ,
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[1].optAmt),
                    //        (isHebrew) ? c.opts[1].product.hebrewName : c.opts[1].product.name ,
                    //        c.opts[1].optRateFirstPeriod, (int) c.opts[1].optTime, Convert.ToInt32(c.opts[1].optPmt), c.opts[1].product),
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[2].optAmt),
                    //        (isHebrew) ? c.opts[2].product.hebrewName : c.opts[2].product.name ,
                    //        c.opts[2].optRateFirstPeriod, (int) c.opts[2].optTime, Convert.ToInt32(c.opts[2].optPmt), c.opts[2].product)
                    //};

                }
                else
                {
                        return new RecommendedStructure[] {
                        new RecommendedStructure(630000, "פריים", 1.300, 360, 906, new GenericProduct(new ProductID(1, "testingProduct1"))) ,
                        new RecommendedStructure(386000, "קבועה לא צמודה", 4.180, 204, 2647, new GenericProduct(new ProductID(2, "testingProduct2"))),
                        new RecommendedStructure(244000, "משתנה כל חמש שנים לא צמודה", 3.760, 240, 1448, new GenericProduct(new ProductID(3, "testingProduct3")))
                    };
                }
            }
        }

       
        public RecommendedStructure[] RecommendedStructureTableValues2
        {
            
            get
            {
                int currentIndex = 1; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts)
                {
                    Composition c = reportData.compositions[currentIndex];
                    RecommendedStructure[] reconendations = MiscUtilities.CalculateRecommendedStructure(c, reportData);
                    return reconendations;

            
                    //return new RecommendedStructure[] {
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[0].optAmt),
                    //        (isHebrew) ? c.opts[0].product.hebrewName : c.opts[0].product.name ,
                    //        c.opts[0].optRateFirstPeriod, (int) c.opts[0].optTime, Convert.ToInt32(c.opts[0].optPmt), c.opts[0].product) ,
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[1].optAmt),
                    //        (isHebrew) ? c.opts[1].product.hebrewName : c.opts[1].product.name ,
                    //        c.opts[1].optRateFirstPeriod, (int) c.opts[1].optTime, Convert.ToInt32(c.opts[1].optPmt), c.opts[1].product),
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[2].optAmt),
                    //        (isHebrew) ? c.opts[2].product.hebrewName : c.opts[2].product.name ,
                    //        c.opts[2].optRateFirstPeriod, (int) c.opts[2].optTime, Convert.ToInt32(c.opts[2].optPmt), c.opts[2].product)
                    //};

                }
                else
                {
                    return new RecommendedStructure[] {
                        new RecommendedStructure(270000, "פריים", 1.300, 360, 906, new GenericProduct(new ProductID(1, "testingProduct1"))) ,
                        new RecommendedStructure(558000, "קבועה לא צמודה", 4.230, 216, 3695, new GenericProduct(new ProductID(1, "testingProduct1"))),
                        new RecommendedStructure(72000, "משתנה כל חמש שנים צמודה", 2.990, 228, 400, new GenericProduct(new ProductID(1, "testingProduct1")))
                    };
                }

            }
        }

        public RecommendedStructure[] RecommendedStructureTableValues3
        {
            
            get
            {
                int currentIndex = 2; // TBD - should be used proprly...

                if (null != reportData && null != reportData.compositions && null != reportData.compositions[currentIndex] &&
                    null != reportData.compositions[currentIndex].opts) {
                    Composition c = reportData.compositions[currentIndex];
                    RecommendedStructure[] reconendations = MiscUtilities.CalculateRecommendedStructure(c, reportData);
                    return reconendations;

                    //bool isHebrew = cultureInfo.Name.Equals(MiscConstants.HEBREW_STR);
                    //    // RecommendedStructure(int amount, string productType, double rate, int time, int pmt)
                    //    return new RecommendedStructure[] {
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[0].optAmt), 
                    //        (isHebrew) ? c.opts[0].product.hebrewName : c.opts[0].product.name , 
                    //        c.opts[0].optRateFirstPeriod, (int) c.opts[0].optTime, Convert.ToInt32(c.opts[0].optPmt), c.opts[0].product) ,
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[1].optAmt),
                    //        (isHebrew) ? c.opts[1].product.hebrewName : c.opts[1].product.name ,
                    //        c.opts[1].optRateFirstPeriod, (int) c.opts[1].optTime, Convert.ToInt32(c.opts[1].optPmt), c.opts[1].product),
                    //    new RecommendedStructure(Convert.ToInt32(c.opts[2].optAmt),
                    //        (isHebrew) ? c.opts[2].product.hebrewName : c.opts[2].product.name ,
                    //        c.opts[2].optRateFirstPeriod, (int) c.opts[2].optTime, Convert.ToInt32(c.opts[2].optPmt), c.opts[2].product)
                    //};
                }
                else
                {
                    return new RecommendedStructure[] {
                        new RecommendedStructure(270000, "פריים", 1.300, 360, 906, new GenericProduct(new ProductID(1, "testingProduct1"))) ,
                        new RecommendedStructure(257000, "קבועה לא צמודה", 4.230, 216, 1702, new GenericProduct(new ProductID(1, "testingProduct1"))),
                        new RecommendedStructure(373000, "קבועה לא צמודה", 3.280, 228, 2393, new GenericProduct(new ProductID(1, "testingProduct1")))
                    };
                }
            }
        }

        // run the report creation
        public static string RunTheReport(LongReportDataObject lrdo, string pdfFileName, CultureInfo cultureInfo)
        {
            // Create a new instance of WisorReportManager class, set filename, culture and data
            WisorReportManager reportManager = new WisorReportManager(pdfFileName, cultureInfo, lrdo);
            // create necessary pages
            reportManager.CreateCoverPage();
            reportManager.CreateDisclaimerPage(1);
            reportManager.CreateTableOfContentsPage(2);
            reportManager.CreateIntroductionPage(3);
            reportManager.CreateOrderSummaryPage(4);
            reportManager.CreateOrderSummaryPage2(5);
            bool state = reportManager.CreateRefinanceAnalysisPage2B(6);
            if (state) // Only for Refinance type with original loan known
            {
                int structuresQuantity = reportManager.CreateRecommendedStructuresPage(7);
                if (structuresQuantity == 1)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(8, 1, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(9);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(10, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(11);
                        reportManager.CreatePart5SummaryPage(12);
                        reportManager.CreateAppendixesPage(13); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(14);
                        reportManager.CreateAmortisationSchedulePage2(15);
                        reportManager.CreateAmortisationSchedulePage3(16);
                        reportManager.CreateEmptyStructuresPage(17);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(10, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(11, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(12);
                        reportManager.CreatePart5SummaryPage(13);
                        reportManager.CreateAppendixesPage(14); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(15);
                        reportManager.CreateAmortisationSchedulePage2(16);
                        reportManager.CreateAmortisationSchedulePage3(17);
                        reportManager.CreateEmptyStructuresPage(18);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(12, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(14, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(15);
                        reportManager.CreatePart5SummaryPage(16);
                        reportManager.CreateAppendixesPage(17); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(18);
                        reportManager.CreateAmortisationSchedulePage2(19);
                        reportManager.CreateAmortisationSchedulePage3(20);
                        reportManager.CreateEmptyStructuresPage(21);
                    }

                }
                if (structuresQuantity == 2)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(8, 1, false);
                    reportManager.CreateRecommendedOrStressTestStructures(9, 2, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(10);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(12);
                        reportManager.CreatePart5SummaryPage(13);
                        reportManager.CreateAppendixesPage(14); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(15);
                        reportManager.CreateAmortisationSchedulePage2(16);
                        reportManager.CreateAmortisationSchedulePage3(17);
                        reportManager.CreateEmptyStructuresPage(18);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(13);
                        reportManager.CreatePart5SummaryPage(14);
                        reportManager.CreateAppendixesPage(15); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(16);
                        reportManager.CreateAmortisationSchedulePage2(17);
                        reportManager.CreateAmortisationSchedulePage3(18);
                        reportManager.CreateEmptyStructuresPage(19);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(14);
                        reportManager.CreatePart5SummaryPage(15);
                        reportManager.CreateAppendixesPage(16); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(17);
                        reportManager.CreateAmortisationSchedulePage2(18);
                        reportManager.CreateAmortisationSchedulePage3(19);
                        reportManager.CreateEmptyStructuresPage(20);
                    }
                }
                if (structuresQuantity == 3)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(8, 1, false);
                    reportManager.CreateRecommendedOrStressTestStructures(9, 2, false);
                    reportManager.CreateRecommendedOrStressTestStructures(10, 3, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(11);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(12, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(13);
                        reportManager.CreatePart5SummaryPage(14);
                        reportManager.CreateAppendixesPage(15); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(16);
                        reportManager.CreateAmortisationSchedulePage2(17);
                        reportManager.CreateAmortisationSchedulePage3(18);
                        reportManager.CreateEmptyStructuresPage(19);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(12, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(14);
                        reportManager.CreatePart5SummaryPage(15);
                        reportManager.CreateAppendixesPage(16); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(17);
                        reportManager.CreateAmortisationSchedulePage2(18);
                        reportManager.CreateAmortisationSchedulePage3(19);
                        reportManager.CreateEmptyStructuresPage(20);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(12, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(14, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(15);
                        reportManager.CreatePart5SummaryPage(16);
                        reportManager.CreateAppendixesPage(17); // Only for Refinance with original loan known and inserted 
                        reportManager.CreateAmortisationSchedulePage1(18);
                        reportManager.CreateAmortisationSchedulePage2(19);
                        reportManager.CreateAmortisationSchedulePage3(20);
                        reportManager.CreateEmptyStructuresPage(21);
                    }
                }


            }
            else // If Part 2B not exist then page numbers continues with page number 6. Also page "Appendixes" not created.
            {
                int structuresQuantity = reportManager.CreateRecommendedStructuresPage(6);
                if (structuresQuantity == 1)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(7, 1, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(8);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(9, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(10);
                        reportManager.CreatePart5SummaryPage(11);
                        reportManager.CreateAmortisationSchedulePage1(12);
                        reportManager.CreateAmortisationSchedulePage2(13);
                        reportManager.CreateAmortisationSchedulePage3(14);
                        reportManager.CreateEmptyStructuresPage(15);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(9, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(10, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(11);
                        reportManager.CreatePart5SummaryPage(12);
                        reportManager.CreateAmortisationSchedulePage1(13);
                        reportManager.CreateAmortisationSchedulePage2(14);
                        reportManager.CreateAmortisationSchedulePage3(15);
                        reportManager.CreateEmptyStructuresPage(16);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(14);
                        reportManager.CreatePart5SummaryPage(15);
                        reportManager.CreateAmortisationSchedulePage1(16);
                        reportManager.CreateAmortisationSchedulePage2(17);
                        reportManager.CreateAmortisationSchedulePage3(18);
                        reportManager.CreateEmptyStructuresPage(19);
                    }

                }
                if (structuresQuantity == 2)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(7, 1, false);
                    reportManager.CreateRecommendedOrStressTestStructures(8, 2, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(9);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(10, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(11);
                        reportManager.CreatePart5SummaryPage(12);
                        reportManager.CreateAmortisationSchedulePage1(13);
                        reportManager.CreateAmortisationSchedulePage2(14);
                        reportManager.CreateAmortisationSchedulePage3(15);
                        reportManager.CreateEmptyStructuresPage(16);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(10, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(11, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(12);
                        reportManager.CreatePart5SummaryPage(13);
                        reportManager.CreateAmortisationSchedulePage1(14);
                        reportManager.CreateAmortisationSchedulePage2(15);
                        reportManager.CreateAmortisationSchedulePage3(16);
                        reportManager.CreateEmptyStructuresPage(17);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(10, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(11, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(13);
                        reportManager.CreatePart5SummaryPage(14);
                        reportManager.CreateAmortisationSchedulePage1(15);
                        reportManager.CreateAmortisationSchedulePage2(16);
                        reportManager.CreateAmortisationSchedulePage3(17);
                        reportManager.CreateEmptyStructuresPage(18);
                    }
                }
                if (structuresQuantity == 3)
                {
                    reportManager.CreateRecommendedOrStressTestStructures(7, 1, false);
                    reportManager.CreateRecommendedOrStressTestStructures(8, 2, false);
                    reportManager.CreateRecommendedOrStressTestStructures(9, 3, false);

                    int stressStructuresQuantity = reportManager.CreateStressTestWisorRecommendedStructuresPage(10);
                    if (stressStructuresQuantity == 1)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);

                        reportManager.CreateStressTestPriceOffersPage(12);
                        reportManager.CreatePart5SummaryPage(13);
                        reportManager.CreateAmortisationSchedulePage1(14);
                        reportManager.CreateAmortisationSchedulePage2(15);
                        reportManager.CreateAmortisationSchedulePage3(16);
                        reportManager.CreateEmptyStructuresPage(17);
                    }
                    if (stressStructuresQuantity == 2)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 2, true);

                        reportManager.CreateStressTestPriceOffersPage(13);
                        reportManager.CreatePart5SummaryPage(14);
                        reportManager.CreateAmortisationSchedulePage1(15);
                        reportManager.CreateAmortisationSchedulePage2(16);
                        reportManager.CreateAmortisationSchedulePage3(17);
                        reportManager.CreateEmptyStructuresPage(18);
                    }
                    if (stressStructuresQuantity == 3)
                    {
                        reportManager.CreateRecommendedOrStressTestStructures(11, 1, true);
                        reportManager.CreateRecommendedOrStressTestStructures(12, 2, true);
                        reportManager.CreateRecommendedOrStressTestStructures(13, 3, true);

                        reportManager.CreateStressTestPriceOffersPage(14);
                        reportManager.CreatePart5SummaryPage(15);
                        reportManager.CreateAmortisationSchedulePage1(16);
                        reportManager.CreateAmortisationSchedulePage2(17);
                        reportManager.CreateAmortisationSchedulePage3(18);
                        reportManager.CreateEmptyStructuresPage(19);
                    }
                }
            }
            // save to file
            reportManager.SavePDFDocument();

            //or only //reportManger.CreateFullReport();
            return reportManager.documentFileName;
        }


    }

    public class PriorLiabilitiesData
    {
        public int MonthlyReturn { get; set; } // currency
        public DateTime Date { get; set; }
        public PriorLiabilitiesData(int monthlyReturn, DateTime date)
        {
            MonthlyReturn = monthlyReturn;
            Date = date;
        }
    }

    public class FutureReleases
    {
        public int Amount { get; set; } // currency
        public DateTime Date { get; set; }
        public FutureReleases(int amount, DateTime date)
        {
            Amount = amount;
            Date = date;
        }
    }

    public class AmortisationData
    {
        public AmortisationData()
        {
            AmortisationTable = new AmortisationTable[MiscConstants.MAX_LOAN_TIME / 12];
            for (int i = 0; i < AmortisationTable.Length; i++)
            {
                AmortisationTable[i] = new AmortisationTable();
            }
        }

        public AmortisationTable[] AmortisationTable { get; set; }
    }

    public class AmortisationTable
    {
        public int Year { get; set; } 
        public double RemainingAmount { get; set; }
        public double PaidSoFar { get; set; }
        public double MonthlyPmt { get; set; }
        public AmortisationTable(int year = 0, double remainingAmount = 0, double paidSoFar = 0, double monthlyPmt = 0)
        {
            Year = year;
            RemainingAmount = remainingAmount;
            PaidSoFar = paidSoFar;
            MonthlyPmt = monthlyPmt;
        }
    }

    class SectionSingleStructureGraphs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public SectionSingleStructureGraphs(int px, int py)
        {
            X = px;
            Y = py;
        }
    }

    public class FixedSavings
    {
        public int Amount { get; set; } // currency
        public int /*string*/
        SavingType { get; set; }
        public decimal /*double*/ AverageYield { get; set; }
        public bool /*string*/ Liquid { get; set; }
        public string LiquidString { get { return Translator.TranslateBoolToYesOrNo(Liquid); } }
        
        public FixedSavings(int amount, int /*string*/ savingType, decimal /*double*/ averageYield, bool /*string*/ liquid)
        {
            Amount = amount;
            SavingType = savingType;
            AverageYield = averageYield;
            Liquid = liquid;
        }
    }

    public class OfferBank
    {
        public int Amount { get; set; } // currency
        public string ProductType { get; set; }
        public double OfferRate { get; set; }
        public int OfferTime { get; set; }
        public int OfferPmt { get; set; }
        
        public OfferBank(int amount, string productType, double offerRate, int offerTime, int offerPmt)
        {
            Amount = amount;
            ProductType = productType;
            OfferRate = offerRate;
            OfferTime = offerTime;
            OfferPmt = offerPmt;
        }
    }

    public class SingleStructureDataTable
    {
        public int Amount { get; set; } 
        public string ProductType { get; set; }
        public double Rate { get; set; }
        public int Time { get; set; }
        public int Pmt { get; set; }

        public SingleStructureDataTable(int amount, string productType, double rate, int time, int pmt)
        {
            Amount = amount;
            ProductType = productType;
            Rate = rate;
            Time = time;
            Pmt = pmt;
        }
    }

    public class SingleStructureFirstFiveYearsTable
    {
        public int Amount { get; set; }
        public int PrincipalPaid { get; set; }
        public int InterestPaid { get; set; }
        public int TotalPaid { get; set; }
        public int RemainingAmount { get; set; }
        public int NextPmt { get; set; }

        public SingleStructureFirstFiveYearsTable(int nextPmt, int remainingAmount, int totalPaid, int interestPaid, int principalPaid, int amount)
        {
            Amount = amount;
            PrincipalPaid = principalPaid;
            InterestPaid = interestPaid;
            TotalPaid = totalPaid;
            RemainingAmount = remainingAmount;
            NextPmt = nextPmt;
        }
    }

    public class AppendixPaidUntilTodayTable
    {
        public int Amount { get; set; }
        public int PrincipalPaid { get; set; }
        public int InterestPaid { get; set; }
        public int TotalPaid { get; set; }
        public int NextPmt { get; set; }

        public AppendixPaidUntilTodayTable(int nextPmt, int totalPaid, int interestPaid, int principalPaid, int amount)
        {
            Amount = amount;
            PrincipalPaid = principalPaid;
            InterestPaid = interestPaid;
            TotalPaid = totalPaid;
            NextPmt = nextPmt;
        }
    }

    public class AppendixExpectedFuturePaymentTable
    {
        public int Amount { get; set; }
        public int PrincipalPaid { get; set; }
        public int InterestPaid { get; set; }
        public int TotalPaid { get; set; }

        public AppendixExpectedFuturePaymentTable(int totalPaid, int interestPaid, int principalPaid, int amount)
        {
            Amount = amount;
            PrincipalPaid = principalPaid;
            InterestPaid = interestPaid;
            TotalPaid = totalPaid;
        }
    }

    public class AppendixTotalPaymentTable
    {
        public int Amount { get; set; }
        public int PrincipalPaid { get; set; }
        public int InterestPaid { get; set; }
        public int TotalPaid { get; set; }

        public AppendixTotalPaymentTable(int totalPaid, int interestPaid, int principalPaid, int amount)
        {
            Amount = amount;
            PrincipalPaid = principalPaid;
            InterestPaid = interestPaid;
            TotalPaid = totalPaid;
        }
    }

    public class SingleStructureTotalPaymentTable
    {
        public int Amount { get; set; }
        public int PrincipalPaid { get; set; }
        public int InterestPaid { get; set; }
        public int TotalPaid { get; set; }

        public SingleStructureTotalPaymentTable(int totalPaid, int interestPaid, int principalPaid, int amount)
        {
            Amount = amount;
            PrincipalPaid = principalPaid;
            InterestPaid = interestPaid;
            TotalPaid = totalPaid;
        }
    }

    public class RecommendedStructure
    {
        public int Amount { get; set; }
        public string ProductType { get; set; }
        public double Rate { get; set; }
        public int Time { get; set; }
        public int Pmt { get; set; }
        public GenericProduct Product { get; set; }

        public LenderCalculationData lenderCalculationData { get; set; }


        public RecommendedStructure(int amount, string productType, double rate, int time, int pmt, GenericProduct product)
        {
            Amount = amount;
            ProductType = productType;
            Rate = rate;
            Time = time;
            Pmt = pmt;
            Product = product;
        }
    }

    //For Refinance only (Section 2.3)
    public class SectionOriginalLoanKnownTable
    {
        public int OriginalAmount { get; set; }
        public string ProductType { get; set; }
        public int OriginalTime { get; set; }
        public int RemainingTime { get; set; }
        public double RateToday { get; set; }
        public int PmtToday { get; set; }

        public SectionOriginalLoanKnownTable(int originalAmount, string productType, int originalTime,  int remainingTime,  double rateToday, int pmtToday)
        {
            OriginalAmount = originalAmount;
            ProductType = productType;
            OriginalTime = originalTime;
            RemainingTime = remainingTime;
            RateToday = rateToday;
            PmtToday = pmtToday;
        }
    }

    //For Refinance only PART 2B
    public class OriginalLoanDetailsTable
    {
        public int OriginalAmount { get; set; }
        public string OriginalProductType { get; set; }
        public DateTime DateTaken { get; set; }
        public int OriginalTime { get; set; }
        public double OriginalRate { get; set; }
        public int FirstPmt { get; set; }

        public double RateToday { get; set; }
        public int TimeLeft { get; set; }

        public OriginalLoanDetailsTable(int originalAmount, string originalProductType, DateTime dateTaken, int originalTime, double originalRate, int firstPmt, int timeLeft, double rateToday)
        {
            OriginalAmount = originalAmount;
            OriginalProductType = originalProductType;
            DateTaken = dateTaken;
            OriginalTime = originalTime;
            OriginalRate = originalRate;
            FirstPmt = firstPmt;

            RateToday = rateToday;
            TimeLeft = timeLeft;
        }
    }

    public class LenderCalculationData
    {
        public int totalPay { get; set; }
        public int totalProfit { get; set; }
        public double totalProfitPercantage { get; set; }

        public LenderCalculationData(int TotalPay, int TotalProfit, double TotalProfitPercantage)
        {
            totalPay = TotalPay;
            totalProfit = TotalProfit;
            totalProfitPercantage = TotalProfitPercantage;
        }
    }

    // methods to translate values to strings
    class Translator
    {
        public static string TranslateBoolToYesOrNo(bool value)
        {
            string translated = "";

            switch (value)
            {
                case true:
                    translated = "כן";
                    break;
                case false:
                    translated = "לא";
                    break;
                default:
                    break;
            }

            return translated;
        }

        public static string TranslateBoolToYesOrNo2(bool value)
        {
            string translated = "";

            switch (value)
            {
                case true:
                    translated = "יש";
                    break;
                case false:
                    translated = "אין";
                    break;
                default:
                    break;
            }

            return translated;
        }
    }

    class Utils
    {

        public static int CalculateSum(List<DebtsReleases2> Debts)
        {
            int sum = 0;
            foreach (var d in Debts)
                sum += d.Sum;
            return sum;
        }

        public static PriorLiabilitiesData[] GetLiabilities(List<DebtsReleases2> Debts)
        {
            List<PriorLiabilitiesData> priorLiabilitiesData = new List<PriorLiabilitiesData>();

            foreach (var d in Debts) {
                priorLiabilitiesData.Add(new PriorLiabilitiesData(d.Sum, new DateTime(d.Year, d.Month, 1)));
            }

            return priorLiabilitiesData.ToArray();
        }

        public static FutureReleases[] GetFutureReleases(List<DebtsReleases2> Debts)
        {
            List<FutureReleases> futureReleases = new List<FutureReleases>();

            foreach (var d in Debts)
            {
                futureReleases.Add(new FutureReleases(d.Sum, new DateTime(d.Year, d.Month, 1)));
            }

            return futureReleases.ToArray();
        } 

        // TBD: Omri - where should be the parameters retrieve from?
        public static AmortisationData GetAmortisation(double amount
            /*, indices Indices, double rate, double inflation, uint time*/)
        {
            AmortisationData amortisationData = null;
            // TBD Omri - where are those come from
            indices Indices = indices.MADAD;
            double rate = 0.01;
            double inflation = 0.001;
            uint time = 360;

            Calculations.CalculateLuahSilukinAllResults(Indices, rate, inflation,
                    amount, time, DateTime.Now /*dateLoanTaken*/, ref amortisationData,
                    null /*env*/, false /*IsBank*/);

            return amortisationData;
        }

        public static FixedSavings[] GetFixedSavings(List<Saving2> Savings)
        {
            List<FixedSavings> fixedSavings = new List<FixedSavings>();

            foreach (var d in Savings)
            {
                fixedSavings.Add(new FixedSavings(d.AmountSaved, d.SavingsType, d.Yield, d.Liquidity));
            }
            return fixedSavings.ToArray();
        }

        public static Offer2[] _BankOffers = null;

        public static void ReadAllBanksData(List<Offer2> offers)
        {
            if (null == _BankOffers)
            {
                _BankOffers = offers.ToArray();
            }
        }

        public static RecycleLoan2[] _RecycleLoan2 = null;
        public static string _RecycleLoanBankName;
        public static void ReadAllRecycleCheckData(RecycleCheck2 RecycleCheck2)
        {
            if (null == _RecycleLoan2)
            {
                _RecycleLoanBankName = RecycleCheck2.RecycleLoanBankName;
                List<RecycleLoan2> RecycleLoan2 = RecycleCheck2.RecycleLoans;
                _RecycleLoan2 = RecycleLoan2.ToArray();
            }
        }

        //public static string GetBankNameByID(int bankID)
        //{
        //    BankBL.GetAllBanks();
        //}

        //public static Offers2 GetBankOffers()
        //{

        //    var bankOffers = Data.Offers.OffersList.GroupBy(o => o.BankId);
        //    foreach (var bankOffer in bankOffers)
        //    {
        //        var newOffer = new OffersOrCheck
        //        {
        //            Type = 1,
        //            ApprovingBank = bankOffer.First().BankId
        //        };

        //        foreach (var offer in bankOffer)
        //        {
        //            newOffer.Tracks.Add(new Track
        //            {
        //                Amount = offer.Sum,
        //                Inflation = offer.Indexation,
        //                PMT = offer.ReturnSum,
        //                Time = offer.Time,
        //                Rate = offer.Interest,
        //                TrackType = offer.Type
        //            });
        //        }

        //}

    }
}
