using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNetLite;

public class ServerOption
{
    public int Port { get; set; } = 32451;
    public int MaxConnectionCount { get; set; } = 1000;
    public int ReceiveBufferSize { get; set; } = 4096;
    public int MaxPacketSize { get; set; } = 1024;        


          
    public void WriteConsole()
    {
        Console.WriteLine("[ ServerOption ]");
        Console.WriteLine($"Port: {Port}");
        Console.WriteLine($"MaxConnectionCount: {MaxConnectionCount}");
        Console.WriteLine($"ReceiveBufferSize: {ReceiveBufferSize}");
        Console.WriteLine($"MaxPacketSize: {MaxPacketSize}");
    }
}

