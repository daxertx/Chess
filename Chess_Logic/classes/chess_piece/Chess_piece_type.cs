using System.Runtime.Serialization;

namespace Chess_Logic.classes.chess_piece
{
    // All possible piece types on a chess square.
    [DataContract(Name = "Chess_piece_type", Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes.chess_piece")]
    public enum Chess_piece_type
    {
        [EnumMember] Pawn,
        [EnumMember] Rook,
        [EnumMember] Knight,
        [EnumMember] Bishop,
        [EnumMember] Queen,
        [EnumMember] King,
        [EnumMember] Empty
    }
}
