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
//*********************************************************************************************************************
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
            AverageChartYAxis.Maximum = 0.5 + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = 0.5 - m_fYAXIS_INCREMENT;
            AverageChartYAxis.Interval = 0.005;

            // Setup the data point series and add a single point to force display
            Series DataPointSeries = Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.IsVisibleInLegend = false;
            DataPointSeries.ChartType = SeriesChartType.Line;
            DataPointSeries.Color = Color.Blue;
            DataPointSeries.Points.AddY(0.5);

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
            if (DataPoints.Count >= m_iMAX_DATA_SIZE)
            {
                DataPoints.RemoveAt(0);
            }
            DataPoints.AddY(fDataPoint);

            // Update the averages on the chart
            DataPointCollection AveragePoints = Series[(int)SeriesIndex.AverageSeries].Points;
            if (AveragePoints.Count >= m_iMAX_DATA_SIZE)
            {
                AveragePoints.RemoveAt(0);
            }
            AveragePoints.AddY(fCurrentAverage);

            //!!! mpullen - limit adjustment is inefficient and can be improved !!!

            // Find the min and max values on the chart
            double fMax = DataPoints.FindMaxByValue("Y1").YValues[0];
            double fMin = DataPoints.FindMinByValue("Y1").YValues[0];

            // Check if the limits need to be tightened
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            bool bMaxTooWide = ((AverageChartYAxis.Maximum - m_fYAXIS_INCREMENT) > fMax);
            bool bMinTooWide = ((AverageChartYAxis.Minimum + m_fYAXIS_INCREMENT) < fMin);
            if (bMaxTooWide && bMinTooWide)
            {
                // Tighten the limits
                while (bMaxTooWide && bMinTooWide)
                {
                    // Adjust them symetrically to maintain center
                    AverageChartYAxis.Maximum += m_fYAXIS_INCREMENT;
                    AverageChartYAxis.Minimum -= m_fYAXIS_INCREMENT;

                    bMaxTooWide = ((AverageChartYAxis.Maximum - m_fYAXIS_INCREMENT) > fMax);
                    bMinTooWide = ((AverageChartYAxis.Minimum + m_fYAXIS_INCREMENT) < fMin);
                }
            }
            else
            {
                // Widen the limits as needed
                while ((fMax >= AverageChartYAxis.Maximum) || (fMin <= AverageChartYAxis.Minimum))
                {
                    // Adjust them symetrically to maintain center
                    AverageChartYAxis.Maximum += m_fYAXIS_INCREMENT;
                    AverageChartYAxis.Minimum -= m_fYAXIS_INCREMENT;
                }
            }
        }

        /// <summary>
        /// Clears the chart data
        /// <\summary>
        public void Clear()
        {
            // Clear the chart data and add a point so the area is displayed
            Series DataPointSeries = Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.Points.Clear();
            DataPointSeries.Points.AddY(0.5);
            Series AverageSeries = Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.Points.Clear();

            // Reset the Y-Axis
            Axis AverageChartYAxis = ChartAreas[0].AxisY;
            AverageChartYAxis.Maximum = 0.5 + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = 0.5 - m_fYAXIS_INCREMENT;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets the maximum data size (read-only)
        /// </summary>
        public int MaxDataSize
        {
            get { return m_iMAX_DATA_SIZE; }
        }

        #endregion
        #region Constants

        private const int m_iMAX_DATA_SIZE = 1000 * 1024; // 1000 * 1024 * 4  = 4 MB
        private const double m_fYAXIS_INCREMENT = 0.01;

        #endregion
        #region Data Members

        private enum SeriesIndex { DataPointSeries, AverageSeries, };

        #endregion
    }
}
