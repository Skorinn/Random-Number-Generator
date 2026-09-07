//*********************************************************************************************************************
// File Name:      StatusPalette.cs
// Description:    Colours used to report severity in the status bar
//
// Copyright (c) 2022-2026 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History:
//====================================================================================================================
// 2026/09/07 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using System.Drawing;

namespace RandomNumberGenerator
{
    /// <summary>
    /// The colours the status bar uses to say how much attention a message needs. They are held here rather
    /// than at the call sites so that every part of the application reports the same severity the same way,
    /// and so that the scheme can be changed in one place.
    /// NOTE: The pairs are chosen to stay readable in a status bar only a line high, where a saturated fill
    /// behind dark text is hard to read. Each is a deeply saturated text colour on a pale tint of itself.
    /// </summary>
    public static class StatusPalette
    {
        #region Constants

        /// <summary>
        /// Text colour for a message that reports nothing out of the ordinary
        /// </summary>
        public static readonly Color NormalText = SystemColors.ControlText;

        /// <summary>
        /// Background colour for a message that reports nothing out of the ordinary, which leaves the status
        /// bar looking like the rest of the window rather than tinting it permanently
        /// </summary>
        public static readonly Color NormalBackground = SystemColors.Control;

        /// <summary>
        /// Text colour for a message reporting that something completed
        /// </summary>
        public static readonly Color SuccessText = Color.FromArgb(30, 91, 60);

        /// <summary>
        /// Background colour for a message reporting that something completed
        /// </summary>
        public static readonly Color SuccessBackground = Color.FromArgb(228, 241, 233);

        /// <summary>
        /// Text colour for a message that needs raising but does not stop the user
        /// </summary>
        public static readonly Color WarningText = Color.FromArgb(122, 74, 18);

        /// <summary>
        /// Background colour for a message that needs raising but does not stop the user
        /// </summary>
        public static readonly Color WarningBackground = Color.FromArgb(251, 240, 220);

        /// <summary>
        /// Text colour for a message reporting work that is still going on
        /// </summary>
        public static readonly Color BusyText = Color.FromArgb(28, 68, 105);

        /// <summary>
        /// Background colour for a message reporting work that is still going on
        /// </summary>
        public static readonly Color BusyBackground = Color.FromArgb(226, 237, 246);

        /// <summary>
        /// Text colour for a message reporting a failure
        /// </summary>
        public static readonly Color ErrorText = Color.FromArgb(140, 29, 20);

        /// <summary>
        /// Background colour for a message reporting a failure
        /// </summary>
        public static readonly Color ErrorBackground = Color.FromArgb(250, 227, 224);

        #endregion
    }
}
