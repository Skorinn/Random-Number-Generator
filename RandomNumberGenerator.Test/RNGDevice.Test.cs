//*********************************************************************************************************************
// File Name:      RNGDevice.Test.cs
// Description:    Unit tests for the RNGDevice class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/01/20 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the RNGDevice class
    /// </summary>
    [TestClass]
    public class RNGDeviceTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGDeviceTests()
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
        /// Tests properties are set correctly using the default constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Default_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_DESCRIPTION = "";
            const int iEXPECTED_PORT = -1;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGDevice rngDevice = new RNGDevice();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_DESCRIPTION, rngDevice.Description);
            Assert.AreEqual(iEXPECTED_PORT, rngDevice.Port);
        }

        /// <summary>
        /// Tests properties are set correctly using the initializing constructor
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Constructor_Initializing_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_DESCRIPTION = "Expected Text";
            const int iEXPECTED_PORT = (int.MaxValue / 2);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGDevice rngDevice = new RNGDevice(sEXPECTED_DESCRIPTION, iEXPECTED_PORT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_DESCRIPTION, rngDevice.Description);
            Assert.AreEqual(iEXPECTED_PORT, rngDevice.Port);
        }

        /// <summary>
        /// Tests set and get for the description and port properties
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DescriptionAndPort_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_DESCRIPTION = "Expected Text";
            const int iEXPECTED_PORT = (int.MaxValue / 2);

            // Create the initial values for the constructor
            const string sINITIAL_DESCRIPTION = "";
            const int iINITIAL_PORT = -1;

            // Create the object under test
            RNGDevice rngDevice = new RNGDevice(sINITIAL_DESCRIPTION, iINITIAL_PORT);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the properties under test
            rngDevice.Description = sEXPECTED_DESCRIPTION;
            rngDevice.Port = iEXPECTED_PORT;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_DESCRIPTION, rngDevice.Description);
            Assert.AreEqual(iEXPECTED_PORT, rngDevice.Port);
        }

        #endregion
    }
}
