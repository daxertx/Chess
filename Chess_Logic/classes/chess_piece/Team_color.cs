using System.Runtime.Serialization;

namespace Chess_Logic.classes.chess_piece
{
    // Represents the team color of a piece or square (White, Black, or Empty).
    [DataContract(Name = "Team_color", Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes.chess_piece")]
    public enum Team_color
    {
        [EnumMember] White,
        [EnumMember] Black,
        [EnumMember] Empty
    }
}
