using chess_website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chess_website.Pages
{
    public class IndexModel : PageModel
    {

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public bool IsLoggedIn { get; set; } = false;

       
        public void OnGet()
        {
            this.IsLoggedIn = false;
        }

        public async Task OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Username) || string.IsNullOrEmpty(this.Password))
            {
                IsLoggedIn = false;
                ViewData["ErrorMessage"] = "יש למלא שם משתמש וסיסמה.";
                return;
            }

            bool ok;
            try
            {
                ChessServer.ChessServiceClient client = new ChessServer.ChessServiceClient();
                ok = await client.LoginAsync(Username.Trim(), Password);
            }
            catch
            {
                IsLoggedIn = false;
                ViewData["ErrorMessage"] = "לא ניתן להתחבר לשרת. ודא שהשרת פועל ונסה שוב.";
                return;
            }

            if (ok)
            {
                IsLoggedIn = true;
                HttpContext.Session.SetString("Username", Username.Trim());
                HttpContext.Session.SetString("Password", Password);
            }
            else
            {
                IsLoggedIn = false;
                ViewData["ErrorMessage"] = "שם משתמש או סיסמה שגויים.";
            }
        }
    }
}
