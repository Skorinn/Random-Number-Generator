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
            
            // Add the legend
            Legend legendHistogram = new Legend();
            legendHistogram.Name = "legendHistogram";
            Legends.Add(legendHistogram);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Plots histogram data for two data sets with specified labels and bin count
        /// </summary>
        /// <param name="data1">IN - First data set to plot</param>
        /// <param name="label1">IN - Label for the first data set</param>
        /// <param name="data2">IN - Second data set to plot</param>
        /// <param name="label2">IN - Label for the second data set</param>
        /// <param name="binCount">IN - Number of bins for the histogram (default 100)</param>
        /// <exception cref="ArgumentNullException">Thrown when data1 or data2 is null</exception>
        /// <exception cref="ArgumentException">Thrown when binCount is less than or equal to zero</exception>
        public void Plot(List<double> data1, string label1, List<double> data2, string label2, int binCount = m_iDEFAULT_BIN_COUNT)
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

            // Calculate dynamic y-axis range based on bin frequencies
            (double fMinYValue, double fMaxYValue) = CalculateFrequencyRange(iBins1, iBins2);

            // Update the chart area with dynamic x-axis and y-axis ranges
            UpdateChartAxisRange(fMinValue, fMaxValue, fMinYValue, fMaxYValue);

            // Create series for first data set
            Series firstSeries = CreateHistogramSeries(label1, m_iSERIES1_ALPHA, Color.Blue);

            // Create series for second data set
            Series secondSeries = CreateHistogramSeries(label2, m_iSERIES2_ALPHA, Color.Red);

            // Add data points to both series
            for (int iBinIndex = 0; iBinIndex < binCount; iBinIndex++)
            {
                double fBinCenter = fMinValue + (iBinIndex + m_fBIN_CENTER_OFFSET) * fBinWidth;
                firstSeries.Points.AddXY(fBinCenter, iBins1[iBinIndex]);
                secondSeries.Points.AddXY(fBinCenter, iBins2[iBinIndex]);
            }

            // Add series to chart
            this.Series.Add(firstSeries);
            this.Series.Add(secondSeries);
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

                // Set Y-axis range with dynamic scaling
                mainChartArea.AxisY.Minimum = fMinYValue;
                mainChartArea.AxisY.Maximum = fMaxYValue;

                // Set optimal interval for Y-axis tick marks
                double fYRange = fMaxYValue - fMinYValue;
                double fYInterval = CalculateOptimalYInterval(fYRange);
                mainChartArea.AxisY.Interval = fYInterval;
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
            
            // Ensure the interval is at least 1 for frequency data (since frequencies are integers)
            return Math.Max(fCalculatedInterval, m_fMINIMUM_Y_INTERVAL);
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
                Color = Color.FromArgb(iAlpha, baseColor)
            };

            return histogramSeries;
        }

        /// <summary>
        /// Calculates the frequency range from both bin arrays for dynamic Y-axis scaling
        /// </summary>
        /// <param name="iBins1">IN - First bin array</param>
        /// <param name="iBins2">IN - Second bin array</param>
        /// <returns>Tuple containing minimum and maximum frequency values</returns>
        private (double fMinYValue, double fMaxYValue) CalculateFrequencyRange(int[] iBins1, int[] iBins2)
        {
            int iMaxFrequency = 0;

            // Find the maximum frequency from both bin arrays
            foreach (int iFrequency in iBins1)
            {
                if (iFrequency > iMaxFrequency)
                {
                    iMaxFrequency = iFrequency;
                }
            }

            foreach (int iFrequency in iBins2)
            {
                if (iFrequency > iMaxFrequency)
                {
                    iMaxFrequency = iFrequency;
                }
            }

            // Calculate Y-axis range with padding
            double fMinYValue = m_fDEFAULT_Y_AXIS_MINIMUM;
            double fMaxYValue;

            if (iMaxFrequency == 0)
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
        private const string m_sY_AXIS_TITLE = "Frequency";
        private const int m_iDEFAULT_BIN_COUNT = 100;

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
        private const double m_fMINIMUM_Y_INTERVAL = 1.0; // Minimum interval for frequency data

        // Series color alpha values
        private const int m_iSERIES1_ALPHA = 120;
        private const int m_iSERIES2_ALPHA = 120;

        #endregion
    }
}
