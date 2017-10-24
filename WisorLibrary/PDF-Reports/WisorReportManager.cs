using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp;
using PdfSharp.Drawing.Layout;
using System.Text.RegularExpressions;
using System.IO;
using WisorLib;
using WisorLibrary.Utilities;

namespace WisorLibrary.ReportApplication
{
    /// <summary>
    /// Creates full PDF Report or individual pages for Wisor system.
    /// </summary>
    class WisorReportManager
    {
        public String documentFileName { get; }
        private LongReportDataObject longReportDataObject;
        private PdfDocument document = new PdfDocument();
        private PdfPage page;
        private XGraphics gfx;
        private XTextFormatter tf;
        private XFont fontH1;
        private XFont fontH2;
        private XFont fontH3_5;
        private XFont fontH2Underline;
        private XFont fontH2Bold;
        private XFont fontH3;
        private XFont fontH3Bold;
        private XFont fontH4;
        private XFont fontH4Bold;
        private XFont fontH5;
        private XFont fontH5Bold;
        private XFont fontH6;
        private XFont fontH6Bold;
        private PageSize pageSizeA4 = PageSize.A4;
        private PageOrientation pageOrientationPortrait = PageOrientation.Portrait;
        private XColor colorGreenTitle = XColor.FromArgb(255, 57, 181, 74);
        private XColor colorLightGreenTitle = XColor.FromArgb(255, 106, 199, 119);
        private XColor colorGreenTable = XColor.FromArgb(255, 153, 223, 163);
        private XColor colorLightYellowTable = XColor.FromArgb(255, 255, 240, 125);
        private XColor colorLightPinkTable = XColor.FromArgb(255, 255, 187, 171);
        private XColor colorLightGreenTable = XColor.FromArgb(255, 182, 232, 189);

        /// <summary>
        /// Instantiates a new instance of the WisorReportManager class.
        /// </summary>
        /// <param name="filename">Report file name</param>
        /// <param name="cultureInfo">Specified culture identifier</param>
        /// <param name="data">Data from LongReportDataObject class for filling the report</param>
        public WisorReportManager(String filename, CultureInfo cultureInfo, LongReportDataObject data)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            documentFileName = filename;
            longReportDataObject = data;

            document.Info.Title = "Wisor Report";
            document.Info.Author = "Wisor LTD.";
            document.Info.Subject = "Created for customers";
            document.Info.Keywords = "Loan, Mortgage, Refinance";

            // Fonts
            XPdfFontOptions fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
            fontH1 = new XFont("Arial", 24, XFontStyle.Bold, fontOptions);
            fontH2 = new XFont("Arial", 14, XFontStyle.Regular, fontOptions);
            fontH2Underline = new XFont("Arial", 14, XFontStyle.Underline, fontOptions);
            fontH2Bold = new XFont("Arial", 14, XFontStyle.Bold, fontOptions);
            fontH3 = new XFont("Arial", 12, XFontStyle.Regular, fontOptions);
            fontH3_5 = new XFont("Arial", 11, XFontStyle.Regular, fontOptions);
            fontH3Bold = new XFont("Arial", 12, XFontStyle.Bold, fontOptions);
            fontH4 = new XFont("Arial", 10, XFontStyle.Regular, fontOptions);
            fontH4Bold = new XFont("Arial", 10, XFontStyle.Bold, fontOptions);
            fontH5 = new XFont("Arial", 9.5, XFontStyle.Regular, fontOptions);
            fontH5Bold = new XFont("Arial", 9.5, XFontStyle.Bold, fontOptions);
            fontH6Bold = new XFont("Arial", 8, XFontStyle.Bold, fontOptions);
            fontH6 = new XFont("Arial", 8, XFontStyle.Regular, fontOptions);
        }

        /// <summary>
        /// Setting the direction of text depending 
        /// on the selected culture(RTL or LTR).
        /// Text should be reversed for RTL support.
        /// </summary>
        /// <param name="originalString">original string</param>
        private string CheckRTL(string originalString)
        {
            if ((System.Threading.Thread.CurrentThread.CurrentUICulture.Name != new CultureInfo("he-IL").Name) &&
               (System.Threading.Thread.CurrentThread.CurrentUICulture.Name != new CultureInfo("ar-JO").Name))
            {
                return originalString;
            }
            else
            {
                List<string> lines = new List<string>();
                using (StringReader reader = new StringReader(originalString))
                {
                    for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    {
                        // Split string into words.
                        string[] result = line.Split(' ');
                        string[] newString = new string[result.Length];
                        for (int i = 0; i < result.Length; i++)
                        {
                            // Hebrew and Arabic Unicode characters
                            string pattern = @"\p{IsArabic}|\p{IsHebrew}";
                            // Check every word.
                            // Thus the English words will not be reversed in RTL.
                            Match match = Regex.Match(result[i], pattern);
                            if (match.Success)
                            {
                                char[] charArray = result[i].ToCharArray();
                                Array.Reverse(charArray);
                                newString[result.Length - 1 - i] = new string(charArray);
                            }
                            else
                            {
                                newString[result.Length - 1 - i] = result[i];
                            }
                        }
                        lines.Add(string.Join(" ", newString));
                    }
                }
                // Use two '\n\n' insted '\n' for "nice" view big text
                return string.Join("\n\n", lines.ToArray());
            }
        }


        /// <summary>
        /// Setting the alignment of text depending 
        /// on the selected culture(RTL or LTR).
        /// </summary>
        private XParagraphAlignment SetAlignmentForCulture()
        {
           return XParagraphAlignment.Left;
        }

