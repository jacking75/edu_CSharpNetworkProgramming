using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudStructures;
using CloudStructures.Structures;
using MessagePack;
using ServerCommon.Redis;

namespace PvPGameServer.Redis
{
    public class MsgWorker
    {
        RedisConnection Connection;

        public Action<EFBinaryRequestInfo> DistributePacketFunc;
        
        bool IsThreadRunning;
        System.Threading.Thread ProcessThread;

        ServerOption ServerOpt;

        string RedisKey;

        Dictionary<int, Action<byte[]>> MsgHandlerMap = new ();


        public void Start(ServerOption serverOption)
        {
            ServerOpt = serverOption;

            RedisKey = KeyDefine.PrefixMessageToGameServer + ServerOpt.ServerUniqueID;

            var config = new RedisConfig("MsgWorker", ServerOpt.RedisAddress);
            Connection = new RedisConnection(config);


            DeleteMessageQueue();

            RegistMsgHandler();


            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }

        public void Destroy()
        {
            MainServer.GlobalLogger.Info("MsgWorker::Destroy - begin");

            IsThreadRunning = false;
            Connection.GetConnection().Close();

            ProcessThread.Join();

            MainServer.GlobalLogger.Info("MsgWorker::Destroy - end");
        }

        void RegistMsgHandler()
        {
            MsgHandlerMap.Add((int)MsgID.NewMatcingRoom, RequestNewMatchingRoom);
        }

        void DeleteMessageQueue()
        {
            var redis = new RedisList<byte[]>(Connection, RedisKey, null);
            var ret = redis.DeleteAsync().Result;
        }

        public void SendInnerPacket(string netSessionID, Enum.PacketID packetId, byte[] sendData)
        {
            MsgPackPacketHeaderInfo.Write(sendData, (UInt16)sendData.Length, (UInt16)packetId, 0);

            var packet = new EFBinaryRequestInfo(sendData);
            packet.SessionID = netSessionID;

            DistributePacketFunc(packet);
        }

        void Process()
        {
            var redis = new RedisList<byte[]>(Connection, RedisKey, null);

            while (IsThreadRunning)
            {
                try
                {
                    var msgData = redis.RightPopAsync().Result;
                    if(msgData.HasValue == false)
                    {
                        System.Threading.Thread.Sleep(32);
                        continue;
                    }

                    var msgID = MsgPackHeaderInfo.ReadID(msgData.Value);

                    if (MsgHandlerMap.ContainsKey(msgID))
                    {
                        MsgHandlerMap[msgID](msgData.Value);
                    }
                    else
                    {
                        MainServer.GlobalLogger.Error($"Invalid MsgID: {msgID}");
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

        void RequestNewMatchingRoom(byte[] msgData)
        {
            var request = MessagePackSerializer.Deserialize<NewMatchingRoom>(msgData);

            var response = new PKTNtfRedisMatchingRoom();
            response.RoomNum = request.RoomNumber;

            var sendData = MessagePackSerializer.Serialize(response);
            SendInnerPacket("redisTo", Enum.PacketID.NTF_REDIS_NEW_MATCHING_ROOM, sendData);

            MainServer.GlobalLogger.Debug($"[RequestNewMatchingRoom] Room: {request.RoomNumber}");
        }

      

        
    }
  
}