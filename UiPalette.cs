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
// 2026/09/08 - Mike Pullen - Take the colours from the system scheme rather than fixing them
//*********************************************************************************************************************

using System.Drawing;
using System.Windows.Forms;

namespace RandomNumberGenerator
{
    /// <summary>
    /// The colours the window is built from: the ground it sits on, the cards laid on that ground, the
    /// hairlines between them, and the text used on each.
    /// NOTE: These are properties rather than fields on purpose. SystemColors reports whatever scheme is in
    /// force now, and a scheme can be changed while the application is running; a field would hold whatever
    /// was in force when the class was first touched. GeneratorForm re-applies these when the system tells
    /// it the scheme has changed.
    /// NOTE: Text has to be taken from the same pair as the surface behind it. Card and CardText go
    /// together, Ground and GroundText go together, and mixing them gives a pair that a high contrast
    /// scheme can render as one colour on itself.
    /// </summary>
    public static class UiPalette
    {
        #region Properties

        /// <summary>
        /// The ground the cards are laid on, and the colour of the tab pages behind them (read-only)
        /// </summary>
        public static Color Ground { get => SystemColors.Control; }

        /// <summary>
        /// Text on the ground (read-only)
        /// </summary>
        public static Color GroundText { get => SystemColors.ControlText; }

        /// <summary>
        /// The face of a card: the settings, the readouts, the comparison and the charts each sit on one
        /// (read-only)
        /// </summary>
        public static Color Card { get => SystemColors.Window; }

        /// <summary>
        /// Text on a card, carrying a value the user reads (read-only)
        /// </summary>
        public static Color CardText { get => SystemColors.WindowText; }

        /// <summary>
        /// The hairline around a card and between the readouts (read-only)
        /// </summary>
        public static Color Line { get => SystemColors.ControlDark; }

        /// <summary>
        /// Text naming a value rather than carrying one, which should recede behind it (read-only)
        /// </summary>
        public static Color MutedText { get => SystemColors.GrayText; }

        /// <summary>
        /// The fill behind the one button carrying the action to take next (read-only)
        /// </summary>
        public static Color Accent { get => SystemColors.Highlight; }

        /// <summary>
        /// Text on the accent (read-only)
        /// </summary>
        public static Color AccentText { get => SystemColors.HighlightText; }

        /// <summary>
        /// The accent under the pointer. There is no system colour for this, so it is taken from the accent
        /// itself and stays in step with whatever scheme that came from. (read-only)
        /// </summary>
        public static Color AccentHover { get => ControlPaint.Light(SystemColors.Highlight); }

        /// <summary>
        /// The accent while the button is held down (read-only)
        /// </summary>
        public static Color AccentPressed { get => ControlPaint.Dark(SystemColors.Highlight); }

        /// <summary>
        /// The marker on the value an unbiased generator is expected to give (read-only)
        /// </summary>
        public static Color Expected { get => SystemColors.GrayText; }

        /// <summary>
        /// The line drawn through the individual readings, and the first of the two compared distributions
        /// (read-only).
        /// NOTE: This and Average are the only fixed colours here, and they are fixed because they are data
        /// rather than chrome: they tell two series apart, so they have to differ from one another rather
        /// than agree with the window. Both are chosen to read on a light ground and on a dark one.
        /// </summary>
        public static Color Trace { get => Color.FromArgb(43, 108, 176); }

        /// <summary>
        /// The line drawn through the running mean, and the second of the two compared distributions
        /// (read-only)
        /// </summary>
        public static Color Average { get => Color.FromArgb(192, 57, 43); }

        #endregion
    }
}
