using CommandLine;
using System;


var serverOpt = ParseCommandLine(args);

// IoThreadPacketDispatcher 에서 바로 패킷을 처리한다. 즉 멀티스레드로 패킷을 처리한다.
var packetDispatcher = new EchoServerIOThreadPacketProcess.IoThreadPacketDispatcher();
packetDispatcher.Init(EchoServerIOThreadPacketProcess.PacketDef.HeaderSize);

var service = new FreeNetLite.NetworkService(serverOpt, packetDispatcher);
service.Initialize();

bool isNoDelay = true;
service.Start("0.0.0.0", serverOpt.Port, 100, isNoDelay);


while (true)
{
	//Console.Write(".");
	string input = Console.ReadLine();

	if (input.Equals("users"))
	{
		Console.WriteLine(service.CurrentConnectdSessionCount());
	}
	else if (input.Equals("exit"))
	{
		service.Stop();

		Console.WriteLine("Exit Process !!!");
		break;
	}

	System.Threading.Thread.Sleep(500);
}


//--port 32451 --max_conn_count 100 --recv_buff_size 4012 --max_packet_size 1024
FreeNetLite.ServerOption ParseCommandLine(string[] args)
{
	var result = Parser.Default.ParseArguments<ServerOption>(args) as Parsed<ServerOption>;
	if (result == null)
	{
		Console.WriteLine("Failed Command Line");
		return null;
	}

	var option = new FreeNetLite.ServerOption
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
