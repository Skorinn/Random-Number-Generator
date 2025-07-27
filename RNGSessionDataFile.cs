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
    /// Interface for the RNG session file writer
    /// </summary>
    public interface IRNGSessionFileWriter
    {
        string FilePath { get; set; }

        bool WriteDataPoint(IXMLDataPoint dataPoint);
        bool WriteSessionEnd();
        bool WriteSessionStart(bool bSimulated, int iTargetValue);
        bool PrepareForAppend(string sFilePath);
    }

    /// <summary>
    /// Interface for the RNG session file reader
    /// </summary>
    public interface IRNGSessionFileReader
    {
        string FilePath { get; set; }
        string LastError { get; }

        bool LoadFile(IRNGSessionData sessionData, uint uBatchSize = 1000);
        void Close();
    }

    /// <summary>
    /// Interface for the Random Number Generator session data file
    /// </summary>
    public interface IRNGSessionDataFile
    {
        string FilePath { get; set; }
        bool SessionInProgress { get; set; }

        bool EndSession();
        bool IsValid();
        bool LoadSession(IRNGSessionData sessionData, string sFilePath);
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
        public RNGSessionDataFile(IRNGSessionFileWriter writer)
        {
            // Writer object provided cannot be null
            if (null == writer)
            {
                throw new ArgumentNullException("Specified writer object cannot be null");
            }

            m_Writer = writer;
        }

        /// <summary>
        /// Construct with the writer and reader
        /// </summary>
        /// <param name="writer">IN - The XML writer object to use for the file (cannot be null)</param>
        /// <param name="reader">IN - The XML reader object to use for the file (can be null)</param>
        public RNGSessionDataFile(IRNGSessionFileWriter writer, IRNGSessionFileReader reader)
        {
            // Writer object provided cannot be null
            if (null == writer)
            {
                throw new ArgumentNullException("Specified writer object cannot be null");
            }

            m_Writer = writer;
            m_Reader = reader;
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
            // If a session is already in progress, there is nothing to do
            if (true == m_bSessionInProgress)
            {
                return true;
            }

            // Session data object provided cannot be null
            if (null == sessionData)
            {
                throw new ArgumentNullException("Specified data object cannot be null");
            }

            // Default the status to failure
            bool bStatus = false;

            try
            {
                // Check if the writer is valid
                bool bValid = IsValid();
                if (bValid)
                {
                    // Write the session start
                    m_bSessionInProgress = true;
                    bStatus = m_Writer.WriteSessionStart(sessionData.Simulated, sessionData.TargetValue);
                }
                else
                {
                    // Invalid writer - likely no file path set
                    throw new InvalidOperationException(" No data file selected. Please select a file before starting a session.");
                }
            }
            catch (InvalidOperationException)
            {
                // Re-throw InvalidOperationException to be handled by calling code
                m_bSessionInProgress = false;
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw UnauthorizedAccessException to be handled by calling code
                m_bSessionInProgress = false;
                throw;
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions to be handled by calling code
                m_bSessionInProgress = false;
                throw;
            }
            catch (Exception)
            {
                // Re-throw other exceptions to be handled by calling code
                m_bSessionInProgress = false;
                throw;
            }

            return bStatus;
        }

        /// <summary>
        /// Writes a data point to the file
        /// </summary>
        /// <param name="dataPoint">IN - The data point to write (cannot be null)</param>
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

            // Check if the writer is valid and a session is in progress
            bool bValid = IsValid();
            if (bValid && m_bSessionInProgress)
            {
                // Write the data point
                bStatus = m_Writer.WriteDataPoint(dataPoint);
            }

            return bStatus;
        }

        /// <summary>
        /// Loads session data from an existing XML file
        /// </summary>
        /// <param name="sessionData">INOUT - The session data object to load into (cannot be null)</param>
        /// <param name="sFilePath">IN - Path to the session file to load (cannot be null or empty)</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool LoadSession(IRNGSessionData sessionData, string sFilePath)
        {
            // Validate parameters
            if (null == sessionData)
            {
                throw new ArgumentNullException("Specified session data object cannot be null");
            }

            if (string.IsNullOrEmpty(sFilePath))
            {
                throw new ArgumentNullException("Specified file path cannot be null or empty");
            }

            bool bStatus = false;

            // Check if we have a reader available
            if (null != m_Reader)
            {
                try
                {
                    // Set the file path and load the session
                    m_Reader.FilePath = sFilePath;
                    bStatus = m_Reader.LoadFile(sessionData);

                    // If loading failed, throw an exception with the detailed error message
                    if (false == bStatus)
                    {
                        string sErrorMessage = m_Reader.LastError;
                        if (string.IsNullOrEmpty(sErrorMessage))
                        {
                            sErrorMessage = $" Unknown error loading file '{Path.GetFileName(sFilePath)}'.";
                        }
                        throw new InvalidDataException(sErrorMessage);
                    }

                    // Update our file path to match the loaded session
                    FilePath = sFilePath;

                    // Prepare the writer for appending to the loaded file
                    PrepareWriterForAppend(sFilePath);
                }
                catch (IOException ioException)
                {
                    // Re-throw IO exceptions to be handled by calling code
                    throw new IOException($" File I/O error accessing '{Path.GetFileName(sFilePath)}': {ioException.Message}", ioException);
                }
                catch (InvalidDataException)
                {
                    // Re-throw InvalidDataException (from XML reader errors) to be handled by calling code
                    throw;
                }
                catch (Exception generalException)
                {
                    // Wrap other exceptions with file context
                    throw new Exception($" Unexpected error loading file '{Path.GetFileName(sFilePath)}': {generalException.Message}", generalException);
                }
            }
            else
            {
                // No reader available - cannot load sessions
                throw new InvalidOperationException(" No XML reader available for loading session data. Use constructor with reader parameter.");
            }

            return bStatus;
        }

        /// <summary>
        /// Prepares the writer for appending data to an existing loaded file
        /// </summary>
        /// <param name="sFilePath">IN - Path to the file to prepare for appending</param>
        private void PrepareWriterForAppend(string sFilePath)
        {
            // Set the session as in progress since we loaded an existing session
            // The file is already open and contains a session start tag
            m_bSessionInProgress = true;
            
            // Configure the writer for append mode with the specific file path
            if (null != m_Writer)
            {
                m_Writer.PrepareForAppend(sFilePath);
            }
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
            // Default to invalid until proven otherwise
            bool bValid = false;

            // Check if the writer exists and has a valid file path
            if (null != m_Writer)
            {
                // Check if a file path is set
                if (!string.IsNullOrEmpty(m_Writer.FilePath))
                {
                    // Only validate the file once
                    if (false == m_bFileValidated)
                    {
                        // Check if a file info object can be created from the writer's file property
                        try
                        {
                            FileInfo file = new FileInfo(m_Writer.FilePath);
                            bValid = true;
                            m_bFileValidated = true;
                        }
                        catch
                        {
                            // File path is not valid
                            bValid = false;
                        }
                    }
                    else
                    {
                        // File was previously validated
                        bValid = true;
                    }
                }
                // No file path set
                else
                {
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

        private IRNGSessionFileWriter m_Writer = null;
        private IRNGSessionFileReader m_Reader = null;
        private bool m_bSessionInProgress = false;
        private bool m_bFileValidated = false;

        #endregion
    }
}
