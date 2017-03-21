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
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;

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


  
   
        public static LoanList LoadLoansFileData(string filename, FieldList fieldsDef)
        {
            string[] lines = null;
            LoanList loans = new LoanList();

            try
            {
                if (File.Exists(filename))
                {
                    string ext = Path.GetExtension(filename);

                    if (".xls" == ext || ".xlsx" == ext)
                    {
                        lines = ExcelUtilities.GetLinesFromFile(filename);
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
            }
            catch (Exception e)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadLoansFileData got Exception: " + e.ToString()/* + ". line: " + line*/);
            }

            return loans;
        }

        private static LoanList LoadLoans(string[] loanLines, FieldList fieldsDef, string filename)
        {
            LoanList loans = new LoanList();
            string line;
            int lineNumber = 1;
            int id = MiscUtilities.GetLoanID();

            for (int i = 0; i < loanLines.Length; i++)
            {
                line = loanLines[i];
                //curr = line;
                if (String.IsNullOrEmpty(line))
                    continue;

                // skip the first line
                // TBD: should read the headers and relate to it
                //if (1 == lineNumber++)
                //    continue;
             
                string[] entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);

                if (String.IsNullOrEmpty(entities[0]))
                {
                    continue;
                }

                // skip the header
                decimal number = 0;
                if (! decimal.TryParse(entities[0], out number))
                {
                    continue;
                }

                if (entities.Length != fieldsDef.Count)
                {
                    Console.WriteLine("ERROR: LoadLoans for loans file: " + filename + " is in a wrong syntax.");
                    WindowsUtilities.loggerMethod("ERROR: LoadLoans for loans file: " + filename + " is in a wrong syntax.");
                    return loans;
                }

                // ensure the line correctness
                const int INDEX_ADD = 1;

                int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT); // // there is a index in the excel file
                int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT); // + INDEX_ADD;
                int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE); // + INDEX_ADD;
                int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME); // + INDEX_ADD;
                int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE); // + INDEX_ADD;
                int ficoIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_FICO);
                int dateTakenIndex = fieldsDef.GetIndexOf(MiscConstants.DATE_TAKEN);
                int desireTerminationMonthIndex = fieldsDef.GetIndexOf(MiscConstants.DESIRE_TERMINATION_MONTH);
                int sequentialNumberIndex = fieldsDef.GetIndexOf(MiscConstants.SEQ_NUMBER);
                //int originalProductIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_PRODUCT);
                int originalRateIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_RATE);
                int originalTimeIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_TIME);

                if (MiscConstants.UNDEFINED_INT < loanAmountIndex /*&& INDEX_ADD <= monthlyPaymentIndex*/ && MiscConstants.UNDEFINED_INT < propertyValueIndex &&
                    MiscConstants.UNDEFINED_INT < yearlyIncomeIndex && INDEX_ADD < ficoIndex)
                {
                    uint uyearlyIncomeIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, yearlyIncomeIndex));
                    uint uloanAmountIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, loanAmountIndex));
                    uint umonthlyPaymentIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, monthlyPaymentIndex));
                    uint upropertyValueIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, propertyValueIndex));
                    uint uageIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, ageIndex));
                    uint ficoIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, ficoIndex));
                    uint desireTerminationMonthIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, desireTerminationMonthIndex));
                    uint sequentialNumberIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, sequentialNumberIndex));
                    // clean percantage etc.
                    double originalRateIndexV =
                        (originalRateIndex >= entities.Length) ? MiscConstants.UNDEFINED_DOUBLE :
                            Convert.ToDouble(CleanupRedundantChars(entities, originalRateIndex, true /*allowDot*/));
                    uint originalTimeIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, originalTimeIndex));
                    if (MiscConstants.UNDEFINED_UINT == sequentialNumberIndexV && MiscConstants.UNDEFINED_INT < sequentialNumberIndex)
                    {
                        CriteriaField cf = fieldsDef.GetField(MiscConstants.SEQ_NUMBER);
                        sequentialNumberIndexV = Convert.ToUInt32(cf.value) + (uint)lineNumber;
                    }

                    DateTime dateTakenIndexV =
                        (dateTakenIndex >= entities.Length) ? DateTime.Now : Convert.ToDateTime(entities[dateTakenIndex]);

                    loans.Add(new loanDetails(id.ToString(),
                        uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV,
                        uyearlyIncomeIndexV, uageIndexV, ficoIndexV,
                        dateTakenIndexV, originalRateIndexV, originalTimeIndexV,
                        desireTerminationMonthIndexV, sequentialNumberIndexV));
                    id++;
                    
                }
                else
                {
                    string err = null;
                    if (INDEX_ADD > loanAmountIndex)
                        err += " illegal loanAmountIndex value: " + loanAmountIndex.ToString();
                    if (INDEX_ADD > monthlyPaymentIndex)
                        err += " illegal monthlyPaymentIndex value: " + monthlyPaymentIndex.ToString();
                    if (INDEX_ADD > propertyValueIndex)
                        err += " illegal propertyValueIndex value: " + propertyValueIndex.ToString();
                    if (INDEX_ADD > yearlyIncomeIndex)
                        err += " illegal yearlyIncomeIndex value: " + yearlyIncomeIndex.ToString();
                    if (INDEX_ADD > ageIndex)
                        err += " illegal ageIndex value: " + ageIndex.ToString();

                    WindowsUtilities.loggerMethod("NOTICE: LoadLoans file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
                }
            }
 
            return loans;
        }

        //public static LoanList LoadCSVFileData(string filename, FieldList fieldsDef)
        //{
        //    LoanList loans = new LoanList();
        //    string currLine = MiscConstants.UNDEFINED_STRING;

        //    string curr = MiscConstants.UNDEFINED_STRING;
        //    string[] entities;
        //    // id should be retrived by some logic
        //    int id = MiscUtilities.GetLoanID();

        //    try
        //    {
        //        if (File.Exists(filename))
        //        {
        //            var file = new FileInfo(filename);
        //            StreamReader fileReader = new System.IO.StreamReader(filename);
        //            string line;
        //            int lineNumber = 1;
        //            do
        //            {
        //                line = fileReader.ReadLine();
        //                curr = line;
        //                if (String.IsNullOrEmpty(line))
        //                    continue;

        //                // skip the first line
        //                // TBD: should read the headers and relate to it
        //                if (1 == lineNumber++)
        //                    continue;

        //                entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);

        //                if (String.IsNullOrEmpty(entities[0]))
        //                {
        //                    continue;
        //                }

        //                if (entities.Length != fieldsDef.Count)
        //                {
        //                    Console.WriteLine("ERROR: LoadCSVFileData for loans file: " + filename + " is in a wrong syntax.");
        //                    WindowsUtilities.loggerMethod("ERROR: LoadCSVFileData for loans file: " + filename + " is in a wrong syntax.");
        //                    return loans;
        //                }

        //                // ensure the line correctness
        //                const int INDEX_ADD = 1;

        //                int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT); // // there is a index in the excel file
        //                int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT); // + INDEX_ADD;
        //                int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE); // + INDEX_ADD;
        //                int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME); // + INDEX_ADD;
        //                int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE); // + INDEX_ADD;
        //                int ficoIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_FICO);
        //                int dateTakenIndex = fieldsDef.GetIndexOf(MiscConstants.DATE_TAKEN);
        //                int desireTerminationMonthIndex = fieldsDef.GetIndexOf(MiscConstants.DESIRE_TERMINATION_MONTH);
        //                int sequentialNumberIndex = fieldsDef.GetIndexOf(MiscConstants.SEQ_NUMBER);
        //                //int originalProductIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_PRODUCT);
        //                int originalRateIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_RATE);
        //                int originalTimeIndex = fieldsDef.GetIndexOf(MiscConstants.ORIGINAL_TIME);

        //                if (MiscConstants.UNDEFINED_INT < loanAmountIndex /*&& INDEX_ADD <= monthlyPaymentIndex*/ && MiscConstants.UNDEFINED_INT < propertyValueIndex &&
        //                    MiscConstants.UNDEFINED_INT < yearlyIncomeIndex && INDEX_ADD < ficoIndex)
        //                {
        //                    currLine = line;
        //                    // skip the header
        //                    if (!line.ToLower().Contains(MiscConstants.LOAN_AMOUNT.ToLower()))
        //                    {
        //                        uint uyearlyIncomeIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, yearlyIncomeIndex));
        //                        uint uloanAmountIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, loanAmountIndex));
        //                        uint umonthlyPaymentIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, monthlyPaymentIndex));
        //                        uint upropertyValueIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, propertyValueIndex));
        //                        uint uageIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, ageIndex));
        //                        uint ficoIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, ficoIndex));
        //                        uint desireTerminationMonthIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, desireTerminationMonthIndex));
        //                        uint sequentialNumberIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, sequentialNumberIndex));
        //                        // clean percantage etc.
        //                        double originalRateIndexV = 
        //                            (originalRateIndex >= entities.Length) ? MiscConstants.UNDEFINED_DOUBLE : 
        //                                Convert.ToDouble(CleanupRedundantChars(entities, originalRateIndex, true /*allowDot*/));
        //                        uint originalTimeIndexV = Convert.ToUInt32(CleanupRedundantChars(entities, originalTimeIndex));
        //                        if (MiscConstants.UNDEFINED_UINT == sequentialNumberIndexV && MiscConstants.UNDEFINED_INT < sequentialNumberIndex)
        //                        {
        //                            CriteriaField cf = fieldsDef.GetField(MiscConstants.SEQ_NUMBER);
        //                            sequentialNumberIndexV = Convert.ToUInt32(cf.value) + (uint)lineNumber;
        //                        }
                        
        //                        DateTime dateTakenIndexV = 
        //                            (dateTakenIndex >= entities.Length) ? DateTime.Now : Convert.ToDateTime(entities[dateTakenIndex]);

        //                        loans.Add(new loanDetails(id.ToString(),
        //                            uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV, 
        //                            uyearlyIncomeIndexV, uageIndexV, ficoIndexV,
        //                            dateTakenIndexV, originalRateIndexV, originalTimeIndexV,
        //                            desireTerminationMonthIndexV, sequentialNumberIndexV));
        //                        id++;
        //                    }
        //                }
        //                else
        //                {
        //                    string err = null;
        //                    if (INDEX_ADD > loanAmountIndex)
        //                        err += " illegal loanAmountIndex value: " + loanAmountIndex.ToString();
        //                    if (INDEX_ADD > monthlyPaymentIndex)
        //                        err += " illegal monthlyPaymentIndex value: " + monthlyPaymentIndex.ToString();
        //                    if (INDEX_ADD > propertyValueIndex)
        //                        err += " illegal propertyValueIndex value: " + propertyValueIndex.ToString();
        //                    if (INDEX_ADD > yearlyIncomeIndex)
        //                        err += " illegal yearlyIncomeIndex value: " + yearlyIncomeIndex.ToString();
        //                    if (INDEX_ADD > ageIndex)
        //                        err += " illegal ageIndex value: " + ageIndex.ToString();

        //                    WindowsUtilities.loggerMethod("NOTICE: LoadCSVFileData file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
        //                }
        //            }
        //            while (!System.String.IsNullOrEmpty(line));

        //        }
        //        else
        //        {
        //            Console.WriteLine("ERROR: LoadCSVFileData file: " + filename + " does not exists!!!");
        //            WindowsUtilities.loggerMethod("ERROR: LoadCSVFileData file: " + filename + " does not exists!!!");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("ERROR: LoadCSVFileData got Exception: " + e.ToString() + ". Curr: " + curr);
        //    }

        //return loans;
        //}

        static string CleanupRedundantChars(string[] entities, int index, bool allowDot = false, string defaultValue = "0")
        {
            string value = MiscConstants.UNDEFINED_STRING;
            if (0 <= index && index < entities.Length)
            {
                string trimed;
                int loc;
                if (! allowDot)
                {
                    loc = entities[index].IndexOf(MiscConstants.DOT_STR);
                    trimed = (0 <= loc) ? entities[index].Remove(loc) : entities[index];
                }
                else
                {
                    trimed = entities[index];
                }
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


    }
}
