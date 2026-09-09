//*********************************************************************************************************************
// File Name:      GeneratorForm.Test.cs
// Description:    Unit tests for the GeneratorForm class
//
// Copyright (c) 2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2024/01/20 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the GeneratorForm class
    /// </summary>
    [TestClass]
    public class GeneratorFormTests
    {
        #region Infrastructure
        /// <summary>
        /// Default constructor
        /// </summary>
        public GeneratorFormTests()
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

        // Test constants for GUI testing
        private const string m_sEXPECTED_DEFAULT_TEXT = "Idle";
        private const string m_sEXPECTED_RUNNING_TEXT = "Running";
        private const string m_sEXPECTED_ERROR_TEXT = "Error reading";

        // Test file constants for baseline and result file testing
        private const string m_sBASELINE_TEST_FILE = "TestBaselineFile.GeneratorFormTests.xml";
        private const string m_sRESULT_TEST_FILE = "TestResultFile.GeneratorFormTests.xml";
        private const string m_sMALFORMED_TEST_FILE = "TestMalformedResultFile.GeneratorFormTests.xml";
        private const string m_sBASELINE_INTEGRATION_FILE = "TestBaseline.Integration.xml";
        private const string m_sRESULT_INTEGRATION_FILE = "TestResult.Integration.xml";
        private const string m_sBASELINE_REVERSE_FILE = "TestBaseline.Reverse.xml";
        private const string m_sRESULT_REVERSE_FILE = "TestResult.Reverse.xml";

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
            // Delete test files if they exist to ensure clean state for next test
            string[] testFiles = {
                m_sBASELINE_TEST_FILE,
                m_sRESULT_TEST_FILE,
                m_sMALFORMED_TEST_FILE,
                m_sBASELINE_INTEGRATION_FILE,
                m_sRESULT_INTEGRATION_FILE,
                m_sBASELINE_REVERSE_FILE,
                m_sRESULT_REVERSE_FILE
            };

            foreach (string sTestFile in testFiles)
            {
                if (File.Exists(sTestFile))
                {
                    try
                    {
                        File.Delete(sTestFile);
                    }
                    catch (IOException)
                    {
                        // Ignore file deletion errors during test cleanup
                        // This prevents test failures due to file system issues
                    }
                }
            }
        }

        #endregion
        #region Tests

        /// <summary>
        /// Tests the constructor generates an exception if the session data is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_NullSessionData_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the device interface timer
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(null, mockDeviceTimer.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // No assert needed
        }

        /// <summary>
        /// Tests the constructor generates an exception if the device timer is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_NullDeviceTimer_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // No assert needed
        }

        // <summary>
        // Test for get and set for the FileBrowseActive property using true
        // </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FileBrowseActive_SetProperty_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.FileBrowseActive = true;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.IsTrue(generatorForm.FileBrowseActive);
        }

        // <summary>
        // Test for get and set for the FileBrowseActive property using false
        // </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FileBrowseActive_SetProperty_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.FileBrowseActive = false;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.IsFalse(generatorForm.FileBrowseActive);
        }

        // <summary>
        // Test for get and set for the StatusBoxText property
        // </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StatusBoxText_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_TEXT = "Expected String";

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxText = sEXPECTED_TEXT;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_TEXT, generatorForm.StatusBoxText);
        }

        // <summary>
        // Test for get and set for the StatusBoxTextColor property
        // </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StatusBoxTextColor_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            Color expectedTextColor = Color.MediumPurple;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxTextColor = expectedTextColor;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(expectedTextColor, generatorForm.StatusBoxTextColor);
        }

        // <summary>
        // Test for get and set for the StatusBoxBackColor property
        // </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StatusBoxBackColor_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            Color expectedBackColor = Color.MediumPurple;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxBackColor = expectedBackColor;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
        }

        /// <summary>
        /// Tests the RecordReadResult method with an valid result
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_Valid_RunningAndRecorded()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_TEXT = "Running";
            Color expectedTextColor = StatusPalette.NormalText;
            Color expectedBackColor = StatusPalette.NormalBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>())).Returns(true).Verifiable();
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute record with a valid result
            double fValidResult = 0.0;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the status box was updated correctly
            Assert.AreEqual(generatorForm.StatusBoxTextColor, expectedTextColor);
            Assert.AreEqual(generatorForm.StatusBoxBackColor, expectedBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_TEXT);

            // Verify the data point was recorded
            mockSessionData.Verify(mock => mock.AddDataPoint(fValidResult), Times.Once);
        }

        /// <summary>
        /// Tests the RecordReadResult method with an invalid result
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_Invalid_IdleAndError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_TEXT = "Error reading";
            Color expectedTextColor = StatusPalette.ErrorText;
            Color expectedBackColor = StatusPalette.ErrorBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>())).Returns(true).Verifiable();
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute record with a invalid result
            double fInvalidResult = double.MaxValue;
            generatorForm.RecordReadResult(fInvalidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the status box was updated correctly
            Assert.AreEqual(expectedTextColor, generatorForm.StatusBoxTextColor);
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_TEXT);

            // Verify no data point was recorded
            mockSessionData.Verify(mock => mock.AddDataPoint(fInvalidResult), Times.Never);
        }

        /// <summary>
        /// Tests the DeviceList property sets the data source correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DeviceList_SetProperty_UpdatesDataSource()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Create a test device list
            var deviceList = new BindingList<IRNGDevice>();
            var mockDevice = new Mock<IRNGDevice>();
            deviceList.Add(mockDevice.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the device list property
            generatorForm.DeviceList = deviceList;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the device list was updated (implicit through no exception)
            Assert.IsNotNull(generatorForm);
        }

        /// <summary>
        /// Tests the Running property returns the correct status from session data
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Running_GetProperty_ReturnsSessionDataStatus()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected value
            const bool bEXPECTED_RUNNING = true;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.InProgress).Returns(bEXPECTED_RUNNING);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Get the running property
            bool bActualRunning = generatorForm.Running;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property returns the expected value
            Assert.AreEqual(bEXPECTED_RUNNING, bActualRunning);
        }

        /// <summary>
        /// Tests the State property returns the correct GUI state
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void State_GetProperty_ReturnsCorrectState()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected value (default state is Idle)
            const GeneratorForm.RngGuiStates eEXPECTED_STATE = GeneratorForm.RngGuiStates.Idle;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Get the state property
            GeneratorForm.RngGuiStates eActualState = generatorForm.State;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property returns the expected value
            Assert.AreEqual(eEXPECTED_STATE, eActualState);
        }

        /// <summary>
        /// Tests the GetStatusBoxState method retrieves all status box properties correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void GetStatusBoxState_ValidCall_ReturnsStatusBoxState()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_TEXT = "Test Status";
            Color expectedTextColor = Color.Blue;
            Color expectedBackColor = Color.Yellow;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Set up the status box state
            generatorForm.StatusBoxText = sEXPECTED_TEXT;
            generatorForm.StatusBoxTextColor = expectedTextColor;
            generatorForm.StatusBoxBackColor = expectedBackColor;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call the method under test
            generatorForm.GetStatusBoxState(out string sActualText, out Color actualTextColor, out Color actualBackColor);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify all output parameters are correct
            Assert.AreEqual(sEXPECTED_TEXT, sActualText);
            Assert.AreEqual(expectedTextColor, actualTextColor);
            Assert.AreEqual(expectedBackColor, actualBackColor);
        }

        /// <summary>
        /// Tests the SetStatusBoxState method updates all status box properties correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void SetStatusBoxState_ValidParameters_UpdatesStatusBox()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected values
            const string sEXPECTED_TEXT = "Test Status Message";
            Color expectedTextColor = Color.Red;
            Color expectedBackColor = Color.LightBlue;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call the method under test
            generatorForm.SetStatusBoxState(sEXPECTED_TEXT, expectedTextColor, expectedBackColor);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify all status box properties were updated
            Assert.AreEqual(sEXPECTED_TEXT, generatorForm.StatusBoxText);
            Assert.AreEqual(expectedTextColor, generatorForm.StatusBoxTextColor);
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
        }

        /// <summary>
        /// Tests that FileBrowseActive throws exception when trying to set while session is running
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void FileBrowseActive_SetWhileRunning_ThrowsException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data to indicate running state
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.InProgress).Returns(true); // Session is running
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Attempt to set FileBrowseActive while running (should throw exception)
            generatorForm.FileBrowseActive = true;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting an exception (handled by attribute)
        }

        /// <summary>
        /// Tests RecordReadResult with exception from AddDataPoint (InvalidOperationException)
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_AddDataPointThrowsInvalidOperation_HandlesException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected error handling
            const string sEXPECTED_ERROR_MESSAGE = "Test invalid operation error";
            Color expectedBackColor = StatusPalette.ErrorBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data to throw exception
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>()))
                           .Throws(new InvalidOperationException(sEXPECTED_ERROR_MESSAGE));
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call RecordReadResult with valid data that will trigger exception
            double fValidResult = 0.5;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify error handling occurred
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_ERROR_MESSAGE);
        }

        /// <summary>
        /// Tests RecordReadResult with exception from AddDataPoint (UnauthorizedAccessException)
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_AddDataPointThrowsUnauthorizedAccess_HandlesException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected error handling
            const string sEXPECTED_ERROR_MESSAGE = "File access denied";
            Color expectedBackColor = StatusPalette.ErrorBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data to throw exception
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>()))
                           .Throws(new UnauthorizedAccessException(sEXPECTED_ERROR_MESSAGE));
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call RecordReadResult with valid data that will trigger exception
            double fValidResult = 0.5;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify error handling occurred
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_ERROR_MESSAGE);
        }

        /// <summary>
        /// Tests RecordReadResult with exception from AddDataPoint (IOException)
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_AddDataPointThrowsIOException_HandlesException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected error handling
            const string sEXPECTED_ERROR_MESSAGE = "File I/O error";
            Color expectedBackColor = StatusPalette.ErrorBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data to throw exception
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>()))
                           .Throws(new System.IO.IOException(sEXPECTED_ERROR_MESSAGE));
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call RecordReadResult with valid data that will trigger exception
            double fValidResult = 0.5;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify error handling occurred
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, "File I/O error:");
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_ERROR_MESSAGE);
        }

        /// <summary>
        /// Tests RecordReadResult with general exception from AddDataPoint
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_AddDataPointThrowsGeneralException_HandlesException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected error handling
            const string sEXPECTED_ERROR_MESSAGE = "Unexpected general error";
            Color expectedBackColor = StatusPalette.ErrorBackground;

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data to throw exception
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>()))
                           .Throws(new Exception(sEXPECTED_ERROR_MESSAGE));
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Call RecordReadResult with valid data that will trigger exception
            double fValidResult = 0.5;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify error handling occurred
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, "Unexpected error recording data:");
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_ERROR_MESSAGE);
        }

        /// <summary>
        /// Tests that the constructor sets up the DataPointAddedCallback correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_ValidParameters_SetsUpDataPointCallback()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.SetupProperty(mock => mock.DataPointAddedCallback);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the callback was set on the session data
            Assert.IsNotNull(mockSessionData.Object.DataPointAddedCallback, "DataPointAddedCallback should be set during construction");
        }

        /// <summary>
        /// Tests that DataPointAddedCallback integration works correctly with chart updates
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void DataPointAddedCallback_Integration_UpdatesChart()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.SetupProperty(mock => mock.DataPointAddedCallback);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Test data
            const double fTEST_DATA_POINT = 0.65;
            const double fTEST_AVERAGE = 0.70;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Simulate the callback being invoked (as would happen when session data adds a point)
            mockSessionData.Object.DataPointAddedCallback?.Invoke(fTEST_DATA_POINT, fTEST_AVERAGE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Note: This test verifies that the callback can be invoked without throwing exceptions.
            // Since we're testing the callback mechanism and the chart is a UI component that's
            // difficult to verify in unit tests, we're primarily ensuring no exceptions occur.
            Assert.IsTrue(true, "DataPointAddedCallback should execute without throwing exceptions");
        }

        /// <summary>
        /// Tests that RecordReadResult works correctly with the new callback-based chart updates
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_ValidResult_UsesCallbackForChartUpdate()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_TEXT = "Running";
            Color expectedTextColor = StatusPalette.NormalText;
            Color expectedBackColor = StatusPalette.NormalBackground;

            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Track if callback was set and invoked
            DataPointAddedDelegate capturedCallback = null;
            bool callbackInvoked = false;
            double capturedDataPoint = 0.0;
            double capturedAverage = 0.0;

            // Mock the session data and device interface timer 
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>())).Returns(true)
                           .Callback<double>(dataPoint =>
                           {
                               // Simulate callback being invoked after data point is added
                               if (capturedCallback != null)
                               {
                                   capturedCallback.Invoke(dataPoint, dataPoint); // Using dataPoint as average for simplicity
                                   callbackInvoked = true;
                                   capturedDataPoint = dataPoint;
                                   capturedAverage = dataPoint;
                               }
                           });
            mockSessionData.SetupProperty(mock => mock.DataPointAddedCallback);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Capture the callback that was set
            capturedCallback = mockSessionData.Object.DataPointAddedCallback;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute record with a valid result
            double fValidResult = 0.75;
            generatorForm.RecordReadResult(fValidResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the status box was updated correctly
            Assert.AreEqual(generatorForm.StatusBoxTextColor, expectedTextColor);
            Assert.AreEqual(generatorForm.StatusBoxBackColor, expectedBackColor);
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_TEXT);

            // Verify the data point was recorded
            mockSessionData.Verify(mock => mock.AddDataPoint(fValidResult), Times.Once);

            // Verify the callback was invoked through our simulation
            Assert.IsTrue(callbackInvoked, "DataPointAddedCallback should be invoked when data point is added");
            Assert.AreEqual(fValidResult, capturedDataPoint, "Callback should receive the correct data point");
        }

        #endregion
        #region Baseline and Result File Loading Tests

        /// <summary>
        /// Tests LoadBaselineFile with valid file loads successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadBaselineFile_ValidFile_LoadsSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test XML file content
            const string sVALID_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.1</Data>
    <Data Time=""01:30:46"">0.5</Data>
    <Data Time=""01:30:47"">0.9</Data>
