//*********************************************************************************************************************
// File Name:      RNGDeviceTimer.Test.cs
// Description:    Unit tests for the RNGDeviceTimer class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/02/04 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Summary description for RNGDeviceTimer
    /// </summary>
    [TestClass]
    public class RNGDeviceTimerTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGDeviceTimerTests()
        {
            // Nothing to do
        }

        /// <summary>
        /// Read result callback function to use for testing
        /// </summary>
        public void RecordReadResult(double fResult)
        {
            // Protect as this is executed from a thread
            lock (this)
            {
                // Validate the result and increment the call counter
                if ((0 > fResult) || (1 < fResult))
                {
                    ++m_iInvalidCallbackCalls;
                }
                ++m_iReadCallbackCalls;
            }
        }

        #endregion
        #region Data Members

        // Counter for tracking calls to the read callback
        private uint m_iReadCallbackCalls = 0;

        // Tracks any invalid values passed to the read callback
        private uint m_iInvalidCallbackCalls = 0;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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
        /// Tests the default property values
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DefaultConstructor_Properites_DefaultValues()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Expected default values
            const int iEXPECTED_INTERVAL = 10;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test using the default constructor
            RNGDeviceTimer timer = new RNGDeviceTimer();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the default values for the properties
            Assert.IsFalse(timer.Enabled);
            Assert.IsFalse(timer.Initialized);
            Assert.AreEqual(iEXPECTED_INTERVAL, timer.Interval);
        }

        /// <summary>
        /// Tests InitializeDevice with valid parameters returns true
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void InitializeDevice_Valid_ReturnsTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test 
            RNGDeviceTimer timer = new RNGDeviceTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Use InitializeDevice to initialize a simulator
            const int iSEED = 0;
            const bool bSIMULATE = true;
            bool bReturn = timer.InitializeDevice(iSEED, bSIMULATE);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the function return and the initilized property values
            Assert.IsTrue(bReturn);
            Assert.IsTrue(timer.Initialized);
        }

        /// <summary>
        /// Tests the read callback is executed when the timer ticks
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void SetReadCallback_Valid_ExecutedByTick()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the number of ticks to simulate
            const uint iNUM_TICKS = 100;

            // Reset the callback counters
            m_iReadCallbackCalls = 0;
            m_iInvalidCallbackCalls = 0;

            // Create the object under test 
            RNGDeviceTimer timer = new RNGDeviceTimer();

            // Initialize the simulator
            const int iSEED = 0;
            const bool bSIMULATE = true;
            bool bReturn = timer.InitializeDevice(iSEED, bSIMULATE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the callback and simulate the timer ticking
            timer.SetReadCallback(RecordReadResult);
            timer.Start();
            for (uint iTickCount = 0; iNUM_TICKS > iTickCount; ++iTickCount)
            {
                timer.TriggerTick();
            }
            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the callback was executed
            bool bCallbackExecuted = (iNUM_TICKS == m_iReadCallbackCalls);
            Assert.IsTrue(bCallbackExecuted);
            m_iReadCallbackCalls = 0;

            // Verify no invalid results were passed to the callback
            bool bInvalidResult = (0 < m_iInvalidCallbackCalls);
            Assert.IsFalse(bInvalidResult);
            m_iInvalidCallbackCalls = 0;
        }

        /// <summary>
        /// Tests that TriggerTick is success if no callback is set
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void TriggerTick_NoCallback_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test 
            RNGDeviceTimer timer = new RNGDeviceTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Simulate the timer ticking without a callback
            timer.Start();
            timer.TriggerTick();
            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // No exception should be thrown
        }

        /// <summary>
        /// Tests that Start sets the Enabled property to true
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void Start_Enabled_IsTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test 
            RNGDeviceTimer timer = new RNGDeviceTimer();

            // Initialize the simulator
            const int iSEED = 0;
            const bool bSIMULATE = true;
            bool bReturn = timer.InitializeDevice(iSEED, bSIMULATE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the timer and record the state of the enabled flag
            timer.Start();
            bool bEnabled = timer.Enabled;

            // Stop the timer
            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//
            Assert.IsTrue(bEnabled);
        }

        /// <summary>
        /// Tests that Stop sets the Enabled property to false
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void Stop_Enabled_IsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test 
            RNGDeviceTimer timer = new RNGDeviceTimer();

            // Initialize the simulator
            const int iSEED = 0;
            const bool bSIMULATE = true;
            bool bReturn = timer.InitializeDevice(iSEED, bSIMULATE);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start then stop the timer
            timer.Start();
            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//
            Assert.IsFalse(timer.Enabled);
        }

        /// <summary>
        /// Tests get and set for the Interval property
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Interval_Set_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected result
            int iVALID_INTERVAL = 978;

            // Create the object under test
            RNGDeviceTimer timer = new RNGDeviceTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the interval to the valid value
            timer.Interval = iVALID_INTERVAL;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(iVALID_INTERVAL, timer.Interval);
        }

        #endregion
    }
}
