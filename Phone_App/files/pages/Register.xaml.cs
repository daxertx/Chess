namespace Phone_App.files.pages;

// Phone app registration page: creates a new user account and opens the games page.
public partial class Register : ContentPage
{
    private const string LocalUrl    = "http://localhost:8733/ChessService/";
    private const string EmulatorUrl = "http://10.0.2.2:8733/ChessService/";

    public Register()
    {
        InitializeComponent();
    }

    // Creates a WCF client pointing to the given server URL.
    private static ChessServer.ChessServiceClient CreateClient(string url)
    {
        return new ChessServer.ChessServiceClient(
            ChessServer.ChessServiceClient.EndpointConfiguration.BasicHttpBinding_IChessService,
            url);
    }

    // Sends registration request to the server and navigates to the games page on success.
    private async void try_Register(object sender, EventArgs e)
    {
        string name = username.Text;
        string pass = password.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            return;

        name = name.Trim();

        try
        {
            bool isRegisterSuccess;
            try
            {
                ChessServer.ChessServiceClient client = CreateClient(LocalUrl);
                isRegisterSuccess = await client.RegisterAsync(name, pass);
            }
            catch
            {
                // Fallback for Android emulator (10.0.2.2 points to the host PC).
                ChessServer.ChessServiceClient client = CreateClient(EmulatorUrl);
                isRegisterSuccess = await client.RegisterAsync(name, pass);
            }

            if (!isRegisterSuccess)
            {
                await DisplayAlert("שגיאה", "קרתה תקלה הרשמה נכשלה", "אישור");
                return;
            }

            Dictionary<string, object> info_move = new Dictionary<string, object>
            {
                { "Username", name },
                { "Password", pass }
            };
            await Shell.Current.GoToAsync("ShowGamesPage", info_move);
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", $"שגיאת תקשורת: {ex.Message}", "אישור");
        }
    }
}
