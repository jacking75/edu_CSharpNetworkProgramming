using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MGAsyncNet
{
    // 응용계층에서 ASNetService를 생성하긴 해야하지만, 그렇다고 ASNetService를 직접 가져다 쓰면 안된다.    
    public interface INetworkSender
    {
        int PostingSend(AsyncSocketContext sockdesc, int length, byte[] data);

        int DisconnectSocket(AsyncSocketContext sockContext);

        int ReleaseAsyncSocketContext(AsyncSocketContext sockContext);        
    }
}
