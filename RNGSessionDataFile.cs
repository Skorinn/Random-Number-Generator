//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.cs
// Description:    Interface to a Random Number Generator file
//
// Copyright (C) 2023-2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/04 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System.IO;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the Random Number Generator session data file
    /// </summary>
    public interface IRNGSessionDataFile
    {
        bool StartSession(IRNGSessionData sessionData);
        bool WriteDataPoint(IRNGSessionData sessionData);
        bool EndSession(IRNGSessionData sessionData);

        string FilePath { get; set; }
        bool Valid { get; }
        bool SessionInProgress { get; }
    }

    /// <summary>
    /// Represents a Random Number Generator session file
    /// </summary>
    public class RNGSessionDataFile : IRNGSessionDataFile
    {
        #region Constructors

        /// <summary>
        /// Construct with the writer and parent
        /// </summary>
        /// <param name="writer">IN - The XML writer object to use to for the file</param>
        public RNGSessionDataFile(IRNGXMLWriter writer)
        {
            m_Writer = writer;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Starts a new session in the data file
        /// </summary>
        /// <param name="sessionData">IN - The data for the new session</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool StartSession(IRNGSessionData sessionData)
        {
            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid
            if (m_Writer != null)
            {
                // Write the session start
                m_bSessionInProgress = true;
                bStatus = m_Writer.WriteSessionStart(sessionData.SessionTime, sessionData.TargetValue);
            }

            return bStatus;
        }

        /// <summary>
        /// Writes a data point to the file
        /// </summary>
        /// <param name="fDataPoint">IN - The average for the data point to write</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool WriteDataPoint(IRNGSessionData sessionData)
        {
            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid
            if (m_Writer != null)
            {
                // Write the data point
                XMLDataPoint dataPoint = new XMLDataPoint(sessionData.SessionTime, sessionData.CurrentAverage);
                bStatus = m_Writer.WriteDataPoint(dataPoint);
            }

            return bStatus;
        }

        /// <summary>
        /// Writes the session end and closes the file
        /// </summary>
        /// <param name="sessionData">IN - The data for the session</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool EndSession(IRNGSessionData sessionData)
        {
            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid
            if (m_Writer != null)
            {
                // Write the session end
                m_bSessionInProgress = false;
                bStatus = m_Writer.WriteSessionEnd(sessionData.SessionTime);
            }

            return bStatus;
        }

        #endregion
        #region Properties

        /// <summary>
        /// File path for this file
        /// </summary>
        public string FilePath { get => m_Writer.FilePath; set => m_Writer.FilePath = value; }

        /// <summary>
        /// Checks the a valid file path has been set
        /// </summary>
        public bool Valid
        {
            get
            {
                // Default to valid
                bool bValid = true;

                // Check if a file info object can be created from the writer's file property
                try
                {
                    FileInfo file = new FileInfo(m_Writer.FilePath);
                }
                catch
                {
                    // File is not valid
                    bValid = false;
                }

                return bValid;
            }
        }

        /// <summary>
        /// Specifies if a session is in progress (start has been written but not end)
        /// </summary>
        public bool SessionInProgress { get => m_bSessionInProgress; }

        #endregion
        #region Data Members

        private IRNGXMLWriter m_Writer = null;
        private bool m_bSessionInProgress = false;

        #endregion
    }
}
