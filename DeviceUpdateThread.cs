//*********************************************************************************************************************
// File Name:      DeviceUpdateThread.cs
// Description:    Thread for asynchronously updating the device list
//
// Copyright (c) 2023 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2023/12/03 - Mike Pullen - Original implementation.
// 2023/12/06 - Mike Pullen - Changed from an always-running watchdog to a thread pool
// 2026/08/31 - Mike Pullen - Only restore the info box when the reading devices message is still displayed
// 2026/09/01 - Mike Pullen - Release the searches run for each controller when they are finished with
// 2026/09/07 - Mike Pullen - Report device status through the shared status palette
// 2026/09/09 - Mike Pullen - Read the severity colours where they are shown rather than holding them, so a
//                            scheme changed while running is followed
// 2026/09/09 - Mike Pullen - Reported the search into the form only once its window exists, as InvokeRequired
//                            cannot say which thread owns a form that was never shown
//*********************************************************************************************************************
using System;
using System.ComponentModel;
using System.Drawing;
using System.Management;
using System.Text.RegularExpressions;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Watchdog for asynchronously updating the device list
    /// </summary>
    /// <param name="oStateInfo">IN - State info for executing in the thread pool (not used)</param>
    public static class DeviceUpdateThread
    {
        public static void ThreadProc(object stateInfo)
        {
            // Discard unused parameters
            _ = stateInfo;

            // Only allow one thread to execute the update at a time (first come first served)
            lock (m_Lock)
            {
                // Backup the info box and display the "reading devices" message
                BackupInfoBox();
                UpdateInfoBox(m_sREADING_DEVICES_MESSAGE, StatusPalette.BusyText, StatusPalette.BusyBackground);

                try
                {
                    // Update the device list
                    GetDevicePorts();

                    // Update the device list in the parent form
                    UpdateDeviceList();
                }
                catch (Exception deviceException)
                {
                    // Searching for devices can fail for reasons outside of this application, and this runs
                    // on a pool thread where an escaping exception would bring the process down. The failure
                    // is reported and the port list is left as it was.
                    ReportDeviceUpdateFailure(deviceException);
                    return;
                }
                finally
                {
                    // Restore the previous info box message, whether or not the search succeeded
                    RestoreInfoBox();
                }
            }
        }

        /// <summary>
        /// Gets a list of ports used by RNG device
        /// </summary>
        /// <returns>List of ports with connected RNG devices</returns>
        private static void GetDevicePorts()
        {
            // Create a new list
            m_DeviceList = new BindingList<IRNGDevice>();

            // Search for all USB controller devices
            using (ManagementObjectSearcher controllerSearcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice"))
            {
                using (ManagementObjectCollection controllerCollection = controllerSearcher.Get())
                {
                    foreach (ManagementBaseObject controller in controllerCollection)
                    {
                        // Get the ID of the dependent device for the controller
                        string sDependent = (string)controller.GetPropertyValue("Dependent");
                        string[] sDependentSplit = System.Text.RegularExpressions.Regex.Split(sDependent, "DeviceID=");
                        string sDeviceID = sDependentSplit[1];

                        // Search all of the USB devices found with this device ID (should only be 1).
                        // The searcher and the results it returns are released once each controller has been
                        // looked at, as the search is run for every controller every time the devices are
                        // updated and holding on to them would leak the resources behind them.
                        using (ManagementObjectSearcher deviceSearcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity Where DeviceID=" + sDeviceID))
                        {
                            using (ManagementObjectCollection deviceCollection = deviceSearcher.Get())
                            {
                                foreach (ManagementBaseObject device in deviceCollection)
                                {
                                    // Attempt to get the name of the device
                                    object oName = device.GetPropertyValue("Name");
                                    if (null != oName)
                                    {
                                        // Check if this is a USB serial device
                                        string sDeviceName = (string)oName;
                                        string sSerialDeviceRegex = @".*USB.*Serial.*COM\d+.*";
                                        bool bSerialDevice = Regex.IsMatch(sDeviceName, sSerialDeviceRegex, RegexOptions.IgnoreCase);
                                        if (bSerialDevice)
                                        {
                                            // Extract the port name and number
                                            string sComPortRegex = @"COM\d+";
                                            string sComName = Regex.Match(sDeviceName, sComPortRegex, RegexOptions.IgnoreCase).Value;
                                            string sPortNumRegex = @"\d+";
                                            string sPortNumString = Regex.Match(sComName, sPortNumRegex, RegexOptions.IgnoreCase).Value;
                                            int iPortNum = Convert.ToInt32(sPortNumString);

                                            // Add the device to the list
                                            RNGDevice currentDevice = new RNGDevice(sComName, iPortNum);
                                            m_DeviceList.Add(currentDevice);
                                        } // END: if (bSerialDevice)
                                    } // END: if (null != oName)

                                    // Break out of the loop if the thread was terminated early
                                    if (Terminating)
                                    {
                                        break;
                                    }
                                } // END: foreach (ManagementBaseObject device in deviceCollection)
                            } // END: using (ManagementObjectCollection deviceCollection = deviceSearcher.Get())
                        } // END: using (ManagementObjectSearcher deviceSearcher = new ManagementObjectSearcher(...))

                        // Break out of the loop if the thread was terminated early
                        if (Terminating)
                        {
                            break;
                        }
                    } // END: foreach (ManagementBaseObject controller in controllerCollection)
                } // END: using (ManagementObjectCollection controllerCollection = controllerSearcher.Get())
            } // END: using (ManagementObjectSearcher controllerSearcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice"))
        }

        /// <summary>
        /// Update the device list in the parent form
        /// </summary>
        private static void UpdateDeviceList()
        {
            // Check if the parent has been set and not terminating early
            if (ParentIsReady)
            {
                // Check if invoke is required (should be)
                if (m_Parent.InvokeRequired)
                {
                    // Invoke the update in the parent thread
                    m_Parent.Invoke(new Action(() => m_Parent.DeviceList = m_DeviceList));
                }
                else
                {
                    // Update here
                    m_Parent.DeviceList = m_DeviceList;
                }
            }
        }

        /// <summary>
        /// Backs up the current text and color of the parent's info box
        /// </summary>
        private static void BackupInfoBox()
        {
            // Make sure there is a window to report into
            if (ParentIsReady)
            {
                // Get the current status box state through the parent's interface
                m_Parent.GetStatusBoxState(out m_sStatusBoxText, out m_StatusBoxTextColor, out m_StatusBoxBackColor);
            }
        }

        /// <summary>
        /// Updates the current text and color of the parent's info box
        /// </summary>
        /// <param name="sText">IN - Text to display in the info box</param>
        /// <param name="textColor">IN - Text color to set for the info box</param>
        /// <param name="backColor">IN - Background color to set for the info box</param>
        private static void UpdateInfoBox(string sText, Color textColor, Color backColor)
        {
            // Make sure there is a window to report into
            if (ParentIsReady)
            {
                // Use parent's method which already handles invoke requirements
                m_Parent.SetStatusBoxState(sText, textColor, backColor);
            }
        }

        /// <summary>
        /// Restores the info box to the state recorded by the backup, but only if the message displayed while
        /// reading the devices is still the one shown. The device search runs for several seconds, during which
        /// the user can start a session or an error can be reported, and restoring unconditionally would replace
        /// those newer messages with a message captured before the search started.
        /// </summary>
        private static void RestoreInfoBox()
        {
            // Make sure there is a window to report into
            if (ParentIsReady)
            {
                // Get the message currently displayed in the info box
                string sCurrentText;
                Color currentTextColor;
                Color currentBackColor;
                m_Parent.GetStatusBoxState(out sCurrentText, out currentTextColor, out currentBackColor);

                // Discard the current colors as only the message identifies who owns the info box
                _ = currentTextColor;
                _ = currentBackColor;

                // Only restore if nothing has been displayed since the reading devices message
                bool bInfoBoxUnchanged = (m_sREADING_DEVICES_MESSAGE == sCurrentText);
                if (bInfoBoxUnchanged)
                {
                    UpdateInfoBox(m_sStatusBoxText, m_StatusBoxTextColor, m_StatusBoxBackColor);
                }
            }
        }

        /// <summary>
        /// Reports that the search for devices failed
        /// </summary>
        /// <param name="deviceException">IN - The failure that stopped the search</param>
        private static void ReportDeviceUpdateFailure(Exception deviceException)
        {
            // Make sure there is a window to report into
            if (ParentIsReady)
            {
                string sMessage = $"{m_sREADING_DEVICES_ERROR} {deviceException.Message}";
                m_Parent.SetStatusBoxState(sMessage, StatusPalette.ErrorText, StatusPalette.ErrorBackground);
            }
        }

        /// <summary>
        /// The binding list of devices
        /// </summary>
        public static BindingList<IRNGDevice> DeviceList { get => m_DeviceList; }

        /// <summary>
        /// The parent form to which to relay the updated device list
        /// </summary>
        public static IGeneratorForm Parent { get => m_Parent; set => m_Parent = value; }

        /// <summary>
        /// Indicates if the thread is terminating early (read-only)
        /// </summary>
        public static bool Terminating { get => (null != m_Parent) && (GeneratorForm.RngGuiStates.Terminating == m_Parent.State); }

        /// <summary>
        /// Whether there is a window to report the search into. A parent has to be set, it has to not be
        /// closing, and its window has to exist: the search runs on a pool thread and reports by calling the
        /// form, and every one of those calls decides how to marshal itself by asking InvokeRequired.
        /// A form that has never been shown has no handle, and InvokeRequired on a handleless control
        /// answers false, which reads as "already on the right thread" - so the search writes the controls
        /// from the pool thread and races whoever else is using them. Nothing is displaying a form that has
        /// no window, so there is nothing to report to and the result is dropped rather than forced in.
        /// </summary>
        private static bool ParentIsReady
        {
            get => ((null != m_Parent) && (false == Terminating) && m_Parent.IsHandleCreated);
        }

        // Synchronizaion objects
        private static object m_Lock = new object();

        // Device list update data members
        private static BindingList<IRNGDevice> m_DeviceList = new BindingList<IRNGDevice>();
        private static IGeneratorForm m_Parent = null;

        // Test and color for restoring the info box after update
        private static string m_sStatusBoxText = string.Empty;
        private static Color m_StatusBoxTextColor = System.Drawing.SystemColors.WindowText;
        private static Color m_StatusBoxBackColor = System.Drawing.SystemColors.Info;

        // Message to display in the info box when reading devices
        private static readonly string m_sREADING_DEVICES_MESSAGE = "Checking attached devices and updating port list...";
        private static readonly string m_sREADING_DEVICES_ERROR = "Unable to check the attached devices. The port list has been left as it was.";
    }
}
