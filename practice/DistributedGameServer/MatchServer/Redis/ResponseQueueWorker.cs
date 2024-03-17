using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using System.Threading.Tasks;
using ServerCommon.Redis;
using ServerCommon;

namespace MatchServer.Redis
{
    public class ResponseQueueWorker
    {
        RedisConnection Connection;

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        ILogger Logger;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();

        PvPGameServerGroup GameServerGroup;

        TokenGenerator TokenGen = new ();

        string RedisAddress;
        
        public bool Start(ILogger logger, string redisAddress)
        {
            RedisAddress = redisAddress;
            Logger = logger;

            var config = new RedisConfig("output", redisAddress);
            Connection = new RedisConnection(config);


            if (ReadPvPGameServerList(RedisAddress) == false)
            {
                Logger.LogError("ResponseQueueWorker::Start - Fail Read PvPGameServerList");
                return false;
            }
            

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();

            Logger.LogInformation($"ResponseQueueWorker Started. address:{redisAddress}");
            return true;
        }

        public void Destroy()
        {
            Logger.LogInformation("ResponseQueueWorker::Destroy - begin");

            IsThreadRunning = false;
            MsgBuffer.Complete();

            ProcessThread.Join();

            Logger.LogInformation("ResponseQueueWorker::Destroy - end");
        }

        bool ReadPvPGameServerList(string redisAddress)
        {
            try
            {
                if (Connection == null)
                {
                    var config = new RedisConfig("read", redisAddress);
                    Connection = new RedisConnection(config);
                }

                var redis = new RedisString<PvPGameServerGroup>(Connection, KeyDefine.PvPGameServerList, null);

                var infoList = redis.GetAsync().Result;
                if (infoList.HasValue == false)
                {
                    return false;
                }
                GameServerGroup = infoList.Value;

                foreach (var server in GameServerGroup.ServerList)
                {
                    Logger.LogInformation($"GameServer: IP:{server.IP}, Port:{server.Port}");
                }
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError($"[ReadPvPGameServerList] err :{ex.Message}");
                return false;
            }
        }

        public void AddMatchingResult(byte[] data)
        {
            MsgBuffer.Post(data);
        }


        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    Process_impl();
                }
                catch (Exception ex)
                {
                    if (IsThreadRunning)
                    {
                       Logger.LogError(ex.ToString());
                    }
                }
            }
        }

        void Process_impl()
        {
            var reqData = MsgBuffer.Receive();
            var msgID = (MsgID)MsgPackHeaderInfo.ReadID(reqData);

            if (msgID == MsgID.ResponseMatching)
            {
                if (ProcessResponseMatching(reqData) == false)
                {
                    // 매칭할 room이 없으므로 뒤에 다시 배정할 수 있게 큐에 넣는다.
                    AddMatchingResult(reqData);
                }
            }
            else if (msgID == MsgID.ReloadGameServerInfo)
            {
                if (ReadPvPGameServerList(RedisAddress) == false)
                {
                    Logger.LogError("ResponseQueueWorker::ProcessImpl - Fail Read PvPGameServerList");
                }
            }
        }

        bool ProcessResponseMatching(byte[] userMatchingResultData)
        {
            var userMatchingResult = MessagePackSerializer.Deserialize<UserPvPMatchingResult>(userMatchingResultData);

            var (takeRoomRet, user1Info, user2Info) = TakeGameRoom();
            if (takeRoomRet == false)
            {
                Logger.LogDebug("게임을 할 수 있는 방이 없음");
                return false;
            }

            var serverMatchingResultBytes1 = MessagePackSerializer.Serialize(user1Info);
            var serverMatchingResultBytes2 = MessagePackSerializer.Serialize(user2Info);

            //요청자에게 알려준다.
            var task1 = MatchingResultToUser(userMatchingResult.UserList[0], serverMatchingResultBytes1);
            var task2 = MatchingResultToUser(userMatchingResult.UserList[1], serverMatchingResultBytes2);

            //게임서버에게 알려준다.
            var task3 = MatchingResultToGameServer(user1Info.Index, user1Info.RoomNumber);
            
            Task.WaitAll(task1, task2, task3);

            Logger.LogDebug($"[Redis에 매칭 정보 등록] GameServer IP :{user1Info.IP}, Port: {user1Info.Port}, Room: {user1Info.RoomNumber}");
            return true;
        }

        Task<bool> MatchingResultToUser(string userID, byte[] serverMatchingResultBytes)
        {
            Logger.LogDebug($"ServerMatchingResult to RedisString, userID: {userID}");

            var key = KeyDefine.PrefixMatchingResult + userID;
            var redis = new RedisString<byte[]>(Connection, key, 
                        TimeSpan.FromMilliseconds(TimeConstant.MaximumWaitingTimeToStartGameMilliSec));            
            return redis.SetAsync(serverMatchingResultBytes);
        }

        Task<long> MatchingResultToGameServer(Int32 serverIndex, int roomNumber)
        {
            Logger.LogDebug($"ServerMatchingResult to RedisList, serverIndex: {serverIndex}");

            var msgData = new NewMatchingRoom
            {
                RoomNumber = roomNumber
            };
            var msgDataBytes = MessagePackSerializer.Serialize(msgData);
            MsgPackHeaderInfo.WriteID(msgDataBytes, MsgID.NewMatcingRoom);

            var key = KeyDefine.PrefixMessageToGameServer + serverIndex;
            var redis = new RedisList<byte[]>(Connection, key, null);            
            return redis.LeftPushAsync(msgDataBytes);
        }


        int RotationIndex = 0;
        (bool, PvPMatchingResult, PvPMatchingResult) TakeGameRoom()
        {
            var serverCount = GameServerGroup.ServerList.Count;
                        
            for(var i = 0; i < serverCount; ++i)
            {
                if (RotationIndex >= serverCount)
                {
                    RotationIndex = 0;
                }

                var serverInfo = GameServerGroup.ServerList[RotationIndex];
                ++RotationIndex;


                var key = KeyDefine.PrefixGameServerRoomQueue + serverInfo.Index;
                var redis = new RedisList<Int32>(Connection, key, null);
                var takeValue = redis.LeftPopAsync().Result;

                if(takeValue.HasValue)
                {
                    var ret1 = new PvPMatchingResult()
                    {
                        IP = serverInfo.IP,
                        Port = serverInfo.Port,
                        RoomNumber = takeValue.Value,
                        Index = serverInfo.Index,
                        Token = TokenGen.Password()
                    };

                    var ret2 = new PvPMatchingResult()
                    {
                        IP = serverInfo.IP,
                        Port = serverInfo.Port,
                        RoomNumber = takeValue.Value,
                        Index = serverInfo.Index,
                        Token = TokenGen.Password()
                    };

                    return (true, ret1, ret2);
                }
            }

            return (false, null, null);
        }

        
        
    }
}
