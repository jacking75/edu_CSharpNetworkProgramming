using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public class ServerOption
    {
        public int MaxConnectionCount = 10000;
        public int ReserveClosingWaitMilliSecond = 100;
        public int ReceiveSecondaryBufferSize = 4012;
                
        public int ClientReceiveBufferSize = 4096;
        public int ClientMaxPacketSize = 1024;        
        public int ClientSendPacketMTU = 1024;

        // 아직 구현이 완전하지 않음
        public UInt16 ClientHeartBeatIntervalSec = 0; // 0 이면 허트비트 사용하지 않음
        public UInt16 ClientHeartBeatMaxWaitTimeSec = 0; // 허트 비트 최대 대기 시간(초). 이 시간 이상으로 허트비트가 안 오면 네트워크 이상

        public int ServerSendPacketMTU = 4096;


        public int SendPacketMTUSize(bool isClient)
        {
            return isClient ? ClientSendPacketMTU : ServerSendPacketMTU;
        }
    }
    
}
