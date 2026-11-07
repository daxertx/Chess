using chess.windows;
using chess_project_client.files.classes;
using System.Windows;
using System.Windows.Input;

namespace chess_project_client.files.windows
{
    // Login and registration window shown when the client starts.
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            this.UsernameBox.Focus();
        }

        // Validates credentials with the server and opens the main menu on success.
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = this.UsernameBox.Text.Trim();
            string password = this.PasswordBox.Password;

            if (!this.FieldsFilled(username, password))
                return;

            bool ok;
            try
            {
                ok = Game.Login(Session.ServerUrl, username, password);
            }
            catch
            {
                this.MessageText.Text = "Could not reach the server. Start it and try again.";
                return;
            }

            if (!ok)
            {
                this.MessageText.Text = "Wrong username or password.";
                return;
            }

            this.EnterGame(username, password);
        }

        // Registers a new user account on the server and opens the main menu on success.
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = this.UsernameBox.Text.Trim();
            string password = this.PasswordBox.Password;

            if (!this.FieldsFilled(username, password))
                return;

            bool created;
            try
            {
                created = Game.Register(Session.ServerUrl, username, password);
            }
            catch
            {
                this.MessageText.Text = "Could not reach the server. Start it and try again.";
                return;
            }

            if (!created)
            {
                this.MessageText.Text = "That username is already taken.";
                return;
            }

            this.EnterGame(username, password);
        }

        // Submits login when pressing Enter in a text field.
        private void Field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Login_Click(sender, e);
        }

        // Checks that both username and password are not empty.
        private bool FieldsFilled(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                this.MessageText.Text = "Type a username and a password.";
                return false;
            }
            return true;
        }

        // Saves the logged-in username and password in Session and opens the main menu.
        private void EnterGame(string username, string password)
        {
            Session.Username = username;
            Session.Password = password;

            menu main = new menu();
            main.Show();
            this.Close();
        }
    }
}
