using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
//using System.Windows.Forms;

namespace WisorLib
{

    public class MiscConstants
    {
        public const int        UNDEFINED_INT = -1;
        public const uint       UNDEFINED_UINT = 0;
        public static string LOAN_AMOUNT = "Loan amount";
        public static string MONTHLY_PAYMENT = "Desired monthly payment";
        public static string PROPERTY_VALUE = "Property value";
        public static string YEARLY_INCOME = "Yearly income";
        public static string AGE = "Borrower age";
        public static string MIN_STR = "min";
        public static string MAX_STR = "max";
        public static string EQUAL_STR = "=";

    }

    /// <summary>
    /// ////////////////////////////
    /// </summary>
    /// 

    public class myCntrl
    {
        public string ID { get; set; }

        public myCntrl(string id)
        {
            ID = id;
        }
    }

    public class fields : myCntrl
    {
        //public string ID { get; set; }
        public bool status { get; set; }
        public string value { get; set; }
        public bool legal { get; set; }
        public int min { get; set; }
        public int max { get; set; }

        public string toString()
        {
            return ("cntrl ID:" + ID + " status: " + status + " value: " + value + " legal: " + legal);
        }

        // constructor
        public fields(string id = "", string value = "", int min = MiscConstants.UNDEFINED_INT, 
            int max = MiscConstants.UNDEFINED_INT) : base (id)
        {
            this.ID = id;
            this.value = value;
            this.legal = true;
            this.status = true;
            this.min = min;
            this.max = min;
        }

        //// constructor
        //public cntrl(string id)
        //{
        //    this.ID = id;
        //    this.value = id;
        //    this.legal = true;
        //    this.status = true;
        //    this.min = MiscConstants.UNDEFINED_INT;
        //    this.max = MiscConstants.UNDEFINED_INT;
        //}


    }


    /// <summary>
    /// /////////////////////////////////////
    /// </summary>

    public class InMemList
    {
        private List<myCntrl> _memoryControls;

        public int Length { get { return _memoryControls.Count; }  }

        public InMemList()
        {
            _memoryControls = new List<myCntrl>();
        }

        /// <summary>
        /// Manage the controls in the memory
        /// </summary>


        public List<myCntrl> GetMemoryControls()
        {
            return _memoryControls;
        }

        public void MemoryControlsCleanup()
        {
            _memoryControls.Clear();
        }

        public void MemoryControlsAdd(myCntrl c)
        {
            _memoryControls.Add(c);
        }

        public myCntrl MemoryControlsGetObject(string id)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            return obj;
        }

        public string MemoryControlsGetValue(string id)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            return obj.value;
        }

        public void MemoryControlsUpdateRange(string id, int min, int max)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            if (null != obj)
            {
                obj.min = min;
                obj.max = max;
            }
        }

        public void ManageControlsUpdateLegal(string id, bool value)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            if (null != obj)
            {
                obj.legal = value;
            }
        }

        public void MemoryControlsUpdateStatus(string id, bool status)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            if (null != obj)
            {
                obj.status = status;
            }
        }

        public void MemoryControlsUpdateText(string id, string text)
        {
            myCntrl obj = _memoryControls.Find(FindControlPredicate(new myCntrl(id)));
            if (null != obj)
            {
                obj.value = text;
            }
        }


        static Predicate<myCntrl> FindControlPredicate(myCntrl c)
        {
            return delegate (myCntrl cntrl)
            {
                return cntrl.ID.ToLower() == c.ID.ToLower();
            };
        }

    }

    /// <summary>
    /// ///////////////////////
    /// </summary>


    public class FileUtils
    {

        public static InMemList LoadXMLFileData(string filename)
        {
            XDocument doc = null;
            InMemList cntrls = new InMemList();

            if (File.Exists(filename))
            {
                doc = XDocument.Load(filename);

                // loop all the items in the document
                foreach (XElement item in from c in doc.Descendants("item") select c)
                {
                    string type = item.Element("type").Value;
                    string id = item.Element("name").Value;
                    string defaultValue = null != item.Element("default") ? item.Element("default").Value : id;
                    string value = null != defaultValue ? defaultValue : item.Element("name").Value;
                    int min = MiscConstants.UNDEFINED_INT, max = MiscConstants.UNDEFINED_INT;

                    // loop all the range values within the item
                    foreach (string tc in from c in item.Descendants("ToCheck") select c)
                    {
                        if (tc.StartsWith(MiscConstants.MIN_STR))
                        {
                            min = Int32.Parse(tc.Substring(tc.IndexOf(MiscConstants.EQUAL_STR) + 1));
                        }
                        if (tc.StartsWith(MiscConstants.MAX_STR))
                        {
                            max = Int32.Parse(tc.Substring(tc.IndexOf(MiscConstants.EQUAL_STR) + 1));
                        }
                    }

                    // add to the memory
                    cntrls.MemoryControlsAdd(new fields(id, value, min, max));
                }

            }
            else
            {
                Console.WriteLine("LoadFileData file: {0} does not exists!!!", filename);
             }

            return cntrls;
        }

        //string orderID = "", double loanAmtWanted = 0, double monthlyPmtWanted = 0,
        //        uint propertyValue = 0, uint income = 0, uint youngestLenderAge = 0

        public enum MortgageType { None = 0, FirstTimeBuyer = 1 , Reordering = 2, Other = 3};
        public enum PaymentType { None = 0, FullAmortizatio = 1, InterestOnly = 2, Ballon = 3};
        public enum MortgagProduct { None = 0 };
        public enum CreditClass { None = 0, Exelent = 1, Good = 2, medium = 3, low = 4 };

        public class loanDetails : myCntrl
        {
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
                uint yearlyIncome, uint borrowerAge) : base (id)
                //,
                //// optional
                //MortgageType mortgageType = MortgageType.None, PaymentType paymentType = PaymentType.None, DateTime? dateTaken = null,
                //MortgagProduct mortgagProduct = MortgagProduct.None, double interestRate = MiscConstants.UNDEFINED_INT,
                //uint termInYears = MiscConstants.UNDEFINED_UINT, CreditClass creditClass = CreditClass.None)
            {
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

        }

            public static InMemList LoadCSVFileData(string filename, InMemList fieldsDef)
        {
            FileStream      fs = File.OpenRead(filename);
            StreamReader    reader = new StreamReader(fs);
            InMemList       entities = new InMemList();

            if (File.Exists(filename))
            {
                while (! reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (values.Length == fieldsDef.Length)
                    {
                        //entities.MemoryControlsAdd(new loanDetails(id, value, min, max));
                    }
                    else
                    {
                        Console.WriteLine("NOTICE: the definition file contain: " + fieldsDef.Length + " parameters while the loan have: " + values.Length);
                    }

                    //listA.Add(values[0]);
                    //listB.Add(values[1]);

                    //// add to the memory
                    //cntrls.MemoryControlsAdd(new cntrl(id, value, min, max));
                }
            }
            else
            {
                Console.WriteLine("LoadCSVFileData file: {0} does not exists!!!", filename);
            }
   
            return entities;
        }

}
    }