using System;
using System.Buffers;
using System.Net;

namespace ServerNet
{
    public readonly struct IncomingMessage : IDisposable
    {
        private readonly IMemoryOwner<byte>? owner;

        public IPEndPoint Endpoint { get; }
        public ReadOnlySequence<byte> Payload { get; }
    
        internal IncomingMessage(IPEndPoint endpoint, IMemoryOwner<byte> owner, int messageLength)
        {
            this.Endpoint = endpoint;
            this.owner = owner;
            this.Payload = new ReadOnlySequence<byte>(owner.Memory.Slice(0, messageLength));
        }
        
        internal IncomingMessage(IPEndPoint endpoint, ReadOnlySequence<byte> payload)
        {
            this.Endpoint = endpoint;
            this.owner = null;
            this.Payload = payload;
        }

        public void Dispose()
        {
            owner?.Dispose();
        }
    }
}