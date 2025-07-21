# TruRNGpro C++ Component - Coding Guidelines

## Overview
This document defines the coding standards and conventions used for the TruRNGpro C++ component of the Random Number Generator project. All C++ code should follow these guidelines to maintain consistency with the existing codebase.

## Project Configuration

### C++ Standard
- **Target Standard**: C++14
- **Platform**: Windows (Visual Studio 2022)
- **Architecture Support**: x86, x64, ARM64

### Compiler Settings
- Use precompiled headers (pch.h/pch.cpp)
- Enable all reasonable warnings
- Treat warnings as errors where appropriate

## File Structure and Organization

### File Headers
All C++ source files must include a standardized header:

```cpp
//*********************************************************************************************************************
//* File Name:      [FileName].h/.cpp
//* Description:    [Brief description of the file's purpose]
//*
//* Copyright (C) [Year] Mike Pullen. All Rights Reserved.
//* Confidential and Proprietary
//*
//* Revision History: 
//=====================================================================================================================
//* [Date] - [Author] - [Description of changes]
//*********************************************************************************************************************
```

### Include Organization
Follow this order for includes:

```cpp
#include "pch.h"              // Precompiled header (first)
#include "LocalHeaders.h"     // Project-specific headers
#include <StandardLibrary>    // Standard library headers
#include <windows.h>          // System headers (if needed)
```

### Header Guards
Use `#pragma once` at the beginning of all header files:

```cpp
#pragma once
```

## Naming Conventions

### Hungarian Notation (Modified for C++)
The project uses a modified Hungarian notation system consistent with the C# components:

#### Member Variables (Private)
- `m_` prefix for member variables
- Type-specific prefixes after `m_`:
  - `m_i` for integers: `m_iDEFAULT_SEED`
  - `m_p` for pointers: `m_pTruRNGProInterface`
  - `m_b` for booleans: `m_bInitialized`
  - Arrays use descriptive names: `m_Buffer`, `m_RandomNumber`

#### Constants
- `mc_i` prefix for member constants: `mc_iTRURNGPRO_BUFFER_SIZE`
- `m_i` prefix for static constants: `m_iDEFAULT_SEED`
- Use descriptive ALL_CAPS for global constants

#### Local Variables
- `i` prefix for integers: `iPort`, `iByteIndex`, `iBytesRead`
- `b` prefix for booleans: `bStatus`, `bSimulate`
- `p` prefix for pointers: `pInterface`
- `r` prefix for references: `rfResult`, `rCurrentByte`

#### Parameters
- Use Hungarian notation with descriptive names
- Reference parameters use `r` prefix: `rfResult`
- Pointer parameters use `p` prefix if applicable
- Document direction in comments: `IN`, `OUT`, `INOUT`

### Class and Interface Names
- PascalCase for all class names
- Use descriptive nouns: `TruRNGpro`, `RNGSimulator`, `RNGInterface`
- Interface classes should clearly indicate their purpose

### Method Names
- PascalCase for all method names
- Use verbs that describe the action: `Initialize()`, `GetBitAverage()`
- Virtual methods should be clearly marked

### Namespace Organization
- Use the project namespace: `RNGInterfaces`
- Keep namespaces focused and meaningful

## Code Structure and Layout

### Class Organization
Organize class members in this order:

```cpp
class ClassName
{
public:
    // Type definitions (typedefs, enums)
    
    // Constructors (default first, then parameterized)
    
    // Destructor
    
    // Public methods (virtual first, then non-virtual)

private:
    // Private methods
    
    // Static constants
    
    // Member variables
};
```

### Method Implementation
#### Header File Implementation
For template or simple methods, implement in header:

```cpp
/// <summary>
/// Brief description of what the method does
/// </summary>
/// <param name="paramName">IN/OUT/INOUT - Parameter description</param>
/// <returns>Description of return value</returns>
ReturnType MethodName(ParameterType paramName)
{
    // Method implementation
    return value;
}
```

#### Inline Implementation Style
Methods implemented in headers should be clear and concise:

```cpp
/// <summary>
/// Default constructor
/// </summary>
RNGSimulator::RNGSimulator() : m_RandomNumber(m_iDEFAULT_SEED)
{
    // Nothing to do
}
```

## Documentation Standards

### XML-Style Documentation
Use XML-style comments consistent with C# components:

```cpp
/// <summary>
/// Brief description of the method/class
/// </summary>
/// <param name="paramName">IN/OUT/INOUT - Parameter description</param>
/// <returns>Description of return value</returns>
```

### Parameter Direction Indicators
- `IN` - Input parameter (read-only)
- `OUT` - Output parameter (write-only) 
- `INOUT` - Input/output parameter (read/write)

### Comments
- Use `//` for single-line comments
- Use `/* */` for multi-line comments  
- Add descriptive comments for complex algorithms
- Explain the "why" not just the "what"

## Memory Management

### Resource Management
- Use RAII principles where possible
- Explicitly manage dynamic memory allocation
- Always pair `new` with `delete`
- Set pointers to `nullptr` after deletion

```cpp
if (nullptr != m_pTruRNGProInterface)
{
    delete m_pTruRNGProInterface;
    m_pTruRNGProInterface = nullptr;
}
```

### Pointer Safety
- Initialize pointers to `nullptr`
- Check for `nullptr` before dereferencing
- Use smart pointers when appropriate for C++14

### Buffer Management
- Use fixed-size arrays for performance-critical code
- Initialize arrays with `{}` syntax: `m_Buffer{}`
- Validate buffer bounds before access

## Error Handling

