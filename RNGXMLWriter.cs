//*********************************************************************************************************************
// File Name:      RNGXMLWriter.cs
// Description:    Handles writing the RNG data to an XML file
//
// Copyright (C) 2023-2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/06 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System;
using System.Xml;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the RNG XML writer
    /// </summary>
    public interface IRNGXMLWriter
    {
        string FilePath { get; set; }

        bool WriteDataPoint(IXMLDataPoint dataPoint);
        bool WriteSessionEnd();
        bool WriteSessionStart(string sStartTime, int iTargetValue);
        bool PrepareForAppend(string sFilePath);
    }

    /// <summary>
    /// Writes RNG data to an XML file
    /// </summary>
    public class RNGXMLWriter : IRNGXMLWriter
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGXMLWriter()
        {
            DefaultWriterSettings();
        }

        /// <summary>
        /// Construct the writer with the specified file path
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to write</param>
        public RNGXMLWriter(string sFilePath)
        {
            m_sFilePath = sFilePath;
            DefaultWriterSettings();
        }

        /// <summary>
        /// Construct the writer with the specified settings
        /// </summary>
        /// <param name="writerSettings">IN - Settings for the XML writer</param>
        public RNGXMLWriter(XmlWriterSettings writerSettings)
        {
            m_Settings = writerSettings;
        }

        /// <summary>
        /// Construct the writer with the specified file path and settings
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to write</param>
        /// <param name="writerSettings">IN - Settings for the XML writer</param>
        public RNGXMLWriter(string sFilePath, XmlWriterSettings writerSettings)
        {
            m_sFilePath = sFilePath;
            m_Settings = writerSettings;
        }

        /// <summary>
        /// Destructor. Ensures the writer is closed.
        /// </summary>
        ~RNGXMLWriter()
        {
            if (m_Writer != null)
            {
                m_Writer.Close();
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Opens or creates the file for writing and records the session start tag
        /// </summary>
        /// <param name="sStartTime">IN - Start time for the session</param>
        /// <param name="iTargetValue">IN - Target value for the session</param>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="InvalidOperationException">Thrown when no file is selected or XML writer is in an invalid state</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during session start</exception>
        public bool WriteSessionStart(string sStartTime, int iTargetValue)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                // Validate file path is set before attempting to create writer
                if (string.IsNullOrEmpty(m_sFilePath))
                {
                    // Throw exception for missing file path - this will bubble up to the GUI
                    throw new InvalidOperationException(" No file selected. Please select a data file before starting a session.");
                }

                // Create (or recreate) the writer
                const bool bRECREATE = true;
                bStatus = CreateWriter(bRECREATE);

                // If the writer was created, write the session start tag
                if (bStatus && null != m_Writer)
                {
                    try
                    {
                        // <Session TargetValue="0" />
                        m_Writer.WriteStartDocument();
                        m_Writer.WriteStartElement("Session");
                        m_Writer.WriteAttributeString("Start", sStartTime);
                        m_Writer.WriteAttributeString("Target", iTargetValue.ToString());
                        m_Writer.Flush();
                    }
                    catch (InvalidOperationException)
                    {
                        // Writer is in an invalid state
                        throw new InvalidOperationException(" XML writer error: Unable to start session. The file may be corrupted or in use.");
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        // File access denied
                        throw new UnauthorizedAccessException(" File access denied. Please check file permissions and ensure the file is not open in another application.");
                    }
                    catch (System.IO.IOException ioEx)
                    {
                        // File I/O error
                        throw new System.IO.IOException($" File I/O error: {ioEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Any other writing error
                        throw new Exception($" Unexpected error starting session: {ex.Message}");
                    }
                }
                else
                {
                    // Writer creation failed
                    throw new InvalidOperationException(" Unable to create XML writer. Please check the file path and ensure the directory exists.");
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Writes the specified data point to the file
        /// </summary>
        /// <param name="dataPoint">IN - The XML data point to write (cannot be null)</param>
        /// <returns>true if successful; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when dataPoint parameter is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when XML writer is in an invalid state</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during data point writing</exception>
        public bool WriteDataPoint(IXMLDataPoint dataPoint)
        {
            // Validate the data point
            if (null == dataPoint)
            {
                throw new ArgumentNullException(" Data point cannot be null.");
            }

            // Limit access to the file to one thread at a time
            lock (this)
            {
                try
                {
                    // Attempt to write the data point to the file
                    bool bStatus = dataPoint.WriteDataPoint(m_Writer);
                    if (!bStatus)
                    {
                        throw new InvalidOperationException(" Failed to write data point to file. The XML writer may be in an invalid state.");
                    }
                    return bStatus;
                }
                catch (InvalidOperationException)
                {
                    // Re-throw with context
                    throw new InvalidOperationException(" Error writing data point: XML writer is in an invalid state.");
                }
                catch (System.IO.IOException ioEx)
                {
                    // File I/O error
                    throw new System.IO.IOException($" File I/O error writing data point: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    // Any other writing error
                    throw new Exception($" Unexpected error writing data point: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Ends the current session and closes the file
        /// </summary>
        /// <returns>true if successful; otherwise false</returns>
        /// <exception cref="InvalidOperationException">Thrown when XML writer is in an invalid state or cannot be accessed</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during session end</exception>
        public bool WriteSessionEnd()
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                // Create the writer if it doesn't exist
                bStatus = CreateWriter();

                // If the writer was created, write the session end tag
                if (bStatus && null != m_Writer)
                {
                    try
                    {
                        // Close the session tag
                        m_Writer.WriteEndElement();

                        // Close the document
                        m_Writer.WriteEndDocument();

                        // Close the writer
                        m_Writer.Flush();
                        m_Writer.Close();
                        bStatus = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Writer is in an invalid state
                        throw new InvalidOperationException(" Error ending session: XML writer is in an invalid state.");
                    }
                    catch (System.IO.IOException ioEx)
                    {
                        // File I/O error
                        throw new System.IO.IOException($" File I/O error ending session: {ioEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Any other writing error
                        throw new Exception($" Unexpected error ending session: {ex.Message}");
                    }
                }
                else
                {
                    // Writer creation/access failed
                    throw new InvalidOperationException(" Unable to access XML writer for ending session.");
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Prepares the writer for appending data to an existing XML file
        /// </summary>
        /// <param name="sFilePath">IN - Path to the file to prepare for appending</param>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during file preparation</exception>
        public bool PrepareForAppend(string sFilePath)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                try
                {
                    // Set the file path member
                    m_sFilePath = sFilePath;

                    // Close any existing writer
                    if (null != m_Writer)
                    {
                        m_Writer.Close();
                        m_Writer = null;
                    }

                    // For appending to XML files, we need to:
                    // 1. Read the existing file to memory
                    // 2. Remove the closing session tag
                    // 3. Create a new writer that will continue from that point
                    
                    // This is a simplified approach - in a full implementation, 
                    // you might use XDocument or other XML manipulation techniques
                    bStatus = PrepareFileForAppending();
                    
                    if (bStatus)
                    {
                        // Create the writer for the modified file
                        bStatus = CreateWriter();
                        if (!bStatus)
                        {
                            throw new InvalidOperationException(" Unable to create XML writer after preparing file for append.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(" Unable to prepare file for appending. The file may be corrupted or have invalid XML structure.");
                    }
                }
                catch (System.IO.IOException)
                {
                    // Re-throw IO exceptions to be handled by calling code
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    // Re-throw access exceptions to be handled by calling code
                    throw;
                }
                catch (InvalidOperationException)
                {
                    // Re-throw operation exceptions to be handled by calling code
                    throw;
                }
                catch (Exception ex)
                {
                    // Wrap other exceptions with context
                    throw new Exception($" Unexpected error preparing file for append: {ex.Message}", ex);
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Prepares the XML file for appending by removing the closing session tag
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="InvalidOperationException">Thrown when file has invalid XML structure</exception>
        private bool PrepareFileForAppending()
        {
            try
            {
                // Read the entire file content
                string fileContent = System.IO.File.ReadAllText(m_sFilePath);
                
                // Find and remove the closing session tag and document tag
                int lastSessionEndIndex = fileContent.LastIndexOf("</Session>");
                if (lastSessionEndIndex > 0)
                {
                    // Remove everything after the last data point
                    fileContent = fileContent.Substring(0, lastSessionEndIndex);
                    
                    // Write the modified content back to the file
                    System.IO.File.WriteAllText(m_sFilePath, fileContent);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException(" Invalid XML structure: No closing session tag found in file.");
                }
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions to be handled by calling code
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Re-throw access exceptions to be handled by calling code
                throw;
            }
            catch (Exception ex)
            {
                // Wrap other exceptions as IO errors since this is a file operation
                throw new System.IO.IOException($" Error reading or writing file during append preparation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Initializes the writer settings to the defaults
        /// </summary>
        private void DefaultWriterSettings()
        {
            m_Settings = new XmlWriterSettings();
            m_Settings.Indent = true;
            m_Settings.IndentChars = "\t";
        }

        /// <summary>
        /// Attempts to create the XML writer object
        /// </summary>
        /// <param name="bRecreate">IN - True to recreate the writer if it already exists (default = false)</param>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="System.ArgumentException">Thrown when file path or settings are invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory does not exist</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        private bool CreateWriter(bool bRecreate = false)
        {
            // Default to success in case the writer is already created
            bool bStatus = true;

            // Check if we need to recreate or create the writer
            if (null == m_Writer || bRecreate)
            {
                try
                {
                    // Close and dispose existing writer if recreating
                    if (bRecreate && null != m_Writer)
                    {
                        m_Writer.Close();
                        m_Writer.Dispose();
                        m_Writer = null;
                    }

                    // Validate file path is set
                    if (string.IsNullOrEmpty(m_sFilePath))
                    {
                        throw new InvalidOperationException(" File path is not set. Cannot create XML writer without a valid file path.");
                    }

                    // Attempt to create the writer
                    m_Writer = XmlWriter.Create(m_sFilePath, m_Settings);
                    bStatus = (m_Writer != null);
                    
                    if (!bStatus)
                    {
                        throw new InvalidOperationException(" Failed to create XML writer for unknown reasons.");
                    }
                }
                catch (System.ArgumentException argEx)
                {
                    // Path or settings are not valid
                    throw new System.ArgumentException($" Invalid file path or XML settings: {argEx.Message}", argEx);
                }
                catch (UnauthorizedAccessException)
                {
                    // File access denied - re-throw to be handled by calling code
                    throw;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    // Directory not found - re-throw to be handled by calling code
                    throw;
                }
                catch (System.IO.IOException)
                {
                    // IO error - re-throw to be handled by calling code
                    throw;
                }
                catch (InvalidOperationException)
                {
                    // Operation error - re-throw to be handled by calling code
                    throw;
                }
                catch (Exception ex)
                {
                    // Any other exception - wrap with context
                    throw new Exception($" Unexpected error creating XML writer: {ex.Message}", ex);
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

        #endregion
        #region Data members

        private string m_sFilePath = "";
        private XmlWriter m_Writer = null;
        private XmlWriterSettings m_Settings = null;

        #endregion
    }
}
