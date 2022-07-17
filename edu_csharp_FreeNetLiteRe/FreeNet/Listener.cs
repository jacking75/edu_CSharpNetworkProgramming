﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNet
{
	class Listener
	{
		// 비동기 Accept를 위한 EventArgs.
		SocketAsyncEventArgs Accept_Args;

		SocketOption RemoteSocketOpt = new SocketOption();

		Socket ListenSocket;

		// Accept처리의 순서를 제어하기 위한 이벤트 변수.
		AutoResetEvent FlowControlEvent;

		// 새로운 클라이언트가 접속했을 때 호출되는 콜백.
		public Action<Socket, object> OnNewClientCallback = null;


		public Listener() { }

		public void Start(string host, int port, int backlog, SocketOption socketOption)
		{
			RemoteSocketOpt = socketOption;

			ListenSocket = new Socket(AddressFamily.InterNetwork,
							SocketType.Stream, ProtocolType.Tcp);


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

				Accept_Args = new SocketAsyncEventArgs();
				Accept_Args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

				var listen_thread = new Thread(DoListen);
				listen_thread.Start();
			}
			catch (Exception e)
			{
                //TODO: 로그 남기기
                Console.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// 루프를 돌며 클라이언트를 받아들입니다.
		/// 하나의 접속 처리가 완료된 후 다음 accept를 수행하기 위해서 event객체를 통해 흐름을 제어하도록 구현되어 있습니다.
		/// </summary>
		void DoListen()
		{
			FlowControlEvent = new AutoResetEvent(false);

			while (true)
			{
				// SocketAsyncEventArgs를 재사용 하기 위해서 null로 만들어 준다.
				Accept_Args.AcceptSocket = null;

				bool pending = true;

				try
				{
					// 비동기 accept를 호출하여 클라이언트의 접속을 받아들입니다.
					// 비동기 매소드 이지만 동기적으로 수행이 완료될 경우도 있으니
					// 리턴값을 확인하여 분기시켜야 합니다.
					pending = ListenSocket.AcceptAsync(Accept_Args);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					continue;
				}

				// 즉시 완료 되면 이벤트가 발생하지 않으므로 리턴값이 false일 경우 콜백 매소드를 직접 호출해 줍니다.
				// pending상태라면 비동기 요청이 들어간 상태이므로 콜백 매소드를 기다리면 됩니다.
				// http://msdn.microsoft.com/ko-kr/library/system.net.sockets.socket.acceptasync%28v=vs.110%29.aspx
				if (!pending)
				{
					OnAcceptCompleted(null, Accept_Args);
				}

				// 클라이언트 접속 처리가 완료되면 이벤트 객체의 신호를 전달받아 다시 루프를 수행하도록 합니다.
				FlowControlEvent.WaitOne();

				// *팁 : 반드시 WaitOne -> Set 순서로 호출 되야 하는 것은 아닙니다.
				//      Accept작업이 굉장히 빨리 끝나서 Set -> WaitOne 순서로 호출된다고 하더라도 
				//      다음 Accept 호출 까지 문제 없이 이루어 집니다.
				//      WaitOne매소드가 호출될 때 이벤트 객체가 이미 signalled 상태라면 스레드를 대기 하지 않고 계속 진행하기 때문입니다.
			}
		}

		/// <summary>
		/// AcceptAsync의 콜백 매소드
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">AcceptAsync 매소드 호출시 사용된 EventArgs</param>
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				// 새로 생긴 소켓을 보관해 놓은뒤~
				var client_socket = e.AcceptSocket;
				client_socket.NoDelay = RemoteSocketOpt.NoDelay;

				// 이 클래스에서는 accept까지의 역할만 수행하고 클라이언트의 접속 이후의 처리는
				// 외부로 넘기기 위해서 콜백 매소드를 호출해 주도록 합니다.
				// 이유는 소켓 처리부와 컨텐츠 구현부를 분리하기 위함입니다.
				// 컨텐츠 구현부분은 자주 바뀔 가능성이 있지만, 소켓 Accept부분은 상대적으로 변경이 적은 부분이기 때문에
				// 양쪽을 분리시켜주는것이 좋습니다.
				// 또한 클래스 설계 방침에 따라 Listen에 관련된 코드만 존재하도록 하기 위한 이유도 있습니다.
				if (OnNewClientCallback != null)
				{
					OnNewClientCallback(client_socket, e.UserToken);
				}
			}
			else
			{
                //TODO: 로그 남기기
                Console.WriteLine("Failed to accept client. " + e.SocketError);
			}

			// 다음 연결을 받아들인다.
			FlowControlEvent.Set();
		}
	}
}
