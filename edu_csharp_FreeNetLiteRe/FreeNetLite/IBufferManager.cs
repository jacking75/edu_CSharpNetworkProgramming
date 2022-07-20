using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeNetLite;

internal interface IBufferManager
{
    void Init(int bufferCountOrMaxTakeBufferSize, int bufferSizeOrbucketCount);

    bool SetBuffer(SocketAsyncEventArgs args);

}
