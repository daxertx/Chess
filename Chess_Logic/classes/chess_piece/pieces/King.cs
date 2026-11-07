using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    // King piece: moves one square in any direction and handles castling.
    public class King : chess_piece.Chess_piece
    {
        private bool hasMoved = false;
        private const int start_file = 4;

        // Prevents infinite recursion when checking if castling squares are attacked.
        [System.ThreadStatic]
        private static bool in_castle_check;

        public King(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.King)
        {
        }

        // Returns all possible king moves (1-step moves and castling).
        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            int currentX  = this.GetX();
            int currentY  = this.GetY();
            var pieceType = board[currentX, currentY].Gettype();
            var teamColor = board[currentX, currentY].GetTeamColor();

            // 8 directions around the king (1 square each).
            int[,] directions =
            {
                { 1, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 },
                { 0, 1 }, { 0, -1 },  { -1, 0 }, { 1, 0 }
            };

            for (int d = 0; d < 8; d++)
            {
                int targetX = currentX + directions[d, 0];
                int targetY = currentY + directions[d, 1];

                if (!Logic.InBoard(targetX, targetY))
                    continue;

                Chess_piece target = board[targetX, targetY];

                if (Logic.IsEmpty(target.GetTeamColor()) || Logic.IsEnemy(teamColor, target.GetTeamColor()))
                    moves.Add(new Game_move(currentX, currentY, targetX, targetY, pieceType, teamColor));
            }

            // Check castling if the king has never moved and is not already inside a check scan.
            if (!this.hasMoved && currentY == start_file && !in_castle_check)
            {
                in_castle_check = true;
                try
                {
                    int rank = currentX;

                    if (!Logic.IsCheckAt(board, rank, currentY, teamColor))
                    {
                        // Check king-side castling (rook at file 7).
                        if (CanCastleTowards(board, rank, currentY, 7, teamColor))
                            moves.Add(new Game_move(rank, currentY, rank, currentY + 2, pieceType, teamColor));

                        // Check queen-side castling (rook at file 0).
                        if (CanCastleTowards(board, rank, currentY, 0, teamColor))
                            moves.Add(new Game_move(rank, currentY, rank, currentY - 2, pieceType, teamColor));
                    }
                }
                finally
                {
                    in_castle_check = false;
                }
            }

            return moves;
        }

        // Uses a loop to check that the rook has not moved, the path is empty, and the king's two steps are safe.
        private bool CanCastleTowards(Chess_piece[,] board, int rank, int kingFile, int rookFile, Team_color teamColor)
        {
            Chess_piece rookPiece = board[rank, rookFile];
            if (!Logic.IsRook(rookPiece.Gettype()))
                return false;
            if (((Rook)rookPiece).HasMoved())
                return false;

            int step = -1;
            if (rookFile > kingFile)
                step = 1;

            // Every square between the king and the rook must be empty.
            for (int file = kingFile + step; file != rookFile; file += step)
            {
                if (!Logic.IsEmpty(board[rank, file].GetTeamColor()))
                    return false;
            }

            // The two squares the king moves through must not be under attack.
            for (int i = 1; i <= 2; i++)
            {
                int passFile = kingFile + (step * i);
                if (Logic.IsCheckAt(board, rank, passFile, teamColor))
                    return false;
            }

            return true;
        }

        // Copies the king and its hasMoved state.
        public override Chess_piece Copy_self()
        {
            King copy = new King(GetX(), GetY(), GetTeamColor());
            copy.hasMoved = this.hasMoved;
            return copy;
        }

        public bool HasMoved()              { return this.hasMoved; }
        public void SetHasMoved(bool value) { this.hasMoved = value; }
    }
}
