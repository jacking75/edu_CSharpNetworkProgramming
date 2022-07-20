using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNetLite;

class Listener
{
	bool IsStarted = false;
	Thread WorkerThread = null;

	bool IsNoDelay = false;

	Socket ListenSocket;

	// 새로운 클라이언트가 접속했을 때 호출되는 콜백.
	public Action<bool, Socket> OnNewClientCallback = null;



	public void Start(string host, int port, int backlog, bool isNoDelay)
	{
		IsNoDelay = isNoDelay;

		ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		IPAddress address;

		if (host == "0.0.0.0" || host == "localhist") {
			address = IPAddress.Any;
		}
		else {
			address = IPAddress.Parse(host);
		}

		var endpoint = new IPEndPoint(address, port);


		try
		{
			ListenSocket.Bind(endpoint);
			ListenSocket.Listen(backlog);

			IsStarted = true;
			WorkerThread = new Thread(Run);
			WorkerThread.Start();
		}
		catch (Exception e)
		{
                //TODO: 로그 남기기
                Console.WriteLine(e.Message);
		}
	}

	public void Stop()
	{
		if (IsStarted == false)
		{
			return;
		}

		ListenSocket.Close();	

		IsStarted = false;
		WorkerThread.Join();
	}

	/// 루프를 돌며 클라이언트를 받아들입니다.
	/// 하나의 접속 처리가 완료된 후 다음 accept를 수행하기 위해서 event객체를 통해 흐름을 제어하도록 구현되어 있습니다.
	void Run()
	{
		Console.WriteLine("Listen Start");

		while (IsStarted)
		{
			try
			{
				var newClient = ListenSocket.Accept();

				newClient.NoDelay = IsNoDelay;

				OnNewClientCallback(true, newClient);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);

				if (IsStarted == false)
				{
					break;
				}
				else
				{
					continue;
				}
			}							
		}

		Console.WriteLine("Listen End");
	}
}
