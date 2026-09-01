//*********************************************************************************************************************
// File Name:      GeneratorForm.Designer.cs
// Description:    Auto-generated code for the Random Number Generator GUI
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

namespace RandomNumberGenerator
{
    partial class GeneratorForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // NOTE: Dispose(bool) is implemented in GeneratorForm.cs so the terminating state is recorded as part
        // of disposal. Do not add another one here.

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_CurrentAverageLabel = new System.Windows.Forms.Label();
            this.m_CurrentAverageTextBox = new System.Windows.Forms.TextBox();
            this.m_SessionTimerTextBox = new System.Windows.Forms.TextBox();
            this.m_SessionTimerLabel = new System.Windows.Forms.Label();
            this.m_ClearButton = new System.Windows.Forms.Button();
            this.m_StopButton = new System.Windows.Forms.Button();
            this.m_StartButton = new System.Windows.Forms.Button();
            this.m_PortLabel = new System.Windows.Forms.Label();
            this.m_PortComboBox = new System.Windows.Forms.ComboBox();
            this.m_StatusTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultChart = new RandomNumberGenerator.RNGChart();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ExecuteTabPage = new System.Windows.Forms.TabPage();
            this.m_SetupGroupBox = new System.Windows.Forms.GroupBox();
            this.m_SimulateToggleLabel = new System.Windows.Forms.Label();
            this.m_SimulateToggle = new CommonControls.ToggleButton();
            this.m_PauseButton = new System.Windows.Forms.Button();
            this.m_FileBrowseButton = new System.Windows.Forms.Button();
            this.m_FileTextBox = new System.Windows.Forms.TextBox();
            this.m_FileLlabel = new System.Windows.Forms.Label();
            this.m_TargetComboBox = new System.Windows.Forms.ComboBox();
            this.m_TargetLabel = new System.Windows.Forms.Label();
            this.m_StatisticsGroupBox = new System.Windows.Forms.GroupBox();
            this.m_MeanDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_MeanDeviationLabel = new System.Windows.Forms.Label();
            this.m_StandardDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_StandardDeviationLabel = new System.Windows.Forms.Label();
            this.m_DataPointsTextBox = new System.Windows.Forms.TextBox();
            this.m_DataPointsLabel = new System.Windows.Forms.Label();
            this.AnalyzeTabPage = new System.Windows.Forms.TabPage();
            this.m_ComparisonGroupBox = new System.Windows.Forms.GroupBox();
            this.m_SkewnessTextBox = new System.Windows.Forms.TextBox();
            this.m_SkewnessLabel = new System.Windows.Forms.Label();
            this.m_MeanDifferenceTextBox = new System.Windows.Forms.TextBox();
            this.m_MeanDifferenceLabel = new System.Windows.Forms.Label();
            this.m_ResultGroupBox = new System.Windows.Forms.GroupBox();
            this.m_ResultKurtosisTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultKurtosisLabel = new System.Windows.Forms.Label();
            this.m_ResultSkewnessTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultSkewnessLabel = new System.Windows.Forms.Label();
            this.m_ResultStdDevTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultStdDevLlabel = new System.Windows.Forms.Label();
            this.m_ResultMeanTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultMeanLabel = new System.Windows.Forms.Label();
            this.m_ResultBrowseButton = new System.Windows.Forms.Button();
            this.m_ResultLabel = new System.Windows.Forms.Label();
            this.m_ResultTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineGroupBox = new System.Windows.Forms.GroupBox();
            this.m_BaselineKurtosisTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineKurtosisLabel = new System.Windows.Forms.Label();
            this.m_BaselineSkewnessTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineSkewnessLabel = new System.Windows.Forms.Label();
            this.m_BaselineStdDevTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineStdDevLlabel = new System.Windows.Forms.Label();
            this.m_BaselineMeanTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineMeanLabel = new System.Windows.Forms.Label();
            this.m_BaselineBrowseButton = new System.Windows.Forms.Button();
            this.m_BaselineLabel = new System.Windows.Forms.Label();
            this.m_BaselineTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultHistogramChart = new RandomNumberGenerator.HistogramChart();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).BeginInit();
            this.MainTabControl.SuspendLayout();
            this.ExecuteTabPage.SuspendLayout();
            this.m_SetupGroupBox.SuspendLayout();
            this.m_StatisticsGroupBox.SuspendLayout();
            this.AnalyzeTabPage.SuspendLayout();
            this.m_ComparisonGroupBox.SuspendLayout();
            this.m_ResultGroupBox.SuspendLayout();
            this.m_BaselineGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).BeginInit();
            this.SuspendLayout();
            // 
            // m_CurrentAverageLabel
            // 
            this.m_CurrentAverageLabel.AutoSize = true;
            this.m_CurrentAverageLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_CurrentAverageLabel.Location = new System.Drawing.Point(21, 72);
            this.m_CurrentAverageLabel.Name = "m_CurrentAverageLabel";
            this.m_CurrentAverageLabel.Size = new System.Drawing.Size(115, 19);
            this.m_CurrentAverageLabel.TabIndex = 0;
            this.m_CurrentAverageLabel.Text = "Currrent Average";
            this.m_CurrentAverageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_CurrentAverageTextBox
            // 
            this.m_CurrentAverageTextBox.Font = new System.Drawing.Font("Courier New", 12F);
            this.m_CurrentAverageTextBox.Location = new System.Drawing.Point(6, 96);
            this.m_CurrentAverageTextBox.Name = "m_CurrentAverageTextBox";
            this.m_CurrentAverageTextBox.ReadOnly = true;
            this.m_CurrentAverageTextBox.Size = new System.Drawing.Size(144, 26);
            this.m_CurrentAverageTextBox.TabIndex = 1;
            this.m_CurrentAverageTextBox.TabStop = false;
            this.m_CurrentAverageTextBox.Text = "0.000000000";
            this.m_CurrentAverageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_SessionTimerTextBox
            // 
            this.m_SessionTimerTextBox.Font = new System.Drawing.Font("Courier New", 12F);
            this.m_SessionTimerTextBox.Location = new System.Drawing.Point(6, 41);
            this.m_SessionTimerTextBox.Name = "m_SessionTimerTextBox";
            this.m_SessionTimerTextBox.ReadOnly = true;
            this.m_SessionTimerTextBox.Size = new System.Drawing.Size(144, 26);
            this.m_SessionTimerTextBox.TabIndex = 2;
            this.m_SessionTimerTextBox.TabStop = false;
            this.m_SessionTimerTextBox.Text = "00:00:00";
            this.m_SessionTimerTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_SessionTimerTextBox.WordWrap = false;
            // 
            // m_SessionTimerLabel
            // 
            this.m_SessionTimerLabel.AutoSize = true;
            this.m_SessionTimerLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_SessionTimerLabel.Location = new System.Drawing.Point(32, 17);
            this.m_SessionTimerLabel.Name = "m_SessionTimerLabel";
            this.m_SessionTimerLabel.Size = new System.Drawing.Size(92, 19);
            this.m_SessionTimerLabel.TabIndex = 3;
            this.m_SessionTimerLabel.Text = "Session Timer";
            // 
            // m_ClearButton
            // 
            this.m_ClearButton.CausesValidation = false;
            this.m_ClearButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.m_ClearButton.Location = new System.Drawing.Point(330, 143);
            this.m_ClearButton.Name = "m_ClearButton";
            this.m_ClearButton.Size = new System.Drawing.Size(91, 35);
            this.m_ClearButton.TabIndex = 9;
            this.m_ClearButton.Text = "CLEAR";
            this.m_ClearButton.UseVisualStyleBackColor = true;
            this.m_ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // m_StopButton
            // 
            this.m_StopButton.CausesValidation = false;
            this.m_StopButton.Enabled = false;
            this.m_StopButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.m_StopButton.Location = new System.Drawing.Point(224, 143);
            this.m_StopButton.Name = "m_StopButton";
            this.m_StopButton.Size = new System.Drawing.Size(91, 35);
            this.m_StopButton.TabIndex = 8;
            this.m_StopButton.Text = "STOP";
            this.m_StopButton.UseVisualStyleBackColor = true;
            this.m_StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // m_StartButton
            // 
            this.m_StartButton.CausesValidation = false;
            this.m_StartButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.m_StartButton.Location = new System.Drawing.Point(12, 143);
            this.m_StartButton.Name = "m_StartButton";
            this.m_StartButton.Size = new System.Drawing.Size(91, 35);
            this.m_StartButton.TabIndex = 6;
            this.m_StartButton.Text = "START";
            this.m_StartButton.UseVisualStyleBackColor = true;
            this.m_StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // m_PortLabel
            // 
            this.m_PortLabel.AutoSize = true;
            this.m_PortLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_PortLabel.Location = new System.Drawing.Point(30, 77);
            this.m_PortLabel.Name = "m_PortLabel";
            this.m_PortLabel.Size = new System.Drawing.Size(34, 19);
            this.m_PortLabel.TabIndex = 7;
            this.m_PortLabel.Text = "Port";
            // 
            // m_PortComboBox
            // 
            this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PortComboBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_PortComboBox.Location = new System.Drawing.Point(12, 102);
            this.m_PortComboBox.MaxLength = 2;
            this.m_PortComboBox.Name = "m_PortComboBox";
            this.m_PortComboBox.Size = new System.Drawing.Size(71, 25);
            this.m_PortComboBox.TabIndex = 3;
            this.m_PortComboBox.SelectedIndexChanged += new System.EventHandler(this.PortComboBox_SelectedIndexChanged);
            this.m_PortComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.PortTextBox_Validating);
            // 
            // m_StatusTextBox
            // 
            this.m_StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_StatusTextBox.BackColor = System.Drawing.SystemColors.Info;
            this.m_StatusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.m_StatusTextBox.Location = new System.Drawing.Point(3, 671);
            this.m_StatusTextBox.Name = "m_StatusTextBox";
            this.m_StatusTextBox.ReadOnly = true;
            this.m_StatusTextBox.Size = new System.Drawing.Size(751, 23);
            this.m_StatusTextBox.TabIndex = 8;
            this.m_StatusTextBox.TabStop = false;
            this.m_StatusTextBox.WordWrap = false;
            // 
            // m_ResultChart
            // 
            this.m_ResultChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultChart.Location = new System.Drawing.Point(6, 202);
            this.m_ResultChart.Name = "m_ResultChart";
            this.m_ResultChart.Size = new System.Drawing.Size(748, 463);
            this.m_ResultChart.TabIndex = 9;
            this.m_ResultChart.TabStop = false;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.MainTabControl.Controls.Add(this.ExecuteTabPage);
            this.MainTabControl.Controls.Add(this.AnalyzeTabPage);
            this.MainTabControl.HotTrack = true;
            this.MainTabControl.Location = new System.Drawing.Point(3, 4);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(768, 726);
            this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.MainTabControl.TabIndex = 10;
            // 
            // ExecuteTabPage
            // 
            this.ExecuteTabPage.Controls.Add(this.m_SetupGroupBox);
            this.ExecuteTabPage.Controls.Add(this.m_StatisticsGroupBox);
            this.ExecuteTabPage.Controls.Add(this.m_ResultChart);
            this.ExecuteTabPage.Controls.Add(this.m_StatusTextBox);
            this.ExecuteTabPage.Location = new System.Drawing.Point(4, 25);
            this.ExecuteTabPage.Name = "ExecuteTabPage";
            this.ExecuteTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ExecuteTabPage.Size = new System.Drawing.Size(760, 697);
            this.ExecuteTabPage.TabIndex = 0;
            this.ExecuteTabPage.Text = "Execute";
            this.ExecuteTabPage.UseVisualStyleBackColor = true;
            // 
            // m_SetupGroupBox
            // 
            this.m_SetupGroupBox.Controls.Add(this.m_SimulateToggleLabel);
            this.m_SetupGroupBox.Controls.Add(this.m_SimulateToggle);
            this.m_SetupGroupBox.Controls.Add(this.m_PauseButton);
            this.m_SetupGroupBox.Controls.Add(this.m_FileBrowseButton);
            this.m_SetupGroupBox.Controls.Add(this.m_FileTextBox);
            this.m_SetupGroupBox.Controls.Add(this.m_StartButton);
            this.m_SetupGroupBox.Controls.Add(this.m_StopButton);
            this.m_SetupGroupBox.Controls.Add(this.m_FileLlabel);
            this.m_SetupGroupBox.Controls.Add(this.m_ClearButton);
            this.m_SetupGroupBox.Controls.Add(this.m_TargetComboBox);
            this.m_SetupGroupBox.Controls.Add(this.m_TargetLabel);
            this.m_SetupGroupBox.Controls.Add(this.m_PortComboBox);
            this.m_SetupGroupBox.Controls.Add(this.m_PortLabel);
            this.m_SetupGroupBox.Location = new System.Drawing.Point(6, 6);
            this.m_SetupGroupBox.Name = "m_SetupGroupBox";
            this.m_SetupGroupBox.Size = new System.Drawing.Size(432, 190);
            this.m_SetupGroupBox.TabIndex = 11;
            this.m_SetupGroupBox.TabStop = false;
            this.m_SetupGroupBox.Text = "Setup";
            // 
            // m_SimulateToggleLabel
            // 
            this.m_SimulateToggleLabel.AutoSize = true;
            this.m_SimulateToggleLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_SimulateToggleLabel.Location = new System.Drawing.Point(97, 77);
            this.m_SimulateToggleLabel.Name = "m_SimulateToggleLabel";
            this.m_SimulateToggleLabel.Size = new System.Drawing.Size(61, 19);
            this.m_SimulateToggleLabel.TabIndex = 11;
            this.m_SimulateToggleLabel.Text = "Simulate";
            this.m_SimulateToggleLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // m_SimulateToggle
            // 
            this.m_SimulateToggle.CausesValidation = false;
            this.m_SimulateToggle.DisabledBackground = System.Drawing.Color.Gray;
            this.m_SimulateToggle.DisabledToggle = System.Drawing.Color.LightGray;
            this.m_SimulateToggle.Location = new System.Drawing.Point(92, 102);
            this.m_SimulateToggle.MinimumSize = new System.Drawing.Size(50, 25);
            this.m_SimulateToggle.Name = "m_SimulateToggle";
            this.m_SimulateToggle.OffBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OffToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.OnBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OnToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.Size = new System.Drawing.Size(71, 25);
            this.m_SimulateToggle.TabIndex = 4;
            this.m_SimulateToggle.UseVisualStyleBackColor = true;
            this.m_SimulateToggle.CheckedChanged += new System.EventHandler(this.SimulateToggle_CheckedChanged);
            // 
            // m_PauseButton
            // 
            this.m_PauseButton.CausesValidation = false;
            this.m_PauseButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.m_PauseButton.Location = new System.Drawing.Point(118, 143);
            this.m_PauseButton.Name = "m_PauseButton";
            this.m_PauseButton.Size = new System.Drawing.Size(91, 35);
            this.m_PauseButton.TabIndex = 7;
            this.m_PauseButton.Text = "PAUSE";
            this.m_PauseButton.UseVisualStyleBackColor = true;
            this.m_PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // m_FileBrowseButton
            // 
            this.m_FileBrowseButton.Location = new System.Drawing.Point(347, 40);
            this.m_FileBrowseButton.Name = "m_FileBrowseButton";
            this.m_FileBrowseButton.Size = new System.Drawing.Size(75, 29);
            this.m_FileBrowseButton.TabIndex = 2;
            this.m_FileBrowseButton.Text = "Browse...";
            this.m_FileBrowseButton.UseVisualStyleBackColor = true;
            this.m_FileBrowseButton.Click += new System.EventHandler(this.FileBrowseButton_Click);
            // 
            // m_FileTextBox
            // 
            this.m_FileTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_FileTextBox.Location = new System.Drawing.Point(12, 42);
            this.m_FileTextBox.Name = "m_FileTextBox";
            this.m_FileTextBox.ReadOnly = true;
            this.m_FileTextBox.Size = new System.Drawing.Size(329, 25);
            this.m_FileTextBox.TabIndex = 1;
            // 
            // m_FileLlabel
            // 
            this.m_FileLlabel.AutoSize = true;
            this.m_FileLlabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_FileLlabel.Location = new System.Drawing.Point(12, 17);
            this.m_FileLlabel.Name = "m_FileLlabel";
            this.m_FileLlabel.Size = new System.Drawing.Size(62, 19);
            this.m_FileLlabel.TabIndex = 10;
            this.m_FileLlabel.Text = "Data File";
            // 
            // m_TargetComboBox
            // 
            this.m_TargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_TargetComboBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_TargetComboBox.Location = new System.Drawing.Point(339, 102);
            this.m_TargetComboBox.Name = "m_TargetComboBox";
            this.m_TargetComboBox.Size = new System.Drawing.Size(83, 25);
            this.m_TargetComboBox.TabIndex = 5;
            // 
            // m_TargetLabel
            // 
            this.m_TargetLabel.AutoSize = true;
            this.m_TargetLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_TargetLabel.Location = new System.Drawing.Point(339, 77);
            this.m_TargetLabel.Name = "m_TargetLabel";
            this.m_TargetLabel.Size = new System.Drawing.Size(83, 19);
            this.m_TargetLabel.TabIndex = 8;
            this.m_TargetLabel.Text = "Target Value";
            // 
            // m_StatisticsGroupBox
            // 
            this.m_StatisticsGroupBox.Controls.Add(this.m_MeanDeviationTextBox);
            this.m_StatisticsGroupBox.Controls.Add(this.m_MeanDeviationLabel);
            this.m_StatisticsGroupBox.Controls.Add(this.m_StandardDeviationTextBox);
            this.m_StatisticsGroupBox.Controls.Add(this.m_StandardDeviationLabel);
            this.m_StatisticsGroupBox.Controls.Add(this.m_DataPointsTextBox);
            this.m_StatisticsGroupBox.Controls.Add(this.m_DataPointsLabel);
            this.m_StatisticsGroupBox.Controls.Add(this.m_CurrentAverageTextBox);
            this.m_StatisticsGroupBox.Controls.Add(this.m_CurrentAverageLabel);
            this.m_StatisticsGroupBox.Controls.Add(this.m_SessionTimerTextBox);
            this.m_StatisticsGroupBox.Controls.Add(this.m_SessionTimerLabel);
            this.m_StatisticsGroupBox.Location = new System.Drawing.Point(444, 6);
            this.m_StatisticsGroupBox.Name = "m_StatisticsGroupBox";
            this.m_StatisticsGroupBox.Size = new System.Drawing.Size(310, 190);
            this.m_StatisticsGroupBox.TabIndex = 10;
            this.m_StatisticsGroupBox.TabStop = false;
            this.m_StatisticsGroupBox.Text = "Statistics";
            // 
            // m_MeanDeviationTextBox
            // 
            this.m_MeanDeviationTextBox.Font = new System.Drawing.Font("Courier New", 12F);
            this.m_MeanDeviationTextBox.Location = new System.Drawing.Point(156, 96);
            this.m_MeanDeviationTextBox.Name = "m_MeanDeviationTextBox";
            this.m_MeanDeviationTextBox.ReadOnly = true;
            this.m_MeanDeviationTextBox.Size = new System.Drawing.Size(144, 26);
            this.m_MeanDeviationTextBox.TabIndex = 8;
            this.m_MeanDeviationTextBox.TabStop = false;
            this.m_MeanDeviationTextBox.Text = "0.000000e0";
            this.m_MeanDeviationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_MeanDeviationTextBox.WordWrap = false;
            // 
            // m_MeanDeviationLabel
            // 
            this.m_MeanDeviationLabel.AutoSize = true;
            this.m_MeanDeviationLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_MeanDeviationLabel.Location = new System.Drawing.Point(175, 72);
            this.m_MeanDeviationLabel.Name = "m_MeanDeviationLabel";
            this.m_MeanDeviationLabel.Size = new System.Drawing.Size(106, 19);
            this.m_MeanDeviationLabel.TabIndex = 9;
            this.m_MeanDeviationLabel.Text = "Mean Deviation";
            // 
            // m_StandardDeviationTextBox
            // 
            this.m_StandardDeviationTextBox.Font = new System.Drawing.Font("Courier New", 12F);
            this.m_StandardDeviationTextBox.Location = new System.Drawing.Point(156, 150);
            this.m_StandardDeviationTextBox.Name = "m_StandardDeviationTextBox";
            this.m_StandardDeviationTextBox.ReadOnly = true;
            this.m_StandardDeviationTextBox.Size = new System.Drawing.Size(144, 26);
            this.m_StandardDeviationTextBox.TabIndex = 6;
            this.m_StandardDeviationTextBox.TabStop = false;
            this.m_StandardDeviationTextBox.Text = "0.000000e0";
            this.m_StandardDeviationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_StandardDeviationTextBox.WordWrap = false;
            // 
            // m_StandardDeviationLabel
            // 
            this.m_StandardDeviationLabel.AutoSize = true;
            this.m_StandardDeviationLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_StandardDeviationLabel.Location = new System.Drawing.Point(165, 126);
            this.m_StandardDeviationLabel.Name = "m_StandardDeviationLabel";
            this.m_StandardDeviationLabel.Size = new System.Drawing.Size(126, 19);
            this.m_StandardDeviationLabel.TabIndex = 7;
            this.m_StandardDeviationLabel.Text = "Standard Deviation";
            // 
            // m_DataPointsTextBox
            // 
            this.m_DataPointsTextBox.Font = new System.Drawing.Font("Courier New", 12F);
            this.m_DataPointsTextBox.Location = new System.Drawing.Point(156, 42);
            this.m_DataPointsTextBox.Name = "m_DataPointsTextBox";
            this.m_DataPointsTextBox.ReadOnly = true;
            this.m_DataPointsTextBox.Size = new System.Drawing.Size(144, 26);
            this.m_DataPointsTextBox.TabIndex = 4;
            this.m_DataPointsTextBox.TabStop = false;
            this.m_DataPointsTextBox.Text = "0";
            this.m_DataPointsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_DataPointsTextBox.WordWrap = false;
            // 
            // m_DataPointsLabel
            // 
            this.m_DataPointsLabel.AutoSize = true;
            this.m_DataPointsLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.m_DataPointsLabel.Location = new System.Drawing.Point(189, 18);
            this.m_DataPointsLabel.Name = "m_DataPointsLabel";
            this.m_DataPointsLabel.Size = new System.Drawing.Size(79, 19);
            this.m_DataPointsLabel.TabIndex = 5;
            this.m_DataPointsLabel.Text = "Data Points";
            // 
            // AnalyzeTabPage
            // 
            this.AnalyzeTabPage.Controls.Add(this.m_ComparisonGroupBox);
            this.AnalyzeTabPage.Controls.Add(this.m_ResultGroupBox);
            this.AnalyzeTabPage.Controls.Add(this.m_BaselineGroupBox);
            this.AnalyzeTabPage.Controls.Add(this.m_ResultHistogramChart);
            this.AnalyzeTabPage.Location = new System.Drawing.Point(4, 25);
            this.AnalyzeTabPage.Name = "AnalyzeTabPage";
            this.AnalyzeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AnalyzeTabPage.Size = new System.Drawing.Size(760, 697);
            this.AnalyzeTabPage.TabIndex = 1;
            this.AnalyzeTabPage.Text = "Analyze";
            this.AnalyzeTabPage.UseVisualStyleBackColor = true;
            // 
            // m_ComparisonGroupBox
            // 
            this.m_ComparisonGroupBox.Controls.Add(this.m_SkewnessTextBox);
            this.m_ComparisonGroupBox.Controls.Add(this.m_SkewnessLabel);
            this.m_ComparisonGroupBox.Controls.Add(this.m_MeanDifferenceTextBox);
            this.m_ComparisonGroupBox.Controls.Add(this.m_MeanDifferenceLabel);
            this.m_ComparisonGroupBox.Location = new System.Drawing.Point(0, 105);
            this.m_ComparisonGroupBox.Name = "m_ComparisonGroupBox";
            this.m_ComparisonGroupBox.Size = new System.Drawing.Size(757, 54);
            this.m_ComparisonGroupBox.TabIndex = 18;
            this.m_ComparisonGroupBox.TabStop = false;
            this.m_ComparisonGroupBox.Text = "Comparison";
            // 
            // m_SkewnessTextBox
            // 
            this.m_SkewnessTextBox.Location = new System.Drawing.Point(503, 19);
            this.m_SkewnessTextBox.Name = "m_SkewnessTextBox";
            this.m_SkewnessTextBox.ReadOnly = true;
            this.m_SkewnessTextBox.Size = new System.Drawing.Size(244, 20);
            this.m_SkewnessTextBox.TabIndex = 3;
            // 
            // m_SkewnessLabel
            // 
            this.m_SkewnessLabel.AutoSize = true;
            this.m_SkewnessLabel.Location = new System.Drawing.Point(389, 22);
            this.m_SkewnessLabel.Name = "m_SkewnessLabel";
            this.m_SkewnessLabel.Size = new System.Drawing.Size(108, 13);
            this.m_SkewnessLabel.TabIndex = 2;
            this.m_SkewnessLabel.Text = "Skewness Difference";
            // 
            // m_MeanDifferenceTextBox
            // 
            this.m_MeanDifferenceTextBox.Location = new System.Drawing.Point(102, 19);
            this.m_MeanDifferenceTextBox.Name = "m_MeanDifferenceTextBox";
            this.m_MeanDifferenceTextBox.ReadOnly = true;
            this.m_MeanDifferenceTextBox.Size = new System.Drawing.Size(272, 20);
            this.m_MeanDifferenceTextBox.TabIndex = 1;
            // 
            // m_MeanDifferenceLabel
            // 
            this.m_MeanDifferenceLabel.AutoSize = true;
            this.m_MeanDifferenceLabel.Location = new System.Drawing.Point(7, 22);
            this.m_MeanDifferenceLabel.Name = "m_MeanDifferenceLabel";
            this.m_MeanDifferenceLabel.Size = new System.Drawing.Size(89, 13);
            this.m_MeanDifferenceLabel.TabIndex = 0;
            this.m_MeanDifferenceLabel.Text = "Mean Difference:";
            // 
            // m_ResultGroupBox
            // 
            this.m_ResultGroupBox.Controls.Add(this.m_ResultKurtosisTextBox);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultKurtosisLabel);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultSkewnessTextBox);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultSkewnessLabel);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultStdDevTextBox);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultStdDevLlabel);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultMeanTextBox);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultMeanLabel);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultBrowseButton);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultLabel);
            this.m_ResultGroupBox.Controls.Add(this.m_ResultTextBox);
            this.m_ResultGroupBox.Location = new System.Drawing.Point(383, 3);
            this.m_ResultGroupBox.Name = "m_ResultGroupBox";
            this.m_ResultGroupBox.Size = new System.Drawing.Size(374, 96);
            this.m_ResultGroupBox.TabIndex = 17;
            this.m_ResultGroupBox.TabStop = false;
            this.m_ResultGroupBox.Text = "Result";
            // 
            // m_ResultKurtosisTextBox
            // 
            this.m_ResultKurtosisTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultKurtosisTextBox.Location = new System.Drawing.Point(52, 67);
            this.m_ResultKurtosisTextBox.Name = "m_ResultKurtosisTextBox";
            this.m_ResultKurtosisTextBox.ReadOnly = true;
            this.m_ResultKurtosisTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_ResultKurtosisTextBox.TabIndex = 16;
            // 
            // m_ResultKurtosisLabel
            // 
            this.m_ResultKurtosisLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultKurtosisLabel.AutoSize = true;
            this.m_ResultKurtosisLabel.Location = new System.Drawing.Point(7, 70);
            this.m_ResultKurtosisLabel.Name = "m_ResultKurtosisLabel";
            this.m_ResultKurtosisLabel.Size = new System.Drawing.Size(47, 13);
            this.m_ResultKurtosisLabel.TabIndex = 15;
            this.m_ResultKurtosisLabel.Text = "Kurtosis:";
            // 
            // m_ResultSkewnessTextBox
            // 
            this.m_ResultSkewnessTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultSkewnessTextBox.Location = new System.Drawing.Point(246, 67);
            this.m_ResultSkewnessTextBox.Name = "m_ResultSkewnessTextBox";
            this.m_ResultSkewnessTextBox.ReadOnly = true;
            this.m_ResultSkewnessTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_ResultSkewnessTextBox.TabIndex = 14;
            // 
            // m_ResultSkewnessLabel
            // 
            this.m_ResultSkewnessLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultSkewnessLabel.AutoSize = true;
            this.m_ResultSkewnessLabel.Location = new System.Drawing.Point(181, 70);
            this.m_ResultSkewnessLabel.Name = "m_ResultSkewnessLabel";
            this.m_ResultSkewnessLabel.Size = new System.Drawing.Size(59, 13);
            this.m_ResultSkewnessLabel.TabIndex = 13;
            this.m_ResultSkewnessLabel.Text = "Skewness:";
            // 
            // m_ResultStdDevTextBox
            // 
            this.m_ResultStdDevTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultStdDevTextBox.Location = new System.Drawing.Point(246, 41);
            this.m_ResultStdDevTextBox.Name = "m_ResultStdDevTextBox";
            this.m_ResultStdDevTextBox.ReadOnly = true;
            this.m_ResultStdDevTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_ResultStdDevTextBox.TabIndex = 12;
            // 
            // m_ResultStdDevLlabel
            // 
            this.m_ResultStdDevLlabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultStdDevLlabel.AutoSize = true;
            this.m_ResultStdDevLlabel.Location = new System.Drawing.Point(191, 48);
            this.m_ResultStdDevLlabel.Name = "m_ResultStdDevLlabel";
            this.m_ResultStdDevLlabel.Size = new System.Drawing.Size(49, 13);
            this.m_ResultStdDevLlabel.TabIndex = 11;
            this.m_ResultStdDevLlabel.Text = "Std Dev:";
            // 
            // m_ResultMeanTextBox
            // 
            this.m_ResultMeanTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultMeanTextBox.Location = new System.Drawing.Point(52, 41);
            this.m_ResultMeanTextBox.Name = "m_ResultMeanTextBox";
            this.m_ResultMeanTextBox.ReadOnly = true;
            this.m_ResultMeanTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_ResultMeanTextBox.TabIndex = 8;
            // 
            // m_ResultMeanLabel
            // 
            this.m_ResultMeanLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultMeanLabel.AutoSize = true;
            this.m_ResultMeanLabel.Location = new System.Drawing.Point(7, 44);
            this.m_ResultMeanLabel.Name = "m_ResultMeanLabel";
            this.m_ResultMeanLabel.Size = new System.Drawing.Size(37, 13);
            this.m_ResultMeanLabel.TabIndex = 7;
            this.m_ResultMeanLabel.Text = "Mean:";
            // 
            // m_ResultBrowseButton
            // 
            this.m_ResultBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultBrowseButton.Location = new System.Drawing.Point(333, 15);
            this.m_ResultBrowseButton.Name = "m_ResultBrowseButton";
            this.m_ResultBrowseButton.Size = new System.Drawing.Size(31, 23);
            this.m_ResultBrowseButton.TabIndex = 5;
            this.m_ResultBrowseButton.Text = "...";
            this.m_ResultBrowseButton.UseVisualStyleBackColor = true;
            this.m_ResultBrowseButton.Click += new System.EventHandler(this.ResultBrowseButton_Click);
            // 
            // m_ResultLabel
            // 
            this.m_ResultLabel.AutoSize = true;
            this.m_ResultLabel.Location = new System.Drawing.Point(6, 18);
            this.m_ResultLabel.Name = "m_ResultLabel";
            this.m_ResultLabel.Size = new System.Drawing.Size(26, 13);
            this.m_ResultLabel.TabIndex = 1;
            this.m_ResultLabel.Text = "File:";
            // 
            // m_ResultTextBox
            // 
            this.m_ResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultTextBox.Location = new System.Drawing.Point(38, 15);
            this.m_ResultTextBox.Name = "m_ResultTextBox";
            this.m_ResultTextBox.ReadOnly = true;
            this.m_ResultTextBox.Size = new System.Drawing.Size(289, 20);
            this.m_ResultTextBox.TabIndex = 3;
            // 
            // m_BaselineGroupBox
            // 
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineKurtosisTextBox);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineKurtosisLabel);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineSkewnessTextBox);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineSkewnessLabel);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineStdDevTextBox);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineStdDevLlabel);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineMeanTextBox);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineMeanLabel);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineBrowseButton);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineLabel);
            this.m_BaselineGroupBox.Controls.Add(this.m_BaselineTextBox);
            this.m_BaselineGroupBox.Location = new System.Drawing.Point(0, 3);
            this.m_BaselineGroupBox.Name = "m_BaselineGroupBox";
            this.m_BaselineGroupBox.Size = new System.Drawing.Size(374, 96);
            this.m_BaselineGroupBox.TabIndex = 9;
            this.m_BaselineGroupBox.TabStop = false;
            this.m_BaselineGroupBox.Text = "Baseline";
            // 
            // m_BaselineKurtosisTextBox
            // 
            this.m_BaselineKurtosisTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineKurtosisTextBox.Location = new System.Drawing.Point(52, 67);
            this.m_BaselineKurtosisTextBox.Name = "m_BaselineKurtosisTextBox";
            this.m_BaselineKurtosisTextBox.ReadOnly = true;
            this.m_BaselineKurtosisTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_BaselineKurtosisTextBox.TabIndex = 16;
            // 
            // m_BaselineKurtosisLabel
            // 
            this.m_BaselineKurtosisLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineKurtosisLabel.AutoSize = true;
            this.m_BaselineKurtosisLabel.Location = new System.Drawing.Point(7, 70);
            this.m_BaselineKurtosisLabel.Name = "m_BaselineKurtosisLabel";
            this.m_BaselineKurtosisLabel.Size = new System.Drawing.Size(47, 13);
            this.m_BaselineKurtosisLabel.TabIndex = 15;
            this.m_BaselineKurtosisLabel.Text = "Kurtosis:";
            // 
            // m_BaselineSkewnessTextBox
            // 
            this.m_BaselineSkewnessTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineSkewnessTextBox.Location = new System.Drawing.Point(246, 67);
            this.m_BaselineSkewnessTextBox.Name = "m_BaselineSkewnessTextBox";
            this.m_BaselineSkewnessTextBox.ReadOnly = true;
            this.m_BaselineSkewnessTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_BaselineSkewnessTextBox.TabIndex = 14;
            // 
            // m_BaselineSkewnessLabel
            // 
            this.m_BaselineSkewnessLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineSkewnessLabel.AutoSize = true;
            this.m_BaselineSkewnessLabel.Location = new System.Drawing.Point(181, 70);
            this.m_BaselineSkewnessLabel.Name = "m_BaselineSkewnessLabel";
            this.m_BaselineSkewnessLabel.Size = new System.Drawing.Size(59, 13);
            this.m_BaselineSkewnessLabel.TabIndex = 13;
            this.m_BaselineSkewnessLabel.Text = "Skewness:";
            // 
            // m_BaselineStdDevTextBox
            // 
            this.m_BaselineStdDevTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineStdDevTextBox.Location = new System.Drawing.Point(246, 41);
            this.m_BaselineStdDevTextBox.Name = "m_BaselineStdDevTextBox";
            this.m_BaselineStdDevTextBox.ReadOnly = true;
            this.m_BaselineStdDevTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_BaselineStdDevTextBox.TabIndex = 12;
            // 
            // m_BaselineStdDevLlabel
            // 
            this.m_BaselineStdDevLlabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineStdDevLlabel.AutoSize = true;
            this.m_BaselineStdDevLlabel.Location = new System.Drawing.Point(191, 44);
            this.m_BaselineStdDevLlabel.Name = "m_BaselineStdDevLlabel";
            this.m_BaselineStdDevLlabel.Size = new System.Drawing.Size(49, 13);
            this.m_BaselineStdDevLlabel.TabIndex = 11;
            this.m_BaselineStdDevLlabel.Text = "Std Dev:";
            // 
            // m_BaselineMeanTextBox
            // 
            this.m_BaselineMeanTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineMeanTextBox.Location = new System.Drawing.Point(52, 41);
            this.m_BaselineMeanTextBox.Name = "m_BaselineMeanTextBox";
            this.m_BaselineMeanTextBox.ReadOnly = true;
            this.m_BaselineMeanTextBox.Size = new System.Drawing.Size(118, 20);
            this.m_BaselineMeanTextBox.TabIndex = 8;
            // 
            // m_BaselineMeanLabel
            // 
            this.m_BaselineMeanLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineMeanLabel.AutoSize = true;
            this.m_BaselineMeanLabel.Location = new System.Drawing.Point(7, 44);
            this.m_BaselineMeanLabel.Name = "m_BaselineMeanLabel";
            this.m_BaselineMeanLabel.Size = new System.Drawing.Size(37, 13);
            this.m_BaselineMeanLabel.TabIndex = 7;
            this.m_BaselineMeanLabel.Text = "Mean:";
            // 
            // m_BaselineBrowseButton
            // 
            this.m_BaselineBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineBrowseButton.Location = new System.Drawing.Point(333, 15);
            this.m_BaselineBrowseButton.Name = "m_BaselineBrowseButton";
            this.m_BaselineBrowseButton.Size = new System.Drawing.Size(31, 23);
            this.m_BaselineBrowseButton.TabIndex = 5;
            this.m_BaselineBrowseButton.Text = "...";
            this.m_BaselineBrowseButton.UseVisualStyleBackColor = true;
            this.m_BaselineBrowseButton.Click += new System.EventHandler(this.BaselineBrowseButton_Click);
            // 
            // m_BaselineLabel
            // 
            this.m_BaselineLabel.AutoSize = true;
            this.m_BaselineLabel.Location = new System.Drawing.Point(6, 18);
            this.m_BaselineLabel.Name = "m_BaselineLabel";
            this.m_BaselineLabel.Size = new System.Drawing.Size(26, 13);
            this.m_BaselineLabel.TabIndex = 1;
            this.m_BaselineLabel.Text = "File:";
            // 
            // m_BaselineTextBox
            // 
            this.m_BaselineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_BaselineTextBox.Location = new System.Drawing.Point(38, 15);
            this.m_BaselineTextBox.Name = "m_BaselineTextBox";
            this.m_BaselineTextBox.ReadOnly = true;
            this.m_BaselineTextBox.Size = new System.Drawing.Size(289, 20);
            this.m_BaselineTextBox.TabIndex = 3;
            // 
            // m_ResultHistogramChart
            // 
            this.m_ResultHistogramChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ResultHistogramChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultHistogramChart.Location = new System.Drawing.Point(6, 165);
            this.m_ResultHistogramChart.Name = "m_ResultHistogramChart";
            this.m_ResultHistogramChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            this.m_ResultHistogramChart.Size = new System.Drawing.Size(748, 500);
            this.m_ResultHistogramChart.TabIndex = 9;
            this.m_ResultHistogramChart.TabStop = false;
            // 
            // GeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(773, 732);
            this.Controls.Add(this.MainTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "GeneratorForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Random Number Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeneratorForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).EndInit();
            this.MainTabControl.ResumeLayout(false);
            this.ExecuteTabPage.ResumeLayout(false);
            this.ExecuteTabPage.PerformLayout();
            this.m_SetupGroupBox.ResumeLayout(false);
            this.m_SetupGroupBox.PerformLayout();
            this.m_StatisticsGroupBox.ResumeLayout(false);
            this.m_StatisticsGroupBox.PerformLayout();
            this.AnalyzeTabPage.ResumeLayout(false);
            this.m_ComparisonGroupBox.ResumeLayout(false);
            this.m_ComparisonGroupBox.PerformLayout();
            this.m_ResultGroupBox.ResumeLayout(false);
            this.m_ResultGroupBox.PerformLayout();
            this.m_BaselineGroupBox.ResumeLayout(false);
            this.m_BaselineGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_CurrentAverageLabel;
        private System.Windows.Forms.TextBox m_CurrentAverageTextBox;
        private System.Windows.Forms.TextBox m_SessionTimerTextBox;
        private System.Windows.Forms.Label m_SessionTimerLabel;
        private System.Windows.Forms.Button m_ClearButton;
        private System.Windows.Forms.Button m_StopButton;
        private System.Windows.Forms.Button m_StartButton;
        private System.Windows.Forms.Label m_PortLabel;
        private System.Windows.Forms.ComboBox m_PortComboBox;
        private System.Windows.Forms.TextBox m_StatusTextBox;
        private RNGChart m_ResultChart;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ExecuteTabPage;
        private System.Windows.Forms.TabPage AnalyzeTabPage;
        private System.Windows.Forms.GroupBox m_StatisticsGroupBox;
        private System.Windows.Forms.GroupBox m_SetupGroupBox;
        private System.Windows.Forms.TextBox m_StandardDeviationTextBox;
        private System.Windows.Forms.Label m_StandardDeviationLabel;
        private System.Windows.Forms.TextBox m_DataPointsTextBox;
        private System.Windows.Forms.Label m_DataPointsLabel;
        private System.Windows.Forms.Button m_FileBrowseButton;
        private System.Windows.Forms.TextBox m_FileTextBox;
        private System.Windows.Forms.Label m_FileLlabel;
        private System.Windows.Forms.ComboBox m_TargetComboBox;
        private System.Windows.Forms.Label m_TargetLabel;
        private System.Windows.Forms.Button m_PauseButton;
        private CommonControls.ToggleButton m_SimulateToggle;
        private System.Windows.Forms.TextBox m_MeanDeviationTextBox;
        private System.Windows.Forms.Label m_MeanDeviationLabel;
        private System.Windows.Forms.Label m_SimulateToggleLabel;
        private System.Windows.Forms.Button m_BaselineBrowseButton;
        private System.Windows.Forms.TextBox m_BaselineTextBox;
        private System.Windows.Forms.Label m_BaselineLabel;
        private HistogramChart m_ResultHistogramChart;
        private System.Windows.Forms.GroupBox m_BaselineGroupBox;
        private System.Windows.Forms.TextBox m_BaselineMeanTextBox;
        private System.Windows.Forms.Label m_BaselineMeanLabel;
        private System.Windows.Forms.TextBox m_BaselineKurtosisTextBox;
        private System.Windows.Forms.Label m_BaselineKurtosisLabel;
        private System.Windows.Forms.TextBox m_BaselineSkewnessTextBox;
        private System.Windows.Forms.Label m_BaselineSkewnessLabel;
        private System.Windows.Forms.TextBox m_BaselineStdDevTextBox;
        private System.Windows.Forms.Label m_BaselineStdDevLlabel;
        private System.Windows.Forms.GroupBox m_ResultGroupBox;
        private System.Windows.Forms.TextBox m_ResultKurtosisTextBox;
        private System.Windows.Forms.Label m_ResultKurtosisLabel;
        private System.Windows.Forms.TextBox m_ResultSkewnessTextBox;
        private System.Windows.Forms.Label m_ResultSkewnessLabel;
        private System.Windows.Forms.TextBox m_ResultStdDevTextBox;
        private System.Windows.Forms.Label m_ResultStdDevLlabel;
        private System.Windows.Forms.TextBox m_ResultMeanTextBox;
        private System.Windows.Forms.Label m_ResultMeanLabel;
        private System.Windows.Forms.Button m_ResultBrowseButton;
        private System.Windows.Forms.Label m_ResultLabel;
        private System.Windows.Forms.TextBox m_ResultTextBox;
        private System.Windows.Forms.GroupBox m_ComparisonGroupBox;
        private System.Windows.Forms.Label m_SkewnessLabel;
        private System.Windows.Forms.TextBox m_MeanDifferenceTextBox;
        private System.Windows.Forms.Label m_MeanDifferenceLabel;
        private System.Windows.Forms.TextBox m_SkewnessTextBox;
    }
}

