//*********************************************************************************************************************
// File Name:      DeviceUpdateThread.cs
// Description:    Thread for asynchronously updating the device list
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/02/03 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RandomNumberGenerator
{
    public interface IRNGSessionTimer
    {
        void Start();
        void Stop();
        void Tick();

        bool Enabled { get; set; }
        double Interval { get; set; }
        TextBox TimerTextBox { set; }
    }

    /// <summary>
    /// Class representing the timer for a random number generator session
    /// </summary>
    public class RNGSessionTimer : IRNGSessionTimer
    {
        #region Constructors

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="sessionData">INOUT - Session data object</param>
        public RNGSessionTimer(IRNGSessionData sessionData) : this(sessionData, null)
        {
            // Nothing to do
        }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="sessionData">INOUT - Session data object</param>
        /// <param name="timerTextBox">INOUT - Text box for displaying the timer</param>
        public RNGSessionTimer(IRNGSessionData sessionData, TextBox timerTextBox)
        {
            m_Data = sessionData;
            m_TimerTextBox = timerTextBox;
            m_Timer.Interval = 1000; // Default interval to 1 second
            m_Timer.Elapsed += this.SessionTimer_Tick;
        }

        /// <summary>
        /// Destructor. Stops the timer and disposes of it.
        /// </summary>
        ~RNGSessionTimer()
        {
            m_Timer.Stop();
            m_Timer.Dispose();
        }

        #endregion
        #region Event Handlers

        /// <summary>
        /// Event handler for the tick of the session timer
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            Tick();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Starts the session timer
        /// </summary>
        public void Start()
        {
            m_bInProgress = true;
            m_Timer.Start();
        }

        /// <summary>
        /// Stops the session timer
        /// </summary>
        public void Stop()
        {
            m_Timer.Stop();
            m_bInProgress = false;
        }

        /// <summary>
        /// Executes a tick of the session timer. 
        /// Exposed for testing, but does not require being called directly.
        /// This will be called automatically by the tick event handler.
        /// </summary>
        public void Tick()
        {
            string sTimerText;

            // Executes in a thread so protect against collision when updating members
            lock (m_Data)
            {
                // Update the timer value
                m_Data.TickSessionTimer((int)Interval);

                // Create a copy of the session timer string
                sTimerText = (string)m_Data.SessionTime.Clone();
            }

            // Update the control
            UpdateTimerText(sTimerText);
        }

        /// <summary>
        /// Updates the session timer text box control (if set)
        /// </summary>
        /// <param name="sTimerText">IN - The text to set in the text box</param>
        private void UpdateTimerText(string sTimerText)
        {
            // Only update if the text box is set
            if (null != m_TimerTextBox)
            {
                // Executes in a thread, so need to invoke in the main GUI thread
                if (true == m_TimerTextBox.InvokeRequired)
                {
                    m_TimerTextBox.Invoke((MethodInvoker)delegate { UpdateTimerText(sTimerText); });
                }
                else
                {
                    m_TimerTextBox.Text = sTimerText;
                }
            }
        }

        #endregion
        #region Properties

        public bool Enabled { get => m_Timer.Enabled; set => m_Timer.Enabled = value; }
        public double Interval { get => m_Timer.Interval; set => m_Timer.Interval = value; }
        public TextBox TimerTextBox { set => m_TimerTextBox = value; }
        public bool InProgress { get => m_bInProgress; set => m_bInProgress = value; }

        #endregion
        #region Data Members

        private System.Timers.Timer m_Timer = new System.Timers.Timer(); // Use system timer instead of forms timer for threading, so the update is reliable
        private bool m_bInProgress = false;
        private IRNGSessionData m_Data = null;
        private TextBox m_TimerTextBox = null;

        #endregion
    }
}
