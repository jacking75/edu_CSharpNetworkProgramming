using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public class ServerOption
    {
        public int MaxConnectionCount { get; set; } = 10000;
        public int ReserveClosingWaitMilliSecond { get; set; } = 100;
        public int ReceiveSecondaryBufferSize { get; set; } = 4012;
                
        public int ClientReceiveBufferSize { get; set; } = 4096;
        public int ClientMaxPacketSize { get; set; } = 1024;        
        public int ClientSendPacketMTU { get; set; } = 1024;
        public int ServerSendPacketMTU { get; set; } = 1024;


        // 아직 구현이 완전하지 않음
        public UInt16 ClientHeartBeatIntervalSec { get; set; } = 0; // 0 이면 허트비트 사용하지 않음
        public UInt16 ClientHeartBeatMaxWaitTimeSec { get; set; } = 0; // 허트 비트 최대 대기 시간(초). 이 시간 이상으로 허트비트가 안 오면 네트워크 이상


        public int SendPacketMTUSize(bool isClient)
        {
            return isClient ? ClientSendPacketMTU : ServerSendPacketMTU;
        }

        public void OutToConsole()
        {
            Console.WriteLine("[ ServerOption ]");
            Console.WriteLine($"MaxConnectionCount: {MaxConnectionCount}");
            Console.WriteLine($"ReserveClosingWaitMilliSecond: {ReserveClosingWaitMilliSecond}");
            Console.WriteLine($"ReceiveSecondaryBufferSize: {ReceiveSecondaryBufferSize}");
            Console.WriteLine($"ClientReceiveBufferSize: {ClientReceiveBufferSize}");
            Console.WriteLine($"ClientMaxPacketSize: {ClientMaxPacketSize}");
            Console.WriteLine($"ClientSendPacketMTU: {ClientSendPacketMTU}");
            Console.WriteLine($"ClientHeartBeatIntervalSec: {ClientHeartBeatIntervalSec}");
            Console.WriteLine($"ClientHeartBeatMaxWaitTimeSec: {ClientHeartBeatMaxWaitTimeSec}");
            Console.WriteLine($"ServerSendPacketMTU: {ServerSendPacketMTU}");
        }
    }
    
}
