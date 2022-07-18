using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet;

public class NetworkDefine
{
#region SYSTEM_PACKET
    public const UInt16 SYS_NTF_CONNECTED = 1;
    
    public const UInt16 SYS_NTF_CLOSED = 2;

    public const UInt16 SYS_START_HEARTBEAT = 3;

    // 리모트에서 받은 패킷의 경우 이 숫자를 넘어서는 것은 에러
    public const UInt16 SYS_NTF_MAX = 100;
#endregion

    
}




