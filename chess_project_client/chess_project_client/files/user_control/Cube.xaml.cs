using chess_project_client.ChessServer;
using chess_project_client.files.classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace chess_project_client.files.user_control
{
    // Visual control representing a single square on the 8x8 chess board.
    public partial class Cube : UserControl
    {
        private Chess_piece_type piece;
        private Team_color       team_color;
        private int x;
        private int y;

        private const string light_square = "#EBECD0";
        private const string dark_square  = "#779556";

        // Initializes the square with its coordinates, background color, and initial piece.
        public Cube(Chess_piece_type type, int x, int y, Team_color pieceTeam, Team_color squareBackground)
        {
            InitializeComponent();

            this.piece      = type;
            this.x          = x;
            this.y          = y;
            this.team_color = pieceTeam;

            string hex = light_square;
            if (squareBackground == Team_color.Black)
                hex = dark_square;
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

            this.Padding = new Thickness(6);

            ShowPiece();
        }

        // Updates the piece type and team displayed on this square.
        public void Update(Chess_piece_type newType, Team_color newTeam)
        {
            this.piece      = newType;
            this.team_color = newTeam;

            ShowPiece();
        }

        // Loads and displays the image for the current piece on this square.
        private void ShowPiece()
        {
            this.Content = null;

            string imgPath = Handle_Chess_Pictures.type_to_img(this.piece, this.team_color);
            if (!string.IsNullOrEmpty(imgPath))
                this.Content = Handle_Chess_Pictures.StringToImage(imgPath);
        }

        public Chess_piece_type Gettype() { return this.piece; }
        public Team_color       GetTeam() { return this.team_color; }
        public int              GetX()    { return this.x; }
        public int              GetY()    { return this.y; }
    }
}
