using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLibrary.Utilities
{
    class CSVUtilities
    {
 
        private static string[] OpenCSVFile(string filename)
        {
            List<string> lines = new List<string>();
            string line = MiscConstants.UNDEFINED_STRING;
            string[] csvLines = null;

            try
            {
                if (File.Exists(filename))
                {
                    var file = new FileInfo(filename);
                    StreamReader fileReader = new System.IO.StreamReader(filename);
                    int lineNumber = 1;

                    do
                    {
                        line = fileReader.ReadLine();
                        // skip the header
                        if (1 == lineNumber++)
                            continue;

                        if (!System.String.IsNullOrEmpty(line))
                            lines.Add(line);
                    }
                    while (!System.String.IsNullOrEmpty(line));

                    fileReader.Close();

                    csvLines = lines.ToArray();
 
                }
                else
                {
                    Console.WriteLine("ERROR: OpenCSVFile file: " + filename + " does not exists!!!");
                    WindowsUtilities.loggerMethod("ERROR: OpenCSVFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: OpenCSVFile got Exception: " + e.ToString() + ". line: " + line);
            }

            return csvLines;
        }

        public static string[] GetLinesFromFile(string filename)
        {
            string[] lines = OpenCSVFile(filename);
            return lines;
        }
        
    }
}
