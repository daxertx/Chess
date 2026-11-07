using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    public class Queen : chess_piece.Chess_piece
    {
        public Queen(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.Queen)
        {
        }

        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            int currentX  = this.GetX();
            int currentY  = this.GetY();
            var pieceType = board[currentX, currentY].Gettype();
            var teamColor = board[currentX, currentY].GetTeamColor();

            // queen combines rook (4 straight) + bishop (4 diagonal) = 8 directions
            int[,] directions = { { 1, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 },
                                   { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };

            for (int d = 0; d < 8; d++)
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
            return new Queen(GetX(), GetY(), GetTeamColor());
        }
    }
}
