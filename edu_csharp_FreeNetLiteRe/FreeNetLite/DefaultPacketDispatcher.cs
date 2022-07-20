using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeNetLite;

/// 패킷을 처리해서 컨텐츠를 실행하는 곳이다.
public class DefaultPacketDispatcher : IPacketDispatcher
{
	UInt16 HeaderSize = 0;
	
	ILogicQueue MessageQueue = new DoubleBufferingQueue();
    
    public void Init(UInt16 headerSize)
    {
		HeaderSize = headerSize;	
	}

	public void ProcessReceiveData(Session session, byte[] buffer, int offset, int size)
	{
		var receiveBufferOffset = offset;
		var receiveBufferReaminSize = size;

		// 남은 데이터가 있다면 계속 반복한다.
		while (receiveBufferReaminSize >= HeaderSize)
		{
			(var packetSize, var packetId) = GetPacketSizeAndId(buffer, receiveBufferOffset);
			if (packetSize > receiveBufferReaminSize)
			{
				break;
			}

			byte[] bodyData = null;
			var bodySize = packetSize - HeaderSize;
			if (bodySize > 0)
			{
				bodyData = new byte[bodySize];
			}

			var packet = new Packet(session, packetId, bodyData);
			IncomingPacket(false, session, packet);

			receiveBufferOffset += packetSize;
			receiveBufferReaminSize -= packetSize;
		}

		//간결한 구현을 위해
		//receiveBufferReaminSize가 남는 상황은 고려하지 않는다!!!
	}

	(UInt16, UInt16) GetPacketSizeAndId(byte[] buffer, int offset)
	{
		var packetSize = FastBinaryRead.UInt16(buffer, offset);
		var packetId = FastBinaryRead.UInt16(buffer, offset + 2);
		return (packetSize, packetId);
	}

	public void IncomingPacket(bool IsSystem, Session user, Packet packet)
    {
        // 여긴 IO스레드에서 호출된다.
        // 완성된 패킷을 메시지큐에 넣어준다.
        
        if(IsSystem == false && packet.Id <= (short)NetworkDefine.SYS_NTF_MAX)
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
