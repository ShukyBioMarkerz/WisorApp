using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using WisorLibrary.Reporting;
using static WisorLib.MiscConstants;

namespace WisorLibrary.DataObjects
{
    public class CompositionReportData
    {
        public string name;
        public uint ttlPmt, ttlPay;
        public uint optXBankTtlPay, optYBankTtlPay, optZBankTtlPay;
        public OptionReportData[] optionReportData;

    }

    public class OptionReportData
    {
        public int optType;
        public string optTypeName;
        public uint optAmt, optTime, optPmt ,optTtlPay;
        public double optRateFirstPeriod;
    }

    public class ResultReportData
    {
        // Loan data
        public string BankName;
        public string ID { get; set; }
        public string ProductName { get; set; }
        public uint PropertyValue { get; set; }
        public uint DesiredMonthlyPayment { get; set; }
        public uint LoanAmount { get; set; }
        public uint OriginalLoanAmount { get; set; }
        public int fico { get; set; }
        public DateTime DateTaken { get; set; }
        public uint DesireTerminationMonth { get; set; }
        public uint BorrowerAge { get; set; }
        public uint YearlyIncome { get; set; }
        public double OriginalRate { get; set; }
        public double OriginalMargin { get; set; }
        public uint OriginalTime { get; set; }
        //public indices indices { set; get; }
        public double OriginalInflation { set; get; }
        public double Debt2Income { set; get; }
        // the selected compositions
        public Composition[] compositions { get; set; }
        CompositionReportData[] compositionReportData;

        // calculation data
        public uint RemaingLoanTime { get; set; }
        public uint FirstMonthlyPMT { get; set; }
        public uint PayUntilToday { get; set; }
        public uint Left2Pay { get; set; }
        public uint EstimateFuturePay { get; set; }
        public double EstimateMargin { get; set; }
        public uint EstimateProfitSoFar { get; set; }
        public double EstimateProfitPercantageSoFar { get; set; }
        public uint EstimateTotalProfit { get; set; }
        public double EstimateTotalProfitPercantage { get; set; }
        public uint EstimateFutureProfit { get; set; }
        public double EstimateFutureProfitPercantage { get; set; }
        public uint RemaingLoanAmount { get; set; }
        public double PTI { get; set; }

        // performance data
        public DateTime StartTime { get; set; }
        public TimeSpan CalculationTime { get; set; }
        public DateTime TotalTime { get; set; }

        static public string GetFileName(string id)
        {
            string fn = AppDomain.CurrentDomain.BaseDirectory
                + MiscConstants.REPORTS_DIR + Path.DirectorySeparatorChar +
                MiscConstants.LENDER_REPORT_PREFIX + MiscConstants.NAME_SEP_CHAR + id + MiscConstants.NAME_SEP_CHAR +
                DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + MiscConstants.XML_EXT;
            return fn;
        }

        public string[] GetProducts()
        {
            return Share.theProductsNames;
        }

        public CompositionReportData[] GetCompositionData()
        {
            return compositionReportData;
        }

        public void SetCompositionData(Composition[] compositions)
        {
            // for the sake of storing as XML
            this.compositions = compositions;

            // prepare the data for reporting
            compositionReportData = new CompositionReportData[compositions.Length];
            for (int i = 0; i < compositions.Length; i++)
            {
                compositionReportData[i] = new CompositionReportData();
                compositionReportData[i].name = compositions[i].name;
                compositionReportData[i].optXBankTtlPay = compositions[i].optXBankTtlPay;
                compositionReportData[i].optYBankTtlPay = compositions[i].optYBankTtlPay;
                compositionReportData[i].optZBankTtlPay = compositions[i].optZBankTtlPay;
                compositionReportData[i].ttlPay = (uint) Math.Round(compositions[i].ttlPay);
                compositionReportData[i].ttlPmt = (uint)Math.Round(compositions[i].ttlPmt);
                compositionReportData[i].optionReportData = new OptionReportData[compositions[i].opts.Length];
                for (int j = 0; j < compositions[i].opts.Length; j++)
                {
                    compositionReportData[i].optionReportData[j] = new OptionReportData();
                    compositionReportData[i].optionReportData[j].optAmt = (uint)Math.Round(compositions[i].opts[j].optAmt);
                    compositionReportData[i].optionReportData[j].optPmt = (uint)Math.Round(compositions[i].opts[j].optPmt);
                    compositionReportData[i].optionReportData[j].optRateFirstPeriod = compositions[i].opts[j].optRateFirstPeriod;
                    compositionReportData[i].optionReportData[j].optTime = compositions[i].opts[j].optTime;
                    compositionReportData[i].optionReportData[j].optTtlPay = (uint)Math.Round(compositions[i].opts[j].optTtlPay);
                    compositionReportData[i].optionReportData[j].optType = compositions[i].opts[j].optType;
                    compositionReportData[i].optionReportData[j].optTypeName = compositions[i].opts[j].product.name;
                }
            }
        }

        public void Activate(bool shouldStoreInDB, bool shouldCreateReport)
        {
            string fn = GetFileName(ID);

            // store in XML file
            if (shouldStoreInDB)
            {
                // ensure the directory realy exists
                if (!Directory.Exists(Path.GetDirectoryName(fn)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fn));

                StoreResultsAsXML(fn);
                //// cleanup the redundant xml statments
                //string string2removeStart = @"<\?xml";
                //string string2removeEnd = @"\?>";
                //FileUtils.DoRemoveStringFromFile(fn, string2removeStart, string2removeEnd);

                // test - read from XML file
                ResultReportData resultReportData;
                LoadResultsFromXML(fn, out resultReportData);
            }

            // create the report
            if (shouldCreateReport)
            {
                  Reporter.LenderReport(fn, this, true /* shouldCreateHTML */ , true /* shouldCreatePDF */
                    /*, CultureInfo cultureInfo*/);
            }
        }
      
       void StoreResultsAsXML(string filename)
        {
            XMLUtilities.WriteToXmlFile<ResultReportData>(filename, this);
         }

        public static void LoadResultsFromXML(string filename, out ResultReportData resultReportData)
        {
            resultReportData = null;

             // ensure the file exists
            if (File.Exists(filename))
            {
                resultReportData = XMLUtilities.ReadFromXmlFile<ResultReportData>(filename);
            }
            else
            {
                WindowsUtilities.loggerMethod("LoadResultsFromXML file: " + filename + " does not exists!!!");
            }
        }

    }
}
