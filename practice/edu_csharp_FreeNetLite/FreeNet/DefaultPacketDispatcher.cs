using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeNet
{
    /// <summary>
    /// 패킷을 처리해서 컨텐츠를 실행하는 곳이다.
    /// FreeNet을 사용할 때 LogicMessageEntry을 참고해서 IMessageDispatcher를 상속 받는 클래스를 맞게 구현하자
    /// </summary>
    public class DefaultPacketDispatcher : IPacketDispatcher
    {
        ILogicQueue MessageQueue = new DoubleBufferingQueue();
        

        public DefaultPacketDispatcher()
        {            
        }
                       

        public void IncomingPacket(bool IsSystem, Session user, Packet packet)
        {
            // 여긴 IO스레드에서 호출된다.
            // 완성된 패킷을 메시지큐에 넣어준다.
            
            if(IsSystem == false && packet.ProtocolId <= (short)NetworkDefine.SYS_NTF_MAX)
            {
                //TODO: 로그 남기기. 여기서는 로거의 인터페이스만 호출해야 한다. 로거의 구현은 애플리케이션에서 구현한다

                // 시스템만 보내어야할 패킷을 상대방이 보냈음. 해킹 의심
                return;
            }

            MessageQueue.Enqueue(packet);
        }
        
        public Queue<Packet> DispatchAll()
        {
            return MessageQueue.TakeAll();
        }

              
    }
}
