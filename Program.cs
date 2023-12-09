//*********************************************************************************************************************
// File Name:      Program.cs
// Description:    Primary class for the Random Number Generator program
//
// Copyright (C) 2022 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 09/10/2022 - Mike Pullen - Original implementation.
// 10/30/2022 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
//*********************************************************************************************************************
using System;
using System.Data;
using System.Windows.Forms;
using System.Xml;

namespace RandomNumberGenerator
{
    /// <summary>
    /// Represents the Random Number Generator program
    /// </summary>
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Setup visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create the session data object
            RNGSessionData sessionData = new RNGSessionData();

            // Create the XML writer and session data file objects
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.IndentChars = "\t";
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer);

            // Create and run the form
            GeneratorForm generatorForm = new GeneratorForm(sessionData, sessionDataFile);
            Application.Run(generatorForm);
        }
    }
}
