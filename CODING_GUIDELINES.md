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
// Copyright (C) [Year] Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// [Date] - [Author] - [Description of changes]
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

### Return Values
- Use boolean return values for success/failure operations
- Return null for failed object creation
- Use out parameters for multiple return values

### Exception Handling
- Use try-catch blocks for external operations (file I/O, device access)
- Provide meaningful error messages
- Log errors appropriately

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