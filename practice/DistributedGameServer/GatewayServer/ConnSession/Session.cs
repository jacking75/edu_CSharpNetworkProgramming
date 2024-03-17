using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GatewayServer.ConnSession
{
    public partial class Session
    {

        public UInt64 UniqueID { get; private set; }

        public string NetSessionID { get; private set; }

        public Int64 CurState { get; private set; }

        public string UserID { get; private set; }


        Int64 IsWaitingForLoginResult;

        public Int16 LobbyNum { get; private set; }
        public UInt16 LobbyServerIndex { get; private set; }
        Int32 IsWaitingForLobbyEnterResult;


        public void Init(UInt64 uniqueID, string netSessionID)
        {
            CurState = (Int64)SessionState.NONE;
            UniqueID = uniqueID;
            NetSessionID = netSessionID;

            S2SPacketEncodingStream = new System.IO.MemoryStream(S2SPacketBuffer);
        }

        // 8바이트 변수 값을 변경하는 것으로 lock을 걸리 않아도 괜찮다
        public void SetStateToLogin() => CurState = (Int64)SessionState.LOGIN;

        public bool SetReqLogin()
        {
            var ret = Interlocked.CompareExchange(ref IsWaitingForLoginResult, 1, 0);
            if (IsWaitingForLoginResult == 1 && ret == 0 && 
                CurState == (Int64)SessionState.CONN)
            {
                return true;
            }

            return false;
        }

        public void LoginFail() => IsWaitingForLobbyEnterResult = 0;

        public void LoginSuccess(string userID)
        {
            IsWaitingForLobbyEnterResult = 0;
            UserID = userID;
            CurState = (Int64)SessionState.LOGIN;
        }

        public bool IsCertified()
        {
            if(CurState >= (Int64)SessionState.LOGIN)
            {
                return true;
            }
            return false;
        }

        public bool SetReqLobbyEnter()
        {
            //한번 실패하면 재 연결 전에는 시도할 수 없다
            var ret = Interlocked.CompareExchange(ref IsWaitingForLobbyEnterResult, 1, 0);
            if(LobbyNum <= 0 && IsWaitingForLobbyEnterResult == 1 && ret == 0)
            {
                return true;
            }

            return false;
        }

        public void EnterLobby(UInt16 lobbyServerIndex, Int16 lobbyNum)
        {
            LobbyServerIndex = lobbyServerIndex;
            LobbyNum = lobbyNum;
            IsWaitingForLobbyEnterResult = 0;
            CurState = (Int64)SessionState.LOBBY;
        }

        public void EnterLobbyFail()
        {
            IsWaitingForLobbyEnterResult = 0;
        }

        public void LeaveLobby()
        {
            LobbyServerIndex = 0;
            LobbyNum = -1;
            IsWaitingForLobbyEnterResult = 0;
            CurState = (Int64)SessionState.LOGIN;
        }

    }

    public enum SessionState
    {
        NONE = 0,
        CONN = 1,
        LOGIN = 2,
        LOBBY = 3,
        ROOM = 4
    }
}
