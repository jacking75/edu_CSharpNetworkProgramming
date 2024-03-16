using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using laster40Net.Message;
using laster40Net.Util;

namespace laster40Net
{
    //TODO 분리하기

    /// <summary>
    /// AsyncIO에 필요한 세션IO의 정보
    /// </summary>
    internal class SessionIOUserToken
    {
        public Socket Socket { get; set; }
        public Session Session { get; set; }

    }
    /// <summary>
    /// 세션의 ID ( 0 값은 존재 하지 않는다.)
    /// </summary>
    internal class SessionID
    {
        private Int64 _topId = 0;
        public int INVALID = 0;

        /// <summary>
        /// 아이디 발급하기
        /// </summary>
        /// <returns>
        /// 발급한 아이디
        /// </returns>
        public Int64 Generate()
        {
            Int64 value = Interlocked.Increment(ref _topId);
            // 이런일이 있겠냐 만은...
            if (value <= 0)
            {
                value = Interlocked.Increment(ref _topId);
            }

            return value;
        }
    }

    //TODO IMessageContext을 상속 받지 않도록 한다
    /// <summary>
    /// 클라이언트들의 연결
    /// </summary>
    internal sealed class Session : IDisposable, IMessageContext
    {
        /// <summary>
        /// 세션간의 고유한 값
        /// </summary>
        public Int64 ID              { get; private set; }
        
        /// <summary>
        /// 소켓 클래스
        /// </summary>
        public Socket Socket        { get; private set; }
        
