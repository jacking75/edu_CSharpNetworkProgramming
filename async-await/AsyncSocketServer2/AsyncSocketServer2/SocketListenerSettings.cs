using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AsyncSocketServer2
{
    class SocketListenerSettings
    {
        // the maximum number of connections the sample is designed to handle simultaneously 
        private Int32 maxConnections;

        // this variable allows us to create some extra SAEA objects for the pool,
        // if we wish.
        private Int32 numberOfSaeaForRecSend;

        // max # of pending connections the listener can hold in queue
        private Int32 backlog;

        // tells us how many objects to put in pool for accept operations
        private Int32 maxSimultaneousAcceptOps;

        // buffer size to use for each socket receive operation
        private Int32 receiveBufferSize;

        // length of message prefix for receive ops
        private Int32 receivePrefixLength;

        // length of message prefix for send ops
        private Int32 sendPrefixLength;

        // See comments in buffer manager.
        private Int32 opsToPreAllocate;

        // Endpoint for the listener.
        private IPEndPoint localEndPoint;

        public SocketListenerSettings(Int32 maxConnections, Int32 excessSaeaObjectsInPool, Int32 backlog, Int32 maxSimultaneousAcceptOps, Int32 receivePrefixLength, Int32 receiveBufferSize, Int32 sendPrefixLength, Int32 opsToPreAlloc, IPEndPoint theLocalEndPoint)
        {
            this.maxConnections = maxConnections;
            this.numberOfSaeaForRecSend = maxConnections + excessSaeaObjectsInPool;
            this.backlog = backlog;
            this.maxSimultaneousAcceptOps = maxSimultaneousAcceptOps;
            this.receivePrefixLength = receivePrefixLength;
            this.receiveBufferSize = receiveBufferSize;
            this.sendPrefixLength = sendPrefixLength;
            this.opsToPreAllocate = opsToPreAlloc;
            this.localEndPoint = theLocalEndPoint;
        }

        public Int32 MaxConnections
        {
            get
            {
                return this.maxConnections;
            }
        }
        public Int32 NumberOfSaeaForRecSend
        {
            get
            {
                return this.numberOfSaeaForRecSend;
            }
        }
        public Int32 Backlog
        {
            get
            {
                return this.backlog;
            }
        }
        public Int32 MaxAcceptOps
        {
            get
            {
                return this.maxSimultaneousAcceptOps;
            }
        }
        public Int32 ReceivePrefixLength
        {
            get
            {
                return this.receivePrefixLength;
            }
        }
        public Int32 BufferSize
        {
            get
            {
                return this.receiveBufferSize;
            }
        }
        public Int32 SendPrefixLength
        {
            get
            {
                return this.sendPrefixLength;
            }
        }
        public Int32 OpsToPreAllocate
        {
            get
            {
                return this.opsToPreAllocate;
            }
        }
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return this.localEndPoint;
            }
        }    
    }    
}
