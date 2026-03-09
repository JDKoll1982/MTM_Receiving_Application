namespace Visual_Inventory_Assistant.Classes;

using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

internal class SearchDao
{
    #region Methods

    public static string GetConnectionString(string? server, string database, string uid, string? password)
    {
        try
        {
            if (string.IsNullOrEmpty(server)) server = "localhost";
            //server = "localhost";
            if (string.IsNullOrEmpty(database)) database = "easy_inventory";

            if (string.IsNullOrEmpty(uid)) uid = "EasyInventory";

            if (string.IsNullOrEmpty(password)) password = "";

            var connectionString =
                $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};Allow User Variables=True";
            return connectionString;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($@"A MySQLError Occurred: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return "";
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"A System Error Occured: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return "";
        }
    }

    public void UserLogin(string userName, string password, Form loginForm)
    {
        try
        {
            var connectionString = GetConnectionString(null, "easy_inventory", "EasyInventory", null);
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            var query = "SELECT * FROM main WHERE username = @username AND password = @password";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", userName);
            command.Parameters.AddWithValue("@password", password);
            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ApplicationVariables.ApplicationUserName = reader["username"].ToString();
                    ApplicationVariables.ApplicationPassword = reader["password"].ToString();
                    ApplicationVariables.VisualPassword = reader["VisualPassword"].ToString();
                    ApplicationVariables.VisualUserName = reader["VisualUserName"].ToString();
                }

                // Close the login form and open the main form
                loginForm.Invoke(new Action(() => { loginForm.Hide(); }));
            }
            else
            {
                MessageBox.Show(@"Invalid username or password", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($@"A MySQLError Occurred: {ex.Message}", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"A System Error Occured: {ex.Message}", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void AddNewUser(string userName, string password, Form newUserForm)
    {
        try
        {
            var connectionString = GetConnectionString(null, "easy_inventory", "EasyInventory", null);
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            // Check if the user already exists
            var checkQuery = "SELECT COUNT(*) FROM main WHERE username = @username";
            using (var checkCommand = new MySqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@username", userName);
                var userExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (userExists)
                {
                    MessageBox.Show(@"User already exists", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Insert new user
            var insertQuery =
                "INSERT INTO main (username, password, VisualPassword, VisualUserName) VALUES (@username, @password, @visualPassword, @visualUserName)";
            using var insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@username", userName);
            insertCommand.Parameters.AddWithValue("@password", password);
            insertCommand.Parameters.AddWithValue("@visualPassword", userName);
            insertCommand.Parameters.AddWithValue("@visualUserName", password);

            var result = insertCommand.ExecuteNonQuery();
            if (result > 0)
            {
                newUserForm.Invoke(new Action(() => { newUserForm.Hide(); }));
                MessageBox.Show(@"User added successfully", @"Success", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(@"Failed to add user", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($@"A MySQLError Occurred: {ex.Message}", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"A System Error Occured: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void UpdateUserVisualCredentials(string? userName, string newVisualUserName, string newVisualPassword)
    {
        try
        {
            var connectionString = GetConnectionString(null, "easy_inventory", "EasyInventory", null);
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            // Check if the user exists
            var checkQuery = "SELECT COUNT(*) FROM main WHERE username = @username";
            using (var checkCommand = new MySqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@username", userName);
                var userExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (!userExists)
                {
                    MessageBox.Show(@"User does not exist", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Update VisualUserName and VisualPassword
            var updateQuery =
                "UPDATE main SET VisualUserName = @newVisualUserName, VisualPassword = @newVisualPassword WHERE username = @username";
            using var updateCommand = new MySqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@username", userName);
            updateCommand.Parameters.AddWithValue("@newVisualUserName", newVisualUserName);
            updateCommand.Parameters.AddWithValue("@newVisualPassword", newVisualPassword);

            var result = updateCommand.ExecuteNonQuery();
            if (result > 0)
                MessageBox.Show(@"User credentials updated successfully", @"Success", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            else
                MessageBox.Show(@"Failed to update user credentials", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (MySqlException ex)
        {
            MessageBox.Show($@"A MySQLError Occurred: {ex.Message}", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"A System Error Occured: {ex.Message}", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion
}