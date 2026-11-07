using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Chess_Logic.classes;
using Chess_Logic.classes.chess_piece;

namespace chess.database
{
    public class Talk_to_database
        {
        public const string DBName = "chess.accdb";

        private static readonly string database_path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\files\database\chess.accdb"));
       
        public static string connection_string = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={database_path};Persist Security Info=False;";

        
        //only one thread accesses file at a time
        private static readonly object db_lock = new object();

        // fills quary
        private static void AddArgs(OleDbCommand cmd, object[] args)
        {
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                cmd.Parameters.AddWithValue("?", args[i]);
            }
        }

        // run select and return table
        private static DataTable GetTable(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);

                        DataTable table = new DataTable();
                        new OleDbDataAdapter(cmd).Fill(table);
                        return table;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        // get a value
        private static object GetScalar(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);
                        con.Open();

                        object result = cmd.ExecuteScalar();
                        if (result == DBNull.Value)
                            return null;
                        return result;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        // change table
        private static int ExecuteNonQuery(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);
                        con.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    return -1;
                }
            }
        }

        // read
        private static int ReadInt(DataRow row, string column)
        {
            object value = row[column];
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToInt32(value);
        }

        // username allready taken?
        public bool UserExists(string username)
        {
            DataTable table = GetTable("SELECT id FROM users WHERE username = ?", username);
            if (table == null)
                return false;
            return table.Rows.Count > 0;
        }

        // creates new user
        public bool CreateUser(string username, string password, bool isadmin = false)
        {
            if (UserExists(username))
                return false;

            return ExecuteNonQuery(
                "INSERT INTO users (username, [password], [admin]) VALUES (?, ?, ?)",
                username, password, isadmin) >= 1;
        }

        public bool CheckLogin(string username, string password)
        {
            DataTable table = GetTable(
                "SELECT id FROM users WHERE username = ? AND [password] = ?",
                username, password);
            if (table == null)
                return false;
            return table.Rows.Count == 1;
        }

        // game exists?
        public bool GameExists(int game_id)
        {
            DataTable table = GetTable("SELECT id FROM games WHERE id = ?", game_id);
            if (table == null)
                return false;
            return table.Rows.Count == 1;
        }

        //save game
        public int SaveFinishedGame(string white, string black, List<Game_move> moves, int result)
        {
            int rows = ExecuteNonQuery(
                "INSERT INTO games (player1, player2, [time], result) VALUES (?, ?, Now(), ?)",
                white, black, result);
            if (rows < 1)
                return -1;

            object max = GetScalar("SELECT MAX(id) FROM games");

            if (max == null)
                return -1;

            int game_id = Convert.ToInt32(max);

            int next_id = 1;

            object max_move = GetScalar("SELECT MAX(id) FROM moves");
            if (max_move != null)

                next_id = Convert.ToInt32(max_move) + 1;

            Chess_piece[,] board = Logic.DefaultBoard();

            for (int i = 0; i < moves.Count; i++)
            {
                Game_move move = moves[i];

                int promotion = 0;
                if (Logic.IsPromotionMove(board, move))
                    promotion = (int)move.GetPieceType();

                int moved_twice = 0;
                if (move.GetMovedTwice())
                    moved_twice = 1;

                ExecuteNonQuery(
                    "INSERT INTO moves (id, game, fromx, fromy, tox, toy, piece_type, team_color, promotion, moved_twice) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    next_id, game_id,
                    move.GetFromX(), move.GetFromY(), move.GetToX(), move.GetToY(),
                    (int)move.GetPieceType(), (int)move.GetTeamColor(),
                    promotion, moved_twice);

                next_id++;
                Logic.RunMove(board, move);
            }

            return game_id;
        }

        // get games as a list
        public string[] GetGamesList()
        {
            DataTable table = GetTable("SELECT id, player1, player2, [time], result FROM games ORDER BY id DESC");
            if (table == null)
                return new string[0];

            string[] games = new string[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = ReadInt(row, "id");

                string player1 = row["player1"].ToString();
                string player2 = row["player2"].ToString();

                string time = "";
                if (row["time"] != DBNull.Value)
                    time = Convert.ToDateTime(row["time"]).ToString("yyyy-MM-dd HH:mm");

                int moves = GetMoveCount(id);
                int result = ReadInt(row, "result");

                games[i] = id + "|" + player1 + "|" + player2 + "|" + time + "|" + moves + "|" + result;
            }
            return games;
        }

        // get all moves for a game
        private static DataTable GetMoves(int game_id)
        {
            return GetTable("SELECT * FROM moves WHERE game = ? ORDER BY id", game_id);
        }

        // get amount of moves played
        public int GetMoveCount(int game_id)
        {
            object n = GetScalar("SELECT COUNT(*) FROM moves WHERE game = ?", game_id);
            
            if (n == null)
                return 0;

            return Convert.ToInt32(n);
        }

        //database to game move
        private static Game_move ReadMove(DataRow row)
        {
            int fromx = ReadInt(row, "fromx");
            int fromy = ReadInt(row, "fromy");
            int tox   = ReadInt(row, "tox");
            int toy   = ReadInt(row, "toy");

            Chess_piece_type pieceType = (Chess_piece_type)ReadInt(row, "piece_type");
            Team_color       teamColor = (Team_color)ReadInt(row, "team_color");

            int promotion = ReadInt(row, "promotion");
            if (promotion != 0)
                pieceType = (Chess_piece_type)promotion;

            Game_move move = new Game_move(fromx, fromy, tox, toy, pieceType, teamColor);
            move.SetMovedTwice(ReadInt(row, "moved_twice") == 1);
            return move;
        }

        // get a specific move
        public Game_move GetMove(int game_id, int count)
        {
            if (!GameExists(game_id))
                return new Game_move();

            DataTable table = GetMoves(game_id);

            if (table == null)
                return new Game_move();

            if (count < 0 || count >= table.Rows.Count)
                return new Game_move();

            return ReadMove(table.Rows[count]);
        }

        //get all games a player had played
        public List<Game_Data> GetGamesData(string player)
        {
            List<Game_Data> back = new List<Game_Data>();
            DataTable table = GetTable("SELECT id, player1, player2, [time], result FROM games WHERE player1 = ? OR player2 = ? ORDER BY id DESC", player, player);
            
            if (table == null)
                return back;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = ReadInt(row, "id");

                string p1 = row["player1"].ToString();
                string p2 = row["player2"].ToString();

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
        // Checks if the user exists and has admin = True.
        public bool IsAdmin(string username, string password)
        {
            DataTable table = GetTable(
                "SELECT id FROM users WHERE username = ? AND [password] = ? AND [admin] = True",
                username, password);

            if (table == null)
                return false;
            return table.Rows.Count == 1;
        }

        // Returns all users from the users table packed as "id|username|password|admin".
        public string[] GetUsersList()
        {
            DataTable table = GetTable("SELECT id, username, [password], [admin] FROM users ORDER BY id");
            if (table == null)
                return new string[0];

            string[] users = new string[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = ReadInt(row, "id");
                string user = row["username"].ToString();
                string pass = row["password"].ToString();

                bool isAdmin = false;
                if (row["admin"] != DBNull.Value)
                    isAdmin = Convert.ToBoolean(row["admin"]);

                users[i] = id + "|" + user + "|" + pass + "|" + isAdmin;
            }
            return users;
        }
    }
}
