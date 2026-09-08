//*********************************************************************************************************************
// File Name:      RNGChart.cs
// Description:    Random Number Generator result chart class
//
// Copyright (c) 2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2024/02/05 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Track the displayed extremes as points are added instead of rescanning the series on
//                            every point, and corrected the limit tightening logic.
// 2026/09/07 - Mike Pullen - Say what is being plotted: a title, an axis title, a key, and a marker on the
//                            value an unbiased generator is expected to sit at
//*********************************************************************************************************************
using System;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Random Number Generator result chart class
    /// </summary>
    public class RNGChart : Chart
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGChart() : base()
        {
            // Create the chart components
            ChartArea chartArea = new ChartArea();
            Legend chartLegend = new Legend();
            Series dataPointSeries = new Series();
            Series averagesSeries = new Series();

            // Initialize the chart
            chartArea.Name = "Results";
            ChartAreas.Add(chartArea);
            chartLegend.Name = "Legend";
            Legends.Add(chartLegend);
            dataPointSeries.ChartArea = "Results";
            dataPointSeries.Legend = "Legend";
            dataPointSeries.Name = "Data Points";
            averagesSeries.ChartArea = "Results";
            averagesSeries.Legend = "Legend";
            averagesSeries.Name = "Averages";
            Series.Add(dataPointSeries);
            Series.Add(averagesSeries);

            // Say what is being plotted. Without this the reader has to know that these are bit averages and
            // that an unbiased generator puts them around a half.
            Titles.Add(new Title(m_sCHART_TITLE, Docking.Top, m_TitleFont, SystemColors.ControlText));

            // Disable the X-Axis. The chart holds a rolling window of the most recent readings, so numbering
            // them would label the axis with a count that means nothing on its own.
            Axis AverageChartXAxis = ChartAreas[0].AxisX;
            AverageChartXAxis.Enabled = AxisEnabled.False;

            // Setup the Y-Axis
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            AverageChartYAxis.Title = m_sYAXIS_TITLE;
            AverageChartYAxis.Maximum = m_fCENTER + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = m_fCENTER - m_fYAXIS_INCREMENT;
            AverageChartYAxis.Interval = m_fYAXIS_TICK_INTERVAL;

            // The grid is there to be measured against, not to be looked at, so it is drawn as hairlines
            // rather than in the black the chart uses by default
            chartArea.BackColor = Color.Transparent;
            chartArea.BorderColor = Color.Transparent;
            AverageChartYAxis.LineColor = UiPalette.Line;
            AverageChartYAxis.MajorTickMark.LineColor = UiPalette.Line;
            AverageChartYAxis.MajorGrid.LineColor = UiPalette.Line;
            AverageChartYAxis.LabelStyle.ForeColor = UiPalette.MutedInk;
            AverageChartYAxis.TitleForeColor = UiPalette.MutedInk;

            // Mark the value an unbiased generator is expected to sit at, so how close the readings are to it
            // can be seen rather than worked out. A strip of no width draws as the single line at its offset.
            StripLine expectedValueLine = new StripLine
            {
                IntervalOffset = m_fCENTER,
                Interval = 0,
                StripWidth = 0,
                BorderColor = m_ExpectedValueColor,
                BorderWidth = m_iEXPECTED_VALUE_WIDTH,
                BorderDashStyle = ChartDashStyle.Dash,
                Text = m_sEXPECTED_VALUE_TEXT,
                ForeColor = m_ExpectedValueColor,
                TextAlignment = System.Drawing.StringAlignment.Far
            };
            AverageChartYAxis.StripLines.Add(expectedValueLine);

            // Put the key above the plot rather than beside it, so the plot keeps the full width
            chartLegend.Docking = Docking.Top;
            chartLegend.Alignment = System.Drawing.StringAlignment.Far;
            chartLegend.IsDockedInsideChartArea = false;

            // Setup the data point series and add a single point to force display
            Series DataPointSeries = Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.LegendText = m_sDATA_POINT_LEGEND;
            DataPointSeries.ChartType = SeriesChartType.Line;
            DataPointSeries.Color = UiPalette.Trace;
            DataPointSeries.Points.AddY(m_fCENTER);

            // Setup the average series (no need to add initial point as data point series will display the chart)
            Series AverageSeries = Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.LegendText = m_sAVERAGE_LEGEND;
            AverageSeries.ChartType = SeriesChartType.Line;
            AverageSeries.Color = UiPalette.Average;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Adds a data point to the chart
        /// </summary>
        /// <param name="fDataPoint">IN - The data point to add</param>
        /// <param name="fCurrentAverage">IN - The current average</param>
        public void AddPoint(double fDataPoint, double fCurrentAverage)
        {
            // Update the data points on the chart
            DataPointCollection DataPoints = Series[(int)SeriesIndex.DataPointSeries].Points;
            bool bExtremeDropped = false;
            if (DataPoints.Count >= MaxDataSize)
            {
                // Count the point rolling off the chart out of the extremes. The same value is often
                // displayed many times over, so the extremes are only out of date once the last point
                // holding one has gone, rather than every time one of them is dropped.
                double fDroppedPoint = DataPoints[0].YValues[0];
                if (fDroppedPoint >= m_fMaxPoint)
                {
                    --m_iMaxPointCount;
                    bExtremeDropped = (0 >= m_iMaxPointCount);
                }

                if (fDroppedPoint <= m_fMinPoint)
                {
                    --m_iMinPointCount;
                    bExtremeDropped = (bExtremeDropped || (0 >= m_iMinPointCount));
                }

                DataPoints.RemoveAt(0);
            }
            DataPoints.AddY(fDataPoint);

            // Update the averages on the chart
            DataPointCollection AveragePoints = Series[(int)SeriesIndex.AverageSeries].Points;
            if (AveragePoints.Count >= MaxDataSize)
            {
                AveragePoints.RemoveAt(0);
            }
            AveragePoints.AddY(fCurrentAverage);

            // Update the extremes of the displayed data. Searching the series for them on every point is
            // prohibitively expensive once the chart holds a large number of points, so they are tracked
            // as points are added and only recalculated when an extreme rolls off the chart.
            if (bExtremeDropped)
            {
                RecalculateExtremes(DataPoints);
            }
            else
            {
                // A new extreme replaces the old one and its count starts again, while a point equal to the
                // one already held adds to how many points are keeping that extreme in place
                if (fDataPoint > m_fMaxPoint)
                {
                    m_fMaxPoint = fDataPoint;
                    m_iMaxPointCount = 1;
                }
                else if (fDataPoint == m_fMaxPoint)
                {
                    ++m_iMaxPointCount;
                }

                if (fDataPoint < m_fMinPoint)
                {
                    m_fMinPoint = fDataPoint;
                    m_iMinPointCount = 1;
                }
                else if (fDataPoint == m_fMinPoint)
                {
                    ++m_iMinPointCount;
                }
            }

            // Adjust the limits to the displayed data
            AdjustAxisLimits();
        }

        /// <summary>
        /// Clears the chart data
        /// </summary>
        public void Clear()
        {
            // Clear the chart data and add a point so the area is displayed
            Series DataPointSeries = Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.Points.Clear();
            DataPointSeries.Points.AddY(m_fCENTER);
            Series AverageSeries = Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.Points.Clear();

            // Reset the tracked extremes to the center of the chart, held by the single point just added
            m_fMaxPoint = m_fCENTER;
            m_fMinPoint = m_fCENTER;
            m_iMaxPointCount = 1;
            m_iMinPointCount = 1;

            // Reset the Y-Axis
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            AverageChartYAxis.Maximum = m_fCENTER + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = m_fCENTER - m_fYAXIS_INCREMENT;
        }

        /// <summary>
        /// Recalculates the extremes of the data currently displayed on the chart
        /// </summary>
        /// <param name="dataPoints">IN - The data points currently displayed on the chart</param>
        private void RecalculateExtremes(DataPointCollection dataPoints)
        {
            // Start from the center so an empty series leaves the limits at their defaults
            double fMax = m_fCENTER;
            double fMin = m_fCENTER;
            int iMaxCount = 0;
            int iMinCount = 0;

            // Walk the displayed points for the extremes, counting how many points hold each of them
            foreach (DataPoint currentPoint in dataPoints)
            {
                double fCurrentValue = currentPoint.YValues[0];

                if (fCurrentValue > fMax)
                {
                    fMax = fCurrentValue;
                    iMaxCount = 1;
                }
                else if (fCurrentValue == fMax)
                {
                    ++iMaxCount;
                }

                if (fCurrentValue < fMin)
                {
                    fMin = fCurrentValue;
                    iMinCount = 1;
                }
                else if (fCurrentValue == fMin)
                {
                    ++iMinCount;
                }
            }

            m_fMaxPoint = fMax;
            m_fMinPoint = fMin;
            m_iMaxPointCount = iMaxCount;
            m_iMinPointCount = iMinCount;
        }

        /// <summary>
        /// Adjusts the Y-Axis limits to contain the data currently displayed on the chart. The limits are
        /// kept symmetrical about the center and are never more than one increment wider than needed.
        /// </summary>
        private void AdjustAxisLimits()
        {
            // Determine how far the displayed data extends from the center of the chart
            double fMaxDistance = Math.Max(Math.Abs(m_fMaxPoint - m_fCENTER), Math.Abs(m_fCENTER - m_fMinPoint));

            // Use the smallest number of increments that still contains the data. Adding one increment keeps
            // the limits outside of the data rather than on top of it.
            double fIncrements = Math.Floor(fMaxDistance / m_fYAXIS_INCREMENT) + 1.0;
            double fHalfSpan = fIncrements * m_fYAXIS_INCREMENT;
            double fMaximum = m_fCENTER + fHalfSpan;
            double fMinimum = m_fCENTER - fHalfSpan;

            // Only touch the axis when the limits actually change to avoid needless redraws
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            bool bLimitsChanged = ((AverageChartYAxis.Maximum != fMaximum) || (AverageChartYAxis.Minimum != fMinimum));
            if (bLimitsChanged)
            {
                AverageChartYAxis.Maximum = fMaximum;
                AverageChartYAxis.Minimum = fMinimum;
            }
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets the maximum data size (read-only)
        /// </summary>
        public virtual int MaxDataSize
        {
            get { return m_iMAX_DATA_SIZE; }
        }

        #endregion
        #region Constants

        private const int m_iMAX_DATA_SIZE = 1000 * 1024; // Maximum number of points held on the chart
        private const double m_fYAXIS_INCREMENT = 0.01;
        private const double m_fYAXIS_TICK_INTERVAL = 0.005;
        private const double m_fCENTER = 0.5; // Statistical mean of the data, which the chart is centered on

        // Labelling, so the chart says what it is showing rather than relying on the reader knowing
        private const string m_sCHART_TITLE = "Bit average per reading";
        private const string m_sYAXIS_TITLE = "Bit average";
        private const string m_sDATA_POINT_LEGEND = "Reading";
        private const string m_sAVERAGE_LEGEND = "Running mean";
        private const string m_sEXPECTED_VALUE_TEXT = "Expected";
        private const int m_iEXPECTED_VALUE_WIDTH = 1;

        #endregion
        #region Data Members

        // Drawing of the expected value marker and the title
        private static readonly Color m_ExpectedValueColor = UiPalette.Expected;
        private static readonly Font m_TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);

        private enum SeriesIndex { DataPointSeries, AverageSeries, };

        // Extremes of the data currently displayed on the chart
        private double m_fMaxPoint = m_fCENTER;
        private double m_fMinPoint = m_fCENTER;

        // How many of the displayed points hold each extreme, so a rescan is only needed once the last of
        // them has rolled off the chart
        private int m_iMaxPointCount = 1;
        private int m_iMinPointCount = 1;

        #endregion
    }
}
