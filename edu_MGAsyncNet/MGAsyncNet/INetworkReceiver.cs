using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGAsyncNet
{
    // 이 인터페이스는 응용계층에서 구현하고, ASNetSerivce생성시에 구현한 개체를 넘겨줘야 합니다
    public interface INetworkReceiver
    {
        // 새로운 연결이 들어왔다.
        // addressinfo => TcpClient.Client.RemoteEndPoint
        //  sockdesc : assock의 멤버가 복사되어 넘어져 온다.
        //  addressinfo : 받은측에서 사용할때 값을 복사해서 사용할것
        void OnRegisterSocket(AsyncSocketContext sockdesc, string addressinfo);

        // 소켓 연결이 해제되었다.
        //  sockdesc : assock의 멤버가 복사되어 넘어져 온다.
        //  socket : 객체풀링등을 하고자 한다면, 응용계층에서 할것~
        void OnReleaseSocket(AsyncSocketContext sockdesc, AsyncSocket socket);

        // connectSocket에 대한 결과
        //  bSuccess가 false이면 ex가 null이 아닌 개체로 전송된다~
        void OnConnectingResult(int requestID, AsyncSocketContext sockdesc, bool bSuccess, Exception ex);

        void OnReceiveData(AsyncSocketContext sockdesc, int length, byte[] data, int offset);
    }
}
