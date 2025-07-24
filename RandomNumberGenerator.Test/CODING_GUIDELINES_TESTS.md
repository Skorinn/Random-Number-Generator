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
// Copyright (C) 2025 Mike Pullen. All Rights Reserved.
// Confidential and Proprietary
//
// Revision History: 
//====================================================================================================================
// 2025/07/21 - [Author] - [Description of changes]
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
/// <exception cref="ExceptionType">Documents any exceptions that might be thrown during test execution</exception>
```

#### Exception Documentation in Tests
All test methods that explicitly test exception scenarios or may throw exceptions should document them:

```csharp
/// <summary>
/// Tests that the constructor throws ArgumentNullException when session data parameter is null
/// </summary>
/// <exception cref="System.ArgumentNullException">Expected exception when constructor parameter is null</exception>
[TestMethod]
[TestCategory("Component")]
[ExpectedException(typeof(System.ArgumentNullException))]
public void Constructor_NullSessionData_Exception()
{
    // Test implementation
}
```

#### Exception Testing Documentation Standards
- **Test Intent**: Clearly document which exception the test expects to receive
- **Test Conditions**: Describe the conditions that should trigger the exception
- **Expected Behavior**: Document that the exception is the expected and correct behavior
- **Exception Types**: Use full type names for clarity in test documentation

#### Exception Testing Best Practices
```csharp
// Good - documents expected exception and condition
/// <summary>
/// Tests that WriteDataPoint throws ArgumentNullException when dataPoint parameter is null
/// </summary>
/// <exception cref="System.ArgumentNullException">Expected exception for null parameter validation</exception>

// Better - includes why this exception is appropriate
/// <summary>
/// Tests that StartSession throws InvalidOperationException when no file path is set,
/// ensuring proper validation before attempting to create XML writer
/// </summary>
/// <exception cref="System.InvalidOperationException">Expected exception when file path validation fails</exception>

// Best - documents expected user experience
/// <summary>
/// Tests that file access errors result in UnauthorizedAccessException being thrown,
/// which should be caught by the GUI layer and displayed as a user-friendly error message
/// </summary>
/// <exception cref="System.UnauthorizedAccessException">Expected exception for file permission errors</exception>
```

#### Helper Method Exception Documentation
Even test helper methods should document exceptions they might throw:

```csharp
/// <summary>
/// Creates a mock XML writer configured for testing exception scenarios
/// </summary>
/// <param name="shouldThrow">IN - Whether the mock should throw exceptions</param>
/// <returns>Configured mock XML writer</returns>
/// <exception cref="ArgumentException">Thrown when mock configuration is invalid</exception>
private Mock<IRNGXMLWriter> CreateMockWriter(bool shouldThrow)
{
    // Implementation
}
```

### Exception Testing Patterns

#### Comprehensive Exception Testing
```csharp
/// <summary>
/// Tests all exception scenarios for WriteSessionStart method to ensure
/// proper error handling and user feedback
/// </summary>
[TestMethod]
[TestCategory("Component")]
public void WriteSessionStart_ExceptionScenarios_ProperErrorHandling()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    var xmlWriter = new RNGXMLWriter(); // No file path set
    
    //**************************************************************//
    // Act & Assert
    //**************************************************************//
    
    // Test missing file path exception
    try
    {
        xmlWriter.WriteSessionStart("00:00:00", 0);
        Assert.Fail("Expected InvalidOperationException was not thrown");
    }
    catch (InvalidOperationException ex)
    {
        StringAssert.Contains(ex.Message, "No file selected");
    }
}
```

### Inline Comments
```csharp
// Create the object under test
var objectUnderTest = new ClassUnderTest();

// Execute the method being tested
var result = objectUnderTest.Method();

// Verify the expected behavior occurred
Assert.IsTrue(result);

// Test exception scenarios to ensure robust error handling
try
{
    objectUnderTest.MethodThatShouldThrow();
    Assert.Fail("Expected exception was not thrown");
}
catch (ExpectedException ex)
{
    // Verify the exception message provides useful information
    Assert.IsNotNull(ex.Message);
    StringAssert.Contains(ex.Message, "expected error description");
}
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
- [ ] Exception testing for all error paths
- [ ] Exception message validation in tests
- [ ] Complete exception documentation using `<exception>` tags
- [ ] Exception propagation testing where appropriate
- [ ] Tests verify user-friendly error handling