        /// <summary>
        /// 접속된 원격의 정보(null일수 있음)
        /// </summary>
        public EndPoint RemoteEndPoint{ 
            get {
                try
                {
                    return Socket.RemoteEndPoint;
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// 버퍼 관리자
        /// </summary>
        public IBufferManager BufferManager { get { return Service._pooledBufferManager; } }
        
        /// <summary>
        /// 메세지 빌더
        /// </summary>
        public IMessageBuilder MessageBuilder { get { return _messageBuilder; } }
        
        /// <summary>
        /// 메세지 
        /// </summary>
        public IMessageResolver MessageResolver { get { return _messageResolver; } }
        
        /// <summary>
        /// 연결이 되어 있나?
        /// </summary>
        public bool IsConnected     { get { return !_closing.IsOn(); } }

        private TcpService Service   { get; set; }


        // 상태를 관리할 값들
        /// <summary>
        /// 받기 중이야?
        /// </summary>
        private AtomicInt _receiving = new AtomicInt();

        /// <summary>
        /// 보내기 중이야?
        /// </summary>
        private AtomicInt _sending = new AtomicInt();

        /// <summary>
        /// 접속 종료 하고 싶은거야?
        /// </summary>
        private AtomicInt _closing = new AtomicInt(1);

        /// <summary>
        /// 모든게 해지된 상태이다.
        /// </summary>
        private AtomicInt _disposed = new AtomicInt();

        /// <summary>
        /// 업데이트 중이야?
        /// </summary>
        private AtomicInt _updating = new AtomicInt();

        /// <summary>
        /// 세션아이디를 관리
        /// </summary>
        private static SessionID _sessionId = new SessionID();

        /// <summary>
        /// 보낼 패킷을 담을 큐
        /// </summary>
        private ConcurrentQueue<ArraySegment<byte>> _sendQueue = new ConcurrentQueue<ArraySegment<byte>>();

        /// <summary>
        /// 현재 보내고 있는 패킷의 리스트( 성능상 List를 계속 생성시키지 않으려고 미리 만들어 둔다 )
        /// </summary>
        private List<ArraySegment<byte>> _sendingArraySegmentList = new List<ArraySegment<byte>>();

        /// <summary>
        /// 세션 Close의 동기화를 위한 오브젝트
        /// </summary>
        private readonly object _closeSync = new object();

        /// <summary>
        /// 세션이 종료된 이유
        /// </summary>
        private CloseReason _closeReason;

        /// <summary>
        /// receive 에 사용되는 EventArgs ( 매번 할당하지 않고 세션이 생성될때 pool에서 꺼내 써고 반납 )
        /// </summary>
        private SocketAsyncEventArgs _receiveEventArgs = null;

        /// <summary>
        /// send 에 사용되는 EventArgs ( 매번 할당하지 않고 세션이 생성될때 pool에서 꺼내 써고 반납 )
        /// </summary>
        private SocketAsyncEventArgs _sendEventArgs = null;

        /// <summary>
        /// 업데이트를 수행하기 위해 사용하는 timer
        /// </summary>
        private System.Threading.Timer _updateTimer = null;

        /// <summary>
        /// 마지막 패킷을 받은 시간
        /// </summary>
        private int _lastActivateTime;

        /// <summary>
        /// 패킷을 보낼때 메세지를 만들어서 보내는 builder
        /// </summary>
        private IMessageBuilder _messageBuilder = null;

        /// <summary>
        /// 패키승ㄹ 받고 메세지를 만들어 주는 resolver
        /// </summary>
        private IMessageResolver _messageResolver = null;

        public Session(TcpService service, Socket client, IMessageBuilder msgBuilder, IMessageResolver msgResolver)
        {
            this.Socket = client;
            this.ID = _sessionId.Generate();
            this.Service = service;
            this._closeReason = CloseReason.Unknown;
            this._lastActivateTime = Environment.TickCount;

            this._messageBuilder = msgBuilder;
            this._messageResolver = msgResolver;
        }

        /// <summary>
        /// 세션의 열기 작업 ( 이 작업이 완료된 후에 io stream이 가능해짐 )
        /// </summary>
        /// <param name="receiveEventArgs">받기에 필요한 이벤트 args</param>
        /// <param name="sendEventArgs">보내기에 필요한 이벤트 args</param>
        public void Open(SocketAsyncEventArgs receiveEventArgs, SocketAsyncEventArgs sendEventArgs)
        {
            _lastActivateTime = Environment.TickCount;
            
            // SocketAsyncEventArgs 초기화
            _receiveEventArgs = receiveEventArgs;
            {
                var token = _receiveEventArgs.UserToken as SessionIOUserToken;
                token.Socket = this.Socket;
                token.Session = this;
            }
            _sendEventArgs = sendEventArgs;
            {
                var token = _sendEventArgs.UserToken as SessionIOUserToken;
                token.Socket = this.Socket;
                token.Session = this;
            }

            if (_messageBuilder != null)
            {
                _messageBuilder.OnOpen(this);
            }

            if (_messageResolver != null)
            {
                _messageResolver.OnOpen(this);
            }

            // 초기 값이 On 이기 때문에 Off로 셋팅해준다.( 잘못된 참조로 Send/Receive 같은 동작들이 발생할수 있기때문에 )
            _closing.Off();

            // 받기 시작~
            PostReceive();

            //TODO: 지정 시간안에 CompletedUpdateTimer 처리를 완료하지 못한 경우에는 기다렸다가 호출하는지 혹은 다른 스레드에서 시간에 맞게 호출하는지 확인 필요.
            // 타이머 셋팅
            _updateTimer = new System.Threading.Timer(CompletedUpdateTimer, null, Service.Config.UpdateSessionIntval, Service.Config.UpdateSessionIntval);

            Service.Logger.Log(LogLevel.Debug, string.Format("Session - 생성 - ID:{0}", ID));
        }

        /// <summary>
        /// 세션이 실제로 닫힘
        /// </summary>
        private void Close()
        {
            if (Socket == null)
            {
                return;
            }

            Service.Logger.Log(LogLevel.Debug, string.Format("Session - 닫음 - ID:{0}", ID) );

            Dispose();
        }

        #region IDisposable Member
        public void Dispose()
        {
            Dispose(_disposed.CasOn());
        }

        public void Dispose(bool disposing)
        {
            if( disposing )
            {
                // send queue에 있던놈 메모리 해제 하기
                ArraySegment<byte> arraySeg;
                while (_sendQueue.TryDequeue(out arraySeg))
                {
                    Service._pooledBufferManager.Return(arraySeg.Array);
                }
                
                // 소켓 닫기
                try
                {
                    Socket.Close();
                    Socket = null;
                }
                catch (Exception e)
                {
                    Service.Logger.Log(LogLevel.Debug, "Socket Close 예외 발생", e);
                }

                // 타이머 종료( Dispose 되어도 비동기적으로 타이머가 호출되기 때문에 Update 가 호출될수 있음 )
                _updateTimer.Dispose();
                _updateTimer = null;

                // 닫힘 알리기
                Service.CloseSession(ID, _closeReason, _receiveEventArgs, _sendEventArgs);
                Service = null;

                if( _messageResolver != null)
                {
                    _messageResolver.OnClose(this);
                    _messageResolver = null;
                }

                if (_messageBuilder!= null)
                {
                    _messageBuilder.OnClose(this);
                    _messageResolver = null;
                }
            }
        }
        #endregion

        /// <summary>
        /// Async 받기 시작
        /// </summary>
        public void PostReceive()
        {
            // 접속이 해제 중인 경우 Receive 상태를 On으로 하면 안되니 주의해야한다.
            if (!IsConnected)
            {
                // bugfix: 접속 종료처리가 되지 않는 문제 해결
                // send 쪽 요청에서 에러가 발생해서 close처리를 시작할때 (postclose) _closing 플래그가 On된 상태로 바꼈으나
                // _receiving 의 경우 계속 On상태로 남아 있어서 종료처리가 되지 않는 문제 해결함
                PostClose(CloseReason.LocalClosing);
                _receiving.Off();
                return;
            }

            bool pending = false;
            try
            {
                pending = Socket.ReceiveAsync(_receiveEventArgs);
                Service.Logger.Log(LogLevel.Debug, string.Format("Session - Receive요청 - ID:{0},pending:{1}", ID, pending));
            }
            catch (Exception e)
            {
                Service.Logger.Log(LogLevel.Debug, "Post Receive에러", e);

                PostClose(CloseReason.SocketError);
                _receiving.Off();

                return;
            }

            // 받기 상태로 On시킴 - Receive 가 에러가 발생하기 전에 Off시키지 않음
            _receiving.On();

            if( !pending )
            {
                CompletedReceive(_receiveEventArgs);
            }
        }

        /// <summary>
        /// Async 보내기 시작
        /// </summary>
        /// <param name="buffer">보내기 버퍼</param>
        /// <param name="offset">버퍼의 시작 Offset</param>
        /// <param name="length">버퍼의 길이</param>
        /// <param name="directly">바로 전송 요청할것인지 intval 간격으로 보내기를 할것인지? true는 바로</param>
        public void PostSend(byte[] buffer, int offset, int length, bool directly)
        {
            if (!IsConnected)
            {
                return;
            }

            if (length > Service.Config.SendBuffer)
            {
                return;
            }

            if (_sendQueue.Count + 1 > Service.Config.SendCount)
            {
                return;
            }

            //TODO ArraySegment 사용하지 않아도 될듯
            // 메모리를 할당 받아서 복사본을 만들어서 sendqueue에 넣음
            var copy = Service._pooledBufferManager.Take(length);
            Array.Copy(buffer, offset, copy, 0, length);
            _sendQueue.Enqueue(new ArraySegment<byte>(copy, 0, length));

            // 바로 보내기 처리
            if (directly)
            {
                UpdateFlushSending();
            }
        }        
        /// <summary>
        /// 받기 완료
        /// </summary>
        /// <param name="e"></param>
        public void CompletedReceive(SocketAsyncEventArgs e)
        {
            Service.Logger.Log(LogLevel.Debug, string.Format("Session - recv완료 - ID:{0},len:{1},offset:{2},error:{3}", ID, e.BytesTransferred, e.Offset, e.SocketError.ToString()));

            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                PostClose(CloseReason.RemoteClosing);
                _receiving.Off();
                return;
            }


            _lastActivateTime = Environment.TickCount;

            bool eventFire = true;
            if (_messageResolver != null)
            {
                if( _messageResolver.OnReceive(this, new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred) ) )
                {
                    eventFire = false;
                }
            }

            if (eventFire)
            {
                Service.FireReceiveEvent(ID, e.Buffer, e.Offset, e.BytesTransferred);
            }

            PostReceive();
        }

        /// <summary>
        /// 보내기 완료
        /// </summary>
        /// <param name="e"></param>
        public void CompletedSend(SocketAsyncEventArgs e)
        {
            Service.Logger.Log(LogLevel.Debug, string.Format("Session - send완료 - ID:{0},len:{1},offset:{2},error:{3}", ID, e.BytesTransferred, e.Offset, e.SocketError.ToString()));

            // 보내기에 사용된 버퍼 반납하기
            if (e.BufferList != null)
            {
                if( e.BufferList.Count != _sendingArraySegmentList.Count )
                {
                    Console.WriteLine("보내기 갯수가 안맞넹");
                }

                foreach (var buf in _sendingArraySegmentList)
                {
                    Service._pooledBufferManager.Return(buf.Array);

                    Service.FireSendEvent(ID, buf.Array, buf.Offset, buf.Count);
                }

                _sendingArraySegmentList.Clear();
            }

            // 보내기 끝
            _sending.Off();

            // 에러가 존재하면 끊기
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                PostClose(CloseReason.LocalClosing);
                return;
            }

            _lastActivateTime = Environment.TickCount;

            // 남은 다음 패킷은 다음 update에서 flush처리
            // UpdateFlushSending();
        }

