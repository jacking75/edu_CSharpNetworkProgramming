using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FastSocketLite.SocketBase
{
    /// <summary>
    /// socket connection collection
    /// </summary>
    public sealed class ConnectionCollection
    {
        /// <summary>
        /// key:ConnectionID
        /// </summary>
        private readonly ConcurrentDictionary<long, IConnection> _dic = new ConcurrentDictionary<long, IConnection>();
        

        public bool Add(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return this._dic.TryAdd(connection.ConnectionID, connection);
        }
        
        public bool Remove(long connectionID)
        {
            return this._dic.TryRemove(connectionID, out var connection);
        }
        
        public IConnection Get(long connectionID)
        {
            this._dic.TryGetValue(connectionID, out var connection);
            return connection;
        }
        
        public IConnection[] ToArray()
        {
            return this._dic.ToArray().Select(c => c.Value).ToArray();
        }
        
        public int Count()
        {
            return this._dic.Count;
        }
        
        public void DisconnectAll()
        {
            var connections = this.ToArray();

            foreach (var conn in connections)
            {
                conn.BeginDisconnect();
            }
        }        
    }
}