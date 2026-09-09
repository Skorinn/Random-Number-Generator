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
// 2026/09/08 - Mike Pullen - Stand aside for the system scheme when a high contrast one is in force
//*********************************************************************************************************************

using System.Drawing;
using System.Windows.Forms;

namespace RandomNumberGenerator
{
    /// <summary>
    /// The colours the status bar uses to say how much attention a message needs. They are held here rather
    /// than at the call sites so that every part of the application reports the same severity the same way,
    /// and so that the scheme can be changed in one place.
    /// NOTE: The pairs are chosen to stay readable in a status bar only a line high, where a saturated fill
    /// behind dark text is hard to read. Each is a deeply saturated text colour on a pale tint of itself.
    /// NOTE: Severity is the one thing here with no system colour to take: there is no system green for
    /// success or red for failure. A high contrast scheme is chosen by someone who needs those particular
    /// colours and no others, so under one these stand aside and the wording carries the severity on its
    /// own - every message already says what it is reporting.
    /// </summary>
    public static class StatusPalette
    {
        #region Properties

        /// <summary>
        /// Text colour for a message that reports nothing out of the ordinary (read-only)
        /// </summary>
        public static Color NormalText { get => UiPalette.MutedText; }

        /// <summary>
        /// Background colour for a message that reports nothing out of the ordinary, which leaves the status
        /// bar looking like the rest of the window rather than tinting it permanently (read-only)
        /// </summary>
        public static Color NormalBackground { get => UiPalette.Ground; }

        /// <summary>
        /// Text colour for a message reporting that something completed (read-only)
        /// </summary>
        public static Color SuccessText { get => SeverityText(m_SUCCESS_TEXT); }

        /// <summary>
        /// Background colour for a message reporting that something completed (read-only)
        /// </summary>
        public static Color SuccessBackground { get => SeverityBackground(m_SUCCESS_BACKGROUND); }

        /// <summary>
        /// Text colour for a message that needs raising but does not stop the user (read-only)
        /// </summary>
        public static Color WarningText { get => SeverityText(m_WARNING_TEXT); }

        /// <summary>
        /// Background colour for a message that needs raising but does not stop the user (read-only)
        /// </summary>
        public static Color WarningBackground { get => SeverityBackground(m_WARNING_BACKGROUND); }

        /// <summary>
        /// Text colour for a message reporting work that is still going on (read-only)
        /// </summary>
        public static Color BusyText { get => SeverityText(m_BUSY_TEXT); }

        /// <summary>
        /// Background colour for a message reporting work that is still going on (read-only)
        /// </summary>
        public static Color BusyBackground { get => SeverityBackground(m_BUSY_BACKGROUND); }

        /// <summary>
        /// Text colour for a message reporting a failure (read-only)
        /// </summary>
        public static Color ErrorText { get => SeverityText(m_ERROR_TEXT); }

        /// <summary>
        /// Background colour for a message reporting a failure (read-only)
        /// </summary>
        public static Color ErrorBackground { get => SeverityBackground(m_ERROR_BACKGROUND); }

        #endregion
        #region Methods

        /// <summary>
        /// The text colour to report a severity in, which stands aside for the system scheme when a high
        /// contrast one is in force
        /// </summary>
        /// <param name="designedColor">IN - The colour to use when the ordinary scheme is in force</param>
        /// <returns>The colour to draw the message in</returns>
        private static Color SeverityText(Color designedColor)
        {
            return SystemInformation.HighContrast ? UiPalette.GroundText : designedColor;
        }

        /// <summary>
        /// The background to report a severity on, which stands aside for the system scheme when a high
        /// contrast one is in force
        /// </summary>
        /// <param name="designedColor">IN - The tint to use when the ordinary scheme is in force</param>
        /// <returns>The colour to draw the message on</returns>
        private static Color SeverityBackground(Color designedColor)
        {
            return SystemInformation.HighContrast ? UiPalette.Ground : designedColor;
        }

        #endregion
        #region Data Members

        // The severity pairs used when the ordinary system scheme is in force
        private static readonly Color m_SUCCESS_TEXT = Color.FromArgb(30, 91, 60);
        private static readonly Color m_SUCCESS_BACKGROUND = Color.FromArgb(228, 241, 233);
        private static readonly Color m_WARNING_TEXT = Color.FromArgb(122, 74, 18);
        private static readonly Color m_WARNING_BACKGROUND = Color.FromArgb(251, 240, 220);
        private static readonly Color m_BUSY_TEXT = Color.FromArgb(28, 68, 105);
        private static readonly Color m_BUSY_BACKGROUND = Color.FromArgb(226, 237, 246);
        private static readonly Color m_ERROR_TEXT = Color.FromArgb(140, 29, 20);
        private static readonly Color m_ERROR_BACKGROUND = Color.FromArgb(250, 227, 224);

        #endregion
    }
}
