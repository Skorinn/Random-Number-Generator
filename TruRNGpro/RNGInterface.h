//*********************************************************************************************************************
//* File Name:      RNGInterface.h
//* Description:    Interface to random number generator peripherals
//*
//* Copyright (c) 2023 Mike Pullen
//* Licensed under the MIT License. See LICENSE in the repository root.
//*
//* Revision History: 
//=====================================================================================================================
//* 2023/12/01 - Mike Pullen - Original implementation.
//*********************************************************************************************************************
#pragma once

namespace RNGInterfaces
{
    /// <summary>
    /// Interface to random number generator peripherals
    /// </summary>
    class RNGInterface
    {
    public:
        RNGInterface() {}
        virtual ~RNGInterface() {}

        virtual bool Initialize(unsigned int iPortNum) = 0;
        virtual bool GetBitAverage(double& rResult) = 0;
    };
}