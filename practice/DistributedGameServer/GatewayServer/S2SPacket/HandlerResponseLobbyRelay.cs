using CSCommon;
using ServerCommon;
using ServerCommon.MQ;

using ZLogger;
using MessagePack;

using System;

namespace GatewayServer.S2SPacket
{
    public partial class Handler
    {
        public void ResponseLobbyRelay(PacketHeaderInfo mqHeader, byte[] packet)
        {
            var multiDataOffset = mqHeader.MultiUserDataOffset;
            
            if (multiDataOffset == 0)
            {
                var csPacketSize = packet.Length - PacketHeaderInfo.HeadSize;
                var csPacketBytes = BinaryUtil.Clone(packet, PacketHeaderInfo.HeadSize, csPacketSize);

                var session = ConnSessionMgrRef.GetSession(mqHeader.UID);
                if (session == null)
                {
                    return;
                }

                SendNetworkFunc(session.NetSessionID, csPacketBytes);
            }
            else
            {
                var multiDataSize = packet.Length - multiDataOffset;
                var multiData = BinaryUtil.Clone(packet, multiDataOffset, multiDataSize);
                var usersData = MessagePackSerializer.Deserialize<GWUserUniqueIdList>(multiData);

                var csPacketSize = packet.Length - PacketHeaderInfo.HeadSize - multiDataSize;
                var csPacketBytes = BinaryUtil.Clone(packet, PacketHeaderInfo.HeadSize, csPacketSize);

                foreach (var userUniqueID in usersData.UserUniqueIdList)
                {
                    var session = ConnSessionMgrRef.GetSession(userUniqueID);
                    if (session == null)
                    {
                        continue;
                    }

                    SendNetworkFunc(session.NetSessionID, csPacketBytes);
                }
            }            
        }

    }
}
