//*********************************************************************************************************************
// File Name:      RNGSessionDataFile.cs
// Description:    Interface to a Random Number Generator file
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/04 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomNumberGenerator
{
    class RNGSessionDataFile
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGSessionDataFile() { }

        /// <summary>
        /// Construct with the specified path and parent
        /// </summary>
        /// <param name="sFilePath">IN - Path for this file</param>
        public RNGSessionDataFile(string sFilePath, GeneratorForm parentForm)
        {
            m_sFilePath = sFilePath;
            m_Parent = parentForm;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Parent form for this file
        /// </summary>
        public GeneratorForm Parent { get => m_Parent; set => m_Parent = value; }

        /// <summary>
        /// File path for this file
        /// </summary>
        public string FilePath { get => m_sFilePath; set => m_sFilePath = value; }

        #endregion
        #region Data Members

        private GeneratorForm m_Parent = null;
        private string m_sFilePath = null;

        #endregion
    }
}
