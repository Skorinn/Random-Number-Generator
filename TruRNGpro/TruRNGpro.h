//*********************************************************************************************************************
//* File Name:      TruRNGpro.h
//* Description:    Interface to the TruRNGPro device
//*
//* Copyright (C) 2022 Mike Pullen. All Rights Reserved.
//* Confidential and Proprietary
//*
//* Revision History: 
//=====================================================================================================================
//* 09/10/2022 - Mike Pullen - Original implementation.
//* 10/30/2022 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
//*********************************************************************************************************************
#pragma once
#include "rng.h"
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
        // Increase buffer size to compensate for reduced sample rate
        static const size_t mc_iTRURNGPRO_BUFFER_SIZE = 32768; // Increased from 4096 to 32768 (8x increase)
        unsigned char m_Buffer[mc_iTRURNGPRO_BUFFER_SIZE]; // Buffer for reading from the device
        RandomFromTrueRNG* m_pTruRNGProInterface; // 3rd party interface that wraps the COM port setup and reads
    };

    /// <summary>
    /// Default constructor. Note: Must call Initialize before use.
    /// </summary>
    TruRNGpro::TruRNGpro() : m_pTruRNGProInterface(nullptr), m_Buffer{}
    {
    }

    /// <summary>
    /// Constructor specifying port number
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    TruRNGpro::TruRNGpro(unsigned int iPortNum) : m_Buffer{}
    {
        m_pTruRNGProInterface = new RandomFromTrueRNG(iPortNum);
    }

    /// <summary>
    /// Destructor 
    /// </summary>
    TruRNGpro::~TruRNGpro()
    {
        delete m_pTruRNGProInterface;
        m_pTruRNGProInterface = nullptr;
    }

    /// <summary>
    /// Initializes the interface to the device
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    /// <returns>true, if the device is ready to use, or false, if there is an error</returns>
    bool TruRNGpro::Initialize(unsigned int iPortNum)
    {
        // Release the existing interface, if it exists, then create a new one
        if (nullptr != m_pTruRNGProInterface)
        {
            delete m_pTruRNGProInterface;
            m_pTruRNGProInterface = nullptr;
        }
        m_pTruRNGProInterface = new RandomFromTrueRNG(iPortNum);

        return !(m_pTruRNGProInterface->bad);
    }

    /// <summary>
    /// Reads data from the device and retrieves the average of the bits
    /// </summary>
    /// <param name="rfResult">OUT - Average of the bits read</param>
    /// <returns>true - if successful, otherwise false</returns>
    bool TruRNGpro::GetBitAverage(double& rfResult)
    {
        // Ensure the interace was initialized
        bool bStatus = (nullptr != m_pTruRNGProInterface);
        rfResult = 0.0;

        if (true == bStatus)
        {
            // Skip and return error if interface is in a bad state
            bStatus = !(m_pTruRNGProInterface->bad);
        }

        if (true == bStatus)
        {
            // Read the data from the device
            int iBytesRead = m_pTruRNGProInterface->fill(m_Buffer, mc_iTRURNGPRO_BUFFER_SIZE);

            // Skip and return an error if the number of bytes read doesn't match what was collected
            bStatus = (iBytesRead == mc_iTRURNGPRO_BUFFER_SIZE);
        }

        if (true == bStatus)
        {
            // Loop through the bytes read from the device
            INT64 iBitSum = 0;
            short iOddCount = 0;
            for (size_t iByteIndex = 0; iByteIndex < mc_iTRURNGPRO_BUFFER_SIZE; ++iByteIndex)
            {
                // Add up the value of each individual bit
                unsigned char& rCurrentByte = m_Buffer[iByteIndex];
                for (size_t iBitIndex = 0; iBitIndex < 8; ++iBitIndex)
                {
                    iBitSum += ((rCurrentByte >> iBitIndex) & 0x01);
                }
            }

            // The result is the average of all of the bits
            rfResult = (static_cast<double>(iBitSum) / (mc_iTRURNGPRO_BUFFER_SIZE * 8));
        }

        return bStatus;
    }
}
