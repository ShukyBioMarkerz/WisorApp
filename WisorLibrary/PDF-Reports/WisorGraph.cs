using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;

namespace WisorLibrary.ReportApplication
{
    /// <summary>
    /// Creates a column graph for Wisor Report.
    /// </summary>
    /// <remarks>
    /// Customizable dimensions of the graph and position on the page.
    /// </remarks>
    class WisorGraph
    {
        private double minPoint1 = 0;
        private double maxPoint1 = 0;
        private double minPoint2 = 0;
        private double maxPoint2 = 0;
        private double maxTicks = 10;
        private double tickSpacing;
        private double range;
        private double niceMin;
        private double niceMax;
        private int frameWidth = 300;
        private int frameHeight = 200;
        private int framePositionX = 20;
        private int framePositionY = 300;
        private int indentChartSection = 25;
        private int chartWidth = 0;
        private int chartHeight = 0;
        private Point[] pointsXY1;
        private Point[] pointsXY2;
        private XGraphics gfx;
        private XGraphicsState state;
        private XFont font;
        private XFont font2;
        private XColor colorGreenChart = XColor.FromArgb(255, 57, 181, 74);
        private XColor colorLightGreenChart = XColor.FromArgb(255, 153, 223, 163);
        private XColor colorLightGrayChart = XColor.FromArgb(255, 217, 217, 217);
        private XColor colorDarkGrayChartTitle = XColor.FromArgb(255, 89, 89, 89);
        private bool multiSeries = false;

        /// <summary>
        /// Instantiates a new instance of the WisorGraph class.
        /// </summary>
        /// <param name="data">The Point[] data with X and Y values for plot the graph</param>
        /// <param name="xgfx">Drawing surface</param>
        /// <param name="x">Position on page by x coordinate</param>
        /// <param name="y">Position on page by y coordinate</param>
        /// <param name="width">Width of the graph</param>
        /// <param name="height">Height of the graph</param>
        public WisorGraph(Point[] data, XGraphics xgfx, int x, int y, int width, int height)
        {
            XPdfFontOptions fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
            font = new XFont("Arial", 12, XFontStyle.Regular, fontOptions);
            font2 = new XFont("Arial", 4.5, XFontStyle.Regular, fontOptions);
            frameWidth = width;
            frameHeight = height;
            framePositionX = x;
            framePositionY = y;
            pointsXY1 = data;
            gfx = xgfx;
            // calculate chart size
            chartWidth = frameWidth - indentChartSection * 2;
            chartHeight = frameHeight - indentChartSection * 2;
            // Find max value in Poin[] data and calculate tickSpacing
            minPoint1 = findMinValueOnAxisY(data);
            maxPoint1 = findMaxValueOnAxisY(data);
            Calculate(maxPoint1, minPoint1);
        }

        /// <summary>
        /// Instantiates a new instance of the WisorGraph class.
        /// </summary>
        /// <param name="data">The Point[] data with X and Y values for plot the graph</param>
        /// <param name="xgfx">Drawing surface</param>
        /// <param name="x">Position on page by x coordinate</param>
        /// <param name="y">Position on page by y coordinate</param>
        /// <param name="width">Width of the graph</param>
        /// <param name="height">Height of the graph</param>
        public WisorGraph(Point[] data1, Point[] data2, XGraphics xgfx, int x, int y, int width, int height)
        {
            XPdfFontOptions fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
            font = new XFont("Arial", 12, XFontStyle.Regular, fontOptions);
            font2 = new XFont("Arial", 4.5, XFontStyle.Regular, fontOptions);
            frameWidth = width;
            frameHeight = height;
            framePositionX = x;
            framePositionY = y;
            pointsXY1 = data1;
            pointsXY2 = data2;
            gfx = xgfx;
            // calculate chart size
            chartWidth = frameWidth - indentChartSection * 2;
            chartHeight = frameHeight - indentChartSection * 2;
            // Find max value in Poin[] data and calculate tickSpacing
            minPoint1 = findMinValueOnAxisY(pointsXY1);
            maxPoint1 = findMaxValueOnAxisY(pointsXY1);
            minPoint2 = findMinValueOnAxisY(pointsXY2);
            maxPoint2 = findMaxValueOnAxisY(pointsXY2);
            Calculate(getMaxValue(maxPoint1, maxPoint2), getMinValue(minPoint1, minPoint2));
            multiSeries = true;
        }

