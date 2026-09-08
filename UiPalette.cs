//*********************************************************************************************************************
// File Name:      UiPalette.cs
// Description:    Surfaces, lines and text colours the interface is built from
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
    /// The colours the window is built from: the ground it sits on, the cards laid on that ground, the
    /// hairlines between them, and the two weights of text used on them.
    /// NOTE: These are fixed rather than taken from SystemColors. The interface is a set of white cards on a
    /// light ground, and a system colour scheme that inverted one without the other would leave the two
    /// disagreeing. The cost is that a high contrast Windows theme is not followed; a machine that needs one
    /// is the reason to reach for SystemColors here instead.
    /// </summary>
    public static class UiPalette
    {
        #region Constants

        /// <summary>
        /// The ground the cards are laid on, and the colour of the tab pages behind them
        /// </summary>
        public static readonly Color Ground = Color.FromArgb(240, 240, 240);

        /// <summary>
        /// The face of a card: the settings, the readouts, the comparison and the charts each sit on one
        /// </summary>
        public static readonly Color Card = Color.FromArgb(255, 255, 255);

        /// <summary>
        /// The hairline around a card and between the readouts
        /// </summary>
        public static readonly Color Line = Color.FromArgb(200, 205, 211);

        /// <summary>
        /// Text carrying a value the user reads
        /// </summary>
        public static readonly Color Ink = Color.FromArgb(27, 31, 36);

        /// <summary>
        /// Text naming a value rather than carrying one, which should recede behind it
        /// </summary>
        public static readonly Color MutedInk = Color.FromArgb(90, 101, 112);

        /// <summary>
        /// The fill behind the one button carrying the action to take next
        /// </summary>
        public static readonly Color Accent = Color.FromArgb(31, 92, 153);

        /// <summary>
        /// The accent under the pointer
        /// </summary>
        public static readonly Color AccentHover = Color.FromArgb(42, 112, 181);

        /// <summary>
        /// The accent while the button is held down
        /// </summary>
        public static readonly Color AccentPressed = Color.FromArgb(23, 72, 121);

        /// <summary>
        /// The line drawn through the individual readings, and the first of the two compared distributions
        /// </summary>
        public static readonly Color Trace = Color.FromArgb(43, 108, 176);

        /// <summary>
        /// The line drawn through the running mean, and the second of the two compared distributions
        /// </summary>
        public static readonly Color Average = Color.FromArgb(192, 57, 43);

        /// <summary>
        /// The marker on the value an unbiased generator is expected to give
        /// </summary>
        public static readonly Color Expected = Color.FromArgb(130, 138, 146);

        #endregion
    }
}
