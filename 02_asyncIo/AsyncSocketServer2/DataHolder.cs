using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;


namespace AsyncSocketServer2;

class DataHolder
{
    // 주의: 소켓이 버퍼로 바이트 배열을 사용하는 경우, 해당 바이트 배열은 .NET에서 관리되지 않으며 메모리 단편화를 일으킬 수 있습니다.
    // 따라서, 먼저 SAEA 객체가 사용하는 버퍼 블록에 데이터를 작성한 다음, 해당 데이터를 유지하거나 작업해야하는 경우 다른 바이트 배열로 복사할 수 있습니다.
    // 이렇게 하면 SAEA 객체를 빠르게 풀에 다시 넣거나 데이터 전송을 빠르게 계속할 수 있습니다.
    // DataHolder에는 데이터를 복사할 수 있는 바이트 배열이 있습니다.
    internal Byte[] dataMessageReceived;

    internal Int32 receivedTransMissionId;

    internal Int32 sessionId;

    // 테스트용. 패킷 분석기를 사용하여 특정 연결을 확인할 수 있습니다.
    internal EndPoint remoteEndpoint;
}
