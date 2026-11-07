using System.Collections.Generic;

namespace chess_project.files.Model
{
    public class Move_List
    {
        private List<Move> moves;

        public Move_List(List<Move> moves)
        {
            this.moves = new List<Move>();
            if (moves != null)
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    this.moves.Add(moves[i]);
                }
            }
        }

        public List<Move> get_moves()
        {
            return this.moves;
        }
    }
}
