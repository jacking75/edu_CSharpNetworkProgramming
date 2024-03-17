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
        OmokBoard OmokBrd;

        User Player1;
        User Player2;

        public int TurnSkipCnt { get; private set; }

        public DateTime TurnExpiredTime { get; private set; }
       
        Int64 MatchingTime = Int64.MaxValue;
        
        int TurnTimeout;



        public void SetMatchingRoomTime()
        {
            // set, get만 하고 64비트 크기이므로 lock을 걸리 알아도 된다
            MatchingTime = (Int64)new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds + ServerCommon.TimeConstant.MaximumWaitingTimeToStartGameMilliSec;
        }

        public void Clear()
        {
            OmokBrd = null;
            TurnExpiredTime = DateTime.MaxValue;
            MatchingTime = Int64.MaxValue;

            // 대기 상태로 변경
            Player1?.EndOmok();
            Player2?.EndOmok();

            Player1 = null;
            Player2 = null;
        }

        public void NotifyReady(User user)
        {
            user.State = user.State == Rooms.UserState.IDLE ? Rooms.UserState.READY : Rooms.UserState.IDLE;

            
            var nftPacket = new PKTNtfReadyOmok
            {
                UserID = user.UserID,
                IsReady = user.State == UserState.READY
            };
            var sendData = MessagePackSerializer.Serialize(nftPacket);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_READY_OMOK, 0);

            Broadcast("", sendData);

            MainServer.GlobalLogger.Debug("Send: NtfReadyOmok");
        }

        public void NotifyStart(string firstUserID)
        {
            var nftPacket = new PKTNtfStartOmok
            {
                FirstUserID = firstUserID
            };
            var sendData = MessagePackSerializer.Serialize(nftPacket);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_START_OMOK, 0);

            Broadcast("", sendData);

            MainServer.GlobalLogger.Debug("Send: NtfStartOmok");
        }


        public bool CheckRoomGameStartTimeOverThenCancel(Int64 curTime)
        {
            if(MatchingTime > curTime)
            {
                return false;
            }

            MatchingTime = Int64.MaxValue;

            SendInnerNtfRoomGameEndPacket(true, Number, "none", "none");
            return true;
        }


        public bool CheckReadyUsers()
        {
            // 인원이 가득 찼는지 체크
            if (RoomUserList.Count != MaxUserCount)
            {
                return false;
            }

            // 모든 유저가 준비상태인지 체크
            foreach (var user in RoomUserList)
            {
                if (user.State != UserState.READY)
                {
                    return false;
                }
            }

            Player1 = RoomUserList[0];
            Player2 = RoomUserList[1];
            return true;
        }

        public string StartOmok()
        {
            if (Player1 == null || Player2 == null)
            {
                return string.Empty;
            }

            OmokBrd = new OmokBoard();
            OmokBrd.Start();

            TurnSkipCnt = 0;
            TurnExpiredTime = DateTime.Now.AddSeconds(TurnTimeout);

            // 선턴 정하기
            Random rand = new Random();
            var firstPlayerIndex = rand.Next(1, 2);

            // 게임중 상태로 변경 (선턴이 블랙)
            Player1.StartOmok(firstPlayerIndex == 1 ? Mok.Black : Mok.White);
            Player2.StartOmok(firstPlayerIndex == 2 ? Mok.Black : Mok.White);

            return firstPlayerIndex == 1 ? Player1.UserID : Player2.UserID;
        }

        public (string, string) EndOmok(OMokResult oMokResult)
        {            
            string winUserID = "", loseUserID = "";

            if (oMokResult == OMokResult.WinBlack)
            {
                winUserID = Player1?.UserID;
                loseUserID = Player2?.UserID;
            }
            else if (oMokResult == OMokResult.WinWhite)
            {
                winUserID = Player2?.UserID;
                loseUserID = Player1?.UserID;
            }
                                    
            return (winUserID, loseUserID);
        }

        public ErrorCode PutMok(Mok mok, int posX, int posY, int turn)
        {
            if (OmokBrd == null)
            {
                return ErrorCode.OMOK_NOT_STARTED;
            }

            // 목 두기
            var putErrorCode = OmokBrd.PutMok(mok, posX, posY, turn);
            if (putErrorCode != ErrorCode.None)
            {
                return putErrorCode;
            }

            TurnExpiredTime = DateTime.Now.AddSeconds(TurnTimeout);
            TurnSkipCnt = mok == Mok.Skip ? TurnSkipCnt + 1 : 0; // 턴 스킵인 경우

            var sendData = MessagePackSerializer.Serialize(new PKTNtfPutMok
            {
                PosX = posX,
                PosY = posY,
                Mok = (int)mok,
            });
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_PUT_MOK, 0);
            Broadcast("", sendData);
            MainServer.GlobalLogger.Debug("Send: NtfPutOmok");
            return ErrorCode.None;
        }

        public bool CheckEndOmok(Mok mok)
        {
            var oMokResult = OmokBrd.CheckWinMok(mok, TurnSkipCnt);
            if (oMokResult == OMokResult.None)
            {
                return false;
            }

            var (winUserID, loseUserID) = EndOmok(oMokResult);
            if (string.IsNullOrEmpty(winUserID) == false)
            {
                var ntfEndOmokData = MessagePackSerializer.Serialize(new PKTNtfEndOmok
                {
                    WinUserID = winUserID
                });
                MsgPackPacketHeaderInfo.Write(ntfEndOmokData, (UInt16)ntfEndOmokData.Length, (UInt16)PacketID.NTF_END_OMOK, 0);
                Broadcast("", ntfEndOmokData);
                MainServer.GlobalLogger.Debug("Send: NtfEndOmok");
            }


            SetGameStop();

            SendInnerNtfRoomGameEndPacket(true, Number, winUserID, loseUserID);
            return true;
        }

        void SetGameStop()
        {
            OmokBrd = null;
        }

        public bool IsPlayingOmok()
        {
            return OmokBrd != null;
        }

        public Tuple<int, string> GetCurTurnInfo()
        {
            if (OmokBrd == null)
            {
                return new Tuple<int, string>(0, string.Empty);
            }

            var curMok = OmokBrd.Turn % 2 == 0 ? Mok.White : Mok.Black;

            string sessionID = string.Empty;
            if ((Player1?.UserMok ?? Mok.None) == curMok)
            {
                sessionID = Player1?.NetSessionID;
            }
            else if ((Player2?.UserMok ?? Mok.None) == curMok)
            {
                sessionID = Player2?.NetSessionID;
            }

            return new Tuple<int, string>(OmokBrd.Turn, sessionID);
        }

        public bool CheckTurnTimeOverThenAutoPass(DateTime curTime)
        {
            if (curTime < TurnExpiredTime)
            {
                return false;
            }
               
            var (turn, sessionID) = GetCurTurnInfo();

            PutMok(Mok.Skip, 0, 0, turn);
            CheckEndOmok(Mok.Skip);

            return true;
        }

        public void RegistAvailableRoomToRedis()
        {
            var ntfTask = new ServerCommon.Redis.NtfRegistAvailableRoomTask
            {
                RoomNumber = Number,
            };

            var sendData = MessagePackSerializer.Serialize(ntfTask);
            ServerCommon.Redis.MsgPackHeaderInfo.WriteID(sendData, ServerCommon.Redis.MsgID.NtfRegistAvailableRoom);
            PushRedisTaskFunc(sendData);
        }

        public void SendInnerNtfRoomGameEndPacket(bool isFail, 
            int roomNumber, 
            string winUserID, 
            string loseUserID)
        {
            var reqPacket = new PKTInternalNtfRoomGameEnd
            {
                IsFail = isFail,
                RoomNumber = roomNumber,
                WinUserID = winUserID,
                LoseUserID = loseUserID
            };

            var sendData = MessagePackSerializer.Serialize(reqPacket);
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)PacketID.NTF_IN_ROOM_GAME_END, 0);

            var packet = new EFBinaryRequestInfo(sendData);
            DistributePacketFunc(packet);
        }
    }
}
