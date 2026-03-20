using Visual_Inventory_Assistant.Classes;

namespace Visual_Inventory_Assistant.Windows;

public partial class NewUser : Form
{
    #region Constructor

    public NewUser()
    {
        InitializeComponent();
    }

    #endregion

    #region Event Handlers

    private void NewUserTextChange(object sender, EventArgs e)
    {
        try
        {
            if (
                NewUserForm_TextBox_UserName.Text.Length > 0
                && NewUserForm_TextBox_Password.Text.Length > 0
            )
                NewUserForm_Button_Save.Enabled = true;
            else
                NewUserForm_Button_Save.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while handling text change: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void NewUserForm_Button_Save_Click(object sender, EventArgs e)
    {
        try
        {
            var searchDao = new SearchDao();
            searchDao.AddNewUser(
                NewUserForm_TextBox_UserName.Text,
                NewUserForm_TextBox_Password.Text,
                this
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while saving new user: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    #endregion
}
