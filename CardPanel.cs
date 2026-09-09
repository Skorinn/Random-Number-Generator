//*********************************************************************************************************************
// File Name:      CardPanel.cs
// Description:    A panel drawn as a card: a light face inside a hairline border
//
// Copyright (c) 2022-2026 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History:
//====================================================================================================================
// 2026/09/07 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using System.Drawing;
using System.Windows.Forms;

namespace RandomNumberGenerator
{
    /// <summary>
    /// A panel drawn as a card: a face laid on the window's ground, inside a hairline border.
    /// NOTE: This exists because a Panel's own FixedSingle border is drawn by the window manager in a system
    /// colour that does not match the rest of the interface, and a GroupBox brings a caption and a sunken
    /// frame that the layout does not want.
    /// </summary>
    public class CardPanel : Panel
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CardPanel()
        {
            BackColor = UiPalette.Card;
            BorderStyle = BorderStyle.None;

            // The border is drawn rather than left to the window manager, so it repaints with the card
            // rather than flickering behind it while the window is resized
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        #endregion
        #region Event Handlers

        /// <summary>
        /// Draws the hairline border around the face
        /// </summary>
        /// <param name="e">IN - The paint event arguments</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen borderPen = new Pen(UiPalette.Line))
            {
                // Inset by one so the line falls inside the panel rather than half outside it
                Rectangle border = new Rectangle(0, 0, (Width - 1), (Height - 1));
                e.Graphics.DrawRectangle(borderPen, border);
            }
        }

        #endregion
    }
}