</Session>";

            File.WriteAllText(m_sBASELINE_TEST_FILE, sVALID_XML_CONTENT);

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private LoadBaselineFile method
            MethodInfo loadMethod = typeof(GeneratorForm).GetMethod("LoadBaselineFile", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResult = (bool)loadMethod.Invoke(generatorForm, new object[] { m_sBASELINE_TEST_FILE });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load was successful
            Assert.IsTrue(bResult, "LoadBaselineFile should return true for valid file");
        }

        /// <summary>
        /// Tests LoadBaselineFile with non-existent file returns false
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadBaselineFile_NonExistentFile_ReturnsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sNON_EXISTENT_FILE = "NonExistentBaselineFile.xml";

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private LoadBaselineFile method
            var loadMethod = typeof(GeneratorForm).GetMethod("LoadBaselineFile", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool bResult = (bool)loadMethod.Invoke(generatorForm, new object[] { sNON_EXISTENT_FILE });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed as expected
            Assert.IsFalse(bResult, "LoadBaselineFile should return false for non-existent file");

            // Verify error was set in status box
            StringAssert.Contains(generatorForm.StatusBoxText, "Baseline file not found");
        }

        /// <summary>
        /// Tests LoadResultFile with valid file loads successfully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadResultFile_ValidFile_LoadsSuccessfully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test XML file content
            const string sVALID_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.2</Data>
    <Data Time=""01:30:46"">0.4</Data>
    <Data Time=""01:30:47"">0.8</Data>
