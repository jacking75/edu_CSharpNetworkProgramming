using CommandLine;
using System;


var serverOpt = ParseCommandLine(args);

// IoThreadPacketDispatcher 에서 바로 패킷을 처리한다. 즉 멀티스레드로 패킷을 처리한다.
var packetDispatcher = new EchoServerIOThreadPacketProcess.IoThreadPacketDispatcher();
packetDispatcher.Init(EchoServerIOThreadPacketProcess.PacketDef.HeaderSize);

var service = new FreeNet.NetworkService(serverOpt);
service.Initialize();

bool isNoDelay = true;
service.Listen("0.0.0.0", serverOpt.Port, 100, isNoDelay);

Console.WriteLine("Started!");


while (true)
{
	//Console.Write(".");
	string input = Console.ReadLine();

	if (input.Equals("users"))
	{
		Console.WriteLine(service.SessionMgr.CurrentConnectdSessionCount());
	}
	else if (input.Equals("Exit Process"))
	{
		service.Stop();

		Console.WriteLine("Exit !!!");
		break;
	}

	System.Threading.Thread.Sleep(500);
}


//--port 32451 --max_conn_count 100 --recv_buff_size 4012 --max_packet_size 1024
FreeNet.ServerOption ParseCommandLine(string[] args)
{
	var result = Parser.Default.ParseArguments<ServerOption>(args) as Parsed<ServerOption>;
	if (result == null)
	{
		Console.WriteLine("Failed Command Line");
		return null;
	}

	var option = new FreeNet.ServerOption
	{
		MaxConnectionCount = result.Value.MaxConnectionCount,
		ReceiveBufferSize = result.Value.ReceiveBufferSize,
		MaxPacketSize = result.Value.MaxPacketSize
	};
	option.WriteConsole();

	return option;
}

public class ServerOption
{
	[Option("port", Required = true, HelpText = "Port Number")]
	public int Port { get; set; } = 32451;

	[Option("max_conn_count", Required = true, HelpText = "MaxConnectionCount")]
	public int MaxConnectionCount { get; set; } = 100;

	[Option("recv_buff_size", Required = true, HelpText = "ReceiveBufferSize")]
	public int ReceiveBufferSize { get; set; } = 4012;

	[Option("max_packet_size", Required = true, HelpText = "MaxPacketSize")]
	public int MaxPacketSize { get; set; } = 1024;

}
