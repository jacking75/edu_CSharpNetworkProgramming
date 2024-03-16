using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

// 2019. 01.15 리팩토링

// MyServerLib C# version
// 2012-6-24 1차 버전 완성
//  2012-6-25 성능향상을 위해 매니저락걸리는 소켓그룹을 분산시켰다. ASIOManager가 ASNetService를 여러개 생성
// veruna2k@nate.com (이메일 및 네이트온 메신져)
//  궁금하시거나 버그리포팅?(^^;;)은 위 주소로 부탁드려요~
// 2012-9-11
//  - receiver에거 ASSOCKDESC 전달시 ASSocket의 멤버 레퍼런스가 아닌, 값이 복사된 새로운 ASSOCKDESC 객체로 넘어감 (넷계층에서 원본객체가 파괴 안될수도 있기 때문에~)
// 2012-9-19
//  - bugfix : ResultConnect에서 연결성공후 첫번째 리시빙 요청안하는 버그 수정
//  - bugfix : []연산자로 dictionary 접근시 키에 해당하는 value가 없으면 널반환이 아니라 예외를던진다.

// 1.1 .net 3.5
//  .net 3.5 에 맞게 업그레이드~
//  ASSocket.handleReceived 변경
//  INetworkReceiver.notifyMessage 변경

namespace MGAsyncNet
{
    public enum MyErrorCode
    {
        None,
        NoSuchSocket,
        OutOfMemory,
        SizeError,
        AlreadyPostConnect,
    }

    // facade격에 해당하는 클래스
    //  응용측에서 인스턴스를 하나 생성해서 관리하세요.
    //  acceptor와 짝을 이루고 싶고, 여러 acceptor를 만들고 싶은 경우가 있기 때문에, 싱글톤으로 안했습니다.
    public class AsyncIOManager
    {
        public static int Min_NetContainer = 1;
        public static int Max_NetContainer = 10;

        INetworkReceiver theReceiver;
        List<AsyncIOWorker> AsyncIOWorkerList = new List<AsyncIOWorker>();
        int MaxCount;
        int CurrentId;
        int IOSize; // 한번에 읽기/쓰기 최대 크기
        int IOFrameMax; // 풀링할 saea의 최대 수 maxconnect와 multiple을 곱해서
        BufferManagerAsync BufferManager;
        SocketAsyncEventArgsPool EventArgsPool;

        public int IOMAXSIZE { get { return IOSize; } }

        public SocketAsyncEventArgs RetreiveEventArgs()
        {
            lock (this)
            {
                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();

                if (false == BufferManager.SetBuffer(saea))
                {
                    throw new OutOfMemoryException("RetreiveSAEA");
                }

                return saea;

            }
            //return saeaPool.Pop();
        }

        public void ReleaseSAEA(SocketAsyncEventArgs e)
        {
            lock (this)
            {
                BufferManager.FreeBuffer(e);
            }
        }

        // io * maxconnect * multiple 만큼 버퍼가 할당됩니다.
        public AsyncIOManager(int ioWorkerCount, INetworkReceiver receiver, int maxIOBufferSize, int maxConnectCount, int multiple)
        {
            theReceiver = receiver;
            MaxCount = ioWorkerCount;

            if (MaxCount < 1)
            {
                MaxCount = System.Environment.ProcessorCount;
            }

            for (int index = 1; index <= MaxCount; index++)
            {
                AsyncIOWorker theone = new AsyncIOWorker(theReceiver, this);
                theone.Index = index;
                AddIOWorker( theone);
            }

            CurrentId = 1;
            IOSize = maxIOBufferSize;
            
            IOFrameMax = maxConnectCount * multiple;
            BufferManager = new BufferManagerAsync(IOSize * IOFrameMax, IOSize);
            BufferManager.InitBuffer();

            EventArgsPool = new SocketAsyncEventArgsPool(/*ioframemax*/);                     
        }

        public int connectSocket(int reqID, AsyncSocket socket, string ipAddress, int port)
        {
            var sel = 1;
            var selected = GetIOWorker(sel);
            return selected.ConnectSocket(reqID, socket, ipAddress, port);
        }

        internal int registerSocket(TcpClient acceptedClient)
        {
            var selected = GetIOWorker(CurrentId);
            int ret = selected.RegisterSocket(acceptedClient);

            if (MaxCount < ++CurrentId)
            {
                CurrentId = 1;
            }

            return ret;
        }

        void AddIOWorker(AsyncIOWorker worker)
        {
            AsyncIOWorkerList.Add(worker);
        }

        AsyncIOWorker GetIOWorker(int index)
        {
            return AsyncIOWorkerList.Find(x => x.Index == index);
        }


    } // end Class
}
