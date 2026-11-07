using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    public class Rook : chess_piece.Chess_piece
    {
        // true once the rook has moved — castling requires both king and rook to be unmoved
        private bool hasMoved = false;

        public Rook(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.Rook)
        {
        }

        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            int currentX  = this.GetX();
            int currentY  = this.GetY();
            var pieceType = board[currentX, currentY].Gettype();
            var teamColor = board[currentX, currentY].GetTeamColor();

            // rook slides in 4 straight directions (no diagonals)
            int[,] directions = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

            for (int d = 0; d < 4; d++)
            {
                int dirX = directions[d, 0];
                int dirY = directions[d, 1];

                for (int step = 1; step < 8; step++)
                {
                    int targetX = currentX + (dirX * step);
                    int targetY = currentY + (dirY * step);

                    if (!Logic.InBoard(targetX, targetY)) break;

                    Chess_piece target = board[targetX, targetY];

                    if (Logic.IsEmpty(target.GetTeamColor()))
                    {
                        moves.Add(new Game_move(currentX, currentY, targetX, targetY, pieceType, teamColor));
                    }
                    else if (Logic.IsEnemy(teamColor, target.GetTeamColor()))
                    {
                        moves.Add(new Game_move(currentX, currentY, targetX, targetY, pieceType, teamColor));
                        break; // can capture but not pass through
                    }
                    else
                    {
                        break; // blocked by own piece
                    }
                }
            }

            return moves;
        }

        // Copy_self must copy hasMoved so board simulations stay accurate
        public override Chess_piece Copy_self()
        {
            Rook copy = new Rook(GetX(), GetY(), GetTeamColor());
            copy.hasMoved = this.hasMoved;
            return copy;
        }

        public bool HasMoved() { return this.hasMoved; }
        public void SetHasMoved(bool value) { this.hasMoved = value; }
    }
}
