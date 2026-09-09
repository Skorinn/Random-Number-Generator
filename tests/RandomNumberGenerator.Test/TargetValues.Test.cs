//*********************************************************************************************************************
// File Name:      TargetValues.Test.cs
// Description:    Unit tests for the TargetValues class
//
// Copyright (c) 2024 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2024/02/05 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Summary description for TargetValues
    /// </summary>
    [TestClass]
    public class TargetValuesTests
    {
        #region Infrastructure

        public TargetValuesTests()
        {
            // Nothing to do
        }

        #endregion
        #region Data Members

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        #region Tests

        [TestMethod]
        [TestCategory("Component")]
        public void ToString_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for a target of 0
            const int c_iZeroValue = 0;
            const string c_sZeroExpected = "0";

            // Test and expected values for a target of 1
            const int c_iOneValue = 1;
            const string c_sOneExpected = "1";

            // Test and expected values for a target of -1
            const int c_iNegativeOneValue = -1;
            const string c_sNegativeOneExpected = "None";

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the zero value
            string sZeroResult = TargetValues.ToString(c_iZeroValue);

            // Test the one value
            string sOneResult = TargetValues.ToString(c_iOneValue);

            // Test the negative one value
            string sNegativeOneResult = TargetValues.ToString(c_iNegativeOneValue);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the zero result
            Assert.AreEqual(c_sZeroExpected, sZeroResult);

            // Analyze the one result
            Assert.AreEqual(c_sOneExpected, sOneResult);

            // Analyze the negative one result
            Assert.AreEqual(c_sNegativeOneExpected, sNegativeOneResult);
        }

        /// <summary>
        /// Tests the ToString method with an invalid value
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ToString_Invalid_Null()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an invalid target
            const int c_iInvalidValue = 2;
            // Expect a null result

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the invalid value
            string sInvalidResult = TargetValues.ToString(c_iInvalidValue);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the invalid result
            Assert.IsNull(sInvalidResult);
        }

        /// <summary>
        /// Tests the ToInt method with a valid string
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ToInt_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for a target of 0
            const string c_sZeroValue = "0";
            const int c_iZeroExpected = 0;

            // Test and expected values for a target of 1
            const string c_sOneValue = "1";
            const int c_iOneExpected = 1;

            // Test and expected values for a target of -1
            const string c_sNegativeOneValue = "None";
            const int c_iNegativeOneExpected = -1;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the zero value
            int iZeroResult = TargetValues.ToInt(c_sZeroValue);

            // Test the one value
            int iOneResult = TargetValues.ToInt(c_sOneValue);

            // Test the negative one value
            int iNegativeOneResult = TargetValues.ToInt(c_sNegativeOneValue);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the zero result
            Assert.AreEqual(c_iZeroExpected, iZeroResult);

            // Analyze the one result
            Assert.AreEqual(c_iOneExpected, iOneResult);

            // Analyze the negative one result
            Assert.AreEqual(c_iNegativeOneExpected, iNegativeOneResult);
        }

        /// <summary>
        /// Tests the ToInt method with an invalid string
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ToInt_Invalid_NoValue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an invalid target
            const string c_sInvalidValue = "2";
            const int c_iInvalidExpected = TargetValues.NO_VALUE_SET;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the invalid value
            int iInvalidResult = TargetValues.ToInt(c_sInvalidValue);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the invalid result
            Assert.AreEqual(c_iInvalidExpected, iInvalidResult);
        }

        /// <summary>
        /// Tests the GetStringAt method with a valid index
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void GetStringAt_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an index of 0
            const uint c_iZeroIndex = 0;
            const string c_sZeroExpected = "None";

            // Test and expected values for an index of 1
            const uint c_iOneIndex = 1;
            const string c_sOneExpected = "0";

            // Test and expected values for an index of 2
            const uint c_iTwoIndex = 2;
            const string c_sTwoExpected = "1";

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the zero index
            string sZeroResult = TargetValues.GetStringAt(c_iZeroIndex);

            // Test the one index
            string sOneResult = TargetValues.GetStringAt(c_iOneIndex);

            // Test the two index
            string sTwoResult = TargetValues.GetStringAt(c_iTwoIndex);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the zero result
            Assert.AreEqual(c_sZeroExpected, sZeroResult);

            // Analyze the one result
            Assert.AreEqual(c_sOneExpected, sOneResult);

            // Analyze the two result
            Assert.AreEqual(c_sTwoExpected, sTwoResult);
        }

        /// <summary>
        /// Tests the GetStringAt method with an invalid index
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void GetStringAt_Invalid_Null()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an invalid index
            const uint c_iInvalidIndex = 3;
            // Expect a null result

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the invalid index
            string sInvalidResult = TargetValues.GetStringAt(c_iInvalidIndex);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the invalid result
            Assert.IsNull(sInvalidResult);
        }

        /// <summary>
        /// Tests the GetValueAt method with a valid index
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void GetValueAt_Valid_Success()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an index of 0
            const uint c_iZeroIndex = 0;
            const int c_iZeroExpected = -1;

            // Test and expected values for an index of 1
            const uint c_iOneIndex = 1;
            const int c_iOneExpected = 0;

            // Test and expected values for an index of 2
            const uint c_iTwoIndex = 2;
            const int c_iTwoExpected = 1;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the zero index
            int iZeroResult = TargetValues.GetValueAt(c_iZeroIndex);

            // Test the one index
            int iOneResult = TargetValues.GetValueAt(c_iOneIndex);

            // Test the two index
            int iTwoResult = TargetValues.GetValueAt(c_iTwoIndex);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the zero result
            Assert.AreEqual(c_iZeroExpected, iZeroResult);

            // Analyze the one result
            Assert.AreEqual(c_iOneExpected, iOneResult);

            // Analyze the two result
            Assert.AreEqual(c_iTwoExpected, iTwoResult);
        }

        /// <summary>
        /// Tests the GetValueAt method with an invalid index
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void GetValueAt_Invalid_NoValue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Test and expected values for an invalid index
            const uint c_iInvalidIndex = 3;
            const int c_iInvalidExpected = TargetValues.NO_VALUE_SET;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Test the invalid index
            int iInvalidResult = TargetValues.GetValueAt(c_iInvalidIndex);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Analyze the invalid result
            Assert.AreEqual(c_iInvalidExpected, iInvalidResult);
        }

        #endregion
    }
}
