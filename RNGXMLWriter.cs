//*********************************************************************************************************************
// File Name:      RNGXMLWriter.cs
// Description:    Handles writing the RNG data to an XML file
//
// Copyright (c) 2023-2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2023/12/06 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Append without rewriting the whole file, allow the file to be read while recording,
//                            and continue a session that was left open by the application stopping
// 2026/09/08 - Mike Pullen - Kept the file the writer is pointed at when a session ends, so a second session
//                            can be recorded into the same file without choosing it again
//*********************************************************************************************************************
using System;
using System.Xml;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Writes RNG data to an XML file
    /// </summary>
    public class RNGXMLWriter : IRNGSessionFileWriter, IDisposable
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
            Dispose(false);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Opens or creates the file for writing and records the session start tag
        /// </summary>
        /// <param name="bSimulated">IN - Whether the session data is simulated or from real device</param>
        /// <param name="iTargetValue">IN - Target value for the session</param>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="InvalidOperationException">Thrown when no file is selected or XML writer is in an invalid state</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during session start</exception>
        public bool WriteSessionStart(bool bSimulated, int iTargetValue)
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
                    throw new InvalidOperationException("No file selected. Please select a data file before starting a session.");
                }

                // Create (or recreate) the writer
                const bool bAPPEND_MODE = false;
                const bool bRECREATE = true;
                bStatus = CreateWriter(bAPPEND_MODE, bRECREATE);

                // If the writer was created, write the session start tag
                if (bStatus && null != m_Writer)
                {
                    try
                    {
                        // A session with no target selected is recorded as having no target rather than
                        // writing out the value used internally to mean nothing has been set yet
                        int iRecordedTarget = (TargetValues.NO_VALUE_SET == iTargetValue) ? m_iNO_TARGET : iTargetValue;

                        // <Session Simulated="true" Target="0" />
                        m_Writer.WriteStartDocument();
                        m_Writer.WriteStartElement(XMLConstants.SESSION_ELEMENT);
                        m_Writer.WriteAttributeString(XMLConstants.SIMULATED_ATTRIBUTE, bSimulated.ToString().ToLower());
                        m_Writer.WriteAttributeString(XMLConstants.TARGET_ATTRIBUTE, iRecordedTarget.ToString());
                        m_Writer.Flush();
                    }
                    catch (InvalidOperationException)
                    {
                        // Writer is in an invalid state
                        throw new InvalidOperationException("XML writer error: Unable to start session. The file may be corrupted or in use.");
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        // File access denied
                        throw new UnauthorizedAccessException("File access denied. Please check file permissions and ensure the file is not open in another application.");
                    }
                    catch (System.IO.IOException ioEx)
                    {
                        // File I/O error
                        throw new System.IO.IOException($"File I/O error: {ioEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Any other writing error
                        throw new Exception($"Unexpected error starting session: {ex.Message}");
                    }
                }
                else
                {
                    // Writer creation failed
                    throw new InvalidOperationException("Unable to create XML writer. Please check the file path and ensure the directory exists.");
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
                throw new ArgumentNullException("Data point cannot be null.");
            }

            // Limit access to the file to one thread at a time
            lock (this)
            {
                try
                {
                    // When appending, the indentation is written directly as the writer indents from the start
                    // of the fragment rather than from the position within the session
                    if (m_bAppendMode)
                    {
                        m_Writer.WriteRaw(m_sAPPEND_INDENT);
                    }

                    // Attempt to write the data point to the file
                    bool bStatus = dataPoint.WriteDataPoint(m_Writer);
                    if (!bStatus)
                    {
                        throw new InvalidOperationException("Failed to write data point to file. The XML writer may be in an invalid state.");
                    }
                    return bStatus;
                }
                catch (InvalidOperationException)
                {
                    // Re-throw with context
                    throw new InvalidOperationException("Error writing data point: XML writer is in an invalid state.");
                }
                catch (System.IO.IOException ioEx)
                {
                    // File I/O error
                    throw new System.IO.IOException($"File I/O error writing data point: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    // Any other writing error
                    throw new Exception($"Unexpected error writing data point: {ex.Message}");
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
                const bool bAPPEND_MODE = false;
                const bool bRECREATE = false;
                bStatus = CreateWriter(bAPPEND_MODE, bRECREATE);

                // If the writer was created, write the session end tag
                if (bStatus && null != m_Writer)
                {
                    try
                    {
                        // Only write the closing session tag if we're NOT in append mode
                        // In append mode, we never wrote a WriteStartElement, so we can't write WriteEndElement
                        if (!m_bAppendMode)
                        {
                            // Close the session tag (matches the WriteStartElement from WriteSessionStart)
                            m_Writer.WriteEndElement();
                            
                            // Close the document
                            m_Writer.WriteEndDocument();
                        }
                        else
                        {
                            // In append mode, we need to write the closing session tag manually
                            // since we never opened it with WriteStartElement
                            m_Writer.WriteRaw($"{m_sAPPEND_NEWLINE}</{XMLConstants.SESSION_ELEMENT}>");
                        }

                        // Close the writer and any associated file stream
                        m_Writer.Flush();
                        m_Writer.Close();
                        m_Writer.Dispose();
                        m_Writer = null; // Ensure clean state for next operation
                        
                        // Dispose of file stream if it exists (append mode)
                        if (null != m_FileStream)
                        {
                            m_FileStream.Close();
                            m_FileStream.Dispose();
                            m_FileStream = null;
                        }
                        
                        // The file the writer is pointed at is kept. Ending a session used to clear it, which
                        // left the writer with nowhere to write and made the next start report that no data
                        // file had been selected. What ends here is the session, not the choice of file.
                        m_bAppendMode = false; // Reset append mode flag
                        bStatus = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Writer is in an invalid state
                        throw new InvalidOperationException("Error ending session: XML writer is in an invalid state.");
                    }
                    catch (System.IO.IOException ioEx)
                    {
                        // File I/O error
                        throw new System.IO.IOException($"File I/O error ending session: {ioEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Any other writing error
                        throw new Exception($"Unexpected error ending session: {ex.Message}");
                    }
                }
                else
                {
                    // Writer creation/access failed
                    throw new InvalidOperationException("Unable to access XML writer for ending session.");
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

                    // Close any existing writer and file stream
                    if (null != m_Writer)
                    {
                        m_Writer.Close();
                        m_Writer.Dispose();
                        m_Writer = null;
                    }
                    
                    if (null != m_FileStream)
                    {
                        m_FileStream.Close();
                        m_FileStream.Dispose();
                        m_FileStream = null;
                    }

                    // For appending to XML files, we need to:
                    // 1. Read the existing file to memory
                    // 2. Remove the closing session tag and document end
                    // 3. Create a new writer that will continue from that point
                    bStatus = PrepareFileForAppending();
                    
                    if (bStatus)
                    {
                        // Create the writer for the modified file using append mode
                        const bool bAPPEND_MODE = true;
                        const bool bRECREATE = false;
                        bStatus = CreateWriter(bAPPEND_MODE, bRECREATE);
                        if (!bStatus)
                        {
                            throw new InvalidOperationException("Unable to create XML writer after preparing file for append.");
                        }
                        
                        // DO NOT write a new Session start element here!
                        // The existing session is already open and we're just appending data to it.
                        // Writing another WriteStartElement would create a duplicate <Session> tag.
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to prepare file for appending. The file may be corrupted or have invalid XML structure.");
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
                    throw new Exception($"Unexpected error preparing file for append: {ex.Message}", ex);
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
                // Only the ends of the file are examined rather than the whole of it, as a session file grows
                // by around 2MB an hour and rewriting all of it to append to it does not scale
                using (System.IO.FileStream fileStream = new System.IO.FileStream(m_sFilePath, System.IO.FileMode.Open,
                                                                                 System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
                {
                    long lFileLength = fileStream.Length;

                    // Three possible session formats have to be handled:
                    // 1. A closed session:      <Session ...>...</Session>
                    // 2. An empty session:      <Session ... />
                    // 3. An unterminated session, left by the application stopping while recording: <Session ...>...

                    // Case 1: read the end of the file and remove the closing tag to continue the session
                    long lEndBlockStart = Math.Max(0, lFileLength - m_iSEARCH_BLOCK_SIZE);
                    byte[] endBlock = ReadFileBlock(fileStream, lEndBlockStart, m_iSEARCH_BLOCK_SIZE);
                    byte[] closingTag = System.Text.Encoding.UTF8.GetBytes($"</{XMLConstants.SESSION_ELEMENT}>");
                    int iClosingIndex = FindLastPattern(endBlock, closingTag);

                    // The application always writes the closing tag last, but a file that has been edited
                    // elsewhere could hold more after it, so the whole file is searched before the session is
                    // treated as one that was never closed
                    if ((0 > iClosingIndex) && (0 < lEndBlockStart))
                    {
                        lEndBlockStart = 0;
                        endBlock = ReadFileBlock(fileStream, 0, (int)Math.Min(lFileLength, m_iMAX_SEARCH_SIZE));
                        iClosingIndex = FindLastPattern(endBlock, closingTag);
                    }

                    if (0 <= iClosingIndex)
                    {
                        // Discard the closing tag and anything after it so data can be written in its place.
                        // The whitespace written in front of the closing tag goes as well, otherwise it is left
                        // behind as a blank line between the existing data and the data appended to it.
                        int iTruncateIndex = iClosingIndex;
                        while ((0 < iTruncateIndex) && IsWhitespace(endBlock[iTruncateIndex - 1]))
                        {
                            --iTruncateIndex;
                        }

                        fileStream.SetLength(lEndBlockStart + iTruncateIndex);
                        return true;
                    }

                    // The remaining cases are identified by the session start tag at the beginning of the file
                    byte[] startBlock = ReadFileBlock(fileStream, 0, m_iSEARCH_BLOCK_SIZE);
                    byte[] sessionTag = System.Text.Encoding.UTF8.GetBytes($"<{XMLConstants.SESSION_ELEMENT}");
                    int iSessionIndex = FindLastPattern(startBlock, sessionTag);
                    if (0 > iSessionIndex)
                    {
                        // No session element at all, so this is not a session file
                        throw new InvalidOperationException($"Invalid XML structure: No {XMLConstants.SESSION_ELEMENT} element found in file or file format is not recognized.");
                    }

                    // Case 2: an empty session closes itself, so the tag is reopened to hold the new data
                    byte[] selfClosingTag = System.Text.Encoding.UTF8.GetBytes(" />");
                    int iSelfClosingIndex = FindLastPattern(startBlock, selfClosingTag);
                    if (iSessionIndex < iSelfClosingIndex)
                    {
                        // Replace the self closing tag with an open one
                        byte[] openTag = System.Text.Encoding.UTF8.GetBytes(">");
                        fileStream.SetLength(iSelfClosingIndex);
                        fileStream.Position = iSelfClosingIndex;
                        fileStream.Write(openTag, 0, openTag.Length);
                        return true;
                    }

                    // Case 3: the session was never closed, so the file is already open for data to be added
                    // to it. Anything after the last complete data point is discarded first, as a file that
                    // the application was stopped part way through writing can end inside a data element,
                    // and appending after that fragment would leave the file malformed.
                    byte[] closingDataTag = System.Text.Encoding.UTF8.GetBytes($"</{XMLConstants.DATA_ELEMENT}>");
                    int iClosingDataIndex = FindLastPattern(endBlock, closingDataTag);
                    if (0 <= iClosingDataIndex)
                    {
                        // Keep everything up to and including the last complete data point
                        fileStream.SetLength(lEndBlockStart + iClosingDataIndex + closingDataTag.Length);
                    }
                    else
                    {
                        // No complete data point was written, so keep only the session tag itself
                        int iSessionTagEnd = FindByte(startBlock, m_byTAG_END, iSessionIndex);
                        if (0 <= iSessionTagEnd)
                        {
                            fileStream.SetLength(iSessionTagEnd + 1);
                        }
                    }

                    return true;
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
                throw new System.IO.IOException($"Error reading or writing file during append preparation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reads a block of the file, of up to the search block size, from the specified position
        /// </summary>
        /// <param name="fileStream">IN - The open file to read from</param>
        /// <param name="lStartPosition">IN - Position in the file to read from</param>
        /// <param name="iMaxSize">IN - Maximum number of bytes to read</param>
        /// <returns>The block that was read, which is shorter than the block size at the end of the file</returns>
        private static byte[] ReadFileBlock(System.IO.FileStream fileStream, long lStartPosition, int iMaxSize)
        {
            // Limit the block to what remains in the file from the start position
            long lRemaining = fileStream.Length - lStartPosition;
            int iBlockSize = (int)Math.Min(lRemaining, iMaxSize);
            byte[] block = new byte[iBlockSize];

            // Read the block, allowing for a read to be satisfied a part at a time
            fileStream.Position = lStartPosition;
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

            return block;
        }

        /// <summary>
        /// Finds the first occurrence of a byte in a block, starting from the specified index
        /// </summary>
        /// <param name="block">IN - The block to search</param>
        /// <param name="value">IN - The byte to search for</param>
        /// <param name="iStartIndex">IN - Index in the block to start searching from</param>
        /// <returns>Index of the first occurrence at or after the start; -1 if the byte is not present</returns>
        private static int FindByte(byte[] block, byte value, int iStartIndex)
        {
            for (int iIndex = Math.Max(0, iStartIndex); iIndex < block.Length; iIndex++)
            {
                if (value == block[iIndex])
                {
                    return iIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks whether a byte is one of the whitespace characters used to lay the file out
        /// </summary>
        /// <param name="value">IN - The byte to check</param>
        /// <returns>true if the byte is whitespace; otherwise, false</returns>
        private static bool IsWhitespace(byte value)
        {
            const byte bySPACE = 0x20;
            const byte byTAB = 0x09;
            const byte byCARRIAGE_RETURN = 0x0D;
            const byte byLINE_FEED = 0x0A;

            return ((bySPACE == value) || (byTAB == value) || (byCARRIAGE_RETURN == value) || (byLINE_FEED == value));
        }

        /// <summary>
        /// Finds the last occurrence of a pattern of bytes in a block.
        /// NOTE: The tags searched for are all ASCII, so the search is done on the bytes rather than on
        /// decoded text to keep the result usable as a position in the file.
        /// </summary>
        /// <param name="block">IN - The block to search</param>
        /// <param name="pattern">IN - The pattern of bytes to search for</param>
        /// <returns>Index of the last occurrence in the block; -1 if the pattern is not present</returns>
        private static int FindLastPattern(byte[] block, byte[] pattern)
        {
            // Walk backwards through the block so the last occurrence is found first
            for (int iIndex = (block.Length - pattern.Length); iIndex >= 0; iIndex--)
            {
                bool bPatternMatched = true;
                for (int iOffset = 0; iOffset < pattern.Length; iOffset++)
                {
                    if (block[iIndex + iOffset] != pattern[iOffset])
                    {
                        bPatternMatched = false;
                        break;
                    }
                }

                if (bPatternMatched)
                {
                    return iIndex;
                }
            }

            return -1;
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
        /// <param name="bAppendMode">IN - True to create writer in append mode (default = false)</param>
        /// <param name="bRecreate">IN - True to recreate the writer if it already exists (default = false)</param>
        /// <returns>true if successful; otherwise, false</returns>
        /// <exception cref="System.ArgumentException">Thrown when file path or settings are invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when the directory does not exist</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        private bool CreateWriter(bool bAppendMode = false, bool bRecreate = false)
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
                        
                        // Also dispose any existing file stream
                        if (null != m_FileStream)
                        {
                            m_FileStream.Close();
                            m_FileStream.Dispose();
                            m_FileStream = null;
                        }
                    }

                    // Validate file path is set
                    if (string.IsNullOrEmpty(m_sFilePath))
                    {
                        throw new InvalidOperationException("File path is not set. Cannot create XML writer without a valid file path.");
                    }

                    // Create writer based on mode (append vs. new file)
                    if (bAppendMode)
                    {
                        // Store original settings values to restore later
                        bool originalOmitDeclaration = m_Settings.OmitXmlDeclaration;
                        ConformanceLevel originalConformanceLevel = m_Settings.ConformanceLevel;
                        bool originalIndent = m_Settings.Indent;

                        try
                        {
                            // Create a FileStream in append mode with proper disposal tracking. Sharing the file
                            // for reading allows a session in progress to be inspected or backed up.
                            m_FileStream = new System.IO.FileStream(m_sFilePath, System.IO.FileMode.Append,
                                                                    System.IO.FileAccess.Write, System.IO.FileShare.Read);

                            // Modify settings for appending (no declaration, fragment mode)
                            m_Settings.OmitXmlDeclaration = true; // Don't write XML declaration when appending
                            m_Settings.ConformanceLevel = ConformanceLevel.Fragment; // Allow fragments for appending

                            // The writer indents from the start of the fragment rather than from the position in
                            // the session, so the indentation of appended data is written directly instead
                            m_Settings.Indent = false;

                            // Create the writer with the file stream
                            m_Writer = XmlWriter.Create(m_FileStream, m_Settings);

                            // Track that we're in append mode
                            m_bAppendMode = true;
                        }
                        catch
                        {
                            // If anything fails, clean up the file stream
                            if (null != m_FileStream)
                            {
                                m_FileStream.Close();
                                m_FileStream.Dispose();
                                m_FileStream = null;
                            }
                            throw;
                        }
                        finally
                        {
                            // Restore original settings
                            m_Settings.OmitXmlDeclaration = originalOmitDeclaration;
                            m_Settings.ConformanceLevel = originalConformanceLevel;
                            m_Settings.Indent = originalIndent;
                        }
                    }
                    else
                    {
                        // Normal mode - create new file or overwrite existing. The stream is created here rather
                        // than letting the writer open the path so the file can be shared for reading, which
                        // allows a session in progress to be inspected or backed up.
                        m_FileStream = new System.IO.FileStream(m_sFilePath, System.IO.FileMode.Create,
                                                                System.IO.FileAccess.Write, System.IO.FileShare.Read);
                        m_Writer = XmlWriter.Create(m_FileStream, m_Settings);

                        // Track that we're NOT in append mode
                        m_bAppendMode = false;
                    }

                    bStatus = (m_Writer != null);
                    
                    if (!bStatus)
                    {
                        throw new InvalidOperationException("Failed to create XML writer for unknown reasons.");
                    }
                }
                catch (System.ArgumentException argEx)
                {
                    // Path or settings are not valid
                    throw new System.ArgumentException($"Invalid file path or XML settings: {argEx.Message}", argEx);
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
                    throw new Exception($"Unexpected error creating XML writer: {ex.Message}", ex);
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Disposes of the XML writer resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method for proper disposal pattern
        /// </summary>
        /// <param name="disposing">IN - True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (m_Writer != null)
                {
                    m_Writer.Close();
                    m_Writer.Dispose();
                    m_Writer = null;
                }
                
                if (m_FileStream != null)
                {
                    m_FileStream.Close();
                    m_FileStream.Dispose();
                    m_FileStream = null;
                }
            }
        }

        #endregion
        #region Properties

        /// <summary>
        /// Path to the XML file
        /// </summary>
        public string FilePath { get => m_sFilePath; set => m_sFilePath = value; }

        #endregion
        #region Constants

        // Size of the blocks read from the ends of the file when preparing it for appending
        private const int m_iSEARCH_BLOCK_SIZE = 8192;

        // Byte that ends an XML tag
        private const byte m_byTAG_END = 0x3E; // >

        // Limit on searching a whole file, which is only reached by a file not written by the application
        private const int m_iMAX_SEARCH_SIZE = 64 * 1024 * 1024;

        // Indentation written before appended data, matching what the writer produces for a new file
        private const string m_sAPPEND_NEWLINE = "\r\n";
        private const string m_sAPPEND_INDENT = m_sAPPEND_NEWLINE + "\t";

        // Target recorded for a session that has no target selected
        private const int m_iNO_TARGET = -1;

        #endregion
        #region Data members

        private string m_sFilePath = "";
        private XmlWriter m_Writer = null;
        private XmlWriterSettings m_Settings = null;
        private bool m_bAppendMode = false; // Track if we're in append mode
        private System.IO.FileStream m_FileStream = null; // Track file stream for proper disposal

        #endregion
    }
}
