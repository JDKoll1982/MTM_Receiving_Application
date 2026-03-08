namespace Visual_Inventory_Assistant.Windows
{
    partial class NewUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewUser));
            this.NewUserForm_GroupBox_Main = new System.Windows.Forms.GroupBox();
            this.NoticeLabel = new System.Windows.Forms.Label();
            this.NewUserForm_TextBox_Password = new System.Windows.Forms.TextBox();
            this.NewUserForm_TextBox_UserName = new System.Windows.Forms.TextBox();
            this.NewUserForm_Label_VisualPassword = new System.Windows.Forms.Label();
            this.NewUserForm_Label_VisualUserName = new System.Windows.Forms.Label();
            this.NewUserForm_Panel_VisualButtons = new System.Windows.Forms.Panel();
            this.NewUserForm_Button_Save = new System.Windows.Forms.Button();
            this.NewUserForm_GroupBox_Main.SuspendLayout();
            this.NewUserForm_Panel_VisualButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // NewUserForm_GroupBox_Main
            // 
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NoticeLabel);
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NewUserForm_TextBox_Password);
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NewUserForm_TextBox_UserName);
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NewUserForm_Label_VisualPassword);
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NewUserForm_Label_VisualUserName);
            this.NewUserForm_GroupBox_Main.Controls.Add(this.NewUserForm_Panel_VisualButtons);
            this.NewUserForm_GroupBox_Main.Location = new System.Drawing.Point(12, 12);
            this.NewUserForm_GroupBox_Main.Name = "NewUserForm_GroupBox_Main";
            this.NewUserForm_GroupBox_Main.Size = new System.Drawing.Size(245, 167);
            this.NewUserForm_GroupBox_Main.TabIndex = 1;
            this.NewUserForm_GroupBox_Main.TabStop = false;
            // 
            // NoticeLabel
            // 
            this.NoticeLabel.AutoEllipsis = true;
            this.NoticeLabel.Location = new System.Drawing.Point(6, 72);
            this.NoticeLabel.Name = "NoticeLabel";
            this.NoticeLabel.Size = new System.Drawing.Size(233, 54);
            this.NoticeLabel.TabIndex = 3;
            this.NoticeLabel.Text = "If you select a User Name and Password that is different from your Visual User Na" +
    "me and Password remember to go into File - Settings and change your Visual Login" +
    " Info.";
            this.NoticeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NewUserForm_TextBox_Password
            // 
            this.NewUserForm_TextBox_Password.Location = new System.Drawing.Point(72, 49);
            this.NewUserForm_TextBox_Password.Name = "NewUserForm_TextBox_Password";
            this.NewUserForm_TextBox_Password.Size = new System.Drawing.Size(167, 20);
            this.NewUserForm_TextBox_Password.TabIndex = 2;
            this.NewUserForm_TextBox_Password.UseSystemPasswordChar = true;
            this.NewUserForm_TextBox_Password.TextChanged += new System.EventHandler(this.NewUserTextChange);
            // 
            // NewUserForm_TextBox_UserName
            // 
            this.NewUserForm_TextBox_UserName.Location = new System.Drawing.Point(72, 21);
            this.NewUserForm_TextBox_UserName.Name = "NewUserForm_TextBox_UserName";
            this.NewUserForm_TextBox_UserName.Size = new System.Drawing.Size(167, 20);
            this.NewUserForm_TextBox_UserName.TabIndex = 1;
            this.NewUserForm_TextBox_UserName.TextChanged += new System.EventHandler(this.NewUserTextChange);
            // 
            // NewUserForm_Label_VisualPassword
            // 
            this.NewUserForm_Label_VisualPassword.AutoSize = true;
            this.NewUserForm_Label_VisualPassword.Location = new System.Drawing.Point(15, 52);
            this.NewUserForm_Label_VisualPassword.Name = "NewUserForm_Label_VisualPassword";
            this.NewUserForm_Label_VisualPassword.Size = new System.Drawing.Size(56, 13);
            this.NewUserForm_Label_VisualPassword.TabIndex = 2;
            this.NewUserForm_Label_VisualPassword.Text = "Password:";
            // 
            // NewUserForm_Label_VisualUserName
            // 
            this.NewUserForm_Label_VisualUserName.AutoSize = true;
            this.NewUserForm_Label_VisualUserName.Location = new System.Drawing.Point(9, 24);
            this.NewUserForm_Label_VisualUserName.Name = "NewUserForm_Label_VisualUserName";
            this.NewUserForm_Label_VisualUserName.Size = new System.Drawing.Size(63, 13);
            this.NewUserForm_Label_VisualUserName.TabIndex = 1;
            this.NewUserForm_Label_VisualUserName.Text = "User Name:";
            // 
            // NewUserForm_Panel_VisualButtons
            // 
            this.NewUserForm_Panel_VisualButtons.Controls.Add(this.NewUserForm_Button_Save);
            this.NewUserForm_Panel_VisualButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.NewUserForm_Panel_VisualButtons.Location = new System.Drawing.Point(3, 134);
            this.NewUserForm_Panel_VisualButtons.Name = "NewUserForm_Panel_VisualButtons";
            this.NewUserForm_Panel_VisualButtons.Size = new System.Drawing.Size(239, 30);
            this.NewUserForm_Panel_VisualButtons.TabIndex = 0;
            // 
            // NewUserForm_Button_Save
            // 
            this.NewUserForm_Button_Save.Dock = System.Windows.Forms.DockStyle.Right;
            this.NewUserForm_Button_Save.Enabled = false;
            this.NewUserForm_Button_Save.Location = new System.Drawing.Point(175, 0);
            this.NewUserForm_Button_Save.Name = "NewUserForm_Button_Save";
            this.NewUserForm_Button_Save.Size = new System.Drawing.Size(64, 30);
            this.NewUserForm_Button_Save.TabIndex = 3;
            this.NewUserForm_Button_Save.Text = "Save";
            this.NewUserForm_Button_Save.UseVisualStyleBackColor = true;
            this.NewUserForm_Button_Save.Click += new System.EventHandler(this.NewUserForm_Button_Save_Click);
            // 
            // NewUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 191);
            this.Controls.Add(this.NewUserForm_GroupBox_Main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(285, 230);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(285, 230);
            this.Name = "NewUser";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New User Entry";
            this.NewUserForm_GroupBox_Main.ResumeLayout(false);
            this.NewUserForm_GroupBox_Main.PerformLayout();
            this.NewUserForm_Panel_VisualButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox NewUserForm_GroupBox_Main;
        private System.Windows.Forms.TextBox NewUserForm_TextBox_Password;
        private System.Windows.Forms.TextBox NewUserForm_TextBox_UserName;
        private System.Windows.Forms.Label NewUserForm_Label_VisualPassword;
        private System.Windows.Forms.Label NewUserForm_Label_VisualUserName;
        private System.Windows.Forms.Panel NewUserForm_Panel_VisualButtons;
        private System.Windows.Forms.Button NewUserForm_Button_Save;
        private System.Windows.Forms.Label NoticeLabel;
    }
}