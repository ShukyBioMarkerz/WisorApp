using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLibrary.DataObjects
{
    public class LoggerFile
    {
        /// <summary>
        /// Enable to write logs
        /// </summary>
        /// 
        private StreamWriter fileStream;
        private string filename;

        public LoggerFile(string outputFilename, string additionalName = MiscConstants.UNDEFINED_STRING, 
            bool mustCreate = false, bool append = true)
        {
            fileStream = null;
    
            if (Share.ShouldStoreAllCombinations || mustCreate)
            {
                string fn = System.IO.Path.GetFileNameWithoutExtension(outputFilename);
                string ext = System.IO.Path.GetExtension(outputFilename);
                string dir = System.IO.Path.GetDirectoryName(outputFilename);
                filename = dir + System.IO.Path.DirectorySeparatorChar + fn + additionalName + MiscConstants.LOGGER_FILE + ext;

                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                fileStream = new StreamWriter(filename, append);
                // will flush its buffer to the underlying stream after every call to StreamWriter.Write.
                // bad performance, yet enable to break the run in the middle and still get the output lines
                // but without the summary.....
                // that's life 
                fileStream.AutoFlush = true;
            }
        }

        public void PrintLog2CSV(string[] msg)
        {
            if (/*Share.ShouldStoreAllCombinations && */null != fileStream)
            {
                try
                {
                    string msg2write = null;
                    for (int i = 0; i < msg.Length; i++)
                    {
                        msg2write += msg[i] + MiscConstants.COMMA;
                    }

                    fileStream.WriteLine(msg2write);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: PrintLog2CSV got Exception: " + e.ToString());
                }
            }
        }

        public void PrintLog(string msg)
        {
            if (/*Share.ShouldStoreAllCombinations &&*/ null != fileStream)
            {
                try
                {
                   fileStream.WriteLine(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: PrintLog got Exception: " + e.ToString());
                }
            }
        }

        public void CloseLog2CSV()
        {
            if (/*Share.ShouldStoreAllCombinations && */null != fileStream)
            {
                fileStream.Close();
            }
        }

    }
}
