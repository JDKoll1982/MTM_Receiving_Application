using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.WinForms.New_Request.Classes;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using static MTM_Waitlist_Application_2._0.Core.Database_Classes.SqlCommands;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Request.Windows
{
    public partial class NewRequest_Window_Pickup : Form
    {
        readonly NewRequestDao _newRequestDao = new NewRequestDao();
        public TimeSpan Eta = new TimeSpan();
        public TimeSpan Time = new TimeSpan();
        public DateTime TimeRemaining = DateTime.Now;

        public NewRequest_Window_Pickup(string workCenter, string workOrder, string partID, string operation)
        {
            InitializeComponent();
            NewRequest_Window_Pickup_OnStartup(workCenter, workOrder, partID, operation);
        }

        private void NewRequest_Window_Pickup_OnStartup(string workCenter, string workOrder, string partID, string operation)
        {
            var getJobTime = NewRequestDao.GetJobTime("Pickup Parts");
            Time = getJobTime.Item1;
            TimeRemaining = DateTime.Now.Add(Time);

            Pickup_TextBox_AllocatedTime.Text = getJobTime.Item2;
            Pickup_TextBox_WorkCenter.Text = workCenter;
            Pickup_TextBox_WorkOrder.Text = workOrder;
            Pickup_TextBox_PartID.Text = partID;
            Pickup_TextBox_Operation.Text = operation;

            var getEta = NewRequestDao.GetEta(Time);

            Eta = getEta.Item1;
            Pickup_TextBox_ETA.Text = getEta.Item2;
        }

        private void Pickup_Button_CompletedParts_Click(object sender, EventArgs e)
        {
            var connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);
            const string query = $"INSERT INTO `waitlist_active` (`WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `AllocatedTime`,`TimeRemaining`, `RequestTime`, `StartTime`, `PartID`, `Operation`) " +
                                 "VALUES (@WorkCenter, @RequestType, @Request, @RequestPriority, @MHandler, @AllocatedTime, @TimeRemaining, @RequestTime, null, @PartID, @Operation)";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@WorkCenter", Pickup_TextBox_WorkCenter.Text);
            command.Parameters.AddWithValue("@RequestType", "Pickup Parts");
            command.Parameters.AddWithValue("@Request", "Pickup Finished Goods. Part Number: " + Pickup_TextBox_PartID.Text);
            command.Parameters.AddWithValue("@RequestPriority", "Normal");
            command.Parameters.AddWithValue("@MHandler", "");
            command.Parameters.AddWithValue("@AllocatedTime", Time);
            command.Parameters.AddWithValue("@TimeRemaining", TimeRemaining);
            command.Parameters.AddWithValue("@RequestTime", DateTime.Now);
            command.Parameters.AddWithValue("@PartID", Pickup_TextBox_PartID.Text);
            command.Parameters.AddWithValue("@Operation", Pickup_TextBox_Operation.Text);


            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();

            this.Close();
        }

        private void Pickup_Button_NCMParts_Click(object sender, EventArgs e)
        {
            var connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);
            const string query = $"INSERT INTO `waitlist_active` (`WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `AllocatedTime`,`TimeRemaining`, `RequestTime`, `StartTime`, `PartID`, `Operation`) " +
                                 "VALUES (@WorkCenter, @RequestType, @Request, @RequestPriority, @MHandler, @AllocatedTime, @TimeRemaining, @RequestTime, null, @PartID, @Operation)";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@WorkCenter", Pickup_TextBox_WorkCenter.Text);
            command.Parameters.AddWithValue("@RequestType", "Pickup Parts");
            command.Parameters.AddWithValue("@Request", "Pickup Finished Goods. Part Number: " + Pickup_TextBox_PartID.Text +  " - Take To NCM.");
            command.Parameters.AddWithValue("@RequestPriority", "Normal");
            command.Parameters.AddWithValue("@MHandler", "");
            command.Parameters.AddWithValue("@AllocatedTime", Time);
            command.Parameters.AddWithValue("@TimeRemaining", TimeRemaining);
            command.Parameters.AddWithValue("@RequestTime", DateTime.Now);
            command.Parameters.AddWithValue("@PartID", Pickup_TextBox_PartID.Text);
            command.Parameters.AddWithValue("@Operation", Pickup_TextBox_Operation.Text);


            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();

            this.Close();
        }
    }
}
