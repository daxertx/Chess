using System.Runtime.Serialization;

namespace Chess_Logic.classes
{
    // Summary of a finished game sent to the website and phone app.
    [DataContract(Name = "Game_Data", Namespace = "http://schemas.datacontract.org/2004/07/chess_project.files.classes")]
    public struct Game_Data
    {
        [DataMember] private string player_won      { get; set; }
        [DataMember] private string player_lost     { get; set; }
        [DataMember] private string date            { get; set; }
        [DataMember] private int    amount_of_moves { get; set; }

        // Creates a finished game record with winner, loser, date, and move count.
        public Game_Data(string p1, string p2, string d, int am)
        {
            this.player_won      = p1;
            this.player_lost     = p2;
            this.date            = d;
            this.amount_of_moves = am;
        }

        public string GetPlayerWon()     { return this.player_won; }
        public string GetPlayerLost()    { return this.player_lost; }
        public string GetDate()          { return this.date; }
        public int    GetAmountOfMoves() { return this.amount_of_moves; }
    }
}
