//*********************************************************************************************************************
// File Name:      RNGSessionTimer.Test.cs
// Description:    Unit tests for the RNGSessionTimer class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 02/03/2024 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Windows.Forms;

namespace RandomNumberGenerator.Test
{
    [TestClass]
    public class RNGSessionTimerTests
    {
        /// <summary>
        /// Tests the Start method sets the Enabled and InProgress properties to true
        /// <\summary>
        [TestMethod]
        public void Start_EnabledandInProgressPropertiesAreTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.TickSessionTimer(It.IsAny<int>()));
            mockSessionData.SetupProperty(mock => mock.SessionTime, "00:00:00");

            // Create the text box for the timer
            TextBox timerTextBox = new TextBox();

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object, timerTextBox);

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
        public void Stop_RunningPropertyIsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.TickSessionTimer(It.IsAny<int>()));
            mockSessionData.SetupProperty(mock => mock.SessionTime, "00:00:00");

            // Create the text box for the timer
            TextBox timerTextBox = new TextBox();

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object, timerTextBox);

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
        /// Tests the Tick method updates the timer text and ticks the data
        /// <\summary>
        [TestMethod]
        public void Tick_DataAndTextAreUpdated()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value
            const string sEXPECTED_VALUE = "00:00:01";

            // Mock the session data and set the session time to the expected value
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.TickSessionTimer(It.IsAny<int>()));
            mockSessionData.SetupProperty(mock => mock.SessionTime, sEXPECTED_VALUE);

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object, timerTextBox);

            //**************************************************************//
            // Act
            //**************************************************************//
            timer.Tick();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the timer text was updated
            Assert.AreEqual(sEXPECTED_VALUE, timerTextBox.Text);

            // Verify the data was updated
            mockSessionData.Verify(mock => mock.TickSessionTimer(It.IsAny<int>()), Times.Once);
        }

        /// <summary>
        /// Tests the Enabled property doesn't affect the InProgress property when set to false
        /// </summary>
        [TestMethod]
        public void Enabled_PropertyDoesNotAffectInProgressProperty()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.TickSessionTimer(It.IsAny<int>()));
            mockSessionData.SetupProperty(mock => mock.SessionTime, "00:00:00");

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object);

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
        /// Verify the Interval property
        /// <\summary>
        [TestMethod]
        public void Interval_PropertyIsSet()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value to a random number
            Random rand = new Random();
            int iExpectedValue = rand.Next();

            // Mock the session data
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object);

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
        public void TimerTextBox_PropertyIsSet()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set the expected value
            const string sEXPECTED_VALUE = "00:00:01";

            // Mock the session data and set the session time to the expected value
            Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
            mockSessionData.Setup(mock => mock.TickSessionTimer(It.IsAny<int>()));
            mockSessionData.SetupProperty(mock => mock.SessionTime, sEXPECTED_VALUE);

            // Create the text box for the timer and set the text
            TextBox timerTextBox = new TextBox();
            timerTextBox.Text = "Initial Value";

            // Create the session timer
            IRNGSessionTimer timer = new RNGSessionTimer(mockSessionData.Object);

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
