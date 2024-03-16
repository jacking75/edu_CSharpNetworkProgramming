using System;
using System.Collections.Generic;

namespace SampleServer
{
	class Program
	{
		

		static void Main(string[] args)
		{
			var serverOpt = new FreeNet.ServerOption();			
			var service = new FreeNet.NetworkService<FreeNet.DefaultMessageResolver>(serverOpt, null);					
			service.Initialize();

			var socketOpt = new FreeNet.SocketOption();
			socketOpt.NoDelay = true;
			service.Listen("0.0.0.0", 7979, 100, socketOpt);
			
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
					Console.WriteLine(service.SessionMgr.GetTotalCount());
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
	}
}
