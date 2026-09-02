//*********************************************************************************************************************
// File Name:      GeneratorForm.cs
// Description:    Implementation of the Random Number Generator GUI
//
// Copyright (C) 2022-2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2022/09/10 - Mike Pullen - Original implementation.
// 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
// 2023/12/02 - Mike Pullen - Added simulate, pause, and target value
// 2026/08/31 - Mike Pullen - Report recovered files, dispose through the standard pattern, and keep processing
//                            messages while waiting for the device update to finish on close
//*********************************************************************************************************************

// Enable to dump the USB device information
//#define DUMP_DEVICES

using DeviceInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the Random Number Generator form
    /// </summary>
    public interface IGeneratorForm : ISynchronizeInvoke
    {
        BindingList<IRNGDevice> DeviceList { set; }
        bool FileBrowseActive { get; set; }
        bool Running { get; }
        Color StatusBoxBackColor { get; set; }
        string StatusBoxText { get; set; }
        Color StatusBoxTextColor { get; set; }
        GeneratorForm.RngGuiStates State { get; }

        object Invoke(Action method);
        void GetStatusBoxState(out string sText, out Color textColor, out Color backColor);
        void RecordReadResult(double fResult);
        void SetStatusBoxState(string sText, Color textColor, Color backColor);
    }

    /// <summary>
    /// Random Number Generator form
    /// </summary>
    public partial class GeneratorForm : Form, IGeneratorForm, IDisposable
    {
        #region Type definitions

        public enum RngGuiStates
        {
            Idle = 0,
            Running = 1,
            Paused = 2,
            Terminating = 3,
            RNG_GUI_STATES_SIZE // Keep at end
        };

        #endregion
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="sessionData">IN - The session data object (cannot be null)</param>
        /// <param name="timer">IN - The timer object (cannot be null)</param>
        public GeneratorForm(IRNGSessionData sessionData, IRNGDeviceTimer timer)
        {
            if (null == sessionData)
            {
                throw new ArgumentNullException("Specified data object cannot be null");
            }

            if (null == timer)
            {
                throw new ArgumentNullException("Specified timer object cannot be null");
            }

            // Record the session and timer objects provided
            m_Data = sessionData;
            m_Timer = timer;

            // Set the status callback for the device timer
            m_Timer.SetReadCallback(RecordReadResult);

            // Set the data point added callback for chart updates
            m_Data.DataPointAddedCallback = OnDataPointAdded;

            // Dump USB devices if debugging and defined
#if DUMP_DEVICES && DEBUG
                DumpUSBDevices();
#endif
            // Initialize the thread pool
            InitializeThreadPool();

            // Initialize GUI
            InitializeComponent();

            // Set the info box to idle
            m_StatusTextBox.Text = m_sIDLE_MESSAGE;
            m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;

            // Start the device update thread and trigger an update
            DeviceUpdateThread.Parent = this;
            ThreadPool.QueueUserWorkItem(state =>
            {
                m_DeviceUpdateComplete.Reset(); // Clear the device update complete flag
                try
                {
                    DeviceUpdateThread.ThreadProc(state);
                }
                finally
                {
                    // Signal that the device update has finished however it ended, so a failure does not
                    // leave the close waiting for an update that will never report itself complete
                    m_DeviceUpdateComplete.Set();
                }
            });

            // Create the source from the list of device ports
            m_DeviceBindingSource = new BindingSource();
            m_DeviceBindingSource.DataSource = new BindingList<RNGDevice>();

            // Set the port combo box data source and define the display and value members
            m_PortComboBox.DataSource = m_DeviceBindingSource.DataSource;
            m_PortComboBox.DisplayMember = RNGDevice.DisplayMember;
            m_PortComboBox.ValueMember = RNGDevice.ValueMember;

            // Set the maximumn number of data points and averages to hold in memory
            // NOTE: Should match the window size of the data for the chart
            m_Data.DataWindowSize = m_ResultChart.MaxDataSize;

            // Ensure the correct default state of the simulate button
            m_SimulateToggle.Checked = m_Data.Simulated;

            // Initialize the target combo box and selection
            m_TargetComboBox.Items.AddRange((object[])TargetValues.TargetStrings.Clone());
            m_TargetComboBox.SelectedIndex = 0;

            // Set the timer text box in the timer object
            m_Data.Timer.TimerTextBox = m_SessionTimerTextBox;
        }

        #endregion
        #region Event Handlers

        /// <summary>
        /// Override for the message processer
        /// </summary>
        /// <param name="rMessageData">INOUT - Message to be processed</param>
        protected override void WndProc(ref Message rMessageData)
        {
            // Start with relaying to the forms message processer
            base.WndProc(ref rMessageData);

            // Check for device messages
            if (USBDeviceNotification.iWM_DEVICECHANGE == rMessageData.Msg)
            {
                switch ((int)rMessageData.WParam)
                {
                    // For both connect and remove, trigger the watchdog to do an update
                    case USBDeviceNotification.iDEVICE_CONNECTED:
                    case USBDeviceNotification.iDEVICE_REMOVED:
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            m_DeviceUpdateComplete.Reset(); // Clear the device update complete flag
                            try
                            {
                                DeviceUpdateThread.ThreadProc(state);
                            }
                            finally
                            {
                                // Signal completion however the update ended, so a failure does not leave
                                // the close waiting for an update that will never report itself complete
                                m_DeviceUpdateComplete.Set();
                            }
                        });
                        break;

                    // Ignore any other events
                    default:
                        // Do nothing
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for the simulate toggle
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void SimulateToggle_CheckedChanged(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Take the simulation status from the toggle rather than by inverting what is recorded. The
            // toggle is also set from code, such as when a session is loaded, and inverting the recorded
            // value there disagrees with the control and sets the two of them toggling each other.
            m_Data.Simulated = m_SimulateToggle.Checked;

            // Record the device interface requires initialization
            m_Timer.Initialized = false;

            // If simulating
            if (m_Data.Simulated)
            {
                // Change the Port field to Seed
                this.m_PortLabel.Text = "Seed";
                this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
                this.m_PortComboBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
                this.m_PortComboBox.Text = "0";
            }
            // Otherwise, using device
            else
            {
                // Change the Seed field to Port
                this.m_PortLabel.Text = "Port";
                this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.m_PortComboBox.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
            }
        }

        /// <summary>
        /// Event handler for start button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void StartButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Transition to the running state
            SetRunningState();
        }

        /// <summary>
        /// Event handler for clicking the pause button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void PauseButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Check if pausing or resuming
            if (RngGuiStates.Paused == m_State)
            {
                SetResumedState();
            }
            else if (RngGuiStates.Running == m_State)
            {
                // Ttansition to the paused state
                SetPausedState();
            }
            else
            {
                // Nothing to do if not in the paused or running states
            }
        }

        /// <summary>
        /// Event handler for stop button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void StopButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Transition to the idle state
            SetIdleState();
        }

        /// <summary>
        /// Event handler for reset button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Reset the session data
            m_Data.Reset();

            // Reset the target value combo
            m_TargetComboBox.SelectedIndex = 0;

            // Clear the chart data and add a point so the area is displayed
            m_ResultChart.Clear();

            // Update the timer and average displayed
            m_SessionTimerTextBox.Text = m_Data.SessionTime;
            m_CurrentAverageTextBox.Text = m_sFLOAT_FORMAT;
            m_DataPointsTextBox.Text = m_sINTEGER_FORMAT;
            m_MeanDeviationTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_StandardDeviationTextBox.Text = m_sEXP_FLOAT_FORMAT;
        }

        /// <summary>
        /// Event handler for change to port number selection
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void PortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Invalidate the device interface
            m_Timer.Initialized = false;
        }

        /// <summary>
        /// Event handler for validating the port number
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments</param>
        private void PortTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Discard unused parameters
            _ = sender;

            // Only need to validate when simulating
            if (m_Data.Simulated)
            {
                // Verify a valid number was specified
                bool bValid = int.TryParse(m_PortComboBox.Text, out int iPortNum);

                if (true == bValid)
                {
                    // Limit to positive integer values
                    bValid = (0 <= iPortNum);
                }

                // If the value specified is not a valid number
                if (false == bValid)
                {
                    // Display error in info box
                    m_StatusTextBox.BackColor = System.Drawing.Color.Red;
                    m_StatusTextBox.Text = m_sINVALID_SEED_ERROR;

                    // Cancel the input
                    e.Cancel = true;
                }
                else
                {
                    // Record the port and initialize the interface
                    m_iSeed = iPortNum;
                }
            }
        }

        /// <summary>
        /// Event handler for the file browse button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void FileBrowseButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Check if a session is currently running and warn the user
            if (Running)
            {
                const string sSessionRunningCaption = "Session In Progress";
                const string sSessionRunningMessage = "A session is currently running. Selecting a new data file will end the current session.\n\nDo you want to continue?";
                DialogResult sessionRunningResult = MessageBox.Show(sSessionRunningMessage, sSessionRunningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // If the user chooses to cancel
                if (DialogResult.No == sessionRunningResult)
                {
                    // Do not continue with file selection
                    return;
                }
                else
                {
                    // End the current session
                    EndSession();
                }
            }

            // Do not allow a new session while processing action
            FileBrowseActive = true;

            // Prompt the user to select a file
            string sSelectedFile = null;
            using (SaveFileDialog dataFileSaveDialog = new SaveFileDialog())
            {
                // Generate a default file name with the format: session_YYYYMMDD_HHMMSS.rng
                dataFileSaveDialog.Title = "Create or open a Random Number Generator data file";
                dataFileSaveDialog.FileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}.rng";
                dataFileSaveDialog.Filter = "RNG data files (*.rng)|*.rng|All files (*.*)|*.*";
                dataFileSaveDialog.DefaultExt = "rng";
                dataFileSaveDialog.AddExtension = true;

                // Allow creation of new files and ensure directory exists
                dataFileSaveDialog.CheckFileExists = false;
                dataFileSaveDialog.CheckPathExists = true;
                dataFileSaveDialog.ValidateNames = true;
                dataFileSaveDialog.DereferenceLinks = true;
                dataFileSaveDialog.OverwritePrompt = false; // We'll handle this ourselves since we want to load existing files

                // Show the file save dialog (which allows creating new files)
                DialogResult result = dataFileSaveDialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    sSelectedFile = dataFileSaveDialog.FileName;
                }
            }

            // Check if the selection will have any effect
            FileBrowseActive = (false == String.IsNullOrEmpty(sSelectedFile));
            if (FileBrowseActive)
            {
                FileBrowseActive = (sSelectedFile != m_Data.FilePath);
            }

            // If the file is being changed
            if (FileBrowseActive)
            {
                // A new session file has been selected, so end the current session
                EndSession();

                // Check if the file exists
                bool bFileExists = File.Exists(sSelectedFile);
                if (bFileExists)
                {
                    // Show loading progress
                    ShowLoadingProgress(Path.GetFileName(sSelectedFile));
                    
                    // Load the existing file by setting the file path and triggering a load operation
                    // This approach works through the interface hierarchy rather than directly parsing XML
                    BeginFileLoad();
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            bool bLoadSuccess = LoadExistingSessionFile(sSelectedFile);
                            ReportFileLoadCompleted(() => OnFileLoadCompleted(sSelectedFile, bLoadSuccess));
                        }
                        finally
                        {
                            EndFileLoad();
                        }
                    });
                }
                else
                {
                    // Verify the user wants to start a new session
                    const string sCaption = "Create Session";
                    const string sMessage = "File does not exist. Would you like to create it and begin a new session?";
                    DialogResult confirmResult = MessageBox.Show(sMessage, sCaption, MessageBoxButtons.OKCancel);

                    // If user accepts to create new file
                    if (DialogResult.OK == confirmResult)
                    {
                        // Set the file path in the session data
                        m_Data.FilePath = sSelectedFile;
                        
                        // Update the file display
                        m_FileTextBox.Text = Path.GetFileName(sSelectedFile);
                        
                        // Show success status
                        SetStatusBoxState($" New file selected: {Path.GetFileName(sSelectedFile)}. Ready for new session.", 
                                        System.Drawing.Color.Black, System.Drawing.Color.LightGreen);
                    }

                    // Re-enable session actions for new file scenario
                    FileBrowseActive = false;
                }
            }
        }



        /// <summary>
        /// Event handler for the baseline browse button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void BaselineBrowseButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Prompt the user to select an existing baseline file
            string sSelectedFile = null;
            using (OpenFileDialog baselineFileOpenDialog = new OpenFileDialog())
            {
                baselineFileOpenDialog.Title = "Select a Random Number Generator baseline file";
                baselineFileOpenDialog.Filter = "RNG data files (*.rng)|*.rng|All files (*.*)|*.*";
                baselineFileOpenDialog.DefaultExt = "rng";
                baselineFileOpenDialog.CheckFileExists = true;
                baselineFileOpenDialog.CheckPathExists = true;
                baselineFileOpenDialog.ValidateNames = true;
                baselineFileOpenDialog.DereferenceLinks = true;

                // Show the file open dialog
                DialogResult result = baselineFileOpenDialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    sSelectedFile = baselineFileOpenDialog.FileName;
                }
            }

            // If a file was selected
            if (false == String.IsNullOrEmpty(sSelectedFile))
            {
                // Show loading progress
                ShowLoadingProgress(Path.GetFileName(sSelectedFile));

                // Load the baseline file on a background thread
                BeginFileLoad();
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        bool bLoadSuccess = LoadBaselineFile(sSelectedFile);
                        ReportFileLoadCompleted(() => OnBaselineLoadCompleted(sSelectedFile, bLoadSuccess));
                    }
                    finally
                    {
                        EndFileLoad();
                    }
                });
            }
        }

        /// <summary>
        /// Event handler for the result browse button
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void ResultBrowseButton_Click(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Prompt the user to select an existing result file
            string sSelectedFile = null;
            using (OpenFileDialog resultFileOpenDialog = new OpenFileDialog())
            {
                resultFileOpenDialog.Title = "Select a Random Number Generator result file";
                resultFileOpenDialog.Filter = "RNG data files (*.rng)|*.rng|All files (*.*)|*.*";
                resultFileOpenDialog.DefaultExt = "rng";
                resultFileOpenDialog.CheckFileExists = true;
                resultFileOpenDialog.CheckPathExists = true;
                resultFileOpenDialog.ValidateNames = true;
                resultFileOpenDialog.DereferenceLinks = true;

                // Show the file open dialog
                DialogResult result = resultFileOpenDialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    sSelectedFile = resultFileOpenDialog.FileName;
                }
            }

            // If a file was selected
            if (false == String.IsNullOrEmpty(sSelectedFile))
            {
                // Show loading progress
                ShowLoadingProgress(Path.GetFileName(sSelectedFile));

                // Load the result file on a background thread
                BeginFileLoad();
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        bool bLoadSuccess = LoadResultFile(sSelectedFile);
                        ReportFileLoadCompleted(() => OnResultLoadCompleted(sSelectedFile, bLoadSuccess));
                    }
                    finally
                    {
                        EndFileLoad();
                    }
                });
            }
        }

        /// <summary>
        /// Event handler for the form closing event
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void GeneratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Set the state to terminating
            m_State = RngGuiStates.Terminating;

            // Reset the info box to the message state
            m_StatusTextBox.ForeColor = System.Drawing.Color.Black;
            m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;

            // Make sure any session is closed out. The session is ended directly rather than through the
            // idle transition, which only ends a session while the state is running or paused and so would
            // not end one now that the state is terminating, nor one left open for appending by a load.
            m_StatusTextBox.Text = m_sCLOSE_STOP_SESSION;
            m_Timer.Stop();
            EndSession();

            // Signal the background work to stop and wait for it to complete (10 second timeout)
            m_StatusTextBox.Text = m_sCLOSE_STOP_DEVICE_UPDATE;
            WaitForBackgroundWork();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Implementation of Invoke as needed by the ISynchronizeInvoke interface
        /// </summary>
        public object Invoke(Action method)
        {
            return base.Invoke(method);
        }

        /// <summary>
        /// Waits for the background work to finish while continuing to process messages.
        /// NOTE: The device update and the file loads report their results by invoking on this thread, so
        /// simply blocking here would stop them from being able to finish and hold the close up until the
        /// wait timed out. Waiting for them also keeps the controls alive until nothing is using them.
        /// </summary>
        private void WaitForBackgroundWork()
        {
            // Do not accept any further input while shutting down, as messages are still being processed
            Enabled = false;

            // Wait in slices, processing messages between them so any pending work can complete
            const int iWAIT_SLICE = 50;
            const int iWAIT_TIMEOUT = 10000;
            int iWaited = 0;
            bool bWorkComplete = (m_DeviceUpdateComplete.WaitOne(0) && m_FileLoadComplete.WaitOne(0));
            while ((false == bWorkComplete) && (iWaited < iWAIT_TIMEOUT))
            {
                Application.DoEvents();
                bWorkComplete = (m_DeviceUpdateComplete.WaitOne(iWAIT_SLICE) && m_FileLoadComplete.WaitOne(0));
                iWaited += iWAIT_SLICE;
            }
        }

        /// <summary>
        /// Reports a file that has been loaded for analysis, raising anything that needs saying about it
        /// rather than reporting a plain success
        /// </summary>
        /// <param name="analysis">IN - The analysis the file was loaded into</param>
        /// <param name="sSuccessMessage">IN - The message to display when there is nothing to raise</param>
        private void ReportAnalysisLoaded(StatisticalAnalysis analysis, string sSuccessMessage)
        {
            // A file can load and still need something raising about it, such as having been recovered
            string sLoadWarning = (null == analysis) ? string.Empty : analysis.LoadWarning;
            bool bWarningReported = (false == string.IsNullOrEmpty(sLoadWarning));
            if (bWarningReported)
            {
                SetStatusBoxState(sLoadWarning, System.Drawing.Color.Black, System.Drawing.Color.Khaki);
            }
            else
            {
                SetStatusBoxState(sSuccessMessage, System.Drawing.Color.Black, System.Drawing.Color.LightGreen);
            }
        }

        /// <summary>
        /// Records that a file load has started so the form is not disposed while it is running
        /// </summary>
        private void BeginFileLoad()
        {
            int iActiveLoads = Interlocked.Increment(ref m_iActiveFileLoads);
            if (1 == iActiveLoads)
            {
                m_FileLoadComplete.Reset();
            }
        }

        /// <summary>
        /// Records that a file load has finished
        /// </summary>
        private void EndFileLoad()
        {
            int iActiveLoads = Interlocked.Decrement(ref m_iActiveFileLoads);
            if (0 >= iActiveLoads)
            {
                m_FileLoadComplete.Set();
            }
        }

        /// <summary>
        /// Reports the result of a file load on the GUI thread, unless the form is being closed
        /// </summary>
        /// <param name="loadCompleted">IN - The action that reports the result of the load</param>
        private void ReportFileLoadCompleted(Action loadCompleted)
        {
            // Nothing is reported once the form is closing, as the controls are about to be disposed
            if (RngGuiStates.Terminating == m_State)
            {
                return;
            }

            // Update the UI on the main thread
            if (InvokeRequired)
            {
                Invoke(loadCompleted);
            }
            else
            {
                loadCompleted();
            }
        }

        /// <summary>
        /// Override of the dispose method from Form. Declared here rather than in the designer file so the
        /// state is recorded no matter which reference the form is disposed through.
        /// </summary>
        /// <param name="disposing">IN - True when disposing managed resources</param>
        protected override void Dispose(bool disposing)
        {
            // Record the GUI is terminating so any running device update stops touching the controls
            m_State = RngGuiStates.Terminating;

            if (disposing && (null != components))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Implementation of GetStatusBoxState to retrieve status box state
        /// </summary>
        public void GetStatusBoxState(out string sText, out Color textColor, out Color backColor)
        {
            // Initialize output parameters
            sText = string.Empty;
            textColor = System.Drawing.Color.Black;
            backColor = System.Drawing.SystemColors.Info;

            // Get UI state on the main thread
            if (InvokeRequired)
            {
                string tempText = string.Empty;
                Color tempTextColor = System.Drawing.Color.Black;
                Color tempBackColor = System.Drawing.SystemColors.Info;

                Invoke(new Action(() =>
                {
                    tempText = m_StatusTextBox.Text;
                    tempTextColor = m_StatusTextBox.ForeColor;
                    tempBackColor = m_StatusTextBox.BackColor;
                }));

                sText = tempText;
                textColor = tempTextColor;
                backColor = tempBackColor;
            }
            else
            {
                sText = m_StatusTextBox.Text;
                textColor = m_StatusTextBox.ForeColor;
                backColor = m_StatusTextBox.BackColor;
            }
        }

        /// <summary>
        /// Updates the status box based on the device read status
        /// /// <param name="fResult">IN - The result of the read (double max indicates error)</param>
        public void RecordReadResult(double fResult)
        {
            // Lock the status box object
            lock (m_StatusTextBox)
            {
                // If there was a read error
                if (double.MaxValue == fResult)
                {
                    // Abort the current run
                    SetIdleState();

                    // Set the status box text and color based on the status
                    SetStatusBoxError(m_sDEVICE_READ_ERROR);
                }
                // Read was successful
                else
                {
                    try
                    {
                        // Record the new data point (this will trigger the chart update via callback)
                        RecordDataPoint(fResult);

                        // Update the displayed average
                        m_CurrentAverageTextBox.Text = CurrentAverage;
                        m_DataPointsTextBox.Text = NumDataPoints;
                        m_MeanDeviationTextBox.Text = MeanDeviation;
                        m_StandardDeviationTextBox.Text = StandardDeviation;

                        // Clear any displayed errors 
                        SetStatusBoxState(RunningMessage, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);
                    }
                    catch (InvalidOperationException invalidOpEx)
                    {
                        // Handle file/operation errors and display in status bar
                        SetIdleState();
                        SetStatusBoxError(invalidOpEx.Message);
                    }
                    catch (UnauthorizedAccessException accessEx)
                    {
                        // Handle file access errors and display in status bar
                        SetIdleState();
                        SetStatusBoxError(accessEx.Message);
                    }
                    catch (System.IO.IOException ioEx)
                    {
                        // Handle file I/O errors and display in status bar
                        SetIdleState();
                        SetStatusBoxError($" File I/O error: {ioEx.Message}");
                    }
                    catch (Exception generalEx)
                    {
                        // Handle any other errors and display in status bar
                        SetIdleState();
                        SetStatusBoxError($" Unexpected error recording data: {generalEx.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Callback method invoked when a data point is added to the session data
        /// This method updates the chart in a thread-safe manner
        /// </summary>
        /// <param name="fDataPoint">IN - The data point that was added</param>
        /// <param name="fCurrentAverage">IN - The current average after adding the data point</param>
        private void OnDataPointAdded(double fDataPoint, double fCurrentAverage)
        {
            // Update the chart on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => m_ResultChart.AddPoint(fDataPoint, fCurrentAverage)));
            }
            else
            {
                m_ResultChart.AddPoint(fDataPoint, fCurrentAverage);
            }
        }

        /// <summary>
        /// Initializes the thread pool
        /// </summary>
        private void InitializeThreadPool()
        {
            // Set the maximum number of threads for the thread pool
            const int iMAX_WORKERS = 6;
            const int iMAX_COMPLETION = 2;
            ThreadPool.SetMaxThreads(iMAX_WORKERS, iMAX_COMPLETION); // Use no more than 8 parallel executions

            // Set the minimum number of threads to the minimum needed for normal execution
            // Workers = 1 for the device update, 1 for updating data set, 1 for file I/O
            // Completion - None currently used
            const int iMIN_WORKS = 3;
            const int iMIN_COMPLETION = 0;
            ThreadPool.SetMinThreads(iMIN_WORKS, iMIN_COMPLETION);
        }

        /// <summary>
        /// Initializes the interface to the device
        /// NOTE1: Port number data member must be set before calling.
        /// NOTE2: Success can be determined using initialized data member
        /// </summary>
        private void InitializeInterface()
        {
            // Set the wait cursor
            Cursor.Current = Cursors.WaitCursor;

            // Attempt to initialize the interface
            int iPort = GetSelectedPort();
            bool bDeviceInitialized = m_Timer.InitializeDevice(iPort, m_Data.Simulated);

            // Initialization failed
            if (false == bDeviceInitialized)
            {
                // Most likely issue is that the device is not at the specified port number
                SetStatusBoxError(m_sDEVICE_INIT_ERROR);
            }
            else
            {
                // Initialized successfully to clear any displayed errors
                SetStatusBoxState(m_sINIT_MESSAGE, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);
            }

            // Restore the cursor
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Checks for an notifies the user if the target value has changed
        /// </summary>
        /// <returns>true if the target has changed; otherwise, false</returns>
        private bool CheckTargetChanged()
        {
            bool bTargetChanged = false;

            // If no target has been set
            if (TargetValues.NO_VALUE_SET == m_Data.TargetValue)
            {
                // Record the current selection
                m_Data.TargetValue = SelectedTarget;
            }
            // Otherwise, if the target has changed
            else if (SelectedTarget != m_Data.TargetValue)
            {
                // Notify the user and prompt if they would like to accept or revert the change
                string sTargetChangedCaption = "Target Changed";
                string sTargetChangedMessage = "The target has changed from " + TargetValues.ToString(m_Data.TargetValue) + " to " +
                    SelectedTarget.ToString() + "\n\nAccept change?";
                MessageBoxButtons TargetChangedButtons = MessageBoxButtons.YesNo;
                DialogResult TargetChangedResult = MessageBox.Show(sTargetChangedMessage, sTargetChangedCaption, TargetChangedButtons);

                // If the user accepts the change
                if (DialogResult.Yes == TargetChangedResult)
                {
                    // Record the change
                    bTargetChanged = true;
                    m_Data.TargetValue = SelectedTarget;
                }
                // If the user does not accept the change
                else
                {
                    // Revert the change to the combo box
                    m_TargetComboBox.SelectedItem = m_Data.TargetValue;
                }
            }

            return bTargetChanged;
        }

        /// <summary>
        /// Gets the selected port (or seed) value
        /// </summary>
        /// <returns></returns>
        private int GetSelectedPort()
        {
            int iPort = 0;

            // If simulating
            if (m_Data.Simulated)
            {
                // Seed member is updated when validating entry, so it can just be returned
                iPort = m_iSeed;
            }
            // Otherwise, using device
            else
            {
                // Get the selected value (default to 0 if no selection)
                iPort = (m_PortComboBox.SelectedValue is null) ? 0 : (int)m_PortComboBox.SelectedValue;
            }

            return iPort;
        }

        /// <summary>
        /// Sets the GUI to the session running state
        /// </summary>
        private void SetRunningState()
        {
            // Set the state
            m_State = RngGuiStates.Running;

            // Disable the interface controls
            m_SimulateToggle.Enabled = false;
            m_PortComboBox.Enabled = false;

            // Disable the target number field
            m_TargetComboBox.Enabled = false;

            // Disable the file browser
            m_FileBrowseButton.Enabled = false;

            // Check if the target number has changed
            CheckTargetChanged();

            // Clear the chart
            m_ResultChart.Clear();

            // Update button statuses
            m_StartButton.Enabled = false;
            m_StopButton.Enabled = true;
            m_PauseButton.Enabled = true;

            // Start a new session (will continue existing session if already in progress)
            StartSession();
        }

        /// <summary>
        /// Sets the GUI to the idle state
        /// </summary>
        private void SetIdleState()
        {
            // End the current session if running
            if ((RngGuiStates.Running == m_State) || (RngGuiStates.Paused == m_State))
            {
                EndSession();
            }

            // Set the state only if not terminating
            if (RngGuiStates.Terminating != m_State)
            {
                m_State = RngGuiStates.Idle;
            }

            // Update the button statuses
            m_StartButton.Enabled = true;
            m_StopButton.Enabled = false;
            m_PauseButton.Enabled = false;

            // Reset the pause button
            m_PauseButton.Text = m_sPAUSE_BUTTON;

            // Enable the file browser when not running a session
            m_FileBrowseButton.Enabled = true;

            // Target number field stays disabled until the session is ended

            // Enable the interface controls
            m_SimulateToggle.Enabled = true;
            m_PortComboBox.Enabled = true;

            // Update the info box
            SetStatusBoxState(m_sIDLE_MESSAGE, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);
        }

        /// <summary>
        /// Sets the GUI to the paused state
        /// </summary>
        private void SetPausedState()
        {
            // Set the state
            m_State = RngGuiStates.Paused;

            // Change the button to resume
            m_PauseButton.Text = m_sRESUME_BUTTON;

            // Disable the timers
            m_Timer.Enabled = false;
            m_Data.PauseSession();

            // Update the info box message
            SetStatusBoxState(m_sPAUSED_MESSAGE, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);
        }

        /// <summary>
        /// Sets the GUI to the resumed state
        /// </summary>
        private void SetResumedState()
        {
            // Set the state
            m_State = RngGuiStates.Running;

            // Change the button back to pause
            m_PauseButton.Text = m_sPAUSE_BUTTON;

            // Re-enable the timers
            m_Timer.Enabled = true;
            m_Data.ResumeSession();

            // Update the info box message
            SetStatusBoxState(m_sIDLE_MESSAGE, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);
        }

        /// <summary>
        /// Start a new session if one is not already in progress
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        private bool StartSession()
        {
            bool bStatus = true;

            try
            {
                // If the device has not been initialized
                if (false == m_Timer.Initialized)
                {
                    // Initialize the device interface
                    InitializeInterface();
                }

                // Update the info box after initializing the device, which updates the status box as well
                SetStatusBoxState(RunningMessage, System.Drawing.Color.Black, System.Drawing.SystemColors.Info);

                // Start a new data session
                bStatus = m_Data.StartSession();

                // Start the read timer
                m_Timer.Start();
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Handle file/operation errors and display in status bar
                SetStatusBoxError(invalidOpEx.Message);
                bStatus = false;
            }
            catch (UnauthorizedAccessException accessEx)
            {
                // Handle file access errors and display in status bar
                SetStatusBoxError(accessEx.Message);
                bStatus = false;
            }
            catch (System.IO.IOException ioEx)
            {
                // Handle file I/O errors and display in status bar
                SetStatusBoxError($" File I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors and display in status bar
                SetStatusBoxError($" Unexpected error starting session: {generalEx.Message}");
                bStatus = false;
            }

            // If session start failed, ensure we're in idle state
            if (!bStatus)
            {
                SetIdleState();
            }

            return bStatus;
        }

        /// <summary>
        /// Ends the current session, if one is in progress
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        private bool EndSession()
        {
            // Default to true as the call is successful if nothings needs to be done
            bool bStatus = true;

            // Stop the read timer if running
            m_Timer.Stop();

            // End the current session and clear the selected data file
            m_Data.EndSession();
            m_FileTextBox.Text = string.Empty;

            // Enable the target number field for the next session
            m_TargetComboBox.Enabled = true;

            return bStatus;
        }

        /// <summary>
        /// Records a new data point
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point</param>
        /// <returns>true, if successful; otherwise false</returns>
        private bool RecordDataPoint(double fDataPoint)
        {
            // Record the data point and check for write
            bool bStatus = m_Data.AddDataPoint(fDataPoint);

            return bStatus;
        }

        /// <summary>
        /// Debugging utility to dump the USB device information
        /// </summary>
        private void DumpUSBDevices()
        {
            // Dump all device information
            using (System.IO.StreamWriter deviceDumpFile = new System.IO.StreamWriter("USBDevices.txt"))
            {
                // Search for all USB controller devices
                ManagementObjectSearcher controllerSearcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice");
                ManagementObjectCollection controllerCollection = controllerSearcher.Get();

                foreach (ManagementBaseObject controller in controllerCollection)
                {
                    // Get the dependent device for the controller
                    string sDependent = (string)controller.GetPropertyValue("Dependent");
                    string[] sDependentSplit = System.Text.RegularExpressions.Regex.Split(sDependent, "DeviceID=");
                    string sDeviceID = sDependentSplit[1];//.Replace("\"", "");

                    ManagementObjectSearcher deviceSearcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity Where DeviceID=" + sDeviceID);
                    ManagementObjectCollection deviceCollection = deviceSearcher.Get();
                    foreach (ManagementBaseObject device in deviceCollection)
                    {
                        deviceDumpFile.WriteLine("\nBegin Device:");
                        deviceDumpFile.WriteLine("uint16 Availability " + ((device.GetPropertyValue("Availability") == null) ? "null" : device.GetPropertyValue("Availability").ToString()));
                        deviceDumpFile.WriteLine("string Caption " + ((device.GetPropertyValue("Caption") == null) ? "null" : device.GetPropertyValue("Caption").ToString()));
                        deviceDumpFile.WriteLine("string ClassGuid " + ((device.GetPropertyValue("ClassGuid") == null) ? "null" : device.GetPropertyValue("ClassGuid").ToString()));
                        deviceDumpFile.WriteLine("string CompatibleID[] " + ((device.GetPropertyValue("CompatibleID") == null) ? "null" : device.GetPropertyValue("CompatibleID").ToString()));
                        deviceDumpFile.WriteLine("uint32 ConfigManagerErrorCode " + ((device.GetPropertyValue("ConfigManagerErrorCode") == null) ? "null" : device.GetPropertyValue("ConfigManagerErrorCode").ToString()));
                        deviceDumpFile.WriteLine("boolean ConfigManagerUserConfig " + ((device.GetPropertyValue("ConfigManagerUserConfig") == null) ? "null" : device.GetPropertyValue("ConfigManagerUserConfig").ToString()));
                        deviceDumpFile.WriteLine("string CreationClassName " + ((device.GetPropertyValue("CreationClassName") == null) ? "null" : device.GetPropertyValue("CreationClassName").ToString()));
                        deviceDumpFile.WriteLine("string Description " + ((device.GetPropertyValue("Description") == null) ? "null" : device.GetPropertyValue("Description").ToString()));
                        deviceDumpFile.WriteLine("string DeviceID " + ((device.GetPropertyValue("DeviceID") == null) ? "null" : device.GetPropertyValue("DeviceID").ToString()));
                        deviceDumpFile.WriteLine("boolean ErrorCleared " + ((device.GetPropertyValue("ErrorCleared") == null) ? "null" : device.GetPropertyValue("ErrorCleared").ToString()));
                        deviceDumpFile.WriteLine("string ErrorDescription " + ((device.GetPropertyValue("ErrorDescription") == null) ? "null" : device.GetPropertyValue("ErrorDescription").ToString()));
                        deviceDumpFile.WriteLine("string HardwareID[] " + ((device.GetPropertyValue("HardwareID") == null) ? "null" : device.GetPropertyValue("HardwareID").ToString()));
                        deviceDumpFile.WriteLine("datetime InstallDate " + ((device.GetPropertyValue("InstallDate") == null) ? "null" : device.GetPropertyValue("InstallDate").ToString()));
                        deviceDumpFile.WriteLine("uint32 LastErrorCode " + ((device.GetPropertyValue("LastErrorCode") == null) ? "null" : device.GetPropertyValue("LastErrorCode").ToString()));
                        deviceDumpFile.WriteLine("string Manufacturer " + ((device.GetPropertyValue("Manufacturer") == null) ? "null" : device.GetPropertyValue("Manufacturer").ToString()));
                        deviceDumpFile.WriteLine("string Name " + ((device.GetPropertyValue("Name") == null) ? "null" : device.GetPropertyValue("Name").ToString()));
                        deviceDumpFile.WriteLine("string PNPClass " + ((device.GetPropertyValue("PNPClass") == null) ? "null" : device.GetPropertyValue("PNPClass").ToString()));
                        deviceDumpFile.WriteLine("string PNPDeviceID " + ((device.GetPropertyValue("PNPDeviceID") == null) ? "null" : device.GetPropertyValue("PNPDeviceID").ToString()));
                        deviceDumpFile.WriteLine("uint16 PowerManagementCapabilities[] " + ((device.GetPropertyValue("PowerManagementCapabilities") == null) ? "null" : device.GetPropertyValue("PowerManagementCapabilities").ToString()));
                        deviceDumpFile.WriteLine("boolean PowerManagementSupported " + ((device.GetPropertyValue("PowerManagementSupported") == null) ? "null" : device.GetPropertyValue("PowerManagementSupported").ToString()));
                        deviceDumpFile.WriteLine("boolean Present " + ((device.GetPropertyValue("Present") == null) ? "null" : device.GetPropertyValue("Present").ToString()));
                        deviceDumpFile.WriteLine("string Service " + ((device.GetPropertyValue("Service") == null) ? "null" : device.GetPropertyValue("Service").ToString()));
                        deviceDumpFile.WriteLine("string Status " + ((device.GetPropertyValue("Status") == null) ? "null" : device.GetPropertyValue("Status").ToString()));
                        deviceDumpFile.WriteLine("uint16 StatusInfo " + ((device.GetPropertyValue("StatusInfo") == null) ? "null" : device.GetPropertyValue("StatusInfo").ToString()));
                        deviceDumpFile.WriteLine("string SystemCreationClassName " + ((device.GetPropertyValue("SystemCreationClassName") == null) ? "null" : device.GetPropertyValue("SystemCreationClassName").ToString()));
                        deviceDumpFile.WriteLine("string SystemName " + ((device.GetPropertyValue("SystemName") == null) ? "null" : device.GetPropertyValue("SystemName").ToString()));
                        deviceDumpFile.WriteLine("uint32 ConfigManagerErrorCode " + ((device.GetPropertyValue("ConfigManagerErrorCode") == null) ? "null" : device.GetPropertyValue("ConfigManagerErrorCode").ToString()));
                        deviceDumpFile.WriteLine("End Device");
                    }
                }
            }
        }

        /// <summary>
        /// Loads session data from an existing file by working through the interface hierarchy
        /// </summary>
        /// <param name="sFilePath">IN - Path to the file to load</param>
        /// <returns>true if successful; otherwise, false</returns>
        private bool LoadExistingSessionFile(string sFilePath)
        {
            bool bStatus = false;
            
            try
            {
                // Load the session through the interface hierarchy
                bStatus = m_Data.LoadSession(sFilePath);
            }
            catch (IOException fileIOException)
            {
                // Display file I/O error using helper method
                SetStatusBoxError($" File I/O error accessing file {Path.GetFileName(sFilePath)}: {fileIOException.Message}");
                bStatus = false;
            }
            catch (Exception generalException)
            {
                // Display general error using helper method
                SetStatusBoxError($" Unexpected error accessing file {Path.GetFileName(sFilePath)}: {generalException.Message}");
                bStatus = false;
            }

            return bStatus;
        }

        /// <summary>
        /// Called when file loading operation completes (on UI thread)
        /// </summary>
        /// <param name="sFilePath">IN - Path to the file that was loaded</param>
        /// <param name="bSuccess">IN - Whether the load operation succeeded</param>
        private void OnFileLoadCompleted(string sFilePath, bool bSuccess)
        {
            // Hide the progress bar
            HideLoadingProgress();

            // Re-enable UI controls
            FileBrowseActive = false;

            if (bSuccess)
            {
                // Update the file display
                m_FileTextBox.Text = Path.GetFileName(sFilePath);
                
                // Update all UI elements with the loaded session data
                UpdateUIFromLoadedSession();

                // A file can load successfully and still have something worth reporting, such as having been
                // recovered after the application was stopped while recording
                string sLoadWarning = m_Data.LastError;
                bool bWarningReported = (false == string.IsNullOrEmpty(sLoadWarning));
                if (bWarningReported)
                {
                    // Show the warning rather than the plain success message
                    SetStatusBoxState(sLoadWarning, System.Drawing.Color.Black, System.Drawing.Color.Khaki);
                }
                else
                {
                    // Show success status using helper method
                    SetStatusBoxState($" File loaded: {Path.GetFileName(sFilePath)}. {m_Data.NumDataPoints} data points loaded. Ready for new session.",
                                    System.Drawing.Color.Black, System.Drawing.Color.LightGreen);
                }
            }
            else
            {
                // Show error status using helper method
                SetStatusBoxError($" Error accessing file {Path.GetFileName(sFilePath)}. Please verify the file exists and is readable.");
            }
        }

        /// <summary>
        /// Updates all UI elements with data from the loaded session
        /// </summary>
        private void UpdateUIFromLoadedSession()
        {
            // Update the statistics display fields
            m_CurrentAverageTextBox.Text = CurrentAverage;
            m_DataPointsTextBox.Text = NumDataPoints;
            m_MeanDeviationTextBox.Text = MeanDeviation;
            m_StandardDeviationTextBox.Text = StandardDeviation;
            m_SessionTimerTextBox.Text = m_Data.SessionTime;

            // Update the target combo box if a target value was loaded
            if (m_Data.TargetValue != TargetValues.NO_VALUE_SET)
            {
                SelectedTarget = m_Data.TargetValue;
            }

            // Update the simulation toggle to match the loaded session
            m_SimulateToggle.Checked = m_Data.Simulated;

            // Note: Chart is automatically updated during file loading via DataPointAddedCallback
            // No need to manually repopulate the chart as it's updated in real-time during the load process
        }

        /// <summary>
        /// Shows a progress bar during file loading operations
        /// </summary>
        private void ShowLoadingProgress(string fileName)
        {
            // Update status to show loading in progress
            SetStatusBoxState($" Loading file: {fileName}...", 
                            System.Drawing.Color.Black, System.Drawing.Color.LightBlue);
            
            // Set cursor to wait cursor to indicate loading
            Cursor.Current = Cursors.WaitCursor;
            this.Cursor = Cursors.WaitCursor;
        }

        /// <summary>
        /// Hides the progress indicator when file loading completes
        /// </summary>
        private void HideLoadingProgress()
        {
            // Restore normal cursor
            Cursor.Current = Cursors.Default;
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Updates the status box state in a thread-safe manner
        /// </summary>
        /// <param name="sText">IN - Text to display in the status box</param>
        /// <param name="textColor">IN - Text color to set for the status box</param>
        /// <param name="backColor">IN - Background color to set for the status box</param>
        public void SetStatusBoxState(string sText, Color textColor, Color backColor)
        {
            // Update UI on the main thread
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    m_StatusTextBox.Text = sText;
                    m_StatusTextBox.ForeColor = textColor;
                    m_StatusTextBox.BackColor = backColor;
                }));
            }
            else
            {
                m_StatusTextBox.Text = sText;
                m_StatusTextBox.ForeColor = textColor;
                m_StatusTextBox.BackColor = backColor;
            }
        }

        /// <summary>
        /// Updates the status box with error information in a thread-safe manner
        /// </summary>
        /// <param name="sText">IN - Error text to display</param>
        public void SetStatusBoxError(string sText)
        {
            SetStatusBoxState(sText, System.Drawing.Color.Black, System.Drawing.Color.Red);
        }

        /// <summary>
        /// Loads baseline data from an existing file using StatisticalAnalysis
        /// </summary>
        /// <param name="sFilePath">IN - Path to the baseline file to load</param>
        /// <returns>true if successful; otherwise, false</returns>
        private bool LoadBaselineFile(string sFilePath)
        {
            bool bStatus = false;

            try
            {
                // Create a new statistical analysis object for the baseline
                m_BaselineAnalysis = new StatisticalAnalysis();

                // Load the baseline file
                List<double> baselineData = m_BaselineAnalysis.LoadResultFile(sFilePath);

                // Check if data was successfully loaded
                bStatus = (baselineData != null && baselineData.Count > 0);
            }
            catch (ArgumentException argEx)
            {
                // Handle argument validation errors
                SetStatusBoxError($" Baseline file error: {argEx.Message}");
                bStatus = false;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle file not found errors
                SetStatusBoxError($" Baseline file not found: {Path.GetFileName(sFilePath)}");
                bStatus = false;
            }
            catch (System.IO.IOException ioEx)
            {
                // Handle file I/O errors
                SetStatusBoxError($" Baseline file I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Handle file loading/parsing errors
                SetStatusBoxError($" Baseline file loading error: {invalidOpEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors
                SetStatusBoxError($" Unexpected error loading baseline file: {generalEx.Message}");
                bStatus = false;
            }

            return bStatus;
        }

        /// <summary>
        /// Loads result data from an existing file using StatisticalAnalysis
        /// </summary>
        /// <param name="sFilePath">IN - Path to the result file to load</param>
        /// <returns>true if successful; otherwise, false</returns>
        private bool LoadResultFile(string sFilePath)
        {
            bool bStatus = false;

            try
            {
                // Create a new statistical analysis object for the result
                m_ResultAnalysis = new StatisticalAnalysis();

                // Load the result file
                List<double> resultData = m_ResultAnalysis.LoadResultFile(sFilePath);

                // Check if data was successfully loaded
                bStatus = (resultData != null && resultData.Count > 0);
            }
            catch (ArgumentException argEx)
            {
                // Handle argument validation errors
                SetStatusBoxError($" Result file error: {argEx.Message}");
                bStatus = false;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle file not found errors
                SetStatusBoxError($" Result file not found: {Path.GetFileName(sFilePath)}");
                bStatus = false;
            }
            catch (System.IO.IOException ioEx)
            {
                // Handle file I/O errors
                SetStatusBoxError($" Result file I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Handle file loading/parsing errors
                SetStatusBoxError($" Result file loading error: {invalidOpEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors
                SetStatusBoxError($" Unexpected error loading result file: {generalEx.Message}");
                bStatus = false;
            }

            return bStatus;
        }
        /// <summary>
        /// Called when baseline file loading operation completes (on UI thread)
        /// </summary>
        /// <param name="sFilePath">IN - Path to the baseline file that was loaded</param>
        /// <param name="bSuccess">IN - Whether the load operation succeeded</param>
        private void OnBaselineLoadCompleted(string sFilePath, bool bSuccess)
        {
            // Hide the progress indicator
            HideLoadingProgress();

            if (bSuccess)
            {
                // Update the baseline file display
                m_BaselineTextBox.Text = Path.GetFileName(sFilePath);

                // Update all baseline statistical fields with data from the loaded baseline analysis
                UpdateBaselineStatisticalFields();

                // Update the histogram chart with baseline data
                UpdateHistogramChartWithBaseline();

                // Update comparison statistics if both files are loaded
                UpdateComparisonStatistics();

                // Show the success status, or anything that needs raising about the file that was loaded
                ReportAnalysisLoaded(m_BaselineAnalysis,
                                     $" Baseline file loaded: {Path.GetFileName(sFilePath)}. Ready for analysis.");
            }
            else
            {
                // Clear any previous baseline data
                m_BaselineAnalysis = null;
                
                // Clear the baseline UI fields
                ClearBaselineStatisticalFields();
                
                // Show error status - specific error message already set by LoadBaselineFile
                // Keep the existing error message in the status box
            }
        }

        /// <summary>
        /// Called when result file loading operation completes (on UI thread)
        /// </summary>
        /// <param name="sFilePath">IN - Path to the result file that was loaded</param>
        /// <param name="bSuccess">IN - Whether the load operation succeeded</param>
        private void OnResultLoadCompleted(string sFilePath, bool bSuccess)
        {
            // Hide the progress indicator
            HideLoadingProgress();

            if (bSuccess)
            {
                // Update the result file display
                m_ResultTextBox.Text = Path.GetFileName(sFilePath);

                // Update all result statistical fields with data from the loaded result analysis
                UpdateResultStatisticalFields();

                // Update the histogram chart with result data
                UpdateHistogramChartWithResult();

                // Update comparison statistics if both files are loaded
                UpdateComparisonStatistics();

                // Show the success status, or anything that needs raising about the file that was loaded
                ReportAnalysisLoaded(m_ResultAnalysis,
                                     $" Result file loaded: {Path.GetFileName(sFilePath)}. Ready for analysis.");
            }
            else
            {
                // Clear any previous result data
                m_ResultAnalysis = null;
                
                // Clear the result UI fields
                ClearResultStatisticalFields();
                
                // Show error status - specific error message already set by LoadResultFile
                // Keep the existing error message in the status box
            }
        }

        /// <summary>
        /// Updates the histogram chart with baseline data
        /// </summary>
        private void UpdateHistogramChartWithBaseline()
        {
            try
            {
                // Check if we have baseline data
                if (m_BaselineAnalysis?.LoadedFileData != null && m_BaselineAnalysis.LoadedFileData.Count > 0)
                {
                    // Get baseline data
                    List<double> baselineData = m_BaselineAnalysis.LoadedFileData;
                    string baselineLabel = $"Baseline ({m_BaselineAnalysis.LoadedFileName})";

                    // Check if we also have result data for comparison
                    if (m_ResultAnalysis?.LoadedFileData != null && m_ResultAnalysis.LoadedFileData.Count > 0)
                    {
                        // Plot both baseline and result data
                        List<double> resultData = m_ResultAnalysis.LoadedFileData;
                        string resultLabel = $"Result ({m_ResultAnalysis.LoadedFileName})";
                        
                        m_ResultHistogramChart.Plot(baselineData, baselineLabel, resultData, resultLabel);
                    }
                    else
                    {
                        // Plot only baseline data with empty result
                        List<double> emptyResultData = new List<double>();
                        
                        m_ResultHistogramChart.Plot(baselineData, baselineLabel, emptyResultData, "Result (No Data)");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during chart update
                SetStatusBoxError($" Error updating histogram chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the histogram chart with result data
        /// </summary>
        private void UpdateHistogramChartWithResult()
        {
            try
            {
                // Check if we have result data
                if (m_ResultAnalysis?.LoadedFileData != null && m_ResultAnalysis.LoadedFileData.Count > 0)
                {
                    // Get result data
                    List<double> resultData = m_ResultAnalysis.LoadedFileData;
                    string resultLabel = $"Result ({m_ResultAnalysis.LoadedFileName})";

                    // Check if we also have baseline data for comparison
                    if (m_BaselineAnalysis?.LoadedFileData != null && m_BaselineAnalysis.LoadedFileData.Count > 0)
                    {
                        // Plot both baseline and result data
                        List<double> baselineData = m_BaselineAnalysis.LoadedFileData;
                        string baselineLabel = $"Baseline ({m_BaselineAnalysis.LoadedFileName})";
                        
                        m_ResultHistogramChart.Plot(baselineData, baselineLabel, resultData, resultLabel);
                    }
                    else
                    {
                        // Plot only result data with empty baseline
                        List<double> emptyBaselineData = new List<double>();
                        
                        m_ResultHistogramChart.Plot(emptyBaselineData, "Baseline (No Data)", resultData, resultLabel);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during chart update
                SetStatusBoxError($" Error updating histogram chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates all baseline statistical fields with data from the loaded baseline analysis
        /// </summary>
        private void UpdateBaselineStatisticalFields()
        {
            // Verify baseline analysis data is available
            if (m_BaselineAnalysis?.LoadedFileStats != null)
            {
                var stats = m_BaselineAnalysis.LoadedFileStats;

                // Update the baseline statistical display fields using the same format as the main statistics
                m_BaselineMeanTextBox.Text = stats.Mean.ToString(m_sFLOAT_FORMAT);
                m_BaselineStdDevTextBox.Text = stats.StandardDeviation.ToString(m_sEXP_FLOAT_FORMAT);
                m_BaselineSkewnessTextBox.Text = stats.Skewness.ToString(m_sEXP_FLOAT_FORMAT);
                m_BaselineKurtosisTextBox.Text = stats.Kurtosis.ToString(m_sEXP_FLOAT_FORMAT);
            }
            else
            {
                // Clear fields if no data is available
                ClearBaselineStatisticalFields();
            }
        }

        /// <summary>
        /// Clears all baseline statistical fields
        /// </summary>
        private void ClearBaselineStatisticalFields()
        {
            m_BaselineTextBox.Text = string.Empty;
            m_BaselineMeanTextBox.Text = m_sFLOAT_FORMAT;
            m_BaselineStdDevTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_BaselineSkewnessTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_BaselineKurtosisTextBox.Text = m_sEXP_FLOAT_FORMAT;
        }

        /// <summary>
        /// Updates all result statistical fields with data from the loaded result analysis
        /// </summary>
        private void UpdateResultStatisticalFields()
        {
            // Verify result analysis data is available
            if (m_ResultAnalysis?.LoadedFileStats != null)
            {
                var stats = m_ResultAnalysis.LoadedFileStats;

                // Update the result statistical display fields using the same format as the main statistics
                m_ResultMeanTextBox.Text = stats.Mean.ToString(m_sFLOAT_FORMAT);
                m_ResultStdDevTextBox.Text = stats.StandardDeviation.ToString(m_sEXP_FLOAT_FORMAT);
                m_ResultSkewnessTextBox.Text = stats.Skewness.ToString(m_sEXP_FLOAT_FORMAT);
                m_ResultKurtosisTextBox.Text = stats.Kurtosis.ToString(m_sEXP_FLOAT_FORMAT);
            }
            else
            {
                // Clear fields if no data is available
                ClearResultStatisticalFields();
            }
        }

        /// <summary>
        /// Clears all result statistical fields
        /// </summary>
        private void ClearResultStatisticalFields()
        {
            m_ResultTextBox.Text = string.Empty;
            m_ResultMeanTextBox.Text = m_sFLOAT_FORMAT;
            m_ResultStdDevTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_ResultSkewnessTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_ResultKurtosisTextBox.Text = m_sEXP_FLOAT_FORMAT;
        }

        /// <summary>
        /// Updates the comparison statistics fields when both baseline and result files are loaded
        /// </summary>
        private void UpdateComparisonStatistics()
        {
            try
            {
                // Check if both baseline and result data are available
                if (m_BaselineAnalysis?.LoadedFileData != null && m_BaselineAnalysis.LoadedFileData.Count > 0 &&
                    m_ResultAnalysis?.LoadedFileData != null && m_ResultAnalysis.LoadedFileData.Count > 0)
                {
                    // Get the statistical data
                    var baselineStats = m_BaselineAnalysis.LoadedFileStats;
                    var resultStats = m_ResultAnalysis.LoadedFileStats;

                    // Calculate mean difference
                    double fMeanDifference = resultStats.Mean - baselineStats.Mean;
                    m_MeanDifferenceTextBox.Text = fMeanDifference.ToString(m_sEXP_FLOAT_FORMAT);

                    // Calculate skewness difference
                    double fSkewnessDifference = resultStats.Skewness - baselineStats.Skewness;
                    m_SkewnessTextBox.Text = fSkewnessDifference.ToString(m_sEXP_FLOAT_FORMAT);

                    // Update the histogram chart with both datasets
                    List<double> baselineData = m_BaselineAnalysis.LoadedFileData;
                    List<double> resultData = m_ResultAnalysis.LoadedFileData;
                    string baselineLabel = $"Baseline ({m_BaselineAnalysis.LoadedFileName})";
                    string resultLabel = $"Result ({m_ResultAnalysis.LoadedFileName})";
                    
                    m_ResultHistogramChart.Plot(baselineData, baselineLabel, resultData, resultLabel);
                }
                else
                {
                    // Clear comparison fields if both files are not loaded
                    ClearComparisonFields();
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during comparison update
                SetStatusBoxError($" Error updating comparison statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all comparison statistical fields
        /// </summary>
        private void ClearComparisonFields()
        {
            m_MeanDifferenceTextBox.Text = m_sEXP_FLOAT_FORMAT;
            m_SkewnessTextBox.Text = m_sEXP_FLOAT_FORMAT;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Whether the system is busy processing a change and should prevent session changes.
        /// NOTE: Exception is thrown by set if a session is currently running.
        /// </summary>
        public bool FileBrowseActive
        {
            get => m_bFileBrowseActive;
            set
            {
                // Do not allow changes while a session is running
                if (Running)
                {
                    throw new InvalidOperationException("Busy property cannot be changed while a session is running.");
                }
                else
                {
                    // Record the state change
                    m_bFileBrowseActive = value;

                    // If setting as busy
                    if (m_bFileBrowseActive)
                    {
                        // Disable the browse, start, and clear buttons
                        m_FileBrowseButton.Enabled = false;
                        m_StartButton.Enabled = false;
                        m_ClearButton.Enabled = false;
                    }
                    // Changes are complete
                    else
                    {
                        // Enable the browse, start, and clear buttons
                        m_FileBrowseButton.Enabled = true;
                        m_StartButton.Enabled = true;
                        m_ClearButton.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Binding list of devices used to populate the port combo box list (write-only)
        /// </summary>
        public BindingList<IRNGDevice> DeviceList
        {
            set
            {
                // Update and rebind the list
                m_DeviceBindingSource.DataSource = value;
                m_PortComboBox.DataSource = m_DeviceBindingSource.DataSource;
            }
        }

        /// <summary>
        /// Whether a session is currently running (read-only)
        /// </summary>
        public bool Running { get => m_Data.InProgress; }

        /// <summary>
        /// Background color of the status box
        /// </summary>
        public Color StatusBoxBackColor { get => m_StatusTextBox.BackColor; set => m_StatusTextBox.BackColor = value; }

        /// <summary>
        /// Text displayed in the statatus box
        /// </summary>
        public string StatusBoxText { get => m_StatusTextBox.Text; set => m_StatusTextBox.Text = value; }

        /// <summary>
        /// Text color of the status box
        /// </summary>
        public Color StatusBoxTextColor { get => m_StatusTextBox.ForeColor; set => m_StatusTextBox.ForeColor = value; }

        /// <summary>
        /// The current state of the GUI (read-only)
        /// </summary>
        public RngGuiStates State { get => m_State; }

        /// <summary>
        /// Current average string (read-only)
        /// </summary>
        private string CurrentAverage { get => m_Data.CurrentAverage.ToString(m_sFLOAT_FORMAT); }

        /// <summary>
        /// Number of data points gathered
        /// </summary>
        private string NumDataPoints { get => m_Data.NumDataPoints.ToString(m_sINTEGER_FORMAT); }

        /// <summary>
        /// Deviation from the statistical mean
        /// </summary>
        private string MeanDeviation { get => m_Data.MeanDeviation.ToString(m_sEXP_FLOAT_FORMAT); }

        /// <summary>
        /// Standard deviation of the data set
        /// </summary>
        private string StandardDeviation { get => m_Data.StandardDeviation.ToString(m_sEXP_FLOAT_FORMAT); }

        /// <summary>
        /// Message to display in the info box while a session is running (read-only)
        /// </summary>
        private string RunningMessage
        {
            get { return $"Running with Target = {m_TargetComboBox.SelectedItem}..."; }
        }

        /// <summary>
        /// Selected target value
        /// </summary>
        private int SelectedTarget
        {
            get { return TargetValues.GetValueAt((uint)m_TargetComboBox.SelectedIndex); }
            set { m_TargetComboBox.SelectedItem = TargetValues.ToString(value); }
        }

        #endregion
        #region Data Members

        // Form status and session data
        private bool m_bFileBrowseActive = false;
        private IRNGSessionData m_Data = null;

        // Device timer
        private IRNGDeviceTimer m_Timer = null;

        // Current state of the GUI
        private RngGuiStates m_State = RngGuiStates.Idle;

        // Device settings
        private int m_iSeed = 0;
        private BindingSource m_DeviceBindingSource;
        private ManualResetEvent m_DeviceUpdateComplete = new ManualResetEvent(false);

        // Tracks file loads running in the background so the form is not disposed while one is using it
        private ManualResetEvent m_FileLoadComplete = new ManualResetEvent(true);
        private int m_iActiveFileLoads = 0;

        // Statistical analysis for baseline data
        private StatisticalAnalysis m_BaselineAnalysis = null;

        // Statistical analysis for result data
        private StatisticalAnalysis m_ResultAnalysis = null;

        // Display settings
        private const string m_sFLOAT_FORMAT = "0.000000000";
        private const string m_sINTEGER_FORMAT = "0";
        private const string m_sEXP_FLOAT_FORMAT = "0.000000e0";

        // Button text
        internal const string m_sPAUSE_BUTTON = "PAUSE";
        internal const string m_sRESUME_BUTTON = "RESUME";

        // Status and error messages for the info box
        private const string m_sINVALID_SEED_ERROR = " Specified seed is not valid. Must be positive integer. Reset to last valid value.";
        private const string m_sDEVICE_INIT_ERROR = " Error initializing TruRNGpro. Please verify device is connected and correct COM port is selected.";
        private const string m_sDEVICE_READ_ERROR = " Error reading from TruRNGpro. Please verify device is connected and correct COM port is selected.";
        private const string m_sINIT_MESSAGE = " Initialized";
        private const string m_sIDLE_MESSAGE = " Idle";
        private const string m_sPAUSED_MESSAGE = " Paused";
        private const string m_sCLOSE_STOP_SESSION = " Ending current session...";
        private const string m_sCLOSE_STOP_DEVICE_UPDATE = " Waiting for USB device search to end...";
        private const string m_sFILE_LOAD_ERROR = " Error loading file. Please verify the file format is correct.";

        #endregion
    }
}
