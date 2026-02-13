namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    partial class AddNewSkid
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
            AddNewSkid_GroupBox_Main = new GroupBox();
            AddNewSkid_TextBox_Full = new TextBox();
            AddNewSkid_TextBox_Y = new TextBox();
            AddNewSkid_Label_x = new Label();
            AddNewSkid_TextBox_X = new TextBox();
            AddNewSkid_Label_SkidName = new Label();
            AddNewSkid_Button_Save = new Button();
            AddNewSkid_Button_Cancel = new Button();
            AddNewSkid_CheckBox_KI = new CheckBox();
            AddNewSkid_GroupBox_Main.SuspendLayout();
            SuspendLayout();
            // 
            // AddNewSkid_GroupBox_Main
            // 
            AddNewSkid_GroupBox_Main.Controls.Add(AddNewSkid_TextBox_Full);
            AddNewSkid_GroupBox_Main.Controls.Add(AddNewSkid_TextBox_Y);
            AddNewSkid_GroupBox_Main.Controls.Add(AddNewSkid_Label_x);
            AddNewSkid_GroupBox_Main.Controls.Add(AddNewSkid_TextBox_X);
            AddNewSkid_GroupBox_Main.Controls.Add(AddNewSkid_Label_SkidName);
            AddNewSkid_GroupBox_Main.Location = new Point(12, 12);
            AddNewSkid_GroupBox_Main.Name = "AddNewSkid_GroupBox_Main";
            AddNewSkid_GroupBox_Main.Size = new Size(259, 85);
            AddNewSkid_GroupBox_Main.TabIndex = 0;
            AddNewSkid_GroupBox_Main.TabStop = false;
            AddNewSkid_GroupBox_Main.Text = "Add New Skid Type";
            // 
            // AddNewSkid_TextBox_Full
            // 
            AddNewSkid_TextBox_Full.Enabled = false;
            AddNewSkid_TextBox_Full.Location = new Point(6, 51);
            AddNewSkid_TextBox_Full.Name = "AddNewSkid_TextBox_Full";
            AddNewSkid_TextBox_Full.ReadOnly = true;
            AddNewSkid_TextBox_Full.Size = new Size(243, 23);
            AddNewSkid_TextBox_Full.TabIndex = 4;
            AddNewSkid_TextBox_Full.TextAlign = HorizontalAlignment.Center;
            // 
            // AddNewSkid_TextBox_Y
            // 
            AddNewSkid_TextBox_Y.Location = new Point(191, 21);
            AddNewSkid_TextBox_Y.Name = "AddNewSkid_TextBox_Y";
            AddNewSkid_TextBox_Y.Size = new Size(58, 23);
            AddNewSkid_TextBox_Y.TabIndex = 3;
            // 
            // AddNewSkid_Label_x
            // 
            AddNewSkid_Label_x.Location = new Point(148, 25);
            AddNewSkid_Label_x.Name = "AddNewSkid_Label_x";
            AddNewSkid_Label_x.Size = new Size(37, 15);
            AddNewSkid_Label_x.TabIndex = 2;
            AddNewSkid_Label_x.Text = "x";
            AddNewSkid_Label_x.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AddNewSkid_TextBox_X
            // 
            AddNewSkid_TextBox_X.Location = new Point(84, 21);
            AddNewSkid_TextBox_X.Name = "AddNewSkid_TextBox_X";
            AddNewSkid_TextBox_X.Size = new Size(58, 23);
            AddNewSkid_TextBox_X.TabIndex = 1;
            AddNewSkid_TextBox_X.TextChanged += AddNewSkid_TextBox_X_TextChanged;
            // 
            // AddNewSkid_Label_SkidName
            // 
            AddNewSkid_Label_SkidName.AutoSize = true;
            AddNewSkid_Label_SkidName.Location = new Point(6, 25);
            AddNewSkid_Label_SkidName.Name = "AddNewSkid_Label_SkidName";
            AddNewSkid_Label_SkidName.Size = new Size(72, 15);
            AddNewSkid_Label_SkidName.TabIndex = 0;
            AddNewSkid_Label_SkidName.Text = "Dimensions:";
            // 
            // AddNewSkid_Button_Save
            // 
            AddNewSkid_Button_Save.Location = new Point(12, 103);
            AddNewSkid_Button_Save.Name = "AddNewSkid_Button_Save";
            AddNewSkid_Button_Save.Size = new Size(75, 23);
            AddNewSkid_Button_Save.TabIndex = 1;
            AddNewSkid_Button_Save.Text = "Save";
            AddNewSkid_Button_Save.UseVisualStyleBackColor = true;
            // 
            // AddNewSkid_Button_Cancel
            // 
            AddNewSkid_Button_Cancel.Location = new Point(196, 103);
            AddNewSkid_Button_Cancel.Name = "AddNewSkid_Button_Cancel";
            AddNewSkid_Button_Cancel.Size = new Size(75, 23);
            AddNewSkid_Button_Cancel.TabIndex = 2;
            AddNewSkid_Button_Cancel.Text = "Cancel";
            AddNewSkid_Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // AddNewSkid_CheckBox_KI
            // 
            AddNewSkid_CheckBox_KI.AutoSize = true;
            AddNewSkid_CheckBox_KI.Location = new Point(111, 107);
            AddNewSkid_CheckBox_KI.Name = "AddNewSkid_CheckBox_KI";
            AddNewSkid_CheckBox_KI.Size = new Size(61, 19);
            AddNewSkid_CheckBox_KI.TabIndex = 3;
            AddNewSkid_CheckBox_KI.Text = "KI Skid";
            AddNewSkid_CheckBox_KI.UseVisualStyleBackColor = true;
            // 
            // AddNewSkid
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(280, 137);
            Controls.Add(AddNewSkid_CheckBox_KI);
            Controls.Add(AddNewSkid_Button_Cancel);
            Controls.Add(AddNewSkid_Button_Save);
            Controls.Add(AddNewSkid_GroupBox_Main);
            Name = "AddNewSkid";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Add New Skid";
            AddNewSkid_GroupBox_Main.ResumeLayout(false);
            AddNewSkid_GroupBox_Main.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox AddNewSkid_GroupBox_Main;
        private Label AddNewSkid_Label_SkidName;
        private TextBox AddNewSkid_TextBox_Y;
        private Label AddNewSkid_Label_x;
        private TextBox AddNewSkid_TextBox_X;
        private TextBox AddNewSkid_TextBox_Full;
        private Button AddNewSkid_Button_Save;
        private Button AddNewSkid_Button_Cancel;
        private CheckBox AddNewSkid_CheckBox_KI;
    }
}