        public void CompletedUpdateTimer(Object e)
        {
            Update();
        }

        public void CompletedMessage(ArraySegment<byte> message)
        {
            Service.FireMessageEvent(ID, message.Array, message.Offset, message.Count);
        }

        /// <summary>
        /// 일정 주기적으로 세션 Update 처리( send 패킷 보내고 좀비 처리 하고 닫기 처리... )
        /// </summary>
        private void Update()
        {
            // 타이머가 dispose되고 난이후에도 호출될수 있기 때문에 이경우 건너 뛴다.
            if (_disposed.IsOn())
            {
                // Console.WriteLine("Session - update - disposed - ID:{0}, tick:{1}", ID, DateTime.Now.Ticks / 10000);
                return;
            }

            // Service.Logger.Log(LogLevel.Debug, string.Format("Session - update - ID:{0}, tick:{1}", ID, DateTime.Now.Ticks / 10000));

            if (!_updating.CasOn())
            {
                return;
            }

            // 보내야할 패킷전송
            UpdateFlushSending();

            // 좀비 처리
            UpdateZombie();

            // 종료 처리
            UpdateClosing();

            _updating.Off();
        }

        /// <summary>
        /// Send 큐에 있는 데이터를 보내기
        /// </summary>
        private void UpdateFlushSending()
        {
            if (!IsConnected)
            {
                return;
            }

            if (_sendQueue.Count <= 0)
            {
                return;
            }

            if (!_sending.CasOn())
            {
                return;
            }

            //TODO:  멀티스레드에서 send를 하면 _sendingArraySegmentList가 경쟁 상태에 빠진다. 락 걸어야 하거나 하나의 스레드에서만 호출해야 한다.
            _sendingArraySegmentList.Clear();

            // 큐에 잇는 리스트를 꺼집어 내서 list를 만들어서 send 시킴
            ArraySegment<byte> arraySeg;
            while (_sendQueue.TryDequeue(out arraySeg))
            {
                if ( _messageBuilder != null )
                {
                    _messageBuilder.OnSend(this, ref _sendingArraySegmentList, arraySeg);
                }
                else
                {
                    _sendingArraySegmentList.Add(arraySeg);
                }
            }
            _sendEventArgs.BufferList = _sendingArraySegmentList;

            try
            {
                bool pending = Socket.SendAsync(_sendEventArgs);
                if (!pending)
                {
                    CompletedSend(_sendEventArgs);
                }
            }
            catch (Exception e)
            {
                // 보낼때 에러가 생기면 close

                Service.Logger.Log(LogLevel.Error, "Session - 패킷전송 실패!", e);

                foreach (var buf in _sendEventArgs.BufferList)
                {
                    Service._pooledBufferManager.Return(buf.Array);
                }

                _sending.Off();

                PostClose(CloseReason.SocketError);
            }
        }

