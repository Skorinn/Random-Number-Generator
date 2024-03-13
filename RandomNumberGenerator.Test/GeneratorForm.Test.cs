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

        #endregion
    }
}