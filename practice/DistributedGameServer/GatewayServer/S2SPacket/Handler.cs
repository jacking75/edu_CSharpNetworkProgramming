using ServerCommon;

using Microsoft.Extensions.Logging;
using ZLogger;

using System;


namespace GatewayServer.S2SPacket
{
    public partial class Handler
    {
        static readonly ILogger<Handler> Logger = ServerCommon.LogManager.GetLogger<Handler>();

        ConnSession.Manager ConnSessionMgrRef;
        MQSubjectManager MQSubjectMgrRef;

        public Func<string, byte[], bool> SendNetworkFunc;
        public Action<Int16, byte[], Int32, Int32> SendMQToLobby;
        public Action<Int32, byte[], Int32, Int32> SendMQToGame;
      

        public void Init(ConnSession.Manager connSessionMgr,
                            MQSubjectManager mqSubjectMgr)
        {
            ConnSessionMgrRef = connSessionMgr;
            MQSubjectMgrRef = mqSubjectMgr;
        }
               

       
    }
}
