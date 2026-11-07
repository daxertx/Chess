
using Model.classes;
using System;
using System.Collections.Generic;
using System.Data;

namespace ViewModel.classes
{
    public class UserDB : BaseDB
    {
        private SettingsDB settingsDB = new SettingsDB();

        public UserDB() : base()
        {
        }

        public bool CreateUser(string username, string password, bool isadmin = false)
        {
            if (UserExists(username))
                return false;

            Settings userSettings = this.settingsDB.CreateDefaultSettings();

            return base.ExecuteNonQuery(
                "INSERT INTO users (username, [password], settings, [admin]) VALUES (?, ?, ?, ?)",
                username, password, userSettings.Id, isadmin) >= 1;
        }

        public bool UserExists(string username)
        {
            object result = base.GetScalar("SELECT COUNT(*) FROM users WHERE username = ?", username);
            if (result == null)
                return false;
            return Convert.ToInt32(result) > 0;
        }

        public bool Correct_login(string username, string password)
        {
            object result = base.GetScalar("SELECT COUNT(*) FROM users WHERE username = ? AND [password] = ?", username, password);
            if (result == null)
                return false;
            return Convert.ToInt32(result) == 1;
        }

        public bool IsAdmin(string username, string password)
        {
            object result = base.GetScalar("SELECT COUNT(*) FROM users WHERE username = ? AND [password] = ? AND [admin] = True", username, password);
            if (result == null)
                return false;
            return Convert.ToInt32(result) == 1;
        }

        public User GetUser(string username)
        {
            DataTable dt = base.GetTable("SELECT username, [password], settings, [admin] FROM users WHERE username = ?", username);
            if (dt == null || dt.Rows.Count == 0)
            {
                Settings fallbackSettings = new Settings(0, ScreenSize.Medium, "default", "default");
                return new User(username, "", false, fallbackSettings);
            }

            DataRow row = dt.Rows[0];
            string pass = row["password"].ToString();
            int settingsId = ReadInt(row, "settings");
            bool isAdmin = false;
            if (row["admin"] != DBNull.Value)
                isAdmin = Convert.ToBoolean(row["admin"]);

            Settings s = this.settingsDB.GetSettingsById(settingsId);
            return new User(username, pass, isAdmin, s);
        }

        public List<User> GetAllUsers()
        {
            List<User> list = new List<User>();
            DataTable dt = base.GetTable("SELECT username, [password], settings, [admin] FROM users ORDER BY username");
            if (dt == null)
                return list;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                string user = row["username"].ToString();
                string pass = row["password"].ToString();
                int settingsId = ReadInt(row, "settings");
                bool isAdmin = false;
                if (row["admin"] != DBNull.Value)
                    isAdmin = Convert.ToBoolean(row["admin"]);

                Settings s = this.settingsDB.GetSettingsById(settingsId);
                list.Add(new User(user, pass, isAdmin, s));
            }
            return list;
        }

        public string[] GetUsersList()
        {
            List<User> all = GetAllUsers();
            string[] users = new string[all.Count];
            for (int i = 0; i < all.Count; i++)
            {
                User u = all[i];
                users[i] = (i + 1) + "|" + u.get_Username() + "|" + u.get_Password() + "|" + u.get_Admin();
            }
            return users;
        }
    }
}
