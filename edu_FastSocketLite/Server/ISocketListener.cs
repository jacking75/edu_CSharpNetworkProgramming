using System;
using System.Net;

namespace FastSocketLite.Server
{
    /// <summary>
    /// socket listener
    /// </summary>
    public interface ISocketListener
    {
        /// <summary>
        /// socket accepted event
        /// </summary>
        event Action<SocketBase.IConnection> Accepted;

        EndPoint EndPoint { get; }

        void Start();
        
        void Stop();
    }
}