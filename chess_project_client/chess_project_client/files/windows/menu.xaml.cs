using chess_project_client.files.windows;
using System.Windows;

namespace chess.windows
{
    // Main menu window where the player chooses PvP, vs AI, Replay, or Admin.
    public partial class menu : Window
    {
        public menu()
        {
            InitializeComponent();
        }

        // Starts an online player-vs-player game.
        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            game game = new game();
            game.Show();
            this.Close();
        }

        // Starts a game against the Stockfish AI.
        private void PlayAI_Click(object sender, RoutedEventArgs e)
        {
            player_vs_ai game = new player_vs_ai();
            game.Show();
            this.Close();
        }

        // Opens the dialog to pick and watch a saved game replay.
        private void PlayReplay_Click(object sender, RoutedEventArgs e)
        {
            ReplayPickDialog pick = new ReplayPickDialog();
            pick.Owner = this;
            if (pick.ShowDialog() != true)
                return;

            Replay game = new Replay(pick.GameId);
            game.Show();
            this.Close();
        }

        // Opens the admin panel window if the user is an admin.
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            admin adminWindow = new admin();
            if (adminWindow.IsLoaded || adminWindow.IsVisible)
            {
                adminWindow.Show();
            }
            else
            {
                try
                {
                    adminWindow.Show();
                }
                catch
                {
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
