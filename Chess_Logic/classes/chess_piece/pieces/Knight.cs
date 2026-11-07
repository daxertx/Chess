using System.Collections.Generic;

namespace Chess_Logic.classes.chess_piece.pieces
{
    public class Knight : chess_piece.Chess_piece
    {
        public Knight(int x, int y, chess_piece.Team_color team_color)
            : base(x, y, team_color, chess_piece.Chess_piece_type.Knight)
        {
        }

        public override List<Game_move> GetPossibleMoves(Chess_piece[,] board)
        {
            List<Game_move> moves = new List<Game_move>();

            int currentX  = this.GetX();
            int currentY  = this.GetY();
            var pieceType = board[currentX, currentY].Gettype();
            var teamColor = board[currentX, currentY].GetTeamColor();

            // all 8 "L"-shaped jumps a knight can make
            int[,] jumps = { { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 },
                              { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 } };

            for (int d = 0; d < 8; d++)
            {
                int targetX = currentX + jumps[d, 0];
                int targetY = currentY + jumps[d, 1];

                if (!Logic.InBoard(targetX, targetY)) continue;

                Chess_piece target = board[targetX, targetY];

                // can land on empty or enemy squares (not own pieces)
                if (Logic.IsEmpty(target.GetTeamColor()) || Logic.IsEnemy(teamColor, target.GetTeamColor()))
                    moves.Add(new Game_move(currentX, currentY, targetX, targetY, pieceType, teamColor));
            }

            return moves;
        }

        public override Chess_piece Copy_self()
        {
            return new Knight(GetX(), GetY(), GetTeamColor());
        }
    }
}
