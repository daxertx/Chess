using System.Collections.Generic;
using Chess_Logic.classes.chess_piece;
using Chess_Logic.classes.chess_piece.pieces;

namespace Chess_Logic.classes
{
    // Helper functions for board setup, move execution, and check detection.
    public static class Logic
    {
        // Piece type checks.
        public static bool IsPawn(Chess_piece_type type)   { return type == Chess_piece_type.Pawn; }
        public static bool IsRook(Chess_piece_type type)   { return type == Chess_piece_type.Rook; }
        public static bool IsKnight(Chess_piece_type type) { return type == Chess_piece_type.Knight; }
        public static bool IsBishop(Chess_piece_type type) { return type == Chess_piece_type.Bishop; }
        public static bool IsQueen(Chess_piece_type type)  { return type == Chess_piece_type.Queen; }
        public static bool IsKing(Chess_piece_type type)   { return type == Chess_piece_type.King; }
        public static bool IsEmpty(Chess_piece_type type)  { return type == Chess_piece_type.Empty; }

        // Team color checks.
        public static bool IsWhite(Team_color color)                    { return color == Team_color.White; }
        public static bool IsBlack(Team_color color)                    { return color == Team_color.Black; }
        public static bool IsEmpty(Team_color color)                    { return color == Team_color.Empty; }
        public static bool IsSameColor(Team_color a, Team_color b)      { return a == b; }

        // Returns true when two squares belong to opposite teams.
        public static bool IsEnemy(Team_color a, Team_color b)
        {
            return a != b && a != Team_color.Empty && b != Team_color.Empty;
        }

        // Returns true if 'from' can move onto 'to' (empty or enemy square).
        public static bool CanGoTo(Chess_piece from, Chess_piece to)
        {
            return IsEnemy(from.GetTeamColor(), to.GetTeamColor()) || IsEmpty(to.Gettype());
        }

        // Checks if coordinates (x, y) are inside the 8x8 board.
        public static bool InBoard(int x, int y)
        {
            return x >= 0 && x <= 7 && y >= 0 && y <= 7;
        }

        // Checks if a pawn move reaches the last rank for promotion.
        public static bool IsPromotionMove(Chess_piece[,] board, Game_move move)
        {
            if (!InBoard(move.GetFromX(), move.GetFromY()))
                return false;
            if (!IsPawn(board[move.GetFromX(), move.GetFromY()].Gettype()))
                return false;

            return move.GetToX() == 0 || move.GetToX() == 7;
        }

        // Checks if a pawn moved two squares forward in one turn.
        public static bool IsDoublePawnAdvance(Chess_piece[,] board, Game_move move)
        {
            if (!InBoard(move.GetFromX(), move.GetFromY()))
                return false;
            if (!IsPawn(board[move.GetFromX(), move.GetFromY()].Gettype()))
                return false;

            int rankDistance = move.GetToX() - move.GetFromX();
            if (rankDistance < 0)
                rankDistance = -rankDistance;

            return rankDistance == 2;
        }

        // Builds and returns the standard 8x8 starting chess board.
        public static Chess_piece[,] DefaultBoard()
        {
            Chess_piece[,] board = new Chess_piece[8, 8];

            // White back rank (row 0) and black back rank (row 7).
            Chess_piece_type[] backRow =
            {
                Chess_piece_type.Rook,
                Chess_piece_type.Knight,
                Chess_piece_type.Bishop,
                Chess_piece_type.Queen,
                Chess_piece_type.King,
                Chess_piece_type.Bishop,
                Chess_piece_type.Knight,
                Chess_piece_type.Rook
            };

            for (int y = 0; y < 8; y++)
            {
                board[0, y] = CreateFullPiece(0, y, Team_color.White, backRow[y]);
                board[1, y] = new Pawn(1, y, Team_color.White);
                board[6, y] = new Pawn(6, y, Team_color.Black);
                board[7, y] = CreateFullPiece(7, y, Team_color.Black, backRow[y]);
            }

            // Empty middle rows (rows 2 to 5).
            for (int x = 2; x < 6; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board[x, y] = new Empty(x, y);
                }
            }

            return board;
        }

        // Creates a piece of any type at (x, y).
        private static Chess_piece CreateFullPiece(int x, int y, Team_color color, Chess_piece_type type)
        {
            if (type == Chess_piece_type.Rook)   return new Rook(x, y, color);
            if (type == Chess_piece_type.Knight) return new Knight(x, y, color);
            if (type == Chess_piece_type.Bishop) return new Bishop(x, y, color);
            if (type == Chess_piece_type.Queen)  return new Queen(x, y, color);
            if (type == Chess_piece_type.King)   return new King(x, y, color);
            if (type == Chess_piece_type.Pawn)   return new Pawn(x, y, color);
            return new Empty(x, y);
        }

