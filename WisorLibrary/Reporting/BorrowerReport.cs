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

        private static String HtmlHeader;
        private static String HtmlFooter;
        private static List<String> TemplatePages;

        private String pagePrefix;
        private String pageSuffix;

        public BorrowerReport()
        {
            if (ISFirstBorrowerReportInit)
            {
                ISFirstBorrowerReportInit = false;

                Assembly assembly = Assembly.GetExecutingAssembly();

                // Load Borrower Report Resources

                TemplatePages = new List<String>();

                HtmlHeader = (new StreamReader(assembly.GetManifestResourceStream("Wisor_Lender_Report_Generator.borrower_template_pages.template_html_header.html"))).ReadToEnd();
                HtmlFooter = (new StreamReader(assembly.GetManifestResourceStream("Wisor_Lender_Report_Generator.borrower_template_pages.template_html_footer.html"))).ReadToEnd();

                for (int i = 1; i <= 21; i++)
                {
                    TemplatePages.Add((new StreamReader(assembly.GetManifestResourceStream(String.Format("Wisor_Lender_Report_Generator.borrower_template_pages.template_page{0}.html", i))).ReadToEnd()));
                }

                pagePrefix = "      <div class= 'a4'>";
                pageSuffix = "      </div>";
            }
        }



        public int GenerateBorrowerHtmlReport(String filename, ResultReportData reportData)
        {
            // Append individual pages

            // Header
            var reportContent = new StringBuilder(GetHTMLHeader());

            // Page 1
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage1Body(reportData));
            reportContent.Append(pageSuffix);

            // Page 2
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage2Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "1"));
            reportContent.Append(pageSuffix);

            // Page 3
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage3Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "2"));
            reportContent.Append(pageSuffix);

            // Page 4
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage4Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "3"));
            reportContent.Append(pageSuffix);

            // Page 5
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage5Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "4"));
            reportContent.Append(pageSuffix);

            // Page 6
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage6Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "5"));
            reportContent.Append(pageSuffix);

            // Page 7
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage7Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "6"));
            reportContent.Append(pageSuffix);

            // Page 8
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage8Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "7"));
            reportContent.Append(pageSuffix);

            // Page 9
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage9Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "8"));
            reportContent.Append(pageSuffix);

            // Page 10
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage10Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "9"));
            reportContent.Append(pageSuffix);

            // Page 11
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage11Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "10"));
            reportContent.Append(pageSuffix);

            // Page 12
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage12Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "11"));
            reportContent.Append(pageSuffix);

            // Page 13
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage13Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "12"));
            reportContent.Append(pageSuffix);

            // Page 14
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage14Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "13"));
            reportContent.Append(pageSuffix);

            // Page 15
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage15Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "14"));
            reportContent.Append(pageSuffix);

            // Page 16
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage16Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "15"));
            reportContent.Append(pageSuffix);

            // Page 17
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage17Body(reportData));
            reportContent.Append(pageSuffix);

            // Page 18
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage18Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "16"));
            reportContent.Append(pageSuffix);

            // Page 19
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage19Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "17"));
            reportContent.Append(pageSuffix);

            // Page 20
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage20Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "18"));
            reportContent.Append(pageSuffix);

            // Page 21
            reportContent.Append(pagePrefix);
            reportContent.Append(GetPage21Body(reportData));
            reportContent.Append(GetFooterBar("footer_image_banner", "19"));
            reportContent.Append(pageSuffix);

            // Footer
            reportContent.Append(HtmlFooter);

            // Save report to disk
            //String EXEPath = new System.IO.FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            //String HTMLReportPath = String.Format("{0}\\{1}.html", EXEPath, ReportId);

            System.IO.File.WriteAllText(filename, reportContent.ToString());


            return 0;
        }

        public int GenerateBorrowerPdfReport(string filename, ResultReportData reportData)
        {
            // Generate individual HTML pages

            // Page 1
            String Page1Content = HtmlHeader;
            Page1Content += pagePrefix;
            Page1Content += GetPage1Body(reportData);
            Page1Content += pageSuffix;
            Page1Content += GetFooterBar("footer-bar-pdf", "");
            Page1Content += HtmlFooter;

            // Page 2
            String Page2Content = HtmlHeader;
            Page2Content += pagePrefix;
            Page2Content += GetPage2Body(reportData);
            Page2Content += pageSuffix;
            Page2Content += GetFooterBar("footer-bar-pdf", "1");
            Page2Content += HtmlFooter;

            // Page 3
            String Page3Content = HtmlHeader;
            Page3Content += pagePrefix;
            Page3Content += GetPage3Body(reportData);
            Page3Content += pageSuffix;
            Page3Content += GetFooterBar("footer-bar-pdf", "2");
            Page3Content += HtmlFooter;

            // Page 4
            String Page4Content = HtmlHeader;
            Page4Content += pagePrefix;
            Page4Content += GetPage4Body(reportData);
            Page4Content += pageSuffix;
            Page4Content += GetFooterBar("footer-bar-pdf", "");
            Page4Content += HtmlFooter;

            // Page 5
            String Page5Content = HtmlHeader;
            Page5Content += pagePrefix;
            Page5Content += GetPage5Body(reportData);
            Page5Content += pageSuffix;
            Page5Content += GetFooterBar("footer-bar-pdf", "");
            Page5Content += HtmlFooter;

            // Page 6
            String Page6Content = HtmlHeader;
            Page6Content += pagePrefix;
            Page6Content += GetPage6Body(reportData);
            Page6Content += pageSuffix;
            Page6Content += GetFooterBar("footer-bar-pdf", "");
            Page6Content += HtmlFooter;

            // Page 7
            String Page7Content = HtmlHeader;
            Page7Content += pagePrefix;
            Page7Content += GetPage7Body(reportData);
            Page7Content += pageSuffix;
            Page7Content += GetFooterBar("footer-bar-pdf", "");
            Page7Content += HtmlFooter;

            // Page 8
            String Page8Content = HtmlHeader;
            Page8Content += pagePrefix;
            Page8Content += GetPage8Body(reportData);
            Page8Content += pageSuffix;
            Page8Content += GetFooterBar("footer-bar-pdf", "");
            Page8Content += HtmlFooter;

            // Page 9
            String Page9Content = HtmlHeader;
            Page9Content += pagePrefix;
            Page9Content += GetPage9Body(reportData);
            Page9Content += pageSuffix;
            Page9Content += GetFooterBar("footer-bar-pdf", "");
            Page9Content += HtmlFooter;

            // Page 10
            String Page10Content = HtmlHeader;
            Page10Content += pagePrefix;
            Page10Content += GetPage10Body(reportData);
            Page10Content += pageSuffix;
            Page10Content += GetFooterBar("footer-bar-pdf", "");
            Page10Content += HtmlFooter;

            // Page 11
            String Page11Content = HtmlHeader;
            Page11Content += pagePrefix;
            Page11Content += GetPage11Body(reportData);
            Page11Content += pageSuffix;
            Page11Content += GetFooterBar("footer-bar-pdf", "");
            Page11Content += HtmlFooter;

            // Page 12
            String Page12Content = HtmlHeader;
            Page12Content += pagePrefix;
            Page12Content += GetPage12Body(reportData);
            Page12Content += pageSuffix;
            Page12Content += GetFooterBar("footer-bar-pdf", "");
            Page12Content += HtmlFooter;

            // Page 13
            String Page13Content = HtmlHeader;
            Page13Content += pagePrefix;
            Page13Content += GetPage13Body(reportData);
            Page13Content += pageSuffix;
            Page13Content += GetFooterBar("footer-bar-pdf", "");
            Page13Content += HtmlFooter;

            // Page 14
            String Page14Content = HtmlHeader;
            Page14Content += pagePrefix;
            Page14Content += GetPage14Body(reportData);
            Page14Content += pageSuffix;
            Page14Content += GetFooterBar("footer-bar-pdf", "");
            Page14Content += HtmlFooter;

            // Page 15
            String Page15Content = HtmlHeader;
            Page15Content += pagePrefix;
            Page15Content += GetPage15Body(reportData);
            Page15Content += pageSuffix;
            Page15Content += GetFooterBar("footer-bar-pdf", "");
            Page15Content += HtmlFooter;

            // Page 16
            String Page16Content = HtmlHeader;
            Page16Content += pagePrefix;
            Page16Content += GetPage16Body(reportData);
            Page16Content += pageSuffix;
            Page16Content += GetFooterBar("footer-bar-pdf", "");
            Page16Content += HtmlFooter;

            // Page 17
            String Page17Content = HtmlHeader;
            Page17Content += pagePrefix;
            Page17Content += GetPage17Body(reportData);
            Page17Content += pageSuffix;
            Page17Content += GetFooterBar("footer-bar-pdf", "");
            Page17Content += HtmlFooter;

            // Page 18
            String Page18Content = HtmlHeader;
            Page18Content += pagePrefix;
            Page18Content += GetPage18Body(reportData);
            Page18Content += pageSuffix;
            Page18Content += GetFooterBar("footer-bar-pdf", "");
            Page18Content += HtmlFooter;

            // Page 19
            String Page19Content = HtmlHeader;
            Page19Content += pagePrefix;
            Page19Content += GetPage19Body(reportData);
            Page19Content += pageSuffix;
            Page19Content += GetFooterBar("footer-bar-pdf", "");
            Page19Content += HtmlFooter;

            // Page 20
            String Page20Content = HtmlHeader;
            Page20Content += pagePrefix;
            Page20Content += GetPage20Body(reportData);
            Page20Content += pageSuffix;
            Page20Content += GetFooterBar("footer-bar-pdf", "");
            Page20Content += HtmlFooter;

            // Page 21
            String Page21Content = HtmlHeader;
            Page21Content += pagePrefix;
            Page21Content += GetPage21Body(reportData);
            Page21Content += pageSuffix;
            Page21Content += GetFooterBar("footer-bar-pdf", "");
            Page21Content += HtmlFooter;
      
            // Create HTML paths

            var htmlPaths = new List<String>();

            for(int i = 1; i <= 21; i++)
            {
                htmlPaths.Add(String.Format("l{0}_{1}.html",reportData.ID, i));
            }

            // Create HTML pages

            System.IO.File.WriteAllText(htmlPaths[0],  Page1Content);
            System.IO.File.WriteAllText(htmlPaths[1],  Page2Content);
            System.IO.File.WriteAllText(htmlPaths[2],  Page3Content);
            System.IO.File.WriteAllText(htmlPaths[3],  Page4Content);
            System.IO.File.WriteAllText(htmlPaths[4],  Page5Content);
            System.IO.File.WriteAllText(htmlPaths[5],  Page6Content);
            System.IO.File.WriteAllText(htmlPaths[6],  Page7Content);
            System.IO.File.WriteAllText(htmlPaths[7],  Page8Content);
            System.IO.File.WriteAllText(htmlPaths[8],  Page9Content);
            System.IO.File.WriteAllText(htmlPaths[9],  Page10Content);
            System.IO.File.WriteAllText(htmlPaths[10], Page11Content);
            System.IO.File.WriteAllText(htmlPaths[11], Page12Content);
            System.IO.File.WriteAllText(htmlPaths[12], Page13Content);
            System.IO.File.WriteAllText(htmlPaths[13], Page14Content);
            System.IO.File.WriteAllText(htmlPaths[14], Page15Content);
            System.IO.File.WriteAllText(htmlPaths[15], Page16Content);
            System.IO.File.WriteAllText(htmlPaths[16], Page17Content);
            System.IO.File.WriteAllText(htmlPaths[17], Page18Content);
            System.IO.File.WriteAllText(htmlPaths[18], Page19Content);
            System.IO.File.WriteAllText(htmlPaths[19], Page20Content);
            System.IO.File.WriteAllText(htmlPaths[20], Page21Content);

            // Create PDF paths

            var pdfPaths = new List<String>();

            for (int i = 1; i <= 21; i++)
            {
                pdfPaths.Add(String.Format("l{0}_{1}.pdf", reportData.ID, i));
            }

            // Generate PDF pages - wkhtmltopdf documentation - https://wkhtmltopdf.org/usage/wkhtmltopdf.txt

            String cmdPrefix = "--background --margin-bottom 0 --margin-left 0 --margin-right 0 --margin-top 0 --zoom 0.66 --viewport-size 3840x2160";

            for (int i = 0; i < htmlPaths.Count; i++)
            {
                String cmd = String.Format("{0} {1} {2}", cmdPrefix, htmlPaths[i], pdfPaths[i]);

                Process process = new Process();

                process.StartInfo.FileName = "wkhtmltopdf.exe";
                process.StartInfo.Arguments = cmd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
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

        public String GetPage1Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[0]);
        }

        public String GetPage2Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[1]);
        }

        public String GetPage3Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[2]);
        }

        public String GetPage4Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[3]);
        }

        public String GetPage5Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[4]);
        }

        public String GetPage6Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[5]);
        }

        public String GetPage7Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[6]);
        }

        public String GetPage8Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[7]);
        }

        public String GetPage9Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[8]);
        }

        public String GetPage10Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[9]);
        }

        public String GetPage11Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[10]);
        }

        public String GetPage12Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[11]);
        }

        public String GetPage13Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[12]);
        }

        public String GetPage14Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[13]);
        }

        public String GetPage15Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[14]);
        }

        public String GetPage16Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[15]);
        }

        public String GetPage17Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[16]);
        }

        public String GetPage18Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[17]);
        }

        public String GetPage19Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[18]);
        }

        public String GetPage20Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[19]);
        }

        public String GetPage21Body(ResultReportData reportData)
        {
            return String.Format(TemplatePages[20]);
        }

        public String GetHTMLHeader()
        {
            return String.Format(HtmlHeader);
        }

        public String GetFooterBar(String className, String pageId = "")
        {
            String footerBar = "\r\n            <div class='{0}'><div class='footer-page-number'>{1}</div></div><div class='footer-text-banner' dir='rtl'>הדו\"ח הופק באמצעות מערכת Wisor ותכניו כפופים לתנאי השימוש באתר Wisor</div>\r\n";
            return String.Format(footerBar, className, pageId);
        }
    }
}
