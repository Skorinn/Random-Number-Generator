//*********************************************************************************************************************
// File Name:      StatisticalAnalysis.Test.cs
// Description:    Unit tests for the StatisticalAnalysis class
//
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2025/07/21 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the StatisticalAnalysis class
    /// </summary>
    [TestClass]
    public class StatisticalAnalysisTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public StatisticalAnalysisTests()
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

        // Test file paths
        private const string m_sTEST_FILE_PATH = "TestStatisticsFile.StatisticalAnalysisTests.xml";
        private const string m_sNON_EXISTENT_FILE = "NonExistentStatistics.xml";
        private const string m_sINVALID_XML_FILE = "InvalidStatistics.xml";

        // Test data values  
        private const double m_fTEST_DATA_1 = 0.1;
        private const double m_fTEST_DATA_2 = 0.5;
        private const double m_fTEST_DATA_3 = 0.9;
        private const double m_fTEST_DATA_4 = 0.3;
        private const double m_fTEST_DATA_5 = 0.7;

        // Test batch sizes
        private const uint m_iBATCH_SIZE = 1000;

        // Expected XML content for valid file
        private const string m_sVALID_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.1</Data>
    <Data Time=""01:30:46"">0.5</Data>
    <Data Time=""01:30:47"">0.9</Data>
    <Data Time=""01:30:48"">0.3</Data>
    <Data Time=""01:30:49"">0.7</Data>
</Session>";

        // Expected malformed XML content
        private const string m_sMALFORMED_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.1</Data>
    <UnclosedElement>
