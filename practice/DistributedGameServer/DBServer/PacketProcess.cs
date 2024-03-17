using MessagePack;
using ServerCommon;
using Microsoft.Extensions.Logging;
using ZLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer
{
    class PacketProcess
    {
        #region member
        public static readonly ILogger<PacketProcess> Logger = LogManager.GetLogger<PacketProcess>();

        Dictionary<UInt16, PKHandler.Base> PacketHandlerMap = new ();

        Action<Int32, string, byte[], int> MQSendFunc;

        ConcurrentQueue<(int, byte[])> WorkQueue = new();

        UInt16 MyServerIndex = 0;
        
        bool IsThreadRunning;
        System.Threading.Thread ProcessThread = null;

        DBMysql SQLDB = new DBMysql();
        DBRedis RedisDB = new DBRedis();
        #endregion


        public void Init(ServerOption serverOpt, Action<Int32, string, byte[], int> mqSendFunc)
        {
            MyServerIndex = (UInt16)serverOpt.ServerIndex;
            MQSendFunc = mqSendFunc;

            //TODO mysql은 현재 미 사용 상태로
            //SQLDB.Init(serverOpt.DBConnString);
            RedisDB.Init(serverOpt.RedisAddres);

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();

            RegistPacketHandler();
        }

        public void Destory()
        {
            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "[Destory] -  begin");

            if (IsThreadRunning)
            {
                IsThreadRunning = false;
                ProcessThread.Join();
            }

            Logger.ZLogInformation(new EventId(LogEventID.DBProgramEnd), "[Destory] -  end");
        }

        void RegistPacketHandler()
        {
            PKHandler.Base.MQSendFunc = MQSendFunc;

            PacketHandlerMap.Add((UInt16)ServerCommon.MQ.PacketID.ReqGatewayLogin, new PKHandler.GatewayLogin(SQLDB, RedisDB));
            PacketHandlerMap.Add((UInt16)ServerCommon.MQ.PacketID.NtfGatewayLogout, new PKHandler.GatewayLogout(SQLDB, RedisDB));

        }

        public void AddReqData(int index, byte[] data)
        {
            WorkQueue.Enqueue((index, data));
        }

        void Process()
        {
            var packetParams = new PKHandler.PacketDataParams();
            packetParams.MyServerIndex = MyServerIndex;
            packetParams.EncodingBuffer = new byte[8012 * 2];
            packetParams.EncodingStream = new System.IO.MemoryStream(packetParams.EncodingBuffer);
            packetParams.MQHeader = new ServerCommon.MQ.PacketHeaderInfo();

            while (IsThreadRunning)
            {
                try
                {
                    ProcessWorkerQueue(packetParams);
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => Console.WriteLine(ex.ToString()));
                }
            }
        }

        void ProcessWorkerQueue(PKHandler.PacketDataParams para)
        {
            try
            {
                if (WorkQueue.TryDequeue(out var work))
                {
                    para.MQHeader.Read(work.Item2);
                    para.MQIndex = work.Item1;
                    para.MQData = work.Item2;

                    var mqID = para.MQHeader.ID;

                    if (PacketHandlerMap.ContainsKey(mqID))
                    {
                        para.EncodingStream.Position = 0;
                        PacketHandlerMap[mqID].Process(para);
                    }
                    else
                    {
                        Logger.ZLogError(new EventId(LogEventID.DBLogicErrorProcessWorkerQueue), $"[ProcessWorkerQueue] Invlid MqID:{mqID}");
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                if (IsThreadRunning)
                {
                    Logger.ZLogError(ex, "");
                }
            }
        }
                     
        

    }
}
