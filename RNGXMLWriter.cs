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
using System.Xml;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the RNG XML writer
    /// </summary>
    public interface IRNGXMLWriter
    {
        bool WriteSessionStart(string sStartTime, int iTargetValue);
        bool WriteDataPoint(IXMLDataPoint dataPoint);
        bool WriteSessionEnd(string sEndtime);

        string FilePath { get; set; }
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

        #endregion
        #region Methods

        /// <summary>
        /// Opens or creates the file for writing and records the session start tag
        /// </summary>
        /// <param name="sStartTime">IN - Start time for the session</param>
        /// <param name="iTargetValue">IN - Target value for the session</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool WriteSessionStart(string sStartTime, int iTargetValue)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                // Create (or recreate) the writer
                m_Writer = XmlWriter.Create(m_sFilePath, m_Settings);
                bStatus = (m_Writer != null);

                // If the writer was created, write the session start tag
                if (bStatus)
                {
                    // <Session TargetValue="0" />
                    m_Writer.WriteStartDocument();
                    m_Writer.WriteStartElement("Session");
                    m_Writer.WriteAttributeString("StartTime", sStartTime);
                    m_Writer.WriteAttributeString("TargetValue", iTargetValue.ToString());
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Writes the specified daa point to the file
        /// </summary>
        /// <param name="sSessionTime">IN - Time for the data point</param>
        /// <param name="fDataPoint">IN - Average value</param>
        /// <returns>true if successful; otherwise false</returns>
        public bool WriteDataPoint(IXMLDataPoint dataPoint)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                // Attempt to write the data point to the file
                bStatus = dataPoint.WriteDataPoint(m_Writer);
            }

            return bStatus;
        }

        /// <summary>
        /// Ends the current session and closes the file
        /// </summary>
        /// <param name="sEndtime">IN - End time for the session</param>
        /// <returns>true if successful; otherwise false</returns>
        public bool WriteSessionEnd(string sEndtime)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                bStatus = (m_Writer != null);

                // If the writer was created, write the session start tag
                if (bStatus)
                {
                    // Write the end time tag
                    m_Writer.WriteElementString("EndTime", sEndtime);

                    // Close the session tag
                    m_Writer.WriteEndElement();

                    // Close the document
                    m_Writer.WriteEndDocument();

                    // Close the writer
                    m_Writer.Close();
                }
            }

            return bStatus;
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
