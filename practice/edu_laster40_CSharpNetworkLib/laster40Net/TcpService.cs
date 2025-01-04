using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

using laster40Net.Util;


namespace laster40Net;

//TODO 분리하기

/// <summary>
/// 접속이 끊어진 이유
/// </summary>
public enum CloseReason
{
    LocalClosing,       // 로컬에서 끊었음
    RemoteClosing,      // 원격에서 끊어졌음
    Timeout,            // 응답이 없어서 끊어졌음
    Shutdown,           // 셧다운 되었음 ( Stop 명령으로 )
    SocketError,        // 소켓 에러
    MessageResolveError,// 메세지 resolver, builder 중 에러가 생겼음
    MessageBuildError,  // 메세지 resolver, builder 중 에러가 생겼음
    Unknown             // 알수 없음
}


public delegate void SessionConnectionEvent(long session, bool success, EndPoint address, Object token);
public delegate void SessionCloseEvent(long session, CloseReason reason);
public delegate void SessionReceiveEvent(long session, byte[] buffer, int offset, int length);
public delegate void SessionSendEvent(long session, byte[] buffer, int offset, int length);
public delegate void SessionMessageEvent(long session, byte[] buffer, int offset, int length);

      

/// <summary>
//// TCP Service ( Facade )
/// </summary>
public class TcpService : IDisposable
{
    /// <summary>
    /// 설정
    /// </summary>
    internal TcpServiceConfig Config { get; private set; }
    
    /// <summary>
    /// 내부에서 사용할 Logger
    /// </summary>
    internal ILogger Logger { get; private set; }
    
    /// <summary>
    /// 생성된 세션들
    /// </summary>
    internal ConcurrentDictionary<long, Session> _sessions = new ConcurrentDictionary<long, Session>();
    
    /// <summary>
    /// 받을때 사용하는 버퍼 관리자
    /// </summary>
    private BufferManager ReceiveBufferManager = null;
    
    /// <summary>
    /// 버퍼 풀 관리자
    /// </summary>
    internal PooledBufferManager _pooledBufferManager = null;
    
    /// <summary>
    /// Receive에 사용하는 AsyncEventArgs 풀
    /// </summary>
    internal ConcurrentStack<SocketAsyncEventArgs> ReceiveSockAsyncEventArgsPool = new ConcurrentStack<SocketAsyncEventArgs>();
    
    /// <summary>
    /// Send에 사용하는 AsyncEventArgs 풀
    /// </summary>
    internal ConcurrentStack<SocketAsyncEventArgs> SendSockAsyncEventArgsPool = new ConcurrentStack<SocketAsyncEventArgs>();
    
    /// <summary>
    /// 작동중이야?
    /// </summary>
    private AtomicInt _running = new AtomicInt();
    
    /// <summary>
    /// 작동중이냐?
    /// </summary>
    public bool IsRunning { get { return _running.IsOn(); } }
    
    /// <summary>
    /// 접속한 세션수
    /// </summary>
    public int SessionCount { get { return _sessions.Count; } }
    
    /// <summary>
    /// 접속 완료 이벤트
    /// </summary>
    public event SessionConnectionEvent ConnectionEvent;
    
    /// <summary>
    /// 접속 종료 이벤트
    /// </summary>
    public event SessionCloseEvent CloseEvent;
    
    /// <summary>
    /// 패킷 받기 완료 이벤트 ( 메세지를 받은 경우 패킷 받기는 오지 않음 )
    /// </summary>
    public event SessionReceiveEvent ReceiveEvent;
    
    /// <summary>
    /// 패킷 보내기 완료 이벤트
    /// </summary>
    public event SessionSendEvent SendEvent;
    
    /// <summary>
    /// 메세지 받기 이벤트
    /// </summary>
    public event SessionMessageEvent MessageEvent;        
    
    /// <summary>
    /// Listener 리스트의 Sync Object
    /// </summary>
    private Object _syncListener = new Object();
    
