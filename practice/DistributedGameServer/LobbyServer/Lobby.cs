using ServerCommon;
using ServerCommon.MQ;

using MessagePack;
using Microsoft.Extensions.Logging;
using ZLogger;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace LobbyServer
{
    public class Lobby
    {
        static readonly ILogger<Lobby> Logger = LogManager.GetLogger<Lobby>();

        public UInt16 Index { get; set; }

        public Int16 Number { get; set; }

        public UInt16 MaxUserCount { get; set; }

        Dictionary<UInt64, LobbyUser> UserDic = new ();
        Dictionary<string, LobbyUser> UserIDDic = new();

        public static Action<Int32, string, byte[], int> MQSendFunc;

        const int MaxPacketLength = 8012;
        byte[] MQPacketEnCodeBuffer = new byte[MaxPacketLength];
        System.IO.MemoryStream MQPacketEnCodeStream;


        public void Init(UInt16 index, Int16 number, UInt16 maxUserCount)
        {
            Index = index;
            Number = number;
            MaxUserCount = maxUserCount;

            MQPacketEnCodeStream = new System.IO.MemoryStream(MQPacketEnCodeBuffer);

            Logger.ZLogInformation($"[Init] Index:{index}, Number:{number}, MaxUserCount:{maxUserCount}");
        }

        public bool AddUser(string userID, UInt16 gwServerIndex, UInt64 uid)
        {
            if (GetUser(userID) != null)
            {
                return false;
            }

            var lobbyUser = new LobbyUser();
            lobbyUser.Set(userID, gwServerIndex, uid);

            UserDic.Add(uid, lobbyUser);
            UserIDDic.Add(userID, lobbyUser);
            return true;
        }
               
        public bool RemoveUser(UInt64 uid)
        {
            var user = GetUser(uid);
            if( user == null)
            {
                return false;
            }

            UserIDDic.Remove(user.UserID);
            UserDic.Remove(uid);

            return true;
        }

        public LobbyUser GetUser(string userID)
        {
            if (UserIDDic.TryGetValue(userID, out var user))
            {
                return user;
            }
            return null;
        }
        public LobbyUser GetUser(UInt64 uid)
        {
            if (UserDic.TryGetValue(uid, out var user))
            {
                return user;
            }
            return null;
        }

        public int CurrentUserCount()
        {
            return UserDic.Count;
        }
                
        public void NotifyPacketNewUser(int mqIndex, UInt16 serverIndex, UInt64 newUserUID, string newUserID)
        {
            var notifyCS = new CSCommon.PKTNtfLobbyEnterNewUser();
            notifyCS.UserID = newUserID;
            var notifyPacket = MessagePackSerializer.Serialize(notifyCS);

            CSCommon.MsgPackPacketHeaderInfo.Write(notifyPacket,
                                                (UInt16)notifyPacket.Length,
                                                CSCommon.PacketID.NtfLobbyEnterNewUser);

            var (len, mqPacket) = MakeRelayMqPacket(serverIndex, notifyPacket);

            Broadcast(mqIndex, 0, len, mqPacket);
        }

        public void NotifyPacketLeaveUser(int mqIndex, UInt16 serverIndex, UInt64 leaveUserUID, string leaveUserID)
        {
            if (CurrentUserCount() == 0)
            {
                return;
            }

            var notifyCS = new CSCommon.PKTNtfLobbyLeaveUser();
            notifyCS.UserID = leaveUserID;
            var notifyPacket = MessagePackSerializer.Serialize(notifyCS);

            CSCommon.MsgPackPacketHeaderInfo.Write(notifyPacket,
                                                (UInt16)notifyPacket.Length,
                                                CSCommon.PacketID.NtfLobbyLeaveUser);

            var (len, mqPacket) = MakeRelayMqPacket(serverIndex, notifyPacket);

            Broadcast(mqIndex, leaveUserUID, len, mqPacket);
        }

        public void NotifyPacketChatMsg(int mqIndex, UInt16 serverIndex, UInt64 chatUserUID, string userID, string chatMsg)
        {
            var notifyData = new CSCommon.PKTNtfLobbyChat();
            notifyData.UserID = userID;
            notifyData.Message = chatMsg;
            var notifyPacket = MessagePackSerializer.Serialize(notifyData);

            CSCommon.MsgPackPacketHeaderInfo.Write(notifyPacket,
                                                (UInt16)notifyPacket.Length,
                                                CSCommon.PacketID.NtfLobbyChat);
                        
            var (len, mqPacket) = MakeRelayMqPacket(serverIndex, notifyPacket);

            Broadcast(mqIndex, 0, len, mqPacket);
        }
             
        (UInt16, byte[]) MakeRelayMqPacket(UInt16 serverIndex, byte[] relayPacket)
        {
            var header = new PacketHeaderInfo();
            header.ID = PacketID.ResLobbyRelay;
            header.SenderIndex = serverIndex;
            header.LobbyNumber = Number;
            header.Write(MQPacketEnCodeBuffer);

            Buffer.BlockCopy(relayPacket, 0,
                MQPacketEnCodeBuffer, PacketHeaderInfo.HeadSize, relayPacket.Length);
            var length = PacketHeaderInfo.HeadSize + relayPacket.Length;
            return ((UInt16)length, MQPacketEnCodeBuffer);
        }

        // 브로드캐스트. 유저가 속한 게이트웨이 서버별로 보낸다.
        void Broadcast(int mqIndex, UInt64 excludeUserUniqueId, UInt16 mqPacketLength, byte[] mqPacketBuffer)
        {
            List<GWUserUniqueIdList> gwIdsList = new List<GWUserUniqueIdList>();

            foreach (var user in UserDic.Values)
            {
                if (user.UID == excludeUserUniqueId)
                {
                    continue;
                }

                DivideGWUserUniqueIdList(user.GatewayServerIndex, user.UID, gwIdsList);
            }

            foreach (var gwIds in gwIdsList)
            {
                PacketHeaderInfo.WriteMultiUserDataOffset(mqPacketBuffer, mqPacketLength);

                var gwIdListData = MessagePackSerializer.Serialize(gwIds);
                
                Buffer.BlockCopy(gwIdListData, 0, mqPacketBuffer, mqPacketLength, gwIdListData.Length);
                                
                var sendLength = mqPacketLength + gwIdListData.Length;
                var subject = SubjectManager.ToGatewayServer(gwIds.GWServerIndex);
                MQSendFunc(mqIndex, subject, mqPacketBuffer, sendLength);
            }
        }

        void DivideGWUserUniqueIdList(UInt16 gwServerIndex, UInt64 uid, List<GWUserUniqueIdList> gwIdsList)
        {
            foreach (var gwIds in gwIdsList)
            {
                if (gwIds.GWServerIndex == gwServerIndex)
                {
                    gwIds.UserUniqueIdList.Add(uid);
                    return;
                }
            }

            var newGWIdList = new GWUserUniqueIdList();
            newGWIdList.GWServerIndex = gwServerIndex;
            newGWIdList.UserUniqueIdList = new List<UInt64>();
            newGWIdList.UserUniqueIdList.Add(uid);
            gwIdsList.Add(newGWIdList);

        }      
    }


    
}
