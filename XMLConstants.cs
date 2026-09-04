//*********************************************************************************************************************
// File Name:      XMLConstants.cs
// Description:    Shared constants for XML element and attribute names used by RNG XML classes
//
// Copyright (c) 2025 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2025/01/27 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

namespace RandomNumberGenerator
{
    /// <summary>
    /// Shared constants for XML element and attribute names used throughout the RNG XML processing classes
    /// </summary>
    public static class XMLConstants
    {
        #region XML Element Names

        /// <summary>
        /// Name of the Session XML element
        /// </summary>
        public const string SESSION_ELEMENT = "Session";

        /// <summary>
        /// Name of the Data XML element
        /// </summary>
        public const string DATA_ELEMENT = "Data";

        #endregion
        #region XML Attribute Names

        /// <summary>
        /// Name of the Simulated XML attribute
        /// </summary>
        public const string SIMULATED_ATTRIBUTE = "Simulated";

        /// <summary>
        /// Name of the Target XML attribute
        /// </summary>
        public const string TARGET_ATTRIBUTE = "Target";

        /// <summary>
        /// Name of the Time XML attribute
        /// </summary>
        public const string TIME_ATTRIBUTE = "Time";

        #endregion
    }
}