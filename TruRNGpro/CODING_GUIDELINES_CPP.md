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
//* Copyright (C) 2025 Mike Pullen. All Rights Reserved.
//* Confidential and Proprietary
//*
//* Revision History: 
//=====================================================================================================================
//* 2025/07/21 - [Author] - [Description of changes]
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
/// Brief description of the method/class and its purpose
/// </summary>
/// <param name="paramName">IN/OUT/INOUT - Parameter description and constraints</param>
/// <returns>Description of return value and what it represents</returns>
/// <exception cref="ExceptionType">Description of when this exception is thrown</exception>
/// <exception cref="std::invalid_argument">Thrown when parameters are invalid</exception>
/// <exception cref="std::runtime_error">Thrown when runtime conditions prevent operation</exception>
```

#### Exception Documentation Standards
All methods that can throw exceptions must document them using XML-style comments:

#### Standard Exception Documentation Template
```cpp
/// <summary>
/// Initializes the RNG device with the specified port number
/// </summary>
/// <param name="iPortNum">IN - Port number for device communication (must be positive)</param>
/// <param name="bSimulate">IN - Whether to use simulation mode instead of real device</param>
/// <returns>true if initialization successful; otherwise, false</returns>
/// <exception cref="std::invalid_argument">Thrown when iPortNum is negative or out of valid range</exception>
/// <exception cref="std::runtime_error">Thrown when device communication fails or device is not found</exception>
/// <exception cref="std::system_error">Thrown when underlying system calls fail</exception>
bool Initialize(int iPortNum, bool bSimulate);
```

#### Exception Documentation Best Practices
- **Complete Coverage**: Document ALL exceptions that can be explicitly thrown by the method
- **Standard Library Exceptions**: Use appropriate standard library exception types (`std::invalid_argument`, `std::runtime_error`, etc.)
- **Specific Conditions**: Clearly state the conditions under which each exception is thrown
- **User-Friendly Language**: Write descriptions that help developers understand how to avoid or handle the exception
- **Consistent Format**: Use consistent language patterns across all exception documentation
- **C++ Specific**: Consider exceptions from standard library functions and system calls

#### Common C++ Exception Types to Document
```cpp
// Parameter validation
/// <exception cref="std::invalid_argument">Thrown when parameter values are invalid</exception>
/// <exception cref="std::out_of_range">Thrown when array/container indices are out of bounds</exception>

// Resource and system errors  
/// <exception cref="std::runtime_error">Thrown when operation cannot be completed due to runtime conditions</exception>
/// <exception cref="std::system_error">Thrown when underlying system calls fail</exception>
/// <exception cref="std::bad_alloc">Thrown when memory allocation fails</exception>

// Logic and state errors
/// <exception cref="std::logic_error">Thrown when method is called in invalid object state</exception>
/// <exception cref="std::domain_error">Thrown when input is outside valid domain</exception>
```

#### Exception Documentation Examples
```cpp
// Good - specific and actionable
/// <exception cref="std::invalid_argument">Thrown when iPortNum is less than 0 or greater than 255</exception>

// Better - includes guidance
/// <exception cref="std::runtime_error">Thrown when device initialization fails. Ensure device is connected and drivers are installed</exception>

// Best - specific condition and user guidance
/// <exception cref="std::system_error">Thrown when COM port access fails, typically due to permissions or port already in use by another application</exception>
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

### Exception Handling Principles
- **Meaningful Messages**: Always provide descriptive error messages that help developers understand what went wrong
- **Standard Library Usage**: Prefer standard library exception types over custom exceptions
- **Exception Safety**: Provide appropriate exception safety guarantees (basic, strong, or no-throw)
- **Resource Management**: Use RAII to ensure proper cleanup even when exceptions occur
- **Performance Consideration**: Use exceptions for exceptional conditions, not normal control flow

