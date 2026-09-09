//*********************************************************************************************************************
// File Name:      RNGSessionData.Test.cs
// Description:    Unit tests for the RNGSessionData class
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
        private const double m_fDEFAULT_AVERAGE = 0.0;
        private const int m_iDEFAULT_DATA_POINTS_SIZE = 0;
        private const int m_iDEFAULT_DATA_WINDOWS_SIZE = 6144; // Updated to match new data window size
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
        [TestCategory("Component")]
        public void Constructor_Default_Properties()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(m_fDEFAULT_AVERAGE, sessionData.CurrentAverage);
            Assert.AreEqual(m_iDEFAULT_DATA_POINTS_SIZE, sessionData.DataPoints.Count);
            Assert.AreEqual(m_iDEFAULT_DATA_WINDOWS_SIZE, sessionData.DataWindowSize);
            Assert.AreEqual(m_iDEFAULT_TARGET, sessionData.TargetValue);
            Assert.AreEqual(m_bDEFAULT_SIMULATED, sessionData.Simulated);
            // Not checking session time as it is covered in the session timer tests
        }

        /// <summary>
        /// Test the constructor generates an exception when the data file is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullDataFile_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(null, mockSessionTimer.Object);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting exception
        }

        /// <summary>
        /// Test the constructor generates an exception when the session timer is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullSessionTimer_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Expecting exception
        }

        /// <summary>
        /// Tests the statistics are updated correctly when building a set of data
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void AddDataPoint_BuildDataSet_StatisticsCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Add the simulated data to the object under test
            AddSimulatedDataPoints(sessionData);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the number of data points
            int iExpectedDataCount = Math.Min(m_iDEFAULT_DATA_WINDOWS_SIZE, m_iSIMULATED_DATA_SIZE);
            Assert.AreEqual(iExpectedDataCount, sessionData.DataPoints.Count);

            // Verify the correct number of writes occured
            int iExpectedWriteCount = m_iSIMULATED_DATA_SIZE / (int)sessionData.WriteFileInterval;
            mockDataFile.Verify(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>()), Times.Exactly(iExpectedWriteCount));

            // Verify the statistics calcluated correctly
            Assert.AreEqual(m_fSimulatedAverage, sessionData.CurrentAverage);
            Assert.AreEqual(m_fSimulatedMax, sessionData.MaxPoint);
            Assert.AreEqual(m_fSimulatedMin, sessionData.MinPoint);
        }

        /// <summary>
        /// Tests resetting the session
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Reset_SessionDataReset()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.Setup(mock => mock.Reset()).Verifiable();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Add data to the session
            AddSimulatedDataPoints(sessionData);

            // Set a target value
            sessionData.TargetValue = 1;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Reset the session
            sessionData.Reset();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the session time was reset
            mockSessionTimer.Verify(mock => mock.Reset(), Times.Once);

            // Verify the data was reset
            Assert.AreEqual(0, sessionData.DataPoints.Count);

            // Verify the running average was reset
            Assert.AreEqual(0.0, sessionData.CurrentAverage);
        }

        /// <summary>
        /// Tests setting the DataPoints property
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
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

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DataWindowSize_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected data window size to use
            const int iEXPECTED_DATA_WINDOW_SIZE = m_iDEFAULT_DATA_WINDOWS_SIZE / 2;

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void TargetValue_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected target value to use
            const int iEXPECTED_TARGET_VALUE = 1;

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Simulated_SetProperty_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected simulated value to use
            const bool bEXPECTED_SIMULATED = true;

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

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
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Simulated_SetProperty_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // The expected simulated value to use
            const bool bEXPECTED_SIMULATED = false;

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

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
        /// Tests that end session ends the file session and writes data if pending
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_PendingData_FileSessionEnded()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.StartSession(It.IsAny<IRNGSessionData>())).Returns(true);
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable();
            mockDataFile.Setup(mock => mock.EndSession()).Verifiable();
            mockDataFile.Setup(mock => mock.SessionInProgress).Returns(false);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.Setup(mock => mock.Stop()).Verifiable();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Start the session and update the mock for the session in progress property of the data file
            sessionData.StartSession();
            mockDataFile.Setup(mock => mock.SessionInProgress).Returns(true);

            // Create a pending data point
            sessionData.AddDataPoint(0.0);

            // Record the number of data points pending
            uint iPendingDataCount = sessionData.PendingDataPointCounter;

            // Reset the invocations tracker for the data file and timer to clear calls made during the start session
            mockDataFile.Invocations.Clear();
            mockSessionTimer.Invocations.Clear();

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.EndSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the session has ended in the data file
            mockDataFile.Verify(mock => mock.EndSession(), Times.Once);

            // Verify how write was called based on the pending data points
            Moq.Times expectedWriteCalls = (iPendingDataCount > 0 ? Times.Once() : Times.Never());
            mockDataFile.Verify(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>()), expectedWriteCalls);

            // Verify the timer was stoped
            mockSessionTimer.Verify(mock => mock.Stop(), Times.Once);
        }

        /// <summary>
        /// Tests that end session ends the file session but writes no data if none is pending
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void EndSession_NoData_FileSessionEnded()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object and set up as session in progress
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable();
            mockDataFile.Setup(mock => mock.EndSession()).Verifiable();
            mockDataFile.SetupProperty(mock => mock.SessionInProgress, true);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.Setup(mock => mock.Stop()).Verifiable();

            // Create the object under test and start the session
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Start the session and update the mock for the session in progress property of the data file
            sessionData.StartSession();
            mockDataFile.Setup(mock => mock.SessionInProgress).Returns(true);

            // Reset the invocations tracker for the data file and timer to clear calls made during the start session
            mockDataFile.Invocations.Clear();
            mockSessionTimer.Invocations.Clear();

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.EndSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the session has ended in the data file
            mockDataFile.Verify(mock => mock.EndSession(), Times.Once);

            // Verify that write was not called
            mockDataFile.Verify(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>()), Times.Never);

            // Verify the timer was stopped
            mockSessionTimer.Verify(mock => mock.Stop(), Times.Once);
        }

        /// <summary>
        /// Test that PauseSession pauses the session timer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void PauseSession_TimerPaused()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.SetupProperty(mock => mock.SessionInProgress, true);

            // Mock the session timer object and  set up the Timer mock to initially return true for the Enabled property
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupProperty(timer => timer.Enabled, true);

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.PauseSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the session timer was paused
            Assert.IsFalse(mockSessionTimer.Object.Enabled);
        }

        /// <summary>
        /// Tests that ResumeSession resumes the session timer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ResumeSession_TimerResumed()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.SetupProperty(mock => mock.SessionInProgress, true);

            // Mock the session timer object and  set up the Timer mock to initially return true for the Enabled property
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.SetupProperty(timer => timer.Enabled, false);

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.ResumeSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the session timer was resumed
            Assert.IsTrue(mockSessionTimer.Object.Enabled);
        }

        /// <summary>
        /// Tests StartSession starts a data session and the timer
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void StartSession_SessionStarted()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data file and timer
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.StartSession(It.IsAny<IRNGSessionData>())).Returns(true).Verifiable();
            var mockSessionTimer = new Mock<IRNGSessionTimer>();
            mockSessionTimer.Setup(mock => mock.Start()).Verifiable();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.StartSession();

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the session and timer have started
            mockDataFile.Verify(mock => mock.StartSession(It.IsAny<IRNGSessionData>()), Times.Once);
            mockSessionTimer.Verify(mock => mock.Start(), Times.Once);
        }

        /// <summary>
        /// Test WritePendingData writes the data point when true is specified
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WritePendingData_True_DataWritten()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data file and timer
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable();
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Create a pending data point
            sessionData.AddDataPoint(0.0);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.WritePendingData(true);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the data has been written
            mockDataFile.Verify(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>()), Times.Once);
        }

        /// <summary>
        /// Tests WritePendingData does not write if no pending data and false is specified
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void WritePendingData_False_NoDataWritten()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the session data file and timer
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true).Verifiable();
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.WritePendingData(false);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that the data has not been written
            mockDataFile.Verify(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>()), Times.Never);
        }

        /// <summary>
        /// Tests that DataPointAddedCallback is set and invoked when data points are added
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DataPointAddedCallback_SetAndInvoked_CallbackExecuted()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Variables to capture callback parameters
            double capturedDataPoint = 0.0;
            double capturedAverage = 0.0;
            int callbackCount = 0;

            // Set up the callback
            sessionData.DataPointAddedCallback = (dataPoint, average) =>
            {
                capturedDataPoint = dataPoint;
                capturedAverage = average;
                callbackCount++;
            };

            const double fTEST_DATA_POINT = 0.75;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Add a data point which should trigger the callback
            sessionData.AddDataPoint(fTEST_DATA_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the callback was invoked with correct parameters
            Assert.AreEqual(1, callbackCount, "Callback should be invoked once");
            Assert.AreEqual(fTEST_DATA_POINT, capturedDataPoint, "Callback should receive the correct data point");
            Assert.AreEqual(fTEST_DATA_POINT, capturedAverage, "Callback should receive the correct average for single data point");
        }

        /// <summary>
        /// Tests that DataPointAddedCallback is invoked multiple times for multiple data points
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DataPointAddedCallback_MultipleDataPoints_CallbackInvokedMultipleTimes()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Variables to track callback invocations
            int callbackCount = 0;
            List<double> capturedDataPoints = new List<double>();
            List<double> capturedAverages = new List<double>();

            // Set up the callback
            sessionData.DataPointAddedCallback = (dataPoint, average) =>
            {
                capturedDataPoints.Add(dataPoint);
                capturedAverages.Add(average);
                callbackCount++;
            };

            const double fFIRST_DATA_POINT = 0.6;
            const double fSECOND_DATA_POINT = 0.8;
            const double fEXPECTED_AVERAGE = (fFIRST_DATA_POINT + fSECOND_DATA_POINT) / 2.0;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Add multiple data points
            sessionData.AddDataPoint(fFIRST_DATA_POINT);
            sessionData.AddDataPoint(fSECOND_DATA_POINT);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the callback was invoked for each data point
            Assert.AreEqual(2, callbackCount, "Callback should be invoked twice");
            Assert.AreEqual(fFIRST_DATA_POINT, capturedDataPoints[0], "First callback should receive first data point");
            Assert.AreEqual(fSECOND_DATA_POINT, capturedDataPoints[1], "Second callback should receive second data point");
            Assert.AreEqual(fEXPECTED_AVERAGE, capturedAverages[1], 0.000001, "Second callback should receive correct average");
        }

        /// <summary>
        /// Tests that batch loading data points invokes callback for each point
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void LoadDataPointsBatch_CallbackSet_CallbackInvokedForEachPoint()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Variables to track callback invocations
            int callbackCount = 0;
            List<double> capturedDataPoints = new List<double>();

            // Set up the callback
            sessionData.DataPointAddedCallback = (dataPoint, average) =>
            {
                capturedDataPoints.Add(dataPoint);
                callbackCount++;
            };

            // Create test data points
            var testDataPoints = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5 };

            //**************************************************************//
            // Act
            //**************************************************************//

            // Load data points in batch
            sessionData.LoadDataPointsBatch(testDataPoints);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the callback was invoked for each data point
            Assert.AreEqual(testDataPoints.Count, callbackCount, "Callback should be invoked for each data point in batch");
            for (int i = 0; i < testDataPoints.Count; i++)
            {
                Assert.AreEqual(testDataPoints[i], capturedDataPoints[i], $"Callback should receive correct data point at index {i}");
            }
        }

        /// <summary>
        /// Tests that DataPointAddedCallback can be null without causing issues
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DataPointAddedCallback_Null_NoException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file object
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true);

            // Mock the session timer object
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            // Leave DataPointAddedCallback as null (default)
            Assert.IsNull(sessionData.DataPointAddedCallback, "Callback should be null by default");

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Add a data point - should not throw exception even with null callback
            try
            {
                sessionData.AddDataPoint(0.5);
                Assert.IsTrue(true, "AddDataPoint should complete without exception when callback is null");
            }
            catch (Exception ex)
            {
                Assert.Fail($"AddDataPoint should handle null callback gracefully but threw: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests the data extremes report no value when there is no data rather than throwing
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void MaxAndMinPoint_NoDataPoints_ReturnsNaN()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file and timer objects
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test, which starts with no data recorded in it
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);

            //**************************************************************//
            // Act
            //**************************************************************//

            double fMaxPoint = sessionData.MaxPoint;
            double fMinPoint = sessionData.MinPoint;

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(double.IsNaN(fMaxPoint), "MaxPoint should report no value when there is no data");
            Assert.IsTrue(double.IsNaN(fMinPoint), "MinPoint should report no value when there is no data");
        }

        /// <summary>
        /// Tests the data extremes report no value after the session data has been reset
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void MaxAndMinPoint_AfterReset_ReturnsNaN()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Mock the data file and timer objects
            var mockDataFile = new Mock<IRNGSessionDataFile>();
            mockDataFile.Setup(mock => mock.WriteDataPoint(It.IsAny<IXMLDataPoint>())).Returns(true);
            var mockSessionTimer = new Mock<IRNGSessionTimer>();

            // Create the object under test and record some data in it
            RNGSessionData sessionData = new RNGSessionData(mockDataFile.Object, mockSessionTimer.Object);
            sessionData.AddDataPoint(0.25);
            sessionData.AddDataPoint(0.75);

            //**************************************************************//
            // Act
            //**************************************************************//

            sessionData.Reset();

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(double.IsNaN(sessionData.MaxPoint), "MaxPoint should report no value after a reset");
            Assert.IsTrue(double.IsNaN(sessionData.MinPoint), "MinPoint should report no value after a reset");
        }

        #endregion
    }
}
