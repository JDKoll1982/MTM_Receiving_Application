using System;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Core;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    public partial class AddNewDunnage : Form
    {
        public string DunnageName { get; set; }
        public string DunnageFull { get; set; }

        public AddNewDunnage()
        {
            InitializeComponent();
            AddNewDunnage_TextBox_Name.TextChanged += new EventHandler(AddNewDunnage_TextBox_Name_TextChanged!);
            AddNewDunnage_Button_Save.Click += new EventHandler(SaveButton_Click!);
            AddNewDunnage_Button_Cancel.Click += new EventHandler(CancelButton_Click!);

            // Initialize non-nullable properties
            DunnageName = string.Empty;
            DunnageFull = string.Empty;

            // Disable the Save button initially
            AddNewDunnage_Button_Save.Enabled = false;
        }

        private void AddNewDunnage_TextBox_Name_TextChanged(object sender, EventArgs e)
        {
            DunnageName = AddNewDunnage_TextBox_Name.Text;
            UpdateDunnageFull();
        }

        private void UpdateDunnageFull()
        {
            if (!string.IsNullOrEmpty(DunnageName))
            {
                DunnageFull = "Dunnage - " + DunnageName;
                AddNewDunnage_TextBox_Full.Text = DunnageFull;
                AddNewDunnage_Button_Save.Enabled = true; // Enable the Save button
            }
            else
            {
                DunnageFull = string.Empty;
                AddNewDunnage_TextBox_Full.Text = string.Empty;
                AddNewDunnage_Button_Save.Enabled = false; // Disable the Save button
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DunnageFull))
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string selectQuery = "SELECT id FROM waitlist_dunnage WHERE Dunnage IS NULL OR Dunnage = '' LIMIT 1";
                string updateQuery = "UPDATE waitlist_dunnage SET Dunnage = @Dunnage WHERE id = @id";
                string insertQuery = "INSERT INTO waitlist_dunnage (Dunnage) VALUES (@Dunnage)";
                string checkDuplicateQuery = "SELECT COUNT(*) FROM waitlist_dunnage WHERE Dunnage = @Dunnage";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Check for duplicate Dunnage
                        using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, connection))
                        {
                            checkDuplicateCommand.Parameters.AddWithValue("@Dunnage", DunnageFull);
                            int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show(@"This Dunnage already exists in the database.");
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
                                updateCommand.Parameters.AddWithValue("@Dunnage", DunnageFull);
                                updateCommand.Parameters.AddWithValue("@id", id);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show(DunnageFull + @" added successfully.");
                            }
                        }
                        else
                        {
                            // Insert a new row if no open spot is found
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Dunnage", DunnageFull);
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show(DunnageFull + @" added successfully.");
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
            if (!string.IsNullOrEmpty(DunnageFull))
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