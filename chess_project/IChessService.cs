using System.Collections.Generic;
using System.ServiceModel;
using Chess_Logic.classes;
using Chess_Logic.classes.chess_piece;

namespace chess_project
{
    [ServiceContract]
    public interface IChessService
    {
        [OperationContract]
        bool Register(string username, string password);

        [OperationContract]
        bool Login(string username, string password);

        [OperationContract]
        void JoinGameQueue(string player);

        [OperationContract]
        void JoinGameVsAi(string player);

        [OperationContract]
        int GetGameId(string player);

        [OperationContract]
        void LeaveGame(string player);

        [OperationContract]
        bool IsWhitePlayer(string player, int gameid);

        [OperationContract]
        List<Game_move> Posible_Moves(int gameid, int x, int y);

        [OperationContract]
        bool CanMove(string player, Game_move action);

        [OperationContract]
        int GetMoveCount(int gameid);

        [OperationContract]
        Game_move GetMoveByIndex(int gameid, int moveIndex);

        [OperationContract]
        int GetGameStatus(int gameid);

        [OperationContract]
        string[] GetGamesList();

        [OperationContract]
        int GetReplayMoveCount(int game_id);

        [OperationContract]
        Game_move Get_Move_From_Replay(int game_id, int move_num);

        [OperationContract]
        List<Game_Data> Login_Website(string user, string pass);

        [OperationContract]
        List<Game_Data> Login_Phone(string user, string pass);

        [OperationContract]
        bool ping();

        [OperationContract]
        bool IsAdmin(string username, string password);

        [OperationContract]
        bool AddUser(string admin_username, string admin_password, string new_username, string new_password, bool isadmin);

        [OperationContract]
        string[] GetUsersList(string admin_username, string admin_password);
    }
}
