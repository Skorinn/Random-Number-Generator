//*********************************************************************************************************************
// File Name:      Program.cs
// Description:    Primary class for the Random Number Generator program
//
// Copyright (c) 2022 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2022/09/10 - Mike Pullen - Original implementation.
// 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
// 2024/03/08 - Mike Pullen - Forced simulator mode for the example as the TruRNGpro header was removed due to unclear
//                            licensing.
// 2026/08/31 - Mike Pullen - Removed the note about forced simulator mode, as the device interface is selected at
//                            runtime and the simulator is no longer forced.
//*********************************************************************************************************************
using System;
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

            // Create the XML writer and session data file objects
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.IndentChars = "\t";
            RNGXMLWriter writer = new RNGXMLWriter(writerSettings);

            // Create the XML reader for session data loading
            RNGXMLReader reader = new RNGXMLReader();

            // Create the session data file with both writer and reader capabilities
            RNGSessionDataFile sessionDataFile = new RNGSessionDataFile(writer, reader);

            // Create the session timer object
            RNGSessionTimer sessionTimer = new RNGSessionTimer();

            // Create the session data object
            RNGSessionData sessionData = new RNGSessionData(sessionDataFile, sessionTimer);

            // Create the device interface timer
            RNGDeviceTimer deviceTimer = new RNGDeviceTimer();

            // Create and run the form
            GeneratorForm generatorForm = new GeneratorForm(sessionData, deviceTimer);
            Application.Run(generatorForm);
        }
    }
}
