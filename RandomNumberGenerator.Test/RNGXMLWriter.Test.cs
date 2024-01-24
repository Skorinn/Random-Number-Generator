//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.Test.cs
// Description:    Unit tests for the RNGXMLWriter class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 01/21/2024 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Xml;

namespace RandomNumberGenerator.Test
{
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
            // Nothing to do
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

        // Paths for files used for testing
        private const string m_sTEST_FILE_PATH = "TestSessionDataFile.xml";

        // Session time to use for testing
        private const string m_sSESSION_START_TIME = "01:00:00";
        private const string m_sSESSION_DATA_TIME = "01:54:21";
        private const string m_sSESSION_END_TIME = "22:11:03";

        // Target value to use for testing
        private const int m_iTARGET_VALUE = 1;

        // Data point to use for testing
        private const double m_fDATA_POINT = 9.1;

        // Define the expected XML entries here as they depend on the time, target, and data values above
        private const string sEXPECTED_SESSION_START_ENTRY = "<Session StartTime=\"01:00:00\" TargetValue=\"1\">";
        private const string sEXPECTED_DATA_POINT_ENTRY = "<DataPoint SessionTime=\"01:54:21\" Value=\"9.1\" />";
        private const string sEXPECTED_SESSION_END_ENTRY = "<Session EndTime=\"22:11:03\" />";

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
        [DeploymentItem(m_sTEST_FILE_PATH)]
        public static void Cleanup()
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
        /// Tests WriteSessionStart() method works correctly
        /// <\summary>
        [TestMethod]
        [DeploymentItem(m_sTEST_FILE_PATH)]
        public void WriteSessionStart_Valid()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_SESSION_START_ENTRY;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session start
            bool bStatus = xmlWriter.WriteSessionStart(m_sSESSION_START_TIME, m_iTARGET_VALUE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(xmlWriter.FilePath));

            // Verify the file contains the expected result
            Assert.IsTrue(File.ReadAllText(xmlWriter.FilePath).Contains(sEXPECTED_RESULT));
        }

        /// <summary>
        /// Tests WriteDataPoint() method works correctly
        /// <\summary>
        [TestMethod]
        [DeploymentItem(m_sTEST_FILE_PATH)]
        public void WriteDataPoint_Valid()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_DATA_POINT_ENTRY;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);

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

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // File contents verified by XMLDataPointTests
        }

        /// <summary>
        /// Tests WriteSessionEnd() method works correctly
        /// <\summary>
        [TestMethod]
        [DeploymentItem(m_sTEST_FILE_PATH)]
        public void WriteSessionEnd_Valid()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_SESSION_END_ENTRY;

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the session end
            bool bStatus = xmlWriter.WriteSessionEnd(m_sSESSION_END_TIME);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(xmlWriter.FilePath));

            // Verify the file contains the expected result
            Assert.IsTrue(File.ReadAllText(xmlWriter.FilePath).Contains(sEXPECTED_RESULT));
        }

        /// <summary>
        /// Test the FilePath property works correctly
        /// <\summary>
        [TestMethod]
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
