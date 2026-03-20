namespace Visual_Inventory_Assistant.Windows
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Visual_Inventory_Assistant.Classes;
    using Visual_Inventory_Assistant.Windows;

    public partial class MainForm : Form
    {
        #region Fields
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName
        );

        private IList<IList<object>> _values = new List<IList<object>>();
        public int _currentRowIndex;

        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void MainForm_Button_Next_Click(object sender, EventArgs e)
        {
            try
            {
                if (MainForm_Button_Next.Text == @"Start")
                    InitializeGoogleSheetsData();
                else if (MainForm_Button_Next.Text == @"Next")
                    LoadNextRow();
                else if (MainForm_Button_Next.Text == @"Push to Visual")
                    InteractWithVisual();
                else if (MainForm_Button_Next.Text == @"Exit")
                    Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void MainForm_Button_Skip_Click(object sender, EventArgs e)
        {
            try
            {
                if (_values != null && _currentRowIndex < _values.Count - 1)
                {
                    _currentRowIndex++;
                    MainForm_Button_Next.Text = @"Push to Visual";
                    FillTextBoxes(_values[_currentRowIndex]);
                }
                else
                {
                    MainForm_Button_Next.Text = @"Exit";
                    MainForm_Button_Skip.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while skipping to the next row: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void MainForm_Button_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadLoginScreen(object sender, EventArgs e)
        {
            var login = new Login();
            login.ShowDialog();
        }

        private void MainForm_MenuStrip_File_Settings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }
        #endregion

        #region Methods
        private void InitializeGoogleSheetsData()
        {
            try
            {
                SetButtonsEnabled(false);
                MainForm_StatusText_Loading.Visible = true;
                MainForm_StatusText_Loading.Text = "Loading Google Sheets data...";

                var filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "visual-easy-inventory-3e055e946d7c.json"
                );

                var match = Regex.Match(
                    ApplicationVariables.GoogleSheetsLink,
                    @"spreadsheets/d/([a-zA-Z0-9-_]+)"
                );
                if (!match.Success)
                {
                    MessageBox.Show(
                        "Invalid Google Sheets link.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                var spreadsheetId = match.Groups[1].Value;
                var range = $"{ApplicationVariables.GoogleSheetsSheet}!A2:E";

                var credential = GoogleCredential
                    .FromFile(filePath)
                    .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                var service = new SheetsService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "EasyInventory",
                    }
                );

                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = request.Execute();
                _values = response.Values;

                if (_values == null || _values.Count == 0)
                {
                    MessageBox.Show(
                        $"{ApplicationVariables.GoogleSheetsSheet} Sheet has no data",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Application.Exit();
                }
                else
                {
                    _currentRowIndex = 0;
                    FillTextBoxes(_values[_currentRowIndex]);
                    MainForm_Button_Next.Text = @"Push to Visual";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while initializing Google Sheets data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                SetButtonsEnabled(true);
                MainForm_Button_Skip.Enabled = true;
                MainForm_StatusText_Loading.Text = "";
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            foreach (Control control in Controls)
                if (control is Button)
                    control.Enabled = enabled;
        }

        private void LoadNextRow()
        {
            try
            {
                if (_values != null && _currentRowIndex < _values.Count - 1)
                {
                    _currentRowIndex++;
                    MainForm_Button_Next.Text = @"Push to Visual";
                    FillTextBoxes(_values[_currentRowIndex]);
                }
                else
                {
                    MainForm_Button_Next.Text = @"Exit";
                    MainForm_Button_Skip.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while loading the next row: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void FillTextBoxes(IList<object> row)
        {
            try
            {
                MainForm_TextBox_PartID.Text = row.Count > 0 ? row[0].ToString() : string.Empty;
                MainForm_TextBox_From.Text = row.Count > 1 ? row[1].ToString() : string.Empty;
                MainForm_TextBox_To.Text = row.Count > 2 ? row[2].ToString() : string.Empty;
                MainForm_TextBox_WorkOrder.Text = row.Count > 3 ? row[3].ToString() : string.Empty;
                MainForm_TextBox_Quantity.Text = row.Count > 4 ? row[4].ToString() : string.Empty;
                MainForm_TextBox_WorkOrder.Text = row.Count > 3 ? row[3].ToString() : string.Empty; // Column D (Work Order)
                MainForm_TextBox_Transaction.Text = $"{_currentRowIndex + 1} / {_values.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while starting Easy Inventory: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void InteractWithVisual()
        {
            try
            {
                MainForm_Button_Next.Enabled = false;
                VisualLogger visualLogger = new VisualLogger();

                // Check if the current row is a "NEW TRANSACTION"
                bool isNewTransaction = visualLogger.IsNewTransaction(MainForm_TextBox_From.Text);

                if (isNewTransaction)
                {
                    // Handle "NEW TRANSACTION" logic
                    visualLogger.HandleNewTransaction();
                }
                else
                {
                    // Pass the isNewTransaction value explicitly
                    visualLogger.OpenVisualCommand(isNewTransaction);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while interacting with Visual: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        #endregion

        private void MainForm_Button_SentOR_Click(object sender, EventArgs e)
        {
            VisualLogger visualLogger = new VisualLogger();
            visualLogger.SendDataToGoogleSheet("MainForm_Button_SentOR");
        }

        private void MainForm_Button_SendClosed_Click(object sender, EventArgs e)
        {
            VisualLogger visualLogger = new VisualLogger();
            visualLogger.SendDataToGoogleSheet("MainForm_Button_SendClosed");
        }

        private void MainForm_Button_SendAddRemove_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the quantity from Visual
                VisualLogger visualLogger = new VisualLogger();
                int visualQuantity = visualLogger.GetQuantityFromAutomationId("4143"); // Automation ID for quantity in Visual

                // Check if the quantity in MainForm_TextBox_TagQty matches the quantity in Visual
                if (
                    int.TryParse(MainForm_TextBox_TagQty.Text, out int tagQuantity)
                    && tagQuantity == visualQuantity
                )
                {
                    // Show an error message if the quantities match
                    MessageBox.Show(
                        "The tag quantity matches the quantity in Visual. Please enter a different value.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return; // Stop further execution
                }

                // Proceed with sending data to Google Sheets
                VisualLogger visualLoggerInstance = new VisualLogger();
                visualLoggerInstance.SendDataToGoogleSheet("MainForm_Button_SendAddRemove");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void MainForm_TextBox_TagQty_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Check if the text in MainForm_TextBox_TagQty is a valid number
                if (int.TryParse(MainForm_TextBox_TagQty.Text, out _))
                {
                    // Enable the button if the text is a valid number
                    MainForm_Button_SendAddRemove.Enabled = true;
                }
                else
                {
                    // Disable the button and clear the text if it's not a valid number
                    MainForm_Button_SendAddRemove.Enabled = false;
                    MainForm_TextBox_TagQty.Clear();
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void MainForm_TextBox_Notes_TextChanged(object sender, EventArgs e) { }

        private void MainForm_MenuStrip_Email_Click(object sender, EventArgs e)
        {
            try
            {
                // Google Sheets API setup
                var filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "visual-easy-inventory-3e055e946d7c.json"
                );
                var credential = GoogleCredential
                    .FromFile(filePath)
                    .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

                var service = new SheetsService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "EasyInventory",
                    }
                );

                // Define the spreadsheet ID
                string spreadsheetId = "1QO0byGw_hJ35FpQUnUaZw_16zhiHXwVgrppB4-JF2TY";

                // Define the email sheets
                var emailSheets = new List<string>
                {
                    "E-Mail: Over Receipts",
                    "E-Mail: Work Order Closed",
                    "E-Mail: Add Remove",
                };

                // Create a new form to display the DataGrids
                var displayForm = new Form
                {
                    Text = "Email Tables",
                    Width = 800,
                    Height = 600,
                };

                // Create a TabControl to hold the DataGrids
                var tabControl = new TabControl { Dock = DockStyle.Fill };

                // Read data from each sheet and populate a DataGrid
                foreach (var sheetName in emailSheets)
                {
                    // Read the header row (row 1)
                    string headerRange = $"{sheetName}!A1:Z1"; // Row 1 for headers
                    var headerRequest = service.Spreadsheets.Values.Get(spreadsheetId, headerRange);
                    var headerResponse = headerRequest.Execute();

                    // Read the data starting from row 2
                    string dataRange = $"{sheetName}!A2:Z"; // Start from row 2 for data
                    var dataRequest = service.Spreadsheets.Values.Get(spreadsheetId, dataRange);
                    var dataResponse = dataRequest.Execute();

                    // Create a DataGrid for the sheet
                    var dataGrid = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        ReadOnly = true,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        ClipboardCopyMode =
                            DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText,
                        CellBorderStyle = DataGridViewCellBorderStyle.Single, // Set borders for cells
                        GridColor = System.Drawing.Color.Black, // Set grid color to black
                    };

                    // Add columns based on the header row
                    if (headerResponse.Values != null && headerResponse.Values.Count > 0)
                    {
                        var headers = headerResponse.Values[0]; // First row contains headers
                        foreach (var header in headers)
                        {
                            dataGrid.Columns.Add(header.ToString(), header.ToString());
                        }
                    }
                    else
                    {
                        dataGrid.Columns.Add("NoData", "No Data");
                    }

                    // Add rows based on the data
                    if (dataResponse.Values != null && dataResponse.Values.Count > 0)
                    {
                        foreach (var row in dataResponse.Values)
                        {
                            dataGrid.Rows.Add(row.ToArray());
                        }
                    }
                    else
                    {
                        dataGrid.Rows.Add("No data found.");
                    }

                    // Add the DataGrid to a new tab
                    var tabPage = new TabPage(sheetName) { Controls = { dataGrid } };
                    tabControl.TabPages.Add(tabPage);
                }

                // Add the TabControl to the form
                displayForm.Controls.Add(tabControl);

                // Show the form
                displayForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
