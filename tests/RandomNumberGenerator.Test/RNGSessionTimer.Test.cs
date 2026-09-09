//*********************************************************************************************************************
// File Name:      RNGSessionTimer.Test.cs
// Description:    Unit tests for the RNGSessionTimer class
//
// Copyright (c) 2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2024/02/03 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows.Forms;

namespace RandomNumberGenerator.Test
{
    [TestClass]
    public class RNGSessionTimerTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGSessionTimerTests()
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

        /// <summary>
        /// Simulates the timer ticks for the specified interval and number of ticks
        /// </summary>
        /// <param name="sessionTimer">INOUT - The session timer to simulate ticks</param>
        /// <param name="iInterval">IN - The interval for each tick</param>
        /// <param name="iNumTicks">IN - The number of ticks to simulate</param>
        private void SimulateTimer(IRNGSessionTimer sessionTimer, int iInterval, uint iNumTicks)
        {
            // Set the interval on the timer
            sessionTimer.Interval = iInterval;

            // Simulate the timer ticks
            for (uint iTickIndex = 0; iNumTicks > iTickIndex; ++iTickIndex)
            {
                sessionTimer.Tick();
            }
        }

        #endregion
        #region Data Members

        // Information about the current test context
        private TestContext testContextInstance;

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
        /// Tests the Start method sets the Enabled and InProgress properties to true
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Start_EnabledandInProgressPropertiesAreTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();

