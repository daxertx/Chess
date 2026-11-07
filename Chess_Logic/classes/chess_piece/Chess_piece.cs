using System.Collections.Generic;
using System.Runtime.Serialization;
using Chess_Logic.classes.chess_piece.pieces;

namespace Chess_Logic.classes.chess_piece
{
    // Base class for every chess piece on the board.
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes.chess_piece")]
    [KnownType(typeof(Pawn))]
    [KnownType(typeof(Rook))]
    [KnownType(typeof(Knight))]
    [KnownType(typeof(Bishop))]
    [KnownType(typeof(Queen))]
    [KnownType(typeof(King))]
    public abstract class Chess_piece
    {
        [DataMember] public int              X         { get; set; }
        [DataMember] public int              Y         { get; set; }
        [DataMember] public Team_color       TeamColor { get; set; }
        [DataMember] public Chess_piece_type Type      { get; set; }

        // Initializes piece position, color, and type.
        protected Chess_piece(int x, int y, Team_color team_color, Chess_piece_type type)
        {
            this.X         = x;
            this.Y         = y;
            this.TeamColor = team_color;
            this.Type      = type;
        }

        // Getters for piece position, color, and type.
        public int              GetX()         { return this.X; }
        public int              GetY()         { return this.Y; }
        public Team_color       GetTeamColor() { return this.TeamColor; }
        public Chess_piece_type Gettype()      { return this.Type; }

        // Setters for piece position, color, and type.
        public void SetX(int x)                         { this.X         = x; }
        public void SetY(int y)                         { this.Y         = y; }
        public void SetTeamColor(Team_color team_color) { this.TeamColor = team_color; }
        public void SetType(Chess_piece_type type)      { this.Type      = type; }

        // Returns a deep copy of this piece.
        public abstract Chess_piece Copy_self();

        // Returns all pseudo-legal moves this piece can make on the board.
        public abstract List<Game_move> GetPossibleMoves(Chess_piece[,] board);
    }
}
