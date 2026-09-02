//*********************************************************************************************************************
// File Name:      RNGChart.Test.cs
// Description:    Unit tests for the RNGChart class
//
// Copyright (C) 2026 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2026/08/31 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms.DataVisualization.Charting;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the RNGChart class
    /// </summary>
    [TestClass]
    public class RNGChartTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGChartTests()
        {
            // Nothing to do
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }

        /// <summary>
        /// Gets the Y-Axis of the chart under test
        /// </summary>
        /// <param name="chart">IN - The chart from which to get the axis</param>
        /// <returns>The Y-Axis of the chart</returns>
        private static Axis GetYAxis(RNGChart chart)
        {
            return chart.ChartAreas[0].AxisY;
        }

        #endregion
        #region Data Members

        // Default limits of the chart, which are one increment either side of the statistical mean
        private const double m_fDEFAULT_MAXIMUM = 0.51;
        private const double m_fDEFAULT_MINIMUM = 0.49;

        // Value used for points that should not move the limits
        private const double m_fCENTER_POINT = 0.5;

        // Values chosen to sit clear of an increment boundary so the expected limits are unambiguous
        private const double m_fHIGH_POINT = 0.5237;
        private const double m_fLOW_POINT = 0.4763;
        private const double m_fWIDENED_MAXIMUM = 0.53;
        private const double m_fWIDENED_MINIMUM = 0.47;

        // Tolerance for comparing the limits, which are accumulated from increments
        private const double m_fCOMPARE_DELTA = 1.0e-9;

        // Number of points used to verify the limits are maintained without rescanning the series
        private const int m_iLARGE_POINT_COUNT = 50000;

        // Size of the chart used to exercise points rolling off of the display
        private const int m_iSMALL_CHART_SIZE = 10;

        #endregion
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        #region Tests

        /// <summary>
        /// Tests the chart is created with the limits centered on the statistical mean
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Default_LimitsCenteredOnMean()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            //**************************************************************//
            // Act
            //**************************************************************//

            RNGChart chart = new RNGChart();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fDEFAULT_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fDEFAULT_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that points inside of the limits leave the limits alone
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AddPoint_PointWithinLimits_LimitsUnchanged()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            chart.AddPoint(m_fCENTER_POINT, m_fCENTER_POINT);
            chart.AddPoint(0.502, m_fCENTER_POINT);
            chart.AddPoint(0.498, m_fCENTER_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fDEFAULT_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fDEFAULT_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that a point above the limits widens them symmetrically
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AddPoint_PointAboveLimits_WidensSymmetrically()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fWIDENED_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fWIDENED_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that a point below the limits widens them symmetrically
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AddPoint_PointBelowLimits_WidensSymmetrically()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            chart.AddPoint(m_fLOW_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fWIDENED_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fWIDENED_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that the limits are not widened again by points already inside of them
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AddPoint_PointsAfterWidening_LimitsHeld()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();
            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Add points inside of the widened limits, which cannot be dropped from the display
            for (int iIndex = 0; iIndex < 100; iIndex++)
            {
                chart.AddPoint(m_fCENTER_POINT, m_fCENTER_POINT);
            }

            //**************************************************************//
            // Assert
            //**************************************************************//

            // The widest point is still displayed, so the limits must still contain it
            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fWIDENED_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fWIDENED_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that the limits are tightened once the widest point is no longer displayed.
        /// NOTE: A timeout is used as an incorrect adjustment can loop without ever settling.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [Timeout(10000)]
        public void AddPoint_WidestPointRolledOff_LimitsTightened()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use a small chart so points roll off of the display quickly
            TestableRNGChart chart = new TestableRNGChart(m_iSMALL_CHART_SIZE);

            // Widen the limits with a single point well outside of them
            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Fill the chart so the widest point is dropped from the display
            for (int iIndex = 0; iIndex < (m_iSMALL_CHART_SIZE * 2); iIndex++)
            {
                chart.AddPoint(m_fCENTER_POINT, m_fCENTER_POINT);
            }

            //**************************************************************//
            // Assert
            //**************************************************************//

            // The widest point is gone, so the limits should be back to the defaults
            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fDEFAULT_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fDEFAULT_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that the limits still contain the data after points roll off of the display
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [Timeout(10000)]
        public void AddPoint_PointsRolledOff_LimitsContainDisplayedData()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use a small chart so points roll off of the display quickly
            TestableRNGChart chart = new TestableRNGChart(m_iSMALL_CHART_SIZE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Drop the widest point from the display, then add a new one below the limits
            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);
            for (int iIndex = 0; iIndex < (m_iSMALL_CHART_SIZE * 2); iIndex++)
            {
                chart.AddPoint(m_fCENTER_POINT, m_fCENTER_POINT);
            }
            chart.AddPoint(m_fLOW_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // The limits should have widened again for the new point
            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fWIDENED_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fWIDENED_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that a large number of points can be added and still be contained by the limits.
        /// NOTE: A timeout is used as searching the series for the extremes on every point does not
        /// complete in a reasonable time for this number of points.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [Timeout(30000)]
        public void AddPoint_LargeNumberOfPoints_LimitsContainData()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();

            // Spread the points either side of the mean, widening as the series grows
            double fLargestPoint = m_fCENTER_POINT;
            double fSmallestPoint = m_fCENTER_POINT;

            //**************************************************************//
            // Act
            //**************************************************************//

            for (int iIndex = 0; iIndex < m_iLARGE_POINT_COUNT; iIndex++)
            {
                // Alternate either side of the mean, moving further out as the count increases
                double fOffset = (0 == (iIndex % 2)) ? (iIndex * 1.0e-6) : (-iIndex * 1.0e-6);
                double fCurrentPoint = m_fCENTER_POINT + fOffset;

                if (fCurrentPoint > fLargestPoint)
                {
                    fLargestPoint = fCurrentPoint;
                }

                if (fCurrentPoint < fSmallestPoint)
                {
                    fSmallestPoint = fCurrentPoint;
                }

                chart.AddPoint(fCurrentPoint, m_fCENTER_POINT);
            }

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Every point added is still displayed, so the limits must contain all of them
            Axis chartYAxis = GetYAxis(chart);
            Assert.IsTrue(chartYAxis.Maximum > fLargestPoint, "Maximum limit must be above the largest point");
            Assert.IsTrue(chartYAxis.Minimum < fSmallestPoint, "Minimum limit must be below the smallest point");
        }

        /// <summary>
        /// Tests that clearing the chart resets the limits to the defaults
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Clear_AfterWidening_LimitsReset()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();
            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Act
            //**************************************************************//

            chart.Clear();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fDEFAULT_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fDEFAULT_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        /// <summary>
        /// Tests that the limits widen correctly for points added after the chart is cleared
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Clear_PointsAddedAfterClear_LimitsWiden()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGChart chart = new RNGChart();
            chart.AddPoint(m_fHIGH_POINT, m_fCENTER_POINT);
            chart.Clear();

            //**************************************************************//
            // Act
            //**************************************************************//

            chart.AddPoint(m_fLOW_POINT, m_fCENTER_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Axis chartYAxis = GetYAxis(chart);
            Assert.AreEqual(m_fWIDENED_MAXIMUM, chartYAxis.Maximum, m_fCOMPARE_DELTA);
            Assert.AreEqual(m_fWIDENED_MINIMUM, chartYAxis.Minimum, m_fCOMPARE_DELTA);
        }

        #endregion
        #region Helper Types

        /// <summary>
        /// Chart with a reduced display size so points rolling off of the chart can be tested
        /// </summary>
        private class TestableRNGChart : RNGChart
        {
            /// <summary>
            /// Initializing constructor
            /// </summary>
            /// <param name="iMaxDataSize">IN - Maximum number of points to display on the chart</param>
            public TestableRNGChart(int iMaxDataSize) : base()
            {
                m_iMaxDataSize = iMaxDataSize;
            }

            /// <summary>
            /// The maximum number of points to display on the chart (read-only)
            /// </summary>
            public override int MaxDataSize { get => m_iMaxDataSize; }

            private readonly int m_iMaxDataSize;
        }

        #endregion
    }
}
