using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace laster40Net.Util
{

    public enum LogLevel
    {
        Debug, Info, Error
    };

    public interface ILogger
    {
        LogLevel Level { get; set; }
        void Log(LogLevel lv, string message);
        void Log(LogLevel lv, string message, Exception e);
        void Log(LogLevel lv, Exception e);
    }

    public class NullLogger : ILogger
    {
        #region ILogger Implement
        public LogLevel Level { get; set; }
        public void Log(LogLevel lv, string message)
        {
        }
        public void Log(LogLevel lv, string message, Exception e)
        {
        }
        public void Log(LogLevel lv, Exception e)
        {
        }
        #endregion
    }

    public class ConsoleLogger : ILogger
    {
        #region ILogger Implement
        public LogLevel Level { get; set; }
        public void Log(LogLevel lv, string message)
        {
            if (lv < this.Level)
                return;

            Console.WriteLine("[{0}][{1}]:{2}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), message);
        }
        public void Log(LogLevel lv, string message, Exception e)
        {
            if (lv < this.Level)
                return;

            Console.WriteLine("[{0}][{1}]:{2},{3},{4}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), message, e.Message, e.StackTrace);
        }
        public void Log(LogLevel lv, Exception e)
        {
            if (lv < this.Level)
                return;

            Console.WriteLine("[{0}][{1}]:{2},{3}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), e.Message, e.StackTrace);
        }
        #endregion
    }

    public class SimpleFileLogger : ILogger
    {
        private Object _syncThis = new Object();
        private StreamWriter _output;

        public SimpleFileLogger(String filename)
        {
            _output = new StreamWriter(filename);
        }
        #region ILogger Implement
        public LogLevel Level { get; set; }
        public void Log(LogLevel lv, string message)
        {
            if (lv < this.Level)
                return;

            lock (_syncThis)
            {
                _output.WriteLine("[{0}][{1}]:{2}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), message);
            }
        }
        public void Log(LogLevel lv, string message, Exception e)
        {
            if (lv < this.Level)
                return;

            lock (_syncThis)
            {
                _output.WriteLine("[{0}][{1}]:{2},{3},{4}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), message, e.Message, e.StackTrace);
            }
        }
        public void Log(LogLevel lv, Exception e)
        {
            if (lv < this.Level)
                return;

            lock (_syncThis)
            {
                _output.WriteLine("[{0}][{1}]:{2},{3}", lv.ToString(), DateTime.Now.ToString("HH:mm:ss.fff"), e.Message, e.StackTrace);
            }
        }
        #endregion
    }

    public static class LoggerUtil
    {
        public static void SetupLog4Net(string xmlFile)
        {
            try
            {
                // log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(xmlFile));
            }
            catch (Exception)
            {
            }
        }
    }

    //TODO NLog 구현하기

    /*
    public class Log4NetLogger : ILogger
    {
        protected log4net.ILog Logger { get; private set; }

        public LogLevel Level { get; set; }

        public Log4NetLogger(string name)
        {
            try
            {
                Logger = log4net.LogManager.GetLogger(name);
            }
            catch (Exception)
            {
            }
        }
        #region ILogger Implement
        public void Log(LogLevel lv, string message)
        {
            if (lv < this.Level)
                return;

            switch (lv)
            {
                case LogLevel.Debug: Logger.Debug(message); break;
                case LogLevel.Error: Logger.Error(message); break;
                case LogLevel.Info: Logger.Info(message); break;
            }
        }
        public void Log(LogLevel lv, string message, Exception e)
        {
            if (lv < this.Level)
                return;
            switch (lv)
            {
                case LogLevel.Debug: Logger.Debug(message, e); break;
                case LogLevel.Error: Logger.Error(message, e); break;
                case LogLevel.Info: Logger.Info(message, e); break;
            }
        }
        public void Log(LogLevel lv, Exception e)
        {
            if (lv < this.Level)
                return;

            switch (lv)
            {
                case LogLevel.Debug: Logger.Debug(e); break;
                case LogLevel.Error: Logger.Error(e); break;
                case LogLevel.Info: Logger.Info(e); break;
            }
        }
        #endregion

    }
     * */
}
