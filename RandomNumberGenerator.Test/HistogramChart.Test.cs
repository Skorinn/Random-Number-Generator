//*********************************************************************************************************************
// File Name:      HistogramChart.Test.cs
// Description:    Unit tests for the HistogramChart class
//
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2025/01/20 - Mike Pullen - Original implementation.
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

        // Test labels
        private const string m_sTEST_LABEL_1 = "Test Series 1";
        private const string m_sTEST_LABEL_2 = "Test Series 2";
        private const string m_sEMPTY_LABEL = "";

        // Test bin counts
        private const int m_iTEST_BIN_COUNT = 10;
        private const int m_iINVALID_BIN_COUNT_ZERO = 0;
        private const int m_iINVALID_BIN_COUNT_NEGATIVE = -5;

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

        #endregion
    }
}