using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    // Pawn piece: moves forward, captures diagonally, and supports double-step and en passant.
    public class Pawn : chess_piece.Chess_piece
    {
        private bool hasMovedTwice = false;

        public Pawn(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.Pawn)
        {
        }

        // Returns all possible moves for this pawn.
        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            bool isWhite = Logic.IsWhite(this.GetTeamColor());

            int dir = -1;
            if (isWhite)
                dir = 1;

            int nextX    = this.GetX() + dir;
            int currentY = this.GetY();

            // One step forward if the square is empty.
            if (Logic.InBoard(nextX, currentY) && Logic.IsEmpty(board[nextX, currentY].GetTeamColor()))
            {
                moves.Add(new Game_move(this.GetX(), currentY, nextX, currentY, Chess_piece_type.Pawn, this.GetTeamColor()));

                // Two steps forward from the starting rank if both squares are empty.
                int startRank = 6;
                if (isWhite)
                    startRank = 1;

                int doubleX = nextX + dir;
                if (this.GetX() == startRank && Logic.InBoard(doubleX, currentY) && Logic.IsEmpty(board[doubleX, currentY].GetTeamColor()))
                {
                    Game_move doubleMove = new Game_move(this.GetX(), currentY, doubleX, currentY, Chess_piece_type.Pawn, this.GetTeamColor());
                    doubleMove.SetMovedTwice(true);
                    moves.Add(doubleMove);
                }
            }

            // Check both diagonal sides (-1 and +1) for captures and en passant.
            int[] sides = { -1, 1 };
            for (int i = 0; i < sides.Length; i++)
            {
                int captureY = currentY + sides[i];

                // Normal diagonal capture.
                if (Logic.InBoard(nextX, captureY) && Logic.IsEnemy(this.GetTeamColor(), board[nextX, captureY].GetTeamColor()))
                    moves.Add(new Game_move(this.GetX(), currentY, nextX, captureY, Chess_piece_type.Pawn, this.GetTeamColor()));

                // En passant capture.
                int epRank = 3;
                if (isWhite)
                    epRank = 4;

                if (this.GetX() == epRank && Logic.InBoard(this.GetX(), captureY))
                {
                    Chess_piece neighbor = board[this.GetX(), captureY];
                    if (Logic.IsPawn(neighbor.Gettype()) && Logic.IsEnemy(this.GetTeamColor(), neighbor.GetTeamColor()))
                    {
                        Pawn enemyPawn = neighbor as Pawn;
                        if (enemyPawn != null && enemyPawn.HasMovedTwice())
                        {
                            moves.Add(new Game_move(this.GetX(), currentY, nextX, captureY, Chess_piece_type.Pawn, this.GetTeamColor()));
                        }
                    }
                }
            }

            return moves;
        }

        // Copies the pawn and its hasMovedTwice flag.
        public override Chess_piece Copy_self()
        {
            Pawn copy = new Pawn(GetX(), GetY(), GetTeamColor());
            copy.hasMovedTwice = this.hasMovedTwice;
            return copy;
        }

        public bool HasMovedTwice()              { return this.hasMovedTwice; }
        public void SetHasMovedTwice(bool value) { this.hasMovedTwice = value; }
    }
}
