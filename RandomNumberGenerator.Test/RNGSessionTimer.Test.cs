//*********************************************************************************************************************
// File Name:      RNGSessionTimer.Test.cs
// Description:    Unit tests for the RNGSessionTimer class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
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
        /// <summary>
        /// Simulates the timer ticks for the specified interval and number of ticks
        /// <\summary>
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

        /// <summary>
        /// Tests the Start method sets the Enabled and InProgress properties to true
        /// <\summary>
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
        /// <\summary>
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
        /// <\summary>
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
        /// <\summary>
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
        /// <\summary>
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
        /// <\summary>
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
    }
}
