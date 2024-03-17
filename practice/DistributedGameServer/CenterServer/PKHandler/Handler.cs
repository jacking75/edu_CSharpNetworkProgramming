using Microsoft.Extensions.Logging;
using ZLogger;
using ServerCommon;

using System;

namespace CenterServer.PKHandler
{
    public partial class Handler
    {
        static readonly ILogger<Handler> Logger = LogManager.GetLogger<Handler>();

        public static ServerOption ServerOpt;

        public static Action<Int32, string, byte[], int> SendMqFunc;

        const int MaxPacketLength = 4096;
        protected byte[] MQPacketEnCodeBuffer = new byte[MaxPacketLength];
        protected System.IO.MemoryStream MQPacketEnCodeStream { get; private set; }


        public void Init()
        {
            MQPacketEnCodeStream = new System.IO.MemoryStream(MQPacketEnCodeBuffer);
        }
               

       
    }
}
