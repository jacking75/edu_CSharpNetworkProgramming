using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MGAsyncNet
{       
    public class AsyncSocket
    {
        #region Static Member
        static Int64 UniqueSequencId = 0;

        static Int64 GenerateUniqueSequenceId()
        {
            return Interlocked.Increment(ref UniqueSequencId);
        }

        static RangeedUniqueIdGenerator UIDAllocator = new RangeedUniqueIdGenerator();


        public static void InitUIDAllocator(UInt64 startNumber, UInt64 maxCount)
        {
            UIDAllocator.Reset(startNumber, maxCount);
        }

        public static UInt64 RetrieveUID()
        {            
            var id = UIDAllocator.Retrieve();
            return id;
        }

        public static void ReleaseUID(UInt64 id)
        {
            UIDAllocator.Release(id);
        }
        #endregion


        public string RemoteAddress = "";

        public TcpClient TCPClient = null;

        int IOCount = 0;

        public AsyncSocketContext Description { get; set; }

        public bool IsSending = false;
        
       
        public AsyncSocket()
        {
            IOCount = 0;
            Description = new AsyncSocketContext();
            Description.NetSender = null;
            Description.ManagedID = AsyncSocket.RetrieveUID();
            Description.UniqueId = AsyncSocket.GenerateUniqueSequenceId();
        }
        
        public void SetTcpClient(TcpClient client)
        {
            TCPClient = client;
        }

        public int ExitIO()
        {
            return System.Threading.Interlocked.Decrement(ref IOCount);
        }

        public int EnterIO()
        {
            return System.Threading.Interlocked.Increment(ref IOCount);
        }

        // 받은 내용을 어떻게 처리할지는 이 함수를 재정의해서 처리하시오!
        public virtual void HandleReceived(int length, byte[] data, int offset, INetworkReceiver receiver)
        {
            //TODO 구현이 좀 이상한듯
            // 보통 아래 receiver.notifyMessage 함수에서 프로토콜에 맞는 객체를 생성하면 될것이다.
            //  생성은 쓰레드제어없이 가능하게하고, enqueuing을 쓰레드제어하면 된다.
            // 객체파괴가 어떻게 일어날지 모르니, desc는 값을 복사해서 사용하자.                
            AsyncSocketContext desc = new AsyncSocketContext();
            desc.UniqueId = Description.UniqueId;
            desc.ManagedID = Description.ManagedID;
            desc.NetSender = Description.NetSender;
            receiver.OnReceiveData(desc, length, data, offset);
        }



        // 이 함수는 객체풀링 등을 할때 상속받은 계층에서 사용하시길~
        //protected void Finit()
        //{
        //    RemoteAddress = "";
        //    TCPClient = null;
        //    Description.NetSender = null;
        //}
    }
}
