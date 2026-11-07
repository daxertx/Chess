using chess_website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace chess_website.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";


        public bool Registered { get; set; } = false;

        public void OnGet()
        {
            Registered = false;
        }

        public async Task OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
            {
                ViewData["ErrorMessage"] = "יש למלא שם משתמש וסיסמה.";
                return;
            }


            bool created;
            try
            {
                ChessServer.ChessServiceClient client = new ChessServer.ChessServiceClient();
                created = await client.RegisterAsync(this.Username.Trim(), this.Password);
            }
            catch
            {
                ViewData["ErrorMessage"] = "לא ניתן להתחבר לשרת. ודא שהשרת פועל ונסה שוב.";
                return;
            }

            if (!created)
            {
                ViewData["ErrorMessage"] = "שם המשתמש כבר תפוס.";
                return;
            }

            Registered = true;
        }
    }
}
