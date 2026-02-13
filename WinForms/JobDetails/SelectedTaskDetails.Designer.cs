namespace MTM_Waitlist_Application_2._0.WinForms.JobDetails
{
    partial class SelectedTaskDetails
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
            DetailsForm_WorkStation = new Label();
            SuspendLayout();
            // 
            // DetailsForm_WorkStation
            // 
            DetailsForm_WorkStation.AutoSize = true;
            DetailsForm_WorkStation.Location = new Point(46, 36);
            DetailsForm_WorkStation.Name = "DetailsForm_WorkStation";
            DetailsForm_WorkStation.Size = new Size(38, 15);
            DetailsForm_WorkStation.TabIndex = 0;
            DetailsForm_WorkStation.Text = "label1";
            // 
            // DetailsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(DetailsForm_WorkStation);
            Name = "SelectedTaskDetails";
            Text = "DetailsForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label DetailsForm_WorkStation;
    }
}