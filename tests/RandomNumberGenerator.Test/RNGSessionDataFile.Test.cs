//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.Test.cs
// Description:    Unit tests for the RNGSessionDataFile class
//
// Copyright (c) 2025 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
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

        // Owned by the tests that record two sessions into one file, so they never share a file with a test
        // that only mocks the writer and leaves nothing on disk
        private const string m_sAPPEND_FILE_PATH = "TestAppend.RNGSessionDataFileTests.xml";

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

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// Tests the constructor generates an exception when the writer is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_NullWriter_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception
        }

        /// <summary>
        /// Tests StartSession() method works correctly when the writer is valid
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_ValidWriter_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>())).Returns(true);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            // Mock the iRNGSessionData interface
            var sessionDataMock = new Mock<IRNGSessionData>();
            sessionDataMock.Setup(mock => mock.Simulated).Returns(false);
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
        /// Tests StartSession() method throws InvalidOperationException when the writer is invalid
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Expected exception when no file path is set</exception>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void StartSession_InvalidWriter_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid file name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(sEMPTY_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            // Mock the IRNGSessionData interface
            var sessionDataMock = new Mock<IRNGSessionData>();
            sessionDataMock.Setup(mock => mock.Simulated).Returns(false);
            sessionDataMock.Setup(mock => mock.TargetValue).Returns(0);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the session - should throw InvalidOperationException
            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests StartSession() generates an exception when the session data is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void StartSession_NullSessionData_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the session
            bool bStatus = sessionDataFile.StartSession(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception
        }

        /// <summary>
        /// Tests WriteDataPoint() method generates an error when the writer is valid but no session was started
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ValidWriterNoSession_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WriteDataPoint_ValidWriterSession_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>())).Returns(true);
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
        /// Tests WriteDataPoint() generates an exception when the data point is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void WriteDataPoint_NullDataPoint_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Write the data point
            bool bStatus = sessionDataFile.WriteDataPoint(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception
        }

        /// <summary>
        /// Tests EndSession() method works correctly when the writer is valid
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_ValidWriter_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_InvalidWriter_Failure()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FileProperty_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected and actual valies for the file path
            const string sEXPECTED_FILE_PATH = "Expected Path";

            // Mock the IRNGSessionFileWriter interface to set the file path
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Valid_ValidFilePath_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Valid_InvalidFilePath_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Use an invalid fiel name for the path (empty string)
            const string sEMPTY_PATH = "";

            // Mock the IRNGSessionFileWriter interface
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
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

        /// <summary>
        /// Tests a session start that reports failure does not leave a session recorded as in progress
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_WriteSessionStartFails_SessionNotInProgress()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock a writer that reports the session start failed without throwing
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>())).Returns(false);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bStatus = sessionDataFile.StartSession(new Mock<IRNGSessionData>().Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the failure is reported and no session is left recorded against the file
            Assert.IsFalse(bStatus);
            Assert.IsFalse(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests a file that cannot be prepared for appending is reported rather than being treated as loaded
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadSession_PrepareForAppendFails_ThrowsAndNoSessionInProgress()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock a writer that reports it cannot prepare the file for appending
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.SetupProperty(mock => mock.FilePath, m_sTEST_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.PrepareForAppend(It.IsAny<string>())).Returns(false);

            // Mock a reader that loads the file successfully
            var xmlReaderMock = new Mock<IRNGSessionFileReader>();
            xmlReaderMock.SetupProperty(mock => mock.FilePath);
            xmlReaderMock.Setup(mock => mock.LoadFile(It.IsAny<IRNGSessionData>(), It.IsAny<uint>())).Returns(true);

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object, xmlReaderMock.Object);

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Verify the failure to prepare the file is reported rather than passing silently
            Assert.ThrowsException<InvalidOperationException>(() =>
                sessionDataFile.LoadSession(new Mock<IRNGSessionData>().Object, m_sTEST_FILE_PATH));

            // Verify no session is left recorded against a file that was not prepared
            Assert.IsFalse(sessionDataFile.SessionInProgress);
        }

        /// <summary>
        /// Tests a second session recorded into a file that already holds readings keeps them, rather than
        /// writing over the readings already there
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_FileAlreadyHoldsReadings_KeepsThemAndAppends()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iEXPECTED_READINGS = 2;
            const bool bNOT_SIMULATED = false;
            const int iTARGET = 1;

            // Start with nothing there, so the first session is the one that creates the file
            if (File.Exists(m_sAPPEND_FILE_PATH))
            {
                File.Delete(m_sAPPEND_FILE_PATH);
            }

            // Mock the session the file is recorded against
            var sessionDataMock = new Mock<IRNGSessionData>();
            sessionDataMock.Setup(mock => mock.Simulated).Returns(bNOT_SIMULATED);
            sessionDataMock.Setup(mock => mock.TargetValue).Returns(iTARGET);

            // Record one reading and stop, which is what pressing Start and then Stop does
            RNGSessionDataFile firstSession = new RNGSessionDataFile(new RNGXMLWriter(m_sAPPEND_FILE_PATH));
            firstSession.StartSession(sessionDataMock.Object);
            firstSession.WriteDataPoint(new XMLDataPoint("00:00:01", 0.111));
            firstSession.EndSession();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Press Start again with the same file still chosen, which is what keeping the file after a
            // session ends now allows
            RNGSessionDataFile secondSession = new RNGSessionDataFile(new RNGXMLWriter(m_sAPPEND_FILE_PATH));
            secondSession.StartSession(sessionDataMock.Object);
            secondSession.WriteDataPoint(new XMLDataPoint("00:00:02", 0.222));
            secondSession.EndSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the file still reads as a session, so the second start left it well formed
            XmlDocument document = new XmlDocument();
            document.Load(m_sAPPEND_FILE_PATH);
            Assert.AreEqual(XMLConstants.SESSION_ELEMENT, document.DocumentElement.Name);

            // Verify both sessions' readings are there, the first one having survived the second start
            Assert.AreEqual(iEXPECTED_READINGS, document.DocumentElement.ChildNodes.Count);
            Assert.AreEqual("0.111", document.DocumentElement.ChildNodes[0].InnerText);
            Assert.AreEqual("0.222", document.DocumentElement.ChildNodes[1].InnerText);

            // Clean up the file this test owns
            File.Delete(m_sAPPEND_FILE_PATH);
        }

        /// <summary>
        /// Tests starting a session on a file that already holds readings reopens it for appending instead
        /// of writing a session start, which is the write that empties the file
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_FileAlreadyHoldsReadings_PreparesForAppendInsteadOfWritingSessionStart()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const bool bPREPARED = true;

            // Put something in the file, as it is the file having content that decides which way this goes
            File.WriteAllText(m_sAPPEND_FILE_PATH, "<Session Simulated=\"false\" Target=\"1\">");

            // Mock a writer that reports the file was prepared successfully
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sAPPEND_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.PrepareForAppend(It.IsAny<string>())).Returns(bPREPARED);

            // Mock the session the file is recorded against
            var sessionDataMock = new Mock<IRNGSessionData>();

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session started and is recorded as being in progress
            Assert.IsTrue(bStatus);
            Assert.IsTrue(sessionDataFile.SessionInProgress);

            // Verify the file was reopened after what it already held, rather than started over
            xmlWriterMock.Verify(mock => mock.PrepareForAppend(m_sAPPEND_FILE_PATH), Times.Once);
            xmlWriterMock.Verify(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);

            // Clean up the file this test owns
            File.Delete(m_sAPPEND_FILE_PATH);
        }

        /// <summary>
        /// Tests a file that exists but has nothing in it is started rather than continued. An empty file is
        /// what a session that never got as far as writing anything leaves behind, and there is no session
        /// element in it to append to.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_FileExistsButIsEmpty_StartsTheSessionRatherThanAppending()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const bool bWRITTEN = true;

            // A file that is there with nothing in it
            File.WriteAllText(m_sAPPEND_FILE_PATH, string.Empty);

            // Mock a writer that reports the session start was written
            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sAPPEND_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>())).Returns(bWRITTEN);

            var sessionDataMock = new Mock<IRNGSessionData>();

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session was begun rather than continued. There is nothing in the file to continue,
            // and preparing an empty one for appending would leave it with data and no session around it.
            Assert.IsTrue(bStatus);
            xmlWriterMock.Verify(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            xmlWriterMock.Verify(mock => mock.PrepareForAppend(It.IsAny<string>()), Times.Never);

            // Clean up the file this test owns
            File.Delete(m_sAPPEND_FILE_PATH);
        }

        /// <summary>
        /// Tests a file holding a session that recorded nothing is still continued rather than written over.
        /// The check is whether the file has anything in it, not whether it has readings, and a session that
        /// was started and stopped without recording leaves an element that has to be appended to.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_FileHoldsASessionWithNoReadings_ContinuesItRatherThanWritingOverIt()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const bool bPREPARED = true;

            // A session that closed itself without recording anything
            File.WriteAllText(m_sAPPEND_FILE_PATH,
                              "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Session Simulated=\"true\" Target=\"-1\" />");

            var xmlWriterMock = new Mock<IRNGSessionFileWriter>();
            xmlWriterMock.Setup(mock => mock.FilePath).Returns(m_sAPPEND_FILE_PATH);
            xmlWriterMock.Setup(mock => mock.PrepareForAppend(It.IsAny<string>())).Returns(bPREPARED);

            var sessionDataMock = new Mock<IRNGSessionData>();

            // Create the object under test
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(xmlWriterMock.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bStatus = sessionDataFile.StartSession(sessionDataMock.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session element already in the file is reopened rather than a second one written
            Assert.IsTrue(bStatus);
            xmlWriterMock.Verify(mock => mock.PrepareForAppend(m_sAPPEND_FILE_PATH), Times.Once);
            xmlWriterMock.Verify(mock => mock.WriteSessionStart(It.IsAny<bool>(), It.IsAny<int>()), Times.Never);

            // Clean up the file this test owns
            File.Delete(m_sAPPEND_FILE_PATH);
        }

        #endregion
    }
}
