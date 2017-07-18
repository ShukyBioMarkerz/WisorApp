using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using WisorLib;
using sharpPDF;
using sharpPDF.Enumerators;

namespace WisorLibrary.Utilities
{
    public class PDFUtilities
    {
        private static XGraphicsState state;
        //private static object txtOutput;

        public static void PdfTesting(/*string filename*/)
        {
            SharpPdfFlow();

            PdfSharpTesting();

        }

        static void PdfSharpTesting(/*string filename*/)
        {
            PdfDocument pdf = new PdfDocument();
            string filename = AppDomain.CurrentDomain.BaseDirectory + MiscConstants.OUTPUT_DIR + 
                Path.DirectorySeparatorChar + "firstpage.pdf";

            string line = "This is my first PDF document";
            int yPoint = 0;

            pdf.Info.Title = "My First PDF";
            pdf.Info.Title = "Wisor: The next best practice in mortgage lending";
            pdf.Info.Author = "Wisor team";
            pdf.Info.Subject = "Loan information";
            PdfPage pdfPage = pdf.AddPage();
            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            graph.DrawString(line, font, XBrushes.Black, new XRect(40, yPoint, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft /*Center*/);
            yPoint = yPoint + 40;

            DrawImage(pdf, graph, 1 /*index*/);

            pdf.Save(filename);
            Process.Start(filename);
        }

        static void DrawImage(PdfDocument pdf, XGraphics gfx, int number)
        {
            BeginBox(gfx, number, "DrawImage (original)");
            XImage image = XImage.FromFile(@"C:\Users\shukyWisor\Pictures\wisor-logo.jpg");
            // Left position in point
            double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
            gfx.DrawImage(image, x, 0);
            EndBox(gfx);
        }

        static void DrawTitle(PdfDocument pdf, PdfPage page, XGraphics gfx, string title)
        {
            int PageCount = 1;

            XRect rect = new XRect(new XPoint(), gfx.PageSize);
            rect.Inflate(-10, -15);
            XFont font = new XFont("Verdana", 14, XFontStyle.Bold);
            gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);
            rect.Offset(0, 5);
            font = new XFont("Verdana", 8, XFontStyle.Italic);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Far;
            gfx.DrawString("Created with " + PdfSharp.ProductVersionInfo.Producer, font, XBrushes.DarkOrchid, rect, format);
            font = new XFont("Verdana", 8);
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(PageCount.ToString(), font, XBrushes.DarkOrchid, rect, format);
            pdf.Outlines.Add(title, page, true);
        }

        static void BeginBox(XGraphics gfx, int number, string title)
        {
            const int dEllipse = 15;
            int borderWidth = 10;
            XColor shadowColor = new XColor();
            XColor backColor = new XColor(), backColor2 = new XColor();
            XPen borderPen = new XPen(backColor);
            XRect rect = new XRect(0, 20, 300, 200);
            if (number % 2 == 0)
                rect.X = 300 - 5;
            rect.Y = 40 + ((number - 1) / 2) * (200 - 5);
            rect.Inflate(-10, -10);
            XRect rect2 = rect;
            rect2.Offset(borderWidth, borderWidth);
            gfx.DrawRoundedRectangle(new XSolidBrush(shadowColor), rect2, new XSize(dEllipse + 8, dEllipse + 8));
            XLinearGradientBrush brush = new XLinearGradientBrush(rect, backColor, backColor2, XLinearGradientMode.Vertical);
            gfx.DrawRoundedRectangle(borderPen, brush, rect, new XSize(dEllipse, dEllipse));
            rect.Inflate(-5, -5);
            XFont font = new XFont("Verdana", 12, XFontStyle.Regular);
            gfx.DrawString(title, font, XBrushes.Navy, rect, XStringFormats.TopCenter);
            rect.Inflate(-10, -5);
            rect.Y += 20;
            rect.Height -= 20;
            state = gfx.Save();
            gfx.TranslateTransform(rect.X, rect.Y);
        }

        static void EndBox(XGraphics gfx)
        {
            gfx.Restore(state);
        }

#if DEBUG_NEW_PDF
#else

        static void SharpPdfFlow()
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + MiscConstants.OUTPUT_DIR +
                Path.DirectorySeparatorChar + "firstpage2.pdf";

            SharpPdfChangeObjectColor(filename);

            SharpPdfDrawElemtOnPage(filename);

            SharpPdfTable(filename);
        }

