using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Visual_Inventory_Assistant.Classes;
using Visual_Inventory_Assistant.Windows;
using Visual_Inventory_Assistant.Windows;

namespace Visual_Inventory_Assistant.Classes
{
    internal class VisualLogger : Form
    {
        // This class handles the interaction with the Visual application and Google Sheets.
        // It includes methods for registering hotkeys, sending keystrokes, and updating Google Sheets.
        // Constants and fields for Google Sheets API

        #region Fields
        // Fields
        private static MainForm mainForm = Application.OpenForms["MainForm"] as MainForm;
        internal static string visualUserName = ApplicationVariables.VisualUserName;
        internal static string visualPassword = ApplicationVariables.VisualPassword;
        public static string visualServer = ApplicationVariables.VisualServer;
        private static string argument = "-d MTMFG -u " + visualUserName + " -p " + visualPassword;
        private static bool windowOpen = false;
        #endregion

        #region Constants
        // Constants
        private const int WM_CLOSE = 0x0010;
        #endregion

        #region DLL Imports
        // DLL Imports
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string lpszWindow
        );

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessageString(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam
        );

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        internal static extern IntPtr SendMessage(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region Visual Command Methods
        // Methods related to opening and interacting with Visual commands

        internal bool IsNewTransaction(string textBoxResult)
        {
            try
            {
                // Read and trim the value from the text box
                string columnBValue = mainForm.MainForm_TextBox_From.Text.Trim();

                // Check if the value is "NEW" (case-insensitive)
                bool isNew = columnBValue.Equals("NEW", StringComparison.OrdinalIgnoreCase);
                return isNew;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while checking for 'NEW': {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            return false; // Default to false if no data or an error occurs
        }

        internal void OpenVisualCommand(bool isNew = false)
        {
            try
            {
                var cmd = Process.Start(@"\\visual\visual908$\VMFG\VM.exe", argument);
                if (cmd == null)
                {
                    throw new InvalidOperationException("Failed to start the process.");
                }
                cmd.WaitForInputIdle();

                // Wait for the "Infor VISUAL" window to fully open
                WaitForWindow("Infor VISUAL - " + visualServer + "/" + visualUserName);

                if (isNew)
                {
                    OpenVisualInventoryCommand(true);
                }
                else
                {
                    OpenVisualInventoryCommand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while opening Visual Command: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void OpenVisualInventoryCommand(bool isNewTransaction = false)
        {
            try
            {
                var runningProcessByName = FindWindow(
                    null,
                    "Inventory Transaction Entry - Infor VISUAL - MTMFG"
                );
                if (runningProcessByName == IntPtr.Zero)
                {
                    var cmd2 = Process.Start(@"\\visual\visual908$\VMFG\VMINVENT.exe", argument);
                    if (cmd2 == null)
                    {
                        throw new InvalidOperationException("Failed to start the process.");
                    }
                    cmd2.WaitForInputIdle();

                    // Wait for the "Inventory Transaction Entry" window to fully open

                    WaitForWindow("Inventory Transaction Entry - Infor VISUAL - MTMFG");
                }
                else
                {
                    // Only call OpenVisualTransferCommand if it's not a "NEW"
                    if (!isNewTransaction)
                    {
                        OpenVisualTransferCommand();
                    }
                    else
                    {
                        HandleNewTransaction();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while opening Visual Inventory Command: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void OpenVisualTransferCommand()
        {
            try
            {
                var runningProcessByName1 = FindWindow(null, "Inventory Transfers");
                if (runningProcessByName1 == IntPtr.Zero)
                {
                    var runningProcessByName = FindWindow(
                        null,
                        "Inventory Transaction Entry - Infor VISUAL - MTMFG"
                    );
                    if (runningProcessByName != IntPtr.Zero)
                    {
                        SetForegroundWindow(
                            FindWindow(null, "Inventory Transaction Entry - Infor Visual - MTMFG")
                        );

                        SendKeys.SendWait("%");
                        SendKeys.SendWait("e");
                        SendKeys.SendWait("s");

                        // Wait for the "Inventory Transfers" window to fully open

                        WaitForWindow("Inventory Transfers");
                        FillTransferPartID();
                    }
                }
                else
                {
                    SetForegroundWindow(runningProcessByName1);

                    FillTransferPartID();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while opening Visual Transfer Command: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        private void WaitForWindow(string windowTitle, int timeoutMilliseconds = 10000)
        {
            int elapsedTime = 0;
            const int pollingInterval = 100; // Check every 100ms

            while (elapsedTime < timeoutMilliseconds)
            {
                var windowHandle = FindWindow(null, windowTitle);
                if (windowHandle != IntPtr.Zero)
                {
                    return;
                }

                Thread.Sleep(pollingInterval);
                elapsedTime += pollingInterval;
            }

            throw new TimeoutException($"Timed out waiting for window '{windowTitle}' to open.");
        }
        #endregion

        #region Fill Methods
        // Methods related to filling data in the Visual application

        private void SendKeysToVisual(string id, AutomationElement handle, string text, bool tabkey)
        {
            AutomationElement element = FindTextBoxByAutomationId(handle, id);
            if (element != null)
            {
                SetForegroundWindow(new IntPtr(element.Current.NativeWindowHandle)); // Focus on the field

                Thread.Sleep(250);
                SendKeys.Send("^a"); // Select all text
                SendKeys.Send("{DEL}"); // Delete the selected text

                Thread.Sleep(250);
                SendKeys.Send(text);
                Thread.Sleep(250);
                if (tabkey == true)
                {
                    SendKeys.Send("{TAB}"); // Simulate pressing the tab key
                }
            }
        }

        internal void HandleNewTransaction()
        {
            try
            {
                // Step 1: Check if the Transfer Window is open and close it if necessary
                var transferWindowHandle = FindWindow(null, "Inventory Transfers");
                if (transferWindowHandle != IntPtr.Zero)
                {
                    closeVisualTransferCommand();
                }

                var inventoryWindowHandle = FindWindow(
                    null,
                    "Inventory Transaction Entry - Infor VISUAL - MTMFG"
                );

                if (inventoryWindowHandle == IntPtr.Zero)
                {
                    OpenVisualInventoryCommand(true);
                }

                // Step 4: Fill the fields in the Inventory Window
                inventoryWindowHandle = FindWindow(
                    null,
                    "Inventory Transaction Entry - Infor VISUAL - MTMFG"
                );
                if (inventoryWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(inventoryWindowHandle);
                    AutomationElement mainWindow = AutomationElement.FromHandle(
                        inventoryWindowHandle
                    );

                    if (mainWindow != null)
                    {
                        // Fill Automation ID 4115 with WorkOrder
                        SendKeysToVisual(
                            "4115",
                            mainWindow,
                            mainForm.MainForm_TextBox_WorkOrder.Text,
                            true
                        );

                        // Fill Automation ID 4116 with "1"
                        SendKeysToVisual("4116", mainWindow, "1", true);

                        // Fill Automation ID 4143 with Quantity
                        SendKeysToVisual(
                            "4143",
                            mainWindow,
                            mainForm.MainForm_TextBox_Quantity.Text,
                            false
                        );

                        // Fill Automation ID 4148 with "002"
                        SendKeysToVisual("4148", mainWindow, "002", false);

                        // Fill Automation ID 4152 with To
                        SendKeysToVisual(
                            "4152",
                            mainWindow,
                            mainForm.MainForm_TextBox_To.Text,
                            true
                        );

                        // Wait for the save button to be clicked
                        FillNewTransactionWaitForSaveButtonClick();
                    }
                    else
                    {
                        MessageBox.Show("Visual Inventory window not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Visual Inventory window not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while handling 'NEW': {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        internal void FillTransferPartID()
        {
            try
            {
                var hwndMain = FindWindow(null, "Inventory Transfers");
                if (hwndMain != IntPtr.Zero)
                {
                    SetForegroundWindow(hwndMain);
                    AutomationElement mainWindow = AutomationElement.FromHandle(hwndMain);
                    if (mainWindow != null)
                    {
                        AutomationElement partIdTextBox = FindTextBoxByAutomationId(
                            mainWindow,
                            "4102"
                        );
                        if (partIdTextBox != null)
                        {
                            SetForegroundWindow(
                                new IntPtr(partIdTextBox.Current.NativeWindowHandle)
                            ); // Set focus on the control
                            SendKeys.Send("^a"); // Select all text
                            SendKeys.Send("{DEL}"); // Delete the selected text
                            SendKeys.Send(mainForm.MainForm_TextBox_PartID.Text); // Simulate typing
                            Thread.Sleep(1000); // Wait for 1 second
                            FillTransferAllTextBoxes(); // Call FillTransferAllTextBoxes after filling the control
                        }
                        else
                        {
                            MessageBox.Show("Part ID text box not found.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Visual Error: Frame not found.");
                    }
                }
                else
                {
                    OpenVisualTransferCommand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Part ID: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferAllTextBoxes()
        {
            try
            {
                var hwndMain = FindWindow(null, "Inventory Transfers");
                if (hwndMain != IntPtr.Zero)
                {
                    SetForegroundWindow(hwndMain);
                    AutomationElement mainWindow = AutomationElement.FromHandle(hwndMain);
                    if (mainWindow != null)
                    {
                        FillTransferQuantity(mainWindow);
                        FillTransferFrom002(mainWindow);
                        FillTransferTo002(mainWindow);
                        FillTransferFromLocation(mainWindow);
                        FillTransferToLocation(mainWindow);
                    }
                    else
                    {
                        MessageBox.Show("Visual Error: Frame not found.");
                    }
                }
                else
                {
                    OpenVisualTransferCommand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling all text boxes: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferQuantity(AutomationElement mainWindow)
        {
            try
            {
                AutomationElement quantityTextBox = FindTextBoxByAutomationId(mainWindow, "4111");
                if (quantityTextBox != null)
                {
                    SetForegroundWindow(new IntPtr(quantityTextBox.Current.NativeWindowHandle)); // Set focus on the control
                    Thread.Sleep(1000); // Wait for 1 second
                    FillTransferCheckForPartsWindow(); // Call FillTransferCheckForPartsWindow after setting the foreground window
                    SendKeys.Send("^a"); // Select all text
                    SendKeys.Send("{DEL}"); // Delete the selected text
                    SendKeys.Send(mainForm.MainForm_TextBox_Quantity.Text); // Simulate typing
                }
                else
                {
                    MessageBox.Show("Quantity text box not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Quantity: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferFrom002(AutomationElement mainWindow)
        {
            try
            {
                AutomationElement transferFromTextBox = FindTextBoxByAutomationId(
                    mainWindow,
                    "4123"
                );
                if (transferFromTextBox != null)
                {
                    SetForegroundWindow(new IntPtr(transferFromTextBox.Current.NativeWindowHandle)); // Set focus on the control
                    SendKeys.Send("^a"); // Select all text
                    SendKeys.Send("{DEL}"); // Delete the selected text
                    SendKeys.Send("002"); // Simulate typing
                }
                else
                {
                    MessageBox.Show("Transfer From text box not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Transfer From 002: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferTo002(AutomationElement mainWindow)
        {
            try
            {
                AutomationElement transferToTextBox = FindTextBoxByAutomationId(mainWindow, "4142");
                if (transferToTextBox != null)
                {
                    SetForegroundWindow(new IntPtr(transferToTextBox.Current.NativeWindowHandle)); // Set focus on the control
                    SendKeys.Send("^a"); // Select all text
                    SendKeys.Send("{DEL}"); // Delete the selected text
                    SendKeys.Send("002"); // Simulate typing
                }
                else
                {
                    MessageBox.Show("Transfer To text box not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Transfer To 002: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferFromLocation(AutomationElement mainWindow)
        {
            try
            {
                AutomationElement transferFromLocationTextBox = FindTextBoxByAutomationId(
                    mainWindow,
                    "4124"
                );
                if (transferFromLocationTextBox != null)
                {
                    SetForegroundWindow(
                        new IntPtr(transferFromLocationTextBox.Current.NativeWindowHandle)
                    ); // Set focus on the control
                    SendKeys.Send("^a"); // Select all text
                    SendKeys.Send("{DEL}"); // Delete the selected text
                    SendKeys.Send(mainForm.MainForm_TextBox_From.Text); // Simulate typing
                }
                else
                {
                    MessageBox.Show("Transfer From Location text box not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Transfer From Location: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferToLocation(AutomationElement mainWindow)
        {
            try
            {
                AutomationElement transferToLocationTextBox = FindTextBoxByAutomationId(
                    mainWindow,
                    "4143"
                );
                if (transferToLocationTextBox != null)
                {
                    windowOpen = false;
                    SetForegroundWindow(
                        new IntPtr(transferToLocationTextBox.Current.NativeWindowHandle)
                    ); // Set focus on the control
                    Thread.Sleep(1000); // Wait for 1 second
                    FillTransferCheckForInventoryTransactionEntryWindow(); // Call FillTransferCheckForInventoryTransactionEntryWindow after filling the control
                    if (windowOpen == false)
                    {
                        SendKeys.Send("^a"); // Select all text
                        SendKeys.Send("{DEL}"); // Delete the selected text
                        SendKeys.Send(mainForm.MainForm_TextBox_To.Text); // Simulate typing
                        SendKeys.Send("{TAB}"); // Simulate pressing the tab key

                        FillTransferWaitForSaveButtonClick(); // Call FillTransferWaitForSaveButtonClick after checking for the window
                    }
                    else
                    {
                        FillTransferWaitForSaveButtonClick();
                    }
                }
                else
                {
                    MessageBox.Show("Transfer To Location text box not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while filling Transfer To Location: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferCheckForPartsWindow()
        {
            try
            {
                var hwndParts = FindWindow("Gupta:AccFrame", "Parts");
                if (hwndParts != IntPtr.Zero)
                {
                    // Bring the Parts window to the foreground
                    SetForegroundWindow(hwndParts);

                    // Simulate pressing the "Up" key followed by the "Enter" key
                    SendKeys.SendWait("{UP}");
                    Thread.Sleep(100); // Small delay to ensure the key press is processed
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(100); // Small delay to ensure the key press is processed
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while interacting with the Parts window: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillTransferCheckForInventoryTransactionEntryWindow()
        {
            try
            {
                var hwndInventoryTransactionEntry = FindWindow(
                    "Gupta:AccFrame",
                    "Inventory Transaction Entry"
                );
                if (hwndInventoryTransactionEntry != IntPtr.Zero)
                {
                    while (true)
                    {
                        if (
                            FindWindow("Gupta:AccFrame", "Inventory Transaction Entry")
                            == IntPtr.Zero
                        )
                        {
                            break;
                        }
                        mainForm.MainForm_StatusText_Loading.Text =
                            "Waiting for the Inventory Transaction Entry Window Detected, Process Stopped...";
                        // Sleep for a short period to avoid busy-waiting
                        windowOpen = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while checking for Inventory Transaction Entry window: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        #endregion

        #region Helper Methods
        // Helper methods for finding text boxes by AutomationId and other utility functions
        private AutomationElement FindTextBoxByAutomationId(
            AutomationElement mainWindow,
            string automationId
        )
        {
            Condition condition = new PropertyCondition(
                AutomationElement.ControlTypeProperty,
                ControlType.Edit
            );
            AutomationElementCollection textBoxes = mainWindow.FindAll(
                TreeScope.Descendants,
                condition
            );

            foreach (AutomationElement textBox in textBoxes)
            {
                if (textBox.Current.AutomationId == automationId)
                {
                    return textBox;
                }
            }
            return null;
        }

        internal void FillTransferWaitForSaveButtonClick()
        {
            try
            {
                mainForm.MainForm_StatusText_Loading.Text =
                    "Transaction " + (mainForm._currentRowIndex + 1).ToString() + " Saved.";
                mainForm.MainForm_Button_Next.Enabled = true;
                mainForm.MainForm_Button_Next.Text = "Next";
                mainForm.MainForm_Button_Next.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while waiting for save button click: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void FillNewTransactionWaitForSaveButtonClick()
        {
            try
            {
                mainForm.MainForm_StatusText_Loading.Text =
                    "Transaction " + (mainForm._currentRowIndex + 1).ToString() + " Saved.";
                mainForm.MainForm_Button_Next.Enabled = true;
                mainForm.MainForm_Button_Next.Text = "Next";
                mainForm.MainForm_Button_Next.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while waiting for save button click: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        internal void closeVisualTransferCommand()
        {
            try
            {
                var runningProcessByName = FindWindow(null, "Inventory Transfers");
                if (runningProcessByName != IntPtr.Zero)
                {
                    SetForegroundWindow(runningProcessByName);
                    SendMessage(runningProcessByName, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while closing Visual Transfer Command: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Any cleanup code if necessary
            }
        }

        public void SendDataToGoogleSheet(string buttonName)
        {
            try
            {
                // Determine the sheet name based on the button that called the method
                string sheetName = buttonName switch
                {
                    "MainForm_Button_SentOR" => "E-Mail: Over Receipts",
                    "MainForm_Button_SendClosed" => "E-Mail: Work Order Closed",
                    "MainForm_Button_SendAddRemove" => "E-Mail: Add Remove",
                    _ => throw new InvalidOperationException("Unknown button name."),
                };

                // Extract Work Order, Quantity, and To Location using Automation IDs
                string workOrder = GetTextFromAutomationId("4115");
                int quantity = GetQuantityFromAutomationId("4143"); // Retrieve quantity as an integer
                string location = GetTextFromAutomationId("4152");

                // Extract Part Number using Automation ID 4127
                string partNumber = GetTextFromAutomationId("4127");

                string columnEValue = string.Empty;

                // If the button is MainForm_Button_SendAddRemove, use MainForm_TextBox_TagQty for the tag quantity
                if (buttonName == "MainForm_Button_SendAddRemove")
                {
                    if (!int.TryParse(mainForm.MainForm_TextBox_TagQty.Text, out int tagQuantity))
                    {
                        MessageBox.Show(
                            "Invalid tag quantity entered. Please ensure it is a valid number.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    // Calculate the adjusted quantity
                    int adjustedQuantity = tagQuantity - quantity;

                    // Determine the value for column E based on the adjusted quantity
                    columnEValue = adjustedQuantity > 0 ? "Add" : "Remove";

                    // Update the quantity to the absolute value for display purposes
                    quantity = Math.Abs(adjustedQuantity);
                }

                // Retrieve notes from MainForm_TextBox_Notes
                string notes = mainForm.MainForm_TextBox_Notes.Text;

                // Validate required fields
                if (
                    string.IsNullOrWhiteSpace(workOrder)
                    || string.IsNullOrWhiteSpace(partNumber)
                    || quantity <= 0
                    || string.IsNullOrWhiteSpace(location)
                )
                {
                    MessageBox.Show(
                        "One or more required fields are empty or invalid. Please ensure all fields are filled.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Prepare the data to send, ensuring all values are strings
                var values = new List<IList<object>>
                {
                    new List<object>
                    {
                        workOrder, // Work Order as text
                        partNumber, // Part Number as text
                        quantity.ToString(), // Quantity as text
                        location, // Location as text
                        sheetName == "E-Mail: Add Remove" ? columnEValue : notes, // Column E value
                    },
                };

                // Google Sheets API setup
                var filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "visual-easy-inventory-3e055e946d7c.json"
                );
                var credential = GoogleCredential
                    .FromFile(filePath)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);

                var service = new SheetsService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "EasyInventory",
                    }
                );

                // Define the spreadsheet ID and range
                string spreadsheetId = "1QO0byGw_hJ35FpQUnUaZw_16zhiHXwVgrppB4-JF2TY"; // Replace with the actual ID
                string range = $"{sheetName}!A2:E"; // Start appending from row 2

                // Create the request to append data
                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange { Values = values };
                var appendRequest = service.Spreadsheets.Values.Append(
                    valueRange,
                    spreadsheetId,
                    range
                );
                appendRequest.ValueInputOption = SpreadsheetsResource
                    .ValuesResource
                    .AppendRequest
                    .ValueInputOptionEnum
                    .RAW;

                // Execute the request
                var response = appendRequest.Execute();

                // Clear the text in MainForm_TextBox_Notes and MainForm_TextBox_TagQty after successful execution
                mainForm.MainForm_TextBox_Notes.Clear();
                mainForm.MainForm_TextBox_TagQty.Clear();

                MessageBox.Show(
                    "Data sent successfully to Google Sheet.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while sending data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public int GetQuantityFromAutomationId(string automationId)
        {
            var inventoryWindowHandle = FindWindow(
                null,
                "Inventory Transaction Entry - Infor VISUAL - MTMFG"
            );
            if (inventoryWindowHandle != IntPtr.Zero)
            {
                AutomationElement mainWindow = AutomationElement.FromHandle(inventoryWindowHandle);
                if (mainWindow != null)
                {
                    var textBox = FindTextBoxByAutomationId(mainWindow, automationId);
                    if (textBox != null)
                    {
                        // Check if the control supports the ValuePattern
                        if (textBox.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                        {
                            var valuePattern = (ValuePattern)pattern;
                            string value = valuePattern.Current.Value; // Retrieve the text input

                            // Remove the decimal and parse as an integer
                            if (int.TryParse(value.Split('.')[0], out int quantity))
                            {
                                return quantity; // Return the parsed integer
                            }
                            else
                            {
                                MessageBox.Show(
                                    $"Invalid quantity value: {value}. Please ensure it is a valid number.",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                return 0; // Default to 0 if the value is invalid
                            }
                        }
                    }
                }
            }

            return 0; // Return 0 if the control is not found or does not support ValuePattern
        }

        private string GetTextFromAutomationId(string automationId, bool enforceWholeNumber = false)
        {
            var inventoryWindowHandle = FindWindow(
                null,
                "Inventory Transaction Entry - Infor VISUAL - MTMFG"
            );
            if (inventoryWindowHandle != IntPtr.Zero)
            {
                AutomationElement mainWindow = AutomationElement.FromHandle(inventoryWindowHandle);
                if (mainWindow != null)
                {
                    var textBox = FindTextBoxByAutomationId(mainWindow, automationId);
                    if (textBox != null)
                    {
                        // Check if the control supports the ValuePattern
                        if (textBox.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                        {
                            var valuePattern = (ValuePattern)pattern;
                            string value = valuePattern.Current.Value; // Retrieve the text input

                            // If enforceWholeNumber is true, ensure the value is a whole number
                            if (enforceWholeNumber)
                            {
                                if (int.TryParse(value, out int wholeNumber))
                                {
                                    return wholeNumber.ToString(); // Return the whole number as a string
                                }
                                else
                                {
                                    MessageBox.Show(
                                        $"Invalid quantity value: {value}. Please ensure it is a whole number.",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error
                                    );
                                    return "0"; // Default to 0 if the value is invalid
                                }
                            }

                            return value; // Return the raw value if not enforcing whole number
                        }
                    }
                }
            }

            return string.Empty; // Return empty if the control is not found or does not support ValuePattern
        }

        #endregion
    }
}
