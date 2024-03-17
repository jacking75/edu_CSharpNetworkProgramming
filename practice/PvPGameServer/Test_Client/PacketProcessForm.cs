using MessagePack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csharp_test_client
{
    public partial class mainForm
    {
        Dictionary<PACKET_ID, Action<byte[]>> PacketFuncDic = new Dictionary<PACKET_ID, Action<byte[]>>();
        void SetPacketHandler()
        {
            PacketFuncDic.Add(PACKET_ID.PACKET_ID_ECHO, PacketProcess_Echo);
            PacketFuncDic.Add(PACKET_ID.RES_LOGIN, PacketProcess_LoginResponse);
            PacketFuncDic.Add(PACKET_ID.RES_ROOM_ENTER, PacketProcess_RoomEnterResponse);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_USER_LIST, PacketProcess_RoomUserListNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_NEW_USER, PacketProcess_RoomNewUserNotify);
            PacketFuncDic.Add(PACKET_ID.RES_ROOM_LEAVE, PacketProcess_RoomLeaveResponse);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_LEAVE_USER, PacketProcess_RoomLeaveUserNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_CHAT, PacketProcess_RoomChatNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_READY_OMOK, PacketProcess_ReadyOmokNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_START_OMOK, PacketProcess_StartOmokNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_PUT_MOK, PacketProcess_PutMokNotify);
            PacketFuncDic.Add(PACKET_ID.RES_PUT_MOK, PacketProcess_PutMokResponse);
            PacketFuncDic.Add(PACKET_ID.NTF_END_OMOK, PacketProcess_EndOmokNotify);
        }

        void PacketProcess(byte[] packet)
        {
            var header = new MsgPackPacketHeadInfo();
            header.Read(packet);

            var packetType = (PACKET_ID)header.Id;
            //DevLog.Write("Packet Error:  PacketID:{packet.PacketID.ToString()},  Error: {(ERROR_CODE)packet.Result}");
            //DevLog.Write("RawPacket: " + packet.PacketID.ToString() + ", " + PacketDump.Bytes(packet.BodyData));

            if (PacketFuncDic.ContainsKey(packetType))
            {
                PacketFuncDic[packetType](packet);
            }
            else
            {
                DevLog.Write("Unknown Packet Id: " + packetType);
            }         
        }

        void PacketProcess_Echo(byte[] bodyData)
        {
            DevLog.Write($"Echo 받음:  {bodyData.Length}");
        }

        void PacketProcess_LoginResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResLogin>(packetData);
            DevLog.Write($"로그인 결과: {(ErrorCode)responsePkt.Result}");
        }


        void PacketProcess_RoomEnterResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResRoomEnter>(packetData);

            DevLog.Write($"방 입장 결과: {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomUserListNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfRoomUserList>(packetData);

            for (int i = 0; i < responsePkt.UserIDList.Count; ++i)
            {
                AddRoomUserList(responsePkt.UserIDList[i]);
            }

            DevLog.Write($"방의 기존 유저 리스트 받음: {responsePkt.UserIDList.Count}명");
        }

        void PacketProcess_RoomNewUserNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfRoomNewUser>(packetData);

            AddRoomUserList(responsePkt.UserID);

            DevLog.Write($"방에 새로 들어온 유저 받음");
        }

        void PacketProcess_RoomLeaveResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResRoomLeave>(packetData);

            LeaveRoom();
            
            DevLog.Write($"방 나가기 결과:  {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomLeaveUserNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packetData);

            RemoveRoomUserList(responsePkt.UserID);

            DevLog.Write($"방에서 나간 유저 받음");
        }
        
        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfRoomChat>(packetData);

            AddRoomChatMessageList(responsePkt.UserID, responsePkt.ChatMessage);
        }
        
        void PacketProcess_ReadyOmokNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfReadyOmok>(packetData);
            SetRoomReady(responsePkt.UserID, responsePkt.IsReady);
        }
        
        void PacketProcess_StartOmokNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfStartOmok>(packetData);
            CurTurn = 1;
            DevLog.Write($"선턴 유저: {responsePkt.FirstUserID}");
        }
        
        void PacketProcess_PutMokResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResPutMok>(packetData);
            DevLog.Write($"{(ErrorCode)responsePkt.Result}");
        }
        void PacketProcess_PutMokNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfPutMok>(packetData);

            if ((Mok) responsePkt.Mok != Mok.Skip)
            {
                PanelPainter.Input(responsePkt.PosX, responsePkt.PosY, (Mok) responsePkt.Mok);
                DrawMok();
            }

            CurTurn++;
            DevLog.Write($"좌표: ({responsePkt.PosX}, {responsePkt.PosY}), 색상: {responsePkt.Mok}");
        }
        
        void PacketProcess_EndOmokNotify(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTNtfEndOmok>(packetData);

            EndOmok();

            if (responsePkt.WinUserID == String.Empty)
            {
                DevLog.Write($"오목 종료!, 무승부");    
            }
            else
            {
                DevLog.Write($"오목 종료!, 승리 유저: {responsePkt.WinUserID}");    
            }
        }
    }
}
