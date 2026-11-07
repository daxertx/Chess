using Chess_Logic.classes.chess_piece;

namespace Model.classes
{
    public class Move : BaseEntity
    {
        private int fromX;
        private int fromY;
        private int toX;
        private int toY;
        private Chess_piece_type pieceType;
        private Team_color pieceColor;
        private Chess_piece_type promotion;
        private bool moved_twice;

        public Move(int id, int fromX, int fromY, int toX, int toY, Chess_piece_type pieceType, Team_color pieceColor, Chess_piece_type promotion, bool moved_twice) : base(id)
        {
            this.fromX = fromX;
            this.fromY = fromY;
            this.toX = toX;
            this.toY = toY;
            this.pieceType = pieceType;
            this.pieceColor = pieceColor;
            this.promotion = promotion;
            this.moved_twice = moved_twice;
        }

        public int get_fromX() { return this.fromX; }
        public int get_fromY() { return this.fromY; }
        public int get_toX() { return this.toX; }
        public int get_toY() { return this.toY; }
        public Chess_piece_type get_pieceType() { return this.pieceType; }
        public Team_color get_pieceColor() { return this.pieceColor; }
        public Chess_piece_type get_promotion() { return this.promotion; }
        public bool get_moved_twice() { return this.moved_twice; }

        public Game_move ToGameMove()
        {
            Chess_piece_type finalType = this.pieceType;
            if (this.promotion != Chess_piece_type.Empty && this.promotion != Chess_piece_type.Pawn)
            {
                finalType = this.promotion;
            }

            Game_move move = new Game_move(this.fromX, this.fromY, this.toX, this.toY, finalType, this.pieceColor);
            move.SetMovedTwice(this.moved_twice);
            return move;
        }
    }
}
