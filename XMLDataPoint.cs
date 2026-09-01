//*********************************************************************************************************************
// File Name:      XMLDataPoint.cs
// Description:    Represents a data point in the XML file structure
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
    /// Interface for the XML data point
    /// </summary>
    public interface IXMLDataPoint
    {
        double DataPoint { get; set; }
        string SessionTime { get; set; }

        bool WriteDataPoint(XmlWriter writer);
    }

    /// <summary>
    /// Represents a data point in the XML file
    /// </summary>
    public class XMLDataPoint : IXMLDataPoint
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public XMLDataPoint() { }

        /// <summary>
        /// Constructs the point with the time and value
        /// </summary>
        /// <param name="sSessionTime">IN - Time for the data point</param>
        /// <param name="fDataPoint">IN - Average value</param>
        public XMLDataPoint(string sSessionTime, double fDataPoint)
        {
            m_sSessionTime = sSessionTime;
            m_fDataPoint = fDataPoint;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Writes the data point to the XML file
        /// </summary>
        /// <param name="writer">IN - XML writer object</param>
        /// <returns>true if successful; otherwise false</returns>
        public bool WriteDataPoint(XmlWriter writer)
        {
            // Default status to success
            bool bStatus = true;

            // Attempt to write the data point to the file
            try
            {
                // <Data Time="01:30:45">0.123456789</Data>
                // NOTE: The value is written with the invariant culture so the file holds the same text
                // wherever it is written, rather than picking up a decimal separator that a machine in
                // another region would read back as a different number.
                writer.WriteStartElement(XMLConstants.DATA_ELEMENT);
                writer.WriteAttributeString(XMLConstants.TIME_ATTRIBUTE, m_sSessionTime);
                writer.WriteValue(m_fDataPoint.ToString(System.Globalization.CultureInfo.InvariantCulture));
                writer.WriteEndElement();
                writer.Flush();
            }
            catch (Exception)
            {
                // Set the status to failure if writing the data point failed
                bStatus = false;
            }

            return bStatus;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Average value for the data point
        /// </summary>
        public double DataPoint { get => m_fDataPoint; set => m_fDataPoint = value; }

        /// <summary>
        /// Session time for the data point
        /// </summary>
        public string SessionTime { get => m_sSessionTime; set => m_sSessionTime = value; }

        #endregion
        #region Data Members

        private string m_sSessionTime = string.Empty;
        private double m_fDataPoint = 0.0;

        #endregion
    }
}