        // Executes a move on the board (including castling, en passant, and promotion).
        public static Chess_piece[,] RunMove(Chess_piece[,] board, Game_move move)
        {
            Chess_piece movingPiece = board[move.GetFromX(), move.GetFromY()];

            bool isPawnMove     = IsPawn(movingPiece.Gettype());
            bool isDiagonal     = move.GetFromY() != move.GetToY();
            bool targetIsEmpty  = IsEmpty(board[move.GetToX(), move.GetToY()].Gettype());
            bool isEnPassant    = isPawnMove && isDiagonal && targetIsEmpty;

            // Reset en-passant flags on all pawns before applying the new move.
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (IsPawn(board[x, y].Gettype()))
                    {
                        ((Pawn)board[x, y]).SetHasMovedTwice(false);
                    }
                }
            }

            // Move the piece to its destination square.
            board[move.GetToX(), move.GetToY()]     = movingPiece.Copy_self();
            board[move.GetFromX(), move.GetFromY()] = new Empty(move.GetFromX(), move.GetFromY());

            board[move.GetToX(), move.GetToY()].SetX(move.GetToX());
            board[move.GetToX(), move.GetToY()].SetY(move.GetToY());

            Chess_piece moved = board[move.GetToX(), move.GetToY()];

            // Handle king movement and castling rook jump.
            if (IsKing(moved.Gettype()))
            {
                ((King)moved).SetHasMoved(true);

                int fileDiff = move.GetToY() - move.GetFromY();
                int x        = move.GetToX();

                if (fileDiff == 2)
                {
                    board[x, 5] = board[x, 7].Copy_self();
                    board[x, 5].SetX(x);
                    board[x, 5].SetY(5);
                    ((Rook)board[x, 5]).SetHasMoved(true);
                    board[x, 7] = new Empty(x, 7);
                }
                else if (fileDiff == -2)
                {
                    board[x, 3] = board[x, 0].Copy_self();
                    board[x, 3].SetX(x);
                    board[x, 3].SetY(3);
                    ((Rook)board[x, 3]).SetHasMoved(true);
                    board[x, 0] = new Empty(x, 0);
                }
            }
            else if (IsRook(moved.Gettype()))
            {
                ((Rook)moved).SetHasMoved(true);
            }
            else if (IsPawn(moved.Gettype()) && move.GetMovedTwice())
            {
                ((Pawn)moved).SetHasMovedTwice(true);
            }

            // Remove the captured pawn in en passant.
            if (isEnPassant)
                board[move.GetFromX(), move.GetToY()] = new Empty(move.GetFromX(), move.GetToY());

            // Promote pawn if it reached the last rank.
            if (IsPawn(moved.Gettype()) && (move.GetToX() == 0 || move.GetToX() == 7))
            {
                Team_color       color         = moved.GetTeamColor();
                Chess_piece_type promoteToType = move.GetPieceType();

                if (promoteToType == Chess_piece_type.Pawn ||
                    promoteToType == Chess_piece_type.King ||
                    promoteToType == Chess_piece_type.Empty)
                {
                    promoteToType = Chess_piece_type.Queen;
                }

                board[move.GetToX(), move.GetToY()] = CreatePiece(move.GetToX(), move.GetToY(), color, promoteToType);
            }

            return board;
        }

        // Returns true if square (x, y) is attacked by any enemy piece.
        public static bool IsCheckAt(Chess_piece[,] board, int x, int y, Team_color king_color)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Chess_piece piece = board[row, col];
                    if (!IsEnemy(piece.GetTeamColor(), king_color))
                        continue;

                    List<Game_move> enemyMoves = piece.GetPossibleMoves(board);
                    for (int i = 0; i < enemyMoves.Count; i++)
                    {
                        if (enemyMoves[i].GetToX() == x && enemyMoves[i].GetToY() == y)
                            return true;
                    }
                }
            }
            return false;
        }

        // Creates a promoted piece (Queen, Rook, Bishop, or Knight).
        public static Chess_piece CreatePiece(int x, int y, Team_color color, Chess_piece_type type)
        {
            switch (type)
            {
                case Chess_piece_type.Queen:  return new Queen(x, y, color);
                case Chess_piece_type.Rook:   return new Rook(x, y, color);
                case Chess_piece_type.Bishop: return new Bishop(x, y, color);
                case Chess_piece_type.Knight: return new Knight(x, y, color);
                default:                      return new Queen(x, y, color);
            }
        }

        // Returns a deep copy of an 8x8 board.
        public static Chess_piece[,] CloneBoard(Chess_piece[,] board)
        {
            Chess_piece[,] b = new Chess_piece[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    b[x, y] = board[x, y].Copy_self();
                }
            }
            return b;
        }
    }
}
