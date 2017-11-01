//#define WDEBUG

#if WDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using RateUtilities;
using System.Globalization;
#else
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using WisorLibrary.DataObjects;
using WisorLib;
using WisorLibrary.Logic;
using System.Linq;
using WisorLibrary.Utilities;
using System.Globalization;
#endif



#if WDEBUG
namespace Wisor
#else
namespace WisorLibrary.Reporting
#endif
{
    class LenderReport
    {
        private static bool ISFirstLenderReportInit = true;
        private static String HtmlHeader;
        private static String HtmlFooter;
        private static String CompositionTemplate;
        private static List<String> LenderPages;
        private static String PagePrefix;
        private static String PageSuffix;
        private String BankLogoPath;
        private String TopBarBankLogoPath;
        private static String SummaryStr;
        private string CurrencySymbol;
        private static CultureInfo CultureInformation;
        private bool IsPrintCovers;
        bool IsNewLoan;
        int numOfPages;



        public LenderReport(CultureInfo cultureInfo, bool isPrintCovers = true)
        {
            //// TBD - debug the English PDF version
            //cultureInfo = CultureInfo.CreateSpecificCulture("en-US");


            CultureInformation = cultureInfo;
            IsPrintCovers = isPrintCovers;
            if (CultureInformation.Name.Equals("he-IL"))
            {
                CurrencySymbol = "₪";
                SummaryStr = "סה\"כ";
            }
            else if (CultureInformation.Name.Equals("en-US"))
            {
                CurrencySymbol = "$";
                SummaryStr = "Summary";

            }
            else
            {
                CurrencySymbol = "$";
                SummaryStr = "Summary";
            }

            BankLogoPath = "images/bank_leumi_logo.png";
            TopBarBankLogoPath = "images/bank_leumi_logo.png";


            if (ISFirstLenderReportInit)
            {
                ISFirstLenderReportInit = false;
                OnInit();
            }
        }

        public static void OnInit()
        {
            try
            {
                // Load Lender Report Resources

                Assembly assembly = Assembly.GetExecutingAssembly();

                LenderPages = new List<String>();

                if (CultureInformation.TextInfo.IsRightToLeft)
                {
                    string fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_header_rtl.html");
                    HtmlHeader = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                    fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_footer_rtl.html");
                    HtmlFooter = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                    fullResourceName = MiscUtilities.GetResourceStream("lender_composition_template_rtl.html");
                    CompositionTemplate = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();

                    for (int i = 1; i <= 4; i++)
                    {
                        fullResourceName = MiscUtilities.GetResourceStream(String.Format("lender_template_page{0}_rtl.html", i));
                        LenderPages.Add((new StreamReader(assembly.GetManifestResourceStream(fullResourceName)).ReadToEnd()));
                    }
                }
                else
                {
                    string fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_header_ltr.html");
                    HtmlHeader = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                    fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_footer_ltr.html");
                    HtmlFooter = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                    fullResourceName = MiscUtilities.GetResourceStream("lender_composition_template_ltr.html");
                    CompositionTemplate = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();

                    for (int i = 1; i <= 4; i++)
                    {
                        fullResourceName = MiscUtilities.GetResourceStream(String.Format("lender_template_page{0}_ltr.html", i));
                        LenderPages.Add((new StreamReader(assembly.GetManifestResourceStream(fullResourceName)).ReadToEnd()));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LenderReport::OnInit ex: " + ex.ToString());
            }

            PagePrefix = "<div class='a4'>";
            PageSuffix = "</div>";
        }



        public int GenerateLenderHtmlReport(string filename, RunEnvironment env)
        {
            //ResultReportData reportData = env.theLoan.resultReportData;

            try
            {
                if (null != env)
                {
                    SetParams(env);
                    
                    // HTML Header
                    var reportContent = new StringBuilder(GetHTMLHeader());

                    if (IsPrintCovers)
                    {
                        // Page 1
                        reportContent.Append(PagePrefix);
                        reportContent.Append(GetPage1Body(env));
                        reportContent.Append(GetFooterBar("footer-bar-html", ""));
                        reportContent.Append(PageSuffix);
                    }

                    // Page 2
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage2Body(env));
                    reportContent.Append(GetFooterBar("footer-bar-html", "1"));
                    reportContent.Append(PageSuffix);

                    // Page 3
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage3Body(env));
                    reportContent.Append(GetFooterBar("footer-bar-html", "2"));
                    reportContent.Append(PageSuffix);

                    if (IsPrintCovers)
                    {
                        // Page 4
                        reportContent.Append(PagePrefix);
                        reportContent.Append(GetPage4Body(env));
                        reportContent.Append(GetFooterBar("footer-bar-html", ""));
                        reportContent.Append(PageSuffix);

                    }

                    // HTML Footer
                    reportContent.Append(HtmlFooter);

                    // Save HTML report on disk
                    System.IO.File.WriteAllText(filename, reportContent.ToString());

                }
                else
                {
                    Console.WriteLine("ERROR GenerateLenderHtmlReport: reportData is null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GenerateLenderHtmlReport::ex: " + ex.ToString());
            }

            return 0;
        }

        public int GenerateLenderPdfReport(string filename, RunEnvironment env)
        {
            SetParams(env);
            
            // Generate individual HTML pages

            // TBD - here is the place to decide if the header and footer are needed. 
            // 1. Skip the pages construction. 2. chage pdfPaths and htmlPaths

            String Page1Content = null;
            String Page2Content = null;
            String Page3Content = null;
            String Page4Content = null;

            var pdfPaths = new List<String>();
            var htmlPaths = new List<String>();

            try
            {

                if (IsPrintCovers)
                {
                    // Page 1
                    Page1Content = HtmlHeader;
                    Page1Content += PagePrefix;
                    Page1Content += GetPage1Body(env);
                    Page1Content += PageSuffix;
                    Page1Content += GetFooterBar("footer-bar-pdf", "");
                    Page1Content += HtmlFooter;
                }

                //if (!IsNewLoan)
                {
                    // Page 2
                    Page2Content = HtmlHeader;
                    Page2Content += PagePrefix;
                    Page2Content += GetPage2Body(env);
                    Page2Content += PageSuffix;
                    Page2Content += GetFooterBar("footer-bar-pdf", "1");
                    Page2Content += HtmlFooter;
                }

                // Page 3
                Page3Content = HtmlHeader;
                Page3Content += PagePrefix;
                Page3Content += GetPage3Body(env);
                Page3Content += PageSuffix;
                Page3Content += GetFooterBar("footer-bar-pdf", "2");
                Page3Content += HtmlFooter;

                if (IsPrintCovers)
                {

                    // Page 4
                    Page4Content = HtmlHeader;
                    Page4Content += PagePrefix;
                    Page4Content += GetPage4Body(env);
                    Page4Content += PageSuffix;
                    Page4Content += GetFooterBar("footer-bar-pdf", "");
                    Page4Content += HtmlFooter;
                }
                // Create HTML paths

                var cultureName = CultureInformation.Name;

                cultureName = cultureName.Replace('-', '_');
                ResultReportData reportData = env.theLoan.resultReportData;
                // should locate the temporary files in a priviladge directory
                string outputDir = MiscUtilities.GetOutputDirectory2();
                
                for (int i = 1; i <= numOfPages; i++)
                {
                    htmlPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.html", reportData.ID, i, cultureName));
                    pdfPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.pdf", reportData.ID, i, cultureName));
                }

                //if (IsPrintCovers)
                // {
                //     for (int i = 1; i <= 4; i++)
                //    {
                //        htmlPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.html", reportData.ID, i, cultureName));
                //        pdfPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.pdf", reportData.ID, i, cultureName));
                //    }
                //}
                //else
                //{
                //    for (int i = 1; i <= 2; i++)
                //    {
                //        htmlPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.html", reportData.ID, i, cultureName));
                //        pdfPaths.Add(String.Format(outputDir + "l{0}_{1}_{2}.pdf", reportData.ID, i, cultureName));
                //    }
                //}

                // Create HTML pages

                //if (IsNewLoan) {
                //    System.IO.File.WriteAllText(htmlPaths[0], Page3Content);
                //}
                //else 
                if (IsPrintCovers)
                {
                    System.IO.File.WriteAllText(htmlPaths[0], Page1Content);
                    System.IO.File.WriteAllText(htmlPaths[1], Page2Content);
                    System.IO.File.WriteAllText(htmlPaths[2], Page3Content);
                    System.IO.File.WriteAllText(htmlPaths[3], Page4Content);
                }
                else
                {
                    System.IO.File.WriteAllText(htmlPaths[0], Page2Content);
                    System.IO.File.WriteAllText(htmlPaths[1], Page3Content);
                }


                // Generate PDF pages

                for (int i = 0; i < htmlPaths.Count; i++)
                {
                    CreatePDF(htmlPaths[i], pdfPaths[i]);
                }

                // Merge PDF pages to a single file - http://www.pdfsharp.net/Features.ashx

                string[] files = pdfPaths.ToArray();
                PdfDocument outputDocument = new PdfDocument();

                foreach (string file in files)
                {
                    // Open the document to import pages from it.
                    PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                    // Iterate pages
                    int count = inputDocument.PageCount;
                    for (int idx = 0; idx < count; idx++)
                    {
                        // Get the page from the external document...
                        PdfPage page = inputDocument.Pages[idx];
                        // ...and add it to the output document.
                        outputDocument.AddPage(page);
                    }
                }

                // Output the complete report

                outputDocument.Save(filename);

                // clean up temp files
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GenerateLenderPdfReport: Exception: " + ex.ToString());
            }

            for (int i = 0; i < htmlPaths.Count; i++)
            {
                File.Delete(htmlPaths[i]);
                File.Delete(pdfPaths[i]);
            }

            return 0;
        }


        // Generate PDF pages - wkhtmltopdf documentation - https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
        public static void CreatePDF(String htmlPath, String pdfPath)
        {
            try
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                string fn = dir + "wkhtmltopdf.exe";
                if (!File.Exists(fn))
                {
                    fn = MiscUtilities.GetBinDirectory() + "wkhtmltopdf.exe"; 
                }
                if (!File.Exists(fn))
                {
                    Console.WriteLine("CreatePDF::ex: can't find the exe file: " + fn);
                }

                // String cmdPrefix = "--margin-bottom 0 --margin-left 0 --margin-right 0 --margin-top 0 --zoom 0.66 --viewport-size 3840x2160";
                //String cmdPrefix = "--margin-bottom 0 --margin-left 0 --margin-right 0 --margin-top 0 --zoom 0.66 --viewport-size 1920x1080";

                //String cmdPrefix = "--orientation Landscape --no-pdf-compression --dpi 160 --image-dpi 160 --image-quality 100 --disable-smart-shrinking --margin-bottom 0 --margin-left 0 --margin-right 0 --margin-top 0 --zoom 0.66 --viewport-size 3840x2160";
                String cmdPrefix = "--orientation Landscape --dpi 800 --image-dpi 6000 --image-quality 100 --disable-smart-shrinking --margin-bottom 0mm --margin-left 0mm --margin-right 0mm --margin-top 0mm --zoom 0.7 --viewport-size 3840x2160";


                String cmd = String.Format("{0} {1} {2}", cmdPrefix, htmlPath, pdfPath);
                //Console.WriteLine("CreatePDF running cmd: " + cmd + ", FileName: " + fn);
                Process process = new Process();

                process.StartInfo.FileName = fn; /*"wkhtmltopdf.exe";*/
                process.StartInfo.Arguments = cmd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreatePDF::ex: " + ex.ToString());
            }
        }


        public String GetPage1Body(RunEnvironment env)
        {
            string title = null;
            ResultReportData reportData = env.theLoan.resultReportData;

            if (CultureInformation.Name.Equals("he-IL"))
            {
                title = "דו\"ח אנליזה של וויזור";
                title = String.Format("{0} ניתוח להלוואה {1}", "" /*reportData.BankName*/, reportData.ID);
            }
            else if (CultureInformation.Name.Equals("en-US"))
            {
                title = String.Format("{0} - Wisor Analysis Report - {1}", reportData.BankName, reportData.ID);
            }
            else
            {
                title = String.Format("{0} - Wisor Analysis Report - {1}", reportData.BankName, reportData.ID);
            }

            String retStr = MiscConstants.UNDEFINED_STRING;
            try
            {

#if WDEBUG
                retStr = String.Format(LenderPages[0], title, BankLogoPath, "אפריל 2017");
#else
                string formatedDate;
                MiscUtilities.ConvertDate2(reportData.DateTaken.ToShortDateString(), out formatedDate);
                retStr = String.Format(LenderPages[0], title, BankLogoPath, formatedDate);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: GetPage1Body Exception: " + ex.ToString());
            }
            return retStr;
        }

        public String GetPage2Body(RunEnvironment env)
        {
            String page = null;
            ResultReportData reportData = env.theLoan.resultReportData;

            try
            {
                // get the products
                string[] products = reportData.GetProducts();

                GenericProduct gp;

                int i = 0;
                int profile = 1, minRateIndex = 0, maxRateIndex = MiscConstants.NumberOfYearsFrProduct;
                string productName;

                String productsUsedInAnalysisRows = "";

                foreach (string p in products)
                {
                    i++;

                    gp = GenericProduct.GetProductByName(p);
                    if (null == gp)
                    {
                        gp = GenericProduct.GetProductFromAllListByName(p);
                    }
                    if (null == gp)
                    {
                        WindowsUtilities.loggerMethod("ERROR: GetPage2Body unrecognized product: " + p);
                        continue;
                    }
                    double bankRateFrom = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, minRateIndex);
                    double bankRateTo = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, maxRateIndex);
                    double borrowerRateFrom = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, minRateIndex);
                    double borrowerRateTo = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, maxRateIndex);


#if WDEBUG
                    productName = "מוצר לדוגמא";
#else
                    // get the name of the product
                    if (CultureInformation.Name.Equals("he-IL"))
                    {
                        if (null == gp.hebrewName)
                            gp = GenericProduct.GetProductFromAllListByName(p);
                        productName = (null != gp.hebrewName) ? gp.hebrewName : p;
                    }
                    else
                    {
                        if (null != gp.name)
                            productName = gp.name;
                        else 
                            productName = p;
                    }
#endif
                    productsUsedInAnalysisRows += String.Format("<tr dir='ltr'>\r\n"
                                                                 + "    <td>{0}</td>\r\n"
                                                                 + "    <td>{1}</td>\r\n"
                                                                 + "    <td>{2}% - {3}%</td>\r\n"
                                                                 + "    <td>{4}% - {5}%</td>\r\n"
                                                                 + "</tr>\r\n"
                                                                 , i, productName, borrowerRateFrom, borrowerRateTo, bankRateFrom, bankRateTo);

                }


                uint totalOriginalLoanAmount = 0;
                uint totalDesiredMonthlyPayment = 0;
                uint totalPayUntilToday = 0;
                uint totalPayFuture = 0;
                uint totalEstimateFuturePay = 0;
                double totalEstimateProfitPercantageSoFar = 0;
                int totalEstimateProfitSoFar = 0;
                double totalEstimateFutureProfitPercantage = 0;
                int totalEstimateFutureProfit = 0;
                uint totalOriginalLoanAmountN = 0, totalPayFutureN = 0;
                double percantageProfitTillNow = MiscConstants.UNDEFINED_DOUBLE;
                double percantageFutureProfit = MiscConstants.UNDEFINED_DOUBLE;


                //if (CultureInformation.Name.Equals("he-IL"))
                //{
                    string compositionSummaryRows = null;

                    // get the entire original loan' detail

