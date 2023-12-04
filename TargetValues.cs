//*********************************************************************************************************************
// File Name:      TargetValues.cs
// Description:    Implementation of the target value options
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/02 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Representation of the target value options
    /// </summary>
    internal static class TargetValues
    {
        /// <summary>
        /// Gets the string for the specified value
        /// </summary>
        /// <param name="iTargetValue">IN - The value for which to get the string</param>
        /// <returns>String for value if found; otherwise, null</returns>
        public static string ToString(int iTargetValue)
        {
            // Default the return to null object
            string sResult = null;

            // Attempt to find the specified value
            int iIndex = Array.FindIndex(m_sTargetValues, iCurrentValue => (iCurrentValue == iTargetValue));
            if (0 <= iIndex)
            {
                // Get the result from the string array
                sResult = TargetStrings[iIndex];
            }

            return sResult;
        }

        /// <summary>
        /// Gets the value for the specified string
        /// </summary>
        /// <param name="sTargetValue">IN - The string for which to get the value</param>
        /// <returns>Value for string if found; otherwise, null</returns>
        public static int? ToInt(string sTargetValue)
        {
            // Default the return to null object
            int? iResult = null;

            // Attempt to find the specified string
            int iIndex = Array.FindIndex(TargetStrings, sCurrentValue => (sCurrentValue == sTargetValue));
            if (0 <= iIndex)
            {
                // Get the result from the value array
                iResult = m_sTargetValues[iIndex];
            }

            return iResult;
        }

        /// <summary>
        /// Gets the string at the specified index
        /// </summary>
        /// <param name="iIndex">IN - Index for the string to get</param>
        /// <returns>String if index is valid; otherwise, null</returns>
        public static string GetStringAt(uint iIndex)
        {
            string sResult = null;

            if (TargetStrings.Length > iIndex)
            {
                sResult = TargetStrings[iIndex];
            }

            return sResult;
        }

        /// <summary>
        /// Gets the value at the specified index
        /// </summary>
        /// <param name="iIndex">IN - Index for the value to get</param>
        /// <returns>Value if index is valid; otherwise, null</returns>
        public static int? GetValueAt(uint iIndex)
        {
            int? iResult = null;

            if (m_sTargetValues.Length > iIndex)
            {
                iResult = m_sTargetValues[iIndex];
            }

            return iResult;
        }

        public static string[] TargetStrings { get; } = { "None", "0", "1" }; // string respresentation of possible targets
        private static readonly int[] m_sTargetValues = { -1, 0, 1 }; // Possible target values
    }
}
