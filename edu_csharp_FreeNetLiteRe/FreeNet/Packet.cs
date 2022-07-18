using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeNet
{
	/// <summary>
	/// byte[] 버퍼를 참조로 보관하여 pop_xxx 매소드 호출 순서대로 데이터 변환을 수행한다.
	/// </summary>
	public class Packet : IPacket
	{
		public Session Owner { get; set; }

		public byte[] BodyData { get; set; }
		
		

		public UInt16 ProtocolId { get; private set; }

				
		public Packet(UInt16 packetId, byte[] body)
		{
			// 참조로만 보관하여 작업한다.
			// 복사가 필요하면 별도로 구현해야 한다.
			BodyData = body;
			ProtocolId = packetId;			
		}

		public Packet(Session owner, UInt16 packetId)
		{
			// 참조로만 보관하여 작업한다.
			// 복사가 필요하면 별도로 구현해야 한다.
			Owner = owner;
			ProtocolId = packetId;
		}
						
		
	}
}
