using chess_project_client.ChessServer;
using chess_project_client.files.classes;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;

namespace chess_project_client.files.windows
{
    public partial class admin : Window
    {
        public class UserRow
        {
            public string Id          { get; set; }
            public string Username    { get; set; }
            public string Password    { get; set; }
            public string IsAdminText { get; set; }
        }

        public admin()
        {
            InitializeComponent();

            ChessServiceClient client = CreateClient();
            bool allowed = false;
            try
            {
                allowed = client.IsAdmin(Session.Username, Session.Password);
            }
            catch
            {
                allowed = false;
            }

            if (!allowed)
            {
                MessageBox.Show("אין לך הרשאות מנהל!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadUsersTable();
        }

        private static ChessServiceClient CreateClient()
        {
            BasicHttpBinding binding  = new BasicHttpBinding(BasicHttpSecurityMode.None);
            EndpointAddress  endpoint = new EndpointAddress(Session.ServerUrl);
            return new ChessServiceClient(binding, endpoint);
        }

        private void LoadUsersTable()
        {
            try
            {
                ChessServiceClient client = CreateClient();
                string[] rawUsers = client.GetUsersList(Session.Username, Session.Password);

                List<UserRow> rows = new List<UserRow>();
                if (rawUsers != null)
                {
                    for (int i = 0; i < rawUsers.Length; i++)
                    {
                        string[] parts = rawUsers[i].Split('|');
                        if (parts.Length < 4)
                            continue;

                        UserRow row = new UserRow();
                        row.Id       = parts[0];
                        row.Username = parts[1];
                        row.Password = parts[2];

                        if (parts[3] == "True")
                            row.IsAdminText = "כן";
                        else
                            row.IsAdminText = "לא";

                        rows.Add(row);
                    }
                }

                this.UsersGrid.ItemsSource = rows;
            }
            catch
            {
            }
        }

        private void createuser(object sender, RoutedEventArgs e)
        {
            string user = username.Text.Trim();
            string pass = password.Text;

            bool makeAdmin = false;
            if (isAdmin.IsChecked == true)
                makeAdmin = true;

            ChessServiceClient client = CreateClient();

            bool result = false;
            try
            {
                result = client.AddUser(Session.Username, Session.Password, user, pass, makeAdmin);
            }
            catch
            {
                result = false;
            }

            if (!result)
            {
                MessageBox.Show("אחד מהקלטים נכשל", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("User created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsersTable();
            }
        }

        private void reset(object sender, RoutedEventArgs e)
        {
            username.Text = "";
            password.Text = "";
            isAdmin.IsChecked = false;
        }
    }
}
