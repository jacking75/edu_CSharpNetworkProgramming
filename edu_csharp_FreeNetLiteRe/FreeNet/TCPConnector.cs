using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNet
{
	//TODO: 서버-서버 접속용으로 변경한다.
	/// <summary>
	/// Endpoint정보를 받아서 서버에 접속한다.
	/// 접속하려는 서버 하나당 인스턴스 한개씩 생성하여 사용하면 된다.
	/// </summary>
	public class TCPConnector
	{
		public Action<Session> ConnectedCallback = null;

		// 원격지 서버와의 연결을 위한 소켓.
		Socket ClientSocket;

		//TODO: NetworkService 객체를 포함하지 않는다 
		NetworkService RefNetworkService;

		
		public TCPConnector(NetworkService networkService)
		{
			RefNetworkService = networkService;
		}

		public void Connect(IPEndPoint remoteEndpoint, bool isNoDelay)
		{
			ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);			
			ClientSocket.NoDelay = isNoDelay;

			// 비동기 접속을 위한 event args.
			SocketAsyncEventArgs event_arg = new SocketAsyncEventArgs();
			event_arg.Completed += OnConnectCompleted;
			event_arg.RemoteEndPoint = remoteEndpoint;


			bool pending = ClientSocket.ConnectAsync(event_arg);

			if (pending == false)
			{
				OnConnectCompleted(null, event_arg);
			}
		}

		void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				var uniqueId = RefNetworkService.MakeSequenceIdForSession();				
				Session token = new Session(false, uniqueId, RefNetworkService.PacketDispatcher, RefNetworkService.ServerOpt);

				// 데이터 수신 준비.
				RefNetworkService.OnConnectCompleted(ClientSocket, token);

				ConnectedCallback(token);				
			}
			else
			{
				//TODO: 로그로 남기기
				// failed.
				Console.WriteLine(string.Format("Failed to connect. {0}", e.SocketError));
			}
		}
	}
}
