using MySqlConnector;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks.Dataflow;

using ServerCommon;


namespace DBServer
{
    class PacketProcessor
    {
        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();

        Dictionary<UInt16, PKHandler.Base> PacketHandlerMap = new Dictionary<UInt16, PKHandler.Base>();
              
        public static Action<int, byte[]> MQSendFunc;
        
        MySqlConnection MysqlConnection = new MySqlConnection(DBServer.ServerOption.MySqlGameConnectionString);

        Cache.Manager CacheMgrRef;
        
        public void CreateAndStart(Cache.Manager cacheMgr)
        {
            CacheMgrRef = cacheMgr;

            RegistPacketHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }
        
        public void Destory()
        {
            IsThreadRunning = false;
            MsgBuffer.Complete();
            ProcessThread.Join();
        }


        public void InsertMsg(byte[] mqData) => MsgBuffer.Post(mqData);
          
        
        void Process()
        {
            MysqlConnection.Open();
            while (IsThreadRunning)
            {
                try
                {
                    var mqData = MsgBuffer.Receive();
                    var mqHeader = new MQPacketHeadInfo();
                    mqHeader.Read(mqData);

                    var mqId = mqHeader.Id;

                    if (PacketHandlerMap.ContainsKey(mqId))
                    {
                        PacketHandlerMap[mqId].Process(mqHeader, mqData);
                    }
                    else
                    {
                        Program.GlobalLogger.LogError($"Invalid mqId: {mqId}");
                    }
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => Console.WriteLine(ex.ToString()));
                }
            }
            MysqlConnection.Close();
        }

        void RegistPacketHandler()
        {
            PacketHandlerMap.Add((UInt16)MqPacketId.MQ_REQ_LOAD_USER_GAME_DATA, new PKHandler.LoadUserGameData(MysqlConnection, CacheMgrRef));
            PacketHandlerMap.Add((UInt16)MqPacketId.MQ_REQ_BUY_ITEM, new PKHandler.BuyItem(MysqlConnection, CacheMgrRef));
            PacketHandlerMap.Add((UInt16)MqPacketId.MQ_REQ_CHANGE_QUICKSLOT, new PKHandler.ChangeQuickSlot(MysqlConnection, CacheMgrRef));
        } 
    }
}
