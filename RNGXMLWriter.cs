//*********************************************************************************************************************
// File Name:      RNGXMLWriter.cs
// Description:    Handles writing the RNG data to an XML file
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
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
    internal interface IRNGXMLWriter
    {
        bool WriteSessionStart(int iTargetValue);
        bool WriteDataPoint(double fAverage);
        bool WriteSessionEnd();

        string FilePath { get; set; }
    }

    /// <summary>
    /// Writes RNG data to an XML file
    /// </summary>
    internal class RNGXMLWriter : IRNGXMLWriter
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        internal RNGXMLWriter()
        {
            DefaultWriterSettings();
        }

        /// <summary>
        /// Construct the writer with the specified file path
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to write</param>
        internal RNGXMLWriter(string sFilePath)
        {
            m_sFilePath = sFilePath;
            DefaultWriterSettings();
        }

        /// <summary>
        /// Construct the writer with the specified settings
        /// </summary>
        /// <param name="writerSettings">IN - Settings for the XML writer</param>
        internal RNGXMLWriter(XmlWriterSettings writerSettings)
        {
            m_Settings = writerSettings;
        }

        /// <summary>
        /// Construct the writer with the specified file path and settings
        /// </summary>
        /// <param name="sFilePath">IN - Path for the file to write</param>
        /// <param name="writerSettings">IN - Settings for the XML writer</param>
        internal RNGXMLWriter(string sFilePath, XmlWriterSettings writerSettings)
        {
            m_sFilePath = sFilePath;
            m_Settings = writerSettings;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Opens or creates the file for writing and records the session start tag
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        public bool WriteSessionStart(int iTargetValue)
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
                    m_Writer.WriteAttributeString("TargetValue", iTargetValue.ToString());
                }
            }

            return bStatus;
        }

        /// <summary>
        /// Writes the specified daa point to the file
        /// </summary>
        /// <param name="fAverage">IN - Average value</param>
        /// <returns>true if successful; otherwise false</returns>
        public bool WriteDataPoint(double fAverage)
        {
            // Default status to failure
            bool bStatus = false;

            // Limit access to the file to one thread at a time
            lock (this)
            {
                // Create the data point object
                XMLDataPoint dataPoint = new XMLDataPoint(fAverage);

                // Attempt to write the data point to the file
                bStatus = dataPoint.WriteDataPoint(m_Writer);
            }

            return bStatus;
        }

        /// <summary>
        /// Ends the current session and closes the file
        /// </summary>
        /// <returns>true if successful; otherwise false</returns>
        public bool WriteSessionEnd()
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

    /// <summary>
    /// Represents a data point in the XML file
    /// </summary>
    internal class XMLDataPoint
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal XMLDataPoint() { }

        /// <summary>
        /// Constructs the objet witht the specified data point and average
        /// </summary>
        /// <param name="fAverage">IN - Average value</param>
        internal XMLDataPoint(double fAverage)
        {
            m_fAverage = fAverage;
        }

        /// <summary>
        /// Writes the data point to the XML file
        /// </summary>
        /// <param name="writer">IN - XML writer object</param>
        /// <returns>true if successful; otherwise false</returns>
        internal bool WriteDataPoint(XmlWriter writer)
        {
            // Default status to success
            bool bStatus = true;

            // Attmept to write the data point to the file
            try
            {
                // <DataPoint Value="0.123456789" Average="0.123456789" />
                writer.WriteStartElement("DataPoint");
                writer.WriteAttributeString("Average", m_fAverage.ToString());
                writer.WriteEndElement();
            }
            catch (Exception)
            {
                // Set the status to failure if writing the data point failed
                bStatus = false;
            }

            return bStatus;
        }

        /// <summary>
        /// Average value for the data point
        /// </summary>
        internal double Average { get => m_fAverage; set => m_fAverage = value; }

        private double m_fAverage = 0.0;
    }
}
