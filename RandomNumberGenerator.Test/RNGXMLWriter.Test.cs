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
using System;
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

        // Simulated flag to use for testing
        private const bool m_bSIMULATED_FLAG = false;

        // Target value to use for testing
        private const int m_iTARGET_VALUE = 1;

        // Define the expected XML entries here as they depend on the simulated flag, target, and data values above
        private const string sEXPECTED_SESSION_START_ENTRY = "<Session Simulated=\"false\" Target=\"1\">";
        private const string sEXPECTED_SESSION_END_ENTRY = "</Session>";
        private const string sEXPECTED_SESSION_END_ENTRY_NO_DATA = "<Session Simulated=\"false\" Target=\"1\" />";

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
            bool bStatus = xmlWriter.WriteSessionStart(m_bSIMULATED_FLAG, m_iTARGET_VALUE);

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
            Assert.IsTrue(File.Exists(m_sTEST_FILE_PATH));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(m_sTEST_FILE_PATH);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteSessionStart() method generates exception if the writer is not valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]

        public void WriteSessionStart_InvalidWriter_Exception()
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
            bool bStatus = xmlWriter.WriteSessionStart(m_bSIMULATED_FLAG, m_iTARGET_VALUE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected, so no need to verify the status
        }

        /// <summary>
        /// Tests WriteSessionStart() method generates exception if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]

        public void WriteSessionStart_NullWriter_Exception()
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
            bool bStatus = xmlWriter.WriteSessionStart(m_bSIMULATED_FLAG, m_iTARGET_VALUE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected, so no need to verify the status
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
        /// Tests WriteDataPoint() method generates exception if the data point fails to write
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]

        public void WriteDataPoint_Failure_Exception()
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

            // Exception expected, so no need to verify the status
        }

        /// <summary>
        /// Tests WriteDataPoint() method generatees exception if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]

        public void WriteDataPoint_NullWriter_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test and set the file path
            RNGXMLWriter xmlWriter = new RNGXMLWriter();

            // Mock the data point class
            Mock<IXMLDataPoint> mockDataPoint = new Mock<IXMLDataPoint>();
            mockDataPoint.Setup(mock => mock.WriteDataPoint(It.IsNotNull<XmlWriter>())).Returns(true);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = xmlWriter.WriteDataPoint(mockDataPoint.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Exception expected, so no need to verify the status
        }

        /// <summary>
        /// Tests WriteDataPoint() method generates exception if the data point is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]

        public void WriteDataPoint_NullDataPoint_Exception()
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

            // Exception expected, so no need to verify the status
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
            xmlWriter.WriteSessionStart(m_bSIMULATED_FLAG, m_iTARGET_VALUE);

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
            Assert.IsTrue(File.Exists(m_sTEST_FILE_PATH));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(m_sTEST_FILE_PATH);
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
            bool bStatus = xmlWriter.WriteSessionStart(m_bSIMULATED_FLAG, m_iTARGET_VALUE);
            bStatus = xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the write was successful
            Assert.IsTrue(bStatus);

            // Verify the session file was cleared
            Assert.IsTrue(string.IsNullOrEmpty(xmlWriter.FilePath));

            // Verify the file was created
            Assert.IsTrue(File.Exists(m_sTEST_FILE_PATH));

            // Verify the file contains the expected result
            string sResult = File.ReadAllText(m_sTEST_FILE_PATH);
            StringAssert.Contains(sResult, sEXPECTED_RESULT);
        }

        /// <summary>
        /// Tests WriteSessionEnd() method generates exception if the writer is NULL
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(InvalidOperationException))]

        public void WriteSessionEnd_NullWriter_Exception()
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

            // Exception expected, so no need to verify the status
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

        /// <summary>
        /// Tests appending to a session file that was never closed, as left by the application being stopped
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void PrepareForAppend_UnterminatedSession_ContinuesTheSession()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // A session file with no closing tag, holding one complete data point
            const string sUNTERMINATED_XML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                             "<Session Simulated=\"false\" Target=\"1\">\r\n" +
                                             "\t<Data Time=\"01:30:45\">0.111</Data>";
            File.WriteAllText(m_sTEST_FILE_PATH, sUNTERMINATED_XML);

            RNGXMLWriter xmlWriter = new RNGXMLWriter();
            XMLDataPoint dataPoint = new XMLDataPoint("01:30:46", 0.222);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bPrepared = xmlWriter.PrepareForAppend(m_sTEST_FILE_PATH);
            bool bWritten = xmlWriter.WriteDataPoint(dataPoint);
            bool bEnded = xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(bPrepared);
            Assert.IsTrue(bWritten);
            Assert.IsTrue(bEnded);

            // Verify the result is a complete file holding both the original and the appended data
            string sFileContent = File.ReadAllText(m_sTEST_FILE_PATH);
            XmlDocument document = new XmlDocument();
            document.LoadXml(sFileContent);
            Assert.AreEqual(XMLConstants.SESSION_ELEMENT, document.DocumentElement.Name);
            Assert.AreEqual(2, document.DocumentElement.ChildNodes.Count);
        }

        /// <summary>
        /// Tests appending to a closed session does not leave a gap where the closing tag was
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void PrepareForAppend_ClosedSession_AppendedDataMatchesLayout()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Write a session the same way the application does, then close it
            RNGXMLWriter firstWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);
            firstWriter.WriteSessionStart(false, 1);
            firstWriter.WriteDataPoint(new XMLDataPoint("01:30:45", 0.111));
            firstWriter.WriteSessionEnd();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Reopen it and append to it the way loading an existing file does
            RNGXMLWriter appendWriter = new RNGXMLWriter();
            appendWriter.PrepareForAppend(m_sTEST_FILE_PATH);
            appendWriter.WriteDataPoint(new XMLDataPoint("01:30:46", 0.222));
            appendWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the appended data is laid out the same as data written in one session
            string sFileContent = File.ReadAllText(m_sTEST_FILE_PATH);
            StringAssert.Contains(sFileContent, "\t<Data Time=\"01:30:46\">0.222</Data>");
            Assert.IsFalse(sFileContent.Contains("\r\n\r\n"), "Appending should not leave a blank line in the file");

            // Verify the file is still valid and holds both points
            XmlDocument document = new XmlDocument();
            document.LoadXml(sFileContent);
            Assert.AreEqual(2, document.DocumentElement.ChildNodes.Count);
        }

        /// <summary>
        /// Tests a session with no target selected records that rather than the value used internally
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteSessionStart_NoTargetSet_RecordsNoTarget()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sEXPECTED_TARGET = "-1";
            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            xmlWriter.WriteSessionStart(false, TargetValues.NO_VALUE_SET);
            xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the recorded target is a real target value rather than the internal one
            XmlDocument document = new XmlDocument();
            document.Load(m_sTEST_FILE_PATH);
            string sRecordedTarget = document.DocumentElement.GetAttribute(XMLConstants.TARGET_ATTRIBUTE);
            Assert.AreEqual(sEXPECTED_TARGET, sRecordedTarget);
        }

        /// <summary>
        /// Tests the file can be read while a session is being written to it
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_SessionInProgress_FileCanBeRead()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            RNGXMLWriter xmlWriter = new RNGXMLWriter(m_sTEST_FILE_PATH);
            xmlWriter.WriteSessionStart(false, 1);
            xmlWriter.WriteDataPoint(new XMLDataPoint("01:30:45", 0.111));

            //**************************************************************//
            // Act
            //**************************************************************//

            // Read the file while the session is still open, as a backup or another tool would
            string sFileContent = string.Empty;
            using (FileStream readStream = new FileStream(m_sTEST_FILE_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(readStream))
                {
                    sFileContent = reader.ReadToEnd();
                }
            }

            xmlWriter.WriteSessionEnd();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the data recorded so far was readable while the session was in progress
            StringAssert.Contains(sFileContent, "0.111");
        }

        #endregion
    }
}
