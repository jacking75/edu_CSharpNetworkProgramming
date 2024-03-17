using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PvPGameServer.Enum;

namespace PvPGameServer.Rooms
{
    public partial class Room
    {
        public int Index { get; private set; }
        public int Number { get; private set; }
        
        int MaxUserCount = 0;

        List<User> RoomUserList = new List<User>();
        
        public static Func<string, byte[], bool> NetSendFunc;
        public static Action<EFBinaryRequestInfo> DistributePacketFunc;
        public static Action<byte[]> PushRedisTaskFunc;
        public static Action<byte[]> PushDBTaskFunc;


        public void Init(int index, int number, int maxUserCount, int turnTimeout)
        {
            Index = index;
            Number = number;
            MaxUserCount = maxUserCount;
            TurnTimeout = turnTimeout;
        }

        public bool AddUser(string userID, string netSessionID)
        {
            if (RoomUserList.Count >= MaxUserCount)
            {
                return false;
            }
            
            if(GetRoomUser(userID) != null)
            {
                return false;
            }

            var roomUser = new User();
            roomUser.Init(userID, netSessionID);
            RoomUserList.Add(roomUser);

            return true;
        }

        public bool RemoveUser(User user)
        {
            return RoomUserList.Remove(user);
        }

        public void RemoveAllUser()
        {
            RoomUserList.Clear();
        }

        public User GetRoomUser(string userID)
        {
            return RoomUserList.Find(x => x.UserID == userID);
        }

        public User GetUserByNetSessionId(string netSessionID)
        {
            return RoomUserList.Find(x => x.NetSessionID == netSessionID);
        }

        public List<string> GetAllUserSessionIDs()
        {
            var sessionList = new List<string>();

            foreach(var user in RoomUserList)
            {
                sessionList.Add(user.NetSessionID);
            }

            return sessionList;
        }

        public void NotifyPacketUserList(string userNetSessionID)
        {
            var packet = new PKTNtfRoomUserList();
            foreach (var user in RoomUserList)
            {
                var userID = user.State == UserState.READY ? $"READY_{user.UserID}" : user.UserID;
                packet.UserIDList.Add(userID);
            }

            var sendData = MessagePackSerializer.Serialize(packet);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_ROOM_USER_LIST, 0);
                        
            NetSendFunc(userNetSessionID, sendData);
        }

        public void NotifyPacketNewUser(string newUserNetSessionID, string newUserID)
        {
            var packet = new PKTNtfRoomNewUser();
            packet.UserID = newUserID;
            
            var sendData = MessagePackSerializer.Serialize(packet);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_ROOM_NEW_USER, 0);

            Broadcast(newUserNetSessionID, sendData);
        }

        public void NotifyPacketLeaveUser(string userID)
        {
            if (RoomUserList.Count == 0)
            {
                return;
            }

            var packet = new PKTNtfRoomLeaveUser();
            packet.UserID = userID;
            
            var sendData = MessagePackSerializer.Serialize(packet);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_ROOM_LEAVE_USER, 0);

            Broadcast("", sendData);
        }

        public void Chat(string senderUserID, string chatMsg)
        {            
            var resPacket = new PKTNtfRoomChat
            {
                UserID = senderUserID,
                ChatMessage = chatMsg
            };

            var sendData = MessagePackSerializer.Serialize(resPacket);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_ROOM_CHAT, 0);

            Broadcast("", sendData);
        }

        public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
        {
            foreach(var user in RoomUserList)
            {
                if (user.NetSessionID == excludeNetSessionID)
                {
                    continue;
                }

                NetSendFunc(user.NetSessionID, sendPacket);
            }
        }
        
    }
}
