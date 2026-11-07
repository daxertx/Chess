namespace chess_project_client.files.classes
{
    public static class Session
    {
        public const string ServerUrl = "http://localhost:8733/ChessService";

        public static string Username = "";
        public static string Password = "";

        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}
