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

namespace WisorLibrary.Reporting
{
    class LenderReport
    {
        private static bool ISFirstLenderReportInit = true;

        private static String HtmlHeader;
        private static String HtmlFooter;
        private static List<String> LenderPages;

        private static String PagePrefix;
        private static String PageSuffix;
        private static String BankLogoPath;


        public LenderReport()
        {
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

                //HtmlHeader = (new StreamReader(assembly.GetManifestResourceStream("Wisor_Lender_Report_Generator.lender_template_pages.template_html_header.html"))).ReadToEnd();
                //HtmlFooter = (new StreamReader(assembly.GetManifestResourceStream("Wisor_Lender_Report_Generator.lender_template_pages.template_html_footer.html"))).ReadToEnd();

                //for (int i = 1; i <= 4; i++)
                //{
                //    LenderPages.Add((new StreamReader(assembly.GetManifestResourceStream(String.Format("Wisor_Lender_Report_Generator.lender_template_pages.template_page{0}.html", i))).ReadToEnd()));
                //}

                string fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_header.html");
                HtmlHeader = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                fullResourceName = MiscUtilities.GetResourceStream("lender_template_html_footer.html");
                HtmlFooter = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();

                for (int i = 1; i <= 4; i++)
                {
                    fullResourceName = MiscUtilities.GetResourceStream(String.Format("lender_template_page{0}.html", i));
                    LenderPages.Add((new StreamReader(assembly.GetManifestResourceStream(fullResourceName)).ReadToEnd()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LenderReport::OnInit ex: " + ex.ToString());
            }



            BankLogoPath = "images/bank_leumi_logo.png";
            PagePrefix = "      <div class='a4'>";
            PageSuffix = "      </div>";
        }

        public int GenerateLenderHtmlReport(string filename, ResultReportData reportData)
        {
            try
            {
                if (null != reportData)
                {
                    // HTML Header
                    var reportContent = new StringBuilder(GetHTMLHeader());

                    // Page 1
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage1Body(reportData));
                    reportContent.Append(GetFooterBar("footer-bar-html", ""));
                    reportContent.Append(PageSuffix);

                    // Page 2
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage2Body(reportData));
                    reportContent.Append(GetFooterBar("footer-bar-html", "1"));
                    reportContent.Append(PageSuffix);

                    // Page 3
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage3Body(reportData));
                    reportContent.Append(GetFooterBar("footer-bar-html", "2"));
                    reportContent.Append(PageSuffix);

                    // Page 4
                    reportContent.Append(PagePrefix);
                    reportContent.Append(GetPage4Body(reportData));
                    reportContent.Append(GetFooterBar("footer-bar-html", ""));
                    reportContent.Append(PageSuffix);

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

        public int GenerateLenderPdfReport(string filename, ResultReportData reportData)
        {
            // Generate individual HTML pages

            // Page 1
            String Page1Content = HtmlHeader;
            Page1Content += PagePrefix;
            Page1Content += GetPage1Body(reportData);
            Page1Content += PageSuffix;
            Page1Content += GetFooterBar("footer-bar-pdf", "");
            Page1Content += HtmlFooter;

            // Page 2
            String Page2Content = HtmlHeader;
            Page2Content += PagePrefix;
            Page2Content += GetPage2Body(reportData);
            Page2Content += PageSuffix;
            Page2Content += GetFooterBar("footer-bar-pdf", "1");
            Page2Content += HtmlFooter;

            // Page 3
            String Page3Content = HtmlHeader;
            Page3Content += PagePrefix;
            Page3Content += GetPage3Body(reportData);
            Page3Content += PageSuffix;
            Page3Content += GetFooterBar("footer-bar-pdf", "2");
            Page3Content += HtmlFooter;

            // Page 4
            String Page4Content = HtmlHeader;
            Page4Content += PagePrefix;
            Page4Content += GetPage4Body(reportData);
            Page4Content += PageSuffix;
            Page4Content += GetFooterBar("footer-bar-pdf", "");
            Page4Content += HtmlFooter;

            // Create HTML paths

            var htmlPaths = new List<String>();

            for (int i = 1; i <= 4; i++)
            {
                htmlPaths.Add(String.Format("l{0}_{1}.html", reportData.ID, i));
            }

            // Create HTML pages

            System.IO.File.WriteAllText(htmlPaths[0], Page1Content);
            System.IO.File.WriteAllText(htmlPaths[1], Page2Content);
            System.IO.File.WriteAllText(htmlPaths[2], Page3Content);
            System.IO.File.WriteAllText(htmlPaths[3], Page4Content);

            // Create PDF paths

            var pdfPaths = new List<String>();

            pdfPaths.Add(String.Format("l{0}_1.pdf", reportData.ID));
            pdfPaths.Add(String.Format("l{0}_2.pdf", reportData.ID));
            pdfPaths.Add(String.Format("l{0}_3.pdf", reportData.ID));
            pdfPaths.Add(String.Format("l{0}_4.pdf", reportData.ID));

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
                String cmdPrefix = "--margin-bottom 0 --margin-left 0 --margin-right 0 --margin-top 0 --zoom 0.66 --viewport-size 3840x2160";
                String cmd = String.Format("{0} {1} {2}", cmdPrefix, htmlPath, pdfPath);
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


        public String GetPage1Body(ResultReportData reportData)
        {
            return String.Format(LenderPages[0], reportData.BankName, reportData.ID, BankLogoPath, reportData.DateTaken);
        }

        public String GetPage2Body(ResultReportData reportData)
        {
           
            String CaseId = "0001";

            // get the products
            string[] products = reportData.GetProducts();

            GenericProduct gp;

            int profile = 1, index = 0, i = 0;

            String productsUsedInAnalysisRows = "";

            foreach (string p in products)
            {
                i++;

                // TBD - Omri. which rate and margin should be get
                gp = GenericProduct.GetProductByName(p);
                double bankRate = RateUtilities.Instance.GetBankRate(gp.productID.numberID, profile, index);
                double borrowerRate = RateUtilities.Instance.GetBorrowerRate(gp.productID.numberID, profile, index);

                productsUsedInAnalysisRows += String.Format("<tr>\r\n"
                                                             + "<td style='border: 1px solid black;'>{0}</td>\r\n"
                                                             + "<td style='border: 1px solid black;'>{1}</td>\r\n"
                                                             + "<td style='border: 1px solid black;'>{2}</td>\r\n"
                                                             + "<td style='border: 1px solid black;'>{3}</td>\r\n"
                                                             + "</tr>\r\n"
                                                             , i, p, borrowerRate, bankRate);

            }

            String page =  String.Format( LenderPages[1]
                                  , reportData.BankName
                                  , CaseId
                                  , reportData.ID

                                  // Left table

                                  , reportData.DateTaken                       // Date taken
                                  , reportData.OriginalLoanAmount              // Amount taken
                                  , reportData.ProductName                     // Product                                                                        
                                  , reportData.OriginalTime                    // Term                                                              
                                  , reportData.OriginalRate                    // Rate
                                  , reportData.FirstMonthlyPMT                 // First monthly payment
                                  , reportData.YearlyIncome                    // Total income
                                  , reportData.EstimateFutureProfitPercantage                     // Debt-to-incme
                                  , reportData.PayUntilToday                   // Paid until today
                                  , reportData.PayFuture                        // Left to pay
                                  , reportData.EstimateFuturePay               // Estimated future payment
                                  
                                  , reportData.OriginalMargin                  // Product margin                                                              
                                  , reportData.EstimateProfitPercantageSoFar   // Estimated % profits so far
                                  , reportData.EstimateProfitSoFar             // Estimated profit so far
                                  , reportData.EstimateTotalProfitPercantage   // Estimated total % profit
                                  , reportData.EstimateTotalProfit             // Estimated total profit
                                  , reportData.EstimateFutureProfitPercantage  // Estimated future % profit
                                  , reportData.EstimateFutureProfit            // Estimated future profit

                                  // Right table

                                  , productsUsedInAnalysisRows
                                  );

            

            //String page = LenderPages[1];
            return page;
        }

        public String GetPage3Body(ResultReportData reportData)
        {
            
            List<String> CompositionTables = new List<String>();
            List<String> profitTables = new List<String>();
            String page = null;

            try
            {

                // get the 3 compositions
                //CompositionReportData[] compData = reportData.GetCompositionData();
                Composition[] compData = reportData.compositions;


                for (int i = 0; i < compData.Length; i++)
                {
                    String CompositionDataRows = "", ProfitDataRows = "";
                    int ttlBankPay, ttlBorrowerPay, ttlProfit;
                    MiscUtilities.CalcaulateProfit(compData[i], out ttlBankPay, out ttlBorrowerPay, out ttlProfit);

	                uint sumTTllProfit = 0;
                    double sumAmount = 0, sumMonthly = 0, sumTTLPay = 0, sumLenderProfit = 0;
                    String TTllProfit = "", LenderProfit = "";

                    for (int j = 0; j < compData[i].opts.Length; j++)
                    {
                        String Option = compData[i].opts[j].product.name;
                        String Amt = compData[i].opts[j].optAmt.ToString();
                        String Rate = compData[i].opts[j].optRateFirstPeriod.ToString();
                        String Time = compData[i].opts[j].optTime.ToString();
                        String PMT = compData[i].opts[j].optPmt.ToString();
                        String TTLPay = compData[i].opts[j].optTtlPay.ToString();
                        TTllProfit = ttlProfit.ToString();
                        LenderProfit = (ttlProfit / reportData.RemaingLoanAmount).ToString();
 
						sumAmount += compData[i].opts[j].optAmt;
						sumMonthly += compData[i].opts[j].optPmt;
						sumTTLPay += compData[i].opts[j].optTtlPay;
						
						sumTTllProfit += (uint) ttlProfit;
						sumLenderProfit += ttlProfit / reportData.RemaingLoanAmount;
						
                        CompositionDataRows += String.Format( "<tr style='border: 1px solid black;'>\r\n"
                                                              + "	<td style='border: 1px solid black; text-align:left;'>{0}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{1}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{2}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{3}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{4}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{5}</td>\r\n"
                                                              + "</tr>\r\n", 
                                                              Option, Amt, Rate, Time, PMT, TTLPay
                                                              );

                        ProfitDataRows += String.Format("<tr style='border: 1px solid black;'>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{0}</td>\r\n"
                                                              + "	<td style='border: 1px solid black;'>{1}</td>\r\n"
                                                              + "</tr>\r\n",
                                                              TTllProfit, LenderProfit
                                                              );


                    }

					/* Bold summary rows (at the bottom) */
					CompositionDataRows += String.Format( "<tr style='border: 1px solid black;'>\r\n"
                                      + "	<td style='border: 1px solid black; text-align:left;'>{0}</td>\r\n"
                                      + "	<td style='border: 1px solid black; weight: bold;'>{1}</td>\r\n"
                                      + "	<td style='border: 1px solid black; weight: bold;'>{2}</td>\r\n"
                                      + "	<td style='border: 1px solid black; weight: bold;'>{3}</td>\r\n"
                                      + "	<td style='border: 1px solid black; weight: bold;'>{4}</td>\r\n"
                                      + "	<td style='border: 1px solid black; weight: bold;'>{5}</td>\r\n"
                                      + "</tr>\r\n", 
                                      "", sumAmount, "", "", sumMonthly, sumTTLPay
                                      );

                    ProfitDataRows += String.Format("<tr style='border: 1px solid black;'>\r\n"
                                                          + "	<td style='border: 1px solid black; weight: bold;'>{0}</td>\r\n"
                                                          + "	<td style='border: 1px solid black; weight: bold;'>{1}</td>\r\n"
                                                          + "</tr>\r\n",
                                                          TTllProfit, LenderProfit
                                                          );

					
                    CompositionTables.Add(CompositionDataRows);
					profitTables.Add(ProfitDataRows);
                }


                page = String.Format( LenderPages[2]
                                      , reportData.BankName
                                      , reportData.ID // CaseId
                                      , reportData.ID
									  
                                      , CompositionTables[0]
                                      , profitTables[0]
									  , ""
									  , ""
									  , ""
									  , ""
									  
                                      , CompositionTables[1]
                                      , profitTables[1]
									  , ""
									  , ""
									  , ""
									  , ""
									  
                                      , CompositionTables[2]
                                      , profitTables[2]
									  , ""
									  , ""
									  , ""
									  , ""
                                      );
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: GetPage3Body Exception: " + ex.ToString());
            }
            

            //String page = LenderPages[2];

            return page;
        }

        public String GetPage4Body(ResultReportData reportData)
        {
            return String.Format(LenderPages[3], BankLogoPath, reportData.DateTaken);
        }

        public String GetHTMLHeader()
        {
            return String.Format(HtmlHeader);
        }

        public String GetFooterBar(String className, String pageId = "")
        {
            String footerBar = "\r\n            <div class='{0}'><div class='footer-left-text'>Wisor &copy; 2017</div><div class='footer-page-number'>{1}</div></div>\r\n";
            return String.Format(footerBar, className, pageId);
        }
    }
}