    /// <summary>
    /// Listener 리스트
    /// </summary>
    private List<Listener> _listeners = new List<Listener>();
    
    /// <summary>
    /// 등록된 커넥터
    /// </summary>
    private static ConcurrentDictionary<IPEndPoint, Connector> _connectors = new ConcurrentDictionary<IPEndPoint, Connector>();
    
    /// <summary>
    /// 메세지 Factory
    /// </summary>
    private Message.IMessageFactory _messageFactory = null;
    
    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="config">
    /// tcp service 에 필요한 config
    /// </param>
    public TcpService(TcpServiceConfig config)
    {
        Setup(config);
        return;
    }

    /// <summary>
    /// xml 설정 파일을 읽어서 설정
    /// </summary>
    /// <param name="configFile">설정 파일</param>
    public TcpService(string configFile)
    {
        TcpServiceConfig config;
        using (StreamReader streamReader = File.OpenText(configFile))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TcpServiceConfig));
            config = serializer.Deserialize(streamReader) as TcpServiceConfig;
        }

        Setup(config);
    }

    /// <summary>
    /// config구조체로 초기화
    /// </summary>
    /// <param name="config"></param>
    private void Setup(TcpServiceConfig config)
    {
        // 콘솔 로거 생성
        if (config.Log != null)
        {
            if (config.Log.Equals("console", StringComparison.OrdinalIgnoreCase))
                this.Logger = new ConsoleLogger();
            else if (config.Log.Equals("file", StringComparison.OrdinalIgnoreCase))
                this.Logger = new SimpleFileLogger(@"netservice.log");
            else
                this.Logger = new NullLogger();
        }
        else
        {
            this.Logger = new ConsoleLogger();
        }

        this.Logger.Level = LogLevel.Error;
        if (config.LogLevel != null)
        {
            this.Logger.Level = (LogLevel)Enum.Parse(typeof(LogLevel), config.LogLevel, true);
        }

        // 16, 128, 256, 1024, 4096 사이즈의 풀을 생성하는 설정
        int[] poolSizes = new int[] { 4096, 16, 128, 256, 1024 };
        this._pooledBufferManager = new PooledBufferManager(poolSizes);

        this.Config = config;
        if (Config.MessageFactoryTypeName != "")
        {
            try
            {
                //TODO 테스트 필요
                Type typeOfClass = Type.GetType("this.Config.MessageFactoryAssemblyName", true);
                var objHandle = Activator.CreateInstance(typeOfClass);
                _messageFactory = (laster40Net.Message.IMessageFactory)objHandle;//.Unwrap();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "메세지 팩토리 생성 실패", e);
            }
        }
    }
    
    /// <summary>
    /// listener 추가
    /// </summary>
    /// <param name="ipString">ip</param>
    /// <param name="port">port</param>
    /// <param name="backlog">backlog</param>
    /// <returns>성공, 실패?</returns>
    public bool StartListener(string ip, int port, int backlog)
    {
        // 시작 중이 아니면 동작 시키지 않음
        if (!IsRunning)
        {
            return false;
        }

        IPAddress.TryParse(ip, out var addr);

        if (addr == null)
        {
            addr = IPAddress.Any;
        }

        IPEndPoint endPoint = new IPEndPoint(addr, port);
        Listener listener = new Listener(this, endPoint, backlog);

        bool ret = listener.Start();

        if (!ret)
        {
            listener.Stop();
            Logger.Log(LogLevel.Error, "Listener 의 초기화에 실패했습니다.");
            return false;
        }

        lock(_syncListener)
        {
            _listeners.Add(listener);
        }

        return true;
    }

    /// <summary>
    /// 접속 시작
    /// </summary>
    /// <param name="ipString">ip</param>
    /// <param name="port">port</param>
    /// <param name="timeoutMillSec">미구현</param>
    /// <param name="token">token</param>
    /// <returns>성공, 실패?</returns>
    public bool StartConnect(string ip, int port, int timeout, int retry, Object token)
    {
        // 시작 중이 아니면 동작 시키지 않음
        if (!IsRunning)
        {
            return false;
        }

        Connector connector = new Connector(this, new TcpServiceConfig.ClientConfig(ip, port, timeout, retry), token);
        if (_connectors.TryAdd(connector.EndPoint, connector))
        {
            if (!connector.Start())
            {
                Connector tmp;
                _connectors.TryRemove(connector.EndPoint, out tmp);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 접속 요청 중지
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool StopConnect(string ip, int port)
    {
        // 시작 중이 아니면 동작 시키지 않음
        if (!IsRunning)
        {
            return false;
        }

        System.Net.IPAddress addr;
        System.Net.IPAddress.TryParse(ip, out addr);
        System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(addr, port);

        if (_connectors.TryGetValue(endPoint, out var connector))
        {
            if (connector != null)
            {
                connector.Stop();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 서비스 시작
    /// </summary>
    /// <returns>
    /// 성공, 실패?
    /// </returns>
    public bool Run()
    {
        // 시작 중이면 안시작 시켜야징
        if (!_running.CasOn())
        {
            return false;
        }

        // 버퍼 초기화 하공
        ReceiveBufferManager = new BufferManager(Config.ReceviceBuffer * Config.MaxConnectionCount, Config.ReceviceBuffer);

        try
        {
            ReceiveBufferManager.InitBuffer();
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "메모리 할당 실패 - 메모리 관리자 초기화 실패", e);
            return false;
        }

        // 풀에 하나씩 넣어주고
        try
        {
            for (int i = 0; i < Config.MaxConnectionCount; i++)
            {
                {
                    SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                    eventArgs.UserToken = new SessionIOUserToken();
                    eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedReceive);
                    ReceiveBufferManager.SetBuffer(eventArgs);
                    ReceiveSockAsyncEventArgsPool.Push(eventArgs);
                }

                // 보내고 난 후 다음 것을 보내야 한다.
                {
                    SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                    eventArgs.UserToken = new SessionIOUserToken();
                    eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedSend);
                    eventArgs.SetBuffer(null, 0, 0);
                    SendSockAsyncEventArgsPool.Push(eventArgs);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "메모리 할당 실패 - 너무 많은 메모리를 할당했습니다.", e);
            return false;
        }

        if (Config.Listeners != null)
        {
            foreach (var config in Config.Listeners)
            {
                StartListener(config.ip, config.port, config.backlog);
            }
        }

        if (Config.Clients != null)
        {
            foreach (var client in Config.Clients)
            {
                StartConnect(client.ip, client.port, client.timeout, client.retry, null);
            }
        }

        return true;
    }

    /// <summary>
    /// 멈춰줘요
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        _running.Off();

        Dispose(true);
        GC.SuppressFinalize(true);
    }


  
    public void Dispose()
    {
        this.Dispose(true);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 모든 listener 를 제거한다.
            lock(_syncListener)
            {
                Task[] tasks = new Task[_listeners.Count];

                for (int i = 0; i < _listeners.Count;++i)
                {
                    tasks[i] = Task.Factory.StartNew((s) => ((Listener)s).Stop(), _listeners[i]);
                }

                Task.WaitAll(tasks);
                _listeners.Clear();
            }

            
            var lists = _connectors.ToArray();

            foreach ( var pair in lists )
            {
                pair.Value.Stop();
            }

            // 소켓 닫기
            var sessions = _sessions.ToArray();
            if( sessions.Length > 0 )
            {
                Task[] tasks = new Task[sessions.Length];

                for (int i=0;i<sessions.Length;++i)
                {
                    tasks[i] = Task.Factory.StartNew( (s) => ((Session)s).PostClose(CloseReason.Shutdown), sessions[i].Value);
                }

                Task.WaitAll(tasks);
            }

            // session들이 모두 종료될때 까지 대기
            while (_sessions.Count > 0)
            {
                Thread.Sleep(1);
            }

            _sessions.Clear();
            ReceiveSockAsyncEventArgsPool.Clear();
            SendSockAsyncEventArgsPool.Clear();
            ReceiveBufferManager = null;
        }
    }
  
    
    /// <summary>
    /// 세션의 접속을 끊어주세요
    /// </summary>
    /// <param name="id"></param>
    public void CloseSession(long id)
    {
        // TODO 태스크를 만들지말고 이런걸 하는 스레드로 작업을 넘기자
        if (_sessions.TryGetValue(id, out var session))
        {
            Task.Factory.StartNew((s) => ((Session)s).PostClose(CloseReason.LocalClosing), session);
        }
    }

    /// <summary>
    /// 세션에 패킷을 보냄
    /// </summary>
    /// <param name="id">보낼놈 세션 id</param>
    /// <param name="buffer">buffer</param>
    /// <param name="offset">offset</param>
    /// <param name="length">length</param>
    /// <param name="directly">바로? 아님 모아서?</param>
    public void SendToSession(long id, byte[] buffer, int offset, int length, bool directly)
    {
        if (_sessions.TryGetValue(id, out var session))
        {
            session.PostSend(buffer, offset, length, directly);
        }
    }

    /// <summary>
    /// 세 클라가 들어왔다~
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal bool NewClient(Socket socket, Object token)
    {
        if (!IsRunning)
        {
            return false;
        }

        if (socket == null)
        {
            return false;
        }

        // 아 이게 제대로 동작 하지 않음
        if (_sessions.Count + 1 > Config.MaxConnectionCount)
        {
            Logger.Log(LogLevel.Error, "Session - 최대 접속자가 제한되어 접속할수 없습니다.");
            return false;
        }

        //TODO ReceiveSockAsyncEventArgsPool 풀을 각 세션별로 가지도록 한다
        SocketAsyncEventArgs receiveEventArgs;
        if (!ReceiveSockAsyncEventArgsPool.TryPop(out receiveEventArgs))
        {
            Logger.Log(LogLevel.Error, "Session - 할당된 충분한 메모리가 존재하지 않음");
            return false;
        }

        SocketAsyncEventArgs sendEventArgs;
        if (!SendSockAsyncEventArgsPool.TryPop(out sendEventArgs))
        {
            ReceiveSockAsyncEventArgsPool.Push(receiveEventArgs);

            Logger.Log(LogLevel.Error, "Session - 할당된 충분한 메모리가 존재하지 않음");
            return false;
        }

        Message.IMessageBuilder messageBuilder = null;
        Message.IMessageResolver messageResolver = null;

        if (_messageFactory != null)
        {
            messageBuilder = _messageFactory.CreateBuilder();
            messageResolver = _messageFactory.CreateResolver();
        }

        // 하나 할당 받아서 리스트에 넣음
        Session client = new Session(this, socket, messageBuilder, messageResolver);
        if (!_sessions.TryAdd(client.ID, client))
        {
            ReceiveSockAsyncEventArgsPool.Push(receiveEventArgs);
            SendSockAsyncEventArgsPool.Push(sendEventArgs);

            Logger.Log(LogLevel.Error, "Session - 세션 리스트에 추가 실패");
            return false;
        }

        Logger.Log(LogLevel.Debug, string.Format("새 클라이언트 접속 - id:{0},endpoint:{1}", client.ID, client.RemoteEndPoint));

        client.Open(receiveEventArgs, sendEventArgs);

        FireConnectionEvent(client.ID, true, client.RemoteEndPoint, token);

        return true;
    }

    /// <summary>
    /// 소켓 끊기( 내부적사용 - 실제로 소켓을 반환함 )
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="reason"></param>
    /// <param name="readEventArgs"></param>
    /// <param name="sendEventArgs"></param>
    internal void CloseSession(long Id, CloseReason reason, SocketAsyncEventArgs readEventArgs, SocketAsyncEventArgs sendEventArgs)
    {
        if (_sessions.TryRemove(Id, out Session session))
        {
            if (this.CloseEvent != null)
            {
                this.CloseEvent(Id, reason);
            }
        }

        ReceiveSockAsyncEventArgsPool.Push(readEventArgs);
        SendSockAsyncEventArgsPool.Push(sendEventArgs);
    }

    internal void FireReceiveEvent(long Id, byte[] buffer, int offset, int length)
    {
        if (this.ReceiveEvent != null)
        {
            this.ReceiveEvent(Id, buffer, offset, length);
        }
    }
    internal void FireMessageEvent(long Id, byte[] buffer, int offset, int length)
    {
        if (this.MessageEvent != null)
        {
            this.MessageEvent(Id, buffer, offset, length);
        }
    }
    internal void FireSendEvent(long Id, byte[] buffer, int offset, int length)
    {
        if (this.SendEvent != null)
        {
            this.SendEvent(Id, buffer, offset, length);
        }
    }
    internal void FireConnectionEvent(long session, bool success, EndPoint address, Object token)
    {
        if (this.ConnectionEvent != null)
        {
            this.ConnectionEvent(session, success, address, token);
        }
    }

    /// <summary>
    /// 접속이 완료됨 (listener)
    /// </summary>
    /// <param name="success"></param>
    /// <param name="socket"></param>
    internal void CompletedAccept(bool success, Socket socket)
    {
        if (!IsRunning)
        {
            return;
        }

        if (!NewClient(socket, null))
        {
            Logger.Log(LogLevel.Error, "TcpService - Accept된 소켓의 세션 생성에 실패");

            EndPoint endPoint = null;

            try
            {
                endPoint = socket.RemoteEndPoint;
            }
            catch (Exception)
            { }

            FireConnectionEvent(0, false, endPoint, null);

            try
            {
                socket.Close();
            }
            catch (Exception)
            { }
        }
    }

    /// <summary>
    /// 접속이 완료됨( startconnect )
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="e"></param>
    internal void CompletedConnect(bool ret, IPEndPoint endPoint, Socket socket, Object token)
    {
        Connector connector;

        if (!_connectors.TryRemove(endPoint, out connector))
        {
            return;
        }

        if (ret)
        {
            if (!NewClient(socket, token))
            {
                Logger.Log(LogLevel.Error, "TcpService - Connect된 소켓의 세션 생성에 실패");

                try
                {
                    socket.Close();
                }
                catch (Exception)
                {
                }

                FireConnectionEvent(0, false, endPoint, token); ;
            }
        }
        else
        {
            Logger.Log(LogLevel.Error, string.Format("TcpService - Connect 실패 - address:{0}", endPoint));

            FireConnectionEvent(0, false, endPoint, token);
        }
    }

    /// <summary>
    /// 패킷 수신이완료됨
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void CompletedReceive(object sender, SocketAsyncEventArgs e)
    {
        var token = e.UserToken as SessionIOUserToken;
        var session = token.Session;

        if (session == null)
        {
            return;
        }

        session.CompletedReceive(e);
    }

    /// <summary>
    /// 패킷 전송이 완료됨
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void CompletedSend(object sender, SocketAsyncEventArgs e)
    {
        var token = e.UserToken as SessionIOUserToken;
        var session = token.Session;

        if (session == null)
        {
            return;
        }

        session.CompletedSend(e);
    }
    
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("--------------------------------------------");
        builder.Append("Session Count : ");
        builder.Append(_sessions.Count);
        builder.AppendLine();
        builder.Append(_pooledBufferManager.ToString());
        return builder.ToString();
    }

} // end Class
