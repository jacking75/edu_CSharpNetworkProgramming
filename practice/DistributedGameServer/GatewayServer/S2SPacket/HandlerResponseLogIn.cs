using CSCommon;
using MessagePack;
using ServerCommon;
using ServerCommon.MQ;
using System;

namespace GatewayServer.S2SPacket
{
    public partial class Handler
    {
        public void ResponseLogIn(PacketHeaderInfo mqHeader, byte[] packet)
        {
            var responseData = MessagePackSerializer.Deserialize<ResGatewayLogin>(packet);

            var session = ConnSessionMgrRef.GetSession(mqHeader.UID);
            if(session == null)
            {
                return;
            }

            if (responseData.Result == (Int16)SErrorCode.None)
            {
                session.LoginSuccess(responseData.UserID);

                SendResponseLoginToClient((Int16)ErrorCode.None, session.NetSessionID);
            }
            else
            {
                session.LoginFail();

                SendResponseLoginToClient(responseData.Result, session.NetSessionID);
            }
            
        }

        void SendResponseLoginToClient(Int16 result, string netSessionID)
        {
            var responseData = new PKTResLogin()
            {
                Result = result,
            };

            var packetData = MessagePackSerializer.Serialize(responseData);
            MsgPackPacketHeaderInfo.Write(packetData,
                                    (UInt16)packetData.Length, 
                                    CSCommon.PacketID.ResLogin);

            SendNetworkFunc(netSessionID, packetData);
        }
    }
}
