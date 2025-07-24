# Random Number Generator Project - Coding Guidelines

## Overview
This document defines the coding standards and conventions used throughout the Random Number Generator project. All new code should follow these guidelines to maintain consistency and quality.

## File Structure and Organization

### File Headers
All source files must include a standardized header:

```csharp
//*********************************************************************************************************************
// File Name:      [FileName].cs
// Description:    [Brief description of the file's purpose]
//
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2025/07/21 - [Author] - [Description of changes]
//*********************************************************************************************************************
```

### Using Statements
- Place using statements at the top of the file
- Group system namespaces first, then third-party, then project namespaces
- Use specific imports rather than wildcards when possible

```csharp
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RandomNumberGenerator;
```

### Namespace Organization
- Use the project name as the root namespace: `RandomNumberGenerator`
- Test projects use: `RandomNumberGenerator.Test`

## Naming Conventions

### Hungarian Notation (Modified)
The project uses a modified Hungarian notation system:

#### Member Variables (Private)
- `m_` prefix for member variables
- Type-specific prefixes after `m_`:
  - `m_s` for strings: `m_sFilePath`
  - `m_i` for integers: `m_iSessionSeconds`
  - `m_f` for floating point: `m_fCurrentAverage`
  - `m_b` for booleans: `m_bInProgress`
  - No additional prefix for objects: `m_Timer`, `m_Data`

#### Constants
- `m_s` prefix for string constants: `m_sIDLE_MESSAGE`
- `m_i` prefix for integer constants: `m_iMAX_DATA_SIZE`
- `m_f` prefix for float constants: `m_fYAXIS_INCREMENT`
- ALL_CAPS for compile-time constants: `WRITE_FILE_INTERVAL`

#### Local Variables
- `s` prefix for strings: `sFilePath`, `sTargetChangedMessage`
- `i` prefix for integers: `iPort`, `iTargetValue`
- `f` prefix for floating point: `fDataPoint`, `fResult`
- `b` prefix for booleans: `bStatus`, `bValid`

#### Parameters
- IN/OUT/INOUT prefixes in documentation
- Use descriptive names without Hungarian notation for parameters

### Class and Interface Names
- PascalCase for all class and interface names
- Interface names start with `I`: `IRNGSessionData`, `IGeneratorForm`
- Class names use descriptive nouns: `RNGSessionData`, `GeneratorForm`

### Method Names
- PascalCase for all method names
- Use verbs that describe the action: `StartSession()`, `WriteDataPoint()`
- Event handlers follow pattern: `[Control][Event]`: `StartButton_Click`

### Properties
- PascalCase for all properties
- Use descriptive nouns: `CurrentAverage`, `SessionTime`
- Boolean properties should be questions: `InProgress`, `Initialized`

## Code Structure and Layout

### Region Organization
Organize code using regions in this order:

```csharp
#region Type definitions
#endregion

#region Constructors  
#endregion

#region Event Handlers
#endregion

#region Methods
#endregion

#region Properties
#endregion

#region Constants
#endregion

#region Data Members
#endregion
```

### Method Structure
Follow this pattern for method organization:

```csharp
/// <summary>
/// Brief description of what the method does
/// </summary>
/// <param name="paramName">IN/OUT/INOUT - Parameter description</param>
/// <returns>Description of return value</returns>
public bool MethodName(int paramName)
{
    // Discard unused parameters (if applicable)
    _ = sender;
    _ = e;

    // Method implementation
    bool bStatus = true;
    
    return bStatus;
}
```

## Documentation Standards

### XML Documentation
- Use XML documentation for all public members
- Include `<summary>`, `<param>`, and `<returns>` tags
- Parameter directions: `IN`, `OUT`, `INOUT`
- Document all explicitly thrown exceptions using `<exception>` tags

#### Standard XML Documentation Template
```csharp
/// <summary>
/// Brief description of what the method does and its purpose
/// </summary>
/// <param name="parameterName">IN/OUT/INOUT - Description of the parameter and its constraints</param>
/// <param name="anotherParameter">IN - Description of another parameter</param>
/// <returns>Description of the return value and what it represents</returns>
/// <exception cref="ArgumentNullException">Thrown when parameterName is null</exception>
/// <exception cref="ArgumentException">Thrown when parameterName contains invalid characters</exception>
/// <exception cref="InvalidOperationException">Thrown when the object is not in a valid state for this operation</exception>
/// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
/// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
public bool ExampleMethod(string parameterName, int anotherParameter)
{
    // Implementation
}
```

