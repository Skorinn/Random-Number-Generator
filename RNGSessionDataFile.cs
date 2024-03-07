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
using System;
using System.IO;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the Random Number Generator session data file
    /// </summary>
    public interface IRNGSessionDataFile
    {
        string FilePath { get; set; }
        bool SessionInProgress { get; set; }

        bool EndSession();
        bool IsValid();
        bool StartSession(IRNGSessionData sessionData);
        bool WriteDataPoint(IXMLDataPoint dataPoint);
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
        /// <param name="writer">IN - The XML writer object to use to for the file (cannot be null)</param>
        public RNGSessionDataFile(IRNGXMLWriter writer)
        {
            // Writer object provided cannot be null
            if (null == writer)
            {
                throw new ArgumentNullException("Specified writer object cannot be null");
            }

            m_Writer = writer;
        }

        /// <summary>
        /// Destructor. Ensures any pending session is ended.
        /// </summary>
        ~RNGSessionDataFile()
        {
            if (m_bSessionInProgress)
            {
                EndSession();
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Starts a new session in the data file
        /// </summary>
        /// <param name="sessionData">IN - The data for the new session (cannot be null)</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool StartSession(IRNGSessionData sessionData)
        {
            // Session data object provided cannot be null
            if (null == sessionData)
            {
                throw new ArgumentNullException("Specified data object cannot be null");
            }

            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid
            bool bValid = IsValid();
            if (bValid)
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
        /// <param name="fDataPoint">IN - The data point object to be written (cannot be null)</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool WriteDataPoint(IXMLDataPoint dataPoint)
        {
            // Data point object provided cannot be null
            if (null == dataPoint)
            {
                throw new ArgumentNullException("Specified data point object cannot be null");
            }

            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid and a session has been started
            bool bValid = IsValid();
            if (bValid && m_bSessionInProgress)
            {
                // Write the data point
                bStatus = m_Writer.WriteDataPoint(dataPoint);
            }

            return bStatus;
        }

        /// <summary>
        /// Writes the session end and closes the file
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        public bool EndSession()
        {
            // Default the status to failure
            bool bStatus = false;

            // Check if the writer is valid
            bool bValid = IsValid();
            if (bValid)
            {
                // Write the session end
                bStatus = m_Writer.WriteSessionEnd();
                m_bSessionInProgress = false;
            }

            return bStatus;
        }

        /// <summary>
        /// Checks that a valid file path has been set
        /// </summary>
        public bool IsValid()
        {
            // Default to valid
            bool bValid = true;

            // Only validate the file once
            if (false == m_bFileValidated)
            {
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

            }
            return bValid;
        }

        #endregion
        #region Properties

        /// <summary>
        /// File path for this file
        /// </summary>
        public string FilePath
        {
            get => (null == m_Writer) ? "" : m_Writer.FilePath;
            set
            {
                if (null != m_Writer)
                {
                    // Set the new path and reset the file validation
                    m_Writer.FilePath = value;
                    m_bFileValidated = false;
                }
            }
        }

        /// <summary>
        /// Specifies if a session is in progress (start has been written but not end)
        /// </summary>
        public bool SessionInProgress { get => m_bSessionInProgress; set => m_bSessionInProgress = value; }

        #endregion
        #region Data Members

        private IRNGXMLWriter m_Writer = null;
        private bool m_bSessionInProgress = false;
        private bool m_bFileValidated = false;

        #endregion
    }
}