        static void SharpPdfChangeObjectColor(string filename)
        {
            pdfDocument myDoc = new pdfDocument("TUTORIAL", "I'm the authur");
            pdfPage myPage = myDoc.addPage(100, 100);
            /*Use the predefined Colors*/
            myPage.addText("Hello World!", 70, 140, predefinedFont.csHelvetica, 10, new pdfColor(predefinedColor.csCyan));
            /*Use the RGB Colors*/
            myPage.addText("Hello World!", 70, 100, predefinedFont.csHelvetica, 10, new pdfColor(112, 27, 184));
            /*Use the Hex Colors*/
            myPage.addText("Hello World!", 70, 60, predefinedFont.csHelvetica, 10, new pdfColor("B81C74"));
            myDoc.createPDF(filename);
            myPage = null;
            myDoc = null;
        }

        static void SharpPdfDrawElemtOnPage(string filename)
        {
            pdfDocument myDoc = new pdfDocument("TUTORIAL", "ME");
            /*Creation of the first page*/
            pdfPage myFirstPage = myDoc.addPage();
            /*Draw the line on the first page*/
            myFirstPage.drawLine(100, 100, 200, 200, predefinedLineStyle.csNormal, new pdfColor(predefinedColor.csBlue), 10);
            /*Creation of the second page*/
            pdfPage mySecondPage = myDoc.addPage();
            /*Draw the rectangle on the second page*/
            mySecondPage.drawRectangle(100, 100, 300, 200, new pdfColor(predefinedColor.csBlue), new pdfColor(predefinedColor.csYellow), 1, predefinedLineStyle.csNormal);
            /*Creation of the third page*/
            pdfPage myThirdPage = myDoc.addPage();
            /*Draw the circle on the third page-*/
            myThirdPage.drawCircle(200, 200, 50, new pdfColor(predefinedColor.csBlue), new pdfColor(predefinedColor.csYellow), predefinedLineStyle.csNormal, 1);
            myDoc.createPDF(filename);
            myFirstPage = null;
            mySecondPage = null;
            myThirdPage = null;
            myDoc = null;
        }

        static void SharpPdfTable(string filename)
        {
            pdfDocument myDoc = new pdfDocument("Sample Application", "Me", false);
            pdfPage myFirstPage = myDoc.addPage();
            myFirstPage.addText("sharpPDF 1.3 - Table Sample", 100, 660, predefinedFont.csHelveticaOblique, 30, new pdfColor(predefinedColor.csCyan));
            /*Table's creation*/
            pdfTable myTable = new pdfTable();
            //Set table's border
            myTable.borderSize = 1;
            myTable.borderColor = new pdfColor(predefinedColor.csDarkBlue);
            /*Create table's header*/
            myTable.tableHeader.addColumn(new pdfTableColumn("id", predefinedAlignment.csRight, 50));
            myTable.tableHeader.addColumn(new pdfTableColumn("user", predefinedAlignment.csCenter, 150));
            myTable.tableHeader.addColumn(new pdfTableColumn("tel", predefinedAlignment.csLeft, 80));
            myTable.tableHeader.addColumn(new pdfTableColumn("email", predefinedAlignment.csLeft, 150));
            /*Create table's rows*/
            pdfTableRow myRow = myTable.createRow();
            myRow[0].columnValue = "1";
            myRow[1].columnValue = "Andrew Red";
            myRow[2].columnValue = "898-0210989";
            myRow[3].columnValue = "Andrew.red@sharppdf.net";
            myTable.addRow(myRow);
            myRow = myTable.createRow();
            myRow[0].columnValue = "2";
            myRow[1].columnValue = "Andrew Green";
            myRow[2].columnValue = "298-55109";
            myRow[3].columnValue = "Andrew.green@sharppdf.net";
            myTable.addRow(myRow);
            myRow = myTable.createRow();
            myRow[0].columnValue = "3";
            myRow[1].columnValue = "Andrew White";
            myRow[2].columnValue = "24-5510943";
            myRow[3].columnValue = "Andrew.white@sharppdf.net";
            /*Set Header's Style*/
            myTable.tableHeaderStyle = new pdfTableRowStyle(predefinedFont.csCourierBoldOblique, 12, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csLightCyan));
            /*Set Row's Style*/
            myTable.rowStyle = new pdfTableRowStyle(predefinedFont.csCourier, 8, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csWhite));
            /*Set Alternate Row's Style*/
            myTable.alternateRowStyle = new pdfTableRowStyle(predefinedFont.csCourier, 8, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csLightYellow));
            /*Set Cellpadding*/
            myTable.cellpadding = 20;
            /*Put the table on the page object*/
            myFirstPage.addTable(myTable, 100, 600);
            myTable = null;

            myDoc.createPDF(filename);
            // Process.Start(filename);
        }
#endif

    }
}
