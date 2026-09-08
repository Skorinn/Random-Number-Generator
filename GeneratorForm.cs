//*********************************************************************************************************************
// File Name:      GeneratorForm.cs
// Description:    Implementation of the Random Number Generator GUI
//
// Copyright (c) 2022-2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2022/09/10 - Mike Pullen - Original implementation.
// 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
// 2023/12/02 - Mike Pullen - Added simulate, pause, and target value
// 2026/08/31 - Mike Pullen - Report recovered files, dispose through the standard pattern, and keep processing
//                            messages while waiting for the device update to finish on close
// 2026/09/07 - Mike Pullen - Reworked the presentation: status bar, statistic readouts, number formats, and
//                            one emphasised button per state
// 2026/09/07 - Mike Pullen - Session in the window title, a status message that says what to do next, and
//                            no value shown for a measure nothing has been measured for yet
// 2026/09/07 - Mike Pullen - Report whether the two analysed sessions differ by more than noise
//*********************************************************************************************************************

// Enable to dump the USB device information
//#define DUMP_DEVICES

using DeviceInterfaces;
using MathNet.Numerics.Statistics;
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

            // Build the button fonts from the one the designer set, so the emphasis follows the design rather
            // than naming a family and size a second time
            m_ActionFontRegular = m_PauseButton.Font;
            m_ActionFontBold = new Font(m_ActionFontRegular, FontStyle.Bold);
            m_VerdictFontRegular = m_VerdictLabel.Font;
            m_VerdictFontBold = new Font(m_VerdictFontRegular, FontStyle.Bold);

            // The height a combo box picks for itself in the dropped down style, which is what the setup row
            // was laid out around. The simple style used for the seed asks for a taller control than that.
            m_iFieldHeight = m_PortComboBox.Height;

            // Colour the controls the designer laid out, and emphasise the button that starts a session
            ApplyTheme();
            SetPrimaryButton(m_StartButton);

            // Lay out the comparison table and fill it in, which puts the verdict into the state that says
            // what has to be loaded before there is anything to compare
            BuildComparisonTable();
            UpdateComparisonTable();

            // Restore the window to where it was left
            RestoreWindowPlacement();

            // Set the info box to idle
            m_StatusLabel.Text = IdleMessage;
            m_StatusLabel.ForeColor = StatusPalette.NormalText;
            m_StatusLabel.BackColor = StatusPalette.NormalBackground;
            m_StatusIndicator.ForeColor = StatusPalette.NormalText;

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
        /// Event handler for the comparison table being resized, which happens whenever the window is
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void ComparisonList_SizeChanged(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            SizeComparisonColumns();
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
                this.m_PortLabel.Text = m_sSEED_LABEL;
                this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
                this.m_PortComboBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
                this.m_PortComboBox.Text = "0";
            }
            // Otherwise, using device
            else
            {
                // Change the Seed field to Port
                this.m_PortLabel.Text = m_sPORT_LABEL;
                this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.m_PortComboBox.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
            }

            // A combo box in the simple style is a text box with a list under it, and it takes a height to
            // fit both. Left at that it is taller than the row it sits in, so its bottom border falls outside
            // and the field looks unfinished. Put it back to the height the dropped down style chooses for
            // itself, which is the height the row was laid out for.
            this.m_PortComboBox.Height = m_iFieldHeight;
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

            // Clearing throws away everything recorded so far and cannot be undone, so ask first. It sits
            // beside the buttons that run a session and is the only one of them that destroys anything.
            const string sCaption = "Clear Session";
            const string sMessage = "This discards the readings shown and resets the statistics.\n\nContinue?";
            DialogResult clearResult = MessageBox.Show(sMessage, sCaption, MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (DialogResult.Yes != clearResult)
            {
                return;
            }

            // Reset the session data
            m_Data.Reset();

            // Reset the target value combo
            m_TargetComboBox.SelectedIndex = 0;

            // Clear the chart data and add a point so the area is displayed
            m_ResultChart.Clear();

            // Show the values the reset has left behind rather than a set of fixed strings, so the readouts
            // cannot say something the session data does not
            UpdateStatisticsDisplay();
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
                    SetStatusBoxError(m_sINVALID_SEED_ERROR);

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
                        
                        // Update the file display and the window title, which carries the file too
                        m_FileTextBox.Text = Path.GetFileName(sSelectedFile);
                        UpdateWindowTitle();

                        // Show success status
                        SetStatusBoxState($"New file selected: {Path.GetFileName(sSelectedFile)}. Ready for new session.", 
                                        StatusPalette.SuccessText, StatusPalette.SuccessBackground);
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

            // Record where the window was left, before it is taken down
            SaveWindowPlacement();

            // Reset the info box to the message state
            m_StatusLabel.ForeColor = StatusPalette.NormalText;
            m_StatusLabel.BackColor = StatusPalette.NormalBackground;
            m_StatusIndicator.ForeColor = StatusPalette.NormalText;

            // Make sure any session is closed out. The session is ended directly rather than through the
            // idle transition, which only ends a session while the state is running or paused and so would
            // not end one now that the state is terminating, nor one left open for appending by a load.
            m_StatusLabel.Text = m_sCLOSE_STOP_SESSION;
            m_Timer.Stop();
            EndSession();

            // Signal the background work to stop and wait for it to complete (10 second timeout)
            m_StatusLabel.Text = m_sCLOSE_STOP_DEVICE_UPDATE;
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
                SetStatusBoxState(sLoadWarning, StatusPalette.WarningText, StatusPalette.WarningBackground);
            }
            else
            {
                SetStatusBoxState(sSuccessMessage, StatusPalette.SuccessText, StatusPalette.SuccessBackground);
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

            if (disposing)
            {
                if (null != components)
                {
                    components.Dispose();
                }

                // Only the bold fonts are created here; the regular ones belong to the designer
                if (null != m_ActionFontBold)
                {
                    m_ActionFontBold.Dispose();
                    m_ActionFontBold = null;
                }

                if (null != m_VerdictFontBold)
                {
                    m_VerdictFontBold.Dispose();
                    m_VerdictFontBold = null;
                }
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
            textColor = StatusPalette.NormalText;
            backColor = StatusPalette.NormalBackground;

            // Get UI state on the main thread
            if (InvokeRequired)
            {
                string tempText = string.Empty;
                Color tempTextColor = StatusPalette.NormalText;
                Color tempBackColor = StatusPalette.NormalBackground;

                Invoke(new Action(() =>
                {
                    tempText = m_StatusLabel.Text;
                    tempTextColor = m_StatusLabel.ForeColor;
                    tempBackColor = m_StatusLabel.BackColor;
                }));

                sText = tempText;
                textColor = tempTextColor;
                backColor = tempBackColor;
            }
            else
            {
                sText = m_StatusLabel.Text;
                textColor = m_StatusLabel.ForeColor;
                backColor = m_StatusLabel.BackColor;
            }
        }

        /// <summary>
        /// Updates the status box based on the device read status
        /// /// <param name="fResult">IN - The result of the read (double max indicates error)</param>
        public void RecordReadResult(double fResult)
        {
            // Lock the status box object
            lock (m_StatusLock)
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

                        // Update the displayed statistics
                        UpdateStatisticsDisplay();

                        // Clear any displayed errors 
                        SetStatusBoxState(RunningMessage, StatusPalette.NormalText, StatusPalette.NormalBackground);
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
                        SetStatusBoxError($"File I/O error: {ioEx.Message}");
                    }
                    catch (Exception generalEx)
                    {
                        // Handle any other errors and display in status bar
                        SetIdleState();
                        SetStatusBoxError($"Unexpected error recording data: {generalEx.Message}");
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
                SetStatusBoxState(m_sINIT_MESSAGE, StatusPalette.NormalText, StatusPalette.NormalBackground);
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

            // Ending the session is now the action to take next
            SetPrimaryButton(m_StopButton);

            UpdateWindowTitle();

            // Start a new session (will continue existing session if already in progress)
            StartSession();
        }

        /// <summary>
        /// Puts the window back to the size and position it was last closed at. A size that was never
        /// recorded, or one that would put the window somewhere the user cannot reach it - a screen that has
        /// since been unplugged, say - is ignored and the window opens where the designer places it.
        /// </summary>
        private void RestoreWindowPlacement()
        {
            System.Drawing.Size savedSize = Properties.Settings.Default.WindowSize;
            System.Drawing.Point savedLocation = Properties.Settings.Default.WindowLocation;

            // Nothing has been saved yet on the first run
            bool bSizeSaved = ((savedSize.Width >= MinimumSize.Width) && (savedSize.Height >= MinimumSize.Height));
            if (false == bSizeSaved)
            {
                return;
            }

            // Only take the position if the window would still land on a screen that is attached
            System.Drawing.Rectangle savedBounds = new System.Drawing.Rectangle(savedLocation, savedSize);
            bool bOnScreen = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                bOnScreen = (bOnScreen || screen.WorkingArea.IntersectsWith(savedBounds));
            }

            Size = savedSize;
            if (true == bOnScreen)
            {
                StartPosition = FormStartPosition.Manual;
                Location = savedLocation;
            }

            if (true == Properties.Settings.Default.WindowMaximised)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>
        /// Records the size and position of the window so the next run opens where this one was left. The
        /// restored bounds are saved rather than the current ones, so a window closed while maximised comes
        /// back maximised and returns to a sensible size when it is restored.
        /// </summary>
        private void SaveWindowPlacement()
        {
            try
            {
                bool bMaximised = (FormWindowState.Maximized == WindowState);
                System.Drawing.Rectangle bounds = (FormWindowState.Normal == WindowState) ? Bounds : RestoreBounds;

                Properties.Settings.Default.WindowMaximised = bMaximised;
                Properties.Settings.Default.WindowLocation = bounds.Location;
                Properties.Settings.Default.WindowSize = bounds.Size;
                Properties.Settings.Default.Save();
            }
            catch (Exception saveException)
            {
                // Remembering where the window was is a convenience, and a convenience must never be able to
                // stop the application closing. This caught only ConfigurationErrorsException before, and the
                // settings system throws more than that: on the first save for a machine it came out as an
                // ArgumentException from inside the configuration stack, which went unhandled and put an
                // error dialog in front of a user who had only asked to close the window.
                SetStatusBoxError($"Unable to save the window position: {saveException.Message}");

                // Record what went wrong where it can be read afterwards, since the window carrying the
                // status bar is on its way out and nobody will see the message above
                ReportPlacementFailure(saveException);
            }
        }

        /// <summary>
        /// Writes a failure to save the window position to a log beside the session files, so a failure that
        /// happens as the application closes leaves something behind to look at
        /// </summary>
        /// <param name="saveException">IN - The failure to record</param>
        private static void ReportPlacementFailure(Exception saveException)
        {
            try
            {
                string sLogPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    m_sLOG_FOLDER, m_sPLACEMENT_LOG);
                Directory.CreateDirectory(Path.GetDirectoryName(sLogPath));
                File.AppendAllText(sLogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {saveException}{Environment.NewLine}{Environment.NewLine}");
            }
            catch (Exception)
            {
                // Failing to write a note about a failure is not worth reporting a failure about
            }
        }

        /// <summary>
        /// Applies the palette to the controls the designer laid out. The colours and the two label fonts
        /// are set here rather than repeated on forty controls in the designer, so that changing the
        /// interface's palette is one edit rather than forty.
        /// </summary>
        private void ApplyTheme()
        {
            // The window and the tab pages are the ground the cards are laid on
            BackColor = UiPalette.Ground;
            ExecuteTabPage.BackColor = UiPalette.Ground;
            AnalyzeTabPage.BackColor = UiPalette.Ground;
            MainTabControl.BackColor = UiPalette.Ground;
            m_StatusStrip.BackColor = UiPalette.Ground;

            // The hairlines between the readout tiles are the layout showing through the gaps its cells
            // leave, so the layout is the colour of the line and the tiles are the colour of the card
            m_StatisticsLayout.BackColor = UiPalette.Line;
            foreach (Panel tile in new Panel[] { m_SessionTimerTile, m_DataPointsTile, m_CurrentAverageTile,
                                                 m_MeanDeviationTile, m_StandardDeviationTile })
            {
                tile.BackColor = UiPalette.Card;
            }

            // Labels name a value rather than carrying one, so they recede behind it
            foreach (Label eyebrow in new Label[] { m_SimulateToggleLabel, m_PortLabel, m_FileLlabel, m_TargetLabel,
                                                    m_SessionTimerLabel, m_DataPointsLabel, m_CurrentAverageLabel,
                                                    m_MeanDeviationLabel, m_StandardDeviationLabel,
                                                    m_BaselineLabel, m_ResultLabel })
            {
                eyebrow.Font = m_EyebrowFont;
                eyebrow.ForeColor = UiPalette.MutedText;
            }

            // The readouts are the values the window exists to show, so they are the largest thing on it
            foreach (TextBox readout in new TextBox[] { m_SessionTimerTextBox, m_DataPointsTextBox,
                                                        m_CurrentAverageTextBox, m_MeanDeviationTextBox,
                                                        m_StandardDeviationTextBox })
            {
                readout.Font = m_ReadoutFont;
                readout.ForeColor = UiPalette.CardText;
                readout.BackColor = UiPalette.Card;
            }

            // The source toggle carries a setting rather than a severity, so it reads as the accent when it
            // is on and as a plain track when it is off. Its own defaults were black on black, which was
            // the one thing on the window that belonged to no scheme at all.
            m_SimulateToggle.OnBackground = UiPalette.Accent;
            m_SimulateToggle.OnToggle = UiPalette.AccentText;
            m_SimulateToggle.OffBackground = UiPalette.Line;
            m_SimulateToggle.OffToggle = UiPalette.Card;
            m_SimulateToggle.DisabledBackground = UiPalette.Ground;
            m_SimulateToggle.DisabledToggle = UiPalette.Line;

            // The switch is drawn on the card, so the corners its rounded shape does not reach take the card
            m_SimulateToggle.BackColor = UiPalette.Card;


            // Clearing is the one action here that destroys something, so it is the quietest thing on the card
            m_ClearButton.BackColor = UiPalette.Card;
            m_ClearButton.ForeColor = UiPalette.MutedText;
            m_ClearButton.FlatAppearance.BorderSize = 0;

            m_ComparisonList.BackColor = UiPalette.Card;
            m_ComparisonList.ForeColor = UiPalette.CardText;

            // The charts sit on cards, so their own furniture is taken from the same pair
            m_ResultChart.ApplyPalette();
            m_ResultHistogramChart.ApplyPalette();
        }

        /// <summary>
        /// Event handler for the system colour scheme being changed while the application is running
        /// </summary>
        /// <param name="e">IN - The event arguments (not used)</param>
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);

            // The palette reports whatever scheme is in force now, so the controls are recoloured from it.
            // Without this the window keeps the scheme it opened under until it is restarted.
            ApplyTheme();
            RefreshPrimaryButton();
            SetStatusBoxState(StatusBoxText, StatusPalette.NormalText, StatusPalette.NormalBackground);
        }

        /// <summary>
        /// Draws the button emphasis again from the palette as it stands now, without changing which button
        /// carries it
        /// </summary>
        private void RefreshPrimaryButton()
        {
            SetPrimaryButton(m_PrimaryButton);
        }

        /// <summary>
        /// Emphasises the one button that carries the action to take next, leaving the others in the system
        /// style. Which button that is changes with the state - starting a session while idle, ending it
        /// while one is running - so it is set as part of each state transition rather than fixed once.
        /// NOTE: The designer applies the same emphasis to the start button, as that is the state the form
        /// opens in and the designer cannot call this.
        /// </summary>
        /// <param name="primaryButton">IN - The button to emphasise, which is left plain if it is disabled</param>
        private void SetPrimaryButton(Button primaryButton)
        {
            // Remembered so the emphasis can be drawn again from a changed colour scheme without the state
            // machine having to be run through a transition to say what it was
            m_PrimaryButton = primaryButton;

            Button[] actionButtons = new Button[] { m_StartButton, m_PauseButton, m_StopButton };
            foreach (Button actionButton in actionButtons)
            {
                bool bIsPrimary = ((actionButton == primaryButton) && (true == actionButton.Enabled));
                actionButton.FlatStyle = FlatStyle.Flat;
                actionButton.UseVisualStyleBackColor = false;

                if (false == actionButton.Enabled)
                {
                    // A flat button given a colour of its own keeps it when it is disabled, so an action
                    // that cannot be taken has to be greyed here or it reads as being available
                    actionButton.FlatAppearance.BorderSize = 1;
                    actionButton.FlatAppearance.BorderColor = UiPalette.Line;
                    actionButton.BackColor = UiPalette.Ground;
                    actionButton.ForeColor = UiPalette.MutedText;
                    actionButton.Font = m_ActionFontRegular;
                }
                else if (true == bIsPrimary)
                {
                    actionButton.FlatAppearance.BorderSize = 0;
                    actionButton.FlatAppearance.MouseOverBackColor = UiPalette.AccentHover;
                    actionButton.FlatAppearance.MouseDownBackColor = UiPalette.AccentPressed;
                    actionButton.BackColor = UiPalette.Accent;
                    actionButton.ForeColor = UiPalette.AccentText;
                    actionButton.Font = m_ActionFontBold;
                }
                else
                {
                    // A card-coloured face inside a hairline, so a secondary action reads as available
                    // without competing with the one that carries the next step
                    actionButton.FlatAppearance.BorderSize = 1;
                    actionButton.FlatAppearance.BorderColor = UiPalette.Line;
                    actionButton.FlatAppearance.MouseOverBackColor = UiPalette.Ground;
                    actionButton.FlatAppearance.MouseDownBackColor = UiPalette.Line;
                    actionButton.BackColor = UiPalette.Card;
                    actionButton.ForeColor = UiPalette.CardText;
                    actionButton.Font = m_ActionFontRegular;
                }
            }
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

            // Starting a session is the action to take next
            SetPrimaryButton(m_StartButton);

            UpdateWindowTitle();

            // Update the info box
            SetStatusBoxState(IdleMessage, StatusPalette.NormalText, StatusPalette.NormalBackground);
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

            // Picking the session back up is what a pause is usually followed by, so that button takes the
            // emphasis while it is paused rather than the one that ends the session for good
            SetPrimaryButton(m_PauseButton);

            // Disable the timers
            m_Timer.Enabled = false;
            m_Data.PauseSession();

            // Update the info box message
            SetStatusBoxState(m_sPAUSED_MESSAGE, StatusPalette.NormalText, StatusPalette.NormalBackground);
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

            // Ending the session remains the action to take next
            SetPrimaryButton(m_StopButton);

            // Re-enable the timers
            m_Timer.Enabled = true;
            m_Data.ResumeSession();

            // Report that the session is running again. This said "Idle" before, which was left over from
            // the pause and disagreed with the session that had just been picked back up.
            SetStatusBoxState(RunningMessage, StatusPalette.NormalText, StatusPalette.NormalBackground);
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
                SetStatusBoxState(RunningMessage, StatusPalette.NormalText, StatusPalette.NormalBackground);

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
                SetStatusBoxError($"File I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors and display in status bar
                SetStatusBoxError($"Unexpected error starting session: {generalEx.Message}");
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
                SetStatusBoxError($"File I/O error accessing file {Path.GetFileName(sFilePath)}: {fileIOException.Message}");
                bStatus = false;
            }
            catch (Exception generalException)
            {
                // Display general error using helper method
                SetStatusBoxError($"Unexpected error accessing file {Path.GetFileName(sFilePath)}: {generalException.Message}");
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
                    SetStatusBoxState(sLoadWarning, StatusPalette.WarningText, StatusPalette.WarningBackground);
                }
                else
                {
                    // Show success status using helper method
                    SetStatusBoxState($"File loaded: {Path.GetFileName(sFilePath)}. {m_Data.NumDataPoints} data points loaded. Ready for new session.",
                                    StatusPalette.SuccessText, StatusPalette.SuccessBackground);
                }
            }
            else
            {
                // Show error status using helper method
                SetStatusBoxError($"Error accessing file {Path.GetFileName(sFilePath)}. Please verify the file exists and is readable.");
            }
        }

        /// <summary>
        /// Shows the statistics currently held by the session data. Every place that changes the session -
        /// recording a reading, loading a file, clearing - reports through here, so the readouts can only
        /// ever show what the session data actually holds.
        /// </summary>
        private void UpdateStatisticsDisplay()
        {
            m_CurrentAverageTextBox.Text = CurrentAverage;
            m_DataPointsTextBox.Text = NumDataPoints;
            m_MeanDeviationTextBox.Text = MeanDeviation;
            m_StandardDeviationTextBox.Text = StandardDeviation;
            m_SessionTimerTextBox.Text = m_Data.SessionTime;

            // The title carries the session too, and it costs nothing when nothing in it has changed
            UpdateWindowTitle();
        }

        /// <summary>
        /// Updates all UI elements with data from the loaded session
        /// </summary>
        private void UpdateUIFromLoadedSession()
        {
            // Update the statistics display fields
            UpdateStatisticsDisplay();

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
            SetStatusBoxState($"Loading file: {fileName}...", StatusPalette.BusyText, StatusPalette.BusyBackground);
            
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
                    m_StatusLabel.Text = sText;
                    m_StatusLabel.ForeColor = textColor;
                    m_StatusLabel.BackColor = backColor;
                    m_StatusIndicator.ForeColor = textColor;
                }));
            }
            else
            {
                m_StatusLabel.Text = sText;
                m_StatusLabel.ForeColor = textColor;
                m_StatusLabel.BackColor = backColor;
                m_StatusIndicator.ForeColor = textColor;
            }
        }

        /// <summary>
        /// Updates the status box with error information in a thread-safe manner
        /// </summary>
        /// <param name="sText">IN - Error text to display</param>
        public void SetStatusBoxError(string sText)
        {
            SetStatusBoxState(sText, StatusPalette.ErrorText, StatusPalette.ErrorBackground);
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
                SetStatusBoxError($"Baseline file error: {argEx.Message}");
                bStatus = false;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle file not found errors
                SetStatusBoxError($"Baseline file not found: {Path.GetFileName(sFilePath)}");
                bStatus = false;
            }
            catch (System.IO.IOException ioEx)
            {
                // Handle file I/O errors
                SetStatusBoxError($"Baseline file I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Handle file loading/parsing errors
                SetStatusBoxError($"Baseline file loading error: {invalidOpEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors
                SetStatusBoxError($"Unexpected error loading baseline file: {generalEx.Message}");
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
                SetStatusBoxError($"Result file error: {argEx.Message}");
                bStatus = false;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Handle file not found errors
                SetStatusBoxError($"Result file not found: {Path.GetFileName(sFilePath)}");
                bStatus = false;
            }
            catch (System.IO.IOException ioEx)
            {
                // Handle file I/O errors
                SetStatusBoxError($"Result file I/O error: {ioEx.Message}");
                bStatus = false;
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Handle file loading/parsing errors
                SetStatusBoxError($"Result file loading error: {invalidOpEx.Message}");
                bStatus = false;
            }
            catch (Exception generalEx)
            {
                // Handle any other errors
                SetStatusBoxError($"Unexpected error loading result file: {generalEx.Message}");
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
                m_BaselineTextBox.Text = DescribeLoadedFile(sFilePath, m_BaselineAnalysis);

                // Update the histogram chart with baseline data
                UpdateHistogramChartWithBaseline();

                // Show the baseline column, and the difference if a result is loaded as well
                UpdateComparisonTable();

                // Show the success status, or anything that needs raising about the file that was loaded
                ReportAnalysisLoaded(m_BaselineAnalysis,
                                     $"Baseline file loaded: {Path.GetFileName(sFilePath)}. Ready for analysis.");
            }
            else
            {
                // Clear any previous baseline data
                m_BaselineAnalysis = null;

                // Clear the baseline UI fields
                m_BaselineTextBox.Text = string.Empty;
                UpdateComparisonTable();

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
                m_ResultTextBox.Text = DescribeLoadedFile(sFilePath, m_ResultAnalysis);

                // Update the histogram chart with result data
                UpdateHistogramChartWithResult();

                // Show the result column, and the difference if a baseline is loaded as well
                UpdateComparisonTable();

                // Show the success status, or anything that needs raising about the file that was loaded
                ReportAnalysisLoaded(m_ResultAnalysis,
                                     $"Result file loaded: {Path.GetFileName(sFilePath)}. Ready for analysis.");
            }
            else
            {
                // Clear any previous result data
                m_ResultAnalysis = null;

                // Clear the result UI fields
                m_ResultTextBox.Text = string.Empty;
                UpdateComparisonTable();

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
                SetStatusBoxError($"Error updating histogram chart: {ex.Message}");
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
                SetStatusBoxError($"Error updating histogram chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds the rows of the comparison table. The measures are fixed, so the rows are made once and
        /// their values replaced as files are loaded, rather than the table being rebuilt each time.
        /// NOTE: The order here is the order the row constants name, and the two have to be kept in step.
        /// </summary>
        private void BuildComparisonTable()
        {
            string sExpected = m_fSTATISTICAL_MEAN.ToString(m_sMEAN_LABEL_FORMAT);
            string[] sMeasures = new string[]
            {
                "Readings",
                "Mean",
                $"Deviation from {sExpected}",
                "Standard deviation",
                "Skewness",
                "Kurtosis",
                $"Chance of this shift from {sExpected} being noise"
            };

            m_ComparisonList.BeginUpdate();
            m_ComparisonList.Items.Clear();
            foreach (string sMeasure in sMeasures)
            {
                ListViewItem measureRow = new ListViewItem(sMeasure);
                measureRow.SubItems.Add(m_sNO_VALUE);
                measureRow.SubItems.Add(m_sNO_VALUE);
                measureRow.SubItems.Add(m_sNO_VALUE);
                m_ComparisonList.Items.Add(measureRow);
            }
            m_ComparisonList.EndUpdate();
        }

        /// <summary>
        /// Puts the session in the window title, so which file is being recorded into and how long it has
        /// been running can be read from the task bar without bringing the window to the front.
        /// </summary>
        private void UpdateWindowTitle()
        {
            string sFileName = m_FileTextBox.Text;
            bool bFileChosen = (false == string.IsNullOrEmpty(sFileName));
            bool bRecording = ((RngGuiStates.Running == m_State) || (RngGuiStates.Paused == m_State));

            if (false == bFileChosen)
            {
                Text = m_sAPPLICATION_NAME;
            }
            else if (false == bRecording)
            {
                Text = $"{sFileName}{m_sTITLE_SEPARATOR}{m_sAPPLICATION_NAME}";
            }
            else
            {
                Text = $"{sFileName}{m_sTITLE_SEPARATOR}{m_Data.SessionTime}{m_sTITLE_SEPARATOR}{m_sAPPLICATION_NAME}";
            }
        }

        /// <summary>
        /// The message to show while nothing is running, which says what to do next rather than reporting
        /// that nothing is happening. Until a file is chosen there is nothing else the user can usefully do.
        /// </summary>
        private string IdleMessage
        {
            get
            {
                bool bFileChosen = (false == string.IsNullOrEmpty(m_FileTextBox.Text));
                return bFileChosen ? m_sREADY_MESSAGE : m_sNO_FILE_MESSAGE;
            }
        }

        /// <summary>
        /// Names a loaded analysis file along with how many readings it holds. Two sessions of very
        /// different lengths are not a fair comparison, and that has to be visible before the comparison is
        /// read rather than after.
        /// </summary>
        /// <param name="sFilePath">IN - Path of the file that was loaded</param>
        /// <param name="analysis">IN - The analysis it was loaded into, which may be null</param>
        /// <returns>The file name, followed by the reading count when one is known</returns>
        private static string DescribeLoadedFile(string sFilePath, StatisticalAnalysis analysis)
        {
            string sFileName = Path.GetFileName(sFilePath);
            List<double> loadedData = analysis?.LoadedFileData;
            if (null == loadedData)
            {
                return sFileName;
            }

            string sReadings = (1 == loadedData.Count) ? m_sSINGLE_READING : m_sMANY_READINGS;
            return $"{sFileName}  —  {loadedData.Count:N0} {sReadings}";
        }

        /// <summary>
        /// Spreads the comparison columns over the width of the table, rather than leaving the values
        /// bunched to the left with empty space beside them. The measure column takes the larger share
        /// because it holds words while the others hold a fixed number of digits.
        /// </summary>
        private void SizeComparisonColumns()
        {
            // Leave a little back so the last column does not sit under the border
            const int iEDGE_ALLOWANCE = 4;
            int iAvailable = (m_ComparisonList.ClientSize.Width - iEDGE_ALLOWANCE);
            if (0 >= iAvailable)
            {
                return;
            }

            const double fVALUE_SHARE = 0.2;
            int iValueWidth = (int)(iAvailable * fVALUE_SHARE);

            // The measure column takes what the rounding of the others leaves, so the widths always add up
            m_BaselineColumn.Width = iValueWidth;
            m_ResultColumn.Width = iValueWidth;
            m_DifferenceColumn.Width = iValueWidth;
            m_MeasureColumn.Width = (iAvailable - (iValueWidth * 3));
        }

        /// <summary>
        /// The value shown for one measure of one session, in the row order the table is built in
        /// </summary>
        /// <param name="iRow">IN - The row of the table the measure sits on</param>
        /// <param name="readings">IN - The readings the session holds, which may be null</param>
        /// <param name="stats">IN - The statistics taken from them, which may be null</param>
        /// <returns>The value to show, or no value when the session is not loaded</returns>
        private static string FormatMeasure(int iRow, List<double> readings, DescriptiveStatistics stats)
        {
            if ((null == readings) || (null == stats))
            {
                return m_sNO_VALUE;
            }

            switch (iRow)
            {
                case m_iREADINGS_ROW:
                    return readings.Count.ToString(m_sCOUNT_FORMAT);
                case m_iMEAN_ROW:
                    return stats.Mean.ToString(m_sVALUE_FORMAT);
                case m_iDEVIATION_ROW:
                    return Math.Abs(stats.Mean - m_fSTATISTICAL_MEAN).ToString(m_sVALUE_FORMAT);
                case m_iSTD_DEV_ROW:
                    return stats.StandardDeviation.ToString(m_sVALUE_FORMAT);
                case m_iSKEWNESS_ROW:
                    return stats.Skewness.ToString(m_sMOMENT_FORMAT);
                case m_iKURTOSIS_ROW:
                    return stats.Kurtosis.ToString(m_sMOMENT_FORMAT);
                case m_iPROBABILITY_ROW:
                    return FormatProbability(SignificanceTest.CompareWithExpected(readings, m_fSTATISTICAL_MEAN));
                default:
                    return m_sNO_VALUE;
            }
        }

        /// <summary>
        /// The difference shown for one measure between the two sessions. A count and a probability are not
        /// subtracted from one another: the difference in reading counts says nothing about the generator,
        /// and the two probabilities are answers to separate questions rather than a pair to compare.
        /// </summary>
        /// <param name="iRow">IN - The row of the table the measure sits on</param>
        /// <param name="baseline">IN - Statistics of the baseline session, which may be null</param>
        /// <param name="result">IN - Statistics of the result session, which may be null</param>
        /// <returns>The difference to show, or no value when it does not apply</returns>
        private static string FormatDifference(int iRow, DescriptiveStatistics baseline, DescriptiveStatistics result)
        {
            if ((null == baseline) || (null == result))
            {
                return m_sNO_VALUE;
            }

            switch (iRow)
            {
                case m_iMEAN_ROW:
                    return (result.Mean - baseline.Mean).ToString(m_sVALUE_DIFFERENCE_FORMAT);
                case m_iDEVIATION_ROW:
                    return (Math.Abs(result.Mean - m_fSTATISTICAL_MEAN) -
                            Math.Abs(baseline.Mean - m_fSTATISTICAL_MEAN)).ToString(m_sVALUE_DIFFERENCE_FORMAT);
                case m_iSTD_DEV_ROW:
                    return (result.StandardDeviation - baseline.StandardDeviation).ToString(m_sVALUE_DIFFERENCE_FORMAT);
                case m_iSKEWNESS_ROW:
                    return (result.Skewness - baseline.Skewness).ToString(m_sMOMENT_DIFFERENCE_FORMAT);
                case m_iKURTOSIS_ROW:
                    return (result.Kurtosis - baseline.Kurtosis).ToString(m_sMOMENT_DIFFERENCE_FORMAT);
                default:
                    return m_sNO_VALUE;
            }
        }

        /// <summary>
        /// A probability written for a reader rather than for a machine. Below a thousandth the digits stop
        /// meaning anything to the eye, so it is reported as being under that rather than to more decimals.
        /// </summary>
        /// <param name="test">IN - The outcome of the test</param>
        /// <returns>The probability, or no value when the test could not be carried out</returns>
        private static string FormatProbability(SignificanceResult test)
        {
            if (false == test.Valid)
            {
                return m_sNO_VALUE;
            }

            if (m_fSMALLEST_REPORTED_PROBABILITY > test.Probability)
            {
                return m_sVERY_SMALL_PROBABILITY;
            }

            return test.Probability.ToString(m_sPROBABILITY_FORMAT);
        }

        /// <summary>
        /// Fills the comparison table from whichever of the two files is loaded, and reports whether the
        /// difference between them is more than noise. A measure with nothing behind it shows as having no
        /// value rather than as a zero, and a difference is only shown when there is something on both
        /// sides of it to subtract.
        /// </summary>
        private void UpdateComparisonTable()
        {
            try
            {
                List<double> baselineReadings = m_BaselineAnalysis?.LoadedFileData;
                List<double> resultReadings = m_ResultAnalysis?.LoadedFileData;
                DescriptiveStatistics baselineStats = m_BaselineAnalysis?.LoadedFileStats;
                DescriptiveStatistics resultStats = m_ResultAnalysis?.LoadedFileStats;

                m_ComparisonList.BeginUpdate();
                for (int iRow = 0; iRow < m_ComparisonList.Items.Count; iRow++)
                {
                    ListViewItem measureRow = m_ComparisonList.Items[iRow];
                    measureRow.SubItems[m_iBASELINE_COLUMN].Text = FormatMeasure(iRow, baselineReadings, baselineStats);
                    measureRow.SubItems[m_iRESULT_COLUMN].Text = FormatMeasure(iRow, resultReadings, resultStats);
                    measureRow.SubItems[m_iDIFFERENCE_COLUMN].Text = FormatDifference(iRow, baselineStats, resultStats);
                }
                m_ComparisonList.EndUpdate();

                UpdateComparisonVerdict(baselineReadings, resultReadings);
            }
            catch (Exception ex)
            {
                // Handle any errors during comparison update
                SetStatusBoxError($"Error updating comparison statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Says in words whether the result session shifted away from the baseline by more than the noise in
        /// the two of them accounts for. This is the question the analysis exists to answer, so it is stated
        /// rather than left to be read off a table of numbers.
        /// NOTE: The test is two-tailed, so a shift in either direction counts.
        /// </summary>
        /// <param name="baselineReadings">IN - Readings of the baseline session, which may be null</param>
        /// <param name="resultReadings">IN - Readings of the result session, which may be null</param>
        private void UpdateComparisonVerdict(List<double> baselineReadings, List<double> resultReadings)
        {
            bool bBothLoaded = ((null != baselineReadings) && (null != resultReadings));
            if (false == bBothLoaded)
            {
                SetVerdict(m_sVERDICT_NEEDS_BOTH, false);
                return;
            }

            SignificanceResult test = SignificanceTest.CompareMeans(baselineReadings, resultReadings);
            if (false == test.Valid)
            {
                SetVerdict(m_sVERDICT_NOT_ENOUGH_DATA, false);
                return;
            }

            double fShift = (Mean(resultReadings) - Mean(baselineReadings));
            string sDirection = (0 <= fShift) ? m_sDIRECTION_HIGHER : m_sDIRECTION_LOWER;
            string sProbability = FormatProbability(test);

            if (true == test.Significant)
            {
                SetVerdict($"Significant shift: the result sits {sDirection} than the baseline by " +
                           $"{Math.Abs(fShift).ToString(m_sVALUE_FORMAT)}. A shift this large would arise by " +
                           $"chance {sProbability} of the time (Welch's t, two-tailed, {test.DegreesOfFreedom.ToString(m_sMOMENT_FORMAT)} df).", true);
            }
            else
            {
                SetVerdict($"No significant shift. The result sits {sDirection} than the baseline by " +
                           $"{Math.Abs(fShift).ToString(m_sVALUE_FORMAT)}, but a shift that large would arise by " +
                           $"chance {sProbability} of the time (Welch's t, two-tailed, {test.DegreesOfFreedom.ToString(m_sMOMENT_FORMAT)} df).", false);
            }
        }

        /// <summary>
        /// Shows the verdict, emphasised only when there is something to notice. Nothing found is the
        /// ordinary outcome and is not worth shouting about.
        /// </summary>
        /// <param name="sVerdict">IN - The wording to show</param>
        /// <param name="bSignificant">IN - Whether the verdict reports a shift worth noticing</param>
        private void SetVerdict(string sVerdict, bool bSignificant)
        {
            m_VerdictLabel.Text = sVerdict;
            m_VerdictLabel.Font = bSignificant ? m_VerdictFontBold : m_VerdictFontRegular;
            m_VerdictLabel.ForeColor = bSignificant ? UiPalette.CardText : UiPalette.MutedText;
        }

        /// <summary>
        /// The mean of a set of readings
        /// </summary>
        /// <param name="readings">IN - The readings, which must not be empty</param>
        /// <returns>The mean</returns>
        private static double Mean(List<double> readings)
        {
            double fTotal = 0.0;
            foreach (double fReading in readings)
            {
                fTotal += fReading;
            }
            return (fTotal / readings.Count);
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
        public Color StatusBoxBackColor { get => m_StatusLabel.BackColor; set => m_StatusLabel.BackColor = value; }

        /// <summary>
        /// Text displayed in the statatus box
        /// </summary>
        public string StatusBoxText { get => m_StatusLabel.Text; set => m_StatusLabel.Text = value; }

        /// <summary>
        /// Text color of the status box
        /// </summary>
        public Color StatusBoxTextColor { get => m_StatusLabel.ForeColor; set => m_StatusLabel.ForeColor = value; }

        /// <summary>
        /// The current state of the GUI (read-only)
        /// </summary>
        public RngGuiStates State { get => m_State; }

        /// <summary>
        /// Current average string (read-only)
        /// </summary>
        /// <summary>
        /// Whether any readings have been taken or loaded (read-only). A mean, a deviation and a spread all
        /// need something to be taken from, so before the first reading arrives they are shown as having no
        /// value rather than as being zero, which would be a measurement the session has not made.
        /// </summary>
        private bool HasReadings { get => (0 < m_Data.NumDataPoints); }

        /// <summary>
        /// Current average of the session, or no value if nothing has been recorded yet (read-only)
        /// </summary>
        private string CurrentAverage { get => HasReadings ? m_Data.CurrentAverage.ToString(m_sVALUE_FORMAT) : m_sNO_VALUE; }

        /// <summary>
        /// Number of data points gathered
        /// </summary>
        private string NumDataPoints { get => m_Data.NumDataPoints.ToString(m_sINTEGER_FORMAT); }

        /// <summary>
        /// Deviation from the statistical mean
        /// </summary>
        private string MeanDeviation { get => HasReadings ? m_Data.MeanDeviation.ToString(m_sVALUE_FORMAT) : m_sNO_VALUE; }

        /// <summary>
        /// Standard deviation of the data set
        /// </summary>
        private string StandardDeviation { get => HasReadings ? m_Data.StandardDeviation.ToString(m_sVALUE_FORMAT) : m_sNO_VALUE; }

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

        // Display settings. Bit averages and the deviations taken from them all sit within a unit range, so
        // they are shown to a fixed six decimals rather than in exponent form, which puts every value on the
        // same scale and lets two of them be compared by eye. Skewness and kurtosis are unbounded and are
        // shown to three, and differences carry an explicit sign so the direction of a change is visible.
        private const string m_sVALUE_FORMAT = "0.000000";
        private const string m_sINTEGER_FORMAT = "0";
        private const string m_sMOMENT_FORMAT = "0.000";
        private const string m_sVALUE_DIFFERENCE_FORMAT = "+0.000000;-0.000000";
        private const string m_sMOMENT_DIFFERENCE_FORMAT = "+0.000;-0.000";
        private const string m_sNO_VALUE = "—";

        // The value an unbiased generator's bit averages sit around, which the deviation is measured from
        private const double m_fSTATISTICAL_MEAN = 0.5;
        private const string m_sMEAN_LABEL_FORMAT = "0.0";

        // How a loaded analysis file reports the amount of data behind it
        private const string m_sSINGLE_READING = "reading";
        private const string m_sMANY_READINGS = "readings";

        // Rows of the comparison table, in the order BuildComparisonTable adds them. The order there and
        // these have to be kept in step.
        private const int m_iREADINGS_ROW = 0;
        private const int m_iMEAN_ROW = 1;
        private const int m_iDEVIATION_ROW = 2;
        private const int m_iSTD_DEV_ROW = 3;
        private const int m_iSKEWNESS_ROW = 4;
        private const int m_iKURTOSIS_ROW = 5;
        private const int m_iPROBABILITY_ROW = 6;

        // How the counts and probabilities in the table are written
        private const string m_sCOUNT_FORMAT = "N0";
        private const string m_sPROBABILITY_FORMAT = "0.000";
        private const string m_sVERY_SMALL_PROBABILITY = "< 0.001";
        private const double m_fSMALLEST_REPORTED_PROBABILITY = 0.001;

        // Wording of the verdict on whether the two sessions differ
        private const string m_sVERDICT_NEEDS_BOTH = "Load a baseline and a result to compare them.";
        private const string m_sVERDICT_NOT_ENOUGH_DATA = "Not enough readings in these sessions to test whether they differ.";
        private const string m_sDIRECTION_HIGHER = "higher";
        private const string m_sDIRECTION_LOWER = "lower";

        // Columns of the comparison table. Column zero names the measure and is never rewritten.
        private const int m_iBASELINE_COLUMN = 1;
        private const int m_iRESULT_COLUMN = 2;
        private const int m_iDIFFERENCE_COLUMN = 3;

        // Guards the status bar, which is written to from the device read path as well as the UI
        private readonly object m_StatusLock = new object();

        // Fonts for the session buttons, built once rather than per state change. The regular one belongs to
        // the designer and is not disposed here; the bold one is created here and is.
        private Font m_ActionFontRegular = null;
        private Font m_ActionFontBold = null;

        // Where a failure to remember the window position is recorded
        private const string m_sLOG_FOLDER = "RandomNumberGenerator";
        private const string m_sPLACEMENT_LOG = "window-placement-errors.log";

        // Height of a field in the setup row, taken from the control that reports it correctly
        private int m_iFieldHeight = 0;

        // The button currently carrying the action to take next, so its emphasis can be redrawn
        private Button m_PrimaryButton = null;

        // Fonts for the comparison verdict, which is emphasised only when it reports a shift
        private Font m_VerdictFontRegular = null;
        private Font m_VerdictFontBold = null;

        // The small capitals that name a value, and the face the values themselves are set in. Both are
        // created here and disposed with the form.
        private static readonly Font m_EyebrowFont = new Font("Segoe UI", 8F, FontStyle.Regular);
        private static readonly Font m_ReadoutFont = new Font("Consolas", 15F, FontStyle.Regular);

        // The two things the same field means, depending on where the readings come from
        private const string m_sPORT_LABEL = "PORT";
        private const string m_sSEED_LABEL = "SEED";

        // Window title
        private const string m_sAPPLICATION_NAME = "Random Number Generator";
        private const string m_sTITLE_SEPARATOR = " - ";

        // Button text
        internal const string m_sPAUSE_BUTTON = "&Pause";
        internal const string m_sRESUME_BUTTON = "&Resume";

        // Status and error messages for the info box
        private const string m_sINVALID_SEED_ERROR = "Specified seed is not valid. Must be positive integer. Reset to last valid value.";
        private const string m_sDEVICE_INIT_ERROR = "Error initializing TruRNGpro. Please verify device is connected and correct COM port is selected.";
        private const string m_sDEVICE_READ_ERROR = "Error reading from TruRNGpro. Please verify device is connected and correct COM port is selected.";
        private const string m_sINIT_MESSAGE = "Initialized";
        private const string m_sNO_FILE_MESSAGE = "Choose a data file to record into.";
        private const string m_sREADY_MESSAGE = "Ready. Press Start to record.";
        private const string m_sPAUSED_MESSAGE = "Paused";
        private const string m_sCLOSE_STOP_SESSION = "Ending current session...";
        private const string m_sCLOSE_STOP_DEVICE_UPDATE = "Waiting for USB device search to end...";
        private const string m_sFILE_LOAD_ERROR = "Error loading file. Please verify the file format is correct.";

        #endregion
    }
}
