using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using laster40Net;
using laster40Net.Util;

namespace laster40Net.Data
{
    public delegate bool DoDBQuery<TDBConn>(TDBConn conn);

    /// <summary>
    /// 디비 쿼리 처리기
    /// - 쿼리를 비동기적으로 처리할수 있도록 해준다.
    /// - 연결된 DB를 사용한다.
    /// - 이미 생성된 fixed된 개수의 스레드에서 처리한다.
    /// </summary>
    public class DBQueryProcess<TDBConn>
        where TDBConn : IDbConnection, new()
    {
        public int ProcessCount { get; private set; }
        public string ConnectionString { get; set; }
        public int QueryCount { get { return _queue.Count; } }
        public bool IsRunning { get { return _isRunning.IsOn(); } }

        private ConcurrentQueue<DoDBQuery<TDBConn>> _queue = new ConcurrentQueue<DoDBQuery<TDBConn>>();
        private AutoResetEvent _event = new AutoResetEvent(false);
        private ILogger Logger { get; set; }
        private Worker[] _worker = null;
        private AtomicInt _isRunning = new AtomicInt();

        public DBQueryProcess(int processCount, ILogger logger)
        {
            this.ProcessCount = processCount;
            this.Logger = logger;
        }

        public bool Start()
        {
            if (!_isRunning.CasOn())
                return false;

            _event.Reset();

            _worker = new Worker[ProcessCount];
            for (int i = 0; i < ProcessCount; ++i)
            {
                _worker[i] = new Worker(this);
                _worker[i].Start();
            }

            return true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            for (int i = 0; i < ProcessCount; ++i)
            {
                _worker[i].Stop();
                _worker[i] = null;
            }
            _worker = null;

            _isRunning.Off();
        }

        public void Enqueue(DoDBQuery<TDBConn> query)
        {
            if (!IsRunning)
                return;
            _queue.Enqueue(query);
            _event.Set();
        }

        public bool Dequeue(out DoDBQuery<TDBConn> query)
        {
            query = default(DoDBQuery<TDBConn>);
            const int Timeout = 100;
            bool signaled = _event.WaitOne(Timeout);
            if (signaled)
            {
                return _queue.TryDequeue(out query);
            }
            return false;
        }



        #region Worker

        /// <summary>
        /// Worker
        /// </summary>
        private class Worker
        {
            private TDBConn _conn = default(TDBConn);
            private DBQueryProcess<TDBConn> _owner = null;
            private Thread _thread = null;
            private AtomicInt _isStop = new AtomicInt();
            private AtomicInt _isDBOpen = new AtomicInt();

            public Worker(DBQueryProcess<TDBConn> process)
            {
                this._owner = process;
            }

            public void Start()
            {
                _thread = new Thread(ThreadEntry);
                _thread.Start();

                return;
            }

            public void Stop()
            {
                _isStop.On();
                _thread.Join();
            }

            /// <summary>
            /// 연결을 유지 시켜준다.
            /// </summary>
            private void KeepConnection()
            {
                if (_isDBOpen.CasOn())
                {
                    _owner.Logger.Log(LogLevel.Debug, "DBQueryProcess - DB 접속 시도");

                    try
                    {
                        _conn = new TDBConn();
                        _conn.ConnectionString = _owner.ConnectionString;
                        _conn.Open();

                        _owner.Logger.Log(LogLevel.Debug, "DBQueryProcess - DB 접속 성공");
                    }
                    catch (Exception e)
                    {
                        _isDBOpen.Off();

                        _owner.Logger.Log(LogLevel.Error, "DBQueryProcess - DB 접속 실패", e);
                    }
                }

                return;
            }
            /// <summary>
            /// 접속을 종료한다.
            /// </summary>
            private void CloseConnection()
            {
                if( _isDBOpen.IsOn() )
                {
                    if( _conn != null )
                    {
                        _conn.Close();
                    }
                    _isDBOpen.Off();
                }
            }

            private void ThreadEntry()
            {
                _owner.Logger.Log(LogLevel.Debug, "DBQueryProcess - Worker 시작");
                for (; ; )
                {
                    if (_isStop.IsOn())
                    {
                        CloseConnection();
                        break;
                    }

                    KeepConnection();

                    if (_isDBOpen.IsOn())
                    {
                        DoDBQuery<TDBConn> query;
                        if (_owner.Dequeue(out query))
                        {
                            try
                            {
                                if (!query(_conn))
                                {
                                    CloseConnection();
                                }
                            }
                            catch (Exception e)
                            {
                                CloseConnection();

                                _owner.Logger.Log(LogLevel.Error, "DBQueryProcess - db 처리중 에러 발생", e);
                            }
                        }
                    }

                    if (_conn.State == System.Data.ConnectionState.Closed)
                    {
                        CloseConnection();
                        _owner.Logger.Log(LogLevel.Error, "DBQueryProcess - 접속이 종료됨");
                    }
                }

                _owner.Logger.Log(LogLevel.Debug, "DBQueryProcess - Worker 종료");

                return;
            }

        }

        #endregion
    }

}
