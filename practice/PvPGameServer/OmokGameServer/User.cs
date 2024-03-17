using System;
using System.Collections.Generic;
using System.Text;

namespace PvPGameServer
{
    public class User
    {
        public int Index { get; private set; } = -1;

        public string SessionID { get; private set; }

        public int MatchRoomNumber { get; private set; } = -1;
        public int RoomNumber { get; private set; } = -1;
        public string UserID { get; private set; }

        public bool IsReserveCloseNetwork { get; private set; } = false;
        public DateTime ReserveCloseNetworkTime { get; private set; }


        public void Init(int index)
        {
            Index = index;
        }

        public void Clear()
        {
            MatchRoomNumber = -1;
            RoomNumber = -1;
            IsReserveCloseNetwork = false;
            SessionID = "";
            UserID = "";
        }

        public void Use(string sessionID)
        {
            SessionID = sessionID;
        }

        public void SetLogin(string userID, int roomNum)
        {
            UserID = userID;
            MatchRoomNumber = roomNum;
        }

        public bool IsConfirm(string netSessionID) => SessionID == netSessionID;

        public void EnteredRoom() => RoomNumber = MatchRoomNumber;

        public void LeaveRoom() => RoomNumber = -1;

        public bool IsStateLogin() => string.IsNullOrEmpty(UserID) == false;

        public bool IsStateRoom() => RoomNumber != -1;

        public void SetReserveCloseNetwork(DateTime time)
        {
            IsReserveCloseNetwork = true;
            ReserveCloseNetworkTime = time;
        }
        
        public bool OverReserveCloseNetworkTime(DateTime time)
        {
            if(IsReserveCloseNetwork == false ||
                ReserveCloseNetworkTime > time)
            {
                return false;
            }

            return true;
        }
    }
}
