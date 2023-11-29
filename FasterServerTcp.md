# FASTER의 TCP 서버 구현 예
FASTER는 마이크로소프트가 오픈 소스로 공개한 로그, KV 저장소 라이브러리 이다.  
  [FASTER 라이브러리 소개](https://docs.google.com/document/d/e/2PACX-1vTJwlb0oqCxnfB6BQ4owsdbmtJau-HlU_nD0e7j5nX4RBCw7gKEwzxEefGTxpgdQjCC6NV5NpmFvCqT/pub )  
KV 저장소 기능을 사용할 때 FASTER에서 제공하는 서버를 그대로 사용할 수 있다.  
이 글에서는 TCP Server 부분의 코드를 일부 가져왔다. 코드가 간단해서 어떻게 서버에서 클라이언트 접속을 처리할 수 있는지 참고 하기에 좋다.  
아래 코드의 위치: https://github.com/microsoft/FASTER/blob/main/cs/remote/src/FASTER.server/Servers/FasterServerTcp.cs  
코드에서 소켓 동작과 관련 없는 것은 제거했다.   
닷넷 5 이상을 타겟으로 닷넷 5 아래 버전에 대한 코드는 제거했다.
```
// FasterServerBase는 몰라도 소켓 동작 부분 코드를 이해하는데 문제 없다.
public class FasterServerTcp : FasterServerBase
{        
	protected readonly ConcurrentDictionary<IMessageConsumer, byte> activeSessions;
    readonly ConcurrentDictionary<WireFormat, ISessionProvider> sessionProviders;
    int activeSessionCount;
		
	readonly SocketAsyncEventArgs acceptEventArg;
	readonly Socket servSocket;

	public FasterServerTcp(string address, int port, int networkBufferSize = default)
		: base(address, port, networkBufferSize)
	{            
		var ip = Address == null ? IPAddress.Any : IPAddress.Parse(Address);
		var endPoint = new IPEndPoint(ip, Port);
		servSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		servSocket.Bind(endPoint);
		servSocket.Listen(512);
		acceptEventArg = new SocketAsyncEventArgs();
		acceptEventArg.Completed += AcceptEventArg_Completed;
	}

	public override void Dispose()
	{
		base.Dispose();
		servSocket.Dispose();
		acceptEventArg.UserToken = null;
		acceptEventArg.Dispose();
	}

	private unsafe void DisposeConnectionSession(SocketAsyncEventArgs e)
	{
		var connArgs = (ConnectionArgs)e.UserToken;
		connArgs.socket.Dispose();

		e.UserToken = null;
		e.Dispose();            
		DisposeSession(connArgs.session);
	}

	public void DisposeSession(IMessageConsumer _session)
	{
		if (_session != null)
		{
			if (activeSessions.TryRemove(_session, out _))
			{
				_session.Dispose();
				Interlocked.Decrement(ref activeSessionCount);
			}
		}
	}

	// 서버 시작
	public override void Start()
	{
		if (!servSocket.AcceptAsync(acceptEventArg))
			AcceptEventArg_Completed(null, acceptEventArg);
	}

	private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
	{
		try
		{
			do
			{
				if (!HandleNewConnection(e)) break;
				e.AcceptSocket = null;
			} while (!servSocket.AcceptAsync(e));
		}
		// socket disposed
		catch (ObjectDisposedException) { }
	}

	private unsafe bool HandleNewConnection(SocketAsyncEventArgs e)
	{
		if (e.SocketError != SocketError.Success)
		{
			e.Dispose();
			return false;
		}

		// Ok to create new event args on accept because we assume a connection to be long-running            
		var receiveEventArgs = new SocketAsyncEventArgs();

		var args = new ConnectionArgs
		{
			socket = e.AcceptSocket
		};

        // GC 발동을 줄이기 위해 pinned 사용. 이부분은 GC 관련 부분이니 함부로 따라하면 안된다.
		var buffer = GC.AllocateArray<byte>(NetworkBufferSize, true);
		args.recvBufferPtr = (byte*)Unsafe.AsPointer(ref buffer[0]);
		
		receiveEventArgs.SetBuffer(buffer, 0, NetworkBufferSize);
		receiveEventArgs.UserToken = args;
		receiveEventArgs.Completed += RecvEventArg_Completed;

		e.AcceptSocket.NoDelay = true;
		
		// If the client already have packets, avoid handling it here on the handler so we don't block future accepts.
		if (!e.AcceptSocket.ReceiveAsync(receiveEventArgs))
			Task.Run(() => RecvEventArg_Completed(null, receiveEventArgs));
		return true;
	}

	private void RecvEventArg_Completed(object sender, SocketAsyncEventArgs e)
	{
		try
		{
			var connArgs = (ConnectionArgs)e.UserToken;
			do
			{
				// No more things to receive
				if (!HandleReceiveCompletion(e)) break;
			} while (!connArgs.socket.ReceiveAsync(e));
		}
		// socket disposed
		catch (ObjectDisposedException)
		{
			DisposeConnectionSession(e);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe bool HandleReceiveCompletion(SocketAsyncEventArgs e)
	{
		var connArgs = (ConnectionArgs)e.UserToken;
		if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success || Disposed)
		{
			DisposeConnectionSession(e);
			return false;
		}

		if (connArgs.session == null)
		{
			return CreateSession(e);
		}

		ProcessRequest(e);

		return true;
	}

	private unsafe bool CreateSession(SocketAsyncEventArgs e)
	{
		var connArgs = (ConnectionArgs)e.UserToken;
		
		INetworkSender networkSender = new TcpNetworkSender(connArgs.socket, provider.GetMaxSizeSettings);            
		if (!AddSession(protocol, ref provider, networkSender, out var session))
		{
			DisposeConnectionSession(e);
			return false;
		}
		connArgs.session = session;

		if (Disposed)
		{
			DisposeConnectionSession(e);
			return false;
		}

		ProcessRequest(e);
		return true;
	}

	private static unsafe void ProcessRequest(SocketAsyncEventArgs e)
	{
		var connArgs = (ConnectionArgs)e.UserToken;
		connArgs.bytesRead += e.BytesTransferred;

		var readHead = connArgs.session.TryConsumeMessages(connArgs.recvBufferPtr, connArgs.bytesRead);

		// The bytes left in the current buffer not consumed by previous operations
		var bytesLeft = connArgs.bytesRead - readHead;
		if (bytesLeft != connArgs.bytesRead)
		{
			// Shift them to the head of the array so we can reset the buffer to a consistent state                
			if (bytesLeft > 0)
				Buffer.MemoryCopy(connArgs.recvBufferPtr + readHead, connArgs.recvBufferPtr, bytesLeft, bytesLeft);
			connArgs.bytesRead = bytesLeft;
		}

		if (connArgs.bytesRead == e.Buffer.Length)
		{
			// Need to grow input buffer
#if NET5_0_OR_GREATER
			var newBuffer = GC.AllocateArray<byte>(e.Buffer.Length * 2, true);
#else
			connArgs.recvHandle.Free();
			var newBuffer = new byte[e.Buffer.Length * 2];
			connArgs.recvHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);
#endif
			connArgs.recvBufferPtr = (byte*)Unsafe.AsPointer(ref newBuffer[0]);
			Array.Copy(e.Buffer, newBuffer, e.Buffer.Length);
			e.SetBuffer(newBuffer, connArgs.bytesRead, newBuffer.Length - connArgs.bytesRead);
		}
		else
			e.SetBuffer(connArgs.bytesRead, e.Buffer.Length - connArgs.bytesRead);
	}
}

internal unsafe class ConnectionArgs
{
	public Socket socket;
	public IMessageConsumer session;
	public byte* recvBufferPtr = null;
	public int bytesRead;
}
```  
  
