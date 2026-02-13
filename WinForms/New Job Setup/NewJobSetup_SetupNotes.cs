using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public partial class NewJobSetupSetupNotes : Form
    {
        string setupNotes = "";
        public NewJobSetupSetupNotes()
        {
            InitializeComponent();
            if (NewJobSetup.SetupNotes.Length > 0)
            {
                NewJobSetup_RTextBox_Notes.Text = NewJobSetup.SetupNotes;
                setupNotes = NewJobSetup.SetupNotes;
            }
        }

        private void NewJobSetup_RTextBox_Notes_TextChanged(object sender, EventArgs e)
        {
            if (NewJobSetup_RTextBox_Notes.Text.Length > 0)
            {
                NewJobSetup_Button_SaveNotes.Enabled = true;
            }
            else
            {
                NewJobSetup_Button_SaveNotes.Enabled = false;
            }
        }

        private void NewJobSetup_Button_SaveNotes_Click(object sender, EventArgs e)
        {
            if (NewJobSetup_RTextBox_Notes.Text.Length > 0)
            {
                if (setupNotes != NewJobSetup_RTextBox_Notes.Text)
                {
                    var result = MessageBox.Show(@"You have made changes, this can not be undone. Continue?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        return; // Do not close the form
                    }
                }
                NewJobSetup.SetupNotes = NewJobSetup_RTextBox_Notes.Text;
                this.Close();
            }
        }

        private void NewJobSetup_Button_CancelNotes_Click(object sender, EventArgs e)
        {
            if (NewJobSetup_RTextBox_Notes.Text.Length > 0)
            {
                if (setupNotes != NewJobSetup_RTextBox_Notes.Text)
                {
                    var result = MessageBox.Show(@"You have unsaved changes, close without saving?", @"Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        return; // Do not close the form
                    }
                }
            }

            NewJobSetup.SetupNotes = NewJobSetup_RTextBox_Notes.Text;
            this.Close();
        }
    }
}
