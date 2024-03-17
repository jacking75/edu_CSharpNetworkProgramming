using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        const int ReserveCloseNetworkWaitTimeMillSec = 1000 * 60;

        void HandlerNtfInnerRoomGameEnd(EFBinaryRequestInfo packetData)
        {
            MainServer.GlobalLogger.Debug("Received Internal: ProcessInnerRoomGameEnd");

            var NtfGameEnd = MessagePackSerializer.Deserialize<PKTInternalNtfRoomGameEnd>(packetData.Data);
                        
            // 룸을 초기화 하고
            var room = GetRoom(NtfGameEnd.RoomNumber);
            if(room == null)
            {
                MainServer.GlobalLogger.Error($"ProcessInnerRoomGameEnd: Invalid RoomNum:{NtfGameEnd.RoomNumber}");
                return;
            }

            if (room.IsPlayingOmok())
            {
                return;
            }


            if (NtfGameEnd.IsFail == false)
            {
                NotifySaveGameResultToDB(NtfGameEnd.WinUserID, NtfGameEnd.LoseUserID);
            }

            ReserveCloseNetworkAllUsers(room);
            
            room.RemoveAllUser();
            room.Clear();
            room.RegistAvailableRoomToRedis();
        }

        void ReserveCloseNetworkAllUsers(Rooms.Room room)
        {
            var reserverTime = DateTime.Now.AddMilliseconds(ReserveCloseNetworkWaitTimeMillSec);
            var sessionList = room.GetAllUserSessionIDs();
            foreach (var sessionID in sessionList)
            {
                UserMgr.ReserveCloseNetwork(sessionID, reserverTime);
            }
        }

        void NotifySaveGameResultToDB(string winUser, string loseUser)
        {
            var dbTask = new DB.NtfSaveGameResultTask
            {
                WinUserID = winUser,
                LoseUserID = loseUser,
            };

            var sendData = MessagePackSerializer.Serialize(dbTask);
            DB.MsgPackHeaderInfo.WriteID(sendData, DB.TaskID.NotifySaveGameResult);
            PushDBTaskFunc(sendData);
        }
    }
}
