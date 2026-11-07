using System.Collections.ObjectModel;

namespace Phone_App.files.pages;

[QueryProperty(nameof(Username), "Username")]
[QueryProperty(nameof(Password), "Password")]
// Page that displays the logged-in player's finished games on the phone app.
public partial class Show_Games : ContentPage
{
    private const string LocalUrl    = "http://localhost:8733/ChessService/";
    private const string EmulatorUrl = "http://10.0.2.2:8733/ChessService/";

    public string Username { get; set; }
    public string Password { get; set; }

    public ObservableCollection<ChessServer.Game_Data> Games { get; set; }

    public Show_Games()
    {
        InitializeComponent();

        this.Games = new ObservableCollection<ChessServer.Game_Data>();
        this.BindingContext = this;
    }

    // Creates a WCF client pointing to the given server URL.
    private static ChessServer.ChessServiceClient CreateClient(string url)
    {
        return new ChessServer.ChessServiceClient(
            ChessServer.ChessServiceClient.EndpointConfiguration.BasicHttpBinding_IChessService,
            url);
    }

    // Automatically loads the player's games when the page appears.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.load();
    }

    // Requests the player's game list from the server and adds each item using a for loop.
    protected async void load()
    {
        if (string.IsNullOrEmpty(this.Username) || string.IsNullOrEmpty(this.Password))
            return;

        this.Games.Clear();

        try
        {
            ChessServer.Game_Data[] game_array;
            try
            {
                ChessServer.ChessServiceClient client = CreateClient(LocalUrl);
                game_array = await client.Login_PhoneAsync(this.Username, this.Password);
            }
            catch
            {
                // Fallback for Android emulator (10.0.2.2 points to the host PC).
                ChessServer.ChessServiceClient client = CreateClient(EmulatorUrl);
                game_array = await client.Login_PhoneAsync(this.Username, this.Password);
            }

            if (game_array != null)
            {
                for (int i = 0; i < game_array.Length; i++)
                {
                    this.add_game(game_array[i]);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", $"שגיאה בטעינת משחקים: {ex.Message}", "אישור");
        }
    }

    // Adds a single game record to the ObservableCollection.
    protected void add_game(ChessServer.Game_Data game)
    {
        this.Games.Add(game);
    }
}
