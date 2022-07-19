using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public class ServerOption
    {
        public int Port { get; set; } = 32451;
        public int MaxConnectionCount { get; set; } = 10000;
        public int ReserveClosingWaitMilliSecond { get; set; } = 256;                
        public int ReceiveBufferSize { get; set; } = 4096;
        public int MaxPacketSize { get; set; } = 1024;        


        // 아직 구현이 완전하지 않음
        public UInt16 HeartBeatIntervalSec { get; set; } = 0; // 0 이면 허트비트 사용하지 않음
        public UInt16 HeartBeatMaxWaitTimeSec { get; set; } = 0; // 허트 비트 최대 대기 시간(초). 이 시간 이상으로 허트비트가 안 오면 네트워크 이상

              
        public void WriteConsole()
        {
            Console.WriteLine("[ ServerOption ]");
            Console.WriteLine($"Port: {Port}");
            Console.WriteLine($"MaxConnectionCount: {MaxConnectionCount}");
            Console.WriteLine($"ReserveClosingWaitMilliSecond: {ReserveClosingWaitMilliSecond}");
            Console.WriteLine($"ReceiveBufferSize: {ReceiveBufferSize}");
            Console.WriteLine($"MaxPacketSize: {MaxPacketSize}");
            Console.WriteLine($"HeartBeatIntervalSec: {HeartBeatIntervalSec}");
            Console.WriteLine($"HeartBeatMaxWaitTimeSec: {HeartBeatMaxWaitTimeSec}");            
        }
    }
    
}
