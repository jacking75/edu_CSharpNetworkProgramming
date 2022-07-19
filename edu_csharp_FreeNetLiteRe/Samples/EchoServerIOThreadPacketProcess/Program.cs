using System;
using System.Collections.Generic;

namespace EchoServerIOThreadPacketProcess
{
	class Program
	{
		static void Main(string[] args)
		{
			// IoThreadPacketDispatcher 에서 바로 패킷을 처리한다. 즉 멀티스레드로 패킷을 처리한다.
			var packetDispatcher = new IoThreadPacketDispatcher();


			var serverOpt = new FreeNet.ServerOption();
						
			var service = new FreeNet.NetworkService(serverOpt);
			service.Initialize();
						
			bool isNoDelay = true;
			service.Listen("0.0.0.0", 7979, 100, isNoDelay);
			
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
		}        	
	}
}
