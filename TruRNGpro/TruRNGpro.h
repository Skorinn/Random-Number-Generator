//*********************************************************************************************************************
//* File Name:      TruRNGpro.h
//* Description:    Interface to the TruRNGPro device
//*
//* Copyright (C) 2022-2024 Mike Pullen. All Rights Reserved.
//* Confidential and Proprietary
//*
//* Revision History: 
//=====================================================================================================================
//* 2022/09/10 - Mike Pullen - Original implementation.
//* 2022/10/30 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
//* 2022/03/08 - Mike Pullen - Removed TruRNGpro code for the example for work due to unclear license terms
//*********************************************************************************************************************
#pragma once
#include "RNGInterface.h"

#include <cassert>

namespace RNGInterfaces
{
    /// <summary>
    /// Interface to the TruRNGpro 
    /// </summary>
    class TruRNGpro : public RNGInterface
    {
    public:
        TruRNGpro(); // Default contructor does nothing and requires init call before use
        TruRNGpro(unsigned int iPortNum);
        virtual ~TruRNGpro();

        virtual bool Initialize(unsigned int iPortNum);
        virtual bool GetBitAverage(double& rResult);

    private:
        static const size_t mc_iTRURNGPRO_BUFFER_SIZE = 4096;// Size of buffer used to read from the device. Determines number of bytes per read.
        unsigned char m_Buffer[mc_iTRURNGPRO_BUFFER_SIZE]; // Buffer for reading from the device
    };

    /// <summary>
    /// Default constructor. Note: Must call Initialize before use.
    /// </summary>
    TruRNGpro::TruRNGpro() : m_Buffer{}
    {
        // Redacted
    }

    /// <summary>
    /// Constructor specifying port number
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    TruRNGpro::TruRNGpro(unsigned int iPortNum) : m_Buffer{}
    {
        // Redacted
    }

    /// <summary>
    /// Destructor 
    /// </summary>
    TruRNGpro::~TruRNGpro()
    {
        // Redacted
    }

    /// <summary>
    /// Initializes the interface to the device
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    /// <returns>true, if the device is ready to use, or false, if there is an error</returns>
    bool TruRNGpro::Initialize(unsigned int iPortNum)
    {
        // Redacted
        return false;
    }

    /// <summary>
    /// Reads data from the device and retrieves the average of the bits
    /// </summary>
    /// <param name="rfResult">OUT - Average of the bits read</param>
    /// <returns>true - if successful, otherwise false</returns>
    bool TruRNGpro::GetBitAverage(double& rfResult)
    {
        // Redacted
        rfResult = 0.0;
        return false;
    }
}
