using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class FileUtils
    {
        
        public static FieldList LoadXMLFileData(string filename)
        {
            XDocument doc = null;
            FieldList fields = new FieldList();

            if (File.Exists(filename))
            {
                doc = XDocument.Load(filename);
                int index = 0;

                // loop all the items in the document
                foreach (XElement item in from c in doc.Descendants("item") select c)
                {
                    string type = item.Element("type").Value;
                    string id = item.Element("name").Value;
                    string defaultValue = null != item.Element("default") ? item.Element("default").Value : id;
                    // <mandatory>yes</mandatory>
                    string mandatoryValue = null != item.Element("mandatory") ? item.Element("mandatory").Value : "no";
                    bool isMandatory = "yes" == mandatoryValue; 
                    string value = null != defaultValue ? defaultValue : item.Element("name").Value;

                    // loop the options
                    List<string> options = new List<string>();
                    foreach (string st in from s in item.Descendants("options") select s)
                    {
                        options.Add(st);
                    }

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
                    fields.Add(new CriteriaField(id, index++, value, min, max, isMandatory, type, options));
                }

            }
            else
            {
                WindowsUtilities.loggerMethod("LoadFileData file: " + filename + " does not exists!!!");
            }

            // if no user involve, make sure to set all the mandatory criteria
            if (! Share.shouldShowCriteriaSelectionWindow)
            {
                FieldList fl = new FieldList();
                foreach (CriteriaField c in fields)
                {
                    if (c.isMandatory)
                        fl.Add(c);
                    //criteriaList.Add(dg.ID);
                }
                Share.theSelectedCriteriaFields = fl;
            }

            Share.theLoadedCriteriaFields = fields;
            // Reorder the cretiria data
            
            return fields;
        }

        public enum MortgageType { None = 0, FirstTimeBuyer = 1, Reordering = 2, Other = 3 };
        public enum PaymentType { None = 0, FullAmortizatio = 1, InterestOnly = 2, Ballon = 3 };
        public enum MortgagProduct { None = 0 };
        public enum CreditClass { None = 0, Exelent = 1, Good = 2, medium = 3, low = 4 };


  
   
        public static LoanList LoadLoansFileData(string filename, FieldList fieldsDef, bool shouldRemoveFractions = false)
        {
            string[] lines = null;
            LoanList loans = new LoanList();

            //try
            //{
                if (File.Exists(filename))
                {
                    string ext = Path.GetExtension(filename);

                    if (".xls" == ext || ".xlsx" == ext)
                    {
                        lines = ExcelUtilities.GetLinesFromFile(filename, shouldRemoveFractions);
                    }
                    else if (".csv" == ext)
                    {
                        lines = CSVUtilities.GetLinesFromFile(filename);
                    }

                    loans = LoadLoans(lines, fieldsDef, filename);
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadLoansFileData file: " + filename + " does not exists!!!");
                }
            //}
            //catch (Exception e)
            //{
            //    WindowsUtilities.loggerMethod("ERROR: LoadLoansFileData got Exception: " + e.ToString()/* + ". line: " + line*/);
            //}

            return loans;
        }

        /*
         * Loading the loand can be asked to be from-line and to-line or by defining the exact loan-ids
         */
        private static LoanList LoadLoans(string[] loanLines, FieldList fieldsDef, string filename)
        {
            LoanContainer loanContainer = new LoanContainer();
            string line = MiscConstants.UNDEFINED_STRING;
            int lineNumber = 1;
            try
            {
                int fromLine = 0, toLine = loanLines.Length;
                int id = MiscUtilities.GetLoanID();
                string[] ids = null;

                // check the configured to load only specific IDs
                if (MiscConstants.UNDEFINED_STRING != Share.LoansLoadIDsFromLine)
                {
                    ids = Share.LoansLoadIDsFromLine.Split(MiscConstants.COMMA);
                }
                else
                {
                    // check the configured lines to load 
                    if (MiscConstants.UNDEFINED_UINT != Share.LoansLoadFromLine)
                        fromLine = (int)Share.LoansLoadFromLine;
                    if (MiscConstants.UNDEFINED_UINT != Share.LoansLoadToLine)
                        toLine = (int)Share.LoansLoadToLine;
                    if (fromLine > loanLines.Length)
                    {
                        WindowsUtilities.loggerMethod("NOTICE: LoadLoans ask to read from line: " + fromLine
                            + " while file have only: " + loanLines.Length);
                        return null;
                    }
                    if (toLine > loanLines.Length)
                    {
                        toLine = loanLines.Length;
                    }
                }

                for (int i = fromLine; i < toLine; i++)
                {
                    line = loanLines[i];
                    //curr = line;
                    if (String.IsNullOrEmpty(line))
                        continue;

                    // skip the first line
                    // TBD: should read the headers and relate to it
                    //if (1 == lineNumber++)
                    //    continue;

                    string[] entities = line.Split(MiscConstants.COMMA);

                    if (String.IsNullOrEmpty(entities[0]))
                    {
                        continue;
                    }

                    // skip the header
                    decimal number = 0;
                    if (!decimal.TryParse(entities[0], out number))
                    {
                        continue;
                    }

                    if (entities.Length != fieldsDef.Count)
                    {
                        WindowsUtilities.loggerMethod("ERROR: LoadLoans for loans file: " + filename + " is in a wrong syntax.");
                        return null;
                    }

                    // if asked for specific IDs, ensure correctness
                    if (null != ids)
                    {
                        if (!ids.Contains(entities[0]))
                            continue;
                    }

                    int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT); // // there is a index in the excel file
                    int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT); // + INDEX_ADD;
                    int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE); // + INDEX_ADD;
                    int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME); // + INDEX_ADD;
                    int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE); // + INDEX_ADD;
                    int ficoIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_FICO);
                    int dateTakenIndex = fieldsDef.GetIndexOf(MiscConstants.DATE_TAKEN);
                    //int desireTerminationMonthIndex = fieldsDef.GetIndexOf(MiscConstants.DESIRE_TERMINATION_MONTH);
                    int sequentialNumberIndex = fieldsDef.GetIndexOf(MiscConstants.SEQ_NUMBER);
                    //int originalProductIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_PRODUCT);
                    int originalRateIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_RATE);
                    int originalTimeIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_TIME);
                    int originalMarginIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_MARGIN);

                    int riskIndex = fieldsDef.GetIndexOf(MiscConstants.RISK_VALUE);
                    int liquidityIndex = fieldsDef.GetIndexOf(MiscConstants.LIQUIDITY_VALUE);
                    int productNameIndex = fieldsDef.GetIndexOf(MiscConstants.PRODUCT_NAME);

                    if (MiscConstants.UNDEFINED_INT < loanAmountIndex /*&& INDEX_ADD <= monthlyPaymentIndex*/ && MiscConstants.UNDEFINED_INT < propertyValueIndex &&
                        MiscConstants.UNDEFINED_INT < yearlyIncomeIndex /*&& INDEX_ADD < ficoIndex*/)
                    {
                        uint uyearlyIncomeIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, yearlyIncomeIndex));
                        uint uloanAmountIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, loanAmountIndex));
                        uint umonthlyPaymentIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, monthlyPaymentIndex));
                        uint upropertyValueIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, propertyValueIndex));
                        uint uageIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, ageIndex));
                        //uint desireTerminationMonthIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, desireTerminationMonthIndex));
                        uint sequentialNumberIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, sequentialNumberIndex));
                        // clean percantage etc.
                        double originalRateIndexV =
                            (originalRateIndex >= entities.Length) ? MiscConstants.UNDEFINED_DOUBLE :
                                Convert.ToDouble(MiscUtilities.CleanupRedundantChars(entities, originalRateIndex, true /*allowDot*/));
                        uint originalTimeIndexV = Convert.ToUInt32(MiscUtilities.CleanupRedundantChars(entities, originalTimeIndex));
                        double originalMarginIndexV =
                            (originalMarginIndex >= entities.Length) ? MiscConstants.UNDEFINED_DOUBLE :
                                Convert.ToDouble(MiscUtilities.CleanupRedundantChars(entities, originalMarginIndex, true /*allowDot*/));
                        DateTime dateTakenIndexV = new DateTime(); 

                        if (MiscConstants.UNDEFINED_UINT == sequentialNumberIndexV && MiscConstants.UNDEFINED_INT < sequentialNumberIndex)
                        {
                            CriteriaField cf = fieldsDef.GetField(MiscConstants.SEQ_NUMBER);
                            sequentialNumberIndexV = Convert.ToUInt32(cf.value) + (uint)lineNumber;
                        }
                        int ficoIndexV = (MiscConstants.UNDEFINED_INT < ficoIndex) ? Convert.ToInt32(MiscUtilities.CleanupRedundantChars(entities, ficoIndex)) : MiscConstants.UNDEFINED_INT;

                        try
                        {
                            dateTakenIndexV =
                                (0 > dateTakenIndex || dateTakenIndex >= entities.Length) ? DateTime.Now :
                                    Convert.ToDateTime(entities[dateTakenIndex]);
                            //DateTime dateS = MiscUtilities.ConvertDate(entities[dateTakenIndex]);
                        }
                        catch (Exception e)
                        {
                            WindowsUtilities.loggerMethod("ERROR: LoadLoans got date Exception: " + e.ToString() + ". line: " + line);
                        }


                        Risk risk = Risk.NONERisk; // (Risk)Enum.Parse(typeof(Risk), CleanupRedundantChars(entities, riskIndex), true);
                        Liquidity liquidity = Liquidity.NONELiquidity; // (Liquidity)Enum.Parse(typeof(Liquidity), CleanupRedundantChars(entities, liquidityIndex), true);

                        string productName =
                            (0 > productNameIndex || productNameIndex >= entities.Length) ?
                            MiscConstants.UNDEFINED_STRING : entities[productNameIndex];
                        ProductID product = new ProductID(MiscConstants.UNDEFINED_INT, productName);

                        if (0 < sequentialNumberIndexV)
                            id = (int)sequentialNumberIndexV;

                        /*loans*/
                        loanContainer.Add(new loanDetails(id.ToString(),
                            uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV,
                            uyearlyIncomeIndexV, uageIndexV, ficoIndexV,
                            dateTakenIndexV, product, true /*shouldCalculate*/,
                            originalRateIndexV, originalTimeIndexV, originalMarginIndexV,
                            /*desireTerminationMonthIndexV,*/ sequentialNumberIndexV, risk, liquidity));
                        id++;

                    }
                    else
                    {
                        string err = null;
                        if (0 > loanAmountIndex)
                            err += " illegal loanAmountIndex value: " + loanAmountIndex.ToString();
                        if (0 > monthlyPaymentIndex)
                            err += " illegal monthlyPaymentIndex value: " + monthlyPaymentIndex.ToString();
                        if (0 > propertyValueIndex)
                            err += " illegal propertyValueIndex value: " + propertyValueIndex.ToString();
                        if (0 > yearlyIncomeIndex)
                            err += " illegal yearlyIncomeIndex value: " + yearlyIncomeIndex.ToString();
                        //if (INDEX_ADD > ageIndex)
                        //    err += " illegal ageIndex value: " + ageIndex.ToString();

                        WindowsUtilities.loggerMethod("NOTICE: LoadLoans file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
                    }
                }
            }
            catch (Exception e)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadLoans got Exception: " + e.ToString() + ". line: " + line);
            }

            LoanList ll = loanContainer.GroupLoansByID();
            return ll;
        }

 
        static uint GetValue(string[] entities, int index)
        {
            uint value = MiscConstants.UNDEFINED_UINT;
            if (0 <= index && index < entities.Length)
            {
                int loc = entities[index].IndexOf(MiscConstants.DOT_STR);
                string trimed = (0 <= loc) ? entities[index].Remove(loc) : entities[index];
                loc = trimed.IndexOf(MiscConstants.PERCANTAGE_STR);
                trimed = (0 <= loc) ? trimed.Remove(loc) : trimed;

                // remove currency symbole
                string pattern = @"(\p{Sc}\s?)?";
                Regex rgx = new Regex(pattern);
                string result = rgx.Replace(trimed, "");
                //loc = trimed.IndexOf(MiscConstants.DOLLAR_STR);
                //trimed = (0 <= loc) ? trimed.Remove(loc) : trimed;
                value = Convert.ToUInt32(result);
            }
            return value;
        }

        public static void DoRemoveStringFromFile(string filename, string string2removeStart, string string2removeEnd)
        {
            string str = System.IO.File.ReadAllText(filename);
            // str = str.Replace(string2remove, "");
            string outs = DoRemoveStringFromString(str, string2removeStart, string2removeEnd);
            System.IO.File.WriteAllText(filename, outs);
        }

        public static string DoRemoveStringFromString(string inputString, string string2removeStart, string string2removeEnd)
        {
            const string anything = ".*";
            string pattern = string2removeStart + anything + string2removeEnd;
            var iqMatch = Regex.Match(inputString, pattern);
            string str = Regex.Replace(inputString, pattern, MiscConstants.UNDEFINED_STRING);
            return str;
        }


    }
}
