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
        void HandlerNtfInnerDisConnectedClient(EFBinaryRequestInfo requestData)
        {
            var sessionID = requestData.SessionID;
            var user = UserMgr.GetUser(sessionID);
            if (user == null)
            {
                return;
            }

            var roomNum = user.RoomNumber;
            if (roomNum >= 0)
            {
                SendNtfInnerRoomLeavePacket(roomNum, user.UserID, sessionID);
            }

            UserMgr.RemoveUser(sessionID);
        }


        void SendNtfInnerRoomLeavePacket(int roomNumber, string userID, string sessionID)
        {
            var ntfData = new PKTInternalNtfRoomLeave()
            {
                RoomNumber = roomNumber,
                UserID = userID,
            };

            var sendData = MessagePackSerializer.Serialize(ntfData);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)Enum.PacketID.NTF_IN_ROOM_LEAVE, 0);

            var packet = new EFBinaryRequestInfo(sendData);
            packet.SessionID = sessionID;

            DistributePacketFunc(packet);
        }
    }
}
