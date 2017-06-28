using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using WisorLibrary.DataObjects;
using WisorLibrary.Utilities;

namespace WisorLibrary.Reporting
{
    class BorrowerReport
    {
        private static bool ISFirstBorrowerReportInit = true;

        private static String BHtmlHeader;
        private static String BHtmlFooter;
        private static List<String> BorrowerPages;

        public BorrowerReport()
        {
            if (ISFirstBorrowerReportInit)
            {
                ISFirstBorrowerReportInit = false;

                Assembly assembly = Assembly.GetExecutingAssembly();
                BorrowerPages = new List<String>();

                // Load Borrower Report Resources
                try
                {
                    string fullResourceName = MiscUtilities.GetResourceStream("template_html_header.html");
                    BHtmlHeader = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();
                    fullResourceName = MiscUtilities.GetResourceStream("template_html_footer.html");
                    BHtmlFooter = (new StreamReader(assembly.GetManifestResourceStream(fullResourceName))).ReadToEnd();

                    for (int i = 1; i <= 21; i++)
                    {
                        fullResourceName = MiscUtilities.GetResourceStream(String.Format("template_page{0}.html", i));
                        BorrowerPages.Add((new StreamReader(assembly.GetManifestResourceStream(fullResourceName)).ReadToEnd()));
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LenderReport::OnInit ex: " + ex.ToString());
                }

             }
        }



        public int GenerateBorrowerHtmlReport(string filename, ResultReportData reportData)
        {
            String BankName = "Bank Leumi";
            String ReportId = "BBB000000001";
            String BankLogoPath = "images/bank_leumi_logo.png";
            String Date = "May 2017";
            String CaseId = "0001";




            String pagePrefix = "<div class= 'a4'>";
            String pageSuffix = "</div>";
            String footerBar = "<div class='{0}'><div class='footer_left_text'>Wisor &copy; 2017</div><div class='footer_page_number'>{1}</div></div>";

            // Append individual pages

            // Header
            var reportContent = new StringBuilder(BHtmlHeader);

            // Page 1
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[0], BankName, ReportId, BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", ""));
            reportContent.Append(pageSuffix);

            // Page 2
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[1], BankName, CaseId, ReportId));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "1"));
            reportContent.Append(pageSuffix);

            // Page 3
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[2], BankName, CaseId, ReportId));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "2"));
            reportContent.Append(pageSuffix);

            // Page 4
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[3], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "3"));
            reportContent.Append(pageSuffix);

            // Page 5
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[4], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "4"));
            reportContent.Append(pageSuffix);

            // Page 6
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[5], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "5"));
            reportContent.Append(pageSuffix);

            // Page 7
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[6], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "6"));
            reportContent.Append(pageSuffix);

            // Page 8
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[7], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "7"));
            reportContent.Append(pageSuffix);

            // Page 9
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[8], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "8"));
            reportContent.Append(pageSuffix);

            // Page 10
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[9], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "9"));
            reportContent.Append(pageSuffix);

            // Page 11
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[10], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "10"));
            reportContent.Append(pageSuffix);

            // Page 12
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[11], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "11"));
            reportContent.Append(pageSuffix);

            // Page 13
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[12], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "12"));
            reportContent.Append(pageSuffix);

            // Page 14
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[13], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "13"));
            reportContent.Append(pageSuffix);

            // Page 15
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[14], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "14"));
            reportContent.Append(pageSuffix);

            // Page 16
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[15], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "15"));
            reportContent.Append(pageSuffix);

            // Page 17
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[16], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "16"));
            reportContent.Append(pageSuffix);

            // Page 18
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[17], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "17"));
            reportContent.Append(pageSuffix);

            // Page 19
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[18], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "18"));
            reportContent.Append(pageSuffix);

            // Page 20
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[19], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", "19"));
            reportContent.Append(pageSuffix);

            // Page 21
            reportContent.Append(pagePrefix);
            reportContent.Append(String.Format(BorrowerPages[20], BankLogoPath, Date));
            reportContent.Append(String.Format(footerBar, "footer_bar_html", ""));
            reportContent.Append(pageSuffix);

            // Footer
            reportContent.Append(BHtmlFooter);

            // Save report to disk
            //String EXEPath = new System.IO.FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            //String HTMLReportPath = String.Format("{0}\\{1}.html", EXEPath, ReportId);

            System.IO.File.WriteAllText(filename, reportContent.ToString());


            return 0;
        }






        public int GenerateBorrowerPdfReport(string filename, ResultReportData reportData)
        {
            return 0;
        }




    }
}
