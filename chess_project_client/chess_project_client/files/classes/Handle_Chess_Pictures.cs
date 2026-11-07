using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace chess_project_client.files.classes
{
    public class Handle_Chess_Pictures
    {
        public static Image StringToImage(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            try
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource   = new Uri("pack://application:,,,/" + path, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                Image img   = new Image();
                img.Source  = bmp;
                img.Stretch = Stretch.Uniform;
                return img;
            }
            catch
            {
                return null;
            }
        }

        public static Image StringToImage(ChessServer.Chess_piece_type piece, ChessServer.Team_color color)
        {
            return StringToImage(type_to_img(piece, color));
        }

        public static string type_to_img(ChessServer.Chess_piece_type piece, ChessServer.Team_color color)
        {
            if (piece == ChessServer.Chess_piece_type.Empty)
                return "";

            string baseName;
            switch (piece)
            {
                case ChessServer.Chess_piece_type.Pawn:
                    baseName = "chess-pawn";
                    break;
                case ChessServer.Chess_piece_type.Bishop:
                    baseName = "chess-bishop";
                    break;
                case ChessServer.Chess_piece_type.Knight:
                    baseName = "chess-knight";
                    break;
                case ChessServer.Chess_piece_type.Rook:
                    baseName = "chess-rook";
                    break;
                case ChessServer.Chess_piece_type.Queen:
                    baseName = "chess-queen";
                    break;
                case ChessServer.Chess_piece_type.King:
                    baseName = "chess-king";
                    break;
                default:
                    return "";
            }

            string colorSuffix;
            if (color == ChessServer.Team_color.White)
                colorSuffix = "-white.png";
            else if (color == ChessServer.Team_color.Black)
                colorSuffix = "-black.png";
            else
                return "";

            return "files/assets/chess_pieces/" + baseName + colorSuffix;
        }
    }
}
