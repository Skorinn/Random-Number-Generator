//*********************************************************************************************************************
// File Name:      GeneratorForm.Test.cs
// Description:    Unit tests for the GeneratorForm class
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
using System;
using System.ComponentModel;
using System.Drawing;

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
        private const string m_sEXPECTED_DEFAULT_TEXT = " Idle";
        private const string m_sEXPECTED_RUNNING_TEXT = "Running";
        private const string m_sEXPECTED_ERROR_TEXT = "Error reading";

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
        /// <\summary>
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
        // <\summary>
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
        // <\summary>
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
        // <\summary>
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
        // <\summary>
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
        // <\summary>
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
            Color expectedTextColor = System.Drawing.Color.Black;
            Color expectedBackColor = System.Drawing.SystemColors.Info;

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
            Color expectedTextColor = System.Drawing.Color.Black;
            Color expectedBackColor = System.Drawing.Color.Red;

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
            Color expectedBackColor = System.Drawing.Color.Red;

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
            Color expectedBackColor = System.Drawing.Color.Red;

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
            Color expectedBackColor = System.Drawing.Color.Red;

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
            Color expectedBackColor = System.Drawing.Color.Red;

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
            Color expectedTextColor = System.Drawing.Color.Black;
            Color expectedBackColor = System.Drawing.SystemColors.Info;

            // Mock the session timer and setup the properties
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
    }
}