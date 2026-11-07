using chess_project_client.files.classes;
using System.Collections.Generic;
using System.Windows;

namespace chess_project_client.files.windows
{
    // Dialog window that lets the user choose a saved game to watch in Replay mode.
    public partial class ReplayPickDialog : Window
    {
        // Represents one saved game row in the picker list.
        private class GameRow
        {
            public int Id;
            public string Display;

            public override string ToString()
            {
                return this.Display;
            }
        }

        public int GameId { get; private set; }

        public ReplayPickDialog()
        {
            InitializeComponent();
            LoadGames();
        }

        // Loads saved games from the server and populates the list for the logged-in user.
        private void LoadGames()
        {
            this.ErrorText.Text = "";
            this.GamesList.Items.Clear();

            string[] rows;
            try
            {
                Game connection = new Game("http://localhost:8733/ChessService", 2);
                rows = connection.GetGamesList();
            }
            catch
            {
                this.EmptyText.Visibility = Visibility.Collapsed;
                this.ErrorText.Text = "Could not reach the server. Start it and press Refresh.";
                return;
            }

            if (rows == null)
                rows = new string[0];

            List<GameRow> games = ParseRows(rows);
            for (int i = 0; i < games.Count; i++)
            {
                this.GamesList.Items.Add(games[i]);
            }

            bool empty = this.GamesList.Items.Count == 0;
            this.EmptyText.Visibility = Visibility.Collapsed;
            if (empty)
                this.EmptyText.Visibility = Visibility.Visible;

            if (!empty)
                this.GamesList.SelectedIndex = 0;
        }

        // Parses each row from the server and keeps only games played by the logged-in user.
        private static List<GameRow> ParseRows(string[] rows)
        {
            List<GameRow> games = new List<GameRow>();
            string currentUser = Session.Username;

            for (int i = 0; i < rows.Length; i++)
            {
                string raw = rows[i];
                string[] parts = raw.Split('|');
                if (parts.Length < 5)
                    continue;

                int id;
                if (!int.TryParse(parts[0], out id))
                    continue;

                string white = parts[1];
                string black = parts[2];
                string time  = parts[3];
                string moves = parts[4];

                // Filter so the player only sees games they participated in.
                if (!string.IsNullOrEmpty(currentUser))
                {
                    if (white != currentUser && black != currentUser)
                        continue;
                }

                GameRow game = new GameRow();
                game.Id = id;
                game.Display = "Game " + id + "   —   " + white + " vs " + black +
                               "   —   " + moves + " moves   —   " + time;
                games.Add(game);
            }

            return games;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadGames();
        }

        private void GamesList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TryAccept();
        }

        private void Watch_Click(object sender, RoutedEventArgs e)
        {
            TryAccept();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // Confirms the selected game and closes the dialog.
        private void TryAccept()
        {
            GameRow selected = this.GamesList.SelectedItem as GameRow;
            if (selected == null)
            {
                this.ErrorText.Text = "Pick a game from the list first.";
                return;
            }

            this.GameId = selected.Id;
            this.DialogResult = true;
        }
    }
}
