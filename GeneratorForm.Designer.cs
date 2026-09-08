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
// 2026/09/07 - Mike Pullen - Cards on a light ground, readouts as separated tiles, and settings in one row,
//                            following the interface mockups
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
            this.m_StatusStrip = new System.Windows.Forms.StatusStrip();
            this.m_StatusIndicator = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ExecuteTabPage = new System.Windows.Forms.TabPage();
            this.m_ExecuteLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_SetupCard = new RandomNumberGenerator.CardPanel();
            this.m_SetupLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_SimulateToggleLabel = new System.Windows.Forms.Label();
            this.m_SimulateToggle = new CommonControls.ToggleButton();
            this.m_PortLabel = new System.Windows.Forms.Label();
            this.m_PortComboBox = new System.Windows.Forms.ComboBox();
            this.m_FileLlabel = new System.Windows.Forms.Label();
            this.m_FileTextBox = new System.Windows.Forms.TextBox();
            this.m_FileBrowseButton = new System.Windows.Forms.Button();
            this.m_TargetLabel = new System.Windows.Forms.Label();
            this.m_TargetComboBox = new System.Windows.Forms.ComboBox();
            this.m_ActionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.m_StopButton = new System.Windows.Forms.Button();
            this.m_PauseButton = new System.Windows.Forms.Button();
            this.m_StartButton = new System.Windows.Forms.Button();
            this.m_ClearButton = new System.Windows.Forms.Button();
            this.m_StatisticsCard = new RandomNumberGenerator.CardPanel();
            this.m_StatisticsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_SessionTimerTile = new System.Windows.Forms.Panel();
            this.m_SessionTimerTextBox = new System.Windows.Forms.TextBox();
            this.m_SessionTimerLabel = new System.Windows.Forms.Label();
            this.m_DataPointsTile = new System.Windows.Forms.Panel();
            this.m_DataPointsTextBox = new System.Windows.Forms.TextBox();
            this.m_DataPointsLabel = new System.Windows.Forms.Label();
            this.m_CurrentAverageTile = new System.Windows.Forms.Panel();
            this.m_CurrentAverageTextBox = new System.Windows.Forms.TextBox();
            this.m_CurrentAverageLabel = new System.Windows.Forms.Label();
            this.m_MeanDeviationTile = new System.Windows.Forms.Panel();
            this.m_MeanDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_MeanDeviationLabel = new System.Windows.Forms.Label();
            this.m_StandardDeviationTile = new System.Windows.Forms.Panel();
            this.m_StandardDeviationTextBox = new System.Windows.Forms.TextBox();
            this.m_StandardDeviationLabel = new System.Windows.Forms.Label();
            this.m_ResultChartCard = new RandomNumberGenerator.CardPanel();
            this.m_ResultChart = new RandomNumberGenerator.RNGChart();
            this.AnalyzeTabPage = new System.Windows.Forms.TabPage();
            this.m_AnalyzeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_SessionsCard = new RandomNumberGenerator.CardPanel();
            this.m_FilesLayout = new System.Windows.Forms.TableLayoutPanel();
            this.m_BaselineLabel = new System.Windows.Forms.Label();
            this.m_BaselineTextBox = new System.Windows.Forms.TextBox();
            this.m_BaselineBrowseButton = new System.Windows.Forms.Button();
            this.m_ResultLabel = new System.Windows.Forms.Label();
            this.m_ResultTextBox = new System.Windows.Forms.TextBox();
            this.m_ResultBrowseButton = new System.Windows.Forms.Button();
            this.m_ComparisonCard = new RandomNumberGenerator.CardPanel();
            this.m_ComparisonList = new System.Windows.Forms.ListView();
            this.m_MeasureColumn = new System.Windows.Forms.ColumnHeader();
            this.m_BaselineColumn = new System.Windows.Forms.ColumnHeader();
            this.m_ResultColumn = new System.Windows.Forms.ColumnHeader();
            this.m_DifferenceColumn = new System.Windows.Forms.ColumnHeader();
            this.m_VerdictLabel = new System.Windows.Forms.Label();
            this.m_HistogramCard = new RandomNumberGenerator.CardPanel();
            this.m_ResultHistogramChart = new RandomNumberGenerator.HistogramChart();
            this.m_StatusStrip.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ExecuteTabPage.SuspendLayout();
            this.m_ExecuteLayout.SuspendLayout();
            this.m_SetupCard.SuspendLayout();
            this.m_SetupLayout.SuspendLayout();
            this.m_ActionPanel.SuspendLayout();
            this.m_StatisticsCard.SuspendLayout();
            this.m_StatisticsLayout.SuspendLayout();
            this.m_SessionTimerTile.SuspendLayout();
            this.m_DataPointsTile.SuspendLayout();
            this.m_CurrentAverageTile.SuspendLayout();
            this.m_MeanDeviationTile.SuspendLayout();
            this.m_StandardDeviationTile.SuspendLayout();
            this.m_ResultChartCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).BeginInit();
            this.AnalyzeTabPage.SuspendLayout();
            this.m_AnalyzeLayout.SuspendLayout();
            this.m_SessionsCard.SuspendLayout();
            this.m_FilesLayout.SuspendLayout();
            this.m_ComparisonCard.SuspendLayout();
            this.m_HistogramCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).BeginInit();
            this.SuspendLayout();
            //
            // m_StatusStrip
            //
            this.m_StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_StatusIndicator,
            this.m_StatusLabel});
            this.m_StatusStrip.Location = new System.Drawing.Point(0, 710);
            this.m_StatusStrip.Name = "m_StatusStrip";
            this.m_StatusStrip.Padding = new System.Windows.Forms.Padding(10, 0, 12, 0);
            this.m_StatusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.m_StatusStrip.Size = new System.Drawing.Size(773, 24);
            this.m_StatusStrip.SizingGrip = false;
            this.m_StatusStrip.TabIndex = 1;
            //
            // m_StatusIndicator
            //
            this.m_StatusIndicator.Margin = new System.Windows.Forms.Padding(0, 3, 4, 2);
            this.m_StatusIndicator.Name = "m_StatusIndicator";
            this.m_StatusIndicator.Size = new System.Drawing.Size(12, 19);
            this.m_StatusIndicator.Text = "●";
            //
            // m_StatusLabel
            //
            this.m_StatusLabel.Name = "m_StatusLabel";
            this.m_StatusLabel.Size = new System.Drawing.Size(735, 19);
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
            this.MainTabControl.ItemSize = new System.Drawing.Size(112, 26);
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.Padding = new System.Drawing.Point(0, 0);
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(773, 710);
            this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.MainTabControl.TabIndex = 0;
            //
            // ExecuteTabPage
            //
            this.ExecuteTabPage.Controls.Add(this.m_ExecuteLayout);
            this.ExecuteTabPage.Location = new System.Drawing.Point(4, 30);
            this.ExecuteTabPage.Name = "ExecuteTabPage";
            this.ExecuteTabPage.Padding = new System.Windows.Forms.Padding(12);
            this.ExecuteTabPage.Size = new System.Drawing.Size(765, 676);
            this.ExecuteTabPage.TabIndex = 0;
            this.ExecuteTabPage.Text = "Record";
            //
            // m_ExecuteLayout
            //
            this.m_ExecuteLayout.ColumnCount = 1;
            this.m_ExecuteLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ExecuteLayout.Controls.Add(this.m_SetupCard, 0, 0);
            this.m_ExecuteLayout.Controls.Add(this.m_StatisticsCard, 0, 1);
            this.m_ExecuteLayout.Controls.Add(this.m_ResultChartCard, 0, 2);
            this.m_ExecuteLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ExecuteLayout.Location = new System.Drawing.Point(12, 12);
            this.m_ExecuteLayout.Name = "m_ExecuteLayout";
            this.m_ExecuteLayout.RowCount = 3;
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.m_ExecuteLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_ExecuteLayout.Size = new System.Drawing.Size(741, 652);
            this.m_ExecuteLayout.TabIndex = 0;
            //
            // m_SetupCard
            //
            this.m_SetupCard.Controls.Add(this.m_SetupLayout);
            this.m_SetupCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SetupCard.Location = new System.Drawing.Point(0, 0);
            this.m_SetupCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.m_SetupCard.Name = "m_SetupCard";
            this.m_SetupCard.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this.m_SetupCard.Size = new System.Drawing.Size(741, 122);
            this.m_SetupCard.TabIndex = 0;
            //
            // m_SetupLayout
            //
            this.m_SetupLayout.BackColor = System.Drawing.Color.Transparent;
            this.m_SetupLayout.ColumnCount = 5;
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.m_SetupLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.m_SetupLayout.Controls.Add(this.m_SimulateToggleLabel, 0, 0);
            this.m_SetupLayout.Controls.Add(this.m_SimulateToggle, 0, 1);
            this.m_SetupLayout.Controls.Add(this.m_PortLabel, 1, 0);
            this.m_SetupLayout.Controls.Add(this.m_PortComboBox, 1, 1);
            this.m_SetupLayout.Controls.Add(this.m_FileLlabel, 2, 0);
            this.m_SetupLayout.Controls.Add(this.m_FileTextBox, 2, 1);
            this.m_SetupLayout.Controls.Add(this.m_FileBrowseButton, 3, 1);
            this.m_SetupLayout.Controls.Add(this.m_TargetLabel, 4, 0);
            this.m_SetupLayout.Controls.Add(this.m_TargetComboBox, 4, 1);
            this.m_SetupLayout.Controls.Add(this.m_ActionPanel, 0, 2);
            this.m_SetupLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SetupLayout.Location = new System.Drawing.Point(14, 12);
            this.m_SetupLayout.Name = "m_SetupLayout";
            this.m_SetupLayout.RowCount = 3;
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.m_SetupLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_SetupLayout.Size = new System.Drawing.Size(711, 96);
            this.m_SetupLayout.TabIndex = 0;
            //
            // m_SimulateToggleLabel
            //
            this.m_SimulateToggleLabel.AutoSize = true;
            this.m_SimulateToggleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SimulateToggleLabel.Location = new System.Drawing.Point(0, 0);
            this.m_SimulateToggleLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_SimulateToggleLabel.Name = "m_SimulateToggleLabel";
            this.m_SimulateToggleLabel.Size = new System.Drawing.Size(128, 18);
            this.m_SimulateToggleLabel.TabIndex = 0;
            this.m_SimulateToggleLabel.Text = "SIMULATE";
            this.m_SimulateToggleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_SimulateToggle
            //
            this.m_SimulateToggle.CausesValidation = false;
            this.m_SimulateToggle.DisabledBackground = System.Drawing.Color.Gray;
            this.m_SimulateToggle.DisabledToggle = System.Drawing.Color.LightGray;
            this.m_SimulateToggle.Location = new System.Drawing.Point(0, 18);
            this.m_SimulateToggle.Margin = new System.Windows.Forms.Padding(0);
            this.m_SimulateToggle.MinimumSize = new System.Drawing.Size(50, 25);
            this.m_SimulateToggle.Name = "m_SimulateToggle";
            this.m_SimulateToggle.OffBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OffToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.OnBackground = System.Drawing.Color.Black;
            this.m_SimulateToggle.OnToggle = System.Drawing.Color.White;
            this.m_SimulateToggle.Size = new System.Drawing.Size(71, 25);
            this.m_SimulateToggle.TabIndex = 2;
            this.m_SimulateToggle.UseVisualStyleBackColor = true;
            this.m_SimulateToggle.CheckedChanged += new System.EventHandler(this.SimulateToggle_CheckedChanged);
            //
            // m_PortLabel
            //
            this.m_PortLabel.AutoSize = true;
            this.m_PortLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_PortLabel.Location = new System.Drawing.Point(128, 0);
            this.m_PortLabel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.m_PortLabel.Name = "m_PortLabel";
            this.m_PortLabel.Size = new System.Drawing.Size(84, 18);
            this.m_PortLabel.TabIndex = 0;
            this.m_PortLabel.Text = "PORT";
            this.m_PortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_PortComboBox
            //
            this.m_PortComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_PortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PortComboBox.Location = new System.Drawing.Point(128, 18);
            this.m_PortComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.m_PortComboBox.MaxLength = 2;
            this.m_PortComboBox.Name = "m_PortComboBox";
            this.m_PortComboBox.Size = new System.Drawing.Size(84, 23);
            this.m_PortComboBox.TabIndex = 3;
            this.m_PortComboBox.SelectedIndexChanged += new System.EventHandler(this.PortComboBox_SelectedIndexChanged);
            this.m_PortComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.PortTextBox_Validating);
            //
            // m_FileLlabel
            //
            this.m_FileLlabel.AutoSize = true;
            this.m_FileLlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_FileLlabel.Location = new System.Drawing.Point(224, 0);
            this.m_FileLlabel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_FileLlabel.Name = "m_FileLlabel";
            this.m_FileLlabel.Size = new System.Drawing.Size(239, 18);
            this.m_FileLlabel.TabIndex = 0;
            this.m_FileLlabel.Text = "DATA FILE";
            this.m_FileLlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_FileTextBox
            //
            this.m_FileTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_FileTextBox.Location = new System.Drawing.Point(224, 18);
            this.m_FileTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_FileTextBox.Name = "m_FileTextBox";
            this.m_FileTextBox.ReadOnly = true;
            this.m_FileTextBox.Size = new System.Drawing.Size(239, 23);
            this.m_FileTextBox.TabIndex = 0;
            //
            // m_FileBrowseButton
            //
            this.m_FileBrowseButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_FileBrowseButton.Location = new System.Drawing.Point(471, 18);
            this.m_FileBrowseButton.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.m_FileBrowseButton.Name = "m_FileBrowseButton";
            this.m_FileBrowseButton.Size = new System.Drawing.Size(100, 25);
            this.m_FileBrowseButton.TabIndex = 1;
            this.m_FileBrowseButton.Text = "&Browse...";
            this.m_FileBrowseButton.UseVisualStyleBackColor = true;
            this.m_FileBrowseButton.Click += new System.EventHandler(this.FileBrowseButton_Click);
            //
            // m_TargetLabel
            //
            this.m_TargetLabel.AutoSize = true;
            this.m_TargetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_TargetLabel.Location = new System.Drawing.Point(587, 0);
            this.m_TargetLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_TargetLabel.Name = "m_TargetLabel";
            this.m_TargetLabel.Size = new System.Drawing.Size(124, 18);
            this.m_TargetLabel.TabIndex = 0;
            this.m_TargetLabel.Text = "TARGET VALUE";
            this.m_TargetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_TargetComboBox
            //
            this.m_TargetComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_TargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_TargetComboBox.Location = new System.Drawing.Point(587, 18);
            this.m_TargetComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.m_TargetComboBox.Name = "m_TargetComboBox";
            this.m_TargetComboBox.Size = new System.Drawing.Size(124, 23);
            this.m_TargetComboBox.TabIndex = 4;
            //
            // m_ActionPanel
            //
            this.m_SetupLayout.SetColumnSpan(this.m_ActionPanel, 5);
            this.m_ActionPanel.BackColor = System.Drawing.Color.Transparent;
            this.m_ActionPanel.Controls.Add(this.m_StopButton);
            this.m_ActionPanel.Controls.Add(this.m_PauseButton);
            this.m_ActionPanel.Controls.Add(this.m_StartButton);
            this.m_ActionPanel.Controls.Add(this.m_ClearButton);
            this.m_ActionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ActionPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.m_ActionPanel.Location = new System.Drawing.Point(0, 46);
            this.m_ActionPanel.Margin = new System.Windows.Forms.Padding(0);
            this.m_ActionPanel.Name = "m_ActionPanel";
            this.m_ActionPanel.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.m_ActionPanel.Size = new System.Drawing.Size(711, 50);
            this.m_ActionPanel.TabIndex = 5;
            this.m_ActionPanel.WrapContents = false;
            //
            // m_StopButton
            //
            this.m_StopButton.CausesValidation = false;
            this.m_StopButton.Enabled = false;
            this.m_StopButton.Location = new System.Drawing.Point(603, 14);
            this.m_StopButton.Margin = new System.Windows.Forms.Padding(0);
            this.m_StopButton.Name = "m_StopButton";
            this.m_StopButton.Size = new System.Drawing.Size(108, 32);
            this.m_StopButton.TabIndex = 2;
            this.m_StopButton.Text = "S&top";
            this.m_StopButton.UseVisualStyleBackColor = true;
            this.m_StopButton.Click += new System.EventHandler(this.StopButton_Click);
            //
            // m_PauseButton
            //
            this.m_PauseButton.CausesValidation = false;
            this.m_PauseButton.Enabled = false;
            this.m_PauseButton.Location = new System.Drawing.Point(487, 14);
            this.m_PauseButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_PauseButton.Name = "m_PauseButton";
            this.m_PauseButton.Size = new System.Drawing.Size(108, 32);
            this.m_PauseButton.TabIndex = 1;
            this.m_PauseButton.Text = "&Pause";
            this.m_PauseButton.UseVisualStyleBackColor = true;
            this.m_PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            //
            // m_StartButton
            //
            this.m_StartButton.CausesValidation = false;
            this.m_StartButton.Enabled = false;
            this.m_StartButton.Location = new System.Drawing.Point(371, 14);
            this.m_StartButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_StartButton.Name = "m_StartButton";
            this.m_StartButton.Size = new System.Drawing.Size(108, 32);
            this.m_StartButton.TabIndex = 0;
            this.m_StartButton.Text = "&Start";
            this.m_StartButton.UseVisualStyleBackColor = true;
            this.m_StartButton.Click += new System.EventHandler(this.StartButton_Click);
            //
            // m_ClearButton
            //
            this.m_ClearButton.CausesValidation = false;
            this.m_ClearButton.FlatAppearance.BorderSize = 0;
            this.m_ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_ClearButton.Location = new System.Drawing.Point(263, 14);
            this.m_ClearButton.Margin = new System.Windows.Forms.Padding(0, 0, 24, 0);
            this.m_ClearButton.Name = "m_ClearButton";
            this.m_ClearButton.Size = new System.Drawing.Size(100, 32);
            this.m_ClearButton.TabIndex = 6;
            this.m_ClearButton.Text = "&Clear";
            this.m_ClearButton.UseVisualStyleBackColor = false;
            this.m_ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            //
            // m_StatisticsCard
            //
            this.m_StatisticsCard.Controls.Add(this.m_StatisticsLayout);
            this.m_StatisticsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StatisticsCard.Location = new System.Drawing.Point(0, 132);
            this.m_StatisticsCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.m_StatisticsCard.Name = "m_StatisticsCard";
            this.m_StatisticsCard.Padding = new System.Windows.Forms.Padding(1);
            this.m_StatisticsCard.Size = new System.Drawing.Size(741, 74);
            this.m_StatisticsCard.TabIndex = 1;
            //
            // m_StatisticsLayout
            //
            this.m_StatisticsLayout.ColumnCount = 5;
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.m_StatisticsLayout.Controls.Add(this.m_SessionTimerTile, 0, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_DataPointsTile, 1, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_CurrentAverageTile, 2, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_MeanDeviationTile, 3, 0);
            this.m_StatisticsLayout.Controls.Add(this.m_StandardDeviationTile, 4, 0);
            this.m_StatisticsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StatisticsLayout.Location = new System.Drawing.Point(1, 1);
            this.m_StatisticsLayout.Name = "m_StatisticsLayout";
            this.m_StatisticsLayout.RowCount = 1;
            this.m_StatisticsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_StatisticsLayout.Size = new System.Drawing.Size(739, 72);
            this.m_StatisticsLayout.TabIndex = 0;
            //
            // m_SessionTimerTile
            //
            this.m_SessionTimerTile.Controls.Add(this.m_SessionTimerTextBox);
            this.m_SessionTimerTile.Controls.Add(this.m_SessionTimerLabel);
            this.m_SessionTimerTile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SessionTimerTile.Location = new System.Drawing.Point(0, 0);
            this.m_SessionTimerTile.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.m_SessionTimerTile.Name = "m_SessionTimerTile";
            this.m_SessionTimerTile.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.m_SessionTimerTile.Size = new System.Drawing.Size(146, 72);
            this.m_SessionTimerTile.TabIndex = 0;
            //
            // m_SessionTimerTextBox
            //
            this.m_SessionTimerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_SessionTimerTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_SessionTimerTextBox.Location = new System.Drawing.Point(14, 30);
            this.m_SessionTimerTextBox.Name = "m_SessionTimerTextBox";
            this.m_SessionTimerTextBox.ReadOnly = true;
            this.m_SessionTimerTextBox.Size = new System.Drawing.Size(118, 22);
            this.m_SessionTimerTextBox.TabIndex = 0;
            this.m_SessionTimerTextBox.TabStop = false;
            this.m_SessionTimerTextBox.Text = "00:00:00";
            this.m_SessionTimerTextBox.WordWrap = false;
            //
            // m_SessionTimerLabel
            //
            this.m_SessionTimerLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_SessionTimerLabel.Location = new System.Drawing.Point(14, 12);
            this.m_SessionTimerLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_SessionTimerLabel.Name = "m_SessionTimerLabel";
            this.m_SessionTimerLabel.Size = new System.Drawing.Size(118, 18);
            this.m_SessionTimerLabel.TabIndex = 0;
            this.m_SessionTimerLabel.Text = "ELAPSED";
            //
            // m_DataPointsTile
            //
            this.m_DataPointsTile.Controls.Add(this.m_DataPointsTextBox);
            this.m_DataPointsTile.Controls.Add(this.m_DataPointsLabel);
            this.m_DataPointsTile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_DataPointsTile.Location = new System.Drawing.Point(147, 0);
            this.m_DataPointsTile.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.m_DataPointsTile.Name = "m_DataPointsTile";
            this.m_DataPointsTile.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.m_DataPointsTile.Size = new System.Drawing.Size(146, 72);
            this.m_DataPointsTile.TabIndex = 1;
            //
            // m_DataPointsTextBox
            //
            this.m_DataPointsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_DataPointsTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_DataPointsTextBox.Location = new System.Drawing.Point(14, 30);
            this.m_DataPointsTextBox.Name = "m_DataPointsTextBox";
            this.m_DataPointsTextBox.ReadOnly = true;
            this.m_DataPointsTextBox.Size = new System.Drawing.Size(118, 22);
            this.m_DataPointsTextBox.TabIndex = 0;
            this.m_DataPointsTextBox.TabStop = false;
            this.m_DataPointsTextBox.Text = "0";
            this.m_DataPointsTextBox.WordWrap = false;
            //
            // m_DataPointsLabel
            //
            this.m_DataPointsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_DataPointsLabel.Location = new System.Drawing.Point(14, 12);
            this.m_DataPointsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_DataPointsLabel.Name = "m_DataPointsLabel";
            this.m_DataPointsLabel.Size = new System.Drawing.Size(118, 18);
            this.m_DataPointsLabel.TabIndex = 0;
            this.m_DataPointsLabel.Text = "READINGS";
            //
            // m_CurrentAverageTile
            //
            this.m_CurrentAverageTile.Controls.Add(this.m_CurrentAverageTextBox);
            this.m_CurrentAverageTile.Controls.Add(this.m_CurrentAverageLabel);
            this.m_CurrentAverageTile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_CurrentAverageTile.Location = new System.Drawing.Point(294, 0);
            this.m_CurrentAverageTile.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.m_CurrentAverageTile.Name = "m_CurrentAverageTile";
            this.m_CurrentAverageTile.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.m_CurrentAverageTile.Size = new System.Drawing.Size(146, 72);
            this.m_CurrentAverageTile.TabIndex = 2;
            //
            // m_CurrentAverageTextBox
            //
            this.m_CurrentAverageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_CurrentAverageTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_CurrentAverageTextBox.Location = new System.Drawing.Point(14, 30);
            this.m_CurrentAverageTextBox.Name = "m_CurrentAverageTextBox";
            this.m_CurrentAverageTextBox.ReadOnly = true;
            this.m_CurrentAverageTextBox.Size = new System.Drawing.Size(118, 22);
            this.m_CurrentAverageTextBox.TabIndex = 0;
            this.m_CurrentAverageTextBox.TabStop = false;
            this.m_CurrentAverageTextBox.Text = "—";
            this.m_CurrentAverageTextBox.WordWrap = false;
            //
            // m_CurrentAverageLabel
            //
            this.m_CurrentAverageLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_CurrentAverageLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_CurrentAverageLabel.Location = new System.Drawing.Point(14, 12);
            this.m_CurrentAverageLabel.Name = "m_CurrentAverageLabel";
            this.m_CurrentAverageLabel.Size = new System.Drawing.Size(118, 18);
            this.m_CurrentAverageLabel.TabIndex = 0;
            this.m_CurrentAverageLabel.Text = "MEAN";
            //
            // m_MeanDeviationTile
            //
            this.m_MeanDeviationTile.Controls.Add(this.m_MeanDeviationTextBox);
            this.m_MeanDeviationTile.Controls.Add(this.m_MeanDeviationLabel);
            this.m_MeanDeviationTile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_MeanDeviationTile.Location = new System.Drawing.Point(441, 0);
            this.m_MeanDeviationTile.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.m_MeanDeviationTile.Name = "m_MeanDeviationTile";
            this.m_MeanDeviationTile.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.m_MeanDeviationTile.Size = new System.Drawing.Size(146, 72);
            this.m_MeanDeviationTile.TabIndex = 3;
            //
            // m_MeanDeviationTextBox
            //
            this.m_MeanDeviationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_MeanDeviationTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_MeanDeviationTextBox.Location = new System.Drawing.Point(14, 30);
            this.m_MeanDeviationTextBox.Name = "m_MeanDeviationTextBox";
            this.m_MeanDeviationTextBox.ReadOnly = true;
            this.m_MeanDeviationTextBox.Size = new System.Drawing.Size(118, 22);
            this.m_MeanDeviationTextBox.TabIndex = 0;
            this.m_MeanDeviationTextBox.TabStop = false;
            this.m_MeanDeviationTextBox.Text = "—";
            this.m_MeanDeviationTextBox.WordWrap = false;
            //
            // m_MeanDeviationLabel
            //
            this.m_MeanDeviationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_MeanDeviationLabel.Location = new System.Drawing.Point(14, 12);
            this.m_MeanDeviationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_MeanDeviationLabel.Name = "m_MeanDeviationLabel";
            this.m_MeanDeviationLabel.Size = new System.Drawing.Size(118, 18);
            this.m_MeanDeviationLabel.TabIndex = 0;
            this.m_MeanDeviationLabel.Text = "DEVIATION FROM 0.5";
            //
            // m_StandardDeviationTile
            //
            this.m_StandardDeviationTile.Controls.Add(this.m_StandardDeviationTextBox);
            this.m_StandardDeviationTile.Controls.Add(this.m_StandardDeviationLabel);
            this.m_StandardDeviationTile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_StandardDeviationTile.Location = new System.Drawing.Point(588, 0);
            this.m_StandardDeviationTile.Margin = new System.Windows.Forms.Padding(0);
            this.m_StandardDeviationTile.Name = "m_StandardDeviationTile";
            this.m_StandardDeviationTile.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this.m_StandardDeviationTile.Size = new System.Drawing.Size(151, 72);
            this.m_StandardDeviationTile.TabIndex = 4;
            //
            // m_StandardDeviationTextBox
            //
            this.m_StandardDeviationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_StandardDeviationTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_StandardDeviationTextBox.Location = new System.Drawing.Point(14, 30);
            this.m_StandardDeviationTextBox.Name = "m_StandardDeviationTextBox";
            this.m_StandardDeviationTextBox.ReadOnly = true;
            this.m_StandardDeviationTextBox.Size = new System.Drawing.Size(123, 22);
            this.m_StandardDeviationTextBox.TabIndex = 0;
            this.m_StandardDeviationTextBox.TabStop = false;
            this.m_StandardDeviationTextBox.Text = "—";
            this.m_StandardDeviationTextBox.WordWrap = false;
            //
            // m_StandardDeviationLabel
            //
            this.m_StandardDeviationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_StandardDeviationLabel.Location = new System.Drawing.Point(14, 12);
            this.m_StandardDeviationLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_StandardDeviationLabel.Name = "m_StandardDeviationLabel";
            this.m_StandardDeviationLabel.Size = new System.Drawing.Size(123, 18);
            this.m_StandardDeviationLabel.TabIndex = 0;
            this.m_StandardDeviationLabel.Text = "STD DEVIATION";
            //
            // m_ResultChartCard
            //
            this.m_ResultChartCard.Controls.Add(this.m_ResultChart);
            this.m_ResultChartCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultChartCard.Location = new System.Drawing.Point(0, 216);
            this.m_ResultChartCard.Margin = new System.Windows.Forms.Padding(0);
            this.m_ResultChartCard.Name = "m_ResultChartCard";
            this.m_ResultChartCard.Padding = new System.Windows.Forms.Padding(10);
            this.m_ResultChartCard.Size = new System.Drawing.Size(741, 436);
            this.m_ResultChartCard.TabIndex = 2;
            //
            // m_ResultChart
            //
            this.m_ResultChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultChart.Location = new System.Drawing.Point(10, 10);
            this.m_ResultChart.Name = "m_ResultChart";
            this.m_ResultChart.Size = new System.Drawing.Size(721, 416);
            this.m_ResultChart.TabIndex = 0;
            this.m_ResultChart.TabStop = false;
            //
            // AnalyzeTabPage
            //
            this.AnalyzeTabPage.Controls.Add(this.m_AnalyzeLayout);
            this.AnalyzeTabPage.Location = new System.Drawing.Point(4, 30);
            this.AnalyzeTabPage.Name = "AnalyzeTabPage";
            this.AnalyzeTabPage.Padding = new System.Windows.Forms.Padding(12);
            this.AnalyzeTabPage.Size = new System.Drawing.Size(765, 676);
            this.AnalyzeTabPage.TabIndex = 1;
            this.AnalyzeTabPage.Text = "Analyse";
            //
            // m_AnalyzeLayout
            //
            this.m_AnalyzeLayout.ColumnCount = 1;
            this.m_AnalyzeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_AnalyzeLayout.Controls.Add(this.m_SessionsCard, 0, 0);
            this.m_AnalyzeLayout.Controls.Add(this.m_ComparisonCard, 0, 1);
            this.m_AnalyzeLayout.Controls.Add(this.m_HistogramCard, 0, 2);
            this.m_AnalyzeLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_AnalyzeLayout.Location = new System.Drawing.Point(12, 12);
            this.m_AnalyzeLayout.Name = "m_AnalyzeLayout";
            this.m_AnalyzeLayout.RowCount = 3;
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 234F));
            this.m_AnalyzeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_AnalyzeLayout.Size = new System.Drawing.Size(741, 652);
            this.m_AnalyzeLayout.TabIndex = 0;
            //
            // m_SessionsCard
            //
            this.m_SessionsCard.Controls.Add(this.m_FilesLayout);
            this.m_SessionsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SessionsCard.Location = new System.Drawing.Point(0, 0);
            this.m_SessionsCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.m_SessionsCard.Name = "m_SessionsCard";
            this.m_SessionsCard.Padding = new System.Windows.Forms.Padding(14, 10, 14, 10);
            this.m_SessionsCard.Size = new System.Drawing.Size(741, 96);
            this.m_SessionsCard.TabIndex = 0;
            //
            // m_FilesLayout
            //
            this.m_FilesLayout.BackColor = System.Drawing.Color.Transparent;
            this.m_FilesLayout.ColumnCount = 2;
            this.m_FilesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.m_FilesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.m_FilesLayout.Controls.Add(this.m_BaselineLabel, 0, 0);
            this.m_FilesLayout.Controls.Add(this.m_BaselineTextBox, 0, 1);
            this.m_FilesLayout.Controls.Add(this.m_BaselineBrowseButton, 1, 1);
            this.m_FilesLayout.Controls.Add(this.m_ResultLabel, 0, 2);
            this.m_FilesLayout.Controls.Add(this.m_ResultTextBox, 0, 3);
            this.m_FilesLayout.Controls.Add(this.m_ResultBrowseButton, 1, 3);
            this.m_FilesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_FilesLayout.Location = new System.Drawing.Point(14, 10);
            this.m_FilesLayout.Name = "m_FilesLayout";
            this.m_FilesLayout.RowCount = 4;
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.m_FilesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.m_FilesLayout.Size = new System.Drawing.Size(711, 76);
            this.m_FilesLayout.TabIndex = 0;
            //
            // m_BaselineLabel
            //
            this.m_BaselineLabel.AutoSize = true;
            this.m_BaselineLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_BaselineLabel.Location = new System.Drawing.Point(0, 0);
            this.m_BaselineLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_BaselineLabel.Name = "m_BaselineLabel";
            this.m_BaselineLabel.Size = new System.Drawing.Size(595, 16);
            this.m_BaselineLabel.TabIndex = 0;
            this.m_BaselineLabel.Text = "BASELINE";
            this.m_BaselineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_BaselineTextBox
            //
            this.m_BaselineTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_BaselineTextBox.Location = new System.Drawing.Point(0, 16);
            this.m_BaselineTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_BaselineTextBox.Name = "m_BaselineTextBox";
            this.m_BaselineTextBox.ReadOnly = true;
            this.m_BaselineTextBox.Size = new System.Drawing.Size(587, 23);
            this.m_BaselineTextBox.TabIndex = 0;
            //
            // m_BaselineBrowseButton
            //
            this.m_BaselineBrowseButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_BaselineBrowseButton.Location = new System.Drawing.Point(595, 16);
            this.m_BaselineBrowseButton.Margin = new System.Windows.Forms.Padding(0);
            this.m_BaselineBrowseButton.Name = "m_BaselineBrowseButton";
            this.m_BaselineBrowseButton.Size = new System.Drawing.Size(116, 25);
            this.m_BaselineBrowseButton.TabIndex = 1;
            this.m_BaselineBrowseButton.Text = "Browse...";
            this.m_BaselineBrowseButton.UseVisualStyleBackColor = true;
            this.m_BaselineBrowseButton.Click += new System.EventHandler(this.BaselineBrowseButton_Click);
            //
            // m_ResultLabel
            //
            this.m_ResultLabel.AutoSize = true;
            this.m_ResultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultLabel.Location = new System.Drawing.Point(0, 42);
            this.m_ResultLabel.Margin = new System.Windows.Forms.Padding(0);
            this.m_ResultLabel.Name = "m_ResultLabel";
            this.m_ResultLabel.Size = new System.Drawing.Size(595, 16);
            this.m_ResultLabel.TabIndex = 0;
            this.m_ResultLabel.Text = "RESULT";
            this.m_ResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_ResultTextBox
            //
            this.m_ResultTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_ResultTextBox.Location = new System.Drawing.Point(0, 58);
            this.m_ResultTextBox.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.m_ResultTextBox.Name = "m_ResultTextBox";
            this.m_ResultTextBox.ReadOnly = true;
            this.m_ResultTextBox.Size = new System.Drawing.Size(587, 23);
            this.m_ResultTextBox.TabIndex = 2;
            //
            // m_ResultBrowseButton
            //
            this.m_ResultBrowseButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_ResultBrowseButton.Location = new System.Drawing.Point(595, 58);
            this.m_ResultBrowseButton.Margin = new System.Windows.Forms.Padding(0);
            this.m_ResultBrowseButton.Name = "m_ResultBrowseButton";
            this.m_ResultBrowseButton.Size = new System.Drawing.Size(116, 25);
            this.m_ResultBrowseButton.TabIndex = 3;
            this.m_ResultBrowseButton.Text = "Browse...";
            this.m_ResultBrowseButton.UseVisualStyleBackColor = true;
            this.m_ResultBrowseButton.Click += new System.EventHandler(this.ResultBrowseButton_Click);
            //
            // m_ComparisonCard
            //
            this.m_ComparisonCard.Controls.Add(this.m_ComparisonList);
            this.m_ComparisonCard.Controls.Add(this.m_VerdictLabel);
            this.m_ComparisonCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ComparisonCard.Location = new System.Drawing.Point(0, 106);
            this.m_ComparisonCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.m_ComparisonCard.Name = "m_ComparisonCard";
            this.m_ComparisonCard.Padding = new System.Windows.Forms.Padding(14, 12, 14, 10);
            this.m_ComparisonCard.Size = new System.Drawing.Size(741, 240);
            this.m_ComparisonCard.TabIndex = 1;
            //
            // m_ComparisonList
            //
            this.m_ComparisonList.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
            this.m_ComparisonList.Location = new System.Drawing.Point(14, 12);
            this.m_ComparisonList.MultiSelect = false;
            this.m_ComparisonList.Name = "m_ComparisonList";
            this.m_ComparisonList.Size = new System.Drawing.Size(713, 176);
            this.m_ComparisonList.TabIndex = 0;
            this.m_ComparisonList.TabStop = false;
            this.m_ComparisonList.UseCompatibleStateImageBehavior = false;
            this.m_ComparisonList.View = System.Windows.Forms.View.Details;
            this.m_ComparisonList.SizeChanged += new System.EventHandler(this.ComparisonList_SizeChanged);
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
            // m_VerdictLabel
            //
            this.m_VerdictLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.m_VerdictLabel.Location = new System.Drawing.Point(14, 188);
            this.m_VerdictLabel.Name = "m_VerdictLabel";
            this.m_VerdictLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.m_VerdictLabel.Size = new System.Drawing.Size(713, 42);
            this.m_VerdictLabel.TabIndex = 1;
            this.m_VerdictLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // m_HistogramCard
            //
            this.m_HistogramCard.Controls.Add(this.m_ResultHistogramChart);
            this.m_HistogramCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_HistogramCard.Location = new System.Drawing.Point(0, 356);
            this.m_HistogramCard.Margin = new System.Windows.Forms.Padding(0);
            this.m_HistogramCard.Name = "m_HistogramCard";
            this.m_HistogramCard.Padding = new System.Windows.Forms.Padding(10);
            this.m_HistogramCard.Size = new System.Drawing.Size(741, 296);
            this.m_HistogramCard.TabIndex = 2;
            //
            // m_ResultHistogramChart
            //
            this.m_ResultHistogramChart.BackColor = System.Drawing.Color.Transparent;
            this.m_ResultHistogramChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultHistogramChart.Location = new System.Drawing.Point(10, 10);
            this.m_ResultHistogramChart.Name = "m_ResultHistogramChart";
            this.m_ResultHistogramChart.Size = new System.Drawing.Size(721, 276);
            this.m_ResultHistogramChart.TabIndex = 0;
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
            this.ClientSize = new System.Drawing.Size(773, 734);
            this.Controls.Add(this.MainTabControl);
            this.Controls.Add(this.m_StatusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(780, 620);
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
            this.m_SetupCard.ResumeLayout(false);
            this.m_SetupLayout.ResumeLayout(false);
            this.m_SetupLayout.PerformLayout();
            this.m_ActionPanel.ResumeLayout(false);
            this.m_StatisticsCard.ResumeLayout(false);
            this.m_StatisticsLayout.ResumeLayout(false);
            this.m_SessionTimerTile.ResumeLayout(false);
            this.m_SessionTimerTile.PerformLayout();
            this.m_DataPointsTile.ResumeLayout(false);
            this.m_DataPointsTile.PerformLayout();
            this.m_CurrentAverageTile.ResumeLayout(false);
            this.m_CurrentAverageTile.PerformLayout();
            this.m_MeanDeviationTile.ResumeLayout(false);
            this.m_MeanDeviationTile.PerformLayout();
            this.m_StandardDeviationTile.ResumeLayout(false);
            this.m_StandardDeviationTile.PerformLayout();
            this.m_ResultChartCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultChart)).EndInit();
            this.AnalyzeTabPage.ResumeLayout(false);
            this.m_AnalyzeLayout.ResumeLayout(false);
            this.m_SessionsCard.ResumeLayout(false);
            this.m_FilesLayout.ResumeLayout(false);
            this.m_FilesLayout.PerformLayout();
            this.m_ComparisonCard.ResumeLayout(false);
            this.m_HistogramCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_ResultHistogramChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip m_ToolTip;
        private System.Windows.Forms.StatusStrip m_StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel m_StatusIndicator;
        private System.Windows.Forms.ToolStripStatusLabel m_StatusLabel;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ExecuteTabPage;
        private System.Windows.Forms.TabPage AnalyzeTabPage;
        private System.Windows.Forms.TableLayoutPanel m_ExecuteLayout;
        private System.Windows.Forms.TableLayoutPanel m_AnalyzeLayout;
        private CardPanel m_SetupCard;
        private System.Windows.Forms.TableLayoutPanel m_SetupLayout;
        private System.Windows.Forms.FlowLayoutPanel m_ActionPanel;
        private System.Windows.Forms.Button m_ClearButton;
        private System.Windows.Forms.Button m_StopButton;
        private System.Windows.Forms.Button m_StartButton;
        private System.Windows.Forms.Button m_PauseButton;
        private System.Windows.Forms.Label m_PortLabel;
        private System.Windows.Forms.ComboBox m_PortComboBox;
        private System.Windows.Forms.Label m_SimulateToggleLabel;
        private CommonControls.ToggleButton m_SimulateToggle;
        private System.Windows.Forms.Label m_FileLlabel;
        private System.Windows.Forms.TextBox m_FileTextBox;
        private System.Windows.Forms.Button m_FileBrowseButton;
        private System.Windows.Forms.Label m_TargetLabel;
        private System.Windows.Forms.ComboBox m_TargetComboBox;
        private CardPanel m_StatisticsCard;
        private System.Windows.Forms.TableLayoutPanel m_StatisticsLayout;
        private System.Windows.Forms.Panel m_SessionTimerTile;
        private System.Windows.Forms.Label m_SessionTimerLabel;
        private System.Windows.Forms.TextBox m_SessionTimerTextBox;
        private System.Windows.Forms.Panel m_DataPointsTile;
        private System.Windows.Forms.Label m_DataPointsLabel;
        private System.Windows.Forms.TextBox m_DataPointsTextBox;
        private System.Windows.Forms.Panel m_CurrentAverageTile;
        private System.Windows.Forms.Label m_CurrentAverageLabel;
        private System.Windows.Forms.TextBox m_CurrentAverageTextBox;
        private System.Windows.Forms.Panel m_MeanDeviationTile;
        private System.Windows.Forms.Label m_MeanDeviationLabel;
        private System.Windows.Forms.TextBox m_MeanDeviationTextBox;
        private System.Windows.Forms.Panel m_StandardDeviationTile;
        private System.Windows.Forms.Label m_StandardDeviationLabel;
        private System.Windows.Forms.TextBox m_StandardDeviationTextBox;
        private CardPanel m_ResultChartCard;
        private RNGChart m_ResultChart;
        private CardPanel m_SessionsCard;
        private System.Windows.Forms.TableLayoutPanel m_FilesLayout;
        private System.Windows.Forms.Label m_BaselineLabel;
        private System.Windows.Forms.TextBox m_BaselineTextBox;
        private System.Windows.Forms.Button m_BaselineBrowseButton;
        private System.Windows.Forms.Label m_ResultLabel;
        private System.Windows.Forms.TextBox m_ResultTextBox;
        private System.Windows.Forms.Button m_ResultBrowseButton;
        private CardPanel m_ComparisonCard;
        private System.Windows.Forms.ListView m_ComparisonList;
        private System.Windows.Forms.ColumnHeader m_MeasureColumn;
        private System.Windows.Forms.ColumnHeader m_BaselineColumn;
        private System.Windows.Forms.ColumnHeader m_ResultColumn;
        private System.Windows.Forms.ColumnHeader m_DifferenceColumn;
        private System.Windows.Forms.Label m_VerdictLabel;
        private CardPanel m_HistogramCard;
        private HistogramChart m_ResultHistogramChart;
    }
}
