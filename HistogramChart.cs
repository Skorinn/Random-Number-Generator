//*********************************************************************************************************************
// File Name:      HistogramChart.cs
// Description:    Histogram chart component for displaying random number generator data distribution
//
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
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
                AxisX = { Title = m_sX_AXIS_TITLE, Minimum = m_fX_AXIS_MINIMUM, Maximum = m_fX_AXIS_MAXIMUM },
                AxisY = { Title = m_sY_AXIS_TITLE }
            };

            this.ChartAreas.Add(mainChartArea);
            
            // Add thelegend
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

            // Calculate bin width and initialize bin arrays
            double fBinWidth = m_fBIN_WIDTH_DIVISOR / binCount;
            int[] iBins1 = new int[binCount];
            int[] iBins2 = new int[binCount];

            // Populate bins for first data set
            foreach (double fValue in data1)
            {
                int iBinIndex = CalculateBinIndex(fValue, binCount);
                iBins1[iBinIndex]++;
            }

            // Populate bins for second data set
            foreach (double fValue in data2)
            {
                int iBinIndex = CalculateBinIndex(fValue, binCount);
                iBins2[iBinIndex]++;
            }

            // Create series for first data set
            Series firstSeries = CreateHistogramSeries(label1, m_iSERIES1_ALPHA, Color.Blue);

            // Create series for second data set
            Series secondSeries = CreateHistogramSeries(label2, m_iSERIES2_ALPHA, Color.Red);

            // Add data points to both series
            for (int iBinIndex = 0; iBinIndex < binCount; iBinIndex++)
            {
                double fBinCenter = (iBinIndex + m_fBIN_CENTER_OFFSET) * fBinWidth;
                firstSeries.Points.AddXY(fBinCenter, iBins1[iBinIndex]);
                secondSeries.Points.AddXY(fBinCenter, iBins2[iBinIndex]);
            }

            // Add series to chart
            this.Series.Add(firstSeries);
            this.Series.Add(secondSeries);
        }

        /// <summary>
        /// Calculates the bin index for a given value
        /// </summary>
        /// <param name="fValue">IN - The value to calculate bin index for</param>
        /// <param name="iBinCount">IN - Total number of bins</param>
        /// <returns>The bin index for the given value</returns>
        private int CalculateBinIndex(double fValue, int iBinCount)
        {
            int iBinIndex = (int)(fValue * iBinCount);
            int iMaxBinIndex = iBinCount - 1;
            
            // Use Math.Min to ensure we don't exceed the maximum bin index
            int iClampedBinIndex = Math.Min(iBinIndex, iMaxBinIndex);
            
            return iClampedBinIndex;
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

        #endregion
        #region Constants

        // Chart configuration constants
        private const string m_sMAIN_CHART_AREA_NAME = "chartAreaHistogram";
        private const string m_sX_AXIS_TITLE = "Value";
        private const string m_sY_AXIS_TITLE = "Frequency";
        private const int m_iDEFAULT_BIN_COUNT = 100;

        // Chart scaling constants
        private const double m_fX_AXIS_MINIMUM = 0.0;
        private const double m_fX_AXIS_MAXIMUM = 1.0;
        private const double m_fBIN_WIDTH_DIVISOR = 1.0;
        private const double m_fBIN_CENTER_OFFSET = 0.5;

        // Series color alpha values
        private const int m_iSERIES1_ALPHA = 120;
        private const int m_iSERIES2_ALPHA = 120;

        #endregion
    }
}