        /// <summary>
        /// Find max value in array.
        /// </summary>
        /// <returns>Max value on AxisY</returns>
        private double findMaxValueOnAxisY(Point[] data)
        {
            double max = Int32.MinValue;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Y > max)
                {
                    max = data[i].Y;
                }
            }
            return max;
        }

        /// <summary>
        /// Find min value in array.
        /// </summary>
        /// <returns>Min value on AxisY</returns>
        private double findMinValueOnAxisY(Point[] data)
        {
            double min = Int32.MaxValue;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Y < min)
                {
                    min = data[i].Y;
                }
            }
            return min;
        }

        /// <summary>
        /// Compare two values and return max.
        /// </summary>
        /// <returns>max value</returns>
        private double getMaxValue(double value1, double value2)
        {
            if (value1 > value2)
                return value1;
            else if (value2 > value1)
                return value2;
            else return value1; //equal
        }

        /// <summary>
        /// Compare two values and return min.
        /// </summary>
        /// <returns>min value</returns>
        private double getMinValue(double value1, double value2)
        {
            if (value1 < value2)
                return value1;
            else if (value2 < value1)
                return value2;
            else return value1; //equal
        }

        /// <summary>
        /// Create Wisor Graph on document.
        /// </summary>
        /// <param name="title">String value for upper title on the graph</param>
        public void CreateGraph(String title)
        {
            string graphTitle = title;
            XRect chart = new XRect(framePositionX + indentChartSection, framePositionY + indentChartSection, chartWidth, chartHeight);
            XRect frame = new XRect(framePositionX, framePositionY, frameWidth, frameHeight);
            XPen borderPenWhite = new XPen(XColors.White, 1);
            XPen borderPenGray = new XPen(colorLightGrayChart, 1);
            gfx.DrawRectangle(borderPenGray, XBrushes.White, frame);
            gfx.DrawRectangle(borderPenWhite, XBrushes.White, chart);
            XRect chartTitle = new XRect(framePositionX, framePositionY + 5, frameWidth, frameHeight);
            state = gfx.Save();
            gfx.TranslateTransform(chart.X, chart.Y);
            XPen penLightGreenColor = new XPen(colorLightGreenChart, 4);
            double columnWidth = 2.5;
            penLightGreenColor.Width = columnWidth;
            XPen penGreenColor = new XPen(colorGreenChart, 4);
            penGreenColor.Width = columnWidth;

            double ratioY = ((double)chartHeight) / (double) getMaxValue(maxPoint1, maxPoint2);
            double newChartPositionY = 0;
            double newChartPositionY2 = 0;
            double stepX = (double)chartWidth / (double)pointsXY1.Length;
            double spacing = GetTickSpacing() * ratioY;
            int gridLinesValue = (int)Math.Ceiling((getMaxValue(maxPoint1, maxPoint2) * ratioY) / spacing);
            int dotValue = 0;
            double positionY = 0;

            if(!multiSeries)
            {
                for (int i = 0; i < gridLinesValue; i++)
                {
                    positionY = chartHeight - spacing * i;
                    dotValue = (int)Math.Ceiling(((spacing * i) / ratioY));
                    // Draw points numbers on Y axis
                    XRect titlesAxisY = new XRect(-20, positionY, 10, 10);
                    gfx.DrawString(dotValue.ToString(), font2, new XSolidBrush(colorDarkGrayChartTitle), titlesAxisY, XStringFormats.TopLeft);
                    // Draw gride Lines on Y axis
                    XPen gridPen = new XPen(colorLightGrayChart, 0.5);
                    gfx.DrawLine(gridPen, new XPoint(0, positionY), new XPoint(chartWidth, positionY));
                }

                for (int i = 0; i < pointsXY1.Length; i++)
                {
                    newChartPositionY = pointsXY1[i].Y * ratioY;
                    // Draw Graph Columns
                    gfx.DrawLine(penLightGreenColor, stepX * i, chartHeight, stepX * i, chartHeight - newChartPositionY);
                    // Draw points numbers on X axis
                    XRect titlesAxisX = new XRect((stepX * i) - 2.3, chartHeight + 5, 10, 10);
                    gfx.DrawString(i.ToString(), font2, new XSolidBrush(colorDarkGrayChartTitle), titlesAxisX, XStringFormats.TopLeft);
                }
            }
            else
            {
                //recalculate step
                stepX = (double)chartWidth / (double)getMaxValue(pointsXY1.Length, pointsXY2.Length);

                for (int i = 0; i < gridLinesValue; i++)
                {
                    positionY = chartHeight - spacing * i;
                    dotValue = (int)Math.Ceiling(((spacing * i) / ratioY));
                    // Draw points numbers on Y axis
                    XRect titlesAxisY = new XRect(-20, positionY, 10, 10);
                    gfx.DrawString(dotValue.ToString(), font2, new XSolidBrush(colorDarkGrayChartTitle), titlesAxisY, XStringFormats.TopLeft);
                    // Draw gride Lines on Y axis
                    XPen gridPen = new XPen(colorLightGrayChart, 0.5);
                    gfx.DrawLine(gridPen, new XPoint(0, positionY), new XPoint(chartWidth, positionY));
                }

                for (int i = 0; i < pointsXY1.Length; i++)
                {
                    newChartPositionY = pointsXY1[i].Y * ratioY;
                    // Draw Graph Columns
                    gfx.DrawLine(penLightGreenColor, stepX * i, chartHeight, stepX * i, chartHeight - newChartPositionY);
                    // Draw points numbers on X axis
                    XRect titlesAxisX = new XRect((stepX * i) - 2.3, chartHeight + 5, 10, 10);
                    gfx.DrawString(i.ToString(), font2, new XSolidBrush(colorDarkGrayChartTitle), titlesAxisX, XStringFormats.TopLeft);
                }

                for (int i = 0; i < pointsXY2.Length; i++)
                {
                    // Draw Graph Columns
                    newChartPositionY2 = pointsXY2[i].Y * ratioY;
                    gfx.DrawLine(penGreenColor, stepX * i + columnWidth, chartHeight, stepX * i + columnWidth, chartHeight - newChartPositionY2);
                }
            }
            
            gfx.Restore(state);
            gfx.DrawString(graphTitle, font, new XSolidBrush(colorDarkGrayChartTitle), chartTitle, XStringFormats.TopCenter);
        }

        /// <summary>
        /// Calculate and update values for tick spacing and nice 
        /// minimum and maximum data points on the axis.
        /// </summary>
        private void Calculate(double maxPoint, double minPoint)
        {
            range = NiceNum(maxPoint - minPoint, false);
            tickSpacing = NiceNum(range / (maxTicks - 1), true);
            niceMin = Math.Floor(minPoint / tickSpacing) * tickSpacing;
            niceMax = Math.Ceiling(maxPoint / tickSpacing) * tickSpacing;
        }

        /// <summary>
        /// Returns a "nice" number approximately equal to range Rounds
        /// the number if round = true Takes the ceiling if round = false.
        /// </summary>
        /// <param name="range">the data range</param>
        /// <param name="round">round whether to round the result</param>
        /// <returns>a "nice" number to be used for the data range</returns>
        private double NiceNum(double range, bool round)
        {
            double exponent; /** exponent of range */
            double fraction; /** fractional part of range */
            double niceFraction; /** nice, rounded fraction */

            exponent = Math.Floor(Math.Log10(range));
            fraction = range / Math.Pow(10, exponent);

            if (round)
            {
                if (fraction < 1.5)
                    niceFraction = 1;
                else if (fraction < 3)
                    niceFraction = 2;
                else if (fraction < 7)
                    niceFraction = 5;
                else
                    niceFraction = 10;
            }
            else
            {
                if (fraction <= 1)
                    niceFraction = 1;
                else if (fraction <= 2)
                    niceFraction = 2;
                else if (fraction <= 5)
                    niceFraction = 5;
                else
                    niceFraction = 10;
            }

            return niceFraction * Math.Pow(10, exponent);
        }

        /// <summary>
        /// Sets the minimum and maximum data points for the axis.
        /// </summary>
        /// <param name="minPoint">minimum data point on the axis</param>
        /// <param name="maxPoint">maximum data point on the axis</param>
        public void SetMinMaxPoints(double minPoint, double maxPoint)
        {
            this.minPoint1 = minPoint;
            this.maxPoint1 = maxPoint;
            //Calculate();
        }

        /// <summary>
        /// Sets maximum number of tick marks we're comfortable with.
        /// </summary>
        /// <param name="maxTicks">the maximum number of tick marks for the axis</param>
        public void SetMaxTicks(double maxTicks)
        {
            this.maxTicks = maxTicks;
            //Calculate();
        }

        /// <summary>
        /// Gets the tick spacing.
        /// </summary>
        /// <returns>the tick spacing</returns>
        public double GetTickSpacing()
        {
            return tickSpacing;
        }

        /// <summary>
        /// Gets the "nice" minimum data point.
        /// </summary>
        /// <returns> the new minimum data point for the axis scale</returns>
        public double GetNiceMin()
        {
            return niceMin;
        }

        /// <summary>
        /// Gets the "nice" maximum data point.
        /// </summary>
        /// <returns> the new maximum data point for the axis scale</returns>
        public double GetNiceMax()
        {
            return niceMax;
        }
    }
}
