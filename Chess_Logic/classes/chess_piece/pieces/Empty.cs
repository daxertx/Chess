using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    // Represents an empty square on the board.
    // Having a real object here (instead of null) means every board cell is always
    // non-null, so piece code never has to null-check before reading a square.
    public class Empty : chess_piece.Chess_piece
    {
        public Empty(int x, int y)
            : base(x, y, Team_color.Empty, chess_piece.Chess_piece_type.Empty)
        {
        }

        // empty squares have no moves
        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            return new List<Game_move>();
        }

        public override Chess_piece Copy_self()
        {
            return new Empty(GetX(), GetY());
        }
    }
}
