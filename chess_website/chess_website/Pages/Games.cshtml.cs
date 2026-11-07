using chess_website.ChessServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chess_website.Pages
{
    // Razor Page model that loads and displays the logged-in player's game history.
    public class GamesModel : PageModel
    {
        public List<Game_Data> Games { get; set; } = new List<Game_Data>();

        public bool ServerDown { get; set; } = false;

        public string Username { get; set; } = "";

        // Fetches the player's games from the WCF server using the session credentials.
        public async Task<IActionResult> OnGetAsync()
        {
            string user = HttpContext.Session.GetString("Username");
            string pass = HttpContext.Session.GetString("Password");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                return RedirectToPage("/Index");

            this.Username = user;

            try
            {
                ChessServer.ChessServiceClient client = new ChessServer.ChessServiceClient();
                Game_Data[] temp = await client.Login_WebsiteAsync(user, pass);

                this.Games = new List<Game_Data>();
                if (temp != null)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        this.Games.Add(temp[i]);
                    }
                }
            }
            catch
            {
                ServerDown = true;
                return Page();
            }

            return Page();
        }
    }
}
