//*********************************************************************************************************************
// File Name:      RNGChart.cs
// Description:    Random Number Generator result chart class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/02/05 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Track the displayed extremes as points are added instead of rescanning the series on
//                            every point, and corrected the limit tightening logic.
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

            // Disable the X-Axis
            Axis AverageChartXAxis = ChartAreas[0].AxisX;
            AverageChartXAxis.Enabled = AxisEnabled.False;

            // Setup the Y-Axis
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            AverageChartYAxis.Maximum = m_fCENTER + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = m_fCENTER - m_fYAXIS_INCREMENT;
            AverageChartYAxis.Interval = m_fYAXIS_TICK_INTERVAL;

            // Setup the data point series and add a single point to force display
            Series DataPointSeries = Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.IsVisibleInLegend = false;
            DataPointSeries.ChartType = SeriesChartType.Line;
            DataPointSeries.Color = Color.Blue;
            DataPointSeries.Points.AddY(m_fCENTER);

            // Setup the average series (no need to add initial point as data point series will display the chart)
            Series AverageSeries = Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.IsVisibleInLegend = false;
            AverageSeries.ChartType = SeriesChartType.Line;
            AverageSeries.Color = Color.Red;
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
                // Record whether the point rolling off the chart is one of the tracked extremes, as the
                // extremes have to be recalculated when the point they were taken from is removed
                double fDroppedPoint = DataPoints[0].YValues[0];
                bExtremeDropped = ((fDroppedPoint >= m_fMaxPoint) || (fDroppedPoint <= m_fMinPoint));
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
                if (fDataPoint > m_fMaxPoint)
                {
                    m_fMaxPoint = fDataPoint;
                }

                if (fDataPoint < m_fMinPoint)
                {
                    m_fMinPoint = fDataPoint;
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

            // Reset the tracked extremes to the center of the chart
            m_fMaxPoint = m_fCENTER;
            m_fMinPoint = m_fCENTER;

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

            // Walk the displayed points for the extremes
            foreach (DataPoint currentPoint in dataPoints)
            {
                double fCurrentValue = currentPoint.YValues[0];

                if (fCurrentValue > fMax)
                {
                    fMax = fCurrentValue;
                }

                if (fCurrentValue < fMin)
                {
                    fMin = fCurrentValue;
                }
            }

            m_fMaxPoint = fMax;
            m_fMinPoint = fMin;
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

        #endregion
        #region Data Members

        private enum SeriesIndex { DataPointSeries, AverageSeries, };

        // Extremes of the data currently displayed on the chart
        private double m_fMaxPoint = m_fCENTER;
        private double m_fMinPoint = m_fCENTER;

        #endregion
    }
}
