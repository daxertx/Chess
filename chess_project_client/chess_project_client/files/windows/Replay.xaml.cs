using chess.windows;
using chess_project_client.ChessServer;
using chess_project_client.files.classes;
using chess_project_client.files.user_control;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace chess_project_client.files.windows
{
    // Window that replays a saved game move by move.
    public partial class Replay : Window
    {
        private Game gameInstance;
        private Cube[,] cubes = new Cube[8, 8];

        private bool board_drawn;
        private string base_title = "Replay";
        private int game_id;

        private int shown_moves = 0;
        private int total_moves = 0;

        private Game_move[] moves;

        // Loads all moves for the chosen game ID and draws the starting board.
        public Replay(int game_id)
        {
            InitializeComponent();

            this.game_id = game_id;
            this.gameInstance = new Game("http://localhost:8733/ChessService", 2);
            this.gameInstance.OnConnectionLost += this.HandleConnectionLost;

            LoadMoves();
            GameStarted();
            ShowStatus();
        }

        // Downloads all moves of the saved game from the server into an array.
        private void LoadMoves()
        {
            try
            {
                this.total_moves = this.gameInstance.ReplayMoveCount(this.game_id);
            }
            catch
            {
                this.total_moves = 0;
            }

            if (this.total_moves < 0)
                this.total_moves = 0;

            this.moves = new Game_move[this.total_moves];
            for (int i = 0; i < this.total_moves; i++)
            {
                this.moves[i] = this.gameInstance.ReplayMove(this.game_id, i);
            }
        }

        // Initializes the replay board display.
        private void GameStarted()
        {
            if (this.board_drawn)
                return;

            try
            {
                this.Title = this.base_title;

                if (this.StatusText != null)
                    this.StatusText.Visibility = Visibility.Collapsed;
                this.boardBox.Visibility = Visibility.Visible;

                DrawStartingBoard();
                this.board_drawn = true;
            }
            catch (Exception ex)
            {
                this.board_drawn = false;
                MessageBox.Show("Failed to draw the board:\n" + ex.Message, "Board Error");
            }
        }

        // Resets the visual board to the initial chess position.
        private void DrawStartingBoard()
        {
            this.board.Children.Clear();

            Cube[,] newCubes = new Board().NewBoard();

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Cube cube = newCubes[file, rank];
                    this.cubes[file, rank] = cube;

                    this.board.Children.Add(cube);
                    Grid.SetColumn(cube, file);
                    Grid.SetRow(cube, 7 - rank);

                    cube.BorderThickness = new Thickness(4);
                }
            }
        }

        // Redraws the board from the start up to the given move index.
        private void ShowPosition(int move_count)
        {
            if (move_count < 0)
                move_count = 0;
            if (move_count > this.total_moves)
                move_count = this.total_moves;

            DrawStartingBoard();

            for (int i = 0; i < move_count; i++)
            {
                Apply_Move(this.moves[i]);
            }

            this.shown_moves = move_count;
            ShowStatus();
        }

        // Updates the move counter label and navigation buttons.
        private void ShowStatus()
        {
            this.MoveText.Text = "move " + this.shown_moves + " / " + this.total_moves;
            this.Title = this.base_title + " — game " + this.game_id;

            this.FirstButton.IsEnabled = this.shown_moves > 0;
            this.BackButton.IsEnabled  = this.shown_moves > 0;
            this.NextButton.IsEnabled  = this.shown_moves < this.total_moves;
            this.LastButton.IsEnabled  = this.shown_moves < this.total_moves;
        }

        // Applies a single move onto the visual board.
        public void Apply_Move(Game_move move)
        {
            int fromFile = move.fromY;
            int fromRank = move.fromX;
            int toFile   = move.toY;
            int toRank   = move.toX;

            if (!OnBoard(fromFile, fromRank) || !OnBoard(toFile, toRank))
                return;
            if (this.cubes[fromFile, fromRank] == null)
                return;
            if (this.cubes[toFile, toRank] == null)
                return;

            Chess_piece_type piece = this.cubes[fromFile, fromRank].Gettype();
            Team_color       team  = this.cubes[fromFile, fromRank].GetTeam();

            Chess_piece_type destinationBefore = this.cubes[toFile, toRank].Gettype();

            this.cubes[toFile, toRank].Update(piece, team);
            this.cubes[fromFile, fromRank].Update(Chess_piece_type.Empty, Team_color.Empty);

            ApplyCastling(piece, fromFile, toFile, fromRank, team);
            ApplyEnPassant(piece, fromFile, toFile, fromRank, destinationBefore);
            ApplyPromotion(piece, toFile, toRank, team, move);
        }

        // Moves the rook when a castling move is replayed.
        private void ApplyCastling(Chess_piece_type piece, int fromFile, int toFile, int rank, Team_color team)
        {
            if (piece != Chess_piece_type.King)
                return;
            if (Math.Abs(toFile - fromFile) != 2)
                return;

            if (toFile > fromFile)
            {
                this.cubes[toFile - 1, rank].Update(Chess_piece_type.Rook, team);
                this.cubes[7, rank].Update(Chess_piece_type.Empty, Team_color.Empty);
            }
            else
            {
                this.cubes[toFile + 1, rank].Update(Chess_piece_type.Rook, team);
                this.cubes[0, rank].Update(Chess_piece_type.Empty, Team_color.Empty);
            }
        }

        // Removes the captured pawn when an en-passant move is replayed.
        private void ApplyEnPassant(Chess_piece_type piece, int fromFile, int toFile, int fromRank, Chess_piece_type destinationBefore)
        {
            if (piece != Chess_piece_type.Pawn)
                return;
            if (Math.Abs(toFile - fromFile) != 1)
                return;
            if (destinationBefore != Chess_piece_type.Empty)
                return;

            this.cubes[toFile, fromRank].Update(Chess_piece_type.Empty, Team_color.Empty);
        }

        // Promotes a pawn when it reaches the last rank in replay.
        private void ApplyPromotion(Chess_piece_type piece, int toFile, int toRank, Team_color team, Game_move move)
        {
            if (piece != Chess_piece_type.Pawn)
                return;
            if (toRank != 0 && toRank != 7)
                return;

            Chess_piece_type promoteTo = move.pieceType;

            if (promoteTo == Chess_piece_type.Pawn ||
                promoteTo == Chess_piece_type.King ||
                promoteTo == Chess_piece_type.Empty)
            {
                promoteTo = Chess_piece_type.Queen;
            }

            this.cubes[toFile, toRank].Update(promoteTo, team);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextMove();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            PreviousMove();
        }

        private void First_Click(object sender, RoutedEventArgs e)
        {
            ShowPosition(0);
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {
            ShowPosition(this.total_moves);
        }

        // Advances the replay by one move.
        private void NextMove()
        {
            if (this.shown_moves >= this.total_moves)
                return;

            Apply_Move(this.moves[this.shown_moves]);
            this.shown_moves++;
            ShowStatus();
        }

        // Steps back one move in the replay.
        private void PreviousMove()
        {
            if (this.shown_moves <= 0)
                return;

            ShowPosition(this.shown_moves - 1);
        }

        // Supports keyboard navigation (arrows, space, home, end, escape).
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Space)
                NextMove();
            else if (e.Key == Key.Left)
                PreviousMove();
            else if (e.Key == Key.Escape)
                Leave();
            else if (e.Key == Key.Home)
                ShowPosition(0);
            else if (e.Key == Key.End)
                ShowPosition(this.total_moves);
        }

        private void HandleConnectionLost()
        {
            this.Dispatcher.BeginInvoke(new Action(this.ConnectionLost));
        }

        private void ConnectionLost()
        {
            this.Title = this.base_title + "  — connection lost";
            MessageBox.Show(
                "Lost connection to the server.",
                "Connection Lost", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static bool OnBoard(int file, int rank)
        {
            return file >= 0 && file <= 7 && rank >= 0 && rank <= 7;
        }

        // Returns to the main menu window.
        private void Leave()
        {
            menu m = new menu();
            m.Show();
            this.Close();
        }
    }
}
