using MessagePack;
using PvPGameServer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPGameServer.PKHandler
{
    public partial class Process
    {
        public static Func<string, byte[], bool> NetSendFunc;
        public static Action<EFBinaryRequestInfo> DistributePacketFunc;
        public static Func<string, bool> ForcedCloseSessionFunc;
        public static Action<byte[]> PushRedisTaskFunc;
        public static Action<byte[]> PushDBTaskFunc;

        UserManager UserMgr;
        Rooms.Manager RoomMgr;

        Dictionary<int, Action<EFBinaryRequestInfo>> PacketHandlerMap = new();


        public void Init(UserManager userManager, Rooms.Manager roomManager)
        {
            UserMgr = userManager;
            RoomMgr = roomManager;

            RegistPacketHandler();
        }

        void RegistPacketHandler()
        {
            PacketHandlerMap.Add((int)PacketID.NTF_IN_CONNECT_CLIENT, HandlerNtfInnerConnectClient);
            PacketHandlerMap.Add((int)PacketID.NTF_IN_DISCONNECT_CLIENT, HandlerNtfInnerDisConnectedClient);
            PacketHandlerMap.Add((int)PacketID.NTF_IN_ROOM_LEAVE, HandlerNtfInnerRoomLeave);
            PacketHandlerMap.Add((int)PacketID.NTF_IN_ROOM_GAME_END, HandlerNtfInnerRoomGameEnd);
            PacketHandlerMap.Add((int)PacketID.NTF_IN_USERS_CHECK_STATE, HandlerNtfInnerUsersCheckState);

            PacketHandlerMap.Add((int)PacketID.REQ_LOGIN, HandlerRequestLogin);
            PacketHandlerMap.Add((int)PacketID.REQ_REDIS_LOGIN_RESULT, HandlerRequestRedisLoginResult);

            PacketHandlerMap.Add((int)PacketID.REQ_ROOM_ENTER, HandlerRequestRoomEnter);
            PacketHandlerMap.Add((int)PacketID.REQ_ROOM_LEAVE, HandlerRequestLeave);
            PacketHandlerMap.Add((int)PacketID.REQ_ROOM_CHAT, HandlerRequestChat);

            PacketHandlerMap.Add((int)PacketID.REQ_READY_OMOK, HandlerRequestReadyOmok);
            PacketHandlerMap.Add((int)PacketID.REQ_PUT_MOK, HandlerRequestPutMok);


            PacketHandlerMap.Add((int)PacketID.NTF_REDIS_NEW_MATCHING_ROOM, HandlerRequestRedisNewMatchRoom);

        }

        public bool Execute(UInt16 packetID, EFBinaryRequestInfo packet)
        {
            if (PacketHandlerMap.ContainsKey(packetID))
            {
                PacketHandlerMap[packetID](packet);
                return true;
            }

            return false;
        }


        Tuple<bool, Rooms.Room, Rooms.User> CheckRoomAndRoomUser(string userNetSessionID)
        {
            var user = UserMgr.GetUser(userNetSessionID);
            if (user == null || user.IsReserveCloseNetwork)
            {
                return new Tuple<bool, Rooms.Room, Rooms.User>(false, null, null);
            }

            var roomNumber = user.RoomNumber;
            var room = RoomMgr.GetRoom(roomNumber);

            if (room == null)
            {
                return new Tuple<bool, Rooms.Room, Rooms.User>(false, null, null);
            }

            var roomUser = room.GetUserByNetSessionId(userNetSessionID);

            if (roomUser == null)
            {
                return new Tuple<bool, Rooms.Room, Rooms.User>(false, room, null);
            }

            return new Tuple<bool, Rooms.Room, Rooms.User>(true, room, roomUser);
        }

        Rooms.Room GetRoom(int roomNum)
        {
            var room = RoomMgr.GetRoom(roomNum);
            return room;
        }

        /// <summary>
        /// ErrorCode만 채워서 응답할 경우
        /// </summary>
        void ResponseToClient<TPKTResponse>(PacketID packetId, ErrorCode errorCode, string sessionID)
            where TPKTResponse : PKTResponse, new()
        {
            var resPacket = new TPKTResponse
            {
                Result = (short)errorCode
            };
            var sendData = MessagePackSerializer.Serialize(resPacket);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)packetId, 0);

            NetSendFunc(sessionID, sendData);
        }
       
    }
}
