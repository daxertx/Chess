using System.Runtime.Serialization;

namespace Chess_Logic.classes.chess_piece
{
    // Represents a single chess move from (fromX, fromY) to (toX, toY).
    [DataContract(Name = "Game_move", Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes.chess_piece")]
    public struct Game_move
    {
        [DataMember] public int fromX { get; set; }
        [DataMember] public int fromY { get; set; }
        [DataMember] public int toX   { get; set; }
        [DataMember] public int toY   { get; set; }

        [DataMember] public Chess_piece_type pieceType   { get; set; }
        [DataMember] public Team_color       teamColor   { get; set; }
        [DataMember] public bool             moved_twice { get; set; }

        // Creates a move with coordinates, piece type, and team color.
        public Game_move(int fromX, int fromY, int toX, int toY, Chess_piece_type pieceType, Team_color teamColor)
        {
            this.fromX       = fromX;
            this.fromY       = fromY;
            this.toX         = toX;
            this.toY         = toY;
            this.pieceType   = pieceType;
            this.teamColor   = teamColor;
            this.moved_twice = false;
        }

        // Getters for move properties.
        public int              GetFromX()      { return this.fromX; }
        public int              GetFromY()      { return this.fromY; }
        public int              GetToX()        { return this.toX; }
        public int              GetToY()        { return this.toY; }
        public Chess_piece_type GetPieceType()  { return this.pieceType; }
        public Team_color       GetTeamColor()  { return this.teamColor; }
        public bool             GetMovedTwice() { return this.moved_twice; }

        // Setters for move properties.
        public void SetFromX(int v)                  { this.fromX       = v; }
        public void SetFromY(int v)                  { this.fromY       = v; }
        public void SetToX(int v)                    { this.toX         = v; }
        public void SetToY(int v)                    { this.toY         = v; }
        public void SetPieceType(Chess_piece_type v) { this.pieceType   = v; }
        public void SetTeamColor(Team_color v)       { this.teamColor   = v; }
        public void SetMovedTwice(bool v)            { this.moved_twice = v; }
    }
}
