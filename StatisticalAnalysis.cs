//*********************************************************************************************************************
// File Name:      StatisticalAnalysis.cs
// Description:    Provides the statistical analysis for the results
//
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2025/08/13 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Provides statistical analysis capabilities for random number generator results
    /// </summary>
    public class StatisticalAnalysis
    {
        #region Methods

        /// <summary>
        /// Loads a result file using RNGXMLReader and creates statistical analysis from the data
        /// </summary>
        /// <param name="sFilePath">IN - Path to the XML result file to load</param>
        /// <returns>List of double values from the loaded file, or null if loading fails</returns>
        /// <exception cref="ArgumentException">Thrown when sFilePath is null or empty</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when the specified file does not exist</exception>
        /// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
        /// <exception cref="InvalidOperationException">Thrown when XML parsing or data loading fails</exception>
        public List<double> LoadResultFile(string sFilePath)
        {
            // Validate input parameter
            if (string.IsNullOrEmpty(sFilePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(sFilePath));
            }

            // Validate file exists
            if (false == System.IO.File.Exists(sFilePath))
            {
                throw new System.IO.FileNotFoundException($"Result file not found: {sFilePath}", sFilePath);
            }

            List<double> resultData = null;

            try
            {
                // Create the XML reader for loading the result file
                RNGXMLReader xmlReader = new RNGXMLReader(sFilePath);

                // Create a temporary session data object to hold the loaded data
                // We need to create dummy file and timer objects since RNGSessionData requires them
                RNGXMLWriter dummyWriter = new RNGXMLWriter();
                RNGSessionDataFile dummyDataFile = new RNGSessionDataFile(dummyWriter, xmlReader);
                RNGSessionTimer dummyTimer = new RNGSessionTimer();
                RNGSessionData tempSessionData = new RNGSessionData(dummyDataFile, dummyTimer);

                // Load the file data into the temporary session data object
                const uint iBATCH_SIZE = 10000; // Use large batch size for efficient loading
                bool bLoadSuccess = xmlReader.LoadFile(tempSessionData, iBATCH_SIZE);

                if (bLoadSuccess)
                {
                    // Extract the data points and convert to List<double>
                    resultData = tempSessionData.DataPoints.ToList();

                    // Create statistical analysis for the loaded data and store it
                    if (resultData.Count > 0)
                    {
                        m_LoadedFileStats = new DescriptiveStatistics(resultData);
                        m_sLoadedFileName = System.IO.Path.GetFileName(sFilePath);
                    }
                }
                else
                {
                    // Get the specific error message from the reader
                    string sErrorMessage = string.IsNullOrEmpty(xmlReader.LastError) ? "Unknown error" : xmlReader.LastError;
                    throw new InvalidOperationException($"Failed to load result file '{System.IO.Path.GetFileName(sFilePath)}': {sErrorMessage}");
                }

                // Clean up the reader
                xmlReader.Close();
            }
            catch (System.Xml.XmlException xmlEx)
            {
                throw new InvalidOperationException($"XML parsing error in file '{System.IO.Path.GetFileName(sFilePath)}': {xmlEx.Message}", xmlEx);
            }
            catch (System.IO.IOException)
            {
                // Re-throw IO exceptions
                throw;
            }
            catch (Exception generalEx)
            {
                throw new InvalidOperationException($"Unexpected error loading result file '{System.IO.Path.GetFileName(sFilePath)}': {generalEx.Message}", generalEx);
            }

            return resultData;
        }

        /// <summary>
        /// Compares two data distributions and displays statistical analysis
        /// </summary>
        /// <param name="setA">IN - First data set to compare</param>
        /// <param name="nameA">IN - Name/label for the first data set</param>
        /// <param name="setB">IN - Second data set to compare</param>
        /// <param name="nameB">IN - Name/label for the second data set</param>
        /// <exception cref="ArgumentNullException">Thrown when setA or setB is null</exception>
        /// <exception cref="ArgumentException">Thrown when nameA or nameB is null or empty</exception>
        public void CompareDistributions(List<double> setA, string nameA, List<double> setB, string nameB)
        {
            // Validate input parameters
            if (null == setA)
            {
                throw new ArgumentNullException(nameof(setA), "First data set cannot be null");
            }

            if (null == setB)
            {
                throw new ArgumentNullException(nameof(setB), "Second data set cannot be null");
            }

            if (string.IsNullOrEmpty(nameA))
            {
                throw new ArgumentException("First data set name cannot be null or empty", nameof(nameA));
            }

            if (string.IsNullOrEmpty(nameB))
            {
                throw new ArgumentException("Second data set name cannot be null or empty", nameof(nameB));
            }

            // Create statistical analysis for both data sets
            DescriptiveStatistics statsA = new DescriptiveStatistics(setA);
            DescriptiveStatistics statsB = new DescriptiveStatistics(setB);

            // Store the current comparison data
            m_StatsA = statsA;
            m_StatsB = statsB;

            // Display statistical comparison
            Console.WriteLine($"--- {nameA} ---");
            Console.WriteLine($"Mean     : {statsA.Mean:F6}");
            //Console.WriteLine($"Median   : {statsA.Median:F6}");
            Console.WriteLine($"Std Dev  : {statsA.StandardDeviation:F6}");
            Console.WriteLine($"Skewness : {statsA.Skewness:F6}");
            Console.WriteLine($"Kurtosis : {statsA.Kurtosis:F6}");

            Console.WriteLine($"\n--- {nameB} ---");
            Console.WriteLine($"Mean     : {statsB.Mean:F6}");
            //Console.WriteLine($"Median   : {statsB.Median:F6}");
            Console.WriteLine($"Std Dev  : {statsB.StandardDeviation:F6}");
            Console.WriteLine($"Skewness : {statsB.Skewness:F6}");
            Console.WriteLine($"Kurtosis : {statsB.Kurtosis:F6}");

            Console.WriteLine($"\n--- Comparison ---");
            Console.WriteLine($"Mean diff: {statsA.Mean - statsB.Mean:F6}");
            Console.WriteLine($"Skew diff: {statsA.Skewness - statsB.Skewness:F6}");
        }

        /// <summary>
        /// Analyzes a single data set loaded from a result file
        /// </summary>
        /// <param name="dataSet">IN - Data set to analyze</param>
        /// <param name="sDataSetName">IN - Name/label for the data set</param>
        /// <exception cref="ArgumentNullException">Thrown when dataSet is null</exception>
        /// <exception cref="ArgumentException">Thrown when sDataSetName is null or empty or dataSet is empty</exception>
        public void AnalyzeSingleDataSet(List<double> dataSet, string sDataSetName)
        {
            // Validate input parameters
            if (null == dataSet)
            {
                throw new ArgumentNullException(nameof(dataSet), "Data set cannot be null");
            }

            if (string.IsNullOrEmpty(sDataSetName))
            {
                throw new ArgumentException("Data set name cannot be null or empty", nameof(sDataSetName));
            }

            if (dataSet.Count == 0)
            {
                throw new ArgumentException("Data set cannot be empty", nameof(dataSet));
            }

            // Create statistical analysis for the data set
            DescriptiveStatistics stats = new DescriptiveStatistics(dataSet);

            // Display statistical analysis
            Console.WriteLine($"--- Statistical Analysis: {sDataSetName} ---");
            Console.WriteLine($"Count    : {dataSet.Count:N0}");
            Console.WriteLine($"Mean     : {stats.Mean:F6}");
            Console.WriteLine($"Std Dev  : {stats.StandardDeviation:F6}");
            Console.WriteLine($"Variance : {stats.Variance:F6}");
            Console.WriteLine($"Skewness : {stats.Skewness:F6}");
            Console.WriteLine($"Kurtosis : {stats.Kurtosis:F6}");
            Console.WriteLine($"Minimum  : {stats.Minimum:F6}");
            Console.WriteLine($"Maximum  : {stats.Maximum:F6}");
            Console.WriteLine($"Range    : {(stats.Maximum - stats.Minimum):F6}");
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets the statistical analysis for the last loaded file (read-only)
        /// </summary>
        public DescriptiveStatistics LoadedFileStats { get => m_LoadedFileStats; }

        /// <summary>
        /// Gets the name of the last loaded file (read-only)
        /// </summary>
        public string LoadedFileName { get => m_sLoadedFileName; }

        /// <summary>
        /// Gets the statistical analysis for data set A from the last comparison (read-only)
        /// </summary>
        public DescriptiveStatistics StatsA { get => m_StatsA; }

        /// <summary>
        /// Gets the statistical analysis for data set B from the last comparison (read-only)
        /// </summary>
        public DescriptiveStatistics StatsB { get => m_StatsB; }

        #endregion
        #region Data Members

        // The statistics objects for the two data sets from comparisons
        private DescriptiveStatistics m_StatsA;
        private DescriptiveStatistics m_StatsB;

        // Statistics and name for loaded file data
        private DescriptiveStatistics m_LoadedFileStats;
        private string m_sLoadedFileName = string.Empty;

        #endregion
    }
}
