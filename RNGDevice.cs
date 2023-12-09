//*********************************************************************************************************************
// File Name:      RNGDevice.cs
// Description:    Implementation of the RNG device class
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/03 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for an RNG device
    /// </summary>
    internal interface IRNGDevice
    {
        string Description { get; set; }
        int Port { get; set; }
    }

    /// <summary>
    /// Representation of an RNG device
    /// </summary>
    internal class RNGDevice : IRNGDevice
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal RNGDevice() { }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="sDescription">IN - Description for the device</param>
        /// <param name="iPort">IN - Device port number</param>
        internal RNGDevice(string sDescription, int iPort)
        {
            m_sDescription = sDescription;
            m_iPort = iPort;
        }

        /// <summary>
        /// Device description
        /// </summary>
        public string Description { get => m_sDescription; set => m_sDescription = value; }

        /// <summary>
        /// Device port number
        /// </summary>
        public int Port { get => m_iPort; set => m_iPort = value; }

        /// <summary>
        /// The member used for display in combo box list
        /// </summary>
        internal static string DisplayMember { get => "Description"; }

        /// <summary>
        /// The value used in combo box list
        /// </summary>
        internal static string ValueMember { get => "Port"; }

        private string m_sDescription = "";
        private int m_iPort = -1;
    }
}
