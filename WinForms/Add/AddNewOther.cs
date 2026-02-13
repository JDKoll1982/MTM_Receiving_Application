using System;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Core;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    public partial class AddNewOther : Form
    {
        public string OtherName { get; set; }
        public string OtherFull { get; set; }

        public AddNewOther()
        {
            InitializeComponent();
            AddNewOther_TextBox_Name.TextChanged += new EventHandler(AddNewOther_TextBox_Name_TextChanged!);
            AddNewOther_Button_Save.Click += new EventHandler(SaveButton_Click!);
            AddNewOther_Button_Cancel.Click += new EventHandler(CancelButton_Click!);

            // Initialize non-nullable properties
            OtherName = string.Empty;
            OtherFull = string.Empty;

            // Disable the Save button initially
            AddNewOther_Button_Save.Enabled = false;
        }

        private void AddNewOther_TextBox_Name_TextChanged(object sender, EventArgs e)
        {
            OtherName = AddNewOther_TextBox_Name.Text;
            UpdateOtherFull();
        }

        private void UpdateOtherFull()
        {
            if (!string.IsNullOrEmpty(OtherName))
            {
                OtherFull = "Other - " + OtherName;
                AddNewOther_TextBox_Full.Text = OtherFull;
                AddNewOther_Button_Save.Enabled = true; // Enable the Save button
            }
            else
            {
                OtherFull = string.Empty;
                AddNewOther_TextBox_Full.Text = string.Empty;
                AddNewOther_Button_Save.Enabled = false; // Disable the Save button
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OtherFull))
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string selectQuery = "SELECT id FROM waitlist_dunnage WHERE Other IS NULL OR Other = '' LIMIT 1";
                string updateQuery = "UPDATE waitlist_dunnage SET Other = @Other WHERE id = @id";
                string insertQuery = "INSERT INTO waitlist_dunnage (Other) VALUES (@Other)";
                string checkDuplicateQuery = "SELECT COUNT(*) FROM waitlist_dunnage WHERE Other = @Other";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Check for duplicate Other
                        using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, connection))
                        {
                            checkDuplicateCommand.Parameters.AddWithValue("@Other", OtherFull);
                            int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show(@"This Other already exists in the database.");
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
                                updateCommand.Parameters.AddWithValue("@Other", OtherFull);
                                updateCommand.Parameters.AddWithValue("@id", id);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show(OtherFull + @" added successfully.");
                            }
                        }
                        else
                        {
                            // Insert a new row if no open spot is found
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Other", OtherFull);
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show(OtherFull + @" added successfully.");
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
                MessageBox.Show(@"Please enter a valid name.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OtherFull))
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