using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public class NewJobSetupComponents
    {
        private static readonly HashSet<string> ExcludedComponents = new HashSet<string>
        {
            "MMC", "MMF", "MMCCS", "MMFCS", "MMR", "MMS", "FGT", "FGF"
        };

        public static NewJobSetup? GetOpenNewJobSetupForm()
        {
            return Application.OpenForms.OfType<NewJobSetup>().FirstOrDefault();
        }

        public static bool FillComboBox(ComboBox cb, string column, string database, string table, string pid, string op)
        {
            bool cbTextChanged = false;
            var connectionString = SqlCommands.GetConnectionString(null, database, null, null);
            string query = $"SELECT DISTINCT {column} FROM {table} WHERE PartID = @pid AND Operation = @op;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@pid", pid);
                    command.Parameters.AddWithValue("@op", op);
                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    var openNewJobSetupForm = GetOpenNewJobSetupForm();
                    if (openNewJobSetupForm == null)
                    {
                        return false;
                    }

                    var componentTexts = GetComponentTexts(openNewJobSetupForm);

                    while (reader.Read())
                    {
                        string item = reader.GetString(0);
                        if (!componentTexts.Contains(item) && !ContainsExcludedComponent(item))
                        {
                            cb.Text = item;
                            cbTextChanged = true;
                            break;
                        }
                    }

                    connection.Close();
                    command.Dispose();
                    if (!cbTextChanged)
                    {
                        cb.Text = @"No Components Found";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(nameof(FillComboBox), ex);
            }

            return cbTextChanged;
        }

        public static void NewJobSetup_ComboBox_PartID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var openNewJobSetupForm = GetOpenNewJobSetupForm();
                if (openNewJobSetupForm == null)
                {
                    return;
                }

                string pid = openNewJobSetupForm.NewJobSetup_ComboBox_PartID.Text;
                string op = openNewJobSetupForm.NewJobSetup_ComboBox_Op.Text;

                var componentComboBoxes = GetComponentComboBoxes(openNewJobSetupForm);

                // Reset all ComboBoxes and Labels
                foreach (var cb in componentComboBoxes)
                {
                    cb.Items.Clear();
                    cb.Text = string.Empty;
                    cb.Enabled = false;
                    cb.Visible = false;
                    cb.TabStop = false; // Prevent tab navigation to this ComboBox
                }

                // Fill and show only the necessary ComboBoxes and Labels
                for (int i = 0; i < componentComboBoxes.Count; i++)
                {
                    bool found = FillComboBox(componentComboBoxes[i], "Component", "mtm_waitlist", "waitlist_components", pid, op);
                    if (found)
                    {
                        ShowComboBoxAndLabel(componentComboBoxes[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(nameof(NewJobSetup_ComboBox_PartID_SelectedIndexChanged), ex);
            }
        }

        private static void ShowComboBoxAndLabel(ComboBox cb)
        {
            cb.Visible = true;
            cb.Enabled = false; // Ensure ComboBox is disabled
            cb.TabStop = false; // Prevent tab navigation to this ComboBox
        }

        private static List<string> GetComponentTexts(NewJobSetup openNewJobSetupForm)
        {
            return new List<string>
            {
                openNewJobSetupForm.NewJobSetup_ComboBox_Component1.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component2.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component3.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component4.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component5.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component6.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component7.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component8.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component9.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component10.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component11.Text,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component12.Text
            };
        }

        private static List<ComboBox> GetComponentComboBoxes(NewJobSetup openNewJobSetupForm)
        {
            return new List<ComboBox>
            {
                openNewJobSetupForm.NewJobSetup_ComboBox_Component1,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component2,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component3,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component4,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component5,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component6,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component7,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component8,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component9,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component10,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component11,
                openNewJobSetupForm.NewJobSetup_ComboBox_Component12
            };
        }

        private static bool ContainsExcludedComponent(string component)
        {
            return ExcludedComponents.Any(excluded => component.Contains(excluded, StringComparison.OrdinalIgnoreCase));
        }
    }
}