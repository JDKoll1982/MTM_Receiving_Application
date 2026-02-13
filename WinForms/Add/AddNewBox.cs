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
    public partial class AddNewBox : Form
    {
        public string BoxX { get; set; }
        public string BoxY { get; set; }
        public string BoxZ { get; set; }
        public string BoxFull { get; set; }

        public AddNewBox()
        {
            InitializeComponent();

            AddNewBox_TextBox_X.KeyPress += new KeyPressEventHandler(AddNewBox_TextBox_X_KeyPress!);
            AddNewBox_TextBox_X.TextChanged += new EventHandler(AddNewBox_TextBox_X_TextChanged!);
            AddNewBox_TextBox_Y.KeyPress += new KeyPressEventHandler(AddNewBox_TextBox_Y_KeyPress!);
            AddNewBox_TextBox_Y.TextChanged += new EventHandler(AddNewBox_TextBox_Y_TextChanged!);
            AddNewBox_TextBox_Z.KeyPress += new KeyPressEventHandler(AddNewBox_TextBox_Z_KeyPress!);
            AddNewBox_TextBox_Z.TextChanged += new EventHandler(AddNewBox_TextBox_Z_TextChanged!);
            AddNewBox_Button_Save.Click += new EventHandler(SaveButton_Click!);
            AddNewBox_Button_Cancel.Click += new EventHandler(CancelButton_Click!);
            AddNewBox_CheckBox_DoubleWalled.CheckedChanged += new EventHandler(AddNewBox_CheckBox_DoubleWalled_CheckedChanged!);

            // Disable the Save button initially
            AddNewBox_Button_Save.Enabled = false;
        }

        private void AddNewBox_TextBox_X_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == '.' && !AddNewBox_TextBox_X.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewBox_TextBox_X_TextChanged(object sender, EventArgs e)
        {
            BoxX = AddNewBox_TextBox_X.Text;

            // Split the text into whole and decimal parts
            string[] parts = BoxX.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewBox_TextBox_X.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewBox_TextBox_X.SelectionStart = AddNewBox_TextBox_X.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewBox_TextBox_X.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewBox_TextBox_X.SelectionStart = AddNewBox_TextBox_X.Text.Length;
            }

            UpdateBoxFull();
        }

        private void AddNewBox_TextBox_Y_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == '.' && !AddNewBox_TextBox_Y.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewBox_TextBox_Y_TextChanged(object sender, EventArgs e)
        {
            BoxY = AddNewBox_TextBox_Y.Text;

            // Split the text into whole and decimal parts
            string[] parts = BoxY.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewBox_TextBox_Y.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewBox_TextBox_Y.SelectionStart = AddNewBox_TextBox_Y.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewBox_TextBox_Y.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewBox_TextBox_Y.SelectionStart = AddNewBox_TextBox_Y.Text.Length;
            }

            UpdateBoxFull();
        }

        private void AddNewBox_TextBox_Z_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == '.' && !AddNewBox_TextBox_Z.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewBox_TextBox_Z_TextChanged(object sender, EventArgs e)
        {
            BoxZ = AddNewBox_TextBox_Z.Text;

            // Split the text into whole and decimal parts
            string[] parts = BoxZ.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewBox_TextBox_Z.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewBox_TextBox_Z.SelectionStart = AddNewBox_TextBox_Z.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewBox_TextBox_Z.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewBox_TextBox_Z.SelectionStart = AddNewBox_TextBox_Z.Text.Length;
            }

            UpdateBoxFull();
        }

        private void AddNewBox_CheckBox_DoubleWalled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBoxFull();
        }

        private void UpdateBoxFull()
        {
            if (!string.IsNullOrEmpty(BoxX) && !string.IsNullOrEmpty(BoxY) && !string.IsNullOrEmpty(BoxZ))
            {
                BoxFull = "Box - " + $"{BoxX} x {BoxY} x {BoxZ}";
                if (AddNewBox_CheckBox_DoubleWalled.Checked)
                {
                    BoxFull += " (DW)";
                }
                AddNewBox_TextBox_Full.Text = BoxFull;
                AddNewBox_Button_Save.Enabled = true; // Enable the Save button
            }
            else
            {
                BoxFull = string.Empty;
                AddNewBox_TextBox_Full.Text = string.Empty;
                AddNewBox_Button_Save.Enabled = false; // Disable the Save button
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(BoxFull))
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string selectQuery = "SELECT id FROM waitlist_dunnage WHERE Box IS NULL OR Box = '' LIMIT 1";
                string updateQuery = "UPDATE waitlist_dunnage SET Box = @Box WHERE id = @id";
                string insertQuery = "INSERT INTO waitlist_dunnage (Box) VALUES (@Box)";
                string checkDuplicateQuery = "SELECT COUNT(*) FROM waitlist_dunnage WHERE Box = @Box";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Check for duplicate Box
                        using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, connection))
                        {
                            checkDuplicateCommand.Parameters.AddWithValue("@Box", BoxFull);
                            int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show(@"This Box already exists in the database.");
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
                                updateCommand.Parameters.AddWithValue("@Box", BoxFull);
                                updateCommand.Parameters.AddWithValue("@id", id);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show(BoxFull + @" added successfully.");
                            }
                        }
                        else
                        {
                            // Insert a new row if no open spot is found
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Box", BoxFull);
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show(BoxFull + @" added successfully.");
                            }
                        }

                        // Close the form after successful save
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.HandleError(nameof(SaveButton_Click), ex);
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Please enter valid values for length, width, and height.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(BoxFull))
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