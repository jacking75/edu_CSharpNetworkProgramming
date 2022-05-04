using System;

namespace FastSocketLite.SocketBase
{
    /// <summary>
    /// packet
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// get or set sent size.
        /// </summary>
        internal int SentSize = 0;
        /// <summary>
        /// get the packet created time
        /// </summary>
        public readonly DateTime CreatedTime = Utils.Date.UtcNow;
        /// <summary>
        /// get payload
        /// </summary>
        public readonly byte[] Payload;
        

        /// <summary>
        /// new
        /// </summary>
        /// <param name="payload"></param>
        /// <exception cref="ArgumentNullException">payload is null.</exception>
        public Packet(byte[] payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            this.Payload = payload;
        }
        

        /// <summary>
        /// get or set tag object
        /// </summary>
        public object Tag { get; set; }
        
        public bool IsSent()
        {
            return this.SentSize == this.Payload.Length;
        }        
    }
}