//*********************************************************************************************************************
// File Name:      SignificanceTest.cs
// Description:    Tests of whether a difference between sets of readings is more than noise
//
// Copyright (c) 2022-2026 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History:
//====================================================================================================================
// 2026/09/07 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace RandomNumberGenerator
{
    /// <summary>
    /// The outcome of a significance test: how large the difference is against the noise around it, how
    /// much data stood behind that, and how often a difference at least that large would turn up by chance
    /// if there were no real difference at all.
    /// </summary>
    public struct SignificanceResult
    {
        #region Properties

        /// <summary>
        /// Whether the test could be carried out. A test needs at least two readings on each side and some
        /// spread among them; a set that never changes gives nothing to measure a difference against.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// The size of the difference measured in standard errors
        /// </summary>
        public double Statistic { get; set; }

        /// <summary>
        /// The degrees of freedom the test was carried out with
        /// </summary>
        public double DegreesOfFreedom { get; set; }

        /// <summary>
        /// How often a difference at least this large would turn up by chance alone, between 0 and 1. Small
        /// values say the difference is hard to explain as noise.
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Whether the difference is significant at the level the application reports against (read-only)
        /// </summary>
        public bool Significant { get => (Valid && (Probability < SIGNIFICANCE_LEVEL)); }

        #endregion
        #region Constants

        /// <summary>
        /// The probability below which a difference is reported as significant. Five in a hundred is the
        /// usual convention, and is the figure the wording in the interface refers to.
        /// </summary>
        public const double SIGNIFICANCE_LEVEL = 0.05;

        #endregion
    }

    /// <summary>
    /// Tests of whether a difference between sets of readings is larger than the noise in them.
    /// NOTE: Both tests are two-tailed, so a shift in either direction counts. A one-tailed test would find
    /// a shift toward a target more easily, but only holds when the direction was predicted before the
    /// readings were taken, and that is not something this application can know.
    /// </summary>
    public static class SignificanceTest
    {
        #region Methods

        /// <summary>
        /// Compares the means of two sets of readings. This is Welch's test, which does not assume the two
        /// sets are the same size or equally spread, and neither holds here: a baseline is often recorded
        /// for far longer than the run being compared against it.
        /// </summary>
        /// <param name="baseline">IN - The readings to compare against (cannot be null)</param>
        /// <param name="result">IN - The readings being tested (cannot be null)</param>
        /// <returns>The outcome, which reports itself invalid when there is too little to test</returns>
        /// <exception cref="ArgumentNullException">Thrown when either set of readings is null</exception>
        public static SignificanceResult CompareMeans(IList<double> baseline, IList<double> result)
        {
            if (null == baseline)
            {
                throw new ArgumentNullException(nameof(baseline), "Baseline readings cannot be null");
            }

            if (null == result)
            {
                throw new ArgumentNullException(nameof(result), "Result readings cannot be null");
            }

            // Both sides need enough readings for a spread to be measured
            bool bEnoughData = ((m_iMINIMUM_READINGS <= baseline.Count) && (m_iMINIMUM_READINGS <= result.Count));
            if (false == bEnoughData)
            {
                return new SignificanceResult { Valid = false };
            }

            double fBaselineMean = Mean(baseline);
            double fResultMean = Mean(result);
            double fBaselineError = (Variance(baseline, fBaselineMean) / baseline.Count);
            double fResultError = (Variance(result, fResultMean) / result.Count);
            double fCombinedError = (fBaselineError + fResultError);

            // Readings that never vary leave nothing to measure a difference against
            if (m_fMINIMUM_ERROR >= fCombinedError)
            {
                return new SignificanceResult { Valid = false };
            }

            double fStatistic = ((fResultMean - fBaselineMean) / Math.Sqrt(fCombinedError));

            // Welch's degrees of freedom, which fall between the two sample sizes according to how much each
            // side contributes to the combined error
            double fDenominator = (((fBaselineError * fBaselineError) / (baseline.Count - 1)) +
                                   ((fResultError * fResultError) / (result.Count - 1)));
            double fFreedom = ((fCombinedError * fCombinedError) / fDenominator);

            return BuildResult(fStatistic, fFreedom);
        }

        /// <summary>
        /// Compares the mean of one set of readings against the value they would be expected to average to
        /// if nothing had shifted them
        /// </summary>
        /// <param name="readings">IN - The readings to test (cannot be null)</param>
        /// <param name="fExpectedMean">IN - The value the readings are expected to average to</param>
        /// <returns>The outcome, which reports itself invalid when there is too little to test</returns>
        /// <exception cref="ArgumentNullException">Thrown when the readings are null</exception>
        public static SignificanceResult CompareWithExpected(IList<double> readings, double fExpectedMean)
        {
            if (null == readings)
            {
                throw new ArgumentNullException(nameof(readings), "Readings cannot be null");
            }

            if (m_iMINIMUM_READINGS > readings.Count)
            {
                return new SignificanceResult { Valid = false };
            }

            double fMean = Mean(readings);
            double fStandardError = (Variance(readings, fMean) / readings.Count);
            if (m_fMINIMUM_ERROR >= fStandardError)
            {
                return new SignificanceResult { Valid = false };
            }

            double fStatistic = ((fMean - fExpectedMean) / Math.Sqrt(fStandardError));
            return BuildResult(fStatistic, (readings.Count - 1));
        }

        /// <summary>
        /// Turns a statistic and its degrees of freedom into an outcome, taking the two-tailed probability
        /// from the distribution the statistic follows
        /// </summary>
        /// <param name="fStatistic">IN - The size of the difference in standard errors</param>
        /// <param name="fFreedom">IN - The degrees of freedom of the test</param>
        /// <returns>The completed outcome</returns>
        private static SignificanceResult BuildResult(double fStatistic, double fFreedom)
        {
            // A statistic that is not a number says the arithmetic ran out of meaning rather than that the
            // difference was enormous, so it is reported as no test rather than as a certainty
            bool bUsable = ((false == double.IsNaN(fStatistic)) && (false == double.IsInfinity(fStatistic)) &&
                            (false == double.IsNaN(fFreedom)) && (0 < fFreedom));
            if (false == bUsable)
            {
                return new SignificanceResult { Valid = false };
            }

            // Both tails, so a shift in either direction counts against the null
            double fUpperTail = (1.0 - StudentT.CDF(0.0, 1.0, fFreedom, Math.Abs(fStatistic)));
            double fProbability = Math.Min(1.0, (2.0 * fUpperTail));

            return new SignificanceResult
            {
                Valid = true,
                Statistic = fStatistic,
                DegreesOfFreedom = fFreedom,
                Probability = fProbability
            };
        }

        /// <summary>
        /// The mean of a set of readings
        /// </summary>
        /// <param name="readings">IN - The readings, which must not be empty</param>
        /// <returns>The mean</returns>
        private static double Mean(IList<double> readings)
        {
            double fTotal = 0.0;
            foreach (double fReading in readings)
            {
                fTotal += fReading;
            }
            return (fTotal / readings.Count);
        }

        /// <summary>
        /// The sample variance of a set of readings, taken about a mean already calculated for them
        /// </summary>
        /// <param name="readings">IN - The readings, of which there must be at least two</param>
        /// <param name="fMean">IN - The mean of those readings</param>
        /// <returns>The sample variance</returns>
        private static double Variance(IList<double> readings, double fMean)
        {
            double fTotal = 0.0;
            foreach (double fReading in readings)
            {
                double fOffset = (fReading - fMean);
                fTotal += (fOffset * fOffset);
            }
            return (fTotal / (readings.Count - 1));
        }

        #endregion
        #region Constants

        // A spread cannot be measured from fewer than two readings
        private const int m_iMINIMUM_READINGS = 2;

        // Below this the readings are effectively all the same value, and dividing by their spread would
        // turn a difference of nothing into a statistic of anything
        private const double m_fMINIMUM_ERROR = 1e-300;

        #endregion
    }
}
