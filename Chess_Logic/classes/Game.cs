using System.Collections.Generic;
using System.Runtime.Serialization;
using Chess_Logic.classes.chess_piece;

namespace Chess_Logic.classes
{
    // Checks chess rules (legal moves, check, checkmate, stalemate) on a given board.
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes")]
    public class Game
    {
        [DataMember]
        private Chess_piece[,] board;
        [DataMember]
        private int moves_played;

        // Creates a new game with the starting chess board.
        public Game()
        {
            this.board        = Logic.DefaultBoard();
            this.moves_played = 0;
        }

        // Wraps an existing board and move count so rules can be checked.
        public Game(Chess_piece[,] board, int moves_played)
        {
            this.board        = board;
            this.moves_played = moves_played;
        }

        public int GetMovesPlayed()
        {
            return this.moves_played;
        }

        public Chess_piece[,] GetBoard()
        {
            return this.board;
        }

        // Copies the entire 8x8 board so we can test moves without changing the real board.
        public Chess_piece[,] CopyBoard()
        {
            Chess_piece[,] copy = new Chess_piece[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    copy[x, y] = this.board[x, y].Copy_self();
                }
            }
            return copy;
        }

        // Returns the team color of the piece at (x, y).
        public Team_color GetTeamAt(int x, int y)
        {
            if (this.board == null)
                return Team_color.Empty;
            if (!Logic.InBoard(x, y))
                return Team_color.Empty;

            return this.board[x, y].GetTeamColor();
        }

        // Checks if a move is legal and does not leave the player's own king in check.
        public bool CanExecuteMove(Game_move move)
        {
            if (this.board == null)
                return false;
            if (!Logic.InBoard(move.GetFromX(), move.GetFromY()))
                return false;
            if (!Logic.InBoard(move.GetToX(), move.GetToY()))
                return false;

            // Check that the piece can reach the target square.
            List<Game_move> possible_moves = this.board[move.GetFromX(), move.GetFromY()].GetPossibleMoves(this.board);
            bool found = false;
            for (int i = 0; i < possible_moves.Count; i++)
            {
                Game_move move_to_compare = possible_moves[i];
                if (move_to_compare.GetToX() == move.GetToX() && move_to_compare.GetToY() == move.GetToY())
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;

            // Simulate the move on a copy of the board and locate our king.
            Chess_piece[,] board_simulation = this.CopyBoard();
            Logic.RunMove(board_simulation, move);

            int king_x = -1;
            int king_y = -1;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Logic.IsKing(board_simulation[x, y].Gettype()) && board_simulation[x, y].GetTeamColor() == move.GetTeamColor())
                    {
                        king_x = x;
                        king_y = y;
                    }
                }
            }

            if (king_x < 0)
                return false;

            // The move is legal only if our king is not in check after the move.
            return !Logic.IsCheckAt(board_simulation, king_x, king_y, move.GetTeamColor());
        }

        // Returns all legal moves for the piece at (x, y).
        public List<Game_move> Posible_Moves(int x, int y)
        {
            if (this.board == null || !Logic.InBoard(x, y))
                return new List<Game_move>();

            List<Game_move> attempt = this.board[x, y].GetPossibleMoves(this.board);
            List<Game_move> legal   = new List<Game_move>();
            for (int i = 0; i < attempt.Count; i++)
            {
                if (CanExecuteMove(attempt[i]))
                {
                    legal.Add(attempt[i]);
                }
            }
            return legal;
        }

        // Checks if the given team has at least one legal move anywhere on the board.
        private bool HasAnyLegalMove(Team_color color)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (this.board[x, y].GetTeamColor() != color)
                        continue;
                    if (Posible_Moves(x, y).Count > 0)
                        return true;
                }
            }
            return false;
        }

        // Returns the game status: 0=ongoing, 1=white in check, 2=black in check, 3=white lost, 4=black lost, 5=draw.
        public int GetGameStatus()
        {
            if (this.board == null)
                return 0;

            Team_color to_move = Team_color.Black;
            if (this.moves_played % 2 == 0)
                to_move = Team_color.White;

            // Find the current player's king on the board.
            int king_x = -1;
            int king_y = -1;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Logic.IsKing(this.board[x, y].Gettype()) && this.board[x, y].GetTeamColor() == to_move)
                    {
                        king_x = x;
                        king_y = y;
                    }
                }
            }

            if (king_x < 0)
                return 0;

            bool in_check       = Logic.IsCheckAt(this.board, king_x, king_y, to_move);
            bool has_legal_move = HasAnyLegalMove(to_move);

            if (has_legal_move)
            {
                if (!in_check)
                    return 0;
                if (to_move == Team_color.White)
                    return 1;
                return 2;
            }

            if (!in_check)
                return 5;
            if (to_move == Team_color.White)
                return 3;
            return 4;
        }
    }
}
