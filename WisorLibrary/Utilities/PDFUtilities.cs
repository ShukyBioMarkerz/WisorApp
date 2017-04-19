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


namespace WisorLibrary.Utilities
{
    public class PDFUtilities
    {
        private static XGraphicsState state;

        public static void WriteFile(/*string filename*/)
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

    }
}