</Session>";

        // Test names for data sets
        private const string m_sTEST_NAME_A = "Test Data Set A";
        private const string m_sTEST_NAME_B = "Test Data Set B";
        private const string m_sSINGLE_DATA_SET_NAME = "Single Test Data";

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
        /// Cleans up after each test runs to ensure we always start with a clean state
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Delete test files if created
            if (File.Exists(m_sTEST_FILE_PATH))
            {
                File.Delete(m_sTEST_FILE_PATH);
            }
            if (File.Exists(m_sNON_EXISTENT_FILE))
            {
                File.Delete(m_sNON_EXISTENT_FILE);
            }
            if (File.Exists(m_sINVALID_XML_FILE))
            {
                File.Delete(m_sINVALID_XML_FILE);
            }
        }

        #endregion
        #region Tests

        #region Constructor Tests

        /// <summary>
        /// Tests that default constructor creates object with correct initial properties
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Default_PropertiesInitializedCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected initial values
            string sEXPECTED_LOADED_FILE_NAME = string.Empty;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify initial properties
            Assert.AreEqual(sEXPECTED_LOADED_FILE_NAME, analysis.LoadedFileName);
            Assert.IsNull(analysis.LoadedFileStats);
            Assert.IsNull(analysis.StatsA);
            Assert.IsNull(analysis.StatsB);
        }

        #endregion
        #region LoadResultFile Tests

        /// <summary>
        /// Tests LoadResultFile with valid XML file loads data successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadResultFile_ValidXMLFile_LoadsDataSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Expected values
            const int iEXPECTED_DATA_COUNT = 5;
            string sEXPECTED_FILE_NAME = Path.GetFileName(m_sTEST_FILE_PATH);

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the result file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the result data was loaded correctly
            Assert.IsNotNull(resultData);
            Assert.AreEqual(iEXPECTED_DATA_COUNT, resultData.Count);

            // Verify the expected data values
            Assert.AreEqual(m_fTEST_DATA_1, resultData[0], 0.000001);
            Assert.AreEqual(m_fTEST_DATA_2, resultData[1], 0.000001);
            Assert.AreEqual(m_fTEST_DATA_3, resultData[2], 0.000001);
            Assert.AreEqual(m_fTEST_DATA_4, resultData[3], 0.000001);
            Assert.AreEqual(m_fTEST_DATA_5, resultData[4], 0.000001);

            // Verify properties were updated
            Assert.AreEqual(sEXPECTED_FILE_NAME, analysis.LoadedFileName);
            Assert.IsNotNull(analysis.LoadedFileStats);

            // Verify statistics properties are accessible and have reasonable values
            Assert.IsTrue(analysis.LoadedFileStats.Count > 0);
            Assert.IsTrue(analysis.LoadedFileStats.Mean >= 0.0);
            Assert.IsTrue(analysis.LoadedFileStats.Mean <= 1.0);
            Assert.IsTrue(analysis.LoadedFileStats.StandardDeviation >= 0.0);
        }

        /// <summary>
        /// Tests LoadResultFile with null file path throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadResultFile_NullFilePath_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load with null file path - should throw exception
            analysis.LoadResultFile(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests LoadResultFile with empty file path throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadResultFile_EmptyFilePath_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load with empty file path - should throw exception
            analysis.LoadResultFile(string.Empty);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests LoadResultFile with non-existent file throws FileNotFoundException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        public void LoadResultFile_NonExistentFile_ThrowsFileNotFoundException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load non-existent file - should throw exception
            analysis.LoadResultFile(m_sNON_EXISTENT_FILE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests LoadResultFile with malformed XML throws InvalidOperationException with XML parsing error
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadResultFile_MalformedXML_ThrowsInvalidOperationException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create malformed XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sMALFORMED_XML_CONTENT);

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load malformed XML file - should throw exception
            analysis.LoadResultFile(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests LoadResultFile with empty file (no data points) returns empty list
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadResultFile_EmptySessionFile_ReturnsEmptyList()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create XML file with no data points
            const string sEMPTY_SESSION_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
</Session>";
            File.WriteAllText(m_sTEST_FILE_PATH, sEMPTY_SESSION_XML);

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the empty session file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify empty result
            Assert.IsNotNull(resultData);
            Assert.AreEqual(0, resultData.Count);
            Assert.IsNull(analysis.LoadedFileStats); // No stats created for empty data
        }

        #endregion
        #region CompareDistributions Tests

        /// <summary>
        /// Tests CompareDistributions with valid data sets completes successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareDistributions_ValidDataSets_CompletesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2, m_fTEST_DATA_3 };
            List<double> dataSetB = new List<double> { m_fTEST_DATA_4, m_fTEST_DATA_5, m_fTEST_DATA_1 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Compare the distributions
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify statistics were created and stored
            Assert.IsNotNull(analysis.StatsA);
            Assert.IsNotNull(analysis.StatsB);
            Assert.AreEqual(dataSetA.Count, analysis.StatsA.Count);
            Assert.AreEqual(dataSetB.Count, analysis.StatsB.Count);
        }

        /// <summary>
        /// Tests CompareDistributions with null first data set throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompareDistributions_NullFirstDataSet_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create second data set only
            List<double> dataSetB = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with null first data set - should throw exception
            analysis.CompareDistributions(null, m_sTEST_NAME_A, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with null second data set throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompareDistributions_NullSecondDataSet_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create first data set only
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with null second data set - should throw exception
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, null, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with null first name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void CompareDistributions_NullFirstName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };
            List<double> dataSetB = new List<double> { m_fTEST_DATA_3, m_fTEST_DATA_4 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with null first name - should throw exception
            analysis.CompareDistributions(dataSetA, null, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with empty first name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void CompareDistributions_EmptyFirstName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };
            List<double> dataSetB = new List<double> { m_fTEST_DATA_3, m_fTEST_DATA_4 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with empty first name - should throw exception
            analysis.CompareDistributions(dataSetA, string.Empty, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with null second name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void CompareDistributions_NullSecondName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };
            List<double> dataSetB = new List<double> { m_fTEST_DATA_3, m_fTEST_DATA_4 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with null second name - should throw exception
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, dataSetB, null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with empty second name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void CompareDistributions_EmptySecondName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };
            List<double> dataSetB = new List<double> { m_fTEST_DATA_3, m_fTEST_DATA_4 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt comparison with empty second name - should throw exception
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, dataSetB, string.Empty);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests CompareDistributions with empty data sets completes successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareDistributions_EmptyDataSets_CompletesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create empty data sets
            List<double> dataSetA = new List<double>();
            List<double> dataSetB = new List<double>();

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Compare the empty distributions
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify statistics were created for empty sets
            Assert.IsNotNull(analysis.StatsA);
            Assert.IsNotNull(analysis.StatsB);
            Assert.AreEqual(0, analysis.StatsA.Count);
            Assert.AreEqual(0, analysis.StatsB.Count);
        }

        #endregion
        #region AnalyzeSingleDataSet Tests

        /// <summary>
        /// Tests AnalyzeSingleDataSet with valid data set completes successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AnalyzeSingleDataSet_ValidDataSet_CompletesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data set
            List<double> dataSet = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2, m_fTEST_DATA_3, m_fTEST_DATA_4, m_fTEST_DATA_5 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Analyze the single data set
            analysis.AnalyzeSingleDataSet(dataSet, m_sSINGLE_DATA_SET_NAME);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify no exception was thrown and method completed
            Assert.IsNotNull(analysis);
        }

        /// <summary>
        /// Tests AnalyzeSingleDataSet with null data set throws ArgumentNullException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AnalyzeSingleDataSet_NullDataSet_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt analysis with null data set - should throw exception
            analysis.AnalyzeSingleDataSet(null, m_sSINGLE_DATA_SET_NAME);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests AnalyzeSingleDataSet with null name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void AnalyzeSingleDataSet_NullName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data set
            List<double> dataSet = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt analysis with null name - should throw exception
            analysis.AnalyzeSingleDataSet(dataSet, null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests AnalyzeSingleDataSet with empty name throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void AnalyzeSingleDataSet_EmptyName_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data set
            List<double> dataSet = new List<double> { m_fTEST_DATA_1, m_fTEST_DATA_2 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt analysis with empty name - should throw exception
            analysis.AnalyzeSingleDataSet(dataSet, string.Empty);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests AnalyzeSingleDataSet with empty data set throws ArgumentException
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentException))]
        public void AnalyzeSingleDataSet_EmptyDataSet_ThrowsArgumentException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create empty data set
            List<double> dataSet = new List<double>();

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt analysis with empty data set - should throw exception
            analysis.AnalyzeSingleDataSet(dataSet, m_sSINGLE_DATA_SET_NAME);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests AnalyzeSingleDataSet with large data set completes successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AnalyzeSingleDataSet_LargeDataSet_CompletesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create large test data set
            List<double> dataSet = new List<double>();
            const int iLARGE_SET_SIZE = 10000;
            Random random = new Random(42); // Use seed for reproducible tests

            for (int iIndex = 0; iIndex < iLARGE_SET_SIZE; iIndex++)
            {
                double fValue = random.NextDouble();
                dataSet.Add(fValue);
            }

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Analyze the large data set
            analysis.AnalyzeSingleDataSet(dataSet, m_sSINGLE_DATA_SET_NAME);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify no exception was thrown and method completed
            Assert.IsNotNull(analysis);
        }

        #endregion
        #region Property Tests

        /// <summary>
        /// Tests LoadedFileStats property returns correct statistics after loading file
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadedFileStats_AfterLoadingFile_ReturnsCorrectStatistics()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Expected values
            const int iEXPECTED_COUNT = 5;

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the result file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify LoadedFileStats property
            Assert.IsNotNull(analysis.LoadedFileStats);
            Assert.AreEqual(iEXPECTED_COUNT, analysis.LoadedFileStats.Count);

            // Verify statistical values make sense for 0-1 range data
            Assert.IsTrue(analysis.LoadedFileStats.Mean >= 0.0);
            Assert.IsTrue(analysis.LoadedFileStats.Mean <= 1.0);
            Assert.IsTrue(analysis.LoadedFileStats.StandardDeviation >= 0.0);
        }

        /// <summary>
        /// Tests LoadedFileName property returns correct filename after loading file
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadedFileName_AfterLoadingFile_ReturnsCorrectFilename()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Expected filename
            string sEXPECTED_FILENAME = Path.GetFileName(m_sTEST_FILE_PATH);

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the result file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify LoadedFileName property
            Assert.AreEqual(sEXPECTED_FILENAME, analysis.LoadedFileName);
        }

        /// <summary>
        /// Tests StatsA and StatsB properties return correct statistics after comparison
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StatsAAndStatsB_AfterComparison_ReturnCorrectStatistics()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test data sets
            List<double> dataSetA = new List<double> { 0.1, 0.2, 0.3 };
            List<double> dataSetB = new List<double> { 0.4, 0.5, 0.6 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Compare the distributions
            analysis.CompareDistributions(dataSetA, m_sTEST_NAME_A, dataSetB, m_sTEST_NAME_B);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify StatsA property
            Assert.IsNotNull(analysis.StatsA);
            Assert.AreEqual(dataSetA.Count, analysis.StatsA.Count);
            Assert.AreEqual(0.2, analysis.StatsA.Mean, 0.000001); // Mean of 0.1, 0.2, 0.3

            // Verify StatsB property  
            Assert.IsNotNull(analysis.StatsB);
            Assert.AreEqual(dataSetB.Count, analysis.StatsB.Count);
            Assert.AreEqual(0.5, analysis.StatsB.Mean, 0.000001); // Mean of 0.4, 0.5, 0.6
        }

        #endregion
        #region Integration Tests

        /// <summary>
        /// Tests complete workflow: LoadResultFile followed by AnalyzeSingleDataSet
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void CompleteWorkflow_LoadFileAndAnalyze_WorksCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the result file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            // Analyze the loaded data
            analysis.AnalyzeSingleDataSet(resultData, "Loaded Data Analysis");

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify both operations completed successfully
            Assert.IsNotNull(resultData);
            Assert.AreEqual(5, resultData.Count);
            Assert.IsNotNull(analysis.LoadedFileStats);
            Assert.IsNotNull(analysis.LoadedFileName);
        }

        /// <summary>
        /// Tests complete workflow: LoadResultFile followed by CompareDistributions
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void CompleteWorkflow_LoadFileAndCompare_WorksCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Create second data set for comparison
            List<double> secondDataSet = new List<double> { 0.2, 0.4, 0.6, 0.8, 1.0 };

            // Create the object under test
            StatisticalAnalysis analysis = new StatisticalAnalysis();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the result file
            List<double> resultData = analysis.LoadResultFile(m_sTEST_FILE_PATH);

            // Compare the loaded data with second data set
            analysis.CompareDistributions(resultData, "Loaded Data", secondDataSet, "Generated Data");

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify all operations completed successfully
            Assert.IsNotNull(resultData);
            Assert.IsNotNull(analysis.LoadedFileStats);
            Assert.IsNotNull(analysis.StatsA);
            Assert.IsNotNull(analysis.StatsB);
            Assert.AreEqual(resultData.Count, analysis.StatsA.Count);
            Assert.AreEqual(secondDataSet.Count, analysis.StatsB.Count);
        }

        #endregion

        #endregion
    }
}