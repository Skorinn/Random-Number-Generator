//*********************************************************************************************************************
// File Name:      RNGSessionData.cs
// Description:    Representation of the data from a Random Number Generator session
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/04 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the RNG session data
    /// </summary>
    internal interface IRNGSessionData
    {
        void Reset();
        void TickSessionTimer(int iInterval);
        void ResetSessionTimings();
        void AddDataPoint(double fDataPoint);


        string SessionTime { get; }
        double CurrentAverage { get; }
        double MaxPoint { get; }
        double MinPoint { get; }
        ConcurrentQueue<double> DataPoints { get; set; }
        int DataWindowSize { get; set; }
        int TargetValue { get; set; }
        bool Simulated { get; set; }
    }

    /// <summary>
    /// Representation of the data from a Random Number Generator session
    /// </summary>
    internal class RNGSessionData : IRNGSessionData
    {
        #region Type definitions
        internal enum PossibleTargetsIndex // Indexes for the targets array
        {
            None = 0,
            Zero = 1,
            One = 2,
            TARGETS_SIZE = 3 // Keep at end
        }
        #endregion
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        internal RNGSessionData() { }

        #endregion
        #region Methods

        /// <summary>
        /// Resets the data to start a a new session
        /// </summary>
        public void Reset()
        {
            // Use of the session time is exclusive
            lock(m_TimerLock)
            {
                // Reset the timer
                m_iSessionMilliseconds = 0;
                m_iSessionSeconds = 0;
                m_iSessionMinutes = 0;
                m_iSessionHours = 0;
            }

            // Clear cannot be done atomically so just create new queues
            m_DataPoints = new ConcurrentQueue<double>();
            m_Averages = new ConcurrentQueue<double>();

            // Clear the set target value and average
            m_iTargetValue = TargetValues.NO_VALUE_SET;
            Interlocked.Exchange(ref m_fCurrentAverage, 0.0);
        }

        /// <summary>
        /// Increments the session counter
        /// </summary>
        /// <param name="iInterval">IN - Amount to add to the timer</param>
        public void TickSessionTimer(int iInterval)
        {
            // Use of the session time is exclusive
            lock(m_TimerLock)
            {
                // Update the millisecond counter then check for rollovers
                m_iSessionMilliseconds += iInterval;
                if (m_iSessionMilliseconds >= 1000)
                {
                    ++m_iSessionSeconds;
                    m_iSessionMilliseconds -= 1000;
                }

                if (m_iSessionSeconds > 60)
                {
                    ++m_iSessionMinutes;
                    m_iSessionSeconds -= 60;
                }

                if (m_iSessionMinutes > 60)
                {
                    ++m_iSessionHours;
                    m_iSessionMinutes -= 60;
                }
            }
        }

        /// <summary>
        /// Resets the session time to 0
        /// </summary>
        public void ResetSessionTimings()
        {
            // Use of the session time is exclusive
            lock(m_TimerLock)
            {
                m_iSessionMilliseconds = 0;
                m_iSessionSeconds = 0;
                m_iSessionMinutes = 0;
                m_iSessionHours = 0;
            }
        }

        /// <summary>
        /// Adds a new data point to the data sets
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point</param>
        public void AddDataPoint(double fDataPoint)
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
        }

        #endregion
        #region Properties

        /// <summary>
        /// Formated session time string
        /// </summary>
        public string SessionTime
        {
            get
            {
                string sTimerText;

                // Use of the session time is exclusive
                lock(m_TimerLock)
                {
                    // Build and return the formatted string representation of the updated session time
                    sTimerText = m_iSessionHours.ToString("00") + ":" + m_iSessionMinutes.ToString("00") + ":" + m_iSessionSeconds.ToString("00");
                }

                return sTimerText;
            }
        }

        /// <summary>
        /// The current average of the data points
        /// </summary>
        public double CurrentAverage { get => m_fCurrentAverage; }

        /// <summary>
        /// Gets the maximum data or average value (whichever is greater)
        /// </summary>
        public double MaxPoint
        {
            get
            {
                double fDataPointsMax = m_DataPoints.Max();
                double fAveragesMax = m_Averages.Max();
                return (fDataPointsMax > fAveragesMax) ? fDataPointsMax : fAveragesMax;
            }
        }

        /// <summary>
        /// Gets the minimum data or average value (whichever is less)
        /// </summary>
        public double MinPoint
        {
            get
            {
                double fDataPointsMin = m_DataPoints.Min();
                double fAveragesMin = m_Averages.Min();
                return (fDataPointsMin > fAveragesMin) ? fDataPointsMin : fAveragesMin;
            }
        }

        /// <summary>
        /// Full data point set
        /// </summary>
        public ConcurrentQueue<double> DataPoints { get => m_DataPoints; set => m_DataPoints = value; }

        /// <summary>
        /// Maximum size for the data and average sets held in memory
        /// </summary>
        public int DataWindowSize { get => m_iDataWindowSize; set => Interlocked.Exchange(ref m_iDataWindowSize, value); }

        /// <summary>
        /// The selected target value for the session
        /// </summary>
        public int TargetValue { get => m_iTargetValue; set => Interlocked.Exchange(ref m_iTargetValue, value); }

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

        #endregion
        #region Data Members

        // Session timer
        private object m_TimerLock = new object();
        private int m_iSessionMilliseconds = 0;
        private int m_iSessionSeconds = 0;
        private int m_iSessionMinutes = 0;
        private int m_iSessionHours = 0;

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

        #endregion
    }
}
