# Random Number Generator Unit Test Project - Coding Guidelines

## Overview
This document defines the coding standards and conventions used for the unit test project of the Random Number Generator solution. All test code should follow these guidelines to maintain consistency, reliability, and maintainability.

## Project Configuration

### Test Framework
- **Framework**: MSTest (.NET Framework 4.8)
- **Mocking Framework**: Moq 4.20.70
- **Testing Categories**: Component, Integration
- **Target Platform**: .NET Framework 4.8

### Required NuGet Packages
```xml
<package id="MSTest.TestFramework" version="2.2.10" />
<package id="MSTest.TestAdapter" version="2.2.10" />
<package id="Moq" version="4.20.70" />
<package id="Castle.Core" version="5.1.1" />
```

## File Structure and Organization

### File Headers
All test files must include the standardized header:

```csharp
//*********************************************************************************************************************
// File Name:      [ClassName].Test.cs
// Description:    Unit tests for the [ClassName] class
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
Follow this order for test file imports:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Drawing;
// Additional framework imports as needed
```

### Namespace Organization
- Use the test project namespace: `RandomNumberGenerator.Test`
- Test class names follow pattern: `[ClassName]Tests`

## Test Class Structure

### Region Organization
Organize test classes using regions in this order:

```csharp
[TestClass]
public class ClassNameTests
{
    #region Infrastructure
    // Default constructor
    // TestContext property
    // Helper methods for tests
    #endregion
    
    #region Data Members
    // Test constants
    // Private fields for test data
    // TestContext instance
    #endregion
    
    #region Additional test attributes
    // Commented examples of MSTest attributes
    #endregion
    
    #region Initialization and cleanup
    // ClassInitialize, ClassCleanup methods
    // TestInitialize, TestCleanup methods
    #endregion
    
    #region Tests
    // All test methods
    #endregion
}
```

### Test Class Declaration
```csharp
/// <summary>
/// Unit tests for the [ClassName] class
/// </summary>
[TestClass]
public class ClassNameTests
{
    // Test implementation
}
```

## Naming Conventions

### Test Class Names
- Pattern: `[ClassName]Tests`
- Examples: `XMLDataPointTests`, `RNGDeviceTimerTests`, `GeneratorFormTests`

### Test Method Names
Use the pattern: `[MethodName]_[Scenario]_[ExpectedResult]`

```csharp
// Examples:
Constructor_Default_Properties()
WriteDataPoint_ValidWriter_Success()
WriteDataPoint_NullWriter_Failure()
InitializeDevice_Valid_ReturnsTrue()
FileBrowseActive_SetProperty_True()
```

### Test Data Members
Follow the same Hungarian notation as production code:

```csharp
// Test constants
private const string m_sTEST_FILE_PATH = "TestFile.xml";
private const string m_sSESSION_DATA_TIME = "01:54:21";
private const double m_fDATA_POINT = 9.1;
private const string sEXPECTED_DATA_POINT_ENTRY = "<Data>...</Data>";

// Test counters and tracking
private uint m_iReadCallbackCalls = 0;
private uint m_iInvalidCallbackCalls = 0;
```

### Expected Values
Use descriptive constants for expected values:

```csharp
// Expected values in arrange section
const string sEXPECTED_TIME = "";
const double fEXPECTED_DATA = 0.0;
const bool bEXPECTED_VALID = true;
const Color expectedTextColor = Color.MediumPurple;
```

## Test Structure and Implementation

### Arrange-Act-Assert Pattern
All tests must follow the AAA pattern with clear section markers:

```csharp
[TestMethod]
[TestCategory("Component")]
public void Method_Scenario_ExpectedResult()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    // Setup test data, mocks, and expected values
    const string sEXPECTED_VALUE = "expected";
    var mockObject = new Mock<IInterface>();
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    // Execute the method under test
    var result = objectUnderTest.MethodToTest();
    
    //**************************************************************//
    // Assert
    //**************************************************************//
    
    // Verify the results
    Assert.AreEqual(sEXPECTED_VALUE, result);
}
```

### Test Method Documentation
```csharp
/// <summary>
/// Tests [specific behavior being tested]
/// </summary>
[TestMethod]
[TestCategory("Component")]
public void TestMethodName()
{
    // Implementation
}
```

## Test Categories and Attributes

### Test Categories
Use consistent test categorization:

```csharp
[TestCategory("Component")]    // Unit tests for component behavior
[TestCategory("Integration")]  // Integration tests with external dependencies
```

