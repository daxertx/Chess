using chess_project_client.ChessServer;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace chess_project_client.files.classes
{
    // Manages communication with the WCF chess server and background polling for a game.
    public class Game
    {
        private ChessServiceClient client;
        private readonly object client_lock = new object();

        private ChessServiceClient click_client;
        private readonly object click_lock = new object();

        private string name;
        private string server_url;

        private int  game_id = -1;
        private bool am_i_white;
        private bool my_turn;
        private bool waiting_for_echo;
        private int  last_seen_move   = 0;
        private int  last_seen_status = 0;
        private bool game_over        = false;

        private const int poll_delay_ms = 75;
        private const int failed_polls_before_warning = 10;
        private int  failed_polls = 0;
        private bool connection_lost_reported = false;

        public event Action<int, bool> OnGameStarted;
        public event Action<Game_move> OnMoveReceived;
        public event Action<int>       OnStatusChanged;
        public event Action            OnConnectionLost;

        private int gamemode;

        public Game(string serverUrl, int gamemode = 0)
        {
            // 0 = online PvP, 1 = vs AI, 2 = replay
            this.server_url   = serverUrl;
            this.client       = CreateClient(serverUrl);
            this.click_client = CreateClient(serverUrl);
            this.name         = Session.Username;
            this.gamemode     = gamemode;

            try
            {
                if (gamemode == 1)
                    this.AiPlay();
                else if (gamemode == 2)
                    this.ReplayLook();
                else
                    this.JoinQueue();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.InnerException + "",
                    "Connection Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void Start(int game_id = -1)
        {
            if (game_id != -1)
                return;

            Thread poll = new Thread(Poll_Loop);
            poll.IsBackground = true;
            poll.Start();
        }

        private static ChessServiceClient CreateClient(string url)
        {
            BasicHttpSecurityMode mode = BasicHttpSecurityMode.None;
            BasicHttpBinding binding   = new BasicHttpBinding(mode);
            EndpointAddress  endpoint  = new EndpointAddress(url);
            return new ChessServiceClient(binding, endpoint);
        }

        public static bool Login(string serverUrl, string username, string password)
        {
            ChessServiceClient client = CreateClient(serverUrl);
            try
            {
                return client.Login(username, password);
            }
            finally
            {
                try { client.Close(); }
                catch { client.Abort(); }
            }
        }

        public static bool Register(string serverUrl, string username, string password)
        {
            ChessServiceClient client = CreateClient(serverUrl);
            try
            {
                return client.Register(username, password);
            }
            finally
            {
                try { client.Close(); }
                catch { client.Abort(); }
            }
        }

        private void RecreateClient()
        {
            if (this.client != null)
            {
                try { this.client.Abort(); }
                catch { }
            }
            this.client = CreateClient(this.server_url);
        }

        private void FixClientIfBroken()
        {
            if (this.client == null)
            {
                this.RecreateClient();
                return;
            }

            if (this.client.State == CommunicationState.Faulted || this.client.State == CommunicationState.Closed)
            {
                this.RecreateClient();
            }
        }

        private void RecreateClickClient()
        {
            if (this.click_client != null)
            {
                try { this.click_client.Abort(); }
                catch { }
            }
            this.click_client = CreateClient(this.server_url);
        }

        private void FixClickClientIfBroken()
        {
            if (this.click_client == null)
            {
                this.RecreateClickClient();
                return;
            }

            if (this.click_client.State == CommunicationState.Faulted || this.click_client.State == CommunicationState.Closed)
            {
                this.RecreateClickClient();
            }
        }

        private void JoinQueue()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    this.client.JoinGameQueue(this.name);
                }
                catch
                {
                    this.RecreateClient();
                    this.client.JoinGameQueue(this.name);
                }
            }
        }

        private void AiPlay()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    this.client.JoinGameVsAi(this.name);
                }
                catch
                {
                    this.RecreateClient();
                    this.client.JoinGameVsAi(this.name);
                }
            }
        }

        private void ReplayLook()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
            }
        }

        public int ReplayMoveCount(int replay_game_id)
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetReplayMoveCount(replay_game_id);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetReplayMoveCount(replay_game_id);
                }
            }
        }

        public Game_move ReplayMove(int replay_game_id, int move_num)
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.Get_Move_From_Replay(replay_game_id, move_num);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.Get_Move_From_Replay(replay_game_id, move_num);
                }
            }
        }

        public string[] GetGamesList()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetGamesList();
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetGamesList();
                }
            }
        }

        private int AskGameId()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetGameId(this.name);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetGameId(this.name);
                }
            }
        }

        private bool AskIsWhite(int found_id)
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.IsWhitePlayer(this.name, found_id);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.IsWhitePlayer(this.name, found_id);
                }
            }
        }

        private int AskMoveCount()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetMoveCount(this.game_id);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetMoveCount(this.game_id);
                }
            }
        }

        private Game_move AskMoveByIndex(int index)
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetMoveByIndex(this.game_id, index);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetMoveByIndex(this.game_id, index);
                }
            }
        }

        private int AskGameStatus()
        {
            lock (this.client_lock)
            {
                this.FixClientIfBroken();
                try
                {
                    return this.client.GetGameStatus(this.game_id);
                }
                catch
                {
                    this.RecreateClient();
                    return this.client.GetGameStatus(this.game_id);
                }
            }
        }

        private void Poll_Loop()
        {
            while (!this.game_over)
            {
                try
                {
                    if (this.game_id == -1)
                        WaitForGameToStart();
                    else
                        FetchGameUpdates();

                    this.failed_polls = 0;
                }
                catch
                {
                    ReportPollFailure();
                }

                Thread.Sleep(poll_delay_ms);
            }
        }

        private void WaitForGameToStart()
        {
            int found_id = this.AskGameId();
            if (found_id == -1)
                return;

            this.game_id    = found_id;
            this.am_i_white = this.AskIsWhite(found_id);
            this.my_turn    = this.am_i_white;

            if (this.OnGameStarted != null)
                this.OnGameStarted(this.game_id, this.am_i_white);
        }
        private void FetchGameUpdates()
        {
            int count = this.AskMoveCount();

            while (this.last_seen_move < count)
            {
                Game_move move = this.AskMoveByIndex(this.last_seen_move);

                if (move.fromX == move.toX && move.fromY == move.toY)
                    break;

                this.last_seen_move++;
                this.my_turn          = !this.my_turn;
                this.waiting_for_echo = false;

                if (this.OnMoveReceived != null)
                    this.OnMoveReceived(move);
            }

            int status = this.AskGameStatus();
            if (status == this.last_seen_status)
                return;

            this.last_seen_status = status;

            if (status >= 3)
                this.game_over = true;

            if (this.OnStatusChanged != null)
                this.OnStatusChanged(status);
        }

        public void Leave()
        {
            if (this.game_over)
                return;
            this.game_over = true;

            try
            {
                lock (this.click_lock)
                {
                    this.FixClickClientIfBroken();
                    try
                    {
                        this.click_client.LeaveGame(this.name);
                    }
                    catch
                    {
                        this.RecreateClickClient();
                        this.click_client.LeaveGame(this.name);
                    }
                }
            }
            catch
            {
            }
        }

        private void ReportPollFailure()
        {
            this.failed_polls++;

            if (this.failed_polls < failed_polls_before_warning)
                return;
            if (this.connection_lost_reported)
                return;

            this.connection_lost_reported = true;
            if (this.OnConnectionLost != null)
                this.OnConnectionLost();
        }

        public bool TryExecuteMove(Game_move move)
        {
            try
            {
                bool accepted;
                lock (this.click_lock)
                {
                    this.FixClickClientIfBroken();
                    try
                    {
                        accepted = this.click_client.CanMove(this.name, move);
                    }
                    catch
                    {
                        this.RecreateClickClient();
                        accepted = this.click_client.CanMove(this.name, move);
                    }
                }

                if (accepted)
                    this.waiting_for_echo = true;

                return accepted;
            }
            catch
            {
                return false;
            }
        }

        public List<Game_move> get_moves(int x, int y)
        {
            try
            {
                Game_move[] moves;
                lock (this.click_lock)
                {
                    this.FixClickClientIfBroken();
                    try
                    {
                        moves = this.click_client.Posible_Moves(this.game_id, x, y);
                    }
                    catch
                    {
                        this.RecreateClickClient();
                        moves = this.click_client.Posible_Moves(this.game_id, x, y);
                    }
                }

                List<Game_move> result = new List<Game_move>();
                if (moves == null)
                    return result;

                for (int i = 0; i < moves.Length; i++)
                {
                    result.Add(moves[i]);
                }
                return result;
            }
            catch
            {
                return new List<Game_move>();
            }
        }

        public Game_move GetMove(int x, int y, int tx, int ty, Chess_piece_type t, Team_color team)
        {
            Game_move move = new Game_move();
            move.fromX     = x;
            move.fromY     = y;
            move.toX       = tx;
            move.toY       = ty;
            move.pieceType = t;
            move.teamColor = team;
            return move;
        }

        public int  GetGameId() { return this.game_id; }
        public bool AmIWhite()  { return this.am_i_white; }

        public bool IsMyTurn()
        {
            return this.my_turn && !this.waiting_for_echo;
        }
    }
}