#if WDEBUG
                    for (int j = 0; j < 3; j++)
#else
                    LoanList ll = env.theLoan.OriginalLoanDetaild;

                if (null != ll)
                {
                    // accumulate the entire loan amount and the entire remainig amount
                    foreach (loanDetails ld in ll)
                    {
                        totalOriginalLoanAmountN += ld.OriginalLoanAmount;
                        totalPayFutureN += ld.resultReportData.RemaingLoanAmount;
                    }


                    foreach (loanDetails ld in ll)
#endif
                    {
#if WDEBUG
                        String prodName = "מוצר א";
                        string rsamudStr = "yes";
#else
                        String prodName = MiscConstants.UNDEFINED_STRING;
                        if (null != ld && null != ld.ProductID)
                            prodName = ld.ProductID.stringTypeId;
                        string rsamudStr = MiscUtilities.IsProductTsamud(ld.indicesFirstTimePeriod) ? "yes" : "no";
                        gp = GenericProduct.GetProductByName(prodName);
                        if (null == gp)
                            gp = GenericProduct.GetProductFromAllListByName(prodName);
#endif

                        if (CultureInformation.Name.Equals("he-IL"))
                        {

                            if (null != gp && null != gp.hebrewName)
                                prodName = gp.hebrewName;
                            else
                            {
#if WDEBUG
                                prodName = "מוצר";
#else
                                gp = GenericProduct.GetProductFromAllListByName(prodName);
                                if (null != gp && null != gp.hebrewName)
                                    prodName = gp.hebrewName;
#endif
                            }

#if WDEBUG
                            rsamudStr = "כן";
#else
                            rsamudStr = MiscUtilities.IsProductTsamud(ld.indicesFirstTimePeriod) ? "כן" : "לא";
#endif

                        }
                        else
                        {
                            prodName = gp.name;
                        }

#if WDEBUG
                        percantageProfitTillNow = 0.0f;
                        percantageFutureProfit = 0.0f;
#else
                        percantageProfitTillNow = (double)ld.resultReportData.EstimateProfitSoFar / totalOriginalLoanAmountN;
                        percantageFutureProfit = (double)ld.resultReportData.EstimateFutureProfit / totalPayFutureN;
#endif

                        compositionSummaryRows += String.Format("<tr>\r\n"
                                                                 + "<td>{0}</td>\r\n"
                                                                 + "<td >{1}</td>\r\n"
                                                                 + "<td >{2}</td>\r\n"
                                                                 + "<td >{3}</td>\r\n"
                                                                 + "<td>{4}</td>\r\n"
                                                                 + "<td>{5}</td>\r\n"
                                                                 + "<td>{6}</td>\r\n"
                                                                 + "<td>{7}</td>\r\n"
                                                                 + "<td>{8}</td>\r\n"
                                                                 + "<td style='position: relative;  border: 0px solid white;'>&nbsp;</td>\r\n"
                                                                 + "<td>{9}</td>\r\n"
                                                                 + "<td>{10}</td>\r\n"
                                                                 + "<td>{11}</td>\r\n"
                                                                 + "<td>{12}</td>\r\n"
                                                                 + "</tr>\r\n\r\n"

#if WDEBUG
                                                                 , CurrencySymbol + "0"
                                                                 , "1"
                                                                 , "2"
                                                                 , "3"
                                                                 , "4"
                                                                 , CurrencySymbol + "100"
                                                                 , CurrencySymbol + "6"
                                                                 , CurrencySymbol + "7"
                                                                 , CurrencySymbol + "8"
                                                                 , CurrencySymbol + "9"
                                                                 , "10"
                                                                 , CurrencySymbol + "11"
                                                                 , "12"
                                                                 );
#else
                                                                , CurrencySymbol + ld.OriginalLoanAmount.ToString("N0")
                                                                , prodName
                                                                , ld.OriginalTime
                                                                , Math.Round(ld.OriginalRate * 100, 3) + "%"
                                                                , rsamudStr
                                                                , CurrencySymbol + ld.DesiredMonthlyPayment.ToString("N0")
                                                                , CurrencySymbol + ld.resultReportData.PayUntilToday.ToString("N0")
                                                                , CurrencySymbol + ld.resultReportData.RemaingLoanAmount.ToString("N0")
                                                                , CurrencySymbol + ld.resultReportData.EstimateFuturePay.ToString("N0")

                                                                , CurrencySymbol + ld.resultReportData.EstimateProfitSoFar.ToString("N0")
                                                                , Math.Round(percantageProfitTillNow * 100, 3) + "%"
                                                                // , Math.Round(ld.resultReportData.EstimateProfitPercantageSoFar * 100, 3) + "%"
                                                                , CurrencySymbol + ld.resultReportData.EstimateFutureProfit.ToString("N0")
                                                                , Math.Round(percantageFutureProfit * 100, 3) + "%"
                                                                  // , Math.Round(ld.resultReportData.EstimateFutureProfitPercantage * 100, 3) + "%"
                                                                  );

                        totalOriginalLoanAmount += ld.OriginalLoanAmount;
                        totalDesiredMonthlyPayment += ld.DesiredMonthlyPayment;
                        totalPayUntilToday += ld.resultReportData.PayUntilToday;
                        totalPayFuture += ld.resultReportData.RemaingLoanAmount;
                        totalEstimateFuturePay += ld.resultReportData.EstimateFuturePay;

                        totalEstimateProfitPercantageSoFar += percantageProfitTillNow; // ld.resultReportData.EstimateProfitPercantageSoFar;
                        totalEstimateProfitSoFar += ld.resultReportData.EstimateProfitSoFar;
                        totalEstimateFutureProfitPercantage += percantageFutureProfit; // ld.resultReportData.EstimateFutureProfitPercantage;
                        totalEstimateFutureProfit += ld.resultReportData.EstimateFutureProfit;
#endif
                    }
                }
                    // Add summary row

                    compositionSummaryRows += String.Format("<tr style='font-weight: bold;'>\r\n"
                                         + "<td>{0}</td>\r\n"
                                         + "<td style='position: relative;  border: 0px solid white;'>{1}</td>\r\n"
                                         + "<td style='position: relative;  border: 0px solid white;'>{2}</td>\r\n"
                                         + "<td style='position: relative;  border: 0px solid white;'>{3}</td>\r\n"
                                         + "<td style='position: relative;  border: 0px solid white;'>{4}</td>\r\n"
                                         + "<td>{5}</td>\r\n"
                                         + "<td>{6}</td>\r\n"
                                         + "<td>{7}</td>\r\n"
                                         + "<td>{8}</td>\r\n"
                                         + "<td style='position: relative;  border: 0px solid white;'>&nbsp;</td>\r\n"
                                         + "<td>{9}</td>\r\n"
                                         + "<td>{10}</td>\r\n"
                                         + "<td>{11}</td>\r\n"
                                         + "<td>{12}</td>\r\n"
                                         + "</tr>\r\n\r\n"

                                         , CurrencySymbol + totalOriginalLoanAmount.ToString("N0")
                                         , "&nbsp;"
                                         , "&nbsp;"
                                         , "&nbsp;"
                                         , "&nbsp;"
                                         , CurrencySymbol + totalDesiredMonthlyPayment.ToString("N0")
                                         , CurrencySymbol + totalPayUntilToday.ToString("N0")
                                         , CurrencySymbol + totalPayFuture.ToString("N0")
                                         , CurrencySymbol + totalEstimateFuturePay.ToString("N0")
                                         , CurrencySymbol + totalEstimateProfitSoFar.ToString("N0")
                                         , Math.Round(totalEstimateProfitPercantageSoFar * 100, 3) + "%"
                                         , CurrencySymbol + totalEstimateFutureProfit.ToString("N0")
                                         , Math.Round(totalEstimateFutureProfitPercantage * 100, 3) + "%"

                                         );

