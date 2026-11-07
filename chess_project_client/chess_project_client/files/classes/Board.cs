using chess_project_client.ChessServer;
using chess_project_client.files.user_control;

namespace chess_project_client.files.classes
{
    // Builds the 8x8 visual grid of Cube controls in the starting chess position.
    public class Board
    {
        // Creates and returns the 8x8 array of Cube squares for a new game.
        public Cube[,] NewBoard()
        {
            Cube[,] board = new Cube[8, 8];

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

            for (int col = 0; col < 8; col++)
            {
                for (int row = 0; row < 8; row++)
                {
                    Team_color squareColor = Team_color.Black;
                    if ((col + row) % 2 == 0)
                        squareColor = Team_color.White;

                    Chess_piece_type pieceType = Chess_piece_type.Empty;
                    Team_color       pieceTeam = Team_color.Empty;

                    if (row == 0)
                    {
                        pieceType = backRow[col];
                        pieceTeam = Team_color.White;
                    }
                    else if (row == 1)
                    {
                        pieceType = Chess_piece_type.Pawn;
                        pieceTeam = Team_color.White;
                    }
                    else if (row == 6)
                    {
                        pieceType = Chess_piece_type.Pawn;
                        pieceTeam = Team_color.Black;
                    }
                    else if (row == 7)
                    {
                        pieceType = backRow[col];
                        pieceTeam = Team_color.Black;
                    }

                    board[col, row] = new Cube(pieceType, col, row, pieceTeam, squareColor);
                }
            }

            return board;
        }
    }
}
