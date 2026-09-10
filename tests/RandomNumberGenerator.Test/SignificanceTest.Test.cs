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

        /// <summary>
        /// Tests the smallest detectable shift agrees with the test it is derived from. A shift of exactly
        /// that size, applied to the readings, should sit right on the edge of significance: this is the
        /// same arithmetic read the other way round, so the two have to meet.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_ShiftOfThatSize_SitsOnTheEdgeOfSignificance()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const double fEXPECTED_MEAN = 0.5;
            const double fEDGE_TOLERANCE = 0.002;

            // Readings with a spread, sitting on the expected value so the shift applied below is the whole
            // of the difference the test sees
            List<double> readings = MakeSpreadReadings(200, fEXPECTED_MEAN, 0.001);

            //**************************************************************//
            // Act
            //**************************************************************//

            double fDetectable = SignificanceTest.CompareWithExpected(readings, fEXPECTED_MEAN).DetectableDifference;

            // Move every reading by exactly that much and ask the test what it makes of it
            List<double> shifted = new List<double>();
            foreach (double fReading in readings)
            {
                shifted.Add(fReading + fDetectable);
            }
            SignificanceResult onTheEdge = SignificanceTest.CompareWithExpected(shifted, fEXPECTED_MEAN);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify a shift of exactly the detectable size lands on the significance level rather than
            // somewhere unrelated to it
            Assert.IsTrue(onTheEdge.Valid);
            Assert.AreEqual(SignificanceResult.SIGNIFICANCE_LEVEL, onTheEdge.Probability, fEDGE_TOLERANCE,
                            $"A shift of {fDetectable} gave a probability of {onTheEdge.Probability}");
        }

        /// <summary>
        /// Tests a shift smaller than the detectable size cannot reach significance, which is the whole
        /// point of warning about it
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_SmallerShift_CannotReachSignificance()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const double fEXPECTED_MEAN = 0.5;
            const double fWELL_UNDER = 0.5;

            List<double> readings = MakeSpreadReadings(200, fEXPECTED_MEAN, 0.001);
            double fDetectable = SignificanceTest.CompareWithExpected(readings, fEXPECTED_MEAN).DetectableDifference;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Half the shift the readings could show
            List<double> shifted = new List<double>();
            foreach (double fReading in readings)
            {
                shifted.Add(fReading + (fDetectable * fWELL_UNDER));
            }
            SignificanceResult tooSmall = SignificanceTest.CompareWithExpected(shifted, fEXPECTED_MEAN);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify a real shift of that size goes unfound, which is what the warning exists to say
            Assert.IsTrue(tooSmall.Valid);
            Assert.IsFalse(tooSmall.Significant,
                           $"A shift of half the detectable size reached significance at {tooSmall.Probability}");
        }

        /// <summary>
        /// Tests more readings pin the mean down more finely, at the rate the arithmetic says they should
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_MoreReadings_PinsTheMeanDownMoreFinely()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Four times the readings should halve the detectable shift, as it falls with the root of the
            // count. The tolerance is loose because the critical value also moves with the count.
            const double fEXPECTED_RATIO = 2.0;
            const double fRATIO_TOLERANCE = 0.15;

            List<double> few = MakeSpreadReadings(100, 0.5, 0.001);
            List<double> many = MakeSpreadReadings(400, 0.5, 0.001);

            //**************************************************************//
            // Act
            //**************************************************************//

            double fFew = SignificanceTest.CompareWithExpected(few, 0.5).DetectableDifference;
            double fMany = SignificanceTest.CompareWithExpected(many, 0.5).DetectableDifference;

            //**************************************************************//
            // Assert
            //**************************************************************//

            Assert.IsTrue(fMany < fFew, "More readings should pin the mean down more finely");
            Assert.AreEqual(fEXPECTED_RATIO, (fFew / fMany), fRATIO_TOLERANCE,
                            $"Four times the readings gave a ratio of {(fFew / fMany)}");
        }

        /// <summary>
        /// Tests readings too few to measure a spread report no detectable shift rather than a number that
        /// looks like one
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_TooFewReadings_ReportsNoAnswer()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            List<double> single = new List<double> { 0.5 };
            List<double> empty = new List<double>();
            List<double> identical = new List<double> { 0.5, 0.5, 0.5, 0.5 };

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Verify each reports no answer rather than zero, which would read as "any shift is detectable"
            Assert.IsTrue(double.IsNaN(SignificanceTest.CompareWithExpected(single, 0.5).DetectableDifference));
            Assert.IsTrue(double.IsNaN(SignificanceTest.CompareWithExpected(empty, 0.5).DetectableDifference));
            Assert.IsTrue(double.IsNaN(SignificanceTest.CompareWithExpected(identical, 0.5).DetectableDifference));
        }

        /// <summary>
        /// Tests a session that cannot show the shift being looked for is reported as not sensitive enough,
        /// and one that can is not
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void IsSensitiveEnough_AgainstTheShiftOfInterest_AnswersEitherWay()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            const double fJUST_INSIDE = 0.9;
            const double fJUST_OUTSIDE = 1.1;

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Verify the answer turns on the shift being looked for rather than on a reading count
            Assert.IsTrue(SignificanceTest.IsSensitiveEnough(SignificanceTest.SHIFT_OF_INTEREST * fJUST_INSIDE));
            Assert.IsTrue(SignificanceTest.IsSensitiveEnough(SignificanceTest.SHIFT_OF_INTEREST));
            Assert.IsFalse(SignificanceTest.IsSensitiveEnough(SignificanceTest.SHIFT_OF_INTEREST * fJUST_OUTSIDE));

            // Verify no answer is not mistaken for a good one
            Assert.IsFalse(SignificanceTest.IsSensitiveEnough(double.NaN));
        }

        /// <summary>
        /// Tests a short device session cannot show the shift being looked for while a longer one can, using
        /// the spread a real device actually produces. This is the case the warning was added for.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_RealDeviceSpread_TurnsOverAtTheExpectedLength()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Each device reading is the average of 32768 bytes, so its spread is about 0.5 over the root
            // of that many bits. A minute of recording is roughly 550 readings.
            const double fDEVICE_SPREAD = 0.00098;
            const int iTEN_SECONDS = 90;
            const int iTWO_MINUTES = 1100;

            List<double> shortSession = MakeSpreadReadings(iTEN_SECONDS, 0.5, fDEVICE_SPREAD);
            List<double> longSession = MakeSpreadReadings(iTWO_MINUTES, 0.5, fDEVICE_SPREAD);

            //**************************************************************//
            // Act
            //**************************************************************//

            bool bShortEnough = SignificanceTest.IsSensitiveEnough(SignificanceTest.CompareWithExpected(shortSession, 0.5).DetectableDifference);
            bool bLongEnough = SignificanceTest.IsSensitiveEnough(SignificanceTest.CompareWithExpected(longSession, 0.5).DetectableDifference);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify ten seconds of device readings cannot speak to the shift and two minutes can
            Assert.IsFalse(bShortEnough, "Ten seconds of readings should not be enough");
            Assert.IsTrue(bLongEnough, "Two minutes of readings should be enough");
        }

        /// <summary>
        /// Tests the difference two sessions can show between them is reported, and that it is coarser than
        /// what either could show on its own - the noise of both stands between them
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DetectableDifference_TwoSessions_IsCoarserThanEitherAlone()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            List<double> baseline = MakeSpreadReadings(300, 0.5, 0.001);
            List<double> result = MakeSpreadReadings(300, 0.5, 0.001);

            //**************************************************************//
            // Act
            //**************************************************************//

            double fBaselineAlone = SignificanceTest.CompareWithExpected(baseline, 0.5).DetectableDifference;
            double fBetween = SignificanceTest.CompareMeans(baseline, result).DetectableDifference;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify comparing two sessions is harder than measuring one against a fixed value, because both
            // sides carry noise
            Assert.IsFalse(double.IsNaN(fBetween));
            Assert.IsTrue(fBetween > fBaselineAlone,
                          $"Between them: {fBetween}, baseline alone: {fBaselineAlone}");
        }

        /// <summary>
        /// Tests a null set of readings is refused rather than dereferenced
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Tests_NullReadings_Exception()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            List<double> readings = new List<double> { 0.5, 0.6 };

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // The side each test is given as null, which the existing cover misses for all but the baseline
            Assert.ThrowsException<ArgumentNullException>(() => SignificanceTest.CompareWithExpected(null, 0.5));
            Assert.ThrowsException<ArgumentNullException>(() => SignificanceTest.CompareMeans(null, readings));
            Assert.ThrowsException<ArgumentNullException>(() => SignificanceTest.CompareMeans(readings, null));
        }

        /// <summary>
        /// Builds readings sitting a fixed distance either side of a mean, alternating, so their spread is
        /// settled rather than whatever a random draw happened to give.
        /// NOTE: fOffset is how far each reading sits from the mean, not the sample standard deviation the
        /// readings end up with. Those are close but not equal: the sample deviation divides by one fewer
        /// than the count, so it comes out slightly the larger of the two.
        /// </summary>
        /// <param name="iCount">IN - How many readings to make</param>
        /// <param name="fMean">IN - The value to centre them on</param>
        /// <param name="fOffset">IN - How far each reading sits either side of the mean</param>
        /// <returns>The readings</returns>
        private static List<double> MakeSpreadReadings(int iCount, double fMean, double fOffset)
        {
            List<double> readings = new List<double>();
            for (int iIndex = 0; iIndex < iCount; ++iIndex)
            {
                // Half above and half below, so the mean lands where it was asked to and the spread is the
                // offset itself
                double fStep = ((0 == (iIndex % 2)) ? fOffset : -fOffset);
                readings.Add(fMean + fStep);
            }
            return readings;
        }

        #endregion
    }
}
