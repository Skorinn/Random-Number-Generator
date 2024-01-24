//*********************************************************************************************************************
// File Name:      GeneratorForm.Test.cs
// Description:    Unit tests for the GeneratorForm class
//
// Copyright (C) 2024 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 01/20/2024 - Mike Pullen - Original implementation.
//*********************************************************************************************************************

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Xml;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the GeneratorForm class
    /// </summary>
    [TestClass]
    public class GeneratorFormTests
    {
        #region Infrastructure
        /// <summary>
        /// Default constructor
        /// </summary>
        public GeneratorFormTests()
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

        // <summary>
        // Test for get and set for the Busy property using true
        // <\summary>
        [TestMethod]
        public void Busy_SetProperty_True()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the dependant data object not under test
            RNGSessionData sessionData = new RNGSessionData();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.Busy = true;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.IsTrue(generatorForm.Busy);
        }

        // <summary>
        // Test for get and set for the Busy property using false
        // <\summary>
        [TestMethod]
        public void Busy_SetProperty_False()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the dependant data object not under test
            RNGSessionData sessionData = new RNGSessionData();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.Busy = false;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.IsFalse(generatorForm.Busy);
        }

        // <summary>
        // Test for get and set for the StatusBoxText property
        // <\summary>
        [TestMethod]
        public void StatusBoxText_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            const string sEXPECTED_TEXT = "Expected String";

            // Create the dependant data object not under test
            RNGSessionData sessionData = new RNGSessionData();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxText = sEXPECTED_TEXT;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(sEXPECTED_TEXT, generatorForm.StatusBoxText);
        }

        // <summary>
        // Test for get and set for the StatusBoxTextColor property
        // <\summary>
        [TestMethod]
        public void StatusBoxTextColor_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            Color expectedTextColor = Color.MediumPurple;

            // Create the dependant data object not under test
            RNGSessionData sessionData = new RNGSessionData();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxTextColor = expectedTextColor;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(expectedTextColor, generatorForm.StatusBoxTextColor);
        }

        // <summary>
        // Test for get and set for the StatusBoxBackColor property
        // <\summary>
        [TestMethod]
        public void StatusBoxBackColor_SetProperty_PropertiesCorrect()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create the expected values
            Color expectedBackColor = Color.MediumPurple;

            // Create the dependant data object not under test
            RNGSessionData sessionData = new RNGSessionData();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create the object under test
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the property under test
            generatorForm.StatusBoxBackColor = expectedBackColor;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the property was set correctly using get
            Assert.AreEqual(expectedBackColor, generatorForm.StatusBoxBackColor);
        }

        #endregion
    }
}