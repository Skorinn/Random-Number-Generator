//*********************************************************************************************************************
// File Name:      DeviceUpdateThread.cs
// Description:    Thread for asynchronously updating the device list
//
// Copyright (C) 2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2023/12/03 - Mike Pullen - Original implementation.
// 2023/12/06 - Mike Pullen - Changed from an always-running watchdog to a thread pool
//*********************************************************************************************************************
using System;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Management;
using System.Text.RegularExpressions;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Watchdog for asynchronously updating the device list
    /// </summary>
    /// <param name="oStateInfo">IN - State info for executing in the thread pool (not used)</param>
    static class DeviceUpdateThread
    {
        public static void ThreadProc(object stateInfo)
        {
            // Discard unused parameters
            _ = stateInfo;

            // Only allow one thread to execute the update at a time (first come first served)
            lock(m_Lock)
            {
                // Backup the info box and display the "reading devices" message
                BackupInfoBox();
                UpdateInfoBox(m_sREADING_DEVICES_MESSAGE, m_READING_DEVICES_TEXTCOLOR, m_READING_DEVICES_BACKCOLOR);

                // Update the device list
                GetDevicePorts();
                UpdateDeviceList();

                // Restore the previous info box message
                UpdateInfoBox(m_sStatusBoxText, m_StatusBoxTextColor, m_StatusBoxBackColor);
            }
        }

        /// <summary>
        /// Gets a list of ports used by RNG device
        /// </summary>
        /// <returns>List of ports with connected RNG devices</returns>
        private static void GetDevicePorts()
        {
            // Create a new list
            m_DeviceList = new BindingList<RNGDevice>();

            // Search for all USB controller devices
            ManagementObjectSearcher controllerSearcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice");
            ManagementObjectCollection controllerCollection = controllerSearcher.Get();
            foreach (ManagementBaseObject controller in controllerCollection)
            {
                // Get the ID of the dependent device for the controller
                string sDependent = (string)controller.GetPropertyValue("Dependent");
                string[] sDependentSplit = System.Text.RegularExpressions.Regex.Split(sDependent, "DeviceID=");
                string sDeviceID = sDependentSplit[1];

                // Search all of the USB devices found with this device ID (should only be 1)
                ManagementObjectSearcher deviceSearcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity Where DeviceID=" + sDeviceID);
                ManagementObjectCollection deviceCollection = deviceSearcher.Get();

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
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the device list in the parent form
        /// </summary>
        private static void UpdateDeviceList()
        {
            // Check if the parent has been set
            if (null != m_Parent)
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
            // Check if invoke is required (should be)
            if (m_Parent.InvokeRequired)
            {
                // Invoke the update in the parent thread
                m_Parent.Invoke(new Action(() => GetInfoBoxState()));
            }
            else
            {
                // Update here
                GetInfoBoxState();
            }
        }

        /// <summary>
        /// Restores the current text and color of the parent's info box
        /// </summary>
        /// <param name="sText">IN - Text to display in the info box</param>
        /// <param name="textColor">IN - Text color to set for the info box</param>
        /// <param name="backColor">IN - Background color to set for the info box</param>
        private static void UpdateInfoBox(string sText, Color textColor, Color backColor)
        {
            // Check if invoke is required (should be)
            if (m_Parent.InvokeRequired)
            {
                // Invoke the update in the parent thread
                m_Parent.Invoke(new Action(() => SetInfoBoxState(sText, textColor, backColor)));
            }
            else
            {
                // Update here
                SetInfoBoxState(sText, textColor, backColor);
            }
        }

        /// <summary>
        /// Gets the text and color of the parent's info box
        /// </summary>
        private static void GetInfoBoxState()
        {
            m_sStatusBoxText = m_Parent.StatusBoxText;
            m_StatusBoxTextColor = m_Parent.StatusBoxTextColor;
            m_StatusBoxBackColor = m_Parent.StatusBoxBackColor;
        }

        /// <summary>
        /// Sets the text and color of the parent's info box
        /// </summary>
        /// <param name="sText">IN - Text to display in the info box</param>
        /// <param name="textColor">IN - Text color to set for the info box</param>
        /// <param name="backColor">IN - Background color to set for the info box</param>
        private static void SetInfoBoxState(string sText, Color textColor, Color backColor)
        {
            m_Parent.StatusBoxText = sText;
            m_Parent.StatusBoxTextColor = textColor;
            m_Parent.StatusBoxBackColor = backColor;
        }

        /// <summary>
        /// The binding list of devices
        /// </summary>
        internal static BindingList<RNGDevice> DeviceList { get => m_DeviceList; }

        /// <summary>
        /// The parent form to which to relay the updated device list
        /// </summary>
        public static GeneratorForm Parent { get => m_Parent; set => m_Parent = value; }

        // Synchronizaion object
        private static object m_Lock = new object();

        // Device list update data members
        private static BindingList<RNGDevice> m_DeviceList = new BindingList<RNGDevice>();
        private static GeneratorForm m_Parent = null;

        // Test and color for restoring the info box after update
        private static string m_sStatusBoxText = "";
        private static Color m_StatusBoxTextColor = System.Drawing.SystemColors.WindowText;
        private static Color m_StatusBoxBackColor = System.Drawing.SystemColors.Info;

        // Message to display in the info box when reading devices
        private static readonly string m_sREADING_DEVICES_MESSAGE = " Checking attached devices and updating port list...";
        private static readonly Color m_READING_DEVICES_TEXTCOLOR = System.Drawing.Color.White;
        private static readonly Color m_READING_DEVICES_BACKCOLOR = System.Drawing.SystemColors.Highlight;
    }
}
