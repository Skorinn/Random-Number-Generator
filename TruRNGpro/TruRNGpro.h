//*********************************************************************************************************************
//* File Name:      TruRNGpro.h
//* Description:    Interface to the TruRNGPro device
//*
//* Copyright (c) 2022 Mike Pullen
//* Licensed under the MIT License. See LICENSE in the repository root.
//*
//* Revision History: 
//=====================================================================================================================
//* 09/10/2022 - Mike Pullen - Original implementation.
//* 10/30/2022 - Mike Pullen - Recreated under VS2022 and added ARM64 support.
//* 09/08/2026 - Mike Pullen - Opened the device again after a failed read rather than giving up on it, so a
//*                            momentary fault no longer ends the session
//* 09/09/2026 - Mike Pullen - Only reopened a device that had been opened once, so an instance that was
//*                            never initialized reports a failed read rather than trying to open COM0
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
        unsigned int m_iPortNum; // Port the interface was opened on, so it can be opened again

        bool ReadBuffer(); // One attempt at filling the buffer from the device
    };

    /// <summary>
    /// Default constructor. Note: Must call Initialize before use.
    /// </summary>
    TruRNGpro::TruRNGpro() : m_pTruRNGProInterface(nullptr), m_Buffer{}, m_iPortNum(0)
    {
    }

    /// <summary>
    /// Constructor specifying port number
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    TruRNGpro::TruRNGpro(unsigned int iPortNum) : m_Buffer{}, m_iPortNum(iPortNum)
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
        m_iPortNum = iPortNum;

        return !(m_pTruRNGProInterface->bad);
    }

    /// <summary>
    /// Makes one attempt at filling the buffer from the device
    /// </summary>
    /// <returns>true, if the whole buffer was filled, otherwise false</returns>
    bool TruRNGpro::ReadBuffer()
    {
        // Nothing to read from if the interface was never built, or has gone bad
        if ((nullptr == m_pTruRNGProInterface) || (m_pTruRNGProInterface->bad))
        {
            return false;
        }

        // Read the data from the device. A short read means the device stopped part way through, which the
        // third party interface reports by marking itself bad, so the count is what decides.
        int iBytesRead = m_pTruRNGProInterface->fill(m_Buffer, mc_iTRURNGPRO_BUFFER_SIZE);

        return (iBytesRead == mc_iTRURNGPRO_BUFFER_SIZE);
    }

    /// <summary>
    /// Reads data from the device and retrieves the average of the bits
    /// </summary>
    /// <param name="rfResult">OUT - Average of the bits read</param>
    /// <returns>true - if successful, otherwise false</returns>
    bool TruRNGpro::GetBitAverage(double& rfResult)
    {
        rfResult = 0.0;

        // Whether there is an interface that could have gone bad. Reopening only makes sense for a device
        // that was opened once and stopped answering; with no interface at all there is nothing to reopen,
        // and the port that would be tried is whatever the member happens to hold - zero, on an instance
        // that was never initialized, which sends the third party code off to open COM0. The class says
        // Initialize must be called first, and an instance that has not been gets a failed read and no
        // attempt at the port it was never given.
        bool bHadInterface = (nullptr != m_pTruRNGProInterface);

        // Try to read from the device as it stands
        bool bStatus = ReadBuffer();

        // A read that fails leaves the third party interface marked bad, and it stays bad: every read
        // after it fails too, so one momentary fault on the USB port ended the session and the readings
        // stopped until the user pressed Start again. The device itself is fine - opening it again and
        // carrying on works - so that is what is done here rather than giving up on it. Only if it will
        // not open again is the read reported as having failed, which is what an unplugged device does.
        if ((false == bStatus) && (true == bHadInterface))
        {
            // Build the interface again on the port it was opened on, and read once more
            bool bReopened = Initialize(m_iPortNum);
            if (true == bReopened)
            {
                bStatus = ReadBuffer();
            }
        }

        if (true == bStatus)
        {
            // Loop through the bytes read from the device
            INT64 iBitSum = 0;
            for (size_t iByteIndex = 0; iByteIndex < mc_iTRURNGPRO_BUFFER_SIZE; ++iByteIndex)
            {
                // Add up the value of each individual bit
                unsigned char& rCurrentByte = m_Buffer[iByteIndex];
                for (size_t iBitIndex = 0; iBitIndex < 8; ++iBitIndex)
                {
                    iBitSum += ((rCurrentByte >> iBitIndex) & 0x01);
                }
            }

            // The result is the average of all of the bits. Every reading is an average of the same number
            // of bits, whether it took one attempt or two, so a recovered read is not a different kind of
            // measurement from the ones around it.
            rfResult = (static_cast<double>(iBitSum) / (mc_iTRURNGPRO_BUFFER_SIZE * 8));
        }

        return bStatus;
    }
}