        /// <summary>
        /// 좀비 처리 ( 일정시간 응답이 없는 애들 죽이기 )
        /// </summary>
        private void UpdateZombie()
        {
            if (!IsConnected)
            {
                return;
            }

            if (Service.Config.SessionReceiveTimeout == 0)
            {
                return;
            }

            if (laster40Net.Util.Tick.Gap(_lastActivateTime, Environment.TickCount) > Service.Config.SessionReceiveTimeout)
            {
                Service.Logger.Log(LogLevel.Debug, string.Format("Session - updatezombie - find zombie - ID:{0}", ID));

                PostClose(CloseReason.Timeout);
            }
        }

        /// <summary>
        /// Message Builder와 Resolver 의 Close처리
        /// </summary>
        public void CloseMessageContext(CloseReason closeReason)
        {
            PostClose(closeReason);
        }

        /// <summary>
        /// 종료 요청된것에 대한 실제 종료 처리
        /// </summary>
        private void UpdateClosing()
        {
            if (!_closing.IsOn())
            {
                return;
            }

            // io 요청이 모두 완료되었나 확인
            if (_receiving.IsOn() || _sending.IsOn())
            {
                return;
            }

            lock (_closeSync)
            {
                Close();
            }
        }

        /// <summary>
        /// 종료 요청
        /// </summary>
        /// <param name="reason">
        /// 종료된 이유
        /// </param>
        public void PostClose(CloseReason reason)
        {
            if (_closing.CasOn())
            {
                Service.Logger.Log(LogLevel.Debug, string.Format("Session - PostClose - ID:{0}", ID));

                _closeReason = reason;
                try
                {
                    if (Socket != null)
                    {
                        Socket.Shutdown(SocketShutdown.Send);
                        Socket.Close();
                    }
                }
                catch (Exception e)
                {
                    Service.Logger.Log(LogLevel.Debug, "Socket Shutdown 예외 발생", e);
                }
            }
        }      

    } // end Class
}
