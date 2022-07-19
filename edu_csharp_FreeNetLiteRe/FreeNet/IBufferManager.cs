using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeNet;

internal interface IBufferManager
{
    void Init(int bufferCountOrMaxTakeBufferSize, int bufferSizeOrbucketCount);
    bool SetBuffer(SocketAsyncEventArgs args);
    void FreeBuffer(SocketAsyncEventArgs args);
}
