using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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


        //public static LoanList LoadExcelFileDataOLD(string filename, FieldList fieldsDef)
        //{
        //    LoanList loans = new LoanList();
        //    string currLine = MiscConstants.UNDEFINED_STRING;

        //    try
        //    {
        //        if (File.Exists(filename))
        //        {
        //            var file = new FileInfo(filename);
        //            var stream = new FileStream(filename, FileMode.Open);
        //            IExcelDataReader excelReader = null;

        //            if (file.Extension == ".xls")
        //            {
        //                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        //            }
        //            else if (file.Extension == ".xlsx")
        //            {
        //                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        //            }

        //            if (excelReader == null)
        //                return loans;

        //            excelReader.IsFirstRowAsColumnNames = true;
        //            DataSet ds = excelReader.AsDataSet();

        //            List<string> data = ReadAllData(ds);
        //            const int INDEX_ADD = 1;

        //            // id should be retrived by some logic
        //            int id = MiscConstants.GetLoanID();
        //            int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT); // // there is a index in the excel file
        //            int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT); // + INDEX_ADD;
        //            int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE); // + INDEX_ADD;
        //            int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME); // + INDEX_ADD;
        //            int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE); // + INDEX_ADD;
        //            int ficoIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_FICO);
                                        
        //            if (INDEX_ADD < loanAmountIndex && INDEX_ADD < monthlyPaymentIndex && INDEX_ADD < propertyValueIndex &&
        //                INDEX_ADD < yearlyIncomeIndex && INDEX_ADD < ficoIndex /*&& INDEX_ADD <= ageIndex*/)
        //            {
        //                // add to the memory
        //                foreach (string line in data)
        //                {
        //                    currLine = line;
        //                    // skip the header
        //                    if (!line.ToLower().Contains(MiscConstants.LOAN_AMOUNT.ToLower()))
        //                    {
        //                        string[] entities = line.Split(MiscConstants.SEERATOR_STR);
        //                        // be prapared for missing data: skip that entry
        //                        uint uloanAmountIndexV = Convert.ToUInt32(entities[loanAmountIndex]);
        //                        uint umonthlyPaymentIndexV = Convert.ToUInt32(entities[monthlyPaymentIndex]);
        //                        uint upropertyValueIndexV = Convert.ToUInt32(entities[propertyValueIndex]);
        //                        uint uyearlyIncomeIndexV = Convert.ToUInt32(entities[yearlyIncomeIndex]);
        //                        // age is not mandatory

        //                        //uint uageIndexV = Convert.ToUInt32(entities[ageIndex]);
        //                        uint uageIndexV = INDEX_ADD > ageIndex ? 0 : Convert.ToUInt32(entities[ageIndex]);
        //                        uint ficoIndexV = Convert.ToUInt32(entities[ficoIndex]);
                                
        //                        loans.Add(new loanDetails(id.ToString(),
        //                            uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV, 
        //                            uyearlyIncomeIndexV, uageIndexV, ficoIndexV));
        //                        id++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                string err = null;
        //                if (INDEX_ADD > loanAmountIndex)
        //                    err += " illegal loanAmountIndex value: " + loanAmountIndex.ToString();
        //                if (INDEX_ADD > monthlyPaymentIndex)
        //                    err += " illegal monthlyPaymentIndex value: " + monthlyPaymentIndex.ToString();
        //                if (INDEX_ADD > propertyValueIndex)
        //                    err += " illegal propertyValueIndex value: " + propertyValueIndex.ToString();
        //                if (INDEX_ADD > yearlyIncomeIndex)
        //                    err += " illegal yearlyIncomeIndex value: " + yearlyIncomeIndex.ToString();
        //                if (INDEX_ADD > ageIndex)
        //                    err += " illegal ageIndex value: " + ageIndex.ToString();

        //                WindowsUtilities.loggerMethod("NOTICE: LoadCSVFileData file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
        //            }


        //            // Free resources (IExcelDataReader is IDisposable)
        //            excelReader.Close();

        //        }
        //        else
        //        {
        //            WindowsUtilities.loggerMethod("LoadCSVFileData file: " + filename + " does not exists!!!");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        WindowsUtilities.loggerMethod("ERROR: LoadCSVFileData got Exception: " + e.ToString() + ". line: " + currLine);
        //    }

        //    return loans;
        //}

        private static List<string> ReadAllData(DataSet dataset)
        {
            string line = null;
            int pos = 0;
            List<string> lines = new List<string>();

            //Data Reader methods
            foreach (DataTable table in dataset.Tables)
            {

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    line = null;
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (0 < table.Rows[i].ItemArray[j].ToString().Length)
                        {
                            // avoid the fraction area
                            pos = table.Rows[i].ItemArray[j].ToString().IndexOf(MiscConstants.DOT_STR);
                            if (0 < pos)
                                line += table.Rows[i].ItemArray[j].ToString().Remove(pos) + MiscConstants.SEERATOR_STR.ToString();
                            else
                                line += table.Rows[i].ItemArray[j].ToString() + MiscConstants.SEERATOR_STR.ToString();
                        }
                    }
                    Console.WriteLine("Excel line is: " + line);
                    if (null != line)
                        lines.Add(line);
                }

            }
            return lines;
        }


        public static LoanList LoadCSVFileData(string filename, FieldList fieldsDef)
        {
            LoanList loans = new LoanList();
            string currLine = MiscConstants.UNDEFINED_STRING;

            string curr = MiscConstants.UNDEFINED_STRING;
            string[] entities;
            // id should be retrived by some logic
            int id = MiscConstants.GetLoanID();

            try
            {
                if (File.Exists(filename))
                {
                    var file = new FileInfo(filename);
                    StreamReader fileReader = new System.IO.StreamReader(filename);
                    string line;
                    int lineNumber = 1;
                    do
                    {
                        line = fileReader.ReadLine();
                        curr = line;
                        if (String.IsNullOrEmpty(line))
                            continue;

                        // skip the first line
                        // TBD: should read the headers and relate to it
                        if (1 == lineNumber++)
                            continue;

                        entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);

                        if (String.IsNullOrEmpty(entities[0]))
                        {
                            continue;
                        }

                        // ensure the line correctness
                        const int INDEX_ADD = 1;

                        int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT); // // there is a index in the excel file
                        int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT); // + INDEX_ADD;
                        int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE); // + INDEX_ADD;
                        int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME); // + INDEX_ADD;
                        int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE); // + INDEX_ADD;
                        int ficoIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_FICO);

                        if (MiscConstants.UNDEFINED_INT < loanAmountIndex /*&& INDEX_ADD <= monthlyPaymentIndex*/ && MiscConstants.UNDEFINED_INT < propertyValueIndex &&
                            MiscConstants.UNDEFINED_INT < yearlyIncomeIndex && INDEX_ADD < ficoIndex)
                        {
                            currLine = line;
                            // skip the header
                            if (!line.ToLower().Contains(MiscConstants.LOAN_AMOUNT.ToLower()))
                            {
                                // be prapared for missing data: skip that entry
                                uint umonthlyPaymentIndexV = MiscConstants.UNDEFINED_UINT;
                                uint uloanAmountIndexV = Convert.ToUInt32(entities[loanAmountIndex]);
                                if (MiscConstants.UNDEFINED_INT != monthlyPaymentIndex)
                                    umonthlyPaymentIndexV = Convert.ToUInt32(entities[monthlyPaymentIndex]);
                                // be ready to non define proiprty value
                                if (String.IsNullOrEmpty(entities[propertyValueIndex]))
                                    continue;
                                uint upropertyValueIndexV = Convert.ToUInt32(entities[propertyValueIndex]);
                                int index = entities[yearlyIncomeIndex].IndexOf(MiscConstants.DOT_STR);
                                string trimed = (0 < index) ? entities[yearlyIncomeIndex].Remove(index) : entities[yearlyIncomeIndex];
                                uint uyearlyIncomeIndexV = Convert.ToUInt32(trimed);
                                // age is not mandatory

                                //uint uageIndexV = Convert.ToUInt32(entities[ageIndex]);
                                uint uageIndexV = INDEX_ADD > ageIndex ? 0 : Convert.ToUInt32(entities[ageIndex]);
                                uint ficoIndexV = Convert.ToUInt32(entities[ficoIndex]);

                                loans.Add(new loanDetails(id.ToString(),
                                    uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV, uyearlyIncomeIndexV, uageIndexV, ficoIndexV));
                                id++;
                            }
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

                            WindowsUtilities.loggerMethod("NOTICE: LoadCSVFileData file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
                        }
                    }
                    while (!System.String.IsNullOrEmpty(line));

                }
                else
                {
                    Console.WriteLine("ERROR: LoadCSVFileData file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: LoadCSVFileData got Exception: " + e.ToString() + ". Curr: " + curr);
            }

        return loans;
        }

  

    }
}
