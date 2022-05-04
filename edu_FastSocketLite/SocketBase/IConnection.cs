using System;
using System.Net;

namespace FastSocketLite.SocketBase
{
    /// <summary>
    /// a connection interface.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// disconnected event
        /// </summary>
        event DisconnectedHandler Disconnected;

        /// <summary>
        /// return the connection is active.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// get the connection latest active time.
        /// </summary>
        DateTime LatestActiveTime { get; }
        
        /// <summary>
        /// get the connection id.
        /// </summary>
        long ConnectionID { get; }
        
        IPEndPoint LocalEndPoint { get; }
        
        IPEndPoint RemoteEndPoint { get; }
        
        object UserData { get; set; }

        void BeginSend(Packet packet);
        
        void BeginReceive();
        
        void BeginDisconnect(Exception ex = null);
    }
}