### Exception Testing
```csharp
/// <summary>
/// Tests the constructor generates an exception when the parameter is null
/// </summary>
[TestMethod]
[TestCategory("Component")]
[ExpectedException(typeof(System.ArgumentNullException))]
public void Constructor_NullParameter_Exception()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    // Create object that should throw exception
    var objectUnderTest = new ClassUnderTest(null);
    
    //**************************************************************//
    // Assert
    //**************************************************************//
    
    // Expecting an exception (handled by attribute)
}
```

## Mocking with Moq

### Mock Object Creation
```csharp
// Create mock objects
var mockInterface = new Mock<IInterface>();
Mock<IRNGSessionData> mockSessionData = new Mock<IRNGSessionData>();
```

### Mock Setup Patterns
```csharp
// Property setup
mockObject.Setup(mock => mock.Property).Returns(expectedValue);
mockObject.SetupProperty(mock => mock.Property, initialValue);
mockObject.SetupAllProperties();

// Method setup with parameters
mockObject.Setup(mock => mock.Method(It.IsAny<string>())).Returns(true);
mockObject.Setup(mock => mock.Method(specificValue)).Returns(result);

// Verifiable setup for later verification
mockObject.Setup(mock => mock.Method(It.IsAny<double>())).Returns(true).Verifiable();
```

### Mock Verification
```csharp
// Verify method was called
mockObject.Verify(mock => mock.Method(expectedValue), Times.Once);
mockObject.Verify(mock => mock.Method(It.IsAny<double>()), Times.Never);

// Verify all verifiable setups
mockObject.VerifyAll();
```

## Assertion Patterns

### Standard Assertions
```csharp
// Equality assertions
Assert.AreEqual(expected, actual);
Assert.AreNotEqual(unexpected, actual);

// Boolean assertions
Assert.IsTrue(condition);
Assert.IsFalse(condition);

// Null assertions
Assert.IsNull(value);
Assert.IsNotNull(value);
```

### String Assertions
```csharp
// String content verification
StringAssert.Contains(actualString, expectedSubstring);
StringAssert.StartsWith(actualString, expectedPrefix);
StringAssert.EndsWith(actualString, expectedSuffix);
```

### Custom Assertion Messages
```csharp
Assert.IsTrue(condition, "Custom message explaining the failure");
Assert.AreEqual(expected, actual, "Values should be equal for reason X");
```

## Test Data Management

### Test Constants
Define test data as constants at the class level:

```csharp
#region Data Members

// Test file paths
private const string m_sTEST_FILE_PATH = "TestSessionDataFile.XMLDataPointTests.xml";

// Test data values
private const string m_sSESSION_DATA_TIME = "01:54:21";
private const double m_fDATA_POINT = 9.1;

// Expected results
private const string sEXPECTED_DATA_POINT_ENTRY = "<Data Time=\"01:54:21\">9.1</Data>";

#endregion
```

### TestContext Usage
```csharp
private TestContext testContextInstance;

/// <summary>
/// Gets or sets the test context which provides
/// information about and functionality for the current test run.
/// </summary>
public TestContext TestContext
{
    get => testContextInstance;
    set => testContextInstance = value;
}
```

## Test Lifecycle Management

### Test Cleanup
```csharp
/// <summary>
/// Cleans up after each test runs to ensure we always start with a clean state
/// </summary>
[TestCleanup]
public void Cleanup()
{
    // Delete test files if created
    if (File.Exists(m_sTEST_FILE_PATH))
    {
        File.Delete(m_sTEST_FILE_PATH);
    }
}
```

### Class-Level Cleanup
```csharp
/// <summary>
/// Cleans up after all tests in the class run
/// </summary>
[ClassCleanup]
public static void ClassCleanup()
{
    // Clean up shared resources
    if (File.Exists(m_sTEST_FILE_PATH))
    {
        File.Delete(m_sTEST_FILE_PATH);
    }
}
```

## Threading and Callback Testing

### Thread-Safe Test Patterns
```csharp
/// <summary>
/// Read result callback function to use for testing
/// </summary>
public void RecordReadResult(double fResult)
{
    // Protect as this is executed from a thread
    lock (this)
    {
        // Validate the result and increment the call counter
        if ((0 > fResult) || (1 < fResult))
        {
            ++m_iInvalidCallbackCalls;
        }
        ++m_iReadCallbackCalls;
    }
}
```

### Callback Verification
```csharp
// Set the callback and simulate execution
timer.SetReadCallback(RecordReadResult);
timer.Start();
for (uint iTickCount = 0; iNUM_TICKS > iTickCount; ++iTickCount)
{
    timer.TriggerTick();
}
timer.Stop();

// Verify the callback was executed correctly
bool bCallbackExecuted = (iNUM_TICKS == m_iReadCallbackCalls);
Assert.IsTrue(bCallbackExecuted);
```

## Error Handling in Tests

