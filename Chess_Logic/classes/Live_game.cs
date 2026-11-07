using System;
using System.Collections.Generic;
using Chess_Logic.classes.chess_piece;

namespace Chess_Logic.classes
{
    // Holds the state of one active game in the server's memory.
    public class Live_game
    {
        public int Id;

        public string White;
        public string Black;
        public bool VsAi;

        public Chess_piece[,] Board = Logic.DefaultBoard();
        public List<Game_move> Moves = new List<Game_move>();

        public int CachedStatus = 0;
        public int CachedAfterMoves = -1;

        public bool Finished;
        public bool Saved;
        public DateTime FinishedAt;

        // -1 while ongoing; set to 3 (white lost) or 4 (black lost) if a player leaves.
        public int ForcedStatus = -1;

        // Returns true if the given player is in this game.
        public bool HasPlayer(string player)
        {
            return this.White == player || this.Black == player;
        }

        // Returns true if the given player is playing White.
        public bool IsWhite(string player)
        {
            return this.White == player;
        }

        // Returns true when it is White's turn (even move count).
        public bool IsWhitesTurn()
        {
            return this.Moves.Count % 2 == 0;
        }
    }
}