#if WDEBUG
                    string formatedDate = "אפריל 2017";
                    //MiscUtilities.ConvertDate2(reportData.OriginalDateTaken.ToShortDateString(), out formatedDate);
#else
                    string formatedDate;
                    MiscUtilities.ConvertDate2(reportData.OriginalDateTaken.ToShortDateString(), out formatedDate);
#endif


                    page = String.Format(LenderPages[1]
                                          , reportData.BankName
                                          , TopBarBankLogoPath
                                          , reportData.ID //CaseId

                                          , compositionSummaryRows

                                          // 
                                          , productsUsedInAnalysisRows

                                          // original date
                                          , formatedDate

                                          , CurrencySymbol + reportData.PropertyValue.ToString("N0")
                                          , CurrencySymbol + reportData.YearlyIncome.ToString("N0")
                                           );
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: GetPage2Body Exception: " + ex.ToString());
            }

            return page;
        }

        public String GetPage3Body(RunEnvironment env)
        {
            ResultReportData reportData = env.theLoan.resultReportData;
            List<String> CompositionTables = new List<String>();
            List<String> CompositionTableNames = new List<String>();
            List<String> CompositionSummaries = new List<String>();
            String page = null;
            string compositionModules = null;

            try
            {
                // get the compositions
                Composition[] compData = reportData.compositions;

                int ttlBankPay, ttlBorrowerPay, ttlProfit;
                int numOfPrintedComp = 0;

                for (int i = 0; i < compData.Length && numOfPrintedComp < MiscUtilities.GetNumberOfProductsInCombination(); i++)
                {
                    if (null == compData[i])
                        continue;

                    // ensure its a win-win composition
                    if (!compData[i].IsWinWin)
                        continue;

                    String CompositionDataRows = "";

                    // calculate the relations between tzamud and not in all the products which consist the composition
                    uint fix = MiscConstants.UNDEFINED_UINT, adjustable = MiscConstants.UNDEFINED_UINT;
                    uint tsamud = MiscConstants.UNDEFINED_UINT, notTsamud = MiscConstants.UNDEFINED_UINT;

                    MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

                    uint sumAmount = 0
                         , sumMonthly = 0
                         , sumTTLPay = 0
                         , sumTTllProfit = 0
                         ;

                    double sumLenderProfit = 0;

                    // get the bank profit from the composition fields
                    uint[] bankPay = new uint[] {
                        compData[i].optXBankTtlPay, compData[i].optYBankTtlPay, compData[i].optZBankTtlPay };

                    for (int j = 0; j < compData[i].opts.Length; j++)
                    {
#if WDEBUG
                        bool isProductFix = true;
                        bool isProductTsamud = true;

                        // accumulate the fix or adjustable
                        if (isProductFix)
                            fix += 100;
                        else
                            adjustable += 100;

                        // accululate Tsamud or not
                        if (isProductTsamud)
                            tsamud += 100;
                        else
                            notTsamud += 100;
#else
                        bool isProductFix = MiscUtilities.IsProductFix(compData[i].opts[j].product.fixOrAdjustable);
                        bool isProductTsamud = MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod);

                        // accumulate the fix or adjustable
                        if (isProductFix)
                            fix += (uint)compData[i].opts[j].optAmt;
                        else
                            adjustable += (uint)compData[i].opts[j].optAmt;

                        // accululate Tsamud or not
                        if (isProductTsamud)
                            tsamud += (uint)compData[i].opts[j].optAmt;
                        else
                            notTsamud += (uint)compData[i].opts[j].optAmt;

#endif

#if WDEBUG
                        String productName = "שם המוצר";
#else
                        String productName = compData[i].opts[j].product.productID.stringTypeId;
#endif
                        String Option = productName;
                        GenericProduct gp = GenericProduct.GetProductByName(productName);
                        if (CultureInformation.Name.Equals("he-IL"))
                        {
                            if (null != gp && null != gp.hebrewName)
                                Option = gp.hebrewName;
                        }
                        else
                        {
                            if (null != gp)
                                Option = gp.name;
                        }

#if WDEBUG
                        uint Amt = 100;
                        double Rate = 100.0f;
                        uint Time = 100;
                        uint PMT = 100;
                        uint TTLPay = 100;
                        uint TTllProfit = 100;
                        double LenderProfit = 100.0f;
#else
                        uint Amt = (uint)compData[i].opts[j].optAmt;
                        double Rate = compData[i].opts[j].optRateFirstPeriod;
                        uint Time = compData[i].opts[j].optTime;
                        uint PMT = (uint)compData[i].opts[j].optPmt;
                        uint TTLPay = (uint)compData[i].opts[j].optTtlPay;
                        uint TTllProfit = TTLPay - bankPay[j];
                        double LenderProfit = (double)TTllProfit / reportData.RemaingLoanAmount * 100;
#endif
                        sumAmount += Amt;
                        sumMonthly += PMT;
                        sumTTLPay += TTLPay;

                        sumTTllProfit += TTllProfit;
                        sumLenderProfit += LenderProfit;

#if WDEBUG
                        bool Indexation = true;

#else
                        bool Indexation = MiscUtilities.IsProductTsamud(compData[i].opts[j].product.originalIndexUsedFirstTimePeriod);
#endif

                        //if (CultureInformation.Name.Equals("he-IL"))
                        //{

                        String indexationStr = null;

                        if (CultureInformation.Name.Equals("he-IL"))
                        {
                            indexationStr = Indexation ? "כן" : "לא";
                        }
                        else
                        {
                            indexationStr = Indexation ? "Yes" : "No";
                        }


                        CompositionDataRows += String.Format("<tr>\r\n"
                                                              + "	<td>{9}{0}</td>\r\n"
                                                              + "	<td>{1}</td>\r\n"
                                                              + "	<td>{2}</td>\r\n"
                                                              + "	<td>{3}</td>\r\n"
                                                              + "	<td>{4}</td>\r\n"
                                                              + "	<td>{9}{5}</td>\r\n"
                                                              + "	<td>{9}{6}</td>\r\n"
                                                              + "	<td style='position: relative;  border: 0px solid white;'>&nbsp;</td>\r\n"
                                                              + "	<td>{9}{7}</td>\r\n"
                                                              + "	<td>{8}%</td>\r\n"
                                                              + "</tr>\r\n",
                                                              Amt.ToString("N0"), Option, Time, Math.Round(Rate * 100, 3) + "%", indexationStr,
                                                              PMT.ToString("N0"), TTLPay.ToString("N0"),
                                                              TTllProfit.ToString("N0"), Math.Round(LenderProfit, 2), CurrencySymbol);
                    }

                    // calculate the fix and adjustable numbers
                    uint entireFixSum = fix + adjustable;
                    uint fixNum = MiscConstants.UNDEFINED_UINT, adjustableNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireFixSum)
                    {
                        fixNum = Convert.ToUInt32((double)fix / entireFixSum * 100);
                        adjustableNum = 100 - fixNum;
                    }

                    // calculate the tsamud vs. not numbers
                    uint entireTsamudSum = tsamud + notTsamud;
                    uint tsamudNum = MiscConstants.UNDEFINED_UINT, notTsamudNum = MiscConstants.UNDEFINED_UINT;
                    if (0 < entireTsamudSum)
                    {
                        tsamudNum = Convert.ToUInt32((double)tsamud / entireTsamudSum * 100);
                        notTsamudNum = 100 - tsamudNum;
                    }
                    bool shouldAddTheNumbers = false;


                    // TBD - this should be managed for all languages properly
                    String FixHeader = MiscConstants.UNDEFINED_STRING,
                        IndexationHeader1 = MiscConstants.UNDEFINED_STRING, IndexationHeader2 = MiscConstants.UNDEFINED_STRING;

                    CalculateHeaders(adjustable, tsamudNum, out FixHeader, out IndexationHeader1, out IndexationHeader2, out shouldAddTheNumbers);


                    // display the data in the header
                    if (shouldAddTheNumbers)
                    {
                        // TBD - should it be done better by the culture 
                        CompositionTableNames.Add(String.Format(FixHeader + "," + IndexationHeader1 + " ( " + IndexationHeader2 + ")",
                            fixNum, adjustableNum));
                    }
                    else
                    {
                        CompositionTableNames.Add(String.Format(FixHeader + "," + IndexationHeader1));
                    }


                    // Summary tables
                    String summary = "";

                    //if (!IsNewLoan)
                    {
                        if (CultureInformation.Name.Equals("he-IL"))
                            summary = String.Format("<tr style='font-weight: bold;'>\r\n"
                                                        + "    <td style='width:260px;'>חיסכון פוטנציאלי ללווה</td>\r\n"
                                                        + "    <td style='width:85px;'>{4}{0}</td>\r\n"
                                                        + "    <td style='width:85px;'>{1}%</td>\r\n"
                                                        + "</tr>\r\n"
                                                        + "<tr style='font-weight: bold; border-top: 1px solid black;'>\r\n"
                                                        + "    <td style='width:260px;'>שיפור רווח פוטנציאלי</td>\r\n"
                                                        + "    <td style='width:85px;'>{4}{2}</td>\r\n"
                                                        + "    <td style='width:85px;'>{3}%</td>\r\n"
                                                        + "</tr>\r\n",
                                                        compData[i].BorrowerProfitCalc.ToString("N0"),
                                                        Math.Round((double)compData[i].BorrowerProfitCalc / reportData.RemaingLoanAmount  /*reportData.PayFuture*/ /*LoanAmount*/ * 100, 2),
                                                        compData[i].BankProfitCalc.ToString("N0"),
                                                        Math.Round((double)compData[i].BankProfitCalc / reportData.EstimateFutureProfit * 100, 2), CurrencySymbol);
                        else
                            summary = String.Format("<tr style='font-weight: bold;'>\r\n"
                                                 + "    <td style='width:260px;'>Borrower can save</td>\r\n"
                                                 + "    <td style='width:85px;'>{4}{0}</td>\r\n"
                                                 + "    <td style='width:85px;'>{1}%</td>\r\n"
                                                 + "</tr>\r\n"
                                                 + "<tr style='font-weight: bold; border-top: 1px solid black;'>\r\n"
                                                 + "    <td style='width:260px;'>Lender can increase profit by</td>\r\n"
                                                 + "    <td style='width:85px;'>{4}{2}</td>\r\n"
                                                 + "    <td style='width:85px;'>{3}%</td>\r\n"
                                                 + "</tr>\r\n",
                                                 compData[i].BorrowerProfitCalc.ToString("N0"),
                                                 Math.Round((double)compData[i].BorrowerProfitCalc / reportData.RemaingLoanAmount * 100, 2),
                                                 compData[i].BankProfitCalc.ToString("N0"),
                                                 Math.Round((double)compData[i].BankProfitCalc / reportData.EstimateFutureProfit * 100, 2), CurrencySymbol);

                       
                    }
                    CompositionSummaries.Add(summary);

                    /* Bold summary rows (at the bottom) */

                    CompositionDataRows += String.Format("<tr style='font-weight: bold;'>\r\n"
                                      + "	<td style='weight: bold;'>{6}{0}</td>\r\n"
                                      + "	<td style='position: relative;  border: 0px solid white; border-left: 0px solid white; weight: bold;'><span class='hide-in-desktop-view'>{1}&nbsp;</span></td>\r\n"
                                      + "	<td style='position: relative;  border: 0px solid white; border-left: 0px solid white; weight: bold;'>&nbsp;</td>\r\n"
                                      + "	<td style='position: relative;  border: 0px solid white; border-left: 0px solid white; weight: bold;'>&nbsp;</td>\r\n"
                                      + "	<td style='position: relative;  border: 0px solid white; border-left: 0px solid white; weight: bold;'>&nbsp;</td>\r\n"
                                      + "	<td style='weight: bold;'>{6}{2}</td>\r\n"
                                      + "	<td style='weight: bold;'>{6}{3}</td>\r\n"
                                      + "	<td style='position: relative;  border: 0px solid white;'>&nbsp;</td>\r\n"
                                      + "	<td style='weight: bold;'>{6}{4}</td>\r\n"
                                      + "	<td style='weight: bold;'>{5}%</td>\r\n"
                                      + "</tr>\r\n",
                                      sumAmount.ToString("N0"), SummaryStr,
                                      sumMonthly.ToString("N0"), sumTTLPay.ToString("N0"),
                                      sumTTllProfit.ToString("N0"), Math.Round(sumLenderProfit, 2), CurrencySymbol
                                      );
                    CompositionTables.Add(CompositionDataRows);

                    numOfPrintedComp++;
                }


                for (int i = 0; i < CompositionTables.Count; i++)
                {
                    compositionModules += String.Format(CompositionTemplate, CompositionTableNames[i], CompositionTables[i], CompositionSummaries[i]);
                }

                page = String.Format(LenderPages[2]
                                      , reportData.BankName
                                      , TopBarBankLogoPath
                                      , reportData.ID // CaseId
                                      , compositionModules
                                      );

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: GetPage3Body Exception: " + ex.ToString());
            }

            //String page = LenderPages[2];

            return page;
        }

        public String GetPage4Body(RunEnvironment env)
        {
            ResultReportData reportData = env.theLoan.resultReportData;
            string title = null;
            String page = null;

            try
            {
                if (CultureInformation.Name.Equals("he-IL"))
                {
                    title = "תודה רבה";
                }
                else if (CultureInformation.Name.Equals("en-US"))
                {
                    title = "Thank you";
                }
                else
                {
                    title = "Thank you";
                }

#if WDEBUG
                string formatedDate = "אפריל 2017";
#else
                string formatedDate;
                MiscUtilities.ConvertDate2(reportData.OriginalDateTaken.ToShortDateString(), out formatedDate);
#endif



                page = String.Format(LenderPages[3], title, BankLogoPath, formatedDate /* reportData.DateTaken */);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: GetPage4Body Exception: " + ex.ToString());
            }
            return page;
        }

        public String GetHTMLHeader()
        {
            return String.Format(HtmlHeader);
        }

        public String GetFooterBar(String className, String pageId = "")
        {
            string wisorCompanyName = null;

            if (CultureInformation.Name.Equals("he-IL"))
            {
                wisorCompanyName = "וויזור טכנולוגיות בעמ";
            }
            else if (CultureInformation.Name.Equals("en-US"))
            {
                wisorCompanyName = "Wisor";
            }
            else
            {
                wisorCompanyName = "Wisor";
            }


            String footerBar = null;

            if (CultureInformation.TextInfo.IsRightToLeft)
            {
                footerBar = "\r\n"
                            + "<div class='{0}'>"
                            + "     <div class='footer-right-text' dir='rtl'>{1} &copy; 2017</div>"
                            + "     <div class='footer-page-number'>{2}</div>"
                            + "</div>\r\n";
            }
            else
            {
                footerBar = "\r\n"
                            + "<div class='{0}'>"
                            + "     <div class='footer-left-text'>{1} &copy; 2017</div>"
                            + "     <div class='footer-page-number'>{2}</div>"
                            + "</div>\r\n";
            }

            return String.Format(footerBar, className, wisorCompanyName, pageId);
        }

        public void SetBankLogoPath(string bankLogoPath)
        {
            BankLogoPath = bankLogoPath;
        }

        public void SetTopBarBankLogoPath(string topBarBankLogoPath)
        {
            TopBarBankLogoPath = topBarBankLogoPath;
        }

        void CalculateHeaders(uint adjustable, uint tsamudNum, out string FixHeader,
             out string IndexationHeader1, out string IndexationHeader2, out bool shouldAddTheNumbers)
        {
            FixHeader = IndexationHeader1 = IndexationHeader2 = MiscConstants.UNDEFINED_STRING;
            shouldAddTheNumbers = false;

            // TBD - Eliad please add the English version

            /*
            אם תמהיל משלב ריבית משתנה – לא פריים – אז חלק ראשון של הכותרת תמיד יהיה: "תמהיל המאפשר יותר שינויים"
            אם תמהיל משלב רק פריים וריבית קבועה – אז חלק ראשון של הכותרת תמיד יהיה: "תמהיל המאפשר יותר יציבות" 
            */
            if (0 < adjustable)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    FixHeader = "תמהיל המאפשר יותר שינויים";
                else
                    FixHeader = "Elastic composition";
            }
            else
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    FixHeader = "תמהיל המאפשר יותר יציבות";
                else
                    FixHeader = "Stable composition";
            }

            /*
            אם 0% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: "כאשר כל הכסף לא צמוד למדד"
            אם עד 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר חלק מהכסף צמוד למדד"
            אם בדיוק 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר חצי מהכסף צמוד למדד"
            אם מעל 50% מהכסף במוצרים צמודים למדד – אז החלק השני של הכותרת תמיד יהיה: כאשר רוב הכסף צמוד למדד"
            אם 100% מהכסף במוצרים צמודים למדד (כנראה לא יקרה כי בינתיים יש תמיד מוצר פריים בתמהיל) – אז החלק השני של הכותרת תמיד יהיה: כאשר כל הכסף צמוד למדד"
            */
            if (CultureInformation.Name.Equals("he-IL"))
                IndexationHeader2 = "{0}% בריבית קבועה, {1}% בריבית משתנה";
            else
                IndexationHeader2 = "Fix interest {0}%, Adjustable interest {1}%";

            if (MiscConstants.UNDEFINED_UINT == tsamudNum)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    IndexationHeader1 = "כאשר כל הכסף לא צמוד למדד";
                else
                    IndexationHeader1 = "When all the money is not indexed";
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 > tsamudNum)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    IndexationHeader1 = "כאשר חלק הכסף צמוד למדד";
                else
                    IndexationHeader1 = "When part of the money is indexed";
                shouldAddTheNumbers = true;
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 == tsamudNum)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    IndexationHeader1 = "כאשר חצי מהכסף צמוד למדד";
                else
                    IndexationHeader1 = "When half of the money is indexed";
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 100 == tsamudNum)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    IndexationHeader1 = "כאשר כל הכסף צמוד למדד";
                else
                    IndexationHeader1 = "When all the money is indexed";
            }
            else if (MiscConstants.UNDEFINED_UINT < tsamudNum && 50 < tsamudNum)
            {
                if (CultureInformation.Name.Equals("he-IL"))
                    IndexationHeader1 = "כאשר רוב הכסף צמוד למדד";
                else
                    IndexationHeader1 = "When most of the money is indexed";
                shouldAddTheNumbers = true;
            }
        }

        void SetParams(RunEnvironment env)
        {             // if the loan is new, always mark it as should refinance
            int numOfMonths = MiscUtilities.CalculateMonthBetweenDates(env.theLoan.DateTaken, DateTime.Now);
            if (numOfMonths <= 0)
                IsNewLoan = true;

            //if (IsNewLoan)
            //    numOfPages = 1;
            //else 
            if (IsPrintCovers)
                numOfPages = 4;
            else
                numOfPages = 2;

        }

    }
}