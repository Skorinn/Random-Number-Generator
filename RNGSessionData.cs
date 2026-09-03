//*********************************************************************************************************************
// File Name:      RNGSessionData.cs
// Description:    Representation of the data from a Random Number Generator session
//
// Copyright (c) 2023-2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2023/12/04 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Report no value for the data extremes when there is no data and account for written
//                            data points individually
//*********************************************************************************************************************
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Delegate for data point added callback
    /// </summary>
    /// <param name="fDataPoint">The data point that was added</param>
    /// <param name="fCurrentAverage">The current average after adding the data point</param>
    public delegate void DataPointAddedDelegate(double fDataPoint, double fCurrentAverage);

    /// <summary>
    /// Interface for the RNG session data
    /// </summary>
    public interface IRNGSessionData
    {
        double CurrentAverage { get; }
        int NumDataPoints { get; }
        double MeanDeviation { get; }
        double StandardDeviation { get; }
        ConcurrentQueue<double> DataPoints { get; set; }
        int DataWindowSize { get; set; }
        string FilePath { get; set; }
        string LastError { get; }
        bool InProgress { get; }
        double MaxPoint { get; }
        double MinPoint { get; }
        string SessionTime { get; }
        bool Simulated { get; set; }
        int TargetValue { get; set; }
        IRNGSessionTimer Timer { get; }
        uint WriteFileInterval { get; }

        DataPointAddedDelegate DataPointAddedCallback { get; set; }

        bool AddDataPoint(double fDataPoint);
        void EndSession();
        bool LoadSession(string sFilePath);
        bool LoadDataPointsBatch(IEnumerable<double> dataPoints, uint uMaxCount = 0);
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
        /// </summary>
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
            bool bStatus = false;
            
            try
            {
                // Validate data file exists
                if (null == m_DataFile)
                {
                    throw new InvalidOperationException(" No data file interface available.");
                }

                // Start the file session
                bStatus = m_DataFile.StartSession(this);

                // Then start the timer if file session was successful
                if (bStatus && null != m_Timer)
                {
                    m_Timer.Start();
                }
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException to be handled by calling code
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw UnauthorizedAccessException to be handled by calling code
                throw;
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions to be handled by calling code
                throw;
            }
            catch (Exception)
            {
                // Re-throw other exceptions to be handled by calling code
                throw;
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
            // Record the data point in the data set
            RecordDataPoint(fDataPoint);

            // Record a new data point exists and Write the data to the file (if interval reached)
            ++m_iPendingDataPointCounter;
            bool bStatus = WritePendingData();

            return bStatus;
        }

        /// <summary>
        /// Records a new data point in the data set and updates statistics
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point to record</param>
        private void RecordDataPoint(double fDataPoint)
        {
            // Add the point to the data set
            AddPointToDataSet(fDataPoint);

            // Update the statistics for the data set the point was added to
            UpdateStatistics();

            // Invoke the callback if it's set with the data point and the current running average
            DataPointAddedCallback?.Invoke(fDataPoint, m_fCurrentAverage);
        }

        /// <summary>
        /// Adds a data point to the data set, making room for it if the set is full
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point to add</param>
        private void AddPointToDataSet(double fDataPoint)
        {
            // Since we have to walk the data to calculate standard devication a lock is
            // required, even though ConcurrentQueue is used, to ensure elements are not added
            // or removed while we are evaluating the data
            lock (m_DataLock)
            {
                // Check if the data set is full
                if (m_DataPoints.Count >= m_iDataWindowSize)
                {
                    double fResult; // unused out parameter
                    m_DataPoints.TryDequeue(out fResult);
                }

                // Add the point to the data set
                m_DataPoints.Enqueue(fDataPoint);
            }
        }

        /// <summary>
        /// Write pending data points to the file if the data interval has been reached
        /// </summary>
        /// <param name="bFlush">IN - Whether to force a write (default = false)</param>
        /// <returns>true, if successful; otherwise false</returns>
        public bool WritePendingData(bool bFlush = false)
        {
            // Default to true as the call is successful if nothings needs to be done
            bool bStatus = true;

            try
            {
                // If flushing and unwritten data exists
                bool bFlushUnwritten = (bFlush && (0 < m_iPendingDataPointCounter));

                // if the data point interval has been reached
                bool bWritePending = (WRITE_FILE_INTERVAL <= m_iPendingDataPointCounter);

                // Write the data points if either condition is met
                if (bFlushUnwritten || bWritePending)
                {
                    if (null == m_DataFile)
                    {
                        throw new InvalidOperationException(" No data file available for writing.");
                    }

                    // Write the pending data points using the dedicated helper method
                    bStatus = WriteIndividualDataPoints();
                }
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException to be handled by calling code
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw UnauthorizedAccessException to be handled by calling code
                throw;
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions to be handled by calling code
                throw;
            }
            catch (Exception)
            {
                // Re-throw other exceptions to be handled by calling code
                throw;
            }

            return bStatus;
        }

        /// <summary>
        /// Writes individual data points from the pending data queue to the file
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="InvalidOperationException">Thrown when data extraction or writing fails</exception>
        private bool WriteIndividualDataPoints()
        {
            // Get the last m_iPendingDataPointCounter number of data points from the queue
            const uint uMAX_PENDING_POINTS = 10000; // Safety limit to prevent excessive memory usage
            uint uPointsToWrite = Math.Min(m_iPendingDataPointCounter, uMAX_PENDING_POINTS);
            
            // Extract the data points in a thread-safe manner
            double[] pendingDataPoints = new double[uPointsToWrite];
            bool bDataExtracted = false;
            
            lock (m_DataLock)
            {
                // Ensure we don't try to extract more points than we have
                uint uAvailablePoints = (uint)m_DataPoints.Count;
                uPointsToWrite = Math.Min(uPointsToWrite, uAvailablePoints);
                
                if (uPointsToWrite > 0)
                {
                    // Convert queue to array to access the last N elements
                    double[] allDataPoints = m_DataPoints.ToArray();
                    
                    // Copy the last uPointsToWrite elements
                    int iStartIndex = Math.Max(0, allDataPoints.Length - (int)uPointsToWrite);
                    Array.Copy(allDataPoints, iStartIndex, pendingDataPoints, 0, (int)uPointsToWrite);
                    bDataExtracted = true;
                }
            }

            // Write each pending data point if we successfully extracted data
            bool bStatus = true;
            if (bDataExtracted && uPointsToWrite > 0)
            {
                string sSessionTime = SessionTime;
                
                // Write each individual data point
                for (uint uIndex = 0; uIndex < uPointsToWrite; uIndex++)
                {
                    XMLDataPoint dataPoint = new XMLDataPoint(sSessionTime, pendingDataPoints[uIndex]);

                    bStatus = m_DataFile.WriteDataPoint(dataPoint);

                    if (!bStatus)
                    {
                        throw new InvalidOperationException($" Failed to write data point {uIndex + 1} of {uPointsToWrite} to file.");
                    }

                    // Account for each point as it is written rather than all of them at the end, so a write
                    // that fails part way through does not leave written points to be written a second time
                    if (0 < m_iPendingDataPointCounter)
                    {
                        --m_iPendingDataPointCounter;
                    }
                }
            }
            else if (uPointsToWrite == 0)
            {
                // No points to write, just reset the counter
                m_iPendingDataPointCounter = 0;
            }
            else
            {
                throw new InvalidOperationException(" Failed to extract pending data points for writing.");
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

            // Since we have to walk the data to calculate standard devication a lock is
            // required, even though ConcurrentQueue is used, to ensure elements are not added
            // or removed while we are evaluating the data
            lock (m_DataLock)
            {
                // Clear cannot be done atomically so just create new queues
                m_DataPoints = new ConcurrentQueue<double>();
            }

            // Clear the set target value and fAverage
            m_iTargetValue = TargetValues.NO_VALUE_SET;
            Interlocked.Exchange(ref m_fCurrentAverage, 0.0);
            Interlocked.Exchange(ref m_iNumDataPoints, 0);
            Interlocked.Exchange(ref m_fMeanDeviation, 0.0);
            Interlocked.Exchange(ref m_fStandardDeviation, 0.0);
        }

        /// <summary>
        /// Loads session data from an existing XML file through the interface hierarchy
        /// </summary>
        /// <param name="sFilePath">IN - Path to the session file to load</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool LoadSession(string sFilePath)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(sFilePath))
            {
                return false;
            }

            // Validate the file exists and is readable
            if (false == System.IO.File.Exists(sFilePath))
            {
                return false;
            }

            bool bStatus = false;

            try
            {
                // Reset current session data to prepare for new data
                Reset();

                // Use the data file interface to load the session (maintains loose coupling)
                bStatus = m_DataFile.LoadSession(this, sFilePath);
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions to be handled by calling code
                throw;
            }
            catch (System.IO.InvalidDataException)
            {
                // Re-throw InvalidDataException (from XML reader errors) to be handled by calling code
                throw;
            }
            catch (System.Exception)
            {
                // Re-throw other exceptions to be handled by calling code
                throw;
            }

            return bStatus;
        }

        /// <summary>
        /// Loads a batch of data points into the session
        /// </summary>
        /// <param name="dataPoints">IN - Collection of data points to load</param>
        /// <param name="uMaxCount">IN - Maximum number of points to load from the collection (0 = no limit)</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool LoadDataPointsBatch(IEnumerable<double> dataPoints, uint uMaxCount = 0)
        {
            // Validate parameters
            if (null == dataPoints)
            {
                return false;
            }

            bool bStatus = true;
            uint uProcessedCount = 0;

            try
            {
                // Process each data point in the batch
                foreach (double fDataPoint in dataPoints)
                {
                    // Check if we've reached the maximum count limit
                    if ((uMaxCount > 0) && (uProcessedCount++ >= uMaxCount))
                    {
                        break;
                    }

                    // Record the data point and report it so the chart fills in as the file is read
                    AddPointToDataSet(fDataPoint);
                    DataPointAddedCallback?.Invoke(fDataPoint, m_fCurrentAverage);
                }

                // Update the statistics once for the whole batch rather than for every point in it. Working
                // them out costs a walk of the data set, which holds up to a million points, so doing that
                // per point makes loading a large file take a length of time that grows with the square of
                // the number of points in it. The average reported to the chart while a batch is being read
                // is the one from the batch before it, so the average line steps rather than curves during a
                // load, and it is correct again as soon as the batch is finished.
                UpdateStatistics();
            }
            catch (System.OutOfMemoryException)
            {
                // Handle memory issues during batch processing
                bStatus = false;
            }
            catch (System.Exception)
            {
                // Any other errors during batch processing
                bStatus = false;
            }

            return bStatus;
        }

        /// <summary>
        /// Updates the statistics to describe the data currently held.
        /// NOTE: The average is worked out once and handed to the standard deviation rather than being
        /// worked out again there, as walking the data is the expensive part of this and the data set can
        /// hold over a million points.
        /// </summary>
        private void UpdateStatistics()
        {
            // Since we have to walk the data to calculate the statistics a lock is required, even though
            // ConcurrentQueue is used, to ensure elements are not added or removed while we are evaluating
            double fAverage;
            double fStandardDeviation;
            lock (m_DataLock)
            {
                fAverage = m_DataPoints.Average();
                fStandardDeviation = CalculateStandardDeviation(fAverage);
            }

            // Record the average and the count
            Interlocked.Exchange(ref m_fCurrentAverage, fAverage);
            Interlocked.Exchange(ref m_iNumDataPoints, m_DataPoints.Count);

            // Record the deviation from the statistical mean
            double fMeanDev = Math.Abs(m_fSTATISTICAL_MEAN - fAverage);
            Interlocked.Exchange(ref m_fMeanDeviation, fMeanDev);

            // Record the standard deviation
            Interlocked.Exchange(ref m_fStandardDeviation, fStandardDeviation);
        }

        /// <summary>
        /// Calculates the standard deviation of the data points currently held
        /// </summary>
        /// <param name="fAverage">IN - The average of the data points currently held</param>
        /// <returns>The standard deviation</returns>
        private double CalculateStandardDeviation(double fAverage)
        {
            double fSumOfSquaresOfDifferences = m_DataPoints.Select(val => (val - fAverage) * (val - fAverage)).Sum();
            double fStandardDeviation = Math.Sqrt(fSumOfSquaresOfDifferences / m_DataPoints.Count);
            return fStandardDeviation;
        }

        #endregion
        #region Properties

        /// <summary>
        /// The current fAverage of the data points (read-only)
        /// </summary>
        public double CurrentAverage { get => m_fCurrentAverage; }

        /// <summary>
        /// The current number of data points (read-only)
        /// </summary>
        public int NumDataPoints { get => m_iNumDataPoints; }

        /// <summary>
        /// Deviation from the statistical mean
        /// </summary>
        public double MeanDeviation { get => m_fMeanDeviation; }

        /// <summary>
        /// Standard deviation of the data set
        /// </summary>
        public double StandardDeviation { get => m_fStandardDeviation; }

        /// <summary>
        /// Full data point set
        /// </summary>
        public ConcurrentQueue<double> DataPoints { get => m_DataPoints; set => m_DataPoints = value; }

        /// <summary>
        /// Maximum size for the data and fAverage sets held in memory
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
        /// Description of anything that needs to be reported about the last file that was loaded, such as the
        /// file not having been closed properly. Empty when there is nothing to report (read-only).
        /// </summary>
        public string LastError { get => (null == m_DataFile) ? string.Empty : m_DataFile.LastError; }

        /// <summary>
        /// Whether or not the session is currently in progress (read-only)
        /// </summary>
        public bool InProgress { get => (null == m_Timer) ? false : m_Timer.InProgress; }

        /// <summary>
        /// Gets the maximum data value, or double.NaN when there is no data (read-only)
        /// </summary>
        public double MaxPoint
        {
            get
            {
                // Work from a snapshot so the set cannot be emptied while it is being examined
                double[] dataPoints = DataPointSnapshot;
                return (0 == dataPoints.Length) ? double.NaN : dataPoints.Max();
            }
        }

        /// <summary>
        /// Gets the minimum data value, or double.NaN when there is no data (read-only)
        /// </summary>
        public double MinPoint
        {
            get
            {
                // Work from a snapshot so the set cannot be emptied while it is being examined
                double[] dataPoints = DataPointSnapshot;
                return (0 == dataPoints.Length) ? double.NaN : dataPoints.Min();
            }
        }

        /// <summary>
        /// Gets a snapshot of the current data points (read-only)
        /// </summary>
        private double[] DataPointSnapshot { get => (null == m_DataPoints) ? new double[0] : m_DataPoints.ToArray(); }

        /// <summary>
        /// Formatted session time string (read-only)
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
        /// Callback that is invoked when a data point is added
        /// </summary>
        public DataPointAddedDelegate DataPointAddedCallback { get; set; }

        /// <summary>
        /// Number of data points pending to be written to the file (read-only)
        /// </summary>
        public uint PendingDataPointCounter { get => m_iPendingDataPointCounter; }

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
        /// Number of data points to accumulate before writing to the data file.
        /// NOTE: Every data point is always written; this only controls how many are buffered per
        /// write, so a value of 1 keeps the file complete after every read at the cost of one
        /// write per data point. Raising it batches writes but risks losing that many points if
        /// the application terminates unexpectedly.
        /// </summary>
        private const uint WRITE_FILE_INTERVAL = 1;

        /// <summary>
        /// The statistical mean the data is expected to sit around
        /// </summary>
        private const double m_fSTATISTICAL_MEAN = 0.5;

        #endregion
        #region Data Members

        // RNG Data
        private double m_fCurrentAverage = 0.0;
        private int m_iNumDataPoints = 0;
        private double m_fMeanDeviation = 0.0;
        private double m_fStandardDeviation = 0.0;
        private ConcurrentQueue<double> m_DataPoints = new ConcurrentQueue<double>();
        // Moderately increase data window to compensate for reduced sample rate
        // Since timer is now 10x slower, we can afford some increase here
        private int m_iDataWindowSize = 6144; // Increased from 4096 to 6144 (1.5x increase)

        // Data lock object
        private object m_DataLock = new object();

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
