using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    public class MQSubjectManager
    {
        ConcurrentDictionary<int, string> LobbyLookUp = new ();
        ConcurrentDictionary<int, string> RoomLookUp = new ();


        public void RequestLobbyRoomMQInfo(UInt16 myServerIndex, 
                                    Action<string,byte[], int, int> MQSendFunc)
        {
            var packetData = new Byte[MQ.PacketHeaderInfo.HeadSize];
            var mqHeader = new MQ.PacketHeaderInfo
            {
                ID = (UInt16)MQ.PacketID.ReqLobbyRoomMqInfo,
                SenderIndex = myServerIndex,
            };
            mqHeader.Write(packetData);

            MQSendFunc("to.C", packetData, 0, MQ.PacketHeaderInfo.HeadSize);
        }

        public void SetLobbyMQAddress(List<MQ.LobbyRangeMQInfo> lobbyList)
        {
            LobbyLookUp.Clear();

            foreach(var lobby in lobbyList)
            {
                var subject = $"to.L{lobby.ServerIndex}";
                for(var i = lobby.StartNum; i <= lobby.LastNum; ++i )
                {
                    LobbyLookUp.TryAdd(i, subject);
                }
            }           
        }

        public void SetRoomMQAddress(List<MQ.RoomRangeMQInfo> roomList)
        {
            RoomLookUp.Clear();

            foreach (var room in roomList)
            {
                var subject = $"to.G{room.ServerIndex}";
                for (var i = room.StartNum; i <= room.LastNum; ++i)
                {
                    LobbyLookUp.TryAdd(i, subject);
                }
            }
        }

        public string GetSubjectLobby(Int32 lobbyNum)
        {
            LobbyLookUp.TryGetValue(lobbyNum, out var subject);
            return subject;
        }

        public string GetSubjectRoom(Int32 roomNum)
        {
            RoomLookUp.TryGetValue(roomNum, out var subject);
            return subject;
        }
    }

    
}
