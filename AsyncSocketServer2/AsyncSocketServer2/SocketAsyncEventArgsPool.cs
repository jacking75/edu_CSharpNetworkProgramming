using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AsyncSocketServer2
{    
    internal sealed class SocketAsyncEventArgsPool
    {        
        //just for assigning an ID so we can watch our objects while testing.
        private Int32 nextTokenId = 0;
        
        // Pool of reusable SocketAsyncEventArgs objects.        
        Stack<SocketAsyncEventArgs> pool;
        
        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            
            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("SocketAsyncEventArgsPool constructor");
            }
            
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        // The number of SocketAsyncEventArgs instances in the pool.         
        internal Int32 Count
        {
            get { return this.pool.Count; }
        }

        internal Int32 AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref nextTokenId);            
            return tokenId;
        }

        // Removes a SocketAsyncEventArgs instance from the pool.
        // returns SocketAsyncEventArgs removed from the pool.
        internal SocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                return this.pool.Pop();
            }
        }

        // Add a SocketAsyncEventArg instance to the pool. 
        // "item" = SocketAsyncEventArgs instance to add to the pool.
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null) 
            { 
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); 
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
