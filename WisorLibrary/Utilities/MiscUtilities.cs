using CommonObjects;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using WisorLibrary.ReportApplication;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;
using static WisorLib.Options;
using static WisorLibrary.ReportApplication.Utils;

namespace WisorLibrary.Utilities
{

    public class MiscUtilities
    {
        // TBD: either read it from file
        public static int GetLoanID()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 1000);
            return randomNumber;
        }

        public static uint GetSequenceID()
        {
            // TBD
            return 1;
        }

        public static string CreateOutputFilename(string orderid, double loanAmtWanted, double monthlyPmtWanted, uint sequenceID, string additionalName)
        {
            string seq = (MiscConstants.UNDEFINED_UINT == sequenceID) ? MiscConstants.UNDEFINED_STRING : MiscConstants.SEQ_STR + sequenceID + MiscConstants.SEQ_STR;
            string add = (String.IsNullOrEmpty(additionalName)) ? MiscConstants.UNDEFINED_STRING : MiscConstants.NAME_SEP_CHAR + additionalName + MiscConstants.NAME_SEP_CHAR;
            string fn = MiscUtilities.GetOutputDirectory() + Path.DirectorySeparatorChar +
                Share.CustomerName + seq + /* orderid + MiscConstants.NAME_SEP_CHAR + */
                 orderid.ToString() + MiscConstants.NAME_SEP_CHAR +
                loanAmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR + add +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;

            return fn;
        }

        public static string GetOutputDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory + MiscConstants.OUTPUT_DIR;
        }

        public static string GetCurrentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static bool SetRatesFilename()
        {
            Share.theSelectionType = SelectionType.ReadRates;
            bool rc;
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            rc = Rates.SetRatesFile(Share.RatesFileName, Share.BankRatesFileName,
                Share.SecondPeriodFilename, Share.SecondPeriodBankRatesFileName);

            return rc;
        }

        public static string GetHistoricRatesFilename()
        {
            //string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            //string filename = dir + MiscConstants.HISTORIC_FILE;
            string dir = GetDataDirectory() + Path.DirectorySeparatorChar;
            string filename = MiscUtilities.GetFilename(dir + Share.HistoricFileName, dir + MiscConstants.HISTORIC_FILE);
            return filename;
            //bool rc = HistoricRate.SetFilename(filename);
            //return rc;
        }

        public static string GetHistoricRatesBBBRFilename()
        {
            string dir = GetDataDirectory() + Path.DirectorySeparatorChar;
            string filename = MiscUtilities.GetFilename(dir + MiscConstants.HISTORIC_BBBR_FILE, MiscConstants.UNDEFINED_STRING);
            return filename;
        }


        public static bool SetRiskAndLiquidityFilename()
        {
            //string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            //string filename = dir + MiscConstants.RISK_LIQUIDITY_FILE;
            string filename = MiscUtilities.GetFilename(Share.RiskAndLiquidityFileName, MiscConstants.RISK_LIQUIDITY_FILE);

            bool rc = RiskLiquidityObject.SetFilename(filename);

            return rc;
        }

        public static bool FindRiskLiquidity(RiskLiquidityValue riskLiquidityValue)
        {
            bool rc = RiskLiquidityObject.Instance.FindRiskLiquidity(riskLiquidityValue);
            return rc;
        }


        public static string GetImagesDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.IMAGES_DIR + Path.DirectorySeparatorChar;
        }

        public static string GetFilenameFromUser()
        {
            string filename = null;
            bool toShow = false;
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            // the directory alrady set ....
            dir = MiscConstants.UNDEFINED_STRING;

            // get the file from the user
            string header = null;
            if (SelectionType.ReadCretiria == Share.theSelectionType)
            {
                header = "Select the critiria and order for the loan";
                toShow = Share.shouldShowCriteriaSelectionWindow;
                filename = dir + Share.CriteriaFileName; // MiscConstants.CRETIRIA_FILE; 
            }
            else if (SelectionType.ReadProducts == Share.theSelectionType)
            {
                header = "Select the products file";
                toShow = Share.shouldShowProductSelectionWindow;
                filename = dir + Share.ProductsFileName; // MiscConstants.PRODUCTS_FILE; 
            }
            else if (SelectionType.ReadLoansFile == Share.theSelectionType)
            {
                header = "Select the loans file";
                toShow = Share.shouldShowLoansSelectionWindow;
                filename = dir + Share.LoansFileName; // MiscConstants.LOAN_FILE; 
            }
            else if (SelectionType.ReadRates == Share.theSelectionType)
            {
                header = "Select the rates file";
                toShow = Share.shouldShowRatesSelectionWindow;
                filename = dir + Share.RatesFileName; // MiscConstants.RATES_FILE; 
            }
            else
            {
                WindowsUtilities.loggerMethod("ERROR: SampleData unrecognized SelectionType: " + Share.theSelectionType.ToString());
                return filename;
            }

            string nfn = GetSpecificFilename(filename, Share.CustomerName);

            if (toShow)
            {
                // Displays an OpenFileDialog so the user can select a file
                /*Microsoft.Win32.*/
                System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.Filter = "all files (*.*)|*.*|XML files (*.xml)|*.xml";
                openFileDialog1.FileName = nfn.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1 /*Path.DirectorySeparatorChar*/);
                openFileDialog1.Title = header;

                // Show the Dialog.
                if (DialogResult.OK == openFileDialog1.ShowDialog())
                {
                    nfn = filename = openFileDialog1.FileName; //    string fn = @"../../Gui.xml";
                }
            }

            return nfn;
        }

        static public string GetSpecificFilename(string filename, string customerName)
        {
            string fin = filename;

            // check if there is a specific file to this customer
            // leave it for now....
            //if (!String.IsNullOrEmpty(customerName))
            //{
            //    string fn = System.IO.Path.GetFileName(filename);
            //    string dirc = System.IO.Path.GetDirectoryName(filename);
            //    string filename2 = dirc + System.IO.Path.DirectorySeparatorChar + Share.CustomerName + fn;

            //    if (File.Exists(filename2))
            //    {
            //        fin = filename2;
            //    }
            //}
            return fin;
        }


        public static LoanList GetLoansFromFile(FieldList fields)
        {
            //WindowsUtilities.loggerMethod("Select the loans file");
            LoanList loans = null;

            Share.theSelectionType = SelectionType.ReadLoansFile;
            //string filename = @"..\..\..\Data\Test Cases.xlsx";
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            string filename = /*dir + */Share.LoansFileName; // MiscConstants.LOAN_FILE; 
            if (Share.shouldShowLoansSelectionWindow)
                filename = MiscUtilities.GetFilenameFromUser();
            if (null != filename)
            {
                try
                {
                    //loans = FileUtils.LoadCSVFileData(filename, fields);
                    loans = FileUtils.LoadLoansFileData(filename, fields);
                }
                catch (Exception e)
                {
                    WindowsUtilities.loggerMethod("ERROR: GetLoansFromFile got Exception: " + e.ToString());
                }
            }

            if (null == loans || 0 >= loans.Count)
            { // use the default values
                WindowsUtilities.loggerMethod("NOTICE: failed to upload loans from file: " + filename);
            }
            else
                WindowsUtilities.loggerMethod("Successfully Upload " + loans.Count + " loans from file: " +
                    filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));

            return loans;
        }



        public static List<ChosenComposition> OrderCompositionListByBorrower(List<ChosenComposition> compositions,
            ListSortDirection order = ListSortDirection.Ascending)
        {
            List<ChosenComposition> SortedSelectedCompositions =
                (ListSortDirection.Descending == order) ?
                    compositions.OrderByDescending(o => o.borrowerPay).ToList() :
                    compositions.OrderBy(o => o.borrowerPay).ToList();
            return SortedSelectedCompositions;
        }

        public static List<ChosenComposition> OrderCompositionListByBank(List<ChosenComposition> compositions,
            ListSortDirection order = ListSortDirection.Descending)
        {
            List<ChosenComposition> SortedSelectedCompositions =
              (ListSortDirection.Descending == order) ?
                    compositions.OrderByDescending(o => o.bankPay).ToList() :
                    compositions.OrderBy(o => o.bankPay).ToList();
            return SortedSelectedCompositions;
        }

        // cleanup the redundant composition - empty, duplicate, only win-win
        internal static Composition[] CleanComposition(Composition[] composition)
        {
            List<Composition> compL = new List<Composition>();

            foreach (Composition comp in composition)
            {
                if (!compL.Exists(Composition.CompositionPredicate(comp)))
                    // ensure its a win-win composition
                    //if (comp.IsWinWin)           leave it for debug purpose
                    compL.Add(comp);
            }

            // ensure the composition number is obeyed
            if (compL.Count > MiscConstants.MAX_NUM_OF_COMBINATION_TO_SELECT)
            {
                // TBD - how to decide which combination to preffer?
                for (int i = compL.Count; i > MiscConstants.MAX_NUM_OF_COMBINATION_TO_SELECT; i--)
                {
                    // to be on the safe side....
                    int loc = i - 1;
                    if (0 <= loc)
                        compL.RemoveAt(loc);
                }

            }
            return compL.ToArray();
        }

        public static Predicate<OneOptType> OneOptTypePredicate(GenericProduct o)
        {

            return delegate (OneOptType opt)
            {
                if (null == o)
                    return true; // avoid to add a null value

                return
                    opt.product.productID.numberID == o.productID.numberID;
            };
        }

        // check if a product exisit in the list
        internal static bool DoesProductExists(GenericProduct[] products, OneOptType[] options)
        {
            bool rc = true;
            List<OneOptType> ol = options.ToList();

            foreach (GenericProduct prod in products)
            {
                if (!ol.Exists(OneOptTypePredicate(prod)))
                    rc = false;
            }

            return rc;
        }

        public static List<ChosenComposition> OrderCompositionListByProfit(List<ChosenComposition> compositions,
            ListSortDirection order = ListSortDirection.Descending)
        {
            List<ChosenComposition> SortedSelectedCompositions =
                (ListSortDirection.Descending == order) ?
                    compositions.OrderByDescending(o => o.profit).ToList() :
                    compositions.OrderBy(o => o.profit).ToList();
            return SortedSelectedCompositions;
        }


        public static uint CalculateLuahSilukinBank(double rateFirstPeriod, double rateSecondPeriod,
            int productFirstTimePeriod, /*indices*/ double productIndexUsedFirstTimePeriod,
            double optAmt, int optTime, double optPmt,
            double indexFirstPeriod, double indexSecondPeriod, int optType, bool printOrNo)
        {
            return Calculations.CalculateLuahSilukinBank(rateFirstPeriod, rateSecondPeriod,
                 productFirstTimePeriod, productIndexUsedFirstTimePeriod,
                 optAmt, (int)optTime, optPmt,
                 indexFirstPeriod, indexSecondPeriod, optType, printOrNo);
        }

        public static uint CalculateMonthlyPayment(uint loanAmount, uint propertyValue, uint yearlyIncome, uint borrowerAge)
        {
            uint desiredMonthlyPayment = 0;

            desiredMonthlyPayment = (uint)Math.Round((double)yearlyIncome / 3);
            return desiredMonthlyPayment;
        }

        public static double GetIndexRateForOption(indices indic)
        {
            double index = 0;

            switch (indic)
            {
                case indices.MADAD:
                    index = MiscConstants.MADAD_Inflation;
                    break;
                case indices.PRIME:
                    index = 0;
                    break;
                default:
                    index = 0;
                    //WindowsUtilities.loggerMethod("NOTICE: GetIndexRateForOption undefined for indic: " + indic);
                    break;
            }

            return index;
        }

        /*
         * For the remaining loan amount we should calculate the luch-silukin precisly according to the known historic rate
         * The function calculate the avarage rate if the rate was change several time in the same month
         */
        //public static double GetHistoricIndexRateForPeriod(indices indic, DateTime dateLoanTaken)
        //{
        //    double index = 0;

        //    switch (indic)
        //    {
        //        case indices.MADAD:
        //            index = MiscConstants.MADAD_Inflation;
        //            break;
        //        case indices.PRIME:
        //            // ensure the file was loaded
        //            //if (null == HistoricRate.Instance || !HistoricRate.Instance.Status)
        //            //    MiscUtilities.SetHistoricRatesFilename();

        //            // get the entire month values
        //            index = HistoricRate.GetHistoricValues(indic, dateLoanTaken, dateLoanTaken.AddMonths(1));
        //            break;
        //        case indices.CPI:
        //            index = 0;
        //            break;
        //        case indices.FED:
        //            index = 0;
        //            break;
        //        case indices.LIBOR:
        //            index = 0;
        //            break;
        //        case indices.EUROBOR:
        //            index = 0;
        //            break;
        //        case indices.BBBR:
        //            index = 0;
        //            break;
        //        case indices.MAKAM:
        //            index = 0;
        //            break;
        //        default:
        //            index = 0;
        //            //WindowsUtilities.loggerMethod("NOTICE: GetIndexRateForOption undefined for indic: " + indic);
        //            break;
        //    }

        //    return index;
        //}

        public static double GetHistoricIndexRateForDate(indices indic, DateTime dateLoanTaken,
            double originalRate, out double margin, bool IsBank)
        {
            double index = 0;
            margin = MiscConstants.UNDEFINED_DOUBLE;

            if (indices.PRIME == indic)
            {
                if (IsBank)
                {
                    margin = MiscConstants.BANK_PRIME_RATE_FACTOR - originalRate;
                    index = originalRate;
                }
                else
                {
                    // ensure the file was loaded
                    index = HistoricRate.GetHistoricIndex(indic, dateLoanTaken);
                    // should calculate the actuall rate the borrower got , based on the Prime + Actuall rate
                    margin = originalRate - index;
                }
            }
            else if (indices.BBBR == indic)
            {
                // ensure the file was loaded
                index = HistoricRateBBBR.GetHistoricIndex(indic, dateLoanTaken);

                // should calculate the actuall rate the borrower got , based on the Prime + Actuall rate
                margin = originalRate - index;

                //if (IsBank)
                //{
                //    margin = 0 /*MiscConstants.BANK_BBBR_RATE_FACTOR - originalRate*/;
                //    index = originalRate;
                //}
                //else
                //{
                //    // ensure the file was loaded
                //    index = HistoricRateBBBR.GetHistoricIndex(indic, dateLoanTaken);

                //    // should calculate the actuall rate the borrower got , based on the Prime + Actuall rate
                //    margin = originalRate - index;
                //}

            }
            else
            {
                index = originalRate;
                margin = 0;
            }
            //  case indices.MADAD:
            //      MiscConstants.MADAD_Inflation

            return index;
        }


        public static int CalculateYearsBetweenDates(DateTime fromDate, DateTime toDate)
        {
            int years = toDate.Year - fromDate.Year;
            return years;
        }

        public static int CalculateMonthBetweenDates(DateTime fromDate, DateTime toDate)
        {
            //int yearOfDateLoanTaken = DateTaken.Year;
            int months = ((toDate.Year * 12) + toDate.Month) - (((int)fromDate.Year * 12) + (int)fromDate.Month);
            return months;
        }

        public static int CalculateDatesBetweenDates(DateTime fromDate, DateTime toDate)
        {
            //int yearOfDateLoanTaken = DateTaken.Year;
            int dayes = (int) /*Math.Abs*/((toDate - fromDate).TotalDays);
            return dayes;
        }

        // if 'date' is between 'from' and 'to', return th days between 'date' and 'from'
        public static int IsDateInBetween(DateTime date, DateTime fromDate, DateTime toDate)
        {
            int days = -1;

            if (date >= fromDate && date <= toDate)
            {
                days = CalculateDatesBetweenDates(fromDate, date);
            }

            return days;
        }

        public static DateTime ConvertDate(string str)
        {
            string formatedDate;

            // dirty change still simpler...
            return ConvertDate2(str, out formatedDate);

            //CultureInfo provider = CultureInfo.InvariantCulture;
            //DateTime dt = default(DateTime);

            //// cleanup the minutes
            //int ind = str.IndexOf(MiscConstants.SPACE_STR);
            //if (0 < ind)
            //    str = str.Remove(str.IndexOf(MiscConstants.SPACE_STR)).Trim();

            //try
            //{
            //    //CultureInfo culture = new CultureInfo("he-IL"); // ("en -US");
            //    //dt = DateTime.Parse(str, culture);
            //    dt = DateTime.ParseExact(str, MiscConstants.DATE_FORMAT, provider);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR ConvertDate Exception: " + ex.ToString() + " for str: " + str);
            //}

            //return dt;
        }


        public static DateTime ConvertDate2(string str, out string formatedDate/*, CultureInfo provider*/)
        {
            DateTime value;
            formatedDate = MiscConstants.UNDEFINED_STRING;
            string currFormat = MiscConstants.DATE_FORMAT_US;

            if (!DateTime.TryParse(str, out value))
            {
                //string[] formats = { "MM/dd/yyyy" };
                if (!DateTime.TryParseExact(str, MiscConstants.DATE_FORMAT_US, new CultureInfo("en-US"),
                       DateTimeStyles.None, out value))
                {
                    currFormat = MiscConstants.DATE_FORMAT;
                    if (!DateTime.TryParseExact(str, MiscConstants.DATE_FORMAT, new CultureInfo("he-IL"),
                       DateTimeStyles.None, out value))
                    {
                        value = DateTime.Now;
                    }
                    //else
                    //{
                    //    value = value;
                    //}
                }
                //else
                //{
                //    dateTaken = value;
                //}
            }
            //else
            //{
            //    dateTaken = value;
            //}

            // formatedDate = value.ToString(currFormat);
            currFormat = MiscConstants.DATE_FORMAT; // "MMM dd yyyy";
            formatedDate = value.ToString(currFormat);
            return value.Date;

            DateTime dt = default(DateTime);
            CultureInfo hebrewCulture = new CultureInfo("he-IL", true);
            CultureInfo englishCulture = new CultureInfo("en-US", true);

            // cleanup the minutes
            int ind = str.IndexOf(MiscConstants.SPACE_STR);
            if (0 < ind)
                str = str.Remove(str.IndexOf(MiscConstants.SPACE_STR)).Trim();

            try
            {
                // Parse the english date string into a date time object
                dt = DateTime.Parse(str);
                // dt = DateTime.ParseExact(str, MiscConstants.DATE_FORMAT, provider);

                // Obtain the formatted string based on the culture passed in.
                string hebrewFormattedDate = dt.ToString(hebrewCulture);
                string englishFormattedDate = dt.ToString(englishCulture);

                // Assign the new formatted date object back to dateToCheck
                dt = DateTime.Parse(hebrewFormattedDate, hebrewCulture);
                dt = DateTime.Parse(englishFormattedDate, englishCulture);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ConvertDate Exception: " + ex.ToString() + " for str: " + str);
            }

            return dt;
        }

        /// <summary>
        /// /////////////////
        /// </summary>
        /// <returns></returns>
        /// 


        public static bool CheckConsistency(FinalLimitPoint[] points)
        {
            if (null == points ||
                null == points[(int)Options.limitPointsLetters.A] ||
                null == points[(int)Options.limitPointsLetters.B] ||
                !points[(int)Options.limitPointsLetters.A].Status ||
                !points[(int)Options.limitPointsLetters.B].Status)
            {
                Console.WriteLine("ERROR: CheckConsistency failed on illegal finalLimitPoints.");
                return false;
            }
            return true;
        }

        public static bool LoadXMLConfigurationFile(string filename, bool isFullConfigFilename)
        {
            string fullFilename = MiscConstants.UNDEFINED_STRING, dir = MiscConstants.UNDEFINED_STRING;

            if (isFullConfigFilename)
            {
                fullFilename = filename;
                dir = System.IO.Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
            }
            else
            {
                dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Share.DataDirectory + Path.DirectorySeparatorChar;
                fullFilename = dir + filename;
            }
            bool rc = false;

            try
            {
                if (File.Exists(fullFilename))
                {
                    WindowsUtilities.loggerMethod("LoadXMLConfigurationFile file: " + fullFilename);

                    XmlDocument doc = new XmlDocument();

                    //load up the xml from the location 
                    doc.Load(fullFilename);

                    if (0 < doc.DocumentElement.ChildNodes.Count)
                        rc = true;

                    // cycle through each child noed 
                    foreach (XmlNode child in doc.DocumentElement.ChildNodes)
                    {
                        foreach (XmlNode node in child)
                        {
                            switch (child.Name/*.ToLower()*/)
                            {
                                case MiscConstants.CUSTOMER_NAME:
                                    Share.CustomerName = node.Value;
                                    break;
                                case MiscConstants.MARKET:
                                    markets market = (markets)Enum.Parse(typeof(markets), node.Value, true);
                                    RunEnvironment.SetMarket(market);
                                    break;
                                case MiscConstants.CRETIRIA_FILENAME:
                                    Share.CriteriaFileName = dir + node.Value;
                                    break;
                                case MiscConstants.LOAN_FILENAME:
                                    Share.LoansFileName = dir + node.Value;
                                    break;
                                case MiscConstants.RATES_FILENAME:
                                    Share.RatesFileName = dir + node.Value;
                                    break;
                                case MiscConstants.BANK_RATES_FILENAME:
                                    Share.BankRatesFileName = dir + node.Value;
                                    break;
                                case MiscConstants.SECOND_PERIOD_RATES_FILENAME:
                                    Share.SecondPeriodFilename = dir + node.Value;
                                    break;
                                case MiscConstants.SECOND_PERIOD_BANK_RATES_FILENAME:
                                    Share.SecondPeriodBankRatesFileName = dir + node.Value;
                                    break;
                                case MiscConstants.HISTORIC_FILENAME:
                                    Share.HistoricFileName = dir + node.Value;
                                    break;
                                case MiscConstants.COMBINATIONS_FILE:
                                    Share.CombinationFileName = dir + node.Value;
                                    break;
                                case MiscConstants.COMBINATIONS_FILE_2_PRODUCTS_IN_COMBINATION:
                                    Share.Products2InCombinationFileName = dir + node.Value;
                                    break;
                                case MiscConstants.RISK_LIQUIDITY_FILENAME:
                                    Share.RiskAndLiquidityFileName = dir + node.Value;
                                    break;
                                case MiscConstants.PRODUCTS_FILENAME:
                                    Share.ProductsFileName = dir + node.Value;
                                    break;
                                case MiscConstants.RISK_FACTOR:
                                    Share.RiskFactor = System.Convert.ToDouble(node.Value);
                                    break;
                                case MiscConstants.LIQUIDITY_FACTOR:
                                    Share.LiquidityFactor = System.Convert.ToDouble(node.Value);
                                    break;
                                case MiscConstants.BENEFIT_FACTOR:
                                    Share.BenefitFactor = System.Convert.ToDouble(node.Value);
                                    break;
                                case MiscConstants.BENEFIT_THRESHOLD:
                                    Share.ProductBeneficialScoreCriteria = System.Convert.ToUInt32(node.Value);
                                    break;
                                case MiscConstants.MAX_COMBINATIONS:
                                    Share.MaxCombinationNumber = System.Convert.ToUInt32(node.Value);
                                    break;
                                case MiscConstants.SHOULD_STORE_REPORT_AS_LONG_PDF:
                                    Share.shouldCreateLongPDFReport = "yes" == node.Value ? true : false;
                                    // ensure the directory realy exists
                                    if (Share.shouldCreateLongPDFReport)
                                    {
                                        string fn = MiscUtilities.GetReportFileName("tempID", FileType.HTML);
                                        if (!Directory.Exists(Path.GetDirectoryName(fn)))
                                            Directory.CreateDirectory(Path.GetDirectoryName(fn));
                                    }
                                    break;
                                case MiscConstants.SHOULD_STORE_REPORT_AS_SHORT_PDF:
                                    Share.shouldCreateShortPDFReport = "yes" == node.Value ? true : false;
                                    // ensure the directory realy exists
                                    if (Share.shouldCreateShortPDFReport)
                                    {
                                        string fn = MiscUtilities.GetReportFileName("tempID", FileType.PDF);
                                        if (!Directory.Exists(Path.GetDirectoryName(fn)))
                                            Directory.CreateDirectory(Path.GetDirectoryName(fn));
                                    }
                                    break;
                                case MiscConstants.SHOULD_STORE_REPORT_IN_DB:
                                    Share.ShouldStoreInDB = "yes" == node.Value ? true : false;
                                    break;
                                case MiscConstants.FROM_TO_LINES_TO_LOAD_LOANS:
                                    //<from_line>,<to_line>
                                    string[] lines = node.Value.Split(MiscConstants.COMMA);
                                    if (!String.IsNullOrEmpty(lines[0]))
                                        Share.LoansLoadFromLine = System.Convert.ToUInt32(lines[0]) /*- 1*/; // the line index starts from zero
                                    if (!String.IsNullOrEmpty(lines[1]))
                                        Share.LoansLoadToLine = System.Convert.ToUInt32(lines[1]) /*- 1*/; // the line index starts from zero
                                    break;
                                case MiscConstants.FROM_IDS_LOAD_LOANS:
                                    Share.LoansLoadIDsFromLine = node.Value;
                                    break;
                                case MiscConstants.NUMBER_OF_PRODUCTS_IN_COMBINATION:
                                    Share.NumberOfProductsInCombination = System.Convert.ToInt32(node.Value);
                                    break;

                                default:
                                    Console.WriteLine("LoadXMLConfigurationFile Illegal input: " + child.Name);
                                    break;
                            }

                        }
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadXMLConfigurationFile file: " + fullFilename + " does not exists!!!");
                }
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("NOTICE: LoadXMLConfigurationFile Exception occured: " + ex.ToString());
            }
            return rc;
        }

        // look if the file already set in the Shared object; if not - look for it
        public static string GetFilename(string file2look, string file2lookFef)
        {
            string dir; //  = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            string filename; // = dir + MiscConstants.COMBINATIONS_FILE;

            if (MiscConstants.UNDEFINED_STRING != file2look)
            {
                filename = file2look;
            }
            else
            {
                dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
                filename = dir + file2lookFef;
            }

            // ensure existance
            if (File.Exists(filename))
                return filename;
            else
                return MiscConstants.UNDEFINED_STRING;
        }

        /*
         * Calculate the product' score.
         * 
         */

        // does the product cross the beneficial threshold?
        public static bool CheckBeneficialProducts(uint score)
        {
            bool rc = false;

            if (Share.ProductBeneficialScoreCriteria >= score)
                rc = true;

            return rc;
        }

        public static uint CalculateProductScore(/*Risk*/ double risk, /*Liquidity*/ double liquidity, /*Benefit*/ double benefit)
        {
            //return (uint)(Convert.ToInt32(risk) * Share.RiskFactor + Convert.ToInt32(liquidity) * Share.LiquidityFactor + Convert.ToInt32(benefit) * Share.BenefitFactor);
            return (uint)(risk * Share.RiskFactor + liquidity * Share.LiquidityFactor + benefit * Share.BenefitFactor);
        }

        // analayze the selected combination fittness to the specific user profile
        // loop the Environment.bestDiffCompositionList and other lists
        public static Composition[] CalculateCompositionScore(List<Composition> composition, int number2select)
        {
            Composition[] comp = composition.ToArray();

            for (int i = 0; i < comp.Length; i++)
            {
                comp[i].score = CalculateCompositionScore(comp[i]);
            }

            // order the list and get the first X elements
            Array.Sort(comp, delegate (Composition c1, Composition c2)
            {
                return c1.score.CompareTo(c2.score);
            });

            // print the results
            Composition[] selected = (Composition[])comp.Take(number2select);
            for (int i = 0; i < selected.Length; i++)
                selected[i].ToString();

            return selected;
        }

        // TBD - Omri: what should be the actuall calculation here?
        public static uint CalculateCompositionScore(Composition composition)
        {
            uint result = MiscConstants.UNDEFINED_UINT;
            double[] amounts = new double[Enum.GetNames(typeof(options)).Length];
            /*Risk*/
            double[] risk = new double[Enum.GetNames(typeof(options)).Length];
            /*Liquidity*/
            double[] liquidity = new double[Enum.GetNames(typeof(options)).Length];
            /*Benefit*/
            double[] benefit = new double[Enum.GetNames(typeof(options)).Length];
            FixOrAdjustable[] fixOrAdjustable = new FixOrAdjustable[Enum.GetNames(typeof(options)).Length];
            int i = 0;

            foreach (Option o in composition.opts)
            {
                risk[i] = o.product.risk;
                liquidity[i] = o.product.liquidity;
                benefit[i] = o.product.benefit;
                fixOrAdjustable[i] = o.product.fixOrAdjustable;
                amounts[i] = o.optAmt;
                i++;
            }

            // check the amounts are in the same range
            double amountScore = CalculateAmountScore(amounts);
            uint riskScore = CalculateRiskScore(risk);
            uint liquidityScore = CalculateLiquidityScore(liquidity);
            uint fixOrAdjustableScore = CalculateFixOrAdjustableScore(fixOrAdjustable);

            // TBD - what should be calculate here
            // risk liquidity benefit fix ....
            result = (uint)(1 - amountScore) + riskScore + liquidityScore + fixOrAdjustableScore;

            return result;
        }

        // calculate the difference percantage between the min and the max amounts
        // the less the result is, means it it spread more equily between the plans
        // meaning: less is good
        public static double CalculateAmountScore(double[] amounts)
        {
            double result = 0;
            double totalAmount = 0;
            double min = MiscConstants.UNDEFINED_DOUBLE, max = MiscConstants.UNDEFINED_DOUBLE;

            for (int i = 0; i < amounts.Length; i++)
            {
                totalAmount += amounts[i];
                if (MiscConstants.UNDEFINED_DOUBLE == min || min > amounts[i])
                    min = amounts[i];
                if (MiscConstants.UNDEFINED_DOUBLE == max || max < amounts[i])
                    max = amounts[i];
            }

            result = (max - min) / totalAmount;
            return result;
        }

        static uint CalculateRiskScore(/*Risk*/double[] risk)
        {
            uint riskScore = MiscConstants.UNDEFINED_UINT;

            for (int i = 0; i < risk.Length; i++)
            {
                riskScore += (uint)risk[i]; // or should it calculate differently?
            }

            return riskScore;
        }

        static uint CalculateLiquidityScore(/*Liquidity*/double[] liquidity)
        {
            uint liquidityScore = MiscConstants.UNDEFINED_UINT;

            for (int i = 0; i < liquidity.Length; i++)
            {
                liquidityScore += (uint)liquidity[i]; // or should it calculate differently?
            }

            return liquidityScore;
        }

        static uint CalculateFixOrAdjustableScore(FixOrAdjustable[] fixOrAdjustable)
        {
            uint fixOrAdjustableScore = MiscConstants.UNDEFINED_UINT;

            for (int i = 0; i < fixOrAdjustable.Length; i++)
            {
                fixOrAdjustableScore += (uint)fixOrAdjustable[i]; // or should it calculate differently?
            }

            return fixOrAdjustableScore;
        }


        public static string GetArray<T>(T[] data)
        {
            string s = MiscConstants.UNDEFINED_STRING;
            foreach (T t in data)
            {
                // Console.WriteLine(t.ToString());
                s += t.ToString() + MiscConstants.COMMA;
            }
            return s;
        }

        public static Composition GetBestBorrowerComposition(Composition[] compositions)
        {
            Composition c = null;
            for (int i = 0; i < compositions.Length; i++)
                if (null != compositions[i])
                    if (MiscConstants.BEST_BORROWER_COMPOSITION == compositions[i].name)
                    {
                        c = compositions[i];
                        break;
                    }

            return c;
        }

        public static Composition GetBestBankComposition(Composition[] compositions)
        {
            Composition c = null;
            for (int i = 0; i < compositions.Length; i++)
                if (null != compositions[i])
                    if (MiscConstants.BEST_BANK_COMPOSITION == compositions[i].name)
                    {
                        c = compositions[i];
                        break;
                    }

            return c;
        }

        public static Composition GetBestDiffComposition(Composition[] compositions)
        {
            Composition c = null;
            for (int i = 0; i < compositions.Length; i++)
                if (null != compositions[i])
                    if (MiscConstants.BEST_DIFF_COMPOSITION == compositions[i].name)
                    {
                        c = compositions[i];
                        break;
                    }

            return c;
        }

        public static void CalcaulateProfitOfSpecificProduct(Composition comp, int productIndex, out int BankPay,
            out int BorrowerPay, out int Profit)
        {
            BankPay = BorrowerPay = Profit = MiscConstants.UNDEFINED_INT;
            if (null != comp)
            {
                if (Share.NumberOfProductsInCombination > productIndex)
                {
                    switch (productIndex)
                    {
                        case (int)Options.options.OPTX:
                            BankPay = (int)(comp.optXBankTtlPay);
                            BorrowerPay = (int)Math.Round(comp.opts[(int)Options.options.OPTX].optTtlPay);
                            break;
                        case (int)Options.options.OPTY:
                            BankPay = (int)(comp.optYBankTtlPay);
                            BorrowerPay = (int)Math.Round(comp.opts[(int)Options.options.OPTY].optTtlPay);
                            break;
                        case (int)Options.options.OPTZ:
                            BankPay = (int)(comp.optZBankTtlPay);
                            BorrowerPay = (int)Math.Round(comp.opts[(int)Options.options.OPTZ].optTtlPay);
                            break;

                    }
                    Profit = BorrowerPay - BankPay;

                }
            }
        }

        public static void CalcaulateProfit(Composition comp, out int ttlBankPay,
            out int ttlBorrowerPay, out int ttlProfit)
        {
            if (null != comp)
            {
                if (MiscUtilities.Use3ProductsInComposition())
                {
                    ttlBankPay = (int)(comp.optXBankTtlPay + comp.optYBankTtlPay + comp.optZBankTtlPay);
                    ttlBorrowerPay = (int)Math.Round(comp.opts[(int)Options.options.OPTX].optTtlPay +
                        comp.opts[(int)Options.options.OPTY].optTtlPay + comp.opts[(int)Options.options.OPTZ].optTtlPay);
                }
                else
                {
                    ttlBankPay = (int)(comp.optXBankTtlPay + comp.optYBankTtlPay);
                    ttlBorrowerPay = (int)Math.Round(comp.opts[(int)Options.options.OPTX].optTtlPay +
                        comp.opts[(int)Options.options.OPTY].optTtlPay);
                }

                ttlProfit = /*(int)comp.ttlPay*/ ttlBorrowerPay - ttlBankPay;
            }
            else
            {
                ttlBankPay = ttlBorrowerPay = ttlProfit = 0;
            }
        }

        public static void CalcaulateProfitAll(Composition comp, loanDetails loan,
                        out int borrowerProfit, out int bankProfit, out int totalProfit, out int bankOriginalProfit)
        {
            int ttlBankPay, ttlBorrowerPay, ttlProfit;
            CalcaulateProfit(comp, out ttlBankPay, out ttlBorrowerPay, out ttlProfit);
            // borrower benefit = diff between wisor composition and the original bank
            borrowerProfit = (int)loan.resultReportData.PayFuture - (int)ttlBorrowerPay;
            // original bank profit: diff between orig borrower pay and orig bank pay
            bankOriginalProfit = (int)loan.resultReportData.PayFuture - (int)loan.resultReportData.BankPayFuture;
            // total bank profit: diff between wisor option profit and original bank profit
            bankProfit = ttlProfit - bankOriginalProfit;
            // total benefit: sum of the borrower profit and the bank profit
            totalProfit = borrowerProfit + bankProfit;
        }

        public static void CalcaulateProfitAll(int ttlBankPay, int ttlBorrowerPay, int ttlProfit, loanDetails loan,
                        out int borrowerProfit, out int bankProfit, out int totalProfit)
        {
            // borrower benefit = diff between wisor composition and the original bank
            borrowerProfit = (int)loan.resultReportData.PayFuture - (int)ttlBorrowerPay;
            // original bank profit: diff between orig borrower pay and orig bank pay
            int bankOptionProfit = (int)loan.resultReportData.PayFuture - (int)loan.resultReportData.BankPayFuture;
            // total bank profit: diff between wisor option profit and original bank profit
            bankProfit = ttlProfit - bankOptionProfit;
            // total benefit: sum of the borrower profit and the bank profit
            totalProfit = borrowerProfit + bankProfit;
        }

        public static string GetFileTypeExtension(FileType fileType)
        {
            string ext = MiscConstants.UNDEFINED_STRING;

            switch (fileType)
            {
                case FileType.CSV:
                    ext = MiscConstants.CSV_EXT;
                    break;
                case FileType.PDF:
                    ext = MiscConstants.PDF_EXT;
                    break;
                case FileType.HTML:
                    ext = MiscConstants.HTML_EXT;
                    break;
                case FileType.XML:
                    ext = MiscConstants.XML_EXT;
                    break;
            }
            return ext;
        }

        public static string GetResourceStream(string resourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> resourceNames = new List<string>(assembly.GetManifestResourceNames());

            string outRsourcePath = resourcePath.Replace(@"/", ".");
            outRsourcePath = resourceNames.FirstOrDefault(r => r.Contains(outRsourcePath));

            return outRsourcePath;
        }

        ////////////////////

        // TBD - bad practice
        // start some log files for misc. logging
        public static void OpenMiscLogger()
        {
            if (Share.shouldDebugLoansCalculation && Share.shouldDebugLoans && null == Share.theMiscLogger)
                Share.theMiscLogger = new LoggerFile(Share.tempLogFile /*+ OrderID*/ + MiscConstants.CSV_EXT /*".txt"*/,
                    MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);
        }

        public static void CloseMiscLogger()
        {
            if (null != Share.theMiscLogger)
                Share.theMiscLogger.CloseLog2CSV();
            Share.theMiscLogger = null;
        }

        public static void PrintMiscLogger(string msg)
        {
            if (null != Share.theMiscLogger)
                Share.theMiscLogger.PrintLog(msg);
        }

        ////////////////////

        public static string[] summaryHeader()
        {
            string[] msg = {
                        "Loan ID",
                        "Original Loan Amount",
                        "Monthly payment",
                        "Date Taken",
                        "Remaining Amount",
                        "Borrower Paid So Far",
                        "Bank Payment So Far",
                        "Orig Borrower Future Payment",
                        "Orig Bank Future Payment",
                        "Orig Bank Future Profit",
                        "Refinance Or No",
                        "Can Save Borrower Money",
                        "Can Increase Bank Profit",
                        "Can Increase total Profit",
                        "Wisor Borrower Payment",
                        "Wisor Bank Payment",
                        "Wisor Bank Profit",
                        "Borrower beneficial",
                        "Borrower beneficial%",
                        "Bank beneficial",
                        "Bank beneficial%",
                        "Total beneficial",
                        "Total beneficial%",
                        "Name",
                        "Age",
                        "LTV",
                        "PTI",
                        "Income",
                        "FICO"
                        };
            return msg;
        }


        public static string[] summaryAccululateHeader()
        {
            string[] msg = {
                        "Loan ID",
                        "Original Loan Amount",
                        "Monthly payment",
                        "Date Taken",
                        "Remaining Amount",
                        "Borrower Paid So Far",
                        "Bank Payment So Far",
                        "Orig Borrower Future Payment",
                        "Orig Bank Future Payment",
                        "Orig Bank Future Profit",
                        // add the accululate data
                        "Max Bor pay",
                        "Max Bor pay Lender pay" ,
                        "Max Bor pay Bor save" ,
                        "Max Bor pay Lender profi",
                        "Max Bor pay name",

                        "Min Bor pay",
                        "Min Bor pay Lender pay" ,
                        "Min Bor pay Bor save" ,
                        "Min Bor pay Lender profi",
                        "Min Bor pay name",

                        "Max lender profit",
                        "Max lender profit Bor pay" ,
                        "Max lender profit Bor save" ,
                        "Max lender profit Lender pay",
                        "Max lender profit name",

                        "Min lender profit",
                        "Min lender profit Bor pay" ,
                        "Min lender profit Bor save" ,
                        "Min lender profit Lender pay",
                        "Min lender profit name"
                    };
            return msg;
        }

        public static void OpenSummaryFile()
        {
            if (Share.shouldDebugLoans)
            {
                Share.theSummaryFile = new LoggerFile(Share.summaryLogFile +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT,
               MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);

                // add the header
                string[] msg = summaryHeader();
                PrintSummaryFile(msg);

                // Ugly but short...
                OpenSummaryFileS();
            }
        }

        public static void CloseSummaryFile()
        {
            string[] msg = {
                "\nRun bulk with TotalNumberOfLoans: " + Share.TotalNumberOfLoans +
                    " can re-finince: " + Share.NumberOfCanRefininceLoans + " which means: " +
                    (double) Share.NumberOfCanRefininceLoans / Share.TotalNumberOfLoans * 100 +
                    " and NumberOfPositiveBeneficialLoans: " + Share.NumberOfPositiveBeneficialLoans +
                    " (" + (double) Share.NumberOfPositiveBeneficialLoans / Share.TotalNumberOfLoans * 100 +
                    "%)"
            };
            // print the total numbers
            PrintSummaryFile(msg);

            if (null != Share.theSummaryFile)
                Share.theSummaryFile.CloseLog2CSV();
            Share.theSummaryFile = null;

            // Ugly but short...
            CloseSummaryFileS();
        }

        public static void PrintSummaryFile(string[] msg)
        {
            if (null != Share.theSummaryFile)
                Share.theSummaryFile.PrintLog2CSV(msg);
        }

        ////////////////////////////////////////////////////
        // maintain few log files for win-win, only borrower, only lender and overall profit

        public static void OpenSummaryFileS()
        {
            if (Share.shouldDebugLoans)
            {
                // add the header
                string[] msg = summaryHeader();

                Share.theWinWinSummaryFile = new LoggerFile(Share.summaryLogFile + "WinWin" +
                    DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT,
                    MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);
                PrintSummaryFileS(Share.theWinWinSummaryFile, summaryAccululateHeader());

                if (!Share.shouldDebugLoansOnlyWinWin)
                {
                    Share.theBorrowerWinSummaryFile = new LoggerFile(Share.summaryLogFile + "BorrowerWin" +
                        DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT,
                       MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);
                    Share.theBankWinSummaryFile = new LoggerFile(Share.summaryLogFile + "BankWin" +
                        DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT,
                       MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);
                    Share.theTotalWinSummaryFile = new LoggerFile(Share.summaryLogFile + "TotalWin" +
                        DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT,
                       MiscConstants.UNDEFINED_STRING, true /*mustCreate*/, false /*append*/);

                    PrintSummaryFileS(Share.theBorrowerWinSummaryFile, msg);
                    PrintSummaryFileS(Share.theBankWinSummaryFile, msg);
                    PrintSummaryFileS(Share.theTotalWinSummaryFile, msg);
                }
            }
        }

        public static void PrintSummaryFileS(LoggerFile file, string[] msg)
        {
            if (null != file /*&& null != Share.theSummaryFile*/)
                file.PrintLog2CSV(msg);
        }

        public static void CloseSummaryFileS()
        {
            string[] msg = {
                "\nRun bulk with TotalNumberOfLoans: " + Share.TotalNumberOfLoans +
                    " can re-finince: " + Share.NumberOfCanRefininceLoans + " which means: " +
                    (double) Share.NumberOfCanRefininceLoans / Share.TotalNumberOfLoans * 100 +
                    " and NumberOfPositiveBeneficialLoans: " + Share.NumberOfPositiveBeneficialLoans +
                    " (" + (double) Share.NumberOfPositiveBeneficialLoans / Share.TotalNumberOfLoans * 100 +
                    "%)"
            };
            // print the total numbers
            PrintSummaryFileS(Share.theWinWinSummaryFile, msg);
            PrintSummaryFileS(Share.theBorrowerWinSummaryFile, msg);
            PrintSummaryFileS(Share.theBankWinSummaryFile, msg);
            PrintSummaryFileS(Share.theTotalWinSummaryFile, msg);

            if (null != Share.theWinWinSummaryFile)
                Share.theWinWinSummaryFile.CloseLog2CSV();
            Share.theWinWinSummaryFile = null;
            if (null != Share.theBorrowerWinSummaryFile)
                Share.theBorrowerWinSummaryFile.CloseLog2CSV();
            Share.theBorrowerWinSummaryFile = null;
            if (null != Share.theBankWinSummaryFile)
                Share.theBankWinSummaryFile.CloseLog2CSV();
            Share.theBankWinSummaryFile = null;
            if (null != Share.theTotalWinSummaryFile)
                Share.theTotalWinSummaryFile.CloseLog2CSV();
            Share.theTotalWinSummaryFile = null;
        }

        ////////////////////////////////////////////////////

        public static void CleanUp()
        {
            // close log debug
            CloseMiscLogger();
            CloseSummaryFile();
        }


        public static ProductsList ReadProductsFromFile()
        {
            ProductsList products = null;
            Share.theSelectionType = SelectionType.ReadProducts;

            string filename = MiscUtilities.GetFilenameFromUser();

            if (null != filename)
            {
                products = GenericProduct.LoadXMLProductsFile(filename);
            }
            return products;
        }

        public static FieldList GetCriteriaFromFile()
        {
            FieldList fields = null;
            Share.theSelectionType = SelectionType.ReadCretiria;

            string filename = MiscUtilities.GetFilenameFromUser();
            if (null != filename)
            {
                fields = FileUtils.LoadXMLFileData(filename);
            }

            return fields;
        }

        public static string GetProductHebrewName(string productName)
        {
            return GenericProduct.GetProductHebrewName(productName);
        }

        public static string GetProductHebrewName(GenericProduct product)
        {
            string name = MiscConstants.UNDEFINED_STRING;
            if (Share.cultureInfo.Name.Equals("he-IL"))
                name = product.hebrewName;
            else
                name = product.name;
            if (MiscConstants.UNDEFINED_STRING == name)
                name = product.name;
            return name;
        }

        public static bool IsProductTsamud(indices indexUsedFirstTimePeriod)
        {
            return (indices.MADAD == indexUsedFirstTimePeriod);
        }

        public static bool IsProductFix(FixOrAdjustable fixOrAdjustable)
        {
            return (FixOrAdjustable.FIX == fixOrAdjustable);
        }

        // ריבית משתנה אשר אינם פריים 
        public static bool IsProductAdjustableInterest(indices indices, FixOrAdjustable fixOrAdjustable, indexJumps indexJumpFirstTimePeriod)
        {
            // TBD - Omri what is the: 1 < (int) indexJumpFirstTimePeriod, it is enumarator!!!
            return (indices.PRIME == indices && FixOrAdjustable.FIX == fixOrAdjustable && 1 < (int)indexJumpFirstTimePeriod);
        }

        ////////////////////////////////////

        // redirect the console output to a file
        public static void RedirectOutput2File()
        {
            string shouldRedirectOutput2File = System.Configuration.ConfigurationManager.AppSettings["ShouldRedirectOutput2File"];
            if (!String.IsNullOrEmpty(shouldRedirectOutput2File) && MiscConstants._YES_KEY == shouldRedirectOutput2File.ToLower())
            {
                string dd = MiscUtilities.GetOutputDirectory2();
                dd = dd + Path.DirectorySeparatorChar + "outLog.txt";

                FileStream filestream = new FileStream(dd, FileMode.Create);
                var streamwriter = new StreamWriter(filestream);
                streamwriter.AutoFlush = true;
                Console.SetOut(streamwriter);
                Console.SetError(streamwriter);
                Console.WriteLine("NOTICE: PrepareRunningFull RunSearch set output file: " + dd);
            }
        }

        static bool firstTimeRun = true;
        public static bool PrepareRunningFull()
        {
            bool rc = true;

            if (firstTimeRun)
            {
                // redirect the console output to a file
                RedirectOutput2File();

                // set the data directory
                string dataDirectory = MiscUtilities.GetDataDirectory();
                Console.WriteLine("NOTICE: PrepareRunningFull RunSearch set data directory: " + dataDirectory);

                MiscUtilities.SetupAllEnv(dataDirectory);

                rc = PrepareRuning();
                firstTimeRun = false;
            }

            return rc;
        }


        static bool PrepareRuning()
        {
            bool rc = true;

            // enable log debug
            MiscUtilities.OpenMiscLogger();
            MiscUtilities.OpenSummaryFile();

            ProductsList products = ReadProductsFromFile();
            if (null == Share.theLoadedProducts)
            {
                WindowsUtilities.loggerMethod("NOTICE: PrepareRuning Failed to upload products definitions.");
            }
            else
            {
                rc = Combinations.SetCombinationsFilename();
                if (!rc)
                {
                    WindowsUtilities.loggerMethod("NOTICE: PrepareRuning Failed to load combination file.");
                    return rc;
                }

                // ensure the rates file is located
                rc = MiscUtilities.SetRatesFilename();
                if (!rc)
                {
                    WindowsUtilities.loggerMethod("NOTICE: PrepareRuning Failed to load rates file.");
                    return rc;
                }

                // Build the input controls from the xml file
                // Load the loan' parameters from a file
                FieldList fields = GetCriteriaFromFile();
                if (null == Share.theSelectedCriteriaFields)
                {
                    WindowsUtilities.loggerMethod("NOTICE: PrepareRuning Failed to upload criteria definitions.");
                }
                else
                {
                    rc = MiscUtilities.SetRiskAndLiquidityFilename();
                    if (!rc)
                    {
                        WindowsUtilities.loggerMethod("NOTICE: PrepareRuning Failed in SetRiskAndLiquidityFilename.");
                        return rc;
                    }
                }

            }

            return rc;
        }

        public static void RunTheLogic(LoanList listOfLoans = null, bool shouldCallPrepare = true)
        {
            LoanList loans;
            bool rc = false;
            try
            {
                if (shouldCallPrepare)
                {
                    rc = PrepareRunningFull();
                }

                if (rc)
                {

                    if (null != listOfLoans)
                    {
                        loans = listOfLoans;
                    }
                    else
                    {
                        // Read customers load data and fire the calculation to all
                        loans = MiscUtilities.GetLoansFromFile(Share.theSelectedCriteriaFields);
                        if (null == loans || 0 >= loans.Count)
                        {
                            WindowsUtilities.loggerMethod("NOTICE: RunTheLogic Failed to upload loans definitions.");
                            return;
                        }
                    }

                    if (Share.shouldRunSync)
                        WindowsUtilities.runLoanMethodSync(loans);
                    //Utilities.RunTheLoansWraperSync(loans);
                    else
                        WindowsUtilities.runLoanMethodASync(loans);
                    //Utilities.RunTheLoansWraperASync(loans);
                }
                else
                {
                    WindowsUtilities.loggerMethod("NOTICE: RunTheLogic Failed in PrepareRuning.");
                }
            }
            catch (ArgumentOutOfRangeException aoore)
            {
                WindowsUtilities.loggerMethod("NOTICE: RunTheLogic ArgumentOutOfRangeException occured: " + aoore.ToString());
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("NOTICE: RunTheLogic Exception occured: " + ex.ToString());
            }
            //WindowsUtilities.loggerMethod("Complete calculate the entire " + loans.Count + " loans");
        }


        /////////////////////////////////////////////////

        /////////////////////////////////////////////////

        public static string CleanupRedundantChars(string[] entities, int index, bool allowDot = false, string defaultValue = "0")
        {
            string value = MiscConstants.UNDEFINED_STRING;
            if (0 <= index && index < entities.Length)
            {
                string str = entities[index];
                string trimed;
                int loc;
                if (!allowDot)
                {
                    loc = str.IndexOf(MiscConstants.DOT_STR);
                    //trimed = (0 <= loc) ? entities[index].Remove(loc) : entities[index];
                    // instead of triming, make round
                    if (0 <= loc)
                    {
                        double d = Math.Round(Convert.ToDouble(str));
                        str = d.ToString();
                    }
                }
                trimed = str;

                loc = trimed.IndexOf(MiscConstants.PERCANTAGE_STR);
                trimed = (0 <= loc) ? trimed.Remove(loc) : trimed;

                // remove currency symbole
                string pattern = @"(\p{Sc}\s?)?";
                Regex rgx = new Regex(pattern);
                value = rgx.Replace(trimed, "");
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }


        static public string GetReportFileName(string id, FileType fileType, bool isLender = true)
        {
            string ext = MiscUtilities.GetFileTypeExtension(fileType);
            string lenderOrBorrowerPrefix = (isLender ? MiscConstants.LENDER_REPORT_PREFIX : MiscConstants.BORROWER_REPORT_PREFIX);
            string languagePrefix = Share.cultureInfo.Name;
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.IMAGES_DIR + Path.DirectorySeparatorChar;
            string fn = AppDomain.CurrentDomain.BaseDirectory +
                MiscConstants.REPORTS_DIR + Path.DirectorySeparatorChar +
                lenderOrBorrowerPrefix + MiscConstants.NAME_SEP_CHAR +
                languagePrefix + MiscConstants.NAME_SEP_CHAR + id + MiscConstants.NAME_SEP_CHAR +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + ext;

            return fn;
        }

        static public bool IsTrue(string str)
        {
            if (!string.IsNullOrEmpty(str) && ("true" == str || "yes" == str))
                return true;
            return false;
        }


        static public void SetLogger(MyDelegate func)
        {
            WindowsUtilities.loggerMethod = func;
        }

        static void SetRunSingleLoanFuncASync(MyRunDelegate func)
        {
            WindowsUtilities.runSingleLoanASyncMethod = func;
        }
        static void SetRunSingleLoanFuncSync(MyRunDelegate func)
        {
            WindowsUtilities.runSingleLoanSyncMethod = func;
        }

        static void SetRunLoanFuncSync(MyRunDelegateListOfLoans func)
        {
            WindowsUtilities.runLoanMethodSync = func;
        }
        static void SetRunLoanFuncASync(MyRunDelegateListOfLoans func)
        {
            WindowsUtilities.runLoanMethodASync = func;
        }


        static public bool SetupAllEnv(string dataDirectory = null)
        {
            bool rc = false;

            //SetRunLoanFunc(Utilities.RunTheLoansSync);
            SetRunSingleLoanFuncASync(MultiThreadingManagment.runSingleLoanASyncMethod);
            SetRunSingleLoanFuncSync(MultiThreadingManagment.runSingleLoanSyncMethod);
            SetRunLoanFuncSync(MultiThreadingManagment.RunTheLoansWraperSync);
            SetRunLoanFuncASync(MultiThreadingManagment.RunTheLoansWraperASync);

            Share.shouldShowCriteriaSelectionWindow = false;
            Share.shouldShowCriteriaSelectionContinue = false;
            Share.shouldShowProductSelectionWindow = false;
            Share.shouldShowProductSelectionContinue = false;
            Share.shouldShowRatesSelectionWindow = false;
            Share.shouldShowLoansSelectionWindow = false;
            Share.ShouldCreateReportOnlyWhenWinWin = false;

            Share.shouldRunFake = false;
            Share.numberOfOption = 3;

            Share.shouldPrintCounters = false;
            Share.CalculatePmtCounter = Share.CalculateLuahSilukinCounter = Share.RateCounter =
                Share.counterOfOneDivisionOfAmounts = Share.CalculatePmtFromCalculateLuahSilukinCounter =
                Share.OptionObjectCounter = Share.SavedCompositionsCounter =
                Share.CalculateLuahSilukinCounterNOTInFirstTimePeriod =
                Share.CalculateLuahSilukinCounterInFirstTimePeriod =
                Share.CalculateLuahSilukinCounterIndexUsedFirstTimePeriod = 0;

            //Share.ShouldCalcTheBankProfit = true;
            Share.numberOfPrintResultsInList = 1; //  100;

            Share.ShouldEachCombinationRunSeparetly = false;
            Share.ShouldStoreAllCombinations = false;

            //Share.shouldCreateHTMLReport = false; // true;
            Share.ShouldStoreInDB = true;
            Share.LoansLoadFromLine = MiscConstants.UNDEFINED_UINT;
            Share.LoansLoadIDsFromLine = MiscConstants.UNDEFINED_STRING;
            Share.shouldDebugLoans = true;
            Share.shouldDebugLoansCalculation = true; //  false;
            Share.shouldDebugLoansOnlyWinWin = false;
            Share.shouldDebugLuchSilukin = false;
            Share.ShouldCreateCombinationDynamickly = false;

            // output log level settings
            Share.printMainInConsole = true;
            Share.printToOutputFile = true;

            Share.printFunctionsInConsole = false;
            Share.printSubFunctionsInConsole = false;
            Share.printPercentageDone = false;
            Share.NumberOfCanRefininceLoans = 0;
            Share.NumberOfPositiveBeneficialLoans = 0;

            // important settings
            Share.shouldRunSync = true;
            Share.shouldRunLogicSync = true;
            Share.shouldCreateShortPDFReport = false; //true;
            Share.shouldCreateLongPDFReport = true;

            bool isFullConfigFilename = false;
            string configFilename = MiscConstants.CONFIGURATION_FILE;

            if (null != dataDirectory)
            {
                Share.DataDirectory = dataDirectory;
                isFullConfigFilename = true;
            }
            else
            {
                string dd = MiscUtilities.GetDataDirectory();
                if (!String.IsNullOrEmpty(dd))
                    Share.DataDirectory = dd;
                else
                    Share.DataDirectory = MiscConstants.DATA_DIR;
            }
            configFilename = Share.DataDirectory + Path.DirectorySeparatorChar + MiscConstants.CONFIGURATION_FILE;

            // load the configuration file
            if (null == WindowsUtilities.loggerMethod)
            {
                SetLogger(LogFunction);
            }
            rc = MiscUtilities.LoadXMLConfigurationFile(configFilename, isFullConfigFilename);

            return rc;
        }

        static public void LogFunction(string msg, bool write2console = true, bool shouldColor = false)
        {
            Console.WriteLine(msg);
        }

        // support languages...
        //static public string GetStringByLanguage(CultureInfo culture, string name)
        //{
        //    string tmp = Properties.Resources.name;
        //    if (!string.IsNullOrEmpty(tmp))
        //    {
        //        if (culture.Name.Equals("he-IL"))
        //        {
        //            tmp = new string(tmp.Reverse().ToArray());
        //        }
        //        else
        //        {

        //        }
        //    }
        //    else
        //    {
        //        tmp = name;
        //    }

        //    return tmp;
        //}

        public static string GetDataDirectory()
        {
            string dataDir = System.Configuration.ConfigurationManager.AppSettings["DataDirectory"];
            if (String.IsNullOrEmpty(dataDir))
            {
                dataDir = GetCurrentDirectory() + MiscConstants.DATA_DIR;
            }
            return dataDir;
        }

        public static string GetOutputDirectory2()
        {
            string outputDir = System.Configuration.ConfigurationManager.AppSettings["OutputDirectory"];
            if (String.IsNullOrEmpty(outputDir))
                outputDir = GetDataDirectory() + Path.DirectorySeparatorChar + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar;
            return outputDir;
        }

        public static string GetBinDirectory()
        {
            string outputDir = System.Configuration.ConfigurationManager.AppSettings["BinDirectory"];
            if (String.IsNullOrEmpty(outputDir))
                outputDir = GetDataDirectory() + Path.DirectorySeparatorChar + MiscConstants.BIN_DIR + Path.DirectorySeparatorChar;
            return outputDir;
        }

        public static void CalculateTypeOfProducts3(GenericProduct[] Products, int[] Amount,
            out string structureTypeString)
        {
            string FixHeader, IndexationHeader1, IndexationHeader2, IndexationHeader3;
            structureTypeString = MiscConstants.UNDEFINED_STRING;

            if (null != Products && 0 < Products.Length && null != Amount && 0 < Amount.Length)
            {
                int len = Math.Min(Products.Length, Amount.Length);
                // calculate the relations between tzamud and not in all the products which consist the composition
                uint fix = MiscConstants.UNDEFINED_UINT, adjustable = MiscConstants.UNDEFINED_UINT;
                uint tsamud = MiscConstants.UNDEFINED_UINT, notTsamud = MiscConstants.UNDEFINED_UINT;

                bool isProductFix = false, isProductTsamud = false;
                for (int j = 0; j < len; j++)
                {
                    isProductFix = MiscUtilities.IsProductFix(Products[j].fixOrAdjustable);
                    isProductTsamud = MiscUtilities.IsProductTsamud(Products[j].originalIndexUsedFirstTimePeriod);

                    // accumulate the fix or adjustable
                    if (isProductFix)
                        fix += (uint)Amount[j];
                    else
                        adjustable += (uint)Amount[j];

                    // accululate Tsamud or not
                    if (isProductTsamud)
                        tsamud += (uint)Amount[j];
                    else
                        notTsamud += (uint)Amount[j];

                    // calculate the fix and adjustable numbers
                    uint entireFixSum = fix + adjustable;
                    uint fixNum = MiscConstants.UNDEFINED_UINT, adjustableNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireFixSum)
                    {
                        fixNum = Convert.ToUInt32((double)fix / entireFixSum * 100);
                        adjustableNum = 100 - fixNum;
                    }

                    // calculate the tsamud vs. not numbers
                    uint entireTsamudSum = tsamud + notTsamud;
                    uint tsamudNum = MiscConstants.UNDEFINED_UINT, notTsamudNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireTsamudSum)
                    {
                        tsamudNum = Convert.ToUInt32((double)tsamud / entireTsamudSum * 100);
                        notTsamudNum = 100 - tsamudNum;
                    }

                    CalculateHeaders(adjustable, tsamudNum, out FixHeader, out IndexationHeader1,
                        out IndexationHeader2, out IndexationHeader3);
                    // set the right numbers to the header
                    string brackets1 = ENG_BRACKETS1;
                    string brackets2 = ENG_BRACKETS2;
                    if (Share.cultureInfo.Name.Equals("he-IL"))
                    {
                        brackets1 = HEB_BRACKETS1;
                        brackets2 = HEB_BRACKETS2;
                    }
                    structureTypeString = FixHeader + ", " + IndexationHeader1 + brackets1 + fixNum + "% "
                        + IndexationHeader2 + " , " + adjustableNum + "% " + IndexationHeader3 + brackets2;

                }
            }
        }


        public static void CalculateTypeOfProducts2(RecommendedStructure[] compData, out string structureTypeString)
        {
            structureTypeString = MiscConstants.UNDEFINED_STRING;
            List<GenericProduct> lgp = new List<GenericProduct>();
            List<int> lamount = new List<int>();

            for (int j = 0; j < compData.Length; j++)
            {
                lgp.Add(compData[j].Product);
                lamount.Add(compData[j].Amount);

                CalculateTypeOfProducts3(lgp.ToArray(), lamount.ToArray(), out structureTypeString);
            }

#if NO_NEED_ANY_MORE
            string FixHeader, IndexationHeader1, IndexationHeader2, IndexationHeader3;
            
            //int ttlBankPay, ttlBorrowerPay, ttlProfit;

            // RecommendedStructure(int amount, string productType, double rate, int time, int pmt)
            if (null != compData && 0 < compData.Length)
            {
                //// ensure its a win-win composition
                //if (!compData[i].IsWinWin)
                //    continue;

                // calculate the relations between tzamud and not in all the products which consist the composition
                uint fix = MiscConstants.UNDEFINED_UINT, adjustable = MiscConstants.UNDEFINED_UINT;
                uint tsamud = MiscConstants.UNDEFINED_UINT, notTsamud = MiscConstants.UNDEFINED_UINT;

                //MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                //uint sumAmount = 0, sumMonthly = 0, sumTTLPay = 0, sumTTllProfit = 0;
                //double sumLenderProfit = 0;

                //// get the bank profit from the composition fields
                //uint[] bankPay = new uint[] {
                //    compData[i].optXBankTtlPay, compData[i].optYBankTtlPay, compData[i].optZBankTtlPay };
                bool isProductFix = false, isProductTsamud = false;
                for (int j = 0; j < compData.Length; j++)
                {
                    isProductFix = MiscUtilities.IsProductFix(compData[j].Product.fixOrAdjustable);
                    isProductTsamud = MiscUtilities.IsProductTsamud(compData[j].Product.originalIndexUsedFirstTimePeriod);

                    // accumulate the fix or adjustable
                    if (isProductFix)
                        fix += (uint)compData[j].Amount;
                    else
                        adjustable += (uint)compData[j].Amount;

                    // accululate Tsamud or not
                    if (isProductTsamud)
                        tsamud += (uint)compData[j].Amount;
                    else
                        notTsamud += (uint)compData[j].Amount;

                    //bool Indexation = MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod);

                    // calculate the fix and adjustable numbers
                    uint entireFixSum = fix + adjustable;
                    uint fixNum = MiscConstants.UNDEFINED_UINT, adjustableNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireFixSum)
                    {
                        fixNum = Convert.ToUInt32((double)fix / entireFixSum * 100);
                        adjustableNum = 100 - fixNum;
                    }

                    // calculate the tsamud vs. not numbers
                    uint entireTsamudSum = tsamud + notTsamud;
                    uint tsamudNum = MiscConstants.UNDEFINED_UINT, notTsamudNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireTsamudSum)
                    {
                        tsamudNum = Convert.ToUInt32((double)tsamud / entireTsamudSum * 100);
                        notTsamudNum = 100 - tsamudNum;
                    }

                    CalculateHeaders(adjustable, tsamudNum, out FixHeader, out IndexationHeader1,
                        out IndexationHeader2, out IndexationHeader3);
                    // set the right numbers to the header
                    structureTypeString = FixHeader + ", " + IndexationHeader1 + " ) " + fixNum + "% "
                        + IndexationHeader2 + " , " + adjustableNum + "% " + IndexationHeader3 + " (";
                   
                }
            }
