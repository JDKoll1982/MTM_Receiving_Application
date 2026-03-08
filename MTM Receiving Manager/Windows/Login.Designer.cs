namespace Visual_Inventory_Assistant.Windows
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.LoginForm_GroupBox_Main = new System.Windows.Forms.GroupBox();
            this.LoginForm_TextBox_Password = new System.Windows.Forms.TextBox();
            this.LoginForm_TextBox_UserName = new System.Windows.Forms.TextBox();
            this.LoginForm_Label_VisualPassword = new System.Windows.Forms.Label();
            this.LoginForm_Label_VisualUserName = new System.Windows.Forms.Label();
            this.LoginForm_Panel_VisualButtons = new System.Windows.Forms.Panel();
            this.LoginForm_Button_Save = new System.Windows.Forms.Button();
            this.LoginForm_Button_NewUser = new System.Windows.Forms.Button();
            this.LoginForm_GroupBox_Main.SuspendLayout();
            this.LoginForm_Panel_VisualButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoginForm_GroupBox_Main
            // 
            this.LoginForm_GroupBox_Main.Controls.Add(this.LoginForm_TextBox_Password);
            this.LoginForm_GroupBox_Main.Controls.Add(this.LoginForm_TextBox_UserName);
            this.LoginForm_GroupBox_Main.Controls.Add(this.LoginForm_Label_VisualPassword);
            this.LoginForm_GroupBox_Main.Controls.Add(this.LoginForm_Label_VisualUserName);
            this.LoginForm_GroupBox_Main.Controls.Add(this.LoginForm_Panel_VisualButtons);
            this.LoginForm_GroupBox_Main.Location = new System.Drawing.Point(12, 12);
            this.LoginForm_GroupBox_Main.Name = "LoginForm_GroupBox_Main";
            this.LoginForm_GroupBox_Main.Size = new System.Drawing.Size(245, 114);
            this.LoginForm_GroupBox_Main.TabIndex = 96;
            this.LoginForm_GroupBox_Main.TabStop = false;
            // 
            // LoginForm_TextBox_Password
            // 
            this.LoginForm_TextBox_Password.Location = new System.Drawing.Point(72, 49);
            this.LoginForm_TextBox_Password.Name = "LoginForm_TextBox_Password";
            this.LoginForm_TextBox_Password.Size = new System.Drawing.Size(167, 20);
            this.LoginForm_TextBox_Password.TabIndex = 2;
            this.LoginForm_TextBox_Password.UseSystemPasswordChar = true;
            // 
            // LoginForm_TextBox_UserName
            // 
            this.LoginForm_TextBox_UserName.Location = new System.Drawing.Point(72, 21);
            this.LoginForm_TextBox_UserName.Name = "LoginForm_TextBox_UserName";
            this.LoginForm_TextBox_UserName.Size = new System.Drawing.Size(167, 20);
            this.LoginForm_TextBox_UserName.TabIndex = 1;
            // 
            // LoginForm_Label_VisualPassword
            // 
            this.LoginForm_Label_VisualPassword.AutoSize = true;
            this.LoginForm_Label_VisualPassword.Location = new System.Drawing.Point(15, 52);
            this.LoginForm_Label_VisualPassword.Name = "LoginForm_Label_VisualPassword";
            this.LoginForm_Label_VisualPassword.Size = new System.Drawing.Size(56, 13);
            this.LoginForm_Label_VisualPassword.TabIndex = 99;
            this.LoginForm_Label_VisualPassword.Text = "Password:";
            // 
            // LoginForm_Label_VisualUserName
            // 
            this.LoginForm_Label_VisualUserName.AutoSize = true;
            this.LoginForm_Label_VisualUserName.Location = new System.Drawing.Point(9, 24);
            this.LoginForm_Label_VisualUserName.Name = "LoginForm_Label_VisualUserName";
            this.LoginForm_Label_VisualUserName.Size = new System.Drawing.Size(63, 13);
            this.LoginForm_Label_VisualUserName.TabIndex = 98;
            this.LoginForm_Label_VisualUserName.Text = "User Name:";
            // 
            // LoginForm_Panel_VisualButtons
            // 
            this.LoginForm_Panel_VisualButtons.Controls.Add(this.LoginForm_Button_Save);
            this.LoginForm_Panel_VisualButtons.Controls.Add(this.LoginForm_Button_NewUser);
            this.LoginForm_Panel_VisualButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LoginForm_Panel_VisualButtons.Location = new System.Drawing.Point(3, 81);
            this.LoginForm_Panel_VisualButtons.Name = "LoginForm_Panel_VisualButtons";
            this.LoginForm_Panel_VisualButtons.Size = new System.Drawing.Size(239, 30);
            this.LoginForm_Panel_VisualButtons.TabIndex = 98;
            // 
            // LoginForm_Button_Save
            // 
            this.LoginForm_Button_Save.Dock = System.Windows.Forms.DockStyle.Right;
            this.LoginForm_Button_Save.Location = new System.Drawing.Point(175, 0);
            this.LoginForm_Button_Save.Name = "LoginForm_Button_Save";
            this.LoginForm_Button_Save.Size = new System.Drawing.Size(64, 30);
            this.LoginForm_Button_Save.TabIndex = 3;
            this.LoginForm_Button_Save.Text = "Login";
            this.LoginForm_Button_Save.UseVisualStyleBackColor = true;
            this.LoginForm_Button_Save.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // LoginForm_Button_NewUser
            // 
            this.LoginForm_Button_NewUser.Dock = System.Windows.Forms.DockStyle.Left;
            this.LoginForm_Button_NewUser.Location = new System.Drawing.Point(0, 0);
            this.LoginForm_Button_NewUser.Name = "LoginForm_Button_NewUser";
            this.LoginForm_Button_NewUser.Size = new System.Drawing.Size(64, 30);
            this.LoginForm_Button_NewUser.TabIndex = 97;
            this.LoginForm_Button_NewUser.TabStop = false;
            this.LoginForm_Button_NewUser.Text = "New User";
            this.LoginForm_Button_NewUser.UseVisualStyleBackColor = true;
            this.LoginForm_Button_NewUser.Click += new System.EventHandler(this.LoginForm_Button_NewUser_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 133);
            this.Controls.Add(this.LoginForm_GroupBox_Main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(285, 172);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(285, 172);
            this.Name = "Login";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.LoginForm_GroupBox_Main.ResumeLayout(false);
            this.LoginForm_GroupBox_Main.PerformLayout();
            this.LoginForm_Panel_VisualButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox LoginForm_GroupBox_Main;
        private System.Windows.Forms.TextBox LoginForm_TextBox_Password;
        private System.Windows.Forms.TextBox LoginForm_TextBox_UserName;
        private System.Windows.Forms.Label LoginForm_Label_VisualPassword;
        private System.Windows.Forms.Label LoginForm_Label_VisualUserName;
        private System.Windows.Forms.Panel LoginForm_Panel_VisualButtons;
        private System.Windows.Forms.Button LoginForm_Button_Save;
        private System.Windows.Forms.Button LoginForm_Button_NewUser;
    }
}