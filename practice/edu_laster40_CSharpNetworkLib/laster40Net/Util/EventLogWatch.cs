using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace laster40Net.Util
{
    public struct EventLogData
    {
        public string Log;
        public string EntryType;
        public string Source;
        public string Category;
        public string UserName;
        public string Message;
        public DateTime TimeGenerated;
    }

    public delegate void EventLogEntryWritten(EventLogData data);

    public class EventLogWatch
    {
        /// <summary>
        /// 이벤트가 쓰여졌을때 발생하는 이벤트
        /// </summary>
        public event EventLogEntryWritten EntryWrittenEvent;

        public string[] WatchLog
        {
            get
            {
                if (_currentEventLog == null)
                    return null;

                return GetEventLogName(_currentEventLog);
            }
            set
            {
                if (value == null)
                {
                    _currentEventLog = GetEventLogAvailable();
                }
                else
                {
                    List<EventLog> logs = new List<EventLog>();
                    var events = GetEventLogAvailable();
                    foreach (var str in value)
                    {
                        var match = Array.Find(events, (EventLog e) => e.Log == str);
                        if (match != null)
                            logs.Add(match);
                    }

                    if (logs.Count == 0)
                        throw new NotSupportedException();
                }
                Start();
            }
        }

        private EventLog[] _currentEventLog = null;

        #region Watcher
        private class WatcherProxy
        {
            private EventLog _eventLog;
            private EventLogWatch _owner;
            public WatcherProxy(EventLogWatch owner, EventLog eventLog)
            {
                _owner = owner;
                _eventLog = eventLog;
            }
            public void OnEntryWritten(object source, EntryWrittenEventArgs e)
            {
                if( _eventLog.Entries.Count == 0 )
                    return;

                EventLogEntry data = _eventLog.Entries[_eventLog.Entries.Count - 1];
                _owner.OnEntryWritten(_eventLog, data);
            }
        }
        #endregion

        /// <summary>
        /// 시작~
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            Stop();

            if (_currentEventLog == null)
            {
                _currentEventLog = GetEventLogAvailable();
            }

            foreach (var log in _currentEventLog)
            {
                WatcherProxy proxy = new WatcherProxy(this, log);
                log.EntryWritten += new EntryWrittenEventHandler(proxy.OnEntryWritten);
                log.EnableRaisingEvents = true;
            }

            return true;
        }

        /// <summary>
        /// 종료
        /// </summary>
        public void Stop()
        {
            if (_currentEventLog == null)
                return;

            foreach (var log in _currentEventLog)
            {
                log.EnableRaisingEvents = false;
                log.Close();
            }

            _currentEventLog = null;
        }
        /// <summary>
        /// 현재 감시중인 이벤트 로그
        /// </summary>
        /// <returns></returns>
        public EventLog[] GetCurrentEventLog()
        {
            return _currentEventLog;
        }

        private void OnEntryWritten(EventLog log, EventLogEntry entry)
        {
            if (EntryWrittenEvent != null)
            {
                EventLogData data = new EventLogData();
                data.Log = log.Log;
                data.EntryType = entry.EntryType.ToString();
                data.Source = entry.Source;
                data.Category = entry.Category;
                data.UserName = entry.UserName;
                data.Message = entry.Message;
                data.TimeGenerated = entry.TimeGenerated;

                EntryWrittenEvent(data);
            }
        }


        #region Static Member
        /// <summary>
        /// 사용가능한 이벤트 로그 리스트
        /// </summary>
        /// <returns></returns>
        public static EventLog[] GetEventLogAvailable()
        {
            return EventLog.GetEventLogs();
        }

        /// <summary>
        /// 이벤트 로그의 이름
        /// </summary>
        /// <returns></returns>
        public static string[] GetEventLogName()
        {
            return GetEventLogName(GetEventLogAvailable());
        }

        /// <summary>
        /// 이벤트 로그 이름 구하기
        /// </summary>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static string[] GetEventLogName(EventLog[] eventLog)
        {
            List<string> names = new List<string>();
            foreach (var log in eventLog)
            {
                names.Add(log.Log);
            }

            return names.ToArray();
        }
        #endregion

    }
}
