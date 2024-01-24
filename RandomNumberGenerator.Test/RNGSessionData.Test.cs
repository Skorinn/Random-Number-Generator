//*********************************************************************************************************************
// File Name:      RNGSessionData.Test.cs
// Description:    Unit tests for the RNGSessionData class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 01/20/2024 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the RNGSessionData class
    /// </summary>
    [TestClass]
    public class RNGSessionDataTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGSessionDataTests()
        {
            // Create the simulated data to use for testing
            CreateSimulatedData();
        }

        /// <summary>
        /// Creates simulated data for testing
        /// </summary>
        private void CreateSimulatedData()
        {
            // Create the simulated data based on the data point size
            for (int iPointIndex = 0; m_iSIMULATED_DATA_SIZE > iPointIndex; ++iPointIndex)
            {
                // Create the simulated data point based on sample size
                int iSampleTotal = 0;
                var randomNumberGenerator = new System.Random();
                for (int iSampleIndex = 0; m_iSIMULATED_SAMPLE_SIZE > iSampleIndex; ++iSampleIndex)
                {
                    // Create the simulated sample
                    iSampleTotal += randomNumberGenerator.Next(0, 1);
                }
                double fSamplePoint = (double)iSampleTotal / (double)m_iSIMULATED_SAMPLE_SIZE;

                // Check if the data window has been exceeded
                if (m_iDEFAULT_DATA_WINDOWS_SIZE <= iPointIndex)
                {
                    // Remove the oldest data point
                    m_WindowedSimulatedDataPoints.Dequeue();
                }

                // Add the new sample
                m_SimulatedDataPoints.Enqueue(fSamplePoint);
                m_WindowedSimulatedDataPoints.Enqueue(fSamplePoint);
            }

            // Calculate the simulated average, max, and min
            m_fSimulatedAverage = m_WindowedSimulatedDataPoints.Average();
            m_fSimulatedMax = m_WindowedSimulatedDataPoints.Max();
            m_fSimulatedMin = m_WindowedSimulatedDataPoints.Min();
        }

        /// <summary>
        /// Adds the simulated data points to the session data
        /// </summary>
        /// <param name="sessionData">INOUT - The session data to which to add the data</param>
        private void AddSimulatedDataPoints(IRNGSessionData sessionData)
        {
            // Add the simulated data points
            foreach (double fDataPoint in m_SimulatedDataPoints)
            {
                sessionData.AddDataPoint(fDataPoint);
            }
        }

        /// <summary>
        /// Simulates the timer ticks for the specified interval and number of ticks
        /// <\summary>
        /// <param name="sessionData">INOUT - The session data to which to add the data</param>
        /// <param name="iInterval">IN - The interval for each tick</param>
        /// <param name="iNumTicks">IN - The number of ticks to simulate</param>
        private void SimulateTimer(IRNGSessionData sessionData, int iInterval, uint iNumTicks)
        {
            // Simulate the timer ticks
            for (uint iTickIndex = 0; iNumTicks > iTickIndex; ++iTickIndex)
            {
                sessionData.TickSessionTimer(iInterval);
            }
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

        // Default values for the RNGSessionData class
        private const string m_sDEFAULT_SESSION_TIME = "00:00:00";
        private const double m_fDEFAULT_AVERAGE = 0.0;
        private const int m_iDEFAULT_DATA_POINTS_SIZE = 0;
        private const int m_iDEFAULT_DATA_WINDOWS_SIZE = 4096;
        private const int m_iDEFAULT_TARGET = TargetValues.NO_VALUE_SET;
        private const bool m_bDEFAULT_SIMULATED = false;

        // Simulated data for testing
        private const int m_iSIMULATED_DATA_SIZE = m_iDEFAULT_DATA_WINDOWS_SIZE * 2;
        private const int m_iSIMULATED_SAMPLE_SIZE = 1000;
        private Queue<double> m_SimulatedDataPoints = new Queue<double>();
        private Queue<double> m_WindowedSimulatedDataPoints = new Queue<double>();
        private double m_fSimulatedAverage = 0.0;
        private double m_fSimulatedMax = 0.0;
        private double m_fSimulatedMin = 0.0;

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
        public void Constructor_Default_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Nothing to do. Expected values set as constant members.

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(m_sDEFAULT_SESSION_TIME, sessionData.SessionTime);
            Assert.AreEqual(m_fDEFAULT_AVERAGE, sessionData.CurrentAverage);
            Assert.AreEqual(m_iDEFAULT_DATA_POINTS_SIZE, sessionData.DataPoints.Count);
            Assert.AreEqual(m_iDEFAULT_DATA_WINDOWS_SIZE, sessionData.DataWindowSize);
            Assert.AreEqual(m_iDEFAULT_TARGET, sessionData.TargetValue);
            Assert.AreEqual(m_bDEFAULT_SIMULATED, sessionData.Simulated);
        }

        /// <summary>
        /// Tests the statistics are updated correctly when building a set of data
        /// </summary>
        [TestMethod]
        public void AddDataPoint_BuildDataSet_StatisticsCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Add the simulated data to the object under test
            AddSimulatedDataPoints(sessionData);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the statistics calcluated correctly
            Assert.AreEqual(m_fSimulatedAverage, sessionData.CurrentAverage);
            Assert.AreEqual(m_fSimulatedMax, sessionData.MaxPoint);
            Assert.AreEqual(m_fSimulatedMin, sessionData.MinPoint);
        }

        /// <summary>
        /// Tests the session time is updated correctly using the tick timer
        /// <\summary>
        [TestMethod]
        public void TickSessionTimer_TickInterval_SessionTimeCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The interval, number of tick, and expected result
            const int iINTERVAL = 100;
            const uint iNUM_TICKS = (uint)1e6;
            const string sEXPECTED_RESULT = "27:46:40";

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Tick the session timer
            SimulateTimer(sessionData, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the statistics calcluated correctly
            Assert.AreEqual(sEXPECTED_RESULT, sessionData.SessionTime);
        }

        /// <summary>
        /// Tests the session timer generates an exception if the interval is less than 1
        /// <\summary>
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TickSessionTimer_IntervalLessThanOne_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The interval, number of tick, and expected result
            const int iINTERVAL = 0;
            const uint iNUM_TICKS = (uint)1e6;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Tick the session timer
            SimulateTimer(sessionData, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Nothing to do. Expected exception.
        }

        /// <summary>
        /// Tests the session timer generates an expection if the interval is greater than 1000
        /// <\summary>
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TickSessionTimer_IntervalGreaterThanOneThousand_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The interval, number of tick, and expected result
            const int iINTERVAL = 1001;
            const uint iNUN_TICKS = (uint)1e6;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Tick the session timer
            SimulateTimer(sessionData, iINTERVAL, iNUN_TICKS);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Nothing to do. Expected exception.
        }

        /// <summary>
        /// Tests resetting the session timer
        /// <\summary>
        [TestMethod]
        public void ResetSessionTimings_SessionTimerReset_SessionTimeCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The interval, number of tick, and expected result
            const int iINTERVAL = 100;
            const uint iNUM_TICKS = (uint)1e6;
            const string sEXPECTED_RESULT = "00:00:00";

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            // Tick the session timer
            SimulateTimer(sessionData, iINTERVAL, iNUM_TICKS);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Reset the session timer
            sessionData.ResetSessionTimings();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the statistics calcluated correctly
            Assert.AreEqual(sEXPECTED_RESULT, sessionData.SessionTime);
        }

        /// <summary>
        /// Tests resetting the entire sesssion data object
        /// <\summary>
        [TestMethod]
        public void ResetSessionData_SessionDataReset_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The interval and number of tick for the session timer
            const int iINTERVAL = 100;
            const uint iNUM_TICKS = (uint)1e6;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            // Tick the session timer
            SimulateTimer(sessionData, iINTERVAL, iNUM_TICKS);

            // Add the simulated data tp the session
            AddSimulatedDataPoints(sessionData);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Reset the session
            sessionData.Reset();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the properties are all reset to defaults
            Assert.AreEqual(m_sDEFAULT_SESSION_TIME, sessionData.SessionTime);
            Assert.AreEqual(m_fDEFAULT_AVERAGE, sessionData.CurrentAverage);
            Assert.AreEqual(m_iDEFAULT_DATA_POINTS_SIZE, sessionData.DataPoints.Count);
            Assert.AreEqual(m_iDEFAULT_DATA_WINDOWS_SIZE, sessionData.DataWindowSize);
            Assert.AreEqual(m_iDEFAULT_TARGET, sessionData.TargetValue);
            Assert.AreEqual(m_bDEFAULT_SIMULATED, sessionData.Simulated);
        }

        /// <summary>
        /// Tests setting the DataPoints property
        /// <\summary>
        [TestMethod]
        public void DataPoints_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the data points object to assign
            ConcurrentQueue<double> dataPoints = new ConcurrentQueue<double>();

            // Populate the data points object with the simulated data
            foreach (double fDataPoint in m_WindowedSimulatedDataPoints)
            {
                dataPoints.Enqueue(fDataPoint);
            }

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the data points property
            sessionData.DataPoints = dataPoints;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify each point in the property
            for (int iPointIndex = 0; m_WindowedSimulatedDataPoints.Count > iPointIndex; ++iPointIndex)
            {
                // Verify the point is correct
                double fExpectedDataPoint = m_WindowedSimulatedDataPoints.ElementAt(iPointIndex);
                double fActualDataPoint = sessionData.DataPoints.ElementAt(iPointIndex);
                Assert.AreEqual(fExpectedDataPoint, fActualDataPoint);
            }
        }

        /// <summary>
        /// Tests setting the DataWindowSize property
        /// <\summary>
        [TestMethod]
        public void DataWindowSize_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected data window size to use
            const int iEXPECTED_DATA_WINDOW_SIZE = m_iDEFAULT_DATA_WINDOWS_SIZE / 2;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the data window size property
            sessionData.DataWindowSize = iEXPECTED_DATA_WINDOW_SIZE;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(iEXPECTED_DATA_WINDOW_SIZE, sessionData.DataWindowSize);
        }

        /// <summary>
        /// Tests setting the TargetValue property
        /// <\summary>
        [TestMethod]
        public void TargetValue_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected target value to use
            const int iEXPECTED_TARGET_VALUE = 1;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the target value property
            sessionData.TargetValue = iEXPECTED_TARGET_VALUE;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(iEXPECTED_TARGET_VALUE, sessionData.TargetValue);
        }

        /// <summary>
        /// Tests setting the Simulated property true
        /// <\summary>
        [TestMethod]
        public void Simulated_SetProperty_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected simulated value to use
            const bool bEXPECTED_SIMULATED = true;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the simulated property
            sessionData.Simulated = bEXPECTED_SIMULATED;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(bEXPECTED_SIMULATED, sessionData.Simulated);
        }

        /// <summary>
        /// Tests setting the Simulated property false
        /// <\summary>
        [TestMethod]
        public void Simulated_SetProperty_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected simulated value to use
            const bool bEXPECTED_SIMULATED = false;

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the simulated property
            sessionData.Simulated = bEXPECTED_SIMULATED;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(bEXPECTED_SIMULATED, sessionData.Simulated);
        }

        #endregion
    }
}
