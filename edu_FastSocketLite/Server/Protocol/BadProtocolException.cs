using System;

namespace FastSocketLite.Server.Protocol
{
    /// <summary>
    /// bad protocol exception
    /// </summary>
    public sealed class BadProtocolException : ApplicationException
    {
        /// <summary>
        /// new
        /// </summary>
        public BadProtocolException()  : base("bad protocol.")
        {
        }

        public BadProtocolException(string message) : base(message)
        {
        }
    }
}