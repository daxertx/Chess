using System;
using System.Collections.Generic;
using System.ServiceModel;
using ViewModel.classes;
using Chess_Logic.classes;
using Chess_Logic.classes.AiHandler;
using Chess_Logic.classes.chess_piece;

namespace chess_project
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChessService : IChessService
    {
        private UserDB user_db = new UserDB();
        private GameDB game_db = new GameDB();

        private class WaitingPlayer
        {
            public string Name;
            public DateTime LastSeen;
        }

        private static readonly List<WaitingPlayer> player_queue = new List<WaitingPlayer>();
        private static readonly List<Live_game> live_games = new List<Live_game>();
        private static int next_game_id = 1;

        private static readonly object games_lock = new object();
        private static readonly TimeSpan queue_timeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan finished_timeout = TimeSpan.FromMinutes(2);
        private static readonly Stock_Fish_Handler stock_fish = new Stock_Fish_Handler();

        public const string AiName = "StockFish";

        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            return this.user_db.CreateUser(username.Trim(), password);
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            return this.user_db.Correct_login(username.Trim(), password);
        }

        public void JoinGameQueue(string player)
        {
            lock (games_lock)
            {
                Cleanup();
                AbandonUnfinished(player);

                if (FindWaiting(player) != null)
                    return;

                if (player_queue.Count > 0)
                {
                    WaitingPlayer opponent = player_queue[0];
                    player_queue.RemoveAt(0);

                    StartGame(opponent.Name, player, false);
                    return;
                }

                WaitingPlayer waiting = new WaitingPlayer();
                waiting.Name = player;
                waiting.LastSeen = DateTime.UtcNow;
                player_queue.Add(waiting);
            }
        }

        public void JoinGameVsAi(string player)
        {
            lock (games_lock)
            {
                Cleanup();
                AbandonUnfinished(player);
                StartGame(player, AiName, true);
            }
        }

        private static Live_game StartGame(string white, string black, bool vs_ai)
        {
            Live_game game = new Live_game();
            game.Id = next_game_id;
            next_game_id++;

            game.White = white;
            game.Black = black;
            game.VsAi = vs_ai;

            live_games.Add(game);
            return game;
        }

        public int GetGameId(string player)
        {
            lock (games_lock)
            {
                WaitingPlayer waiting = FindWaiting(player);
                if (waiting != null)
                    waiting.LastSeen = DateTime.UtcNow;

                Live_game game = FindLiveGame(player);
                if (game == null)
                    return -1;

                return game.Id;
            }
        }

        public void LeaveGame(string player)
        {
            lock (games_lock)
            {
                WaitingPlayer waiting = FindWaiting(player);
                if (waiting != null)
                    player_queue.Remove(waiting);

                Live_game game = FindLiveGame(player);
                if (game == null)
                    return;

                int result = 4;
                if (game.IsWhite(player))
                    result = 3;

                game.ForcedStatus = result;
                Finish(game, result);
            }
        }

        public bool IsWhitePlayer(string player, int gameid)
        {
            lock (games_lock)
            {
                Live_game game = FindGame(gameid);
                if (game == null)
                    return false;

                return game.IsWhite(player);
            }
        }

        public static string[] GetWaitingPlayers()
        {
            lock (games_lock)
            {
                Cleanup();

                string[] names = new string[player_queue.Count];
                for (int i = 0; i < player_queue.Count; i++)
                {
                    names[i] = player_queue[i].Name;
                }

                return names;
            }
        }

        public List<Game_move> Posible_Moves(int gameid, int x, int y)
        {
            lock (games_lock)
            {
                Live_game game = FindGame(gameid);
                if (game == null)
                    return new List<Game_move>();

                List<Game_move> rules = Rules(game).Posible_Moves(x, y);
                return rules;
            }
        }

        public int GetMoveCount(int gameid)
        {
            lock (games_lock)
            {
                Live_game game = FindGame(gameid);
                if (game == null)
                    return 0;

                return game.Moves.Count;
            }
        }

        public Game_move GetMoveByIndex(int gameid, int moveIndex)
        {
            lock (games_lock)
            {
                Live_game game = FindGame(gameid);
                if (game == null)
                    return new Game_move();
                if (moveIndex < 0 || moveIndex >= game.Moves.Count)
                    return new Game_move();

                return game.Moves[moveIndex];
            }
        }

        public bool CanMove(string player, Game_move action)
        {
            lock (games_lock)
            {
                Live_game game = FindLiveGame(player);
                if (game == null)
                    return false;
                if (game.Finished)
                    return false;

                bool is_white = game.IsWhite(player);
                if (is_white != game.IsWhitesTurn())
                    return false;

                Team_color player_color = Team_color.Black;
                if (is_white)
                    player_color = Team_color.White;

                Game rules = Rules(game);

                if (rules.GetTeamAt(action.GetFromX(), action.GetFromY()) != player_color)
                    return false;

                action.SetTeamColor(player_color);

                if (!rules.CanExecuteMove(action))
                    return false;

                action.SetMovedTwice(Logic.IsDoublePawnAdvance(game.Board, action));
                Play(game, action);

                if (game.VsAi)
                    PlayAiMove(game);

                return true;
            }
        }

        private static void Play(Live_game game, Game_move move)
        {
            Logic.RunMove(game.Board, move);
            game.Moves.Add(move);
        }

        private static void PlayAiMove(Live_game game)
        {
            Game_move ai_move = stock_fish.RunMoveStockFish(game.Board, Team_color.Black);

            if (ai_move.GetFromX() == ai_move.GetToX() && ai_move.GetFromY() == ai_move.GetToY())
                return;

            ai_move.SetTeamColor(Team_color.Black);
            ai_move.SetMovedTwice(Logic.IsDoublePawnAdvance(game.Board, ai_move));

            Play(game, ai_move);
        }

        public int GetGameStatus(int gameid)
        {
            lock (games_lock)
            {
                Live_game game = FindGame(gameid);
                if (game == null)
                    return 0;

                if (game.ForcedStatus >= 0)
                    return game.ForcedStatus;

                if (game.CachedAfterMoves == game.Moves.Count)
                    return game.CachedStatus;

                int status = Rules(game).GetGameStatus();

                game.CachedStatus = status;
                game.CachedAfterMoves = game.Moves.Count;

                if (status == 3 || status == 4 || status == 5)
                    Finish(game, status);

                return status;
            }
        }

        private void Finish(Live_game game, int result)
        {
            if (game.Finished)
                return;

            game.Finished = true;
            game.FinishedAt = DateTime.UtcNow;

            if (this.game_db.SaveFinishedGame(game.White, game.Black, game.Moves, result) != -1)
                game.Saved = true;
        }

        public string[] GetGamesList()
        {
            return this.game_db.GetGamesList();
        }

        public int GetReplayMoveCount(int game_id)
        {
            return this.game_db.GetMoveCount(game_id);
        }

        public Game_move Get_Move_From_Replay(int game_id, int move_num)
        {
            return this.game_db.GetMove(game_id, move_num);
        }

        private static Game Rules(Live_game game)
        {
            return new Game(game.Board, game.Moves.Count);
        }

        private static Live_game FindGame(int gameid)
        {
            for (int i = 0; i < live_games.Count; i++)
            {
                if (live_games[i].Id == gameid)
                    return live_games[i];
            }
            return null;
        }

        private static Live_game FindLiveGame(string player)
        {
            for (int i = 0; i < live_games.Count; i++)
            {
                Live_game game = live_games[i];
                if (!game.Finished && game.HasPlayer(player))
                    return game;
            }
            return null;
        }

        private static WaitingPlayer FindWaiting(string player)
        {
            for (int i = 0; i < player_queue.Count; i++)
            {
                if (player_queue[i].Name == player)
                    return player_queue[i];
            }
            return null;
        }

        private static void AbandonUnfinished(string player)
        {
            for (int i = live_games.Count - 1; i >= 0; i--)
            {
                if (!live_games[i].Finished && live_games[i].HasPlayer(player))
                    live_games.RemoveAt(i);
            }

            WaitingPlayer waiting = FindWaiting(player);
            if (waiting != null)
                player_queue.Remove(waiting);
        }

        private static void Cleanup()
        {
            DateTime now = DateTime.UtcNow;

            DateTime queue_cutoff = now - queue_timeout;
            for (int i = player_queue.Count - 1; i >= 0; i--)
            {
                if (player_queue[i].LastSeen < queue_cutoff)
                    player_queue.RemoveAt(i);
            }

            DateTime finished_cutoff = now - finished_timeout;
            for (int i = live_games.Count - 1; i >= 0; i--)
            {
                if (live_games[i].Finished && live_games[i].FinishedAt < finished_cutoff)
                    live_games.RemoveAt(i);
            }
        }

        public List<Game_Data> Login_Website(string user, string pass)
        {
            if (!this.user_db.Correct_login(user, pass))
                return new List<Game_Data>();

            List<Game_Data> games = this.game_db.GetGamesData(user);
            return games;
        }

        public List<Game_Data> Login_Phone(string user, string pass)
        {
            if (!this.user_db.Correct_login(user, pass))
                return new List<Game_Data>();

            List<Game_Data> games = this.game_db.GetGamesData(user);
            return games;
        }

        public bool ping()
        {
            return true;
        }

        public bool IsAdmin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            return this.user_db.IsAdmin(username.Trim(), password);
        }

        public bool AddUser(string admin_username, string admin_password, string new_username, string new_password, bool isadmin)
        {
            if (string.IsNullOrWhiteSpace(new_username) || string.IsNullOrEmpty(new_password))
                return false;

            if (IsAdmin(admin_username, admin_password))
            {
                return this.user_db.CreateUser(new_username.Trim(), new_password, isadmin);
            }
            return false;
        }

        public string[] GetUsersList(string admin_username, string admin_password)
        {
            if (!IsAdmin(admin_username, admin_password))
                return new string[0];

            return this.user_db.GetUsersList();
        }
    }
}
