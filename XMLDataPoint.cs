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

            // Attmept to write the data point to the file
            try
            {
                // <DataPoint Value="0.123456789" Average="0.123456789" />
                writer.WriteStartElement("Data");
                writer.WriteAttributeString("Time", m_sSessionTime);
                writer.WriteValue(m_fDataPoint.ToString());
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

        private string m_sSessionTime = "";
        private double m_fDataPoint = 0.0;

        #endregion
    }
}
