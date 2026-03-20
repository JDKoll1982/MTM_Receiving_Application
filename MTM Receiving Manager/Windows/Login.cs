using Visual_Inventory_Assistant.Classes;

namespace Visual_Inventory_Assistant.Windows;

public partial class Login : Form
{
    #region Constructor

    public Login()
    {
        InitializeComponent();
        LoginForm_TextBox_UserName.Focus();
    }

    #endregion

    #region Event Handlers

    private void LoginButton_Click(object sender, EventArgs e)
    {
        try
        {
            var userName = LoginForm_TextBox_UserName.Text;
            var password = LoginForm_TextBox_Password.Text;
            var searchDao = new SearchDao();
            searchDao.UserLogin(userName, password, this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred during login: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void LoginForm_Button_NewUser_Click(object sender, EventArgs e)
    {
        try
        {
            var newUser = new NewUser();
            newUser.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while opening the new user form: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    #endregion

    #region Overrides

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.CloseReason == CloseReason.UserClosing)
            Application.Exit();
    }

    #endregion
}
