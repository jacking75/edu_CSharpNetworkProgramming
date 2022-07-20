using System;
using CommandLine;


var serverOpt = ParseCommandLine(args);

var packetDispatcher = new FreeNet.DefaultPacketDispatcher();
packetDispatcher.Init(EchoServer.PacketDef.HeaderSize);

var service = new FreeNet.NetworkService(serverOpt, packetDispatcher);
service.Initialize();

bool isNoDelay = true;
service.Listen("0.0.0.0", serverOpt.Port, 100, isNoDelay);

Console.WriteLine("Started!");


// 패킷 처리기 생성 및 실행
var packetProcess = new EchoServer.PacketProcess(service);
packetProcess.Start();


while (true)
{
	string input = Console.ReadLine();

	if (input.Equals("users"))
	{
		Console.WriteLine(service.SessionMgr.CurrentConnectdSessionCount());
	}
	else if (input.Equals("exit"))
	{
		Console.WriteLine("Exit Process  !!!");

		packetProcess.Stop();
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
		Port = result.Value.Port,
		MaxConnectionCount = result.Value.MaxConnectionCount,
		ReceiveBufferSize = result.Value.ReceiveBufferSize,
		MaxPacketSize = result.Value.MaxPacketSize
	};
	option.WriteConsole();

	return option;
}

public class ServerOption
{
	[Option("port", Required = false, HelpText = "Port Number")]
	public int Port { get; set; } = 11021;

	[Option("max_conn_count", Required = false, HelpText = "MaxConnectionCount")]
	public int MaxConnectionCount { get; set; } = 100;

	[Option("recv_buff_size", Required = false, HelpText = "ReceiveBufferSize")]
	public int ReceiveBufferSize { get; set; } = 4012;

	[Option("max_packet_size", Required = false, HelpText = "MaxPacketSize")]
	public int MaxPacketSize { get; set; } = 1024;

}

