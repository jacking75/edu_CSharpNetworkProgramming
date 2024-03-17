using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;
using PvPGameServer.Redis;


namespace PvPGameServer
{
    class PacketProcessor
    {
        bool IsThreadRunning = false;
        Thread ProcessThread = null;
        Thread UpdateThread = null;

        public Func<string, byte[], bool> NetSendFunc;
        public Action<EFBinaryRequestInfo> DistributePacketFunc;
        public Func<string, bool> ForcedCloseSessionFunc;

        //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        BufferBlock<EFBinaryRequestInfo> MsgBuffer = new ();

        Redis.TaskWorker RedisWorker = new ();
        Redis.MsgWorker RedisMsgWorker = new();
        
        DB.TaskWorker DBWorker = new();

        UserManager UserMgr = new ();

        Rooms.Manager RoomMgr = new ();
        
        PKHandler.Process PacketProcessHandler = new();
                

        public void CreateAndStart(ServerOption serverOpt)
        {
            RedisWorker.DistributePacketFunc = DistributePacketFunc;
            RedisWorker.Start(serverOpt);

            RedisMsgWorker.DistributePacketFunc = DistributePacketFunc;
            RedisMsgWorker.Start(serverOpt);

            DBWorker.DistributePacketFunc = DistributePacketFunc;
            DBWorker.Start(serverOpt);

            
            var maxUserCount = serverOpt.RoomMaxCount * serverOpt.RoomMaxUserCount;
            UserMgr.Init(maxUserCount);


            Rooms.Room.NetSendFunc = NetSendFunc;
            Rooms.Room.DistributePacketFunc = DistributePacketFunc;
            Rooms.Room.PushRedisTaskFunc = RedisWorker.PushTask;
            Rooms.Room.PushDBTaskFunc = DBWorker.PushTask;
            RoomMgr.Init(serverOpt);
            
            
            InitPacketHandler();


            IsThreadRunning = true;
            ProcessThread = new Thread(this.Process);
            ProcessThread.Start();

            UpdateThread = new Thread(this.Update);
            UpdateThread.Start();
        }

        public void Destroy()
        {
            MainServer.GlobalLogger.Info("PacketProcessor::Destroy - begin");

            IsThreadRunning = false;
            MsgBuffer.Complete();

            RoomMgr.Destory();

            UpdateThread.Join();
            ProcessThread.Join();
            
            RedisMsgWorker.Destroy();
            RedisWorker.Destroy();
            DBWorker.Destroy();

            MainServer.GlobalLogger.Info("PacketProcessor::Destroy - end");
        }


        public void InsertPacket(EFBinaryRequestInfo data)
        {
            MsgBuffer.Post(data);
        }

        void InitPacketHandler()
        {
            PKHandler.Process.NetSendFunc = NetSendFunc;
            PKHandler.Process.DistributePacketFunc = DistributePacketFunc;
            PKHandler.Process.ForcedCloseSessionFunc = ForcedCloseSessionFunc;
            PKHandler.Process.PushRedisTaskFunc = RedisWorker.PushTask;
            PKHandler.Process.PushDBTaskFunc = DBWorker.PushTask;

            PacketProcessHandler.Init(UserMgr, RoomMgr);
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    var packet = MsgBuffer.Receive();

                    var header = new MsgPackPacketHeaderInfo();
                    header.Read(packet.Data);

                    if(PacketProcessHandler.Execute(header.ID, packet) == false)
                    {
                        MainServer.GlobalLogger.Warn($"존재하지 않는 Packet ID: {header.ID}");
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


        void Update()
        {
            while (IsThreadRunning)
            {
                SendInnerNtfUsersCheckStatePacket();

                Thread.Sleep(128);
            }
        }

        void SendInnerNtfUsersCheckStatePacket()
        {
            var packet = new EFBinaryRequestInfo(null);
            packet.Data = new byte[MsgPackPacketHeaderInfo.HeadSize];

            MsgPackPacketHeaderInfo.WritePacketID(packet.Data, 
                (UInt16)Enum.PacketID.NTF_IN_USERS_CHECK_STATE);

            InsertPacket(packet);
        }
    }
}
