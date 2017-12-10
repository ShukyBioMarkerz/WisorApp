using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp;
using PdfSharp.Drawing.Layout;
using System.Text.RegularExpressions;
using System.IO;
using WisorLibrary.ReportApplication;
using WisorLibrary.Utilities;
using WisorLib;

namespace WisorLibrary.ReportApplication
{
    /// <summary>
    /// Creates full PDF Report or individual pages for Wisor system.
    /// </summary>
    class WisorReportManagerShort
    {
        private String documentFileName;
        private ShortReportDataObject shortReportDataObject;
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
        //private PageOrientation pageOrientationPortrait = PageOrientation.Landscape;

        /// <summary>
        /// Instantiates a new instance of the WisorReportManager class.
        /// </summary>
        /// <param name="filename">Report file name</param>
        /// <param name="cultureInfo">Specified culture identifier</param>
        /// <param name="data">Data from shortReportDataObject class for filling the report</param>
        public WisorReportManagerShort(String filename, CultureInfo cultureInfo, ShortReportDataObject data)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            documentFileName = filename;
            shortReportDataObject = data;

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
            string fullFilename = MiscUtilities.GetImagesDirectory() + "top_short.png";
            if (File.Exists(fullFilename))
            {
                XImage imageTop = XImage.FromFile(fullFilename);
                gfx.DrawImage(imageTop, 0, 0, 842, 26);
            }
            else
            {
                Console.WriteLine("ERROR Short::SetInitialViewOfPage file: " + fullFilename + " does not exists");
            }

