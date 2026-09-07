//*********************************************************************************************************************
// File Name:      DeviceUpdateThread.Test.cs
// Description:    Unit tests for the DeviceUpdateThread class
//
// Copyright (c) 2025 Mike Pullen
// Licensed under the MIT License. See LICENSE in the repository root.
//
// Revision History: 
//====================================================================================================================
// 2025/07/21 - Mike Pullen - Original implementation.
// 2026/08/31 - Mike Pullen - Added coverage for the info box only being restored when it is unchanged
//*********************************************************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace RandomNumberGenerator.Test
{
    /// <summary>
    /// Unit tests for the DeviceUpdateThread class
    /// </summary>
    [TestClass]
    public class DeviceUpdateThreadTests
    {
        #region Infrastructure

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeviceUpdateThreadTests()
        {
            // Nothing to do
        }

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

        #endregion
        #region Data Members

        // Test constants for device thread testing
        private const string m_sTEST_STATUS_TEXT = "Test Status Text";
        private readonly Color m_TEST_STATUS_TEXT_COLOR = Color.Blue;
        private readonly Color m_TEST_STATUS_BACK_COLOR = Color.Yellow;
        private const string m_sREADING_DEVICES_MESSAGE = "Checking attached devices and updating port list...";
        private const string m_sSESSION_RUNNING_MESSAGE = " Running session";

        #endregion
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        #region Initialization and cleanup

        /// <summary>
        /// Cleans up after each test runs to ensure we always start with a clean state
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Reset the parent to null to avoid test interdependencies
            DeviceUpdateThread.Parent = null;
        }

        #endregion
        #region Tests

        /// <summary>
        /// Tests the DeviceList property returns a valid binding list
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void DeviceList_GetProperty_ReturnsValidBindingList()
        {
            //**************************************************************//
            // Arrange & Act
            //**************************************************************//

            // Get the device list property
            BindingList<IRNGDevice> deviceList = DeviceUpdateThread.DeviceList;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the device list is not null
            Assert.IsNotNull(deviceList);
        }

        /// <summary>
        /// Tests the Parent property setter and getter work correctly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Parent_SetAndGetProperty_WorksCorrectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form
            var mockParent = new Mock<IGeneratorForm>();

            //**************************************************************//
            // Act
            //**************************************************************//

            // Set the parent property
            DeviceUpdateThread.Parent = mockParent.Object;

            // Get the parent property
            IGeneratorForm actualParent = DeviceUpdateThread.Parent;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the parent was set correctly
            Assert.AreEqual(mockParent.Object, actualParent);
        }

        /// <summary>
        /// Tests the Terminating property returns false when parent state is not Terminating
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Terminating_ParentNotTerminating_ReturnsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form with non-terminating state
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Get the terminating property
            bool bTerminating = DeviceUpdateThread.Terminating;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify terminating is false
            Assert.IsFalse(bTerminating);
        }

        /// <summary>
        /// Tests the Terminating property returns true when parent state is Terminating
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Terminating_ParentTerminating_ReturnsTrue()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form with terminating state
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Terminating);

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Get the terminating property
            bool bTerminating = DeviceUpdateThread.Terminating;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify terminating is true
            Assert.IsTrue(bTerminating);
        }

        /// <summary>
        /// Tests the Terminating property returns false when parent is null
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void Terminating_ParentNull_ReturnsFalse()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set parent to null
            DeviceUpdateThread.Parent = null;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Get the terminating property
            bool bTerminating = DeviceUpdateThread.Terminating;

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify terminating is false when parent is null
            Assert.IsFalse(bTerminating);
        }

        /// <summary>
        /// Tests ThreadProc with valid parent updates device list and manages status box
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void ThreadProc_ValidParent_UpdatesDeviceListAndManagesStatusBox()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);
            mockParent.Setup(mock => mock.InvokeRequired).Returns(true);
            mockParent.Setup(mock => mock.Invoke(It.IsAny<Action>())).Callback<Action>(action => action());
            
            // Setup status box state backup
            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          text = m_sTEST_STATUS_TEXT;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            // Setup status box state setting (should be called twice - once for reading message, once for restore)
            mockParent.Setup(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>())).Verifiable();

            // Setup device list setting
            mockParent.SetupSet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>()).Verifiable();

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute the thread procedure
            DeviceUpdateThread.ThreadProc(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the status box was read twice - once to back it up and once to check it before restoring
            mockParent.Verify(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny), Times.Exactly(2));

            // Verify only the reading message was displayed. The mock reports a different message is displayed
            // when the update completes, so the backed up message must not be restored over the top of it.
            mockParent.Verify(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>()), Times.Once);
            mockParent.Verify(mock => mock.SetStatusBoxState(m_sREADING_DEVICES_MESSAGE, It.IsAny<Color>(), It.IsAny<Color>()), Times.Once);

            // Verify Invoke was called to update device list
            mockParent.Verify(mock => mock.Invoke(It.IsAny<Action>()), Times.Once);
        }

        /// <summary>
        /// Tests ThreadProc with null parent handles gracefully
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ThreadProc_NullParent_HandlesGracefully()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Set parent to null
            DeviceUpdateThread.Parent = null;

            //**************************************************************//
            // Act & Assert
            //**************************************************************//

            // Execute the thread procedure - should not throw exception
            try
            {
                DeviceUpdateThread.ThreadProc(null);
                Assert.IsTrue(true, "ThreadProc completed without exception");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ThreadProc should handle null parent gracefully but threw: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests ThreadProc with parent not requiring invoke updates directly
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ThreadProc_ParentNoInvokeRequired_UpdatesDirectly()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form that doesn't require invoke
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);
            mockParent.Setup(mock => mock.InvokeRequired).Returns(false);
            
            // Setup status box state backup
            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          text = m_sTEST_STATUS_TEXT;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            // Setup status box state setting
            mockParent.Setup(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>())).Verifiable();

            // Setup device list setting
            mockParent.SetupSet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>()).Verifiable();

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute the thread procedure
            DeviceUpdateThread.ThreadProc(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify Invoke was not called since InvokeRequired is false
            mockParent.Verify(mock => mock.Invoke(It.IsAny<Action>()), Times.Never);

            // Verify device list was set directly
            mockParent.VerifySet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>(), Times.Once);
        }

        /// <summary>
        /// Tests ThreadProc respects thread synchronization with concurrent calls
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void ThreadProc_ConcurrentCalls_RespectsSynchronization()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);
            mockParent.Setup(mock => mock.InvokeRequired).Returns(false);
            
            // Setup status box state backup
            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          text = m_sTEST_STATUS_TEXT;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            mockParent.Setup(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>()));
            mockParent.SetupSet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>()).Verifiable();

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            // Counter to track concurrent executions
            int iExecutionCounter = 0;
            int iMaxConcurrentExecutions = 0;
            object counterLock = new object();

            // Override the mock to track concurrent executions
            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          lock (counterLock)
                          {
                              iExecutionCounter++;
                              if (iExecutionCounter > iMaxConcurrentExecutions)
                              {
                                  iMaxConcurrentExecutions = iExecutionCounter;
                              }
                          }

                          // Simulate some processing time
                          Thread.Sleep(10);

                          lock (counterLock)
                          {
                              iExecutionCounter--;
                          }

                          text = m_sTEST_STATUS_TEXT;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            //**************************************************************//
            // Act
            //**************************************************************//

            // Start multiple threads to call ThreadProc concurrently
            const int iNumThreads = 3;
            Thread[] threads = new Thread[iNumThreads];

            for (int i = 0; i < iNumThreads; i++)
            {
                threads[i] = new Thread(() => DeviceUpdateThread.ThreadProc(null));
                threads[i].Start();
            }

            // Wait for all threads to complete. The timeout has to allow for every thread to run a full device
            // search in turn, as a thread still running when the test ends interferes with the tests that follow.
            const int iJOIN_TIMEOUT = 60000; // 60 second timeout
            for (int i = 0; i < iNumThreads; i++)
            {
                bool bThreadCompleted = threads[i].Join(iJOIN_TIMEOUT);
                Assert.IsTrue(bThreadCompleted, "ThreadProc threads must complete before the test ends");
            }

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify that only one thread executed at a time (synchronization working)
            Assert.AreEqual(1, iMaxConcurrentExecutions, "ThreadProc should allow only one concurrent execution due to locking");
        }

        /// <summary>
        /// Tests the info box is restored when nothing has been displayed since the reading devices message
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ThreadProc_InfoBoxUnchanged_RestoresPreviousMessage()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);
            mockParent.Setup(mock => mock.InvokeRequired).Returns(false);

            // Track the message displayed in the info box so the mock behaves like the real status box
            string sDisplayedText = m_sTEST_STATUS_TEXT;

            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          text = sDisplayedText;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            mockParent.Setup(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>()))
                      .Callback<string, Color, Color>((text, textColor, backColor) =>
                      {
                          // Discard the unused colors
                          _ = textColor;
                          _ = backColor;

                          sDisplayedText = text;
                      });

            mockParent.SetupSet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>()).Verifiable();

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute the thread procedure
            DeviceUpdateThread.ThreadProc(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the message displayed before the update was restored
            Assert.AreEqual(m_sTEST_STATUS_TEXT, sDisplayedText);
            mockParent.Verify(mock => mock.SetStatusBoxState(m_sTEST_STATUS_TEXT, m_TEST_STATUS_TEXT_COLOR, m_TEST_STATUS_BACK_COLOR), Times.Once);
        }

        /// <summary>
        /// Tests a message displayed while the devices are being read is not overwritten by the restore.
        /// NOTE: Covers the info box being left with a stale message when a session is started during an update.
        /// </summary>
        [TestMethod]
        [TestCategory("Component")]
        public void ThreadProc_InfoBoxChangedDuringUpdate_NewerMessageKept()
        {
            //**************************************************************//
            // Arrange
            //**************************************************************//

            // Create mock parent form
            var mockParent = new Mock<IGeneratorForm>();
            mockParent.Setup(mock => mock.State).Returns(GeneratorForm.RngGuiStates.Idle);
            mockParent.Setup(mock => mock.InvokeRequired).Returns(false);

            // Track the message displayed in the info box so the mock behaves like the real status box
            string sDisplayedText = m_sTEST_STATUS_TEXT;

            mockParent.Setup(mock => mock.GetStatusBoxState(out It.Ref<string>.IsAny, out It.Ref<Color>.IsAny, out It.Ref<Color>.IsAny))
                      .Callback(new GetStatusBoxStateCallback((out string text, out Color textColor, out Color backColor) =>
                      {
                          text = sDisplayedText;
                          textColor = m_TEST_STATUS_TEXT_COLOR;
                          backColor = m_TEST_STATUS_BACK_COLOR;
                      }));

            mockParent.Setup(mock => mock.SetStatusBoxState(It.IsAny<string>(), It.IsAny<Color>(), It.IsAny<Color>()))
                      .Callback<string, Color, Color>((text, textColor, backColor) =>
                      {
                          // Discard the unused colors
                          _ = textColor;
                          _ = backColor;

                          sDisplayedText = text;
                      });

            // Simulate the GUI displaying a newer message while the device search is still running. The device
            // list is updated at the end of the search, immediately before the info box is restored.
            mockParent.SetupSet(mock => mock.DeviceList = It.IsAny<BindingList<IRNGDevice>>())
                      .Callback<BindingList<IRNGDevice>>(deviceList =>
                      {
                          // Discard the unused device list
                          _ = deviceList;

                          sDisplayedText = m_sSESSION_RUNNING_MESSAGE;
                      });

            // Set the parent
            DeviceUpdateThread.Parent = mockParent.Object;

            //**************************************************************//
            // Act
            //**************************************************************//

            // Execute the thread procedure
            DeviceUpdateThread.ThreadProc(null);

            //**************************************************************//
            // Assert
            //**************************************************************//

            // Verify the newer message was left displayed rather than being replaced by the backup
            Assert.AreEqual(m_sSESSION_RUNNING_MESSAGE, sDisplayedText);
            mockParent.Verify(mock => mock.SetStatusBoxState(m_sTEST_STATUS_TEXT, It.IsAny<Color>(), It.IsAny<Color>()), Times.Never);
        }

        #endregion
        #region Helper Types

        /// <summary>
        /// Delegate for GetStatusBoxState callback to handle out parameters in Moq
        /// </summary>
        /// <param name="text">OUT - Status box text</param>
        /// <param name="textColor">OUT - Status box text color</param>
        /// <param name="backColor">OUT - Status box background color</param>
        private delegate void GetStatusBoxStateCallback(out string text, out Color textColor, out Color backColor);

        #endregion
    }
}