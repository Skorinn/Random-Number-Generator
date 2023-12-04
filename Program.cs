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
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GeneratorForm());
        }
    }
}
