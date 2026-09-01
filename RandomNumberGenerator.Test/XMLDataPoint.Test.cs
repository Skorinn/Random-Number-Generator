//*********************************************************************************************************************
// File Name:      XMLDataPoint.Test.cs
// Description:    Unit tests for the XMLDataPoint class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/01/21 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the XMLDataPoint class
    /// </summary>
    [TestClass]
    public class XMLDataPointTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public XMLDataPointTests()
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
        private const string m_sTEST_FILE_PATH = "TestSessionDataFile.XMLDataPointTests.xml";

        // Session time to use for testing
        private const string m_sSESSION_DATA_TIME = "01:54:21";

        // Data point to use for testing
        private const double m_fDATA_POINT = 9.1;

        // Define the expected XML entries here as they depend on the time, target, and data values above
        private const string sEXPECTED_DATA_POINT_ENTRY = "<Data Time=\"01:54:21\">9.1</Data>";

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
        /// Cleans up after each test runs to ensure we always start with a clean file
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
            const string sEXPECTED_TIME = "";
            const double fEXPECTED_DATA = 0.0;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_TIME, xmlDataPoint.SessionTime);
            Assert.AreEqual(fEXPECTED_DATA, xmlDataPoint.DataPoint);
        }

        /// <summary>
        /// Tests the properties are set correctly using the initializing constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Initializing_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_TIME = m_sSESSION_DATA_TIME;
            const double fEXPECTED_DATA = m_fDATA_POINT;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint(sEXPECTED_TIME, fEXPECTED_DATA);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_TIME, xmlDataPoint.SessionTime);
            Assert.AreEqual(fEXPECTED_DATA, xmlDataPoint.DataPoint);
        }

        /// <summary>
        /// Tests WriteDataPoint with a valid writer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ValidWriter_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            string sEXPECTED_RESULT = sEXPECTED_DATA_POINT_ENTRY;

            // Create the XML writer
            XmlWriter xmlWriter = XmlWriter.Create(m_sTEST_FILE_PATH);

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint(m_sSESSION_DATA_TIME, m_fDATA_POINT);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point to the file
            bool bStatus = xmlDataPoint.WriteDataPoint(xmlWriter);

            // Close the file
            xmlWriter.Close();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the file was created
            Assert.IsTrue(File.Exists(m_sTEST_FILE_PATH));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(m_sTEST_FILE_PATH);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteDataPoint with a null writer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_NullWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint(m_sSESSION_DATA_TIME, m_fDATA_POINT);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point to the file
            bool bStatus = xmlDataPoint.WriteDataPoint(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write failed
            Assert.IsFalse(bStatus);

            // Verify the file was not created
            Assert.IsFalse(File.Exists(m_sTEST_FILE_PATH));
        }

        /// <summary>
        /// Tests WriteDataPoint with a writer that throws an exception
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ExceptionWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint(m_sSESSION_DATA_TIME, m_fDATA_POINT);

            // Create the XML writer
            XmlWriter xmlWriter = XmlWriter.Create(m_sTEST_FILE_PATH);

            // Close the file
            xmlWriter.Close();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point to the file
            bool bStatus = xmlDataPoint.WriteDataPoint(xmlWriter);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write failed
            Assert.IsFalse(bStatus);
        }

        /// <summary>
        /// Tests the SessionTime property
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void SessionTime_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_TIME = m_sSESSION_DATA_TIME;

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property
            xmlDataPoint.SessionTime = sEXPECTED_TIME;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_TIME, xmlDataPoint.SessionTime);
        }

        /// <summary>
        /// Tests the DataPoint property
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DataPoint_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const double fEXPECTED_DATA = m_fDATA_POINT;

            // Create the object under test
            XMLDataPoint xmlDataPoint = new XMLDataPoint();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property
            xmlDataPoint.DataPoint = fEXPECTED_DATA;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(fEXPECTED_DATA, xmlDataPoint.DataPoint);
        }

        #endregion
    }
}
