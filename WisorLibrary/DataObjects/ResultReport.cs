using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;

namespace WisorLibrary.DataObjects
{
    /*
     * manage pool of threads to store the data in the DB
     * and to create the reports
     */

    public class ResultReport
    {
        static public string GetFileName(string id)
        {
            string fn = AppDomain.CurrentDomain.BaseDirectory
                + MiscConstants.OUTPUT_DIR + Path.DirectorySeparatorChar +
                Share.ResultFileName + MiscConstants.NAME_SEP_CHAR + id + MiscConstants.NAME_SEP_CHAR +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.XML_EXT;
            return fn;
        }

        
        public ResultReport(loanDetails theLoan, Composition bestDiffComposition, 
            Composition bestBankComposition, Composition bestBorrowerComposition,
            bool ShouldStoreInDB, bool shouldCreateReport)
        {
            // store in the DB

            // store in XML file

            string fn = GetFileName(theLoan.ID);
            // ensure the directory realy exists
            if (!Directory.Exists(Path.GetDirectoryName(fn)))
                Directory.CreateDirectory(Path.GetDirectoryName(fn));

            StoreResultsAsXML(fn, theLoan, bestDiffComposition, bestBankComposition, bestBorrowerComposition);
            // cleanup the redundant xml statments
            string string2removeStart = @"<\?xml";
            string string2removeEnd = @"\?>";
            FileUtils.DoRemoveStringFromFile(fn, string2removeStart, string2removeEnd);
            
            // test - read from XML file
            loanDetails theLoanT;
            Composition bestDiffCompositionT, bestBankCompositionT, bestBorrowerCompositionT;

            LoadResultsFromXML(fn, out theLoanT, out bestDiffCompositionT,
            out bestBankCompositionT, out bestBorrowerCompositionT);
            // create the report

        }

        void StoreResultsAsXML(string filename, loanDetails theLoan, Composition bestDiffComposition,
            Composition bestBankComposition, Composition bestBorrowerComposition)
        {
            // Bank name, report ID, 

            XMLUtilities.WriteToXmlFile<loanDetails>(filename, theLoan);
            bestDiffComposition.name = "bestDiffComposition";
            XMLUtilities.WriteToXmlFile<Composition>(filename, bestDiffComposition, true);
            bestBankComposition.name = "bestBankComposition";
            XMLUtilities.WriteToXmlFile<Composition>(filename, bestBankComposition, true);
            bestBorrowerComposition.name = "bestBorrowerComposition";
            XMLUtilities.WriteToXmlFile<Composition>(filename, bestBorrowerComposition, true);
            // store the products which are involved and thier Rate and Margin
            // Share.theLoadedProducts + get the rate * 2 - but by which risk
            // TBD
        }

        public static void LoadResultsFromXML(string filename, out loanDetails theLoan, out Composition bestDiffComposition,
            out Composition bestBankComposition, out Composition bestBorrowerComposition)
        {
            theLoan = null;
            bestDiffComposition = bestBankComposition = bestBorrowerComposition = null;
            // ensure the file exists
            if (File.Exists(filename))
            {
                Composition[] compositions;
                XMLUtilities.LoadResultsXMLFile(filename, out theLoan, out compositions);
                // TBD - get the peroper compositions
                bestDiffComposition = compositions[0];
                bestBankComposition = compositions[1];
                bestBorrowerComposition = compositions[2];
            }
            else
            {
                WindowsUtilities.loggerMethod("LoadFromXML file: " + filename + " does not exists!!!");
            }
        }

        void Add2DBqueue(loanDetails theLoan, Composition bestDiffComposition,
            Composition bestBankComposition, Composition bestBorrowerComposition)
        {

        }

        void Add2Reportqueue(loanDetails theLoan, Composition bestDiffComposition,
            Composition bestBankComposition, Composition bestBorrowerComposition)
        {

        }
    }
}
