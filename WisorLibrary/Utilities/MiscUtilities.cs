using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Logic;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;
using static WisorLib.Options;

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
            string fn = AppDomain.CurrentDomain.BaseDirectory // + Path.DirectorySeparatorChar
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar +
                Share.CustomerName + seq + /* orderid + MiscConstants.NAME_SEP_CHAR + */
                 loanAmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR +
                monthlyPmtWanted.ToString() + MiscConstants.NAME_SEP_CHAR + add +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.CSV_EXT;

            return fn;
        }

        public static bool SetRatesFilename()
        {
            Share.theSelectionType = SelectionType.ReadRates;
            //string ratesFilename = @"..\..\..\Data\RateFileGeneric.csv";
            //string ratesBankFilename = @"..\..\..\Data\CitiRateMarginGeneric.csv";
            bool rc;
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            //string borrowerFullfilename = MiscUtilities.GetFilenameFromUser(); // dir + MiscConstants.RATES_FILE;
            //string bankFullfilename = dir + MiscConstants.BANK_RATES_FILE;
            //string fn2 = MiscUtilities.GetSpecificFilename(bankFullfilename, Share.CustomerName);

            //rc = Rates.SetRatesFile(borrowerFullfilename, fn2);
            rc = Rates.SetRatesFile(Share.RatesFileName, Share.BankRatesFileName);

            //if (!File.Exists(fullfilename))
            //{
            //    fullfilename = Utilities.GetFilenameFromUser();
            //    rc = Rates.SetRatesFile(fullfilename, bankfullfilename);
            //}
            //else
            //{
            //    rc = Rates.SetRatesFile(fullfilename, bankfullfilename);
            //}

            return rc;
        }

        public static bool SetHistoricRatesFilename()
        {
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            string filename = dir + MiscConstants.HISTORIC_FILE;

            bool rc = HistoricRate.SetFilename(filename);

            return rc;
        }

        public static bool SetRiskAndLiquidityFilename()
        {
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            string filename = dir + MiscConstants.RISK_LIQUIDITY_FILE;

            bool rc = RiskLiquidityObject.SetFilename(filename);

            return rc;
        }

        public static bool FindRiskLiquidity(RiskLiquidityValue riskLiquidityValue)
        {
            bool rc = RiskLiquidityObject.Instance.FindRiskLiquidity(riskLiquidityValue);
            return rc;
        }

        public static void TestRiskLiquidity()
        {
            RiskLiquidityValue riskLiquidityValue = new RiskLiquidityValue();
            string[] products = { "3.1ARMUSA", "10.1ARMUSA", "Fixed15yrsUSA", "7.1ARMUSA",
                "5.1ARMUSA" };
            riskLiquidityValue.liquidity = Liquidity.MoreLiquidity4; //  MinimumLiquidity1; // LessLiquidity2; 
            riskLiquidityValue.risk = Risk.MaximumRisk5; //  MinimumRisk1;  // LessRisk2; // MaximumRisk5
            bool rc = false;

            for (int i = 0; i < products.Length; i++)
            {
                riskLiquidityValue.productID = new ProductID(i, products[i]);
                rc = MiscUtilities.FindRiskLiquidity(riskLiquidityValue);
                if (rc)
                    Console.WriteLine("TestRiskLiquidity succedded for: " + riskLiquidityValue.ToString());
                else
                    Console.WriteLine("TestRiskLiquidity failed for: " + riskLiquidityValue.ToString());
            }
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
            string filename = dir + Share.LoansFileName; // MiscConstants.LOAN_FILE; 
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
        public static List<ChosenComposition> OrderCompositionListByProfit(List<ChosenComposition> compositions,
            ListSortDirection order = ListSortDirection.Descending)
        {
            List<ChosenComposition> SortedSelectedCompositions =
                (ListSortDirection.Descending == order) ?
                    compositions.OrderByDescending(o => o.profit).ToList() :
                    compositions.OrderBy(o => o.profit).ToList();
            return SortedSelectedCompositions;
        }




        public static uint CalculateMonthlyPayment(uint loanAmount, uint propertyValue, uint yearlyIncome, uint borrowerAge)
        {
            uint desiredMonthlyPayment = 0;

            desiredMonthlyPayment = (uint)yearlyIncome / 3;
            return desiredMonthlyPayment;
        }

        public static double GetIndexRateForOption(indices indic)
        {
            double index = 0;

            switch (indic)
            {
                case indices.MADAD:
                    index = 0.018;
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
        public static double GetHistoricIndexRateForPeriod(indices indic, DateTime dateLoanTaken)
        {
            double index = 0;

            switch (indic)
            {
                case indices.MADAD:
                    index = 0.018;
                    break;
                case indices.PRIME:
                    // ensure the file was loaded
                    if (null == HistoricRate.Instance || !HistoricRate.Instance.Status)
                        MiscUtilities.SetHistoricRatesFilename();
                    //index = HistoricRate.GetHistoricIndex(indic, dateLoanTaken);
                    // get the entire month values
                    index = HistoricRate.GetHistoricValues(indic, dateLoanTaken, dateLoanTaken.AddMonths(1));
                    break;
                case indices.CPI:
                    index = 0;
                    break;
                case indices.FED:
                    index = 0;
                    break;
                case indices.LIBOR:
                    index = 0;
                    break;
                case indices.EUROBOR:
                    index = 0;
                    break;
                case indices.BBBR:
                    index = 0;
                    break;
                case indices.MAKAM:
                    index = 0;
                    break;
                default:
                    index = 0;
                    //WindowsUtilities.loggerMethod("NOTICE: GetIndexRateForOption undefined for indic: " + indic);
                    break;
            }

            return index;
        }

        public static double GetHistoricIndexRateForDate(indices indic, DateTime dateLoanTaken)
        {
            double index = 0;

            switch (indic)
            {
                case indices.MADAD:
                    index = 0.018;
                    break;
                case indices.PRIME:
                    // ensure the file was loaded
                    if (null == HistoricRate.Instance || !HistoricRate.Instance.Status)
                        MiscUtilities.SetHistoricRatesFilename();
                    index = HistoricRate.GetHistoricIndex(indic, dateLoanTaken);
                    break;
                case indices.CPI:
                    index = 0;
                    break;
                case indices.FED:
                    index = 0;
                    break;
                case indices.LIBOR:
                    index = 0;
                    break;
                case indices.EUROBOR:
                    index = 0;
                    break;
                case indices.BBBR:
                    index = 0;
                    break;
                case indices.MAKAM:
                    index = 0;
                    break;
                default:
                    index = 0;
                    //WindowsUtilities.loggerMethod("NOTICE: GetIndexRateForOption undefined for indic: " + indic);
                    break;
            }

            return index;
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
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime dt = default(DateTime);

            // cleanup the minutes
            int ind = str.IndexOf(MiscConstants.SPACE_STR);
            if (0 < ind)
                str = str.Remove(str.IndexOf(MiscConstants.SPACE_STR)).Trim();

            try
            {
                //CultureInfo culture = new CultureInfo("he-IL"); // ("en -US");
                //dt = DateTime.Parse(str, culture);
                dt = DateTime.ParseExact(str, MiscConstants.DATE_FORMAT, provider);
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

        public static void LoadXMLConfigurationFile(string filename)
        {
            string dir = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MiscConstants.DATA_DIR + Path.DirectorySeparatorChar;
            string fullFilename = dir + filename;

            try
            {
                if (File.Exists(fullFilename))
                {
                    XmlDocument doc = new XmlDocument();

                    //load up the xml from the location 
                    doc.Load(fullFilename);

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
                                case MiscConstants.HISTORIC_FILENAME:
                                    Share.HistoricFileName = dir + node.Value;
                                    break;
                                case MiscConstants.COMBINATIONS_FILENAME:
                                    Share.CombinationFileName = dir + node.Value;
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
                                case SHOULD_CREATE_REPORT:
                                    Share.ShouldCreateReport = "yes" == node.Value ? true : false;
                                    break;
                                case SHOULD_STORE_REPORT_IN_DB:
                                    Share.ShouldStoreInDB = "yes" == node.Value ? true : false;
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
            Array.Sort(comp, delegate (Composition c1, Composition c2) {
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
            /*Risk*/ double [] risk = new double[Enum.GetNames(typeof(options)).Length];
            /*Liquidity*/ double [] liquidity = new double[Enum.GetNames(typeof(options)).Length];
            /*Benefit*/ double[] benefit = new double[Enum.GetNames(typeof(options)).Length];
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
            result = (uint) (1 - amountScore) + riskScore + liquidityScore + fixOrAdjustableScore;

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
                riskScore += (uint) risk[i]; // or should it calculate differently?
            }

            return riskScore;
        }

        static uint  CalculateLiquidityScore(/*Liquidity*/double[] liquidity)
        {
            uint liquidityScore = MiscConstants.UNDEFINED_UINT;

            for (int i = 0; i < liquidity.Length; i++)
            {
                liquidityScore += (uint)liquidity[i]; // or should it calculate differently?
            }

            return liquidityScore;
        }

        static uint  CalculateFixOrAdjustableScore(FixOrAdjustable[] fixOrAdjustable)
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
                s += t.ToString() + MiscConstants.COMMA_SEERATOR_STR;
            }
            return s;
        }


    }

}
