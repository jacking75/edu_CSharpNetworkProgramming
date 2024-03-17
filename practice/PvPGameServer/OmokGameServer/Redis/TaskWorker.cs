using CloudStructures;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;
using CloudStructures.Structures;
using PvPGameServer.Enum;
using ServerCommon.Redis;

namespace PvPGameServer.Redis
{
    public class TaskWorker
    {
        int ServerUniqueID = 0;

        RedisConnection Connection;

        public Action<EFBinaryRequestInfo> DistributePacketFunc;

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();

        Dictionary<int, Action<byte[]>> TaskHandlerMap = new Dictionary<int, Action<byte[]>>();

        public void Start(ServerOption serverOpt)
        {
            ServerUniqueID = serverOpt.ServerUniqueID;

            var config = new RedisConfig("gameServer", serverOpt.RedisAddress);
            Connection = new RedisConnection(config);

            RegisterServerRooms(serverOpt.ServerUniqueID, serverOpt.RoomStartNumber, serverOpt.RoomMaxCount);

            RegistRedisTaskHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }

        public void Destroy()
        {
            MainServer.GlobalLogger.Info("RedisWorker::Destroy - begin");

            IsThreadRunning = false;
            MsgBuffer.Complete();

            ProcessThread.Join();

            MainServer.GlobalLogger.Info("RedisWorker::Destroy - end");
        }

        public void PushTask(byte[] data)
        {
            MsgBuffer.Post(data);
        }

        public void SendInnerPacket(string netSessionID, PacketID packetId, byte[] sendData)
        {
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)packetId, 0);

            var packet = new EFBinaryRequestInfo(sendData);
            packet.SessionID = netSessionID;

            DistributePacketFunc(packet);
        }

        bool RegisterServerRooms(int serverIndex, int roomStartNum, int roomMaxCount)
        {
            var key = KeyDefine.PrefixGameServerRoomQueue + serverIndex;
            try
            {
                long result = 0;
                var redis = new RedisList<Int32>(Connection, key, null);
                var ret = redis.DeleteAsync().Result;

                for (var i = 0; i < roomMaxCount; i++)
                {
                    result = redis.RightPushAsync(roomStartNum + i).Result;
                }

                MainServer.GlobalLogger.Info($"방 등록 성공: Count={roomMaxCount}");
                return true;
            }
            catch (Exception e)
            {
                MainServer.GlobalLogger.Error("[방 등록 실패] " + e.Message);
                return false;
            }
        }

        void RegistRedisTaskHandler()
        {
            TaskHandlerMap.Add((int)MsgID.ReqLogin, RequestLogin);
            TaskHandlerMap.Add((int)MsgID.NtfRegistAvailableRoom, NotifyRegistAvilableRoom);
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    var reqTask = MsgBuffer.Receive();

                    var taskId = MsgPackHeaderInfo.ReadID(reqTask);

                    if (TaskHandlerMap.ContainsKey(taskId))
                    {
                        TaskHandlerMap[taskId](reqTask);
                    }
                    else
                    {
                        MainServer.GlobalLogger.Error($"Invalid TaskId: {taskId}");
                    }
                }
                catch (Exception ex)
                {
                    if (IsThreadRunning)
                    {
                        MainServer.GlobalLogger.Error(ex.ToString());
                    }
                }
            }
        }

        void RequestLogin(byte[] taskData)
        {
            var response = new PKTResRedisLogin();
            var request = MessagePackSerializer.Deserialize<ReqLoginTask>(taskData);

            var key = KeyDefine.PrefixMatchingResult + request.UserID;
            var redis = new RedisString<byte[]>(Connection, key, null);
            var matchingResultBytes = redis.GetAsync().Result;

            if (matchingResultBytes.HasValue == false)
            {
                response.Result = (short)ErrorCode.LOGIN_NOT_EXIST_CLIENT_SESSION;
            }
            else
            {
                var matchingResult = MessagePackSerializer.Deserialize<PvPMatchingResult>(matchingResultBytes.Value);

                if (matchingResult.Token != request.AuthToken)
                {
                    response.Result = (short)ErrorCode.LOGIN_INVALID_AUTHTOKEN;
                }
                else
                {
                    response.Result = (short)ErrorCode.None;
                    response.UserID = request.UserID;
                    response.RoomNum = matchingResult.RoomNumber;
                }
            }

            var sendData = MessagePackSerializer.Serialize(response);
            SendInnerPacket(request.NetSessionID, PacketID.REQ_REDIS_LOGIN_RESULT, sendData);
        }
        
        void NotifyRegistAvilableRoom(byte[] taskData)
        {            
            try
            {
                var notify = MessagePackSerializer.Deserialize<NtfRegistAvailableRoomTask>(taskData);
                var roomNumber = notify.RoomNumber;

                var key = KeyDefine.PrefixGameServerRoomQueue + ServerUniqueID;
                var redis = new RedisList<Int32>(Connection, key, null);
                var result = redis.RightPushAsync(roomNumber).Result;
 
                MainServer.GlobalLogger.Info($"사용 가능 방 등록: {roomNumber}");
            }
            catch (Exception e)
            {
                MainServer.GlobalLogger.Error("[사용 가능 방 등록 실패] " + e.Message);
            }
        }


    }
}
