namespace Visual_Inventory_Assistant.Windows
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.SettingsForm_GroupBox_Visual = new System.Windows.Forms.GroupBox();
            this.SettingsForm_TextBox_VisualPassword = new System.Windows.Forms.TextBox();
            this.SettingsForm_TextBox_VisualUserName = new System.Windows.Forms.TextBox();
            this.SettingsForm_Label_VisualPassword = new System.Windows.Forms.Label();
            this.SettingsForm_Label_VisualUserName = new System.Windows.Forms.Label();
            this.SettingsForm_Panel_VisualButtons = new System.Windows.Forms.Panel();
            this.SettingsForm_Button_SaveVisual = new System.Windows.Forms.Button();
            this.SettingsForm_Button_ResetVisual = new System.Windows.Forms.Button();
            this.SettingsForm_GroupBox_Sheets = new System.Windows.Forms.GroupBox();
            this.SettingsForm_TextBox_SheetsLink = new System.Windows.Forms.TextBox();
            this.SettingsForm_Label_SheetsLink = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SettingsForm_Button_SaveGoogle = new System.Windows.Forms.Button();
            this.SettingsForm_Button_Reset = new System.Windows.Forms.Button();
            this.Settings_CurrentUser = new System.Windows.Forms.TextBox();
            this.SettingsForm_Label_CurrentUser = new System.Windows.Forms.Label();
            this.SettingsForm_Button_Exit = new System.Windows.Forms.Button();
            this.SettingsForm_GroupBox_Visual.SuspendLayout();
            this.SettingsForm_Panel_VisualButtons.SuspendLayout();
            this.SettingsForm_GroupBox_Sheets.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingsForm_GroupBox_Visual
            // 
            this.SettingsForm_GroupBox_Visual.Controls.Add(this.SettingsForm_TextBox_VisualPassword);
            this.SettingsForm_GroupBox_Visual.Controls.Add(this.SettingsForm_TextBox_VisualUserName);
            this.SettingsForm_GroupBox_Visual.Controls.Add(this.SettingsForm_Label_VisualPassword);
            this.SettingsForm_GroupBox_Visual.Controls.Add(this.SettingsForm_Label_VisualUserName);
            this.SettingsForm_GroupBox_Visual.Controls.Add(this.SettingsForm_Panel_VisualButtons);
            this.SettingsForm_GroupBox_Visual.Location = new System.Drawing.Point(10, 32);
            this.SettingsForm_GroupBox_Visual.Name = "SettingsForm_GroupBox_Visual";
            this.SettingsForm_GroupBox_Visual.Size = new System.Drawing.Size(201, 114);
            this.SettingsForm_GroupBox_Visual.TabIndex = 97;
            this.SettingsForm_GroupBox_Visual.TabStop = false;
            this.SettingsForm_GroupBox_Visual.Text = "Infor Visual Login Information";
            // 
            // SettingsForm_TextBox_VisualPassword
            // 
            this.SettingsForm_TextBox_VisualPassword.Location = new System.Drawing.Point(72, 49);
            this.SettingsForm_TextBox_VisualPassword.Name = "SettingsForm_TextBox_VisualPassword";
            this.SettingsForm_TextBox_VisualPassword.Size = new System.Drawing.Size(122, 20);
            this.SettingsForm_TextBox_VisualPassword.TabIndex = 2;
            this.SettingsForm_TextBox_VisualPassword.TextChanged += new System.EventHandler(this.Visual_TextChanged);
            // 
            // SettingsForm_TextBox_VisualUserName
            // 
            this.SettingsForm_TextBox_VisualUserName.Location = new System.Drawing.Point(72, 21);
            this.SettingsForm_TextBox_VisualUserName.Name = "SettingsForm_TextBox_VisualUserName";
            this.SettingsForm_TextBox_VisualUserName.Size = new System.Drawing.Size(122, 20);
            this.SettingsForm_TextBox_VisualUserName.TabIndex = 1;
            this.SettingsForm_TextBox_VisualUserName.TextChanged += new System.EventHandler(this.Visual_TextChanged);
            // 
            // SettingsForm_Label_VisualPassword
            // 
            this.SettingsForm_Label_VisualPassword.AutoSize = true;
            this.SettingsForm_Label_VisualPassword.Location = new System.Drawing.Point(15, 52);
            this.SettingsForm_Label_VisualPassword.Name = "SettingsForm_Label_VisualPassword";
            this.SettingsForm_Label_VisualPassword.Size = new System.Drawing.Size(56, 13);
            this.SettingsForm_Label_VisualPassword.TabIndex = 95;
            this.SettingsForm_Label_VisualPassword.Text = "Password:";
            // 
            // SettingsForm_Label_VisualUserName
            // 
            this.SettingsForm_Label_VisualUserName.AutoSize = true;
            this.SettingsForm_Label_VisualUserName.Location = new System.Drawing.Point(9, 24);
            this.SettingsForm_Label_VisualUserName.Name = "SettingsForm_Label_VisualUserName";
            this.SettingsForm_Label_VisualUserName.Size = new System.Drawing.Size(63, 13);
            this.SettingsForm_Label_VisualUserName.TabIndex = 96;
            this.SettingsForm_Label_VisualUserName.Text = "User Name:";
            // 
            // SettingsForm_Panel_VisualButtons
            // 
            this.SettingsForm_Panel_VisualButtons.Controls.Add(this.SettingsForm_Button_SaveVisual);
            this.SettingsForm_Panel_VisualButtons.Controls.Add(this.SettingsForm_Button_ResetVisual);
            this.SettingsForm_Panel_VisualButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SettingsForm_Panel_VisualButtons.Location = new System.Drawing.Point(3, 81);
            this.SettingsForm_Panel_VisualButtons.Name = "SettingsForm_Panel_VisualButtons";
            this.SettingsForm_Panel_VisualButtons.Size = new System.Drawing.Size(195, 30);
            this.SettingsForm_Panel_VisualButtons.TabIndex = 0;
            // 
            // SettingsForm_Button_SaveVisual
            // 
            this.SettingsForm_Button_SaveVisual.Dock = System.Windows.Forms.DockStyle.Right;
            this.SettingsForm_Button_SaveVisual.Enabled = false;
            this.SettingsForm_Button_SaveVisual.Location = new System.Drawing.Point(131, 0);
            this.SettingsForm_Button_SaveVisual.Name = "SettingsForm_Button_SaveVisual";
            this.SettingsForm_Button_SaveVisual.Size = new System.Drawing.Size(64, 30);
            this.SettingsForm_Button_SaveVisual.TabIndex = 3;
            this.SettingsForm_Button_SaveVisual.Text = "Save";
            this.SettingsForm_Button_SaveVisual.UseVisualStyleBackColor = true;
            this.SettingsForm_Button_SaveVisual.Click += new System.EventHandler(this.SettingsForm_Button_SaveVisual_Click);
            // 
            // SettingsForm_Button_ResetVisual
            // 
            this.SettingsForm_Button_ResetVisual.Dock = System.Windows.Forms.DockStyle.Left;
            this.SettingsForm_Button_ResetVisual.Location = new System.Drawing.Point(0, 0);
            this.SettingsForm_Button_ResetVisual.Name = "SettingsForm_Button_ResetVisual";
            this.SettingsForm_Button_ResetVisual.Size = new System.Drawing.Size(64, 30);
            this.SettingsForm_Button_ResetVisual.TabIndex = 99;
            this.SettingsForm_Button_ResetVisual.TabStop = false;
            this.SettingsForm_Button_ResetVisual.Text = "Reset";
            this.SettingsForm_Button_ResetVisual.UseVisualStyleBackColor = true;
            this.SettingsForm_Button_ResetVisual.Click += new System.EventHandler(this.SettingsForm_Button_ResetVisual_Click);
            // 
            // SettingsForm_GroupBox_Sheets
            // 
            this.SettingsForm_GroupBox_Sheets.Controls.Add(this.SettingsForm_TextBox_SheetsLink);
            this.SettingsForm_GroupBox_Sheets.Controls.Add(this.SettingsForm_Label_SheetsLink);
            this.SettingsForm_GroupBox_Sheets.Controls.Add(this.panel1);
            this.SettingsForm_GroupBox_Sheets.Location = new System.Drawing.Point(10, 151);
            this.SettingsForm_GroupBox_Sheets.Name = "SettingsForm_GroupBox_Sheets";
            this.SettingsForm_GroupBox_Sheets.Size = new System.Drawing.Size(201, 80);
            this.SettingsForm_GroupBox_Sheets.TabIndex = 89;
            this.SettingsForm_GroupBox_Sheets.TabStop = false;
            this.SettingsForm_GroupBox_Sheets.Text = "Google Sheets Information";
            // 
            // SettingsForm_TextBox_SheetsLink
            // 
            this.SettingsForm_TextBox_SheetsLink.Location = new System.Drawing.Point(72, 21);
            this.SettingsForm_TextBox_SheetsLink.Name = "SettingsForm_TextBox_SheetsLink";
            this.SettingsForm_TextBox_SheetsLink.Size = new System.Drawing.Size(122, 20);
            this.SettingsForm_TextBox_SheetsLink.TabIndex = 4;
            this.SettingsForm_TextBox_SheetsLink.TextChanged += new System.EventHandler(this.Google_TextChanged);
            // 
            // SettingsForm_Label_SheetsLink
            // 
            this.SettingsForm_Label_SheetsLink.AutoSize = true;
            this.SettingsForm_Label_SheetsLink.Location = new System.Drawing.Point(9, 24);
            this.SettingsForm_Label_SheetsLink.Name = "SettingsForm_Label_SheetsLink";
            this.SettingsForm_Label_SheetsLink.Size = new System.Drawing.Size(61, 13);
            this.SettingsForm_Label_SheetsLink.TabIndex = 88;
            this.SettingsForm_Label_SheetsLink.Text = "Sheet Link:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.SettingsForm_Button_SaveGoogle);
            this.panel1.Controls.Add(this.SettingsForm_Button_Reset);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(195, 30);
            this.panel1.TabIndex = 0;
            // 
            // SettingsForm_Button_SaveGoogle
            // 
            this.SettingsForm_Button_SaveGoogle.Dock = System.Windows.Forms.DockStyle.Right;
            this.SettingsForm_Button_SaveGoogle.Enabled = false;
            this.SettingsForm_Button_SaveGoogle.Location = new System.Drawing.Point(131, 0);
            this.SettingsForm_Button_SaveGoogle.Name = "SettingsForm_Button_SaveGoogle";
            this.SettingsForm_Button_SaveGoogle.Size = new System.Drawing.Size(64, 30);
            this.SettingsForm_Button_SaveGoogle.TabIndex = 5;
            this.SettingsForm_Button_SaveGoogle.Text = "Save";
            this.SettingsForm_Button_SaveGoogle.UseVisualStyleBackColor = true;
            this.SettingsForm_Button_SaveGoogle.Click += new System.EventHandler(this.SettingsForm_Button_SaveGoogle_Click);
            // 
            // SettingsForm_Button_Reset
            // 
            this.SettingsForm_Button_Reset.Dock = System.Windows.Forms.DockStyle.Left;
            this.SettingsForm_Button_Reset.Location = new System.Drawing.Point(0, 0);
            this.SettingsForm_Button_Reset.Name = "SettingsForm_Button_Reset";
            this.SettingsForm_Button_Reset.Size = new System.Drawing.Size(64, 30);
            this.SettingsForm_Button_Reset.TabIndex = 86;
            this.SettingsForm_Button_Reset.TabStop = false;
            this.SettingsForm_Button_Reset.Text = "Reset";
            this.SettingsForm_Button_Reset.UseVisualStyleBackColor = true;
            this.SettingsForm_Button_Reset.Click += new System.EventHandler(this.SettingsForm_Button_Reset_Click);
            // 
            // Settings_CurrentUser
            // 
            this.Settings_CurrentUser.Location = new System.Drawing.Point(89, 8);
            this.Settings_CurrentUser.Name = "Settings_CurrentUser";
            this.Settings_CurrentUser.ReadOnly = true;
            this.Settings_CurrentUser.Size = new System.Drawing.Size(115, 20);
            this.Settings_CurrentUser.TabIndex = 100;
            this.Settings_CurrentUser.TabStop = false;
            // 
            // SettingsForm_Label_CurrentUser
            // 
            this.SettingsForm_Label_CurrentUser.AutoSize = true;
            this.SettingsForm_Label_CurrentUser.Location = new System.Drawing.Point(19, 10);
            this.SettingsForm_Label_CurrentUser.Name = "SettingsForm_Label_CurrentUser";
            this.SettingsForm_Label_CurrentUser.Size = new System.Drawing.Size(69, 13);
            this.SettingsForm_Label_CurrentUser.TabIndex = 98;
            this.SettingsForm_Label_CurrentUser.Text = "Current User:";
            // 
            // SettingsForm_Button_Exit
            // 
            this.SettingsForm_Button_Exit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SettingsForm_Button_Exit.Location = new System.Drawing.Point(0, 233);
            this.SettingsForm_Button_Exit.Name = "SettingsForm_Button_Exit";
            this.SettingsForm_Button_Exit.Size = new System.Drawing.Size(218, 30);
            this.SettingsForm_Button_Exit.TabIndex = 6;
            this.SettingsForm_Button_Exit.Text = "Exit";
            this.SettingsForm_Button_Exit.UseVisualStyleBackColor = true;
            this.SettingsForm_Button_Exit.Click += new System.EventHandler(this.SettingsForm_Button_Exit_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 263);
            this.Controls.Add(this.SettingsForm_Button_Exit);
            this.Controls.Add(this.Settings_CurrentUser);
            this.Controls.Add(this.SettingsForm_Label_CurrentUser);
            this.Controls.Add(this.SettingsForm_GroupBox_Sheets);
            this.Controls.Add(this.SettingsForm_GroupBox_Visual);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(234, 302);
            this.Name = "SettingsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.SettingsForm_GroupBox_Visual.ResumeLayout(false);
            this.SettingsForm_GroupBox_Visual.PerformLayout();
            this.SettingsForm_Panel_VisualButtons.ResumeLayout(false);
            this.SettingsForm_GroupBox_Sheets.ResumeLayout(false);
            this.SettingsForm_GroupBox_Sheets.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GroupBox SettingsForm_GroupBox_Visual;
        private Panel SettingsForm_Panel_VisualButtons;
        private TextBox SettingsForm_TextBox_VisualPassword;
        private TextBox SettingsForm_TextBox_VisualUserName;
        private Label SettingsForm_Label_VisualPassword;
        private Label SettingsForm_Label_VisualUserName;
        private Button SettingsForm_Button_SaveVisual;
        private Button SettingsForm_Button_ResetVisual;
        private GroupBox SettingsForm_GroupBox_Sheets;
        private TextBox SettingsForm_TextBox_SheetsLink;
        private Label SettingsForm_Label_SheetsLink;
        private Panel panel1;
        private Button SettingsForm_Button_SaveGoogle;
        private Button SettingsForm_Button_Reset;
        private TextBox Settings_CurrentUser;
        private Label SettingsForm_Label_CurrentUser;
        private Button SettingsForm_Button_Exit;
    }
}