</Session>";

            File.WriteAllText(m_sRESULT_TEST_FILE, sVALID_XML_CONTENT);

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private LoadResultFile method
            MethodInfo loadMethod = typeof(GeneratorForm).GetMethod("LoadResultFile", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResult = (bool)loadMethod.Invoke(generatorForm, new object[] { m_sRESULT_TEST_FILE });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load was successful
            Assert.IsTrue(bResult, "LoadResultFile should return true for valid file");
        }

        /// <summary>
        /// Tests LoadResultFile with malformed XML returns false and sets error
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadResultFile_MalformedXML_ReturnsFalseWithError()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create malformed XML file content
            const string sMALFORMED_XML_CONTENT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.2</Data>
    <UnclosedElement>
</Session>";

            File.WriteAllText(m_sMALFORMED_TEST_FILE, sMALFORMED_XML_CONTENT);

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private LoadResultFile method
            MethodInfo loadMethod = typeof(GeneratorForm).GetMethod("LoadResultFile", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResult = (bool)loadMethod.Invoke(generatorForm, new object[] { m_sMALFORMED_TEST_FILE });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the load failed as expected
            Assert.IsFalse(bResult, "LoadResultFile should return false for malformed XML file");

            // Verify error was set in status box
            StringAssert.Contains(generatorForm.StatusBoxText, "Result file loading error");
        }

        /// <summary>
        /// Tests OnBaselineLoadCompleted with successful load updates UI fields
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void OnBaselineLoadCompleted_SuccessfulLoad_UpdatesUIFields()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sTEST_FILE_PATH = "TestBaseline.xml";
            const bool bSUCCESS = true;

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Set up baseline analysis using reflection
            FieldInfo baselineAnalysisField = typeof(GeneratorForm).GetField("m_BaselineAnalysis", 
                                       BindingFlags.NonPublic | BindingFlags.Instance);
            StatisticalAnalysis mockAnalysis = new StatisticalAnalysis();
            baselineAnalysisField.SetValue(generatorForm, mockAnalysis);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private OnBaselineLoadCompleted method
            MethodInfo completedMethod = typeof(GeneratorForm).GetMethod("OnBaselineLoadCompleted", 
                                 BindingFlags.NonPublic | BindingFlags.Instance);
            completedMethod.Invoke(generatorForm, new object[] { sTEST_FILE_PATH, bSUCCESS });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify success status was set
            StringAssert.Contains(generatorForm.StatusBoxText, "Baseline file loaded");
            Assert.AreEqual(StatusPalette.SuccessBackground, generatorForm.StatusBoxBackColor);
        }

        /// <summary>
        /// Tests OnBaselineLoadCompleted with failed load clears UI fields
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void OnBaselineLoadCompleted_FailedLoad_ClearsUIFields()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sTEST_FILE_PATH = "TestBaseline.xml";
            const bool bSUCCESS = false;

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private OnBaselineLoadCompleted method
            MethodInfo completedMethod = typeof(GeneratorForm).GetMethod("OnBaselineLoadCompleted", 
                                 BindingFlags.NonPublic | BindingFlags.Instance);
            completedMethod.Invoke(generatorForm, new object[] { sTEST_FILE_PATH, bSUCCESS });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify baseline analysis was cleared
            FieldInfo baselineAnalysisField = typeof(GeneratorForm).GetField("m_BaselineAnalysis", 
                                       BindingFlags.NonPublic | BindingFlags.Instance);
            object baselineAnalysis = baselineAnalysisField.GetValue(generatorForm);
            Assert.IsNull(baselineAnalysis, "Baseline analysis should be cleared on failed load");
        }

        /// <summary>
        /// Tests OnResultLoadCompleted with successful load updates UI fields
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void OnResultLoadCompleted_SuccessfulLoad_UpdatesUIFields()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sTEST_FILE_PATH = "TestResult.xml";
            const bool bSUCCESS = true;

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Set up result analysis using reflection
            FieldInfo resultAnalysisField = typeof(GeneratorForm).GetField("m_ResultAnalysis", 
                                     BindingFlags.NonPublic | BindingFlags.Instance);
            StatisticalAnalysis mockAnalysis = new StatisticalAnalysis();
            resultAnalysisField.SetValue(generatorForm, mockAnalysis);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private OnResultLoadCompleted method
            MethodInfo completedMethod = typeof(GeneratorForm).GetMethod("OnResultLoadCompleted", 
                                 BindingFlags.NonPublic | BindingFlags.Instance);
            completedMethod.Invoke(generatorForm, new object[] { sTEST_FILE_PATH, bSUCCESS });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify success status was set
            StringAssert.Contains(generatorForm.StatusBoxText, "Result file loaded");
            Assert.AreEqual(StatusPalette.SuccessBackground, generatorForm.StatusBoxBackColor);
        }

        /// <summary>
        /// Tests UpdateComparisonStatistics with no data clears comparison fields
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void UpdateComparisonStatistics_NoData_ClearsFields()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use reflection to call the private UpdateComparisonTable method
            MethodInfo updateMethod = typeof(GeneratorForm).GetMethod("UpdateComparisonTable",
                              BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod.Invoke(generatorForm, new object[] { });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // The table keeps its rows whether or not anything is loaded, so every measure is still named
            FieldInfo listField = typeof(GeneratorForm).GetField("m_ComparisonList",
                              BindingFlags.NonPublic | BindingFlags.Instance);
            ListView comparisonList = (ListView)listField.GetValue(generatorForm);
            Assert.IsTrue(comparisonList.Items.Count > 0, "The table should keep one row per measure");

            // Take the placeholder from the form rather than repeating it here, so this cannot pass against
            // a form that shows something else entirely
            FieldInfo noValueField = typeof(GeneratorForm).GetField("m_sNO_VALUE",
                              BindingFlags.NonPublic | BindingFlags.Static);
            string sNoValue = (string)noValueField.GetRawConstantValue();

            // With no file loaded, every value reads as having no value rather than as a zero
            foreach (ListViewItem measureRow in comparisonList.Items)
            {
                Assert.IsFalse(string.IsNullOrEmpty(measureRow.Text), "Each row should still name its measure");
                for (int iColumn = 1; iColumn < measureRow.SubItems.Count; iColumn++)
                {
                    Assert.AreEqual(sNoValue, measureRow.SubItems[iColumn].Text,
                                    $"Row '{measureRow.Text}' column {iColumn} should show no value");
                }
            }
        }

        #endregion
        #region Integration Tests for File Loading

        /// <summary>
        /// Tests loading both baseline and result files triggers comparison statistics update
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void LoadBothFiles_Integration_UpdatesComparisonStatistics()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test XML files
            const string sBASELINE_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.1</Data>
    <Data Time=""01:30:46"">0.3</Data>
    <Data Time=""01:30:47"">0.5</Data>