        /// <summary>
        /// Set the initial appearance of the page, 
        /// headers and footer and page number.
        /// </summary>
        /// <param name="currentPageNumber">Page number</param>
        private void SetInitialViewOfPage(int currentPageNumber)
        {
            // Paint Header
            string fullFilename = MiscUtilities.GetImagesDirectory() + "top.png";
            if (File.Exists(fullFilename))
            {
                XImage imageTop = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageTop, 25, 20, 545, 62);
            }
            else
            {
                Console.WriteLine("ERROR SetInitialViewOfPage file: " + fullFilename + " does not exists");
            }
            fullFilename = MiscUtilities.GetImagesDirectory() + "bottom.png";
            if (File.Exists(fullFilename))
            {
                XImage imageBottom = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageBottom, 25, 770, 75, 55);
            }
            else
            {
                Console.WriteLine("ERROR SetInitialViewOfPage file: " + fullFilename + " does not exists");
            }
            
            // Page Footer Number
            XRect pageNumber = new XRect(47, 805, 111, 30);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(currentPageNumber.ToString(), fontH6Bold, XBrushes.Black, pageNumber, XStringFormats.TopLeft);
            // Page Footer Title
            XRect pageFooterTitle4 = new XRect(140, 810, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("Wisor", fontH6Bold, XBrushes.Black, pageFooterTitle4, XStringFormats.TopLeft);
            XRect pageFooterTitle2 = new XRect(157, 810, 150, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.pageFooterTitle2), fontH6Bold, XBrushes.Black, pageFooterTitle2, XStringFormats.TopLeft);
            XRect pageFooterTitle3 = new XRect(282, 810, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("Wisor", fontH6Bold, XBrushes.Black, pageFooterTitle3, XStringFormats.TopLeft);
            XRect pageFooterTitle1 = new XRect(272, 810, 150, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.pageFooterTitle1), fontH6Bold, XBrushes.Black, pageFooterTitle1, XStringFormats.TopLeft);
        }

        /// <summary>
        /// Creates a full report with all pages.
        /// </summary>
        /// <returns>Returns the status of method execution 
        /// success or failure</returns>
        public Boolean CreateFullReport()
        {
            try
            {
                //CreateCoverPage();
                //CreateDisclaimerPage(1);
                //CreateTableOfContentsPage(2);
                //CreateIntroductionPage(3);
                CreateOrderSummaryPage(4);
                CreateOrderSummaryPage2(5);
                bool state = CreateRefinanceAnalysisPage2B(6);
                if (state) // Only for Refinance type with original loan known
                {
                    int structuresQuantity = CreateRecommendedStructuresPage(7);
                    if (structuresQuantity == 1)
                    {
                        CreateRecommendedOrStressTestStructures(8, 1, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(9);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(10, 1, true);

                            CreateStressTestPriceOffersPage(11);
                            CreatePart5SummaryPage(12);
                            CreateAppendixesPage(13); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(14);
                            CreateAmortisationSchedulePage2(15);
                            CreateAmortisationSchedulePage3(16);
                            CreateEmptyStructuresPage(17);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(10, 1, true);
                            CreateRecommendedOrStressTestStructures(11, 2, true);

                            CreateStressTestPriceOffersPage(12);
                            CreatePart5SummaryPage(13);
                            CreateAppendixesPage(14); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(15);
                            CreateAmortisationSchedulePage2(16);
                            CreateAmortisationSchedulePage3(17);
                            CreateEmptyStructuresPage(18);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(12, 1, true);
                            CreateRecommendedOrStressTestStructures(13, 2, true);
                            CreateRecommendedOrStressTestStructures(14, 3, true);

                            CreateStressTestPriceOffersPage(15);
                            CreatePart5SummaryPage(16);
                            CreateAppendixesPage(17); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(18);
                            CreateAmortisationSchedulePage2(19);
                            CreateAmortisationSchedulePage3(20);
                            CreateEmptyStructuresPage(21);
                        }

                    }
                    if (structuresQuantity == 2)
                    {
                        CreateRecommendedOrStressTestStructures(8, 1, false);
                        CreateRecommendedOrStressTestStructures(9, 2, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(10);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);

                            CreateStressTestPriceOffersPage(12);
                            CreatePart5SummaryPage(13);
                            CreateAppendixesPage(14); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(15);
                            CreateAmortisationSchedulePage2(16);
                            CreateAmortisationSchedulePage3(17);
                            CreateEmptyStructuresPage(18);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);
                            CreateRecommendedOrStressTestStructures(12, 2, true);

                            CreateStressTestPriceOffersPage(13);
                            CreatePart5SummaryPage(14);
                            CreateAppendixesPage(15); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(16);
                            CreateAmortisationSchedulePage2(17);
                            CreateAmortisationSchedulePage3(18);
                            CreateEmptyStructuresPage(19);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);
                            CreateRecommendedOrStressTestStructures(12, 2, true);
                            CreateRecommendedOrStressTestStructures(13, 3, true);

                            CreateStressTestPriceOffersPage(14);
                            CreatePart5SummaryPage(15);
                            CreateAppendixesPage(16); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(17);
                            CreateAmortisationSchedulePage2(18);
                            CreateAmortisationSchedulePage3(19);
                            CreateEmptyStructuresPage(20);
                        }
                    }
                    if (structuresQuantity == 3)
                    {
                        CreateRecommendedOrStressTestStructures(8, 1, false);
                        CreateRecommendedOrStressTestStructures(9, 2, false);
                        CreateRecommendedOrStressTestStructures(10, 3, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(11);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(12, 1, true);

                            CreateStressTestPriceOffersPage(13);
                            CreatePart5SummaryPage(14);
                            CreateAppendixesPage(15); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(16);
                            CreateAmortisationSchedulePage2(17);
                            CreateAmortisationSchedulePage3(18);
                            CreateEmptyStructuresPage(19);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(12, 1, true);
                            CreateRecommendedOrStressTestStructures(13, 2, true);

                            CreateStressTestPriceOffersPage(14);
                            CreatePart5SummaryPage(15);
                            CreateAppendixesPage(16); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(17);
                            CreateAmortisationSchedulePage2(18);
                            CreateAmortisationSchedulePage3(19);
                            CreateEmptyStructuresPage(20);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(12, 1, true);
                            CreateRecommendedOrStressTestStructures(13, 2, true);
                            CreateRecommendedOrStressTestStructures(14, 3, true);

                            CreateStressTestPriceOffersPage(15);
                            CreatePart5SummaryPage(16);
                            CreateAppendixesPage(17); // Only for Refinance with original loan known and inserted 
                            CreateAmortisationSchedulePage1(18);
                            CreateAmortisationSchedulePage2(19);
                            CreateAmortisationSchedulePage3(20);
                            CreateEmptyStructuresPage(21);
                        }
                    }
                }
                else // If Part 2B not exist then page numbers continues with page number 6. Also page "Appendixes" not created.
                {
                    int structuresQuantity = CreateRecommendedStructuresPage(6);
                    if (structuresQuantity == 1)
                    {
                        CreateRecommendedOrStressTestStructures(7, 1, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(8);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(9, 1, true);

                            CreateStressTestPriceOffersPage(10);
                            CreatePart5SummaryPage(11);
                            CreateAmortisationSchedulePage1(12);
                            CreateAmortisationSchedulePage2(13);
                            CreateAmortisationSchedulePage3(14);
                            CreateEmptyStructuresPage(15);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(9, 1, true);
                            CreateRecommendedOrStressTestStructures(10, 2, true);

                            CreateStressTestPriceOffersPage(11);
                            CreatePart5SummaryPage(12);
                            CreateAmortisationSchedulePage1(13);
                            CreateAmortisationSchedulePage2(14);
                            CreateAmortisationSchedulePage3(15);
                            CreateEmptyStructuresPage(16);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);
                            CreateRecommendedOrStressTestStructures(12, 2, true);
                            CreateRecommendedOrStressTestStructures(13, 3, true);

                            CreateStressTestPriceOffersPage(14);
                            CreatePart5SummaryPage(15);
                            CreateAmortisationSchedulePage1(16);
                            CreateAmortisationSchedulePage2(17);
                            CreateAmortisationSchedulePage3(18);
                            CreateEmptyStructuresPage(19);
                        }

                    }
                    if (structuresQuantity == 2)
                    {
                        CreateRecommendedOrStressTestStructures(7, 1, false);
                        CreateRecommendedOrStressTestStructures(8, 2, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(9);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(10, 1, true);

                            CreateStressTestPriceOffersPage(11);
                            CreatePart5SummaryPage(12);
                            CreateAmortisationSchedulePage1(13);
                            CreateAmortisationSchedulePage2(14);
                            CreateAmortisationSchedulePage3(15);
                            CreateEmptyStructuresPage(16);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(10, 1, true);
                            CreateRecommendedOrStressTestStructures(11, 2, true);

                            CreateStressTestPriceOffersPage(12);
                            CreatePart5SummaryPage(13);
                            CreateAmortisationSchedulePage1(14);
                            CreateAmortisationSchedulePage2(15);
                            CreateAmortisationSchedulePage3(16);
                            CreateEmptyStructuresPage(17);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(10, 1, true);
                            CreateRecommendedOrStressTestStructures(11, 2, true);
                            CreateRecommendedOrStressTestStructures(12, 3, true);

                            CreateStressTestPriceOffersPage(13);
                            CreatePart5SummaryPage(14);
                            CreateAmortisationSchedulePage1(15);
                            CreateAmortisationSchedulePage2(16);
                            CreateAmortisationSchedulePage3(17);
                            CreateEmptyStructuresPage(18);
                        }
                    }
                    if (structuresQuantity == 3)
                    {
                        CreateRecommendedOrStressTestStructures(7, 1, false);
                        CreateRecommendedOrStressTestStructures(8, 2, false);
                        CreateRecommendedOrStressTestStructures(9, 3, false);

                        int stressStructuresQuantity = CreateStressTestWisorRecommendedStructuresPage(10);
                        if (stressStructuresQuantity == 1)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);

                            CreateStressTestPriceOffersPage(12);
                            CreatePart5SummaryPage(13);
                            CreateAmortisationSchedulePage1(14);
                            CreateAmortisationSchedulePage2(15);
                            CreateAmortisationSchedulePage3(16);
                            CreateEmptyStructuresPage(17);
                        }
                        if (stressStructuresQuantity == 2)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);
                            CreateRecommendedOrStressTestStructures(12, 2, true);

                            CreateStressTestPriceOffersPage(13);
                            CreatePart5SummaryPage(14);
                            CreateAmortisationSchedulePage1(15);
                            CreateAmortisationSchedulePage2(16);
                            CreateAmortisationSchedulePage3(17);
                            CreateEmptyStructuresPage(18);
                        }
                        if (stressStructuresQuantity == 3)
                        {
                            CreateRecommendedOrStressTestStructures(11, 1, true);
                            CreateRecommendedOrStressTestStructures(12, 2, true);
                            CreateRecommendedOrStressTestStructures(13, 3, true);

                            CreateStressTestPriceOffersPage(14);
                            CreatePart5SummaryPage(15);
                            CreateAmortisationSchedulePage1(16);
                            CreateAmortisationSchedulePage2(17);
                            CreateAmortisationSchedulePage3(18);
                            CreateEmptyStructuresPage(19);
                        }
                    }
                }
                // Save and start View 
                document.Save(documentFileName); //  "Report.pdf"
                Process.Start(documentFileName); //  "Report.pdf"
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CreateFullReport Exception message: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Saves the document in a file using the path in the settings.
        /// </summary>
        /// <returns>Returns the status of method execution 
        /// success or failure</returns>
        public Boolean SavePDFDocument()
        {
            try
            {
                document.Save(documentFileName); //  "Report.pdf"
#if DEBUG
                Process.Start(documentFileName); //  "Report.pdf" // don't use in release
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region methodsForCreatingEveryPages

        /// <summary>
        /// Creates the cover of the PDF document.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateCoverPage()
        {
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            gfx = XGraphics.FromPdfPage(page);
            string fullFilename = MiscUtilities.GetImagesDirectory() + "cover_page.png";
            if (File.Exists(fullFilename))
            {
                XImage imageTop = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageTop, 0, 0, 594, 840);
            }
            else
            {
                Console.WriteLine("ERROR CreateCoverPage file: " + fullFilename + " does not exists");
            }
        }

        /// <summary>
        /// Creates the Page 1: Disclaimer.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateDisclaimerPage(int number)
        {
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);

            SetInitialViewOfPage(number);

            XRect dateOfReport = new XRect(465, 100, 100, 50);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.dateOfReport.ToString("MM/dd/yyyy")), fontH6Bold, XBrushes.Black, dateOfReport, XStringFormats.TopLeft);
            XRect disclaimerText = new XRect(25, 170, 540, 530);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.disclaimerText), fontH4, XBrushes.Black, disclaimerText, XStringFormats.TopLeft);
            string fullFilename = MiscUtilities.GetImagesDirectory() + "wisor_signature.png";
            if (File.Exists(fullFilename))
            {
                XImage imageSignature = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageSignature, 495, 695, 70.5, 18);
            }
            else
            {
                Console.WriteLine("ERROR CreateDisclaimerPage file: " + fullFilename + " does not exists");
            }
        }

        /// <summary>
        /// Creates the Page 2: Table of Contents.
        /// </summary>
        /// <remarks>
        /// Page is NOT constant.
        /// </remarks>
        public void CreateTableOfContentsPage(int number)
        {
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // Section title
            XRect sectionTitle = new XRect(250, 100, 310, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionTitle), fontH1, new XSolidBrush(colorGreenTitle), sectionTitle, XStringFormats.TopLeft);
            // Part1
            XRect part1IntroductionNumber = new XRect(250, 150, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("1", fontH3Bold, XBrushes.Black, part1IntroductionNumber, XStringFormats.TopLeft);
            XRect part1Introduction = new XRect(212, 150, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part1Introduction), fontH3Bold, XBrushes.Black, part1Introduction, XStringFormats.TopLeft);
            XRect conseptsAndDefinitionsNumber = new XRect(212, 163, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("1.1", fontH3, XBrushes.Black, conseptsAndDefinitionsNumber, XStringFormats.TopLeft);
            XRect conseptsAndDefinitions = new XRect(174, 163, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conseptsAndDefinitions), fontH3, XBrushes.Black, conseptsAndDefinitions, XStringFormats.TopLeft);
            XRect whatToDoWithReportNumber = new XRect(212, 176, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("1.2", fontH3, XBrushes.Black, whatToDoWithReportNumber, XStringFormats.TopLeft);
            XRect whatToDoWithReport = new XRect(174, 176, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.whatToDoWithReport), fontH3, XBrushes.Black, whatToDoWithReport, XStringFormats.TopLeft);
            // Part2
            XRect part2OrderDetailsNumber = new XRect(250, 226, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("2", fontH3Bold, XBrushes.Black, part2OrderDetailsNumber, XStringFormats.TopLeft);
            XRect part2OrderDetails = new XRect(212, 226, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part2OrderDetails), fontH3Bold, XBrushes.Black, part2OrderDetails, XStringFormats.TopLeft);
            // Purchase any
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers) ||
                 (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_PriceOffers) ||
                 (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers) ||
                 (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_PriceOffers))
            {
                XRect loanDetailsNumber = new XRect(212, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.1", fontH3, XBrushes.Black, loanDetailsNumber, XStringFormats.TopLeft);
                XRect loanDetails = new XRect(174, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanDetails), fontH3, XBrushes.Black, loanDetails, XStringFormats.TopLeft);
                XRect personalPreferencesNumber = new XRect(212, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.2", fontH3, XBrushes.Black, personalPreferencesNumber, XStringFormats.TopLeft);
                XRect personalPreferences = new XRect(174, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.personalPreferences), fontH3, XBrushes.Black, personalPreferences, XStringFormats.TopLeft);
                XRect principalApprovalNumber = new XRect(212, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.3", fontH3, XBrushes.Black, principalApprovalNumber, XStringFormats.TopLeft);
                XRect principalApproval = new XRect(174, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.principalApproval), fontH3, XBrushes.Black, principalApproval, XStringFormats.TopLeft);
                XRect priceOffersNumber = new XRect(212, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.4", fontH3, XBrushes.Black, priceOffersNumber, XStringFormats.TopLeft);
                XRect priceOffers = new XRect(174, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.priceOffers), fontH3, XBrushes.Black, priceOffers, XStringFormats.TopLeft);
            }
            // Refinance NO loan inserted with NO price offers or with price offers presented
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                XRect loanDetailsNumber = new XRect(212, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.1", fontH3, XBrushes.Black, loanDetailsNumber, XStringFormats.TopLeft);
                XRect loanDetails = new XRect(174, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanDetails), fontH3, XBrushes.Black, loanDetails, XStringFormats.TopLeft);
                XRect personalPreferencesNumber = new XRect(212, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.2", fontH3, XBrushes.Black, personalPreferencesNumber, XStringFormats.TopLeft);
                XRect personalPreferences = new XRect(174, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.personalPreferences), fontH3, XBrushes.Black, personalPreferences, XStringFormats.TopLeft);
                XRect originalLoanDetailsNumber = new XRect(212, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.3", fontH3, XBrushes.Black, originalLoanDetailsNumber, XStringFormats.TopLeft);
                XRect originalLoanDetails = new XRect(174, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.originalLoanDetails), fontH3, XBrushes.Black, originalLoanDetails, XStringFormats.TopLeft);
                XRect principalApprovalNumber = new XRect(212, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.4", fontH3, XBrushes.Black, principalApprovalNumber, XStringFormats.TopLeft);
                XRect principalApproval = new XRect(174, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.principalApproval), fontH3, XBrushes.Black, principalApproval, XStringFormats.TopLeft);
                XRect priceOffersNumber = new XRect(212, 291, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.5", fontH3, XBrushes.Black, priceOffersNumber, XStringFormats.TopLeft);
                XRect priceOffers = new XRect(174, 291, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.priceOffers), fontH3, XBrushes.Black, priceOffers, XStringFormats.TopLeft);
            }
            // Refinance loan inserted with NO price offers or with price offers presented
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers))
            {
                XRect loanDetailsNumber = new XRect(212, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.1", fontH3, XBrushes.Black, loanDetailsNumber, XStringFormats.TopLeft);
                XRect loanDetails = new XRect(174, 239, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanDetails), fontH3, XBrushes.Black, loanDetails, XStringFormats.TopLeft);
                XRect personalPreferencesNumber = new XRect(212, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.2", fontH3, XBrushes.Black, personalPreferencesNumber, XStringFormats.TopLeft);
                XRect personalPreferences = new XRect(174, 252, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.personalPreferences), fontH3, XBrushes.Black, personalPreferences, XStringFormats.TopLeft);
                XRect originalLoanDetailsNumber = new XRect(212, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.3", fontH3, XBrushes.Black, originalLoanDetailsNumber, XStringFormats.TopLeft);
                XRect originalLoanDetails = new XRect(174, 265, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.originalLoanDetails), fontH3, XBrushes.Black, originalLoanDetails, XStringFormats.TopLeft);
                XRect principalApprovalNumber = new XRect(212, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.4", fontH3, XBrushes.Black, principalApprovalNumber, XStringFormats.TopLeft);
                XRect principalApproval = new XRect(174, 278, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.principalApproval), fontH3, XBrushes.Black, principalApproval, XStringFormats.TopLeft);
                XRect priceOffersNumber = new XRect(212, 291, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.5", fontH3, XBrushes.Black, priceOffersNumber, XStringFormats.TopLeft);
                XRect priceOffers = new XRect(174, 291, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.priceOffers), fontH3, XBrushes.Black, priceOffers, XStringFormats.TopLeft);
                XRect originalLoanAnalysisNumber = new XRect(212, 304, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("2.6", fontH3, XBrushes.Black, originalLoanAnalysisNumber, XStringFormats.TopLeft);
                XRect originalLoanAnalysis = new XRect(174, 304, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.originalLoanAnalysis), fontH3, XBrushes.Black, originalLoanAnalysis, XStringFormats.TopLeft);
            }
            // Part3 Constant
            XRect part3RecommandedStructuresNumber = new XRect(250, 354, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("3", fontH3Bold, XBrushes.Black, part3RecommandedStructuresNumber, XStringFormats.TopLeft);
            XRect part3RecommandedStructures = new XRect(212, 354, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part3RecommandedStructures), fontH3Bold, XBrushes.Black, part3RecommandedStructures, XStringFormats.TopLeft);
            // Part4
            XRect part4StressTestNumber = new XRect(250, 404, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("4", fontH3Bold, XBrushes.Black, part4StressTestNumber, XStringFormats.TopLeft);
            XRect part4StressTest = new XRect(212, 404, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part4StressTest), fontH3Bold, XBrushes.Black, part4StressTest, XStringFormats.TopLeft);
            // No Price offers for any type
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers))
            {
                XRect changesToRecommendationsNumber = new XRect(212, 417, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("4.1", fontH3, XBrushes.Black, changesToRecommendationsNumber, XStringFormats.TopLeft);
                XRect changesToRecommendations = new XRect(174, 417, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.changesToRecommendations), fontH3, XBrushes.Black, changesToRecommendations, XStringFormats.TopLeft);
                XRect stressTestNumber = new XRect(212, 430, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("4.2", fontH3, XBrushes.Black, stressTestNumber, XStringFormats.TopLeft);
                XRect stressTest = new XRect(174, 430, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTest), fontH3, XBrushes.Black, stressTest, XStringFormats.TopLeft);
            }
            // Including Price offers for any type
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                XRect changesToRecommendationsNumber = new XRect(212, 417, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("4.1", fontH3, XBrushes.Black, changesToRecommendationsNumber, XStringFormats.TopLeft);
                XRect changesToRecommendations = new XRect(174, 417, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.changesToRecommendations), fontH3, XBrushes.Black, changesToRecommendations, XStringFormats.TopLeft);
                XRect stressTestNumber = new XRect(212, 430, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("4.2", fontH3, XBrushes.Black, stressTestNumber, XStringFormats.TopLeft);
                XRect stressTest = new XRect(174, 430, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTest), fontH3, XBrushes.Black, stressTest, XStringFormats.TopLeft);
                XRect stressTestOffersNumber = new XRect(212, 443, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("4.3", fontH3, XBrushes.Black, stressTestOffersNumber, XStringFormats.TopLeft);
                XRect stressTestOffers = new XRect(174, 443, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTestOffers), fontH3, XBrushes.Black, stressTestOffers, XStringFormats.TopLeft);
            }
            // Part5 constant
            XRect part5SummaryNumber = new XRect(250, 493, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("5", fontH3Bold, XBrushes.Black, part5SummaryNumber, XStringFormats.TopLeft);
            XRect part5Summary = new XRect(212, 493, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part5Summary), fontH3Bold, XBrushes.Black, part5Summary, XStringFormats.TopLeft);
            XRect SummaryAndRecommandationNumber = new XRect(212, 506, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("5.1", fontH3, XBrushes.Black, SummaryAndRecommandationNumber, XStringFormats.TopLeft);
            XRect SummaryAndRecommandation = new XRect(174, 506, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.SummaryAndRecommandation), fontH3, XBrushes.Black, SummaryAndRecommandation, XStringFormats.TopLeft);
            XRect stressTestchoosingFinalCompositionNumber = new XRect(212, 519, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("5.2", fontH3, XBrushes.Black, stressTestchoosingFinalCompositionNumber, XStringFormats.TopLeft);
            XRect choosingFinalComposition = new XRect(174, 519, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.choosingFinalComposition), fontH3, XBrushes.Black, choosingFinalComposition, XStringFormats.TopLeft);
            XRect FutureStepsNumber = new XRect(212, 532, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("5.3", fontH3, XBrushes.Black, FutureStepsNumber, XStringFormats.TopLeft);
            XRect FutureSteps = new XRect(174, 532, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.FutureSteps), fontH3, XBrushes.Black, FutureSteps, XStringFormats.TopLeft);
            // Part6
            XRect part6AppendixedNumber = new XRect(250, 582, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("6", fontH3Bold, XBrushes.Black, part6AppendixedNumber, XStringFormats.TopLeft);
            XRect part6Appendixed = new XRect(212, 582, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part6Appendixed), fontH3Bold, XBrushes.Black, part6Appendixed, XStringFormats.TopLeft);
            // Purchase NO price offers or Refinance NO loan inserted NO price offers
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers))
            {
                XRect silukinForRecomendationsNumber = new XRect(212, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("א", fontH3, XBrushes.Black, silukinForRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinForRecomendations = new XRect(174, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinForRecomendations), fontH3, XBrushes.Black, silukinForRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendationsNumber = new XRect(212, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ב", fontH3, XBrushes.Black, silukinStressTestRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendations = new XRect(174, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestRecomendations), fontH3, XBrushes.Black, silukinStressTestRecomendations, XStringFormats.TopLeft);
                XRect emptyStructureFormNumber = new XRect(212, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ג", fontH3, XBrushes.Black, emptyStructureFormNumber, XStringFormats.TopLeft);
                XRect emptyStructureForm = new XRect(174, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.emptyStructureForm), fontH3, XBrushes.Black, emptyStructureForm, XStringFormats.TopLeft);
            }
            // Purchase with price offers or Refinance NO loan with price offers ??
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                XRect silukinForRecomendationsNumber = new XRect(212, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("א", fontH3, XBrushes.Black, silukinForRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinForRecomendations = new XRect(174, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinForRecomendations), fontH3, XBrushes.Black, silukinForRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendationsNumber = new XRect(212, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ב", fontH3, XBrushes.Black, silukinStressTestRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendations = new XRect(174, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestRecomendations), fontH3, XBrushes.Black, silukinStressTestRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestOffersNumber = new XRect(212, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ג", fontH3, XBrushes.Black, silukinStressTestOffersNumber, XStringFormats.TopLeft);
                XRect silukinStressTestOffers = new XRect(174, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestOffers), fontH3, XBrushes.Black, silukinStressTestOffers, XStringFormats.TopLeft);
                XRect emptyStructureFormNumber = new XRect(212, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ד", fontH3, XBrushes.Black, emptyStructureFormNumber, XStringFormats.TopLeft);
                XRect emptyStructureForm = new XRect(174, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.emptyStructureForm), fontH3, XBrushes.Black, emptyStructureForm, XStringFormats.TopLeft);
            }
            // Refinance loan inserted with NO price offers 
            if (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers)
            {
                XRect silukinOriginalLoanNumber = new XRect(212, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("א", fontH3, XBrushes.Black, silukinOriginalLoanNumber, XStringFormats.TopLeft);
                XRect silukinOriginalLoan = new XRect(174, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinOriginalLoan), fontH3, XBrushes.Black, silukinOriginalLoan, XStringFormats.TopLeft);
                XRect silukinForRecomendationsNumber = new XRect(212, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ב", fontH3, XBrushes.Black, silukinForRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinForRecomendations = new XRect(174, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinForRecomendations), fontH3, XBrushes.Black, silukinForRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendationsNumber = new XRect(212, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ג", fontH3, XBrushes.Black, silukinStressTestRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendations = new XRect(174, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestRecomendations), fontH3, XBrushes.Black, silukinStressTestRecomendations, XStringFormats.TopLeft);
                XRect emptyStructureFormNumber = new XRect(212, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ד", fontH3, XBrushes.Black, emptyStructureFormNumber, XStringFormats.TopLeft);
                XRect emptyStructureForm = new XRect(174, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.emptyStructureForm), fontH3, XBrushes.Black, emptyStructureForm, XStringFormats.TopLeft);
            }
            // Refinance loan inserted with price offers
            if (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers)
            {
                XRect silukinOriginalLoanNumber = new XRect(212, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("א", fontH3, XBrushes.Black, silukinOriginalLoanNumber, XStringFormats.TopLeft);
                XRect silukinOriginalLoan = new XRect(174, 595, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinOriginalLoan), fontH3, XBrushes.Black, silukinOriginalLoan, XStringFormats.TopLeft);
                XRect silukinForRecomendationsNumber = new XRect(212, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ב", fontH3, XBrushes.Black, silukinForRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinForRecomendations = new XRect(174, 608, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinForRecomendations), fontH3, XBrushes.Black, silukinForRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendationsNumber = new XRect(212, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ג", fontH3, XBrushes.Black, silukinStressTestRecomendationsNumber, XStringFormats.TopLeft);
                XRect silukinStressTestRecomendations = new XRect(174, 621, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestRecomendations), fontH3, XBrushes.Black, silukinStressTestRecomendations, XStringFormats.TopLeft);
                XRect silukinStressTestOffersNumber = new XRect(212, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ד", fontH3, XBrushes.Black, silukinStressTestOffersNumber, XStringFormats.TopLeft);
                XRect silukinStressTestOffers = new XRect(174, 634, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.silukinStressTestOffers), fontH3, XBrushes.Black, silukinStressTestOffers, XStringFormats.TopLeft);
                XRect emptyStructureFormNumber = new XRect(212, 647, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("ה", fontH3, XBrushes.Black, emptyStructureFormNumber, XStringFormats.TopLeft);
                XRect emptyStructureForm = new XRect(174, 647, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.emptyStructureForm), fontH3, XBrushes.Black, emptyStructureForm, XStringFormats.TopLeft);
            }
        }

        /// <summary>
        /// Creates the Part 1: Introduction.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateIntroductionPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // Section title
            XRect pageIntroductionHeader = new XRect(250, 100, 310, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.pageIntroductionHeader), fontH1, new XSolidBrush(colorGreenTitle), pageIntroductionHeader, XStringFormats.TopLeft);
            // sec.1.1
            XRect conceptsAndDefinitionsHeader11 = new XRect(250, 150, 310, 30);
            XPen conceptsAndDefinitionsHeader11Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(conceptsAndDefinitionsHeader11Underline,
                new XPoint(450, 167),
                new XPoint(563, 167));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsHeader11), fontH2, new XSolidBrush(colorGreenTitle), conceptsAndDefinitionsHeader11, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText1 = new XRect(10, 180, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText1), fontH5, XBrushes.Black, conceptsAndDefinitionsText1, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionHeaderNested1 = new XRect(250, 240, 310, 30);
            XPen conceptsAndDefinitionHeaderNested1Underline = new XPen(colorGreenTitle, 1);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionHeaderNested1), fontH2, new XSolidBrush(colorGreenTitle), conceptsAndDefinitionHeaderNested1, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part1 = new XRect(10, 260, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part1), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part1, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part2 = new XRect(510, 280, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part2), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part2, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part3 = new XRect(495, 280, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part3), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part3, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part4 = new XRect(370, 280, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part4), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part4, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part5 = new XRect(290, 280, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part5), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part5, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText2Part6 = new XRect(52, 280, 300, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText2Part6), fontH5, XBrushes.Black, conceptsAndDefinitionsText2Part6, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionHeaderNested2 = new XRect(250, 300, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionHeaderNested2), fontH2, new XSolidBrush(colorGreenTitle), conceptsAndDefinitionHeaderNested2, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText3 = new XRect(10, 320, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText3), fontH5, XBrushes.Black, conceptsAndDefinitionsText3, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionHeaderNested3 = new XRect(250, 380, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionHeaderNested3), fontH2, new XSolidBrush(colorGreenTitle), conceptsAndDefinitionHeaderNested3, XStringFormats.TopLeft);
            XRect conceptsAndDefinitionsText4 = new XRect(10, 400, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.conceptsAndDefinitionsText4), fontH5, XBrushes.Black, conceptsAndDefinitionsText4, XStringFormats.TopLeft);
            // Sec.1.1
            XRect whatToDoWithReportHeader12 = new XRect(250, 530, 310, 30);
            XPen whatToDoWithReportHeader12Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(whatToDoWithReportHeader12Underline,
                new XPoint(425, 545),
                new XPoint(563, 545));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.whatToDoWithReportHeader12), fontH2, new XSolidBrush(colorGreenTitle), whatToDoWithReportHeader12, XStringFormats.TopLeft);
            XRect whatToDoWithReportText = new XRect(10, 555, 550, 230);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.whatToDoWithReportText), fontH5, XBrushes.Black, whatToDoWithReportText, XStringFormats.TopLeft);
        }

        /// <summary>
        /// Creates the Part 2: Order summary.
        /// </summary>
        /// <remarks>
        /// Page is NOT constant.
        /// </remarks>
        public void CreateOrderSummaryPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // SECTION 2
            XRect rect = new XRect(250, 100, 310, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part2Title), fontH1, new XSolidBrush(colorGreenTitle), rect, XStringFormats.TopLeft);
            XRect rectOrderNumberTitle = new XRect(450, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.orderNumberTitle), fontH6Bold, XBrushes.Black, rectOrderNumberTitle, XStringFormats.TopLeft);
            XRect rectOrderNumberValue = new XRect(380, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.OrderNumberValue), fontH6Bold, XBrushes.Black, rectOrderNumberValue, XStringFormats.TopLeft);
            XRect rectEmailTitle = new XRect(450, 150, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.emailTitle), fontH6Bold, XBrushes.Black, rectEmailTitle, XStringFormats.TopLeft);
            XRect rectEmailValue = new XRect(360, 150, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.EmailValue), fontH6Bold, XBrushes.Black, rectEmailValue, XStringFormats.TopLeft);
            //
            // SECTION 2.1
            //
            XRect sectionOrderDetails = new XRect(250, 180, 310, 30);
            XPen sectionOrderDetailsUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionOrderDetailsUnderline,
                new XPoint(471, 197),
                new XPoint(563, 197));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionOrderDetails), fontH2, new XSolidBrush(colorGreenTitle), sectionOrderDetails, XStringFormats.TopLeft);
            // Left side (LTV ratio and PTI ratio)
            XRect ltvRatio = new XRect(30, 210, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.ltvRatioTitle), fontH6Bold, XBrushes.Black, ltvRatio, XStringFormats.TopLeft);
            XRect ltvRatioValue = new XRect(30, 225, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.LtvRatioValue.ToString()) + "%", fontH6Bold, new XSolidBrush(colorGreenTitle), ltvRatioValue, XStringFormats.TopLeft);
            XRect ptiRatio = new XRect(30, 240, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.ptiRatioTitle), fontH6Bold, XBrushes.Black, ptiRatio, XStringFormats.TopLeft);
            XRect ptiRatioValue = new XRect(30, 255, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.PtiRatioValue.ToString("N0")) + "%", fontH6Bold, new XSolidBrush(colorGreenTitle), ptiRatioValue, XStringFormats.TopLeft);
            // Middle (Titles)
            XRect numOfBorrowersTitle = new XRect(260, 210, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.numOfBorrowersTitle), fontH6Bold, XBrushes.Black, numOfBorrowersTitle, XStringFormats.TopLeft);
            XRect youngestAgeTitle = new XRect(260, 225, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.youngestAgeTitle), fontH6Bold, XBrushes.Black, youngestAgeTitle, XStringFormats.TopLeft);
            XRect totalIncomeTitle = new XRect(260, 240, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.totalIncomeTitle), fontH6Bold, XBrushes.Black, totalIncomeTitle, XStringFormats.TopLeft);
            XRect obligationsTitle = new XRect(260, 255, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.obligationsTitle), fontH6Bold, XBrushes.Black, obligationsTitle, XStringFormats.TopLeft);
            XRect futureMoneyTitle = new XRect(260, 270, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.futureMoneyTitle), fontH6Bold, XBrushes.Black, futureMoneyTitle, XStringFormats.TopLeft);
            XRect savingsTitle = new XRect(260, 285, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.savingsTitle), fontH6Bold, XBrushes.Black, savingsTitle, XStringFormats.TopLeft);
            // Middle (Values)
            XRect numOfBorrowersValue = new XRect(230, 210, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.NumOfBorrowersValue.ToString("N0")), fontH6, XBrushes.Black, numOfBorrowersValue, XStringFormats.TopLeft);
            XRect youngestAgeValue = new XRect(230, 225, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.YoungestAgeValue.ToString()), fontH6, XBrushes.Black, youngestAgeValue, XStringFormats.TopLeft);
            XRect totalIncomeValue = new XRect(230, 240, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalIncomeValue.ToString("N0")), fontH6, XBrushes.Black, totalIncomeValue, XStringFormats.TopLeft);
            XRect obligationsValue = new XRect(230, 255, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.ObligationsValue.ToString("N0")), fontH6, XBrushes.Black, obligationsValue, XStringFormats.TopLeft);
            XRect futureMoneyValue = new XRect(230, 270, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.FutureMoneyValue), fontH6, XBrushes.Black, futureMoneyValue, XStringFormats.TopLeft);
            XRect savingsValue = new XRect(230, 285, 50, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SavingsValue), fontH6, XBrushes.Black, savingsValue, XStringFormats.TopLeft);
            // PURCHASE USED PROPERTY
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers) ||
                    (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_PriceOffers))
            {
                // Right side (Titles)
                XRect dealTypeTitle = new XRect(450, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.dealTypeTitle), fontH6Bold, XBrushes.Black, dealTypeTitle, XStringFormats.TopLeft);
                XRect propertyValueTitle = new XRect(450, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.propertyValueTitle), fontH6Bold, XBrushes.Black, propertyValueTitle, XStringFormats.TopLeft);
                XRect loanAmountTitle = new XRect(450, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanAmountTitle), fontH6Bold, XBrushes.Black, loanAmountTitle, XStringFormats.TopLeft);
                XRect downPaymentTitle = new XRect(450, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.downPaymentTitle), fontH6Bold, XBrushes.Black, downPaymentTitle, XStringFormats.TopLeft);
                XRect monthlyPaymentTitle = new XRect(450, 270, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.monthlyPaymentTitle), fontH6Bold, XBrushes.Black, monthlyPaymentTitle, XStringFormats.TopLeft);
                // Right side (Values)
                XRect dealTypeValue = new XRect(370, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.DealTypeValue), fontH6Bold, XBrushes.Black, dealTypeValue, XStringFormats.TopLeft);
                XRect propertyValueValue = new XRect(370, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PropertyValueValue.ToString("N0")), fontH6, XBrushes.Black, propertyValueValue, XStringFormats.TopLeft);
                XRect loanAmountValue = new XRect(370, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.LoanAmountValue.ToString("N0")), fontH6, XBrushes.Black, loanAmountValue, XStringFormats.TopLeft);
                XRect downPaymentValue = new XRect(370, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.DownPaymentValue.ToString("N0")), fontH6, XBrushes.Black, downPaymentValue, XStringFormats.TopLeft);
                XRect monthlyPaymentValue = new XRect(370, 270, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.MonthlyPaymentValue.ToString("N0")), fontH6, XBrushes.Black, monthlyPaymentValue, XStringFormats.TopLeft);
            }
            // PURCHASE NEW PROPERTY
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_PriceOffers))
            {
                // Right side (Titles)
                XRect dealTypeTitle = new XRect(450, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.dealTypeTitle), fontH6Bold, XBrushes.Black, dealTypeTitle, XStringFormats.TopLeft);
                XRect propertyValueTitle = new XRect(450, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.propertyValueTitle), fontH6Bold, XBrushes.Black, propertyValueTitle, XStringFormats.TopLeft);
                XRect loanAmountTitle = new XRect(450, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanAmountTitle), fontH6Bold, XBrushes.Black, loanAmountTitle, XStringFormats.TopLeft);
                XRect downPaymentTitle = new XRect(450, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.downPaymentTitle), fontH6Bold, XBrushes.Black, downPaymentTitle, XStringFormats.TopLeft);
                XRect monthlyPaymentTitle = new XRect(450, 270, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.monthlyPaymentTitle), fontH6Bold, XBrushes.Black, monthlyPaymentTitle, XStringFormats.TopLeft);
                XRect dateOfEntranceTitle = new XRect(450, 285, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.dateOfEntranceTitle), fontH6Bold, XBrushes.Black, dateOfEntranceTitle, XStringFormats.TopLeft);
                XRect rentTodayTitle = new XRect(450, 300, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.rentTodayTitle), fontH6Bold, XBrushes.Black, rentTodayTitle, XStringFormats.TopLeft);
                // Right side (Values)
                XRect dealTypeValue = new XRect(370, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.DealTypeValue), fontH6Bold, XBrushes.Black, dealTypeValue, XStringFormats.TopLeft);
                XRect propertyValueValue = new XRect(370, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PropertyValueValue.ToString("N0")), fontH6, XBrushes.Black, propertyValueValue, XStringFormats.TopLeft);
                XRect loanAmountValue = new XRect(370, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.LoanAmountValue.ToString("N0")), fontH6, XBrushes.Black, loanAmountValue, XStringFormats.TopLeft);
                XRect downPaymentValue = new XRect(370, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.DownPaymentValue.ToString("N0")), fontH6, XBrushes.Black, downPaymentValue, XStringFormats.TopLeft);
                XRect monthlyPaymentValue = new XRect(370, 270, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.MonthlyPaymentValue.ToString("N0")), fontH6, XBrushes.Black, monthlyPaymentValue, XStringFormats.TopLeft);
                XRect dateOfEntranceValue = new XRect(370, 285, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.DateOfEntranceValue.ToString("MM/dd/yyyy")), fontH6, XBrushes.Black, dateOfEntranceValue, XStringFormats.TopLeft);
                XRect rentTodayValue = new XRect(370, 300, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.RentTodayValue.ToString()), fontH6, XBrushes.Black, rentTodayValue, XStringFormats.TopLeft);
            }

            // REFINANCE ANY
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                // Right side (Titles)
                XRect dealTypeTitle = new XRect(450, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.dealTypeTitle), fontH6Bold, XBrushes.Black, dealTypeTitle, XStringFormats.TopLeft);
                XRect propertyValueTitle = new XRect(450, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.propertyValueTitle), fontH6Bold, XBrushes.Black, propertyValueTitle, XStringFormats.TopLeft);
                XRect loanAmountTitle = new XRect(450, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.loanAmountTitle), fontH6Bold, XBrushes.Black, loanAmountTitle, XStringFormats.TopLeft);
                XRect monthlyPaymentTitle = new XRect(450, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.monthlyPaymentTitle), fontH6Bold, XBrushes.Black, monthlyPaymentTitle, XStringFormats.TopLeft);
                // Right side (Values)
                XRect dealTypeValue = new XRect(370, 210, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.DealTypeValue), fontH6Bold, XBrushes.Black, dealTypeValue, XStringFormats.TopLeft);
                XRect propertyValueValue = new XRect(370, 225, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PropertyValueValue.ToString("N0")), fontH6, XBrushes.Black, propertyValueValue, XStringFormats.TopLeft);
                XRect loanAmountValue = new XRect(370, 240, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.LoanAmountValue.ToString("N0")), fontH6, XBrushes.Black, loanAmountValue, XStringFormats.TopLeft);
                XRect monthlyPaymentValue = new XRect(370, 255, 111, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.MonthlyPaymentValue.ToString("N0")), fontH6, XBrushes.Black, monthlyPaymentValue, XStringFormats.TopLeft);
            }
            // TABLES
            // First Right table
             XPen tableLine1 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine1,
                new XPoint(450, 345),
                new XPoint(563, 345));
            gfx.DrawLine(tableLine1,
                new XPoint(450, 365),
                new XPoint(563, 365));
            // Titles
            tf.Alignment = SetAlignmentForCulture();
            XRect priorLiabilitiesTableTitle = new XRect(410, 332, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.priorLiabilitiesTableTitle), fontH6Bold, XBrushes.Black, priorLiabilitiesTableTitle, XStringFormats.TopLeft);
            XRect priorLiabilitiesTableEndDateTitle = new XRect(336, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.priorLiabilitiesTableEndDateTitle), fontH6Bold, XBrushes.Black, priorLiabilitiesTableEndDateTitle, XStringFormats.TopLeft);
            XRect priorLiabilitiesTableMonthlyReturnTitle = new XRect(410, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.priorLiabilitiesTableMonthlyReturnTitle), fontH6Bold, XBrushes.Black, priorLiabilitiesTableMonthlyReturnTitle, XStringFormats.TopLeft);
            int positionY1 = 365;
            // Values
            for (int i = 0; i < longReportDataObject.PriorLiabilitiesTableValues.Length; i++)
            {
                positionY1 += 20;
                gfx.DrawLine(tableLine1,
                new XPoint(450, positionY1),
                new XPoint(563, positionY1));
                XRect priorLiabilitiesTableMonthlyReturnValue = new XRect(410, positionY1 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PriorLiabilitiesTableValues[i].MonthlyReturn.ToString("N0")), fontH6, XBrushes.Black, priorLiabilitiesTableMonthlyReturnValue, XStringFormats.TopLeft);
                XRect priorLiabilitiesTableEndDateValues = new XRect(342, positionY1 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.PriorLiabilitiesTableValues[i].Date.ToString("MM/dd/yyyy")), fontH6, XBrushes.Black, priorLiabilitiesTableEndDateValues, XStringFormats.TopLeft);
            }
            //
            // SECOND TABLE CENTER
            //
            XPen tableLine2 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine2,
                new XPoint(290, 345),
                new XPoint(403, 345));
            gfx.DrawLine(tableLine2,
                new XPoint(290, 365),
                new XPoint(403, 365));
            // TITLES
            XRect futureReleasesTableTitle = new XRect(250, 332, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.futureReleasesTableTitle), fontH6Bold, XBrushes.Black, futureReleasesTableTitle, XStringFormats.TopLeft);
            XRect futureReleasesTableAmountTitle = new XRect(250, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.futureReleasesTableAmountTitle), fontH6Bold, XBrushes.Black, futureReleasesTableAmountTitle, XStringFormats.TopLeft);
            XRect futureReleasesTableDateTitle = new XRect(186, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.futureReleasesTableDateTitle), fontH6Bold, XBrushes.Black, futureReleasesTableDateTitle, XStringFormats.TopLeft);
            // VALUES
            int positionY2 = 365;
            for (int i = 0; i < longReportDataObject.FutureReleasesTableValues.Length; i++)
            {
                positionY2 += 20;
                gfx.DrawLine(tableLine2,
                new XPoint(290, positionY2),
                new XPoint(403, positionY2));
                XRect futureReleasesTableAmountValue = new XRect(250, positionY2 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FutureReleasesTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, futureReleasesTableAmountValue, XStringFormats.TopLeft);
                XRect futureReleasesTableDateValue = new XRect(183, positionY2 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.FutureReleasesTableValues[i].Date.ToString("MM/dd/yyyy")), fontH6, XBrushes.Black, futureReleasesTableDateValue, XStringFormats.TopLeft);
            }
            //
            // THIRD TABLE (LEFT)
            //
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
                new XPoint(40, 345),
                new XPoint(243, 345));
            gfx.DrawLine(tableLine3,
                new XPoint(40, 365),
                new XPoint(243, 365));
            // TITLES
            XRect fixedSavingsTableTitle = new XRect(90, 332, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.fixedSavingsTableTitle), fontH6Bold, XBrushes.Black, fixedSavingsTableTitle, XStringFormats.TopLeft);
            XRect fixedSavingsTableAmountTitle = new XRect(90, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.fixedSavingsTableAmountTitle), fontH6Bold, XBrushes.Black, fixedSavingsTableAmountTitle, XStringFormats.TopLeft);
            XRect fixedSavingsTableSavingTypeTitle = new XRect(40, 350, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.fixedSavingsTableSavingTypeTitle), fontH6Bold, XBrushes.Black, fixedSavingsTableSavingTypeTitle, XStringFormats.TopLeft);
            XRect fixedSavingsTableAverageYieldTitle = new XRect(0, 350, 140, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.fixedSavingsTableAverageYieldTitle), fontH6Bold, XBrushes.Black, fixedSavingsTableAverageYieldTitle, XStringFormats.TopLeft);
            XRect fixedSavingsTableLiquidTitle = new XRect(0, 350, 57, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.fixedSavingsTableLiquidTitle), fontH6Bold, XBrushes.Black, fixedSavingsTableLiquidTitle, XStringFormats.TopLeft);
            int positionY3 = 365;
            // VALUES
            for (int i = 0; i < longReportDataObject.FixedSavingsTableValues.Length; i++)
            {
                positionY3 += 20;
                gfx.DrawLine(tableLine3,
                    new XPoint(40, positionY3),
                    new XPoint(243, positionY3));
                XRect fixedSavingsTableAmountValue = new XRect(90, positionY3 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FixedSavingsTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, fixedSavingsTableAmountValue, XStringFormats.TopLeft);

                XRect fixedSavingsTableSavingTypeValue = new XRect(40, positionY3 - 13, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.FixedSavingsTableValues[i].SavingType.ToString()), fontH6, XBrushes.Black, fixedSavingsTableSavingTypeValue, XStringFormats.TopLeft);

                XRect fixedSavingsTableAverageYieldValue = new XRect(0, positionY3 - 13, 140, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.FixedSavingsTableValues[i].AverageYield.ToString("0.00")) + "%", fontH6, XBrushes.Black, fixedSavingsTableAverageYieldValue, XStringFormats.TopLeft);

                XRect fixedSavingsTableLiquidValue = new XRect(0, positionY3 - 13, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.FixedSavingsTableValues[i].LiquidString), fontH6, XBrushes.Black, fixedSavingsTableLiquidValue, XStringFormats.TopLeft);
            }
            //
            // SECTION 2.2
            //
            int heightSection22 = -85;
            XRect rect3 = new XRect(250, 530 + heightSection22, 310, 30);
            XPen underline3 = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline3,
                new XPoint(380, 547 + heightSection22),
                new XPoint(563, 547 + heightSection22));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionPersonalPreferences), fontH2, new XSolidBrush(colorGreenTitle), rect3, XStringFormats.TopLeft);
            // Right Title
            XRect planningPreferencesTitle = new XRect(250, 560 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.planningPreferencesTitle), fontH6Bold, XBrushes.Black, planningPreferencesTitle, XStringFormats.TopLeft);
            XRect liquidityPreferencesTitle = new XRect(250, 575 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.liquidityPreferencesTitle), fontH6Bold, XBrushes.Black, liquidityPreferencesTitle, XStringFormats.TopLeft);
            XRect changesPreferencesTitle = new XRect(250, 590 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.changesPreferencesTitle), fontH6Bold, XBrushes.Black, changesPreferencesTitle, XStringFormats.TopLeft);
            // Right Value
            XRect planningPreferencesValue = new XRect(170, 560 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.PlanningPreferencesValue.ToString()), fontH6, XBrushes.Black, planningPreferencesValue, XStringFormats.TopLeft);
            XRect liquidityPreferencesValue = new XRect(170, 575 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.LiquidityPreferencesValue.ToString()), fontH6, XBrushes.Black, liquidityPreferencesValue, XStringFormats.TopLeft);
            XRect changesPreferencesValue = new XRect(170, 590 + heightSection22, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.ChangesPreferencesValue.ToString()), fontH6, XBrushes.Black, changesPreferencesValue, XStringFormats.TopLeft);
            if(longReportDataObject.CurrentСhangesPreferences == LongReportDataObject.СhangesPreferencesScenario.ChangeInMonthlyPayment)
            {
                // Title(Optional bottom)
                XRect newPmtWhenText = new XRect(170, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.newPmtWhenText), fontH6Bold, XBrushes.Black, newPmtWhenText, XStringFormats.TopLeft);
                XRect newPmtYearsText = new XRect(110, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.newPmtYearsText), fontH6Bold, XBrushes.Black, newPmtYearsText, XStringFormats.TopLeft);
                // Value(Optional bottom)
                XRect newPmtYearsValue = new XRect(140, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.NewPmtYearsValue.ToString()), fontH6, XBrushes.Black, newPmtYearsValue, XStringFormats.TopLeft);
            }
            if(longReportDataObject.CurrentСhangesPreferences == LongReportDataObject.СhangesPreferencesScenario.EarlyRepayment)
            {
                // Title(Optional bottom)
                XRect repaymentWhenText = new XRect(170, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.newPmtWhenText), fontH6Bold, XBrushes.Black, repaymentWhenText, XStringFormats.TopLeft);
                XRect repaymentYearsText = new XRect(110, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.newPmtYearsText), fontH6Bold, XBrushes.Black, repaymentYearsText, XStringFormats.TopLeft);
                // Value(Optional bottom)
                XRect repaymentYearsValue = new XRect(140, 605 + heightSection22, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.RepaymentYearsValue.ToString()), fontH6, XBrushes.Black, repaymentYearsValue, XStringFormats.TopLeft);
            }
            
            // Need more description

            /* XRect newRefundExpectedTitle = new XRect(170, 620, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.newRefundExpectedTitle), fontH6Bold, XBrushes.Black, newRefundExpectedTitle, XStringFormats.TopLeft);
            XRect newRefundExpectedValue = new XRect(100, 620, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + "7.000", fontH6, XBrushes.Black, newRefundExpectedValue, XStringFormats.TopLeft); */

            // REFINANCE ONLY
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                //
                // SECTION 2.3
                //
                XRect sectionOriginalLoanText = new XRect(250, 547, 310, 30);
                XPen sectionOriginalLoanTextUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(sectionOriginalLoanTextUnderline,
                    new XPoint(410, 564),
                    new XPoint(563, 564));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.sectionOriginalLoan), fontH2, new XSolidBrush(colorGreenTitle), sectionOriginalLoanText, XStringFormats.TopLeft);
                if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers) ||
                    (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers))
                {
                    XRect originalLoanNotKnown = new XRect(230, 577, 310, 30);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.originalLoanNotKnown), fontH6, XBrushes.Black, originalLoanNotKnown, XStringFormats.TopLeft);
                }
                if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers) ||
                    (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers))
                {
                    // TABLE SECTION 2.3
                    int heightPosition3 = 360;
                    int currentLinePositionY = 260;
                    XRect offeringBankName = new XRect(360, heightPosition3 + 220, 200, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.offeringBankName), fontH6Bold, XBrushes.Black, offeringBankName, XStringFormats.TopLeft);
                    XRect offeringBankNameValue = new XRect(380, heightPosition3 + 220, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferingBankNameValue), fontH6, XBrushes.Black, offeringBankNameValue, XStringFormats.TopLeft);
                    XPen tableLine4 = new XPen(XColors.Black, 1);
                    gfx.DrawLine(tableLine4,
                        new XPoint(210, heightPosition3 + 245),
                        new XPoint(563, heightPosition3 + 245));
                    gfx.DrawLine(tableLine4,
                        new XPoint(210, heightPosition3 + 260),
                        new XPoint(563, heightPosition3 + 260));
                    XRect titleOriginalAmount = new XRect(391, heightPosition3 + 248, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleOriginalAmount), fontH6Bold, XBrushes.Black, titleOriginalAmount, XStringFormats.TopLeft);
                    XRect titleProductType = new XRect(340, heightPosition3 + 248, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleProductionType), fontH6Bold, XBrushes.Black, titleProductType, XStringFormats.TopLeft);
                    XRect titleOriginalTime = new XRect(275, heightPosition3 + 248, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleOriginalTime), fontH6Bold, XBrushes.Black, titleOriginalTime, XStringFormats.TopLeft);
                    XRect titleRemainingTime = new XRect(315, heightPosition3 + 248, 57, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleRemainingTime), fontH6Bold, XBrushes.Black, titleRemainingTime, XStringFormats.TopLeft);
                    XRect titleRateToday = new XRect(171, heightPosition3 + 248, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleRateToday), fontH6Bold, XBrushes.Black, titleRateToday, XStringFormats.TopLeft);
                    XRect titlePmtToday = new XRect(107, heightPosition3 + 248, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titlePmtToday), fontH6Bold, XBrushes.Black, titlePmtToday, XStringFormats.TopLeft);
                    for (int i = 0; i < longReportDataObject.SectionOriginalLoanKnownTableValues.Length; i++)
                    {
                        currentLinePositionY += 15;
                        gfx.DrawLine(tableLine4,
                            new XPoint(210, heightPosition3 + currentLinePositionY),
                            new XPoint(563, heightPosition3 + currentLinePositionY));
                        XRect titleOriginalAmountValue = new XRect(405, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].OriginalAmount.ToString("N0")), fontH6, XBrushes.Black, titleOriginalAmountValue, XStringFormats.TopLeft);
                        XRect productTypeValue = new XRect(340, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, productTypeValue, XStringFormats.TopLeft);
                        XRect originalTimeValue = new XRect(280, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].OriginalTime.ToString()), fontH6, XBrushes.Black, originalTimeValue, XStringFormats.TopLeft);
                        XRect remainingTimeValue = new XRect(215, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].RemainingTime.ToString()), fontH6, XBrushes.Black, remainingTimeValue, XStringFormats.TopLeft);
                        XRect rateTodayValue = new XRect(164, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].RateToday.ToString("0.000")) + "%", fontH6, XBrushes.Black, rateTodayValue, XStringFormats.TopLeft);
                        XRect pmtTodayValue = new XRect(105, heightPosition3 + currentLinePositionY - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SectionOriginalLoanKnownTableValues[i].PmtToday.ToString("N0")), fontH6, XBrushes.Black, pmtTodayValue, XStringFormats.TopLeft);
                    }
                    //right and left lines from top to bottom 
                    gfx.DrawLine(tableLine4,
                        new XPoint(563, heightPosition3 + 245),
                        new XPoint(563, heightPosition3 + currentLinePositionY));
                    gfx.DrawLine(tableLine4,
                        new XPoint(508, heightPosition3 + currentLinePositionY),
                        new XPoint(508, heightPosition3 + 245));
                    gfx.DrawLine(tableLine4,
                        new XPoint(210, heightPosition3 + currentLinePositionY),
                        new XPoint(210, heightPosition3 + 245));
                    // Background for total Pmt
                    XRect totalPmtTodayBackground = new XRect(210, heightPosition3 + currentLinePositionY + 15, 55, 15);
                    gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), totalPmtTodayBackground);
                    // Total Pmt
                    gfx.DrawLine(tableLine4,
                       new XPoint(210, heightPosition3 + currentLinePositionY + 15),
                       new XPoint(330, heightPosition3 + currentLinePositionY + 15));
                    gfx.DrawLine(tableLine4,
                       new XPoint(210, heightPosition3 + currentLinePositionY + 30),
                       new XPoint(330, heightPosition3 + currentLinePositionY + 30));
                    XRect titleTotalPmtToday = new XRect(175, heightPosition3 + currentLinePositionY + 18, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleTotalPmtToday), fontH6Bold, XBrushes.Black, titleTotalPmtToday, XStringFormats.TopLeft);
                    XRect titleTotalPmtTodayValue = new XRect(150, heightPosition3 + currentLinePositionY + 18, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPmtToday.ToString("N0")), fontH6Bold, XBrushes.Black, titleTotalPmtTodayValue, XStringFormats.TopLeft);
                    // Background for total amount
                    XRect totalOriginalAmountBackground = new XRect(435, heightPosition3 + currentLinePositionY + 15, 55, 15);
                    gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), totalOriginalAmountBackground);
                    // Total Amount
                    gfx.DrawLine(tableLine4,
                       new XPoint(435, heightPosition3 + currentLinePositionY + 15),
                       new XPoint(563, heightPosition3 + currentLinePositionY + 15));
                    gfx.DrawLine(tableLine4,
                       new XPoint(435, heightPosition3 + currentLinePositionY + 30),
                       new XPoint(563, heightPosition3 + currentLinePositionY + 30));
                    XRect titleTotalOriginalAmount = new XRect(410, heightPosition3 + currentLinePositionY + 18, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.titleTotalOriginalAmount), fontH6Bold, XBrushes.Black, titleTotalOriginalAmount, XStringFormats.TopLeft);
                    XRect totalOriginalAmountValue = new XRect(380, heightPosition3 + currentLinePositionY + 18, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalOriginalAmountValue, XStringFormats.TopLeft);
                }
            }
        }

        /// <summary>
        /// Creates the Part 2: Order summary(Refinance).
        /// </summary>
        /// <remarks>
        /// Page is NOT constant.
        /// </remarks>
        public void CreateOrderSummaryPage2(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // SECTION 2.3 PRINCIPAL APPROVAL
            XRect sectionPrincipalApproval = new XRect(250, 100, 310, 30);
            XPen sectionPrincipalApprovalUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionPrincipalApprovalUnderline,
                new XPoint(250, 115),
                new XPoint(563, 115));
            tf.Alignment = SetAlignmentForCulture();
            XRect approvingBankName = new XRect(450, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.approvingBankName), fontH6Bold, XBrushes.Black, approvingBankName, XStringFormats.TopLeft);
            XRect approvingBankNameValue1 = new XRect(350, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.ApprovingBankNameValue[0]), fontH6, XBrushes.Black, approvingBankNameValue1, XStringFormats.TopLeft);
            XRect approvingBankNameValue2 = new XRect(300, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL("," + longReportDataObject.ApprovingBankNameValue[1]), fontH6, XBrushes.Black, approvingBankNameValue2, XStringFormats.TopLeft);
            XRect mainAccountName = new XRect(450, 150, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.mainAccountName), fontH6Bold, XBrushes.Black, mainAccountName, XStringFormats.TopLeft);
            XRect mainAccountNameValue = new XRect(350, 150, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.mainAccountNameValue), fontH6, XBrushes.Black, mainAccountNameValue, XStringFormats.TopLeft);
            XRect secondaryAccountName = new XRect(450, 165, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.secondaryAccountName), fontH6Bold, XBrushes.Black, secondaryAccountName, XStringFormats.TopLeft);
            XRect secondaryAccountNameValue = new XRect(350, 165, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.secondaryAccountNameValue), fontH6, XBrushes.Black, secondaryAccountNameValue, XStringFormats.TopLeft);
            // SECTION 2.4 (or 2.5)
            XRect sectionPriceOffers = new XRect(250, 195, 310, 30);
            XPen sectionPriceOffersUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionPriceOffersUnderline,
                new XPoint(425, 210),
                new XPoint(563, 210));
            tf.Alignment = SetAlignmentForCulture();
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseNew_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.PurchaseUsed_PriceOffers))
            {
                // Numbering 2.3 - 2.4
                tf.DrawString(CheckRTL(Properties.Resources.sectionPrincipalApproval), fontH2, new XSolidBrush(colorGreenTitle), sectionPrincipalApproval, XStringFormats.TopLeft);
                tf.DrawString(CheckRTL(Properties.Resources.sectionPriceOffers), fontH2, new XSolidBrush(colorGreenTitle), sectionPriceOffers, XStringFormats.TopLeft);
            }
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_NoLoanInserted_PriceOffers))
            {
                // Numbering 2.4 - 2.5
                tf.DrawString(CheckRTL(Properties.Resources.sectionPrincipalApproval2), fontH2, new XSolidBrush(colorGreenTitle), sectionPrincipalApproval, XStringFormats.TopLeft);
                tf.DrawString(CheckRTL(Properties.Resources.sectionPriceOffers2), fontH2, new XSolidBrush(colorGreenTitle), sectionPriceOffers, XStringFormats.TopLeft);
            }
            //
            // TABLE
            //

            //
            // Case when we have 1 offer
            if (longReportDataObject.NumberPriceOffers == 1)
            {
                int heightPosition3 = -10;
                XPen tableLine3 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(563, 245 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition3),
                    new XPoint(563, 260 + heightPosition3));
                // TITLES
                XRect titleOfferBankName = new XRect(410, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName, XStringFormats.TopLeft);
                XRect titleOfferAmount = new XRect(392, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount, XStringFormats.TopLeft);
                XRect titleOfferProductType = new XRect(340, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType, XStringFormats.TopLeft);
                XRect titleOfferRate = new XRect(230, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate, XStringFormats.TopLeft);
                XRect titleOfferTime = new XRect(270, 248 + heightPosition3, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime, XStringFormats.TopLeft);
                XRect titleOfferPmt = new XRect(134, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt, XStringFormats.TopLeft);
                XRect titleOfferBankValue = new XRect(390, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue), fontH6, XBrushes.Black, titleOfferBankValue, XStringFormats.TopLeft);

                int positionY3 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues.Length; i++)
                {
                    positionY3 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY3 + heightPosition3),
                        new XPoint(563, positionY3 + heightPosition3));

                    XRect titleOfferAmountValues = new XRect(405, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground = new XRect(508, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground);
                XRect titleOfferPmtBackground = new XRect(240, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground);
                XRect titleOfferTotalFuturePaymentValueBackground = new XRect(30, positionY3 + 0.5 + heightPosition3, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(240, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition3),
                    new XPoint(508, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY3 + 15 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY3 + 15 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + heightPosition3),
                   new XPoint(180, positionY3 + heightPosition3));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + 15 + heightPosition3),
                   new XPoint(180, positionY3 + 15 + heightPosition3));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue = new XRect(405, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue = new XRect(130, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue = new XRect(27, positionY3 + 15 - 12 + heightPosition3, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment = new XRect(70, positionY3 + 15 - 12 + heightPosition3, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment, XStringFormats.TopLeft);
            }

            //
            // Case when we have 2 offers
            if (longReportDataObject.NumberPriceOffers == 2)
            {
                //
                //FIRST TABLE
                int heightPosition3 = -10;
                XPen tableLine3 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(563, 245 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition3),
                    new XPoint(563, 260 + heightPosition3));
                // TITLES
                XRect titleOfferBankName = new XRect(410, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName, XStringFormats.TopLeft);
                XRect titleOfferAmount = new XRect(392, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount, XStringFormats.TopLeft);
                XRect titleOfferProductType = new XRect(340, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType, XStringFormats.TopLeft);
                XRect titleOfferRate = new XRect(230, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate, XStringFormats.TopLeft);
                XRect titleOfferTime = new XRect(270, 248 + heightPosition3, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime, XStringFormats.TopLeft);
                XRect titleOfferPmt = new XRect(134, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt, XStringFormats.TopLeft);
                XRect titleOfferBankValue = new XRect(390, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue), fontH6, XBrushes.Black, titleOfferBankValue, XStringFormats.TopLeft);

                int positionY3 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues.Length; i++)
                {
                    positionY3 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY3 + heightPosition3),
                        new XPoint(563, positionY3 + heightPosition3));

                    XRect titleOfferAmountValues = new XRect(405, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground = new XRect(508, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground);
                XRect titleOfferPmtBackground = new XRect(240, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground);
                XRect titleOfferTotalFuturePaymentValueBackground = new XRect(30, positionY3 + 0.5 + heightPosition3, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(240, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition3),
                    new XPoint(508, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY3 + 15 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY3 + 15 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + heightPosition3),
                   new XPoint(180, positionY3 + heightPosition3));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + 15 + heightPosition3),
                   new XPoint(180, positionY3 + 15 + heightPosition3));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue = new XRect(405, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue = new XRect(130, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue = new XRect(27, positionY3 + 15 - 12 + heightPosition3, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment = new XRect(70, positionY3 + 15 - 12 + heightPosition3, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment, XStringFormats.TopLeft);

                //
                // SECOND TABLE
                int heightPosition4 = 180;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition4),
                    new XPoint(563, 245 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition4),
                    new XPoint(563, 260 + heightPosition4));
                // TITLES
                XRect titleOfferBankName2 = new XRect(410, 233 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName2, XStringFormats.TopLeft);
                XRect titleOfferAmount2 = new XRect(392, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount2, XStringFormats.TopLeft);
                XRect titleOfferProductType2 = new XRect(340, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType2, XStringFormats.TopLeft);
                XRect titleOfferRate2 = new XRect(230, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate2, XStringFormats.TopLeft);
                XRect titleOfferTime2 = new XRect(270, 248 + heightPosition4, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime2, XStringFormats.TopLeft);
                XRect titleOfferPmt2 = new XRect(134, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt2, XStringFormats.TopLeft);
                XRect titleOfferBankValue2 = new XRect(390, 233 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue2), fontH6, XBrushes.Black, titleOfferBankValue2, XStringFormats.TopLeft);

                int positionY4 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues2.Length; i++)
                {
                    positionY4 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY4 + heightPosition4),
                        new XPoint(563, positionY4 + heightPosition4));

                    XRect titleOfferAmountValues = new XRect(405, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground2 = new XRect(508, positionY4 + 0.5 + heightPosition4, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground2);
                XRect titleOfferPmtBackground2 = new XRect(240, positionY4 + 0.5 + heightPosition4, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground2);
                XRect titleOfferTotalFuturePaymentValueBackground2 = new XRect(30, positionY4 + 0.5 + heightPosition4, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground2);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition4),
                    new XPoint(563, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition4),
                    new XPoint(240, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition4),
                    new XPoint(508, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition4),
                    new XPoint(295, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY4 + 15 + heightPosition4),
                    new XPoint(295, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY4 + 15 + heightPosition4),
                    new XPoint(563, positionY4 + 15 + heightPosition4));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY4 + heightPosition4),
                   new XPoint(180, positionY4 + heightPosition4));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY4 + 15 + heightPosition4),
                   new XPoint(180, positionY4 + 15 + heightPosition4));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue2 = new XRect(405, positionY4 + 15 - 12 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue2.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue2, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue2 = new XRect(130, positionY4 + 15 - 12 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue2.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue2, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue2 = new XRect(27, positionY4 + 15 - 12 + heightPosition4, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue2.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue2, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment2 = new XRect(70, positionY4 + 15 - 12 + heightPosition4, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment2, XStringFormats.TopLeft);
            }

            // Case when we have 3 offers
            if (longReportDataObject.NumberPriceOffers == 3)
            {
                // FIRST TABLE
                int heightPosition3 = -10;
                XPen tableLine3 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(563, 245 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition3),
                    new XPoint(563, 260 + heightPosition3));
                // TITLES
                XRect titleOfferBankName = new XRect(410, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName, XStringFormats.TopLeft);
                XRect titleOfferAmount = new XRect(392, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount, XStringFormats.TopLeft);
                XRect titleOfferProductType = new XRect(340, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType, XStringFormats.TopLeft);
                XRect titleOfferRate = new XRect(230, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate, XStringFormats.TopLeft);
                XRect titleOfferTime = new XRect(270, 248 + heightPosition3, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime, XStringFormats.TopLeft);
                XRect titleOfferPmt = new XRect(134, 248 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt, XStringFormats.TopLeft);
                XRect titleOfferBankValue = new XRect(390, 233 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue), fontH6, XBrushes.Black, titleOfferBankValue, XStringFormats.TopLeft);

                int positionY3 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues.Length; i++)
                {
                    positionY3 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY3 + heightPosition3),
                        new XPoint(563, positionY3 + heightPosition3));

                    XRect titleOfferAmountValues = new XRect(405, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY3 - 12 + heightPosition3, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground = new XRect(508, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground);
                XRect titleOfferPmtBackground = new XRect(240, positionY3 + 0.5 + heightPosition3, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground);
                XRect titleOfferTotalFuturePaymentValueBackground = new XRect(30, positionY3 + 0.5 + heightPosition3, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition3),
                    new XPoint(240, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition3),
                    new XPoint(508, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY3 + 15 + heightPosition3),
                    new XPoint(295, positionY3 + 15 + heightPosition3));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY3 + 15 + heightPosition3),
                    new XPoint(563, positionY3 + 15 + heightPosition3));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + heightPosition3),
                   new XPoint(180, positionY3 + heightPosition3));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY3 + 15 + heightPosition3),
                   new XPoint(180, positionY3 + 15 + heightPosition3));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue = new XRect(405, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue = new XRect(130, positionY3 + 15 - 12 + heightPosition3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue = new XRect(27, positionY3 + 15 - 12 + heightPosition3, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment = new XRect(70, positionY3 + 15 - 12 + heightPosition3, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment, XStringFormats.TopLeft);

                //
                // SECOND TABLE
                int heightPosition4 = 180;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition4),
                    new XPoint(563, 245 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition4),
                    new XPoint(563, 260 + heightPosition4));
                // TITLES
                XRect titleOfferBankName2 = new XRect(410, 233 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName2, XStringFormats.TopLeft);
                XRect titleOfferAmount2 = new XRect(392, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount2, XStringFormats.TopLeft);
                XRect titleOfferProductType2 = new XRect(340, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType2, XStringFormats.TopLeft);
                XRect titleOfferRate2 = new XRect(230, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate2, XStringFormats.TopLeft);
                XRect titleOfferTime2 = new XRect(270, 248 + heightPosition4, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime2, XStringFormats.TopLeft);
                XRect titleOfferPmt2 = new XRect(134, 248 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt2, XStringFormats.TopLeft);
                XRect titleOfferBankValue2 = new XRect(390, 233 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue2), fontH6, XBrushes.Black, titleOfferBankValue2, XStringFormats.TopLeft);

                int positionY4 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues2.Length; i++)
                {
                    positionY4 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY4 + heightPosition4),
                        new XPoint(563, positionY4 + heightPosition4));

                    XRect titleOfferAmountValues = new XRect(405, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY4 - 12 + heightPosition4, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues2[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground2 = new XRect(508, positionY4 + 0.5 + heightPosition4, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground2);
                XRect titleOfferPmtBackground2 = new XRect(240, positionY4 + 0.5 + heightPosition4, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground2);
                XRect titleOfferTotalFuturePaymentValueBackground2 = new XRect(30, positionY4 + 0.5 + heightPosition4, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground2);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition4),
                    new XPoint(563, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition4),
                    new XPoint(240, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition4),
                    new XPoint(508, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition4),
                    new XPoint(295, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY4 + 15 + heightPosition4),
                    new XPoint(295, positionY4 + 15 + heightPosition4));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY4 + 15 + heightPosition4),
                    new XPoint(563, positionY4 + 15 + heightPosition4));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY4 + heightPosition4),
                   new XPoint(180, positionY4 + heightPosition4));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY4 + 15 + heightPosition4),
                   new XPoint(180, positionY4 + 15 + heightPosition4));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue2 = new XRect(405, positionY4 + 15 - 12 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue2.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue2, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue2 = new XRect(130, positionY4 + 15 - 12 + heightPosition4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue2.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue2, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue2 = new XRect(27, positionY4 + 15 - 12 + heightPosition4, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue2.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue2, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment2 = new XRect(70, positionY4 + 15 - 12 + heightPosition4, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment2, XStringFormats.TopLeft);

                //
                // THIRD TABLE
                int heightPosition5 = 370;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition5),
                    new XPoint(563, 245 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 260 + heightPosition5),
                    new XPoint(563, 260 + heightPosition5));
                // TITLES
                XRect titleOfferBankName3 = new XRect(410, 233 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferBankName), fontH6Bold, XBrushes.Black, titleOfferBankName3, XStringFormats.TopLeft);
                XRect titleOfferAmount3 = new XRect(392, 248 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferAmount), fontH6Bold, XBrushes.Black, titleOfferAmount3, XStringFormats.TopLeft);
                XRect titleOfferProductType3 = new XRect(340, 248 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferProductType), fontH6Bold, XBrushes.Black, titleOfferProductType3, XStringFormats.TopLeft);
                XRect titleOfferRate3 = new XRect(230, 248 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferRate), fontH6Bold, XBrushes.Black, titleOfferRate3, XStringFormats.TopLeft);
                XRect titleOfferTime3 = new XRect(270, 248 + heightPosition5, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTime), fontH6Bold, XBrushes.Black, titleOfferTime3, XStringFormats.TopLeft);
                XRect titleOfferPmt3 = new XRect(134, 248 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferPmt), fontH6Bold, XBrushes.Black, titleOfferPmt3, XStringFormats.TopLeft);
                XRect titleOfferBankValue3 = new XRect(390, 233 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.TitleOfferBankValue3), fontH6, XBrushes.Black, titleOfferBankValue3, XStringFormats.TopLeft);

                int positionY5 = 260;
                for (int i = 0; i < longReportDataObject.OfferBankTableValues3.Length; i++)
                {
                    positionY5 += 15;
                    gfx.DrawLine(tableLine3,
                        new XPoint(240, positionY5 + heightPosition5),
                        new XPoint(563, positionY5 + heightPosition5));

                    XRect titleOfferAmountValues = new XRect(405, positionY5 - 12 + heightPosition5, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountValues, XStringFormats.TopLeft);
                    XRect titleOfferProductTypeValue = new XRect(340, positionY5 - 12 + heightPosition5, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues3[i].ProductType.ToString()), fontH6, XBrushes.Black, titleOfferProductTypeValue, XStringFormats.TopLeft);
                    XRect titleOfferRateValue = new XRect(235, positionY5 - 12 + heightPosition5, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues3[i].OfferRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, titleOfferRateValue, XStringFormats.TopLeft);
                    XRect titleOfferTimeValue = new XRect(170, positionY5 - 12 + heightPosition5, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OfferBankTableValues3[i].OfferTime.ToString()), fontH6, XBrushes.Black, titleOfferTimeValue, XStringFormats.TopLeft);
                    XRect titleOfferPmtValue = new XRect(130, positionY5 - 12 + heightPosition5, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankTableValues3[i].OfferPmt.ToString("N0")), fontH6, XBrushes.Black, titleOfferPmtValue, XStringFormats.TopLeft);
                }
                // Color background
                XRect titleOfferAmountTotalValueBackground3 = new XRect(508, positionY5 + 0.5 + heightPosition5, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleOfferAmountTotalValueBackground3);
                XRect titleOfferPmtBackground3 = new XRect(240, positionY5 + 0.5 + heightPosition5, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferPmtBackground3);
                XRect titleOfferTotalFuturePaymentValueBackground3 = new XRect(30, positionY5 + 0.5 + heightPosition5, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleOfferTotalFuturePaymentValueBackground3);

                gfx.DrawLine(tableLine3,
                    new XPoint(563, 245 + heightPosition5),
                    new XPoint(563, positionY5 + 15 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, 245 + heightPosition5),
                    new XPoint(240, positionY5 + 15 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, 245 + heightPosition5),
                    new XPoint(508, positionY5 + 15 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, 245 + heightPosition5),
                    new XPoint(295, positionY5 + 15 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, positionY5 + 15 + heightPosition5),
                    new XPoint(295, positionY5 + 15 + heightPosition5));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, positionY5 + 15 + heightPosition5),
                    new XPoint(563, positionY5 + 15 + heightPosition5));
                // Left row next to the table
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY5 + heightPosition5),
                   new XPoint(180, positionY5 + heightPosition5));
                gfx.DrawLine(tableLine3,
                   new XPoint(30, positionY5 + 15 + heightPosition5),
                   new XPoint(180, positionY5 + 15 + heightPosition5));
                // VALUES TOTAL (Highlighted Value)
                XRect titleOfferAmountTotalValue3 = new XRect(405, positionY5 + 15 - 12 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferBankAmountTotalValue3.ToString("N0")), fontH6, XBrushes.Black, titleOfferAmountTotalValue3, XStringFormats.TopLeft);
                XRect TitleOfferPmtTotalValue3 = new XRect(130, positionY5 + 15 - 12 + heightPosition5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TitleOfferPmtTotalValue3.ToString("N0")), fontH6Bold, XBrushes.Black, TitleOfferPmtTotalValue3, XStringFormats.TopLeft);
                // Left row next to the table (TITLES and VALUES)
                XRect titleOfferTotalFuturePaymentValue3 = new XRect(27, positionY5 + 15 - 12 + heightPosition5, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalFuturePaymentValue3.ToString("N0")), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePaymentValue3, XStringFormats.TopLeft);
                XRect titleOfferTotalFuturePayment3 = new XRect(70, positionY5 + 15 - 12 + heightPosition5, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleOfferTotalFuturePayment3, XStringFormats.TopLeft);
            }
            
        }

        /// <summary>
        /// Creates the Part 2B: Refinance analysis of existing loan
        /// </summary>
        /// <remarks>
        /// Page is NOT constant.
        /// </remarks>
        public bool CreateRefinanceAnalysisPage2B(int number)
        {
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers))
            {
                // Create an empty page and set view
                page = document.AddPage();
                page.Size = pageSizeA4;
                page.Orientation = pageOrientationPortrait;
                // Get an XGraphics object for drawing
                gfx = XGraphics.FromPdfPage(page);
                tf = new XTextFormatter(gfx);
                SetInitialViewOfPage(number);

                // SECTION 2.6 ORIGINAL LOAN ANALYSIS
                XRect sectionOriginalLoanAnalysis = new XRect(250, 100, 310, 30);
                XPen sectionOriginalLoanAnalysisUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(sectionOriginalLoanAnalysisUnderline,
                    new XPoint(422, 115),
                    new XPoint(563, 115));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.sectionOriginalLoanAnalysis), fontH2, new XSolidBrush(colorGreenTitle), sectionOriginalLoanAnalysis, XStringFormats.TopLeft);

                XRect titleOriginalLoanDetails = new XRect(250, 130, 310, 30);
                XPen titleOriginalLoanDetailsUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleOriginalLoanDetailsUnderline,
                    new XPoint(443, 145),
                    new XPoint(563, 145));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalLoanDetails), fontH2, new XSolidBrush(colorGreenTitle), titleOriginalLoanDetails, XStringFormats.TopLeft);
                // TABLE SECTION
                int heightPosition3 = -60;
                int currentLinePositionY = 260;
                XRect originalBankName = new XRect(360, heightPosition3 + 220, 200, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.originalBankName), fontH6Bold, XBrushes.Black, originalBankName, XStringFormats.TopLeft);
                XRect originalBankNameValue = new XRect(380, heightPosition3 + 220, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.OfferingBankNameValue), fontH6, XBrushes.Black, originalBankNameValue, XStringFormats.TopLeft);
                XPen tableLine4 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + 260),
                    new XPoint(563, heightPosition3 + 260));
                XRect titleOriginalAmount = new XRect(391, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalAmount), fontH6Bold, XBrushes.Black, titleOriginalAmount, XStringFormats.TopLeft);
                XRect titleProductType = new XRect(340, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleProductionType), fontH6Bold, XBrushes.Black, titleProductType, XStringFormats.TopLeft);
                XRect titleDateTaken = new XRect(275, heightPosition3 + 248, 150, 20); 
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleDateTaken), fontH6Bold, XBrushes.Black, titleDateTaken, XStringFormats.TopLeft);
                XRect titleOriginalTime = new XRect(315, heightPosition3 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalTime), fontH6Bold, XBrushes.Black, titleOriginalTime, XStringFormats.TopLeft);
                XRect titleOriginalRate = new XRect(171, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalRate), fontH6Bold, XBrushes.Black, titleOriginalRate, XStringFormats.TopLeft);
                XRect titleFirstPmt = new XRect(107, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstPmt), fontH6Bold, XBrushes.Black, titleFirstPmt, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine4,
                   new XPoint(30, heightPosition3 + 245),
                   new XPoint(140, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(30, heightPosition3 + 260),
                    new XPoint(140, heightPosition3 + 260));
                XRect titleTimeLeft = new XRect(30, heightPosition3 + 248, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTimeLeft), fontH6Bold, XBrushes.Black, titleTimeLeft, XStringFormats.TopLeft);
                XRect titleRateToday = new XRect(-30, heightPosition3 + 248, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRateToday), fontH6Bold, XBrushes.Black, titleRateToday, XStringFormats.TopLeft);
                for (int i = 0; i < longReportDataObject.OriginalLoanDetailsTableValues.Length; i++)
                {
                    currentLinePositionY += 15;
                    gfx.DrawLine(tableLine4,
                        new XPoint(210, heightPosition3 + currentLinePositionY),
                        new XPoint(563, heightPosition3 + currentLinePositionY));
                    gfx.DrawLine(tableLine4,
                        new XPoint(30, heightPosition3 + currentLinePositionY),
                        new XPoint(140, heightPosition3 + currentLinePositionY));
                    XRect originalAmountValue = new XRect(405, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalAmount.ToString("N0")), fontH6, XBrushes.Black, originalAmountValue, XStringFormats.TopLeft);
                    XRect productTypeValue = new XRect(340, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalProductType.ToString()), fontH6, XBrushes.Black, productTypeValue, XStringFormats.TopLeft);
                    XRect dateTakenValue = new XRect(215, heightPosition3 + currentLinePositionY - 12, 150, 20); 
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].DateTaken.ToString("MM/yyyy")), fontH6, XBrushes.Black, dateTakenValue, XStringFormats.TopLeft);
                    XRect originalTimeValue = new XRect(280, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalTime.ToString()), fontH6, XBrushes.Black, originalTimeValue, XStringFormats.TopLeft);
                    XRect originalRateValue = new XRect(164, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalRate.ToString("0.000")) + "%", fontH6, XBrushes.Black, originalRateValue, XStringFormats.TopLeft);
                    XRect firstPmtValue = new XRect(105, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].FirstPmt.ToString("N0")), fontH6, XBrushes.Black, firstPmtValue, XStringFormats.TopLeft);

                    XRect titleTimeLeftValue = new XRect(30, heightPosition3 + currentLinePositionY - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].TimeLeft.ToString()), fontH6, XBrushes.Black, titleTimeLeftValue, XStringFormats.TopLeft);
                    XRect titleRateTodayValue = new XRect(-35, heightPosition3 + currentLinePositionY - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].RateToday.ToString("0.000")) + "%", fontH6, XBrushes.Black, titleRateTodayValue, XStringFormats.TopLeft);
                }
                //right and left lines from top to bottom 
                gfx.DrawLine(tableLine4,
                    new XPoint(563, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + currentLinePositionY));
                gfx.DrawLine(tableLine4,
                    new XPoint(508, heightPosition3 + currentLinePositionY),
                    new XPoint(508, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + currentLinePositionY),
                    new XPoint(210, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(30, heightPosition3 + currentLinePositionY),
                    new XPoint(30, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(140, heightPosition3 + currentLinePositionY),
                    new XPoint(140, heightPosition3 + 245));
                // Total Pmt
                gfx.DrawLine(tableLine4,
                   new XPoint(210, heightPosition3 + currentLinePositionY + 15),
                   new XPoint(330, heightPosition3 + currentLinePositionY + 15));
                gfx.DrawLine(tableLine4,
                   new XPoint(210, heightPosition3 + currentLinePositionY + 30),
                   new XPoint(330, heightPosition3 + currentLinePositionY + 30));
                XRect titleTotalOriginalPmt = new XRect(175, heightPosition3 + currentLinePositionY + 18, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalOriginalPmt), fontH6Bold, XBrushes.Black, titleTotalOriginalPmt, XStringFormats.TopLeft);
                XRect titleTotalOriginalPmtValue = new XRect(150, heightPosition3 + currentLinePositionY + 18, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalPmt2B.ToString("N0")), fontH6Bold, XBrushes.Black, titleTotalOriginalPmtValue, XStringFormats.TopLeft);
                // Total Amount
                gfx.DrawLine(tableLine4,
                   new XPoint(435, heightPosition3 + currentLinePositionY + 15),
                   new XPoint(563, heightPosition3 + currentLinePositionY + 15));
                gfx.DrawLine(tableLine4,
                   new XPoint(435, heightPosition3 + currentLinePositionY + 30),
                   new XPoint(563, heightPosition3 + currentLinePositionY + 30));
                XRect titleTotalOriginalAmount2B = new XRect(410, heightPosition3 + currentLinePositionY + 18, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalOriginalAmount), fontH6Bold, XBrushes.Black, titleTotalOriginalAmount2B, XStringFormats.TopLeft);
                XRect totalOriginalAmount2BValue = new XRect(380, heightPosition3 + currentLinePositionY + 18, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalOriginalAmount2BValue, XStringFormats.TopLeft);
                WisorGraph wisorGraphRight = new WisorGraph(longReportDataObject.SingleStructureGraphs2, longReportDataObject.SingleStructureGraphs, gfx, 30, 365, 300, 200);
                wisorGraphRight.CreateGraph(CheckRTL(longReportDataObject.SingleStructureGraphsTitleRight));
                WisorGraph wisorGraphRight2 = new WisorGraph(longReportDataObject.SingleStructureGraphs, longReportDataObject.SingleStructureGraphs2, gfx, 30, 570, 300, 200);
                wisorGraphRight2.CreateGraph(CheckRTL(longReportDataObject.SingleStructureGraphsTitleRight));
                // PAYMENT SO FAR
                XRect titleFromLoanTakenUntilToday = new XRect(250, 365, 310, 30);
                XPen titleFromLoanTakenUntilTodayUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleFromLoanTakenUntilTodayUnderline,
                    new XPoint(410, 380),
                    new XPoint(563, 380));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFromLoanTakenUntilToday), fontH2, new XSolidBrush(colorGreenTitle), titleFromLoanTakenUntilToday, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine4,
                   new XPoint(435, 395),
                   new XPoint(563, 395));
                gfx.DrawLine(tableLine4,
                   new XPoint(435, 410),
                   new XPoint(563, 410));
                XRect totalPaymentSoFar = new XRect(410, 400, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentSoFar), fontH6Bold, XBrushes.Black, totalPaymentSoFar, XStringFormats.TopLeft);
                XRect totalPaymentSoFarValue = new XRect(380, 400, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentSoFarValue, XStringFormats.TopLeft);
                XRect totalInterestPaidSoFar = new XRect(410, 420, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalInterestPaidSoFar), fontH6Bold, XBrushes.Black, totalInterestPaidSoFar, XStringFormats.TopLeft);
                XRect totalInterestPaidSoFarValue = new XRect(360, 420, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalInterestPaidSoFarValue, XStringFormats.TopLeft);
                XRect totalPrincipalPaidSoFar = new XRect(410, 434, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPrincipalPaidSoFar), fontH6Bold, XBrushes.Black, totalPrincipalPaidSoFar, XStringFormats.TopLeft);
                XRect totalPrincipalPaidSoFarValue = new XRect(350, 434, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalPrincipalPaidSoFarValue, XStringFormats.TopLeft);
                XRect totalPmtToday = new XRect(410, 448, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPmtToday), fontH6Bold, XBrushes.Black, totalPmtToday, XStringFormats.TopLeft);
                XRect totalPmtTodayValue = new XRect(380, 448, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalPmtTodayValue, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine4,
                   new XPoint(425, 468),
                   new XPoint(563, 468));
                gfx.DrawLine(tableLine4,
                   new XPoint(425, 483),
                   new XPoint(563, 483));
                XRect titleTotalRemainingAmount = new XRect(410, 473, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalRemainingAmount), fontH6Bold, XBrushes.Black, titleTotalRemainingAmount, XStringFormats.TopLeft);
                XRect totalRemainingAmount = new XRect(360, 473, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalRemainingAmount, XStringFormats.TopLeft);
                // FUTURE EXPECTED PAYMENT
                XRect titleFutureExpectedPayment = new XRect(250, 503, 310, 30);
                XPen titleFutureExpectedPaymentUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleFutureExpectedPaymentUnderline,
                    new XPoint(405, 518),
                    new XPoint(563, 518));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFutureExpectedPayment), fontH2, new XSolidBrush(colorGreenTitle), titleFutureExpectedPayment, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine4,
                  new XPoint(435, 533),
                  new XPoint(563, 533));
                gfx.DrawLine(tableLine4,
                   new XPoint(435, 548),
                   new XPoint(563, 548));
                XRect titleTotalFuturePayment = new XRect(410, 538, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferTotalFuturePayment), fontH6Bold, XBrushes.Black, titleTotalFuturePayment, XStringFormats.TopLeft);
                XRect totalFuturePayment = new XRect(380, 538, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalFuturePayment, XStringFormats.TopLeft);
                XRect totalFutureInterestPayment = new XRect(410, 558, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.totalFutureInterestPayment), fontH6Bold, XBrushes.Black, totalFutureInterestPayment, XStringFormats.TopLeft);
                XRect totalFutureInterestPaymentValue = new XRect(360, 558, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalFutureInterestPaymentValue, XStringFormats.TopLeft);
                XRect totalFuturePrincipalPayment = new XRect(410, 573, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.totalFuturePrincipalPayment), fontH6Bold, XBrushes.Black, totalFuturePrincipalPayment, XStringFormats.TopLeft);
                XRect totalFuturePrincipalPaymentValue = new XRect(350, 573, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalFuturePrincipalPaymentValue, XStringFormats.TopLeft);
                // TOTAL EXPECTED PAYMENT
                XRect titleTotalExpectedPayment = new XRect(250, 603, 310, 30);
                XPen titleTotalExpectedPaymentUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleTotalExpectedPaymentUnderline,
                    new XPoint(435, 618),
                    new XPoint(563, 618));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalExpectedPayment), fontH2, new XSolidBrush(colorGreenTitle), titleTotalExpectedPayment, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine4,
                  new XPoint(435, 633),
                  new XPoint(563, 633));
                gfx.DrawLine(tableLine4,
                   new XPoint(435, 648),
                   new XPoint(563, 648));
                XRect titleTotalPayment = new XRect(410, 638, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPayment), fontH6Bold, XBrushes.Black, titleTotalPayment, XStringFormats.TopLeft);
                XRect totalPayment = new XRect(380, 638, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalPayment, XStringFormats.TopLeft);
                XRect titleTotalInterestPayment = new XRect(410, 658, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalInterestPayment), fontH6Bold, XBrushes.Black, titleTotalInterestPayment, XStringFormats.TopLeft);
                XRect totalInterestPayment = new XRect(360, 658, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalInterestPayment, XStringFormats.TopLeft);
                XRect titleTotalPrincipalPayment = new XRect(410, 673, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPrincipalPayment), fontH6Bold, XBrushes.Black, titleTotalPrincipalPayment, XStringFormats.TopLeft);
                XRect totalPrincipalPayment = new XRect(350, 673, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalPrincipalPayment, XStringFormats.TopLeft);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Creates the Part 3: Wisor recommended structures.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public int CreateRecommendedStructuresPage(int number, bool shouldWritePreview = false)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);
            // SECTION .3
            XRect part3RecommandedStructures = new XRect(250, 100, 310, 30);
            XPen part3RecommandedStructuresUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(part3RecommandedStructuresUnderline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            if (shouldWritePreview)
                tf.DrawString(CheckRTL(Properties.Resources.part3RecommandedStructures), fontH1, new XSolidBrush(colorGreenTitle), part3RecommandedStructures, XStringFormats.TopLeft);
            else 
                tf.DrawString(CheckRTL(Properties.Resources.partRecommandedStructures), fontH1, new XSolidBrush(colorGreenTitle), part3RecommandedStructures, XStringFormats.TopLeft);

            XRect rectOrderNumberTitle = new XRect(450, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.orderNumberTitle), fontH6Bold, XBrushes.Black, rectOrderNumberTitle, XStringFormats.TopLeft);
            XRect rectOrderNumberValue = new XRect(380, 135, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.OrderNumberValue), fontH6Bold, XBrushes.Black, rectOrderNumberValue, XStringFormats.TopLeft);
            // המלצה 1 Header 
            XRect titleRecommandedStructuresText = new XRect(250, 165, 310, 30);
            XPen titleRecommandedStructuresTextUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleRecommandedStructuresTextUnderline,
                new XPoint(462, 182),
                new XPoint(563, 182));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructuresText), fontH2, new XSolidBrush(colorGreenTitle), titleRecommandedStructuresText, XStringFormats.TopLeft);

            int changeHeight = 60;
            //if (shouldWritePreview)
            {
                changeHeight = 0;
                XRect titleRecommandedStructuresTextVariant1 = new XRect(10, 205, 550, 70);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructuresTextVariant1), fontH5, XBrushes.Black, titleRecommandedStructuresTextVariant1, XStringFormats.TopLeft);
            }
            //
            // HEADER RECOMMENDED STRUCTURE 1
            //
            XRect titleRecommandedStructures1 = new XRect(250, 285, 310, 30);
            XPen titleRecommandedStructures1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleRecommandedStructures1Underline,
                new XPoint(30, 300),
                new XPoint(563, 300));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures1), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures1, XStringFormats.TopLeft);
            // TABLE TOP
            int heighPosition = 95;
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + 245),
                new XPoint(563, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + 260),
                new XPoint(563, heighPosition + 260));
            // TITLES
            // XRect structureTypeText = new XRect(260, 315, 300, 20);
            XRect structureTypeText = new XRect(60, 315, 500, 20);
            tf.Alignment = SetAlignmentForCulture();

            // Shuky - the header should be calculate...
            string structureTypeString;
            MiscUtilities.CalculateTypeOfProducts2(longReportDataObject.RecommendedStructureTableValues1,
                out structureTypeString);
            string s = CheckRTL(structureTypeString);
            tf.DrawString(CheckRTL(structureTypeString), fontH5Bold, XBrushes.Black, structureTypeText, XStringFormats.TopLeft);
            // tf.DrawString(CheckRTL(Properties.Resources.structureTypeText), fontH5Bold, XBrushes.Black, structureTypeText, XStringFormats.TopLeft);

            XRect titleStructureAmount = new XRect(391, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureAmount), fontH6Bold, XBrushes.Black, titleStructureAmount, XStringFormats.TopLeft);
            XRect titleStructureRate = new XRect(230, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureRate), fontH6Bold, XBrushes.Black, titleStructureRate, XStringFormats.TopLeft);
            XRect titleStructureProductType = new XRect(340, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureProductType), fontH6Bold, XBrushes.Black, titleStructureProductType, XStringFormats.TopLeft);
            XRect titleStructurePmt = new XRect(136, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructurePmt), fontH6Bold, XBrushes.Black, titleStructurePmt, XStringFormats.TopLeft);
            XRect titleStructureTime = new XRect(270, heighPosition + 248, 57, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureTime), fontH6Bold, XBrushes.Black, titleStructureTime, XStringFormats.TopLeft);
            // VALUES
            int positionY1 = 260;
            for (int i = 0; i < longReportDataObject.RecommendedStructureTableValues1.Length; i++)
            {
                positionY1 += 15;
                gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + positionY1),
                new XPoint(563, heighPosition + positionY1));

                XRect structureAmountValue1 = new XRect(405, heighPosition + positionY1 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues1[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                XRect structureProductTypeValue1 = new XRect(340, heighPosition + positionY1 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues1[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                XRect structureRateValue1 = new XRect(235, heighPosition + positionY1 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL((100 * longReportDataObject.RecommendedStructureTableValues1[i].Rate).ToString("0.00") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                XRect structureTimeValue1 = new XRect(170, heighPosition + positionY1 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues1[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                XRect structurePmtValue1 = new XRect(135, heighPosition + positionY1 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues1[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
            }
            gfx.DrawLine(tableLine3,
                new XPoint(563, heighPosition + 245),
                new XPoint(563, heighPosition + positionY1 + 15));
            gfx.DrawLine(tableLine3,
                new XPoint(508, heighPosition + positionY1 + 15),
                new XPoint(508, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + positionY1 + 15),
                new XPoint(240, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(295, heighPosition + positionY1 + 15),
                new XPoint(295, heighPosition + 245));
            // TOTAL ROW
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + positionY1 + 15),
                new XPoint(295, heighPosition + positionY1 + 15));
            gfx.DrawLine(tableLine3,
                new XPoint(508, heighPosition + positionY1 + 15),
                new XPoint(563, heighPosition + positionY1 + 15));
            // LEFT ROW NEXT TO THE TABLE
            gfx.DrawLine(tableLine3,
                new XPoint(30, heighPosition + positionY1 + 15),
                new XPoint(180, heighPosition + positionY1 + 15));
            gfx.DrawLine(tableLine3,
                new XPoint(30, heighPosition + positionY1),
                new XPoint(180, heighPosition + positionY1));
            // TOTAL
            XRect totalStructureAmount = new XRect(405, heighPosition + positionY1 + 15 - 12, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount1.ToString("N0")), fontH6, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
            XRect totalStructurePmt = new XRect(135, heighPosition + positionY1 + 15 - 12, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt1.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
            // LEFT VALUE NEXT TO THE TABLE
            XRect titleStructureTotalPaymentValueBackground = new XRect(30, heighPosition + positionY1 + 0.5, 60, 14);
            gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), titleStructureTotalPaymentValueBackground);
            XRect titleStructureTotalPayment = new XRect(70, heighPosition + positionY1 + 15 - 12, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureTotalPayment), fontH6Bold, XBrushes.Black, titleStructureTotalPayment, XStringFormats.TopLeft);
            XRect titleStructureTotalPaymentValue = new XRect(27, heighPosition + positionY1 + 15 - 12, 60, 15);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment1.ToString("N0")), fontH6Bold, XBrushes.Black, titleStructureTotalPaymentValue, XStringFormats.TopLeft);
            
            // Case when we have 2 structures
            if (longReportDataObject.NumberRecommendedStructures == 2 || longReportDataObject.NumberRecommendedStructures == 3)
            {
                //
                // HEADER RECOMMENDED STRUCTURE 2
                //
                XRect titleRecommandedStructures2 = new XRect(250, 450, 310, 30);
                XPen titleRecommandedStructures2Underline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleRecommandedStructures2Underline,
                    new XPoint(30, 465),
                    new XPoint(563, 465));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures2), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures2, XStringFormats.TopLeft);

                // TABLE TOP
                int heightPosition2 = 260;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition2 + 245),
                    new XPoint(563, heightPosition2 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition2 + 260),
                    new XPoint(563, heightPosition2 + 260));
                // TITLES
                XRect structureTypeText2 = new XRect(60, heightPosition2 + 220, 500, 20);
                tf.Alignment = SetAlignmentForCulture();

                // Shuky - the header should be calculate...
                MiscUtilities.CalculateTypeOfProducts2(longReportDataObject.RecommendedStructureTableValues2,
                    out structureTypeString);
                tf.DrawString(CheckRTL(structureTypeString), fontH5Bold, XBrushes.Black, structureTypeText2, XStringFormats.TopLeft);

                // tf.DrawString(CheckRTL(Properties.Resources.structureTypeText2), fontH5Bold, XBrushes.Black, structureTypeText2, XStringFormats.TopLeft);
                XRect titleStructureAmount2 = new XRect(391, heightPosition2 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureAmount), fontH6Bold, XBrushes.Black, titleStructureAmount2, XStringFormats.TopLeft);
                XRect titleStructureRate2 = new XRect(230, heightPosition2 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureRate), fontH6Bold, XBrushes.Black, titleStructureRate2, XStringFormats.TopLeft);
                XRect titleStructureProductType2 = new XRect(340, heightPosition2 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureProductType), fontH6Bold, XBrushes.Black, titleStructureProductType2, XStringFormats.TopLeft);
                XRect titleStructurePmt2 = new XRect(136, heightPosition2 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructurePmt), fontH6Bold, XBrushes.Black, titleStructurePmt2, XStringFormats.TopLeft);
                XRect titleStructureTime2 = new XRect(270, heightPosition2 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureTime), fontH6Bold, XBrushes.Black, titleStructureTime2, XStringFormats.TopLeft);
                // VALUES
                int positionY2 = 260;
                for (int i = 0; i < longReportDataObject.RecommendedStructureTableValues2.Length; i++)
                {
                    positionY2 += 15;
                    gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition2 + positionY2),
                    new XPoint(563, heightPosition2 + positionY2));

                    XRect structureAmountValue1 = new XRect(405, heightPosition2 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    XRect structureProductTypeValue1 = new XRect(340, heightPosition2 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues2[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    XRect structureRateValue1 = new XRect(235, heightPosition2 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((100 * longReportDataObject.RecommendedStructureTableValues2[i].Rate).ToString("0.00") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    XRect structureTimeValue1 = new XRect(170, heightPosition2 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues2[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    XRect structurePmtValue1 = new XRect(135, heightPosition2 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues2[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                }
                gfx.DrawLine(tableLine3,
                    new XPoint(563, heightPosition2 + 245),
                    new XPoint(563, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, heightPosition2 + positionY2 + 15),
                    new XPoint(508, heightPosition2 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition2 + positionY2 + 15),
                    new XPoint(240, heightPosition2 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, heightPosition2 + positionY2 + 15),
                    new XPoint(295, heightPosition2 + 245));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition2 + positionY2 + 15),
                    new XPoint(295, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, heightPosition2 + positionY2 + 15),
                    new XPoint(563, heightPosition2 + positionY2 + 15));
                // LEFT ROW NEXT TO THE TABLE
                gfx.DrawLine(tableLine3,
                    new XPoint(30, heightPosition2 + positionY2 + 15),
                    new XPoint(180, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(30, heightPosition2 + positionY2),
                    new XPoint(180, heightPosition2 + positionY2));
                // TOTAL
                XRect totalStructureAmount2 = new XRect(405, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount2.ToString("N0")), fontH6, XBrushes.Black, totalStructureAmount2, XStringFormats.TopLeft);
                XRect totalStructurePmt2 = new XRect(135, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt2.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt2, XStringFormats.TopLeft);
                // LEFT VALUE NEXT TO THE TABLE
                XRect titleStructureTotalPaymentValueBackground2 = new XRect(30, heightPosition2 + positionY2 + 0.5, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightPinkTable), titleStructureTotalPaymentValueBackground2);
                XRect titleStructureTotalPayment2 = new XRect(70, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureTotalPayment), fontH6Bold, XBrushes.Black, titleStructureTotalPayment2, XStringFormats.TopLeft);
                XRect titleStructureTotalPaymentValue2 = new XRect(27, heightPosition2 + positionY2 + 15 - 12, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment2.ToString("N0")), fontH6Bold, XBrushes.Black, titleStructureTotalPaymentValue2, XStringFormats.TopLeft);
            }

            // For case when we have 3 structures
            if (longReportDataObject.NumberRecommendedStructures == 3)
            {
                //
                // HEADER RECOMMENDED STRUCTURE 3
                //
                XRect titleRecommandedStructures3 = new XRect(250, 620, 310, 30);
                XPen titleRecommandedStructures3Underline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleRecommandedStructures3Underline,
                    new XPoint(30, 635),
                    new XPoint(563, 635));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures3), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures3, XStringFormats.TopLeft);

                // TABLE TOP
                int heightPosition3 = 430;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition3 + 260),
                    new XPoint(563, heightPosition3 + 260));
                // TITLES
                // XRect structureTypeText3 = new XRect(260, heightPosition3 + 220, 300, 20);
                XRect structureTypeText3 = new XRect(60, heightPosition3 + 220, 500, 20);
                tf.Alignment = SetAlignmentForCulture();

                // Shuky - the header should be calculate...
                MiscUtilities.CalculateTypeOfProducts2(longReportDataObject.RecommendedStructureTableValues3,
                    out structureTypeString);
                tf.DrawString(CheckRTL(structureTypeString), fontH5Bold, XBrushes.Black, structureTypeText3, XStringFormats.TopLeft);

                //tf.DrawString(CheckRTL(Properties.Resources.structureTypeText3), fontH5Bold, XBrushes.Black, structureTypeText3, XStringFormats.TopLeft);
                XRect titleStructureAmount3 = new XRect(391, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureAmount), fontH6Bold, XBrushes.Black, titleStructureAmount3, XStringFormats.TopLeft);
                XRect titleStructureRate3 = new XRect(230, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureRate), fontH6Bold, XBrushes.Black, titleStructureRate3, XStringFormats.TopLeft);
                XRect titleStructureProductType3 = new XRect(340, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureProductType), fontH6Bold, XBrushes.Black, titleStructureProductType3, XStringFormats.TopLeft);
                XRect titleStructurePmt3 = new XRect(136, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructurePmt), fontH6Bold, XBrushes.Black, titleStructurePmt3, XStringFormats.TopLeft);
                XRect titleStructureTime3 = new XRect(270, heightPosition3 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureTime), fontH6Bold, XBrushes.Black, titleStructureTime3, XStringFormats.TopLeft);
                // VALUES
                int positionY3 = 260;
                for (int i = 0; i < longReportDataObject.RecommendedStructureTableValues3.Length; i++)
                {
                    positionY3 += 15;
                    gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition3 + positionY3),
                    new XPoint(563, heightPosition3 + positionY3));

                    XRect structureAmountValue1 = new XRect(405, heightPosition3 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    XRect structureProductTypeValue1 = new XRect(340, heightPosition3 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues3[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    XRect structureRateValue1 = new XRect(235, heightPosition3 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((100 * longReportDataObject.RecommendedStructureTableValues3[i].Rate).ToString("0.00") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    XRect structureTimeValue1 = new XRect(170, heightPosition3 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.RecommendedStructureTableValues3[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    XRect structurePmtValue1 = new XRect(135, heightPosition3 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.RecommendedStructureTableValues3[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                }
                gfx.DrawLine(tableLine3,
                    new XPoint(563, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + positionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, heightPosition3 + positionY3 + 15),
                    new XPoint(508, heightPosition3 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition3 + positionY3 + 15),
                    new XPoint(240, heightPosition3 + 245));
                gfx.DrawLine(tableLine3,
                    new XPoint(295, heightPosition3 + positionY3 + 15),
                    new XPoint(295, heightPosition3 + 245));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heightPosition3 + positionY3 + 15),
                    new XPoint(295, heightPosition3 + positionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(508, heightPosition3 + positionY3 + 15),
                    new XPoint(563, heightPosition3 + positionY3 + 15));
                // LEFT ROW NEXT TO THE TABLE
                gfx.DrawLine(tableLine3,
                    new XPoint(30, heightPosition3 + positionY3 + 15),
                    new XPoint(180, heightPosition3 + positionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(30, heightPosition3 + positionY3),
                    new XPoint(180, heightPosition3 + positionY3));
                // TOTAL
                XRect totalStructureAmount3 = new XRect(405, heightPosition3 + positionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount3.ToString("N0")), fontH6, XBrushes.Black, totalStructureAmount3, XStringFormats.TopLeft);
                XRect totalStructurePmt3 = new XRect(135, heightPosition3 + positionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt3.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt3, XStringFormats.TopLeft);
                // LEFT VALUE NEXT TO THE TABLE
                XRect titleStructureTotalPaymentValueBackground3 = new XRect(30, heightPosition3 + positionY3 + 0.5, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), titleStructureTotalPaymentValueBackground3);
                XRect titleStructureTotalPayment3 = new XRect(70, heightPosition3 + positionY3 + 15 - 12, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStructureTotalPayment), fontH6Bold, XBrushes.Black, titleStructureTotalPayment3, XStringFormats.TopLeft);
                XRect titleStructureTotalPaymentValue3 = new XRect(27, heightPosition3 + positionY3 + 15 - 12, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment3.ToString("N0")), fontH6Bold, XBrushes.Black, titleStructureTotalPaymentValue3, XStringFormats.TopLeft);
            }
            return longReportDataObject.NumberRecommendedStructures;
        }

        /// <summary>
        /// Creates the Part 3: Detailed recommended structure.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateRecommendedOrStressTestStructures(int pageNumber, int structureNumber, bool createStressTestStructure)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(pageNumber);

            // Header
            XRect sectionTitleSingleStructure = new XRect(250, 100, 310, 30);
            XPen sectionTitleSingleStructureUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleSingleStructureUnderline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
            {
                // Create page with header number 1
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructure), fontH1, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructure, XStringFormats.TopLeft);
            }
            if (structureNumber == 2)
            {
                // Create page with header number 2
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructure2), fontH1, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructure, XStringFormats.TopLeft);
            }
            if (structureNumber == 3)
            {
                // Create page with header number 3
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructure3), fontH1, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructure, XStringFormats.TopLeft);
            }
            // SECTION STATS
            XRect titleSingleStructureFixed = new XRect(91, 174, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureFixed), fontH6, XBrushes.Black, titleSingleStructureFixed, XStringFormats.TopLeft);
            XRect singleStructureFixedValue = new XRect(52, 174, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue, XStringFormats.TopLeft);
            if (structureNumber == 2)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue, XStringFormats.TopLeft);
            if (structureNumber == 3)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue, XStringFormats.TopLeft);
            XRect titleSingleStructureAdjustable = new XRect(91, 188, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureAdjustable), fontH6, XBrushes.Black, titleSingleStructureAdjustable, XStringFormats.TopLeft);
            XRect singleStructureAdjustableValue = new XRect(52, 188, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue, XStringFormats.TopLeft);
            if (structureNumber == 2)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue, XStringFormats.TopLeft);
            if (structureNumber == 3)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue, XStringFormats.TopLeft);
            XRect titleSingleStructureNoTsamud = new XRect(65, 174, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureNoTsamud), fontH6, XBrushes.Black, titleSingleStructureNoTsamud, XStringFormats.TopLeft);
            XRect singleStructureNoTsamudValue = new XRect(30, 174, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue, XStringFormats.TopLeft);
            if (structureNumber == 2)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue, XStringFormats.TopLeft);
            if (structureNumber == 3)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue, XStringFormats.TopLeft);
            XRect titleSingleStructureTsamud = new XRect(65, 188, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureTsamud), fontH6, XBrushes.Black, titleSingleStructureTsamud, XStringFormats.TopLeft);
            XRect singleStructureTsamudValue = new XRect(30, 188, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue, XStringFormats.TopLeft);
            if (structureNumber == 2)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue, XStringFormats.TopLeft);
            if (structureNumber == 3)
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue, XStringFormats.TopLeft);
            // Section SingleStructureStats 
            int heighPosition2 = -90; 
            XPen tableLine2 = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(tableLine2,
                new XPoint(35, heighPosition2 + 260),
                new XPoint(100, heighPosition2 + 260));
            gfx.DrawLine(tableLine2,
                new XPoint(140, heighPosition2 + 260),
                new XPoint(205, heighPosition2 + 260));
            XRect titleSingleStructureRateType = new XRect(105, 160, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureRateType), fontH6Bold, XBrushes.Black, titleSingleStructureRateType, XStringFormats.TopLeft);
            XRect titleSingleStructureInflation = new XRect(49, 160, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureInflation), fontH6Bold, XBrushes.Black, titleSingleStructureInflation, XStringFormats.TopLeft);
            //
            // FIRST TABLE (SectionSingleStructureData)
            //
            int heighPosition = -90;
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + 245),
                new XPoint(563, heighPosition + 245));
            gfx.DrawLine(tableLine3,
               new XPoint(240, heighPosition + 260),
               new XPoint(563, heighPosition + 260));
            XRect titleStructureProductType = new XRect(340, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureProductType), fontH6Bold, XBrushes.Black, titleStructureProductType, XStringFormats.TopLeft);
            XRect titleStructureRate = new XRect(230, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureRate), fontH6Bold, XBrushes.Black, titleStructureRate, XStringFormats.TopLeft);
            XRect titleStructureTime = new XRect(270, heighPosition + 248, 57, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureTime), fontH6Bold, XBrushes.Black, titleStructureTime, XStringFormats.TopLeft);
            XRect titleStructurePmt = new XRect(136, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructurePmt), fontH6Bold, XBrushes.Black, titleStructurePmt, XStringFormats.TopLeft);
            XRect titleStructureAmount = new XRect(391, heighPosition + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStructureAmount), fontH6Bold, XBrushes.Black, titleStructureAmount, XStringFormats.TopLeft);
            int positionY = 260;
            for (int i = 0; i < longReportDataObject.SingleStructureDataTableValues.Length; i++)
            {
                positionY += 15;
                gfx.DrawLine(tableLine3,
                    new XPoint(240, heighPosition + positionY),
                    new XPoint(563, heighPosition + positionY));
                XRect structureAmountValue1 = new XRect(405, heighPosition + positionY - 10, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, structureAmountValue1, XStringFormats.TopLeft);
                }
                XRect structureProductTypeValue1 = new XRect(340, heighPosition + positionY - 10, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues2[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues3[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues2[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues3[i].ProductType.ToString()), fontH6, XBrushes.Black, structureProductTypeValue1, XStringFormats.TopLeft);
                }
                XRect structureRateValue1 = new XRect(235, heighPosition + positionY - 10, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues2[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues3[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues2[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues3[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, structureRateValue1, XStringFormats.TopLeft);
                }
                    
                XRect structureTimeValue1 = new XRect(170, heighPosition + positionY - 10, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues2[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.SingleStructureDataTableValues3[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues2[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString(CheckRTL(longReportDataObject.StressTestStructureDataTableValues3[i].Time.ToString()), fontH6, XBrushes.Black, structureTimeValue1, XStringFormats.TopLeft);
                }
                XRect structurePmtValue1 = new XRect(135, heighPosition + positionY - 10, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues2[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureDataTableValues3[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues2[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestStructureDataTableValues3[i].Pmt.ToString("N0")), fontH6, XBrushes.Black, structurePmtValue1, XStringFormats.TopLeft);
                }
                    
            }
            gfx.DrawLine(tableLine3,
                new XPoint(563, heighPosition + 245),
                new XPoint(563, heighPosition + positionY + 15));
            gfx.DrawLine(tableLine3,
                new XPoint(508, heighPosition + positionY + 15),
                new XPoint(508, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + positionY + 15),
                new XPoint(240, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(295, heighPosition + positionY + 15),
                new XPoint(295, heighPosition + 245));
            gfx.DrawLine(tableLine3,
                new XPoint(240, heighPosition + positionY + 15),
                new XPoint(295, heighPosition + positionY + 15));
            gfx.DrawLine(tableLine3,
                new XPoint(508, heighPosition + positionY + 15),
                new XPoint(563, heighPosition + positionY + 15));
            // TOTAL AMOUNT
            XRect totalStructureAmount = new XRect(405, heighPosition + 308, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount1.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructureAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructureAmount1.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructureAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructureAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructureAmount, XStringFormats.TopLeft);
            }
            // TOTAL PMT
            XRect totalStructurePmt = new XRect(135, heighPosition + 308, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt1.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt2.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalStructurePmt3.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
            }     
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructurePmt1.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructurePmt2.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalStructurePmt3.ToString("N0")), fontH6Bold, XBrushes.Black, totalStructurePmt, XStringFormats.TopLeft);
            }
            //  SECOND HEADER 
            XRect sectionTitleSingleStructureAnalysis = new XRect(250, 265, 310, 30);
            XPen sectionTitleSingleStructureAnalysisUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleSingleStructureAnalysisUnderline,
                new XPoint(462, 280),
                new XPoint(563, 280));
            tf.Alignment = SetAlignmentForCulture();
            if (structureNumber == 1)
            {
                // Create page with header number 1
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructureAnalysis), fontH2, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructureAnalysis, XStringFormats.TopLeft);
            }
            if (structureNumber == 2)
            {
                // Create page with header number 2
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructureAnalysis2), fontH2, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructureAnalysis, XStringFormats.TopLeft);
            }
            if (structureNumber == 3)
            {
                // Create page with header number 3
                tf.DrawString(CheckRTL(Properties.Resources.sectionTitleSingleStructureAnalysis3), fontH2, new XSolidBrush(colorGreenTitle), sectionTitleSingleStructureAnalysis, XStringFormats.TopLeft);
            }
           
            // Small Underlined Header
            XRect sectionTitleFirstFiveYears = new XRect(250, 310, 310, 30);
            XPen sectionTitleFirstFiveYearsUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleFirstFiveYearsUnderline,
                new XPoint(400, 325),
                new XPoint(563, 325));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionTitleFirstFiveYears), fontH3_5, new XSolidBrush(colorGreenTitle), sectionTitleFirstFiveYears, XStringFormats.TopLeft);
            //
            // SECOND TABLE (sectionSingleStructureFirstFiveYears)
            //
            int heightPosition3 = 95; 
            XPen tableLine4 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine4,
                new XPoint(175, heightPosition3 + 245),
                new XPoint(563, heightPosition3 + 245));
            gfx.DrawLine(tableLine4,
               new XPoint(175, heightPosition3 + 260),
               new XPoint(563, heightPosition3 + 260));
            XRect titleFirstFiveYearsAmount = new XRect(391, heightPosition3 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsAmount), fontH6Bold, XBrushes.Black, titleFirstFiveYearsAmount, XStringFormats.TopLeft);
            XRect titleFirstFiveYearsPrincipalPaid = new XRect(340, heightPosition3 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsPrincipalPaid), fontH6Bold, XBrushes.Black, titleFirstFiveYearsPrincipalPaid, XStringFormats.TopLeft);
            XRect titleFirstFiveYearsInterestPaid = new XRect(275, heightPosition3 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsInterestPaid), fontH6Bold, XBrushes.Black, titleFirstFiveYearsInterestPaid, XStringFormats.TopLeft);
            XRect titleFirstFiveYearsTotalPaid = new XRect(315, heightPosition3 + 248, 57, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsTotalPaid), fontH6Bold, XBrushes.Black, titleFirstFiveYearsTotalPaid, XStringFormats.TopLeft);
            XRect titleFirstFiveYearsNextPayment = new XRect(97, heightPosition3 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsNextPayment), fontH6Bold, XBrushes.Black, titleFirstFiveYearsNextPayment, XStringFormats.TopLeft);
            XRect titleFirstFiveYearsRemainingAmount = new XRect(171, heightPosition3 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsRemainingAmount), fontH6Bold, XBrushes.Black, titleFirstFiveYearsRemainingAmount, XStringFormats.TopLeft);
            int positionY2 = 260;
            for (int i = 0; i < longReportDataObject.SingleStructureFirstFiveYearsTableValues.Length; i++)
            {
                positionY2 += 15;
                gfx.DrawLine(tableLine4,
                    new XPoint(175, heightPosition3 + positionY2),
                    new XPoint(563, heightPosition3 + positionY2));
                XRect titleFirstFiveYearsAmountValue1 = new XRect(405, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleFirstFiveYearsAmountValue1, XStringFormats.TopLeft);
                }
                    
                XRect firstFiveYearsPrincipalPaidValue1 = new XRect(340, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues2[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues3[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues2[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues3[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsPrincipalPaidValue1, XStringFormats.TopLeft);
                }
                   
                XRect firstFiveYearsInterestPaidValue1 = new XRect(280, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues2[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues3[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (!createStressTestStructure)
                    {
                        if (structureNumber == 1)
                            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                        if (structureNumber == 2)
                            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues2[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                        if (structureNumber == 3)
                            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues3[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsInterestPaidValue1, XStringFormats.TopLeft);
                    }
                }
                    
                XRect firstFiveYearsTotalPaidValue1 = new XRect(215, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalPaidValue1, XStringFormats.TopLeft);
                XRect firstFiveYearsRemainingAmountValue1 = new XRect(164, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues2[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues3[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues2[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues3[i].RemainingAmount.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsRemainingAmountValue1, XStringFormats.TopLeft);
                }
                    
                XRect firstFiveYearsNextPaymentValue1 = new XRect(90, heightPosition3 + positionY2 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues2[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureFirstFiveYearsTableValues3[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues2[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureFirstFiveYearsTableValues3[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsNextPaymentValue1, XStringFormats.TopLeft);
                }
                   
            }
            gfx.DrawLine(tableLine4,
                new XPoint(175, heightPosition3 + positionY2 + 15),
                new XPoint(563, heightPosition3 + positionY2 + 15));
            gfx.DrawLine(tableLine4,
                new XPoint(563, heightPosition3 + 245),
                new XPoint(563, heightPosition3 + positionY2 + 15));
            gfx.DrawLine(tableLine4,
                new XPoint(508, heightPosition3 + positionY2 + 15),
                new XPoint(508, heightPosition3 + 245));
            gfx.DrawLine(tableLine4,
                new XPoint(175, heightPosition3 + positionY2 + 15),
                new XPoint(175, heightPosition3 + 245));
            XRect firstFiveYearsTotalPaidSoFarBackground = new XRect(323, heightPosition3 + positionY2 + 0.5, 55, 14);
            gfx.DrawRectangle(new XSolidBrush(colorGreenTable), firstFiveYearsTotalPaidSoFarBackground);
            XRect firstFiveYearsTotalInterestPaid = new XRect(280, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalInterestPaid1.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalInterestPaid2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalInterestPaid3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalInterestPaid1.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalInterestPaid2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalInterestPaid3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalInterestPaid, XStringFormats.TopLeft);
            }
                
            XRect firstFiveYearsTotalNextPmt = new XRect(90, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalNextPmt1.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalNextPmt2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.firstFiveYearsTotalNextPmt3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalNextPmt1.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalNextPmt2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalNextPmt3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalNextPmt, XStringFormats.TopLeft);
            }
               
            XRect firstFiveYearsTotalAmount = new XRect(407, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalAmount1.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalAmount, XStringFormats.TopLeft);
            }
                
            XRect firstFiveYearsTotalPrincipalPaid = new XRect(340, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPrincipalPaid1.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPrincipalPaid2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPrincipalPaid3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFiveYearsTotalPrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFiveYearsTotalPrincipalPaid2.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFiveYearsTotalPrincipalPaid3.ToString("N0")), fontH6, XBrushes.Black, firstFiveYearsTotalPrincipalPaid, XStringFormats.TopLeft);
            }
               
            XRect firstFiveYearsTotalPaidSoFar = new XRect(215, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPaidSoFar1.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPaidSoFar2.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalPaidSoFar3.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalPaidSoFar.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalPaidSoFar2.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalPaidSoFar3.ToString("N0")), fontH6Bold, XBrushes.Black, firstFiveYearsTotalPaidSoFar, XStringFormats.TopLeft);
            }
                
            XRect firstFiveYearsTotalRemainingAmount = new XRect(164, heightPosition3 + positionY2 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalRemainingAmount1.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalRemainingAmount2.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.FirstFiveYearsTotalRemainingAmount3.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
            } 
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalRemainingAmount1.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalRemainingAmount2.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressFirstFiveYearsTotalRemainingAmount3.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), firstFiveYearsTotalRemainingAmount, XStringFormats.TopLeft);
            }
               
            // Small Underlined Header
            XRect sectionTitleTotalPayment = new XRect(250, 450, 310, 30);
            XPen sectionTitleTotalPaymentUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleTotalPaymentUnderline,
                new XPoint(422, 465),
                new XPoint(563, 465));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionTitleTotalPayment), fontH3_5, new XSolidBrush(colorGreenTitle), sectionTitleTotalPayment, XStringFormats.TopLeft);
            //
            // THIRD TABLE (sectionSingleStructureFirstFiveYears)
            //
            int heightPosition4 = 235; 
            XPen tableLine5 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine5,
                new XPoint(327, heightPosition4 + 245),
                new XPoint(563, heightPosition4 + 245));
            gfx.DrawLine(tableLine5,
                new XPoint(327, heightPosition4 + 260),
                new XPoint(563, heightPosition4 + 260));
            XRect titleTotalPaymentAmount = new XRect(391, heightPosition4 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentAmount), fontH6Bold, XBrushes.Black, titleTotalPaymentAmount, XStringFormats.TopLeft);
            XRect titleTotalPaymentInterestPaid = new XRect(275, heightPosition4 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsInterestPaid), fontH6Bold, XBrushes.Black, titleTotalPaymentInterestPaid, XStringFormats.TopLeft);
            XRect titleTotalPaymentPrincipalPaid = new XRect(340, heightPosition4 + 248, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentPrincipalPaid), fontH6Bold, XBrushes.Black, titleTotalPaymentPrincipalPaid, XStringFormats.TopLeft);
            XRect titleTotalPaymentTotalPaid = new XRect(315, heightPosition4 + 248, 57, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentTotalPaid), fontH6Bold, XBrushes.Black, titleTotalPaymentTotalPaid, XStringFormats.TopLeft);
            int positionY3 = 260;
            for (int i = 0; i < longReportDataObject.SingleStructureTotalPaymentTableValues.Length; i++)
            {
                positionY3 += 15;
                gfx.DrawLine(tableLine5,
                new XPoint(327, heightPosition4 + positionY3),
                new XPoint(563, heightPosition4 + positionY3));
                XRect titleTotalPaymentAmountValue1 = new XRect(405, heightPosition4 + positionY3 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                }  
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues2[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues3[i].Amount.ToString("N0")), fontH6, XBrushes.Black, titleTotalPaymentAmountValue1, XStringFormats.TopLeft);
                }
                XRect totalPaymentPrincipalPaidValue1 = new XRect(340, heightPosition4 + positionY3 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues2[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues3[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                }   
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues2[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues3[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentPrincipalPaidValue1, XStringFormats.TopLeft);
                }
                XRect totalPaymentInterestPaidValue1 = new XRect(280, heightPosition4 + positionY3 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues2[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues3[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                }  
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues2[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues3[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaidValue1, XStringFormats.TopLeft);
                }
                XRect totalPaymentTotalPaidValue1 = new XRect(225, heightPosition4 + positionY3 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if (!createStressTestStructure)
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues2[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureTotalPaymentTableValues3[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                }
                else
                {
                    if (structureNumber == 1)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 2)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues2[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                    if (structureNumber == 3)
                        tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressStructureTotalPaymentTableValues3[i].TotalPaid.ToString("N0")), fontH6, new XSolidBrush(colorGreenTitle), totalPaymentTotalPaidValue1, XStringFormats.TopLeft);
                }
            }
            gfx.DrawLine(tableLine5,
                new XPoint(327, heightPosition4 + positionY3 + 15),
                new XPoint(563, heightPosition4 + positionY3 + 15));
            gfx.DrawLine(tableLine5,
                new XPoint(563, heightPosition4 + 245),
                new XPoint(563, heightPosition4 + positionY3 + 15));
            gfx.DrawLine(tableLine5,
                new XPoint(508, heightPosition4 + positionY3 + 15),
                new XPoint(508, heightPosition4 + 245));
            gfx.DrawLine(tableLine5,
                new XPoint(327, heightPosition4 + positionY3 + 15),
                new XPoint(327, heightPosition4 + 245));
            // TOTAL
            XRect totalPaymentTotalAmount = new XRect(405, heightPosition4 + positionY3 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
            }
            XRect totalPaymentTotalPrincipalPaid = new XRect(340, heightPosition4 + positionY3 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPrincipalPaid2.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPrincipalPaid3.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPrincipalPaid2.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPrincipalPaid3.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
            }
            XRect totalPaymentTotalInterestPaid = new XRect(280, heightPosition4 + positionY3 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalInterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalInterestPaid2.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalInterestPaid3.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
            }
            else
            {

                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalInterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalInterestPaid2.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalInterestPaid3.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
            }
            XRect totalPaymentTotalPaidAmountBackground = new XRect(327.5, heightPosition4 + positionY3 + 0.5, 55, 14);
            gfx.DrawRectangle(new XSolidBrush(colorGreenTable), totalPaymentTotalPaidAmountBackground);
            XRect totalPaymentTotalPaidAmount = new XRect(215, heightPosition4 + positionY3 + 3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPaidAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPaidAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalPaymentTotalPaidAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPaidAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPaidAmount2.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTotalPaymentTotalPaidAmount3.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPaidAmount, XStringFormats.TopLeft);
            }
            // Left block
            gfx.DrawLine(tableLine3,
              new XPoint(30, heightPosition4 + 305),
              new XPoint(240, heightPosition4 + 305));
            gfx.DrawLine(tableLine3,
               new XPoint(30, heightPosition4 + 320),
               new XPoint(240, heightPosition4 + 320));
            XRect offerAvarageLoanTime = new XRect(125, 500, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanTime), fontH6Bold, XBrushes.Black, offerAvarageLoanTime, XStringFormats.TopLeft);
            XRect offerAvarageLoanTimeValue = new XRect(95, 500, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureFixed.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureFixed2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureFixed3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
            }
            XRect offerAvarageLoanRate = new XRect(125, 515, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanRate), fontH6Bold, XBrushes.Black, offerAvarageLoanRate, XStringFormats.TopLeft);
            XRect offerAvarageLoanRateValue = new XRect(95, 515, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureAdjustable.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureAdjustable2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString(CheckRTL(longReportDataObject.StressSingleStructureAdjustable3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
            }
            XRect forEachShekelLoanHowMuchPaiedBack = new XRect(80, 542, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.forEachShekelLoanHowMuchPaiedBack), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBack, XStringFormats.TopLeft);
            XRect forEachShekelLoanHowMuchPaiedBackValue = new XRect(30, 543, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            if (!createStressTestStructure)
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
            }
            else
            {
                if (structureNumber == 1)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressSingleStructureAdjustable.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
                if (structureNumber == 2)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressSingleStructureAdjustable2.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
                if (structureNumber == 3)
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressSingleStructureAdjustable3.ToString("N0")), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
            }
            // Add the Graph
            WisorGraph wisorGraphLeft = new WisorGraph(longReportDataObject.SingleStructureGraphs, gfx, 20, 590, 270, 150);
            wisorGraphLeft.CreateGraph(CheckRTL(longReportDataObject.SingleStructureGraphsTitleLeft));
            WisorGraph wisorGraphRight = new WisorGraph(longReportDataObject.SingleStructureGraphs2, longReportDataObject.SingleStructureGraphs, gfx, 300, 590, 270, 150);
            wisorGraphRight.CreateGraph(CheckRTL(longReportDataObject.SingleStructureGraphsTitleRight));
        }

        /// <summary>
        /// Creates the Part 4: Stress test Wisor recommended structures.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public int CreateStressTestWisorRecommendedStructuresPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // Header part4Title
            XRect part4Title = new XRect(250, 100, 310, 30);
            XPen part4TitleUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(part4TitleUnderline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part4Title), fontH1, new XSolidBrush(colorGreenTitle), part4Title, XStringFormats.TopLeft);
            // Order number
            XRect rectOrderNumberTitle = new XRect(450, 140, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.orderNumberTitle), fontH6Bold, XBrushes.Black, rectOrderNumberTitle, XStringFormats.TopLeft);
            XRect rectOrderNumberValue = new XRect(380, 140, 111, 30);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.OrderNumberValue), fontH6Bold, XBrushes.Black, rectOrderNumberValue, XStringFormats.TopLeft);
            //
            // SECTION HOW CHANGES AFFECT TEXT
            //
            // Header 4.1 Underlined
            XRect sectionTitleHowChangesAffect = new XRect(250, 165, 310, 30);
            XPen sectionTitleHowChangesAffectUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleHowChangesAffectUnderline,
                new XPoint(360, 180),
                new XPoint(563, 180));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionTitleHowChangesAffect), fontH2, new XSolidBrush(colorGreenTitle), sectionTitleHowChangesAffect, XStringFormats.TopLeft);
            XRect sectionHowChangesAffectText = new XRect(10, 205, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionHowChangesAffectText), fontH5, XBrushes.Black, sectionHowChangesAffectText, XStringFormats.TopLeft);
            // Nested Header 1 
            XRect sectionHowChangesAffectTextNestedTitle1 = new XRect(250, 260, 310, 30);
            XPen sectionHowChangesAffectTextNestedTitle1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionHowChangesAffectTextNestedTitle1Underline,
                new XPoint(430, 275),
                new XPoint(563, 275));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionHowChangesAffectTextNestedTitle1), fontH2, new XSolidBrush(colorGreenTitle), sectionHowChangesAffectTextNestedTitle1, XStringFormats.TopLeft);
            XRect sectionHowChangesAffectText2 = new XRect(10, 290, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionHowChangesAffectText2), fontH5, XBrushes.Black, sectionHowChangesAffectText2, XStringFormats.TopLeft);
            // Nested Header 2 
            XRect sectionHowChangesAffectTextNestedTitle2 = new XRect(250, 335, 310, 30);
            XPen sectionHowChangesAffectTextNestedTitle2Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionHowChangesAffectTextNestedTitle1Underline,
                new XPoint(430, 350),
                new XPoint(563, 350));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionHowChangesAffectTextNestedTitle1), fontH2, new XSolidBrush(colorGreenTitle), sectionHowChangesAffectTextNestedTitle2, XStringFormats.TopLeft);
            XRect sectionHowChangesAffectText3 = new XRect(10, 365, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionHowChangesAffectText3), fontH5, XBrushes.Black, sectionHowChangesAffectText3, XStringFormats.TopLeft);
            // HEADER TITLE STRESS TEST
            XRect stressTest = new XRect(250, 420, 310, 30);
            XPen stressTestUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(stressTestUnderline,
                new XPoint(350, 435),
                new XPoint(563, 435));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.stressTest), fontH2, new XSolidBrush(colorGreenTitle), stressTest, XStringFormats.TopLeft);
            //
            // RECOMMENDED STRUCTURES 1
            //
            XRect titleRecommandedStructures1 = new XRect(250, 450, 310, 30);
            XPen titleRecommandedStructures1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleRecommandedStructures1Underline,
                new XPoint(30, 465),
                new XPoint(563, 465));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures1), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures1, XStringFormats.TopLeft);
            XRect stressTestText1 = new XRect(10, 480, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.stressTestText1), fontH5, XBrushes.Black, stressTestText1, XStringFormats.TopLeft);
            int heighPosition = 220; 
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
               new XPoint(30, heighPosition + 305),
               new XPoint(180, heighPosition + 305));
            gfx.DrawLine(tableLine3,
               new XPoint(30, heighPosition + 320),
               new XPoint(180, heighPosition + 320));
            // TOTAL (Left highlighted Value)
            XRect titleStressTestDifferenceInTotalPayment = new XRect(70, heighPosition + 307, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment, XStringFormats.TopLeft);
            XRect titleStressTestDifferenceInTotalPaymentValue = new XRect(27, heighPosition + 308, 60, 15);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment1.ToString("N0")), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPaymentValue, XStringFormats.TopLeft);
            int heightPosition5 = 220;
            int positionX = 190;
            XPen tableLine5 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine5,
               new XPoint(positionX+30, heightPosition5 + 305),
               new XPoint(positionX+180, heightPosition5 + 305));
            gfx.DrawLine(tableLine5,
               new XPoint(positionX+30, heightPosition5 + 320),
               new XPoint(positionX+180, heightPosition5 + 320));
            // TOTAL (Left highlighted Value)
            XRect titleStressTestTotalStressPayment = new XRect(positionX+70, heightPosition5 + 307, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStressTestTotalStressPayment), fontH6Bold, XBrushes.Black, titleStressTestTotalStressPayment, XStringFormats.TopLeft);
            XRect stressTestTotalStressPaymentBackground = new XRect(positionX+30, heightPosition5 + 305.5, 60, 14);
            gfx.DrawRectangle(new XSolidBrush(colorLightGreenTable), stressTestTotalStressPaymentBackground);
            XRect stressTestTotalStressPayment = new XRect(positionX+27, heightPosition5 + 308, 60, 15);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment1.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestTotalStressPayment, XStringFormats.TopLeft);
            int heightPosition7 = 220; 
            int positionX7 = 380;
            XPen tableLine7 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine7,
               new XPoint(positionX7 + 30, heightPosition7 + 305),
               new XPoint(positionX7 + 180, heightPosition7 + 305));
            gfx.DrawLine(tableLine7,
               new XPoint(positionX7 + 30, heightPosition7 + 320),
               new XPoint(positionX7 + 180, heightPosition7 + 320));
            // TOTAL (Left highlighted Value)
            XRect titleStressTestOriginalTotalPayment = new XRect(positionX7 + 70, heightPosition7 + 307, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStressTestOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestOriginalTotalPayment, XStringFormats.TopLeft);
            XRect stressTestOriginalTotalPayment = new XRect(positionX7 + 27, heightPosition7 + 308, 60, 15);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment1.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestOriginalTotalPayment, XStringFormats.TopLeft);
            //
            // RECOMMENDED STRUCTURES 2
            //
            if (longReportDataObject.NumberStressTestRecommendedStructures == 2 || longReportDataObject.NumberStressTestRecommendedStructures == 3)
            {
                XRect titleRecommandedStructures2 = new XRect(250, 545, 310, 30);
                XPen titleRecommandedStructures2Underline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleRecommandedStructures2Underline,
                    new XPoint(30, 560),
                    new XPoint(563, 560));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures2), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures2, XStringFormats.TopLeft);
                XRect stressTestText22 = new XRect(10, 575, 550, 70);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTestText2), fontH5, XBrushes.Black, stressTestText22, XStringFormats.TopLeft);
                int heighPosition8 = 315;
                XPen tableLine8 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine8,
                   new XPoint(30, heighPosition8 + 305),
                   new XPoint(180, heighPosition8 + 305));
                gfx.DrawLine(tableLine8,
                   new XPoint(30, heighPosition8 + 320),
                   new XPoint(180, heighPosition8 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestDifferenceInTotalPayment22 = new XRect(70, heighPosition8 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment22, XStringFormats.TopLeft);
                XRect titleStressTestDifferenceInTotalPaymentValue22 = new XRect(27, heighPosition8 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment2.ToString("N0")), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPaymentValue22, XStringFormats.TopLeft);
                int heightPosition9 = 315;
                int positionX9 = 190;
                XPen tableLine9 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine9,
                   new XPoint(positionX9 + 30, heightPosition9 + 305),
                   new XPoint(positionX9 + 180, heightPosition9 + 305));
                gfx.DrawLine(tableLine9,
                   new XPoint(positionX9 + 30, heightPosition9 + 320),
                   new XPoint(positionX9 + 180, heightPosition9 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestTotalStressPayment23 = new XRect(positionX9 + 70, heightPosition9 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestTotalStressPayment), fontH6Bold, XBrushes.Black, titleStressTestTotalStressPayment23, XStringFormats.TopLeft);
                XRect stressTestTotalStressPaymentBackground23 = new XRect(positionX9 + 30, heightPosition9 + 305.5, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightPinkTable), stressTestTotalStressPaymentBackground23);
                XRect stressTestTotalStressPayment23 = new XRect(positionX9 + 27, heightPosition9 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment2.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestTotalStressPayment23, XStringFormats.TopLeft);
                int heightPosition10 = 315;
                int positionX10 = 380;
                XPen tableLine10 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine10,
                   new XPoint(positionX10 + 30, heightPosition10 + 305),
                   new XPoint(positionX10 + 180, heightPosition10 + 305));
                gfx.DrawLine(tableLine10,
                   new XPoint(positionX10 + 30, heightPosition10 + 320),
                   new XPoint(positionX10 + 180, heightPosition10 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestOriginalTotalPayment24 = new XRect(positionX10 + 70, heightPosition10 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestOriginalTotalPayment24, XStringFormats.TopLeft);
                XRect stressTestOriginalTotalPayment24 = new XRect(positionX10 + 27, heightPosition10 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment2.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestOriginalTotalPayment24, XStringFormats.TopLeft);
            }
            
            // 
            // RECOMMENDED STRUCTURES 3
            //
            if (longReportDataObject.NumberStressTestRecommendedStructures == 3)
            {
                XRect titleRecommandedStructures3 = new XRect(250, 650, 310, 30);
                XPen titleRecommandedStructures3Underline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(titleRecommandedStructures3Underline,
                    new XPoint(30, 665),
                    new XPoint(563, 665));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRecommandedStructures3), fontH3, new XSolidBrush(colorGreenTitle), titleRecommandedStructures3, XStringFormats.TopLeft);
                XRect stressTestText32 = new XRect(10, 675, 550, 70);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTestText3), fontH5, XBrushes.Black, stressTestText32, XStringFormats.TopLeft);
                int heighPosition11 = 420;
                XPen tableLine11 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine11,
                   new XPoint(30, heighPosition11 + 305),
                   new XPoint(180, heighPosition11 + 305));
                gfx.DrawLine(tableLine11,
                   new XPoint(30, heighPosition11 + 320),
                   new XPoint(180, heighPosition11 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestDifferenceInTotalPayment32 = new XRect(70, heighPosition11 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment32, XStringFormats.TopLeft);
                XRect titleStressTestDifferenceInTotalPaymentValue32 = new XRect(27, heighPosition11 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment3.ToString("N0")), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPaymentValue32, XStringFormats.TopLeft);
                int heightPosition12 = 420;
                int positionX12 = 190;
                XPen tableLine12 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine12,
                   new XPoint(positionX12 + 30, heightPosition12 + 305),
                   new XPoint(positionX12 + 180, heightPosition12 + 305));
                gfx.DrawLine(tableLine12,
                   new XPoint(positionX12 + 30, heightPosition12 + 320),
                   new XPoint(positionX12 + 180, heightPosition12 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestTotalStressPayment33 = new XRect(positionX12 + 70, heightPosition12 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestTotalStressPayment), fontH6Bold, XBrushes.Black, titleStressTestTotalStressPayment33, XStringFormats.TopLeft);
                XRect stressTestTotalStressPaymentBackground33 = new XRect(positionX12 + 30, heightPosition12 + 305.5, 60, 14);
                gfx.DrawRectangle(new XSolidBrush(colorLightYellowTable), stressTestTotalStressPaymentBackground33);
                XRect stressTestTotalStressPayment33 = new XRect(positionX12 + 27, heightPosition12 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment3.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestTotalStressPayment33, XStringFormats.TopLeft);
                int heightPosition13 = 420;
                int positionX13 = 380;
                XPen tableLine13 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine13,
                   new XPoint(positionX13 + 30, heightPosition13 + 305),
                   new XPoint(positionX13 + 180, heightPosition13 + 305));
                gfx.DrawLine(tableLine13,
                   new XPoint(positionX13 + 30, heightPosition13 + 320),
                   new XPoint(positionX13 + 180, heightPosition13 + 320));
                // TOTAL (Left highlighted Value)
                XRect titleStressTestOriginalTotalPayment34 = new XRect(positionX13 + 70, heightPosition13 + 307, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestOriginalTotalPayment34, XStringFormats.TopLeft);
                XRect stressTestOriginalTotalPayment34 = new XRect(positionX13 + 27, heightPosition13 + 308, 60, 15);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StructureTotalPayment3.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestOriginalTotalPayment34, XStringFormats.TopLeft);
            }
            return longReportDataObject.NumberStressTestRecommendedStructures;
        }

        /// <summary>
        /// Creates the Part 4B: Stress test price offers.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateStressTestPriceOffersPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // Header
            XRect sectionTitleStressTestPriceOffers = new XRect(250, 106, 310, 30);
            XPen sectionTitleStressTestPriceOffersUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(sectionTitleStressTestPriceOffersUnderline,
                new XPoint(365, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionTitleStressTestPriceOffers), fontH2, new XSolidBrush(colorGreenTitle), sectionTitleStressTestPriceOffers, XStringFormats.TopLeft);
            //
            // FIRST PRICE OFFER
            //
            int heightPosition1 = 0;
            // Header Recommanded Structures 3
            XRect stressTestSummaryPriceOfferTitle = new XRect(250, 152 + heightPosition1, 310, 30);
            XPen stressTestSummaryPriceOfferTitleUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(stressTestSummaryPriceOfferTitleUnderline,
                new XPoint(30, 167 + heightPosition1),
                new XPoint(563, 167 + heightPosition1));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.stressTestSummaryPriceOfferTitle), fontH3Bold, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOfferTitle, XStringFormats.TopLeft);
            XRect stressTestSummaryPriceOffer1Value = new XRect(210, 152 + heightPosition1, 310, 30);
            tf.Alignment = SetAlignmentForCulture();
            // Change
            tf.DrawString(CheckRTL(longReportDataObject.stressTestSummaryPriceOfferValue1), fontH3, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOffer1Value, XStringFormats.TopLeft);
            int height = 40;
            int weight = 0;
            XRect titleSingleStructureFixed = new XRect(71 + weight, 174 + height + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureFixed), fontH6, XBrushes.Black, titleSingleStructureFixed, XStringFormats.TopLeft);
            XRect singleStructureFixedValue = new XRect(32 + weight, 174 + height + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue, XStringFormats.TopLeft);
            XRect titleSingleStructureAdjustable = new XRect(71 + weight, 188 + height + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureAdjustable), fontH6, XBrushes.Black, titleSingleStructureAdjustable, XStringFormats.TopLeft);
            XRect singleStructureAdjustableValue = new XRect(32 + weight, 188 + height + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue, XStringFormats.TopLeft);
            XRect titleSingleStructureNoTsamud = new XRect(55 + weight, 174 + height + heightPosition1, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureNoTsamud), fontH6, XBrushes.Black, titleSingleStructureNoTsamud, XStringFormats.TopLeft);
            XRect singleStructureNoTsamudValue = new XRect(30 + weight, 174 + height + heightPosition1, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue, XStringFormats.TopLeft);
            XRect titleSingleStructureTsamud = new XRect(55 + weight, 188 + height + heightPosition1, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureTsamud), fontH6, XBrushes.Black, titleSingleStructureTsamud, XStringFormats.TopLeft);
            XRect singleStructureTsamudValue = new XRect(30 + weight, 188 + height + heightPosition1, 30, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue, XStringFormats.TopLeft);
            //Section SingleStructureStats 
            XPen tableLine2 = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(tableLine2,
                new XPoint(35 + weight, 170 + height + heightPosition1),
                new XPoint(95 + weight, 170 + height + heightPosition1));
            gfx.DrawLine(tableLine2,
                new XPoint(120 + weight, 170 + height + heightPosition1),
                new XPoint(185 + weight, 170 + height + heightPosition1));
            XRect titleSingleStructureRateType = new XRect(85 + weight, 160 + height + heightPosition1, 100, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureRateType), fontH6Bold, XBrushes.Black, titleSingleStructureRateType, XStringFormats.TopLeft);
            XRect titleSingleStructureInflation = new XRect(39+ weight, 160 + height + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureInflation), fontH6Bold, XBrushes.Black, titleSingleStructureInflation, XStringFormats.TopLeft);
            int positionX = 155;
            int positionY = -20;
            // Left block
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 265 + positionY + heightPosition1),
              new XPoint(240 + positionX,  265 + positionY + heightPosition1));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX, 280 + positionY + heightPosition1),
               new XPoint(240 + positionX, 280 + positionY + heightPosition1));
            XRect forEachShekelLoanHowMuchPaiedBack = new XRect(80 + positionX, 267 + positionY + heightPosition1, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.forEachShekelLoanHowMuchPaiedBack), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBack, XStringFormats.TopLeft);
            XRect forEachShekelLoanHowMuchPaiedBackValue = new XRect(30 + positionX, 267 + positionY + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString()), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue, XStringFormats.TopLeft);
            XRect offerAvarageLoanTime = new XRect(125 + positionX, 215 + positionY + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanTime), fontH6Bold, XBrushes.Black, offerAvarageLoanTime, XStringFormats.TopLeft);
            XRect offerAvarageLoanTimeValue = new XRect(95 + positionX, 215 + positionY + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue, XStringFormats.TopLeft);
            XRect offerAvarageLoanRate = new XRect(125 + positionX, 230 + positionY + heightPosition1, 111, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanRate), fontH6Bold, XBrushes.Black, offerAvarageLoanRate, XStringFormats.TopLeft);
            XRect offerAvarageLoanRateValue = new XRect(95 + positionX, 230 + positionY + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue, XStringFormats.TopLeft);
            int positionX1 = 373;
            int positionY1 = -20;
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX1, 265 + positionY1 + heightPosition1),
              new XPoint(190 + positionX1, 265 + positionY1 + heightPosition1));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX1, 280 + positionY1 + heightPosition1),
               new XPoint(190 + positionX1, 280 + positionY1 + heightPosition1));
            XRect titleStressTestDifferenceInTotalPayment = new XRect(37 + positionX1, 267 + positionY1 + heightPosition1, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment, XStringFormats.TopLeft);
            XRect stressTestDifferenceInTotalPaymentValue = new XRect(50 + positionX1, 267 + positionY1 + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestDifferenceInTotalPaymentValue.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestDifferenceInTotalPaymentValue, XStringFormats.TopLeft);
            int positionX2 = 373;
            int positionY2 = -75;
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX2, 265 + positionY2 + heightPosition1),
              new XPoint(190 + positionX2, 265 + positionY2 + heightPosition1));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX2, 280 + positionY2 + heightPosition1),
               new XPoint(190 + positionX2, 280 + positionY2 + heightPosition1));
            XRect titleOfferOriginalTotalPayment = new XRect(37 + positionX2, 267 + positionY2 + heightPosition1, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleOfferOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleOfferOriginalTotalPayment, XStringFormats.TopLeft);
            XRect offerOriginalTotalPaymentValue = new XRect(50 + positionX2, 267 + positionY2 + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferOriginalTotalPaymentValue.ToString("N0")), fontH6Bold, XBrushes.Black, offerOriginalTotalPaymentValue, XStringFormats.TopLeft);
            int positionX3 = 373;
            int positionY3 = -60;
            gfx.DrawLine(tableLine3,
              new XPoint(30 + positionX3, 265 + positionY3 + heightPosition1),
              new XPoint(190 + positionX3, 265 + positionY3 + heightPosition1));
            gfx.DrawLine(tableLine3,
               new XPoint(30 + positionX3, 280 + positionY3 + heightPosition1),
               new XPoint(190 + positionX3, 280 + positionY3 + heightPosition1));
            XRect titleOfferNewPayment = new XRect(37 + positionX3, 267 + positionY3 + heightPosition1, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleOfferNewPayment), fontH6Bold, XBrushes.Black, titleOfferNewPayment, XStringFormats.TopLeft);
            XRect offerTotalNewPayment = new XRect(35 + positionX3, 267 + positionY3 + heightPosition1, 50, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalNewPayment.ToString("N0")), fontH6Bold, XBrushes.Black, offerTotalNewPayment, XStringFormats.TopLeft);
            //
            // SECOND PRICE OFFER
            //
            if (longReportDataObject.NumberStressTestPriceOffers == 2 || longReportDataObject.NumberStressTestPriceOffers == 3)
            {
                int heightPosition2 = 130;
                // Header Recommanded Structures 3
                XRect stressTestSummaryPriceOfferTitle2 = new XRect(250, 152 + heightPosition2, 310, 30);
                XPen stressTestSummaryPriceOfferTitleUnderline2 = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(stressTestSummaryPriceOfferTitleUnderline2,
                    new XPoint(30, 167 + heightPosition2),
                    new XPoint(563, 167 + heightPosition2));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTestSummaryPriceOfferTitle), fontH3Bold, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOfferTitle2, XStringFormats.TopLeft);
                XRect stressTestSummaryPriceOffer1Value2 = new XRect(210, 152 + heightPosition2, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                // Change
                tf.DrawString(CheckRTL(longReportDataObject.stressTestSummaryPriceOfferValue2), fontH3, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOffer1Value2, XStringFormats.TopLeft);
                int height2 = 40;
                int weight2 = 0;
                XRect titleSingleStructureFixed2 = new XRect(71 + weight2, 174 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureFixed), fontH6, XBrushes.Black, titleSingleStructureFixed2, XStringFormats.TopLeft);
                XRect singleStructureFixedValue2 = new XRect(32 + weight2, 174 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureAdjustable2 = new XRect(71 + weight2, 188 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureAdjustable), fontH6, XBrushes.Black, titleSingleStructureAdjustable2, XStringFormats.TopLeft);
                XRect singleStructureAdjustableValue2 = new XRect(32 + weight2, 188 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureNoTsamud2 = new XRect(55 + weight2, 174 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureNoTsamud), fontH6, XBrushes.Black, titleSingleStructureNoTsamud2, XStringFormats.TopLeft);
                XRect singleStructureNoTsamudValue2 = new XRect(30 + weight2, 174 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureTsamud2 = new XRect(55 + weight2, 188 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureTsamud), fontH6, XBrushes.Black, titleSingleStructureTsamud2, XStringFormats.TopLeft);
                XRect singleStructureTsamudValue2 = new XRect(30 + weight2, 188 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud2.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue2, XStringFormats.TopLeft);
                //Section SingleStructureStats 
                XPen tableLine22 = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(tableLine22,
                    new XPoint(35 + weight2, 170 + height2 + heightPosition2),
                    new XPoint(95 + weight2, 170 + height2 + heightPosition2));
                gfx.DrawLine(tableLine22,
                    new XPoint(120 + weight2, 170 + height2 + heightPosition2),
                    new XPoint(185 + weight2, 170 + height2 + heightPosition2));
                XRect titleSingleStructureRateType2 = new XRect(85 + weight2, 160 + height2 + heightPosition2, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureRateType), fontH6Bold, XBrushes.Black, titleSingleStructureRateType2, XStringFormats.TopLeft);
                XRect titleSingleStructureInflation2 = new XRect(39 + weight2, 160 + height2 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureInflation), fontH6Bold, XBrushes.Black, titleSingleStructureInflation2, XStringFormats.TopLeft);
                int positionX44 = 155;
                int positionY44 = -20;
                // Left block
                XPen tableLine33 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX44, 265 + positionY44 + heightPosition2),
                  new XPoint(240 + positionX44, 265 + positionY44 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX44, 280 + positionY44 + heightPosition2),
                   new XPoint(240 + positionX44, 280 + positionY44 + heightPosition2));
                XRect forEachShekelLoanHowMuchPaiedBack2 = new XRect(80 + positionX44, 267 + positionY44 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.forEachShekelLoanHowMuchPaiedBack), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBack2, XStringFormats.TopLeft);
                XRect forEachShekelLoanHowMuchPaiedBackValue2 = new XRect(30 + positionX44, 267 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString()), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue2, XStringFormats.TopLeft);
                XRect offerAvarageLoanTime2 = new XRect(125 + positionX44, 215 + positionY44 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanTime), fontH6Bold, XBrushes.Black, offerAvarageLoanTime2, XStringFormats.TopLeft);
                XRect offerAvarageLoanTimeValue2 = new XRect(95 + positionX44, 215 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue2, XStringFormats.TopLeft);
                XRect offerAvarageLoanRate2 = new XRect(125 + positionX44, 230 + positionY44 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanRate), fontH6Bold, XBrushes.Black, offerAvarageLoanRate2, XStringFormats.TopLeft);
                XRect offerAvarageLoanRateValue2 = new XRect(95 + positionX44, 230 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable2.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue2, XStringFormats.TopLeft);
                int positionX11 = 373;
                int positionY11 = -20;
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX11, 265 + positionY11 + heightPosition2),
                  new XPoint(190 + positionX11, 265 + positionY11 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX11, 280 + positionY11 + heightPosition2),
                   new XPoint(190 + positionX11, 280 + positionY11 + heightPosition2));
                XRect titleStressTestDifferenceInTotalPayment2 = new XRect(37 + positionX11, 267 + positionY11 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment2, XStringFormats.TopLeft);
                XRect stressTestDifferenceInTotalPaymentValue2 = new XRect(50 + positionX11, 267 + positionY11 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestDifferenceInTotalPaymentValue2.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestDifferenceInTotalPaymentValue2, XStringFormats.TopLeft);
                int positionX22 = 373;
                int positionY22 = -75;
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX22, 265 + positionY22 + heightPosition2),
                  new XPoint(190 + positionX22, 265 + positionY22 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX22, 280 + positionY22 + heightPosition2),
                   new XPoint(190 + positionX22, 280 + positionY22 + heightPosition2));
                XRect titleOfferOriginalTotalPayment2 = new XRect(37 + positionX22, 267 + positionY22 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleOfferOriginalTotalPayment2, XStringFormats.TopLeft);
                XRect offerOriginalTotalPaymentValue2 = new XRect(50 + positionX22, 267 + positionY22 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferOriginalTotalPaymentValue2.ToString("N0")), fontH6Bold, XBrushes.Black, offerOriginalTotalPaymentValue2, XStringFormats.TopLeft);
                int positionX33 = 373;
                int positionY33 = -60;
                gfx.DrawLine(tableLine33,
                  new XPoint(30 + positionX33, 265 + positionY33 + heightPosition2),
                  new XPoint(190 + positionX33, 265 + positionY33 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(30 + positionX33, 280 + positionY33 + heightPosition2),
                   new XPoint(190 + positionX33, 280 + positionY33 + heightPosition2));
                XRect titleOfferNewPayment2 = new XRect(37 + positionX33, 267 + positionY33 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferNewPayment), fontH6Bold, XBrushes.Black, titleOfferNewPayment2, XStringFormats.TopLeft);
                XRect offerTotalNewPayment2 = new XRect(35 + positionX33, 267 + positionY33 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalNewPayment2.ToString("N0")), fontH6Bold, XBrushes.Black, offerTotalNewPayment2, XStringFormats.TopLeft);
            }
            //
            // THIRD PRICE OFFER
            //
            if (longReportDataObject.NumberStressTestPriceOffers == 3)
            {
                int heightPosition2 = 260;
                // Header Recommanded Structures 3
                XRect stressTestSummaryPriceOfferTitle2 = new XRect(250, 152 + heightPosition2, 310, 30);
                XPen stressTestSummaryPriceOfferTitleUnderline2 = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(stressTestSummaryPriceOfferTitleUnderline2,
                    new XPoint(30, 167 + heightPosition2),
                    new XPoint(563, 167 + heightPosition2));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.stressTestSummaryPriceOfferTitle), fontH3Bold, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOfferTitle2, XStringFormats.TopLeft);
                XRect stressTestSummaryPriceOffer1Value2 = new XRect(210, 152 + heightPosition2, 310, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.stressTestSummaryPriceOfferValue3), fontH3, new XSolidBrush(colorGreenTitle), stressTestSummaryPriceOffer1Value2, XStringFormats.TopLeft);
                int height2 = 40;
                int weight2 = 0;
                XRect titleSingleStructureFixed2 = new XRect(71 + weight2, 174 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureFixed), fontH6, XBrushes.Black, titleSingleStructureFixed2, XStringFormats.TopLeft);
                XRect singleStructureFixedValue2 = new XRect(32 + weight2, 174 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureFixedValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureAdjustable2 = new XRect(71 + weight2, 188 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureAdjustable), fontH6, XBrushes.Black, titleSingleStructureAdjustable2, XStringFormats.TopLeft);
                XRect singleStructureAdjustableValue2 = new XRect(32 + weight2, 188 + height2 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureAdjustableValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureNoTsamud2 = new XRect(55 + weight2, 174 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureNoTsamud), fontH6, XBrushes.Black, titleSingleStructureNoTsamud2, XStringFormats.TopLeft);
                XRect singleStructureNoTsamudValue2 = new XRect(30 + weight2, 174 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalNoTsamud3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureNoTsamudValue2, XStringFormats.TopLeft);
                XRect titleSingleStructureTsamud2 = new XRect(55 + weight2, 188 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureTsamud), fontH6, XBrushes.Black, titleSingleStructureTsamud2, XStringFormats.TopLeft);
                XRect singleStructureTsamudValue2 = new XRect(30 + weight2, 188 + height2 + heightPosition2, 30, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureTotalTsamud3.ToString() + "%"), fontH6, XBrushes.Black, singleStructureTsamudValue2, XStringFormats.TopLeft);
                //Section SingleStructureStats 
                XPen tableLine22 = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(tableLine22,
                    new XPoint(35 + weight2, 170 + height2 + heightPosition2),
                    new XPoint(95 + weight2, 170 + height2 + heightPosition2));
                gfx.DrawLine(tableLine22,
                    new XPoint(120 + weight2, 170 + height2 + heightPosition2),
                    new XPoint(185 + weight2, 170 + height2 + heightPosition2));
                XRect titleSingleStructureRateType2 = new XRect(85 + weight2, 160 + height2 + heightPosition2, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureRateType), fontH6Bold, XBrushes.Black, titleSingleStructureRateType2, XStringFormats.TopLeft);
                XRect titleSingleStructureInflation2 = new XRect(39 + weight2, 160 + height2 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleSingleStructureInflation), fontH6Bold, XBrushes.Black, titleSingleStructureInflation2, XStringFormats.TopLeft);
                int positionX44 = 155;
                int positionY44 = -20;
                // Left block
                XPen tableLine33 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX44, 265 + positionY44 + heightPosition2),
                  new XPoint(240 + positionX44, 265 + positionY44 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX44, 280 + positionY44 + heightPosition2),
                   new XPoint(240 + positionX44, 280 + positionY44 + heightPosition2));
                XRect forEachShekelLoanHowMuchPaiedBack2 = new XRect(80 + positionX44, 267 + positionY44 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.forEachShekelLoanHowMuchPaiedBack), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBack2, XStringFormats.TopLeft);
                XRect forEachShekelLoanHowMuchPaiedBackValue2 = new XRect(30 + positionX44, 267 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString()), fontH6Bold, XBrushes.Black, forEachShekelLoanHowMuchPaiedBackValue2, XStringFormats.TopLeft);
                XRect offerAvarageLoanTime2 = new XRect(125 + positionX44, 215 + positionY44 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanTime), fontH6Bold, XBrushes.Black, offerAvarageLoanTime2, XStringFormats.TopLeft);
                XRect offerAvarageLoanTimeValue2 = new XRect(95 + positionX44, 215 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureFixed3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanTimeValue2, XStringFormats.TopLeft);
                XRect offerAvarageLoanRate2 = new XRect(125 + positionX44, 230 + positionY44 + heightPosition2, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanRate), fontH6Bold, XBrushes.Black, offerAvarageLoanRate2, XStringFormats.TopLeft);
                XRect offerAvarageLoanRateValue2 = new XRect(95 + positionX44, 230 + positionY44 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.SingleStructureAdjustable3.ToString() + "%"), fontH6Bold, XBrushes.Black, offerAvarageLoanRateValue2, XStringFormats.TopLeft);
                int positionX11 = 373;
                int positionY11 = -20;
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX11, 265 + positionY11 + heightPosition2),
                  new XPoint(190 + positionX11, 265 + positionY11 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX11, 280 + positionY11 + heightPosition2),
                   new XPoint(190 + positionX11, 280 + positionY11 + heightPosition2));
                XRect titleStressTestDifferenceInTotalPayment2 = new XRect(37 + positionX11, 267 + positionY11 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleStressTestDifferenceInTotalPayment), fontH6Bold, XBrushes.Black, titleStressTestDifferenceInTotalPayment2, XStringFormats.TopLeft);
                XRect stressTestDifferenceInTotalPaymentValue2 = new XRect(50 + positionX11, 267 + positionY11 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.StressTestDifferenceInTotalPaymentValue3.ToString("N0")), fontH6Bold, XBrushes.Black, stressTestDifferenceInTotalPaymentValue2, XStringFormats.TopLeft);
                int positionX22 = 373;
                int positionY22 = -75;
                gfx.DrawLine(tableLine33,
                  new XPoint(45 + positionX22, 265 + positionY22 + heightPosition2),
                  new XPoint(190 + positionX22, 265 + positionY22 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(45 + positionX22, 280 + positionY22 + heightPosition2),
                   new XPoint(190 + positionX22, 280 + positionY22 + heightPosition2));
                XRect titleOfferOriginalTotalPayment2 = new XRect(37 + positionX22, 267 + positionY22 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferOriginalTotalPayment), fontH6Bold, XBrushes.Black, titleOfferOriginalTotalPayment2, XStringFormats.TopLeft);
                XRect offerOriginalTotalPaymentValue2 = new XRect(50 + positionX22, 267 + positionY22 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferOriginalTotalPaymentValue3.ToString("N0")), fontH6Bold, XBrushes.Black, offerOriginalTotalPaymentValue2, XStringFormats.TopLeft);
                int positionX33 = 373;
                int positionY33 = -60;
                gfx.DrawLine(tableLine33,
                  new XPoint(30 + positionX33, 265 + positionY33 + heightPosition2),
                  new XPoint(190 + positionX33, 265 + positionY33 + heightPosition2));
                gfx.DrawLine(tableLine33,
                   new XPoint(30 + positionX33, 280 + positionY33 + heightPosition2),
                   new XPoint(190 + positionX33, 280 + positionY33 + heightPosition2));
                XRect titleOfferNewPayment2 = new XRect(37 + positionX33, 267 + positionY33 + heightPosition2, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOfferNewPayment), fontH6Bold, XBrushes.Black, titleOfferNewPayment2, XStringFormats.TopLeft);
                XRect offerTotalNewPayment2 = new XRect(35 + positionX33, 267 + positionY33 + heightPosition2, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.OfferTotalNewPayment3.ToString("N0")), fontH6Bold, XBrushes.Black, offerTotalNewPayment2, XStringFormats.TopLeft);
            }
        }

        /// <summary>
        /// Creates the Part 5: Summary.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreatePart5SummaryPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // PART 5 TITLE
            XRect part5Title = new XRect(250, 100, 310, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.part5Title), fontH1, new XSolidBrush(colorGreenTitle), part5Title, XStringFormats.TopLeft);
            // SECTION 5.1
            XRect summaryAndRecommandation = new XRect(250, 150, 310, 30);
            XPen summaryAndRecommandationUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(summaryAndRecommandationUnderline,
                new XPoint(450, 167),
                new XPoint(563, 167));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.SummaryAndRecommandation), fontH2, new XSolidBrush(colorGreenTitle), summaryAndRecommandation, XStringFormats.TopLeft);
            XRect summaryStatsText = new XRect(10, 200, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.summaryStatsText), fontH5, XBrushes.Black, summaryStatsText, XStringFormats.TopLeft);
            // SECTION 5.2
            XRect titleChoosingFinalStructure = new XRect(250, 350, 310, 30);
            XPen titleChoosingFinalStructureUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleChoosingFinalStructureUnderline,
                new XPoint(430, 365),
                new XPoint(563, 365));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleChoosingFinalStructure), fontH2, new XSolidBrush(colorGreenTitle), titleChoosingFinalStructure, XStringFormats.TopLeft);
            XRect choosingFinalStructureText = new XRect(10, 400, 550, 70);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.choosingFinalStructureText), fontH5, XBrushes.Black, choosingFinalStructureText, XStringFormats.TopLeft);
            // SECTION 5.3
            XRect futureSteps = new XRect(250, 510, 310, 30);
            XPen futureStepsUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(futureStepsUnderline,
                new XPoint(450, 525),
                new XPoint(563, 525));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.FutureSteps), fontH2, new XSolidBrush(colorGreenTitle), futureSteps, XStringFormats.TopLeft);
            XRect movingForwardText = new XRect(10, 550, 550, 230);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.movingForwardText), fontH5, XBrushes.Black, movingForwardText, XStringFormats.TopLeft);
        }

        public bool CreateAppendixesPage(int number)
        {
            // Return true only for Loan known and Inserted
            if ((longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_NoPriceOffers) ||
                (longReportDataObject.CurrentReportType == LongReportDataObject.ReportType.Refinance_LoanInserted_PriceOffers))
            {
                // Create an empty page and set view
                page = document.AddPage();
                page.Size = pageSizeA4;
                page.Orientation = pageOrientationPortrait;
                // Get an XGraphics object for drawing
                gfx = XGraphics.FromPdfPage(page);
                tf = new XTextFormatter(gfx);
                SetInitialViewOfPage(number);

                // HEADER
                XRect appendixTitleRefinanceAnalysis = new XRect(250, 100, 310, 30);
                XPen appendixTitleRefinanceAnalysisUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixTitleRefinanceAnalysisUnderline,
                    new XPoint(400, 115),
                    new XPoint(563, 115));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixTitleRefinanceAnalysis), fontH2, new XSolidBrush(colorGreenTitle), appendixTitleRefinanceAnalysis, XStringFormats.TopLeft);
                // HEADER
                XRect appendixSectionTitleOriginalLoan = new XRect(250, 130, 310, 30);
                XPen appendixSectionTitleOriginalLoanUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixSectionTitleOriginalLoanUnderline,
                    new XPoint(441, 145),
                    new XPoint(563, 145));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixSectionTitleOriginalLoan), fontH2, new XSolidBrush(colorGreenTitle), appendixSectionTitleOriginalLoan, XStringFormats.TopLeft);
                //
                // TOP TABLE
                //
                int heightPosition3 = -60;
                int currentLinePositionY = 260;
                // TITLES
                XRect originalBankName = new XRect(360, heightPosition3 + 220, 200, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.originalBankName), fontH6Bold, XBrushes.Black, originalBankName, XStringFormats.TopLeft);
                XRect originalBankNameValue = new XRect(380, heightPosition3 + 220, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.OfferingBankNameValue), fontH6, XBrushes.Black, originalBankNameValue, XStringFormats.TopLeft);
                XPen tableLine4 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + 260),
                    new XPoint(563, heightPosition3 + 260));
                XRect titleOriginalAmount = new XRect(391, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalAmount), fontH6Bold, XBrushes.Black, titleOriginalAmount, XStringFormats.TopLeft);
                XRect titleProductType = new XRect(340, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleProductionType), fontH6Bold, XBrushes.Black, titleProductType, XStringFormats.TopLeft);
                XRect titleDateTaken = new XRect(275, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleDateTaken), fontH6Bold, XBrushes.Black, titleDateTaken, XStringFormats.TopLeft);
                XRect titleOriginalTime = new XRect(312, heightPosition3 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalTime), fontH6Bold, XBrushes.Black, titleOriginalTime, XStringFormats.TopLeft);
                XRect titleOriginalRate = new XRect(171, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleOriginalRate), fontH6Bold, XBrushes.Black, titleOriginalRate, XStringFormats.TopLeft);
                XRect titleFirstPmt = new XRect(107, heightPosition3 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstPmt), fontH6Bold, XBrushes.Black, titleFirstPmt, XStringFormats.TopLeft);
                // LEFT PART OF TABLE 
                gfx.DrawLine(tableLine4,
                   new XPoint(30, heightPosition3 + 245),
                   new XPoint(140, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(30, heightPosition3 + 260),
                    new XPoint(140, heightPosition3 + 260));
                XRect titleTimeLeft = new XRect(30, heightPosition3 + 248, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTimeLeft), fontH6Bold, XBrushes.Black, titleTimeLeft, XStringFormats.TopLeft);
                XRect titleRateToday = new XRect(-30, heightPosition3 + 248, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleRateToday), fontH6Bold, XBrushes.Black, titleRateToday, XStringFormats.TopLeft);
                // TABLE VALUES
                for (int i = 0; i < longReportDataObject.OriginalLoanDetailsTableValues.Length; i++)
                {
                    currentLinePositionY += 15; // STEP
                    gfx.DrawLine(tableLine4,
                        new XPoint(210, heightPosition3 + currentLinePositionY),
                        new XPoint(563, heightPosition3 + currentLinePositionY));
                    gfx.DrawLine(tableLine4,
                        new XPoint(30, heightPosition3 + currentLinePositionY),
                        new XPoint(140, heightPosition3 + currentLinePositionY));
                    XRect originalAmountValue = new XRect(405, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalAmount.ToString("N0")), fontH6, XBrushes.Black, originalAmountValue, XStringFormats.TopLeft);
                    XRect productTypeValue = new XRect(340, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalProductType.ToString()), fontH6, XBrushes.Black, productTypeValue, XStringFormats.TopLeft);
                    XRect dateTakenValue = new XRect(275, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].DateTaken.ToString("MM/yyyy")), fontH6, XBrushes.Black, dateTakenValue, XStringFormats.TopLeft);
                    XRect originalTimeValue = new XRect(215, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalTime.ToString()), fontH6, XBrushes.Black, originalTimeValue, XStringFormats.TopLeft);
                    XRect originalRateValue = new XRect(164, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].OriginalRate.ToString("0.000")) + "%", fontH6, XBrushes.Black, originalRateValue, XStringFormats.TopLeft);
                    XRect firstPmtValue = new XRect(105, heightPosition3 + currentLinePositionY - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].FirstPmt.ToString("N0")), fontH6, XBrushes.Black, firstPmtValue, XStringFormats.TopLeft);
                    XRect titleTimeLeftValue = new XRect(30, heightPosition3 + currentLinePositionY - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].TimeLeft.ToString()), fontH6, XBrushes.Black, titleTimeLeftValue, XStringFormats.TopLeft);
                    XRect titleRateTodayValue = new XRect(-35, heightPosition3 + currentLinePositionY - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(longReportDataObject.OriginalLoanDetailsTableValues[i].RateToday.ToString("0.000")) + "%", fontH6, XBrushes.Black, titleRateTodayValue, XStringFormats.TopLeft);
                }
                // RIGHT AND LEFT LINES (FROM TOP TO BOTTOM)
                gfx.DrawLine(tableLine4,
                    new XPoint(563, heightPosition3 + 245),
                    new XPoint(563, heightPosition3 + currentLinePositionY));
                gfx.DrawLine(tableLine4,
                    new XPoint(508, heightPosition3 + currentLinePositionY),
                    new XPoint(508, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(210, heightPosition3 + currentLinePositionY),
                    new XPoint(210, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(30, heightPosition3 + currentLinePositionY),
                    new XPoint(30, heightPosition3 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(140, heightPosition3 + currentLinePositionY),
                    new XPoint(140, heightPosition3 + 245));
                // TOTAL PMT
                gfx.DrawLine(tableLine4,
                   new XPoint(210, heightPosition3 + currentLinePositionY + 15),
                   new XPoint(330, heightPosition3 + currentLinePositionY + 15));
                gfx.DrawLine(tableLine4,
                   new XPoint(210, heightPosition3 + currentLinePositionY + 30),
                   new XPoint(330, heightPosition3 + currentLinePositionY + 30));
                XRect titleTotalOriginalPmt = new XRect(175, heightPosition3 + currentLinePositionY + 18, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalOriginalPmt), fontH6Bold, XBrushes.Black, titleTotalOriginalPmt, XStringFormats.TopLeft);
                XRect titleTotalOriginalPmtValue = new XRect(150, heightPosition3 + currentLinePositionY + 18, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalPmt2B.ToString("N0")), fontH6Bold, XBrushes.Black, titleTotalOriginalPmtValue, XStringFormats.TopLeft);
                // TOTAL ORIGINAL AMOUNT
                gfx.DrawLine(tableLine4,
                   new XPoint(435, heightPosition3 + currentLinePositionY + 15),
                   new XPoint(563, heightPosition3 + currentLinePositionY + 15));
                gfx.DrawLine(tableLine4,
                   new XPoint(435, heightPosition3 + currentLinePositionY + 30),
                   new XPoint(563, heightPosition3 + currentLinePositionY + 30));
                XRect titleTotalOriginalAmount2B = new XRect(410, heightPosition3 + currentLinePositionY + 18, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalOriginalAmount), fontH6Bold, XBrushes.Black, titleTotalOriginalAmount2B, XStringFormats.TopLeft);
                XRect totalOriginalAmount2BValue = new XRect(380, heightPosition3 + currentLinePositionY + 18, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.TotalOriginalAmount2B.ToString("N0")), fontH6Bold, XBrushes.Black, totalOriginalAmount2BValue, XStringFormats.TopLeft);
                // HEADER
                XRect appendixSectionTitleUntilToday = new XRect(250, 415, 310, 30);
                XPen appendixSectionTitleUntilTodayUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixSectionTitleUntilTodayUnderline,
                    new XPoint(415, 430),
                    new XPoint(563, 430));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixSectionTitleUntilToday), fontH2, new XSolidBrush(colorGreenTitle), appendixSectionTitleUntilToday, XStringFormats.TopLeft);
                //
                // MIDLE TABLE (RIGHT)
                //
                int heightPosition6 = 210;
                gfx.DrawLine(tableLine4,
                    new XPoint(275, heightPosition6 + 245),
                    new XPoint(563, heightPosition6 + 245));
                gfx.DrawLine(tableLine4,
                   new XPoint(275, heightPosition6 + 260),
                   new XPoint(563, heightPosition6 + 260));
                // TABLE TITLES
                XRect paidUntilTodayTitleAmount = new XRect(391, heightPosition6 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsAmount), fontH6Bold, XBrushes.Black, paidUntilTodayTitleAmount, XStringFormats.TopLeft);
                XRect paidUntilTodayTitlePrincipalPaid = new XRect(350, heightPosition6 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsPrincipalPaid), fontH6Bold, XBrushes.Black, paidUntilTodayTitlePrincipalPaid, XStringFormats.TopLeft);
                XRect paidUntilTodayTitleInterestPaid = new XRect(290, heightPosition6 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsInterestPaid), fontH6Bold, XBrushes.Black, paidUntilTodayTitleInterestPaid, XStringFormats.TopLeft);
                XRect paidUntilTodayTitleTotalPaid = new XRect(325, heightPosition6 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsTotalPaid), fontH6Bold, XBrushes.Black, paidUntilTodayTitleTotalPaid, XStringFormats.TopLeft);
                XRect paidUntilTodayTitleNextPmt = new XRect(175, heightPosition6 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.paidUntilTodayTitleNextPmt), fontH6Bold, XBrushes.Black, paidUntilTodayTitleNextPmt, XStringFormats.TopLeft);
                int positionY2 = 260;
                // TABLE VALUES
                for (int i = 0; i < longReportDataObject.SingleStructureFirstFiveYearsTableValues.Length; i++)
                {
                    positionY2 += 15;
                    gfx.DrawLine(tableLine4,
                        new XPoint(275, heightPosition6 + positionY2),
                        new XPoint(563, heightPosition6 + positionY2));
                    XRect paidUntilTodayAmount = new XRect(405, heightPosition6 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaidUntilTodayTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, paidUntilTodayAmount, XStringFormats.TopLeft);

                    XRect paidUntilTodayPrincipalPaid = new XRect(350, heightPosition6 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaidUntilTodayTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, paidUntilTodayPrincipalPaid, XStringFormats.TopLeft);

                    XRect paidUntilTodayInterestPaid = new XRect(290, heightPosition6 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaidUntilTodayTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, paidUntilTodayInterestPaid, XStringFormats.TopLeft);

                    XRect paidUntilTodayTotalPaid = new XRect(230, heightPosition6 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaidUntilTodayTableValues[i].TotalPaid.ToString("N0")), fontH6, XBrushes.Black, paidUntilTodayTotalPaid, XStringFormats.TopLeft);

                    XRect paidUntilTodayNextPmt = new XRect(174, heightPosition6 + positionY2 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaidUntilTodayTableValues[i].NextPmt.ToString("N0")), fontH6, XBrushes.Black, paidUntilTodayNextPmt, XStringFormats.TopLeft);
                }
                gfx.DrawLine(tableLine4,
                    new XPoint(275, heightPosition6 + positionY2 + 15),
                    new XPoint(563, heightPosition6 + positionY2 + 15));
                gfx.DrawLine(tableLine4,
                    new XPoint(563, heightPosition6 + 245),
                    new XPoint(563, heightPosition6 + positionY2 + 15));
                gfx.DrawLine(tableLine4,
                    new XPoint(508, heightPosition6 + positionY2 + 15),
                    new XPoint(508, heightPosition6 + 245));
                gfx.DrawLine(tableLine4,
                    new XPoint(275, heightPosition6 + positionY2 + 15),
                    new XPoint(275, heightPosition6 + 245));
                // TOTAL (LAST ROW OF TABLE)
                XRect paidUntilTodayTotalTotalPaidBackground = new XRect(335, heightPosition6 + positionY2 + 0.5, 48, 14);
                gfx.DrawRectangle(new XSolidBrush(colorGreenTable), paidUntilTodayTotalTotalPaidBackground);
                XRect paidUntilTodayTotalInterestPaid = new XRect(290, heightPosition6 + positionY2 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PaidUntilTodayTotalInterestPaid.ToString("N0")), fontH6Bold, XBrushes.Black, paidUntilTodayTotalInterestPaid, XStringFormats.TopLeft);
                XRect paidUntilTodayTotalNextPmt = new XRect(173, heightPosition6 + positionY2 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PaidUntilTodayTotalNextPmt.ToString("N0")), fontH6Bold, XBrushes.Black, paidUntilTodayTotalNextPmt, XStringFormats.TopLeft);
                XRect paidUntilTodayTotalAmount = new XRect(407, heightPosition6 + positionY2 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PaidUntilTodayTotalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, paidUntilTodayTotalAmount, XStringFormats.TopLeft);
                XRect paidUntilTodayTotalPrincipalPaid = new XRect(350, heightPosition6 + positionY2 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PaidUntilTodayTotalPrincipalPaid.ToString("N0")), fontH6Bold, XBrushes.Black, paidUntilTodayTotalPrincipalPaid, XStringFormats.TopLeft);
                XRect paidUntilTodayTotalTotalPaid = new XRect(230, heightPosition6 + positionY2 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.PaidUntilTodayTotalTotalPaid.ToString("N0")), fontH6Bold, XBrushes.Black, paidUntilTodayTotalTotalPaid, XStringFormats.TopLeft);
                // HEADER
                XRect appendixSectionTitleFutureExpected = new XRect(13, 415, 250, 30);
                XPen appendixSectionTitleFutureExpectedUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixSectionTitleFutureExpectedUnderline,
                    new XPoint(110, 430),
                    new XPoint(263, 430));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixSectionTitleFutureExpected), fontH2, new XSolidBrush(colorGreenTitle), appendixSectionTitleFutureExpected, XStringFormats.TopLeft);
                //
                // MIDDLE TABLE 2 (LEFT)
                //
                int heightPosition4 = 210;
                int positionX4 = -297;
                XPen tableLine5 = new XPen(XColors.Black, 1);
                gfx.DrawLine(tableLine5,
                    new XPoint(327 + positionX4, heightPosition4 + 245),
                    new XPoint(563 + positionX4, heightPosition4 + 245));
                gfx.DrawLine(tableLine5,
                    new XPoint(327 + positionX4, heightPosition4 + 260),
                    new XPoint(563 + positionX4, heightPosition4 + 260));
                // TABLE TITLES
                XRect futurePaymentTitleRemainingAmount = new XRect(405 + positionX4, heightPosition4 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.futurePaymentTitleRemainingAmount), fontH6Bold, XBrushes.Black, futurePaymentTitleRemainingAmount, XStringFormats.TopLeft);
                XRect futurePaymentTitlePrincipalPayment = new XRect(340 + positionX4, heightPosition4 + 248, 150, 20); 
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.futurePaymentTitlePrincipalPayment), fontH6Bold, XBrushes.Black, futurePaymentTitlePrincipalPayment, XStringFormats.TopLeft);
                XRect futurePaymentTitleInterestPayment = new XRect(280 + positionX4, heightPosition4 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.futurePaymentTitleInterestPayment), fontH6Bold, XBrushes.Black, futurePaymentTitleInterestPayment, XStringFormats.TopLeft);
                XRect futurePaymentTitleTotalPayment = new XRect(315 + positionX4, heightPosition4 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.futurePaymentTitleTotalPayment), fontH6Bold, XBrushes.Black, futurePaymentTitleTotalPayment, XStringFormats.TopLeft);
                int positionY3 = 260;
                // TABLE VALUES
                for (int i = 0; i < longReportDataObject.SingleStructureTotalPaymentTableValues.Length; i++)
                {
                    positionY3 += 15;
                    gfx.DrawLine(tableLine5,
                    new XPoint(327 + positionX4, heightPosition4 + positionY3),
                    new XPoint(563 + positionX4, heightPosition4 + positionY3));
                    XRect futurePaymentRemainingAmount = new XRect(405 + positionX4, heightPosition4 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixExpectedFuturePaymentTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, futurePaymentRemainingAmount, XStringFormats.TopLeft);

                    XRect futurePaymentPrincipalPayment = new XRect(340 + positionX4, heightPosition4 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixExpectedFuturePaymentTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, futurePaymentPrincipalPayment, XStringFormats.TopLeft);

                    XRect futurePaymentInterestPayment = new XRect(280 + positionX4, heightPosition4 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixExpectedFuturePaymentTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, futurePaymentInterestPayment, XStringFormats.TopLeft);

                    XRect futurePaymentTotalPayment = new XRect(225 + positionX4, heightPosition4 + positionY3 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixExpectedFuturePaymentTableValues[i].TotalPaid.ToString("N0")), fontH6, XBrushes.Black, futurePaymentTotalPayment, XStringFormats.TopLeft);
                }
                gfx.DrawLine(tableLine5,
                    new XPoint(327 + positionX4, heightPosition4 + positionY3 + 15),
                    new XPoint(563 + positionX4, heightPosition4 + positionY3 + 15));
                gfx.DrawLine(tableLine5,
                    new XPoint(563 + positionX4, heightPosition4 + 245),
                    new XPoint(563 + positionX4, heightPosition4 + positionY3 + 15));
                gfx.DrawLine(tableLine5,
                    new XPoint(508 + positionX4, heightPosition4 + positionY3 + 15),
                    new XPoint(508 + positionX4, heightPosition4 + 245));
                gfx.DrawLine(tableLine5,
                    new XPoint(327 + positionX4, heightPosition4 + positionY3 + 15),
                    new XPoint(327 + positionX4, heightPosition4 + 245));
                // TOTAL (LAST ROW IN TABLE)
                XRect futurePaymentTotalRemainingAmount = new XRect(405 + positionX4, heightPosition4 + positionY3 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FuturePaymentTotalRemainingAmount.ToString("N0")), fontH6Bold, XBrushes.Black, futurePaymentTotalRemainingAmount, XStringFormats.TopLeft);
                XRect futurePaymentTotalPrincipalPayment = new XRect(340 + positionX4, heightPosition4 + positionY3 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FuturePaymentTotalPrincipalPayment.ToString("N0")), fontH6Bold, XBrushes.Black, futurePaymentTotalPrincipalPayment, XStringFormats.TopLeft);
                XRect futurePaymentTotalInterestPayment = new XRect(280 + positionX4, heightPosition4 + positionY3 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FuturePaymentTotalInterestPayment.ToString("N0")), fontH6Bold, XBrushes.Black, futurePaymentTotalInterestPayment, XStringFormats.TopLeft);
                XRect futurePaymentTotalPaidAmountBackground = new XRect(327.5 + positionX4, heightPosition4 + positionY3 + 0.5, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorGreenTable), futurePaymentTotalPaidAmountBackground);
                XRect futurePaymentTotalTotalPayment = new XRect(225 + positionX4, heightPosition4 + positionY3 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.FuturePaymentTotalTotalPayment.ToString("N0")), fontH6Bold, XBrushes.Black, futurePaymentTotalTotalPayment, XStringFormats.TopLeft);
                // HEADER
                XRect appendixSectionFullLoanLifeTime = new XRect(250, 600, 310, 30);
                XPen appendixSectionFullLoanLifeTimeUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixSectionFullLoanLifeTimeUnderline,
                    new XPoint(441, 615),
                    new XPoint(563, 615));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixSectionFullLoanLifeTime), fontH2, new XSolidBrush(colorGreenTitle), appendixSectionFullLoanLifeTime, XStringFormats.TopLeft);
                //
                // BOTTOM TABLE
                //
                int heightPosition10 = 390;
                gfx.DrawLine(tableLine5,
                    new XPoint(327, heightPosition10 + 245),
                    new XPoint(563, heightPosition10 + 245));
                gfx.DrawLine(tableLine5,
                    new XPoint(327, heightPosition10 + 260),
                    new XPoint(563, heightPosition10 + 260));
                // TABLE TITLES
                XRect totalPaymentTitleAmount = new XRect(391, heightPosition10 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentAmount), fontH6Bold, XBrushes.Black, totalPaymentTitleAmount, XStringFormats.TopLeft);
                XRect totalPaymentTitlePrincipalPaid = new XRect(279, heightPosition10 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleFirstFiveYearsInterestPaid), fontH6Bold, XBrushes.Black, totalPaymentTitlePrincipalPaid, XStringFormats.TopLeft);
                XRect totalPaymentTitleInterestPaid = new XRect(340, heightPosition10 + 248, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentPrincipalPaid), fontH6Bold, XBrushes.Black, totalPaymentTitleInterestPaid, XStringFormats.TopLeft);
                XRect totalPaymentTitleTotalPaid = new XRect(315, heightPosition10 + 248, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.titleTotalPaymentTotalPaid), fontH6Bold, XBrushes.Black, totalPaymentTitleTotalPaid, XStringFormats.TopLeft);
                int positionY10 = 260;
                // TABLE VALUES
                for (int i = 0; i < longReportDataObject.SingleStructureTotalPaymentTableValues.Length; i++)
                {
                    positionY10 += 15;
                    gfx.DrawLine(tableLine5,
                    new XPoint(327, heightPosition10 + positionY10),
                    new XPoint(563, heightPosition10 + positionY10));
                    XRect totalPaymentAmount = new XRect(405, heightPosition10 + positionY10 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixTotalPaymentTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, totalPaymentAmount, XStringFormats.TopLeft);
                        
                    XRect totalPaidPrincipalPaid = new XRect(340, heightPosition10 + positionY10 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixTotalPaymentTableValues[i].PrincipalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaidPrincipalPaid, XStringFormats.TopLeft);
                        
                    XRect totalPaymentInterestPaid = new XRect(280, heightPosition10 + positionY10 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixTotalPaymentTableValues[i].InterestPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentInterestPaid, XStringFormats.TopLeft);
                       
                    XRect totalPaymentTotalPaid = new XRect(225, heightPosition10 + positionY10 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixTotalPaymentTableValues[i].TotalPaid.ToString("N0")), fontH6, XBrushes.Black, totalPaymentTotalPaid, XStringFormats.TopLeft);    
                }
                gfx.DrawLine(tableLine5,
                    new XPoint(327, heightPosition10 + positionY10 + 15),
                    new XPoint(563, heightPosition10 + positionY10 + 15));
                gfx.DrawLine(tableLine5,
                    new XPoint(563, heightPosition10 + 245),
                    new XPoint(563, heightPosition10 + positionY10 + 15));
                gfx.DrawLine(tableLine5,
                    new XPoint(508, heightPosition10 + positionY10 + 15),
                    new XPoint(508, heightPosition10 + 245));
                gfx.DrawLine(tableLine5,
                    new XPoint(327, heightPosition10 + positionY10 + 15),
                    new XPoint(327, heightPosition10 + 245));
                // TOTAL (LAST ROW IN TABLE)
                XRect totalPaymentTotalAmount = new XRect(405, heightPosition10 + positionY10 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaymentTotalAmount.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalAmount, XStringFormats.TopLeft);
                XRect totalPaymentTotalPrincipalPaid = new XRect(340, heightPosition10 + positionY10 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaymentTotalPrincipalPaid.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalPrincipalPaid, XStringFormats.TopLeft);
                XRect totalPaymentTotalInterestPaid = new XRect(280, heightPosition10 + positionY10 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaymentTotalInterestPaid.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalInterestPaid, XStringFormats.TopLeft);
                XRect totalPaymentTotalPaidAmountBackground = new XRect(327.5, heightPosition10 + positionY10 + 0.5, 55, 14);
                gfx.DrawRectangle(new XSolidBrush(colorGreenTable), totalPaymentTotalPaidAmountBackground);
                XRect totalPaymentTotalTotalPaid = new XRect(225, heightPosition10 + positionY10 + 3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixPaymentTotalTotalPaid.ToString("N0")), fontH6Bold, XBrushes.Black, totalPaymentTotalTotalPaid, XStringFormats.TopLeft);
                // HEADER
                XRect appendixSectionTitleSummary = new XRect(136, 600, 100, 30);
                XPen appendixSectionTitleSummaryUnderline = new XPen(colorGreenTitle, 1);
                gfx.DrawLine(appendixSectionTitleSummaryUnderline,
                    new XPoint(200, 615),
                    new XPoint(240, 615));
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.appendixSectionTitleSummary), fontH2, new XSolidBrush(colorGreenTitle), appendixSectionTitleSummary, XStringFormats.TopLeft);
                //
                // LEFT BOTTOM BLOCK (3 ROWS)
                //
                gfx.DrawLine(tableLine5,
                  new XPoint(30, 675),
                  new XPoint(240, 675));
                gfx.DrawLine(tableLine5,
                   new XPoint(30, 690),
                   new XPoint(240, 690));
                XRect appendixSectionAverageLoanTime = new XRect(125, 635, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanTime), fontH6Bold, XBrushes.Black, appendixSectionAverageLoanTime, XStringFormats.TopLeft);
                XRect appendixSectionAverageLoanTimeValue = new XRect(95, 635, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AppendixSectionAverageLoanTime.ToString()), fontH6Bold, XBrushes.Black, appendixSectionAverageLoanTimeValue, XStringFormats.TopLeft);

                XRect appendixSectionAverageLoanRate = new XRect(125, 650, 111, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.offerAvarageLoanRate), fontH6Bold, XBrushes.Black, appendixSectionAverageLoanRate, XStringFormats.TopLeft);
                XRect appendixSectionAverageLoanRateValue = new XRect(95, 650, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AppendixSectionAverageLoanRate.ToString() + "%"), fontH6Bold, XBrushes.Black, appendixSectionAverageLoanRateValue, XStringFormats.TopLeft);

                XRect appendixSectionTitleTotalRepaymentCost = new XRect(80, 678, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.forEachShekelLoanHowMuchPaiedBack), fontH6Bold, XBrushes.Black, appendixSectionTitleTotalRepaymentCost, XStringFormats.TopLeft);
                XRect appendixSectionTitleTotalRepaymentCostValue = new XRect(30, 678, 50, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪ " + CheckRTL(longReportDataObject.AppendixSectionTotalRepaymentCost.ToString()), fontH6Bold, XBrushes.Black, appendixSectionTitleTotalRepaymentCostValue, XStringFormats.TopLeft);

                return true;
            }
            else return false;
        }
        /// <summary>
        /// Creates the second cover page.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateCoverPage2()
        {
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            gfx = XGraphics.FromPdfPage(page);
            string fullFilename = MiscUtilities.GetImagesDirectory() + "cover_page2.png";
            if (File.Exists(fullFilename))
            {
                XImage imageTop = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageTop, 0, 0, 594, 840);
            }
            else
            {
                Console.WriteLine("ERROR CreateCoverPage2 file: " + fullFilename + " does not exists");
            }
        }

        /// <summary>
        /// Creates the Amortisation schedule.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateAmortisationSchedulePage1(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // section title
            XRect appendixTileAmortisationSchedule = new XRect(60, 100, 500, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTileAmortisationSchedule), fontH1, new XSolidBrush(colorGreenTitle), appendixTileAmortisationSchedule, XStringFormats.TopLeft);
            // sec.1.1
            XRect appendixTextHeadline = new XRect(250, 160, 310, 30);
            XPen appendixTextHeadlineUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(appendixTextHeadlineUnderline,
                new XPoint(250, 177),
                new XPoint(563, 177));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTextHeadline), fontH2, new XSolidBrush(colorGreenTitle), appendixTextHeadline, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation3 = new XRect(85, 202, 100, 30);
            XPen titleAmortisation3Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation3Underline,
                new XPoint(30, 217),
                new XPoint(190, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation3), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation3, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation2 = new XRect(270, 202, 100, 30);
            XPen titleAmortisation2Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation3Underline,
                new XPoint(210, 217),
                new XPoint(370, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation2), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation2, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation1 = new XRect(450, 202, 100, 30);
            XPen titleAmortisation1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation1Underline,
                new XPoint(390, 217),
                new XPoint(550, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation1), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation1, XStringFormats.TopLeft);
            int positionX3 = 0;
            int positionY3 = -30;
            XPen tableLine3 = new XPen(XColors.Black, 1);
            int currentPositionY = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX3, currentPositionY + positionY3),
                    new XPoint(190 + positionX3, currentPositionY + positionY3));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX3, currentPositionY + 15 + positionY3),
                   new XPoint(190 + positionX3, currentPositionY + 15 + positionY3));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY += 15;
            }
            XRect titleTableAmortisationYear = new XRect(33 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative = new XRect(-5 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable = new XRect(-70 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX3, currentPositionY + 15 + positionY3),
                   new XPoint(190 + positionX3, currentPositionY + 15 + positionY3));
            int positionX4 = 180;
            int positionY4 = -30;
            int currentPositionY2 = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX4, currentPositionY2 + positionY4),
                    new XPoint(190 + positionX4, currentPositionY2 + positionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX4, currentPositionY2 + 15 + positionY4),
                   new XPoint(190 + positionX4, currentPositionY2 + 15 + positionY4));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY2 += 15;
            }
            XRect titleTableAmortisationYear2 = new XRect(33 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear2, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative2 = new XRect(-5 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative2, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable2 = new XRect(-70 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable2, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX4, currentPositionY2 + 15 + positionY4),
                   new XPoint(190 + positionX4, currentPositionY2 + 15 + positionY4));
            int positionX5 = 360;
            int positionY5 = -30;
            int currentPositionY5 = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX5, currentPositionY5 + positionY5),
                    new XPoint(190 + positionX5, currentPositionY5 + positionY5));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY5 += 15;
            }
            XRect titleTableAmortisationYear3 = new XRect(33 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear3, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative3 = new XRect(-5 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative3, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable3 = new XRect(-70 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable3, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
        }

        /// <summary>
        /// Creates the Amortisation schedule.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateAmortisationSchedulePage2(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // section title
            XRect appendixTileAmortisationSchedule = new XRect(60, 100, 500, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTileAmortisationSchedule2), fontH1, new XSolidBrush(colorGreenTitle), appendixTileAmortisationSchedule, XStringFormats.TopLeft);
            // sec.1.1
            XRect appendixTextHeadline = new XRect(250, 160, 310, 30);
            XPen appendixTextHeadlineUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(appendixTextHeadlineUnderline,
                new XPoint(250, 177),
                new XPoint(563, 177));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTextHeadline), fontH2, new XSolidBrush(colorGreenTitle), appendixTextHeadline, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation3 = new XRect(85, 202, 100, 30);
            XPen titleAmortisation3Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation3Underline,
                new XPoint(30, 217),
                new XPoint(190, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation3), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation3, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation2 = new XRect(270, 202, 100, 30);
            XPen titleAmortisation2Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation3Underline,
                new XPoint(210, 217),
                new XPoint(370, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation2), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation2, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation1 = new XRect(450, 202, 100, 30);
            XPen titleAmortisation1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation1Underline,
                new XPoint(390, 217),
                new XPoint(550, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation1), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation1, XStringFormats.TopLeft);
            int positionX3 = 0;
            int positionY3 = -30;
            XPen tableLine3 = new XPen(XColors.Black, 1);
            int currentPositionY = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX3, currentPositionY + positionY3),
                    new XPoint(190 + positionX3, currentPositionY + positionY3));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX3, currentPositionY + 15 + positionY3),
                   new XPoint(190 + positionX3, currentPositionY + 15 + positionY3));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX3, currentPositionY + 17 + positionY3, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i+1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY += 15;
            }

            XRect titleTableAmortisationYear = new XRect(33 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative = new XRect(-5 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable = new XRect(-70 + positionX3, 267 + positionY3, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX3, currentPositionY + 15 + positionY3),
                   new XPoint(190 + positionX3, currentPositionY + 15 + positionY3));
            int positionX4 = 180;
            int positionY4 = -30;
            int currentPositionY2 = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX4, currentPositionY2 + positionY4),
                    new XPoint(190 + positionX4, currentPositionY2 + positionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX4, currentPositionY2 + 15 + positionY4),
                   new XPoint(190 + positionX4, currentPositionY2 + 15 + positionY4));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX4, currentPositionY2 + 17 + positionY4, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY2 += 15;
            }
            XRect titleTableAmortisationYear2 = new XRect(33 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear2, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative2 = new XRect(-5 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative2, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable2 = new XRect(-70 + positionX4, 267 + positionY4, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable2, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX4, currentPositionY2 + 15 + positionY4),
                   new XPoint(190 + positionX4, currentPositionY2 + 15 + positionY4));
            int positionX5 = 360;
            int positionY5 = -30;
            int currentPositionY5 = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX5, currentPositionY5 + positionY5),
                    new XPoint(190 + positionX5, currentPositionY5 + positionY5));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("N0")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY5 += 15;
            }
            XRect titleTableAmortisationYear3 = new XRect(33 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear3, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative3 = new XRect(-5 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative3, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable3 = new XRect(-70 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable3, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
        }

        /// <summary>
        /// Creates the Amortisation schedule.
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateAmortisationSchedulePage3(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // section title
            XRect appendixTileAmortisationSchedule = new XRect(60, 100, 500, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTileAmortisationSchedule3), fontH1, new XSolidBrush(colorGreenTitle), appendixTileAmortisationSchedule, XStringFormats.TopLeft);
            // sec.1.1
            XRect appendixTextHeadline = new XRect(250, 160, 310, 30);
            XPen appendixTextHeadlineUnderline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(appendixTextHeadlineUnderline,
                new XPoint(250, 177),
                new XPoint(563, 177));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixTextHeadline), fontH2, new XSolidBrush(colorGreenTitle), appendixTextHeadline, XStringFormats.TopLeft);
            // Header Recommanded Structures 3
            XRect titleAmortisation1 = new XRect(450, 202, 100, 30);
            XPen titleAmortisation1Underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(titleAmortisation1Underline,
                new XPoint(390, 217),
                new XPoint(550, 217));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleAmortisation1), fontH3, new XSolidBrush(colorGreenTitle), titleAmortisation1, XStringFormats.TopLeft);
            XPen tableLine3 = new XPen(XColors.Black, 1);
            int positionX5 = 360;
            int positionY5 = -30;
            int currentPositionY5 = 265;
            for (int i = 0; i < 30; i++)
            {
                gfx.DrawLine(tableLine3,
                    new XPoint(30 + positionX5, currentPositionY5 + positionY5),
                    new XPoint(190 + positionX5, currentPositionY5 + positionY5));
                gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
                XRect tableAmortisation3Value1 = new XRect(27 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(longReportDataObject.AmortisationTableValues[i].Year.ToString()), fontH6Bold, XBrushes.Black, tableAmortisation3Value1, XStringFormats.TopLeft);
                XRect tableAmortisation3Value2 = new XRect(-7 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                XRect tableAmortisation3Value3 = new XRect(-70 + positionX5, currentPositionY5 + 17 + positionY5, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                if ((i + 1) % 5 == 0)
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("0,000")), fontH6Bold, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("0,000")), fontH6Bold, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                else
                {
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Cumulative.ToString("0,000")), fontH6, XBrushes.Black, tableAmortisation3Value2, XStringFormats.TopLeft);
                    tf.DrawString("₪ " + CheckRTL(longReportDataObject.AmortisationTableValues[i].Payable.ToString("0,000")), fontH6, XBrushes.Black, tableAmortisation3Value3, XStringFormats.TopLeft);
                }
                currentPositionY5 += 15;
            }
            XRect titleTableAmortisationYear3 = new XRect(33 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationYear), fontH6Bold, XBrushes.Black, titleTableAmortisationYear3, XStringFormats.TopLeft);
            XRect titleTableAmortisationCumulative3 = new XRect(-5 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationCumulative), fontH6Bold, XBrushes.Black, titleTableAmortisationCumulative3, XStringFormats.TopLeft);
            XRect titleTableAmortisationPayable3 = new XRect(-70 + positionX5, 267 + positionY5, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.titleTableAmortisationPayable), fontH6Bold, XBrushes.Black, titleTableAmortisationPayable3, XStringFormats.TopLeft);
            gfx.DrawLine(tableLine3,
                   new XPoint(30 + positionX5, currentPositionY5 + 15 + positionY5),
                   new XPoint(190 + positionX5, currentPositionY5 + 15 + positionY5));
        }

        /// <summary>
        /// Creates the Empty structures 
        /// (latest page)
        /// </summary>
        /// <remarks>
        /// Page is constant.
        /// </remarks>
        public void CreateEmptyStructuresPage(int number)
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = pageOrientationPortrait;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(number);

            // section title
            XRect appendixTileAmortisationSchedule = new XRect(60, 100, 500, 30);
            XPen underline = new XPen(colorGreenTitle, 1);
            gfx.DrawLine(underline,
                new XPoint(35, 122),
                new XPoint(563, 122));
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.appendixSectionTileEmptyStructures), fontH1, new XSolidBrush(colorGreenTitle), appendixTileAmortisationSchedule, XStringFormats.TopLeft);
            int positionX = 155;
            int positionY = -60;
            XPen tableLine3 = new XPen(XColors.Black, 1);
            gfx.DrawLine(tableLine3,
             new XPoint(250 + positionX, 220 + positionY),
             new XPoint(405 + positionX, 220 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(200 + positionX, 250 + positionY),
              new XPoint(405 + positionX, 250 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 265 + positionY),
              new XPoint(405 + positionX, 265 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX, 280 + positionY),
               new XPoint(405 + positionX, 280 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 295 + positionY),
              new XPoint(405 + positionX, 295 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX, 310 + positionY),
               new XPoint(405 + positionX, 310 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 325 + positionY),
              new XPoint(405 + positionX, 325 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX, 340 + positionY),
               new XPoint(405 + positionX, 340 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 355 + positionY),
              new XPoint(405 + positionX, 355 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX, 370 + positionY),
               new XPoint(405 + positionX, 370 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX, 385 + positionY),
              new XPoint(100 + positionX, 385 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(350 + positionX, 385 + positionY),
              new XPoint(405 + positionX, 385 + positionY));
            gfx.DrawLine(tableLine3,
             new XPoint(-32 + positionX, 265 + positionY),
             new XPoint(20 + positionX, 265 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX, 280 + positionY),
               new XPoint(20 + positionX, 280 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX, 295 + positionY),
              new XPoint(20 + positionX, 295 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX, 310 + positionY),
               new XPoint(20 + positionX, 310 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX, 325 + positionY),
              new XPoint(20 + positionX, 325 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX, 340 + positionY),
               new XPoint(20 + positionX, 340 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX, 355 + positionY),
              new XPoint(20 + positionX, 355 + positionY));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX, 370 + positionY),
               new XPoint(20 + positionX, 370 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX, 385 + positionY),
              new XPoint(20 + positionX, 385 + positionY));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX, 385 + positionY),
              new XPoint(20 + positionX, 385 + positionY));
            XRect sectionEmptyStructureAmount = new XRect(235 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureAmount), fontH6Bold, XBrushes.Black, sectionEmptyStructureAmount, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTypeRoute = new XRect(180 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTypeRoute), fontH6Bold, XBrushes.Black, sectionEmptyStructureTypeRoute, XStringFormats.TopLeft);
            XRect sectionEmptyStructureLinkage = new XRect(70 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureLinkage), fontH6Bold, XBrushes.Black, sectionEmptyStructureLinkage, XStringFormats.TopLeft);
            XRect sectionEmptyStructureInterest = new XRect(25 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureInterest), fontH6Bold, XBrushes.Black, sectionEmptyStructureInterest, XStringFormats.TopLeft);
            XRect sectionEmptyStructureMonths = new XRect(-15 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureMonths), fontH6Bold, XBrushes.Black, sectionEmptyStructureMonths, XStringFormats.TopLeft);
            XRect sectionEmptyStructureFirstRefund = new XRect(-60 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureFirstRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureFirstRefund, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTotalRefund = new XRect(-140 + positionX, 267 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTotalRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureTotalRefund, XStringFormats.TopLeft);
            XRect sectionEmptyStructureOfferDate = new XRect(254 + positionX, 210 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureOfferDate), fontH6Bold, XBrushes.Black, sectionEmptyStructureOfferDate, XStringFormats.TopLeft);
            XRect sectionEmptyStructureBank = new XRect(254 + positionX, 240 + positionY, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureBank), fontH6Bold, XBrushes.Black, sectionEmptyStructureBank, XStringFormats.TopLeft);
            int positionX6 = 155;
            int positionY6 = 150;
            gfx.DrawLine(tableLine3,
             new XPoint(250 + positionX6, 220 + positionY6),
             new XPoint(405 + positionX6, 220 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(200 + positionX6, 250 + positionY6),
              new XPoint(405 + positionX6, 250 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX6, 265 + positionY6),
              new XPoint(405 + positionX6, 265 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX6, 280 + positionY6),
               new XPoint(405 + positionX6, 280 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX6, 295 + positionY6),
              new XPoint(405 + positionX6, 295 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX6, 310 + positionY6),
               new XPoint(405 + positionX6, 310 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX6, 325 + positionY6),
              new XPoint(405 + positionX6, 325 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX6, 340 + positionY6),
               new XPoint(405 + positionX6, 340 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX6, 355 + positionY6),
              new XPoint(405 + positionX6, 355 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX6, 370 + positionY6),
               new XPoint(405 + positionX6, 370 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX6, 385 + positionY6),
              new XPoint(100 + positionX6, 385 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(350 + positionX6, 385 + positionY6),
              new XPoint(405 + positionX6, 385 + positionY6));
            gfx.DrawLine(tableLine3,
             new XPoint(-32 + positionX6, 265 + positionY6),
             new XPoint(20 + positionX6, 265 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX6, 280 + positionY6),
               new XPoint(20 + positionX6, 280 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX6, 295 + positionY6),
              new XPoint(20 + positionX6, 295 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX6, 310 + positionY6),
               new XPoint(20 + positionX6, 310 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX6, 325 + positionY6),
              new XPoint(20 + positionX6, 325 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX6, 340 + positionY6),
               new XPoint(20 + positionX6, 340 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX6, 355 + positionY6),
              new XPoint(20 + positionX6, 355 + positionY6));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX6, 370 + positionY6),
               new XPoint(20 + positionX6, 370 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX6, 385 + positionY6),
              new XPoint(20 + positionX6, 385 + positionY6));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX6, 385 + positionY6),
              new XPoint(20 + positionX6, 385 + positionY6));
            XRect sectionEmptyStructureAmount2 = new XRect(235 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureAmount), fontH6Bold, XBrushes.Black, sectionEmptyStructureAmount2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTypeRoute2 = new XRect(180 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTypeRoute), fontH6Bold, XBrushes.Black, sectionEmptyStructureTypeRoute2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureLinkage2 = new XRect(70 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureLinkage), fontH6Bold, XBrushes.Black, sectionEmptyStructureLinkage2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureInterest2 = new XRect(25 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureInterest), fontH6Bold, XBrushes.Black, sectionEmptyStructureInterest2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureMonths2 = new XRect(-15 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureMonths), fontH6Bold, XBrushes.Black, sectionEmptyStructureMonths2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureFirstRefund2 = new XRect(-60 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureFirstRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureFirstRefund2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTotalRefund2 = new XRect(-140 + positionX6, 267 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTotalRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureTotalRefund2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureOfferDate2 = new XRect(254 + positionX6, 210 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureOfferDate), fontH6Bold, XBrushes.Black, sectionEmptyStructureOfferDate2, XStringFormats.TopLeft);
            XRect sectionEmptyStructureBank2 = new XRect(254 + positionX6, 240 + positionY6, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureBank), fontH6Bold, XBrushes.Black, sectionEmptyStructureBank2, XStringFormats.TopLeft);
            int positionX7 = 155;
            int positionY7 = 360;
            gfx.DrawLine(tableLine3,
             new XPoint(250 + positionX7, 220 + positionY7),
             new XPoint(405 + positionX7, 220 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(200 + positionX7, 250 + positionY7),
              new XPoint(405 + positionX7, 250 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX7, 265 + positionY7),
              new XPoint(405 + positionX7, 265 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX7, 280 + positionY7),
               new XPoint(405 + positionX7, 280 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX7, 295 + positionY7),
              new XPoint(405 + positionX7, 295 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX7, 310 + positionY7),
               new XPoint(405 + positionX7, 310 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX7, 325 + positionY7),
              new XPoint(405 + positionX7, 325 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX7, 340 + positionY7),
               new XPoint(405 + positionX7, 340 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX7, 355 + positionY7),
              new XPoint(405 + positionX7, 355 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(45 + positionX7, 370 + positionY7),
               new XPoint(405 + positionX7, 370 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(45 + positionX7, 385 + positionY7),
              new XPoint(100 + positionX7, 385 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(350 + positionX7, 385 + positionY7),
              new XPoint(405 + positionX7, 385 + positionY7));
            gfx.DrawLine(tableLine3,
             new XPoint(-32 + positionX7, 265 + positionY7),
             new XPoint(20 + positionX7, 265 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX7, 280 + positionY7),
               new XPoint(20 + positionX7, 280 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX7, 295 + positionY7),
              new XPoint(20 + positionX7, 295 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX7, 310 + positionY7),
               new XPoint(20 + positionX7, 310 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX7, 325 + positionY7),
              new XPoint(20 + positionX7, 325 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX7, 340 + positionY7),
               new XPoint(20 + positionX7, 340 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX7, 355 + positionY7),
              new XPoint(20 + positionX7, 355 + positionY7));
            gfx.DrawLine(tableLine3,
               new XPoint(-32 + positionX7, 370 + positionY7),
               new XPoint(20 + positionX7, 370 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX7, 385 + positionY7),
              new XPoint(20 + positionX7, 385 + positionY7));
            gfx.DrawLine(tableLine3,
              new XPoint(-32 + positionX7, 385 + positionY7),
              new XPoint(20 + positionX7, 385 + positionY7));
            XRect sectionEmptyStructureAmount3 = new XRect(235 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureAmount), fontH6Bold, XBrushes.Black, sectionEmptyStructureAmount3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTypeRoute3 = new XRect(180 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTypeRoute), fontH6Bold, XBrushes.Black, sectionEmptyStructureTypeRoute3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureLinkage3 = new XRect(70 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureLinkage), fontH6Bold, XBrushes.Black, sectionEmptyStructureLinkage3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureInterest3 = new XRect(25 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureInterest), fontH6Bold, XBrushes.Black, sectionEmptyStructureInterest3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureMonths3 = new XRect(-15 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureMonths), fontH6Bold, XBrushes.Black, sectionEmptyStructureMonths3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureFirstRefund3 = new XRect(-60 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureFirstRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureFirstRefund3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureTotalRefund3 = new XRect(-140 + positionX7, 267 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureTotalRefund), fontH6Bold, XBrushes.Black, sectionEmptyStructureTotalRefund3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureOfferDate3 = new XRect(254 + positionX7, 210 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureOfferDate), fontH6Bold, XBrushes.Black, sectionEmptyStructureOfferDate3, XStringFormats.TopLeft);
            XRect sectionEmptyStructureBank3 = new XRect(254 + positionX7, 240 + positionY7, 150, 20);
            tf.Alignment = SetAlignmentForCulture();
            tf.DrawString(CheckRTL(Properties.Resources.sectionEmptyStructureBank), fontH6Bold, XBrushes.Black, sectionEmptyStructureBank3, XStringFormats.TopLeft);
        }
        
        #endregion
    }
}
