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
    public partial class AddNewCardboard : Form
    {
        public string CardboardX { get; set; }
        public string CardboardY { get; set; }
        public string CardboardFull { get; set; }

        public AddNewCardboard()
        {
            InitializeComponent();
            AddNewCardboard_TextBox_X.KeyPress += new KeyPressEventHandler(AddNewCardboard_TextBox_X_KeyPress!);
            AddNewCardboard_TextBox_X.TextChanged += new EventHandler(AddNewCardboard_TextBox_X_TextChanged!);
            AddNewCardboard_TextBox_Y.KeyPress += new KeyPressEventHandler(AddNewCardboard_TextBox_Y_KeyPress!);
            AddNewCardboard_TextBox_Y.TextChanged += new EventHandler(AddNewCardboard_TextBox_Y_TextChanged!);
            AddNewCardboard_Button_Save.Click += new EventHandler(SaveButton_Click!);
            AddNewCardboard_Button_Cancel.Click += new EventHandler(CancelButton_Click!);

            // Initialize non-nullable properties
            CardboardX = string.Empty;
            CardboardY = string.Empty;
            CardboardFull = string.Empty;

            // Disable the Save button initially
            AddNewCardboard_Button_Save.Enabled = false;
        }

        private void AddNewCardboard_TextBox_X_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == '.' && !AddNewCardboard_TextBox_X.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewCardboard_TextBox_X_TextChanged(object sender, EventArgs e)
        {
            CardboardX = AddNewCardboard_TextBox_X.Text;

            // Split the text into whole and decimal parts
            string[] parts = CardboardX.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewCardboard_TextBox_X.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewCardboard_TextBox_X.SelectionStart = AddNewCardboard_TextBox_X.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewCardboard_TextBox_X.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewCardboard_TextBox_X.SelectionStart = AddNewCardboard_TextBox_X.Text.Length;
            }

            UpdateCardboardFull();
        }

        private void AddNewCardboard_TextBox_Y_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar == '.' && !AddNewCardboard_TextBox_Y.Text.Contains('.'))
            {
                return;
            }

            // If we get here, the character is not allowed
            e.Handled = true;
        }

        private void AddNewCardboard_TextBox_Y_TextChanged(object sender, EventArgs e)
        {
            CardboardY = AddNewCardboard_TextBox_Y.Text;

            // Split the text into whole and decimal parts
            string[] parts = CardboardY.Split('.');

            // Check the whole number part
            if (parts[0].Length > 3)
            {
                AddNewCardboard_TextBox_Y.Text = parts[0].Substring(0, 3) + (parts.Length > 1 ? "." + parts[1] : "");
                AddNewCardboard_TextBox_Y.SelectionStart = AddNewCardboard_TextBox_Y.Text.Length;
            }

            // Check the decimal part
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                AddNewCardboard_TextBox_Y.Text = parts[0] + "." + parts[1].Substring(0, 2);
                AddNewCardboard_TextBox_Y.SelectionStart = AddNewCardboard_TextBox_Y.Text.Length;
            }

            UpdateCardboardFull();
        }

        private void UpdateCardboardFull()
        {
            if (!string.IsNullOrEmpty(CardboardX) && !string.IsNullOrEmpty(CardboardY))
            {
                CardboardFull = "Cardboard - " + $"{CardboardX} x {CardboardY}";
                AddNewCardboard_TextBox_Full.Text = CardboardFull;
                AddNewCardboard_Button_Save.Enabled = true; // Enable the Save button
            }
            else
            {
                CardboardFull = string.Empty;
                AddNewCardboard_TextBox_Full.Text = string.Empty;
                AddNewCardboard_Button_Save.Enabled = false; // Disable the Save button
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CardboardFull))
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string selectQuery = "SELECT id FROM waitlist_dunnage WHERE Cardboard IS NULL OR Cardboard = '' LIMIT 1";
                string updateQuery = "UPDATE waitlist_dunnage SET Cardboard = @Cardboard WHERE id = @id";
                string insertQuery = "INSERT INTO waitlist_dunnage (Cardboard) VALUES (@Cardboard)";
                string checkDuplicateQuery = "SELECT COUNT(*) FROM waitlist_dunnage WHERE Cardboard = @Cardboard";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Check for duplicate Cardboard
                        using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, connection))
                        {
                            checkDuplicateCommand.Parameters.AddWithValue("@Cardboard", CardboardFull);
                            int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show(@"This Cardboard already exists in the database.");
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
                                updateCommand.Parameters.AddWithValue("@Cardboard", CardboardFull);
                                updateCommand.Parameters.AddWithValue("@id", id);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show(CardboardFull + @" added successfully.");
                            }
                        }
                        else
                        {
                            // Insert a new row if no open spot is found
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Cardboard", CardboardFull);
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show(CardboardFull + @" added successfully.");
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
                MessageBox.Show(@"Please enter valid values for length and width.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CardboardFull))
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