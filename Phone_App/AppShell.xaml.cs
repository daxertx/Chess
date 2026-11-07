namespace Phone_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("ShowGamesPage", typeof(Phone_App.files.pages.Show_Games));
            Routing.RegisterRoute("ShowRegisterPage", typeof(Phone_App.files.pages.Register));
        }
    }
}
