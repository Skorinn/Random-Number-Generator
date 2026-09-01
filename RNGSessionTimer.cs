//*********************************************************************************************************************
// File Name:      RNGSessionTimer.cs
// Description:    Timer for tracking the elapsed time of a Random Number Generator session
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History:
//====================================================================================================================
// 2024/02/03 - Mike Pullen - Original implementation.
// 2026/09/01 - Mike Pullen - Corrected the file header, rolled the session time over at 60 rather than 61, and
//                            replaced the destructor with a dispose
//*********************************************************************************************************************
using System;
using System.Windows.Forms;

namespace RandomNumberGenerator
{
    public interface IRNGSessionTimer
    {
        bool Enabled { get; set; }
        bool InProgress { get; }
        int Interval { get; set; }
        string SessionTime { get; }
        TextBox TimerTextBox { set; }

        void Reset();
        void Start();
        void Stop();
        void Tick();
    }

    /// <summary>
    /// Class representing the timer for a random number generator session
    /// </summary>
    public class RNGSessionTimer : IRNGSessionTimer, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public RNGSessionTimer() : this(null)
        {
            // Nothing to do
        }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="timerTextBox">IN - Text box for displaying the timer</param>
        public RNGSessionTimer(TextBox timerTextBox)
        {
            // Initialize the timer
            m_Timer.Interval = 1000; // Default interval to 1 second
            m_Timer.Elapsed += this.SessionTimer_Tick;

            // Set the text box for the timer
            m_TimerTextBox = timerTextBox;
        }

        /// <summary>
        /// Stops the timer and disposes of it.
        /// NOTE: This replaces a destructor, which ran on the finalizer thread where the timer may already
        /// have been finalized and where a failure would bring the process down. The timer held here has a
        /// finalizer of its own, so there is nothing left that needs one here.
        /// </summary>
        public void Dispose()
        {
            if (null != m_Timer)
            {
                m_Timer.Stop();
                m_Timer.Dispose();
            }
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
            // Update the timer value
            TickSessionTimer();

            // Update the control
            UpdateTimerText();
        }

        /// <summary>
        /// Resets the session time to 0
        /// </summary>
        public void Reset()
        {
            // Use of the session time is exclusive
            lock (m_TimerLock)
            {
                m_iSessionMilliseconds = 0;
                m_iSessionSeconds = 0;
                m_iSessionMinutes = 0;
                m_iSessionHours = 0;
            }

            // Update the control
            UpdateTimerText();
        }

        /// <summary>
        /// Increments the session counter
        /// </summary>
        private void TickSessionTimer()
        {
            // Use of the session time is exclusive
            lock (m_TimerLock)
            {
                // Update the millisecond counter then check for rollovers
                m_iSessionMilliseconds += Interval;
                if (m_iSessionMilliseconds >= 1000)
                {
                    ++m_iSessionSeconds;
                    m_iSessionMilliseconds -= 1000;
                }

                if (m_iSessionSeconds >= 60)
                {
                    ++m_iSessionMinutes;
                    m_iSessionSeconds -= 60;
                }

                if (m_iSessionMinutes >= 60)
                {
                    ++m_iSessionHours;
                    m_iSessionMinutes -= 60;
                }
            }
        }

        /// <summary>
        /// Updates the session timer text box control (if set)
        /// </summary>
        private void UpdateTimerText()
        {
            // Only update if the text box is set
            if (null != m_TimerTextBox)
            {
                // Executes in a thread, so need to invoke in the main GUI thread
                if (true == m_TimerTextBox.InvokeRequired)
                {
                    m_TimerTextBox.Invoke((MethodInvoker)delegate { UpdateTimerText(); });
                }
                else
                {
                    m_TimerTextBox.Text = SessionTime;
                }
            }
        }

        #endregion
        #region Properties

        /// <summary>
        /// Whether the timer is actively running
        /// </summary>
        public bool Enabled { get => m_Timer.Enabled; set => m_Timer.Enabled = value; }

        /// <summary>
        /// Whether the timer has been started (read-only)
        /// </summary>
        public bool InProgress { get => m_bInProgress; }

        /// <summary>
        /// Timer interval in milliseconds. Valid values are 1-1000.
        /// </summary>
        public int Interval
        {
            get => (int)m_Timer.Interval;
            set
            {
                // Validate the interval
                if (0 >= value)
                {
                    throw new ArgumentOutOfRangeException("Interval must be greater than 0");
                }
                else if (1000 < value)
                {
                    throw new ArgumentOutOfRangeException("Interval cannot be greater than 1000");
                }

                m_Timer.Interval = value;
            }
        }

        /// <summary>
        /// Formated session time string (read-only)
        /// </summary>
        public string SessionTime
        {
            get
            {
                string sTimerText;

                // Use of the session time is exclusive
                lock (m_TimerLock)
                {
                    // Build and return the formatted string representation of the updated session time
                    sTimerText = $"{m_iSessionHours:00}:{m_iSessionMinutes:00}:{m_iSessionSeconds:00}";
                }

                return sTimerText;
            }
        }

        /// <summary>
        /// Text box for displaying the timer (write-only)
        /// </summary>
        public TextBox TimerTextBox { set => m_TimerTextBox = value; }

        #endregion
        #region Data Members

        // The actual timer
        private System.Timers.Timer m_Timer = new System.Timers.Timer(); // Use system timer instead of forms timer for threading, so the update is reliable

        // Records when the timer has been started
        private bool m_bInProgress = false;

        // Text box for displaying the timer
        private TextBox m_TimerTextBox = null;

        // Session timer counters
        private object m_TimerLock = new object();
        private int m_iSessionMilliseconds = 0;
        private int m_iSessionSeconds = 0;
        private int m_iSessionMinutes = 0;
        private int m_iSessionHours = 0;
        #endregion
    }
}
