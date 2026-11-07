namespace Phone_App.files.pages;

// Phone app login page: authenticates the user or navigates to registration.
public partial class Login : ContentPage
{
    private const string LocalUrl    = "http://localhost:8733/ChessService/";
    private const string EmulatorUrl = "http://10.0.2.2:8733/ChessService/";

    public Login()
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

    // Validates credentials with the server and navigates to the games page.
    private async void try_Login(object sender, EventArgs e)
    {
        string name = username.Text;
        string pass = password.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            return;

        name = name.Trim();

        try
        {
            bool isLoginSuccess;
            try
            {
                ChessServer.ChessServiceClient client = CreateClient(LocalUrl);
                isLoginSuccess = await client.LoginAsync(name, pass);
            }
            catch
            {
                // Fallback for Android emulator (10.0.2.2 points to the host PC).
                ChessServer.ChessServiceClient client = CreateClient(EmulatorUrl);
                isLoginSuccess = await client.LoginAsync(name, pass);
            }

            if (!isLoginSuccess)
            {
                await DisplayAlert("שגיאה", "שם משתמש או סיסמה שגוי", "אישור");
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

    // Navigates to the registration page.
    private async void Register(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("ShowRegisterPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", $"שגיאה: {ex.Message}", "אישור");
        }
    }
}
