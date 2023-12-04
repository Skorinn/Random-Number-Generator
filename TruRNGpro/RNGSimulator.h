//*********************************************************************************************************************
//* File Name:      RNGSimulator.h
//* Description:    Simulator for the RNGSimulator device
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
#include "RNGInterface.h"

#include <random>

namespace RNGInterfaces
{
    /// <summary>
    /// Interface to the RNGSimulator 
    /// </summary>
    class RNGSimulator : public RNGInterface
    {
    public:
        typedef std::mt19937 RNGType;

    public:
        RNGSimulator();
        RNGSimulator(unsigned int iPortNum);
        virtual ~RNGSimulator() {}; // Destructor does nothing

        virtual bool Initialize(unsigned int iPortNum);
        virtual bool GetBitAverage(double& rResult);

    private:
        static const RNGType::result_type m_iDEFAULT_SEED = 8709648; // Default seed to create the generator
        RNGType m_RandomNumber;  // to seed mersenne twister.
    };

    /// <summary>
    /// Default constructor
    /// </summary>
    RNGSimulator::RNGSimulator() : m_RandomNumber(m_iDEFAULT_SEED)
    {
        // Nothing to do
    }

    /// <summary>
    /// Constructor that seeds the generator with the number specified
    /// </summary>
    RNGSimulator::RNGSimulator(unsigned int iPortNum) : m_RandomNumber(static_cast<RNGType::result_type>(iPortNum))
    {
        // Nothing to do
    }

    /// <summary>
    /// Seeds the generator with the number specified
    /// </summary>
    /// <param name="iPortNum">IN - COM port the device is connected through</param>
    /// <returns>true, if the device is ready to use, or false, if there is an error</returns>
    bool RNGSimulator::Initialize(unsigned int iPortNum)
    {
        // Reseed the generator with the specified number
        m_RandomNumber.seed(static_cast<RNGType::result_type>(iPortNum));

        // Always successful
        return true;
    }

    /// <summary>
    /// Gets a bit average using the built-in pseudo-random number generator
    /// </summary>
    /// <param name="rfResult">OUT - Average of the bits read</param>
    /// <returns>true - if successful, otherwise false</returns>
    bool RNGSimulator::GetBitAverage(double& rfResult)
    {
        // Default the return value
        bool bStatus = true;

        // The number of samples to average
        constexpr size_t iNUM_SAMPLES = 4094;

        // Get the samples
        rfResult = 0;
        for (size_t iSample = 0; iNUM_SAMPLES > iSample; ++iSample)
        {
            rfResult += static_cast<double>(m_RandomNumber() % 2);
        }
        rfResult /= iNUM_SAMPLES;

        return bStatus;
    }
}