#### Exception Documentation Best Practices
- **Complete Coverage**: Document ALL exceptions that can be explicitly thrown by the method
- **Specific Conditions**: Clearly state the conditions under which each exception is thrown
- **User-Friendly Language**: Write descriptions that help developers understand how to avoid or handle the exception
- **Consistent Format**: Use consistent language patterns across all exception documentation
- **Inheritance Awareness**: Consider exceptions thrown by base class methods or interface implementations

#### Exception Documentation Examples
```csharp
// Good - specific and actionable
/// <exception cref="ArgumentNullException">Thrown when the dataPoint parameter is null</exception>
/// <exception cref="InvalidOperationException">Thrown when no file path is set or XML writer is in an invalid state</exception>

// Better - includes guidance
/// <exception cref="UnauthorizedAccessException">Thrown when file access is denied. Check file permissions and ensure the file is not open in another application</exception>

// Best - specific condition and user guidance
/// <exception cref="System.IO.IOException">Thrown when file I/O operations fail, such as when the disk is full or the file is corrupted</exception>
```

#### Exception Documentation
All methods that explicitly throw exceptions must document them:

```csharp
/// <summary>
/// Opens or creates a file for writing
/// </summary>
/// <param name="filePath">IN - Path to the file to create or open</param>
/// <returns>true if successful; otherwise, false</returns>
/// <exception cref="ArgumentNullException">Thrown when filePath parameter is null or empty</exception>
/// <exception cref="UnauthorizedAccessException">Thrown when file access is denied</exception>
/// <exception cref="System.IO.IOException">Thrown when file I/O operations fail</exception>
/// <exception cref="InvalidOperationException">Thrown when the operation cannot be completed due to object state</exception>
public bool OpenFile(string filePath)
{
    // Implementation
}
```

### Comments
- Use `//` for single-line comments
- Use `/* */` for multi-line comments
- Add comments for TODO items: `//!!! author - description !!!`
- Add section headers for complex logic

## Error Handling and Validation

### Parameter Validation
Always validate parameters in public methods:

```csharp
public void MethodName(object parameter)
{
    if (null == parameter)
    {
        throw new ArgumentNullException("Specified parameter cannot be null");
    }
}
```

### Exception Handling Principles
- **Meaningful Messages**: Always provide descriptive error messages that help users understand what went wrong
- **Exception Propagation**: Re-throw exceptions to bubble errors up to the GUI layer for user feedback
- **Context Preservation**: When wrapping exceptions, preserve the original exception as the inner exception
- **Avoid Silent Failures**: Never catch exceptions and return false without proper error reporting

#### Exception Throwing Guidelines
```csharp
// Good - descriptive message with user-actionable information
throw new InvalidOperationException(" No file selected. Please select a data file before starting a session.");

// Good - preserving original exception context
catch (IOException ioEx)
{
    throw new IOException($" File I/O error accessing '{fileName}': {ioEx.Message}", ioEx);
}

// Bad - generic message without context
throw new Exception("Error occurred");

// Bad - silently returning false
catch (Exception)
{
    return false; // Don't do this - exceptions should bubble up
}
```

#### Exception Documentation Requirements
- Document all explicitly thrown exceptions using `<exception cref="ExceptionType">description</exception>`
- Include when the exception is thrown and what causes it
- Provide actionable information for handling the exception
- Group related exceptions logically in documentation

### Return Values
- Use boolean return values for success/failure operations
- Return null for failed object creation
- Use out parameters for multiple return values
- Throw exceptions for error conditions that should be reported to the user

### Exception Handling
- Use try-catch blocks for external operations (file I/O, device access)
- Provide meaningful error messages that will be displayed to users
- Re-throw exceptions to propagate errors to the GUI layer
- Use specific exception types rather than generic Exception when possible

### Function Calls in Conditionals
- Do not make function calls inside conditional statements
- Extract function calls to separate variables before using in conditionals
- This improves debugging, readability, and allows for easier breakpoint placement

```csharp
// Incorrect - function call inside conditional
if (m_Reader.ReadToFollowing("Session"))
{
    // Process session
}

// Correct - extract function call
bool bSessionFound = m_Reader.ReadToFollowing("Session");
if (bSessionFound)
{
    // Process session
}
```

### Well-Named Constants for Method Parameters
- Use descriptive, well-named constants for boolean and other parameters in method calls
- This improves code readability, maintainability, and self-documentation
- Constants should clearly indicate the purpose and meaning of the parameter

