using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLibrary.DataObjects
{
    public class LogCombinationResults
    {
        /// <summary>
        /// Enable to write logs
        /// </summary>
        /// 
        private StreamWriter fileStream;
        private string filename;

        public LogCombinationResults(string outputFilename, string additionalName = MiscConstants.UNDEFINED_STRING)
        {
            fileStream = null;
    
            if (Share.ShouldStoreAllCombinations)
            {
                string fn = System.IO.Path.GetFileNameWithoutExtension(outputFilename);
                string ext = System.IO.Path.GetExtension(outputFilename);
                string dir = System.IO.Path.GetDirectoryName(outputFilename);
                filename = dir + System.IO.Path.DirectorySeparatorChar + fn + additionalName + MiscConstants.LOGGER_FILE + ext;

                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                fileStream = new StreamWriter(filename);
            }
        }

        public void PrintLog2CSV(string[] msg)
        {
            if (Share.ShouldStoreAllCombinations && null != fileStream)
            {
                try
                {
                    string msg2write = null;
                    for (int i = 0; i < msg.Length; i++)
                    {
                        msg2write += msg[i] + MiscConstants.COMMA_SEERATOR_STR;
                    }

                    fileStream.WriteLine(msg2write);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: PrintLog2CSV got Exception: " + e.ToString());
                }
            }
        }

        public void CloseLog2CSV()
        {
            if (Share.ShouldStoreAllCombinations && null != fileStream)
            {
                fileStream.Close();
            }
        }

    }
}
