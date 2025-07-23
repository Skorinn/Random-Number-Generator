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
//*********************************************************************************************************************

// Enable to dump the USB device information
//#define DUMP_DEVICES

using DeviceInterfaces;
using System;
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
    /// <\summary>
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
                DeviceUpdateThread.ThreadProc(state);
                m_DeviceUpdateComplete.Set(); // Signal that the device update has finished
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
                            DeviceUpdateThread.ThreadProc(state);
                            m_DeviceUpdateComplete.Set(); // Signal that the device update has finished
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

            // Update the simulation status and button
            m_Data.Simulated = !(m_Data.Simulated);
            m_SimulateToggle.Checked = m_Data.Simulated;

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
        /// <param="sender">IN - Sender of the event (not used)</param>
        /// <param="e">IN - The event arguments (not used)</param>
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

            // Do not allow a new session while processing action
            FileBrowseActive = true;

            // Prompt the user to select a file
            string sSelectedFile = null;
            using (OpenFileDialog dataFileOpenDialog = new OpenFileDialog())
            {
                // Generate a default file name with the format: session_YYYYMMDD_HHMMSS.rng
                dataFileOpenDialog.Title = "Create or open a Random Number Generator data file";
                dataFileOpenDialog.FileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}.rng";
                dataFileOpenDialog.Filter = "RNG data files (*.rng)|*.rng|All files (*.*)|*.*";
                dataFileOpenDialog.DefaultExt = "rng";
                dataFileOpenDialog.AddExtension = true;

                // Allow creation of new files but directory must exist
                dataFileOpenDialog.CheckFileExists = false;
                dataFileOpenDialog.CheckPathExists = true;
                dataFileOpenDialog.ValidateNames = true;
                dataFileOpenDialog.DereferenceLinks = true;

                // Show the file open dialog
                DialogResult result = dataFileOpenDialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    sSelectedFile = dataFileOpenDialog.FileName;
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
                // Check if the file exists
                bool bFileExists = File.Exists(sSelectedFile);
                if (bFileExists)
                {
                    // Load the existing file by setting the file path and triggering a load operation
                    // This approach works through the interface hierarchy rather than directly parsing XML
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        bool bLoadSuccess = LoadExistingSessionFile(sSelectedFile);
                        
                        // Update the UI on the main thread
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => OnFileLoadCompleted(sSelectedFile, bLoadSuccess)));
                        }
                        else
                        {
                            OnFileLoadCompleted(sSelectedFile, bLoadSuccess);
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

            // Make sure any running session is stoped
            m_StatusTextBox.Text = m_sCLOSE_STOP_SESSION;
            StopButton_Click(sender, e);

            // Signal the device update thread to stop and wait for it to complete (10 second timeout)
            m_StatusTextBox.Text = m_sCLOSE_STOP_DEVICE_UPDATE;
            m_DeviceUpdateComplete.WaitOne(10000);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Implementation of Invoke as needed by the ISynchronizeInvoke interface
        /// <\summary>
        public object Invoke(Action method)
        {
            return base.Invoke(method);
        }

        /// <summary>
        /// Override of the dispose method from Form
        /// </summary>
        public new void Dispose()
        {
            // Record the GUI is terminating and execute the base class dispose
            m_State = RngGuiStates.Terminating;
            base.Dispose();
        }

        /// <summary>
        /// Implementation of GetStatusBoxState to retrieve status box state
        /// </summary>
        public void GetStatusBoxState(out string sText, out Color textColor, out Color backColor)
        {
            // Initialize output parameters
            sText = "";
            textColor = System.Drawing.Color.Black;
            backColor = System.Drawing.SystemColors.Info;

            // Get UI state on the main thread
            if (InvokeRequired)
            {
                string tempText = "";
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
        /// </summary>
        /// <param name="fResult">IN - The result of the read (double max indicates error)</param>
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
            // Set the state only if not terminating
            if (RngGuiStates.Terminating != m_State)
            {
                m_State = RngGuiStates.Idle;
            }

            // End the current session
            EndSession();

            // Update the button statuses
            m_StartButton.Enabled = true;
            m_StopButton.Enabled = false;
            m_PauseButton.Enabled = false;

            // Reset the pause button
            m_PauseButton.Text = m_sPAUSE_BUTTON;

            // Enable the file browser when not running a session
            m_FileBrowseButton.Enabled = true;

            // Enable the target number field
            m_TargetComboBox.Enabled = true;

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

            // End the current session
            m_Data.EndSession();

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
            // Re-enable UI controls
            FileBrowseActive = false;

            if (bSuccess)
            {
                // Update the file display
                m_FileTextBox.Text = Path.GetFileName(sFilePath);
                
                // Show success status using helper method
                SetStatusBoxState($" File selected: {Path.GetFileName(sFilePath)}. Ready for new session.", 
                                System.Drawing.Color.Black, System.Drawing.Color.LightGreen);
            }
            else
            {
                // Show error status using helper method
                SetStatusBoxError($" Error accessing file {Path.GetFileName(sFilePath)}. Please verify the file exists and is readable.");
            }
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