### Exception Testing
```csharp
// Use ExpectedException attribute for expected exceptions
[ExpectedException(typeof(ArgumentNullException))]

// For more complex exception testing, use try-catch
try
{
    // Action that should throw
    objectUnderTest.MethodThatShouldThrow();
    Assert.Fail("Expected exception was not thrown");
}
catch (ExpectedException)
{
    // Expected exception was thrown
}
```

### Validation Testing
```csharp
// Test both valid and invalid inputs
[TestMethod]
public void Method_ValidInput_Success() { /* ... */ }

[TestMethod]
public void Method_InvalidInput_Failure() { /* ... */ }
```

## File and Resource Testing

### File System Testing
```csharp
/// <summary>
/// Tests file operations with proper cleanup
/// </summary>
[TestMethod]
public void WriteFile_ValidPath_Success()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    // Create expected file content
    string sExpectedContent = "expected content";
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    // Perform file operation
    objectUnderTest.WriteToFile(m_sTEST_FILE_PATH, sExpectedContent);
    
    //**************************************************************//
    // Assert
    //**************************************************************//
    
    // Verify file was created and contains expected content
    Assert.IsTrue(File.Exists(m_sTEST_FILE_PATH));
    string actualContent = File.ReadAllText(m_sTEST_FILE_PATH);
    StringAssert.Contains(actualContent, sExpectedContent);
}
```

## Integration Testing Patterns

### Device Simulation Testing
```csharp
[TestMethod]
[TestCategory("Integration")]
public void InitializeDevice_Simulator_Success()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    const int iSEED = 0;
    const bool bSIMULATE = true;
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    bool bResult = timer.InitializeDevice(iSEED, bSIMULATE);
    
    //**************************************************************//
    // Assert
    //**************************************************************//
    
    Assert.IsTrue(bResult);
    Assert.IsTrue(timer.Initialized);
}
```

## Performance and Reliability

### Test Data Volumes
```csharp
// Use meaningful test data volumes
const uint iNUM_TICKS = 100;
const int iVALID_INTERVAL = 978;
const size_t iNUM_SAMPLES = 4094;
```

### Test Isolation
- Each test should be independent
- Clean up resources after each test
- Reset state between tests
- Use fresh mock objects for each test

### Test Reliability
```csharp
// Verify multiple aspects of behavior
Assert.IsTrue(bCallbackExecuted);
Assert.IsFalse(bInvalidResult);
Assert.AreEqual(expectedCount, actualCount);
```

## Documentation and Comments

### Test Method Documentation
```csharp
/// <summary>
/// Tests [specific behavior] when [condition] to ensure [expected outcome]
/// </summary>
```

### Inline Comments
```csharp
// Create the object under test
var objectUnderTest = new ClassUnderTest();

// Execute the method being tested
var result = objectUnderTest.Method();

// Verify the expected behavior occurred
Assert.IsTrue(result);
```

## Quality Guidelines

### Test Coverage
- Test all public methods and properties
- Test both success and failure paths
- Test edge cases and boundary conditions
- Test exception handling

### Test Maintenance
- Keep tests simple and focused
- Use descriptive test names
- Avoid test interdependencies
- Update tests when production code changes

### Code Review Checklist
Before submitting test code, verify:
- [ ] Proper file header and documentation
- [ ] Consistent naming conventions
- [ ] AAA pattern implementation
- [ ] Appropriate test categories
- [ ] Mock setup and verification
- [ ] Resource cleanup
- [ ] Test isolation
- [ ] Edge case coverage

## Build Integration

### Post-Build Events
The test project includes post-build events to copy dependencies:

```xml
<PostBuildEvent>
xcopy /Y "$(SolutionDir)bin\TruRNGpro.dll" "$(TargetDir)"
if errorlevel  1 goto end
echo Copied to "$(TargetDir)"
:end
</PostBuildEvent>
```

### Test Execution
- Tests can be run individually or as a suite
- Use Test Explorer in Visual Studio
- Categorize tests for selective execution
- Ensure all tests pass before code submission

---

*This document should be updated as the test framework evolves and new testing patterns emerge.*

### Exception Handling
- Test exception scenarios using ExpectedException attribute
- Verify proper error messages and exception types
- Test both expected and unexpected exception paths

### Function Calls in Conditionals
- Do not make function calls inside conditional statements in test code
- Extract function calls to separate variables before using in conditionals
- This improves test debugging, readability, and allows for easier breakpoint placement

```csharp
// Incorrect - function call inside conditional
if (sessionData.LoadSession(testFilePath))
{
    // Assert success
}

// Correct - extract function call
bool bLoadSuccess = sessionData.LoadSession(testFilePath);
if (bLoadSuccess)
{
    // Assert success
}