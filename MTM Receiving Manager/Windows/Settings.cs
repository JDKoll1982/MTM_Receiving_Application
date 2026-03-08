using Visual_Inventory_Assistant.Classes;

namespace Visual_Inventory_Assistant.Windows;

public partial class SettingsForm : Form
{
    #region Constructor

    public SettingsForm()
    {
        InitializeComponent();
        FillSettings();
    }

    #endregion

    #region Methods

    private void FillSettings()
    {
        try
        {
            Settings_CurrentUser.Text = ApplicationVariables.ApplicationUserName;
            SettingsForm_TextBox_VisualUserName.Text = ApplicationVariables.VisualUserName;
            SettingsForm_TextBox_VisualPassword.Text = ApplicationVariables.VisualPassword;
            SettingsForm_TextBox_SheetsLink.Text = ApplicationVariables.GoogleSheetsLink;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while filling settings: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void Visual_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (SettingsForm_TextBox_VisualUserName.Text.Length > 0 &&
                SettingsForm_TextBox_VisualPassword.Text.Length > 0 &&
                SettingsForm_TextBox_VisualUserName.Text != ApplicationVariables.VisualUserName &&
                SettingsForm_TextBox_VisualPassword.Text != ApplicationVariables.VisualPassword)
                SettingsForm_Button_SaveVisual.Enabled = true;
            else
                SettingsForm_Button_SaveVisual.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while handling text change: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Google_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (SettingsForm_TextBox_SheetsLink.Text.Length > 0 &&
                SettingsForm_TextBox_SheetsLink.Text != ApplicationVariables.GoogleSheetsLink)
                SettingsForm_Button_SaveGoogle.Enabled = true;
            else
                SettingsForm_Button_SaveGoogle.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while handling text change: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsForm_Button_ResetVisual_Click(object sender, EventArgs e)
    {
        try
        {
            SettingsForm_TextBox_VisualUserName.Text = ApplicationVariables.VisualUserName;
            SettingsForm_TextBox_VisualPassword.Text = ApplicationVariables.VisualPassword;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while resetting visual settings: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsForm_Button_Reset_Click(object sender, EventArgs e)
    {
        try
        {
            SettingsForm_TextBox_SheetsLink.Text = ApplicationVariables.GoogleSheetsLink;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while resetting Google Sheets link: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsForm_Button_SaveVisual_Click(object sender, EventArgs e)
    {
        try
        {
            var searchDao = new SearchDao();
            searchDao.UpdateUserVisualCredentials(ApplicationVariables.ApplicationUserName,
                SettingsForm_TextBox_VisualUserName.Text,
                SettingsForm_TextBox_VisualPassword.Text);
            ApplicationVariables.VisualUserName = SettingsForm_TextBox_VisualUserName.Text;
            ApplicationVariables.VisualPassword = SettingsForm_TextBox_VisualPassword.Text;
            SettingsForm_Button_SaveVisual.Enabled = false;
            FillSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while saving visual settings: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsForm_Button_SaveGoogle_Click(object sender, EventArgs e)
    {
        try
        {
            ApplicationVariables.GoogleSheetsLink = SettingsForm_TextBox_SheetsLink.Text;
            SettingsForm_Button_SaveGoogle.Enabled = false;
            FillSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while saving Google Sheets link: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    private void SettingsForm_Button_Exit_Click(object sender, EventArgs e)
    {
        Hide();
    }
}