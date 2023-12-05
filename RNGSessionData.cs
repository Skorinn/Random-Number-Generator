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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomNumberGenerator
{
    class RNGSessionData
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
        internal void Reset()
        {
            // Reset the timer
            m_iSessionMilliseconds = 0;
            m_iSessionSeconds = 0;
            m_iSessionMinutes = 0;
            m_iSessionHours = 0;

            // Clear the data sets
            m_DataPoints.Clear();
            m_Averages.Clear();

            // Clear the set target value
            m_iTargetValue = null;
        }

        /// <summary>
        /// Increments the session counter
        /// </summary>
        /// <param name="iInterval">IN - Amount to add to the timer</param>
        internal void TickSessionTimer(int iInterval)
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

        /// <summary>
        /// Resets the session time to 0
        /// </summary>
        internal void ResetSessionTimings()
        {
            m_iSessionMilliseconds = 0;
            m_iSessionSeconds = 0;
            m_iSessionMinutes = 0;
            m_iSessionHours = 0;
        }

        /// <summary>
        /// Adds a new data point to the data sets
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point</param>
        internal void AddDataPoint(double fDataPoint)
        {
            // Remove the oldest point if at the window size limit
            if (m_DataPoints.Count >= m_iDataWindowSize)
            {
                m_DataPoints.RemoveAt(0);
            }

            // Add the point to the data set
            m_DataPoints.Add(fDataPoint);

            // Remove the oldest average if at the window size limit
            if (m_Averages.Count >= m_iDataWindowSize)
            {
                m_Averages.RemoveAt(0);
            }

            // Add the new average value
            m_Averages.Add(CurrentAverage);
        }

        #endregion
        #region Properties

        /// <summary>
        /// Formated session time string
        /// </summary>
        internal string SessionTime
        {
            get
            {
                // Build and return the formatted string representation of the updated session time
                string sTimerText = m_iSessionHours.ToString("00") + ":" + m_iSessionMinutes.ToString("00") + ":" + m_iSessionSeconds.ToString("00");
                return sTimerText;
            }
        }
        
        /// <summary>
        /// The current average of the data points
        /// </summary>
        internal double CurrentAverage { get => m_DataPoints.Average(); }

        /// <summary>
        /// Gets the maximum data or average value (whichever is greater)
        /// </summary>
        internal double MaxPoint
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
        internal double MinPoint
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
        internal List<double> DataPoints { get => m_DataPoints; set => m_DataPoints = value; }

        /// <summary>
        /// Average set
        /// </summary>
        internal List<double> Averages { get => m_Averages; set => m_Averages = value; }

        /// <summary>
        /// Maximum size for the data and average sets held in memory
        /// </summary>
        internal int DataWindowSize { get => m_iDataWindowSize; set => m_iDataWindowSize = value; }

        /// <summary>
        /// The selected target value for the session
        /// </summary>
        internal int? TargetValue { get => m_iTargetValue; set => m_iTargetValue = value; }

        /// <summary>
        /// Whether the data is simulated or from a real RNG device
        /// </summary>
        internal bool Simulated { get => m_bSimulated; set => m_bSimulated = value; }

        #endregion
        #region Data Members

        // Session timer
        private int m_iSessionMilliseconds = 0;
        private int m_iSessionSeconds = 0;
        private int m_iSessionMinutes = 0;
        private int m_iSessionHours = 0;

        // RNG Data
        private List<double> m_DataPoints = new List<double>();
        private List<double> m_Averages = new List<double>();
        private int m_iDataWindowSize = 4096;

        // Target values
        private int? m_iTargetValue = null;

        // Whether RNG data is simulated or real
        private bool m_bSimulated = false;

        #endregion
    }
}
