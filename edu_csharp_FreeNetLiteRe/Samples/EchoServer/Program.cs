using System;
using System.Collections.Generic;
using CommandLine;

namespace SampleServer;

class Program
{
	static void Main(string[] args)
	{
		var serverOpt = ParseCommandLine(args);
		var service = new FreeNet.NetworkService(serverOpt, null);
		service.Initialize();

		bool isNoDelay = true;
		service.Listen("0.0.0.0", 7979, 100, isNoDelay);

		Console.WriteLine("Started!");


		// 패킷 처리기 생성 및 실행
		var packetProcess = new PacketProcess(service);
		packetProcess.Start();


		while (true)
		{
			//Console.Write(".");
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
	}


	static FreeNet.ServerOption ParseCommandLine(string[] args)
	{
		var option = new FreeNet.ServerOption
		{
			//Port = 32452,
			//MaxConnectionNumber = 32,
			//Name = "EchoServer"
		};

		return option;
	}

	public class ServerOption
	{
		[Option("port", Required = true, HelpText = "Server Port Number")]
		public int MaxConnectionCount { get; set; } = 10000;

		[Option("port", Required = true, HelpText = "Server Port Number")]
		public int ReserveClosingWaitMilliSecond { get; set; } = 100;

		[Option("port", Required = true, HelpText = "Server Port Number")]
		public int ReceiveSecondaryBufferSize { get; set; } = 4012;

		[Option("c_recv_buff_size", Required = true, HelpText = "Server Port Number")]
		public int ClientReceiveBufferSize { get; set; } = 4096;

		[Option("c_max_packet_size", Required = true, HelpText = "Server Port Number")]
		public int ClientMaxPacketSize { get; set; } = 1024;

		[Option("c_send_mtu", Required = true, HelpText = "Server Port Number")]
		public int ClientSendPacketMTU { get; set; } = 1024;

		[Option("s_send_mtu", Required = true, HelpText = "Server Port Number")]
		public int ServerSendPacketMTU { get; set; } = 1024;
	}
}
