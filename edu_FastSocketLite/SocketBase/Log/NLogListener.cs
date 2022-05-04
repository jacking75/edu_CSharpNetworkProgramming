using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastSocketLite.SocketBase.Log
{
    /// <summary>
    /// NLog trace listener
    /// </summary>
    class NLogListener : ITraceListener
    {
        Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            _Logger.Debug(message);
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            _Logger.Error($"{message},  Exception: {ex.ToString()}");
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            _Logger.Info(message);
        }
    }
}
