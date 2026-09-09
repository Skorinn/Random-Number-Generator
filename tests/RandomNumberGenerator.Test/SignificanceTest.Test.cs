//*********************************************************************************************************************
// File Name:      SignificanceTest.Test.cs
// Description:    Unit tests for the SignificanceTest class
//
// Copyright (c) 2022-2026 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History:
//====================================================================================================================
// 2026/09/07 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the SignificanceTest class
    /// </summary>
    [TestClass]
    public class SignificanceTestTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public SignificanceTestTests()
        {
            // Nothing to do
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }

        #endregion
        #region Tests

        /// <summary>
        /// Tests that two sets of readings drawn from the same source are not reported as differing. This is
        /// the outcome that matters most: a test that cries significance over noise is worse than no test.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_SameSource_ReportsNoSignificantShift()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Both sets drawn from the same distribution, from a fixed seed so the outcome does not vary
            Random source = new Random(m_iFIXED_SEED);
            List<double> baseline = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);
            List<double> result = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareMeans(baseline, result);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(test.Valid, "The test should be able to run on two full sets of readings");
            Assert.IsFalse(test.Significant,
                           $"Readings from the same source should not be reported as shifted. p = {test.Probability}");
        }

        /// <summary>
        /// Tests that a set of readings shifted well away from the baseline is reported as significant
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_ShiftedResult_ReportsSignificantShift()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // A shift of several standard errors, which is what the test exists to find
            Random source = new Random(m_iFIXED_SEED);
            List<double> baseline = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);
            List<double> result = BuildReadings(source, m_iTYPICAL_COUNT, (m_fEXPECTED_MEAN + m_fCLEAR_SHIFT));

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareMeans(baseline, result);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(test.Valid, "The test should be able to run on two full sets of readings");
            Assert.IsTrue(test.Significant,
                          $"A shift of {m_fCLEAR_SHIFT} should be found. p = {test.Probability}");
            Assert.IsTrue((0 < test.Statistic), "A result above the baseline should give a positive statistic");
        }

        /// <summary>
        /// Tests that the test does not depend on the two sets being the same size, which is the reason
        /// Welch's test is used rather than the one that assumes they are
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_VeryDifferentSizes_StillReportsAShift()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // A long baseline against a short run, which is the usual shape of these comparisons
            Random source = new Random(m_iFIXED_SEED);
            List<double> baseline = BuildReadings(source, m_iLONG_COUNT, m_fEXPECTED_MEAN);
            List<double> result = BuildReadings(source, m_iSHORT_COUNT, (m_fEXPECTED_MEAN + m_fCLEAR_SHIFT));

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareMeans(baseline, result);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(test.Valid, "A long baseline against a short run should still be testable");
            Assert.IsTrue(test.Significant,
                          $"A clear shift should be found even from a short run. p = {test.Probability}");

            // Welch's degrees of freedom fall between the two sizes rather than being their total
            Assert.IsTrue((test.DegreesOfFreedom < (m_iLONG_COUNT + m_iSHORT_COUNT)),
                          $"Degrees of freedom should not be the combined count. Actual: {test.DegreesOfFreedom}");
        }

        /// <summary>
        /// Tests that too few readings to measure a spread from are reported as no test rather than as a
        /// result, so nothing downstream shows a probability that was never calculated
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_TooFewReadings_ReportsNoTest()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            List<double> single = new List<double> { m_fEXPECTED_MEAN };
            Random source = new Random(m_iFIXED_SEED);
            List<double> full = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareMeans(single, full);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsFalse(test.Valid, "A single reading has no spread and cannot be tested");
            Assert.IsFalse(test.Significant, "An untestable comparison must never report significance");
        }

        /// <summary>
        /// Tests that readings which never change are reported as no test rather than dividing by a spread
        /// of zero and reporting certainty
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_NoSpreadInEitherSet_ReportsNoTest()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            List<double> flatBaseline = new List<double> { 0.5, 0.5, 0.5, 0.5, 0.5 };
            List<double> flatResult = new List<double> { 0.6, 0.6, 0.6, 0.6, 0.6 };

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareMeans(flatBaseline, flatResult);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsFalse(test.Valid, "Readings with no spread give nothing to measure a difference against");
            Assert.IsFalse(test.Significant, "An untestable comparison must never report significance");
        }

        /// <summary>
        /// Tests that a null set of readings is refused rather than dereferenced
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompareMeans_NullBaseline_ThrowsArgumentNullException()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            Random source = new Random(m_iFIXED_SEED);
            List<double> full = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceTest.CompareMeans(null, full);
        }

        /// <summary>
        /// Tests that readings sitting where they are expected to sit are not reported as having shifted
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareWithExpected_ReadingsAtTheExpectedValue_ReportsNoSignificantShift()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            Random source = new Random(m_iFIXED_SEED);
            List<double> readings = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareWithExpected(readings, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(test.Valid, "A full set of readings should be testable against the expected value");
            Assert.IsFalse(test.Significant,
                           $"Readings around the expected value should not be reported as shifted. p = {test.Probability}");
            Assert.AreEqual((m_iTYPICAL_COUNT - 1), test.DegreesOfFreedom, m_fFREEDOM_TOLERANCE,
                            "A one sample test has one degree of freedom fewer than it has readings");
        }

        /// <summary>
        /// Tests that readings sitting away from the expected value are reported as having shifted
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareWithExpected_ShiftedReadings_ReportsSignificantShift()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            Random source = new Random(m_iFIXED_SEED);
            List<double> readings = BuildReadings(source, m_iTYPICAL_COUNT, (m_fEXPECTED_MEAN + m_fCLEAR_SHIFT));

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult test = SignificanceTest.CompareWithExpected(readings, m_fEXPECTED_MEAN);

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(test.Valid, "A full set of readings should be testable against the expected value");
            Assert.IsTrue(test.Significant,
                          $"Readings shifted by {m_fCLEAR_SHIFT} should be found. p = {test.Probability}");
        }

        /// <summary>
        /// Tests that the reported probability stays within the range a probability can take, whichever way
        /// the readings fall
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void CompareMeans_EitherDirection_ProbabilityStaysWithinRange()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            Random source = new Random(m_iFIXED_SEED);
            List<double> baseline = BuildReadings(source, m_iTYPICAL_COUNT, m_fEXPECTED_MEAN);
            List<double> above = BuildReadings(source, m_iTYPICAL_COUNT, (m_fEXPECTED_MEAN + m_fCLEAR_SHIFT));
            List<double> below = BuildReadings(source, m_iTYPICAL_COUNT, (m_fEXPECTED_MEAN - m_fCLEAR_SHIFT));

            //**************************************************************//
            // Act
            //**************************************************************//

            SignificanceResult shiftedUp = SignificanceTest.CompareMeans(baseline, above);
            SignificanceResult shiftedDown = SignificanceTest.CompareMeans(baseline, below);
            SignificanceResult unshifted = SignificanceTest.CompareMeans(baseline, baseline);

            //**************************************************************//
            // Assert
            //**************************************************************//

            foreach (SignificanceResult test in new SignificanceResult[] { shiftedUp, shiftedDown, unshifted })
            {
                Assert.IsTrue(((0.0 <= test.Probability) && (1.0 >= test.Probability)),
                              $"A probability should sit between zero and one. Actual: {test.Probability}");
            }

            // A two-tailed test does not care which way the shift went, so both should be found
            Assert.IsTrue(shiftedUp.Significant, "A shift upwards should be found");
            Assert.IsTrue(shiftedDown.Significant, "A shift downwards should be found");
            Assert.IsTrue((0 > shiftedDown.Statistic), "A result below the baseline should give a negative statistic");

            // A set compared against itself has shifted by nothing at all
            Assert.AreEqual(1.0, unshifted.Probability, m_fPROBABILITY_TOLERANCE,
                            "A set compared against itself should be as unremarkable as it is possible to be");
        }

        /// <summary>
        /// Builds a set of readings scattered around a mean, in the shape the device produces: a bit average
        /// close to a half, varying by a small amount either side
        /// </summary>
        /// <param name="source">IN - The source of the scatter</param>
        /// <param name="iCount">IN - How many readings to build</param>
        /// <param name="fMean">IN - The value to scatter them around</param>
        /// <returns>The readings</returns>
        private static List<double> BuildReadings(Random source, int iCount, double fMean)
        {
            List<double> readings = new List<double>(iCount);
            for (int iIndex = 0; iIndex < iCount; iIndex++)
            {
                // Two draws averaged, which gathers the readings toward the middle the way the device does
                double fScatter = (((source.NextDouble() + source.NextDouble()) / 2.0) - 0.5);
                readings.Add(fMean + (fScatter * m_fSCATTER_WIDTH));
            }
            return readings;
        }

        #endregion
        #region Constants

        // A fixed seed, so a test that passes today passes tomorrow
        private const int m_iFIXED_SEED = 20260907;

        // The value an unbiased generator's readings average to
        private const double m_fEXPECTED_MEAN = 0.5;

        // How widely the readings are scattered, of the order the device produces
        private const double m_fSCATTER_WIDTH = 0.02;

        // A shift large enough that it should be found rather than missed
        private const double m_fCLEAR_SHIFT = 0.004;

        // Session sizes, of the order a real comparison is made from
        private const int m_iTYPICAL_COUNT = 400;
        private const int m_iLONG_COUNT = 2000;
        private const int m_iSHORT_COUNT = 120;

        // Tolerances
        private const double m_fFREEDOM_TOLERANCE = 0.001;
        private const double m_fPROBABILITY_TOLERANCE = 0.000001;

        #endregion
    }
}
