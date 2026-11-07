using System;

namespace Model.classes
{
    public class GameTBL : BaseEntity
    {
        private User white_player;
        private User black_player;
        private DateTime date;
        private Move_List moveList;
        private GameStatus status;

        public GameTBL(int id, User white_player, User black_player, DateTime date, Move_List moveList, GameStatus status) : base(id)
        {
            this.white_player = white_player;
            this.black_player = black_player;
            this.date = date;
            this.moveList = moveList;
            this.status = status;
        }

        public string get_white_username()
        {
            return this.white_player.get_Username();
        }

        public string get_black_username()
        {
            return this.black_player.get_Username();
        }

        public DateTime get_date()
        {
            return this.date;
        }

        public Move_List get_moveList()
        {
            return this.moveList;
        }

        public GameStatus get_status()
        {
            return this.status;
        }

        public int get_Game_id()
        {
            return this.Id;
        }

        public void set_Game_id(int id)
        {
            this.Id = id;
        }
    }
}
