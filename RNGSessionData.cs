//*********************************************************************************************************************
// File Name:      RNGSessionData.cs
// Description:    Representation of the data from a Random Number Generator session
//
// Copyright (C) 2023-2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/04 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the RNG session data
    /// </summary>
    public interface IRNGSessionData
    {
        double CurrentAverage { get; }
        ConcurrentQueue<double> DataPoints { get; set; }
        int DataWindowSize { get; set; }
        string FilePath { get; set; }
        bool InProgress { get; }
        double MaxPoint { get; }
        double MinPoint { get; }
        string SessionTime { get; }
        bool Simulated { get; set; }
        int TargetValue { get; set; }
        IRNGSessionTimer Timer { get; }
        uint WriteFileInterval { get; }

        bool AddDataPoint(double fDataPoint);
        void EndSession();
        void PauseSession();
        void Reset();
        void ResumeSession();
        bool StartSession();
        bool WritePendingData(bool bFlush = false);
    }

    /// <summary>
    /// Representation of the data from a Random Number Generator session
    /// </summary>
    public class RNGSessionData : IRNGSessionData
    {
        #region Type definitions
        public enum PossibleTargetsIndex // Indexes for the targets array
        {
            None = 0,
            Zero = 1,
            One = 2,
            TARGETS_SIZE // Keep at end
        }
        #endregion
        #region Constructors

        /// <summary>
        /// Initializing constructor
        /// <\summary>
        /// <param name="dataFile">IN - File to which to write the data (cannot be null)</param>
        /// <param name="timer">IN - Timer for the session (cannot be null)</param>
        public RNGSessionData(IRNGSessionDataFile dataFile, IRNGSessionTimer timer)
        {
            // Data file object provided cannot be null
            if (null == dataFile)
            {
                throw new ArgumentNullException("Specified data file object cannot be null");
            }

            // Timer object provided cannot be null
            if (null == timer)
            {
                throw new ArgumentNullException("Specified timer object cannot be null");
            }

            m_DataFile = dataFile;
            m_Timer = timer;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Starts a new data session
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        public bool StartSession()
        {
            // End any session currently in progress
            EndSession();

            // Start a new session
            bool bStatus = (null != m_DataFile);
            if (bStatus)
            {
                bStatus = m_DataFile.StartSession(this);
            }

            // Then start the timer
            bStatus = (null != m_Timer);
            if (bStatus)
            {
                m_Timer.Start();
            }

            return bStatus;

        }

        /// <summary>
        /// Pauses the current data session
        /// </summary>
        public void PauseSession()
        {
            // If timer is valid and session is in progress
            if ((null != m_Timer) && FileSessionInProgress)
            {
                // Pause the timer
                m_Timer.Enabled = false;
            }
        }

        /// <summary>
        /// Resumes the current data session
        /// </summary>
        public void ResumeSession()
        {
            // If timer is valid and session is in progress
            if ((null != m_Timer) && FileSessionInProgress)
            {
                // Resume the timer
                m_Timer.Enabled = true;
            }
        }

        /// <summary>
        /// Ends the current data session
        /// </summary>
        public void EndSession()
        {
            // Stop the timer
            if (null != m_Timer)
            {
                m_Timer.Stop();
            }

            // If there is pending data
            if (0 < m_iPendingDataPointCounter)
            {
                // Flush the pending data
                WritePendingData(true);
            }

            // Close out any session in progress
            if ((null != m_DataFile) && FileSessionInProgress)
            {
                m_DataFile.EndSession();
            }
        }

        /// <summary>
        /// Adds a new data point to the data sets
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point</param>
        /// <returns>true, if successful; otherwise false</returns>
        public bool AddDataPoint(double fDataPoint)
        {
            // Check if the data set is full
            if (m_DataPoints.Count >= m_iDataWindowSize)
            {
                double fResult; // unused out parameter
                m_DataPoints.TryDequeue(out fResult);
            }

            // Add the point to the data set
            m_DataPoints.Enqueue(fDataPoint);

            // Update the current average
            Interlocked.Exchange(ref m_fCurrentAverage, m_DataPoints.Average());

            // Record a new data point exists and Write the data to the file (if interval reached)
            ++m_iPendingDataPointCounter;
            bool bStatus = WritePendingData();

            return bStatus;
        }

        /// <summary>
        /// Write a data point to the file if the data interval has been reached
        /// </summary>
        /// <param name="bFlush">IN - Whether to force a write (default = false)</param>
        /// <returns>true, if successful; otherwise false</returns>
        public bool WritePendingData(bool bFlush = false)
        {
            // Default to true as the call is successful if nothings needs to be done
            bool bStatus = true;

            // If flushing and unwritten data exists
            bool bFlushUnwritten = (bFlush && (0 < m_iPendingDataPointCounter));

            // if the data point interval has been reached
            bool bWritePending = (WRITE_FILE_INTERVAL <= m_iPendingDataPointCounter);

            // Write the data point if either condition is met
            if (bFlushUnwritten || bWritePending)
            {
                // Write the data point and reset the counter
                XMLDataPoint dataPoint = new XMLDataPoint(SessionTime, CurrentAverage);
                bStatus = (m_DataFile != null);
                if (bStatus)
                {
                    bStatus = m_DataFile.WriteDataPoint(dataPoint);
                }

                m_iPendingDataPointCounter = 0;
            }

            return bStatus;
        }

        /// <summary>
        /// Resets the data to start a a new session
        /// </summary>
        public void Reset()
        {
            // Reset the timer
            if (null != m_Timer)
            {
                m_Timer.Reset();
            }

            // Clear cannot be done atomically so just create new queues
            m_DataPoints = new ConcurrentQueue<double>();

            // Clear the set target value and average
            m_iTargetValue = TargetValues.NO_VALUE_SET;
            Interlocked.Exchange(ref m_fCurrentAverage, 0.0);
        }

        #endregion
        #region Properties

        /// <summary>
        /// The current average of the data points (read-only)
        /// </summary>
        public double CurrentAverage { get => m_fCurrentAverage; }

        /// <summary>
        /// Full data point set
        /// </summary>
        public ConcurrentQueue<double> DataPoints { get => m_DataPoints; set => m_DataPoints = value; }

        /// <summary>
        /// Maximum size for the data and average sets held in memory
        /// </summary>
        public int DataWindowSize { get => m_iDataWindowSize; set => Interlocked.Exchange(ref m_iDataWindowSize, value); }

        /// <summary>
        /// The path to the session data file (empty string if no file object set)
        /// </summary>
        public string FilePath
        {
            get
            {
                return (null == m_DataFile) ? "" : m_DataFile.FilePath;
            }

            set
            {
                if (null != m_DataFile)
                {
                    m_DataFile.FilePath = value;
                }
            }
        }

        /// <summary>
        /// Whether or not the session is currently in progress (read-only)
        /// </summary>
        public bool InProgress { get => (null == m_Timer) ? false : m_Timer.InProgress; }

        /// <summary>
        /// Gets the maximum data value (read-only)
        /// </summary>
        public double MaxPoint { get => (null == m_DataPoints) ? int.MaxValue : m_DataPoints.Max(); }

        /// <summary>
        /// Gets the minimum data value (read-only)
        /// </summary>
        public double MinPoint { get => (null == m_DataPoints) ? int.MinValue : m_DataPoints.Min(); }

        /// <summary>
        /// Formated session time string (read-only)
        /// </summary>
        public string SessionTime { get => (null == m_Timer) ? "" : m_Timer.SessionTime; }

        /// <summary>
        /// Whether the data is simulated or from a real RNG device
        /// </summary>
        public bool Simulated
        {
            get
            {
                // This will compare the value with TRUE. If it is equal, it returns TRUE. It sets the value to TRUE,
                // but it already is TRUE, so that has no effect.
                int iCurrentValue = Interlocked.CompareExchange(ref m_iSimulated, m_iTRUE, m_iTRUE);
                return (iCurrentValue == m_iTRUE);
            }

            set => Interlocked.Exchange(ref m_iSimulated, (value ? m_iTRUE : m_iFALSE));
        }

        /// <summary>
        /// The selected target value for the session. Ends any session in progress.
        /// </summary>
        public int TargetValue
        {
            get => m_iTargetValue;
            set
            {
                // End any session currently in progress
                EndSession();

                // Record the change
                Interlocked.Exchange(ref m_iTargetValue, value);
            }
        }

        /// <summary>
        /// The timer object for the data session (read-only)
        /// </summary>
        public IRNGSessionTimer Timer { get => m_Timer; }

        /// <summary>
        /// Interval at which data points should be written to the data file (read-only)
        /// </summary>
        public uint WriteFileInterval { get => WRITE_FILE_INTERVAL; }

        /// <summary>
        /// Whether or not a session is open in the cooresponding data file (read-only)
        /// </summary>
        private bool FileSessionInProgress { get => (null == m_DataFile) ? false : m_DataFile.SessionInProgress; }

        #endregion
        #region Constants

        /// <summary>
        /// Interval at which data should be record to the file (if used)
        /// </summary>
        private const uint WRITE_FILE_INTERVAL = 1000;

        #endregion
        #region Data Members

        // RNG Data
        private double m_fCurrentAverage = 0.0;
        private ConcurrentQueue<double> m_DataPoints = new ConcurrentQueue<double>();
        private int m_iDataWindowSize = 4096;

        // Target values
        private int m_iTargetValue = TargetValues.NO_VALUE_SET;

        // Whether RNG data is simulated or real
        private int m_iSimulated = m_iFALSE;
        private const int m_iTRUE = 1;
        private const int m_iFALSE = 0;

        // File and trackers for recording results to the file
        private uint m_iPendingDataPointCounter = 0;
        private IRNGSessionDataFile m_DataFile = null;

        // Session timer
        private IRNGSessionTimer m_Timer = null;

        #endregion
    }
}
