using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNetLite;

// Endpoint정보를 받아서 서버에 접속한다.
// 접속하려는 서버 하나당 인스턴스 한개씩 생성하여 사용하면 된다.
public class TCPConnector
{
	//NetworkService 클래스의 OnConnectCompleted와 연결한다
	public Action<Session> ConnectedCallback = null;

	//NetworkService 클래스의 OnNewClient와 연결한다
	public Action<bool, Socket> OnNewSessionCallback = null;


	// 원격지 서버와의 연결을 위한 소켓.
	Socket ClientSocket;

	IPacketDispatcher Dispatcher;
	ServerOption ServerOpt;

	
	public void Init(IPacketDispatcher dispatcher, ServerOption serverOption)
        {
		Dispatcher = dispatcher;
		ServerOpt = serverOption;	
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
			// 데이터 수신 준비.
			OnNewSessionCallback(false, ClientSocket);
		}
		else
		{
			//TODO: 로그로 남기기
			// failed.
			Console.WriteLine(string.Format("Failed to connect. {0}", e.SocketError));
		}
	}
}
