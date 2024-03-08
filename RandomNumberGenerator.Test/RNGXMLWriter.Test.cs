//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.Test.cs
// Description:    Unit tests for the RNGXMLWriter class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/01/21 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Xml;

namespace RandomNumberGenerator.Test
{
    /// <summary
    /// Mock implementation of the IXMLDataPoint interface
    /// <\summary>
    public class MockXMLDataPoint : IXMLDataPoint
    {
        /// <summary>
        /// Mock implementation of the WriteDataPoint method that writes an empty node
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public bool WriteDataPoint(XmlWriter writer)
        {
            // Write an empty node and return true
            writer.WriteElementString("DataPoint", "");
            return true;
        }

        public string SessionTime { get; set; }
        public double DataPoint { get; set; } = 0.0;
    }

    /// <summary>
    /// Unit tests for the RNGXMLWriter class
    /// </summary>
    [TestClass]
    public class RNGXMLWriterTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGXMLWriterTests()
        {
            // Initialize the XML writer settings
            m_writerSettings = new XmlWriterSettings();
            m_writerSettings.Indent = true;
            m_writerSettings.IndentChars = "\t";
            m_writerSettings.ConformanceLevel = ConformanceLevel.Auto;
        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion
        #region Data Members

        // Information about the current test context
        private TestContext testContextInstance;

        // XML writer settings to use for testing
        private XmlWriterSettings m_writerSettings;

        // Paths for files used for testing
        private const string m_sTEST_FILE_PATH = "TestSessionDataFile.RNGXMLWriterTests.xml";

        // Session time to use for testing
        private const string m_sSESSION_START_TIME = "01:00:00";
        private const string m_sSESSION_DATA_TIME = "01:54:21";

        // Target value to use for testing
        private const int m_iTARGET_VALUE = 1;

        // Data point to use for testing
        private const double m_fDATA_POINT = 9.1;

        // Define the expected XML entries here as they depend on the time, target, and data values above
        private const string sEXPECTED_SESSION_START_ENTRY = "<Session Start=\"01:00:00\" Target=\"1\">";
        private const string sEXPECTED_DATA_POINT_ENTRY = "<DataPoint Time=\"01:54:21\" Value=\"9.1\" />";
        private const string sEXPECTED_SESSION_END_ENTRY = "</Session>";
        private const string sEXPECTED_SESSION_END_ENTRY_NO_DATA = "<Session Start=\"01:00:00\" Target=\"1\" />";

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
        /// Cleans up after each rwar runs to ensure we always start with a clean file
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Delete the test file if created
            if (File.Exists(m_sTEST_FILE_PATH))
            {
                File.Delete(m_sTEST_FILE_PATH);
            }
        }

        #endregion
        #region Tests

        /// <summary>
        /// Tests properties are set correctly using the default constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Default_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected default values
            const string sEXPECTED_FILE_PATH = "";

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlWriter.FilePath);
        }

        /// <summary>
        /// Tests WriteSessionStart() method works correctly when the writer is valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionStart_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_SESSION_START_ENTRY;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);

            // Create the custom mock of the data point class
            MockXMLDataPoint mockDataPoint = new MockXMLDataPoint();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start and record the result
            bool bStatus = xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);

            // Write a data point to the file using the custom mock
            xmlWriter.WriteDataPoint(mockDataPoint);

            // Write the session end (no need to record the return from session end as it is tested separately)
            xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(xmlWriter.FilePath));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(xmlWriter.FilePath);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteSessionStart() method returns false if the writer is not valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionStart_InvalidWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Create the object under test and set the file path to an invalid value
            RNGXMLWriter xmlWriter = new RNGXMLWriter(sEMPTY_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start and record the result
            bool bStatus = xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests WriteSessionStart() method returns false if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionStart_NullWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start and record the result
            bool bStatus = xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests WriteDataPoint() method returns true if the data point write is successful
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_Success_ReturnsTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH, m_writerSettings);

            // Mock the data point class
            Mock<IXMLDataPoint> mockDataPoint = new Mock<IXMLDataPoint>();
            mockDataPoint.Setup(mock => mock.WriteDataPoint(It.IsAny<XmlWriter>())).Returns(true);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            // This will call the mock object's WriteDataPoint method so no need to verify the file contents here
            // or to write the session start and end.
            bool bStatus = xmlWriter.WriteDataPoint(mockDataPoint.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // File contents verified by XMLDataPointTests
        }

        /// <summary>
        /// Tests WriteDataPoint() method returns false if the data point fails to write
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_Failure_ReturnsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH, m_writerSettings);

            // Mock the data point class
            Mock<IXMLDataPoint> mockDataPoint = new Mock<IXMLDataPoint>();
            mockDataPoint.Setup(mock => mock.WriteDataPoint(It.IsAny<XmlWriter>())).Returns(false);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = xmlWriter.WriteDataPoint(mockDataPoint.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests WriteDataPoint() method returns false if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_NullWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            // Mock the data point class
            Mock<IXMLDataPoint> mockDataPoint = new Mock<IXMLDataPoint>();
            mockDataPoint.Setup(mock => mock.WriteDataPoint(It.IsAny<XmlWriter>())).Returns(true);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = xmlWriter.WriteDataPoint(mockDataPoint.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests WriteDataPoint() method returns false if the data point is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_NullDataPoint_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH, m_writerSettings);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = xmlWriter.WriteDataPoint(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests WriteSessionEnd() method works correctly when data points have been written
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionEnd_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_SESSION_END_ENTRY;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH, m_writerSettings);

            // Create the custom mock of the data point class
            MockXMLDataPoint mockDataPoint = new MockXMLDataPoint();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start (no need to record the return from session start as it is tested separately)
            xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);

            // Write a data point to the file using the custom mock
            xmlWriter.WriteDataPoint(mockDataPoint);

            // Write the session end and record the result
            bool bStatus = xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(xmlWriter.FilePath));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(xmlWriter.FilePath);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteSessionEnd() method works correctly when no data points have been written
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionEnd_NoData_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_SESSION_END_ENTRY_NO_DATA;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH, m_writerSettings);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start and end (no need to record the return from session start as it is tested separately)
            xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);
            bool bStatus = xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(xmlWriter.FilePath));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(xmlWriter.FilePath);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteSessionEnd() method returns false if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionEnd_NullWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session end and record the result
            bool bStatus = xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was not successful
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Test the FilePath property works correctly
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FilePath_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = m_sTEST_FILE_PATH;

            // Create the object under test
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the file path
            xmlWriter.FilePath = m_sTEST_FILE_PATH;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_RESULT, xmlWriter.FilePath);
        }

        #endregion
    }
}
