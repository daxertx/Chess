namespace Model.classes
{
    public class Settings : BaseEntity
    {
        private ScreenSize screenSize;
        private string white_piece_style;
        private string black_piece_style;

        public Settings(int id, ScreenSize cs, string whiteStyle, string blackStyle) : base(id)
        {
            this.screenSize = cs;
            this.white_piece_style = whiteStyle;
            this.black_piece_style = blackStyle;
        }

        public ScreenSize get_screenSize()
        {
            return this.screenSize;
        }

        public string get_white_piece_style()
        {
            return this.white_piece_style;
        }

        public string get_black_piece_style()
        {
            return this.black_piece_style;
        }
    }

    public enum ScreenSize
    {
        Small,
        Medium,
        Large
    }
}
