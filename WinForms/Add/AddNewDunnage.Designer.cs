namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    partial class AddNewDunnage
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
            AddNewDunnage_GroupBox_Main = new GroupBox();
            AddNewDunnage_TextBox_Full = new TextBox();
            AddNewDunnage_TextBox_Name = new TextBox();
            AddNewDunnage_Label_DunnageName = new Label();
            AddNewDunnage_Button_Save = new Button();
            AddNewDunnage_Button_Cancel = new Button();
            AddNewDunnage_GroupBox_Main.SuspendLayout();
            SuspendLayout();
            // 
            // AddNewDunnage_GroupBox_Main
            // 
            AddNewDunnage_GroupBox_Main.Controls.Add(AddNewDunnage_TextBox_Full);
            AddNewDunnage_GroupBox_Main.Controls.Add(AddNewDunnage_TextBox_Name);
            AddNewDunnage_GroupBox_Main.Controls.Add(AddNewDunnage_Label_DunnageName);
            AddNewDunnage_GroupBox_Main.Location = new Point(12, 12);
            AddNewDunnage_GroupBox_Main.Name = "AddNewDunnage_GroupBox_Main";
            AddNewDunnage_GroupBox_Main.Size = new Size(259, 85);
            AddNewDunnage_GroupBox_Main.TabIndex = 0;
            AddNewDunnage_GroupBox_Main.TabStop = false;
            AddNewDunnage_GroupBox_Main.Text = "Add New Dunnage Type";
            // 
            // AddNewDunnage_TextBox_Full
            // 
            AddNewDunnage_TextBox_Full.Enabled = false;
            AddNewDunnage_TextBox_Full.Location = new Point(6, 51);
            AddNewDunnage_TextBox_Full.Name = "AddNewDunnage_TextBox_Full";
            AddNewDunnage_TextBox_Full.ReadOnly = true;
            AddNewDunnage_TextBox_Full.Size = new Size(243, 23);
            AddNewDunnage_TextBox_Full.TabIndex = 4;
            AddNewDunnage_TextBox_Full.TextAlign = HorizontalAlignment.Center;
            // 
            // AddNewDunnage_TextBox_Name
            // 
            AddNewDunnage_TextBox_Name.Location = new Point(54, 21);
            AddNewDunnage_TextBox_Name.Name = "AddNewDunnage_TextBox_Name";
            AddNewDunnage_TextBox_Name.Size = new Size(195, 23);
            AddNewDunnage_TextBox_Name.TabIndex = 1;
            // 
            // AddNewDunnage_Label_DunnageName
            // 
            AddNewDunnage_Label_DunnageName.AutoSize = true;
            AddNewDunnage_Label_DunnageName.Location = new Point(6, 25);
            AddNewDunnage_Label_DunnageName.Name = "AddNewDunnage_Label_DunnageName";
            AddNewDunnage_Label_DunnageName.Size = new Size(42, 15);
            AddNewDunnage_Label_DunnageName.TabIndex = 0;
            AddNewDunnage_Label_DunnageName.Text = "Name:";
            // 
            // AddNewDunnage_Button_Save
            // 
            AddNewDunnage_Button_Save.Location = new Point(12, 103);
            AddNewDunnage_Button_Save.Name = "AddNewDunnage_Button_Save";
            AddNewDunnage_Button_Save.Size = new Size(75, 23);
            AddNewDunnage_Button_Save.TabIndex = 3;
            AddNewDunnage_Button_Save.Text = "Save";
            AddNewDunnage_Button_Save.UseVisualStyleBackColor = true;
            // 
            // AddNewDunnage_Button_Cancel
            // 
            AddNewDunnage_Button_Cancel.Location = new Point(196, 103);
            AddNewDunnage_Button_Cancel.Name = "AddNewDunnage_Button_Cancel";
            AddNewDunnage_Button_Cancel.Size = new Size(75, 23);
            AddNewDunnage_Button_Cancel.TabIndex = 4;
            AddNewDunnage_Button_Cancel.Text = "Cancel";
            AddNewDunnage_Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // AddNewDunnage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(280, 137);
            Controls.Add(AddNewDunnage_Button_Cancel);
            Controls.Add(AddNewDunnage_Button_Save);
            Controls.Add(AddNewDunnage_GroupBox_Main);
            Name = "AddNewDunnage";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Add New Dunnage";
            AddNewDunnage_GroupBox_Main.ResumeLayout(false);
            AddNewDunnage_GroupBox_Main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox AddNewDunnage_GroupBox_Main;
        private Label AddNewDunnage_Label_DunnageName;
        private TextBox AddNewDunnage_TextBox_Y;
        private Label AddNewDunnage_Label_x;
        private TextBox AddNewDunnage_TextBox_Name;
        private TextBox AddNewDunnage_TextBox_Full;
        private Button AddNewDunnage_Button_Save;
        private Button AddNewDunnage_Button_Cancel;
    }
}