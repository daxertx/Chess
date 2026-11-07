using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace chess_project_client.files.classes
{
    // Fills the little a–h / 1–8 strips that sit around a board. White sees files a..h
    // from left to right with rank 8 on top; black sees everything the other way round,
    // so the labels are built from the same am_i_white flag the board itself uses.
    public static class BoardLabels
    {
        // one square is 640 / 8
        private const double cell = 80;
        private const double strip = 22;

        public static void Fill(Panel rankLabels, Panel fileLabels, bool amWhite)
        {
            rankLabels.Children.Clear();
            fileLabels.Children.Clear();

            for (int i = 0; i < 8; i++)
            {
                int rankNumber = 8 - i;
                if (!amWhite) rankNumber = i + 1;
                rankLabels.Children.Add(MakeCell(rankNumber.ToString(), strip, cell));
            }

            for (int i = 0; i < 8; i++)
            {
                int fileIndex = i;
                if (!amWhite) fileIndex = 7 - i;
                char letter = (char)('a' + fileIndex);
                fileLabels.Children.Add(MakeCell(letter.ToString(), cell, strip));
            }
        }

        // A fixed-size box with the text centred, so each label lines up with its row or column.
        private static Border MakeCell(string text, double width, double height)
        {
            TextBlock label = new TextBlock();
            label.Text = text;
            label.Foreground = new SolidColorBrush(Color.FromRgb(0xB8, 0xB4, 0xAE));
            label.FontSize = 15;
            label.FontWeight = FontWeights.SemiBold;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;

            Border box = new Border();
            box.Width = width;
            box.Height = height;
            box.Child = label;
            return box;
        }
    }
}
