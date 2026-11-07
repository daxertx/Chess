
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Chess_Logic.classes;
using Chess_Logic.classes.chess_piece;
using Chess_Logic.classes.ui;


namespace chess_project.files.userControl
{
    public partial class Cube : UserControl
    {
        private Chess_piece piece;
        public Cube(Chess_piece piece,Team_color background)
        {
            InitializeComponent();
            this.piece = piece;

            string hex = "#eeeed2";//white
            if(Logic.IsBlack(background))
                hex = "#769656";//black

            Color backGroundColor = (Color)ColorConverter.ConvertFromString(hex);
            this.Background = new SolidColorBrush(backGroundColor);

            string url = Handle_chess_pictures.type_to_img(piece.Gettype(),piece.GetTeamColor());
            if(url != "" )
            {
                chess_piece = Handle_chess_pictures.StringToImage(url);
            }
        }
    }
}
