using System.Drawing;
using System.Windows.Forms;

namespace Visual_Inventory_Assistant.Windows
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            MainForm_MenuStrip = new MenuStrip();
            MainForm_MenuStrip_File = new ToolStripMenuItem();
            MainForm_MenuStrip_File_Settings = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            setupEmailToolStripMenuItem = new ToolStripMenuItem();
            MainForm_GroupBox_CurrentItem = new GroupBox();
            MainForm_TextBox_WorkOrder = new TextBox();
            MainForm_Label_Grouped = new Label();
            MainForm_TextBox_Transaction = new TextBox();
            MainForm_TextBox_To = new TextBox();
            MainForm_TextBox_From = new TextBox();
            MainForm_TextBox_Quantity = new TextBox();
            MainForm_TextBox_PartID = new TextBox();
            MainForm_Label_Transaction = new Label();
            MainForm_Label_To = new Label();
            MainForm_Label_From = new Label();
            MainForm_Label_Quantity = new Label();
            MainForm_Label_PartID = new Label();
            MainForm_Panel_Buttons = new Panel();
            MainForm_TextBox_Notes = new TextBox();
            MainForm_TextBox_TagQty = new TextBox();
            MainForm_Button_SendAddRemove = new Button();
            MainForm_Button_SendClosed = new Button();
            MainForm_Button_SentOR = new Button();
            MainForm_Button_Skip = new Button();
            MainForm_Button_Next = new Button();
            MainForm_Button_Exit = new Button();
            MainForm_StatusStrip = new StatusStrip();
            MainForm_StatusText_Loading = new ToolStripStatusLabel();
            MainForm_MenuStrip.SuspendLayout();
            MainForm_GroupBox_CurrentItem.SuspendLayout();
            MainForm_Panel_Buttons.SuspendLayout();
            MainForm_StatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // MainForm_MenuStrip
            // 
            MainForm_MenuStrip.Items.AddRange(new ToolStripItem[] { MainForm_MenuStrip_File });
            MainForm_MenuStrip.Location = new Point(0, 0);
            MainForm_MenuStrip.Name = "MainForm_MenuStrip";
            MainForm_MenuStrip.Size = new Size(492, 24);
            MainForm_MenuStrip.TabIndex = 0;
            MainForm_MenuStrip.Text = "menuStrip1";
            // 
            // MainForm_MenuStrip_File
            // 
            MainForm_MenuStrip_File.DropDownItems.AddRange(new ToolStripItem[] { MainForm_MenuStrip_File_Settings, toolStripSeparator1, exitToolStripMenuItem, setupEmailToolStripMenuItem });
            MainForm_MenuStrip_File.Name = "MainForm_MenuStrip_File";
            MainForm_MenuStrip_File.Size = new Size(37, 20);
            MainForm_MenuStrip_File.Text = "File";
            // 
            // MainForm_MenuStrip_File_Settings
            // 
            MainForm_MenuStrip_File_Settings.Name = "MainForm_MenuStrip_File_Settings";
            MainForm_MenuStrip_File_Settings.ShortcutKeys = Keys.Alt | Keys.S;
            MainForm_MenuStrip_File_Settings.Size = new Size(180, 22);
            MainForm_MenuStrip_File_Settings.Text = "Settings";
            MainForm_MenuStrip_File_Settings.Click += MainForm_MenuStrip_File_Settings_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(180, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // setupEmailToolStripMenuItem
            // 
            setupEmailToolStripMenuItem.Name = "setupEmailToolStripMenuItem";
            setupEmailToolStripMenuItem.Size = new Size(180, 22);
            setupEmailToolStripMenuItem.Text = "View Google Sheets";
            setupEmailToolStripMenuItem.Click += MainForm_MenuStrip_Email_Click;
            // 
            // MainForm_GroupBox_CurrentItem
            // 
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_WorkOrder);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_Grouped);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_Transaction);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_To);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_From);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_Quantity);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_TextBox_PartID);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_Transaction);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_To);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_From);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_Quantity);
            MainForm_GroupBox_CurrentItem.Controls.Add(MainForm_Label_PartID);
            MainForm_GroupBox_CurrentItem.Location = new Point(12, 27);
            MainForm_GroupBox_CurrentItem.Margin = new Padding(4, 3, 4, 3);
            MainForm_GroupBox_CurrentItem.Name = "MainForm_GroupBox_CurrentItem";
            MainForm_GroupBox_CurrentItem.Padding = new Padding(4, 3, 4, 3);
            MainForm_GroupBox_CurrentItem.Size = new Size(474, 118);
            MainForm_GroupBox_CurrentItem.TabIndex = 1;
            MainForm_GroupBox_CurrentItem.TabStop = false;
            MainForm_GroupBox_CurrentItem.Text = "Current Item";
            // 
            // MainForm_TextBox_WorkOrder
            // 
            MainForm_TextBox_WorkOrder.Location = new Point(127, 83);
            MainForm_TextBox_WorkOrder.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_WorkOrder.Name = "MainForm_TextBox_WorkOrder";
            MainForm_TextBox_WorkOrder.ReadOnly = true;
            MainForm_TextBox_WorkOrder.Size = new Size(100, 23);
            MainForm_TextBox_WorkOrder.TabIndex = 11;
            MainForm_TextBox_WorkOrder.TextAlign = HorizontalAlignment.Center;
            // 
            // MainForm_Label_Grouped
            // 
            MainForm_Label_Grouped.AutoSize = true;
            MainForm_Label_Grouped.Location = new Point(51, 87);
            MainForm_Label_Grouped.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_Grouped.Name = "MainForm_Label_Grouped";
            MainForm_Label_Grouped.Size = new Size(71, 15);
            MainForm_Label_Grouped.TabIndex = 10;
            MainForm_Label_Grouped.Text = "Work Order:";
            // 
            // MainForm_TextBox_Transaction
            // 
            MainForm_TextBox_Transaction.Location = new Point(365, 83);
            MainForm_TextBox_Transaction.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_Transaction.Name = "MainForm_TextBox_Transaction";
            MainForm_TextBox_Transaction.ReadOnly = true;
            MainForm_TextBox_Transaction.Size = new Size(100, 23);
            MainForm_TextBox_Transaction.TabIndex = 9;
            MainForm_TextBox_Transaction.TextAlign = HorizontalAlignment.Center;
            // 
            // MainForm_TextBox_To
            // 
            MainForm_TextBox_To.Location = new Point(295, 54);
            MainForm_TextBox_To.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_To.Name = "MainForm_TextBox_To";
            MainForm_TextBox_To.ReadOnly = true;
            MainForm_TextBox_To.Size = new Size(170, 23);
            MainForm_TextBox_To.TabIndex = 8;
            // 
            // MainForm_TextBox_From
            // 
            MainForm_TextBox_From.Location = new Point(57, 54);
            MainForm_TextBox_From.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_From.Name = "MainForm_TextBox_From";
            MainForm_TextBox_From.ReadOnly = true;
            MainForm_TextBox_From.Size = new Size(170, 23);
            MainForm_TextBox_From.TabIndex = 7;
            // 
            // MainForm_TextBox_Quantity
            // 
            MainForm_TextBox_Quantity.Location = new Point(295, 25);
            MainForm_TextBox_Quantity.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_Quantity.Name = "MainForm_TextBox_Quantity";
            MainForm_TextBox_Quantity.ReadOnly = true;
            MainForm_TextBox_Quantity.Size = new Size(170, 23);
            MainForm_TextBox_Quantity.TabIndex = 6;
            // 
            // MainForm_TextBox_PartID
            // 
            MainForm_TextBox_PartID.Location = new Point(57, 25);
            MainForm_TextBox_PartID.Margin = new Padding(4, 3, 4, 3);
            MainForm_TextBox_PartID.Name = "MainForm_TextBox_PartID";
            MainForm_TextBox_PartID.ReadOnly = true;
            MainForm_TextBox_PartID.Size = new Size(170, 23);
            MainForm_TextBox_PartID.TabIndex = 5;
            // 
            // MainForm_Label_Transaction
            // 
            MainForm_Label_Transaction.AutoSize = true;
            MainForm_Label_Transaction.Location = new Point(288, 87);
            MainForm_Label_Transaction.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_Transaction.Name = "MainForm_Label_Transaction";
            MainForm_Label_Transaction.Size = new Size(70, 15);
            MainForm_Label_Transaction.TabIndex = 4;
            MainForm_Label_Transaction.Text = "Transaction:";
            // 
            // MainForm_Label_To
            // 
            MainForm_Label_To.AutoSize = true;
            MainForm_Label_To.Location = new Point(266, 58);
            MainForm_Label_To.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_To.Name = "MainForm_Label_To";
            MainForm_Label_To.Size = new Size(22, 15);
            MainForm_Label_To.TabIndex = 3;
            MainForm_Label_To.Text = "To:";
            // 
            // MainForm_Label_From
            // 
            MainForm_Label_From.AutoSize = true;
            MainForm_Label_From.Location = new Point(13, 58);
            MainForm_Label_From.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_From.Name = "MainForm_Label_From";
            MainForm_Label_From.Size = new Size(38, 15);
            MainForm_Label_From.TabIndex = 2;
            MainForm_Label_From.Text = "From:";
            // 
            // MainForm_Label_Quantity
            // 
            MainForm_Label_Quantity.AutoSize = true;
            MainForm_Label_Quantity.Location = new Point(233, 29);
            MainForm_Label_Quantity.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_Quantity.Name = "MainForm_Label_Quantity";
            MainForm_Label_Quantity.Size = new Size(56, 15);
            MainForm_Label_Quantity.TabIndex = 1;
            MainForm_Label_Quantity.Text = "Quantity:";
            // 
            // MainForm_Label_PartID
            // 
            MainForm_Label_PartID.AutoSize = true;
            MainForm_Label_PartID.Location = new Point(6, 29);
            MainForm_Label_PartID.Margin = new Padding(4, 0, 4, 0);
            MainForm_Label_PartID.Name = "MainForm_Label_PartID";
            MainForm_Label_PartID.Size = new Size(45, 15);
            MainForm_Label_PartID.TabIndex = 0;
            MainForm_Label_PartID.Text = "Part ID:";
            // 
            // MainForm_Panel_Buttons
            // 
            MainForm_Panel_Buttons.Controls.Add(MainForm_TextBox_Notes);
            MainForm_Panel_Buttons.Controls.Add(MainForm_TextBox_TagQty);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_SendAddRemove);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_SendClosed);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_SentOR);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_Skip);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_Next);
            MainForm_Panel_Buttons.Controls.Add(MainForm_Button_Exit);
            MainForm_Panel_Buttons.Location = new Point(12, 151);
            MainForm_Panel_Buttons.Margin = new Padding(4, 3, 4, 3);
            MainForm_Panel_Buttons.Name = "MainForm_Panel_Buttons";
            MainForm_Panel_Buttons.Size = new Size(474, 95);
            MainForm_Panel_Buttons.TabIndex = 2;
            // 
            // MainForm_TextBox_Notes
            // 
            MainForm_TextBox_Notes.Location = new Point(3, 69);
            MainForm_TextBox_Notes.Name = "MainForm_TextBox_Notes";
            MainForm_TextBox_Notes.PlaceholderText = "Enter notes here.";
            MainForm_TextBox_Notes.Size = new Size(465, 23);
            MainForm_TextBox_Notes.TabIndex = 7;
            MainForm_TextBox_Notes.TextChanged += MainForm_TextBox_Notes_TextChanged;
            // 
            // MainForm_TextBox_TagQty
            // 
            MainForm_TextBox_TagQty.Location = new Point(168, 42);
            MainForm_TextBox_TagQty.Name = "MainForm_TextBox_TagQty";
            MainForm_TextBox_TagQty.PlaceholderText = "Quantity on Tag";
            MainForm_TextBox_TagQty.Size = new Size(138, 23);
            MainForm_TextBox_TagQty.TabIndex = 6;
            MainForm_TextBox_TagQty.TextChanged += MainForm_TextBox_TagQty_TextChanged;
            // 
            // MainForm_Button_SendAddRemove
            // 
            MainForm_Button_SendAddRemove.Enabled = false;
            MainForm_Button_SendAddRemove.Location = new Point(168, 3);
            MainForm_Button_SendAddRemove.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_SendAddRemove.Name = "MainForm_Button_SendAddRemove";
            MainForm_Button_SendAddRemove.Size = new Size(138, 29);
            MainForm_Button_SendAddRemove.TabIndex = 5;
            MainForm_Button_SendAddRemove.Text = "Add/Remove";
            MainForm_Button_SendAddRemove.UseVisualStyleBackColor = true;
            MainForm_Button_SendAddRemove.Click += MainForm_Button_SendAddRemove_Click_1;
            // 
            // MainForm_Button_SendClosed
            // 
            MainForm_Button_SendClosed.Location = new Point(332, 3);
            MainForm_Button_SendClosed.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_SendClosed.Name = "MainForm_Button_SendClosed";
            MainForm_Button_SendClosed.Size = new Size(138, 29);
            MainForm_Button_SendClosed.TabIndex = 4;
            MainForm_Button_SendClosed.Text = "Work Order Closed";
            MainForm_Button_SendClosed.UseVisualStyleBackColor = true;
            MainForm_Button_SendClosed.Click += MainForm_Button_SendClosed_Click;
            // 
            // MainForm_Button_SentOR
            // 
            MainForm_Button_SentOR.Location = new Point(4, 3);
            MainForm_Button_SentOR.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_SentOR.Name = "MainForm_Button_SentOR";
            MainForm_Button_SentOR.Size = new Size(138, 29);
            MainForm_Button_SentOR.TabIndex = 3;
            MainForm_Button_SentOR.Text = "Over Receipt";
            MainForm_Button_SentOR.UseVisualStyleBackColor = true;
            MainForm_Button_SentOR.Click += MainForm_Button_SentOR_Click;
            // 
            // MainForm_Button_Skip
            // 
            MainForm_Button_Skip.Enabled = false;
            MainForm_Button_Skip.Location = new Point(80, 38);
            MainForm_Button_Skip.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_Skip.Name = "MainForm_Button_Skip";
            MainForm_Button_Skip.Size = new Size(62, 29);
            MainForm_Button_Skip.TabIndex = 2;
            MainForm_Button_Skip.Text = "Skip";
            MainForm_Button_Skip.UseVisualStyleBackColor = true;
            MainForm_Button_Skip.Click += MainForm_Button_Skip_Click;
            // 
            // MainForm_Button_Next
            // 
            MainForm_Button_Next.Location = new Point(328, 38);
            MainForm_Button_Next.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_Next.Name = "MainForm_Button_Next";
            MainForm_Button_Next.Size = new Size(142, 29);
            MainForm_Button_Next.TabIndex = 1;
            MainForm_Button_Next.Text = "Start";
            MainForm_Button_Next.UseVisualStyleBackColor = true;
            MainForm_Button_Next.Click += MainForm_Button_Next_Click;
            // 
            // MainForm_Button_Exit
            // 
            MainForm_Button_Exit.DialogResult = DialogResult.Cancel;
            MainForm_Button_Exit.Location = new Point(3, 38);
            MainForm_Button_Exit.Margin = new Padding(4, 3, 4, 3);
            MainForm_Button_Exit.Name = "MainForm_Button_Exit";
            MainForm_Button_Exit.Size = new Size(62, 29);
            MainForm_Button_Exit.TabIndex = 0;
            MainForm_Button_Exit.Text = "Exit";
            MainForm_Button_Exit.UseVisualStyleBackColor = true;
            MainForm_Button_Exit.Click += MainForm_Button_Exit_Click;
            // 
            // MainForm_StatusStrip
            // 
            MainForm_StatusStrip.Items.AddRange(new ToolStripItem[] { MainForm_StatusText_Loading });
            MainForm_StatusStrip.Location = new Point(0, 249);
            MainForm_StatusStrip.Name = "MainForm_StatusStrip";
            MainForm_StatusStrip.Size = new Size(492, 22);
            MainForm_StatusStrip.TabIndex = 3;
            MainForm_StatusStrip.Text = "statusStrip1";
            // 
            // MainForm_StatusText_Loading
            // 
            MainForm_StatusText_Loading.ForeColor = Color.Red;
            MainForm_StatusText_Loading.Name = "MainForm_StatusText_Loading";
            MainForm_StatusText_Loading.Size = new Size(86, 17);
            MainForm_StatusText_Loading.Text = "Loading Data...";
            MainForm_StatusText_Loading.Visible = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = MainForm_Button_Exit;
            ClientSize = new Size(492, 271);
            Controls.Add(MainForm_StatusStrip);
            Controls.Add(MainForm_Panel_Buttons);
            Controls.Add(MainForm_GroupBox_CurrentItem);
            Controls.Add(MainForm_MenuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = MainForm_MenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new Size(508, 310);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Infor Visual Easy Inventory";
            Load += LoadLoginScreen;
            MainForm_MenuStrip.ResumeLayout(false);
            MainForm_MenuStrip.PerformLayout();
            MainForm_GroupBox_CurrentItem.ResumeLayout(false);
            MainForm_GroupBox_CurrentItem.PerformLayout();
            MainForm_Panel_Buttons.ResumeLayout(false);
            MainForm_Panel_Buttons.PerformLayout();
            MainForm_StatusStrip.ResumeLayout(false);
            MainForm_StatusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private MenuStrip MainForm_MenuStrip;
        private ToolStripMenuItem MainForm_MenuStrip_File;
        private ToolStripMenuItem MainForm_MenuStrip_File_Settings;
        private GroupBox MainForm_GroupBox_CurrentItem;
        private Panel MainForm_Panel_Buttons;
        private Button MainForm_Button_Exit;
        private Button MainForm_Button_Skip;
        private Label MainForm_Label_Transaction;
        private Label MainForm_Label_To;
        private Label MainForm_Label_From;
        private Label MainForm_Label_Quantity;
        private Label MainForm_Label_PartID;
        private TextBox MainForm_TextBox_Transaction;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Label MainForm_Label_Grouped;
        private StatusStrip MainForm_StatusStrip;
        internal TextBox MainForm_TextBox_From;
        internal TextBox MainForm_TextBox_Quantity;
        internal TextBox MainForm_TextBox_PartID;
        internal TextBox MainForm_TextBox_To;
        public ToolStripStatusLabel MainForm_StatusText_Loading;
        public Button MainForm_Button_Next;
        public TextBox MainForm_TextBox_WorkOrder;
        private Button MainForm_Button_SendClosed;
        private Button MainForm_Button_SentOR;
        private Button MainForm_Button_SendAddRemove;
        public TextBox MainForm_TextBox_TagQty;
        public TextBox MainForm_TextBox_Notes;
        private ToolStripMenuItem setupEmailToolStripMenuItem;
    }
}