### Exception Testing Review
When reviewing exception tests, verify:
- [ ] All exception scenarios are tested
- [ ] Exception messages are validated for usefulness
- [ ] Exception types are verified to be appropriate
- [ ] Exception propagation through layers is tested
- [ ] User experience during exception conditions is tested
- [ ] System stability after exceptions is verified
- [ ] Test documentation explains expected exception behavior

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

## Recent Updates
- Added comprehensive exception testing guidelines and documentation standards
- Defined exception testing patterns for various error scenarios
- Established requirements for testing exception propagation through application layers
- Updated code review checklist to include exception testing verification
- Added guidelines for validating exception messages and user experience during error conditions

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
```

### Well-Named Constants for Method Parameters in Tests
- Use descriptive, well-named constants for boolean and other parameters in test method calls
- This improves test readability, maintainability, and makes test intent clearer
- Constants should clearly indicate the purpose and expected behavior being tested

```csharp
// Incorrect - magic boolean literals in tests
CreateWriter(true, false);
timer.InitializeDevice(123, true);

// Correct - use well-named constants in tests
const bool bAPPEND_MODE = true;
const bool bRECREATE = false;
CreateWriter(bAPPEND_MODE, bRECREATE);

const int iTEST_SEED = 123;
const bool bSIMULATE_DEVICE = true;
timer.InitializeDevice(iTEST_SEED, bSIMULATE_DEVICE);
```

#### Benefits in Test Code
- **Test Intent Clarity**: Constants make the test purpose immediately obvious
- **Easier Test Maintenance**: Parameter changes are centralized and named
- **Better Test Documentation**: Constants serve as inline documentation
- **Reduced Test Errors**: Less likely to pass wrong parameters in test setup
- **Improved Test Debugging**: Breakpoints can be set on constant declarations

## Error Handling in Tests

### Exception Testing Principles
- **Comprehensive Coverage**: Test all exception paths to ensure robust error handling
- **User Experience Focus**: Verify that exceptions result in appropriate user feedback
- **Message Validation**: Assert that exception messages are helpful and actionable
- **Exception Propagation**: Test that exceptions properly bubble up through the call stack
- **Graceful Degradation**: Verify that the application remains stable after exceptions

#### Exception Testing Guidelines
```csharp
// Good - tests specific exception type and message
[TestMethod]
[ExpectedException(typeof(ArgumentNullException))]
public void Method_NullParameter_ThrowsArgumentNullException()
{
    // Test that validates proper parameter checking
}

// Better - validates exception message content
[TestMethod]
public void Method_InvalidState_ThrowsMeaningfulException()
{
    try
    {
        objectUnderTest.MethodThatShouldFail();
        Assert.Fail("Expected exception was not thrown");
    }
    catch (InvalidOperationException ex)
    {
        StringAssert.Contains(ex.Message, "expected error description");
        StringAssert.Contains(ex.Message, "user guidance");
    }
}

// Best - tests complete error handling flow including GUI response
[TestMethod]
public void StartSession_FileAccessDenied_DisplaysErrorInStatusBar()
{
    // Arrange: Set up conditions that cause file access exception
    // Act: Trigger the operation that should fail
    // Assert: Verify exception is caught and status bar shows error
}
```

### Exception Testing Categories

#### Parameter Validation Testing
Test that methods properly validate input parameters:

```csharp
/// <summary>
/// Tests that all public methods properly validate null parameters
/// and throw ArgumentNullException with descriptive messages
/// </summary>
[TestMethod]
[TestCategory("Component")]
public void PublicMethods_NullParameters_ThrowArgumentNullException()
{
    // Test multiple methods for consistent parameter validation
}
```

#### State Validation Testing
Test that methods check object state before execution:

```csharp
/// <summary>
/// Tests that operations requiring initialization throw InvalidOperationException
/// when called on uninitialized objects
/// </summary>
[TestMethod]
[TestCategory("Component")]
public void Operations_UninitializedObject_ThrowInvalidOperationException()
{
    // Test that proper state checking is implemented
}
```

#### External Dependency Error Testing
Test handling of external system failures:

```csharp
/// <summary>
/// Tests that file I/O errors are properly handled and result in appropriate
/// IOException being thrown with meaningful error messages
/// </summary>
[TestMethod]
[TestCategory("Integration")]
public void FileOperations_IOErrors_ThrowIOException()
{
    // Test external dependency error handling
}
```

### Exception Testing
```csharp
// Use ExpectedException attribute for simple exception testing
[ExpectedException(typeof(ArgumentNullException))]