```csharp
// Incorrect - magic boolean literals
CreateWriter(true, false);
ProcessData(false, true, 1024);

// Correct - use well-named constants
const bool bAPPEND_MODE = true;
const bool bRECREATE = false;
CreateWriter(bAPPEND_MODE, bRECREATE);

const bool bVALIDATE_INPUT = false;
const bool bFORCE_PROCESSING = true;
const int iMAX_BUFFER_SIZE = 1024;
ProcessData(bVALIDATE_INPUT, bFORCE_PROCESSING, iMAX_BUFFER_SIZE);
```

#### Benefits of Well-Named Constants
- **Self-Documenting Code**: The constant name explains what the parameter does
- **Easier Debugging**: Breakpoints can be set on constant declarations
- **Reduced Errors**: Less likely to pass parameters in wrong order
- **Better Maintenance**: Changes to parameter values are centralized
- **Improved Readability**: Code reads like natural language

## Threading and Concurrency

### Thread Safety
- Use `lock` statements for thread-safe operations
- Use `Interlocked` for simple atomic operations
- Use `ConcurrentQueue<T>` for thread-safe collections

```csharp
// Thread-safe property access
lock (m_DataLock)
{
    // Critical section
}
```

### UI Thread Operations
- Use `Invoke()` for UI updates from background threads
- Check `InvokeRequired` before UI operations

```csharp
if (m_Parent.InvokeRequired)
{
    m_Parent.Invoke(new Action(() => UpdateUI()));
}
else
{
    UpdateUI();
}
```

## Resource Management

### IDisposable Implementation
- Implement IDisposable for classes managing resources
- Provide both finalizer and Dispose methods when needed
- Use `using` statements for disposable objects

### Memory Management
- Dispose of timers, file handles, and other resources
- Avoid memory leaks by properly cleaning up event handlers

## Testing Standards

### Unit Test Organization
Use the same region structure as production code:

```csharp
#region Infrastructure
#endregion

#region Data Members  
#endregion

#region Additional test attributes
#endregion

#region Initialization and cleanup
#endregion

#region Tests
#endregion
```

### Test Naming
- Test class names: `[ClassName]Tests`
- Test method names: `[Method]_[Scenario]_[ExpectedResult]`
- Use `[TestCategory("Component")]` for categorization

### Test Structure
Follow the Arrange-Act-Assert pattern:

```csharp
[TestMethod]
public void Method_Scenario_ExpectedResult()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    //**************************************************************//
    // Assert
    //**************************************************************//
}
```

## Formatting and Style

### Indentation and Spacing
- Use tabs for indentation (configured as 4 spaces)
- Place opening braces on new lines
- Use consistent spacing around operators

### Line Length
- Prefer lines under 120 characters
- Break long parameter lists across multiple lines
- Align continuation lines appropriately

### Code Organization
- Group related functionality together
- Separate logical sections with blank lines
- Keep methods focused and concise

## Interface Design

### Interface Definitions
- Keep interfaces focused and cohesive
- Use read-only properties where appropriate
- Provide clear contracts through documentation

### Implementation Guidelines
- Implement interfaces explicitly when needed
- Provide meaningful default values
- Validate interface contracts in implementations

## Performance Considerations

### Efficiency Guidelines
- Use appropriate data structures (`ConcurrentQueue<T>` for thread-safe collections)
- Minimize object creation in tight loops
- Cache frequently accessed properties

### Resource Usage
- Set appropriate thread pool limits
- Use efficient string operations (avoid concatenation in loops)
- Dispose of resources promptly

## Project-Specific Guidelines

### GUI Components
- Use descriptive names for controls: `m_StartButton`, `m_StatusTextBox`
- Implement proper event handling patterns
- Maintain UI responsiveness with background operations

### Device Integration
- Abstract hardware dependencies through interfaces
- Provide simulation modes for testing
- Handle device connection/disconnection gracefully

### Data Management
- Use thread-safe collections for concurrent access
- Implement proper validation for user inputs
- Provide clear error messages for validation failures

## Compliance and Quality

### Code Analysis
- Address all compiler warnings
- Use appropriate access modifiers
- Follow SOLID principles where applicable

### Review Checklist
Before submitting code, verify:
- [ ] Proper file header and documentation
- [ ] Consistent naming conventions
- [ ] Appropriate error handling
- [ ] Thread safety considerations
- [ ] Resource disposal
- [ ] Unit test coverage
- [ ] Performance implications

---

*This document should be updated as the project evolves and new patterns emerge.*