#### Exception Throwing Guidelines
```cpp
// Good - descriptive message with actionable information
throw std::invalid_argument("Port number must be between 0 and 255, received: " + std::to_string(iPortNum));

// Good - preserving system error context
catch (const std::system_error& sysErr)
{
    throw std::runtime_error("Device initialization failed: " + sysErr.what());
}

// Good - using appropriate standard exception types
if (nullptr == pDevice)
{
    throw std::logic_error("Device must be initialized before calling GetBitAverage()");
}

// Bad - generic message without context
throw std::runtime_error("Error occurred");

// Bad - throwing exceptions for normal conditions
if (m_Buffer.empty())
{
    throw std::runtime_error("Buffer empty"); // Should return false instead
}
```

#### Exception Safety Guidelines
```cpp
// Strong exception safety - all or nothing
bool LoadConfiguration(const std::string& configFile)
{
    // Make a copy of current state
    auto backup = m_currentConfig;
    
    try
    {
        // Attempt to load new configuration
        auto newConfig = ParseConfigFile(configFile);
        m_currentConfig = std::move(newConfig);
        return true;
    }
    catch (...)
    {
        // Restore original state on any exception
        m_currentConfig = std::move(backup);
        throw; // Re-throw the exception
    }
}
```

#### Exception Documentation Requirements
- Document all explicitly thrown exceptions using `<exception cref="ExceptionType">description</exception>`
- Include when the exception is thrown and what causes it
- Provide actionable information for handling the exception
- Consider exceptions from called standard library functions
- Group related exceptions logically in documentation

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

### Well-Named Constants for Method Parameters
- Use descriptive, well-named constants for boolean and other parameters in method calls
- This improves code readability, maintainability, and self-documentation
- Constants should clearly indicate the purpose and meaning of the parameter

```cpp
// Incorrect - magic boolean literals
CreateConnection(true, false);
ProcessBuffer(false, true, 4096);

// Correct - use well-named constants
const bool bENABLE_SSL = true;
const bool bALLOW_RECONNECT = false;
CreateConnection(bENABLE_SSL, bALLOW_RECONNECT);

const bool bVALIDATE_CHECKSUM = false;
const bool bFORCE_PROCESSING = true;
const size_t iMAX_BUFFER_SIZE = 4096;
ProcessBuffer(bVALIDATE_CHECKSUM, bFORCE_PROCESSING, iMAX_BUFFER_SIZE);
```

#### Benefits of Well-Named Constants
- **Self-Documenting Code**: The constant name explains what the parameter does
- **Easier Debugging**: Breakpoints can be set on constant declarations
- **Reduced Errors**: Less likely to pass parameters in wrong order
- **Better Maintenance**: Changes to parameter values are centralized
- **Improved Readability**: Code reads like natural language
- **Compile-Time Optimization**: Compiler can optimize constant expressions

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
- [ ] Complete exception documentation using `<exception>` tags
- [ ] Exception messages are descriptive and actionable
- [ ] Exception types are appropriate for error conditions
- [ ] Exception safety guarantees are clearly defined
- [ ] RAII principles used for exception-safe resource management

### Exception Documentation Review
When reviewing exception documentation, verify:
- [ ] All explicitly thrown exceptions are documented
- [ ] Exception conditions are clearly described
- [ ] Exception messages provide actionable information
- [ ] Standard library exception types are used appropriately
- [ ] Exception safety level is appropriate for the method
- [ ] Resource cleanup occurs properly even when exceptions are thrown

### Static Analysis
- Address all compiler warnings
- Use appropriate access modifiers
- Follow const-correctness principles
- Validate pointer usage
- Ensure exception specifications are accurate and complete

---

*This document should be updated as the C++ codebase evolves and new patterns emerge.*

## Recent Updates
- Added comprehensive exception documentation standards and requirements
- Defined exception throwing guidelines and best practices  
- Established exception safety principles for C++14 development
- Updated code review checklist to include exception documentation verification