#endif
        }

        public static void CalculateTypeOfProducts(ResultReportData reportData, int indexOfComposition,
            out string structureTypeString)
        {
            structureTypeString = MiscConstants.UNDEFINED_STRING;
            string FixHeader, IndexationHeader1, IndexationHeader2, IndexationHeader3;
            int ttlBankPay, ttlBorrowerPay, ttlProfit;
            int i = indexOfComposition;
            Composition[] compData = reportData.compositions;
            if (null != compData && null != compData[i])
            {
                //// ensure its a win-win composition
                //if (!compData[i].IsWinWin)
                //    continue;

                // calculate the relations between tzamud and not in all the products which consist the composition
                uint fix = MiscConstants.UNDEFINED_UINT, adjustable = MiscConstants.UNDEFINED_UINT;
                uint tsamud = MiscConstants.UNDEFINED_UINT, notTsamud = MiscConstants.UNDEFINED_UINT;

                MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                uint sumAmount = 0, sumMonthly = 0, sumTTLPay = 0, sumTTllProfit = 0;
                double sumLenderProfit = 0;

                // get the bank profit from the composition fields
                uint[] bankPay = new uint[] {
                    compData[i].optXBankTtlPay, compData[i].optYBankTtlPay, compData[i].optZBankTtlPay };

                for (int j = 0; j < compData[i].opts.Length; j++)
                {
                    bool isProductFix = MiscUtilities.IsProductFix(compData[i].opts[j].product.fixOrAdjustable);
                    bool isProductTsamud = MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod);

                    // accumulate the fix or adjustable
                    if (isProductFix)
                        fix += (uint)compData[i].opts[j].optAmt;
                    else
                        adjustable += (uint)compData[i].opts[j].optAmt;

                    // accululate Tsamud or not
                    if (isProductTsamud)
                        tsamud += (uint)compData[i].opts[j].optAmt;
                    else
                        notTsamud += (uint)compData[i].opts[j].optAmt;

                    string productName = compData[i].opts[j].product.productID.stringTypeId;
                    string Option = productName;
                    uint Amt = (uint)compData[i].opts[j].optAmt;
                    double Rate = compData[i].opts[j].optRateFirstPeriod;
                    uint Time = compData[i].opts[j].optTime;
                    uint PMT = (uint)compData[i].opts[j].optPmt;
                    uint TTLPay = (uint)compData[i].opts[j].optTtlPay;
                    uint TTllProfit = TTLPay - bankPay[j];
                    double LenderProfit = (double)TTllProfit / reportData.RemaingLoanAmount * 100;

                    sumAmount += Amt;
                    sumMonthly += PMT;
                    sumTTLPay += TTLPay;
                    sumTTllProfit += TTllProfit;
                    sumLenderProfit += LenderProfit;

                    bool Indexation = MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod);

                    // calculate the fix and adjustable numbers
                    uint entireFixSum = fix + adjustable;
                    uint fixNum = MiscConstants.UNDEFINED_UINT, adjustableNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireFixSum)
                    {
                        fixNum = Convert.ToUInt32((double)fix / entireFixSum * 100);
                        adjustableNum = 100 - fixNum;
                    }

                    // calculate the tsamud vs. not numbers
                    uint entireTsamudSum = tsamud + notTsamud;
                    uint tsamudNum = MiscConstants.UNDEFINED_UINT, notTsamudNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireTsamudSum)
                    {
                        tsamudNum = Convert.ToUInt32((double)tsamud / entireTsamudSum * 100);
                        notTsamudNum = 100 - tsamudNum;
                    }

                    CalculateHeaders(adjustable, tsamudNum, out FixHeader, out IndexationHeader1,
                        out IndexationHeader2, out IndexationHeader3);
                    // set the right numbers to the header
                    IndexationHeader2 = String.Format(IndexationHeader2, fixNum, adjustableNum);

                }

            }
        }




        public static void CalculateHeaders(uint adjustable, uint tsamudNum, out string FixHeader,
             out string IndexationHeader1, out string IndexationHeader2, out string IndexationHeader3)
        {
            FixHeader = IndexationHeader1 = IndexationHeader2 = IndexationHeader3 = MiscConstants.UNDEFINED_STRING;

            /*
            אם תמהיל משלב ריבית משתנה – לא פריים – אז חלק ראשון של הכותרת תמיד יהיה: "תמהיל המאפשר יותר שינויים"
            אם תמהיל משלב רק פריים וריבית קבועה – אז חלק ראשון של הכותרת תמיד יהיה: "תמהיל המאפשר יותר יציבות" 
            */
            if (0 < adjustable)
            {
                FixHeader = Properties.Resources.stressTestFlexible;
            }
            else
            {
                FixHeader = Properties.Resources.stressTestStability;
            }

            /*
            אם 0% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: "כאשר כל הכסף לא צמוד למדד"
            אם עד 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר חלק מהכסף צמוד למדד"
            אם בדיוק 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר חצי מהכסף צמוד למדד"
            אם מעל 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר רוב הכסף צמוד למדד"
            אם 100% מהכסף במוצרים צמודים למדד (כנראה לא יקרה כי בינתיים יש תמיד מוצר פריים בתמהיל) – אז החלק השני של הכותרת תמיד יהיה: כאשר כל הכסף צמוד למדד"
            */
            IndexationHeader2 = Properties.Resources.stressTestFixRate;
            IndexationHeader3 = Properties.Resources.stressTestDynamicRate;

            if (MiscConstants.UNDEFINED_UINT == tsamudNum)
            {
                IndexationHeader1 = Properties.Resources.stressTestAllMoneyNotTsamud;
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 > tsamudNum)
            {
                IndexationHeader1 = Properties.Resources.stressTestPartMoneyTsamud;
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 == tsamudNum)
            {
                IndexationHeader1 = Properties.Resources.stressTestHalfMoneyTsamud;
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 100 == tsamudNum)
            {
                IndexationHeader1 = Properties.Resources.stressTestAllMoneyTsamud;
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 < tsamudNum)
            {
                IndexationHeader1 = Properties.Resources.stressTestMostMoneyTsamud;
            }
        }

        public static RecommendedStructure[] CalculateRecommendedStructure(Composition c, ResultReportData reportData)
        {
            bool isHebrew = Share.cultureInfo.Name.Equals(MiscConstants.HEBREW_STR);
            List<RecommendedStructure> recommendedStructureList = new List<RecommendedStructure>();

            if (null != c && null != c.opts)
            {
                for (int i = 0; i < c.opts.Length; i++)
                {
                    if (null == c.opts[i])
                        continue;

                    RecommendedStructure currRecommend = new RecommendedStructure(
                        Convert.ToInt32(c.opts[i].optAmt),
                        (isHebrew) ? c.opts[i].product.hebrewName : c.opts[i].product.name,
                        c.opts[i].optRateFirstPeriod, (int)c.opts[i].optTime, Convert.ToInt32(c.opts[i].optPmt), c.opts[i].product);
                    double TotalProfitPercantage, TotalPay, TotalProfit, TotalBankPay;
                    TotalPay = c.opts[i].optTtlPay;
                    TotalBankPay = c.optXBankTtlPay + c.optYBankTtlPay + c.optZBankTtlPay;
                    TotalProfit = TotalPay - TotalBankPay;
                    TotalProfitPercantage = (double)TotalProfit / reportData.RemaingLoanAmount * 100;
                    LenderCalculationData lenderCalculationData = new LenderCalculationData(
                        (int)TotalPay, (int)TotalProfit, TotalProfitPercantage);
                    currRecommend.lenderCalculationData = lenderCalculationData;
                    recommendedStructureList.Add(currRecommend);
                }
            }
            return recommendedStructureList.ToArray();
        }

        public static bool SendEmailMessageByMailgun(MailMessage msg)
        {
            string MailServer = ConfigurationManager.AppSettings["MailServer"];
            string MailApiKey = ConfigurationManager.AppSettings["MailApiKey"];
            string MailDomain = ConfigurationManager.AppSettings["MailDomain"];
            HttpStatusCode res = HttpStatusCode.NotImplemented;
            if (null != MailServer && null != MailApiKey && null != MailDomain)
            {
                RestSharp.RestClient client = new RestClient(MailServer);
                client.Authenticator = new HttpBasicAuthenticator("api", MailApiKey);
                RestRequest request = new RestRequest();
                request.AddParameter("domain", MailDomain, ParameterType.UrlSegment);
                request.Resource = "{domain}/messages";
                request.AddParameter("from", msg.From);
                request.AddParameter("to", msg.To);
                request.AddParameter("subject", msg.Subject);
                request.AddParameter("text", msg.Body);
                request.AddParameter("html", msg.Body);
                request.Method = Method.POST;

                IRestResponse rr = client.Execute(request);
                res = rr.StatusCode;
            }

            return HttpStatusCode.OK == res ? true : false;
        }

        public static bool RunLongPDFreport(string reportFilename, OrderDataContainer2 orderDataContainer2,
             ResultReportData reportData, CultureInfo cultureInfo)
        {
            LongReportDataObject lrdo = new LongReportDataObject(reportData, orderDataContainer2, cultureInfo);
            lrdo.OrderNumberValue = reportData.ID;
            lrdo.EmailValue = orderDataContainer2.Userid;
            bool rc = true;

            // Create a new instance of WisorReportManager class, set filename, culture and data
            WisorReportManager reportManager = new WisorReportManager(reportFilename, cultureInfo, lrdo);
            // it's about time to use the direct PDF lib now...
            bool createFullReport = true;
            if (createFullReport)
            {
                reportManager.CreateFullReport();
            }
            // create the composition summery
            else
            {
                int structuresQuantity = reportManager.CreateRecommendedStructuresPage(1);
                // Save and start View 
                rc = reportManager.SavePDFDocument();
            }

            return rc;
        }

        public static bool RunShortPDFreport(string reportFilename, OrderDataContainer2 orderDataContainer2,
            ResultReportData reportData, CultureInfo cultureInfo, RunEnvironment env)
        {
            ShortReportDataObject lrdo = new ShortReportDataObject(env, reportData);
            //lrdo.OrderNumberValue = reportData.ID;
            //lrdo.EmailValue = orderDataContainer2.Userid;
            bool rc = true;

            // Create a new instance of WisorReportManager class, set filename, culture and data
            WisorReportManagerShort reportManager = new WisorReportManagerShort(reportFilename, cultureInfo, lrdo);
            rc = reportManager.CreateFullReport();

            return rc;
        }


        public static bool Use3ProductsInComposition()
        {
            return (3 == Share.NumberOfProductsInCombination);
        }

        public static int GetNumberOfProductsInCombination()
        {
            return Share.NumberOfProductsInCombination;
        }

        public static DateValueCollection LoadHistoricRateFile(string fn)
        {
            DateValueCollection data = null;
            string filename;

            if (String.IsNullOrEmpty(fn))
                filename = MiscUtilities.GetHistoricRatesFilename();
            else
                filename = fn;

            bool Status = false;

            if (!String.IsNullOrEmpty(filename))
            {
                //setFilename(filename);
                data = LoadHistoricRatesCSVFile(filename);
                if (null != data && 0 < data.Capacity)
                {
                    Status = true;
                }
                else
                {
                    Status = false;
                    WindowsUtilities.loggerMethod("ERROR HistoricRate failed to load borrower rates from file: " + filename);
                }
            }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE HistoricRate without setting the rates file name");
            }
            return data;
        }

        private static DateValueCollection LoadHistoricRatesCSVFile(string filename)
        {
            DateValueCollection historicDate = null;
            string line = MiscConstants.UNDEFINED_STRING;

            try
            {
                string ext = Path.GetExtension(filename);
                string[] lines = null;

                if (".xls" == ext || ".xlsx" == ext)
                {
                    lines = ExcelUtilities.GetLinesFromFile(filename, false /*shouldRemoveFractions*/);
                }
                else if (".csv" == ext)
                {
                    lines = CSVUtilities.GetLinesFromFile(filename);
                }
                if (null == lines || 0 >= lines.Length)
                {
                    Console.WriteLine("ERROR: LoadHistoricRatesCSVFile failed to load from file: " + filename);
                    return null;
                }

                historicDate = new DateValueCollection();
                string[] entities;

                int lineNumber = 1;
                double theRate = MiscConstants.UNDEFINED_DOUBLE;
                DateTime? theDate = null;

                for (int li = 0; li < lines.Length; li++)
                {
                    line = lines[li];
                    if (String.IsNullOrEmpty(line))
                        continue;

                    // skip the first line
                    // TBD: should read the headers and relate to it
                    if (1 == lineNumber++)
                        continue;

                    entities = line.Split(MiscConstants.COMMA);
                    if (String.IsNullOrEmpty(entities[0]))
                    {
                        continue;
                    }
                    //theDate = DateTime.Parse(entities[0]);
                    theDate = MiscUtilities.ConvertDate(entities[0]);

                    //// ensure the line correctness. 2 are the product name and the profile
                    //if (MiscConstants.NumberOfYearsFrProduct + 2 != entities.Length)
                    //{
                    //    continue;
                    //}

                    // clean all redundant chars e.g. %
                    for (int i = 1; i < entities.Length; i++)
                    {
                        int index = entities[i].IndexOf(MiscConstants.PERCANTAGE_STR);
                        string trimed = (0 < index) ? entities[i].Remove(index).Trim() : entities[i].Trim();
                        theRate = Double.Parse(trimed);
                    }

                    if (null != theDate)
                        historicDate.Add(new DateValue((DateTime)theDate, theRate));
                }
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadHistoricRatesCSVFile got Exception: " + ex.ToString() + ". line: " + line);
            }

            return historicDate;
        }

        public static void PrintResultReportData(string msg, ResultReportData data)
        {
            string ToPrint = msg + " :  " +
                "   PayUntilToday: " + data.PayUntilToday +
                "   PayFuture: " + data.PayFuture +
                "   RemaingLoanAmount: " + data.RemaingLoanAmount +
                "   RemaingLoanTime: " + data.RemaingLoanTime +
                "   FirstMonthlyPMT: " + data.FirstMonthlyPMT +
                "   MonthlyPaymentCalc: " + data.MonthlyPaymentCalc +
                "   BankPayUntilToday: " + data.BankPayUntilToday +
                "   BankPayFuture: " + data.BankPayFuture;
            Log2File(ToPrint);
        }

        // log to file
        public static void Log2File(string msg)
        {
            if (Share.shouldDebugLoansCalculation)
            {
                MiscUtilities.PrintMiscLogger(msg);
            }
        }

        public static string TranslateBoolToYesOrNo(bool value)
        {
            string translated = MiscConstants.UNDEFINED_STRING;
            
            switch (value)
            {
                case true:
                    translated = Properties.Resources.YesString;
                    break;
                case false:
                    translated = Properties.Resources.NoString;
                    break;
                default:
                    break;
            }

            return translated;
        }

        public static string TranslateBoolToYesOrNo2(bool value)
        {
            string translated = MiscConstants.UNDEFINED_STRING;

            switch (value)
            {
                case true:
                    translated = Properties.Resources.HaveString;
                    break;
                case false:
                    translated = Properties.Resources.DontHaveString;
                    break;
                default:
                    break;
            }

            return translated;
        }

    }

}
