using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLibrary.Utilities
{
    public class ExcelUtilities
    {
        /// <summary>
        /// Read the loans from excel file
        /// </summary>
        
        static bool ShouldRemoveFractions;

        private static string[] OpenExcelFile(string filename)
        {
            string[] excelLines = null;

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
                        return excelLines;

                    //excelReader.IsFirstRowAsColumnNames = true;
                    DataSet ds = excelReader.AsDataSet();

                    List<string> Lines = ReadAllData(ds);

                    // Free resources 
                    if (null != excelReader)
                        excelReader.Close();

                    excelLines = Lines.ToArray();
                }
                else
                {
                    WindowsUtilities.loggerMethod("OpenExcelFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                WindowsUtilities.loggerMethod("ERROR: OpenExcelFile got Exception: " + e.ToString()/* + ". line: " + currLine*/);
            }

            return excelLines;
        }

        public static string[] GetLinesFromFile(string filename, bool shouldRemoveFractions/* = true*/)
        {
            ShouldRemoveFractions = shouldRemoveFractions;
            string[] lines = OpenExcelFile(filename);
            return lines;
        }

   
        private static List<string> ReadAllData(DataSet dataset)
        {
            string line = null;
            int pos = 0;
            List<string> lines = new List<string>();
            //int lineNumber = 1;
            DateTime dateValue;

            //Data Reader methods
            foreach (DataTable table in dataset.Tables)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    line = null;
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        string str = table.Rows[i].ItemArray[j].ToString();
                        if (0 < str.Length)
                        {
                            // avoid the fraction area
                            if (!DateTime.TryParse(str, out dateValue))
                            {
                                if (ShouldRemoveFractions)
                                    pos = str.IndexOf(MiscConstants.DOT_STR);
                                else
                                    pos = 0;

                                if (0 < pos)
                                    line += str.Remove(pos).Trim() + MiscConstants.COMMA;
                                else
                                    line += str + MiscConstants.COMMA;
                            }
                            else
                            {
                                // it is a date
                                line += str + MiscConstants.COMMA;
                            }
                        }
                    }
                    //Console.WriteLine("Excel line is: " + line);
                    // skip the header
                    //if (1 == lineNumber++)
                    //    continue;

                    if (null != line)
                    {
                        // remove the trailing comma
                        line = line.Remove(line.LastIndexOf(MiscConstants.COMMA));
                        lines.Add(line);
                    }
                }

            }
            return lines;
        }


    }
}
