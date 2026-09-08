//*********************************************************************************************************************
// File Name:      GeneratorForm.Designer.cs
// Description:    Auto-generated code for the Random Number Generator GUI
//
// Copyright (c) 2022-2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History:
//====================================================================================================================
// 2022/09/10 - Mike Pullen - Original implementation.
// 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
// 2023/12/02 - Mike Pullen - Added simulate, pause, and target value
// 2026/09/07 - Mike Pullen - Status bar, borderless statistic readouts, and an emphasised primary button
// 2026/09/07 - Mike Pullen - Resizable window laid out in bands, so the chart takes the space that is left,
//                            and one comparison table in place of the mirrored analysis fields
// 2026/09/07 - Mike Pullen - Tooltips on the statistics and no-value placeholders before the first reading
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
            this.components = new System.ComponentModel.Container();
            this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_CurrentAverageLabel = new System.Windows.Forms.Label();
            this.m_CurrentAverageTextBox = new System.Windows.Forms.TextBox();
            this.m_SessionTimerTextBox = new System.Windows.Forms.TextBox();
            this.m_SessionTimerLabel = new System.Windows.Forms.Label();
            this.m_ClearButton = new System.Windows.Forms.Button();
            this.m_StopButton = new System.Windows.Forms.Button();
            this.m_StartButton = new System.Windows.Forms.Button();
            this.m_PortLabel = new System.Windows.Forms.Label();
            this.m_PortComboBox = new System.Windows.Forms.ComboBox();
            this.m_StatusStrip = new System.Windows.Forms.StatusStrip();
            this.m_StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_ResultChart = new RandomNumberGenerator.RNGChart();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ExecuteTabPage = new System.Windows.Forms.TabPage();
            this.m_ExecuteLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_SetupGroupBox = new System.Windows.Forms.GroupBox();
            this.m_SetupLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_ActionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.m_SimulateToggleLabel = new System.Windows.Forms.Label();
            this.m_SimulateToggle = new CommonControls.ToggleButton();
            this.m_PauseButton = new System.Windows.Forms.Button();
            this.m_FileBrowseButton = new System.Windows.Forms.Button();
            this.m_FileTextBox = new System.Windows.Forms.TextBox();
            this.m_FileLlabel = new System.Windows.Forms.Label();
            this.m_TargetComboBox = new System.Windows.Forms.ComboBox();
            this.m_TargetLabel = new System.Windows.Forms.Label();
            this.m_StatisticsGroupBox = new System.Windows.Forms.GroupBox();
            this.m_StatisticsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_MeanDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_MeanDeviationLabel = new System.Windows.Forms.Label();
            this.m_StandardDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_StandardDeviationLabel = new System.Windows.Forms.Label();
            this.m_DataPointsTextBox = new System.Windows.Forms.TextBox();
            this.m_DataPointsLabel = new System.Windows.Forms.Label();
            this.AnalyzeTabPage = new System.Windows.Forms.TabPage();
            this.m_AnalyzeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_FilesGroupBox = new System.Windows.Forms.GroupBox();
            this.m_FilesLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_ResultBrowseButton = new System.Windows.Forms.Button();
            this.m_ResultLabel = new System.Windows.Forms.Label();
            this.m_ResultTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineBrowseButton = new System.Windows.Forms.Button();
            this.m_BaselineLabel = new System.Windows.Forms.Label();
            this.m_BaselineTextBox = new System.Windows.Forms.TextBox();
            this.m_ComparisonGroupBox = new System.Windows.Forms.GroupBox();
            this.m_ComparisonList = new System.Windows.Forms.ListView();
            this.m_VerdictLabel = new System.Windows.Forms.Label();
            this.m_MeasureColumn = new System.Windows.Forms.ColumnHeader();
            this.m_BaselineColumn = new System.Windows.Forms.ColumnHeader();
            this.m_ResultColumn = new System.Windows.Forms.ColumnHeader();
            this.m_DifferenceColumn = new System.Windows.Forms.ColumnHeader();
            this.m_ResultHistogramChart = new RandomNumberGenerator.HistogramChart();
            this.m_StatusStrip.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ExecuteTabPage.SuspendLayout();
            this.m_ExecuteLayout.SuspendLayout();
            this.m_SetupGroupBox.SuspendLayout();
            this.m_SetupLayout.SuspendLayout();
            this.m_ActionPanel.SuspendLayout();
            this.m_StatisticsGroupBox.SuspendLayout();
            this.m_StatisticsLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).BeginInit();
            this.AnalyzeTabPage.SuspendLayout();
            this.m_AnalyzeLayout.SuspendLayout();
            this.m_FilesGroupBox.SuspendLayout();
            this.m_FilesLayout.SuspendLayout();
            this.m_ComparisonGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).BeginInit();
            this.SuspendLayout();
            //
            // m_StatusStrip
            //
            this.m_StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_StatusLabel});
            this.m_StatusStrip.Location = new System.Drawing.Point(0, 710);
            this.m_StatusStrip.Name = "m_StatusStrip";
            this.m_StatusStrip.Padding = new System.Windows.Forms.Padding(6, 0, 12, 0);
            this.m_StatusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.m_StatusStrip.Size = new System.Drawing.Size(773, 22);
            this.m_StatusStrip.SizingGrip = false;
            this.m_StatusStrip.TabIndex = 1;
            //
            // m_StatusLabel
            //
            this.m_StatusLabel.Name = "m_StatusLabel";
            this.m_StatusLabel.Size = new System.Drawing.Size(755, 17);
            this.m_StatusLabel.Spring = true;
            this.m_StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // MainTabControl
            //
            this.MainTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.MainTabControl.Controls.Add(this.ExecuteTabPage);
            this.MainTabControl.Controls.Add(this.AnalyzeTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.HotTrack = true;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.Padding = new System.Drawing.Point(16, 4);
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(773, 710);
            this.MainTabControl.TabIndex = 0;
            //
            // ExecuteTabPage
            //
            this.ExecuteTabPage.Controls.Add(this.m_ExecuteLayout);
            this.ExecuteTabPage.Location = new System.Drawing.Point(4, 27);
            this.ExecuteTabPage.Name = "ExecuteTabPage";
            this.ExecuteTabPage.Padding = new System.Windows.Forms.Padding(8);
            this.ExecuteTabPage.Size = new System.Drawing.Size(765, 679);
            this.ExecuteTabPage.TabIndex = 0;
            this.ExecuteTabPage.Text = "Record";
            this.ExecuteTabPage.UseVisualStyleBackColor = true;
            //
            // m_ExecuteLayout
            //
            this.m_ExecuteLayout.ColumnCount = 1;
            this.m_ExecuteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ExecuteLayout.Controls.Add(this.m_SetupGroupBox, 0, 0);
            this.m_ExecuteLayout.Controls.Add(this.m_StatisticsGroupBox, 0, 1);
            this.m_ExecuteLayout.Controls.Add(this.m_ResultChart, 0, 2);
            this.m_ExecuteLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ExecuteLayout.Location = new System.Drawing.Point(8, 8);
            this.m_ExecuteLayout.Name = "m_ExecuteLayout";
            this.m_ExecuteLayout.RowCount = 3;
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ExecuteLayout.Size = new System.Drawing.Size(749, 663);
            this.m_ExecuteLayout.TabIndex = 0;
            //
            // m_SetupGroupBox
            //
            this.m_SetupGroupBox.Controls.Add(this.m_SetupLayout);
            this.m_SetupGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SetupGroupBox.Location = new System.Drawing.Point(3, 3);
            this.m_SetupGroupBox.Name = "m_SetupGroupBox";
            this.m_SetupGroupBox.Padding = new System.Windows.Forms.Padding(10, 6, 10, 8);
            this.m_SetupGroupBox.Size = new System.Drawing.Size(743, 152);
            this.m_SetupGroupBox.TabIndex = 0;
            this.m_SetupGroupBox.TabStop = false;
            this.m_SetupGroupBox.Text = "Setup";
            //
            // m_SetupLayout
            //
            this.m_SetupLayout.ColumnCount = 4;
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.m_SetupLayout.Controls.Add(this.m_FileLlabel, 0, 0);
            this.m_SetupLayout.Controls.Add(this.m_FileTextBox, 0, 1);
            this.m_SetupLayout.Controls.Add(this.m_FileBrowseButton, 3, 1);
            this.m_SetupLayout.Controls.Add(this.m_PortLabel, 0, 2);
            this.m_SetupLayout.Controls.Add(this.m_SimulateToggleLabel, 1, 2);
            this.m_SetupLayout.Controls.Add(this.m_TargetLabel, 3, 2);
            this.m_SetupLayout.Controls.Add(this.m_PortComboBox, 0, 3);
            this.m_SetupLayout.Controls.Add(this.m_SimulateToggle, 1, 3);
            this.m_SetupLayout.Controls.Add(this.m_TargetComboBox, 3, 3);
            this.m_SetupLayout.Controls.Add(this.m_ActionPanel, 0, 4);
            this.m_SetupLayout.Controls.Add(this.m_ClearButton, 3, 4);
            this.m_SetupLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SetupLayout.Location = new System.Drawing.Point(10, 22);
            this.m_SetupLayout.Name = "m_SetupLayout";
            this.m_SetupLayout.RowCount = 5;
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_SetupLayout.Size = new System.Drawing.Size(723, 122);
            this.m_SetupLayout.TabIndex = 0;
            //
            // m_FileLlabel
            //
            this.m_FileLlabel.AutoSize = true;
            this.m_SetupLayout.SetColumnSpan(this.m_FileLlabel, 3);
            this.m_FileLlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_FileLlabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_FileLlabel.Location = new System.Drawing.Point(3, 0);
            this.m_FileLlabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_FileLlabel.Name = "m_FileLlabel";
            this.m_FileLlabel.Size = new System.Drawing.Size(601, 19);
            this.m_FileLlabel.TabIndex = 0;
            this.m_FileLlabel.Text = "Data file";
            this.m_FileLlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_FileTextBox
            //
            this.m_SetupLayout.SetColumnSpan(this.m_FileTextBox, 3);
            this.m_FileTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_FileTextBox.Location = new System.Drawing.Point(3, 22);
            this.m_FileTextBox.Name = "m_FileTextBox";
            this.m_FileTextBox.ReadOnly = true;
            this.m_FileTextBox.Size = new System.Drawing.Size(601, 23);
            this.m_FileTextBox.TabIndex = 0;
            //
            // m_FileBrowseButton
            //
            this.m_FileBrowseButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_FileBrowseButton.Location = new System.Drawing.Point(610, 22);
            this.m_FileBrowseButton.Name = "m_FileBrowseButton";
            this.m_FileBrowseButton.Size = new System.Drawing.Size(110, 25);
            this.m_FileBrowseButton.TabIndex = 1;
            this.m_FileBrowseButton.Text = "&Browse...";
            this.m_FileBrowseButton.UseVisualStyleBackColor = true;
            this.m_FileBrowseButton.Click += new System.EventHandler(this.FileBrowseButton_Click);
            //
            // m_PortLabel
            //
            this.m_PortLabel.AutoSize = true;
            this.m_PortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_PortLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_PortLabel.Location = new System.Drawing.Point(3, 48);
            this.m_PortLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_PortLabel.Name = "m_PortLabel";
            this.m_PortLabel.Size = new System.Drawing.Size(98, 19);
            this.m_PortLabel.TabIndex = 0;
            this.m_PortLabel.Text = "Port";
            this.m_PortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_SimulateToggleLabel
            //
            this.m_SimulateToggleLabel.AutoSize = true;
            this.m_SimulateToggleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SimulateToggleLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_SimulateToggleLabel.Location = new System.Drawing.Point(107, 48);
            this.m_SimulateToggleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_SimulateToggleLabel.Name = "m_SimulateToggleLabel";
            this.m_SimulateToggleLabel.Size = new System.Drawing.Size(110, 19);
            this.m_SimulateToggleLabel.TabIndex = 0;
            this.m_SimulateToggleLabel.Text = "Device / Simulator";
            this.m_SimulateToggleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_TargetLabel
            //
            this.m_TargetLabel.AutoSize = true;
            this.m_TargetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_TargetLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_TargetLabel.Location = new System.Drawing.Point(610, 48);
            this.m_TargetLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_TargetLabel.Name = "m_TargetLabel";
            this.m_TargetLabel.Size = new System.Drawing.Size(110, 19);
            this.m_TargetLabel.TabIndex = 0;
            this.m_TargetLabel.Text = "Target value";
            this.m_TargetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_PortComboBox
            //
            this.m_PortComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PortComboBox.Location = new System.Drawing.Point(3, 70);
            this.m_PortComboBox.MaxLength = 2;
            this.m_PortComboBox.Name = "m_PortComboBox";
            this.m_PortComboBox.Size = new System.Drawing.Size(98, 23);
            this.m_PortComboBox.TabIndex = 2;
            this.m_PortComboBox.SelectedIndexChanged += new System.EventHandler(this.PortComboBox_SelectedIndexChanged);
            this.m_PortComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.PortTextBox_Validating);
            //
            // m_SimulateToggle
            //
            this.m_SimulateToggle.CausesValidation = false;
            this.m_SimulateToggle.DisabledBackground = System.Drawing.Color.Gray;
            this.m_SimulateToggle.DisabledToggle = System.Drawing.Color.LightGray;
            this.m_SimulateToggle.Location = new System.Drawing.Point(107, 70);
            this.m_SimulateToggle.MinimumSize = new System.Drawing.Size(50, 25);
            this.m_SimulateToggle.Name = "m_SimulateToggle";
            this.m_SimulateToggle.OffBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OffToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.OnBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OnToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.Size = new System.Drawing.Size(71, 25);
            this.m_SimulateToggle.TabIndex = 3;
            this.m_SimulateToggle.UseVisualStyleBackColor = true;
            this.m_SimulateToggle.CheckedChanged += new System.EventHandler(this.SimulateToggle_CheckedChanged);
            //
            // m_TargetComboBox
            //
            this.m_TargetComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_TargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_TargetComboBox.Location = new System.Drawing.Point(610, 70);
            this.m_TargetComboBox.Name = "m_TargetComboBox";
            this.m_TargetComboBox.Size = new System.Drawing.Size(110, 23);
            this.m_TargetComboBox.TabIndex = 4;
            //
            // m_ActionPanel
            //
            this.m_SetupLayout.SetColumnSpan(this.m_ActionPanel, 3);
            this.m_ActionPanel.Controls.Add(this.m_StartButton);
            this.m_ActionPanel.Controls.Add(this.m_PauseButton);
            this.m_ActionPanel.Controls.Add(this.m_StopButton);
            this.m_ActionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ActionPanel.Location = new System.Drawing.Point(0, 96);
            this.m_ActionPanel.Margin = new System.Windows.Forms.Padding(0);
            this.m_ActionPanel.Name = "m_ActionPanel";
            this.m_ActionPanel.Padding = new System.Windows.Forms.Padding(3, 6, 0, 0);
            this.m_ActionPanel.Size = new System.Drawing.Size(607, 26);
            this.m_ActionPanel.TabIndex = 5;
            this.m_ActionPanel.WrapContents = false;
            //
            // m_StartButton
            //
            this.m_StartButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(92)))), ((int)(((byte)(153)))));
            this.m_StartButton.CausesValidation = false;
            this.m_StartButton.FlatAppearance.BorderSize = 0;
            this.m_StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_StartButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.m_StartButton.ForeColor = System.Drawing.Color.White;
            this.m_StartButton.Location = new System.Drawing.Point(3, 6);
            this.m_StartButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_StartButton.Name = "m_StartButton";
            this.m_StartButton.Size = new System.Drawing.Size(116, 32);
            this.m_StartButton.TabIndex = 0;
            this.m_StartButton.Text = "&Start";
            this.m_StartButton.UseVisualStyleBackColor = false;
            this.m_StartButton.Click += new System.EventHandler(this.StartButton_Click);
            //
            // m_PauseButton
            //
            this.m_PauseButton.CausesValidation = false;
            this.m_PauseButton.Enabled = false;
            this.m_PauseButton.Location = new System.Drawing.Point(127, 6);
            this.m_PauseButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_PauseButton.Name = "m_PauseButton";
            this.m_PauseButton.Size = new System.Drawing.Size(100, 32);
            this.m_PauseButton.TabIndex = 1;
            this.m_PauseButton.Text = "&Pause";
            this.m_PauseButton.UseVisualStyleBackColor = true;
            this.m_PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            //
            // m_StopButton
            //
            this.m_StopButton.CausesValidation = false;
            this.m_StopButton.Enabled = false;
            this.m_StopButton.Location = new System.Drawing.Point(235, 6);
            this.m_StopButton.Margin = new System.Windows.Forms.Padding(0);
            this.m_StopButton.Name = "m_StopButton";
            this.m_StopButton.Size = new System.Drawing.Size(100, 32);
            this.m_StopButton.TabIndex = 2;
            this.m_StopButton.Text = "S&top";
            this.m_StopButton.UseVisualStyleBackColor = true;
            this.m_StopButton.Click += new System.EventHandler(this.StopButton_Click);
            //
            // m_ClearButton
            //
            this.m_ClearButton.CausesValidation = false;
            this.m_ClearButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_ClearButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this.m_ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_ClearButton.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_ClearButton.Location = new System.Drawing.Point(610, 102);
            this.m_ClearButton.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.m_ClearButton.Name = "m_ClearButton";
            this.m_ClearButton.Size = new System.Drawing.Size(110, 32);
            this.m_ClearButton.TabIndex = 6;
            this.m_ClearButton.Text = "&Clear";
            this.m_ClearButton.UseVisualStyleBackColor = true;
            this.m_ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            //
            // m_StatisticsGroupBox
            //
            this.m_StatisticsGroupBox.Controls.Add(this.m_StatisticsLayout);
            this.m_StatisticsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StatisticsGroupBox.Location = new System.Drawing.Point(3, 161);
            this.m_StatisticsGroupBox.Name = "m_StatisticsGroupBox";
            this.m_StatisticsGroupBox.Padding = new System.Windows.Forms.Padding(10, 4, 10, 6);
            this.m_StatisticsGroupBox.Size = new System.Drawing.Size(743, 70);
            this.m_StatisticsGroupBox.TabIndex = 1;
            this.m_StatisticsGroupBox.TabStop = false;
            this.m_StatisticsGroupBox.Text = "Statistics";
            //
            // m_StatisticsLayout
            //
            this.m_StatisticsLayout.ColumnCount = 5;
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.Controls.Add(this.m_SessionTimerLabel, 0, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_DataPointsLabel, 1, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_CurrentAverageLabel, 2, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_MeanDeviationLabel, 3, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_StandardDeviationLabel, 4, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_SessionTimerTextBox, 0, 1);
            this.m_StatisticsLayout.Controls.Add(this.m_DataPointsTextBox, 1, 1);
            this.m_StatisticsLayout.Controls.Add(this.m_CurrentAverageTextBox, 2, 1);
            this.m_StatisticsLayout.Controls.Add(this.m_MeanDeviationTextBox, 3, 1);
            this.m_StatisticsLayout.Controls.Add(this.m_StandardDeviationTextBox, 4, 1);
            this.m_StatisticsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StatisticsLayout.Location = new System.Drawing.Point(10, 20);
            this.m_StatisticsLayout.Name = "m_StatisticsLayout";
            this.m_StatisticsLayout.RowCount = 2;
            this.m_StatisticsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.m_StatisticsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_StatisticsLayout.Size = new System.Drawing.Size(723, 44);
            this.m_StatisticsLayout.TabIndex = 0;
            //
            // m_SessionTimerLabel
            //
            this.m_SessionTimerLabel.AutoSize = true;
            this.m_SessionTimerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SessionTimerLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_SessionTimerLabel.Location = new System.Drawing.Point(3, 0);
            this.m_SessionTimerLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_SessionTimerLabel.Name = "m_SessionTimerLabel";
            this.m_SessionTimerLabel.Size = new System.Drawing.Size(138, 18);
            this.m_SessionTimerLabel.TabIndex = 0;
            this.m_SessionTimerLabel.Text = "Session timer";
            this.m_SessionTimerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // m_DataPointsLabel
            //
            this.m_DataPointsLabel.AutoSize = true;
            this.m_DataPointsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_DataPointsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_DataPointsLabel.Location = new System.Drawing.Point(147, 0);
            this.m_DataPointsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_DataPointsLabel.Name = "m_DataPointsLabel";
            this.m_DataPointsLabel.Size = new System.Drawing.Size(138, 18);
            this.m_DataPointsLabel.TabIndex = 0;
            this.m_DataPointsLabel.Text = "Data points";
            this.m_DataPointsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // m_CurrentAverageLabel
            //
            this.m_CurrentAverageLabel.AutoSize = true;
            this.m_CurrentAverageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_CurrentAverageLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_CurrentAverageLabel.Location = new System.Drawing.Point(291, 0);
            this.m_CurrentAverageLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_CurrentAverageLabel.Name = "m_CurrentAverageLabel";
            this.m_CurrentAverageLabel.Size = new System.Drawing.Size(138, 18);
            this.m_CurrentAverageLabel.TabIndex = 0;
            this.m_CurrentAverageLabel.Text = "Current average";
            this.m_CurrentAverageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // m_MeanDeviationLabel
            //
            this.m_MeanDeviationLabel.AutoSize = true;
            this.m_MeanDeviationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_MeanDeviationLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_MeanDeviationLabel.Location = new System.Drawing.Point(435, 0);
            this.m_MeanDeviationLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_MeanDeviationLabel.Name = "m_MeanDeviationLabel";
            this.m_MeanDeviationLabel.Size = new System.Drawing.Size(138, 18);
            this.m_MeanDeviationLabel.TabIndex = 0;
            this.m_MeanDeviationLabel.Text = "Deviation from 0.5";
            this.m_MeanDeviationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // m_StandardDeviationLabel
            //
            this.m_StandardDeviationLabel.AutoSize = true;
            this.m_StandardDeviationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StandardDeviationLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_StandardDeviationLabel.Location = new System.Drawing.Point(579, 0);
            this.m_StandardDeviationLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_StandardDeviationLabel.Name = "m_StandardDeviationLabel";
            this.m_StandardDeviationLabel.Size = new System.Drawing.Size(141, 18);
            this.m_StandardDeviationLabel.TabIndex = 0;
            this.m_StandardDeviationLabel.Text = "Standard deviation";
            this.m_StandardDeviationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // m_SessionTimerTextBox
            //
            this.m_SessionTimerTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.m_SessionTimerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_SessionTimerTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_SessionTimerTextBox.Font = new System.Drawing.Font("Consolas", 13F);
            this.m_SessionTimerTextBox.Location = new System.Drawing.Point(3, 21);
            this.m_SessionTimerTextBox.Name = "m_SessionTimerTextBox";
            this.m_SessionTimerTextBox.ReadOnly = true;
            this.m_SessionTimerTextBox.Size = new System.Drawing.Size(138, 21);
            this.m_SessionTimerTextBox.TabIndex = 0;
            this.m_SessionTimerTextBox.TabStop = false;
            this.m_SessionTimerTextBox.Text = "00:00:00";
            this.m_SessionTimerTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_SessionTimerTextBox.WordWrap = false;
            //
            // m_DataPointsTextBox
            //
            this.m_DataPointsTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.m_DataPointsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_DataPointsTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_DataPointsTextBox.Font = new System.Drawing.Font("Consolas", 13F);
            this.m_DataPointsTextBox.Location = new System.Drawing.Point(147, 21);
            this.m_DataPointsTextBox.Name = "m_DataPointsTextBox";
            this.m_DataPointsTextBox.ReadOnly = true;
            this.m_DataPointsTextBox.Size = new System.Drawing.Size(138, 21);
            this.m_DataPointsTextBox.TabIndex = 0;
            this.m_DataPointsTextBox.TabStop = false;
            this.m_DataPointsTextBox.Text = "0";
            this.m_DataPointsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_DataPointsTextBox.WordWrap = false;
            //
            // m_CurrentAverageTextBox
            //
            this.m_CurrentAverageTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.m_CurrentAverageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_CurrentAverageTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_CurrentAverageTextBox.Font = new System.Drawing.Font("Consolas", 13F);
            this.m_CurrentAverageTextBox.Location = new System.Drawing.Point(291, 21);
            this.m_CurrentAverageTextBox.Name = "m_CurrentAverageTextBox";
            this.m_CurrentAverageTextBox.ReadOnly = true;
            this.m_CurrentAverageTextBox.Size = new System.Drawing.Size(138, 21);
            this.m_CurrentAverageTextBox.TabIndex = 0;
            this.m_CurrentAverageTextBox.TabStop = false;
            this.m_CurrentAverageTextBox.Text = "—";
            this.m_CurrentAverageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_CurrentAverageTextBox.WordWrap = false;
            //
            // m_MeanDeviationTextBox
            //
            this.m_MeanDeviationTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.m_MeanDeviationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_MeanDeviationTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_MeanDeviationTextBox.Font = new System.Drawing.Font("Consolas", 13F);
            this.m_MeanDeviationTextBox.Location = new System.Drawing.Point(435, 21);
            this.m_MeanDeviationTextBox.Name = "m_MeanDeviationTextBox";
            this.m_MeanDeviationTextBox.ReadOnly = true;
            this.m_MeanDeviationTextBox.Size = new System.Drawing.Size(138, 21);
            this.m_MeanDeviationTextBox.TabIndex = 0;
            this.m_MeanDeviationTextBox.TabStop = false;
            this.m_MeanDeviationTextBox.Text = "—";
            this.m_MeanDeviationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_MeanDeviationTextBox.WordWrap = false;
            //
            // m_StandardDeviationTextBox
            //
            this.m_StandardDeviationTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.m_StandardDeviationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_StandardDeviationTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_StandardDeviationTextBox.Font = new System.Drawing.Font("Consolas", 13F);
            this.m_StandardDeviationTextBox.Location = new System.Drawing.Point(579, 21);
            this.m_StandardDeviationTextBox.Name = "m_StandardDeviationTextBox";
            this.m_StandardDeviationTextBox.ReadOnly = true;
            this.m_StandardDeviationTextBox.Size = new System.Drawing.Size(141, 21);
            this.m_StandardDeviationTextBox.TabIndex = 0;
            this.m_StandardDeviationTextBox.TabStop = false;
            this.m_StandardDeviationTextBox.Text = "—";
            this.m_StandardDeviationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_StandardDeviationTextBox.WordWrap = false;
            //
            // m_ResultChart
            //
            this.m_ResultChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultChart.Location = new System.Drawing.Point(3, 237);
            this.m_ResultChart.Name = "m_ResultChart";
            this.m_ResultChart.Size = new System.Drawing.Size(743, 423);
            this.m_ResultChart.TabIndex = 2;
            this.m_ResultChart.TabStop = false;
            //
            // AnalyzeTabPage
            //
            this.AnalyzeTabPage.Controls.Add(this.m_AnalyzeLayout);
            this.AnalyzeTabPage.Location = new System.Drawing.Point(4, 27);
            this.AnalyzeTabPage.Name = "AnalyzeTabPage";
            this.AnalyzeTabPage.Padding = new System.Windows.Forms.Padding(8);
            this.AnalyzeTabPage.Size = new System.Drawing.Size(765, 679);
            this.AnalyzeTabPage.TabIndex = 1;
            this.AnalyzeTabPage.Text = "Analyse";
            this.AnalyzeTabPage.UseVisualStyleBackColor = true;
            //
            // m_AnalyzeLayout
            //
            this.m_AnalyzeLayout.ColumnCount = 1;
            this.m_AnalyzeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_AnalyzeLayout.Controls.Add(this.m_FilesGroupBox, 0, 0);
            this.m_AnalyzeLayout.Controls.Add(this.m_ComparisonGroupBox, 0, 1);
            this.m_AnalyzeLayout.Controls.Add(this.m_ResultHistogramChart, 0, 2);
            this.m_AnalyzeLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_AnalyzeLayout.Location = new System.Drawing.Point(8, 8);
            this.m_AnalyzeLayout.Name = "m_AnalyzeLayout";
            this.m_AnalyzeLayout.RowCount = 3;
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 244F));
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_AnalyzeLayout.Size = new System.Drawing.Size(749, 663);
            this.m_AnalyzeLayout.TabIndex = 0;
            //
            // m_FilesGroupBox
            //
            this.m_FilesGroupBox.Controls.Add(this.m_FilesLayout);
            this.m_FilesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_FilesGroupBox.Location = new System.Drawing.Point(3, 3);
            this.m_FilesGroupBox.Name = "m_FilesGroupBox";
            this.m_FilesGroupBox.Padding = new System.Windows.Forms.Padding(10, 4, 10, 6);
            this.m_FilesGroupBox.Size = new System.Drawing.Size(743, 86);
            this.m_FilesGroupBox.TabIndex = 0;
            this.m_FilesGroupBox.TabStop = false;
            this.m_FilesGroupBox.Text = "Sessions";
            //
            // m_FilesLayout
            //
            this.m_FilesLayout.ColumnCount = 3;
            this.m_FilesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.m_FilesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_FilesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.m_FilesLayout.Controls.Add(this.m_BaselineLabel, 0, 0);
            this.m_FilesLayout.Controls.Add(this.m_BaselineTextBox, 1, 0);
            this.m_FilesLayout.Controls.Add(this.m_BaselineBrowseButton, 2, 0);
            this.m_FilesLayout.Controls.Add(this.m_ResultLabel, 0, 1);
            this.m_FilesLayout.Controls.Add(this.m_ResultTextBox, 1, 1);
            this.m_FilesLayout.Controls.Add(this.m_ResultBrowseButton, 2, 1);
            this.m_FilesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_FilesLayout.Location = new System.Drawing.Point(10, 20);
            this.m_FilesLayout.Name = "m_FilesLayout";
            this.m_FilesLayout.RowCount = 2;
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.m_FilesLayout.Size = new System.Drawing.Size(723, 60);
            this.m_FilesLayout.TabIndex = 0;
            //
            // m_BaselineLabel
            //
            this.m_BaselineLabel.AutoSize = true;
            this.m_BaselineLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_BaselineLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_BaselineLabel.Location = new System.Drawing.Point(3, 0);
            this.m_BaselineLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_BaselineLabel.Name = "m_BaselineLabel";
            this.m_BaselineLabel.Size = new System.Drawing.Size(68, 30);
            this.m_BaselineLabel.TabIndex = 0;
            this.m_BaselineLabel.Text = "Baseline";
            this.m_BaselineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_BaselineTextBox
            //
            this.m_BaselineTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_BaselineTextBox.Location = new System.Drawing.Point(77, 3);
            this.m_BaselineTextBox.Name = "m_BaselineTextBox";
            this.m_BaselineTextBox.ReadOnly = true;
            this.m_BaselineTextBox.Size = new System.Drawing.Size(527, 23);
            this.m_BaselineTextBox.TabIndex = 0;
            //
            // m_BaselineBrowseButton
            //
            this.m_BaselineBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_BaselineBrowseButton.Location = new System.Drawing.Point(610, 3);
            this.m_BaselineBrowseButton.Name = "m_BaselineBrowseButton";
            this.m_BaselineBrowseButton.Size = new System.Drawing.Size(110, 24);
            this.m_BaselineBrowseButton.TabIndex = 1;
            this.m_BaselineBrowseButton.Text = "Browse...";
            this.m_BaselineBrowseButton.UseVisualStyleBackColor = true;
            this.m_BaselineBrowseButton.Click += new System.EventHandler(this.BaselineBrowseButton_Click);
            //
            // m_ResultLabel
            //
            this.m_ResultLabel.AutoSize = true;
            this.m_ResultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.m_ResultLabel.Location = new System.Drawing.Point(3, 30);
            this.m_ResultLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.m_ResultLabel.Name = "m_ResultLabel";
            this.m_ResultLabel.Size = new System.Drawing.Size(68, 30);
            this.m_ResultLabel.TabIndex = 0;
            this.m_ResultLabel.Text = "Result";
            this.m_ResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_ResultTextBox
            //
            this.m_ResultTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultTextBox.Location = new System.Drawing.Point(77, 33);
            this.m_ResultTextBox.Name = "m_ResultTextBox";
            this.m_ResultTextBox.ReadOnly = true;
            this.m_ResultTextBox.Size = new System.Drawing.Size(527, 23);
            this.m_ResultTextBox.TabIndex = 2;
            //
            // m_ResultBrowseButton
            //
            this.m_ResultBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultBrowseButton.Location = new System.Drawing.Point(610, 33);
            this.m_ResultBrowseButton.Name = "m_ResultBrowseButton";
            this.m_ResultBrowseButton.Size = new System.Drawing.Size(110, 24);
            this.m_ResultBrowseButton.TabIndex = 3;
            this.m_ResultBrowseButton.Text = "Browse...";
            this.m_ResultBrowseButton.UseVisualStyleBackColor = true;
            this.m_ResultBrowseButton.Click += new System.EventHandler(this.ResultBrowseButton_Click);
            //
            // m_ComparisonGroupBox
            //
            this.m_ComparisonGroupBox.Controls.Add(this.m_ComparisonList);
            this.m_ComparisonGroupBox.Controls.Add(this.m_VerdictLabel);
            this.m_ComparisonGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ComparisonGroupBox.Location = new System.Drawing.Point(3, 95);
            this.m_ComparisonGroupBox.Name = "m_ComparisonGroupBox";
            this.m_ComparisonGroupBox.Padding = new System.Windows.Forms.Padding(10, 4, 10, 8);
            this.m_ComparisonGroupBox.Size = new System.Drawing.Size(743, 162);
            this.m_ComparisonGroupBox.TabIndex = 1;
            this.m_ComparisonGroupBox.TabStop = false;
            this.m_ComparisonGroupBox.Text = "Comparison";
            //
            // m_ComparisonList
            //
            this.m_ComparisonList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_MeasureColumn,
            this.m_BaselineColumn,
            this.m_ResultColumn,
            this.m_DifferenceColumn});
            this.m_ComparisonList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ComparisonList.FullRowSelect = true;
            this.m_ComparisonList.GridLines = true;
            this.m_ComparisonList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_ComparisonList.HideSelection = true;
            this.m_ComparisonList.Location = new System.Drawing.Point(10, 20);
            this.m_ComparisonList.MultiSelect = false;
            this.m_ComparisonList.Name = "m_ComparisonList";
            this.m_ComparisonList.Size = new System.Drawing.Size(723, 134);
            this.m_ComparisonList.TabIndex = 0;
            this.m_ComparisonList.TabStop = false;
            this.m_ComparisonList.UseCompatibleStateImageBehavior = false;
            this.m_ComparisonList.View = System.Windows.Forms.View.Details;
            this.m_ComparisonList.SizeChanged += new System.EventHandler(this.ComparisonList_SizeChanged);
            //
            // m_VerdictLabel
            //
            this.m_VerdictLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.m_VerdictLabel.Location = new System.Drawing.Point(10, 154);
            this.m_VerdictLabel.Name = "m_VerdictLabel";
            this.m_VerdictLabel.Padding = new System.Windows.Forms.Padding(2, 8, 2, 0);
            this.m_VerdictLabel.Size = new System.Drawing.Size(723, 36);
            this.m_VerdictLabel.TabIndex = 1;
            this.m_VerdictLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_MeasureColumn
            //
            this.m_MeasureColumn.Text = "Measure";
            this.m_MeasureColumn.Width = 220;
            //
            // m_BaselineColumn
            //
            this.m_BaselineColumn.Text = "Baseline";
            this.m_BaselineColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_BaselineColumn.Width = 150;
            //
            // m_ResultColumn
            //
            this.m_ResultColumn.Text = "Result";
            this.m_ResultColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_ResultColumn.Width = 150;
            //
            // m_DifferenceColumn
            //
            this.m_DifferenceColumn.Text = "Difference";
            this.m_DifferenceColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_DifferenceColumn.Width = 150;
            //
            // m_ResultHistogramChart
            //
            this.m_ResultHistogramChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultHistogramChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultHistogramChart.Location = new System.Drawing.Point(3, 263);
            this.m_ResultHistogramChart.Name = "m_ResultHistogramChart";
            this.m_ResultHistogramChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            this.m_ResultHistogramChart.Size = new System.Drawing.Size(743, 397);
            this.m_ResultHistogramChart.TabIndex = 2;
            this.m_ResultHistogramChart.TabStop = false;
            //
            // m_ToolTip
            //
            this.m_ToolTip.AutoPopDelay = 12000;
            this.m_ToolTip.InitialDelay = 500;
            this.m_ToolTip.ReshowDelay = 100;
            this.m_ToolTip.SetToolTip(this.m_SessionTimerLabel, "How long this session has been recording, not counting time spent paused.");
            this.m_ToolTip.SetToolTip(this.m_SessionTimerTextBox, "How long this session has been recording, not counting time spent paused.");
            this.m_ToolTip.SetToolTip(this.m_DataPointsLabel, "How many readings have been taken. One is taken every tenth of a second.");
            this.m_ToolTip.SetToolTip(this.m_DataPointsTextBox, "How many readings have been taken. One is taken every tenth of a second.");
            this.m_ToolTip.SetToolTip(this.m_CurrentAverageLabel, "The mean of every reading so far. An unbiased generator settles near 0.5.");
            this.m_ToolTip.SetToolTip(this.m_CurrentAverageTextBox, "The mean of every reading so far. An unbiased generator settles near 0.5.");
            this.m_ToolTip.SetToolTip(this.m_MeanDeviationLabel, "How far the mean sits from 0.5, the value an unbiased generator is expected to give.");
            this.m_ToolTip.SetToolTip(this.m_MeanDeviationTextBox, "How far the mean sits from 0.5, the value an unbiased generator is expected to give.");
            this.m_ToolTip.SetToolTip(this.m_StandardDeviationLabel, "How spread out the readings are around their mean. A larger figure means noisier readings.");
            this.m_ToolTip.SetToolTip(this.m_StandardDeviationTextBox, "How spread out the readings are around their mean. A larger figure means noisier readings.");
            //
            // GeneratorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(773, 732);
            this.Controls.Add(this.MainTabControl);
            this.Controls.Add(this.m_StatusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(740, 600);
            this.Name = "GeneratorForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Random Number Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeneratorForm_FormClosing);
            this.m_StatusStrip.ResumeLayout(false);
            this.m_StatusStrip.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.ExecuteTabPage.ResumeLayout(false);
            this.m_ExecuteLayout.ResumeLayout(false);
            this.m_SetupGroupBox.ResumeLayout(false);
            this.m_SetupLayout.ResumeLayout(false);
            this.m_SetupLayout.PerformLayout();
            this.m_ActionPanel.ResumeLayout(false);
            this.m_StatisticsGroupBox.ResumeLayout(false);
            this.m_StatisticsLayout.ResumeLayout(false);
            this.m_StatisticsLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).EndInit();
            this.AnalyzeTabPage.ResumeLayout(false);
            this.m_AnalyzeLayout.ResumeLayout(false);
            this.m_FilesGroupBox.ResumeLayout(false);
            this.m_FilesLayout.ResumeLayout(false);
            this.m_FilesLayout.PerformLayout();
            this.m_ComparisonGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.StatusStrip m_StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel m_StatusLabel;
        private RNGChart m_ResultChart;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ExecuteTabPage;
        private System.Windows.Forms.TabPage AnalyzeTabPage;
        private System.Windows.Forms.TableLayoutPanel m_ExecuteLayout;
        private System.Windows.Forms.TableLayoutPanel m_AnalyzeLayout;
        private System.Windows.Forms.GroupBox m_StatisticsGroupBox;
        private System.Windows.Forms.TableLayoutPanel m_StatisticsLayout;
        private System.Windows.Forms.GroupBox m_SetupGroupBox;
        private System.Windows.Forms.TableLayoutPanel m_SetupLayout;
        private System.Windows.Forms.FlowLayoutPanel m_ActionPanel;
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
        private System.Windows.Forms.GroupBox m_FilesGroupBox;
        private System.Windows.Forms.TableLayoutPanel m_FilesLayout;
        private System.Windows.Forms.Button m_BaselineBrowseButton;
        private System.Windows.Forms.TextBox m_BaselineTextBox;
        private System.Windows.Forms.Label m_BaselineLabel;
        private System.Windows.Forms.Button m_ResultBrowseButton;
        private System.Windows.Forms.TextBox m_ResultTextBox;
        private System.Windows.Forms.Label m_ResultLabel;
        private System.Windows.Forms.GroupBox m_ComparisonGroupBox;
        private System.Windows.Forms.ListView m_ComparisonList;
        private System.Windows.Forms.Label m_VerdictLabel;
        private System.Windows.Forms.ColumnHeader m_MeasureColumn;
        private System.Windows.Forms.ColumnHeader m_BaselineColumn;
        private System.Windows.Forms.ColumnHeader m_ResultColumn;
        private System.Windows.Forms.ColumnHeader m_DifferenceColumn;
        private HistogramChart m_ResultHistogramChart;
        private System.Windows.Forms.ToolTip m_ToolTip;
    }
}
