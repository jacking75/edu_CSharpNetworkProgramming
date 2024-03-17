using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GatewayServer.ConnSession
{
    public partial class Session
    {
        byte[] S2SPacketBuffer = new byte[1024 * 2];
        public System.IO.MemoryStream S2SPacketEncodingStream { get; private set; }



        public (int, byte[]) MakeRequestGatewayLoginMQPacket(UInt16 myServerIndex, string userID, string authToken)
        {
            var requestData = new ServerCommon.MQ.ReqGatewayLogin()
            {
                UserID = userID,
                AuthToken = authToken
            };

            S2SPacketEncodingStream.Position = 0;
            MessagePack.MessagePackSerializer.Serialize(S2SPacketEncodingStream, requestData);

            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo
            {
                ID = (UInt16)ServerCommon.MQ.PacketID.ReqGatewayLogin,
                SenderIndex = myServerIndex,
                LobbyNumber = 0,
                UID = UniqueID
            };
            mqHeader.Write(S2SPacketBuffer);

            var length = (int)S2SPacketEncodingStream.Position;
            return (length, S2SPacketBuffer);
        }

        public (int, byte[]) MakeNotifyGatewayLogOut(UInt16 myServerIndex, string userID)
        {
            var requestData = new ServerCommon.MQ.NtfGatewayLogout()
            {
                UserID = userID,
            };

            S2SPacketEncodingStream.Position = 0;
            MessagePack.MessagePackSerializer.Serialize(S2SPacketEncodingStream, requestData);

            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo
            {
                ID = (UInt16)ServerCommon.MQ.PacketID.NtfGatewayLogout,
                SenderIndex = myServerIndex,
                LobbyNumber = 0,
                UID = UniqueID
            };
            mqHeader.Write(S2SPacketBuffer);

            var length = (int)S2SPacketEncodingStream.Position;
            return (length, S2SPacketBuffer);
        }

        public (int, byte[]) MakeRequestLobbyEnterMQPacket(UInt16 myServerIndex, Int16 lobbyNum)
        {
            var requestData = new ServerCommon.MQ.ReqLobbyEnter()
            {
                UserID = this.UserID,
                LobbyNumber = lobbyNum
            };

            S2SPacketEncodingStream.Position = 0;
            MessagePack.MessagePackSerializer.Serialize(S2SPacketEncodingStream, requestData);

            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo
            {
                ID = (UInt16)ServerCommon.MQ.PacketID.ReqLobbyEnter,
                SenderIndex = myServerIndex,
                LobbyNumber = lobbyNum,
                UID = UniqueID
            };
            mqHeader.Write(S2SPacketBuffer);

            var length = (int)S2SPacketEncodingStream.Position;
            return (length, S2SPacketBuffer);
        }

        public (int, byte[]) MakeRequestLobbyLeaveMQPacket(UInt16 myServerIndex, bool isDisConnected)
        {
            var requestData = new ServerCommon.MQ.ReqLobbyLeave()
            {
                IsDisConnected = isDisConnected,
            };

            S2SPacketEncodingStream.Position = 0;
            MessagePack.MessagePackSerializer.Serialize(S2SPacketEncodingStream, requestData);

            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo
            {
                ID = (UInt16)ServerCommon.MQ.PacketID.ReqLobbyLeave,
                SenderIndex = myServerIndex,
                LobbyNumber = LobbyNum,
                UID = UniqueID
            };
            mqHeader.Write(S2SPacketBuffer, 0);

            var length = (int)S2SPacketEncodingStream.Position;
            return (length, S2SPacketBuffer);
        }

        public (int, byte[]) MakeRequestLobbyRelayMQPacket(UInt16 myServerIndex, byte[] packetData)
        {
            S2SPacketEncodingStream.Position = ServerCommon.MQ.PacketHeaderInfo.HeadSize;
            S2SPacketEncodingStream.Write(packetData, 0, packetData.Length);
            
            var mqHeader = new ServerCommon.MQ.PacketHeaderInfo
            {
                ID = ServerCommon.MQ.PacketID.ReqLobbyRelay,
                SenderIndex = myServerIndex,
                LobbyNumber = LobbyNum,
                UID = UniqueID
            };
            mqHeader.Write(S2SPacketBuffer);

            var length = (int)S2SPacketEncodingStream.Position;
            return (length, S2SPacketBuffer);
        }
    }

   
}
