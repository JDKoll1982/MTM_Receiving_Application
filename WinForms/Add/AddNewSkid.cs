using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    public partial class AddNewSkid : Form
    {
        public string SkidX { get; set; }
        public string SkidY { get; set; }
        public string SkidFull { get; set; }

        public AddNewSkid()
        {
            InitializeComponent();
            AddNewSkid_TextBox_X.KeyPress += new KeyPressEventHandler(AddNewSkid_TextBox_X_KeyPress!);
            AddNewSkid_TextBox_X.TextChanged += new EventHandler(AddNewSkid_TextBox_X_TextChanged!);
            AddNewSkid_TextBox_Y.KeyPress += new KeyPressEventHandler(AddNewSkid_TextBox_Y_KeyPress!);
            AddNewSkid_TextBox_Y.TextChanged += new EventHandler(AddNewSkid_TextBox_Y_TextChanged!);
            AddNewSkid_Button_Save.Click += new EventHandler(SaveButton_Click!);
            AddNewSkid_Button_Cancel.Click += new EventHandler(CancelButton_Click!);
            AddNewSkid_CheckBox_KI.CheckedChanged += new EventHandler(AddNewSkid_CheckBox_KI_CheckedChanged!);

            // Initialize non-nullable properties
            SkidX = string.Empty;
            SkidY = string.Empty;
            SkidFull = string.Empty;

            // Disable the Save button initially
            AddNewSkid_Button_Save.Enabled = false;
        }

        private void AddNewSkid_TextBox_X_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace, delete, tab, etc.)
            if (char.IsControl(e.KeyChar) && e.KeyChar != (char)Keys.Tab)
            {
                return;
            }

            // Allow digits (0-9)
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Allow one decimal point
            if (e.KeyChar == '.' && !AddNewSkid_TextBox_X.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewSkid_TextBox_X_TextChanged(object sender, EventArgs e)
        {
            SkidX = AddNewSkid_TextBox_X.Text;

            // Split the text into whole and decimal parts
            string[] parts = SkidX.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewSkid_TextBox_X.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewSkid_TextBox_X.SelectionStart = AddNewSkid_TextBox_X.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewSkid_TextBox_X.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewSkid_TextBox_X.SelectionStart = AddNewSkid_TextBox_X.Text.Length;
            }

            UpdateSkidFull();
        }

        private void AddNewSkid_TextBox_Y_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow control keys (backspace, delete, tab, etc.)
            if (char.IsControl(e.KeyChar) && e.KeyChar != (char)Keys.Tab)
            {
                return;
            }

            // Allow digits (0-9)
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Allow one decimal point
            if (e.KeyChar == '.' && !AddNewSkid_TextBox_Y.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewSkid_TextBox_Y_TextChanged(object sender, EventArgs e)
        {
            SkidY = AddNewSkid_TextBox_Y.Text;

            // Split the text into whole and decimal parts
            string[] parts = SkidY.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewSkid_TextBox_Y.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewSkid_TextBox_Y.SelectionStart = AddNewSkid_TextBox_Y.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewSkid_TextBox_Y.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewSkid_TextBox_Y.SelectionStart = AddNewSkid_TextBox_Y.Text.Length;
            }

            UpdateSkidFull();
        }

        private void AddNewSkid_CheckBox_KI_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSkidFull();
        }

        private void UpdateSkidFull()
        {
            if (!string.IsNullOrEmpty(SkidX) && !string.IsNullOrEmpty(SkidY))
            {
                SkidFull = $"{SkidX} x {SkidY}";
                if (AddNewSkid_CheckBox_KI.Checked)
                {
                    SkidFull = "KI Skid - " + SkidFull;
                }
                else
                {
                    SkidFull = "Skid - " + SkidFull;
                }
                AddNewSkid_TextBox_Full.Text = SkidFull;
                AddNewSkid_Button_Save.Enabled = true; // Enable the Save button
            }
            else
            {
                SkidFull = string.Empty;
                AddNewSkid_TextBox_Full.Text = string.Empty;
                AddNewSkid_Button_Save.Enabled = false; // Disable the Save button
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SkidFull))
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string selectQuery = "SELECT id FROM waitlist_dunnage WHERE Skid IS NULL OR Skid = '' LIMIT 1";
                string updateQuery = "UPDATE waitlist_dunnage SET Skid = @Skid WHERE id = @id";
                string insertQuery = "INSERT INTO waitlist_dunnage (Skid) VALUES (@Skid)";
                string checkDuplicateQuery = "SELECT COUNT(*) FROM waitlist_dunnage WHERE Skid = @Skid";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Check for duplicate Skid
                        using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, connection))
                        {
                            checkDuplicateCommand.Parameters.AddWithValue("@Skid", SkidFull);
                            int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show(@"This Skid already exists in the database.");
                                return;
                            }
                        }

                        // Find the first open spot
                        int id = -1;
                        using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                        {
                            using (MySqlDataReader reader = selectCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    id = reader.GetInt32("id");
                                }
                            }
                        }

                        if (id != -1)
                        {
                            // Update the first open spot
                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@Skid", SkidFull);
                                updateCommand.Parameters.AddWithValue("@id", id);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show(SkidFull + @" added successfully.");
                            }
                        }
                        else
                        {
                            // Insert a new row if no open spot is found
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Skid", SkidFull);
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show(SkidFull + @" added successfully.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.HandleError(nameof(SaveButton_Click), ex);
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Please enter valid values for length and width.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SkidFull))
            {
                var result = MessageBox.Show(@"You have unsaved changes. Are you sure you want to cancel?", @"Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
    }
}