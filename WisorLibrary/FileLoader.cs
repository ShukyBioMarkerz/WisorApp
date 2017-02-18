using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WisorLib
{

    /// <summary>
    /// ///////////////////////
    /// </summary>


    public class FileUtils
    {

        public static FieldList LoadXMLFileData(string filename)
        {
            XDocument doc = null;
            FieldList fields = new FieldList();

            if (File.Exists(filename))
            {
                doc = XDocument.Load(filename);
                int index = 1;

                // loop all the items in the document
                foreach (XElement item in from c in doc.Descendants("item") select c)
                {
                    string type = item.Element("type").Value;
                    string id = item.Element("name").Value;
                    string defaultValue = null != item.Element("default") ? item.Element("default").Value : id;
                    string mandatoryValue = null != item.Element("mandatory") ? item.Element("mandatory").Value : "no";
                    bool isMandatory = "yes" == mandatoryValue; 
                    // <mandatory>yes</mandatory>
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
                    fields.Add(new CriteriaField(id, index++, value, min, max, isMandatory));
                }

            }
            else
            {
                Console.WriteLine("LoadFileData file: {0} does not exists!!!", filename);
            }

            return fields;
        }

        //string orderID = "", double loanAmtWanted = 0, double monthlyPmtWanted = 0,
        //        uint propertyValue = 0, uint income = 0, uint youngestLenderAge = 0

        public enum MortgageType { None = 0, FirstTimeBuyer = 1, Reordering = 2, Other = 3 };
        public enum PaymentType { None = 0, FullAmortizatio = 1, InterestOnly = 2, Ballon = 3 };
        public enum MortgagProduct { None = 0 };
        public enum CreditClass { None = 0, Exelent = 1, Good = 2, medium = 3, low = 4 };


        public static LoanList LoadCSVFileData(string filename, FieldList fieldsDef)
        {
            LoanList loans = new LoanList();

            try
            {
                if (File.Exists(filename))
                {
                    var file = new FileInfo(filename);
                    var stream = new FileStream(filename, FileMode.Open);
                    IExcelDataReader excelReader = null;

                    if (file.Extension == ".xls")
                    {
                        excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (file.Extension == ".xlsx")
                    {
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }

                    if (excelReader == null)
                        return loans;

                    excelReader.IsFirstRowAsColumnNames = true;
                    DataSet ds = excelReader.AsDataSet();

                    List<string> data = ReadAllData(ds);

                    // id should be retrived by some logic
                    int id = MiscConstants.GetLoanID();
                    int loanAmountIndex = fieldsDef.GetIndexOf(MiscConstants.LOAN_AMOUNT);
                    int monthlyPaymentIndex = fieldsDef.GetIndexOf(MiscConstants.MONTHLY_PAYMENT);
                    int propertyValueIndex = fieldsDef.GetIndexOf(MiscConstants.PROPERTY_VALUE);
                    int yearlyIncomeIndex = fieldsDef.GetIndexOf(MiscConstants.YEARLY_INCOME);
                    int ageIndex = fieldsDef.GetIndexOf(MiscConstants.AGE);

                    if (0 <= loanAmountIndex && 0 <= monthlyPaymentIndex && 0 <= propertyValueIndex &&
                        0 <= yearlyIncomeIndex && 0 <= ageIndex)
                    {
                        // add to the memory
                        foreach (string line in data)
                        {
                            // skip the header
                            if (!line.ToLower().Contains(MiscConstants.LOAN_AMOUNT.ToLower()))
                            {
                                string[] entities = line.Split(MiscConstants.SEERATOR_STR);
                                uint uloanAmountIndexV = Convert.ToUInt32(entities[loanAmountIndex]);
                                uint umonthlyPaymentIndexV = Convert.ToUInt32(entities[monthlyPaymentIndex]);
                                uint upropertyValueIndexV = Convert.ToUInt32(entities[propertyValueIndex]);
                                uint uyearlyIncomeIndexV = Convert.ToUInt32(entities[yearlyIncomeIndex]);
                                uint uageIndexV = Convert.ToUInt32(entities[ageIndex]);

                                loans.Add(new loanDetails(id.ToString(),
                                    uloanAmountIndexV, umonthlyPaymentIndexV, upropertyValueIndexV, uyearlyIncomeIndexV, uageIndexV));
                                id++;
                            }
                        }
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
                        if (0 > ageIndex)
                            err += " illegal ageIndex value: " + ageIndex.ToString();

                        Console.WriteLine("NOTICE: LoadCSVFileData file: illegal index of the mandatory parameters. Probably missing some cretiria. " + err);
                    }


                    // Free resources (IExcelDataReader is IDisposable)
                    excelReader.Close();

                }
                else
                {
                    Console.WriteLine("LoadCSVFileData file: {0} does not exists!!!", filename);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: LoadCSVFileData got Exception: " + e.ToString());
            }

            return loans;
        }

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


    }
}
