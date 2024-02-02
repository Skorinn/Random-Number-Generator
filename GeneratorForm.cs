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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Interface for the Random Number Generator form
    /// <\summary>
    public interface IGeneratorForm : ISynchronizeInvoke
    {
        bool Busy { get; set; }
        bool Running { get; }
        BindingList<IRNGDevice> DeviceList { set; }

        string StatusBoxText { get; set; }
        Color StatusBoxTextColor { get; set; }
        Color StatusBoxBackColor { get; set; }

        // Action version of Invoke not included in ISynchronizeInvoke
        object Invoke(Action method);
    }

    /// <summary>
    /// Random Number Generator form
    /// </summary>
    public partial class GeneratorForm : Form, IGeneratorForm
    {
        #region Imports

        [DllImport("TruRNGpro.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern bool Initialize(int iPort, bool bSimulate);

        [DllImport("TruRNGpro.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool GetRandomBitAverage(ref double fResult);

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="sessionData">IN - The session data object</param>
        /// <param name="sessionDataFile">IN - The session data file object</param>
        public GeneratorForm(IRNGSessionData sessionData, IRNGSessionDataFile sessionDataFile, IRNGSessionTimer sessionTimer)
        {
            // Record the session object provided
            m_Data = sessionData;
            m_DataFile = sessionDataFile;
            m_SessionTimer = sessionTimer;

            // Dump USB devices if debugging and defined
#if DUMP_DEVICES && DEBUG
                DumpUSBDevices();
#endif
            // Initialize the thread pool
            InitializeThreadPool();

            // Initialize GUI
            InitializeComponent();
            InitPlot();

            // Set the info box to idle
            m_StatusTextBox.Text = m_sIDLE_MESSAGE;
            m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;

            // Start the device update thread and trigger an update
            DeviceUpdateThread.Parent = this;
            ThreadPool.QueueUserWorkItem(DeviceUpdateThread.ThreadProc);

            // Create the source from the list of device ports
            m_DeviceBindingSource = new BindingSource();
            m_DeviceBindingSource.DataSource = new BindingList<RNGDevice>();

            // Set the port combo box data source and define the display and value members
            m_PortComboBox.DataSource = m_DeviceBindingSource.DataSource;
            m_PortComboBox.DisplayMember = RNGDevice.DisplayMember;
            m_PortComboBox.ValueMember = RNGDevice.ValueMember;

            // Set the maximumn number of data points and averages to hold in memory
            // NOTE: Should match the window size of the data for the chart
            m_Data.DataWindowSize = m_iMAX_DATA_SIZE;

            // Ensure the correct default state of the simulate button
            m_SimulateToggle.Checked = m_Data.Simulated;

            // Initialize the target combo box and selection
            m_TargetComboBox.Items.AddRange((object[])TargetValues.TargetStrings.Clone());
            m_TargetComboBox.SelectedIndex = 0;

            // Set the timer text box in the timer object
            m_SessionTimer.TimerTextBox = m_SessionTimerTextBox;
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
                        ThreadPool.QueueUserWorkItem(DeviceUpdateThread.ThreadProc);
                        break;

                    // Ignore any other events
                    default:
                        // Do nothing
                        break;
                }
            }
        }
            
        /// <summary>
        /// Event handler for tick of the timer to read data
        /// </summary>
        /// <param name="sender">IN - Sender of the event (not used)</param>
        /// <param name="e">IN - The event arguments (not used)</param>
        private void ReadTimer_Tick(object sender, EventArgs e)
        {
            // Discard unused parameters
            _ = sender;
            _ = e;

            // Update the average
            double fCurrentValue = 0.0;
            bool bStatus = GetRandomBitAverage(ref fCurrentValue);

            if (false == bStatus)
            {
                // Stop so the user can correct the error
                StopButton_Click(sender, e);

                // Record and display error condition
                m_bInterfaceInitialized = false;
                this.m_StatusTextBox.BackColor = System.Drawing.Color.Red;
                m_StatusTextBox.Text = m_sDEVICE_READ_ERROR;
            }
            else // Successfully read from generator
            {
                // Record the new data point
                RecordDataPoint(fCurrentValue);

                // Update the displayed average and add the point to the chart
                m_CurrentAverageTextBox.Text = CurrentAverage;
                AddChartPoint(fCurrentValue);

                // Clear any displayed errors 
                m_StatusTextBox.Text = RunningMessage;
                this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;
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

            // Record the interface requires initialization
            m_bInterfaceInitialized = false;

            // End any session in progress
            EndSession();

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

            // Disable the interface controls
            m_SimulateToggle.Enabled = false;
            m_PortComboBox.Enabled = false;

            // Disable the target number field
            m_TargetComboBox.Enabled = false;

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
            if (m_bPaused)
            {
                // Resume
                m_bPaused = false;

                // Change the button back to pause
                m_PauseButton.Text = m_sPAUSE_BUTTON;

                // Re-enable the timers
                m_ReadTimer.Enabled = true;
                m_SessionTimer.Enabled = true;

                // Update the info box message
                m_StatusTextBox.Text = m_sIDLE_MESSAGE;
            }
            else
            {
                // Pause
                m_bPaused = true;

                // Change the button to resume
                m_PauseButton.Text = m_sRESUME_BUTTON;

                // Disable the timers
                m_ReadTimer.Enabled = false;
                m_SessionTimer.Enabled = false;

                // Update the info box message
                m_StatusTextBox.Text = m_sPAUSED_MESSAGE;
            }

            // Reset the background color of the info box
            this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;
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

            // End the current session
            EndSession();

            // Update the button statuses
            m_StartButton.Enabled = true;
            m_StopButton.Enabled = false;
            m_PauseButton.Enabled = false;

            // Reset the pause button
            m_PauseButton.Text = m_sPAUSE_BUTTON;
            m_bPaused = false;

            // Enable the target number field
            m_TargetComboBox.Enabled = true;

            // Enable the interface controls
            m_SimulateToggle.Enabled = true;
            m_PortComboBox.Enabled = true;

            // Update the info box
            m_StatusTextBox.Text = m_sIDLE_MESSAGE;
            this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;
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

            /// !!! mpullen - TODO !!!
            // Warn the user that resetting will clear the currently selected file

            // Reset the session data
            m_Data.Reset();

            // Reset the target value combo
            m_TargetComboBox.SelectedIndex = 0;

            // Clear the chart data and add a point so the area is displayed
            Series DataPointSeries = m_AverageChart.Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.Points.Clear();
            DataPointSeries.Points.AddY(0.5);
            Series AverageSeries = m_AverageChart.Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.Points.Clear();

            // Reset the Y-Axis
            Axis AverageChartYAxis = m_AverageChart.ChartAreas[0].AxisY;
            AverageChartYAxis.Maximum = 0.5 + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = 0.5 - m_fYAXIS_INCREMENT;

            // Update the timer and average displayed
            m_SessionTimerTextBox.Text = m_Data.SessionTime;
            m_CurrentAverageTextBox.Text = m_sAVERAGE_FORMAT;
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
            m_bInterfaceInitialized = false;
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
                    this.m_StatusTextBox.BackColor = System.Drawing.Color.Red;
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
            Busy = true;

            // Prompt the user to select a file
            string sSelectedFile = null;
            using (OpenFileDialog dataFileOpenDialog = new OpenFileDialog())
            {
                dataFileOpenDialog.Title = "Create or open a Random Number Generage data file";
                dataFileOpenDialog.FileName = "Select a data file";
                dataFileOpenDialog.Filter = "RNG data files (*.rng)|*.rng";

                // Allow creation of new files but directory must exist
                dataFileOpenDialog.CheckFileExists = false;
                dataFileOpenDialog.CheckPathExists = true;

                // Show the file open dialog
                DialogResult result = dataFileOpenDialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    sSelectedFile = dataFileOpenDialog.FileName;
                }
            }

            //!!! mpullen - need to verify if it is possible to go from having a file selected to not having one !!!

            // Check if the selection will have any effect
            Busy = (false == String.IsNullOrEmpty(sSelectedFile));
            if (Busy)
            {
                Busy = (sSelectedFile != m_DataFile.FilePath); 
            }

            // If the file is being changed
            if (Busy)
            {
                // Close any open session
                EndSession();

                // Check if the file exists
                bool bFileExists = File.Exists(sSelectedFile);
                if (bFileExists)
                {
                    //!!! mpullen - TBD load file

                    // Leave busy flag set until the file has been loaded (updated by thread)
                }
                else
                {
                    // Verify the user wants to start a new session
                    const string sCaption = "Create Session";
                    const string sMessage = "File does not exist. Would you like to create it and begin a new session?";
                    DialogResult confirmResult = MessageBox.Show(sMessage, sCaption, MessageBoxButtons.OKCancel);

                    // Reset the string if user opts to cancel
                    if (DialogResult.OK != confirmResult)
                    {
                        sSelectedFile = null;
                    }

                    // Either way we can re-enable session actions
                    Busy = false;
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

            // Make sure any running session is stoped
            StopButton_Click(sender, e);
            if (m_DataFile.SessionInProgress)
            {
                m_DataFile.EndSession();
            }
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
        /// Initializes the plot of the results
        /// </summary>
        private void InitPlot()
        {
            // Disable the X-Axis
            Axis AverageChartXAxis = m_AverageChart.ChartAreas[0].AxisX;
            AverageChartXAxis.Enabled = AxisEnabled.False;

            // Setup the Y-Axis
            Axis AverageChartYAxis = m_AverageChart.ChartAreas[0].AxisY;
            AverageChartYAxis.Maximum = 0.5 + m_fYAXIS_INCREMENT;
            AverageChartYAxis.Minimum = 0.5 - m_fYAXIS_INCREMENT;
            AverageChartYAxis.Interval = 0.005;

            // Setup the data point series and add a single point to force display
            Series DataPointSeries = m_AverageChart.Series[(int)SeriesIndex.DataPointSeries];
            DataPointSeries.IsVisibleInLegend = false;
            DataPointSeries.ChartType = SeriesChartType.Line;
            DataPointSeries.Color = Color.Blue;
            DataPointSeries.Points.AddY(0.5);

            // Setup the average series (no need to add initial point as data point series will display the chart)
            Series AverageSeries = m_AverageChart.Series[(int)SeriesIndex.AverageSeries];
            AverageSeries.IsVisibleInLegend = false;
            AverageSeries.ChartType = SeriesChartType.Line;
            AverageSeries.Color = Color.Red;
        }

        /// <summary>
        /// Initializes the interface to the device
        /// NOTE1: Port number data member must be set before calling.
        /// NOTE2: Success can be determined using initialized data member
        /// </summary>
        private void InitializeInterface()
        {
            // Attempt to initialize the interface
            int iPort = GetSelectedPort();
            m_bInterfaceInitialized = Initialize(iPort, m_Data.Simulated);

            // Initialization failed
            if (false == m_bInterfaceInitialized)
            {
                // Most likely issue is that the device is not at the specified port number
                this.m_StatusTextBox.BackColor = System.Drawing.Color.Red;
                m_StatusTextBox.Text = m_sDEVICE_INIT_ERROR;
            }
            else
            {
                // Initialized successfully to clear any displayed errors
                m_StatusTextBox.Text = m_sINIT_MESSAGE;
                this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;
            }
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

                    // If a session is currently in progress
                    if (m_DataFile.SessionInProgress)
                    {
                        // End it and start a new session (next time start is clicked
                        m_DataFile.EndSession();
                    }
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
        /// Start a new session if one is not already in progress
        /// </summary>
        /// <returns>true if successful; otherwise, false</returns>
        private bool StartSession()
        {
            bool bStatus = true;

            // If the interface is not initialized, attempt to initialize it
            if (false == m_bInterfaceInitialized)
            {
                InitializeInterface();
            }

            // Update the info box
            m_StatusTextBox.Text = RunningMessage;
            this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;

            // Write the session start if a file has been selected and session is not currently in progress
            if (m_DataFile.Valid && (false == m_DataFile.SessionInProgress))
            {
                bStatus = m_DataFile.StartSession(m_Data);
            }

            // Start the timers
            m_ReadTimer.Start();
            m_SessionTimer.Start();

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

            // Stop the timers if running
            m_ReadTimer.Stop();

            if (m_SessionTimer.InProgress)
            {
                m_SessionTimer.Stop();
            }

            // Only need to write to the file if a session is in progress
            if (m_DataFile.SessionInProgress)
            {
                // Flush any pending data and end the session
                bStatus = FlushPendingData();
                bStatus &= m_DataFile.EndSession();
            }

            return bStatus;
        }

        /// <summary>
        /// Adds a data point to the chart
        /// </summary>
        /// <param name="fDataPoint">IN - Data point to be added</param>
        private void AddChartPoint(double fDataPoint)
        {
            // Update the data points on the chart
            DataPointCollection DataPoints = m_AverageChart.Series[(int)SeriesIndex.DataPointSeries].Points;
            if (DataPoints.Count >= m_iMAX_DATA_SIZE)
            {
                DataPoints.RemoveAt(0);
            }
            DataPoints.AddY(fDataPoint);

            // Update the averages on the chart
            DataPointCollection AveragePoints = m_AverageChart.Series[(int)SeriesIndex.AverageSeries].Points;
            if (AveragePoints.Count >= m_iMAX_DATA_SIZE)
            {
                AveragePoints.RemoveAt(0);
            }
            AveragePoints.AddY(m_Data.CurrentAverage);

            // Find the min and max values on the chart
            double fMax = m_Data.MaxPoint;
            double fMin = m_Data.MinPoint;

            // Check if the limits need to be tightened
            Axis AverageChartYAxis = m_AverageChart.ChartAreas[0].AxisY;
            bool bMaxTooWide = ((AverageChartYAxis.Maximum - m_fYAXIS_INCREMENT) > fMax);
            bool bMinTooWide = ((AverageChartYAxis.Minimum + m_fYAXIS_INCREMENT) < fMin);
            if (bMaxTooWide && bMinTooWide)
            {
                // Tighten the limits
                while (bMaxTooWide && bMinTooWide)
                {
                    // Adjust them symetrically to maintain center
                    AverageChartYAxis.Maximum += m_fYAXIS_INCREMENT;
                    AverageChartYAxis.Minimum -= m_fYAXIS_INCREMENT;

                    bMaxTooWide = ((AverageChartYAxis.Maximum - m_fYAXIS_INCREMENT) > fMax);
                    bMinTooWide = ((AverageChartYAxis.Minimum + m_fYAXIS_INCREMENT) < fMin);
                }
            }
            else
            {
                // Widen the limits as needed
                while ((fMax >= AverageChartYAxis.Maximum) || (fMin <= AverageChartYAxis.Minimum))
                {
                    // Adjust them symetrically to maintain center
                    AverageChartYAxis.Maximum += m_fYAXIS_INCREMENT;
                    AverageChartYAxis.Minimum -= m_fYAXIS_INCREMENT;
                }
            }
        }

        /// <summary>
        /// Records a new data point
        /// </summary>
        /// <param name="fDataPoint">IN - The new data point</param>
        /// <returns>true, if successful; otherwise false</returns>
        private bool RecordDataPoint(double fDataPoint)
        {
            // Record the data point and check for write
            m_Data.AddDataPoint(fDataPoint);
            bool bStatus = WritePendingData();

            return bStatus;
        }

        /// <summary>
        /// Write a datap point to the file if the data interval has beenr reached
        /// </summary>
        /// <returns>true, if successful; otherwise false</returns>
        private bool WritePendingData()
        {
            // Default to true as the call is successful if nothings needs to be done
            bool bStatus = true;

            // If the data point interval has been reached
            if (m_iWRITE_FILE_INTERVAL <= ++m_iPendingDataPointCounter)
            {
                // Write the data point and reset the counter
                bStatus = m_DataFile.WriteDataPoint(m_Data);
                m_iPendingDataPointCounter = 0;
            }

            return bStatus;
        }

        /// <summary>
        /// Flushes the data point to the file if one is pending
        /// </summary>
        /// <returns>true, if successful; otherwise false</returns>
        private bool FlushPendingData()
        {
            // Default to true as the call is successful if nothings needs to be done
            bool bStatus = true;

            // Write the data point if one is pending and reset the counter
            if (0 < m_iPendingDataPointCounter)
            {
                bStatus = m_DataFile.WriteDataPoint(m_Data);
                m_iPendingDataPointCounter = 0;
            }

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

        #endregion
        #region Properties

        /// <summary>
        /// Whether the system is busy processing a change and should prevent session changes.
        /// NOTE: Exception is thrown by set if a session is currently running.
        /// </summary>
        public bool Busy
        { 
            get => m_bBusy;
            set
            {
                // Do not allow changes while a session is running
                if (Running)
                {
                    throw new Exception(m_sBUSY_CHANGE_WHILE_RUNNING_EXCEPTION);
                }
                else
                {
                    // Record the state change
                    m_bBusy = value;

                    // If setting as busy
                    if (m_bBusy)
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
        /// Whether a session is currently running
        /// </summary>
        public bool Running { get => m_SessionTimer.Running; }

        /// <summary>
        /// Binding list of devices used to populate the port combo box list
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
        /// Text displayed in the statatus box
        /// </summary>
        public string StatusBoxText { get => m_StatusTextBox.Text; set => m_StatusTextBox.Text = value; }

        /// <summary>
        /// Text color of the status box
        /// </summary>
        public Color StatusBoxTextColor { get => m_StatusTextBox.ForeColor; set => m_StatusTextBox.ForeColor = value; }

        /// <summary>
        /// Background color of the status box
        /// </summary>
        public Color StatusBoxBackColor { get => m_StatusTextBox.BackColor; set => m_StatusTextBox.BackColor = value; }

        /// <summary>
        /// Selected target value
        /// </summary>
        private int SelectedTarget
        {
            get { return TargetValues.GetValueAt((uint)m_TargetComboBox.SelectedIndex); }
            set { m_TargetComboBox.SelectedItem = TargetValues.ToString(value); }
        }

        /// <summary>
        /// Message to display in the info box while a session is running
        /// </summary>
        private string RunningMessage
        {
            get { return $"Running with Target = {m_TargetComboBox.SelectedItem}..."; }
        }

        /// <summary>
        /// Current average string
        /// </summary>
        private string CurrentAverage { get => m_Data.CurrentAverage.ToString(m_sAVERAGE_FORMAT); }

        #endregion
        #region Data Members

        // Form status and session data
        private bool m_bBusy = false;
        private IRNGSessionData m_Data = null;
        private IRNGSessionDataFile m_DataFile = null;
        private uint m_iPendingDataPointCounter = 0;
        private const uint m_iWRITE_FILE_INTERVAL = 1000;

        // Session timer
        IRNGSessionTimer m_SessionTimer = null;
        private bool m_bPaused = false;

        // Chart
        private int m_iMAX_DATA_SIZE = 1000 * 1024; // 1000 * 1024 * 4  = 4 MB
        private double m_fYAXIS_INCREMENT = 0.01;
        private enum SeriesIndex { DataPointSeries, AverageSeries, };

        // Device settings
        private int m_iSeed = 0;
        private bool m_bInterfaceInitialized = false;
        private BindingSource m_DeviceBindingSource;

        // Display settings
        private const string m_sAVERAGE_FORMAT = "0.000000000";

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

        // Other error messages
        private const string m_sBUSY_CHANGE_WHILE_RUNNING_EXCEPTION = "Busy property cannot be changed while a session is running.";

        #endregion
    }
}
