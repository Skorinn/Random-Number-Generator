//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.Test.cs
// Description:    Unit tests for the RNGSessionDataFile class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/01/20 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the RNGSessionDataFile class
    /// </summary>
    [TestClass]
    public class RNGSessionDataFileTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGSessionDataFileTests()
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
        private const string m_sTEST_FILE_PATH = "TestSessionDataFile.RNGSessionDataFileTests.xml";

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
        /// Cleans up after class runs
        /// </summary>
        [ClassCleanup]
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
        [TestCategory("Component")]
        public void Constructor_Default_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected default values
            const string sEXPECTED_FILE_PATH = m_sTEST_FILE_PATH;
            const bool bEXPECTED_VALID = true;
            const bool bEXPECTED_SESSION_IN_PROGRESS = false;

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(sEXPECTED_FILE_PATH);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_FILE_PATH, sessionDataFile.FilePath);
            Assert.AreEqual(bEXPECTED_VALID, sessionDataFile.IsValid());
            Assert.AreEqual(bEXPECTED_SESSION_IN_PROGRESS, sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests StartSession() method works correctly when the writer is valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_ValidWriter_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            // Mock the iRNGSessionData interface
            var sessionDataMock = new Mock<IRNGSessionData>();
            sessionDataMock.Setup(mock => mock.SessionTime).Returns("00:00:00");
            sessionDataMock.Setup(mock => mock.TargetValue).Returns(0);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the session
            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value and in progress flags
            Assert.IsTrue(bStatus);
            Assert.IsTrue(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests StartSession() method returns false when the writer is invalid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_InvalidWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(sEMPTY_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<string>(), It.IsAny<int>())).Returns(false);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            // Mock the iRNGSessionData interface
            var sessionDataMock = new Mock<IRNGSessionData>();
            sessionDataMock.Setup(mock => mock.SessionTime).Returns("00:00:00");
            sessionDataMock.Setup(mock => mock.TargetValue).Returns(0);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the session
            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value and in progress flags
            Assert.IsFalse(bStatus);
            Assert.IsFalse(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests WriteDataPoint() method generates an error when the writer is valid but no session was started
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ValidWriterNoSession_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable(); ;

            // Mock the data point interface
            var dataPointMock = new Mock<IXMLDataPoint>();

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = sessionDataFile.WriteDataPoint(dataPointMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value
            Assert.IsFalse(bStatus);

            // Verify no data point was written
            xmlWriterMock.Verify(mock => mock.WriteDataPoint(dataPointMock.Object), Times.Never);
        }

        /// <summary>
        /// Tests WriteDataPoint() method works correctly when the writer is valid and a session is in progress
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ValidWriterSession_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable(); ;

            // Mock the data point interface
            var dataPointMock = new Mock<IXMLDataPoint>();

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            // Start a session
            sessionDataFile.StartSession(new Mock<IRNGSessionData>().Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = sessionDataFile.WriteDataPoint(dataPointMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value
            Assert.IsTrue(bStatus);

            // Verify no data point was written
            xmlWriterMock.Verify(mock => mock.WriteDataPoint(dataPointMock.Object), Times.Once);
        }

        /// <summary>
        /// Tests EndSession() method works correctly when the writer is valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_ValidWriter_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionEnd()).Returns(true);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // End the session
            bool bStatus = sessionDataFile.EndSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value and in progress flags
            Assert.IsTrue(bStatus);
            Assert.IsFalse(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests EndSession() method returns false when the writer is invalid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_InvalidWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(sEMPTY_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionEnd()).Returns(false);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // End the session
            bool bStatus = sessionDataFile.EndSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the return value and in progress flags
            Assert.IsFalse(bStatus);
            Assert.IsFalse(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests the FileProperty property works correctly
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FileProperty_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected and actual valies for the file path
            const string sEXPECTED_FILE_PATH = "Expected Path";

            // Mock the IRNGXMLWriter interface to set the file path
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.SetupProperty(mock => mock.FilePath, m_sTEST_FILE_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the file property
            sessionDataFile.FilePath = sEXPECTED_FILE_PATH;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the property was set correctly
            Assert.AreEqual(sEXPECTED_FILE_PATH, sessionDataFile.FilePath);
        }

        /// <summary>
        /// Tests the valid property when the file path is valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Valid_ValidFilePath_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the file path to a valid value
            sessionDataFile.FilePath = m_sTEST_FILE_PATH;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the property was set correctly
            Assert.IsTrue(sessionDataFile.IsValid());
        }

        /// <summary>
        /// Tests the valid property when the file path is not valid
        /// <\summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Valid_InvalidFilePath_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGXMLWriter interface
            var xmlWriterMock = new Mock<IRNGXMLWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(sEMPTY_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the file path to an invalid value
            sessionDataFile.FilePath = sEMPTY_PATH;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the the property was set correctly
            Assert.IsFalse(sessionDataFile.IsValid());
        }

        #endregion
    }
}