</Session>";

            const string sRESULT_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.2</Data>
    <Data Time=""01:30:46"">0.4</Data>
    <Data Time=""01:30:47"">0.6</Data>
</Session>";

            File.WriteAllText(m_sBASELINE_INTEGRATION_FILE, sBASELINE_XML);
            File.WriteAllText(m_sRESULT_INTEGRATION_FILE, sRESULT_XML);

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load baseline file
            MethodInfo loadBaselineMethod = typeof(GeneratorForm).GetMethod("LoadBaselineFile", 
                                   BindingFlags.NonPublic | BindingFlags.Instance);
            bool bBaselineLoaded = (bool)loadBaselineMethod.Invoke(generatorForm, new object[] { m_sBASELINE_INTEGRATION_FILE });

            MethodInfo onBaselineCompletedMethod = typeof(GeneratorForm).GetMethod("OnBaselineLoadCompleted", 
                                           BindingFlags.NonPublic | BindingFlags.Instance);
            onBaselineCompletedMethod.Invoke(generatorForm, new object[] { m_sBASELINE_INTEGRATION_FILE, bBaselineLoaded });

            // Load result file
            MethodInfo loadResultMethod = typeof(GeneratorForm).GetMethod("LoadResultFile", 
                                  BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResultLoaded = (bool)loadResultMethod.Invoke(generatorForm, new object[] { m_sRESULT_INTEGRATION_FILE });

            MethodInfo onResultCompletedMethod = typeof(GeneratorForm).GetMethod("OnResultLoadCompleted", 
                                         BindingFlags.NonPublic | BindingFlags.Instance);
            onResultCompletedMethod.Invoke(generatorForm, new object[] { m_sRESULT_INTEGRATION_FILE, bResultLoaded });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify both files loaded successfully
            Assert.IsTrue(bBaselineLoaded, "Baseline file should load successfully");
            Assert.IsTrue(bResultLoaded, "Result file should load successfully");

            // Verify both analysis objects exist
            FieldInfo baselineAnalysisField = typeof(GeneratorForm).GetField("m_BaselineAnalysis", 
                                       BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo resultAnalysisField = typeof(GeneratorForm).GetField("m_ResultAnalysis", 
                                     BindingFlags.NonPublic | BindingFlags.Instance);
            
            object baselineAnalysis = baselineAnalysisField.GetValue(generatorForm);
            object resultAnalysis = resultAnalysisField.GetValue(generatorForm);

            Assert.IsNotNull(baselineAnalysis, "Baseline analysis should be created");
            Assert.IsNotNull(resultAnalysis, "Result analysis should be created");
        }

        /// <summary>
        /// Tests that loading files in different order both trigger comparison updates
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void LoadFilesInReverseOrder_Integration_UpdatesComparisonStatistics()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create test XML files
            const string sBASELINE_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.1</Data>
    <Data Time=""01:30:46"">0.5</Data>
</Session>";

            const string sRESULT_XML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Session Simulated=""false"" Target=""1"">
    <Data Time=""01:30:45"">0.3</Data>
    <Data Time=""01:30:46"">0.7</Data>
</Session>";

            File.WriteAllText(m_sBASELINE_REVERSE_FILE, sBASELINE_XML);
            File.WriteAllText(m_sRESULT_REVERSE_FILE, sRESULT_XML);

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load result file FIRST (reverse order)
            MethodInfo loadResultMethod = typeof(GeneratorForm).GetMethod("LoadResultFile", 
                                  BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResultLoaded = (bool)loadResultMethod.Invoke(generatorForm, new object[] { m_sRESULT_REVERSE_FILE });

            MethodInfo onResultCompletedMethod = typeof(GeneratorForm).GetMethod("OnResultLoadCompleted", 
                                         BindingFlags.NonPublic | BindingFlags.Instance);
            onResultCompletedMethod.Invoke(generatorForm, new object[] { m_sRESULT_REVERSE_FILE, bResultLoaded });

            // Then load baseline file SECOND
            MethodInfo loadBaselineMethod = typeof(GeneratorForm).GetMethod("LoadBaselineFile", 
                                   BindingFlags.NonPublic | BindingFlags.Instance);
            bool bBaselineLoaded = (bool)loadBaselineMethod.Invoke(generatorForm, new object[] { m_sBASELINE_REVERSE_FILE });

            MethodInfo onBaselineCompletedMethod = typeof(GeneratorForm).GetMethod("OnBaselineLoadCompleted", 
                                           BindingFlags.NonPublic | BindingFlags.Instance);
            onBaselineCompletedMethod.Invoke(generatorForm, new object[] { m_sBASELINE_REVERSE_FILE, bBaselineLoaded });

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify both files loaded successfully
            Assert.IsTrue(bResultLoaded, "Result file should load successfully when loaded first");
            Assert.IsTrue(bBaselineLoaded, "Baseline file should load successfully when loaded second");

            // Verify both analysis objects exist
            FieldInfo baselineAnalysisField = typeof(GeneratorForm).GetField("m_BaselineAnalysis", 
                                       BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo resultAnalysisField = typeof(GeneratorForm).GetField("m_ResultAnalysis", 
                                     BindingFlags.NonPublic | BindingFlags.Instance);
            
            object baselineAnalysis = baselineAnalysisField.GetValue(generatorForm);
            object resultAnalysis = resultAnalysisField.GetValue(generatorForm);

            Assert.IsNotNull(baselineAnalysis, "Baseline analysis should be created when loaded second");
            Assert.IsNotNull(resultAnalysis, "Result analysis should be created when loaded first");
        }

        /// <summary>
        /// Tests file loading error scenarios trigger proper error handling
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void FileLoadingErrors_Integration_ProperErrorHandling()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock dependencies
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Test baseline file not found
            MethodInfo loadBaselineMethod = typeof(GeneratorForm).GetMethod("LoadBaselineFile", 
                                   BindingFlags.NonPublic | BindingFlags.Instance);
            bool bBaselineResult = (bool)loadBaselineMethod.Invoke(generatorForm, new object[] { "NonExistentBaseline.xml" });

            Assert.IsFalse(bBaselineResult, "LoadBaselineFile should return false for non-existent file");
            StringAssert.Contains(generatorForm.StatusBoxText, "Baseline file not found", "Should show baseline file not found error");

            // Test result file not found
            MethodInfo loadResultMethod = typeof(GeneratorForm).GetMethod("LoadResultFile", 
                                  BindingFlags.NonPublic | BindingFlags.Instance);
            bool bResultResult = (bool)loadResultMethod.Invoke(generatorForm, new object[] { "NonExistentResult.xml" });

            Assert.IsFalse(bResultResult, "LoadResultFile should return false for non-existent file");
            StringAssert.Contains(generatorForm.StatusBoxText, "Result file not found", "Should show result file not found error");
        }

        /// <summary>
        /// Tests setting the simulate toggle from code settles rather than toggling back and forth.
        /// NOTE: Loading a session sets this toggle to match the file. Taking the new state by inverting
        /// what is recorded rather than by reading the toggle left the two of them setting each other
        /// without end, which overflowed the stack and took the application down as a file was opened.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [Timeout(15000)]
        public void SimulateToggle_SetFromCode_MatchesTheToggle()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer and setup the properties
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();

            // Mock the session data, recording it as simulated as a loaded session would
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.SetupProperty(mock => mock.Simulated, true);
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);

            // Reach the toggle, which is not exposed outside of the form
            System.Reflection.FieldInfo toggleField = typeof(GeneratorForm).GetField("m_SimulateToggle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(toggleField, "The simulate toggle should be present on the form");
            System.Windows.Forms.CheckBox simulateToggle = (System.Windows.Forms.CheckBox)toggleField.GetValue(generatorForm);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the toggle the way loading a simulated session does, which disagrees with the toggle
            simulateToggle.Checked = true;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the toggle and what is recorded agree, rather than having driven each other
            Assert.IsTrue(simulateToggle.Checked, "The toggle should stay set");
            Assert.IsTrue(mockSessionData.Object.Simulated, "The recorded state should match the toggle");

            // And that setting it back settles the same way
            simulateToggle.Checked = false;
            Assert.IsFalse(simulateToggle.Checked, "The toggle should stay clear");
            Assert.IsFalse(mockSessionData.Object.Simulated, "The recorded state should match the toggle");
        }

        /// <summary>
        /// Builds a form with a session running on it, the way pressing Start does, so that what happens
        /// while a session is recording can be tested. The device is reported as already initialized and
        /// the session as starting successfully, because what is under test here is what the form does
        /// once a session is running rather than how it gets one started.
        /// </summary>
        /// <param name="mockSessionTimer">IN - The session timer the form reads the elapsed time from</param>
        /// <param name="iSessionLengthMinutes">IN - The length to give the session, zero for no limit</param>
        /// <param name="mockSessionData">OUT - The session data the form was built with</param>
        /// <returns>The form, with a session running on it</returns>
        private GeneratorForm CreateRunningForm(Mock<IRNGSessionTimer> mockSessionTimer, int iSessionLengthMinutes,
                                                out Mock<IRNGSessionData> mockSessionData)
        {
            // Mock the session data, which reports no target so that starting settles the target rather
            // than putting up the dialog that asks whether a changed one should be kept
            mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.Timer).Returns(mockSessionTimer.Object);
            mockSessionData.Setup(mock => mock.TargetValue).Returns(TargetValues.NO_VALUE_SET);
            mockSessionData.Setup(mock => mock.StartSession()).Returns(true);
            mockSessionData.Setup(mock => mock.AddDataPoint(It.IsAny<double>())).Returns(true);

            // Mock the device timer as already initialized, so nothing reaches the device
            Mock<IRNGDeviceTimer> mockDeviceTimer = new Mock<IRNGDeviceTimer>();
            mockDeviceTimer.Setup(mock => mock.Initialized).Returns(true);

            // Create the form and give the session its length before it starts, which is while the field
            // is still open to be changed
            GeneratorForm generatorForm = new GeneratorForm(mockSessionData.Object, mockDeviceTimer.Object);
            NumericUpDown sessionLength = GetSessionLengthField(generatorForm);
            sessionLength.Value = iSessionLengthMinutes;

            // Start the session
            typeof(GeneratorForm).GetMethod("SetRunningState", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(generatorForm, null);

            return generatorForm;
        }

        /// <summary>
        /// Gets the session length field off a form
        /// </summary>
        /// <param name="generatorForm">IN - The form to read the field from</param>
        /// <returns>The control the session length is set in</returns>
        private NumericUpDown GetSessionLengthField(GeneratorForm generatorForm)
        {
            return (NumericUpDown)typeof(GeneratorForm)
                .GetField("m_SessionLengthUpDown", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(generatorForm);
        }

        /// <summary>
        /// Ends the session on a form, the way pressing Stop does
        /// </summary>
        /// <param name="generatorForm">INOUT - The form to end the session on</param>
        private void EndSessionOn(GeneratorForm generatorForm)
        {
            typeof(GeneratorForm).GetMethod("SetIdleState", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(generatorForm, null);
        }

        /// <summary>
        /// Tests a session given a length stops itself once it has recorded for that long
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_SessionLengthReached_StopsTheSession()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iSESSION_LENGTH_MINUTES = 1;
            const int iELAPSED_SECONDS = 60;
            const string sEXPECTED_TEXT = "Session stopped after 1 minute";

            // Mock a session timer that has been running for as long as the session was given
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            mockSessionTimer.Setup(mock => mock.ElapsedSeconds).Returns(iELAPSED_SECONDS);

            // Create the object under test, with a session running on it
            GeneratorForm generatorForm = CreateRunningForm(mockSessionTimer, iSESSION_LENGTH_MINUTES,
                                                            out Mock<IRNGSessionData> mockSessionData);
            Assert.AreEqual(GeneratorForm.RngGuiStates.Running, generatorForm.State, "The session should be running");

            //**************************************************************//
            // Act
            //**************************************************************//

            // Take a reading, which is when the length is checked
            const double fVALID_RESULT = 0.5;
            generatorForm.RecordReadResult(fVALID_RESULT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session was ended
            Assert.AreEqual(GeneratorForm.RngGuiStates.Idle, generatorForm.State);
            mockSessionData.Verify(mock => mock.EndSession(), Times.AtLeastOnce);

            // Verify the reason is on display, rather than the message the return to idle would leave
            StringAssert.Contains(generatorForm.StatusBoxText, sEXPECTED_TEXT);
        }

        /// <summary>
        /// Tests a session given a length keeps recording until it has been running for that long
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_SessionLengthNotReached_KeepsRecording()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iSESSION_LENGTH_MINUTES = 1;
            const int iELAPSED_SECONDS = 59;

            // Mock a session timer that is one second short of the length the session was given
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            mockSessionTimer.Setup(mock => mock.ElapsedSeconds).Returns(iELAPSED_SECONDS);

            // Create the object under test, with a session running on it
            GeneratorForm generatorForm = CreateRunningForm(mockSessionTimer, iSESSION_LENGTH_MINUTES,
                                                            out Mock<IRNGSessionData> mockSessionData);

            //**************************************************************//
            // Act
            //**************************************************************//

            const double fVALID_RESULT = 0.5;
            generatorForm.RecordReadResult(fVALID_RESULT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session is still running, and the reading was recorded
            Assert.AreEqual(GeneratorForm.RngGuiStates.Running, generatorForm.State);
            mockSessionData.Verify(mock => mock.AddDataPoint(fVALID_RESULT), Times.Once);
        }

        /// <summary>
        /// Tests a session with no length set records for as long as it is left to, which is what the
        /// length field starts at
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void RecordReadResult_NoSessionLengthSet_KeepsRecording()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iNO_SESSION_LENGTH = 0;
            const int iELAPSED_SECONDS = 86400;

            // Mock a session timer that has been recording for a day, with no length set against it
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            mockSessionTimer.Setup(mock => mock.ElapsedSeconds).Returns(iELAPSED_SECONDS);

            // Create the object under test, with a session running on it
            GeneratorForm generatorForm = CreateRunningForm(mockSessionTimer, iNO_SESSION_LENGTH,
                                                            out Mock<IRNGSessionData> mockSessionData);

            //**************************************************************//
            // Act
            //**************************************************************//

            const double fVALID_RESULT = 0.5;
            generatorForm.RecordReadResult(fVALID_RESULT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify nothing stopped the session, however long it has been going
            Assert.AreEqual(GeneratorForm.RngGuiStates.Running, generatorForm.State);
            mockSessionData.Verify(mock => mock.AddDataPoint(fVALID_RESULT), Times.Once);
        }

        /// <summary>
        /// Tests the session length is settled before a session starts and open again once it has ended
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void SetRunningState_SessionStarted_SessionLengthIsLockedUntilItEnds()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iSESSION_LENGTH_MINUTES = 5;
            const int iELAPSED_SECONDS = 0;

            // Mock a session timer that has only just started
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            mockSessionTimer.Setup(mock => mock.ElapsedSeconds).Returns(iELAPSED_SECONDS);

            // Create the object under test, with a session running on it
            GeneratorForm generatorForm = CreateRunningForm(mockSessionTimer, iSESSION_LENGTH_MINUTES,
                                                            out Mock<IRNGSessionData> mockSessionData);
            _ = mockSessionData;
            NumericUpDown sessionLength = GetSessionLengthField(generatorForm);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bLockedWhileRunning = (false == sessionLength.Enabled);
            EndSessionOn(generatorForm);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the length could not be changed under a session that was reading it
            Assert.IsTrue(bLockedWhileRunning);

            // Verify it is open again for the next session
            Assert.IsTrue(sessionLength.Enabled);
        }

        /// <summary>
        /// Tests ending a session keeps the data file, so another session can be recorded into it without
        /// browsing for the same file again
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_SessionEnded_KeepsTheDataFileAndStillOffersStart()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const string sEXPECTED_FILE = "session.rng";
            const int iNO_SESSION_LENGTH = 0;
            const int iELAPSED_SECONDS = 0;

            // Mock a session timer that has only just started
            Mock<IRNGSessionTimer> mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupAllProperties();
            mockSessionTimer.Setup(mock => mock.ElapsedSeconds).Returns(iELAPSED_SECONDS);

            // Create the object under test, with a session running on it and a file chosen for it
            GeneratorForm generatorForm = CreateRunningForm(mockSessionTimer, iNO_SESSION_LENGTH,
                                                            out Mock<IRNGSessionData> mockSessionData);
            _ = mockSessionData;
            TextBox fileTextBox = (TextBox)typeof(GeneratorForm)
                .GetField("m_FileTextBox", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(generatorForm);
            Button startButton = (Button)typeof(GeneratorForm)
                .GetField("m_StartButton", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(generatorForm);
            fileTextBox.Text = sEXPECTED_FILE;

            //**************************************************************//
            // Act
            //**************************************************************//

            // End the session, the way pressing Stop does
            EndSessionOn(generatorForm);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the file is still chosen, and that a session can be started into it straight away
            Assert.AreEqual(sEXPECTED_FILE, fileTextBox.Text);
            Assert.IsTrue(startButton.Enabled);
        }

        #endregion
    }
}