### Return Value Conventions
- Use `bool` return types for success/failure operations
- `true` indicates success, `false` indicates failure
- Use reference parameters for output values

```cpp
bool GetBitAverage(double& rfResult)
{
    bool bStatus = true;
    // Implementation
    return bStatus;
}
```

### Error Checking Patterns
Use consistent error checking patterns:

```cpp
bool bStatus = (nullptr != m_pInterface);
if (true == bStatus)
{
    // Continue processing
    bStatus = m_pInterface->SomeOperation();
}

if (true == bStatus)
{
    // Next operation
}

return bStatus;
```

### Exception Handling
- Minimize exception usage in performance-critical code
- Use exceptions for truly exceptional conditions
- Provide strong exception safety guarantees

### Function Calls in Conditionals
- Do not make function calls inside conditional statements
- Extract function calls to separate variables before using in conditionals
- This improves debugging, readability, and allows for easier breakpoint placement

```cpp
// Incorrect - function call inside conditional
if (device.Initialize())
{
    // Process device
}

// Correct - extract function call
bool bInitialized = device.Initialize();
if (bInitialized)
{
    // Process device
}
```

## Interface Design

### Abstract Base Classes
Define clear interface contracts:

```cpp
class RNGInterface
{
public:
    RNGInterface() {}
    virtual ~RNGInterface() {}

    virtual bool Initialize(unsigned int iPortNum) = 0;
    virtual bool GetBitAverage(double& rResult) = 0;
};
```

### Virtual Method Guidelines
- Mark destructors as `virtual` in base classes
- Use `virtual` keyword consistently
- Consider `override` keyword for derived classes (C++11+)

### Implementation Classes
- Inherit publicly from interfaces
- Implement all pure virtual methods
- Provide meaningful default behavior where appropriate

## Performance Considerations

### Efficiency Guidelines
- Use appropriate data types for the context
- Prefer stack allocation over heap allocation
- Use `const` and `constexpr` where appropriate
- Minimize dynamic memory allocation in hot paths

### Loop Optimization
```cpp
// Prefer range-based iteration limits
for (size_t iByteIndex = 0; iByteIndex < mc_iTRURNGPRO_BUFFER_SIZE; ++iByteIndex)
{
    // Process buffer[iByteIndex]
}
```

### Type Usage
- Use `size_t` for array indices and sizes
- Use `unsigned int` for port numbers and counts
- Use `INT64` for large numeric calculations
- Use `double` for floating-point results

## Platform-Specific Code

### Windows API Integration
- Use Windows types appropriately: `BOOL`, `DWORD`, `HMODULE`
- Handle Windows API return codes properly
- Use appropriate calling conventions: `APIENTRY`, `__declspec(dllexport)`

### DLL Development
```cpp
extern "C" __declspec(dllexport) bool FunctionName(parameters)
{
    // Implementation
    return result;
}
```

### Threading Considerations
- Design for single-threaded access unless specified
- Use appropriate synchronization if threading is required
- Consider thread safety for global/static variables

## Modern C++14 Features

### Recommended C++14 Features
- Use `constexpr` for compile-time constants
- Use `auto` for complex type deduction
- Use range-based for loops where appropriate
- Use `nullptr` instead of `NULL`

```cpp
// Good: Modern C++14 style
constexpr size_t iNUM_SAMPLES = 4094;
auto result = static_cast<RNGType::result_type>(iPortNum);
```

### Type Safety
- Use `static_cast` for explicit conversions
- Avoid C-style casts
- Use `const` wherever possible
- Prefer strong typing over void pointers

## Project-Specific Guidelines

### Device Interface Pattern
- All device interfaces inherit from `RNGInterface`
- Provide both device and simulator implementations
- Use factory pattern for object creation
- Handle device initialization gracefully

### Simulation Support
- Provide simulation modes for testing
- Use deterministic algorithms for reproducible results
- Seed random number generators appropriately

```cpp
// Simulator constructor with seeding
RNGSimulator::RNGSimulator(unsigned int iPortNum) 
    : m_RandomNumber(static_cast<RNGType::result_type>(iPortNum))
{
    // Nothing to do
}
```

### Buffer Operations
- Use fixed-size buffers for performance
- Validate buffer operations
- Handle partial reads appropriately

## Formatting and Style

### Indentation and Spacing
- Use tabs for indentation (configured as 4 spaces)
- Place opening braces on new lines for functions and classes
- Use consistent spacing around operators

### Line Length
- Prefer lines under 120 characters
- Break long parameter lists across multiple lines
- Align continuation lines appropriately

### Code Organization
- Group related functionality together
- Separate logical sections with blank lines
- Keep methods focused and concise

## Build and Deployment

### Precompiled Headers
- Use `pch.h` for common includes
- Include `pch.h` first in all `.cpp` files
- Keep PCH headers stable and commonly used

### Export Specifications
- Use `extern "C"` for C-style exports
- Use `__declspec(dllexport)` for Windows DLL exports
- Provide clear function signatures for external interfaces

## Quality and Compliance

### Code Review Checklist
Before submitting C++ code, verify:
- [ ] Proper file header and documentation
- [ ] Consistent naming conventions
- [ ] Appropriate memory management
- [ ] Proper error handling
- [ ] Resource cleanup in destructors
- [ ] Thread safety considerations
- [ ] Performance implications
- [ ] Platform compatibility

### Static Analysis
- Address all compiler warnings
- Use appropriate access modifiers
- Follow const-correctness principles
- Validate pointer usage

---

*This document should be updated as the C++ codebase evolves and new patterns emerge.*