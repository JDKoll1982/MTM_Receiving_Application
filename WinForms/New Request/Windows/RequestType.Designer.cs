namespace MTM_Waitlist_Application_2._0.WinForms.New_Request.Windows
{
    partial class NewRequest_Window_RequestType
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
            groupBox1 = new GroupBox();
            RequestType_ComboBox_Operation = new ComboBox();
            RequestType_ComboBox_PartID = new ComboBox();
            RequestType_ComboBox_WorkOrder = new ComboBox();
            RequestType_ComboBox_WorkCenter = new ComboBox();
            RequestType_Label_Operation = new Label();
            RequestType_Label_PartID = new Label();
            RequestType_Label_WorkOrder = new Label();
            RequestType_Label_WorkCenter = new Label();
            RequestType_GroupBox_Buttons = new GroupBox();
            RequestType_Button_Scrap = new Button();
            RequestType_Button_NewJobSetup = new Button();
            RequestType_Button_Die = new Button();
            RequestType_Button_Coil = new Button();
            RequestType_Button_Bring = new Button();
            RequestType_Button_Pickup = new Button();
            groupBox1.SuspendLayout();
            RequestType_GroupBox_Buttons.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(RequestType_ComboBox_Operation);
            groupBox1.Controls.Add(RequestType_ComboBox_PartID);
            groupBox1.Controls.Add(RequestType_ComboBox_WorkOrder);
            groupBox1.Controls.Add(RequestType_ComboBox_WorkCenter);
            groupBox1.Controls.Add(RequestType_Label_Operation);
            groupBox1.Controls.Add(RequestType_Label_PartID);
            groupBox1.Controls.Add(RequestType_Label_WorkOrder);
            groupBox1.Controls.Add(RequestType_Label_WorkCenter);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(569, 78);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Work Center and Job Details";
            // 
            // RequestType_ComboBox_Operation
            // 
            RequestType_ComboBox_Operation.Enabled = false;
            RequestType_ComboBox_Operation.FormattingEnabled = true;
            RequestType_ComboBox_Operation.Location = new Point(456, 42);
            RequestType_ComboBox_Operation.Name = "RequestType_ComboBox_Operation";
            RequestType_ComboBox_Operation.Size = new Size(106, 23);
            RequestType_ComboBox_Operation.TabIndex = 7;
            // 
            // RequestType_ComboBox_PartID
            // 
            RequestType_ComboBox_PartID.Enabled = false;
            RequestType_ComboBox_PartID.FormattingEnabled = true;
            RequestType_ComboBox_PartID.Location = new Point(306, 42);
            RequestType_ComboBox_PartID.Name = "RequestType_ComboBox_PartID";
            RequestType_ComboBox_PartID.Size = new Size(144, 23);
            RequestType_ComboBox_PartID.TabIndex = 6;
            // 
            // RequestType_ComboBox_WorkOrder
            // 
            RequestType_ComboBox_WorkOrder.Enabled = false;
            RequestType_ComboBox_WorkOrder.FormattingEnabled = true;
            RequestType_ComboBox_WorkOrder.Location = new Point(156, 42);
            RequestType_ComboBox_WorkOrder.Name = "RequestType_ComboBox_WorkOrder";
            RequestType_ComboBox_WorkOrder.Size = new Size(144, 23);
            RequestType_ComboBox_WorkOrder.TabIndex = 5;
            // 
            // RequestType_ComboBox_WorkCenter
            // 
            RequestType_ComboBox_WorkCenter.FormattingEnabled = true;
            RequestType_ComboBox_WorkCenter.Location = new Point(6, 42);
            RequestType_ComboBox_WorkCenter.Name = "RequestType_ComboBox_WorkCenter";
            RequestType_ComboBox_WorkCenter.Size = new Size(144, 23);
            RequestType_ComboBox_WorkCenter.TabIndex = 4;
            RequestType_ComboBox_WorkCenter.SelectedIndexChanged += RequestType_ComboBox_WorkCenter_SelectedIndexChanged;
            // 
            // RequestType_Label_Operation
            // 
            RequestType_Label_Operation.AutoSize = true;
            RequestType_Label_Operation.Location = new Point(479, 24);
            RequestType_Label_Operation.Name = "RequestType_Label_Operation";
            RequestType_Label_Operation.Size = new Size(60, 15);
            RequestType_Label_Operation.TabIndex = 3;
            RequestType_Label_Operation.Text = "Operation";
            // 
            // RequestType_Label_PartID
            // 
            RequestType_Label_PartID.AutoSize = true;
            RequestType_Label_PartID.Location = new Point(341, 24);
            RequestType_Label_PartID.Name = "RequestType_Label_PartID";
            RequestType_Label_PartID.Size = new Size(75, 15);
            RequestType_Label_PartID.TabIndex = 2;
            RequestType_Label_PartID.Text = "Part Number";
            // 
            // RequestType_Label_WorkOrder
            // 
            RequestType_Label_WorkOrder.AutoSize = true;
            RequestType_Label_WorkOrder.Location = new Point(194, 24);
            RequestType_Label_WorkOrder.Name = "RequestType_Label_WorkOrder";
            RequestType_Label_WorkOrder.Size = new Size(68, 15);
            RequestType_Label_WorkOrder.TabIndex = 1;
            RequestType_Label_WorkOrder.Text = "Work Order";
            // 
            // RequestType_Label_WorkCenter
            // 
            RequestType_Label_WorkCenter.AutoSize = true;
            RequestType_Label_WorkCenter.Location = new Point(42, 24);
            RequestType_Label_WorkCenter.Name = "RequestType_Label_WorkCenter";
            RequestType_Label_WorkCenter.Size = new Size(73, 15);
            RequestType_Label_WorkCenter.TabIndex = 0;
            RequestType_Label_WorkCenter.Text = "Work Center";
            // 
            // RequestType_GroupBox_Buttons
            // 
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_Scrap);
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_NewJobSetup);
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_Die);
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_Coil);
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_Bring);
            RequestType_GroupBox_Buttons.Controls.Add(RequestType_Button_Pickup);
            RequestType_GroupBox_Buttons.Location = new Point(12, 96);
            RequestType_GroupBox_Buttons.Name = "RequestType_GroupBox_Buttons";
            RequestType_GroupBox_Buttons.Size = new Size(569, 60);
            RequestType_GroupBox_Buttons.TabIndex = 1;
            RequestType_GroupBox_Buttons.TabStop = false;
            RequestType_GroupBox_Buttons.Text = "What does your request involve?";
            // 
            // RequestType_Button_Scrap
            // 
            RequestType_Button_Scrap.Location = new Point(382, 22);
            RequestType_Button_Scrap.Name = "RequestType_Button_Scrap";
            RequestType_Button_Scrap.Size = new Size(84, 32);
            RequestType_Button_Scrap.TabIndex = 5;
            RequestType_Button_Scrap.Text = "Scrap";
            RequestType_Button_Scrap.UseVisualStyleBackColor = true;
            // 
            // RequestType_Button_NewJobSetup
            // 
            RequestType_Button_NewJobSetup.Location = new Point(476, 22);
            RequestType_Button_NewJobSetup.Name = "RequestType_Button_NewJobSetup";
            RequestType_Button_NewJobSetup.Size = new Size(84, 32);
            RequestType_Button_NewJobSetup.TabIndex = 4;
            RequestType_Button_NewJobSetup.Text = "New Job";
            RequestType_Button_NewJobSetup.UseVisualStyleBackColor = true;
            RequestType_Button_NewJobSetup.Click += RequestType_Button_NewJobSetup_Click;
            // 
            // RequestType_Button_Die
            // 
            RequestType_Button_Die.Location = new Point(288, 22);
            RequestType_Button_Die.Name = "RequestType_Button_Die";
            RequestType_Button_Die.Size = new Size(84, 32);
            RequestType_Button_Die.TabIndex = 3;
            RequestType_Button_Die.Text = "Die";
            RequestType_Button_Die.UseVisualStyleBackColor = true;
            // 
            // RequestType_Button_Coil
            // 
            RequestType_Button_Coil.Location = new Point(194, 22);
            RequestType_Button_Coil.Name = "RequestType_Button_Coil";
            RequestType_Button_Coil.Size = new Size(84, 32);
            RequestType_Button_Coil.TabIndex = 2;
            RequestType_Button_Coil.Text = "Coil";
            RequestType_Button_Coil.UseVisualStyleBackColor = true;
            // 
            // RequestType_Button_Bring
            // 
            RequestType_Button_Bring.Location = new Point(100, 22);
            RequestType_Button_Bring.Name = "RequestType_Button_Bring";
            RequestType_Button_Bring.Size = new Size(84, 32);
            RequestType_Button_Bring.TabIndex = 1;
            RequestType_Button_Bring.Text = "Bring";
            RequestType_Button_Bring.UseVisualStyleBackColor = true;
            // 
            // RequestType_Button_Pickup
            // 
            RequestType_Button_Pickup.Location = new Point(6, 22);
            RequestType_Button_Pickup.Name = "RequestType_Button_Pickup";
            RequestType_Button_Pickup.Size = new Size(84, 32);
            RequestType_Button_Pickup.TabIndex = 0;
            RequestType_Button_Pickup.Text = "Pickup";
            RequestType_Button_Pickup.UseVisualStyleBackColor = true;
            RequestType_Button_Pickup.Click += RequestType_Button_Pickup_Click;
            // 
            // NewRequest_Window_RequestType
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(590, 163);
            Controls.Add(RequestType_GroupBox_Buttons);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            Name = "NewRequest_Window_RequestType";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Request - Request Type";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            RequestType_GroupBox_Buttons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private GroupBox RequestType_GroupBox_Buttons;
        private ComboBox RequestType_ComboBox_Operation;
        private ComboBox RequestType_ComboBox_PartID;
        private ComboBox RequestType_ComboBox_WorkOrder;
        private ComboBox RequestType_ComboBox_WorkCenter;
        private Label RequestType_Label_Operation;
        private Label RequestType_Label_PartID;
        private Label RequestType_Label_WorkOrder;
        private Label RequestType_Label_WorkCenter;
        private Button RequestType_Button_NewJobSetup;
        private Button RequestType_Button_Die;
        private Button RequestType_Button_Coil;
        private Button RequestType_Button_Bring;
        private Button RequestType_Button_Pickup;
        private Button RequestType_Button_Scrap;
    }
}