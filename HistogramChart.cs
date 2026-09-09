//*********************************************************************************************************************
// File Name:      HistogramChart.cs
// Description:    Histogram chart component for displaying random number generator data distribution
//
// Copyright (c) 2025 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2025/08/13 - Mike Pullen - Initial version
// 2026/09/07 - Mike Pullen - Choose the bin count from the sample size, label the axes to tick precision,
//                            and put the key above the plot
// 2026/09/07 - Mike Pullen - Plot each session as a share of its own readings, so two sessions of
//                            different lengths can be compared
// 2026/09/09 - Mike Pullen - Let the share axis interval follow the data rather than holding it at whole
//                            numbers, which it was doing from when the axis counted readings
//*********************************************************************************************************************
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Histogram chart class for displaying data distribution of random number generator results
    /// </summary>
    public class HistogramChart : Chart
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public HistogramChart()
        {
            // Set chart docking and visual properties
            this.Dock = DockStyle.Fill;
            this.Palette = ChartColorPalette.Bright;

            // Create and configure the main chart area
            ChartArea mainChartArea = new ChartArea(m_sMAIN_CHART_AREA_NAME)
            {
                AxisX = { Title = m_sX_AXIS_TITLE },
                AxisY = { Title = m_sY_AXIS_TITLE }
            };

            this.ChartAreas.Add(mainChartArea);

            // Add the legend, above the plot rather than beside it so the distribution keeps the full width
            Legend legendHistogram = new Legend();
            legendHistogram.Name = "legendHistogram";
            legendHistogram.Docking = Docking.Top;
            legendHistogram.Alignment = StringAlignment.Far;
            legendHistogram.IsDockedInsideChartArea = false;
            Legends.Add(legendHistogram);

            // Colour everything the palette owns
            ApplyPalette();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Takes the chart's colours from the palette. The grid is there to be measured against rather than
        /// looked at, so it is drawn as hairlines rather than in the black the chart uses by default.
        /// NOTE: This is called again whenever the system colour scheme changes, so it must set every colour
        /// it owns rather than assuming what was set when the chart was built. The two distributions are
        /// coloured when they are plotted, as they only exist then.
        /// </summary>
        public void ApplyPalette()
        {
            ChartArea mainChartArea = this.ChartAreas[0];
            mainChartArea.BackColor = Color.Transparent;
            mainChartArea.BorderColor = Color.Transparent;

            foreach (Axis axis in new Axis[] { mainChartArea.AxisX, mainChartArea.AxisY })
            {
                axis.LineColor = UiPalette.Line;
                axis.MajorTickMark.LineColor = UiPalette.Line;
                axis.MajorGrid.LineColor = UiPalette.Line;
                axis.LabelStyle.ForeColor = UiPalette.MutedText;
                axis.TitleForeColor = UiPalette.MutedText;
            }

            foreach (Legend chartLegend in Legends)
            {
                chartLegend.BackColor = Color.Transparent;
                chartLegend.ForeColor = UiPalette.CardText;
            }
        }

        /// <summary>
        /// Plots histogram data for two data sets, choosing how many bins to divide them into from how much
        /// data there is. Too many bins for the sample leaves single readings standing alone as spikes,
        /// which reads as a scatter of lines rather than as a distribution with a shape.
        /// </summary>
        /// <param name="data1">IN - First data set to plot</param>
        /// <param name="label1">IN - Label for the first data set</param>
        /// <param name="data2">IN - Second data set to plot</param>
        /// <param name="label2">IN - Label for the second data set</param>
        /// <exception cref="ArgumentNullException">Thrown when data1 or data2 is null</exception>
        public void Plot(List<double> data1, string label1, List<double> data2, string label2)
        {
            // Validate before counting, so a null set is reported rather than dereferenced
            if (null == data1)
            {
                throw new ArgumentNullException(nameof(data1), "First data set cannot be null");
            }

            if (null == data2)
            {
                throw new ArgumentNullException(nameof(data2), "Second data set cannot be null");
            }

            Plot(data1, label1, data2, label2, ChooseBinCount(data1.Count + data2.Count));
        }

        /// <summary>
        /// Plots histogram data for two data sets with specified labels and bin count
        /// </summary>
        /// <param name="data1">IN - First data set to plot</param>
        /// <param name="label1">IN - Label for the first data set</param>
        /// <param name="data2">IN - Second data set to plot</param>
        /// <param name="label2">IN - Label for the second data set</param>
        /// <param name="binCount">IN - Number of bins for the histogram</param>
        /// <exception cref="ArgumentNullException">Thrown when data1 or data2 is null</exception>
        /// <exception cref="ArgumentException">Thrown when binCount is less than or equal to zero</exception>
        public void Plot(List<double> data1, string label1, List<double> data2, string label2, int binCount)
        {
            // Validate input parameters
            if (null == data1)
            {
                throw new ArgumentNullException(nameof(data1), "First data set cannot be null");
            }

            if (null == data2)
            {
                throw new ArgumentNullException(nameof(data2), "Second data set cannot be null");
            }

            if (binCount <= 0)
            {
                throw new ArgumentException("Bin count must be greater than zero", nameof(binCount));
            }

            // Clear existing series
            this.Series.Clear();

            // Calculate dynamic x-axis range based on actual data
            (double fMinValue, double fMaxValue) = CalculateDataRange(data1, data2);

            // Calculate bin width based on data range
            double fDataRange = fMaxValue - fMinValue;
            double fBinWidth = (fDataRange > 0) ? fDataRange / binCount : m_fDEFAULT_BIN_WIDTH;
            int[] iBins1 = new int[binCount];
            int[] iBins2 = new int[binCount];

            // Populate bins for first data set
            foreach (double fValue in data1)
            {
                int iBinIndex = CalculateBinIndexWithRange(fValue, binCount, fMinValue, fMaxValue);
                iBins1[iBinIndex]++;
            }

            // Populate bins for second data set
            foreach (double fValue in data2)
            {
                int iBinIndex = CalculateBinIndexWithRange(fValue, binCount, fMinValue, fMaxValue);
                iBins2[iBinIndex]++;
            }

            // Show each set as a share of its own readings rather than as a count of them. Two sessions are
            // rarely the same length, and on raw counts the longer one stands taller in every bin, which
            // hides the thing the comparison exists to show: whether the shape has shifted.
            double[] fShare1 = CalculateShares(iBins1, data1.Count);
            double[] fShare2 = CalculateShares(iBins2, data2.Count);

            // Calculate dynamic y-axis range based on the bin shares
            (double fMinYValue, double fMaxYValue) = CalculateFrequencyRange(fShare1, fShare2);

            // Update the chart area with dynamic x-axis and y-axis ranges
            UpdateChartAxisRange(fMinValue, fMaxValue, fMinYValue, fMaxYValue);

            // Create series for first data set
            Series firstSeries = CreateHistogramSeries(label1, m_iSERIES1_ALPHA, UiPalette.Trace);

            // Create series for second data set
            Series secondSeries = CreateHistogramSeries(label2, m_iSERIES2_ALPHA, UiPalette.Average);

            // Add data points to both series
            for (int iBinIndex = 0; iBinIndex < binCount; iBinIndex++)
            {
                double fBinCenter = fMinValue + (iBinIndex + m_fBIN_CENTER_OFFSET) * fBinWidth;
                firstSeries.Points.AddXY(fBinCenter, fShare1[iBinIndex]);
                secondSeries.Points.AddXY(fBinCenter, fShare2[iBinIndex]);
            }

            // Add series to chart
            this.Series.Add(firstSeries);
            this.Series.Add(secondSeries);
        }

        /// <summary>
        /// Chooses how many bins to divide the data into. The count follows the cube root of the sample
        /// size, which is the usual rule for keeping the bars wide enough to show a shape without smoothing
        /// the distribution away, and is held between a floor and a ceiling so that a handful of readings
        /// still produces a readable chart and a long session does not produce a comb.
        /// </summary>
        /// <param name="iSampleCount">IN - How many readings there are across both data sets</param>
        /// <returns>The number of bins to use</returns>
        private static int ChooseBinCount(int iSampleCount)
        {
            if (m_iMINIMUM_BIN_COUNT >= iSampleCount)
            {
                return m_iMINIMUM_BIN_COUNT;
            }

            int iChosen = (int)Math.Ceiling(m_fBIN_COUNT_FACTOR * Math.Pow(iSampleCount, m_fBIN_COUNT_EXPONENT));
            return Math.Min(Math.Max(iChosen, m_iMINIMUM_BIN_COUNT), m_iMAXIMUM_BIN_COUNT);
        }

        /// <summary>
        /// The number of decimal places an axis label needs to tell one tick from the next. Printing a
        /// double at full precision fills the axis with digits that carry no information and collide with
        /// the label beside them.
        /// </summary>
        /// <param name="fInterval">IN - The interval between ticks</param>
        /// <returns>A numeric format string for the axis labels</returns>
        private static string GetAxisLabelFormat(double fInterval)
        {
            if (0 >= fInterval)
            {
                return m_sDEFAULT_LABEL_FORMAT;
            }

            int iDecimals = (int)Math.Ceiling(-Math.Log10(fInterval));
            iDecimals = Math.Min(Math.Max(iDecimals, 0), m_iMAXIMUM_LABEL_DECIMALS);

            // An interval of one or more needs no decimals at all, and the whole number format is returned
            // rather than one with an empty run of zeroes after the point. Both produce the same labels,
            // because a format ending in a decimal point with nothing after it drops the point, but only
            // one of them says so.
            if (m_iNO_DECIMALS == iDecimals)
            {
                return m_sWHOLE_NUMBER_LABEL_FORMAT;
            }

            return m_sDECIMAL_LABEL_PREFIX + new string(m_cLABEL_DECIMAL_PLACE, iDecimals);
        }

        /// <summary>
        /// Calculates the data range from both data sets, handling empty sets appropriately
        /// </summary>
        /// <param name="data1">IN - First data set</param>
        /// <param name="data2">IN - Second data set</param>
        /// <returns>Tuple containing minimum and maximum values across both data sets</returns>
        private (double fMinValue, double fMaxValue) CalculateDataRange(List<double> data1, List<double> data2)
        {
            double fMinValue = double.MaxValue;
            double fMaxValue = double.MinValue;
            bool bHasData = false;

            // Process first data set
            if (data1.Count > 0)
            {
                foreach (double fValue in data1)
                {
                    if (fValue < fMinValue) fMinValue = fValue;
                    if (fValue > fMaxValue) fMaxValue = fValue;
                    bHasData = true;
                }
            }

            // Process second data set
            if (data2.Count > 0)
            {
                foreach (double fValue in data2)
                {
                    if (fValue < fMinValue) fMinValue = fValue;
                    if (fValue > fMaxValue) fMaxValue = fValue;
                    bHasData = true;
                }
            }

            // Handle case where no data exists
            if (!bHasData)
            {
                fMinValue = m_fDEFAULT_X_AXIS_MINIMUM;
                fMaxValue = m_fDEFAULT_X_AXIS_MAXIMUM;
            }
            // Handle case where all values are the same
            else if (Math.Abs(fMaxValue - fMinValue) < m_fMINIMUM_RANGE_THRESHOLD)
            {
                double fCenter = (fMinValue + fMaxValue) / 2.0;
                fMinValue = fCenter - (m_fMINIMUM_RANGE_THRESHOLD / 2.0);
                fMaxValue = fCenter + (m_fMINIMUM_RANGE_THRESHOLD / 2.0);
            }
            else
            {
                // Add padding to the range for better visualization
                double fRange = fMaxValue - fMinValue;
                double fPadding = fRange * m_fRANGE_PADDING_FACTOR;
                fMinValue -= fPadding;
                fMaxValue += fPadding;
            }

            return (fMinValue, fMaxValue);
        }

        /// <summary>
        /// Updates the chart area with the specified x-axis and y-axis ranges
        /// </summary>
        /// <param name="fMinXValue">IN - Minimum x-axis value</param>
        /// <param name="fMaxXValue">IN - Maximum x-axis value</param>
        /// <param name="fMinYValue">IN - Minimum y-axis value</param>
        /// <param name="fMaxYValue">IN - Maximum y-axis value</param>
        private void UpdateChartAxisRange(double fMinXValue, double fMaxXValue, double fMinYValue, double fMaxYValue)
        {
            if (this.ChartAreas.Count > 0)
            {
                ChartArea mainChartArea = this.ChartAreas[0];
                
                // Set X-axis range and interval
                mainChartArea.AxisX.Minimum = fMinXValue;
                mainChartArea.AxisX.Maximum = fMaxXValue;

                // Set optimal interval for X-axis tick marks
                double fXRange = fMaxXValue - fMinXValue;
                double fXInterval = CalculateOptimalInterval(fXRange);
                mainChartArea.AxisX.Interval = fXInterval;

                // Label the ticks to the precision that tells them apart, and no further. Left to itself the
                // chart prints the full double, which runs the labels into one another.
                mainChartArea.AxisX.LabelStyle.Format = GetAxisLabelFormat(fXInterval);

                // Set Y-axis range with dynamic scaling
                mainChartArea.AxisY.Minimum = fMinYValue;
                mainChartArea.AxisY.Maximum = fMaxYValue;

                // Set optimal interval for Y-axis tick marks
                double fYRange = fMaxYValue - fMinYValue;
                double fYInterval = CalculateOptimalYInterval(fYRange);
                mainChartArea.AxisY.Interval = fYInterval;

                // The axis carries each session as a share of its own readings, so it is labelled to the
                // precision the interval needs, the same way the value axis is. It used to be labelled in
                // whole numbers because it counted readings, and a share of a session is not a count.
                mainChartArea.AxisY.LabelStyle.Format = GetAxisLabelFormat(fYInterval);
            }
        }

        /// <summary>
        /// Calculates an optimal interval for x-axis tick marks based on the data range
        /// </summary>
        /// <param name="fRange">IN - The data range</param>
        /// <returns>Optimal interval for axis tick marks</returns>
        private double CalculateOptimalInterval(double fRange)
        {
            // Calculate power of 10 that gives us approximately 5-10 intervals
            double fLogRange = Math.Log10(fRange);
            double fPowerOf10 = Math.Floor(fLogRange);
            double fNormalizedRange = fRange / Math.Pow(10, fPowerOf10);

            double fBaseInterval;
            if (fNormalizedRange <= 2.0)
            {
                fBaseInterval = 0.2;
            }
            else if (fNormalizedRange <= 5.0)
            {
                fBaseInterval = 0.5;
            }
            else
            {
                fBaseInterval = 1.0;
            }

            return fBaseInterval * Math.Pow(10, fPowerOf10);
        }

        /// <summary>
        /// Calculates an optimal interval for y-axis tick marks based on the frequency range
        /// </summary>
        /// <param name="fYRange">IN - The frequency range</param>
        /// <returns>Optimal interval for Y-axis tick marks</returns>
        private double CalculateOptimalYInterval(double fYRange)
        {
            // For frequency data, we want nice round numbers for intervals
            if (fYRange <= 0)
            {
                return m_fDEFAULT_Y_INTERVAL;
            }

            // Calculate power of 10 that gives us approximately 5-10 intervals
            double fLogRange = Math.Log10(fYRange);
            double fPowerOf10 = Math.Floor(fLogRange);
            double fNormalizedRange = fYRange / Math.Pow(10, fPowerOf10);

            double fBaseInterval;
            if (fNormalizedRange <= 1.0)
            {
                fBaseInterval = 0.1;
            }
            else if (fNormalizedRange <= 2.0)
            {
                fBaseInterval = 0.2;
            }
            else if (fNormalizedRange <= 5.0)
            {
                fBaseInterval = 0.5;
            }
            else
            {
                fBaseInterval = 1.0;
            }

            double fCalculatedInterval = fBaseInterval * Math.Pow(10, fPowerOf10);

            // The interval follows the data. It used to be held at one or more, because the axis counted
            // readings and a count cannot fall between two whole numbers; the axis carries each session as
            // a share of its own readings now, and a share can. Holding it at one put a comparison whose
            // tallest bin held a percent or two of its session between a single pair of ticks.
            return fCalculatedInterval;
        }

        /// <summary>
        /// Calculates the bin index for a given value within a specified range
        /// </summary>
        /// <param name="fValue">IN - The value to calculate bin index for</param>
        /// <param name="iBinCount">IN - Total number of bins</param>
        /// <param name="fMinValue">IN - Minimum value of the range</param>
        /// <param name="fMaxValue">IN - Maximum value of the range</param>
        /// <returns>The bin index for the given value</returns>
        private int CalculateBinIndexWithRange(double fValue, int iBinCount, double fMinValue, double fMaxValue)
        {
            // Handle edge cases
            if (fValue <= fMinValue) return 0;
            if (fValue >= fMaxValue) return iBinCount - 1;

            // Calculate normalized position (0.0 to 1.0)
            double fRange = fMaxValue - fMinValue;
            double fNormalizedValue = (fValue - fMinValue) / fRange;

            // Calculate bin index
            int iBinIndex = (int)(fNormalizedValue * iBinCount);

            // Ensure we don't exceed the maximum bin index
            return Math.Min(iBinIndex, iBinCount - 1);
        }

        /// <summary>
        /// Creates a histogram series with specified properties
        /// </summary>
        /// <param name="sSeriesName">IN - Name/label for the series</param>
        /// <param name="iAlpha">IN - Alpha transparency value for the series color</param>
        /// <param name="baseColor">IN - Base color for the series</param>
        /// <returns>Configured Series object for histogram display</returns>
        /// <exception cref="ArgumentException">Thrown when sSeriesName is null or empty</exception>
        private Series CreateHistogramSeries(string sSeriesName, int iAlpha, Color baseColor)
        {
            // Validate series name
            if (string.IsNullOrEmpty(sSeriesName))
            {
                throw new ArgumentException("Series name cannot be null or empty", nameof(sSeriesName));
            }

            // Create and configure the series
            Series histogramSeries = new Series(sSeriesName)
            {
                ChartType = SeriesChartType.Column,
                ChartArea = m_sMAIN_CHART_AREA_NAME,
                Color = Color.FromArgb(iAlpha, baseColor),
                BorderColor = Color.FromArgb(iAlpha, baseColor),
                BorderWidth = 0
            };

            // Fill most of the space each bin is given, so the bars read as a distribution rather than as a
            // row of separated lines
            histogramSeries["PointWidth"] = m_sPOINT_WIDTH;

            return histogramSeries;
        }

        /// <summary>
        /// Turns bin counts into the share of the readings that fell into each bin, as a percentage of the
        /// set they came from. A set with nothing in it contributes nothing rather than dividing by zero.
        /// </summary>
        /// <param name="iBins">IN - The number of readings that fell into each bin</param>
        /// <param name="iTotalReadings">IN - How many readings the set held altogether</param>
        /// <returns>The share of the set in each bin, as a percentage</returns>
        private static double[] CalculateShares(int[] iBins, int iTotalReadings)
        {
            double[] fShares = new double[iBins.Length];
            if (0 >= iTotalReadings)
            {
                return fShares;
            }

            for (int iBinIndex = 0; iBinIndex < iBins.Length; iBinIndex++)
            {
                fShares[iBinIndex] = ((iBins[iBinIndex] * m_fPERCENT) / iTotalReadings);
            }
            return fShares;
        }

        /// <summary>
        /// Calculates the frequency range from both bin arrays for dynamic Y-axis scaling
        /// </summary>
        /// <param name="fBins1">IN - First bin array</param>
        /// <param name="fBins2">IN - Second bin array</param>
        /// <returns>Tuple containing minimum and maximum frequency values</returns>
        private (double fMinYValue, double fMaxYValue) CalculateFrequencyRange(double[] fBins1, double[] fBins2)
        {
            double iMaxFrequency = 0;

            // Find the maximum frequency from both bin arrays
            foreach (double iFrequency in fBins1)
            {
                if (iFrequency > iMaxFrequency)
                {
                    iMaxFrequency = iFrequency;
                }
            }

            foreach (double iFrequency in fBins2)
            {
                if (iFrequency > iMaxFrequency)
                {
                    iMaxFrequency = iFrequency;
                }
            }

            // Calculate Y-axis range with padding
            double fMinYValue = m_fDEFAULT_Y_AXIS_MINIMUM;
            double fMaxYValue;

            if (m_fMINIMUM_SHARE >= iMaxFrequency)
            {
                // No data points - use default range
                fMaxYValue = m_fDEFAULT_Y_AXIS_MAXIMUM;
            }
            else
            {
                // Add padding to the maximum frequency for better visualization
                double fPadding = iMaxFrequency * m_fY_RANGE_PADDING_FACTOR;
                fMaxYValue = iMaxFrequency + fPadding;
                
                // Ensure minimum padding
                if (fMaxYValue < (iMaxFrequency + m_fMINIMUM_Y_PADDING))
                {
                    fMaxYValue = iMaxFrequency + m_fMINIMUM_Y_PADDING;
                }
            }

            return (fMinYValue, fMaxYValue);
        }

        #endregion
        #region Constants

        // Chart configuration constants
        private const string m_sMAIN_CHART_AREA_NAME = "chartAreaHistogram";
        private const string m_sX_AXIS_TITLE = "Value";
        private const string m_sY_AXIS_TITLE = "% of the session's readings";

        // Choosing the bin count. The factor and exponent are the usual cube-root rule; the floor and
        // ceiling keep a small sample readable and a long session from turning into a comb.
        private const double m_fBIN_COUNT_FACTOR = 2.0;
        private const double m_fBIN_COUNT_EXPONENT = 1.0 / 3.0;
        private const int m_iMINIMUM_BIN_COUNT = 10;
        private const int m_iMAXIMUM_BIN_COUNT = 60;

        // Turning bin counts into shares. A bin holding nothing at all is treated as empty rather than as a
        // share too small to see, so an empty comparison falls back to the default axis.
        private const double m_fPERCENT = 100.0;
        private const double m_fMINIMUM_SHARE = 0.0;

        // Axis labelling
        private const string m_sDEFAULT_LABEL_FORMAT = "0.000";
        private const int m_iMAXIMUM_LABEL_DECIMALS = 6;

        // Building the format for a given number of decimals
        private const string m_sWHOLE_NUMBER_LABEL_FORMAT = "0";
        private const string m_sDECIMAL_LABEL_PREFIX = "0.";
        private const char m_cLABEL_DECIMAL_PLACE = '0';
        private const int m_iNO_DECIMALS = 0;

        // How much of the space a bin is given the bar fills
        private const string m_sPOINT_WIDTH = "0.9";

        // Dynamic range calculation constants
        private const double m_fDEFAULT_X_AXIS_MINIMUM = 0.0;
        private const double m_fDEFAULT_X_AXIS_MAXIMUM = 1.0;
        private const double m_fDEFAULT_BIN_WIDTH = 0.01;
        private const double m_fMINIMUM_RANGE_THRESHOLD = 1e-6;
        private const double m_fRANGE_PADDING_FACTOR = 0.05; // 5% padding on each side
        private const double m_fBIN_CENTER_OFFSET = 0.5;

        // Y-axis dynamic scaling constants
        private const double m_fDEFAULT_Y_AXIS_MINIMUM = 0.0;
        private const double m_fDEFAULT_Y_AXIS_MAXIMUM = 10.0;
        private const double m_fY_RANGE_PADDING_FACTOR = 0.1; // 10% padding for Y-axis
        private const double m_fMINIMUM_Y_PADDING = 2.0; // Minimum padding for Y-axis
        private const double m_fDEFAULT_Y_INTERVAL = 1.0;

        // Series color alpha values
        private const int m_iSERIES1_ALPHA = 120;
        private const int m_iSERIES2_ALPHA = 120;

        #endregion
    }
}
