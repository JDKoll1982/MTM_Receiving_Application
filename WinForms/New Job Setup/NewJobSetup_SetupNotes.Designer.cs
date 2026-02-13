namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    partial class NewJobSetupSetupNotes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            NewJobSetup_Button_SaveNotes = new Button();
            NewJobSetup_Button_CancelNotes = new Button();
            NewJobSetup_RTextBox_Notes = new RichTextBox();
            SuspendLayout();
            // 
            // NewJobSetup_Button_SaveNotes
            // 
            NewJobSetup_Button_SaveNotes.Location = new Point(12, 343);
            NewJobSetup_Button_SaveNotes.Name = "NewJobSetup_Button_SaveNotes";
            NewJobSetup_Button_SaveNotes.Size = new Size(75, 23);
            NewJobSetup_Button_SaveNotes.TabIndex = 1;
            NewJobSetup_Button_SaveNotes.Text = "Save";
            NewJobSetup_Button_SaveNotes.UseVisualStyleBackColor = true;
            NewJobSetup_Button_SaveNotes.Click += NewJobSetup_Button_SaveNotes_Click;
            // 
            // NewJobSetup_Button_CancelNotes
            // 
            NewJobSetup_Button_CancelNotes.Location = new Point(448, 343);
            NewJobSetup_Button_CancelNotes.Name = "NewJobSetup_Button_CancelNotes";
            NewJobSetup_Button_CancelNotes.Size = new Size(75, 23);
            NewJobSetup_Button_CancelNotes.TabIndex = 2;
            NewJobSetup_Button_CancelNotes.Text = "Cancel";
            NewJobSetup_Button_CancelNotes.UseVisualStyleBackColor = true;
            NewJobSetup_Button_CancelNotes.Click += NewJobSetup_Button_CancelNotes_Click;
            // 
            // NewJobSetup_RTextBox_Notes
            // 
            NewJobSetup_RTextBox_Notes.Location = new Point(12, 12);
            NewJobSetup_RTextBox_Notes.Name = "NewJobSetup_RTextBox_Notes";
            NewJobSetup_RTextBox_Notes.Size = new Size(511, 325);
            NewJobSetup_RTextBox_Notes.TabIndex = 0;
            NewJobSetup_RTextBox_Notes.Text = "";
            NewJobSetup_RTextBox_Notes.TextChanged += NewJobSetup_RTextBox_Notes_TextChanged;
            // 
            // NewJobSetup_SetupNotes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(535, 374);
            Controls.Add(NewJobSetup_RTextBox_Notes);
            Controls.Add(NewJobSetup_Button_CancelNotes);
            Controls.Add(NewJobSetup_Button_SaveNotes);
            Name = "NewJobSetupSetupNotes";
            Text = "Setup Notes";
            ResumeLayout(false);
        }

        #endregion

        private Button NewJobSetup_Button_SaveNotes;
        private Button NewJobSetup_Button_CancelNotes;
        private RichTextBox NewJobSetup_RTextBox_Notes;
    }
}