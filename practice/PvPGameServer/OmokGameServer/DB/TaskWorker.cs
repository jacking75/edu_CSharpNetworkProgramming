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
using MySqlConnector;

namespace PvPGameServer.DB
{
    public class TaskWorker
    {
        MySqlConnection MysqlConnection;

        public Action<EFBinaryRequestInfo> DistributePacketFunc;

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();

        Dictionary<int, TaskBase> TaskHandlerMap = new ();

        public void Start(ServerOption serverOpt)
        {
            //TODO: 테스트 편의를 위해 미사용 상태로
            MysqlConnection = new MySqlConnection(serverOpt.MySqlConnectionString);
            MysqlConnection.Open();

            RegistRedisTaskHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }

        public void Destroy()
        {
            MainServer.GlobalLogger.Info("DBWorker::Destroy - begin");

            if (IsThreadRunning)
            {
                IsThreadRunning = false;
                MsgBuffer.Complete();

                ProcessThread.Join();
            }

            MainServer.GlobalLogger.Info("DBWorker::Destroy - end");
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
                
        void RegistRedisTaskHandler()
        {
            TaskHandlerMap.Add((int)TaskID.NotifySaveGameResult, new TaskSaveGameResult());
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                Process_Impl();
            }
        }

        void Process_Impl()
        {
            try
            {
                var reqTask = MsgBuffer.Receive();
                var taskID = MsgPackHeaderInfo.ReadID(reqTask);

                if (TaskHandlerMap.ContainsKey(taskID))
                {
                    TaskHandlerMap[taskID].Process(MysqlConnection, reqTask);
                }
                else
                {
                    MainServer.GlobalLogger.Error($"Invalid TaskID: {taskID}");
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
}
