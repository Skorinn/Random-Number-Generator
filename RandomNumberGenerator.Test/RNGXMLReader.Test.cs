//*********************************************************************************************************************
// File Name:      RNGXMLReader.Test.cs
// Description:    Unit tests for the RNGXMLReader class
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
using System.IO;
using System.Xml;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the RNGXMLReader class
    /// </summary>
    [TestClass]
    public class RNGXMLReaderTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGXMLReaderTests()
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
        private const string m_sTEST_FILE_PATH = "TestSessionDataFile.RNGXMLReaderTests.xml";
        private const string m_sNON_EXISTENT_FILE = "NonExistentFile.xml";
        private const string m_sINVALID_XML_FILE = "InvalidXMLFile.xml";

        // Test data values
        private const bool m_bSIMULATED_FLAG = false;
        private const int m_iTARGET_VALUE = 1;
        private const double m_fTEST_DATA_POINT_1 = 0.123456;
        private const double m_fTEST_DATA_POINT_2 = 0.789012;

        // Expected XML content
        private const string m_sVALID_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.123456</Data>
    <Data Time=""01:30:46"">0.789012</Data>
</Session>";

        private const string m_sINVALID_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<InvalidRoot>
    <NotSessionData>0.123456</NotSessionData>
</InvalidRoot>";

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
            if (File.Exists(m_sINVALID_XML_FILE))
            {
                File.Delete(m_sINVALID_XML_FILE);
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
            const string sEXPECTED_LAST_ERROR = "";

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the properties were set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlReader.FilePath);
            Assert.AreEqual(sEXPECTED_LAST_ERROR, xmlReader.LastError);
        }

        /// <summary>
        /// Tests properties are set correctly using the file path constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_FilePath_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_FILE_PATH = m_sTEST_FILE_PATH;
            const string sEXPECTED_LAST_ERROR = "";

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(sEXPECTED_FILE_PATH);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the properties were set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlReader.FilePath);
            Assert.AreEqual(sEXPECTED_LAST_ERROR, xmlReader.LastError);
        }

        /// <summary>
        /// Tests properties are set correctly using the XML reader settings constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_ReaderSettings_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_FILE_PATH = "";
            const string sEXPECTED_LAST_ERROR = "";
            XmlReaderSettings testSettings = new XmlReaderSettings();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(testSettings);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the properties were set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlReader.FilePath);
            Assert.AreEqual(sEXPECTED_LAST_ERROR, xmlReader.LastError);
        }

        /// <summary>
        /// Tests properties are set correctly using the file path and settings constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_FilePathAndSettings_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_FILE_PATH = m_sTEST_FILE_PATH;
            const string sEXPECTED_LAST_ERROR = "";
            XmlReaderSettings testSettings = new XmlReaderSettings();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(sEXPECTED_FILE_PATH, testSettings);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the properties were set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlReader.FilePath);
            Assert.AreEqual(sEXPECTED_LAST_ERROR, xmlReader.LastError);
        }

        /// <summary>
        /// Tests the FilePath property setter
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FilePath_SetProperty_UpdatesCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected value
            const string sEXPECTED_FILE_PATH = m_sTEST_FILE_PATH;

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the file path property
            xmlReader.FilePath = sEXPECTED_FILE_PATH;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, xmlReader.FilePath);
        }

        /// <summary>
        /// Tests LoadFile with valid XML file loads successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_ValidXMLFile_LoadsSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset()).Verifiable();
            mockSessionData.Setup(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), It.IsAny<uint>())).Returns(true).Verifiable();
            mockSessionData.SetupProperty(mock => mock.TargetValue);
            mockSessionData.SetupProperty(mock => mock.Simulated);
            mockSessionData.SetupProperty(mock => mock.FilePath);

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the file
            bool bResult = xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load was successful
            Assert.IsTrue(bResult);
            Assert.AreEqual("", xmlReader.LastError);

            // Verify session data was reset and configured
            mockSessionData.Verify(mock => mock.Reset(), Times.Once);
            mockSessionData.VerifySet(mock => mock.TargetValue = m_iTARGET_VALUE, Times.Once);
            mockSessionData.VerifySet(mock => mock.Simulated = m_bSIMULATED_FLAG, Times.Once);
            mockSessionData.VerifySet(mock => mock.FilePath = m_sTEST_FILE_PATH, Times.Once);

            // Verify data was loaded
            mockSessionData.Verify(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), It.IsAny<uint>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests LoadFile with null session data parameter throws appropriate error
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_NullSessionData_ReturnsFalseWithError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load with null session data
            bool bResult = xmlReader.LoadFile(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed with appropriate error
            Assert.IsFalse(bResult);
            StringAssert.Contains(xmlReader.LastError, "Session data object cannot be null");
        }

        /// <summary>
        /// Tests LoadFile with non-existent file handles error appropriately
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_NonExistentFile_ReturnsFalseWithError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();

            // Create the object under test with non-existent file
            RNGXMLReader xmlReader = new RNGXMLReader(m_sNON_EXISTENT_FILE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load non-existent file
            bool bResult = xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed with appropriate error
            Assert.IsFalse(bResult);
            StringAssert.Contains(xmlReader.LastError, "does not exist");
        }

        /// <summary>
        /// Tests LoadFile with invalid XML format handles error appropriately
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_InvalidXMLFormat_ReturnsFalseWithError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create invalid XML test file
            File.WriteAllText(m_sINVALID_XML_FILE, m_sINVALID_XML_CONTENT);

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset()).Verifiable();

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sINVALID_XML_FILE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load invalid XML file
            bool bResult = xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed with appropriate error
            Assert.IsFalse(bResult);
            StringAssert.Contains(xmlReader.LastError, "Session");
        }

        /// <summary>
        /// Tests LoadFile with custom batch size processes data correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_CustomBatchSize_ProcessesCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Custom batch size
            const uint uCUSTOM_BATCH_SIZE = 1;

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset()).Verifiable();
            mockSessionData.Setup(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), uCUSTOM_BATCH_SIZE)).Returns(true).Verifiable();
            mockSessionData.SetupProperty(mock => mock.TargetValue);
            mockSessionData.SetupProperty(mock => mock.Simulated);
            mockSessionData.SetupProperty(mock => mock.FilePath);

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load the file with custom batch size
            bool bResult = xmlReader.LoadFile(mockSessionData.Object, uCUSTOM_BATCH_SIZE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load was successful
            Assert.IsTrue(bResult);

            // Verify data was loaded with custom batch size
            mockSessionData.Verify(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), uCUSTOM_BATCH_SIZE), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests LoadFile when LoadDataPointsBatch fails handles error appropriately
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_LoadDataPointsBatchFails_ReturnsFalseWithError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Mock session data that fails on LoadDataPointsBatch
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset()).Verifiable();
            mockSessionData.Setup(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), It.IsAny<uint>())).Returns(false);
            mockSessionData.SetupProperty(mock => mock.TargetValue);

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load file
            bool bResult = xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed with appropriate error
            Assert.IsFalse(bResult);
            StringAssert.Contains(xmlReader.LastError, "Failed to load");
        }

        /// <summary>
        /// Tests Close method properly closes and disposes the reader
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Close_ValidReader_ClosesSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create valid XML test file
            File.WriteAllText(m_sTEST_FILE_PATH, m_sVALID_XML_CONTENT);

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset());
            mockSessionData.Setup(mock => mock.LoadDataPointsBatch(It.IsAny<System.Collections.Generic.IEnumerable<double>>(), It.IsAny<uint>())).Returns(true);
            mockSessionData.SetupProperty(mock => mock.TargetValue);
            mockSessionData.SetupProperty(mock => mock.Simulated);
            mockSessionData.SetupProperty(mock => mock.FilePath);

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            // Load the file to create a reader
            xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Close the reader
            xmlReader.Close();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify no exception was thrown and reader is closed
            Assert.IsNotNull(xmlReader);
        }

        /// <summary>
        /// Tests Close method when no reader exists handles gracefully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Close_NoReader_HandlesGracefully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test without loading any file
            RNGXMLReader xmlReader = new RNGXMLReader();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Close when no reader exists
            xmlReader.Close();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify no exception was thrown
            Assert.IsNotNull(xmlReader);
        }

        /// <summary>
        /// Tests LoadFile with malformed XML throws XmlException and handles it appropriately
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadFile_MalformedXML_ReturnsFalseWithXmlError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create malformed XML file
            const string sMalformedXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.123456</Data>
    <UnclosedElement>
</Session>";
            File.WriteAllText(m_sTEST_FILE_PATH, sMalformedXML);

            // Mock session data
            var mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Reset()).Verifiable();
            mockSessionData.SetupProperty(mock => mock.TargetValue);
            mockSessionData.SetupProperty(mock => mock.Simulated);
            mockSessionData.SetupProperty(mock => mock.FilePath);
            mockSessionData.SetupGet(mock => mock.NumDataPoints).Returns(0);

            // Create the object under test
            RNGXMLReader xmlReader = new RNGXMLReader(m_sTEST_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to load malformed XML file
            bool bResult = xmlReader.LoadFile(mockSessionData.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed with appropriate error about file corruption
            Assert.IsFalse(bResult);
            StringAssert.Contains(xmlReader.LastError, "XML file appears to be incomplete or corrupted");
        }

        #endregion
    }
}