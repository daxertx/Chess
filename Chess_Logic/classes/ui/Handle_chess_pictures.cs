using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Chess_Logic.classes;
using Chess_Logic.classes.chess_piece;
using Chess_Logic.classes.chess_piece.pieces;
namespace Chess_Logic.classes.ui
{
    public static class Handle_chess_pictures
    {
        public static Image StringToImage(string path)
        {
            if (path == "")
            {
                return null;
            }
            Image img = new Image();
            img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.Relative));
            return img;
        }
        public static Image StringToImage(Chess_piece_type piece, Team_color color)
        {
            string path = type_to_img(piece, color);
            if (path == "")
            {
                return null;
            }
            Image img = new Image();
            img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.Relative));
            return img;
        }
        public static string type_to_img(Chess_piece_type piece,Team_color color)
        {
            string img_name = "";
            switch (piece)
            {
                case Chess_piece_type.Pawn:
                    img_name = "chess-pawn";
                    break;
                case Chess_piece_type.Bishop:
                    img_name = "chess-bishop";
                    break;
                case Chess_piece_type.Knight:
                    img_name = "chess-knight";
                    break;
                case Chess_piece_type.Rook:
                    img_name = "chess-rook";
                    break;
                case Chess_piece_type.Queen:
                    img_name = "chess-queen";
                    break;
                case Chess_piece_type.King:
                    img_name = "chess-king";
                    break;
                case Chess_piece_type.Empty:
                    break;
            }
            if (Logic.IsWhite(color))
            {
                img_name += "-white.png";
            }
            else if (Logic.IsBlack(color))
            {
                img_name += "-black.png";
            }
            return img_name;
        }
    }
}