// For more complex exception testing, use try-catch
try
{
    // Action that should throw
    objectUnderTest.MethodThatShouldThrow();
    Assert.Fail("Expected exception was not thrown");
}
catch (ExpectedException ex)
{
    // Verify exception details
    Assert.IsNotNull(ex.Message);
    StringAssert.Contains(ex.Message, "expected content");
}

// Test exception chaining and inner exceptions
catch (Exception ex)
{
    Assert.IsNotNull(ex.InnerException);
    Assert.IsInstanceOfType(ex.InnerException, typeof(ExpectedInnerException));
}
```

### Exception Message Testing
Always verify that exception messages provide value to developers and users:

```csharp
/// <summary>
/// Tests that exception messages contain actionable information for users
/// </summary>
[TestMethod]
public void Exceptions_ContainActionableMessages()
{
    try
    {
        // Trigger exception condition
        objectUnderTest.FailingMethod();
        Assert.Fail("Expected exception was not thrown");
    }
    catch (InvalidOperationException ex)
    {
        // Verify message provides context
        StringAssert.Contains(ex.Message, "what went wrong");
        StringAssert.Contains(ex.Message, "what user should do");
        
        // Verify message is user-friendly, not just technical
        Assert.IsFalse(ex.Message.Contains("null reference"));
        Assert.IsTrue(ex.Message.contains("file") || ex.Message.contains("select"));
    }
}
```

### Validation Testing
```csharp
// Test both valid and invalid inputs
[TestMethod]
public void Method_ValidInput_Success() { /* ... */ }

[TestMethod]
public void Method_InvalidInput_Failure() { /* ... */ }

// Test boundary conditions that might cause exceptions
[TestMethod]
public void Method_BoundaryConditions_ProperHandling() { /* ... */ }
```

### Exception Propagation Testing
Test that exceptions properly flow through the application layers:

```csharp
/// <summary>
/// Tests that XML writer exceptions properly propagate to the GUI layer
/// and result in status bar error messages being displayed to the user
/// </summary>
[TestMethod]
[TestCategory("Integration")]
public void XMLWriterException_PropagatesCorrectly_DisplaysInStatusBar()
{
    //**************************************************************//
    // Arrange
    //**************************************************************//
    
    // Set up mock that throws exception
    var mockWriter = new Mock<IRNGXMLWriter>();
    mockWriter.Setup(w => w.WriteSessionStart(It.IsAny<string>(), It.IsAny<int>()))
              .Throws(new InvalidOperationException("Test exception message"));
    
    //**************************************************************//
    // Act
    //**************************************************************//
    
    // Trigger operation that should propagate exception
    bool result = sessionDataFile.StartSession(mockSessionData.Object);
    
    //**************************************************************//
    // Assert
    //**************************************************************//
    
    // Verify exception was caught and handled appropriately
    Assert.IsFalse(result);
    // Additional assertions to verify error was logged/displayed
}
```

### Exception Documentation Requirements for Tests
- Document all expected exceptions in test methods using `<exception>` tags
- Include the purpose of exception testing in test summaries
- Verify that exception messages are helpful for debugging
- Test that exceptions don't leave the system in an invalid state
- Ensure exceptions are properly logged or displayed to users when appropriate

## Quality Guidelines

### Exception Testing Coverage
As part of comprehensive testing, ensure:
- All public methods that can throw exceptions have corresponding exception tests
- Exception messages are validated for usefulness
- Exception types are appropriate for the error conditions
- Exception handling doesn't mask important error information
- System remains stable after exception conditions