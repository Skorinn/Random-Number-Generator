//*********************************************************************************************************************
// File Name:      HistogramChart.Test.cs
// Description:    Unit tests for the HistogramChart class
//
// Copyright (c) 2025 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2025/01/20 - Mike Pullen - Original implementation.
// 2026/09/07 - Mike Pullen - Cover the chosen bin count and the axis label precision
// 2026/09/07 - Mike Pullen - Cover the axis being a share of a session rather than a count of readings
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the HistogramChart class
    /// </summary>
    [TestClass]
    public class HistogramChartTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public HistogramChartTests()
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

        #endregion
        #region Data Members

        // Test data sets
        private readonly List<double> m_lstTEST_DATA_1 = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5 };
        private readonly List<double> m_lstTEST_DATA_2 = new List<double> { 0.6, 0.7, 0.8, 0.9, 1.0 };
        private readonly List<double> m_lstSAME_VALUES = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5 };
        private readonly List<double> m_lstWIDE_RANGE = new List<double> { -100.0, -50.0, 0.0, 50.0, 100.0 };
        private readonly List<double> m_lstSINGLE_VALUE = new List<double> { 0.5 };

        // Test data sets for dynamic Y-axis scaling tests
        private readonly List<double> m_lstHIGH_FREQUENCY_DATA = new List<double> 
        { 
            0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1,  // 10 values at 0.1
            0.2, 0.2, 0.2, 0.2, 0.2,                              // 5 values at 0.2
            0.3                                                    // 1 value at 0.3
        };
        private readonly List<double> m_lstLOW_FREQUENCY_DATA = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5 };
        private readonly List<double> m_lstMIXED_FREQUENCY_DATA = new List<double> 
        { 
            0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5,    // 10 values at 0.5
            0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5,    // 20 total at 0.5
            0.6, 0.7, 0.8, 0.9                                     // 1 each at other values
        };
        private readonly List<double> m_lstLARGE_DATASET = new List<double>();

        // Test labels
        private const string m_sTEST_LABEL_1 = "Test Series 1";
        private const string m_sTEST_LABEL_2 = "Test Series 2";
        private const string m_sEMPTY_LABEL = "";

        // Test bin counts
        private const int m_iTEST_BIN_COUNT = 10;
        private const int m_iSMALL_BIN_COUNT = 5;
        private const int m_iLARGE_BIN_COUNT = 20;
        private const int m_iINVALID_BIN_COUNT_ZERO = 0;
        private const int m_iINVALID_BIN_COUNT_NEGATIVE = -5;

        // Sample sizes for checking how the bin count is chosen, and the bounds it is held between
        private const int m_iTINY_SAMPLE_SIZE = 5;
        private const int m_iMODERATE_SAMPLE_SIZE = 200;
        private const int m_iLARGE_SAMPLE_SIZE = 100000;
        private const int m_iMINIMUM_BIN_COUNT = 10;
        private const int m_iMAXIMUM_BIN_COUNT = 60;
        private const int m_iMAXIMUM_LABEL_DECIMALS = 6;

        // The chart plots shares of a session rather than counts of readings, so the axis is a percentage
        private const double m_fY_AXIS_HEADROOM = 4.0;
        private const double m_fUNEVEN_SHARE_ALLOWANCE = 3.0;

        // Y-axis scaling test constants
        private const double m_fEXPECTED_Y_AXIS_TOLERANCE = 0.01; // Tolerance for Y-axis value comparisons
        private const double m_fDEFAULT_Y_MINIMUM = 0.0;
        private const double m_fDEFAULT_Y_MAXIMUM_EMPTY = 10.0;

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
        #region Initialization and cleanup

        /// <summary>
        /// Initializes test data before running tests
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize the large dataset with 1000 points for performance testing
            m_lstLARGE_DATASET.Clear();
            Random random = new Random(42); // Fixed seed for consistent test results
            for (int i = 0; i < 1000; i++)
            {
                m_lstLARGE_DATASET.Add(random.NextDouble());
            }
        }

        #endregion
        #region Tests

        /// <summary>
        /// Tests that the constructor properly initializes the chart
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Default_CreatesInstance()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // No arrangement needed for constructor test

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the histogram chart
            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart was created successfully (basic instantiation test)
            Assert.IsNotNull(histogramChart);
        }

        /// <summary>
        /// Tests that Plot method throws ArgumentNullException when data1 is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Plot_NullData1_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with null data1 - should throw exception
            histogramChart.Plot(null, m_sTEST_LABEL_1, m_lstTEST_DATA_2, m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method throws ArgumentNullException when data2 is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Plot_NullData2_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with null data2 - should throw exception
            histogramChart.Plot(m_lstTEST_DATA_1, m_sTEST_LABEL_1, null, m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method throws ArgumentException when binCount is zero
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void Plot_ZeroBinCount_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with zero bin count - should throw exception
            histogramChart.Plot(m_lstTEST_DATA_1, m_sTEST_LABEL_1, m_lstTEST_DATA_2, m_sTEST_LABEL_2, m_iINVALID_BIN_COUNT_ZERO);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method throws ArgumentException when binCount is negative
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void Plot_NegativeBinCount_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with negative bin count - should throw exception
            histogramChart.Plot(m_lstTEST_DATA_1, m_sTEST_LABEL_1, m_lstTEST_DATA_2, m_sTEST_LABEL_2, m_iINVALID_BIN_COUNT_NEGATIVE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method successfully executes with valid data
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_ValidData_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot with valid data - should not throw exception
            try
            {
                histogramChart.Plot(m_lstTEST_DATA_1, m_sTEST_LABEL_1, m_lstTEST_DATA_2, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with valid data");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with valid data. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method successfully executes with default bin count
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_NoSpecifiedBinCount_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot without specifying bin count - should not throw exception
            try
            {
                histogramChart.Plot(m_lstTEST_DATA_1, m_sTEST_LABEL_1, m_lstTEST_DATA_2, m_sTEST_LABEL_2);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with default bin count");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with valid data and default bin count. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method handles empty data sets correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_EmptyDataSets_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData1 = new List<double>();
            List<double> emptyData2 = new List<double>();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot with empty data sets - should not throw exception
            try
            {
                histogramChart.Plot(emptyData1, m_sTEST_LABEL_1, emptyData2, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with empty data sets");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with empty data sets. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method handles data with identical values correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_IdenticalValues_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot with identical values - should not throw exception
            try
            {
                histogramChart.Plot(m_lstSAME_VALUES, m_sTEST_LABEL_1, m_lstSAME_VALUES, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with identical values");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with identical values. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method handles wide range data correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_WideRangeData_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot with wide range data - should not throw exception
            try
            {
                histogramChart.Plot(m_lstWIDE_RANGE, m_sTEST_LABEL_1, m_lstWIDE_RANGE, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with wide range data");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with wide range data. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method handles single data point correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_SingleDataPoint_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Call Plot with single data point - should not throw exception
            try
            {
                histogramChart.Plot(m_lstSINGLE_VALUE, m_sTEST_LABEL_1, m_lstSINGLE_VALUE, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

                // If we reach here, the method executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with single data point");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should not throw exception with single data point. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Plot method handles empty label strings correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void Plot_EmptyLabel_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with empty label - should throw exception during series creation
            histogramChart.Plot(m_lstTEST_DATA_1, m_sEMPTY_LABEL, m_lstTEST_DATA_2, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method handles null label strings correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void Plot_NullLabel_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call Plot with null label - should throw exception during series creation
            histogramChart.Plot(m_lstTEST_DATA_1, null, m_lstTEST_DATA_2, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected - handled by ExpectedException attribute
        }

        /// <summary>
        /// Tests that Plot method can be called multiple times successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_MultipleCalls_ExecutesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            try
            {
                // First plot call
                histogramChart.Plot(m_lstTEST_DATA_1, "Series1A", m_lstTEST_DATA_2, "Series2A", m_iTEST_BIN_COUNT);

                // Second plot call - should clear previous and create new series
                histogramChart.Plot(m_lstWIDE_RANGE, "Series1B", m_lstSAME_VALUES, "Series2B", m_iTEST_BIN_COUNT);

                // If we reach here, both method calls executed successfully
                Assert.IsTrue(true, "Plot method executed successfully with multiple calls");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Plot method should support multiple calls. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests that Y-axis scales dynamically based on bin frequencies with high frequency data
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_HighFrequencyData_YAxisScalesDynamically()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData = new List<double>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot high frequency data which should result in Y-axis maximum > 10
            histogramChart.Plot(m_lstHIGH_FREQUENCY_DATA, m_sTEST_LABEL_1, emptyData, m_sTEST_LABEL_2, m_iSMALL_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis minimum is set to default (0.0)
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should be set to default value for frequency data");

            // Verify Y-axis maximum scales beyond default to accommodate high frequency
            Assert.IsTrue((fYAxisMaximum > m_fDEFAULT_Y_MAXIMUM_EMPTY), 
                         $"Y-axis maximum should scale beyond default {m_fDEFAULT_Y_MAXIMUM_EMPTY} for high frequency data. Actual: {fYAxisMaximum}");

            // Verify Y-axis interval is set appropriately
            double fYAxisInterval = chartArea.AxisY.Interval;
            Assert.IsTrue((fYAxisInterval >= 1.0), 
                         $"Y-axis interval should be at least 1.0 for frequency data. Actual: {fYAxisInterval}");
        }

        /// <summary>
        /// Tests that Y-axis scales appropriately for low frequency data
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_LowFrequencyData_YAxisScalesAppropriately()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData = new List<double>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot low frequency data which should result in modest Y-axis scaling
            histogramChart.Plot(m_lstLOW_FREQUENCY_DATA, m_sTEST_LABEL_1, emptyData, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis minimum is set to default (0.0)
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should be set to default value for frequency data");

            // The chart plots each session as a share of its own readings, so with five readings spread so
            // that no bin holds more than one, the tallest bar is a fifth of the session
            const double fEXPECTED_TALLEST_SHARE = 20.0;
            Assert.IsTrue((fYAxisMaximum >= fEXPECTED_TALLEST_SHARE),
                         $"Y-axis maximum should reach the tallest share. Actual: {fYAxisMaximum}");
            Assert.IsTrue((fYAxisMaximum <= (fEXPECTED_TALLEST_SHARE + m_fY_AXIS_HEADROOM)),
                         $"Y-axis maximum should not tower over the tallest share. Actual: {fYAxisMaximum}");

            // Verify Y-axis interval is set appropriately
            double fYAxisInterval = chartArea.AxisY.Interval;
            Assert.IsTrue((fYAxisInterval >= 1.0), 
                         $"Y-axis interval should be at least 1.0 for frequency data. Actual: {fYAxisInterval}");
        }

        /// <summary>
        /// Tests that Y-axis uses default range when both data sets are empty
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_EmptyDataSets_YAxisUsesDefaultRange()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData1 = new List<double>();
            List<double> emptyData2 = new List<double>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot empty data sets
            histogramChart.Plot(emptyData1, m_sTEST_LABEL_1, emptyData2, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis uses default range for empty data
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should use default value for empty data");
            Assert.AreEqual(m_fDEFAULT_Y_MAXIMUM_EMPTY, fYAxisMaximum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis maximum should use default value for empty data");
        }

        /// <summary>
        /// Tests that Y-axis scales correctly when comparing two data sets with different frequencies
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_TwoDataSetsWithDifferentFrequencies_YAxisScalesToAccommodateBoth()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot mixed frequency data sets - should scale to accommodate the highest frequency
            histogramChart.Plot(m_lstMIXED_FREQUENCY_DATA, m_sTEST_LABEL_1, m_lstLOW_FREQUENCY_DATA, m_sTEST_LABEL_2, m_iSMALL_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis minimum is set to default (0.0)
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should be set to default value");

            // Verify Y-axis maximum scales to accommodate the highest frequency from either data set
            // Mixed frequency data has 20 values at 0.5, so Y-axis should scale significantly
            Assert.IsTrue((fYAxisMaximum > 15.0), 
                         $"Y-axis maximum should scale to accommodate highest frequency. Actual: {fYAxisMaximum}");

            // Verify both series were created
            Assert.AreEqual(2, histogramChart.Series.Count, "Two series should be created for the data sets");
        }

        /// <summary>
        /// Tests that Y-axis scaling works correctly with large data sets
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_LargeDataSet_YAxisScalesAppropriately()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData = new List<double>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot large data set
            histogramChart.Plot(m_lstLARGE_DATASET, m_sTEST_LABEL_1, emptyData, m_sTEST_LABEL_2, m_iLARGE_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis minimum is set to default (0.0)
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should be set to default value");

            // A thousand readings spread over twenty bins put roughly a twentieth of the session in each, so
            // the axis stays around that share however many readings there are. Plotting shares rather than
            // counts is what lets a long session and a short one be compared on the same chart.
            const double fEVEN_SHARE = 5.0;
            Assert.IsTrue((fYAxisMaximum > fEVEN_SHARE),
                         $"Y-axis maximum should reach past an even share. Actual: {fYAxisMaximum}");
            Assert.IsTrue((fYAxisMaximum < (fEVEN_SHARE * m_fUNEVEN_SHARE_ALLOWANCE)),
                         $"Y-axis maximum should stay near the share, not the count. Actual: {fYAxisMaximum}");

            // Verify Y-axis interval is reasonable
            double fYAxisInterval = chartArea.AxisY.Interval;
            Assert.IsTrue((fYAxisInterval >= 1.0), 
                         $"Y-axis interval should be at least 1.0 for frequency data. Actual: {fYAxisInterval}");
        }

        /// <summary>
        /// Tests that Y-axis scaling adds appropriate padding to the maximum frequency
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_WithPadding_YAxisMaximumExceedsMaximumFrequency()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData = new List<double>();

            // Create data with known maximum frequency
            List<double> knownFrequencyData = new List<double> 
            { 
                0.5, 0.5, 0.5, 0.5, 0.5  // 5 values in one bin
            };

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot data with 5 bins, so all values will be in one bin with frequency = 5
            histogramChart.Plot(knownFrequencyData, m_sTEST_LABEL_1, emptyData, m_sTEST_LABEL_2, m_iSMALL_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis range
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify Y-axis maximum includes padding beyond the maximum frequency (5)
            // Should be at least 7 (5 + minimum padding of 2.0) due to padding
            Assert.IsTrue((fYAxisMaximum >= 7.0), 
                         $"Y-axis maximum should include padding beyond maximum frequency of 5. Actual: {fYAxisMaximum}");
            
            // Verify it's actually greater than the raw frequency value
            Assert.IsTrue((fYAxisMaximum > 5.0), 
                         $"Y-axis maximum should exceed the raw maximum frequency of 5. Actual: {fYAxisMaximum}");
        }

        /// <summary>
        /// Tests that Y-axis interval calculation works correctly for different frequency ranges
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_DifferentFrequencyRanges_YAxisIntervalCalculatedCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> emptyData = new List<double>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot high frequency data to test interval calculation
            histogramChart.Plot(m_lstHIGH_FREQUENCY_DATA, m_sTEST_LABEL_1, emptyData, m_sTEST_LABEL_2, m_iSMALL_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get Y-axis properties
            var chartArea = histogramChart.ChartAreas[0];
            double fYAxisMaximum = chartArea.AxisY.Maximum;
            double fYAxisInterval = chartArea.AxisY.Interval;

            // Verify interval is appropriate for the frequency range
            Assert.IsTrue((fYAxisInterval >= 1.0), 
                         $"Y-axis interval should be at least 1.0 for frequency data. Actual: {fYAxisInterval}");

            // Verify interval creates reasonable number of tick marks (approximately 5-10)
            double fNumberOfIntervals = fYAxisMaximum / fYAxisInterval;
            Assert.IsTrue((fNumberOfIntervals >= 3.0 && fNumberOfIntervals <= 15.0), 
                         $"Y-axis should have reasonable number of intervals (3-15). Actual: {fNumberOfIntervals}");
        }

        /// <summary>
        /// Tests that X-axis and Y-axis scaling work together correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_BothAxisScaling_XAndYAxisScaleIndependently()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Plot wide range data with mixed frequencies
            histogramChart.Plot(m_lstWIDE_RANGE, m_sTEST_LABEL_1, m_lstMIXED_FREQUENCY_DATA, m_sTEST_LABEL_2, m_iTEST_BIN_COUNT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify chart area was created
            Assert.IsTrue((histogramChart.ChartAreas.Count > 0), "Chart area should be created");

            // Get axis properties
            var chartArea = histogramChart.ChartAreas[0];
            double fXAxisMinimum = chartArea.AxisX.Minimum;
            double fXAxisMaximum = chartArea.AxisX.Maximum;
            double fYAxisMinimum = chartArea.AxisY.Minimum;
            double fYAxisMaximum = chartArea.AxisY.Maximum;

            // Verify X-axis scales to accommodate wide range data (-100 to 100)
            Assert.IsTrue((fXAxisMinimum < -50.0), $"X-axis minimum should accommodate wide range data. Actual: {fXAxisMinimum}");
            Assert.IsTrue((fXAxisMaximum > 50.0), $"X-axis maximum should accommodate wide range data. Actual: {fXAxisMaximum}");

            // Verify Y-axis scales independently based on frequency data
            Assert.AreEqual(m_fDEFAULT_Y_MINIMUM, fYAxisMinimum, m_fEXPECTED_Y_AXIS_TOLERANCE, 
                           "Y-axis minimum should be independent of X-axis range");
            Assert.IsTrue((fYAxisMaximum > m_fDEFAULT_Y_MAXIMUM_EMPTY),
                         $"Y-axis maximum should scale based on frequency, not X-axis range. Actual: {fYAxisMaximum}");
        }

        #endregion
        #region Bin Count and Axis Labelling Tests

        /// <summary>
        /// Tests that a plot with no bin count given divides the data into bins that follow the sample size
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_NoBinCount_ChoosesBinCountFromSampleSize()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            // Two hundred readings spread over a range, which the cube root rule divides into twelve bins
            List<double> spreadData = BuildSpreadData(m_iMODERATE_SAMPLE_SIZE);
            const int iEXPECTED_BINS = 12;

            //**************************************************************//
            // Act
            //**************************************************************//

            histogramChart.Plot(spreadData, m_sTEST_LABEL_1, new List<double>(), m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.AreEqual(iEXPECTED_BINS, histogramChart.Series[0].Points.Count,
                            "The bin count should follow the cube root of the sample size");
        }

        /// <summary>
        /// Tests that a handful of readings still produces enough bins to show a shape
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_TinySample_UsesTheMinimumBinCount()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> tinyData = BuildSpreadData(m_iTINY_SAMPLE_SIZE);

            //**************************************************************//
            // Act
            //**************************************************************//

            histogramChart.Plot(tinyData, m_sTEST_LABEL_1, new List<double>(), m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.AreEqual(m_iMINIMUM_BIN_COUNT, histogramChart.Series[0].Points.Count,
                            "A sample smaller than the floor should still be given the floor");
        }

        /// <summary>
        /// Tests that a long session does not produce more bins than the chart can show as bars
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_LargeSample_CapsTheBinCount()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();
            List<double> largeData = BuildSpreadData(m_iLARGE_SAMPLE_SIZE);

            //**************************************************************//
            // Act
            //**************************************************************//

            histogramChart.Plot(largeData, m_sTEST_LABEL_1, new List<double>(), m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.AreEqual(m_iMAXIMUM_BIN_COUNT, histogramChart.Series[0].Points.Count,
                            "A sample large enough to exceed the ceiling should be held at it");
        }

        /// <summary>
        /// Tests that the axis labels are printed to the precision that tells one tick from the next, rather
        /// than at the full precision of a double, which runs the labels into one another
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Plot_BitAverages_LabelsAxisToTickPrecision()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            HistogramChart histogramChart = new HistogramChart();

            // Values of the shape the device produces, which are what showed the full precision on the axis
            List<double> bitAverages = new List<double>();
            for (int iIndex = 0; iIndex < m_iMODERATE_SAMPLE_SIZE; iIndex++)
            {
                bitAverages.Add(0.489044189453125 + (iIndex * 0.0001220703125));
            }

            //**************************************************************//
            // Act
            //**************************************************************//

            histogramChart.Plot(bitAverages, m_sTEST_LABEL_1, new List<double>(), m_sTEST_LABEL_2);

            //**************************************************************//
            // Assert
            //**************************************************************//

            var chartArea = histogramChart.ChartAreas[0];
            string sFormat = chartArea.AxisX.LabelStyle.Format;
            Assert.IsFalse(string.IsNullOrEmpty(sFormat), "The x-axis should be given a label format");

            // The label the axis minimum produces has to be short enough to sit beside its neighbour
            string sLabel = chartArea.AxisX.Minimum.ToString(sFormat);
            int iDecimals = (sLabel.Length - sLabel.IndexOf('.') - 1);
            Assert.IsTrue((iDecimals <= m_iMAXIMUM_LABEL_DECIMALS),
                          $"An axis label should not run to more than {m_iMAXIMUM_LABEL_DECIMALS} decimals. Actual: '{sLabel}'");

            // The frequency axis counts readings, so it is labelled with whole numbers
            string sFrequencyLabel = 3.0.ToString(chartArea.AxisY.LabelStyle.Format);
            Assert.AreEqual("3", sFrequencyLabel, "The frequency axis should be labelled with whole numbers");
        }

        /// <summary>
        /// Builds a set of readings spread evenly over a range, so the bins they fall into are decided by
        /// how many of them there are rather than by where they happen to sit
        /// </summary>
        /// <param name="iCount">IN - How many readings to build</param>
        /// <returns>The readings</returns>
        private static List<double> BuildSpreadData(int iCount)
        {
            List<double> data = new List<double>(iCount);
            for (int iIndex = 0; iIndex < iCount; iIndex++)
            {
                data.Add(iIndex / (double)iCount);
            }
            return data;
        }

        #endregion
    }
}