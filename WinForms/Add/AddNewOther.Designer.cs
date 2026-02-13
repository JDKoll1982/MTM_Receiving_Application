namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    partial class AddNewOther
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
            AddNewOther_GroupBox_Main = new GroupBox();
            AddNewOther_TextBox_Full = new TextBox();
            AddNewOther_TextBox_Name = new TextBox();
            AddNewOther_Label_OtherName = new Label();
            AddNewOther_Button_Save = new Button();
            AddNewOther_Button_Cancel = new Button();
            AddNewOther_GroupBox_Main.SuspendLayout();
            SuspendLayout();
            // 
            // AddNewOther_GroupBox_Main
            // 
            AddNewOther_GroupBox_Main.Controls.Add(AddNewOther_TextBox_Full);
            AddNewOther_GroupBox_Main.Controls.Add(AddNewOther_TextBox_Name);
            AddNewOther_GroupBox_Main.Controls.Add(AddNewOther_Label_OtherName);
            AddNewOther_GroupBox_Main.Location = new Point(12, 12);
            AddNewOther_GroupBox_Main.Name = "AddNewOther_GroupBox_Main";
            AddNewOther_GroupBox_Main.Size = new Size(259, 85);
            AddNewOther_GroupBox_Main.TabIndex = 0;
            AddNewOther_GroupBox_Main.TabStop = false;
            AddNewOther_GroupBox_Main.Text = "Add New Other Type";
            // 
            // AddNewOther_TextBox_Full
            // 
            AddNewOther_TextBox_Full.Enabled = false;
            AddNewOther_TextBox_Full.Location = new Point(6, 51);
            AddNewOther_TextBox_Full.Name = "AddNewOther_TextBox_Full";
            AddNewOther_TextBox_Full.ReadOnly = true;
            AddNewOther_TextBox_Full.Size = new Size(243, 23);
            AddNewOther_TextBox_Full.TabIndex = 4;
            AddNewOther_TextBox_Full.TextAlign = HorizontalAlignment.Center;
            // 
            // AddNewOther_TextBox_Name
            // 
            AddNewOther_TextBox_Name.Location = new Point(54, 21);
            AddNewOther_TextBox_Name.Name = "AddNewOther_TextBox_Name";
            AddNewOther_TextBox_Name.Size = new Size(195, 23);
            AddNewOther_TextBox_Name.TabIndex = 1;
            // 
            // AddNewOther_Label_OtherName
            // 
            AddNewOther_Label_OtherName.AutoSize = true;
            AddNewOther_Label_OtherName.Location = new Point(6, 25);
            AddNewOther_Label_OtherName.Name = "AddNewOther_Label_OtherName";
            AddNewOther_Label_OtherName.Size = new Size(42, 15);
            AddNewOther_Label_OtherName.TabIndex = 0;
            AddNewOther_Label_OtherName.Text = "Name:";
            // 
            // AddNewOther_Button_Save
            // 
            AddNewOther_Button_Save.Location = new Point(12, 103);
            AddNewOther_Button_Save.Name = "AddNewOther_Button_Save";
            AddNewOther_Button_Save.Size = new Size(75, 23);
            AddNewOther_Button_Save.TabIndex = 3;
            AddNewOther_Button_Save.Text = "Save";
            AddNewOther_Button_Save.UseVisualStyleBackColor = true;
            // 
            // AddNewOther_Button_Cancel
            // 
            AddNewOther_Button_Cancel.Location = new Point(196, 103);
            AddNewOther_Button_Cancel.Name = "AddNewOther_Button_Cancel";
            AddNewOther_Button_Cancel.Size = new Size(75, 23);
            AddNewOther_Button_Cancel.TabIndex = 4;
            AddNewOther_Button_Cancel.Text = "Cancel";
            AddNewOther_Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // AddNewOther
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(280, 137);
            Controls.Add(AddNewOther_Button_Cancel);
            Controls.Add(AddNewOther_Button_Save);
            Controls.Add(AddNewOther_GroupBox_Main);
            Name = "AddNewOther";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Add New Other";
            AddNewOther_GroupBox_Main.ResumeLayout(false);
            AddNewOther_GroupBox_Main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox AddNewOther_GroupBox_Main;
        private Label AddNewOther_Label_OtherName;
        private TextBox AddNewOther_TextBox_Y;
        private Label AddNewOther_Label_x;
        private TextBox AddNewOther_TextBox_Name;
        private TextBox AddNewOther_TextBox_Full;
        private Button AddNewOther_Button_Save;
        private Button AddNewOther_Button_Cancel;
    }
}