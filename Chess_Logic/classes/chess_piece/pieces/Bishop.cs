using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    public class Bishop : chess_piece.Chess_piece
    {
        public Bishop(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.Bishop)
        {
        }

        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            int currentX  = this.GetX();
            int currentY  = this.GetY();
            var pieceType = board[currentX, currentY].Gettype();
            var teamColor = board[currentX, currentY].GetTeamColor();

            // bishop slides along all 4 diagonal directions
            int[,] directions = { { 1, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 } };

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

        public override Chess_piece Copy_self()
        {
            return new Bishop(GetX(), GetY(), GetTeamColor());
        }
    }
}
