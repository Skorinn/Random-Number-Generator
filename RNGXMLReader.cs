//*********************************************************************************************************************
// File Name:      RNGXMLReader.cs
// Description:    Handles reading RNG data from an XML file
//
// Copyright (C) 2023-2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/12/18 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Keep the data recovered from a file that was not closed properly
//*********************************************************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Reads RNG data from an XML file
    /// </summary>
    public class RNGXMLReader : IRNGSessionFileReader, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGXMLReader()
        {
            DefaultReaderSettings();
        }

        /// <summary>
        /// Construct the reader with the specified file path
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to read</param>
        public RNGXMLReader(string sFilePath)
        {
            m_sFilePath = sFilePath;
            DefaultReaderSettings();
        }

        /// <summary>
        /// Construct the reader with the specified settings
        /// </summary>
        /// <param name="readerSettings">IN - Settings for the XML reader</param>
        public RNGXMLReader(XmlReaderSettings readerSettings)
        {
            m_Settings = readerSettings;
        }

        /// <summary>
        /// Construct the reader with the specified file path and settings
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to read</param>
        /// <param name="readerSettings">IN - Settings for the XML reader</param>
        public RNGXMLReader(string sFilePath, XmlReaderSettings readerSettings)
        {
            m_sFilePath = sFilePath;
            m_Settings = readerSettings;
        }

        /// <summary>
        /// Destructor. Ensures the reader is closed.
        /// </summary>
        ~RNGXMLReader()
        {
            Dispose(false);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Loads the complete session data from the XML file using windowed batch processing
        /// </summary>
        /// <param name="sessionData">INOUT - The session data object to load into</param>
        /// <param name="uBatchSize">IN - Number of data points to process in each batch (default = 1000)</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool LoadFile(IRNGSessionData sessionData, uint uBatchSize = 1000)
        {
            // Validate parameters
            if (null == sessionData)
            {
                m_sLastError = " Session data object cannot be null.";
                return false;
            }

            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                try
                {
                    // Clear any previous error
                    m_sLastError = "";

                    // Create the reader
                    bStatus = CreateReader();

                    if (bStatus && (null != m_Reader))
                    {
                        // Read and process the session node
                        bool bSimulated = false;
                        int iTargetValue = TargetValues.NO_VALUE_SET;
                        bStatus = ReadSessionNode(out bSimulated, out iTargetValue);

                        if (bStatus)
                        {
                            // Reset session data and set target value and simulation flag
                            sessionData.Reset();
                            sessionData.TargetValue = iTargetValue;
                            sessionData.Simulated = bSimulated;

                            // Establish whether the session was closed before reading the data, so a file
                            // left unfinished by the application stopping can be told apart from a damaged one
                            bool bSessionUnterminated = (false == HasClosingSessionTag());

                            // Load all data points in batches
                            bStatus = ReadDataNodes(sessionData, uBatchSize, bSessionUnterminated);

                            // If successful, set the file path
                            if (bStatus)
                            {
                                sessionData.FilePath = m_sFilePath;
                            }
                        }
                    }
                    else
                    {
                        bool bErrorEmpty = string.IsNullOrEmpty(m_sLastError);
                        if (bErrorEmpty)
                        {
                            m_sLastError = $" Unable to create XML reader for file '{System.IO.Path.GetFileName(m_sFilePath)}'.";
                        }
                    }
                }
                catch (XmlException xmlException)
                {
                    // XML parsing errors - invalid file format
                    m_sLastError = $" XML parsing error in file '{System.IO.Path.GetFileName(m_sFilePath)}': {xmlException.Message}";
                    bStatus = false;
                }
                catch (IOException ioException)
                {
                    // File I/O errors - file access issues
                    m_sLastError = $" File I/O error accessing '{System.IO.Path.GetFileName(m_sFilePath)}': {ioException.Message}";
                    bStatus = false;
                }
                catch (Exception generalException)
                {
                    // Any other unexpected errors
                    m_sLastError = $" Unexpected error loading file '{System.IO.Path.GetFileName(m_sFilePath)}': {generalException.Message}";
                    bStatus = false;
                }
                finally
                {
                    // Ensure reader is closed
                    Close();
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Checks whether the file closes the session it contains. A session file is written as the data is
        /// recorded and the closing tag is only written when the session ends, so a file without one was left
        /// unfinished by the application being stopped rather than being damaged.
        /// </summary>
        /// <returns>true if the closing session tag is present; otherwise, false</returns>
        private bool HasClosingSessionTag()
        {
            bool bClosingTagFound = false;

            try
            {
                // The closing tag is the last thing in the file, so only the end of it has to be read
                using (FileStream fileStream = new FileStream(m_sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    const int iSEARCH_BLOCK_SIZE = 8192;
                    int iBlockSize = (int)Math.Min(fileStream.Length, iSEARCH_BLOCK_SIZE);
                    byte[] block = new byte[iBlockSize];

                    fileStream.Position = fileStream.Length - iBlockSize;
                    int iTotalRead = 0;
                    while (iTotalRead < iBlockSize)
                    {
                        int iBytesRead = fileStream.Read(block, iTotalRead, iBlockSize - iTotalRead);
                        if (0 == iBytesRead)
                        {
                            break;
                        }

                        iTotalRead += iBytesRead;
                    }

                    // The tags are ASCII, so decoding the block cannot split the text being searched for
                    string sBlockText = System.Text.Encoding.UTF8.GetString(block, 0, iTotalRead);
                    bClosingTagFound = sBlockText.Contains($"</{XMLConstants.SESSION_ELEMENT}>");
                }
            }
            catch (Exception)
            {
                // The file cannot be examined, so treat the session as closed and let the parsing report
                // whatever is wrong with it
                bClosingTagFound = true;
            }

            return bClosingTagFound;
        }

        /// <summary>
        /// Reads the Session node and extracts session-level attributes
        /// </summary>
        /// <param name="bSimulated">OUT - Simulated flag from the session</param>
        /// <param name="iTargetValue">OUT - Target value parsed from the session</param>
        /// <returns>true if successful; otherwise, false</returns>
        private bool ReadSessionNode(out bool bSimulated, out int iTargetValue)
        {
            // Initialize output parameters
            bSimulated = false;
            iTargetValue = TargetValues.NO_VALUE_SET;

            // Read to the Session element
            bool bSessionFound = m_Reader.ReadToFollowing(XMLConstants.SESSION_ELEMENT);
            if (bSessionFound)
            {
                // Parse session attributes
                string sSimulatedString = m_Reader.GetAttribute(XMLConstants.SIMULATED_ATTRIBUTE);
                string sTargetString = m_Reader.GetAttribute(XMLConstants.TARGET_ATTRIBUTE);

                // Parse simulated flag if present
                bSimulated = ParseSimulatedValue(sSimulatedString);

                // Parse target value if present
                iTargetValue = ParseTargetValue(sTargetString);

                return true;
            }
            else
            {
                m_sLastError = $" Invalid XML file format: No '{XMLConstants.SESSION_ELEMENT}' element found in file '{System.IO.Path.GetFileName(m_sFilePath)}'.";
                return false;
            }
        }

        /// <summary>
        /// Parses the simulated value from the simulated string attribute
        /// </summary>
        /// <param name="sSimulatedString">IN - Simulated string to parse</param>
        /// <returns>Parsed simulated value or false if parsing fails</returns>
        private bool ParseSimulatedValue(string sSimulatedString)
        {
            bool bSimulated = false;

            bool bSimulatedStringExists = false == string.IsNullOrEmpty(sSimulatedString);
            if (bSimulatedStringExists)
            {
                bool bSimulatedParsed = bool.TryParse(sSimulatedString, out bool bParsedSimulated);
                if (bSimulatedParsed)
                {
                    bSimulated = bParsedSimulated;
                }
            }

            return bSimulated;
        }

        /// <summary>
        /// Parses the target value from the target string attribute
        /// </summary>
        /// <param name="sTargetString">IN - Target string to parse</param>
        /// <returns>Parsed target value or NO_VALUE_SET if parsing fails</returns>
        private int ParseTargetValue(string sTargetString)
        {
            int iTargetValue = TargetValues.NO_VALUE_SET;

            bool bTargetStringExists = false == string.IsNullOrEmpty(sTargetString);
            if (bTargetStringExists)
            {
                bool bTargetParsed = int.TryParse(sTargetString, out int iParsedTarget);
                if (bTargetParsed)
                {
                    iTargetValue = iParsedTarget;
                }
            }

            return iTargetValue;
        }

        /// <summary>
        /// Reads all Data nodes from the XML file using windowed batch processing
        /// </summary>
        /// <param name="sessionData">INOUT - Session data object to load data into</param>
        /// <param name="uBatchSize">IN - Number of data points to process in each batch</param>
        /// <returns>true if successful; otherwise, false</returns>
        private bool ReadDataNodes(IRNGSessionData sessionData, uint uBatchSize, bool bSessionUnterminated)
        {
            bool bStatus = true;

            // Records a file that ends part way through so it can be reported once the data is loaded
            bool bFileIncomplete = false;
            string sIncompleteDetail = string.Empty;

            // Load data points in batches for memory efficiency
            List<double> dataBatch = new List<double>((int)uBatchSize);

            try
            {
                // Use ReadToFollowing to find all Data elements sequentially
                while (m_Reader.ReadToFollowing(XMLConstants.DATA_ELEMENT))
                {
                    // Process the current data node
                    bool bDataProcessed = ProcessSingleDataNode(dataBatch);
                    if (bDataProcessed)
                    {
                        // Process batch when it reaches the specified size
                        if (dataBatch.Count >= uBatchSize)
                        {
                            bool bBatchLoaded = sessionData.LoadDataPointsBatch(dataBatch, uBatchSize);
                            if (false == bBatchLoaded)
                            {
                                m_sLastError = " Failed to load data points batch into session.";
                                bStatus = false;
                                break;
                            }
                            dataBatch.Clear();
                        }
                    }
                }
            }
            catch (XmlException xmlException)
            {
                if (bSessionUnterminated)
                {
                    // The session was never closed, which is what a session file looks like when the
                    // application is stopped while recording. Every data point read up to that point is
                    // complete and is kept, so the file is recovered rather than rejected.
                    bFileIncomplete = true;
                    sIncompleteDetail = xmlException.Message;
                }
                else
                {
                    // The session was closed but the file still does not parse, so it is damaged rather
                    // than simply unfinished and is not something that can be recovered from
                    m_sLastError = $" XML file appears to be incomplete or corrupted. Loaded {sessionData.NumDataPoints} data points successfully before encountering: {xmlException.Message}";
                    bStatus = false;
                }
            }

            // Process any remaining data points in the final batch. This has to include the points read
            // before an incomplete file ended, otherwise recovered data would be discarded here.
            if (bStatus && (dataBatch.Count > 0))
            {
                bStatus = sessionData.LoadDataPointsBatch(dataBatch, (uint)dataBatch.Count);
                if (false == bStatus)
                {
                    m_sLastError = " Failed to load final data points batch into session.";
                }
            }

            // Report an incomplete file as a warning, as the data that was recovered is still usable
            if (bStatus && bFileIncomplete)
            {
                string sFileName = System.IO.Path.GetFileName(m_sFilePath);
                m_sLastError = $" File '{sFileName}' was not closed properly, which happens when the application is" +
                               $" stopped while recording. Recovered {sessionData.NumDataPoints} data points ({sIncompleteDetail})";
            }

            return bStatus;
        }

        /// <summary>
        /// Processes a single Data node and adds the parsed value to the batch
        /// </summary>
        /// <param name="dataBatch">INOUT - Batch list to add the parsed data point to</param>
        /// <returns>true if data point was successfully parsed and added; otherwise, false</returns>
        private bool ProcessSingleDataNode(List<double> dataBatch)
        {
            // Read the inner text of the Data element
            string sDataValue = string.Empty;

            // If we're positioned on a Data element, read its content
            if (m_Reader.NodeType == XmlNodeType.Element && m_Reader.Name == XMLConstants.DATA_ELEMENT)
            {
                // Read the text content of the element
                bool bReadSuccess = m_Reader.Read();
                if (bReadSuccess && m_Reader.NodeType == XmlNodeType.Text)
                {
                    string sElementText = m_Reader.Value;

                    // The value is only taken once the element that holds it has been closed. A file that
                    // the application was stopped part way through writing can end inside a Data element,
                    // and the digits written so far are not the value that was being recorded.
                    bool bEndReadSuccess = m_Reader.Read();
                    bool bElementClosed = (bEndReadSuccess && (XmlNodeType.EndElement == m_Reader.NodeType) &&
                                           (XMLConstants.DATA_ELEMENT == m_Reader.Name));
                    if (bElementClosed)
                    {
                        sDataValue = sElementText;
                    }
                }
            }

            // The value is parsed with the invariant culture to match how it is written, so a file written
            // in one region is read back as the same number in another
            double fDataPoint;
            bool bDataPointParsed = double.TryParse(sDataValue, System.Globalization.NumberStyles.Float,
                                                    System.Globalization.CultureInfo.InvariantCulture, out fDataPoint);
            if (bDataPointParsed)
            {
                dataBatch.Add(fDataPoint);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Closes the XML reader
        /// </summary>
        public void Close()
        {
            if (null != m_Reader)
            {
                m_Reader.Close();
                m_Reader.Dispose();
                m_Reader = null;
            }

            // The reader does not own the stream it was given, so it has to be closed here
            if (null != m_FileStream)
            {
                m_FileStream.Close();
                m_FileStream.Dispose();
                m_FileStream = null;
            }
        }

        /// <summary>
        /// Initializes the reader settings to the defaults
        /// </summary>
        private void DefaultReaderSettings()
        {
            m_Settings = new XmlReaderSettings();
            m_Settings.IgnoreWhitespace = true;
            m_Settings.IgnoreComments = true;
        }

        /// <summary>
        /// Attempts to create the XML reader object
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        private bool CreateReader()
        {
            // Default to success in case the reader is already created
            bool bStatus = true;

            // Only create the reader if it doesn't exist
            if (null == m_Reader)
            {
                try
                {
                    // Validate the file exists and is readable
                    bool bFileExists = File.Exists(m_sFilePath);
                    if (false == bFileExists)
                    {
                        m_sLastError = $" File '{System.IO.Path.GetFileName(m_sFilePath)}' does not exist or cannot be accessed.";
                        return false;
                    }

                    // Attempt to create the reader. The stream is opened here rather than letting the reader
                    // open the path so a file that is currently being recorded to can still be read.
                    m_FileStream = new FileStream(m_sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    m_Reader = XmlReader.Create(m_FileStream, m_Settings);
                    bStatus = (m_Reader != null);
                    
                    if (false == bStatus)
                    {
                        m_sLastError = $" Failed to create XML reader for file '{System.IO.Path.GetFileName(m_sFilePath)}'.";
                    }
                }
                catch (ArgumentException argException)
                {
                    // Path or settings are not valid
                    m_sLastError = $" Invalid file path or XML reader settings for '{System.IO.Path.GetFileName(m_sFilePath)}': {argException.Message}";
                    bStatus = false;
                }
                catch (IOException ioException)
                {
                    // File I/O errors
                    m_sLastError = $" File I/O error accessing '{System.IO.Path.GetFileName(m_sFilePath)}': {ioException.Message}";
                    bStatus = false;
                }
                catch (Exception generalException)
                {
                    // Any other unexpected errors
                    m_sLastError = $" Unexpected error creating XML reader for '{System.IO.Path.GetFileName(m_sFilePath)}': {generalException.Message}";
                    bStatus = false;
                }
            }

            return bStatus;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Path to the XML file
        /// </summary>
        public string FilePath { get => m_sFilePath; set => m_sFilePath = value; }

        /// <summary>
        /// Last error message from XML reading operations
        /// </summary>
        public string LastError { get => m_sLastError; }

        #endregion
        #region Data members

        private string m_sFilePath = "";
        private XmlReader m_Reader = null;
        private XmlReaderSettings m_Settings = null;
        private FileStream m_FileStream = null; // Track the file stream for proper disposal
        private string m_sLastError = "";

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the resources used by the RNGXMLReader
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations
        /// </summary>
        /// <param name="disposing">true if called from Dispose method; false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Discard unused parameters. The reader and the file it holds open are closed whether this is a
            // disposal or a finalization, as closing only on disposal would leave the file held by anything
            // that was not disposed, which is what the destructor is here to prevent.
            _ = disposing;

            try
            {
                Close();
            }
            catch (Exception)
            {
                // Closing can fail while finalizing, where there is nothing left to report it to and an
                // escaping exception would bring the process down, so it is given up on instead
            }
        }

        #endregion
    }
}