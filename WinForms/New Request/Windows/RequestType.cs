using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup;
using MTM_Waitlist_Application_2._0.WinForms.New_Request.Classes;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Request.Windows
{
    public partial class NewRequest_Window_RequestType : Form
    {

        public static NewRequestDao NewRequestDao = new NewRequestDao();

        readonly DataTable _workCenterDataTable = new();
        readonly MySqlDataAdapter _workCenterDataAdapter = new();

        public NewRequest_Window_RequestType()
        {
            InitializeComponent();

            OnStartup_FillComboBoxes();
        }

        private void OnStartup_FillComboBoxes()
        {
            FillComboBox(RequestType_ComboBox_WorkCenter, "work_center", "mtm_waitlist", "application_database", _workCenterDataAdapter, _workCenterDataTable, "Select Work Center");
        }

        public static void FillComboBox(ComboBox comboBox, string column, string database, string table, MySqlDataAdapter dataAdapter, DataTable dataTable, string placeHolder)
        {
            comboBox.DataSource = NewRequestDao.ComboBoxFiller(column, table, dataAdapter, dataTable, placeHolder);
            comboBox.DisplayMember = column; // Set the DisplayMember to the column name
            comboBox.SelectedIndex = 0;
        }

        public void RequestType_ComboBox_WorkCenter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RequestType_ComboBox_WorkCenter.SelectedIndex <= 0)
            {
                RequestType_ComboBox_WorkOrder.Text = "";
                RequestType_ComboBox_WorkOrder.Enabled = false;
                RequestType_Button_Bring.Enabled = false;
                RequestType_Button_Coil.Enabled = false;
                RequestType_Button_Pickup.Enabled = false;
                RequestType_Button_NewJobSetup.Enabled = false;
                RequestType_Button_Scrap.Enabled = false;
                RequestType_Button_Die.Enabled = false;

                RequestType_ComboBox_PartID.Text = "";
                RequestType_ComboBox_PartID.Enabled = false;

                RequestType_ComboBox_Operation.Text = "";
                RequestType_ComboBox_Operation.Enabled = false;
            }
            else
            {
                NewRequestDao.OnStatrup_StepOne(RequestType_ComboBox_WorkOrder, RequestType_ComboBox_PartID, RequestType_ComboBox_Operation, RequestType_ComboBox_WorkCenter.Text);
                if (RequestType_ComboBox_PartID.Text != "")
                {
                    RequestType_Button_Bring.Enabled = true;
                    RequestType_Button_Coil.Enabled = true;
                    RequestType_Button_Pickup.Enabled = true;
                    RequestType_Button_NewJobSetup.Enabled = true;
                    RequestType_Button_Scrap.Enabled = true;
                    RequestType_Button_Die.Enabled = true;
                }
                else
                {
                    RequestType_Button_Bring.Enabled = false;
                    RequestType_Button_Coil.Enabled = false;
                    RequestType_Button_Pickup.Enabled = false;
                    RequestType_Button_NewJobSetup.Enabled = true;
                    RequestType_Button_Scrap.Enabled = false;
                    RequestType_Button_Die.Enabled = false;
                }
            }
        }

        private void RequestType_Button_NewJobSetup_Click(object sender, EventArgs e)
        {
            NewJobSetup newJobSetup = new NewJobSetup(RequestType_ComboBox_WorkCenter.Text);
            newJobSetup.Show();
        }

        private void RequestType_Button_Pickup_Click(object sender, EventArgs e)
        {
            NewRequest_Window_Pickup newRequestWindowPickup = new NewRequest_Window_Pickup(RequestType_ComboBox_WorkCenter.Text, RequestType_ComboBox_WorkOrder.Text, RequestType_ComboBox_PartID.Text, RequestType_ComboBox_Operation.Text);
            newRequestWindowPickup.Show();
            newRequestWindowPickup.FormClosed += (s, args) => this.Close();
        }
    }
}
