using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLibrary.DataObjects
{
    public class LongReportDataObject
    {
        // The header' section
        public string MainHeader2 {  get { return "סיכום פרטי הלוואה"; } } // Summary of Loan Information: סיכום פרטי הלוואה
        public string MainHeader21 { get { return "פרטי עסקה"; } } // Transaction details: סיכום פרטי הלוואה
        public string MainHeader22 { get { return "העדפות סיכון ונזילות אישיות"; } } //העדפות סיכון ונזילות אישיות
                                                                                    
        
        // section 2.

        public string       OrderNumberTitle { get { return "מספר הזמנה"; }  } // מספר הזמנה
        public string       OrderNumberValue { get { return "1234"; } } 

        public string       EmailTitle { get { return "דואל שם משתמש"; } }
        public string       EmailValue { get { return "ShukyIsMe@abcd.com"; } } // דוא"ל שם משתמש

        // section 2.1.
        public string TransactionTypeTitle { get { return "סוג עסקה"; } }
        public string TransactionTypeValue { get { return "רכישת נכס יד ראשונה"; } }
   
        public string MortgagedPropertyValueTitle { get   { return " שווי נכס משועבד"; } }
        public int MortgagedPropertyValueValue { get { return 123; } }

        public string DesireLoanAmountTitle { get { return " סכום הלוואה רצוי"; } }
        public int DesireLoanAmountValue { get { return 1234567; } }

        public string ExistingCapitalTitle { get { return " הון עצמי קיים"; } }
        public int ExistingCapitalValue { get { return 567890; } }

        public string DesireMonthlyReturnTitle { get { return "  החזר חודשי רצוי"; } } 
        public int DesireMonthlyReturnValue { get { return 12334; } }

        public string EstimateAccessToProprtyTitle { get { return "כניסה משוערת לנכס"; } }
        public DateTime EstimateAccessToProprtyValue { get { return DateTime.Now; } }

        public string CurrentlyRentalPayTitle { get { return " שכירות משולמת כיום"; } }
        public int CurrentlyRentalPayValue { get { return 5643; } }

        public string NumberOfBorrowersTitle { get { return " מספר לווים בעסקה"; } }
        public int NumberOfBorrowersValue { get { return 2; } }

        public string YoungestBorrowerAgeTitle { get { return " גיל הלווה הצעיר"; } }
         public int YoungestBorrowerAgeValue { get { return 46; } }

        public string TotalNetIncomeTitle { get {return " הכנסות נטו";}}
        public int TotalNetIncomeValue { get { return 123456; } }

        public string PriorLiabilitiesTitle { get { return " התחייבויות קיימות"; } }
        public string PriorLiabilitiesValue { get { return "יש"; } }

        public string FutureReleasesTitle { get { return " שיחרורים עתידיים"; } }
        public string FutureReleasesValue { get { return "יש"; } }

        public string FixedSavingsTitle { get { return " חסכונות קבועים"; } }
        public string FixedSavingsValue { get { return "יש"; } }

        // section 2.1. table
        // The PriorLiabilitiesTable description (right table)
        // The titles
        public string PriorLiabilitiesTableTitle { get { return " התחייבויות קיימות"; } }
        public string PriorLiabilitiesTableMonthlyReturnTitle { get { return " החזר חודשי"; } }
        public string PriorLiabilitiesTableEndDateTitle { get { return " תאריך סיום"; } }
        // The values
        public PriorLiabilitiesData[] PriorLiabilitiesTableValues { get {
                return new PriorLiabilitiesData[] {
                    new PriorLiabilitiesData(400, new DateTime(2002, 11, 1)) ,
                    new PriorLiabilitiesData(440, new DateTime(2024, 04, 1)),
                    new PriorLiabilitiesData(500, new DateTime(2022, 03, 1))
                };
            }
        }



        // The FutureReleasesTable description (middle table)
        // The titles
        public string FutureReleasesTableTitle { get { return " שיחרורים עתידיים"; } }
        public string FutureReleasesTableAmountTitle { get { return " סכום"; } }
        public string FutureReleasesTableDateTitle { get { return " תאריך שחרור"; } }

        // The values
        public FutureReleases[] FutureReleasesTableValues
        {
            get
            {
                return new FutureReleases[] {
                    new FutureReleases(50000, new DateTime(2022, 08, 1)) ,
                    new FutureReleases(20000, new DateTime(2021, 12, 1)),
                    new FutureReleases(30000, new DateTime(2023, 05, 1))
                };
            }
        }


        // The FixedSavingsTable description (left table)
        // The titles
        public string FixedSavingsTableTitle { get { return " חסכונות קבועים"; } }
        public string FixedSavingsTableAmountTitle { get { return " סכום"; } }
        public string FixedSavingsTableSavingTypeTitle { get { return " סוג חיסכון"; } }
        public string FixedSavingsTableAverageYieldTitle { get { return " תשואה ממוצעת"; } }
        public string FixedSavingsTableLiquidTitle { get { return " נזיל"; } }

        // The values
        public FixedSavings[] FixedSavingsTableValues
        {
            get
            {
                return new FixedSavings[] {
                    new FixedSavings(400, "פקמ", 2, true ) ,
                    new FixedSavings(550, "מניות", 3, false),
                    new FixedSavings(450, "אחר", 4, true)
                };
            }
        }


        public string FinincingRateTitle { get { return " שיעור מימון"; } }
        public double FinincingRateValue { get { return 0.123; } }

        public string ReturnOnIncomeRatioTitle { get { return " יחס החזר מהכנסה"; } }
        public double ReturnOnIncomeRatioValue { get { return 45.123; } }

        // section 2.2.
        public string ExpectedPropertyHoldingTimeTitle { get { return " זמן אחזקה צפוי "; } }
        public string ExpectedPropertyHoldingTimeValue { get { return "זמן ארוך"; } }

        public string StabilityAndLiquidityTitle { get { return " יציבות ונזילות"; } }
        public string StabilityAndLiquidityValue { get { return "שקט נפשי, החזר יציב"; } }

        public string ExpectedChangesTitle { get { return " שינויים צפויים"; } }
        public string ExpectedChangesValue { get { return "צפי להחזר חודשי חדש בעוד 2 שנים החזר חדש צפוי 7,000 שח"; } }
    }

    public class PriorLiabilitiesData
    {
        public int      MonthlyReturn { get; set; } // currency
        public DateTime Date { get; set; }
        public PriorLiabilitiesData(int monthlyReturn, DateTime date)
        {
            MonthlyReturn = monthlyReturn;
            Date = date;
        }
    }

    public class FutureReleases
    {
        public int      Amount { get; set; } // currency
        public DateTime Date { get; set; }
        public FutureReleases(int amount, DateTime date)
        {
            Amount = amount;
            Date = date;
        }
    }

    public class FixedSavings
    {
        public int Amount { get; set; } // currency
        public string SavingType { get; set; }
        public double AverageYield { get; set; }
        public bool Liquid { get; set; }
        public FixedSavings(int amount, string savingType, double averageYield, bool liquid)
        {
            Amount = amount;
            SavingType = savingType;
            AverageYield = averageYield;
            Liquid = liquid;
        }
    }

    // methods to translate values to strings
    public class Translator {
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

            return Translator.ReverseStringByLanguage(translated);
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

            return Translator.ReverseStringByLanguage(translated);
        }

        // support languages...
        static public string GetStringByLanguage(string name, CultureInfo culture = null)
        {
            if (null == culture)
                culture = new CultureInfo("he-IL", true);

            string tmp = WisorLibrary.Language.ResourceManager.GetString(name, culture);
            if (!string.IsNullOrEmpty(tmp))
            {
                if (culture.Name.Equals("he-IL"))
                {
                    tmp = new string(tmp.Reverse().ToArray());
                }
                else
                {

                }
            }
            else
            {
                tmp = name;
            }

            return tmp;
        }

        // reverse - RTL
        static public string ReverseStringByLanguage(string name, CultureInfo culture = null)
        {
            if (null == culture)
                culture = new CultureInfo("he-IL", true);

            string tmp = name;
            if (!string.IsNullOrEmpty(tmp))
            {
                if (culture.Name.Equals("he-IL"))
                {
                    tmp = new string(tmp.Reverse().ToArray());
                }
                else
                {

                }
            }
            else
            {
                tmp = name;
            }

            return tmp;
        }

    }
}
