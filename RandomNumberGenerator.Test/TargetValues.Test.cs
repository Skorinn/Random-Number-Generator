//*********************************************************************************************************************
// File Name:      TargetValues.Test.cs
// Description:    Unit tests for the TargetValues class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2024/02/05 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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
            //
            // TODO: Add constructor logic here
            //
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
        public void TestMethod1()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            //**************************************************************//
            // Act
            //**************************************************************//

            //**************************************************************//
            // Assert
            //**************************************************************//
        }

        #endregion
    }
}
