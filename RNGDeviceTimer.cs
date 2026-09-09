//*********************************************************************************************************************
// File Name:      RNGDeviceTimer.cs
// Description:    Implementation of the timer for reading from the device
//
// Copyright (c) 2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2024/02/03 - Mike Pullen - Original implementation.
// 2026/09/08 - Mike Pullen - Marshalled the native bool as one byte, so a device that fails to initialize
//                            is reported as having failed rather than as ready
//*********************************************************************************************************************
using System;
using System.Runtime.InteropServices;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Delegate for the function to report device read result
    /// </summary>
    public delegate void DeviceReadCallbackDelegate(double fData);

    /// <summary>
    /// Interface for the RNG device timer
    /// </summary>
    public interface IRNGDeviceTimer
    {
        bool Enabled { get; set; }
        bool Initialized { get; set; }
        int Interval { get; set; }

        bool InitializeDevice(int iPort, bool bSimulate);
        void SetReadCallback(DeviceReadCallbackDelegate readCallback);
        void Start();
        void Stop();
        void TriggerTick();
    }

    /// <summary>
    /// Timer for reading from the RNG device
    /// </summary>
    public class RNGDeviceTimer : IRNGDeviceTimer
    {
        #region Imports

        // The native side returns a C++ bool, which is one byte. Left to itself the marshaller expects the
        // four byte Windows BOOL, so it reads three bytes of whatever the call left behind along with the
        // answer, and a native false comes back as true often enough to matter: initializing against a port
        // with nothing on it reported success, and the failure only showed up as a read error a moment
        // later. I1 is the one byte bool, which is what these functions actually return and take.
        [DllImport("TruRNGpro.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Initialize(int iPort, [MarshalAs(UnmanagedType.I1)] bool bSimulate);

        [DllImport("TruRNGpro.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool GetRandomBitAverage(ref double fResult);

        #endregion
        #region Constructors

        /// <summary>
        /// Default constructor. Defaults interval to 100ms for improved performance.
        /// </summary>
        public RNGDeviceTimer()
        {
            m_Timer.Interval = 100; // Change from 10ms to 100ms (10x decimation)
            m_Timer.Tick += new System.EventHandler(this.Ticked);
        }

        #endregion
        #region Event Handlers

        /// <summary>
        /// Event handler for tick of the timer to read data
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void Ticked(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Update the average
            double fCurrentValue = 0.0;
            bool bStatus = GetRandomBitAverage(ref fCurrentValue);

            if (false == bStatus)
            {
                // Set the result to double max to indicate error
                fCurrentValue = double.MaxValue;

                // Flag the interface needs to be reinitialized
                m_bInterfaceInitialized = false;
            }

            // Execute the read callback
            m_fpReadCallback?.Invoke(fCurrentValue);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Initializes the device interface
        /// </summary>
        /// <param name="iPort">IN - The port to use for the device</param>
        /// <param name="bSimulate">IN - Whether to simulate the device</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool InitializeDevice(int iPort, bool bSimulate)
        {
            m_bInterfaceInitialized = Initialize(iPort, bSimulate);
            return m_bInterfaceInitialized;
        }

        /// <summary>
        /// Sets the function to execute to report device read result
        /// </summary>
        public void SetReadCallback(DeviceReadCallbackDelegate readCallback)
        {
            m_fpReadCallback = readCallback;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Start()
        {
            m_Timer.Start();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            m_Timer.Stop();
        }

        /// <summary>
        /// Manually triggers a tick of the timer
        /// </summary>
        public void TriggerTick()
        {
            Ticked(this, EventArgs.Empty);
        }

        #endregion
        #region Properties

        /// <summary>
        /// Whether the timer is enabled
        /// </summary>
        public bool Enabled { get => m_Timer.Enabled; set => m_Timer.Enabled = value; }

        /// <summary>
        /// The timer interval
        /// </summary>
        public int Interval { get => m_Timer.Interval; set => m_Timer.Interval = value; }

        /// <summary>
        /// Whether the device interface is initialized
        /// </summary>
        public bool Initialized { get => m_bInterfaceInitialized; set => m_bInterfaceInitialized = value; }

        #endregion
        #region Constants


        #endregion
        #region Data Members

        private System.Windows.Forms.Timer m_Timer = new System.Windows.Forms.Timer();
        private DeviceReadCallbackDelegate m_fpReadCallback = null;
        private bool m_bInterfaceInitialized = false;

        #endregion
    }
}
