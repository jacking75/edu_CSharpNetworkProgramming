using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PvPGameServer.Rooms
{
    public class Manager
    {
        List<Room> RoomList = new List<Room>();
        
        Tuple<int,int> RoomNumberRange = new Tuple<int, int>(-1, -1);

        int RoomStartNumber;
        
        bool IsThreadRunning;
        Thread TimeoutThread;
                
        int StartCheckedRoomNumbber = 0;
        const int NumberOfRoomsCheckedAtOnce = 100;
        const Int32 RoomCheckIntervalTimeMillSec = 100;


        public void Init(ServerOption serverOpt)
        {
            var maxRoomCount = serverOpt.RoomMaxCount;
            var startNumber = serverOpt.RoomStartNumber;
            var maxUserCount = serverOpt.RoomMaxUserCount;

            for(int i = 0; i < maxRoomCount; ++i)
            {
                var roomNumber = (startNumber + i);
                var room = new Room();
                room.Init(i, roomNumber, maxUserCount, serverOpt.TurnTimeout);

                RoomList.Add(room);
            }
            
            var minRoomNum = RoomList[0].Number;
            var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
            RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);
            MainServer.GlobalLogger.Info($"방 번호 범위: {minRoomNum} ~ {maxRoomNum}");

            RoomStartNumber = RoomList[0].Number;

            // 시간 체크 스레드 시작
            IsThreadRunning = true;
            TimeoutThread = new Thread(Update);
            TimeoutThread.Start();
        }

        public Room GetRoom(int roomNumber)
        {
            var index = roomNumber - RoomStartNumber;

            if(index < 0 || index >= RoomList.Count)
            {
                return null;
            }

            return RoomList[index];
        }

        public void Destory()
        {
            if (IsThreadRunning)
            {
                IsThreadRunning = false;
                TimeoutThread.Join();
            }
        }

        void Update()
        {
            while (IsThreadRunning)
            {
                Update_Impl();

                Thread.Sleep(RoomCheckIntervalTimeMillSec);
            }
        }

        void Update_Impl()
        {
            var curTime = DateTime.Now;
            var curTimeMillSec = (Int64)new TimeSpan(curTime.Ticks).TotalMilliseconds;

            var beginNum = StartCheckedRoomNumbber;
            var endNum = beginNum + NumberOfRoomsCheckedAtOnce;

            if (endNum > RoomList.Count)
            {
                endNum = RoomList.Count;
                StartCheckedRoomNumbber = 0;
            }
            else
            {
                StartCheckedRoomNumbber = endNum;
            }


            for (; beginNum < endNum; ++beginNum )
            {
                var room = RoomList[(int)beginNum];

                if (room.CheckRoomGameStartTimeOverThenCancel(curTimeMillSec))
                {
                    continue;
                }

                if (!room.IsPlayingOmok())
                {
                    continue;
                }

                if(room.CheckTurnTimeOverThenAutoPass(curTime))
                {
                    continue;
                }                
            }            
        }
    }
}
