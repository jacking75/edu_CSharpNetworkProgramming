using Microsoft.Extensions.Logging;
using ZLogger;
using ServerCommon;
using ServerCommon.MQ;

using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace CenterServer.PKHandler
{
    public partial class Handler
    {
        public void RequestLobbyRoomMQInfo(int mqIndex, PacketHeaderInfo mqHead, byte[] mqData)
        {
            Logger.ZLogInformation("[RequestLobbyRoomMQInfo]");

            var responseData = new ResLobbyRoomMQInfo();

            foreach (var lobby in ServerOpt.LobbyRangeMQInfoList)
            {
                responseData.LobbyList.Add(new ServerCommon.MQ.LobbyRangeMQInfo
                {
                    StartNum = lobby.StartNum,
                    LastNum = lobby.LastNum,
                    ServerIndex = lobby.ServerIndex
                }
                );
            }

            foreach (var lobby in ServerOpt.RoomRangeMQInfoList)
            {
                responseData.RoomList.Add(new ServerCommon.MQ.RoomRangeMQInfo
                {
                    StartNum = lobby.StartNum,
                    LastNum = lobby.LastNum,
                    ServerIndex = lobby.ServerIndex
                }
                );
            }

            var packetData = MessagePackSerializer.Serialize(responseData);

            var mqPacketHeader = new PacketHeaderInfo();
            mqPacketHeader.Type = 0;
            mqPacketHeader.ID = PacketID.ResLobbyRoomMqInfo;
            mqPacketHeader.SenderIndex = ServerOpt.ServerIndex;
            mqPacketHeader.Write(packetData);

            var subject = SubjectManager.ToGatewayServer(mqHead.SenderIndex);
            SendMqFunc(mqIndex, subject, packetData, packetData.Length);
        }

        
    }
}
