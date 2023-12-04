//*********************************************************************************************************************
// File Name:      dllmain.cpp
// Description:    Entry point and exported functions
//
// Copyright (C) 2022-2023 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//=====================================================================================================================
// 2022/09/10 - Mike Pullen - Original implementation.
// 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
// 2023/12/01 - Mike Pullen - Added interface (class) and simulator
//*********************************************************************************************************************
#include "pch.h"
#include "TruRNGpro.h"
#include "RNGSimulator.h"

using namespace RNGInterfaces;

static RNGInterface* g_pDeviceInterface = nullptr; // Interface to the device

/// <summary>
/// DLL entry point 
/// </summary>
/// <param name="">IN - Handle to DLL module</param>
/// <param name="iCallReason">IN - Reason for the call</param>
/// <param name="">IN - Reserved</param>
/// <returns>TRUE, always</returns>
BOOL APIENTRY DllMain(HMODULE, DWORD  iCallReason, LPVOID)
{
    // Perform actions based on the reason for calling.
    switch (iCallReason)
    {
    case DLL_PROCESS_ATTACH:
        // Default to the simulator when first loaded
        if (nullptr == g_pDeviceInterface)
        {
            g_pDeviceInterface = dynamic_cast<RNGInterface*>(new RNGSimulator);
        }
        break;

    case DLL_THREAD_ATTACH:
        // No thread-specific initialization.
        break;

    case DLL_THREAD_DETACH:
        // No thread-specific cleanup.
        break;

    case DLL_PROCESS_DETACH:
        // Free any previously created interface object
        if (g_pDeviceInterface)
        {
            delete g_pDeviceInterface;
            g_pDeviceInterface = nullptr;
        }
        break;
    }
    return TRUE;  // Successful DLL_PROCESS_ATTACH.
}

/// <summary>
/// Initilize the interface with the specified port number 
/// </summary>
/// <param name="iPort">IN - COM port number the device to which the device is connected</param>
/// <returns>true, if interface is ready to use, otherwise false</returns>
extern "C" __declspec(dllexport) bool Initialize(int iPort, bool bSimulate)
{
    // Default the return value
    bool bStatus = false;

    // Free any previously created interface object
    if (g_pDeviceInterface)
    {
        delete g_pDeviceInterface;
        g_pDeviceInterface = nullptr;
    }

    // Create the new interface object
    if (bSimulate)
    {
        // If simulating, create a simulator object
        g_pDeviceInterface = dynamic_cast<RNGInterface*>(new RNGSimulator);
    }
    else
    {
        // Create the real device interface
        g_pDeviceInterface = dynamic_cast<RNGInterface*>(new TruRNGpro);
    }

    // If the object was created successfully
    if (g_pDeviceInterface)
    {
        // Initialize the object
        bStatus = g_pDeviceInterface->Initialize(iPort);
    }

    return bStatus;
}

/// <summary>
/// Read data from the device and get an average of the bits read 
/// NOTE: Initialize must be called first
/// </summary>
/// <param name="rfResult">OUT - Average of the bits read</param>
/// <returns>true, if successful, otherwise false</returns>
extern "C" __declspec(dllexport) bool GetRandomBitAverage(double& rfResult)
{
    // Default the return value
    bool bStatus = false;
    
    // If a device has been initialized
    if (g_pDeviceInterface)
    {
        // Get a bit average
        bStatus = g_pDeviceInterface->GetBitAverage(rfResult);
    }

    return bStatus;
}