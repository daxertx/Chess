namespace Model.classes
{
    public class User
    {
        private Settings settings;
        private string username;
        private string password;
        private bool admin;

        public User(string username, string password, bool admin, Settings settings)
        {
            this.username = username;
            this.password = password;
            this.admin = admin;
            this.settings = settings;
        }

        public string get_Username()
        {
            return this.username;
        }

        public string get_Password()
        {
            return this.password;
        }

        public bool get_Admin()
        {
            return this.admin;
        }

        public Settings get_Settings()
        {
            return this.settings;
        }
    }
}
