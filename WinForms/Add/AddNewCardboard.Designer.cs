namespace MTM_Waitlist_Application_2._0.WinForms.Add
{
    partial class AddNewCardboard
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
            AddNewCardboard_GroupBox_Main = new GroupBox();
            AddNewCardboard_TextBox_Full = new TextBox();
            AddNewCardboard_TextBox_X = new TextBox();
            AddNewCardboard_Label_CardboardName = new Label();
            AddNewCardboard_Button_Save = new Button();
            AddNewCardboard_Button_Cancel = new Button();
            AddNewCardboard_Label_x = new Label();
            AddNewCardboard_TextBox_Y = new TextBox();
            AddNewCardboard_GroupBox_Main.SuspendLayout();
            SuspendLayout();
            // 
            // AddNewCardboard_GroupBox_Main
            // 
            AddNewCardboard_GroupBox_Main.Controls.Add(AddNewCardboard_TextBox_Full);
            AddNewCardboard_GroupBox_Main.Controls.Add(AddNewCardboard_TextBox_Y);
            AddNewCardboard_GroupBox_Main.Controls.Add(AddNewCardboard_Label_x);
            AddNewCardboard_GroupBox_Main.Controls.Add(AddNewCardboard_TextBox_X);
            AddNewCardboard_GroupBox_Main.Controls.Add(AddNewCardboard_Label_CardboardName);
            AddNewCardboard_GroupBox_Main.Location = new Point(12, 12);
            AddNewCardboard_GroupBox_Main.Name = "AddNewCardboard_GroupBox_Main";
            AddNewCardboard_GroupBox_Main.Size = new Size(259, 85);
            AddNewCardboard_GroupBox_Main.TabIndex = 0;
            AddNewCardboard_GroupBox_Main.TabStop = false;
            AddNewCardboard_GroupBox_Main.Text = "Add New Cardboard Type";
            // 
            // AddNewCardboard_TextBox_Full
            // 
            AddNewCardboard_TextBox_Full.Enabled = false;
            AddNewCardboard_TextBox_Full.Location = new Point(6, 51);
            AddNewCardboard_TextBox_Full.Name = "AddNewCardboard_TextBox_Full";
            AddNewCardboard_TextBox_Full.ReadOnly = true;
            AddNewCardboard_TextBox_Full.Size = new Size(243, 23);
            AddNewCardboard_TextBox_Full.TabIndex = 4;
            AddNewCardboard_TextBox_Full.TextAlign = HorizontalAlignment.Center;
            // 
            // AddNewCardboard_TextBox_X
            // 
            AddNewCardboard_TextBox_X.Location = new Point(84, 21);
            AddNewCardboard_TextBox_X.Name = "AddNewCardboard_TextBox_X";
            AddNewCardboard_TextBox_X.Size = new Size(58, 23);
            AddNewCardboard_TextBox_X.TabIndex = 1;
            AddNewCardboard_TextBox_X.TextChanged += this.AddNewCardboard_TextBox_X_TextChanged;
            // 
            // AddNewCardboard_Label_CardboardName
            // 
            AddNewCardboard_Label_CardboardName.AutoSize = true;
            AddNewCardboard_Label_CardboardName.Location = new Point(6, 25);
            AddNewCardboard_Label_CardboardName.Name = "AddNewCardboard_Label_CardboardName";
            AddNewCardboard_Label_CardboardName.Size = new Size(72, 15);
            AddNewCardboard_Label_CardboardName.TabIndex = 0;
            AddNewCardboard_Label_CardboardName.Text = "Dimensions:";
            // 
            // AddNewCardboard_Button_Save
            // 
            AddNewCardboard_Button_Save.Location = new Point(12, 103);
            AddNewCardboard_Button_Save.Name = "AddNewCardboard_Button_Save";
            AddNewCardboard_Button_Save.Size = new Size(75, 23);
            AddNewCardboard_Button_Save.TabIndex = 3;
            AddNewCardboard_Button_Save.Text = "Save";
            AddNewCardboard_Button_Save.UseVisualStyleBackColor = true;
            // 
            // AddNewCardboard_Button_Cancel
            // 
            AddNewCardboard_Button_Cancel.Location = new Point(196, 103);
            AddNewCardboard_Button_Cancel.Name = "AddNewCardboard_Button_Cancel";
            AddNewCardboard_Button_Cancel.Size = new Size(75, 23);
            AddNewCardboard_Button_Cancel.TabIndex = 4;
            AddNewCardboard_Button_Cancel.Text = "Cancel";
            AddNewCardboard_Button_Cancel.UseVisualStyleBackColor = true;
            // 
            // AddNewCardboard_Label_x
            // 
            AddNewCardboard_Label_x.Location = new Point(148, 25);
            AddNewCardboard_Label_x.Name = "AddNewCardboard_Label_x";
            AddNewCardboard_Label_x.Size = new Size(37, 15);
            AddNewCardboard_Label_x.TabIndex = 2;
            AddNewCardboard_Label_x.Text = "x";
            AddNewCardboard_Label_x.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AddNewCardboard_TextBox_Y
            // 
            AddNewCardboard_TextBox_Y.Location = new Point(191, 21);
            AddNewCardboard_TextBox_Y.Name = "AddNewCardboard_TextBox_Y";
            AddNewCardboard_TextBox_Y.Size = new Size(58, 23);
            AddNewCardboard_TextBox_Y.TabIndex = 2;
            // 
            // AddNewCardboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(280, 137);
            Controls.Add(AddNewCardboard_Button_Cancel);
            Controls.Add(AddNewCardboard_Button_Save);
            Controls.Add(AddNewCardboard_GroupBox_Main);
            Name = "AddNewCardboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Add New Cardboard";
            AddNewCardboard_GroupBox_Main.ResumeLayout(false);
            AddNewCardboard_GroupBox_Main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox AddNewCardboard_GroupBox_Main;
        private Label AddNewCardboard_Label_CardboardName;
        private TextBox AddNewCardboard_TextBox_X;
        private TextBox AddNewCardboard_TextBox_Full;
        private Button AddNewCardboard_Button_Save;
        private Button AddNewCardboard_Button_Cancel;
        private TextBox AddNewCardboard_TextBox_Y;
        private Label AddNewCardboard_Label_x;
    }
}