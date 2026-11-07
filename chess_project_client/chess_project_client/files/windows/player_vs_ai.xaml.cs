using chess.windows;
using chess_project_client.ChessServer;
using chess_project_client.files.classes;
using chess_project_client.files.user_control;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace chess_project_client.files.windows
{
    // Window for a player-vs-AI chess game against Stockfish.
    public partial class player_vs_ai : Window
    {
        private Game gameInstance;
        private Cube cube_clicked;
        private Cube[,] cubes = new Cube[8, 8];

        private bool board_drawn;
        private bool board_game_over = false;
        private string base_title = "Chess Game";

        private static readonly SolidColorBrush picked_brush  = MakeBrush("#F6F669");
        private static readonly SolidColorBrush move_brush    = MakeBrush("#6FA8DC");
        private static readonly SolidColorBrush refused_brush = MakeBrush("#D96459");

        // Creates a frozen brush from a hex color string.
        private static SolidColorBrush MakeBrush(string hex)
        {
            SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        // Initializes the AI game window and starts polling the server.
        public player_vs_ai()
        {
            InitializeComponent();

            this.gameInstance = new Game("http://localhost:8733/ChessService", 1);

            this.gameInstance.OnGameStarted    += this.HandleGameStarted;
            this.gameInstance.OnMoveReceived   += this.HandleMoveReceived;
            this.gameInstance.OnStatusChanged  += this.HandleStatusChanged;
            this.gameInstance.OnConnectionLost += this.HandleConnectionLost;

            this.Closing += this.OnWindowClosing;

            this.gameInstance.Start();
        }

        // Notifies the server if the player closes the window before the game ends.
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.gameInstance != null)
                this.gameInstance.Leave();
        }

        private void HandleGameStarted(int game_id, bool am_i_white)
        {
            this.Dispatcher.BeginInvoke(new Action<int, bool>(this.GameStarted), game_id, am_i_white);
        }

        private void HandleMoveReceived(Game_move move)
        {
            this.Dispatcher.BeginInvoke(new Action<Game_move>(this.Apply_Move), move);
        }

        private void HandleStatusChanged(int status)
        {
            this.Dispatcher.BeginInvoke(new Action<int>(this.ApplyStatus), status);
        }

        private void HandleConnectionLost()
        {
            this.Dispatcher.BeginInvoke(new Action(this.ConnectionLost));
        }

        // Draws the 8x8 board when the AI game starts.
        public void GameStarted(int game_id, bool am_i_white)
        {
            if (this.board_drawn)
                return;

            try
            {
                this.base_title = "Chess — Black";
                if (am_i_white)
                    this.base_title = "Chess — White";
                this.Title = this.base_title;

                if (this.StatusText != null)
                    this.StatusText.Visibility = Visibility.Collapsed;
                this.boardBox.Visibility = Visibility.Visible;

                BoardLabels.Fill(this.rankLabels, this.fileLabels, am_i_white);

                Cube[,] newCubes = new Board().NewBoard();

                for (int file = 0; file < 8; file++)
                {
                    for (int rank = 0; rank < 8; rank++)
                    {
                        Cube cube = newCubes[file, rank];
                        this.cubes[file, rank] = cube;

                        int column;
                        int row;
                        if (am_i_white)
                        {
                            column = file;
                            row    = 7 - rank;
                        }
                        else
                        {
                            column = 7 - file;
                            row    = rank;
                        }

                        this.board.Children.Add(cube);
                        Grid.SetColumn(cube, column);
                        Grid.SetRow(cube, row);

                        cube.BorderThickness = new Thickness(4);
                        cube.MouseDown += this.Cube_MouseDown;
                    }
                }

                this.board_drawn = true;
            }
            catch (Exception ex)
            {
                this.board_drawn = false;
                MessageBox.Show("Failed to draw the board:\n" + ex.Message, "Board Error");
            }
        }

        // Updates the UI board with a move received from the server.
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

            ClearSelection();
        }

        // Moves the rook on the UI board when a king castles.
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

        // Clears the captured pawn's square during an en-passant capture.
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

        // Replaces a pawn with the promoted piece on the last rank.
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

        // Handles clicking a square: first click selects a piece, second click sends the move.
        public void Cube_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.board_game_over)
                return;
            if (this.gameInstance == null)
                return;

            Cube cube = (Cube)sender;
            ClearHighlights();

            if (!this.gameInstance.IsMyTurn())
                return;

            if (this.cube_clicked == null)
            {
                SelectPiece(cube);
                return;
            }

            if (this.cube_clicked == cube)
            {
                this.cube_clicked = null;
                return;
            }

            SendMove(cube);
        }

        // Highlights the selected piece and all of its legal destination squares.
        private void SelectPiece(Cube cube)
        {
            if (cube.Gettype() == Chess_piece_type.Empty)
                return;

            Team_color myColor = Team_color.Black;
            if (this.gameInstance.AmIWhite())
                myColor = Team_color.White;

            if (cube.GetTeam() != myColor)
                return;

            List<Game_move> moves = this.gameInstance.get_moves(cube.GetY(), cube.GetX());
            for (int i = 0; i < moves.Count; i++)
            {
                Game_move move = moves[i];
                int file = move.toY;
                int rank = move.toX;
                if (!OnBoard(file, rank))
                    continue;
                if (this.cubes[file, rank] == null)
                    continue;

                this.cubes[file, rank].BorderBrush = move_brush;
            }

            cube.BorderBrush  = picked_brush;
            this.cube_clicked = cube;
        }

        // Sends the chosen move to the server (asking for promotion piece if needed).
        private void SendMove(Cube target)
        {
            Game_move move = this.gameInstance.GetMove(
                this.cube_clicked.GetY(), this.cube_clicked.GetX(),
                target.GetY(), target.GetX(),
                this.cube_clicked.Gettype(), this.cube_clicked.GetTeam());

            if (IsPromotion(this.cube_clicked, target))
            {
                PromotionDialog dialog = new PromotionDialog(this.cube_clicked.GetTeam());
                dialog.Owner = this;
                dialog.ShowDialog();
                move.pieceType = dialog.ChosenPiece;
            }

            bool accepted = this.gameInstance.TryExecuteMove(move);

            if (!accepted)
                target.BorderBrush = refused_brush;

            this.cube_clicked = null;
        }

        // Checks if moving a pawn to 'target' reaches its promotion rank.
        private static bool IsPromotion(Cube from, Cube target)
        {
            if (from.Gettype() != Chess_piece_type.Pawn)
                return false;

            int promotionRank = 0;
            if (from.GetTeam() == Team_color.White)
                promotionRank = 7;

            return target.GetY() == promotionRank;
        }

        // Updates the window title for check and shows the end-of-game message on checkmate/draw.
        private void ApplyStatus(int status)
        {
            bool amWhite = this.gameInstance != null && this.gameInstance.AmIWhite();

            if (status == 0)
            {
                this.Title = this.base_title;
                return;
            }

            if (status == 1 || status == 2)
            {
                bool myKingInCheck = (status == 1 && amWhite) || (status == 2 && !amWhite);

                if (myKingInCheck)
                    this.Title = this.base_title + "  ⚠ YOU ARE IN CHECK!";
                else
                    this.Title = this.base_title + "  — opponent is in check";
                return;
            }

            this.board_game_over = true;

            string message;
            if (status == 5)
            {
                message = "Stalemate — it's a draw!";
            }
            else
            {
                bool whiteWins = (status == 4);
                bool iWon      = (amWhite == whiteWins);

                if (iWon)
                    message = "Checkmate — you win!";
                else
                    message = "Checkmate — you lose.";
            }

            this.Title = this.base_title + "  — " + message;
            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            menu back = new menu();
            back.Show();
            this.Close();
        }

        // Alerts the user if the connection to the server is lost.
        private void ConnectionLost()
        {
            if (this.board_game_over)
                return;
            this.board_game_over = true;

            this.Title = this.base_title + "  — connection lost";
            if (this.StatusText != null)
            {
                this.StatusText.Text       = "Lost connection to the server.";
                this.StatusText.Visibility = Visibility.Visible;
            }

            MessageBox.Show(
                "Lost connection to the server.\n\nThe server may have been closed, or the tunnel URL may have changed.",
                "Connection Lost", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static bool OnBoard(int file, int rank)
        {
            return file >= 0 && file <= 7 && rank >= 0 && rank <= 7;
        }

        // Removes highlight borders from all squares on the board.
        private void ClearHighlights()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (this.cubes[x, y] != null)
                        this.cubes[x, y].BorderBrush = null;
                }
            }
        }

        private void ClearSelection()
        {
            this.cube_clicked = null;
            ClearHighlights();
        }
    }
}
