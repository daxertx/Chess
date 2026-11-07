using Chess_Logic.classes;
using Chess_Logic.classes.chess_piece;
using chess_project.files.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace chess_project.files.View_Model
{
    public class GameDB : BaseDB
    {
        private UserDB userDB = new UserDB();

        public GameDB() : base()
        {
            EnsureGameStates();
        }

        private void EnsureGameStates()
        {
            object countObj = base.GetScalar("SELECT COUNT(*) FROM game_state");
            int count = 0;
            if (countObj != null)
                count = Convert.ToInt32(countObj);

            if (count >= 6)
                return;

            string[] meanings = { "Ongoing", "WhiteInCheck", "BlackInCheck", "WhiteLost", "BlackLost", "Draw" };
            for (int i = 0; i < meanings.Length; i++)
            {
                object exists = base.GetScalar("SELECT COUNT(*) FROM game_state WHERE state = ?", i);
                if (exists == null || Convert.ToInt32(exists) == 0)
                {
                    base.ExecuteNonQuery("INSERT INTO game_state (state, meanning) VALUES (?, ?)", i, meanings[i]);
                }
            }
        }

        public int save_Game_DB(GameTBL game)
        {
            string white = game.get_white_username();
            string black = game.get_black_username();

            if (!this.userDB.UserExists(white))
                this.userDB.CreateUser(white, "ai", false);
            if (!this.userDB.UserExists(black))
                this.userDB.CreateUser(black, "ai", false);

            int rows = base.ExecuteNonQuery(
                "INSERT INTO games (white_player_id, black_player_id, [time], result) VALUES (?, ?, Now(), ?)",
                white, black, (int)game.get_status());

            if (rows < 1)
                return -1;

            object max = base.GetScalar("SELECT MAX(id) FROM games");
            if (max == null)
                return -1;

            int gameId = Convert.ToInt32(max);
            game.set_Game_id(gameId);
            return gameId;
        }

        public bool Game_Over(GameTBL game)
        {
            if (game.get_Game_id() <= 0 || !Game_Exists(game.get_Game_id()))
            {
                int newId = save_Game_DB(game);
                if (newId <= 0)
                    return false;
            }

            return Upload_Moves(game);
        }

        public bool Game_Exists(int gameId)
        {
            object res = base.GetScalar("SELECT COUNT(*) FROM games WHERE id = ?", gameId);
            if (res == null)
                return false;
            return Convert.ToInt32(res) == 1;
        }

        private bool Upload_Moves(GameTBL game)
        {
            int gameId = game.get_Game_id();
            List<Move> moves = game.get_moveList().get_moves();

            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                int movedTwiceInt = 0;
                if (move.get_moved_twice())
                    movedTwiceInt = 1;

                base.ExecuteNonQuery(
                    "INSERT INTO moves " +
                    "(game, fromx, fromy, tox, toy, piece_type, team_color, promotion, moved_twice)" +
                    " VALUES" +
                    " (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    gameId,
                    move.get_fromX(),
                    move.get_fromY(),
                    move.get_toX(),
                    move.get_toY(),
                    (int)move.get_pieceType(),
                    (int)move.get_pieceColor(),
                    (int)move.get_promotion(),
                    movedTwiceInt);
            }

            base.ExecuteNonQuery("UPDATE games SET result = ? WHERE id = ?", (int)game.get_status(), gameId);
            return true;
        }

        public int SaveFinishedGame(string white, string black, List<Game_move> rawMoves, int result)
        {
            User whiteUser = this.userDB.GetUser(white);
            User blackUser = this.userDB.GetUser(black);

            Chess_piece[,] board = Logic.DefaultBoard();
            List<Move> modelMoves = new List<Move>();

            for (int i = 0; i < rawMoves.Count; i++)
            {
                Game_move gm = rawMoves[i];
                Chess_piece_type promo = Chess_piece_type.Empty;
                if (Logic.IsPromotionMove(board, gm))
                    promo = gm.GetPieceType();

                Move m = new Move(
                    i + 1,
                    gm.GetFromX(),
                    gm.GetFromY(),
                    gm.GetToX(),
                    gm.GetToY(),
                    gm.GetPieceType(),
                    gm.GetTeamColor(),
                    promo,
                    gm.GetMovedTwice());

                modelMoves.Add(m);
                Logic.RunMove(board, gm);
            }

            Move_List moveList = new Move_List(modelMoves);
            GameTBL gameTbl = new GameTBL(0, whiteUser, blackUser, DateTime.Now, moveList, (GameStatus)result);

            if (!Game_Over(gameTbl))
                return -1;

            return gameTbl.get_Game_id();
        }

        public List<Move> Get_Moves(int gameId)
        {
            List<Move> moves = new List<Move>();
            DataTable dt = base.GetTable("SELECT * FROM moves WHERE game = ? ORDER BY id ASC", gameId);
            if (dt == null)
                return moves;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int movedTwiceVal = ReadInt(row, "moved_twice");

                Move move = new Move(
                    ReadInt(row, "id"),
                    ReadInt(row, "fromx"),
                    ReadInt(row, "fromy"),
                    ReadInt(row, "tox"),
                    ReadInt(row, "toy"),
                    (Chess_piece_type)ReadInt(row, "piece_type"),
                    (Team_color)ReadInt(row, "team_color"),
                    (Chess_piece_type)ReadInt(row, "promotion"),
                    movedTwiceVal == 1
                );
                moves.Add(move);
            }
            return moves;
        }

        public int GetMoveCount(int gameId)
        {
            object n = base.GetScalar("SELECT COUNT(*) FROM moves WHERE game = ?", gameId);
            if (n == null)
                return 0;
            return Convert.ToInt32(n);
        }

        public Game_move GetMove(int gameId, int index)
        {
            if (!Game_Exists(gameId))
                return new Game_move();

            List<Move> moves = Get_Moves(gameId);
            if (index < 0 || index >= moves.Count)
                return new Game_move();

            return moves[index].ToGameMove();
        }

        public GameStatus Get_Game_Result(int gameId)
        {
            object res = base.GetScalar("SELECT result FROM games WHERE id = ?", gameId);
            if (res == null)
                return GameStatus.Ongoing;
            return (GameStatus)Convert.ToInt32(res);
        }

        public string[] GetGamesList()
        {
            DataTable table = base.GetTable("SELECT id, white_player_id, black_player_id, [time], result FROM games ORDER BY id DESC");
            if (table == null)
                return new string[0];

            string[] games = new string[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = ReadInt(row, "id");
                string player1 = row["white_player_id"].ToString();
                string player2 = row["black_player_id"].ToString();

                string time = "";
                if (row["time"] != DBNull.Value)
                    time = Convert.ToDateTime(row["time"]).ToString("yyyy-MM-dd HH:mm");

                int moves = GetMoveCount(id);
                int result = ReadInt(row, "result");

                games[i] = id + "|" + player1 + "|" + player2 + "|" + time + "|" + moves + "|" + result;
            }
            return games;
        }

        public List<Game_Data> GetGamesData(string player)
        {
            List<Game_Data> back = new List<Game_Data>();
            DataTable table = base.GetTable("SELECT id, white_player_id, black_player_id, [time], result FROM games WHERE white_player_id = ? OR black_player_id = ? ORDER BY id DESC", player, player);
            if (table == null)
                return back;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = ReadInt(row, "id");
                string p1 = row["white_player_id"].ToString();
                string p2 = row["black_player_id"].ToString();

                string time = "";
                if (row["time"] != DBNull.Value)
                    time = Convert.ToDateTime(row["time"]).ToString("yyyy-MM-dd HH:mm");

                int moves = GetMoveCount(id);
                int result = ReadInt(row, "result");

                string won = p1;
                string lost = p2;
                if (result == 3)
                {
                    won = p2;
                    lost = p1;
                }

                Game_Data data = new Game_Data(won, lost, time, moves);
                back.Add(data);
            }

            return back;
        }
    }
}