            string BottomfullFilename = MiscUtilities.GetImagesDirectory() + "bottom.png";
            if (File.Exists(BottomfullFilename))
            {
                XImage imageBottom = XImage.FromFile(BottomfullFilename);
                gfx.DrawImage(imageBottom, 25, 770, 75, 55);
            }
            else
            {
                Console.WriteLine("ERROR Short::SetInitialViewOfPage file: " + BottomfullFilename + " does not exists");
            }

           
            // Page Footer Number
            XRect pageNumber = new XRect(420, 575, 111, 30);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(currentPageNumber.ToString(), fontH6, XBrushes.Black, pageNumber, XStringFormats.TopLeft);
            // Page Footer Title
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("he-IL").Name)
            {
                XRect pageFooterTitle = new XRect(730, 575, 100, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderFooterWisor), fontH6, XBrushes.Black, pageFooterTitle, XStringFormats.TopLeft);
            }
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-US").Name)
            {
                XRect pageFooterTitle = new XRect(5, 575, 100, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderFooterWisor), fontH6, XBrushes.Black, pageFooterTitle, XStringFormats.TopLeft);
            }
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-GB").Name)
            {
                XRect pageFooterTitle = new XRect(10, 575, 100, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderFooterWisor), fontH6, XBrushes.Black, pageFooterTitle, XStringFormats.TopLeft);

                XRect pageFooterConfidential = new XRect(780, 575, 100, 30);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("Confidential"), fontH6, XBrushes.Black, pageFooterConfidential, XStringFormats.TopLeft);
            }
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
                if (CreateFirstLenderPage())
                {
                    CreateSecondLenderPage();
                    // Save and start View 
                    document.Save(documentFileName);
                    if (Share.ShouldDisplayReportOnline)
                        Process.Start(documentFileName);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR Short::CreateFullReport Exception message: " + ex.Message);
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
                document.Save(documentFileName);
#if DEBUG
                Process.Start(documentFileName); // don't use in release
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateFirstLenderPage()
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = PageOrientation.Landscape;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(1);
            XPen tableLine3 = new XPen(XColors.Black, 1);

            #region israelReport
            //
            // IL
            //
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("he-IL").Name)
            {
                //
                // BIG RIGHT TABLE
                //
                int heightPosition2 = -105;
                int horizontPosition = 100;
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + 230),
                    new XPoint(685 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + 260),
                    new XPoint(685 + horizontPosition, heightPosition2 + 260));
                // TABLE TITLE
                XRect lenderTable1Title = new XRect(580 + horizontPosition, heightPosition2 + 200, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Title), fontH5Bold, XBrushes.Black, lenderTable1Title, XStringFormats.TopLeft);
                // PAGE TITLE
                XRect lenderCase = new XRect(200 + horizontPosition, heightPosition2 + 165, 300, 20);
                //525
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);
                // LABELS INSIDE TABLE
                XRect lenderTable1Future = new XRect(243 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Future), fontH6Bold, XBrushes.Black, lenderTable1Future, XStringFormats.TopLeft);
                XRect lenderTable1Balance = new XRect(314 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Balance), fontH6Bold, XBrushes.Black, lenderTable1Balance, XStringFormats.TopLeft);
                XRect lenderTable1Pay = new XRect(370 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Pay), fontH6Bold, XBrushes.Black, lenderTable1Pay, XStringFormats.TopLeft);
                XRect lenderTable1Return = new XRect(416 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Return), fontH6Bold, XBrushes.Black, lenderTable1Return, XStringFormats.TopLeft);
                XRect lenderTable1Indexed = new XRect(463 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Indexed), fontH6Bold, XBrushes.Black, lenderTable1Indexed, XStringFormats.TopLeft);
                XRect lenderTable1Rate = new XRect(494 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Rate), fontH6Bold, XBrushes.Black, lenderTable1Rate, XStringFormats.TopLeft);
                XRect lenderTable1Time = new XRect(525 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Time), fontH6Bold, XBrushes.Black, lenderTable1Time, XStringFormats.TopLeft);
                XRect lenderTable1Product = new XRect(579 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Product), fontH6Bold, XBrushes.Black, lenderTable1Product, XStringFormats.TopLeft);
                XRect lenderTable1Amount = new XRect(646 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Amount), fontH6Bold, XBrushes.Black, lenderTable1Amount, XStringFormats.TopLeft);
                
                // VALUES INSIDE TABLE
                int positionY2 = 260;
                int SumOfOriginalFuturePayment, SumOfOriginalBalanceToPay, SumOfOriginalPaySoFar, SumOfOriginalMonthlyPayment, SumOfOriginalLoanAmountTotal;
                SumOfOriginalFuturePayment = SumOfOriginalBalanceToPay = SumOfOriginalPaySoFar = SumOfOriginalMonthlyPayment = SumOfOriginalLoanAmountTotal = 0;
                for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTableValues.Length; i++)
                {
                    positionY2 += 30;
                    gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2),
                    new XPoint(685 + horizontPosition, heightPosition2 + positionY2));

                    XRect lenderTable1FutureValue = new XRect(247 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].FuturePayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable1FutureValue, XStringFormats.TopLeft);
                    SumOfOriginalFuturePayment += shortReportDataObject.ShortOriginalLoanTableValues[i].FuturePayment;
                    XRect lenderTable1BalanceValue = new XRect(315 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Balance.ToString("N0")), fontH6, XBrushes.Black, lenderTable1BalanceValue, XStringFormats.TopLeft);
                    SumOfOriginalBalanceToPay += shortReportDataObject.ShortOriginalLoanTableValues[i].Balance;
                    XRect lenderTable1PayValue = new XRect(370 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪ " + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].PaySoFar.ToString("N0")), fontH6, XBrushes.Black, lenderTable1PayValue, XStringFormats.TopLeft);
                    SumOfOriginalPaySoFar += shortReportDataObject.ShortOriginalLoanTableValues[i].PaySoFar;
                    XRect lenderTable1ReturnValue = new XRect(420 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].ReturnValue.ToString("N0")), fontH6, XBrushes.Black, lenderTable1ReturnValue, XStringFormats.TopLeft);
                    SumOfOriginalMonthlyPayment += shortReportDataObject.ShortOriginalLoanTableValues[i].ReturnValue;
                    XRect lenderTable1IndexedValue = new XRect(470 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(MiscUtilities.TranslateBoolToYesOrNo(shortReportDataObject.ShortOriginalLoanTableValues[i].Indexed)), fontH6, XBrushes.Black, lenderTable1IndexedValue, XStringFormats.TopLeft);
                    XRect lenderTable1RateValue = new XRect(490 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].Rate * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable1RateValue, XStringFormats.TopLeft);
                    XRect lenderTable1TimeValue = new XRect(528 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Time.ToString("N0")), fontH6, XBrushes.Black, lenderTable1TimeValue, XStringFormats.TopLeft);
                    XRect lenderTable1ProductValue = new XRect(557 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Product.ToString()), fontH6, XBrushes.Black, lenderTable1ProductValue, XStringFormats.TopLeft);
                    XRect lenderTable1AmountValue = new XRect(642 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable1AmountValue, XStringFormats.TopLeft);
                    SumOfOriginalLoanAmountTotal += shortReportDataObject.ShortOriginalLoanTableValues[i].Amount;
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(685 + horizontPosition, heightPosition2 + 230),
                   new XPoint(685 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(640 + horizontPosition, heightPosition2 + 230),
                    new XPoint(640 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(551 + horizontPosition, heightPosition2 + 230),
                    new XPoint(551 + horizontPosition, heightPosition2 + positionY2));
                gfx.DrawLine(tableLine3,
                   new XPoint(519 + horizontPosition, heightPosition2 + positionY2),
                   new XPoint(519 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(460 + horizontPosition, heightPosition2 + positionY2 + 15),
                  new XPoint(460 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(415 + horizontPosition, heightPosition2 + positionY2 + 15),
                   new XPoint(415 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(487 + horizontPosition, heightPosition2 + positionY2),
                   new XPoint(487 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(360 + horizontPosition, heightPosition2 + positionY2 + 15),
                  new XPoint(360 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(310 + horizontPosition, heightPosition2 + positionY2 + 15),
                   new XPoint(310 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(240 + horizontPosition, heightPosition2 + 230));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(460 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(640 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(685 + horizontPosition, heightPosition2 + positionY2 + 15));
                // TOTAL
                XRect lenderTable1FutureTotal = new XRect(247 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪" + CheckRTL(SumOfOriginalFuturePayment.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1FutureTotal, XStringFormats.TopLeft);
                XRect lenderTable1BalanceTotal = new XRect(314 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪" + CheckRTL(SumOfOriginalBalanceToPay.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1BalanceTotal, XStringFormats.TopLeft);
                XRect lenderTable1PayTotal = new XRect(370 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪" + CheckRTL(SumOfOriginalPaySoFar.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1PayTotal, XStringFormats.TopLeft);
                XRect lenderTable1ReturnTotal = new XRect(420 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪" + CheckRTL(SumOfOriginalMonthlyPayment.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1ReturnTotal, XStringFormats.TopLeft);
                XRect lenderTable1AmountTotal = new XRect(642 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₪" + CheckRTL(SumOfOriginalLoanAmountTotal.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1AmountTotal, XStringFormats.TopLeft);
                //
                // LITTLE LEFT TABLE
                //
                int horizontPosition2 = -405;
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(700 + horizontPosition2, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                    new XPoint(700 + horizontPosition2, heightPosition2 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable2ExpectedProfitPercent = new XRect(460 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfitPercent), fontH6Bold, XBrushes.Black, lenderTable2ExpectedProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable2ExpectedProfit = new XRect(525 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfit), fontH6Bold, XBrushes.Black, lenderTable2ExpectedProfit, XStringFormats.TopLeft);
                XRect lenderTable2ProfitPercent = new XRect(578 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ProfitPercent), fontH6Bold, XBrushes.Black, lenderTable2ProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable2Profit = new XRect(650 + horizontPosition2, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2Profit), fontH6Bold, XBrushes.Black, lenderTable2Profit, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                int PositionY3 = 260;
                int SumOfExpectedProfit, SumOfProfit;
                double SumOfExpectedProfitPercent, SumOfProfitPercent;
                SumOfExpectedProfit =  SumOfProfit = 0;
                SumOfExpectedProfitPercent = SumOfProfitPercent = 0;
                for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTableValues.Length; i++)
                {
                    PositionY3 += 30;
                    gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3),
                    new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3));

                    XRect lenderTable2FutureProfitPercentValue = new XRect(468 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfitPercantage * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable2FutureProfitPercentValue, XStringFormats.TopLeft);
                    SumOfExpectedProfitPercent += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfitPercantage * 100;
                    XRect lenderTable2FutureProfitValue = new XRect(525 + horizontPosition2, heightPosition2 + PositionY3 - 20, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable2FutureProfitValue, XStringFormats.TopLeft);
                    SumOfExpectedProfit += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfit;
                    XRect lenderTable2ProfitPercentValue = new XRect(588 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitPercantageSoFar * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable2ProfitPercentValue, XStringFormats.TopLeft);
                    SumOfProfitPercent += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitPercantageSoFar * 100;
                    XRect lenderTable2ProfitValue = new XRect(650 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitSoFar.ToString("N0")), fontH6, XBrushes.Black, lenderTable2ProfitValue, XStringFormats.TopLeft);
                    SumOfProfit += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitSoFar;
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(700 + horizontPosition2, heightPosition2 + 230),
                   new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(635 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(635 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(574 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                   new XPoint(511 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                   new XPoint(511 + horizontPosition2, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                  new XPoint(456 + horizontPosition2, heightPosition2 + 230));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                    new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                
                for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTableValues.Length; i++)
                {
                    // TOTAL
                    XRect lenderTable2FutureProfitPercentTotal = new XRect(466 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(SumOfExpectedProfitPercent.ToString("0.000")) + "%", fontH6Bold, XBrushes.Black, lenderTable2FutureProfitPercentTotal, XStringFormats.TopLeft);
                    XRect lenderTable2FutureProfitTotal = new XRect(524 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(SumOfExpectedProfit.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable2FutureProfitTotal, XStringFormats.TopLeft);
                    XRect lenderTable2ProfitPercentTotal = new XRect(588 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(SumOfProfitPercent.ToString("0.000")) + "%", fontH6Bold, XBrushes.Black, lenderTable2ProfitPercentTotal, XStringFormats.TopLeft);
                    XRect lenderTable2ProfitTotal = new XRect(648 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("₪" + CheckRTL(SumOfProfit.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable2ProfitTotal, XStringFormats.TopLeft);
                }
                    
                //
                // BOTTOM TABLE
                //
                int horizontPosition3 = -405;
                int heightPosition3 = 150;
                // TABLE TITLE
                XRect lenderTable3Title = new XRect(655 + horizontPosition3, heightPosition3 + 210, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Title), fontH5Bold, XBrushes.Black, lenderTable3Title, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + 260),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable3Number = new XRect(765 + horizontPosition3, heightPosition3 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("#"), fontH6Bold, XBrushes.Black, lenderTable3Number, XStringFormats.TopLeft);
                XRect lenderTable3Margin = new XRect(500 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Margin), fontH6Bold, XBrushes.Black, lenderTable3Margin, XStringFormats.TopLeft);
                XRect lenderTable3Rate = new XRect(595 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Rate), fontH6Bold, XBrushes.Black, lenderTable3Rate, XStringFormats.TopLeft);
                XRect lenderTable3Product = new XRect(703 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Product), fontH6Bold, XBrushes.Black, lenderTable3Product, XStringFormats.TopLeft);

                // VALUES INSIDE TABLE
                int PositionY4 = 260;
                for (int i = 0; i < shortReportDataObject.ShortProductsUsedInAnalysisTableValues.Length; i++)
                {
                    PositionY4 += 15;
                    gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + PositionY4),
                    new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));
                    XRect lenderTable3MarginValue = new XRect(493 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Margin.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3MarginValue, XStringFormats.TopLeft);
                    XRect lenderTable3RateValue = new XRect(595 + horizontPosition3, heightPosition3 + PositionY4 - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3RateValue, XStringFormats.TopLeft);
                    XRect lenderTable3ProductValue = new XRect(670 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Product.ToString()), fontH6, XBrushes.Black, lenderTable3ProductValue, XStringFormats.TopLeft);

                    XRect lenderTable3NumberValue = new XRect(765 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString((i + 1).ToString(), fontH6, XBrushes.Black, lenderTable3NumberValue, XStringFormats.TopLeft);
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(776 + horizontPosition3, heightPosition3 + 230),
                   new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(760 + horizontPosition3, heightPosition3 + PositionY4),
                   new XPoint(760 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(665 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(665 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                    new XPoint(564 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(564 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                  new XPoint(456 + horizontPosition3, heightPosition3 + PositionY4),
                  new XPoint(456 + horizontPosition3, heightPosition3 + 230));
                //
                // RIGHT ROWS
                //
                XRect lenderRowsDate = new XRect(1143 + horizontPosition3, heightPosition3 + 209, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsDate), fontH5, XBrushes.Black, lenderRowsDate, XStringFormats.TopLeft);
                XRect lenderRowsDateValue = new XRect(1000 + horizontPosition3, heightPosition3 + 209, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.GetDate().ToString("dd/MM/yyyy")), fontH5, XBrushes.Black, lenderRowsDateValue, XStringFormats.TopLeft);
                XRect lenderRowsProperty = new XRect(1131 + horizontPosition3, heightPosition3 + 223, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsProperty), fontH5, XBrushes.Black, lenderRowsProperty, XStringFormats.TopLeft);
                XRect lenderRowsPropertyValue = new XRect(1000 + horizontPosition3, heightPosition3 + 223, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₪" + shortReportDataObject.GetPropertyValue().ToString("N0")), fontH5, XBrushes.Black, lenderRowsPropertyValue, XStringFormats.TopLeft);

                XRect lenderRowsMonthly = new XRect(1112 + horizontPosition3, heightPosition3 + 237, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsMonthly), fontH5, XBrushes.Black, lenderRowsMonthly, XStringFormats.TopLeft);
                XRect lenderRowsMonthlyValue = new XRect(1000 + horizontPosition3, heightPosition3 + 237, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₪" + shortReportDataObject.GetIncome().ToString("N0")), fontH5, XBrushes.Black, lenderRowsMonthlyValue, XStringFormats.TopLeft);

                gfx.DrawLine(tableLine3,
                  new XPoint(1000 + horizontPosition3, heightPosition3 + 220),
                  new XPoint(1170 + horizontPosition3, heightPosition3 + 220));
                gfx.DrawLine(tableLine3,
                  new XPoint(1000 + horizontPosition3, heightPosition3 + 220 + 15),
                  new XPoint(1170 + horizontPosition3, heightPosition3 + 220 + 15));
            }

            #endregion
            //
            // If not en-US culture
            //
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-US").Name)
            {
                //
                // BIG LEFT TABLE
                //
                int heightPosition2 = -105;
                int horizontPosition = -190;
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + 230),
                    new XPoint(685 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + 260),
                    new XPoint(685 + horizontPosition, heightPosition2 + 260));
                // TABLE TITLE
                XRect lenderTable1Title = new XRect(240 + horizontPosition, heightPosition2 + 200 /*210*/, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Title), fontH5Bold, XBrushes.Black, lenderTable1Title, XStringFormats.TopLeft);
                // PAGE TITLE
                XRect lenderCase = new XRect(525 + horizontPosition, heightPosition2 + 165, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);
                // LABELS INSIDE TABLE
                XRect lenderTable1Future = new XRect(617 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Future), fontH6Bold, XBrushes.Black, lenderTable1Future, XStringFormats.TopLeft);
                XRect lenderTable1Balance = new XRect(562 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Balance), fontH6Bold, XBrushes.Black, lenderTable1Balance, XStringFormats.TopLeft);
                XRect lenderTable1Pay = new XRect(506 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Pay), fontH6Bold, XBrushes.Black, lenderTable1Pay, XStringFormats.TopLeft);
                XRect lenderTable1Return = new XRect(456 + horizontPosition, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Return), fontH6Bold, XBrushes.Black, lenderTable1Return, XStringFormats.TopLeft);
                //XRect lenderTable1Indexed = new XRect(424 + horizontPosition, heightPosition2 + 240, 57, 20);
                //tf.Alignment = SetAlignmentForCulture();
                //tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Indexed), fontH6Bold, XBrushes.Black, lenderTable1Indexed, XStringFormats.TopLeft);
                XRect lenderTable1Rate = new XRect(404 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Rate), fontH6Bold, XBrushes.Black, lenderTable1Rate, XStringFormats.TopLeft);
                XRect lenderTable1Time = new XRect(370 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Time), fontH6Bold, XBrushes.Black, lenderTable1Time, XStringFormats.TopLeft);
                XRect lenderTable1Product = new XRect(316 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Product), fontH6Bold, XBrushes.Black, lenderTable1Product, XStringFormats.TopLeft);
                XRect lenderTable1Amount = new XRect(253 + horizontPosition, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Amount), fontH6Bold, XBrushes.Black, lenderTable1Amount, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                int positionY2 = 260;
                int SumOfOriginalFuturePayment, SumOfOriginalBalanceToPay, SumOfOriginalPaySoFar, SumOfOriginalMonthlyPayment, SumOfOriginalLoanAmountTotal;
                SumOfOriginalFuturePayment = SumOfOriginalBalanceToPay = SumOfOriginalPaySoFar = SumOfOriginalMonthlyPayment = SumOfOriginalLoanAmountTotal = 0;
                int SumOfLenderProfit = 0, SumOfEstimateFutureProfit = 0;
                double SumOfLenderProfitPercantage = 0, SumOfEstimateFutureProfitPercantage = 0;

                for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTableValues.Length; i++)
                {
                    positionY2 += 30;
                    gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2),
                    new XPoint(685 + horizontPosition, heightPosition2 + positionY2));
 
                    XRect lenderTable1FutureValue = new XRect(630 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].FuturePayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable1FutureValue, XStringFormats.TopLeft);
                    SumOfOriginalFuturePayment += shortReportDataObject.ShortOriginalLoanTableValues[i].FuturePayment;
                    XRect lenderTable1BalanceValue = new XRect(560 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Balance.ToString("N0")), fontH6, XBrushes.Black, lenderTable1BalanceValue, XStringFormats.TopLeft);
                    SumOfOriginalBalanceToPay += shortReportDataObject.ShortOriginalLoanTableValues[i].Balance;
                    XRect lenderTable1PayValue = new XRect(506 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].PaySoFar.ToString("N0")), fontH6, XBrushes.Black, lenderTable1PayValue, XStringFormats.TopLeft);
                    SumOfOriginalPaySoFar += shortReportDataObject.ShortOriginalLoanTableValues[i].PaySoFar;
                    XRect lenderTable1ReturnValue = new XRect(456 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].ReturnValue.ToString("N0")), fontH6, XBrushes.Black, lenderTable1ReturnValue, XStringFormats.TopLeft);
                    SumOfOriginalMonthlyPayment += shortReportDataObject.ShortOriginalLoanTableValues[i].ReturnValue;
                    //XRect lenderTable1IndexedValue = new XRect(428 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    //tf.Alignment = SetAlignmentForCulture();
                    //tf.DrawString(CheckRTL(MiscUtilities.TranslateBoolToYesOrNo(shortReportDataObject.ShortOriginalLoanTableValues[i].Indexed)), fontH6, XBrushes.Black, lenderTable1IndexedValue, XStringFormats.TopLeft);
                    XRect lenderTable1RateValue = new XRect(400 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].Rate * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable1RateValue, XStringFormats.TopLeft);
                    XRect lenderTable1TimeValue = new XRect(372 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Time.ToString("N0")), fontH6, XBrushes.Black, lenderTable1TimeValue, XStringFormats.TopLeft);
                    XRect lenderTable1ProductValue = new XRect(310 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Product.ToString()), fontH6, XBrushes.Black, lenderTable1ProductValue, XStringFormats.TopLeft);
                    XRect lenderTable1AmountValue = new XRect(247 + horizontPosition, heightPosition2 + positionY2 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable1AmountValue, XStringFormats.TopLeft);
                    SumOfOriginalLoanAmountTotal += shortReportDataObject.ShortOriginalLoanTableValues[i].Amount;
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(685 + horizontPosition, heightPosition2 + 230),
                   new XPoint(685 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(610 + horizontPosition, heightPosition2 + 230),
                    new XPoint(610 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(555 + horizontPosition, heightPosition2 + 230),
                    new XPoint(555 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                   new XPoint(496 + horizontPosition, heightPosition2 + positionY2 + 15),
                   new XPoint(496 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(436 + horizontPosition, heightPosition2 + positionY2 + 15),
                  new XPoint(436 + horizontPosition, heightPosition2 + 230));
                //gfx.DrawLine(tableLine3,
                //   new XPoint(422 + horizontPosition, heightPosition2 + positionY2),
                //   new XPoint(422 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(394 + horizontPosition, heightPosition2 + positionY2),
                   new XPoint(394 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(365 + horizontPosition, heightPosition2 + positionY2),
                  new XPoint(365 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(295 + horizontPosition, heightPosition2 + positionY2 + 15),
                   new XPoint(295 + horizontPosition, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(240 + horizontPosition, heightPosition2 + 230));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(295 + horizontPosition, heightPosition2 + positionY2 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(436 + horizontPosition, heightPosition2 + positionY2 + 15),
                    new XPoint(685 + horizontPosition, heightPosition2 + positionY2 + 15));
                // TOTAL
                XRect lenderTable1FutureTotal = new XRect(630 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfOriginalFuturePayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable1FutureTotal, XStringFormats.TopLeft);
                XRect lenderTable1BalanceTotal = new XRect(560 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfOriginalBalanceToPay.ToString("N0")), fontH6, XBrushes.Black, lenderTable1BalanceTotal, XStringFormats.TopLeft);
                XRect lenderTable1PayTotal = new XRect(506 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfOriginalPaySoFar.ToString("N0")), fontH6, XBrushes.Black, lenderTable1PayTotal, XStringFormats.TopLeft);
                XRect lenderTable1ReturnTotal = new XRect(456 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfOriginalMonthlyPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable1ReturnTotal, XStringFormats.TopLeft);
                XRect lenderTable1AmountTotal = new XRect(247 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfOriginalLoanAmountTotal.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable1AmountTotal, XStringFormats.TopLeft);
                
                
                //
                // LITTLE RIGHT TABLE
                //
                int horizontPosition2 = 80;
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(700 + horizontPosition2, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                    new XPoint(700 + horizontPosition2, heightPosition2 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable2ExpectedProfitPercent = new XRect(638 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfitPercent), fontH6Bold, XBrushes.Black, lenderTable2ExpectedProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable2ExpectedProfit = new XRect(575 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfit), fontH6Bold, XBrushes.Black, lenderTable2ExpectedProfit, XStringFormats.TopLeft);
                XRect lenderTable2ProfitPercent = new XRect(515 + horizontPosition2, heightPosition2 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ProfitPercent), fontH6Bold, XBrushes.Black, lenderTable2ProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable2Profit = new XRect(460 + horizontPosition2, heightPosition2 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2Profit), fontH6Bold, XBrushes.Black, lenderTable2Profit, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                int PositionY3 = 260;
                for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTableValues.Length; i++)
                {
                    PositionY3 += 30;
                    gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3),
                    new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3));
                    XRect lenderTable2FutureProfitPercentValue = new XRect(645 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfitPercantage * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable2FutureProfitPercentValue, XStringFormats.TopLeft);
                    SumOfEstimateFutureProfitPercantage += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfitPercantage;
                    XRect lenderTable2FutureProfitValue = new XRect(580 + horizontPosition2, heightPosition2 + PositionY3 - 20, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable2FutureProfitValue, XStringFormats.TopLeft);
                    SumOfEstimateFutureProfit += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateFutureProfit;
                    XRect lenderTable2ProfitPercentValue = new XRect(530 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL((shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitPercantageSoFar * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable2ProfitPercentValue, XStringFormats.TopLeft);
                    SumOfLenderProfitPercantage += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitPercantageSoFar;
                    XRect lenderTable2ProfitValue = new XRect(465 + horizontPosition2, heightPosition2 + PositionY3 - 20, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString("$" + CheckRTL(shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitSoFar.ToString("N0")), fontH6, XBrushes.Black, lenderTable2ProfitValue, XStringFormats.TopLeft);
                    SumOfLenderProfit += shortReportDataObject.ShortOriginalLoanTableValues[i].EstimateProfitSoFar;
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(700 + horizontPosition2, heightPosition2 + 230),
                   new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(635 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(635 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                    new XPoint(574 + horizontPosition2, heightPosition2 + 230),
                    new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                gfx.DrawLine(tableLine3,
                   new XPoint(511 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                   new XPoint(511 + horizontPosition2, heightPosition2 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                  new XPoint(456 + horizontPosition2, heightPosition2 + 230));
                // TOTAL ROW
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                    new XPoint(700 + horizontPosition2, heightPosition2 + PositionY3 + 15));

                // TOTAL
                XRect lenderTable2FutureProfitPercentTotal = new XRect(645 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL((SumOfEstimateFutureProfitPercantage * 100).ToString("0.000")) + "%", fontH6, XBrushes.Black, lenderTable2FutureProfitPercentTotal, XStringFormats.TopLeft);
                XRect lenderTable2FutureProfitTotal = new XRect(580 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfEstimateFutureProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable2FutureProfitTotal, XStringFormats.TopLeft);
                XRect lenderTable2ProfitPercentTotal = new XRect(530 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL((SumOfLenderProfitPercantage * 100).ToString("0.000")) + "%", fontH6, XBrushes.Black, lenderTable2ProfitPercentTotal, XStringFormats.TopLeft);
                XRect lenderTable2ProfitTotal = new XRect(465 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("$" + CheckRTL(SumOfLenderProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable2ProfitTotal, XStringFormats.TopLeft);

                // BOTTOM TABLE
                //
                int horizontPosition3 = -405;
                int heightPosition3 = 150;
                // TABLE TITLE
                XRect lenderTable3Title = new XRect(240 + horizontPosition, heightPosition3 + 210, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Title), fontH5Bold, XBrushes.Black, lenderTable3Title, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + 260),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable3Margin = new XRect(707 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Margin), fontH6Bold, XBrushes.Black, lenderTable3Margin, XStringFormats.TopLeft);
                XRect lenderTable3Rate = new XRect(600 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Rate), fontH6Bold, XBrushes.Black, lenderTable3Rate, XStringFormats.TopLeft);
                XRect lenderTable3Product = new XRect(490 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Product), fontH6Bold, XBrushes.Black, lenderTable3Product, XStringFormats.TopLeft);
                XRect lenderTable3Number = new XRect(463 + horizontPosition3, heightPosition3 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("#"), fontH6Bold, XBrushes.Black, lenderTable3Number, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                int PositionY4 = 260;
                for (int i = 0; i < shortReportDataObject.ShortProductsUsedInAnalysisTableValues.Length; i++)
                {
                    PositionY4 += 15;
                    gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition3, heightPosition3 + PositionY4),
                    new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));
                    XRect lenderTable3MarginValue = new XRect(705 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Margin.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3MarginValue, XStringFormats.TopLeft);
                    XRect lenderTable3RateValue = new XRect(605 + horizontPosition3, heightPosition3 + PositionY4 - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3RateValue, XStringFormats.TopLeft);
                    XRect lenderTable3ProductValue = new XRect(490 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValues[i].Product.ToString()), fontH6, XBrushes.Black, lenderTable3ProductValue, XStringFormats.TopLeft);
                    XRect lenderTable3NumberValue = new XRect(462 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString((i + 1).ToString(), fontH6, XBrushes.Black, lenderTable3NumberValue, XStringFormats.TopLeft);
                }
                // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                gfx.DrawLine(tableLine3,
                   new XPoint(776 + horizontPosition3, heightPosition3 + 230),
                   new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                    new XPoint(675 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(675 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                    new XPoint(574 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(574 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(475 + horizontPosition3, heightPosition3 + PositionY4),
                   new XPoint(475 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(456 + horizontPosition3, heightPosition3 + PositionY4),
                  new XPoint(456 + horizontPosition3, heightPosition3 + 230));
                //
                // RIGHT ROWS
                //
                XRect lenderRowsDate = new XRect(1000 + horizontPosition3, heightPosition3 + 209, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsDate), fontH5, XBrushes.Black, lenderRowsDate, XStringFormats.TopLeft);
                XRect lenderRowsDateValue = new XRect(1110 + horizontPosition3, heightPosition3 + 209, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.GetDate().ToString("dd/MM/yyyy")), fontH5, XBrushes.Black, lenderRowsDateValue, XStringFormats.TopLeft);
                XRect lenderRowsProperty = new XRect(1000 + horizontPosition3, heightPosition3 + 223, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsProperty), fontH5, XBrushes.Black, lenderRowsProperty, XStringFormats.TopLeft);
                XRect lenderRowsPropertyValue = new XRect(1110 + horizontPosition3, heightPosition3 + 223, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("$" + shortReportDataObject.GetPropertyValue().ToString("N0")), fontH5, XBrushes.Black, lenderRowsPropertyValue, XStringFormats.TopLeft);
                XRect lenderRowsMonthly = new XRect(1000 + horizontPosition3, heightPosition3 + 237, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsMonthly), fontH5, XBrushes.Black, lenderRowsMonthly, XStringFormats.TopLeft);
                XRect lenderRowsMonthlyValue = new XRect(1110 + horizontPosition3, heightPosition3 + 237, 100, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("$" + shortReportDataObject.GetIncome().ToString("N0")), fontH5, XBrushes.Black, lenderRowsMonthlyValue, XStringFormats.TopLeft);

                gfx.DrawLine(tableLine3,
                  new XPoint(1000 + horizontPosition3, heightPosition3 + 220),
                  new XPoint(1170 + horizontPosition3, heightPosition3 + 220));
                gfx.DrawLine(tableLine3,
                  new XPoint(1000 + horizontPosition3, heightPosition3 + 220 + 15),
                  new XPoint(1170 + horizontPosition3, heightPosition3 + 220 + 15));
            }
            //
            // If en-GB culture
            //
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-GB").Name)
            {
                if (null == shortReportDataObject.OriginalLoanUKTableShortValue)
                {
                    Console.WriteLine("NOTICE: CreateFirstLenderPage found null  OriginalLoanUKTableShortValue.");
                    return false;
                }

                // PAGE TITLE
                XRect lenderCase = new XRect(335, 60, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);
                #region rightTable
                //
                // RIGHT TABLE
                //
                int horizontPosition3 = 0;
                int heightPosition3 = -100;
                // TABLE TITLE
                XRect lenderTable3Title = new XRect(428 + horizontPosition3, heightPosition3 + 210, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Title), fontH5Bold, XBrushes.Black, lenderTable3Title, XStringFormats.TopLeft);
                gfx.DrawLine(tableLine3,
                    new XPoint(428 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                    new XPoint(428 + horizontPosition3, heightPosition3 + 260),
                    new XPoint(776 + horizontPosition3, heightPosition3 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable3Margin = new XRect(707 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Margin), fontH6Bold, XBrushes.Black, lenderTable3Margin, XStringFormats.TopLeft);

                XRect lenderTable3InitialMargin = new XRect(650 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3MarginInitial), fontH6Bold, XBrushes.Black, lenderTable3InitialMargin, XStringFormats.TopLeft);

                XRect lenderTable3Rate = new XRect(586 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Rate), fontH6Bold, XBrushes.Black, lenderTable3Rate, XStringFormats.TopLeft);

                XRect lenderTable3InitialRate = new XRect(535 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3RateInitial), fontH6Bold, XBrushes.Black, lenderTable3InitialRate, XStringFormats.TopLeft);

                XRect lenderTable3Product = new XRect(470 + horizontPosition3, heightPosition3 + 240, 150, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Product), fontH6Bold, XBrushes.Black, lenderTable3Product, XStringFormats.TopLeft);

                XRect lenderTable3Number = new XRect(435 + horizontPosition3, heightPosition3 + 240, 57, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("#"), fontH6Bold, XBrushes.Black, lenderTable3Number, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                int PositionY4 = 260;
                for (int i = 0; i < shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK.Length; i++)
                {
                    PositionY4 += 15;
                    gfx.DrawLine(tableLine3,
                    new XPoint(428 + horizontPosition3, heightPosition3 + PositionY4),
                    new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));

                    XRect lenderTable3MarginValue = new XRect(725 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK[i].Margin2.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3MarginValue, XStringFormats.TopLeft);

                    XRect lenderTable3InitialMarginValue = new XRect(655 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK[i].Margin.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3InitialMarginValue, XStringFormats.TopLeft);

                    XRect lenderTable3InitialRateValue = new XRect(600 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK[i].Rate2.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3InitialRateValue, XStringFormats.TopLeft);
    
                    XRect lenderTable3RateValue = new XRect(540 + horizontPosition3, heightPosition3 + PositionY4 - 12, 100, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK[i].Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable3RateValue, XStringFormats.TopLeft);

                    XRect lenderTable3ProductValue = new XRect(452 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(shortReportDataObject.ShortProductsUsedInAnalysisTableValuesUK[i].Product.ToString()), fontH6, XBrushes.Black, lenderTable3ProductValue, XStringFormats.TopLeft);

                    XRect lenderTable3NumberValue = new XRect(434 + horizontPosition3, heightPosition3 + PositionY4 - 12, 150, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString((i + 1).ToString(), fontH6, XBrushes.Black, lenderTable3NumberValue, XStringFormats.TopLeft);
                }
                // VERTICAL LINES FROM TOP TO BOTTOM
                gfx.DrawLine(tableLine3,
                   new XPoint(776 + horizontPosition3, heightPosition3 + 230),
                   new XPoint(776 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                    new XPoint(703 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(703 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                    new XPoint(645 + horizontPosition3, heightPosition3 + 230),
                    new XPoint(645 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(583 + horizontPosition3, heightPosition3 + 230),
                   new XPoint(583 + horizontPosition3, heightPosition3 + PositionY4));
                gfx.DrawLine(tableLine3,
                   new XPoint(527 + horizontPosition3, heightPosition3 + PositionY4),
                   new XPoint(527 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                   new XPoint(447 + horizontPosition3, heightPosition3 + PositionY4),
                   new XPoint(447 + horizontPosition3, heightPosition3 + 230));
                gfx.DrawLine(tableLine3,
                  new XPoint(428 + horizontPosition3, heightPosition3 + PositionY4),
                  new XPoint(428 + horizontPosition3, heightPosition3 + 230));
                #endregion

                #region leftTable(original loan)
                //
                // LEFT TABLE
                //
                int horizontPosition2 = -350;
                int heightPosition2 = -130;
                gfx.DrawLine(tableLine3,
                    new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                    new XPoint(700 + horizontPosition2, heightPosition2 + 260));
                // LABELS INSIDE TABLE
                XRect lenderTable1Title = new XRect(457 + horizontPosition2, heightPosition2 + 245, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Title), fontH5Bold, XBrushes.Black, lenderTable1Title, XStringFormats.TopLeft);
                // VALUES INSIDE TABLE
                XRect lenderTable1Date = new XRect(457 + horizontPosition2, heightPosition2 + 263, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderRowsDate), fontH5, XBrushes.Black, lenderTable1Date, XStringFormats.TopLeft);
                XRect lenderTable1DateValue = new XRect(635 + horizontPosition2, heightPosition2 + 263, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.DateTaken.ToString("dd/MM/yyyy")), fontH5, XBrushes.Black, lenderTable1DateValue, XStringFormats.TopLeft);

                XRect lenderTable1Amount = new XRect(457 + horizontPosition2, heightPosition2 + 278, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Amount), fontH5, XBrushes.Black, lenderTable1Amount, XStringFormats.TopLeft);
                XRect lenderTable1AmountValue = new XRect(635 + horizontPosition2, heightPosition2 + 278, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString("₤" + CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.Amount.ToString("N0")), fontH5, XBrushes.Black, lenderTable1AmountValue, XStringFormats.TopLeft);

                XRect lenderTable1Product = new XRect(457 + horizontPosition2, heightPosition2 + 293, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable1Product), fontH5, XBrushes.Black, lenderTable1Product, XStringFormats.TopLeft);
                XRect lenderTable1ProductValue = new XRect(635 + horizontPosition2, heightPosition2 + 293, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.Product), fontH5, XBrushes.Black, lenderTable1ProductValue, XStringFormats.TopLeft);

                XRect lenderTable1Term = new XRect(457 + horizontPosition2, heightPosition2 + 308, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Term), fontH5, XBrushes.Black, lenderTable1Term, XStringFormats.TopLeft);
                XRect lenderTable1TermValue = new XRect(635 + horizontPosition2, heightPosition2 + 308, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.Term.ToString("N0")), fontH5, XBrushes.Black, lenderTable1TermValue, XStringFormats.TopLeft);

                XRect lenderTable1InitialRate = new XRect(457 + horizontPosition2, heightPosition2 + 323, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3InitialRate), fontH5, XBrushes.Black, lenderTable1InitialRate, XStringFormats.TopLeft);
                XRect lenderTable1InitialRateValue = new XRect(635 + horizontPosition2, heightPosition2 + 323, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.InitialRate.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1InitialRateValue, XStringFormats.TopLeft);

                XRect lenderTable1Rate = new XRect(457 + horizontPosition2, heightPosition2 + 338, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Rate), fontH5, XBrushes.Black, lenderTable1Rate, XStringFormats.TopLeft);
                XRect lenderTable1RateValue = new XRect(635 + horizontPosition2, heightPosition2 + 338, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.FollowOnRate.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1RateValue, XStringFormats.TopLeft);

                XRect lenderTable1DateOfRateChange = new XRect(457 + horizontPosition2, heightPosition2 + 353, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3DateOfRateChange), fontH5, XBrushes.Black, lenderTable1DateOfRateChange, XStringFormats.TopLeft);
                XRect lenderTable1DateOfRateChangeValue = new XRect(635 + horizontPosition2, heightPosition2 + 353, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.DateOfRateChange.ToString("dd/MM/yyyy")), fontH5, XBrushes.Black, lenderTable1DateOfRateChangeValue, XStringFormats.TopLeft);

                XRect lenderTable1FirstPMT = new XRect(457 + horizontPosition2, heightPosition2 + 368, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3FirstPMT), fontH5Bold, XBrushes.Black, lenderTable1FirstPMT, XStringFormats.TopLeft);
                XRect lenderTable1FirstPMTValue = new XRect(635 + horizontPosition2, heightPosition2 + 368, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.FirstPMT.ToString("N0")), fontH5Bold, XBrushes.Black, lenderTable1FirstPMTValue, XStringFormats.TopLeft);

                XRect lenderTable1FollowOnPMT = new XRect(457 + horizontPosition2, heightPosition2 + 383, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3FollowOnPMT), fontH5Bold, XBrushes.Black, lenderTable1FollowOnPMT, XStringFormats.TopLeft);
                XRect lenderTable1FollowOnPMTValue = new XRect(635 + horizontPosition2, heightPosition2 + 383, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.FollowOnPMT.ToString("N0")), fontH5Bold, XBrushes.Black, lenderTable1FollowOnPMTValue, XStringFormats.TopLeft);

                XRect lenderTable1PaidUntilToday = new XRect(457 + horizontPosition2, heightPosition2 + 398, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3PaidUntilToday), fontH5, XBrushes.Black, lenderTable1PaidUntilToday, XStringFormats.TopLeft);
                XRect lenderTable1PaidUntilTodayValue = new XRect(635 + horizontPosition2, heightPosition2 + 398, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.PaidUntilToday.ToString("N0")), fontH5, XBrushes.Black, lenderTable1PaidUntilTodayValue, XStringFormats.TopLeft);

                XRect lenderTable1LeftToPay = new XRect(457 + horizontPosition2, heightPosition2 + 413, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3LeftToPay), fontH5Bold, XBrushes.Black, lenderTable1LeftToPay, XStringFormats.TopLeft);
                XRect lenderTable1LeftToPayValue = new XRect(635 + horizontPosition2, heightPosition2 + 413, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.LeftToPay.ToString("N0")), fontH5Bold, XBrushes.Black, lenderTable1LeftToPayValue, XStringFormats.TopLeft);

                XRect lenderTable1FuturePayment = new XRect(457 + horizontPosition2, heightPosition2 + 428, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3FuturePayment), fontH5Bold, XBrushes.Black, lenderTable1FuturePayment, XStringFormats.TopLeft);
                XRect lenderTable1FuturePaymentValue = new XRect(635 + horizontPosition2, heightPosition2 + 428, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.FuturePayment.ToString("N0")), fontH5Bold, XBrushes.Black, lenderTable1FuturePaymentValue, XStringFormats.TopLeft);

                XRect lenderTable1TotalIncome = new XRect(457 + horizontPosition2, heightPosition2 + 458, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3TotalIncome), fontH5, XBrushes.Black, lenderTable1TotalIncome, XStringFormats.TopLeft);
                XRect lenderTable1TotalIncomeValue = new XRect(635 + horizontPosition2, heightPosition2 + 458, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.TotalIncome.ToString("N0")), fontH5, XBrushes.Black, lenderTable1TotalIncomeValue, XStringFormats.TopLeft);

                XRect lenderTable1PTIPercent = new XRect(457 + horizontPosition2, heightPosition2 + 473, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3PTIPercent), fontH5, XBrushes.Black, lenderTable1PTIPercent, XStringFormats.TopLeft);
                XRect lenderTable1PTIPercentValue = new XRect(635 + horizontPosition2, heightPosition2 + 473, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.PTI.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1PTIPercentValue, XStringFormats.TopLeft);

                XRect lenderTable1MarginInitial = new XRect(457 + horizontPosition2, heightPosition2 + 488, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3MarginInitial), fontH5Bold, XBrushes.Black, lenderTable1MarginInitial, XStringFormats.TopLeft);
                XRect lenderTable1MarginInitialValue = new XRect(635 + horizontPosition2, heightPosition2 + 488, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.InitialMargin.ToString("0.000")) + "%", fontH5Bold, XBrushes.Black, lenderTable1MarginInitialValue, XStringFormats.TopLeft);

                XRect lenderTable1Margin = new XRect(457 + horizontPosition2, heightPosition2 + 503, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable3Margin), fontH5, XBrushes.Black, lenderTable1Margin, XStringFormats.TopLeft);
                XRect lenderTable1MarginValue = new XRect(635 + horizontPosition2, heightPosition2 + 503, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.FollowUpMargin.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1MarginValue, XStringFormats.TopLeft);

                XRect lenderTable1ProfitPercent = new XRect(457 + horizontPosition2, heightPosition2 + 518, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ProfitPercent), fontH5, XBrushes.Black, lenderTable1ProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable1ProfitPercentValue = new XRect(635 + horizontPosition2, heightPosition2 + 518, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.EstimatePercentProfit.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1ProfitPercentValue, XStringFormats.TopLeft);

                XRect lenderTable1Profit = new XRect(457 + horizontPosition2, heightPosition2 + 533, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2Profit), fontH5, XBrushes.Black, lenderTable1Profit, XStringFormats.TopLeft);
                XRect lenderTable1ProfitValue = new XRect(635 + horizontPosition2, heightPosition2 + 533, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.EstimateProfit.ToString("N0")), fontH5, XBrushes.Black, lenderTable1ProfitValue, XStringFormats.TopLeft);


                XRect lenderTable1ExpectedPercentProfitTotal = new XRect(457 + horizontPosition2, heightPosition2 + 548, 350, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedPercentProfitTotal), fontH5, XBrushes.Black, lenderTable1ExpectedPercentProfitTotal, XStringFormats.TopLeft);
                XRect lenderTable1ExpectedPercentProfitTotalValue = new XRect(635 + horizontPosition2, heightPosition2 + 548, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.EstimateTotalPercentProfit.ToString("0.000")) + "%", fontH5, XBrushes.Black, lenderTable1ExpectedPercentProfitTotalValue, XStringFormats.TopLeft);


                XRect lenderTable1ExpectedProfitTotal = new XRect(457 + horizontPosition2, heightPosition2 + 563, 350, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfitTotal), fontH5, XBrushes.Black, lenderTable1ExpectedProfitTotal, XStringFormats.TopLeft);
                XRect lenderTable1ExpectedProfitTotalValue = new XRect(635 + horizontPosition2, heightPosition2 + 563, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.EstimateTotalProfit.ToString("N0")), fontH5, XBrushes.Black, lenderTable1ExpectedProfitTotalValue, XStringFormats.TopLeft);

                XRect lenderTable1ExpectedProfit = new XRect(457 + horizontPosition2, heightPosition2 + 578, 350, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfit), fontH5, XBrushes.Black, lenderTable1ExpectedProfit, XStringFormats.TopLeft);
                XRect lenderTable1ExpectedProfitValue = new XRect(635 + horizontPosition2, heightPosition2 + 578, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL("₤" + shortReportDataObject.OriginalLoanUKTableShortValue.EstimateFutureProfit.ToString("N0")), fontH5, XBrushes.Black, lenderTable1ExpectedProfitValue, XStringFormats.TopLeft);

                XRect lenderTable1ExpectedProfitPercent = new XRect(457 + horizontPosition2, heightPosition2 + 593, 350, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderTable2ExpectedProfitPercent), fontH5, XBrushes.Black, lenderTable1ExpectedProfitPercent, XStringFormats.TopLeft);
                XRect lenderTable1ExpectedProfitPercentValue = new XRect(635 + horizontPosition2, heightPosition2 + 593, 250, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(shortReportDataObject.OriginalLoanUKTableShortValue.EstimateFuturePercentProfit.ToString("0.000")) + "%", fontH5Bold, XBrushes.Black, lenderTable1ExpectedProfitPercentValue, XStringFormats.TopLeft);
                #endregion
            }

            return true;
        }

        public void CreateSecondLenderPage()
        {
            // Create an empty page and set view
            page = document.AddPage();
            page.Size = pageSizeA4;
            page.Orientation = PageOrientation.Landscape;
            // Get an XGraphics object for drawing
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            SetInitialViewOfPage(2);
            XPen tableLine3 = new XPen(XColors.Black, 1);


            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("he-IL").Name)
            {
                //int count = 2;
                int heightPosition2 = -105;
                int heightPosition3 = -75;
                int horizontPosition = 220;

                // PAGE TITLE
                XRect lenderCase = new XRect(350, heightPosition2 + 165, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);

                if (0 >= shortReportDataObject.NumberOfCompositions())
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else if (0 >= shortReportDataObject.NumberOfWinWinCompositions() && Share.ShouldCreateReportOnlyWhenWinWin)
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableWinWinCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else
                {

                    for (int j = 0; j < shortReportDataObject.NumberOfCompositions(); j++)
                    {
                        //
                        // BIG RIGHT TABLE
                        //
                        if (j != 0) heightPosition2 = heightPosition2 + 155;
						if (j != 0) heightPosition3 = heightPosition3 + 155;

                        gfx.DrawLine(tableLine3,
                            new XPoint(190 + horizontPosition, heightPosition2 + 230),
                            new XPoint(555 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(190 + horizontPosition, heightPosition2 + 260),
                            new XPoint(555 + horizontPosition, heightPosition2 + 260));
                        // TABLE TITLE
                        XRect lenderTable4Title = new XRect(150 + horizontPosition, heightPosition2 + 210, 1800, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        string compositionHeader = shortReportDataObject.GetCompositionHeader(j);
                        tf.DrawString(CheckRTL(compositionHeader), fontH5Bold, XBrushes.Black, lenderTable4Title, XStringFormats.TopLeft);
                        // LABELS INSIDE TABLE
                        XRect lenderTable4Total = new XRect(195 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Total), fontH6Bold, XBrushes.Black, lenderTable4Total, XStringFormats.TopLeft);
                        XRect lenderTable4Monthly = new XRect(258 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Monthly), fontH6Bold, XBrushes.Black, lenderTable4Monthly, XStringFormats.TopLeft);
                        XRect lenderTable4Indexed = new XRect(307 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Indexed), fontH6Bold, XBrushes.Black, lenderTable4Indexed, XStringFormats.TopLeft);
                        XRect lenderTable4Rate = new XRect(338 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Rate), fontH6Bold, XBrushes.Black, lenderTable4Rate, XStringFormats.TopLeft);
                        XRect lenderTable4Time = new XRect(372 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Time), fontH6Bold, XBrushes.Black, lenderTable4Time, XStringFormats.TopLeft);
                        XRect lenderTable4Product = new XRect(429 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Product), fontH6Bold, XBrushes.Black, lenderTable4Product, XStringFormats.TopLeft);
                        XRect lenderTable4Amount = new XRect(510 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Amount), fontH6Bold, XBrushes.Black, lenderTable4Amount, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int positionY2 = 260;
                        int NumberOfProductsInCompositions = shortReportDataObject.NumberOfProductsInCompositions(j);
                        for (int p = 0; p < NumberOfProductsInCompositions; p++)
                        {
                            OriginalLoanTable4Short olt4su = shortReportDataObject.GetProductInCompositionUSandIL(j, p);

                            positionY2 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(190 + horizontPosition, heightPosition2 + positionY2),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2));

                            XRect lenderTable4TotalValue = new XRect(200 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₪" + CheckRTL(olt4su.TotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalValue, XStringFormats.TopLeft);
                            XRect lenderTable4MonthlyValue = new XRect(263 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₪" + CheckRTL(olt4su.Monthly.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyValue, XStringFormats.TopLeft);
                            XRect lenderTable4IndexedValue = new XRect(308 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(MiscUtilities.TranslateBoolToYesOrNo(olt4su.Indexed)), fontH6, XBrushes.Black, lenderTable4IndexedValue, XStringFormats.TopLeft);
                            XRect lenderTable4RateValue = new XRect(337 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL((olt4su.Rate * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable4RateValue, XStringFormats.TopLeft);
                            XRect lenderTable4TimeValue = new XRect(374 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.Time.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TimeValue, XStringFormats.TopLeft);
                            XRect lenderTable4ProductValue = new XRect(412 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.Product.ToString()), fontH6, XBrushes.Black, lenderTable4ProductValue, XStringFormats.TopLeft);
                            XRect lenderTable4AmountValue = new XRect(507 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₪" + CheckRTL(olt4su.Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable4AmountValue, XStringFormats.TopLeft);
                        }
                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(555 + horizontPosition, heightPosition2 + 230),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(500 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(500 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(397 + horizontPosition, heightPosition2 + positionY2),
                          new XPoint(397 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(367 + horizontPosition, heightPosition2 + positionY2),
                           new XPoint(367 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(333 + horizontPosition, heightPosition2 + positionY2),
                           new XPoint(333 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(303 + horizontPosition, heightPosition2 + positionY2 + 15),
                          new XPoint(303 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(250 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(250 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(190 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(190 + horizontPosition, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(190 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(303 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                            new XPoint(500 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2 + 15));
                        // TOTAL
                        OriginalLoanTable4Short olt4s = shortReportDataObject.GetProductSummaryInCompositionUSandIL(j);
                        XRect lenderTable4TotalTotal = new XRect(505 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪" + CheckRTL(olt4s.Amount.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4TotalTotal, XStringFormats.TopLeft);
                        XRect lenderTable4MonthlyTotal = new XRect(200 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪" + CheckRTL(olt4s.TotalPayment.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4MonthlyTotal, XStringFormats.TopLeft);
                        XRect lenderTable4AmountTotal = new XRect(260 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪" + CheckRTL(olt4s.Monthly.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4AmountTotal, XStringFormats.TopLeft);
                        //
                        // LITTLE MIDDLE TABLE
                        //
                        int horizontPosition2 = -190;
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 260));
                        // LABELS INSIDE TABLE
                        XRect lenderTable5ProfitPercent = new XRect(461 + horizontPosition2, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5ProfitPercent), fontH6Bold, XBrushes.Black, lenderTable5ProfitPercent, XStringFormats.TopLeft);
                        XRect lenderTable5Profit = new XRect(521 + horizontPosition2, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5Profit), fontH6Bold, XBrushes.Black, lenderTable5Profit, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int PositionY3 = 260;
                        int SumOfLenderProfit = 0;
                        double SumOfLenderProfitPercantage = 0;
                        for (int i = 0; i < shortReportDataObject.NumberOfProductsInCompositions(j); i++)
                        {
                            int LenderProfit = shortReportDataObject.GetCompositionLenderProfit(j, i);
                            double LenderProfitPercantage = shortReportDataObject.GetCompositionLenderProfitPercantage(j, i);
                            SumOfLenderProfit += LenderProfit;
                            SumOfLenderProfitPercantage += LenderProfitPercantage;
                            PositionY3 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3));

                            XRect lenderTable5ProfitPercentValue = new XRect(463 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(LenderProfitPercantage.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable5ProfitPercentValue, XStringFormats.TopLeft);

                            XRect lenderTable5ProfitValue = new XRect(521 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₪" + CheckRTL(LenderProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable5ProfitValue, XStringFormats.TopLeft);
                        }
                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(511 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                           new XPoint(511 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                          new XPoint(456 + horizontPosition2, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        // TOTAL
                        XRect lenderTable5ProfitPercentTotal = new XRect(463 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(SumOfLenderProfitPercantage.ToString("0.000") + "%"), fontH6Bold, XBrushes.Black, lenderTable5ProfitPercentTotal, XStringFormats.TopLeft);
                        XRect lenderTable5ProfitTotal = new XRect(521 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₪" + CheckRTL(SumOfLenderProfit.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable5ProfitTotal, XStringFormats.TopLeft);
                        //
                        // RIGHT ROWS
                        //
                        int horizontPosition3 = -950;
                        XRect lenderRows2Borrower = new XRect(1115 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Borrower), fontH5Bold, XBrushes.Black, lenderRows2Borrower, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue = new XRect(1045 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();

                        double borrowerCanSavePercantage;
                        int borrowerCanSave = shortReportDataObject.BorrwerCanSave(j, out borrowerCanSavePercantage);
                        double LenderCanIncreasePercantage;
                        int LenderCanIncrease = shortReportDataObject.LenderCanIncrease(j, out LenderCanIncreasePercantage);

                        tf.DrawString(CheckRTL("₪" + borrowerCanSave.ToString("N0")), fontH5Bold, XBrushes.Black, lenderRows2BorrowerValue, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue2 = new XRect(1000 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(borrowerCanSavePercantage + "%"), fontH5Bold, XBrushes.Black, lenderRows2BorrowerValue2, XStringFormats.TopLeft);

                        XRect lenderRows2Lender = new XRect(1115 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Lender), fontH5Bold, XBrushes.Black, lenderRows2Lender, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue = new XRect(1040 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL("₪" + LenderCanIncrease.ToString("N0")), fontH5Bold, XBrushes.Black, lenderRows2LenderValue, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue2 = new XRect(1000 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(LenderCanIncreasePercantage + "%"), fontH5Bold, XBrushes.Black, lenderRows2LenderValue2, XStringFormats.TopLeft);

                        gfx.DrawLine(tableLine3,
                          new XPoint(995 + horizontPosition3, heightPosition3 + 220),
                          new XPoint(1200 + horizontPosition3, heightPosition3 + 220));
                    }
                }
            }
            // 
            // If en-US culture
            // 
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-US").Name)
            {
                //int count = 2;
                int heightPosition2 = -105;
                int heightPosition3 = -75;
                int horizontPosition = -190;

                // PAGE TITLE
                XRect lenderCase = new XRect(525 + horizontPosition, heightPosition2 + 165, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);

                if (0 >= shortReportDataObject.NumberOfCompositions())
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else if (0 >= shortReportDataObject.NumberOfWinWinCompositions() && Share.ShouldCreateReportOnlyWhenWinWin)
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableWinWinCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else
                {
                    // int SumOfAmount = 0, SumOfMonthlyPayment = 0, SumOfExpectedPayment = 0;
                    for (int j = 0; j < shortReportDataObject.NumberOfCompositions(); j++)
                    {
                        //
                        // BIG LEFT TABLE
                        //
                        if (j != 0) heightPosition2 = heightPosition2 + 155;
						if (j != 0) heightPosition3 = heightPosition3 + 155;

                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + 230),
                            new XPoint(555 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + 260),
                            new XPoint(555 + horizontPosition, heightPosition2 + 260));
                        // TABLE TITLE
                        XRect lenderTable4Title = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        string compositionHeader = shortReportDataObject.GetCompositionHeader(j);
                        tf.DrawString(CheckRTL(compositionHeader), fontH5Bold, XBrushes.Black, lenderTable4Title, XStringFormats.TopLeft);
                        // LABELS INSIDE TABLE
                        XRect lenderTable4Total = new XRect(505 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Total), fontH6Bold, XBrushes.Black, lenderTable4Total, XStringFormats.TopLeft);
                        XRect lenderTable4Monthly = new XRect(457 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Monthly), fontH6Bold, XBrushes.Black, lenderTable4Monthly, XStringFormats.TopLeft);
                        //XRect lenderTable4Indexed = new XRect(424 + horizontPosition, heightPosition2 + 240, 57, 20);
                        //tf.Alignment = SetAlignmentForCulture();
                        //tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Indexed), fontH6Bold, XBrushes.Black, lenderTable4Indexed, XStringFormats.TopLeft);
                        XRect lenderTable4Rate = new XRect(409 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Rate), fontH6Bold, XBrushes.Black, lenderTable4Rate, XStringFormats.TopLeft);
                        XRect lenderTable4Time = new XRect(369 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Time), fontH6Bold, XBrushes.Black, lenderTable4Time, XStringFormats.TopLeft);
                        XRect lenderTable4Product = new XRect(310 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Product), fontH6Bold, XBrushes.Black, lenderTable4Product, XStringFormats.TopLeft);
                        XRect lenderTable4Amount = new XRect(252 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Amount), fontH6Bold, XBrushes.Black, lenderTable4Amount, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int positionY2 = 260;
                        int NumberOfProductsInCompositions = shortReportDataObject.NumberOfProductsInCompositions(j);

                        for (int p = 0; p < NumberOfProductsInCompositions; p++)
                        {
                            OriginalLoanTable4Short olt4su2 = shortReportDataObject.GetProductInCompositionUSandIL(j, p);

                            positionY2 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + positionY2),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2));
                            
                            XRect lenderTable4TotalValue = new XRect(507 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("$" + CheckRTL(olt4su2.TotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalValue, XStringFormats.TopLeft);
                            // SumOfExpectedPayment += olt4su.TotalPayment;
                            XRect lenderTable4MonthlyValue = new XRect(457 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("$" + CheckRTL(olt4su2.Monthly.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyValue, XStringFormats.TopLeft);
                            // SumOfMonthlyPayment += olt4su.Monthly;
                            //XRect lenderTable4IndexedValue = new XRect(425 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            //tf.Alignment = SetAlignmentForCulture();
                            //tf.DrawString(CheckRTL(MiscUtilities.TranslateBoolToYesOrNo(olt4su.Indexed)), fontH6, XBrushes.Black, lenderTable4IndexedValue, XStringFormats.TopLeft);
                            XRect lenderTable4RateValue = new XRect(405 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL((olt4su2.Rate * 100).ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable4RateValue, XStringFormats.TopLeft);
                            XRect lenderTable4TimeValue = new XRect(368 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su2.Time.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TimeValue, XStringFormats.TopLeft);
                            XRect lenderTable4ProductValue = new XRect(305 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su2.Product.ToString()), fontH6, XBrushes.Black, lenderTable4ProductValue, XStringFormats.TopLeft);
                            XRect lenderTable4AmountValue = new XRect(252 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("$" + CheckRTL(olt4su2.Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable4AmountValue, XStringFormats.TopLeft);
                            // SumOfAmount += olt4su.Amount;
                        }
                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(555 + horizontPosition, heightPosition2 + 230),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(496 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(496 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(446 + horizontPosition, heightPosition2 + positionY2 + 15),
                          new XPoint(446 + horizontPosition, heightPosition2 + 230));
                        //gfx.DrawLine(tableLine3,
                        //   new XPoint(422 + horizontPosition, heightPosition2 + positionY2),
                        //   new XPoint(422 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(394 + horizontPosition, heightPosition2 + positionY2),
                           new XPoint(394 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(365 + horizontPosition, heightPosition2 + positionY2),
                          new XPoint(365 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(295 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(295 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(240 + horizontPosition, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(295 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                            new XPoint(446 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(555 + horizontPosition, heightPosition2 + positionY2 + 15));
                        // TOTAL
                        OriginalLoanTable4Short olt4su = shortReportDataObject.GetCompositionSummaryUSandIL(j);
                        XRect lenderTable4TotalTotal = new XRect(505 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("$" + CheckRTL(olt4su.TotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalTotal, XStringFormats.TopLeft);
                        XRect lenderTable4MonthlyTotal = new XRect(252 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("$" + CheckRTL(olt4su.Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyTotal, XStringFormats.TopLeft);
                        XRect lenderTable4AmountTotal = new XRect(457 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("$" + CheckRTL(olt4su.Monthly.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4AmountTotal, XStringFormats.TopLeft);
                        //
                        // LITTLE RIGHT TABLE
                        //
                        int horizontPosition2 = -60;
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 260));
                        // LABELS INSIDE TABLE
                        XRect lenderTable5ProfitPercent = new XRect(520 + horizontPosition2, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5ProfitPercent), fontH6Bold, XBrushes.Black, lenderTable5ProfitPercent, XStringFormats.TopLeft);
                        XRect lenderTable5Profit = new XRect(463 + horizontPosition2, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5Profit), fontH6Bold, XBrushes.Black, lenderTable5Profit, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int PositionY3 = 260;
                        int SumOfLenderProfit = 0;
                        double SumOfLenderProfitPercantage = 0;
                        for (int i = 0; i < NumberOfProductsInCompositions; i++)
                        {
                            PositionY3 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3));

                            int LenderProfit = shortReportDataObject.GetCompositionLenderProfit(j, i);
                            double LenderProfitPercantage = shortReportDataObject.GetCompositionLenderProfitPercantage(j, i);
                            SumOfLenderProfit += LenderProfit;
                            SumOfLenderProfitPercantage += LenderProfitPercantage;

                            XRect lenderTable5ProfitPercentValue = new XRect(527 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(LenderProfitPercantage.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable5ProfitPercentValue, XStringFormats.TopLeft);

                            XRect lenderTable5ProfitValue = new XRect(466 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("$ " + CheckRTL(LenderProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable5ProfitValue, XStringFormats.TopLeft);
                        }
                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(511 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                           new XPoint(511 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                          new XPoint(456 + horizontPosition2, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        // TOTAL
                        XRect lenderTable5ProfitPercentTotal = new XRect(527 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(SumOfLenderProfitPercantage.ToString("0.000") + " %"), fontH6, XBrushes.Black, lenderTable5ProfitPercentTotal, XStringFormats.TopLeft);
                        XRect lenderTable5ProfitTotal = new XRect(466 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("$ " + CheckRTL(SumOfLenderProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable5ProfitTotal, XStringFormats.TopLeft);
                        //
                        // RIGHT ROWS
                        //
                        double borrowerCanSavePercantage;
                        int borrowerCanSave = shortReportDataObject.BorrwerCanSave(j, out borrowerCanSavePercantage);
                        double LenderCanIncreasePercantage;
                        int LenderCanIncrease = shortReportDataObject.LenderCanIncrease(j, out LenderCanIncreasePercantage);

                        int horizontPosition3 = -430;
                        XRect lenderRows2Borrower = new XRect(1000 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Borrower), fontH5, XBrushes.Black, lenderRows2Borrower, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue = new XRect(1110 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL("$" + borrowerCanSave.ToString("N0")), fontH5, XBrushes.Black, lenderRows2BorrowerValue, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue2 = new XRect(1160 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(borrowerCanSavePercantage + "%"), fontH5, XBrushes.Black, lenderRows2BorrowerValue2, XStringFormats.TopLeft);

                        XRect lenderRows2Lender = new XRect(1000 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Lender), fontH5, XBrushes.Black, lenderRows2Lender, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue = new XRect(1110 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL("$" + LenderCanIncrease.ToString("N0")), fontH5, XBrushes.Black, lenderRows2LenderValue, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue2 = new XRect(1160 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(LenderCanIncreasePercantage + "%"), fontH5, XBrushes.Black, lenderRows2LenderValue2, XStringFormats.TopLeft);

                        gfx.DrawLine(tableLine3,
                          new XPoint(995 + horizontPosition3, heightPosition3 + 220),
                          new XPoint(1200 + horizontPosition3, heightPosition3 + 220));
                    }
                }
            }
            // ***
            // if en-GB (UK report)
            // *** new code
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == new CultureInfo("en-GB").Name)
            {
                //int count = shortReportDataObject.NumberOfCompositions(); /* 2;*/
                int heightPosition2 = -105;
                int heightPosition3 = -75;
                int horizontPosition = -190;

                // PAGE TITLE
                XRect lenderCase = new XRect(525 + horizontPosition, heightPosition2 + 165, 300, 20);
                tf.Alignment = SetAlignmentForCulture();
                tf.DrawString(CheckRTL(Properties.Resources.lenderCase + " " + shortReportDataObject.GetID()), fontH1, XBrushes.Black, lenderCase, XStringFormats.TopLeft);

                if (0 >= shortReportDataObject.NumberOfCompositions())
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else if (0 >= shortReportDataObject.NumberOfWinWinCompositions() && Share.ShouldCreateReportOnlyWhenWinWin)
                {
                    XRect noAvaliableCompositionsRect = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                    tf.Alignment = SetAlignmentForCulture();
                    tf.DrawString(CheckRTL(Properties.Resources.noAvaliableWinWinCompositions), fontH1, XBrushes.Black, noAvaliableCompositionsRect, XStringFormats.TopLeft);
                }
                else
                {

                    for (int j = 0; j < shortReportDataObject.NumberOfCompositions(); j++)
                    {
                        //
                        // BIG LEFT TABLE
                        //
                        if (j != 0) heightPosition2 = heightPosition2 + 155;
                        if (j != 0) heightPosition3 = heightPosition3 + 155;

                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + 230),
                            new XPoint(638 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + 260),
                            new XPoint(638 + horizontPosition, heightPosition2 + 260));
                        // TABLE TITLE
                        XRect lenderTable4Title = new XRect(240 + horizontPosition, heightPosition2 + 210, 800, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        // replace the header of the table coorectly
                        string compositionHeader = shortReportDataObject.GetCompositionHeader(j);
                        tf.DrawString(CheckRTL(compositionHeader), fontH5Bold, XBrushes.Black, lenderTable4Title, XStringFormats.TopLeft);
                        // LABELS INSIDE TABLE
                        XRect lenderTable4Total = new XRect(591 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Total), fontH6Bold, XBrushes.Black, lenderTable4Total, XStringFormats.TopLeft);
                        XRect lenderTable4Monthly = new XRect(532 + horizontPosition, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Monthly), fontH6Bold, XBrushes.Black, lenderTable4Monthly, XStringFormats.TopLeft);
                        XRect lenderTable4MonthlyPMT = new XRect(478 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4MonthlyPMT), fontH6Bold, XBrushes.Black, lenderTable4MonthlyPMT, XStringFormats.TopLeft);
                        XRect lenderTable4FollowOnRate = new XRect(429 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4FollowOnRate), fontH6Bold, XBrushes.Black, lenderTable4FollowOnRate, XStringFormats.TopLeft);
                        XRect lenderTable4Time = new XRect(400 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Time), fontH6Bold, XBrushes.Black, lenderTable4Time, XStringFormats.TopLeft);
                        XRect lenderTable4Rate = new XRect(372 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Rate), fontH6Bold, XBrushes.Black, lenderTable4Rate, XStringFormats.TopLeft);
                        XRect lenderTable4Amount = new XRect(326 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Amount), fontH6Bold, XBrushes.Black, lenderTable4Amount, XStringFormats.TopLeft);
                        XRect lenderTable4Product = new XRect(262 + horizontPosition, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable4Product), fontH6Bold, XBrushes.Black, lenderTable4Product, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int positionY2 = 260;
                        //for (int i = 0; i < shortReportDataObject.ShortOriginalLoanTable4ValuesUK.Length; i++)

                        //int NumberOfCompositions = shortReportDataObject.NumberOfCompositions();
                        //for (int i = 0; i < NumberOfCompositions; i++)
                        //{
                        // set the composition header properly by calling the CalculateTypeOfProducts
                        // each composition should reside in different table


                        int SummeryTotalAmount = 0, SummeryFirstPMT = 0, SummerySecondPMT = 0, SummeryTotalPayment = 0;
                        int NumberOfProductsInCompositions = shortReportDataObject.NumberOfProductsInCompositions(j);
                        for (int p = 0; p < NumberOfProductsInCompositions; p++)
                        {
                            OriginalLoanTable4ShortUK olt4su = shortReportDataObject.GetProductInComposition(j, p);

                            positionY2 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + positionY2),
                            new XPoint(638 + horizontPosition, heightPosition2 + positionY2));

                            XRect lenderTable4TotalValue = new XRect(589 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            // tf.DrawString("₤ " + CheckRTL(shortReportDataObject.ShortOriginalLoanTable4ValuesUK[i].TotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalValue, XStringFormats.TopLeft);
                            tf.DrawString("₤" + CheckRTL(olt4su.TotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalValue, XStringFormats.TopLeft);
                            SummeryTotalPayment += olt4su.TotalPayment;
                            XRect lenderTable4MonthlyValue = new XRect(543 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₤" + CheckRTL(olt4su.Monthly.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyValue, XStringFormats.TopLeft);
                            SummerySecondPMT += olt4su.Monthly;
                            XRect lenderTable4MonthlyPMTValue = new XRect(491 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₤" + CheckRTL(olt4su.MonthlyPMT.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyPMTValue, XStringFormats.TopLeft);
                            SummeryFirstPMT += olt4su.MonthlyPMT;
                            XRect lenderTable4FollowOnRateValue = new XRect(436 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.FollowOnRate.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable4FollowOnRateValue, XStringFormats.TopLeft);
                            XRect lenderTable4TimeValue = new XRect(400 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.Time.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TimeValue, XStringFormats.TopLeft);
                            XRect lenderTable4RateValue = new XRect(367 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.Rate.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable4RateValue, XStringFormats.TopLeft);
                            XRect lenderTable4AmountValue = new XRect(323 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₤" + CheckRTL(olt4su.Amount.ToString("N0")), fontH6, XBrushes.Black, lenderTable4AmountValue, XStringFormats.TopLeft);
                            SummeryTotalAmount += olt4su.Amount;
                            XRect lenderTable4ProductValue = new XRect(243 + horizontPosition, heightPosition2 + positionY2 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString(CheckRTL(olt4su.Product.ToString()), fontH6, XBrushes.Black, lenderTable4ProductValue, XStringFormats.TopLeft);
                        }

                        //}

                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(638 + horizontPosition, heightPosition2 + 230),
                            new XPoint(638 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(584 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(584 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(530 + horizontPosition, heightPosition2 + positionY2 + 15),
                          new XPoint(530 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                         new XPoint(476 + horizontPosition, heightPosition2 + positionY2 + 15),
                         new XPoint(476 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(422 + horizontPosition, heightPosition2 + positionY2),
                           new XPoint(422 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(397 + horizontPosition, heightPosition2 + positionY2),
                           new XPoint(397 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(365 + horizontPosition, heightPosition2 + positionY2 + 15),
                          new XPoint(365 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                           new XPoint(318 + horizontPosition, heightPosition2 + positionY2 + 15),
                           new XPoint(318 + horizontPosition, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(240 + horizontPosition, heightPosition2 + positionY2),
                            new XPoint(240 + horizontPosition, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(318 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(365 + horizontPosition, heightPosition2 + positionY2 + 15));
                        gfx.DrawLine(tableLine3,
                            new XPoint(476 + horizontPosition, heightPosition2 + positionY2 + 15),
                            new XPoint(638 + horizontPosition, heightPosition2 + positionY2 + 15));
                        // TOTAL
                        XRect lenderTable4TotalTotal = new XRect(589 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₤" + CheckRTL(SummeryTotalPayment.ToString("N0")), fontH6, XBrushes.Black, lenderTable4TotalTotal, XStringFormats.TopLeft);
                        XRect lenderTable4AmountTotal = new XRect(543 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₤" + CheckRTL(SummerySecondPMT.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4AmountTotal, XStringFormats.TopLeft);
                        XRect lenderTable4MonthlyPMTTotal = new XRect(487 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₤" + CheckRTL(SummeryFirstPMT.ToString("N0")), fontH6Bold, XBrushes.Black, lenderTable4MonthlyPMTTotal, XStringFormats.TopLeft);
                        XRect lenderTable4MonthlyTotal = new XRect(323 + horizontPosition, heightPosition2 + positionY2 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₤" + CheckRTL(SummeryTotalAmount.ToString("N0")), fontH6, XBrushes.Black, lenderTable4MonthlyTotal, XStringFormats.TopLeft);
                        
                        //
                        // LITTLE RIGHT TABLE
                        //
                        int horizontPosition2 = 5;
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + 260),
                            new XPoint(574 + horizontPosition2, heightPosition2 + 260));
                        // LABELS INSIDE TABLE
                        XRect lenderTable5ProfitPercent = new XRect(527 + horizontPosition2, heightPosition2 + 240, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5ProfitPercent), fontH6Bold, XBrushes.Black, lenderTable5ProfitPercent, XStringFormats.TopLeft);
                        XRect lenderTable5Profit = new XRect(459 + horizontPosition2, heightPosition2 + 240, 57, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderTable5Profit), fontH6Bold, XBrushes.Black, lenderTable5Profit, XStringFormats.TopLeft);
                        // VALUES INSIDE TABLE
                        int PositionY3 = 260;
                        double SumOfLenderProfitPercantage = MiscConstants.UNDEFINED_DOUBLE;

                        for (int i = 0; i < NumberOfProductsInCompositions; i++)
                        {
                            PositionY3 += 15;
                            gfx.DrawLine(tableLine3,
                            new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3));

                            XRect lenderTable5ProfitPercentValue = new XRect(530 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            int CompositionLenderProfit = shortReportDataObject.GetCompositionLenderProfit(j, i);
                            double CompositionLenderProfitPercantage = Math.Round(shortReportDataObject.GetCompositionLenderProfitPercantage(j, i), 3);
                            SumOfLenderProfitPercantage += CompositionLenderProfitPercantage;
                            tf.DrawString(CheckRTL(CompositionLenderProfitPercantage.ToString("0.000") + "%"), fontH6, XBrushes.Black, lenderTable5ProfitPercentValue, XStringFormats.TopLeft);
                            // tf.DrawString(CheckRTL(Math.Round(CompositionLenderProfitPercantage,3) + "%"), fontH6, XBrushes.Black, lenderTable5ProfitPercentValue, XStringFormats.TopLeft);
                            XRect lenderTable5ProfitValue = new XRect(465 + horizontPosition2, heightPosition2 + PositionY3 - 12, 150, 20);
                            tf.Alignment = SetAlignmentForCulture();
                            tf.DrawString("₤" + CheckRTL(CompositionLenderProfit.ToString("N0")), fontH6, XBrushes.Black, lenderTable5ProfitValue, XStringFormats.TopLeft);
                        }
                        // VERTICAL LINES FROM TOP TO BOTTOM (DRAW RIGHT TO LEFT)
                        gfx.DrawLine(tableLine3,
                            new XPoint(574 + horizontPosition2, heightPosition2 + 230),
                            new XPoint(574 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        gfx.DrawLine(tableLine3,
                           new XPoint(511 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                           new XPoint(511 + horizontPosition2, heightPosition2 + 230));
                        gfx.DrawLine(tableLine3,
                          new XPoint(456 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                          new XPoint(456 + horizontPosition2, heightPosition2 + 230));
                        // TOTAL ROW
                        gfx.DrawLine(tableLine3,
                            new XPoint(454 + horizontPosition2, heightPosition2 + PositionY3 + 15),
                            new XPoint(572 + horizontPosition2, heightPosition2 + PositionY3 + 15));
                        // TOTAL
                        XRect lenderTable5ProfitPercentTotal = new XRect(530 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        // double ExpectedFuturePercentTotal = shortReportDataObject.LenderTable2ExpectedFuturePercentTotal;
                        double LenderTable2LenderProfitPercantage = SumOfLenderProfitPercantage; //  shortReportDataObject.LenderTable2LenderProfitPercantage(j);
                        int LenderProfitTotal = shortReportDataObject.LenderTable2LenderProfitTotal(j);
                        tf.DrawString(CheckRTL(LenderTable2LenderProfitPercantage.ToString("0.000") + " %"), fontH6, XBrushes.Black, lenderTable5ProfitPercentTotal, XStringFormats.TopLeft);
                        XRect lenderTable5ProfitTotal = new XRect(475 + horizontPosition2, heightPosition2 + PositionY3 + 15 - 12, 150, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString("₤" + CheckRTL(LenderProfitTotal.ToString("N0")), fontH6, XBrushes.Black, lenderTable5ProfitTotal, XStringFormats.TopLeft);
                        //
                        // RIGHT ROWS
                        //
                        int horizontPosition3 = -400;
                        XRect lenderRows2Borrower = new XRect(1000 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Borrower), fontH5, XBrushes.Black, lenderRows2Borrower, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue = new XRect(1110 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        double borrowerCanSavePercantage;
                        int borrowerCanSave = shortReportDataObject.BorrwerCanSave(j, out borrowerCanSavePercantage);
                        tf.DrawString("₤" + CheckRTL(borrowerCanSave.ToString("N0")), fontH5, XBrushes.Black, lenderRows2BorrowerValue, XStringFormats.TopLeft);
                        XRect lenderRows2BorrowerValue2 = new XRect(1160 + horizontPosition3, heightPosition3 + 209, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(borrowerCanSavePercantage.ToString()) + " % ", fontH5, XBrushes.Black, lenderRows2BorrowerValue2, XStringFormats.TopLeft);

                        XRect lenderRows2Lender = new XRect(1000 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(Properties.Resources.lenderRows2Lender), fontH5, XBrushes.Black, lenderRows2Lender, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue = new XRect(1110 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        double LenderCanIncreasePercantage;
                        int lenderCanIncrease = shortReportDataObject.LenderCanIncrease(j, out LenderCanIncreasePercantage);
                        tf.DrawString("₤" + CheckRTL(lenderCanIncrease.ToString("N0")), fontH5, XBrushes.Black, lenderRows2LenderValue, XStringFormats.TopLeft);
                        XRect lenderRows2LenderValue2 = new XRect(1160 + horizontPosition3, heightPosition3 + 223, 100, 20);
                        tf.Alignment = SetAlignmentForCulture();
                        tf.DrawString(CheckRTL(LenderCanIncreasePercantage.ToString()) + " % ", fontH5, XBrushes.Black, lenderRows2LenderValue2, XStringFormats.TopLeft);

                        gfx.DrawLine(tableLine3,
                          new XPoint(995 + horizontPosition3, heightPosition3 + 220),
                          new XPoint(1200 + horizontPosition3, heightPosition3 + 220));
                    }
                }
            }
        }
    }
}
