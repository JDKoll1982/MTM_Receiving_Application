using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Windows.Documents;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.WinForms.Add;
using MySql.Data.MySqlClient;
using static MTM_Waitlist_Application_2._0.WinForms.New_Request.Classes.NewRequestDao;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public partial class NewJobSetup : Form
    {
        public static readonly FormsSqlCommands FormsSqlCommands = new FormsSqlCommands();

        public static readonly NewJobSetupSaveFunctions SaveFunctions = new NewJobSetupSaveFunctions();

        public static string SetupNotes { get; set; } = string.Empty;

        private bool _startup = false;

        private bool _isWorkOrderComboBoxChanging = false;

        readonly DataTable _newJobSetupComboBoxWorkStationDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxWorkStationDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxPartIdDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxPartIdDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxCoilDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxCoilDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxSkidsDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxSkidsDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxContainerDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxContainerDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxCardboardDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxCardboardDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxBoxDataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxBoxDataAdapter = new();

        readonly DataTable _newJobSetupComboBoxOther1DataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxOther1DataAdapter = new();

        readonly DataTable _newJobSetupComboBoxOther2DataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxOther2DataAdapter = new();

        readonly DataTable _newJobSetupComboBoxOther3DataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxOther3DataAdapter = new();

        readonly DataTable _newJobSetupComboBoxOther4DataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxOther4DataAdapter = new();

        readonly DataTable _newJobSetupComboBoxOther5DataTable = new();
        readonly MySqlDataAdapter _newJobSetupComboBoxOther5DataAdapter = new();


        public NewJobSetup(string workCenter)
        {
            try
            {
                InitializeComponent();
                NewJobSetup_Form_Load(this, EventArgs.Empty, workCenter);
                FormsSqlCommands.PurgeDies();
                _isWorkOrderComboBoxChanging = false;
                NewJobSetup_ComboBox_PartID.SelectedIndexChanged += NewJobSetupComponents.NewJobSetup_ComboBox_PartID_SelectedIndexChanged;
                NewJobSetup_Button_SaveChanges.Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void NewJobSetup_Form_Load(object sender, EventArgs e, string workCenter)
        {
            try
            {
                StartupFillBaseComboBoxes(workCenter);

                NewJobSetup_ComboBox_WorkStation.Text = workCenter;

                List<WorkOrder> workOrders = NewJobSetup_ComboBox_WorkOrders_Fill();
                NewJobSetup_ComboBox_WorkOrders_Update(workOrders);

                NewJobSetup_ComboBox_Op.Enabled = false;
                NewJobSetup_ComboBox_Die.Enabled = false; // Disable Die ComboBox
                NewJobSetup_ComboBox_Coil.Enabled = false; // Disable Component ComboBox

                NewJobSetup_ComboBox_PartID_SelectedIndexChanged(null, null);
                NewJobSetup_ComboBox_WorkOrder_SelectedIndexChanged(null, null);

                NewJobSetup_RadioButton_WorkInProgress.CheckedChanged += RadioButtonChecked!;
                NewJobSetup_RadioButton_OutsideService.CheckedChanged += RadioButtonChecked!;
                NewJobSetup_RadioButton_FinishedGoods.CheckedChanged += RadioButtonChecked!;

                NewJobSetup_ComboBox_WorkOrders.Select();

            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void StartupFillBaseComboBoxes(string workCenter)
        {
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_WorkStation, "work_center", "mtm_waitlist", "application_database", _newJobSetupComboBoxWorkStationDataAdapter, _newJobSetupComboBoxWorkStationDataTable, "Enter Work Station");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_PartID, "PartID", "mtm_waitlist", "waitlist_jobs", _newJobSetupComboBoxPartIdDataAdapter, _newJobSetupComboBoxPartIdDataTable, "Enter a Part ID");
            NewJobSetup_ComboBox_Op.Items.Add("Enter Operation");
            NewJobSetup_ComboBox_Die.Items.Add("Enter Die");
            NewJobSetup_TextBox_DieLocation.Text = @"No Die Selected";

            StartupFillSecondaryComboBoxes();
        }

        private void StartupFillSecondaryComboBoxes()
        {
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Coil, "Component", "mtm_waitlist", "waitlist_components", _newJobSetupComboBoxCoilDataAdapter, _newJobSetupComboBoxCoilDataTable, "Enter Coil");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Skid, "Skid", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxSkidsDataAdapter, _newJobSetupComboBoxSkidsDataTable, "Enter Skid");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Container, "Dunnage", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxContainerDataAdapter, _newJobSetupComboBoxContainerDataTable, "Enter Container");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Cardboard, "Cardboard", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxCardboardDataAdapter, _newJobSetupComboBoxCardboardDataTable, "Enter Cardboard");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Box, "Box", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxBoxDataAdapter, _newJobSetupComboBoxBoxDataTable, "Enter Box");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Other1, "Other", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxOther1DataAdapter, _newJobSetupComboBoxOther1DataTable, "Enter Other");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Other2, "Other", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxOther2DataAdapter, _newJobSetupComboBoxOther2DataTable, "Enter Other");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Other3, "Other", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxOther3DataAdapter, _newJobSetupComboBoxOther3DataTable, "Enter Other");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Other4, "Other", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxOther4DataAdapter, _newJobSetupComboBoxOther4DataTable, "Enter Other");
            ComboBoxHelper.ResetAndFillComboBox(NewJobSetup_ComboBox_Other5, "Other", "mtm_waitlist", "waitlist_dunnage", _newJobSetupComboBoxOther5DataAdapter, _newJobSetupComboBoxOther5DataTable, "Enter Other");
            _startup = true;
        }

        private List<WorkOrder> NewJobSetup_ComboBox_WorkOrders_Fill()
        {
            List<WorkOrder> workOrders = new();
            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT WorkOrder FROM waitlist_workorders", connection);

                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string workOrderNumber = reader.GetString(0);
                    string partId = NewJobSetup_ComboBox_WorkOrder_GetPartIDFromWorkOrder(workOrderNumber);
                    workOrders.Add(new WorkOrder { WorkOrderNumber = workOrderNumber, PartId = partId });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return workOrders;
        }

        private void NewJobSetup_ComboBox_WorkOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NewJobSetup_ComboBox_WorkOrders.SelectedIndex == 0)
            {
                NewJobSetup_ComboBox_PartID.SelectedIndex = 0;
                ResetAll();
                return;
            }

            _isWorkOrderComboBoxChanging = true;

            try
            {
                string workOrderNumber = NewJobSetup_ComboBox_WorkOrders.Text;
                string partId = NewJobSetup_ComboBox_WorkOrder_GetPartIDFromWorkOrder(workOrderNumber);

                if (!string.IsNullOrEmpty(partId))
                {
                    NewJobSetup_ComboBox_PartID.Text = partId;
                    UpdatePartDescription(partId);
                    ChangeOperationComboBox();
                    ChangeDieComboBox();
                    ChangeCoilComboBox();
                    ChangeContainerComboBox();
                    ChangeSkidComboBox();
                    ChangeCardboardComboBox();
                    ChangeBoxesComboBox();
                    ChangeOtherComboBoxes();
                    ChangeRadioButtons();
                    CheckSetupNotes();
                }
            }
            finally
            {
                _isWorkOrderComboBoxChanging = false;
            }
        }

        private void ResetAll()
        {
            NewJobSetup_ComboBox_Op.DataSource = null;
            NewJobSetup_ComboBox_Op.Items.Clear();
            NewJobSetup_ComboBox_Op.Items.Add("Enter Operation");
            DisableComboBox(NewJobSetup_ComboBox_Op);
            if (NewJobSetup_ComboBox_Op.Items.Count > 0) NewJobSetup_ComboBox_Op.SelectedIndex = 0;

            NewJobSetup_ComboBox_Die.DataSource = null;
            NewJobSetup_ComboBox_Die.Items.Clear();
            NewJobSetup_ComboBox_Die.Items.Add("Enter Die");
            NewJobSetup_TextBox_DieLocation.Text = @"No Die Selected";
            DisableComboBox(NewJobSetup_ComboBox_Die);
            if (NewJobSetup_ComboBox_Die.Items.Count > 0) NewJobSetup_ComboBox_Die.SelectedIndex = 0;

            NewJobSetup_ComboBox_Coil.DataSource = null;
            NewJobSetup_ComboBox_Coil.Items.Clear();
            NewJobSetup_ComboBox_Coil.Items.Add("Enter Coil");
            DisableComboBox(NewJobSetup_ComboBox_Coil);
            if (NewJobSetup_ComboBox_Coil.Items.Count > 0) NewJobSetup_ComboBox_Coil.SelectedIndex = 0;

            NewJobSetup_ComboBox_Component1.DataSource = null;
            NewJobSetup_ComboBox_Component1.Items.Clear();
            NewJobSetup_ComboBox_Component1.Enabled = false;
            NewJobSetup_ComboBox_Component1.Visible = false;

            NewJobSetup_ComboBox_Component2.DataSource = null;
            NewJobSetup_ComboBox_Component2.Items.Clear();
            NewJobSetup_ComboBox_Component2.Enabled = false;
            NewJobSetup_ComboBox_Component2.Visible = false;

            NewJobSetup_ComboBox_Component3.DataSource = null;
            NewJobSetup_ComboBox_Component3.Items.Clear();
            NewJobSetup_ComboBox_Component3.Enabled = false;
            NewJobSetup_ComboBox_Component3.Visible = false;

            NewJobSetup_ComboBox_Component4.DataSource = null;
            NewJobSetup_ComboBox_Component4.Items.Clear();
            NewJobSetup_ComboBox_Component4.Enabled = false;
            NewJobSetup_ComboBox_Component4.Visible = false;

            NewJobSetup_ComboBox_Component5.DataSource = null;
            NewJobSetup_ComboBox_Component5.Items.Clear();
            NewJobSetup_ComboBox_Component5.Enabled = false;
            NewJobSetup_ComboBox_Component5.Visible = false;

            NewJobSetup_ComboBox_Component6.DataSource = null;
            NewJobSetup_ComboBox_Component6.Items.Clear();
            NewJobSetup_ComboBox_Component6.Enabled = false;
            NewJobSetup_ComboBox_Component6.Visible = false;

            NewJobSetup_ComboBox_Component7.DataSource = null;
            NewJobSetup_ComboBox_Component7.Items.Clear();
            NewJobSetup_ComboBox_Component7.Enabled = false;
            NewJobSetup_ComboBox_Component7.Visible = false;

            NewJobSetup_ComboBox_Component8.DataSource = null;
            NewJobSetup_ComboBox_Component8.Items.Clear();
            NewJobSetup_ComboBox_Component8.Enabled = false;
            NewJobSetup_ComboBox_Component8.Visible = false;

            NewJobSetup_ComboBox_Component9.DataSource = null;
            NewJobSetup_ComboBox_Component9.Items.Clear();
            NewJobSetup_ComboBox_Component9.Enabled = false;
            NewJobSetup_ComboBox_Component9.Visible = false;

            NewJobSetup_ComboBox_Component10.DataSource = null;
            NewJobSetup_ComboBox_Component10.Items.Clear();
            NewJobSetup_ComboBox_Component10.Enabled = false;
            NewJobSetup_ComboBox_Component10.Visible = false;

            NewJobSetup_ComboBox_Component11.DataSource = null;
            NewJobSetup_ComboBox_Component11.Items.Clear();
            NewJobSetup_ComboBox_Component11.Enabled = false;
            NewJobSetup_ComboBox_Component11.Visible = false;

            NewJobSetup_ComboBox_Component12.DataSource = null;
            NewJobSetup_ComboBox_Component12.Items.Clear();
            NewJobSetup_ComboBox_Component12.Enabled = false;
            NewJobSetup_ComboBox_Component12.Visible = false;

            if (NewJobSetup_ComboBox_Container.Items.Count > 0) NewJobSetup_ComboBox_Container.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Skid.Items.Count > 0) NewJobSetup_ComboBox_Skid.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Cardboard.Items.Count > 0) NewJobSetup_ComboBox_Cardboard.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Box.Items.Count > 0) NewJobSetup_ComboBox_Box.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Other1.Items.Count > 0) NewJobSetup_ComboBox_Other1.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Other2.Items.Count > 0) NewJobSetup_ComboBox_Other2.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Other3.Items.Count > 0) NewJobSetup_ComboBox_Other3.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Other4.Items.Count > 0) NewJobSetup_ComboBox_Other4.SelectedIndex = 0;

            if (NewJobSetup_ComboBox_Other5.Items.Count > 0) NewJobSetup_ComboBox_Other5.SelectedIndex = 0;

            NewJobSetup_TextBox_PartDescription.Text = string.Empty;

            NewJobSetup_RadioButton_FinishedGoods.Checked = false;

            NewJobSetup_RadioButton_OutsideService.Checked = false;

            NewJobSetup_RadioButton_WorkInProgress.Checked = false;

            NewJobSetup_CheckBox_NoteExists.Checked = false;
        }

        private void NewJobSetup_ComboBox_PartID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_startup == true)
                {
                    ResetAll();
                }
                if (NewJobSetup_ComboBox_PartID.SelectedIndex == 0)
                {
                    NewJobSetup_Button_SaveChanges.Enabled = false;
                    NewJobSetup_Button_ShowNotes.Enabled = false;
                    if (_startup == true)
                    {
                        ResetAll();
                    }
                }
                else
                {
                    NewJobSetup_Button_SaveChanges.Enabled = true;
                    NewJobSetup_Button_ShowNotes.Enabled = true;
                    ChangeOperationComboBox();
                    NewJobSetup_ComboBox_Operation_SelectedIndexChanged(null, null);

                    UpdatePartDescription(NewJobSetup_ComboBox_PartID.Text);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void NewJobSetup_ComboBox_Operation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isWorkOrderComboBoxChanging)
                return;

            if (NewJobSetup_ComboBox_PartID.SelectedIndex > 0)
            {
                ChangeDieComboBox();
                ChangeCoilComboBox();
                ChangeContainerComboBox();
                ChangeSkidComboBox();
                ChangeCardboardComboBox();
                ChangeBoxesComboBox();
                ChangeOtherComboBoxes();
                ChangeRadioButtons();
                CheckSetupNotes();
            }

        }

        private void ChangeOperationComboBox()
        {
            NewJobSetupChangeHelper.ChangeOperationComboBox(NewJobSetup_ComboBox_Op, NewJobSetup_ComboBox_PartID, DebugTextBox);
        }

        private void ChangeDieComboBox()
        {
            NewJobSetupChangeHelper.ChangeDieComboBox(NewJobSetup_ComboBox_Die, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op, NewJobSetup_TextBox_DieLocation);
        }

        private void ChangeCoilComboBox()
        {
            NewJobSetupChangeHelper.ChangeCoilComboBox(NewJobSetup_ComboBox_Coil, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op, NewJobSetup_ComboBox_Die, NewJobSetup_Label_Coil);
        }

        private void ChangeContainerComboBox()
        {
            NewJobSetupChangeHelper.ChangeContainerComboBox(NewJobSetup_ComboBox_Container, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op);
        }

        private void ChangeSkidComboBox()
        {
            NewJobSetupChangeHelper.ChangeSkidComboBox(NewJobSetup_ComboBox_Skid, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op);
        }

        private void ChangeCardboardComboBox()
        {
            NewJobSetupChangeHelper.ChangeCardboardComboBox(NewJobSetup_ComboBox_Cardboard, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op);
        }

        private void ChangeBoxesComboBox()
        {
            NewJobSetupChangeHelper.ChangeBoxesComboBox(NewJobSetup_ComboBox_Box, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op);
        }

        private void ChangeOtherComboBoxes()
        {
            NewJobSetupChangeHelper.ChangeOtherComboBoxes(NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op, NewJobSetup_ComboBox_Other1, NewJobSetup_ComboBox_Other2, NewJobSetup_ComboBox_Other3, NewJobSetup_ComboBox_Other4, NewJobSetup_ComboBox_Other5);
        }

        private void ChangeRadioButtons()
        {
            NewJobSetupChangeHelper.ChangeRadioButtons(NewJobSetup_RadioButton_WorkInProgress, NewJobSetup_RadioButton_OutsideService, NewJobSetup_RadioButton_FinishedGoods, NewJobSetup_ComboBox_PartID, NewJobSetup_ComboBox_Op);
        }

        private void CheckSetupNotes()
        {
            ObservableCollection<string> setupNotes = FormsSqlCommands.ComboBoxFillerFilterTwo(0, "SetupNotes", "mtm_waitlist",
                "waitlist_setupnotes", "PartID",
                NewJobSetup_ComboBox_PartID.Text, "Operation", NewJobSetup_ComboBox_Op.Text);

            if (setupNotes.Count > 0)
            {
                SetupNotes = setupNotes[0];
                NewJobSetup_CheckBox_NoteExists.Checked = true;
            }
            else
            {
                SetupNotes = string.Empty;
                NewJobSetup_CheckBox_NoteExists.Checked = false;
            }
        }

        private void RadioButtonChecked(object sender, EventArgs e)
        {
            if (sender is RadioButton selectedRadioButton && selectedRadioButton.Checked)
            {
                // Uncheck the other RadioButtons
                if (selectedRadioButton != NewJobSetup_RadioButton_WorkInProgress)
                {
                    NewJobSetup_RadioButton_WorkInProgress.Checked = false;
                }
                if (selectedRadioButton != NewJobSetup_RadioButton_OutsideService)
                {
                    NewJobSetup_RadioButton_OutsideService.Checked = false;
                }
                if (selectedRadioButton != NewJobSetup_RadioButton_FinishedGoods)
                {
                    NewJobSetup_RadioButton_FinishedGoods.Checked = false;
                }
            }
        }

        private void DisableComboBox(ComboBox comboBox)
        {
            comboBox.Enabled = false;
        }

        private void UpdatePartDescription(string partId)
        {
            try
            {
                string connectionString = SqlCommands.GetConnectionString(null, null, null, null);
                string query = "SELECT Description FROM waitlist_jobs WHERE PartID = @PartID";

                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@PartID", partId);

                connection.Open();
                object? result = command.ExecuteScalar();
                NewJobSetup_TextBox_PartDescription.Text = result?.ToString() ?? "No description found";
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void NewJobSetup_ComboBox_WorkOrders_Update(List<WorkOrder> workOrders)
        {
            try
            {
                workOrders.Insert(0, new WorkOrder { WorkOrderNumber = "Enter Work Order", PartId = "Enter Part ID" });
                var filteredWorkOrders = NewJobSetup_ComboBox_WorkOrders_Filter(workOrders);
                NewJobSetup_ComboBox_WorkOrders.DisplayMember = "WorkOrder";
                NewJobSetup_ComboBox_WorkOrders.DataSource = new ObservableCollection<WorkOrder>(filteredWorkOrders);
                NewJobSetup_ComboBox_WorkOrders.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public List<WorkOrder> NewJobSetup_ComboBox_WorkOrders_Filter(List<WorkOrder> workOrders)
        {
            try
            {
                return workOrders.Where(wo => !string.IsNullOrWhiteSpace(wo.PartId)).ToList();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return new List<WorkOrder>();
            }
        }

        private string? NewJobSetup_ComboBox_WorkOrder_GetPartIDFromWorkOrder(string workOrderNumber)
        {
            string? partId = null;
            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT PartID FROM waitlist_workorders WHERE WorkOrder = @WorkOrderNumber", connection);
                command.Parameters.AddWithValue("@WorkOrderNumber", workOrderNumber);

                connection.Open();
                partId = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return partId;
        }

        private void ShowAddNewForm(Type formType)
        {
            try
            {
                Form formInstance = (Form)Activator.CreateInstance(formType)!;
                formInstance.Show();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void NewJobSetup_MenuStrip_AddNewBox(object sender, EventArgs e)
        {
            ShowAddNewForm(typeof(AddNewBox));
        }

        private void NewJobSetup_MenuStrip_AddNewCardBoard(object sender, EventArgs e)
        {
            ShowAddNewForm(typeof(AddNewCardboard));
        }

        private void NewJobSetup_MenuStrip_AddNewDunnage(object sender, EventArgs e)
        {
            ShowAddNewForm(typeof(AddNewDunnage));
        }

        private void NewJobSetup_MenuStrip_AddNewOther(object sender, EventArgs e)
        {
            ShowAddNewForm(typeof(AddNewOther));
        }

        private void NewJobSetup_MenuStrip_AddNewSkid(object sender, EventArgs e)
        {
            ShowAddNewForm(typeof(AddNewSkid));
        }

        private void NewJobSetup_Button_SaveChanges_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Any previously saved data will be overwritten. Do you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }

            // Existing code for saving changes
            string partid = NewJobSetup_ComboBox_PartID.Text;
            string op = NewJobSetup_ComboBox_Op.Text;
            string dunnage = GetComboBoxText(NewJobSetup_ComboBox_Container);
            string skid = GetComboBoxText(NewJobSetup_ComboBox_Skid);
            string cardboard = GetComboBoxText(NewJobSetup_ComboBox_Cardboard);
            string boxes = GetComboBoxText(NewJobSetup_ComboBox_Box);
            string other1 = GetComboBoxText(NewJobSetup_ComboBox_Other1);
            string other2 = GetComboBoxText(NewJobSetup_ComboBox_Other2);
            string other3 = GetComboBoxText(NewJobSetup_ComboBox_Other3);
            string other4 = GetComboBoxText(NewJobSetup_ComboBox_Other4);
            string other5 = GetComboBoxText(NewJobSetup_ComboBox_Other5);
            string notes = SetupNotes;

            // Assuming you have RadioButtons for part type
            string parttype = GetSelectedPartType();

            if (string.IsNullOrEmpty(parttype))
            {
                MessageBox.Show("Please select a Completed Part Type.");
                return;
            }

            // Call the CheckDunnageSaveData method
            SaveFunctions.CheckDunnageSaveData(0, partid, op, dunnage, skid, cardboard, boxes, other1, other2, other3, other4, other5, parttype);
            SaveFunctions.SaveSetupNotes(partid, op, notes);
            // Call ResetAll after saving is complete
            NewJobSetup_ComboBox_WorkOrders.SelectedIndex = 0;
            ResetAll();
        }

        private string GetComboBoxText(ComboBox comboBox)
        {
            return comboBox.SelectedIndex > 0 ? comboBox.Text : string.Empty;
        }

        private string GetSelectedPartType()
        {
            // Assuming you have RadioButtons for part type
            if (NewJobSetup_RadioButton_FinishedGoods.Checked) return NewJobSetup_RadioButton_FinishedGoods.Text;
            if (NewJobSetup_RadioButton_OutsideService.Checked) return NewJobSetup_RadioButton_OutsideService.Text;
            if (NewJobSetup_RadioButton_WorkInProgress.Checked) return NewJobSetup_RadioButton_WorkInProgress.Text;
            // Add more RadioButtons as needed

            return null;
        }

        private void NewJobSetup_Button_Cancel_Click(object sender, EventArgs e)
        {
            if (NewJobSetup_ComboBox_PartID.SelectedIndex != 0)
            {
                var result = MessageBox.Show("You will lose any unsaved changes. Do you want to continue?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel)
                {
                    return; // Do not close the form
                }
            }
            this.Close();
        }

        private void NewJobSetup_Button_Reset_Click(object sender, EventArgs e)
        {
            ResetAll();
            NewJobSetup_ComboBox_WorkOrders.SelectedIndex = 0;
            NewJobSetup_ComboBox_PartID.SelectedIndex = 0;
            NewJobSetup_ComboBox_WorkStation.SelectedIndex = 0;
            NewJobSetup_ComboBox_WorkStation.Select();
        }

        private void NewJobSetup_Button_ShowNotes_Click(object sender, EventArgs e)
        {
            NewJobSetupSetupNotes newJobSetupSetupNotes = new();
            newJobSetupSetupNotes.ShowDialog();
        }

        private void NewJobSetup_Button_Save_Click(object sender, EventArgs e)
        {

            PooledData pooled = GeneratePooledData();

            SqlCommands.AddToWaitlistSetup(pooled);

            NewJobSetup_Button_SaveChanges_Click(null, null);

        }

        private PooledData GeneratePooledData()
        {
            PooledData pooled = new PooledData();
            pooled.JobType = "New Job Setup";
            pooled.Priority = "Normal";
            pooled.JobStatus = "New";
            pooled.WorkStation = NewJobSetup_ComboBox_WorkStation.Text;
            pooled.WorkOrder = NewJobSetup_ComboBox_WorkOrders.Text;
            pooled.PartNumber = NewJobSetup_ComboBox_PartID.Text;
            pooled.Operation = NewJobSetup_ComboBox_Op.Text;
            pooled.PartDescription = NewJobSetup_TextBox_PartDescription.Text;
            pooled.DieFgt = NewJobSetup_ComboBox_Die.Text;
            pooled.DieLocation = NewJobSetup_TextBox_DieLocation.Text;
            pooled.Component1 = NewJobSetup_ComboBox_Coil.Text;
            pooled.Component2 = NewJobSetup_ComboBox_Component1.Text;
            pooled.Component3 = NewJobSetup_ComboBox_Component2.Text;
            pooled.Component4 = NewJobSetup_ComboBox_Component3.Text;
            pooled.Component5 = NewJobSetup_ComboBox_Component4.Text;
            pooled.Component6 = NewJobSetup_ComboBox_Component5.Text;
            pooled.Component7 = NewJobSetup_ComboBox_Component6.Text;
            pooled.Component8 = NewJobSetup_ComboBox_Component7.Text;
            pooled.Component9 = NewJobSetup_ComboBox_Component8.Text;
            pooled.Component10 = NewJobSetup_ComboBox_Component9.Text;
            pooled.Component11 = NewJobSetup_ComboBox_Component10.Text;
            pooled.Component12 = NewJobSetup_ComboBox_Component11.Text;
            pooled.Container = NewJobSetup_ComboBox_Container.Text;
            pooled.Skid = NewJobSetup_ComboBox_Skid.Text;
            pooled.Cardboard = NewJobSetup_ComboBox_Cardboard.Text;
            pooled.Box = NewJobSetup_ComboBox_Box.Text;
            pooled.Other1 = NewJobSetup_ComboBox_Other1.Text;
            pooled.Other2 = NewJobSetup_ComboBox_Other2.Text;
            pooled.Other3 = NewJobSetup_ComboBox_Other3.Text;
            pooled.Other4 = NewJobSetup_ComboBox_Other4.Text;
            pooled.Other5 = NewJobSetup_ComboBox_Other5.Text;
            pooled.SetupNote = SetupNotes;
            pooled.PartType = GetSelectedPartType();

            pooled.Operator = string.Empty;
            pooled.SetupTech = string.Empty;
            pooled.MaterialHandler = string.Empty;

            pooled.StartTime = string.Empty;
            pooled.EndTime = string.Empty;
            pooled.ElapsedTime = string.Empty;



            return pooled;
        }
    }


    public class PooledData
    {
        public string? JobType { get; set; }
        public string? Priority { get; set; }
        public string? JobStatus { get; set; }
        public string? WorkStation { get; set; }
        public string? WorkOrder { get; set; }
        public string? PartNumber { get; set; }
        public string? Operation { get; set; }
        public string? PartDescription { get; set; }
        public string? DieFgt { get; set; }
        public string? DieLocation { get; set; }
        public string? Component1 { get; set; }
        public string? Component2 { get; set; }
        public string? Component3 { get; set; }
        public string? Component4 { get; set; }
        public string? Component5 { get; set; }
        public string? Component6 { get; set; }
        public string? Component7 { get; set; }
        public string? Component8 { get; set; }
        public string? Component9 { get; set; }
        public string? Component10 { get; set; }
        public string? Component11 { get; set; }
        public string? Component12 { get; set; }
        public string? Container { get; set; }
        public string? Skid { get; set; }
        public string? Cardboard { get; set; }
        public string? Box { get; set; }
        public string? Other1 { get; set; }
        public string? Other2 { get; set; }
        public string? Other3 { get; set; }
        public string? Other4 { get; set; }
        public string? Other5 { get; set; }
        public string? SetupNote { get; set; }
        public string? Operator { get; set; }
        public string? SetupTech { get; set; }
        public string? MaterialHandler { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? ElapsedTime { get; set; }
        public string? PartType { get; set; }
    }

    public class WorkOrder
    {
        public string? WorkOrderNumber { get; set; }
        public string? PartId { get; set; }
        public override string? ToString()
        {
            try
            {
                return WorkOrderNumber;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return string.Empty;
            }
        }
    }
}