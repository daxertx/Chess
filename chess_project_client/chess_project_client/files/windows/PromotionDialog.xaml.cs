using chess_project_client.ChessServer;
using chess_project_client.files.classes;
using System.Windows;

namespace chess_project_client.files.windows
{
    public partial class PromotionDialog : Window
    {
        // The piece the player chose — defaults to Queen so closing the window also gives Queen
        public Chess_piece_type ChosenPiece { get; private set; } = Chess_piece_type.Queen;

        public PromotionDialog(Team_color team)
        {
            InitializeComponent();

            // Load the piece image for this player's color into each button
            QueenBtn.Content  = Handle_Chess_Pictures.StringToImage(Chess_piece_type.Queen,  team);
            RookBtn.Content   = Handle_Chess_Pictures.StringToImage(Chess_piece_type.Rook,   team);
            BishopBtn.Content = Handle_Chess_Pictures.StringToImage(Chess_piece_type.Bishop, team);
            KnightBtn.Content = Handle_Chess_Pictures.StringToImage(Chess_piece_type.Knight, team);
        }

        private void Queen_Click(object sender, RoutedEventArgs e)
        {
            ChosenPiece  = Chess_piece_type.Queen;
            DialogResult = true;
        }

        private void Rook_Click(object sender, RoutedEventArgs e)
        {
            ChosenPiece  = Chess_piece_type.Rook;
            DialogResult = true;
        }

        private void Bishop_Click(object sender, RoutedEventArgs e)
        {
            ChosenPiece  = Chess_piece_type.Bishop;
            DialogResult = true;
        }

        private void Knight_Click(object sender, RoutedEventArgs e)
        {
            ChosenPiece  = Chess_piece_type.Knight;
            DialogResult = true;
        }
    }
}