            // Create the text box for the timer
            TextBox timerTextBox = new TextBox();

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);

            //**************************************************************//
            // Act
            //**************************************************************//
            timer.Start();

            //**************************************************************//
            // Assert
            //**************************************************************//
            Assert.IsTrue(timer.Enabled);
            Assert.IsTrue(timer.InProgress);
        }

        /// <summary>
        /// Tests the Stop method sets the Enabled and InProgress properties to false
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Stop_RunningPropertyIsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the text box for the timer
            TextBox timerTextBox = new TextBox();

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);

            //**************************************************************//
            // Act
            //**************************************************************//
            timer.Start();
            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//
            Assert.IsFalse(timer.Enabled);
            Assert.IsFalse(timer.InProgress);
        }

        /// <summary>
        /// Tests the Tick method updates the session timer using the default interval
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Tick_DefaultInterval_SessionTimeUpdated()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value (single tick with default interval of 1s)
            const string sEXPECTED_VALUE = "00:00:01";

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Tick the timer
            timer.Tick();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the timer text was updated
            Assert.AreEqual(sEXPECTED_VALUE, timerTextBox.Text);
        }

        /// <summary>
        /// Tests the Tick method updates the session timer using a non-default interval
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Tick_NonDefaultInterval_SessionTimeUpdated()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value (single tick with non-default interval of 2s)
            const string sEXPECTED_VALUE = "00:00:02";

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the interval to 200ms and tick the timer 10 times (2 seconds total)
            SimulateTimer(timer, 200, 10);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the timer text was updated
            Assert.AreEqual(sEXPECTED_VALUE, timerTextBox.Text);
        }

        /// <summary>
        /// Tests Tick() does not generate an exception when the TimerTextBox is not set
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Tick_TimerTextBoxNotSet_NoException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Tick the timer
            timer.Tick();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // No exception should be thrown
        }

        /// <summary>
        /// Tests Reset method resets the session timer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Reset_SessionTimerIsReset()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value
            const string sEXPECTED_VALUE = "00:00:00";

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer and tick it once to update the session timer to non-zero
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);
            timer.Tick();

            //**************************************************************//
            // Act
            //**************************************************************//
            timer.Reset();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the timer text was updated
            Assert.AreEqual(sEXPECTED_VALUE, timerTextBox.Text);
        }

        /// <summary>
        /// Tests the Enabled property doesn't affect the InProgress property when set to false
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Enabled_PropertyDoesNotAffectInProgressProperty()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start the timer then set the enabled property to false
            timer.Start();
            timer.Enabled = false;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the in progress property is still true
            Assert.IsTrue(timer.InProgress);
        }

        /// <summary>
        /// Verify the Interval property cna be set and read back
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Interval_PropertyIsSet()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value to somethign different from the default (1s)
            int iExpectedValue = 59;

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Act
            //**************************************************************//
            timer.Interval = iExpectedValue;

            //**************************************************************//
            // Assert
            //**************************************************************//
            Assert.AreEqual(iExpectedValue, timer.Interval);
        }

        /// <summary>
        /// Test setting the TimerTextBox property
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void TimerTextBox_PropertyIsSet()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value
            const string sEXPECTED_VALUE = "00:00:01";

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the timer text box and tick the timer
            timer.TimerTextBox = timerTextBox;
            timer.Tick();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the timer text object was updated by the timer
            Assert.AreEqual(sEXPECTED_VALUE, timerTextBox.Text);
        }

        /// <summary>
        /// Tests the elapsed time is reported in seconds, which is what a session length is measured against
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ElapsedSeconds_TimerHasRun_ReportsTheWholeElapsedTime()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // An hour, two minutes and three seconds, so the hours and minutes have to be carried into the
            // total rather than only the seconds being reported
            const int iINTERVAL = 1000;
            const uint iNUM_TICKS = 3723;
            const int iEXPECTED_SECONDS = 3723;

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Act
            //**************************************************************//

            SimulateTimer(timer, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the elapsed time agrees with the time shown, so the two cannot drift apart
            Assert.AreEqual(iEXPECTED_SECONDS, timer.ElapsedSeconds);
            Assert.AreEqual("01:02:03", timer.SessionTime);
        }

        /// <summary>
        /// Tests a timer that has not run reports nothing elapsed, so a session length is never reached
        /// before the session has recorded anything
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ElapsedSeconds_TimerNotStarted_ReportsZero()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iEXPECTED_SECONDS = 0;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.AreEqual(iEXPECTED_SECONDS, timer.ElapsedSeconds);
        }

        /// <summary>
        /// Tests resetting the timer takes the elapsed time back to nothing, so the length given to one
        /// session is not counted as already part run by the next
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ElapsedSeconds_TimerReset_ReportsZero()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iINTERVAL = 1000;
            const uint iNUM_TICKS = 90;
            const int iEXPECTED_SECONDS = 0;

            // Create the session timer and run it for a while
            IRNGSessionTimer timer = new RNGSessionTimer();
            SimulateTimer(timer, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Act
            //**************************************************************//

            timer.Reset();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.AreEqual(iEXPECTED_SECONDS, timer.ElapsedSeconds);
        }

        /// <summary>
        /// Tests starting the timer times the new session from nothing, rather than carrying on from where
        /// the last one finished
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Start_AfterAPreviousSession_TimesTheNewSessionFromNothing()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iINTERVAL = 1000;
            const uint iNUM_TICKS = 90;
            const int iEXPECTED_SECONDS = 0;
            const string sEXPECTED_TIME = "00:00:00";

            // Run a session and stop it, the way pressing Start and then Stop does
            IRNGSessionTimer timer = new RNGSessionTimer();
            timer.Start();
            SimulateTimer(timer, iINTERVAL, iNUM_TICKS);
            timer.Stop();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start a second session
            timer.Start();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the new session is timed from nothing. Carrying on from the last one put a session that
            // had been given a length past it before it had recorded anything.
            Assert.AreEqual(iEXPECTED_SECONDS, timer.ElapsedSeconds);
            Assert.AreEqual(sEXPECTED_TIME, timer.SessionTime);
        }

        /// <summary>
        /// Tests stopping the timer leaves the time the session reached on display, so how long it ran can
        /// still be read once it has ended
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Stop_SessionHasRun_LeavesTheTimeItReachedOnDisplay()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const int iINTERVAL = 1000;
            const uint iNUM_TICKS = 65;
            const int iEXPECTED_SECONDS = 65;
            const string sEXPECTED_TIME = "00:01:05";

            // Create the text box the time is shown in, and the timer that writes to it
            TextBox timerTextBox = new TextBox();
            IRNGSessionTimer timer = new RNGSessionTimer(timerTextBox);
            timer.Start();
            SimulateTimer(timer, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Act
            //**************************************************************//

            timer.Stop();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the time the session reached survives it ending
            Assert.AreEqual(iEXPECTED_SECONDS, timer.ElapsedSeconds);
            Assert.AreEqual(sEXPECTED_TIME, timerTextBox.Text);
        }

        /// <summary>
        /// Tests a session that ran its length out is followed by one that gets its length over again,
        /// rather than one that is already past it
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ElapsedSeconds_SecondSessionAfterALengthWasReached_StartsBelowThatLengthAgain()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // One minute, which is the shortest length a session can be given
            const int iINTERVAL = 1000;
            const uint iNUM_TICKS = 60;
            const int iLENGTH_SECONDS = 60;

            // Run a session until it has been going for the length it was given, then stop it
            IRNGSessionTimer timer = new RNGSessionTimer();
            timer.Start();
            SimulateTimer(timer, iINTERVAL, iNUM_TICKS);
            bool bFirstReachedIt = (timer.ElapsedSeconds >= iLENGTH_SECONDS);
            timer.Stop();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start a second session with the same length still set
            timer.Start();
            bool bSecondReachedIt = (timer.ElapsedSeconds >= iLENGTH_SECONDS);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the first session ran its length out
            Assert.IsTrue(bFirstReachedIt);

            // Verify the second one has not, so it records rather than stopping on its first reading
            Assert.IsFalse(bSecondReachedIt);
        }

        #endregion
    }
}
